using CADCode;
using CADCodeProxy.CSV;

namespace CADCodeProxy.Machining;

public class MultiBore : IBoringToken, IMachiningOperation {

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

    void IMachiningOperation.AddToCode(CADCodeCodeClass code) {

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

        // TODO: Maybe write HoleCount to CSV record
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

    internal static MultiBore FromTokenRecord(TokenRecord tokenRecord) {

        if (!tokenRecord.Name.Equals("multibore", StringComparison.InvariantCultureIgnoreCase)) {
            throw new InvalidOperationException($"Can not map token '{tokenRecord.Name}' to multibore.");
        }

        if (!double.TryParse(tokenRecord.StartX, out double startX)) {
            throw new InvalidOperationException("Start X value not specified or invalid for Bore operation");
        }

        if (!double.TryParse(tokenRecord.StartY, out double startY)) {
            throw new InvalidOperationException("Start Y value not specified or invalid for Bore operation");
        }

        if (!double.TryParse(tokenRecord.EndX, out double endX)) {
            throw new InvalidOperationException("End X value not specified or invalid for Bore operation");
        }

        if (!double.TryParse(tokenRecord.EndY, out double endY)) {
            throw new InvalidOperationException("End Y value not specified or invalid for Bore operation");
        }
        
        if (!double.TryParse(tokenRecord.StartZ, out double depth)) {
            throw new InvalidOperationException("Start Z value not specified or invalid for Bore operation");
        }

        if (!double.TryParse(tokenRecord.Pitch, out double spacing)) {
            throw new InvalidOperationException("Pitch value not specified or invalid for Bore operation");
        }

        if (!int.TryParse(tokenRecord.SequenceNum, out int sequenceNum)) {
            sequenceNum = 0;
        }

        if (!int.TryParse(tokenRecord.NumberOfPasses, out int numberOfPasses)) {
            numberOfPasses = 0;
        }

        Point startPosition = new(startX, startY);
        Point endPosition = new(endX, endY);

        if (string.IsNullOrEmpty(tokenRecord.ToolName)) {

            if (!double.TryParse(tokenRecord.ToolDiameter, out double toolDiameter)) {
                throw new InvalidOperationException("Tool value not specified or invalid for Bore operation");
            }

            return new(toolDiameter, startPosition, endPosition, spacing, depth, sequenceNum, numberOfPasses);

        } else {

            return new(tokenRecord.ToolName, startPosition, endPosition, spacing, depth, sequenceNum, numberOfPasses);

        }

    }

}
