using CADCode;
using CADCodeProxy.CSV;

namespace CADCodeProxy.Machining;

public class MultiBore : IToken {

    public string ToolName { get; set; } = string.Empty;
    public double ToolDiameter { get; set; } = 0;
    public Point Start { get; set; }
    public Point End { get; set; }
    public int HoleCount { get; set; }
    public double Spacing { get; set; }
    public double Depth { get; set; }
    public int SequenceNumber { get; set; } = 0;
    public int NumberOfPasses { get; set; } = 0;

    public MultiBore(string toolName, Point start, Point end, int holeCount, double spacing, double depth, int sequenceNumber = 0, int numberOfPasses = 0) {
        ToolName = toolName;
        ToolDiameter = 0;
        Start = start;
        End = end;
        HoleCount = holeCount;
        Spacing = spacing;
        Depth = depth;
        SequenceNumber = sequenceNumber;
        NumberOfPasses = numberOfPasses;
    }

    public MultiBore(double toolDiameter, Point start, Point end, int holeCount, double spacing, double depth, int sequenceNumber = 0, int numberOfPasses = 0) {
        ToolName = string.Empty;
        ToolDiameter = toolDiameter;
        Start = start;
        End = end;
        HoleCount = holeCount;
        Spacing = spacing;
        Depth = depth;
        SequenceNumber = sequenceNumber;
        NumberOfPasses = numberOfPasses;
    }

    public MultiBore(string toolName, Point start, Point end, double spacing, double depth, int sequenceNumber = 0, int numberOfPasses = 0) {
        ToolName = toolName;
        ToolDiameter = 0;
        Start = start;
        End = end;
        HoleCount = 0;
        Spacing = spacing;
        Depth = depth;
        SequenceNumber = sequenceNumber;
        NumberOfPasses = numberOfPasses;
    }

    public MultiBore(double toolDiameter, Point start, Point end, double spacing, double depth, int sequenceNumber = 0, int numberOfPasses = 0) {
        ToolName = string.Empty;
        ToolDiameter = toolDiameter;
        Start = start;
        End = end;
        HoleCount = 0;
        Spacing = spacing;
        Depth = depth;
        SequenceNumber = sequenceNumber;
        NumberOfPasses = numberOfPasses;
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
                        HoleCount,
                        SequenceNumber: SequenceNumber,
                        NumberOfPasses: NumberOfPasses);

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
            ToolDiameter = ToolDiameter.ToString(),
            SequenceNum = SequenceNumber == 0 ? "" : SequenceNumber.ToString(),
            NumberOfPasses = NumberOfPasses == 0 ? "" : NumberOfPasses.ToString()
        };

    }
}
