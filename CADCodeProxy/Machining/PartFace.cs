namespace CADCodeProxy.Machining;

public class PartFace {

    public required string ProgramName { get; set; }
    public double Rotation { get; set; }
    public bool IsMirrored { get; set; } = false;
    public required IToken[] Tokens { get; set; }

    public IMachiningOperation[] GetMachiningOperations() {

        var accumulator = new TokenAccumulator();

        foreach (var token in Tokens) {
            accumulator.AddToken(token);
        }

        return accumulator.GetMachiningOperations();

    }

}
