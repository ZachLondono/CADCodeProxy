using CsvHelper.Configuration;

namespace CADCodeProxy.CSV;

public class TokenRecordReadMap : ClassMap<TokenRecord> {

    public TokenRecordReadMap() {

        Map(t => t.Name).Index(5);//.Name("Name");
        Map(t => t.StartX).Index(7);//.Name("StartX");
        Map(t => t.StartY).Index(8);//.Name("StartY");
        Map(t => t.StartZ).Index(9);//.Name("StartZ");
        Map(t => t.EndX).Index(10);//.Name("EndX");
        Map(t => t.EndY).Index(11);//.Name("EndY");
        Map(t => t.EndZ).Index(12);//.Name("EndZ");
        Map(t => t.CenterX).Index(13);//.Name("CenterX");
        Map(t => t.CenterY).Index(14);//.Name("CenterY");
        Map(t => t.PocketX).Index(15);//.Name("PocketX");
        Map(t => t.PocketY).Index(16);//.Name("PocketY");
        Map(t => t.Radius).Index(17);//.Name("Radius");
        Map(t => t.Pitch).Index(18);//.Name("Pitch");
        Map(t => t.NumberOfPasses).Index(19);//.Name("Number Of Passes");
        Map(t => t.OffsetSide).Index(20);//.Name("OffsetSide");
        Map(t => t.ToolName).Index(22);//.Name("ToolName");
        Map(t => t.ToolDiameter).Index(23);//.Name("ToolDiameter");
        Map(t => t.SequenceNum).Index(25);//.Name("SequenceNum");
        Map(t => t.ArcDirection).Index(37);//.Name("ArcDirection");
        Map(t => t.StartAngle).Index(38);//.Name("StartAngle");
        Map(t => t.EndAngle).Index(39);//.Name("EndAngle");
        Map(t => t.FeedSpeed).Index(41);//.Name("FeedSpeed");
        Map(t => t.SpindleSpeed).Index(42);//.Name("SpindleSpeed");

    }

}
