using CADCode;
using CADCodeProxy.CSV;

namespace CADCodeProxy.Machining;

public record FreePocketSegment : IToken, IMachiningOperation {

    public required string ToolName { get; init; }
    public required Point Start { get; init; }
    public required Point End { get; init; }
    public required double StartDepth { get; init; }
    public required double EndDepth { get; init; }
    public int SequenceNumber { get; init; } = 0;
    public int NumberOfPasses { get; init; } = 0;
    public double FeedSpeed { get; init; }
    public double SpindleSpeed { get; init; }

    void IMachiningOperation.AddToCode(CADCodeCodeClass code) {

        code.DefinePocket(
            StartX: (float) Start.X,
            StartY: (float) Start.Y,
            StartZ: (float) StartDepth,
            EndX: (float) End.X,
            EndY: (float) End.Y,
            Endz: (float) EndDepth,
            CenterX: 0,
            CenterY: 0,
            CenterZ: 0,
            Radius: 0,
            ArcDirection: ArcTypes.CC_UNKNOWN_ARC,
            Offset: OffsetTypes.CC_OFFSET_NONE,
            OffsetAmount: 0,
            Rotation: RotationTypes.CC_ROTATION_AUTO,
            Overlap: 0,
            ToolName: ToolName,
            ToolDiameter: 0,
            FeedSpeed: (float) FeedSpeed,
            EntrySpeed: 0,
            RotationSpeed: (float) SpindleSpeed, // TODO: Make sure this is actually spindle speed
            NestedRouteSequence: SequenceNumber,
            Normal: new object[] { 0, 0, 1 },
            InsideOut: true,
            NumberOfPasses: NumberOfPasses);

    }

    TokenRecord IToken.ToTokenRecord() {

        return new() {
            Name = "FreePocket",
            ToolName = ToolName,
            StartX = Start.X.ToString(),
            StartY = Start.Y.ToString(),
            StartZ = StartDepth.ToString(),
            EndX = End.X.ToString(),
            EndY = End.Y.ToString(),
            EndZ = EndDepth.ToString(),
            SequenceNum = SequenceNumber.ToString(),
            NumberOfPasses = NumberOfPasses.ToString(),
            FeedSpeed = FeedSpeed.ToString(),
            SpindleSpeed = SpindleSpeed.ToString()
        };

    }

    internal static FreePocketSegment FromTokenRecord(TokenRecord tokenRecord) {

        if (!tokenRecord.Name.Split('*',2).First().Equals("freepocket", StringComparison.InvariantCultureIgnoreCase)) {
            throw new InvalidOperationException($"Can not map token '{tokenRecord.Name}' to Free Pocket Segment.");
        }

        if (!double.TryParse(tokenRecord.StartX, out double startX)) {
            throw new InvalidOperationException("Start X value not specified or invalid for Free Pocket Segment");
        }

        if (!double.TryParse(tokenRecord.StartY, out double startY)) {
            throw new InvalidOperationException("Start Y value not specified or invalid for Free Pocket Segment");
        }

        if (!double.TryParse(tokenRecord.StartZ, out double startZ)) {
            throw new InvalidOperationException("Start Z value not specified or invalid for Free Pocket Segment");
        }

        if (!double.TryParse(tokenRecord.EndX, out double endX)) {
            throw new InvalidOperationException("End X value not specified or invalid for Free Pocket Segment");
        }

        if (!double.TryParse(tokenRecord.EndY, out double endY)) {
            throw new InvalidOperationException("End Y value not specified or invalid for Free Pocket Segment");
        }

        if (!double.TryParse(tokenRecord.EndZ, out double endZ)) {
            throw new InvalidOperationException("End Z value not specified or invalid for Free Pocket Segment");
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
            StartDepth = startZ,
            EndDepth = endZ,
            SequenceNumber = sequenceNum,
            NumberOfPasses = numberOfPasses,
            FeedSpeed = feedSpeed,
            SpindleSpeed = spindleSpeed,
        };

    }

}
