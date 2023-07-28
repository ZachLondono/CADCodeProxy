using CsvHelper.Configuration.Attributes;

namespace CADCodeProxy.CSV;

public class TokenRecord {

    public string Name { get; set; } = string.Empty;
    public string StartX { get; set; } = string.Empty;
    public string StartY { get; set; } = string.Empty;
    public string StartZ { get; set; } = string.Empty;
    public string EndX { get; set; } = string.Empty;
    public string EndY { get; set; } = string.Empty;
    public string EndZ { get; set; } = string.Empty;
    public string CenterX { get; set; } = string.Empty;
    public string CenterY { get; set; } = string.Empty;
    public string PocketX { get; set; } = string.Empty;
    public string PocketY { get; set; } = string.Empty;
    public string Radius { get; set; } = string.Empty;
    public string Pitch { get; set; } = string.Empty;
    public string NumberOfPasses { get; set; } = string.Empty;
    public string OffsetSide { get; set; } = string.Empty;
    public string ToolName { get; set; } = string.Empty;
    public string ToolDiameter { get; set; } = string.Empty;
    public string SequenceNum { get; set; } = string.Empty;
    public string ArcDirection { get; set; } = string.Empty;
    public string StartAngle { get; set; } = string.Empty;
    public string EndAngle { get; set; } = string.Empty;
    public string FeedSpeed { get; set; } = string.Empty;
    public string SpindleSpeed { get; set; } = string.Empty;

}
