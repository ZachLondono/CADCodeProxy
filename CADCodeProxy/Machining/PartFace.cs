namespace CADCodeProxy.Machining;

public record PartFace {

    public required string ProgramName { get; init; }
    public required IToken[] Tokens { get; init; }

}
