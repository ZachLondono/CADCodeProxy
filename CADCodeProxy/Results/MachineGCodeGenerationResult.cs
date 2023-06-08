namespace CADCodeProxy.Results;

public class MachineGCodeGenerationResult {

    public required string MachineName { get; init; }
    public required MaterialGCodeGenerationResult[] MaterialGCodeGenerationResults { get; init; }

}
