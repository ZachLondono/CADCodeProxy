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
    private readonly List<WS_Job> _wsJobs = new();
    // TODO: add a list of 'unreleased com objects' which can be released in the Dispose method, incase an exception is thrown and they have not yet been released

    public void Initialize() {

        _bootObj = new CADCodeBootObject() {
            MessageMethod = MessageTypes.CC_USE_EVENTS,
            //MessageMethod = MessageTypes.CC_USE_MESSAGEBOX
        };

        if (_bootObj is null) {
            throw new InvalidOperationException($"Could not initialize CADCode");
        }

        _bootObj.CreateError += (l, s) => {
            Console.WriteLine($"[ERROR] {l} - {s}");
            if (l == 33012) {
                throw new CADCodeAuthorizationException(s);
            }
        };

        var initResult = _bootObj.Init();
        
        if (initResult != 0) {
            throw new InvalidOperationException($"Could not initialize CADCode - {_bootObj.GetErrorString(initResult)}");
        }

    }

    public bool TryWriteWSBatch(string outputFilePath) {

        if (RuntimeInformation.ProcessArchitecture != Architecture.X86
            || !_wsJobs.Any()) {
            return false;
        }
        
        WS_Application app = new WS_Application();
        app.WriteError += (ec, s) => Console.WriteLine($"[WXML][ERR] {ec} - {s}");
        app.NodeError += (ec, s) => Console.WriteLine($"[WXML][ERR] {ec} - {s}");
        app.Jobs.Add(_wsJobs.First());

        var result = app.SaveFile(outputFilePath);
        Console.WriteLine($"Result from saving WSXML file: {result} - {_bootObj?.GetErrorString(result)}");

        Marshal.ReleaseComObject(app);
        foreach (var job in  _wsJobs) {
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

        var toolFile = CreateToolFile(_bootObj, machine.ToolFilePath);
        var files = CreateFiles(machine);
        var labels = CreateLabel(_bootObj, batch.Name, machine.LabelDatabaseOutputDirectory);
        var code = CreateCode(_bootObj, batch.Name, machine, toolFile);

        List<MaterialGCodeGenerationResult> materialResults = new();
        var groups = batch.Parts.GroupBy(p => new PartGroupKey(p.Material, p.Thickness));
        foreach (var group in groups) {
            var matResult = GenerateCodeForMaterialType(batch.InfoFields, group.Key, group.ToArray(), inventory, units, _bootObj, labels, files, code);
            materialResults.Add(matResult);
        }

        // TODO: add single programs. 'code.DoOutput(units, 0, 0);'

        if (RuntimeInformation.ProcessArchitecture == Architecture.X86) {
            var job = new WS_Job();
            var result = code.GetWINStepMachining(ref job, AllMachining: true);
            _wsJobs.Add(job);
        } else {
            Console.WriteLine("WSXML skipped because application is not x86");
        }

        ReleaseComObjects(labels, toolFile, files, code);

        return new() {
            MachineName = machine.Name,
            MaterialGCodeGenerationResults = materialResults.ToArray()
        };

    }

    public void GenerateProgramFromWinStepFile(string filePath, Machine machine) {

        if (_bootObj is null) {
            Initialize();
        }

        if (_bootObj is null) {
            throw new InvalidOperationException("Could not initialize CADCode");
        }

        var toolFIle = CreateToolFile(_bootObj, machine.ToolFilePath);
        var code = CreateCode(_bootObj, "", machine, toolFIle);

        var reader = _bootObj.CreateFileReader();
        reader.CodeObject = code;

        var result = reader.ReadWinStep(filePath);

        Console.WriteLine(_bootObj.GetErrorString(result));

    }

    private static MaterialGCodeGenerationResult GenerateCodeForMaterialType(InfoFields batchInfoFields, PartGroupKey partGroupKey, Machining.Part[] batchParts, InventoryItem[] inventory, UnitTypes units, CADCodeBootObject bootObj, CADCodeLabelClass labels, CADCodeFileClass files, CADCodeCodeClass code) {

        var optimizer = CreateOptimizer(bootObj, files);

        List<CutlistInventory> sheetStock = inventory.Where(i => i.MaterialName == partGroupKey.MaterialName && i.PanelThickness == partGroupKey.Thickness)
                                                    .Select(i => i.AsCutlistInventory())
                                                    .ToList();

        // TODO: Do something if there is no valid materials

        List<CADCode.Part> parts = batchParts.SelectMany(p => p.AsCADCodePart(units)).ToList();

        code.Border(1.0f, 1.0f, (float)partGroupKey.Thickness, units, OriginType.CC_LL, $"{partGroupKey.MaterialName} {partGroupKey.Thickness}", AxisTypes.CC_AUTO_AXIS);
        foreach (var part in batchParts) {
            part.AddToLabels(batchInfoFields, labels);
            part.AddToCode(code);
        }
        code.EndPanel();

        sheetStock.ForEach(ss => optimizer.AddSheetStockByRef(ss, units));
        parts.ForEach(p => optimizer.AddPartByRef(p));

        optimizer.Optimize(typeOptimizeMethod.CC_OPT_ANYKIND, code, GenerateResultName(), 0, 0, labels);

        var usedInventory = sheetStock.Select(UsedInventory.FromCutlistInventory)
                                        .Where(i => i.Qty > 0)
                                        .ToArray();

        var placedParts = parts.Where(p => p.PatternNumber != 0)
                                .Select(PlacedPart.FromPart)
                                .ToArray();

        var unplacedParts = optimizer.GetUnplacedParts();

        var programs = code.GetProcessedFileNames()
                            .AsEnumerable()
                            .Select(s => Path.GetFileName(s.Split(',').First().Trim()))
                            .ToArray();

        ReleaseComObject(optimizer); 

        return new MaterialGCodeGenerationResult() {
            MaterialName = partGroupKey.MaterialName,
            MaterialThickness = partGroupKey.Thickness,
            ProgramNames = programs,
            UnplacedParts = unplacedParts,
            PlacedParts = placedParts,
            UsedInventory = usedInventory,
        };

    }

    private static CADCodeToolFileClass CreateToolFile(CADCodeBootObject boot, string filePath) {
        var toolFile = boot.CreateToolFile();

        if (toolFile is null) throw new InvalidOperationException("Could not create tool file");

        int result = toolFile.ReadToolFile(filePath);
        if (result != 0) {
            throw new InvalidOperationException($"Could not load tool file from file '{filePath}'");
        }

        return toolFile;
    }

    private static CADCodeLabelClass CreateLabel(CADCodeBootObject boot, string batchName, string outputDirectory) {
        var labels = boot.CreateLabels();               // Stores the data that will be written to the label database

        if (labels is null) throw new InvalidOperationException("Could not create CADCode label object");

        labels.LabelModuleError += (l, s) => Console.WriteLine($"[LABL][ERROR] {l} - {s}");
        labels.Progress += (l) => Console.WriteLine($"[LABL][PROG] {l}");

        labels.JobName = batchName; // This is needed to set the label database's table name

        var fileName = RemoveInvalidFileNameChars(batchName);
        var directory = Path.Combine(outputDirectory, fileName);

        if (!Directory.Exists(directory)) {
            Directory.CreateDirectory(directory);
        }

        labels.LabelFileName = Path.Combine(outputDirectory, fileName, $"{fileName}.mdb"); // This is either a relative path (where the root directory is set by the CADCodeFileClass) or an absolute path

        return labels;
    }

    private static CADCodePanelOptimizerClass CreateOptimizer(CADCodeBootObject boot, CADCodeFileClass files) {
        var optimizer = boot.CreatePanelOptimizer();    // Does the work of optimizing a group of parts onto a group of available sheet stock

        if (optimizer is null) throw new InvalidOperationException("Could not create CADCode optimizer object");

        optimizer.OptimizeError += (l,s) => Console.WriteLine($"[OPTI][ERROR] {l} - {s}");
        optimizer.Progress += (l) => Console.WriteLine($"[OPTI][PROG] {l}");

        optimizer.FileLocations = files;
        optimizer.Settings(100, 20, 1, 12.53, 10, 0f);  // Error 33011 - Did not set optimizer settings `CADCodePanelOptimizer.Settings()`
        //optimizer.ToolFile = toolFile;  // error 33014 (CC_MISSING_SETTINGS) when setting tool file

        return optimizer;
    }

    private static CADCodeCodeClass CreateCode(CADCodeBootObject boot, string batchName, Machine machine, CADCodeToolFileClass toolFile) {
        var code = boot.CreateCode();                   // Stores the machining data needed for all the parts, and does the work of creating the individual part programs

        if (code is null) throw new InvalidOperationException("Could not create CADCode code object");

        code.MachiningInfo += (i) => Console.WriteLine($"[CODE][INFO] {i}");
        code.MachiningError += (l,s) => Console.WriteLine($"[CODE][ERROR] {l} - {s}");

        string outputDir = Path.Combine(machine.NestOutputDirectory, RemoveInvalidFileNameChars(batchName));
        if (!Directory.Exists(outputDir)) {
            Directory.CreateDirectory(outputDir);
        }

        code.SetOutputPath(outputDir); // The code class's output path must either be set by setting the 'FileStructures' property or by calling the 'SetOutputPath' method: 33009 CC_OUTPUT_PATH_NOT_DEFINED
        code.ToolFile = toolFile; // Must be set
        code.BatchName = batchName;
        code.PicturesFilledCircles = false;
        code.GeneratePictures = true;
        code.SetPicturePath(machine.PictureOutputDirectory); // if generate pictures is set, the picture path must be set

        code.WriteWinStepFiles = true;

        return code;

    }

    private static CADCodeFileClass CreateFiles(Machine machine) {
        return new CADCodeFileClass {
            ToolFilePath = machine.ToolFilePath,
            FileOutputDirectory = machine.NestOutputDirectory,
            LabelFilePath = machine.NestOutputDirectory,
            PictureFileLocation = machine.NestOutputDirectory,
            ReportsLocation = machine.NestOutputDirectory,
            StartingSubProgramNumber = 1,
            StartingProgramNumber = 1,
        };
    }

    private static string GenerateResultName() => new Random().Next(0, 100000).ToString("D6");

    private static string RemoveInvalidFileNameChars(string fileName) => string.Concat(fileName.Split(Path.GetInvalidFileNameChars()));

    private static void ReleaseComObjects(CADCodeLabelClass labels, CADCodeToolFileClass toolFile, CADCodeFileClass files, CADCodeCodeClass code) {
#pragma warning disable CA1416 // Validate platform compatibility
        ReleaseComObject(labels);
        ReleaseComObject(toolFile);
        ReleaseComObject(files);
        ReleaseComObject(code);
#pragma warning restore CA1416 // Validate platform compatibility
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

    private static void ReleaseComObject(object obj) {

        try {
            Marshal.ReleaseComObject(obj);
        } catch { }

    }

    private readonly record struct PartGroupKey(string MaterialName, double Thickness);

}
