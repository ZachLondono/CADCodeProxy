using CsvHelper.Configuration;

namespace CADCodeProxy.CSV;

public class TokenRecordMap : ClassMap<TokenRecord> {

    public TokenRecordMap() {

        Map().Index(0).Constant("");
        Map().Index(1).Constant("");
        Map().Index(2).Constant("");
        Map().Index(3).Constant("");
        Map().Index(4).Constant("");
        Map(t => t.Name).Index(5).Name("Name");
        Map().Index(6).Constant("");
        Map(t => t.StartX).Index(7).Name("StartX");
        Map(t => t.StartY).Index(8).Name("StartY");
        Map(t => t.StartZ).Index(9).Name("StartZ");
        Map(t => t.EndX).Index(10).Name("EndX");
        Map(t => t.EndY).Index(11).Name("EndY");
        Map(t => t.EndZ).Index(12).Name("EndZ");
        Map(t => t.CenterX).Index(13).Name("CenterX");
        Map(t => t.CenterY).Index(14).Name("CenterY");
        Map(t => t.PocketX).Index(15).Name("PocketX");
        Map(t => t.PocketY).Index(16).Name("PocketY");
        Map(t => t.Radius).Index(17).Name("Radius");
        Map(t => t.Pitch).Index(18).Name("Pitch");
        Map(t => t.NumberOfPasses).Index(19).Name("Number Of Passes");
        Map(t => t.OffsetSide).Index(20).Name("OffsetSide");
        Map().Index(21).Constant("");
        Map(t => t.ToolName).Index(22).Name("ToolName");
        Map(t => t.ToolDiameter).Index(23).Name("ToolDiameter");
        Map().Index(24).Constant("");
        Map(t => t.SequenceNum).Index(25).Name("SequenceNum");
        Map().Index(26).Constant("");
        Map().Index(27).Constant("");
        Map().Index(28).Constant("");
        Map().Index(29).Constant("");
        Map().Index(30).Constant("");
        Map().Index(31).Constant("");
        Map().Index(32).Constant("");
        Map().Index(33).Constant("");
        Map().Index(34).Constant("");
        Map().Index(35).Constant("");
        Map().Index(36).Constant("");
        Map(t => t.ArcDirection).Index(37).Name("ArcDirection");
        Map(t => t.StartAngle).Index(38).Name("StartAngle");
        Map(t => t.EndAngle).Index(39).Name("EndAngle");
        Map().Index(40).Constant("");
        Map(t => t.FeedSpeed).Index(41).Name("FeedSpeed");
        Map(t => t.SpindleSpeed).Index(42).Name("SpindleSpeed");

    }

}
