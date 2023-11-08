using CADCode;
using CADCodeProxy.CNC;
using CADCodeProxy.Enums;
using CADCodeProxy.Machining;
using CADCodeProxy.Results;

namespace CADCodeProxy;

public class GCodeGenerator {

    public delegate void GenerationEventHandler(string message);
    public event GenerationEventHandler? GenerationEvent;

    public delegate void CADCodeProgressEventHandler(int value);
    public event CADCodeProgressEventHandler? CADCodeProgressEvent;

    public delegate void CADCodeErrorEventHandler(int value);
    public event CADCodeErrorEventHandler? CADCodeErrorEvent;

    public LinearUnits Units { get; init; }

    public GCodeGenerator(LinearUnits units) {
        Units = units;
    }

    public List<InventoryItem> Inventory { get; } = new();

    public GCodeGenerationResult GeneratePrograms(IEnumerable<Machine> machines, Batch batch, string wsReportOutputDirectory) {

        if (!batch.Parts.Any()) {
            GenerationEvent?.Invoke("No parts in batch");
            return new GCodeGenerationResult() {
                WinStepReportFilePath = null,
                MachineResults = Array.Empty<MachineGCodeGenerationResult>()
            };
        }

        using var cadcode = new CADCodeProxy.CADCodeProxy();

        if (CADCodeProgressEvent is not null) {
            cadcode.ProgressEvent += CADCodeProgressEvent.Invoke;
        }
    
        if (CADCodeErrorEvent is not null) {
            cadcode.ProgressEvent += CADCodeErrorEvent.Invoke;
        }

        GenerationEvent?.Invoke("Initializing CADCode proxy");
        cadcode.Initialize();
        
        var units = GetCCUnits();
        var inventory = Inventory.ToArray();

        List<MachineGCodeGenerationResult> machineResults = new();
        foreach (var machine in machines) {
        
            GenerationEvent?.Invoke($"Generating G-code for '{machine.Name}'");
            var result = cadcode.GenerateGCode(machine, batch, inventory, units);

            machineResults.Add(result);

            var programCount = result.MaterialGCodeGenerationResults.Sum(r => r.ProgramNames.Length);
            GenerationEvent?.Invoke($"Generated '{programCount}' nested programs for '{machine.Name}'");
        
        }
        
        var fileName = string.Concat(batch.Name.Split(Path.GetInvalidFileNameChars()));
        string? wsReportFilePath = Path.Combine(wsReportOutputDirectory, $"{fileName}.xml");
        if (!cadcode.TryWriteWSBatch(wsReportFilePath)) {
            wsReportFilePath = null;
        }
        
        return new GCodeGenerationResult() {
            WinStepReportFilePath = wsReportFilePath,
            MachineResults = machineResults.ToArray()
        };

    }

    //public GCodeGenerationResult GenerateProgramFromWSXMLFile(string filePath, Machine machine) {

    //    using var cadcode = new CADCodeProxy.CADCodeProxy();

    //    cadcode.GenerateProgramFromWinStepFile(filePath, machine);

    //    return new GCodeGenerationResult() {
    //        WinStepReportFilePath = null,
    //        MachineResults = Array.Empty<MachineGCodeGenerationResult>()
    //    };

    //}

    internal UnitTypes GetCCUnits() => Units switch {
        LinearUnits.Millimeters => UnitTypes.CC_U_METRIC,
        LinearUnits.Inches => UnitTypes.CC_U_INCH,
        _ => UnitTypes.CC_U_AUTO
        //UnitTypes.CC_U_FRACTIONS // not sure what this unit type is
    };

}
