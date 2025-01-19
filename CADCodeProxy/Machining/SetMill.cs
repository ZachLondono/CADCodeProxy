using CADCode;
using CADCodeProxy.CADCodeProxy;
using CADCodeProxy.Enums;

namespace CADCodeProxy.Machining;

public class SetMill : IMachiningOperation {

    public required string ToolName { get; set; }
    public required Point Start { get; set; }
    public required double StartDepth { get; set; }
    public required Offset Offset { get; set; } = Offset.Center;
    public required int SequenceNumber { get; set; } = 0;
    public required int NumberOfPasses { get; set; } = 0;
    public required double FeedSpeed { get; set; } = 0;
    public required double SpindleSpeed { get; set; } = 0;

    void IMachiningOperation.AddToCode(CADCodeCodeClass code, double xOffset, double yOffset) {

        code.RouteSetmill(
                        (float)Start.X + (float)xOffset,
                        (float)Start.Y + (float)yOffset,
                        (float)StartDepth,
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

}
