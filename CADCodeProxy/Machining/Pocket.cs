using CADCode;
using CADCodeProxy.CSV;

namespace CADCodeProxy.Machining;

public record Pocket : IToken {

    public required string ToolName { get; init; }
    public required Point CornerA { get; init; }
    public required Point CornerB { get; init; }
    public required Point CornerC { get; init; }
    public required Point CornerD { get; init; }
    public required double StartDepth { get; init; }
    public required double EndDepth { get; init; }
    public int SequenceNumber { get; init; } = 0;
    public int NumberOfPasses { get; init; } = 0;

    void IToken.AddToCode(CADCodeCodeClass code) {

        code.Pocket((float) CornerA.X,
                    (float) CornerA.Y,
                    (float) CornerB.X,
                    (float) CornerB.Y,
                    (float) CornerC.X,
                    (float) CornerC.Y,
                    (float) CornerD.X,
                    (float) CornerD.Y,
                    (float) StartDepth,
                    (float) EndDepth,
                    ToolName,
                    FaceTypes.CC_UPPER_FACE,
                    0f,
                    0f,
                    0f,
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
            NumberOfPasses = NumberOfPasses == 0 ? "" : NumberOfPasses.ToString()
        };

    }

}
