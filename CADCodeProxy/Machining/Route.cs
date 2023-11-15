using CADCode;
using CADCodeProxy.CADCodeProxy;
using CADCodeProxy.CSV;
using CADCodeProxy.Enums;

namespace CADCodeProxy.Machining;

public class Route : IToken {

    public required string ToolName { get; set; }
    public required Point Start { get; set; }
    public required Point End { get; set; }
    public required double StartDepth { get; set; }
    public required double EndDepth { get; set; }
    public Offset Offset { get; set; } = Offset.Center;
    public int SequenceNumber { get; set; } = 0;
    public int NumberOfPasses { get; set; } = 0;

    void IToken.AddToCode(CADCodeCodeClass code) {

        code.RouteLine((float) Start.X,
                       (float) Start.Y,
                       (float) StartDepth,
                       (float) End.X,
                       (float) End.Y,
                       (float) EndDepth,
                       ToolName,
                       0f,
                       Offset.AsCCOffset(),
                       0f,
                       RotationTypes.CC_ROTATION_AUTO,
                       FaceTypes.CC_UPPER_FACE,
                       0f,
                       0f,
                       0f,
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
            NumberOfPasses = NumberOfPasses == 0 ? "" : NumberOfPasses.ToString()
        };

    }

    internal static Route FromTokenRecord(TokenRecord tokenRecord) {

        if (!double.TryParse(tokenRecord.StartX, out double startX)) {
            throw new InvalidOperationException("Start X value not specified or invalid for Bore operation");
        }

        if (!double.TryParse(tokenRecord.EndX, out double startY)) {
            throw new InvalidOperationException("Start Y value not specified or invalid for Bore operation");
        }

        if (!double.TryParse(tokenRecord.EndX, out double endX)) {
            throw new InvalidOperationException("End X value not specified or invalid for Bore operation");
        }

        if (!double.TryParse(tokenRecord.EndY, out double endY)) {
            throw new InvalidOperationException("End Y value not specified or invalid for Bore operation");
        }

        if (!double.TryParse(tokenRecord.StartZ, out double startDepth)) {
            throw new InvalidOperationException("Start Z value not specified or invalid for Bore operation");
        }

        if (!double.TryParse(tokenRecord.EndZ, out double endDepth)) {
            throw new InvalidOperationException("End Z value not specified or invalid for Bore operation");
        }

        if (!int.TryParse(tokenRecord.SequenceNum, out int sequenceNum)) {
            sequenceNum = 0;
        }

        if (!int.TryParse(tokenRecord.NumberOfPasses, out int numberOfPasses)) {
            numberOfPasses = 0;
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
            NumberOfPasses = numberOfPasses
        };

    }

}
