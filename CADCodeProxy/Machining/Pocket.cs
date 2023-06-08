using CADCode;

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

}
