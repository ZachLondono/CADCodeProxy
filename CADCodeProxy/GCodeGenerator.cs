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
    public string ApplicationName { get; set; } = "CADCodeProxy";

	public List<InventoryItem> Inventory { get; } = [];

    public GCodeGenerationResult GeneratePrograms(IEnumerable<Machine> machines, Batch batch) {

        if (batch.Parts.Length == 0) {
            GenerationEvent?.Invoke("No parts in batch");
            return new GCodeGenerationResult() {
                WinStepReportFilePath = null,
                MachineResults = [] 
            };
        } else {
            GenerationEvent?.Invoke($"{batch.Parts.Length} Parts in batch");
        }

        using var cadcode = new CADCodeProxy.CADCodeProxy() {
            LabelCreatorName = ApplicationName
        };

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

        List<MachineGCodeGenerationResult> machineResults = [];
        foreach (var machine in machines) {

            if (!IsMachineSettingValid(machine)) {
                CADCodeErrorEvent?.Invoke($"Skipping machine '{machine.Name}' because it is misconfigured.");
                continue;
            }

            GenerationEvent?.Invoke($"Generating G-code for '{machine.Name}'");
            var result = cadcode.GenerateGCode(machine, batch, inventory, units);

            machineResults.Add(result);

            var programCount = result.MaterialGCodeGenerationResults.Sum(r => r.ProgramNames.Length);
            GenerationEvent?.Invoke($"Generated '{programCount}' nested programs for '{machine.Name}'");

        }

        return new GCodeGenerationResult() {
            WinStepReportFilePath = null,
            MachineResults = [.. machineResults]
        };

    }

    internal bool IsMachineSettingValid(Machine machine) {

        bool isValid = true;

        if (string.IsNullOrWhiteSpace(machine.Name)) {
            CADCodeErrorEvent?.Invoke("Machine is missing a name");
            isValid = false;
        }

        if (!File.Exists(machine.ToolFilePath)) {
            CADCodeErrorEvent?.Invoke($"Tool file doesn't exist or can not be accessed - '{machine.ToolFilePath}'");
            isValid = false;
        }

        if (!File.Exists(machine.SinglePartToolFilePath)) {
            CADCodeErrorEvent?.Invoke($"Tool file doesn't exist or can not be accessed - '{machine.SinglePartToolFilePath}'");
            isValid = false;
        }

        if (!Directory.Exists(machine.NestOutputDirectory)) {
            CADCodeErrorEvent?.Invoke($"Nest output directory doesn't exist or can not be accessed - '{machine.NestOutputDirectory}'");
            isValid = false;
        }

        if (!Directory.Exists(machine.SingleProgramOutputDirectory)) {
            CADCodeErrorEvent?.Invoke($"Single program output directory doesn't exist or can not be accessed - '{machine.SingleProgramOutputDirectory}'");
            isValid = false;
        }

        if (!Directory.Exists(machine.PictureOutputDirectory)) {
            CADCodeErrorEvent?.Invoke($"Picture output directory doesn't exist or can not be accessed - '{machine.PictureOutputDirectory}'");
            isValid = false;
        }

        if (!Directory.Exists(machine.LabelDatabaseOutputDirectory)) {
            CADCodeErrorEvent?.Invoke($"Label output directory doesn't exist or can not be accessed - '{machine.LabelDatabaseOutputDirectory}'");
            isValid = false;
        }

        return isValid;

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
