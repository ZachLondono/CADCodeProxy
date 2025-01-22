using CADCode;
using CADCodeProxy.CSV;

namespace CADCodeProxy.Machining.Tokens;

public record CircularPocket : IRoutingToken, IMachiningOperation {

    public required string ToolName { get; init; }
    public required Point Center { get; init; }
    public required double Depth { get; init; }
    public required double Radius { get; init; }
    public int SequenceNumber { get; init; } = 0;
    public int NumberOfPasses { get; init; } = 0;
    public double FeedSpeed { get; init; }
    public double SpindleSpeed { get; init; }

    void IMachiningOperation.AddToCode(CADCodeCodeClass code, double xOffset, double yOffset) {

        code.DefinePocket(
            StartX: (float) (Center.X + xOffset),
            StartY: (float) (Center.Y - Radius + yOffset),
            StartZ: (float) Depth,
            EndX: (float) (Center.X + xOffset),
            EndY: (float) (Center.Y + Radius + yOffset),
            Endz: (float) Depth,
            CenterX: (float) (Center.X + xOffset),
            CenterY: (float) (Center.Y + yOffset),
            CenterZ: (float) Depth,
            Radius: (float) Radius,
            ArcDirection: ArcTypes.CC_CLOCKWISE_ARC,
            Offset: OffsetTypes.CC_OFFSET_NONE,
            OffsetAmount: 0,
            Rotation: RotationTypes.CC_ROTATION_AUTO,
            Overlap: 0,
            ToolName: ToolName,
            ToolDiameter: 0,
            FeedSpeed: (float) FeedSpeed,
            EntrySpeed: 0,
            RotationSpeed: (float) SpindleSpeed,
            NestedRouteSequence: SequenceNumber,
            Normal: new object?[] { null, 0, 0, 1, 0, null, null, null, 0, null, null, 1, null, 0 },
            InsideOut: false,
            NumberOfPasses: NumberOfPasses);

        code.DefinePocket(
            StartX: (float) (Center.X + xOffset),
            StartY: (float) (Center.Y + Radius + yOffset),
            StartZ: (float) Depth,
            EndX: (float) (Center.X + xOffset),
            EndY: (float) (Center.Y - Radius + yOffset),
            Endz: (float) Depth,
            CenterX: (float) (Center.X + xOffset),
            CenterY: (float) (Center.Y + yOffset),
            CenterZ: (float) Depth,
            Radius: (float) Radius,
            ArcDirection: ArcTypes.CC_CLOCKWISE_ARC,
            Offset: OffsetTypes.CC_OFFSET_NONE,
            OffsetAmount: 0,
            Rotation: RotationTypes.CC_ROTATION_AUTO,
            Overlap: 0,
            ToolName: ToolName,
            ToolDiameter: 0,
            FeedSpeed: (float) FeedSpeed,
            EntrySpeed: 0,
            RotationSpeed: (float) SpindleSpeed,
            NestedRouteSequence: SequenceNumber,
            Normal: new object?[] { null, 0, 0, 1, 0, null, null, null, 0, null, null, 1, null, 0 },
            InsideOut: false,
            NumberOfPasses: NumberOfPasses);

    }

    TokenRecord IToken.ToTokenRecord() {

        return new TokenRecord() {
            Name = "Pocket",
            CenterX = Center.X.ToString(),
            CenterY = Center.Y.ToString(),
            StartZ = Depth.ToString(),
            Radius = Radius.ToString(),
            ToolName = ToolName,
            SequenceNum = SequenceNumber == 0 ? "" : SequenceNumber.ToString(),
            NumberOfPasses = NumberOfPasses == 0 ? "" : NumberOfPasses.ToString(),
            FeedSpeed = FeedSpeed == 0 ? "" : FeedSpeed.ToString(),
            SpindleSpeed = SpindleSpeed == 0 ? "" : SpindleSpeed.ToString(),
        };

    }

    internal static CircularPocket FromTokenRecord(TokenRecord tokenRecord) {

        if (!tokenRecord.Name.Split('*', 2).First().Equals("pocket", StringComparison.InvariantCultureIgnoreCase)) {
            throw new InvalidOperationException($"Can not map token '{tokenRecord.Name}' to Circular Pocket.");
        }

        if (!double.TryParse(tokenRecord.CenterX, out double centerX)) {
            throw new InvalidOperationException("Center X value not specified or invalid for Circular Pocket operation");
        }

        if (!double.TryParse(tokenRecord.CenterY, out double centerY)) {
            throw new InvalidOperationException("Center Y value not specified or invalid for Circular Pocket operation");
        }

        if (!double.TryParse(tokenRecord.StartZ, out double startZ)) {
            throw new InvalidOperationException("Start Z value not specified or invalid for Circular Pocket operation");
        }

        if (!double.TryParse(tokenRecord.Radius, out double radius)) {
            throw new InvalidOperationException("Radius value not specified or invalid for Circular Pocket operation");
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
            Center = new(centerX, centerY),
            Depth = startZ,
            Radius = radius,
            SequenceNumber = sequenceNum,
            NumberOfPasses = numberOfPasses,
            FeedSpeed = feedSpeed,
            SpindleSpeed = spindleSpeed,
        };

    }

}