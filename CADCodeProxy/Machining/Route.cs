using CADCode;
using CADCodeProxy.CADCodeProxy;
using CADCodeProxy.CSV;
using CADCodeProxy.Enums;

namespace CADCodeProxy.Machining;

public class Route : IToken, IMachiningOperation {

    public required string ToolName { get; set; }
    public required Point Start { get; set; }
    public required Point End { get; set; }
    public required double StartDepth { get; set; }
    public required double EndDepth { get; set; }
    public Offset Offset { get; set; } = Offset.Center;
    public int SequenceNumber { get; set; } = 0;
    public int NumberOfPasses { get; set; } = 0;
    public double FeedSpeed { get; set; } = 0;
    public double SpindleSpeed { get; set; } = 0;

    void IMachiningOperation.AddToCode(CADCodeCodeClass code) {

        code.RouteLine((float)Start.X,
                       (float)Start.Y,
                       (float)StartDepth,
                       (float)End.X,
                       (float)End.Y,
                       (float)EndDepth,
                       ToolName,
                       0f,
                       Offset.AsCCOffset(),
                       0f,
                       RotationTypes.CC_ROTATION_AUTO,
                       FaceTypes.CC_UPPER_FACE,
                       (float)FeedSpeed,
                       0f,
                       (float)SpindleSpeed,
                       0f,
                       "",
                       SequenceNumber,
                       NumberOfPasses: NumberOfPasses);

    }

    TokenRecord IToken.ToTokenRecord() {

        return new() {
            Name = "Route",
            StartX = Start.X.ToString(),
            StartY = Start.Y.ToString(),
            StartZ = StartDepth.ToString(),
            EndX = End.X.ToString(),
            EndY = End.Y.ToString(),
            EndZ = EndDepth.ToString(),
            OffsetSide = Offset.ToCSVCode(),
            ToolName = ToolName,
            SequenceNum = SequenceNumber == 0 ? "" : SequenceNumber.ToString(),
            NumberOfPasses = NumberOfPasses == 0 ? "" : NumberOfPasses.ToString(),
            FeedSpeed = FeedSpeed.ToString(),
            SpindleSpeed = SpindleSpeed.ToString()
        };

    }

    internal static Route FromTokenRecord(TokenRecord tokenRecord) {

        if (!tokenRecord.Name.Equals("route", StringComparison.InvariantCultureIgnoreCase)) {
            throw new InvalidOperationException($"Can not map token '{tokenRecord.Name}' to route.");
        }

        if (!double.TryParse(tokenRecord.StartX, out double startX)) {
            throw new InvalidOperationException("Start X value not specified or invalid for Route operation");
        }

        if (!double.TryParse(tokenRecord.StartY, out double startY)) {
            throw new InvalidOperationException("Start Y value not specified or invalid for Route operation");
        }

        if (!double.TryParse(tokenRecord.EndX, out double endX)) {
            throw new InvalidOperationException("End X value not specified or invalid for Route operation");
        }

        if (!double.TryParse(tokenRecord.EndY, out double endY)) {
            throw new InvalidOperationException("End Y value not specified or invalid for Route operation");
        }

        if (!double.TryParse(tokenRecord.StartZ, out double startDepth)) {
            throw new InvalidOperationException("Start Z value not specified or invalid for Route operation");
        }

        if (!double.TryParse(tokenRecord.EndZ, out double endDepth)) {
            throw new InvalidOperationException("End Z value not specified or invalid for Route operation");
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

        return new() {
            ToolName = tokenRecord.ToolName,
            Start = new(startX, startY),
            End = new(endX, endY),
            StartDepth = startDepth,
            EndDepth = endDepth,
            Offset = offset,
            SequenceNumber = sequenceNum,
            NumberOfPasses = numberOfPasses,
            FeedSpeed = feedSpeed,
            SpindleSpeed = spindleSpeed
        };

    }

}
