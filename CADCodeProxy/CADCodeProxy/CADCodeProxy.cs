using CADCode;
using CADCodeProxy.CNC;
using CADCodeProxy.Events;
using CADCodeProxy.Exceptions;
using CADCodeProxy.Machining;
using CADCodeProxy.Results;
using CCWSXML;
using System.Runtime.InteropServices;
using ErrorEventHandler = CADCodeProxy.Events.ErrorEventHandler;

namespace CADCodeProxy.CADCodeProxy;

internal class CADCodeProxy : IDisposable {

    private CADCodeBootObject? _bootObj = null;

    public string LabelCreatorName { get; set; } = "";

    public event ErrorEventHandler? ErrorEvent;

    public event CADCodeProgressEventHandler? CADCodeProgressEvent;
    public event CADCodeInfoEventHandler? CADCodeInfoEvent;
    public event CADCodeErrorEventHandler? CADCodeErrorEvent;

    public void Initialize() {

        _bootObj = new CADCodeBootObject() {
            MessageMethod = MessageTypes.CC_USE_EVENTS,
            //MessageMethod = MessageTypes.CC_USE_MESSAGEBOX
        };

        if (_bootObj is null) {
            throw new InvalidOperationException($"Could not initialize CADCode");
        }

        // TODO: Not sure if it is really necessary (or proper) to throw exceptions in this event handler, when the result is checked after calling Init
        // 1) Verify that whenever CADCode cannot be authorized (ie Another pc is using it or it cannot access the authentication server) the event is called and the same error is returned from Init
        // 2) Verify that whenever CADCode initiated (ie CADCode is not installed) the event is called and the same error is returned from Init
        _bootObj.CreateError += (l, s) => {
            CADCodeErrorEvent?.Invoke(new("CADCodeBootObject", "CreateError", l, s));
            if (l == 33012) {
                throw new CADCodeAuthorizationException(s);
            } else {
                throw new CADCodeInitializationException(l, $"Could not initialize CADCode - {s}");
            }
        };

        var initResult = _bootObj.Init();

        if (initResult == 33012) {
            throw new CADCodeAuthorizationException(_bootObj.GetErrorString(initResult));
        } else if (initResult != 0) {
            throw new CADCodeInitializationException(initResult, $"Could not initialize CADCode - {_bootObj.GetErrorString(initResult)}");
        }

    }

    public MachineGCodeGenerationResult GenerateGCode(Machine machine, Batch batch, InventoryItem[] inventory, UnitTypes units) {

        if (_bootObj is null) {
            Initialize();
        }

        if (_bootObj is null) {
            throw new InvalidOperationException("Could not initialize CADCode");
        }

        CADCodeToolFileClass? toolFile = null;
        CADCodeFileClass? files = null;
        CADCodeCodeClass? code = null;

        List<MaterialGCodeGenerationResult> materialResults = [];

        try {

            toolFile = CreateToolFile(_bootObj, machine.ToolFilePath);
            files = CreateFiles(machine);
            code = CreateCode(_bootObj, batch.Name, machine, toolFile, machine.NestOutputDirectory);

            var groups = batch.Parts.GroupBy(p => new PartGroupKey(p.Material, p.Thickness));

            ValidateInventory(inventory, groups);

            foreach (var group in groups) {

                CADCodeLabelClass createLabelClass(string resultNumber) {
                    return CreateLabel(_bootObj, batch.Name, group.Key.MaterialName, group.Key.Thickness, resultNumber, machine.LabelDatabaseOutputDirectory);
                }

                var matResult = GenerateCodeForMaterialType(batch.InfoFields, group.Key, [.. group], inventory, units, _bootObj, files, code, createLabelClass);
                materialResults.Add(matResult);

            }

            GenerateSinglePrograms(_bootObj, batch, machine, units);

        } catch (Exception ex) {
            ErrorEvent?.Invoke(new("Error occurred while generating gcode", ex));
            throw;
        } finally {
            ReleaseComObjects(toolFile, files, code);
        }

        return new() {
            MachineName = machine.Name,
            ToolFilePath = machine.ToolFilePath,
            NestOutputDirectory = machine.NestOutputDirectory,
            SingleProgramOutputDirectory = machine.SingleProgramOutputDirectory,
            PictureOutputDirectory = machine.PictureOutputDirectory,
            LabelDatabaseOutputDirectory = machine.LabelDatabaseOutputDirectory,
            MaterialGCodeGenerationResults = [.. materialResults]
        };

    }

