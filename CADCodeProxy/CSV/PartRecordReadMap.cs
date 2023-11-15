using CsvHelper.Configuration;

namespace CADCodeProxy.CSV;

public class PartRecordReadMap : ClassMap<PartRecord> {

    public PartRecordReadMap() {

        Map(p => p.JobName).Index(0);//.Name("JobName");
        Map(p => p.CabinetNumber).Index(1);//.Name("Cabinet Number");
        Map(p => p.PartID).Index(2);//.Name("PartID");
        Map(p => p.ProductName).Index(3);//;
        Map(p => p.Qty).Index(4);//.Name("Qty");
        Map(p => p.Border).Index(5);//.Name("Border");
        Map(p => p.Length).Index(7);//.Name("Length / Start X");
        Map(p => p.Width).Index(8);//.Name("Width / Start Y");
        Map(p => p.Thickness).Index(9);//.Name("Thickness / Start Z");
        Map(p => p.FileName).Index(27);//.Name("File Name");
        Map(p => p.Face6FileName).Index(29);//.Name("Face 6 File Name");
        Map(p => p.Face6Flag).Index(30);//.Name("Face 6 Flag");
        Map(p => p.Mirror).Index(31);//.Name("Runfield");
        Map(p => p.Material).Index(32);//.Name("Material");
        Map(p => p.Graining).Index(33);//.Name("Graining");
        Map(p => p.Rotation).Index(35);//.Name("Rotation");
        Map(p => p.Description).Index(47);
        Map(p => p.CustomerInfo1).Index(48);
        Map(p => p.Level1).Index(49);
        Map(p => p.Comment1).Index(50);
        Map(p => p.Comment2).Index(51);
        Map(p => p.WidthInches).Index(53);
        Map(p => p.LengthInches).Index(54);
        Map(p => p.Side1Color).Index(56);
        Map(p => p.Side1Material).Index(57);
        Map(p => p.WidthColor1).Index(59);
        Map(p => p.WidthMaterial1).Index(60);
        Map(p => p.WidthColor2).Index(61);
        Map(p => p.WidthMaterial2).Index(62);
        Map(p => p.LengthColor1).Index(63);
        Map(p => p.LengthMaterial1).Index(64);
        Map(p => p.LengthColor2).Index(65);
        Map(p => p.LengthMaterial2).Index(66);

    }

}
