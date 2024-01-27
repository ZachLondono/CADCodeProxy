using CADCode;
using CADCodeProxy.CNC;
using CADCodeProxy.Enums;
using CADCodeProxy.Events;
using CADCodeProxy.Machining;
using CADCodeProxy.Results;
using ErrorEventHandler = CADCodeProxy.Events.ErrorEventHandler;

namespace CADCodeProxy;

public class GCodeGenerator(LinearUnits units) {

    public event InfoEventHandler? InfoEvent;
    public event ErrorEventHandler? ErrorEvent;

    public event CADCodeInfoEventHandler? CADCodeInfoEvent;
    public event CADCodeProgressEventHandler? CADCodeProgressEvent;
    public event CADCodeErrorEventHandler? CADCodeErrorEvent;

	public LinearUnits Units { get; init; } = units;
    public string ApplicationName { get; set; } = "CADCodeProxy";

	public List<InventoryItem> Inventory { get; } = [];

    public GCodeGenerationResult GeneratePrograms(IEnumerable<Machine> machines, Batch batch) {

        if (batch.Parts.Length == 0) {
            InfoEvent?.Invoke("No parts in batch");
            return new GCodeGenerationResult() {
                WinStepReportFilePath = null,
                MachineResults = [] 
            };
        } else {
            InfoEvent?.Invoke($"{batch.Parts.Length} Parts in batch");
        }

        using var cadcode = new CADCodeProxy.CADCodeProxy() {
            LabelCreatorName = ApplicationName
        };

        cadcode.CADCodeProgressEvent += (i) => {
			CADCodeProgressEvent?.Invoke(i);
		};

		cadcode.CADCodeErrorEvent += (m) => {
            CADCodeErrorEvent?.Invoke(m);
        };

        cadcode.CADCodeInfoEvent += (m) => {
            CADCodeInfoEvent?.Invoke(m);
        };

        InfoEvent?.Invoke("Initializing CADCode proxy");
        cadcode.Initialize();

        var units = GetCCUnits();
        var inventory = Inventory.ToArray();

        List<MachineGCodeGenerationResult> machineResults = [];
        foreach (var machine in machines) {

            if (!IsMachineSettingValid(machine)) {
                ErrorEvent?.Invoke(new($"Skipping machine '{machine.Name}' because it is misconfigured."));
                continue;
            }

            InfoEvent?.Invoke($"Generating G-code for '{machine.Name}'");
            var result = cadcode.GenerateGCode(machine, batch, inventory, units);

            machineResults.Add(result);

            var programCount = result.MaterialGCodeGenerationResults.Sum(r => r.ProgramNames.Length);
            InfoEvent?.Invoke($"Generated '{programCount}' nested programs for '{machine.Name}'");

        }

        return new GCodeGenerationResult() {
            WinStepReportFilePath = null,
            MachineResults = [.. machineResults]
        };

    }

    internal bool IsMachineSettingValid(Machine machine) {

        bool isValid = true;

        if (string.IsNullOrWhiteSpace(machine.Name)) {
            ErrorEvent?.Invoke(new("Machine is missing a name"));
            isValid = false;
        }

        if (!File.Exists(machine.ToolFilePath)) {
            ErrorEvent?.Invoke(new($"Tool file doesn't exist or can not be accessed - '{machine.ToolFilePath}'"));
            isValid = false;
        }

        if (!File.Exists(machine.SinglePartToolFilePath)) {
            ErrorEvent?.Invoke(new($"Tool file doesn't exist or can not be accessed - '{machine.SinglePartToolFilePath}'"));
            isValid = false;
        }

        if (!Directory.Exists(machine.NestOutputDirectory)) {
            ErrorEvent?.Invoke(new($"Nest output directory doesn't exist or can not be accessed - '{machine.NestOutputDirectory}'"));
            isValid = false;
        }

        if (!Directory.Exists(machine.SingleProgramOutputDirectory)) {
            ErrorEvent?.Invoke(new($"Single program output directory doesn't exist or can not be accessed - '{machine.SingleProgramOutputDirectory}'"));
            isValid = false;
        }

        if (!Directory.Exists(machine.PictureOutputDirectory)) {
            ErrorEvent?.Invoke(new($"Picture output directory doesn't exist or can not be accessed - '{machine.PictureOutputDirectory}'"));
            isValid = false;
        }

        if (!Directory.Exists(machine.LabelDatabaseOutputDirectory)) {
            ErrorEvent?.Invoke(new($"Label output directory doesn't exist or can not be accessed - '{machine.LabelDatabaseOutputDirectory}'"));
            isValid = false;
        }

        return isValid;

    }

    internal UnitTypes GetCCUnits() => Units switch {
        LinearUnits.Millimeters => UnitTypes.CC_U_METRIC,
        LinearUnits.Inches => UnitTypes.CC_U_INCH,
        _ => UnitTypes.CC_U_AUTO
        //UnitTypes.CC_U_FRACTIONS // not sure what this unit type is
    };

}
