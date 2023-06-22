using CADCode;
using CADCodeProxy.CNC;
using CADCodeProxy.Enums;
using CADCodeProxy.Machining;
using CADCodeProxy.Results;

namespace CADCodeProxy;

public class GCodeGenerator {

    public LinearUnits Units { get; init; }

    public GCodeGenerator(LinearUnits units) {
        Units = units;
    }

    public List<InventoryItem> Inventory { get; } = new();

    public GCodeGenerationResult GeneratePrograms(IEnumerable<Machine> machines, Batch batch, string wsReportOutputDirectory) {

        if (!batch.Parts.Any()) {
            return new GCodeGenerationResult() {
                WinStepReportFilePath = null,
                MachineResults = Array.Empty<MachineGCodeGenerationResult>()
            };
        }

        using var cadcode = new CADCodeProxy.CADCodeProxy();

        cadcode.Initialize();
        
        var units = GetCCUnits();
        var inventory = Inventory.ToArray();
        
        List<MachineGCodeGenerationResult> machineResults = new();
        foreach (var machine in machines) {
        
            var result = cadcode.GenerateGCode(machine, batch, inventory, units);
        
            machineResults.Add(result);
        
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
