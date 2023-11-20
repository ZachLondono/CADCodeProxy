using CADCodeProxy.CSV;

namespace CADCodeProxy.Machining;

public class Fillet : IToken {

    // TODO: get rid of ToolName
    public string ToolName { get; set; } = "";
    public required double Radius { get; set; } = 0;

    TokenRecord IToken.ToTokenRecord() {
        
        return new() {
            Name = "Fillet",
            Radius = Radius.ToString(),
        };

    }

    internal static Fillet FromTokenRecord(TokenRecord tokenRecord) {

        if (!tokenRecord.Name.Equals("fillet", StringComparison.InvariantCultureIgnoreCase)) {
            throw new InvalidOperationException($"Can not map token '{tokenRecord.Name}' to fillet.");
        }

        if (!double.TryParse(tokenRecord.Radius, out double radius)) {
            throw new InvalidOperationException("Radius value not specified or invalid for fillet operation");
        }

        return new() {
            Radius = radius,
        };

    }

}
