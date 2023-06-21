using CADCode;
using CADCodeProxy.CSV;

namespace CADCodeProxy.Machining;

public record Pocket : IToken {

    public required string ToolName { get; set; }
    public required Point CornerA { get; set; }
    public required Point CornerB { get; set; }
    public required Point CornerC { get; set; }
    public required Point CornerD { get; set; }
    public required double StartDepth { get; set; }
    public required double EndDepth { get; set; }

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
                    0,
                    0
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
        };

    }

}
