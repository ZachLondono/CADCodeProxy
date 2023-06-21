using CADCode;
using CADCodeProxy.CADCodeProxy;
using CADCodeProxy.CSV;
using CADCodeProxy.Enums;

namespace CADCodeProxy.Machining;

public record Route : IToken {

    public required string ToolName { get; init; }
    public required Point Start { get; init; }
    public required Point End { get; init; }
    public required double StartDepth { get; init; }
    public required double EndDepth { get; init; }
    public Offset Offset { get; init; } = Offset.Center;

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
                       0);

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
        };

    }

}
