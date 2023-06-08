using CADCode;

namespace CADCodeProxy.Machining;

public record Bore : IToken {

    public string ToolName { get; init; } = string.Empty;
    public double ToolDiameter { get; init; } = 0;
    public Point Position { get; init; }
    public double Depth { get; init; }

    public Bore(string toolName, Point position, double depth) {
        ToolName = toolName;
        ToolDiameter = 0;
        Position = position;
        Depth = depth;
    }

    public Bore(double toolDiameter, Point position, double depth) {
        ToolName = string.Empty;
        ToolDiameter = toolDiameter;
        Position = position;
        Depth = depth;
    }

    void IToken.AddToCode(CADCodeCodeClass code) {

        code.Bore((float)Position.X,
                    (float)Position.Y,
                    (float)Depth,
                    FaceTypes.CC_UPPER_FACE,
                    (float) ToolDiameter,
                    ToolName,
                    "",
                    "",
                    0f,
                    0f,
                    "");

    }

}
