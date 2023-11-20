namespace CADCodeProxy.Machining;

public class PartFace {

    public required string ProgramName { get; set; }
    public bool IsRotated { get; set; } = false;
    public bool IsMirrored { get; set; } = false;
    public required IToken[] Tokens { get; set; }

    // TODO: Fix tokens that use fillets
    public IMachiningOperation[] GetMachiningOperations() => Tokens.OfType<IMachiningOperation>().ToArray();

}
