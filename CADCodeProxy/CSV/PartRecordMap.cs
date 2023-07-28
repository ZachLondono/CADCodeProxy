using CsvHelper.Configuration;

namespace CADCodeProxy.CSV;

public class PartRecordMap : ClassMap<PartRecord> {

    public PartRecordMap() {

        Map(p => p.JobName).Index(0).Name("JobName");
        Map(p => p.CabinetNumber).Index(1).Name("Cabinet Number");
        Map(p => p.PartID).Index(2).Name("PartID");
        Map(p => p.ProductName).Index(3).Name("Product Name");
        Map(p => p.Qty).Index(4).Name("Qty");
        Map(p => p.Border).Index(5).Name("Border");
        Map().Index(6).Constant("Face");
        Map(p => p.Width).Index(7).Name("Width / Start X");
        Map(p => p.Length).Index(8).Name("Length / Start Y");
        Map(p => p.Thickness).Index(9).Name("Thickness / Start Z");
        Map().Index(10).Constant("").Name("End X");
        Map().Index(11).Constant("").Name("End Y");
        Map().Index(12).Constant("").Name("End Z");
        Map().Index(13).Constant("").Name("Center X");
        Map().Index(14).Constant("").Name("Center Y");
        Map().Index(15).Constant("").Name("Pocket X");
        Map().Index(16).Constant("").Name("Pocket Y");
        Map().Index(17).Constant("").Name("Radius");
        Map().Index(18).Constant("").Name("Pitch");
        Map().Index(19).Constant("").Name("Number of Passes");
        Map().Index(20).Constant("").Name("Offset");
        Map().Index(21).Constant("");
        Map().Index(22).Constant("").Name("Tool Number");
        Map().Index(23).Constant("").Name("Tool Diameter");
        Map().Index(24).Constant("");
        Map().Index(25).Constant("").Name("Sequence Number");
        Map().Index(26).Constant("");
        Map(p => p.FileName).Index(27).Name("File Name");
        Map().Index(28).Constant("");
        Map(p => p.Face6FileName).Index(29).Name("Face 6 File Name");
        Map(p => p.Face6Flag).Index(30).Name("Face 6 Flag");
        Map(p => p.Mirror).Index(31).Name("Runfield");
        Map(p => p.Material).Index(32).Name("Material");
        Map(p => p.Graining).Index(33).Name("Graining");
        Map().Index(34).Constant("");
        Map(p => p.Rotation).Index(35).Name("Rotation");
        Map().Index(36).Constant("");
        Map().Index(37).Constant("").Name("Arc Direction");
        Map().Index(38).Constant("").Name("Start Angle");
        Map().Index(39).Constant("").Name("End Angle");
        Map().Index(40).Constant("");
        Map().Index(41).Constant("").Name("Feed Speed");
        Map().Index(42).Constant("").Name("Rotation");
        Map().Index(43).Constant("").Name("Machine as Island Part");
        Map().Index(44).Constant("").Name("Machine as Small Part");
        Map().Index(45).Constant("").Name("ProductId");
        Map().Index(46).Constant("").Name("ProductClass");
        Map(p => p.Description).Index(47).Name("Description");
        Map(p => p.CustomerInfo1).Index(48).Name("CustomerInfo1");
        Map(p => p.Level1).Index(49).Name("Level1");
        Map(p => p.Comment1).Index(50).Name("Comment1");
        Map(p => p.Comment2).Index(51).Name("Comment2");
        Map().Index(52).Constant("");
        Map(p => p.WidthInches).Index(53).Name("Width Inches");
        Map(p => p.LengthInches).Index(54).Name("Length Inches");
        Map().Index(55).Constant("");
        Map(p => p.Side1Color).Index(56).Name("Side 1 Color");
        Map(p => p.Side1Material).Index(57).Name("Side 1 Material");
        Map().Index(58).Constant("");
        Map(p => p.WidthColor1).Index(59).Name("Width 1 Color");
        Map(p => p.WidthMaterial1).Index(60).Name("Width 1 Material");
        Map(p => p.WidthColor2).Index(61).Name("Width 2 Color");
        Map(p => p.WidthMaterial2).Index(62).Name("Width 2 Material");
        Map(p => p.LengthColor1).Index(63).Name("Length 1 Color");
        Map(p => p.LengthMaterial1).Index(64).Name("Length 1 Material");
        Map(p => p.LengthColor2).Index(65).Name("Length 2 Color");
        Map(p => p.LengthMaterial2).Index(66).Name("Length 2 Material");

    }

}
