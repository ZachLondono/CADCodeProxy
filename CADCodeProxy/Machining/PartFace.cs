namespace CADCodeProxy.Machining;

public record PartFace {

    public required string ProgramName { get; init; }
    public bool IsRotated { get; init; } = false;
    public bool IsMirrored { get; init; } = false;
    public required IToken[] Tokens { get; init; }

}
