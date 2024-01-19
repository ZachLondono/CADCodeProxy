namespace CADCodeProxy.Results;

public class MachineGCodeGenerationResult {

    public required string MachineName { get; init; }
    public required string ToolFilePath { get; init; }
    public required string NestOutputDirectory { get; init; }
    public required string SingleProgramOutputDirectory { get; init; }
    public required string PictureOutputDirectory { get; init; }
    public required string LabelDatabaseOutputDirectory { get; init; }
    public required MaterialGCodeGenerationResult[] MaterialGCodeGenerationResults { get; init; }

}
