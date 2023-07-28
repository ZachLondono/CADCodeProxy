using CADCode;
using CADCodeProxy.CSV;

namespace CADCodeProxy.Machining;

public record Bore : IToken {

    public string ToolName { get; init; } = string.Empty;
    public double ToolDiameter { get; init; } = 0;
    public Point Position { get; init; }
    public double Depth { get; init; }
    public int SequenceNumber { get; init; } = 0;
    public int NumberOfPasses { get; init; } = 0;

    public Bore(string toolName, Point position, double depth, int sequenceNumber = 0, int numberOfPasses = 0) {
        ToolName = toolName;
        ToolDiameter = 0;
        Position = position;
        Depth = depth;
        SequenceNumber = sequenceNumber;
        NumberOfPasses = numberOfPasses;
    }

    public Bore(double toolDiameter, Point position, double depth, int sequenceNumber = 0, int numberOfPasses = 0) {
        ToolName = string.Empty;
        ToolDiameter = toolDiameter;
        Position = position;
        Depth = depth;
        SequenceNumber = sequenceNumber;
        NumberOfPasses = numberOfPasses;
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
                    "",
                    SequenceNumber: SequenceNumber,
                    NumberOfPasses: NumberOfPasses);

    }

    TokenRecord IToken.ToTokenRecord() {

        return new() {
            Name = "Bore",
            StartX = Position.X.ToString(),
            StartY = Position.Y.ToString(),
            StartZ = Depth.ToString(),
            ToolName = ToolName,
            ToolDiameter = ToolDiameter.ToString(),
            SequenceNum = SequenceNumber == 0 ? "" : SequenceNumber.ToString(),
            NumberOfPasses = NumberOfPasses == 0 ? "" : NumberOfPasses.ToString()
        };

    }

}
