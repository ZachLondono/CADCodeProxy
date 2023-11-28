using CADCode;
using CADCodeProxy.CSV;

namespace CADCodeProxy.Machining;

public record OutlineSegment : IToken, IMachiningOperation {

    public required string ToolName { get; init; }
    public required Point Start { get; init; }
    public required Point End { get; init; }
    public required double StartDepth { get; init; }
    public required double EndDepth { get; init; }
    public int SequenceNumber { get; init; } = 0;
    public int NumberOfPasses { get; init; } = 0;
    public double FeedSpeed { get; init; } = 0;
    public double SpindleSpeed { get; init; } = 0;

    void IMachiningOperation.AddToCode(CADCodeCodeClass code) {

        code.DefineOutLine(
                            StartX: (float)Start.X,
                            StartY: (float)Start.Y,
                            EndX: (float)End.X,
                            EndY: (float)End.Y,
                            CenterX: 0,
                            CenterY: 0,
                            Radius: 0,
                            ArcDirection: ArcTypes.CC_UNKNOWN_ARC,
                            Offset: OffsetTypes.CC_OFFSET_OUTSIDE,
                            ToolName: ToolName,
                            FeedSpeed: (float)FeedSpeed,
                            SpindleSpeed: (float)SpindleSpeed,
                            NestedRouteSequence: SequenceNumber,
                            NumberOfPasses: NumberOfPasses,
                            KerfClearance: 0);

    }

    TokenRecord IToken.ToTokenRecord() {

        return new() {
            Name = "OUTLINE",
            StartX = Start.X.ToString(),
            StartY = Start.Y.ToString(),
            StartZ = StartDepth.ToString(),
            EndX = End.X.ToString(),
            EndY = End.Y.ToString(),
            EndZ = EndDepth.ToString(),
            ToolName = ToolName,
            SequenceNum = SequenceNumber == 0 ? "" : SequenceNumber.ToString(),
            NumberOfPasses = NumberOfPasses == 0 ? "" : NumberOfPasses.ToString(),
            FeedSpeed = FeedSpeed.ToString(),
            SpindleSpeed = SpindleSpeed.ToString()
        };

    }

    internal static OutlineSegment FromTokenRecord(TokenRecord tokenRecord) {

        if (!tokenRecord.Name.Split('*',2).First().Equals("shape", StringComparison.InvariantCultureIgnoreCase)
            && !tokenRecord.Name.Equals("outline", StringComparison.InvariantCultureIgnoreCase)) {
            throw new InvalidOperationException($"Can not map token '{tokenRecord.Name}' to outline/shape segment.");
        }

        if (!double.TryParse(tokenRecord.StartX, out double startX)) {
            throw new InvalidOperationException("Start X value not specified or invalid for Outline operation");
        }

        if (!double.TryParse(tokenRecord.StartY, out double startY)) {
            throw new InvalidOperationException("Start Y value not specified or invalid for Outline operation");
        }

        if (!double.TryParse(tokenRecord.EndX, out double endX)) {
            throw new InvalidOperationException("End X value not specified or invalid for Outline operation");
        }

        if (!double.TryParse(tokenRecord.EndY, out double endY)) {
            throw new InvalidOperationException("End Y value not specified or invalid for Outline operation");
        }

        if (!double.TryParse(tokenRecord.StartZ, out double startDepth)) {
            throw new InvalidOperationException("Start Z value not specified or invalid for Outline operation");
        }

        if (!double.TryParse(tokenRecord.EndZ, out double endDepth)) {
            throw new InvalidOperationException("End Z value not specified or invalid for Outline operation");
        }

        if (!int.TryParse(tokenRecord.SequenceNum, out int sequenceNum)) {
            sequenceNum = 0;
        }

        if (!int.TryParse(tokenRecord.NumberOfPasses, out int numberOfPasses)) {
            numberOfPasses = 0;
        }

        if (!double.TryParse(tokenRecord.FeedSpeed, out double feedSpeed)) {
            feedSpeed = 0;
        }

        if (!double.TryParse(tokenRecord.SpindleSpeed, out double spindleSpeed)) {
            spindleSpeed = 0;
        }

        return new() {
            ToolName = tokenRecord.ToolName,
            Start = new(startX, startY),
            End = new(endX, endY),
            StartDepth = startDepth,
            EndDepth = endDepth,
            SequenceNumber = sequenceNum,
            NumberOfPasses = numberOfPasses,
            FeedSpeed = feedSpeed,
            SpindleSpeed = spindleSpeed
        };

    }

}
