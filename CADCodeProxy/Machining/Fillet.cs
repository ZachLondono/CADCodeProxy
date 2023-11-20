using CADCode;
using CADCodeProxy.CSV;

namespace CADCodeProxy.Machining;

public class Fillet : IToken {

    // TODO: get rid of ToolName
    public string ToolName { get; set; } = "";
    public required double Radius { get; set; } = 0;

    void IToken.AddToCode(CADCodeCodeClass code) {
        // TODO: Fillet should not be a normal token since it does not map to a CADCode operation. It is more of a "MetaToken" that provides information to other adjacent tokens.
        throw new NotImplementedException();
    }

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
