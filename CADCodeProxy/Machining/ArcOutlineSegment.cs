using CADCode;
using CADCodeProxy.CADCodeProxy;
using CADCodeProxy.Enums;

namespace CADCodeProxy.Machining;

public class ArcOutlineSegment : IMachiningOperation {

    public required string ToolName { get; set; }
    public required Point Start { get; set; }
    public required Point End { get; set; }
    public required double Radius { get; set; }
    public required ArcDirection Direction { get; set; }
    public required double StartDepth { get; set; }
    public required double EndDepth { get; set; }
    public int SequenceNumber { get; set; } = 0;
    public int NumberOfPasses { get; set; } = 0;
    public double FeedSpeed { get; set; } = 0;
    public double SpindleSpeed { get; set; } = 0;

    void IMachiningOperation.AddToCode(CADCodeCodeClass code) {

        code.DefineOutLine(
                            StartX: (float)Start.X,
                            StartY: (float)Start.Y,
                            EndX: (float)End.X,
                            EndY: (float)End.Y,
                            CenterX: 0,
                            CenterY: 0,
                            Radius: (float)Radius,
                            ArcDirection: Direction.AsCCArcType(),
                            Offset: OffsetTypes.CC_OFFSET_OUTSIDE,
                            ToolName: ToolName,
                            FeedSpeed: (float)FeedSpeed,
                            SpindleSpeed: (float)SpindleSpeed,
                            NestedRouteSequence: SequenceNumber,
                            NumberOfPasses: NumberOfPasses,
                            KerfClearance: 0);

    }

}
