namespace CADCodeProxy.CNC;

public record Machine {

    public required string Name { get; set; }
    public required string ToolFilePath { get; set; }
    public required string SinglePartToolFilePath { get; set; }
    public required string NestOutputDirectory { get; set; }
    public required string SingleProgramOutputDirectory { get; set; }
    public required string PictureOutputDirectory { get; set; }
    public required string LabelDatabaseOutputDirectory { get; set; }

}
