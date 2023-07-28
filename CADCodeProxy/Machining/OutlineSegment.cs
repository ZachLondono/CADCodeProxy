using CADCode;
using CADCodeProxy.CSV;

namespace CADCodeProxy.Machining;

public record OutlineSegment : IToken {

    public required string ToolName { get; init; }
    public required Point Start { get; init; }
    public required Point End { get; init; }
    public required double StartDepth { get; init; }
    public required double EndDepth { get; init; }
    public int SequenceNumber { get; init; } = 0;
    public int NumberOfPasses { get; init; } = 0;

    void IToken.AddToCode(CADCodeCodeClass code) {

        code.DefineOutLine(
                            StartX: (float) Start.X,
                            StartY: (float) Start.Y,
                            EndX: (float) End.X,
                            EndY: (float) End.Y,
                            CenterX: 0,
                            CenterY: 0,
                            Radius: 0,
                            ArcDirection: ArcTypes.CC_CLOCKWISE_ARC,
                            Offset: OffsetTypes.CC_OFFSET_OUTSIDE,
                            ToolName: ToolName,
                            FeedSpeed: 0,
                            SpindleSpeed: 0,
                            NestedRouteSequence: SequenceNumber,
                            NumberOfPasses: NumberOfPasses,
                            KerfClearance: 0);

    }

    TokenRecord IToken.ToTokenRecord() {
        
        return new() {
            Name = "OUTLINE",
            StartX = Start.X.ToString(),
            StartY = Start.Y.ToString(),
            StartZ = StartDepth.ToString(),
            EndX = End.X.ToString(),
            EndY = End.Y.ToString(),
            EndZ = EndDepth.ToString(),
            ToolName = ToolName,
            SequenceNum = SequenceNumber == 0 ? "" : SequenceNumber.ToString(),
            NumberOfPasses = NumberOfPasses == 0 ? "" : NumberOfPasses.ToString()
        };

    }
}
