using CADCode;
using CADCodeProxy.CNC;
using CADCodeProxy.Exceptions;
using CADCodeProxy.Machining;
using CADCodeProxy.Results;
using CCWSXML;
using System.Runtime.InteropServices;

namespace CADCodeProxy.CADCodeProxy;

internal class CADCodeProxy : IDisposable {

    private CADCodeBootObject? _bootObj = null;
    private readonly List<WS_Job> _wsJobs = [];
    // TODO: add a list of 'unreleased com objects' which can be released in the Dispose method, incase an exception is thrown and they have not yet been released

    public string LabelCreatorName { get; set; } = "";

    public delegate void ProgressEventHandler(int value);
    public event ProgressEventHandler? ProgressEvent;

    public delegate void InformationEventHandler(string message);
    public event InformationEventHandler? InformationEvent;

    public delegate void ErrorEventHandler(string message);
    public event ErrorEventHandler? ErrorEvent;

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
            ErrorEvent?.Invoke($"Failed to create CADCode instance, or authorize CADCode {l} - {s}");
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

    public bool TryWriteWSBatch(string outputFilePath) {

        if (RuntimeInformation.ProcessArchitecture != Architecture.X86
            || _wsJobs.Count == 0) {
            return false;
        }

        WS_Application app = new();
        app.WriteError += (ec, s) => ErrorEvent?.Invoke($"[WXML]{ec} - {s}");
        app.NodeError += (ec, s) => ErrorEvent?.Invoke($"[WXML][ERR] {ec} - {s}");
        app.Jobs.Add(_wsJobs.First());

        var result = app.SaveFile(outputFilePath);
        if (result != 0) {
            ErrorEvent?.Invoke($"Error writing WSXML File [{result}] - {_bootObj?.GetErrorString(result)}");
        }

        Marshal.ReleaseComObject(app);
        foreach (var job in _wsJobs) {
            Marshal.ReleaseComObject(job);
        }

        return (result == 0);

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

            if (ProgressEvent is not null) {
                code.Progress += ProgressEvent.Invoke;
            }

            if (ErrorEvent is not null) {
                code.MachiningError += (L, S) => ErrorEvent?.Invoke($"{L} - {S}");
            }

            var groups = batch.Parts.GroupBy(p => new PartGroupKey(p.Material, p.Thickness));

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

            foreach (var group in groups) {

                Func<string, CADCodeLabelClass> createLabelClass = (string resultNumber) => {
                    return CreateLabel(_bootObj, batch.Name, group.Key.MaterialName, group.Key.Thickness, resultNumber, machine.LabelDatabaseOutputDirectory);
                };

                var matResult = GenerateCodeForMaterialType(batch.InfoFields, group.Key, [.. group], inventory, units, _bootObj,  files, code, createLabelClass);
                materialResults.Add(matResult);

            }

            var singlePartToolFile = CreateToolFile(_bootObj, machine.SinglePartToolFilePath);
            var singlePartCode = CreateCode(_bootObj, batch.Name, machine, singlePartToolFile, machine.SingleProgramOutputDirectory);
            var result = GenerateSinglePrograms(batch.Parts, units, singlePartCode);
            if (result != 0) {
                ErrorEvent?.Invoke($"Non-zero response returned while generating single part programs - {result}");
            }

        } catch {
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

    private MaterialGCodeGenerationResult GenerateCodeForMaterialType(InfoFields batchInfoFields, PartGroupKey partGroupKey, Machining.Part[] batchParts, InventoryItem[] inventory, UnitTypes units, CADCodeBootObject bootObj, CADCodeFileClass files, CADCodeCodeClass code, Func<string, CADCodeLabelClass> createLabels) {

        var optimizer = CreateOptimizer(bootObj, files);

        try {

            if (ProgressEvent is not null) {
                optimizer.Progress += ProgressEvent.Invoke;
            }
            if (ErrorEvent is not null) {
                optimizer.OptimizeError += (L, S) => ErrorEvent?.Invoke($"{L} - {S}");
            }

            List<CutlistInventory> sheetStock = inventory.Where(i => i.MaterialName == partGroupKey.MaterialName && i.PanelThickness == partGroupKey.Thickness)
                                                        .Select(i => i.AsCutlistInventory())
                                                        .ToList();

            List<(CADCode.Part Part, Guid PartId)> parts = batchParts.SelectMany(p => p.ToCADCodePart(units).Select(cp => (cp, p.Id))).ToList();

            var partLabels = new List<PartLabel>();

            var resultNumber = GenerateResultNumber();
            string resultName = (resultNumber).ToString("D5");
            code.StartingProgramNumber = resultNumber;

            var labels = createLabels(resultName);
            if (ProgressEvent is not null) {
                labels.Progress += ProgressEvent.Invoke;
            }

            code.Border(1.0f, 1.0f, (float)partGroupKey.Thickness, units, OriginType.CC_UL, $"{partGroupKey.MaterialName} {partGroupKey.Thickness}", AxisTypes.CC_AUTO_AXIS);
            foreach (var batchPart in batchParts) {
                partLabels.Add(batchPart.AddToLabels(batchInfoFields, labels));
                batchPart.AddNestPartToCode(code);
            }
            code.EndPanel();

            if (batchParts.Any(p => p.SecondaryFace is not null)) {
                code.Border(1.0f, 1.0f, (float)partGroupKey.Thickness, units, OriginType.CC_UL, $"6{partGroupKey.MaterialName} {partGroupKey.Thickness}", AxisTypes.CC_AUTO_AXIS, Face6:true);
                foreach (var batchPart in batchParts) {
                    batchPart.AddNestSecondaryFacePartToCode(code);
                }
                code.EndPanel();
            }

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
            
        } catch {
            throw;
        } finally {
            ReleaseComObject(optimizer);
        }

    }

    private static int GenerateSinglePrograms(Machining.Part[] batchParts, UnitTypes units, CADCodeCodeClass code) {

        var resultNumber = GenerateResultNumber();
        code.StartingProgramNumber = resultNumber;

        foreach (var part in batchParts) {
            part.AddPrimaryFaceSinglePartToCode(code, units);
        }

        return code.DoOutput(units, 0, 0);

    }

    private static CADCodeToolFileClass CreateToolFile(CADCodeBootObject boot, string filePath) {

        var toolFile = boot.CreateToolFile()
                        ?? throw new InvalidOperationException("Could not create tool file");

        int result = toolFile.ReadToolFile(filePath);
        if (result != 0) {
            throw new InvalidOperationException($"Could not load tool file from file '{filePath}'");
        }

        return toolFile;
    }

    private CADCodeLabelClass CreateLabel(CADCodeBootObject boot, string batchName, string materialName, double materialThickness, string resultNumber, string outputDirectory) {

        // Stores the data that will be written to the label database
        var labels = boot.CreateLabels()
                        ?? throw new InvalidOperationException("Could not create CADCode label object");

        labels.Creator = LabelCreatorName;

        labels.LabelModuleError += (l, s) => ErrorEvent?.Invoke($"Error with label module {l} - {s}");
        labels.Progress += (l) => ProgressEvent?.Invoke(l);

        string prefix = $"{materialName} {Math.Round(materialThickness, 0)}"; 
        labels.JobName = $"{prefix} {resultNumber}";

        var fileName = RemoveInvalidFileNameChars(batchName.Trim());
        var directory = Path.Combine(outputDirectory, fileName);

        if (!Directory.Exists(directory)) {
            Directory.CreateDirectory(directory);
        }

        labels.LabelFileName = Path.Combine(outputDirectory, fileName, $"{fileName}.mdb"); // This is either a relative path (where the root directory is set by the CADCodeFileClass) or an absolute path

        return labels;
    }

    private CADCodePanelOptimizerClass CreateOptimizer(CADCodeBootObject boot, CADCodeFileClass files) {

        // Does the work of optimizing a group of parts onto a group of available sheet stock
        var optimizer = boot.CreatePanelOptimizer()
                        ?? throw new InvalidOperationException("Could not create CADCode optimizer object");

        optimizer.OptimizeError += (l, s) => ErrorEvent?.Invoke($"Optimization Error {l} - {s}");
        optimizer.Progress += (l) => ProgressEvent?.Invoke(l);
        optimizer.ReportedProgress += (int PanelsUsed, ref int PartsToGo) => InformationEvent?.Invoke($"{PanelsUsed} panels used | {PartsToGo} parts left");

        optimizer.FileLocations = files;
        optimizer.Settings(100, 20, 1, 12.53, 10, 0f);  // Error 33011 - Did not set optimizer settings `CADCodePanelOptimizer.Settings()` when not set
        //optimizer.ToolFile = toolFile;  // error 33014 (CC_MISSING_SETTINGS) when setting tool file

        return optimizer;
    }

    private CADCodeCodeClass CreateCode(CADCodeBootObject boot, string batchName, Machine machine, CADCodeToolFileClass toolFile, string outputDirectoryRoot) {

        // Stores the machining data needed for all the parts, and does the work of creating the individual part programs
        var code = boot.CreateCode()
                    ?? throw new InvalidOperationException("Could not create CADCode code object");

		code.MachiningInfo += (i) => InformationEvent?.Invoke($"Machining Info - {i}");
        code.MachiningError += (l, s) => ErrorEvent?.Invoke($"Machining Error - {l} - {s}");
        code.StartProcess += (f) => InformationEvent?.Invoke($"Process Started - {f}");
        code.Progress += (l) => ProgressEvent?.Invoke(l);
        code.PictureFileWritten += (f) => InformationEvent?.Invoke($"Picture file written - {f}");
        code.LastProgramNumberUsed += (n) => InformationEvent?.Invoke($"Last program number used '{n}'");
        code.NameChange += (o, n) => InformationEvent?.Invoke($"Name change {o} => {n}");

        string outputDir = Path.Combine(outputDirectoryRoot, RemoveInvalidFileNameChars(batchName));
        if (!Directory.Exists(outputDir)) {
            Directory.CreateDirectory(outputDir);
        }

        code.SetOutputPath(outputDir); // The code class's output path must either be set by setting the 'FileStructures' property or by calling the 'SetOutputPath' method: 33009 CC_OUTPUT_PATH_NOT_DEFINED
        code.ToolFile = toolFile; // Must be set
        code.BatchName = batchName;
        code.PicturesFilledCircles = false;
        code.GeneratePictures = true;
        code.SetPicturePath(machine.PictureOutputDirectory); // if generate pictures is set, the picture path must be set
        code.AllowPanelRotation = true; // Required to make Omni work

        code.WriteWinStepFiles = true;

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
        foreach (var job in _wsJobs) {
            ReleaseComObject(job);
        }
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
            ErrorEvent?.Invoke($"Failed to release COM object {obj} - {ex.Message}");
        }

    }

    private readonly record struct PartGroupKey(string MaterialName, double Thickness);

}
