using CADCode;
using CADCodeProxy.CSV;

namespace CADCodeProxy.Machining;

public record Part {

    public required string Name { get; init; }
    public required int Qty { get; init; }
    public required double Width { get; init; }
    public required double Length { get; init; }
    public required double Thickness { get; init; }
    public required string Material { get; init; }
    public required bool IsGrained { get; init; }
    public InfoFields InfoFields { get; init; } = new();
    public required PartFace PrimaryFace { get; init; }
    public PartFace? SecondaryFace { get; init; } = null;
    public EdgeBanding Width1Banding { get; init; } = new("", "");
    public EdgeBanding Width2Banding { get; init; } = new("", "");
    public EdgeBanding Length1Banding { get; init; } = new("", "");
    public EdgeBanding Length2Banding { get; init; } = new("", "");

    internal void AddNestPartToCode(CADCodeCodeClass code) {

        float width = (float)Width;
        float length = (float)Length;

        code.NestedPart(width, length, OriginType.CC_LL, PrimaryFace.ProgramName, AxisTypes.CC_AUTO_AXIS, 0);
        foreach (var token in PrimaryFace.Tokens) {
            token.AddToCode(code);
        }

        if (SecondaryFace is not null) {
            code.NestedPart(width, length, OriginType.CC_LL, SecondaryFace.ProgramName, AxisTypes.CC_AUTO_AXIS, 0);
            foreach (var token in SecondaryFace.Tokens) {
                token.AddToCode(code);
            }
        }

    }

    internal void AddPrimaryFaceSinglePartToCode(CADCodeCodeClass code, UnitTypes units) {

        float width = (float)Width;
        float length = (float)Length;
        float thickness = (float)Thickness;

        code.Border(width, length, thickness, units, OriginType.CC_LL, PrimaryFace.ProgramName, AxisTypes.CC_X_AXIS);

        foreach (var token in PrimaryFace.Tokens) {
            token.AddToCode(code);
        }

        code.EndPanel();

    }

    internal void AddToLabels(InfoFields batchInfo, CADCodeLabelClass labels) {
        labels.NewLabel();
        foreach (var (field, value) in InfoFields) {
            labels.AddField(field, value);
        }
        foreach (var (field, value) in batchInfo) {
            labels.AddField(field, value);
        }
        AddEdgeBandingLabelFields(labels, "Width", 1, Width1Banding);
        AddEdgeBandingLabelFields(labels, "Width", 2, Width2Banding);
        AddEdgeBandingLabelFields(labels, "Length", 1, Length1Banding);
        AddEdgeBandingLabelFields(labels, "Length", 2, Length2Banding);
        labels.EndLabel();
    }

    private static void AddEdgeBandingLabelFields(CADCodeLabelClass labels, string edgeName, int edgeNum, EdgeBanding banding) {
        labels.AddField($"{edgeName} Color {edgeNum}", banding.Color);
        labels.AddField($"{edgeName} Material {edgeNum}", banding.Material);
    } 

    internal CADCode.Part[] ToCADCodePart(UnitTypes units) {

        var parts = new List<CADCode.Part>();

        string width = Width.ToString();
        string length = Length.ToString();

        for (int i = 0; i < Qty; i++) {

            parts.Add(new() {
                QuantityOrdered = 1,
                Face5Filename = PrimaryFace.ProgramName,
                Width = width,
                Length = length,
                Thickness = (float) Thickness,
                Material = Material,
                Units = units,
                RotationAllowed = IsGrained ? 0 : 1, // This does not seem to have any affect
                Graining = IsGrained ? "Y" : "N",    // This is the important field required to make sure that the parts are oriented correctly on grained material. The Graining flag on the 'CutListInventory' class seems to have no affect.
                Rotated = PrimaryFace.IsRotated,
                Face5Runfield = PrimaryFace.IsMirrored ? "Mirror On" : "",
                WidthColor1 = Width1Banding.Color,
                WidthMaterial1 = Width1Banding.Material,
                WidthColor2 = Width2Banding.Color,
                WidthMaterial2 = Width2Banding.Material,
                LengthColor1 = Length1Banding.Color,
                LengthMaterial1 = Length1Banding.Material,
                LengthColor2 = Length2Banding.Color,
                LengthMaterial2 = Length2Banding.Material,
                //DoLabel = true
            });

            if (SecondaryFace is not null) {
                parts.Add(new() {
                    QuantityOrdered = 1,
                    Face5Filename = SecondaryFace.ProgramName,
                    Width = width,
                    Length = length,
                    Thickness = (float) Thickness,
                    Material = Material,
                    Units = units,
                    RotationAllowed = IsGrained ? 0 : 1,
                    Graining = IsGrained ? "Y" : "N",
                    Rotated = SecondaryFace.IsRotated,
                    Face5Runfield = SecondaryFace.IsMirrored ? "Mirror On" : "",
                    //DoLabel = true
                });
            }

        }

        return parts.ToArray();

    }

    internal PartRecord ToPartRecord(string jobName) {
        return new() {
            CabNumber = "",
            PartID = "",
            PartName = Name,
            JobName = jobName,
            Qty = Qty.ToString(),
            Width = Width.ToString(),
            Length = Length.ToString(),
            Thickness = Thickness.ToString(),
            Material = Material,
            Graining = IsGrained ? "Y" : "N",
            FileName = PrimaryFace.ProgramName,
            Mirror = PrimaryFace.IsMirrored ? "Mirror On" : "",
            Rotation = PrimaryFace.IsRotated ? "90" : "",
            WidthColor1 = Width1Banding.Color,
            WidthMaterial1 = Width1Banding.Material,
            WidthColor2 = Width2Banding.Color,
            WidthMaterial2 = Width2Banding.Material,
            LengthColor1 = Length1Banding.Color,
            LengthMaterial1 = Length1Banding.Material,
            LengthColor2 = Length2Banding.Color,
            LengthMaterial2 = Length2Banding.Material,
        };
    }

}
