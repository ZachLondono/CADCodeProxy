using CADCode;
using CADCodeProxy.CSV;

namespace CADCodeProxy.Machining;

public class Pocket : IToken, IMachiningOperation {

    public required string ToolName { get; set; }
    public required Point CornerA { get; set; }
    public required Point CornerB { get; set; }
    public required Point CornerC { get; set; }
    public required Point CornerD { get; set; }
    public required double StartDepth { get; set; }
    public required double EndDepth { get; set; }
    public int SequenceNumber { get; set; } = 0;
    public int NumberOfPasses { get; set; } = 0;
    public double FeedSpeed { get; set; } = 0;
    public double SpindleSpeed { get; set; } = 0;

    void IMachiningOperation.AddToCode(CADCodeCodeClass code) {

        code.Pocket((float)CornerA.X,
                    (float)CornerA.Y,
                    (float)CornerB.X,
                    (float)CornerB.Y,
                    (float)CornerC.X,
                    (float)CornerC.Y,
                    (float)CornerD.X,
                    (float)CornerD.Y,
                    (float)StartDepth,
                    (float)EndDepth,
                    ToolName,
                    FaceTypes.CC_UPPER_FACE,
                    (float)FeedSpeed,
                    0f,
                    (float)SpindleSpeed,
                    0f,
                    "",
                    SequenceNumber,
                    0,
                    NumberOfPasses: NumberOfPasses
            );

    }

    TokenRecord IToken.ToTokenRecord() {

        return new() {
            Name = "Pocket",
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
            ToolName = ToolName,
            SequenceNum = SequenceNumber == 0 ? "" : SequenceNumber.ToString(),
            NumberOfPasses = NumberOfPasses == 0 ? "" : NumberOfPasses.ToString(),
            FeedSpeed = FeedSpeed.ToString(),
            SpindleSpeed = SpindleSpeed.ToString()
        };

    }

    internal static Pocket FromTokenRecord(TokenRecord tokenRecord) {

        if (!tokenRecord.Name.Split('*',2).First().Equals("pocket", StringComparison.InvariantCultureIgnoreCase)) {
            throw new InvalidOperationException($"Can not map token '{tokenRecord.Name}' to pocket.");
        }

        if (!double.TryParse(tokenRecord.StartX, out double startX)) {
            throw new InvalidOperationException("Start X value not specified or invalid for Pocket operation");
        }

        if (!double.TryParse(tokenRecord.StartY, out double startY)) {
            throw new InvalidOperationException("Start Y value not specified or invalid for Pocket operation");
        }

        if (!double.TryParse(tokenRecord.EndX, out double endX)) {
            throw new InvalidOperationException("End X value not specified or invalid for Pocket operation");
        }

        if (!double.TryParse(tokenRecord.EndY, out double endY)) {
            throw new InvalidOperationException("End Y value not specified or invalid for Pocket operation");
        }

        if (!double.TryParse(tokenRecord.PocketX, out double pocketX)) {
            throw new InvalidOperationException("Pocket X value not specified or invalid for Pocket operation");
        }

        if (!double.TryParse(tokenRecord.PocketY, out double pocketY)) {
            throw new InvalidOperationException("Pocket Y value not specified or invalid for Pocket operation");
        }

        if (!double.TryParse(tokenRecord.CenterX, out double centerX)) {
            throw new InvalidOperationException("Center X value not specified or invalid for Pocket operation");
        }

        if (!double.TryParse(tokenRecord.CenterY, out double centerY)) {
            throw new InvalidOperationException("Center Y value not specified or invalid for Pocket operation");
        }

        if (!double.TryParse(tokenRecord.StartZ, out double startDepth)) {
            throw new InvalidOperationException("Start Z value not specified or invalid for Pocket operation");
        }

        if (!double.TryParse(tokenRecord.EndZ, out double endDepth)) {
            throw new InvalidOperationException("End Z value not specified or invalid for Pocket operation");
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
            CornerA = new(startX, startY),
            CornerB = new(centerX, centerY),
            CornerC = new(endX, endY),
            CornerD = new(pocketX, pocketY),
            StartDepth = startDepth,
            EndDepth = endDepth,
            SequenceNumber = sequenceNum,
            NumberOfPasses = numberOfPasses,
            FeedSpeed = feedSpeed,
            SpindleSpeed = spindleSpeed
        };

    }

}
