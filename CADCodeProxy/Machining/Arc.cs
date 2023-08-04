using CADCode;
using CADCodeProxy.CADCodeProxy;
using CADCodeProxy.CSV;
using CADCodeProxy.Enums;

namespace CADCodeProxy.Machining;

public class Arc : IToken {

    public required string ToolName { get; init; }
    public required Point Start { get; init; }
    public required Point End { get; init; }
    public required double Radius { get; init; }
    public required double StartDepth { get; init; }
    public required double EndDepth { get; init; }
    public required ArcDirection Direction { get; set; }
    public Offset Offset { get; init; } = Offset.Center;
    public int SequenceNumber { get; init; } = 0;
    public int NumberOfPasses { get; init; } = 0;

    void IToken.AddToCode(CADCodeCodeClass code) {

        code.RouteArc(StartX: (float) Start.X,
                        StartY: (float) Start.Y,
                        StartZ: (float) StartDepth,
                        EndX: (float) End.X,
                        EndY: (float) End.Y,
                        Endz: (float) EndDepth,
                        ToolName: ToolName,
                        ToolDiameter: 0f,
                        Offset: Offset.AsCCOffset(),
                        OffsetAmount: 0,
                        ToolRotation: RotationTypes.CC_ROTATION_AUTO,
                        Face: 0,
                        FeedSpeed: 0,
                        EntrySpeed: 0,
                        SpindleSpeed: 0,
                        CornerFeed: 0,
                        RType: "",
                        CenterX: 0,
                        CenterY: 0,
                        OffsetX: 0,
                        OffsetY: 0,
                        Radius: (float) Radius,
                        ArcDirection: Direction.AsCCArcType(),
                        Bulge: 0,
                        NestedRouteSequence: SequenceNumber,
                        NumberOfPasses: NumberOfPasses);

    }

    TokenRecord IToken.ToTokenRecord() {

        string name = Direction switch {
            ArcDirection.ClockWise => "CWArc",
            ArcDirection.CounterClockWise => "CCWArc",
            _ => throw new InvalidOperationException("Arc direction must be specified") 
        };

        return new() {
            Name = name,
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

}