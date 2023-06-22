using CsvHelper.Configuration;

namespace CADCodeProxy.CSV;

public class PartRecordMap : ClassMap<PartRecord> {

    public PartRecordMap() {

        Map(p => p.JobName).Index(0).Name("JobName");
        Map(p => p.CabNumber).Index(1).Name("CabNumber");
        Map(p => p.PartID).Index(2).Name("PartID");
        Map(p => p.PartName).Index(3).Name("PartName");
        Map(p => p.Qty).Index(4).Name("Qty");
        Map(p => p.Border).Index(5).Name("Border");
        Map().Index(6).Constant("");
        Map(p => p.Width).Index(7).Name("Width");
        Map(p => p.Length).Index(8).Name("Length");
        Map(p => p.Thickness).Index(9).Name("Thickness");
        Map().Index(10).Constant("");
        Map().Index(11).Constant("");
        Map().Index(12).Constant("");
        Map().Index(13).Constant("");
        Map().Index(14).Constant("");
        Map().Index(15).Constant("");
        Map().Index(16).Constant("");
        Map().Index(17).Constant("");
        Map().Index(18).Constant("");
        Map().Index(19).Constant("");
        Map().Index(20).Constant("");
        Map().Index(21).Constant("");
        Map().Index(22).Constant("");
        Map().Index(23).Constant("");
        Map().Index(24).Constant("");
        Map().Index(25).Constant("");
        Map().Index(26).Constant("");
        Map(p => p.FileName).Index(27);
        Map().Index(28).Constant("");
        Map().Index(29).Constant("");
        Map().Index(30).Constant("");
        Map(p => p.Mirror).Index(31);
        Map(p => p.Material).Index(32);
        Map(p => p.Graining).Index(33);
        Map().Index(34).Constant("");
        Map(p => p.Rotation).Index(35);
        Map().Index(35).Constant(""); // Machine as Island Part
        Map().Index(36).Constant(""); // Machine as Small Part
        Map().Index(37).Constant(""); // ProductId 
        Map().Index(38).Constant(""); // ProductClass
        Map().Index(38).Constant("");
        Map(p => p.WidthColor1).Index(39);
        Map(p => p.WidthColor2).Index(40);
        Map(p => p.WidthMaterial1).Index(39);
        Map(p => p.WidthMaterial2).Index(40);
        Map(p => p.LengthColor1).Index(41);
        Map(p => p.LengthColor2).Index(42);
        Map(p => p.LengthMaterial1).Index(43);
        Map(p => p.LengthMaterial2).Index(44);

    }

}
