using CADCode;
using CADCodeProxy.CNC;
using CADCodeProxy.Enums;
using CADCodeProxy.Machining;
using CADCodeProxy.Results;

namespace CADCodeProxy;

public class GCodeGenerator(LinearUnits units) {

    public delegate void GenerationEventHandler(string message);
    public event GenerationEventHandler? GenerationEvent;

    public delegate void CADCodeProgressEventHandler(int value);
    public event CADCodeProgressEventHandler? CADCodeProgressEvent;

    public delegate void CADCodeErrorEventHandler(string message);
    public event CADCodeErrorEventHandler? CADCodeErrorEvent;

	public LinearUnits Units { get; init; } = units;

	public List<InventoryItem> Inventory { get; } = [];

    public GCodeGenerationResult GeneratePrograms(IEnumerable<Machine> machines, Batch batch) {

        if (!batch.Parts.Any()) {
            GenerationEvent?.Invoke("No parts in batch");
            return new GCodeGenerationResult() {
                WinStepReportFilePath = null,
                MachineResults = Array.Empty<MachineGCodeGenerationResult>()
            };
        } else {
            GenerationEvent?.Invoke($"{batch.Parts.Length} Parts in batch");
        }

        using var cadcode = new CADCodeProxy.CADCodeProxy();

        cadcode.ProgressEvent += (i) => {
			CADCodeProgressEvent?.Invoke(i);
		};

		cadcode.ErrorEvent += (m) => {
            CADCodeErrorEvent?.Invoke(m);
        };

        cadcode.InformationEvent += (m) => {
            GenerationEvent?.Invoke(m);
        };

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

        return new GCodeGenerationResult() {
            WinStepReportFilePath = null,
            MachineResults = machineResults.ToArray()
        };

    }

    /*
    public GCodeGenerationResult GenerateProgramFromWSXMLFile(string filePath, Machine machine) {
        using var cadcode = new CADCodeProxy.CADCodeProxy();
        cadcode.GenerateProgramFromWinStepFile(filePath, machine);
        return new GCodeGenerationResult() {
            WinStepReportFilePath = null,
            MachineResults = Array.Empty<MachineGCodeGenerationResult>()
        };
    }
    */

    internal UnitTypes GetCCUnits() => Units switch {
        LinearUnits.Millimeters => UnitTypes.CC_U_METRIC,
        LinearUnits.Inches => UnitTypes.CC_U_INCH,
        _ => UnitTypes.CC_U_AUTO
        //UnitTypes.CC_U_FRACTIONS // not sure what this unit type is
    };

}