    private static void ValidateInventory(InventoryItem[] inventory, IEnumerable<IGrouping<PartGroupKey, Machining.Part>> groups) {

        foreach (var group in groups) {

            var matchingInventory = inventory.Where(i => i.MaterialName == group.Key.MaterialName && i.PanelThickness == group.Key.Thickness).ToList();

            if (matchingInventory.Count == 0) {
                throw new InvalidInventoryException($"No valid {group.Key.Thickness}mm thick '{group.Key.MaterialName}' inventory available");
            }

            // TODO: take into account panel trim when checking size of parts
            // TODO: make sure width / length is correct
            var largestLength = group.Select(p => {
                if (p.IsGrained) return p.Length;
                return Math.Min(p.Width, p.Length);
            })
                .Max();

            var largestWidth = group.Select(p => {
                if (p.IsGrained) return p.Width;
                return Math.Min(p.Width, p.Length);
            })
                .Max();

            if (matchingInventory.Any(i => i.PanelWidth < largestWidth) || matchingInventory.Any(i => i.PanelLength < largestLength)) {
                throw new InvalidInventoryException($"No valid {group.Key.Thickness}mm thick '{group.Key.MaterialName}' inventory large enough for all parts in batch");
            }

        }

    }

    private MaterialGCodeGenerationResult GenerateCodeForMaterialType(InfoFields batchInfoFields, PartGroupKey partGroupKey, Machining.Part[] batchParts, InventoryItem[] inventory, UnitTypes units, CADCodeBootObject bootObj, CADCodeFileClass files, CADCodeCodeClass code, Func<string, CADCodeLabelClass> createLabels) {

        var optimizer = CreateOptimizer(bootObj, files);

        try {

            List<CutlistInventory> sheetStock = inventory.Where(i => i.MaterialName == partGroupKey.MaterialName && i.PanelThickness == partGroupKey.Thickness)
                                                        .Select(i => i.AsCutlistInventory())
                                                        .ToList();

            List<(CADCode.Part Part, Guid PartId)> parts = batchParts.SelectMany(p => p.ToCADCodePart(units).Select(cp => (cp, p.Id))).ToList();

            var partLabels = new List<PartLabel>();

            var resultNumber = GenerateResultNumber();
            string resultName = (resultNumber).ToString("D5");
            code.StartingProgramNumber = resultNumber;

            var labels = createLabels(resultName);

            code.Border(1.0f, 1.0f, (float)partGroupKey.Thickness, units, OriginType.CC_UL, $"{partGroupKey.MaterialName} {partGroupKey.Thickness}", AxisTypes.CC_AUTO_AXIS);
            foreach (var batchPart in batchParts) {
                partLabels.Add(batchPart.AddToLabels(batchInfoFields, labels));
                batchPart.AddNestPartToCode(code);
            }
            code.EndPanel();

            sheetStock.ForEach(ss => optimizer.AddSheetStockByRef(ss, units));
            parts.ForEach(p => optimizer.AddPartByRef(p.Part));

            optimizer.Optimize(typeOptimizeMethod.CC_OPT_ANYKIND, code, resultName, 0, 0, labels);

            ReleaseComObject(labels);

            var usedInventory = sheetStock.Select(UsedInventory.FromCutlistInventory)
                                            .Where(i => i.Qty > 0)
                                            .ToArray();

            var placedParts = parts.Where(p => p.Part.PatternNumber != 0)
                                    .Select(val => PlacedPart.FromPart(val.Part, val.PartId))
                                    .ToArray();

            var unplacedParts = optimizer.GetUnplacedParts();

            var programs = code.GetProcessedFileNames()
                                .AsEnumerable()
                                .Select(s => Path.GetFileName(s.Split(',').First().Trim()))
                                .ToArray();

            return new MaterialGCodeGenerationResult() {
                MaterialName = partGroupKey.MaterialName,
                MaterialThickness = partGroupKey.Thickness,
                ProgramNames = programs,
                UnplacedParts = unplacedParts,
                PlacedParts = placedParts,
                UsedInventory = usedInventory,
                PartLabels = [.. partLabels]
            };

        } catch (Exception ex) {
            ErrorEvent?.Invoke(new("Error occurred while generating gcode", ex));
            throw;
        } finally {
            ReleaseComObject(optimizer);
        }

    }

