using CADCode;
using CADCodeProxy.CADCodeProxy;
using CADCodeProxy.CSV;
using CADCodeProxy.Enums;

namespace CADCodeProxy.Machining;

public class Arc : IToken, IMachiningOperation {

    public required string ToolName { get; set; }
    public required Point Start { get; set; }
    public required Point End { get; set; }
    public required double Radius { get; set; }
    public required double StartDepth { get; set; }
    public required double EndDepth { get; set; }
    public required ArcDirection Direction { get; set; }
    public Offset Offset { get; set; } = Offset.Center;
    public int SequenceNumber { get; set; } = 0;
    public int NumberOfPasses { get; set; } = 0;
    public double FeedSpeed { get; set; } = 0;
    public double SpindleSpeed { get; set; } = 0;

    void IMachiningOperation.AddToCode(CADCodeCodeClass code) {

        code.RouteArc(StartX: (float)Start.X,
                        StartY: (float)Start.Y,
                        StartZ: (float)StartDepth,
                        EndX: (float)End.X,
                        EndY: (float)End.Y,
                        Endz: (float)EndDepth,
                        ToolName: ToolName,
                        ToolDiameter: 0f,
                        Offset: Offset.AsCCOffset(),
                        OffsetAmount: 0,
                        ToolRotation: RotationTypes.CC_ROTATION_AUTO,
                        Face: 0,
                        FeedSpeed: (float)FeedSpeed,
                        EntrySpeed: 0,
                        SpindleSpeed: (float)SpindleSpeed,
                        CornerFeed: 0,
                        RType: "",
                        CenterX: 0,
                        CenterY: 0,
                        OffsetX: 0,
                        OffsetY: 0,
                        Radius: (float)Radius,
                        ArcDirection: Direction.AsCCArcType(),
                        Bulge: 0,
                        NestedRouteSequence: SequenceNumber,
                        NumberOfPasses: NumberOfPasses);

    }

    TokenRecord IToken.ToTokenRecord() {

        string direction = Direction switch {
            ArcDirection.ClockWise => "CW",
            ArcDirection.CounterClockWise => "CCW",
            _ => throw new InvalidOperationException("Arc direction must be specified")
        };

        return new() {
            Name = "Arc",
            StartX = Start.X.ToString(),
            StartY = Start.Y.ToString(),
            StartZ = StartDepth.ToString(),
            EndX = End.X.ToString(),
            EndY = End.Y.ToString(),
            EndZ = EndDepth.ToString(),
            Radius = Radius.ToString(),
            OffsetSide = Offset.ToCSVCode(),
            ToolName = ToolName,
            ArcDirection = direction,
            SequenceNum = SequenceNumber == 0 ? "" : SequenceNumber.ToString(),
            NumberOfPasses = NumberOfPasses == 0 ? "" : NumberOfPasses.ToString(),
            FeedSpeed = FeedSpeed.ToString(),
            SpindleSpeed = SpindleSpeed.ToString(),
        };

    }

    internal static Arc FromTokenRecord(TokenRecord tokenRecord) {

        if (!tokenRecord.Name.Equals("arc", StringComparison.InvariantCultureIgnoreCase)
            && !tokenRecord.Name.Equals("cwarc", StringComparison.InvariantCultureIgnoreCase)
            && !tokenRecord.Name.Equals("ccwarc", StringComparison.InvariantCultureIgnoreCase)) {
            throw new InvalidOperationException($"Can not map token '{tokenRecord.Name}' to arc.");
        }

        if (!double.TryParse(tokenRecord.StartX, out double startX)) {
            throw new InvalidOperationException("Start X value not specified or invalid for Arc operation");
        }

        if (!double.TryParse(tokenRecord.StartY, out double startY)) {
            throw new InvalidOperationException("Start Y value not specified or invalid for Arc operation");
        }

        if (!double.TryParse(tokenRecord.EndX, out double endX)) {
            throw new InvalidOperationException("End X value not specified or invalid for Arc operation");
        }

        if (!double.TryParse(tokenRecord.EndY, out double endY)) {
            throw new InvalidOperationException("End Y value not specified or invalid for Arc operation");
        }

        if (!double.TryParse(tokenRecord.StartZ, out double startDepth)) {
            throw new InvalidOperationException("Start Z value not specified or invalid for Arc operation");
        }

        if (!double.TryParse(tokenRecord.EndZ, out double endDepth)) {
            throw new InvalidOperationException("End Z value not specified or invalid for Arc operation");
        }

        if (!double.TryParse(tokenRecord.Radius, out double radius)) {
            throw new InvalidOperationException("Radius value not specified or invalid for Arc operation");
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

        Offset offset = OffsetExtension.FromCSVCode(tokenRecord.OffsetSide);

        var arcDirection = tokenRecord.Name.ToLower() switch {
            "cwarc" => ArcDirection.ClockWise,
            "ccwarc" => ArcDirection.CounterClockWise,
            "arc" => tokenRecord.ArcDirection.ToLower() switch {
                "cw" => ArcDirection.ClockWise,
                "ccw" => ArcDirection.CounterClockWise,
                _ => ArcDirection.Unknown
            },
            _ => ArcDirection.Unknown
        };

        return new() {
            ToolName = tokenRecord.ToolName,
            Start = new(startX, startY),
            End = new(endX, endY),
            Radius = radius,
            StartDepth = startDepth,
            EndDepth = endDepth,
            Direction = arcDirection,
            Offset = offset,
            SequenceNumber = sequenceNum,
            NumberOfPasses = numberOfPasses,
            FeedSpeed = feedSpeed,
            SpindleSpeed = spindleSpeed
        };

    }

}