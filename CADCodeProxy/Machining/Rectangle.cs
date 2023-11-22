using CADCodeProxy.CSV;
using CADCodeProxy.Enums;

namespace CADCodeProxy.Machining;

public class Rectangle : IToken {

    public required string ToolName { get; set; }
    public required Point CornerA { get; set; }
    public required Point CornerB { get; set; }
    public required Point CornerC { get; set; }
    public required Point CornerD { get; set; }
    public required double StartDepth { get; set; }
    public required double EndDepth { get; set; }
    public Offset Offset { get; set; } = Offset.Center; // TODO: figure out how to handle inside and outside, I think it will just work if passed to CADCode
    public int SequenceNumber { get; set; } = 0;
    public int NumberOfPasses { get; set; } = 0;
    public double FeedSpeed { get; set; } = 0;
    public double SpindleSpeed { get; set; } = 0;

    public double Radius { get; set; } = 0;

    internal IToken[] GetComponentTokens() {

        Route CreateRoute(Point start, Point end) => new Route() {
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

		List<IToken> tokens = new();

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

        return tokens.ToArray();

	}

    TokenRecord IToken.ToTokenRecord() {

        return new() {
            Name = "Rectangle",
            StartX = CornerA.X.ToString(),
            StartY = CornerA.Y.ToString(),
            StartZ = StartDepth.ToString(),
            EndX = CornerB.X.ToString(),
            EndY = CornerB.Y.ToString(),
            EndZ = EndDepth.ToString(),
            CenterX = CornerC.X.ToString(),
            CenterY = CornerC.Y.ToString(),
            PocketX = CornerD.X.ToString(),
            PocketY = CornerD.Y.ToString(),
            OffsetSide = Offset.ToCSVCode(),
            ToolName = ToolName,
            SequenceNum = SequenceNumber == 0 ? "" : SequenceNumber.ToString(),
            NumberOfPasses = NumberOfPasses == 0 ? "" : NumberOfPasses.ToString(),
            FeedSpeed = FeedSpeed.ToString(),
            SpindleSpeed = SpindleSpeed.ToString(),
            Radius = Radius.ToString()
        };

    }

    internal static Rectangle FromTokenRecord(TokenRecord tokenRecord) {

        if (!tokenRecord.Name.Equals("rectangle", StringComparison.InvariantCultureIgnoreCase)) {
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
