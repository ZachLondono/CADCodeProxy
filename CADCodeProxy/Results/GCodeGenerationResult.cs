namespace CADCodeProxy.Results;

public class GCodeGenerationResult {

    public required string? WinStepReportFilePath { get; init; }
    public required MachineGCodeGenerationResult[] MachineResults { get; init; }

}
