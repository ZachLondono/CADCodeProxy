using CADCodeProxy.CSV;
using CADCodeProxy.Enums;

namespace CADCodeProxy.Machining;

public record Rectangle : IToken {

    public required string ToolName { get; init; }
    public required Point CornerA { get; init; }
    public required Point CornerB { get; init; }
    public required Point CornerC { get; init; }
    public required Point CornerD { get; init; }
    public required double StartDepth { get; init; }
    public required double EndDepth { get; init; }
    public Offset Offset { get; init; } = Offset.Center; // TODO: figure out how to handle inside and outside, I think it will just work if passed to CADCode
    public int SequenceNumber { get; init; } = 0;
    public int NumberOfPasses { get; init; } = 0;
    public double FeedSpeed { get; init; } = 0;
    public double SpindleSpeed { get; init; } = 0;
    public double Radius { get; init; } = 0;

    internal IToken[] GetComponentTokens() {

        Route CreateRoute(Point start, Point end) => new() {
            ToolName = ToolName,
            Start = start,
            End = end,
            StartDepth = StartDepth,
            EndDepth = EndDepth,
            Offset = Offset,
            SequenceNumber = SequenceNumber,
            NumberOfPasses = NumberOfPasses,
            FeedSpeed = FeedSpeed,
            SpindleSpeed = SpindleSpeed
        };

        List<IToken> tokens = [];

        Point start = new(
                (CornerA.X + CornerB.X) / 2,
                (CornerA.Y + CornerB.Y) / 2
            );

        tokens.Add(CreateRoute(start, CornerB));
        if (Radius != 0) tokens.Add(new Fillet() { Radius = Radius });
        tokens.Add(CreateRoute(CornerB, CornerC));
        if (Radius != 0) tokens.Add(new Fillet() { Radius = Radius });
        tokens.Add(CreateRoute(CornerC, CornerD));
        if (Radius != 0) tokens.Add(new Fillet() { Radius = Radius });
        tokens.Add(CreateRoute(CornerD, CornerA));
        if (Radius != 0) tokens.Add(new Fillet() { Radius = Radius });
        tokens.Add(CreateRoute(CornerA, start));

        return [.. tokens];

    }

    TokenRecord IToken.ToTokenRecord() {

        return new() {
            Name = "Rectangle",
            StartX = CornerA.X.ToString(),
            StartY = CornerA.Y.ToString(),
            StartZ = StartDepth.ToString(),
            EndX = CornerC.X.ToString(),
            EndY = CornerC.Y.ToString(),
            EndZ = EndDepth.ToString(),
            CenterX = CornerB.X.ToString(),
            CenterY = CornerB.Y.ToString(),
            PocketX = CornerD.X.ToString(),
            PocketY = CornerD.Y.ToString(),
            OffsetSide = Offset.ToCSVCode(),
            ToolName = ToolName,
            SequenceNum = SequenceNumber == 0 ? "" : SequenceNumber.ToString(),
            NumberOfPasses = NumberOfPasses == 0 ? "" : NumberOfPasses.ToString(),
            FeedSpeed = FeedSpeed == 0 ? "" : FeedSpeed.ToString(),
            SpindleSpeed = SpindleSpeed == 0 ? "" : SpindleSpeed.ToString(),
            Radius = Radius == 0 ? "" : Radius.ToString()
        };

    }

    internal static Rectangle FromTokenRecord(TokenRecord tokenRecord) {

        if (!tokenRecord.Name.Split('*',2).First().Equals("rectangle", StringComparison.InvariantCultureIgnoreCase)) {
            throw new InvalidOperationException($"Can not map token '{tokenRecord.Name}' to Rectangle.");
        }

        if (!double.TryParse(tokenRecord.StartX, out double startX)) {
            throw new InvalidOperationException("Start X value not specified or invalid for Rectangle operation");
        }

        if (!double.TryParse(tokenRecord.StartY, out double startY)) {
            throw new InvalidOperationException("Start Y value not specified or invalid for Rectangle operation");
        }

        if (!double.TryParse(tokenRecord.EndX, out double endX)) {
            throw new InvalidOperationException("End X value not specified or invalid for Rectangle operation");
        }

        if (!double.TryParse(tokenRecord.EndY, out double endY)) {
            throw new InvalidOperationException("End Y value not specified or invalid for Rectangle operation");
        }

        if (!double.TryParse(tokenRecord.PocketX, out double pocketX)) {
            throw new InvalidOperationException("Pocket X value not specified or invalid for Rectangle operation");
        }

        if (!double.TryParse(tokenRecord.PocketY, out double pocketY)) {
            throw new InvalidOperationException("Pocket Y value not specified or invalid for Rectangle operation");
        }

        if (!double.TryParse(tokenRecord.CenterX, out double centerX)) {
            throw new InvalidOperationException("Center X value not specified or invalid for Rectangle operation");
        }

        if (!double.TryParse(tokenRecord.CenterY, out double centerY)) {
            throw new InvalidOperationException("Center Y value not specified or invalid for Rectangle operation");
        }

        if (!double.TryParse(tokenRecord.StartZ, out double startDepth)) {
            throw new InvalidOperationException("Start Z value not specified or invalid for Rectangle operation");
        }

        if (!double.TryParse(tokenRecord.EndZ, out double endDepth)) {
            throw new InvalidOperationException("End Z value not specified or invalid for Rectangle operation");
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

        if (!double.TryParse(tokenRecord.Radius, out double radius)) {
            radius = 0;
        }

        Offset offset = OffsetExtension.FromCSVCode(tokenRecord.OffsetSide);

        return new() {
            ToolName = tokenRecord.ToolName,
            CornerA = new(startX, startY),
            CornerB = new(centerX, centerY),
            CornerC = new(endX, endY),
            CornerD = new(pocketX, pocketY),
            StartDepth = startDepth,
            EndDepth = endDepth,
            Offset = offset,
            SequenceNumber = sequenceNum,
            NumberOfPasses = numberOfPasses,
            FeedSpeed = feedSpeed,
            SpindleSpeed = spindleSpeed,
            Radius = radius
        };

    }

}