    private void GenerateSinglePrograms(CADCodeBootObject bootObj, Batch batch, Machine machine, UnitTypes units) {

        var toolFile = CreateToolFile(bootObj, machine.SinglePartToolFilePath);
        var code = CreateCode(bootObj, batch.Name, machine, toolFile, machine.SingleProgramOutputDirectory);

        var resultNumber = GenerateResultNumber();
        code.StartingProgramNumber = resultNumber;

        foreach (var part in batch.Parts) {
            part.AddPrimaryFaceSinglePartToCode(code, units);
        }

        var result = code.DoOutput(units, 0, 0);

        if (result != 0) {
            ErrorEvent?.Invoke(new($"Non-zero response returned while generating single part programs - {result}"));
        }

    }

    private CADCodeToolFileClass CreateToolFile(CADCodeBootObject boot, string filePath) {

        var toolFile = boot.CreateToolFile()
                        ?? throw new InvalidOperationException("Could not create tool file");

        string eventSourceName = "CADCodeToolFileClass";

        toolFile.ToolFileError += (l, s) => CADCodeErrorEvent?.Invoke(new(eventSourceName, "ToolFileError", l, s));
        toolFile.ToolFileSaved += (fileName) => CADCodeInfoEvent?.Invoke(new(eventSourceName, "ToolFileSaved", fileName));

        int result = toolFile.ReadToolFile(filePath);
        if (result != 0) {
            ErrorEvent?.Invoke(new($"Could not load tool file from file '{filePath}'"));
        }

        return toolFile;
    }

    private CADCodeLabelClass CreateLabel(CADCodeBootObject boot, string batchName, string materialName, double materialThickness, string resultNumber, string outputDirectory) {

        // Stores the data that will be written to the label database
        var labels = boot.CreateLabels()
                        ?? throw new InvalidOperationException("Could not create CADCode label object");

        labels.Creator = LabelCreatorName;

        string eventSourceName = "CADCodeLabelClass";

        labels.LabelModuleError += (l, s) => CADCodeErrorEvent?.Invoke(new(eventSourceName, "LabelModuleError", l, s));
        labels.Progress += (val) => CADCodeProgressEvent?.Invoke(new(eventSourceName, val));

        string prefix = $"{materialName} {Math.Round(materialThickness, 0)}";
        labels.JobName = $"{prefix} {resultNumber}";

        var fileName = RemoveInvalidFileNameChars(batchName.Trim());
        var directory = Path.Combine(outputDirectory, fileName);

        if (!Directory.Exists(directory)) {
            Directory.CreateDirectory(directory);
            if (!Directory.Exists(directory)) {
                ErrorEvent?.Invoke(new($"Failed to create label output directory '{directory}'"));
            }
        }

        labels.LabelFileName = Path.Combine(outputDirectory, fileName, $"{fileName}.mdb"); // This is either a relative path (where the root directory is set by the CADCodeFileClass) or an absolute path

        return labels;
    }

    private CADCodePanelOptimizerClass CreateOptimizer(CADCodeBootObject boot, CADCodeFileClass files) {

        // Does the work of optimizing a group of parts onto a group of available sheet stock
        var optimizer = boot.CreatePanelOptimizer()
                        ?? throw new InvalidOperationException("Could not create CADCode optimizer object");

        string eventSourceName = "CADCodePanelOptimizerClass";

        optimizer.Progress += (val) => CADCodeProgressEvent?.Invoke(new(eventSourceName, val));

        optimizer.OptimizeError += (l, s) => CADCodeErrorEvent?.Invoke(new(eventSourceName, "OptimizeError", l, s));
        optimizer.PrintingError += (l, s) => CADCodeErrorEvent?.Invoke(new(eventSourceName, "PrintingError", l, s));

        optimizer.ReportedProgress += (int PanelsUsed, ref int PartsToGo) => CADCodeInfoEvent?.Invoke(new(eventSourceName, "ReportedProgress", $"{PanelsUsed} panels used | {PartsToGo} parts left"));
        optimizer.FileWritten += (string filename) => CADCodeInfoEvent?.Invoke(new(eventSourceName, "FileWritten", filename));
        optimizer.PartsMissing += (int howMany) => CADCodeInfoEvent?.Invoke(new(eventSourceName, "PartsMissing", howMany.ToString()));
        //optimizer.NestedIntervalCheck += () => _;

        optimizer.FileLocations = files;
        optimizer.Settings(100, 20, 1, 12.53, 10, 0f);  // Error 33011 - Did not set optimizer settings `CADCodePanelOptimizer.Settings()` when not set
        //optimizer.ToolFile = toolFile;  // error 33014 (CC_MISSING_SETTINGS) when setting tool file

        return optimizer;
    }

