using CADCode;

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

    internal void AddToCode(CADCodeCodeClass code) {

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

    internal void AddToLabels(InfoFields batchInfo, CADCodeLabelClass labels) {
        labels.NewLabel();
        foreach (var (field, value) in InfoFields) {
            labels.AddField(field, value);
        }
        foreach (var (field, value) in batchInfo) {
            labels.AddField(field, value);
        }
        labels.EndLabel();
    }

    internal CADCode.Part[] AsCADCodePart(UnitTypes units) {

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
                Graining = IsGrained ? "Y" : "N"    // This is the important field required to make sure that the parts are oriented correctly on grained material. The Graining flag on the 'CutListInventory' class seems to have no affect.
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
                    Graining = IsGrained ? "Y" : "N" 
                    //DoLabel = true
                });
            }

        }

        return parts.ToArray();

    }

}
