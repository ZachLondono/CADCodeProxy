using CADCode;
using CADCodeProxy.CSV;

namespace CADCodeProxy.Machining;

public record MultiBore : IToken {

    public string ToolName { get; init; } = string.Empty;
    public double ToolDiameter { get; init; } = 0;
    public Point Start { get; init; }
    public Point End { get; init; }
    public int HoleCount { get; init; }
    public double Spacing { get; init; }
    public double Depth { get; init; }

    public MultiBore(string toolName, Point start, Point end, int holeCount, double spacing, double depth) {
        ToolName = toolName;
        ToolDiameter = 0;
        Start = start;
        End = end;
        HoleCount = holeCount;
        Spacing = spacing;
        Depth = depth;
    }

    public MultiBore(double toolDiameter, Point start, Point end, int holeCount, double spacing, double depth) {
        ToolName = string.Empty;
        ToolDiameter = toolDiameter;
        Start = start;
        End = end;
        HoleCount = holeCount;
        Spacing = spacing;
        Depth = depth;
    }

    public MultiBore(string toolName, Point start, Point end, double spacing, double depth) {
        ToolName = toolName;
        ToolDiameter = 0;
        Start = start;
        End = end;
        HoleCount = 0;
        Spacing = spacing;
        Depth = depth;
    }

    public MultiBore(double toolDiameter, Point start, Point end, double spacing, double depth) {
        ToolName = string.Empty;
        ToolDiameter = toolDiameter;
        Start = start;
        End = end;
        HoleCount = 0;
        Spacing = spacing;
        Depth = depth;
    }

    void IToken.AddToCode(CADCodeCodeClass code) {

        code.MultiBore((float)Start.X,
                        (float)Start.Y,
                        (float)Depth,
                        (float)End.X,
                        (float)End.Y,
                        FaceTypes.CC_UPPER_FACE,
                        (float)ToolDiameter,
                        ToolName,
                        (float) Spacing,
                        0,
                        0,
                        "",
                        HoleCount);

    }

    TokenRecord IToken.ToTokenRecord() {

        return new() {
            Name = "MultiBore",
            StartX = Start.X.ToString(),
            StartY = Start.Y.ToString(),
            StartZ = Depth.ToString(),
            EndX = End.X.ToString(),
            EndY = End.Y.ToString(),
            Pitch = Spacing.ToString(),
            ToolName = ToolName,
            ToolDiameter = ToolDiameter.ToString()
        };

    }
}
