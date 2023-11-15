using CADCode;
using CADCodeProxy.CSV;

namespace CADCodeProxy.Machining;

public class Fillet : IToken {

    /*
     *  Note: this may not be a valid token. May instead have to have some time of 'Outline Accumulator' which converts a list of Outline / fillet csv records into a list of outline tokens and vice versa
     */

    public required string ToolName { get; set; }
    public double Radius { get; set; } = 0;
    public int SequenceNumber { get; set; } = 0;
    public int NumberOfPasses { get; set; } = 0;

    void IToken.AddToCode(CADCodeCodeClass code) {

        code.DefineOutLine(
                            StartX: 0,
                            StartY: 0,
                            EndX: 0,
                            EndY: 0,
                            CenterX: 0,
                            CenterY: 0,
                            Radius: (float) Radius,
                            ArcDirection: ArcTypes.CC_CLOCKWISE_ARC,
                            Offset: OffsetTypes.CC_OFFSET_OUTSIDE,
                            ToolName: ToolName,
                            FeedSpeed: 0,
                            SpindleSpeed: 0,
                            NestedRouteSequence: SequenceNumber,
                            NumberOfPasses: NumberOfPasses,
                            KerfClearance: 0);

    }

    TokenRecord IToken.ToTokenRecord() {
        
        return new() {
            Name = "Fillet",
            Radius = Radius.ToString(),
            ToolName = ToolName,
            SequenceNum = SequenceNumber == 0 ? "" : SequenceNumber.ToString(),
            NumberOfPasses = NumberOfPasses == 0 ? "" : NumberOfPasses.ToString()
        };

    }

    internal static Fillet FromTokenRecord(TokenRecord tokenRecord) {
        throw new NotImplementedException();
    }

}