    private CADCodeCodeClass CreateCode(CADCodeBootObject boot, string batchName, Machine machine, CADCodeToolFileClass toolFile, string outputDirectoryRoot) {

        // Stores the machining data needed for all the parts, and does the work of creating the individual part programs
        var code = boot.CreateCode()
                    ?? throw new InvalidOperationException("Could not create CADCode code object");

        string eventSourceName = "CADCodeCodeClass";

        code.Progress += (val) => CADCodeProgressEvent?.Invoke(new(eventSourceName, val));

        code.MachiningError += (l, s) => CADCodeErrorEvent?.Invoke(new(eventSourceName, "MachiningError", l, s));

        code.MachiningInfo += (i) => CADCodeInfoEvent?.Invoke(new(eventSourceName, "MachiningInfo", i));
        code.StartProcess += (f) => CADCodeInfoEvent?.Invoke(new(eventSourceName, "ProcessStarted", f));
        code.PictureFileWritten += (f) => CADCodeInfoEvent?.Invoke(new(eventSourceName, "PictureFileWritten", f));
        code.LastProgramNumberUsed += (n) => CADCodeInfoEvent?.Invoke(new(eventSourceName, "LastProgramNumberUsed", n.ToString()));
        code.FileDetails += (fileDetails) => CADCodeInfoEvent?.Invoke(new(eventSourceName, "FileDetails", fileDetails));
        code.FileWritten += (fileName) => CADCodeInfoEvent?.Invoke(new(eventSourceName, "FileName", fileName));

        //code.NameChange += (o, n) => CADCodeInfoEvent?.Invoke(new(code, $"Name change {o} => {n}"));
        //code.SawOutPutChanged += () => CADCodeInfoEvent?.Invoke(new(code, ""));
        //code.FileSplit += () => CADCodeInfoEvent?.Invoke(new(code, ""));
        //code.DirectOutPutChanged += () => CADCodeInfoEvent?.Invoke(new(code, ""));

        string outputDir = Path.Combine(outputDirectoryRoot, RemoveInvalidFileNameChars(batchName));
        if (!Directory.Exists(outputDir)) {
            Directory.CreateDirectory(outputDir);
            if (!Directory.Exists(outputDir)) {
                ErrorEvent?.Invoke(new($"Failed to create gcode output directory '{outputDir}'"));
            }
        }

        code.SetOutputPath(outputDir); // The code class's output path must either be set by setting the 'FileStructures' property or by calling the 'SetOutputPath' method: 33009 CC_OUTPUT_PATH_NOT_DEFINED
        code.ToolFile = toolFile; // Must be set
        code.BatchName = batchName;
        code.PicturesFilledCircles = false;
        code.GeneratePictures = true;
        code.SetPicturePath(machine.PictureOutputDirectory); // if generate pictures is set, the picture path must be set
        code.AllowPanelRotation = true; // Required to make Omni work

        //code.WriteWinStepFiles = true;

        return code;

    }

    private static CADCodeFileClass CreateFiles(Machine machine) {
        return new CADCodeFileClass {
            ToolFilePath = machine.ToolFilePath,
            FileOutputDirectory = machine.NestOutputDirectory,
            LabelFilePath = machine.LabelDatabaseOutputDirectory,
            PictureFileLocation = machine.PictureOutputDirectory,
            ReportsLocation = machine.NestOutputDirectory, // TODO: Add reports location to machine settings, if we can ever get reports to work
            StartingSubProgramNumber = 1,
            StartingProgramNumber = 1,
        };
    }

    private static int GenerateResultNumber() => new Random().Next(0, 10000);

    private static string RemoveInvalidFileNameChars(string fileName) => string.Concat(fileName.Split(Path.GetInvalidFileNameChars()));

    private void ReleaseComObjects(CADCodeToolFileClass? toolFile, CADCodeFileClass? files, CADCodeCodeClass? code) {
        ReleaseComObject(toolFile);
        ReleaseComObject(files);
        ReleaseComObject(code);
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        GC.WaitForPendingFinalizers();
    }

    public void Dispose() {
        if (_bootObj is not null) {
            _bootObj.CloseAllOpenWindows();
            ReleaseComObject(_bootObj);
        }
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        GC.WaitForPendingFinalizers();
    }

    private void ReleaseComObject(object? obj) {

        if (obj is null) {
            return;
        }

        try {
            Marshal.ReleaseComObject(obj);
        } catch (Exception ex) {
            ErrorEvent?.Invoke(new($"Failed to release COM object {obj}", ex));
        }

    }

    private readonly record struct PartGroupKey(string MaterialName, double Thickness);

}
