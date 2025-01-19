using CADCode;
using CADCodeProxy.CSV;
using CADCodeProxy.Machining.Tokens;
using CADCodeProxy.Results;

namespace CADCodeProxy.Machining;

public class Part {

    public Guid Id { get; } = Guid.NewGuid();
    public required int Qty { get; set; }
    public required double Width { get; set; }
    public required double Length { get; set; }
    public required double Thickness { get; set; }
    public required string Material { get; set; }
    public required bool IsGrained { get; set; }
    public InfoFields InfoFields { get; set; } = [];
    public required PartFace PrimaryFace { get; set; }
    public PartFace? SecondaryFace { get; set; } = null;
    public EdgeBanding Width1Banding { get; set; } = new("", "");
    public EdgeBanding Width2Banding { get; set; } = new("", "");
    public EdgeBanding Length1Banding { get; set; } = new("", "");
    public EdgeBanding Length2Banding { get; set; } = new("", "");

    internal void AddNestPartToCode(CADCodeCodeClass code) {

        float panelX = (float)Length;
        float panelY = (float)Width;

        code.NestedPart(panelX, panelY, OriginType.CC_LL, PrimaryFace.ProgramName, AxisTypes.CC_AUTO_AXIS, (float)PrimaryFace.Rotation);

        // This only works when the Origin Type is set to 'CC_LL' and the rotation is set to 0, 90, 180 or 270 degrees.
        double xOffset = PrimaryFace.Rotation == 180 || PrimaryFace.Rotation == 270 ? -panelX : 0;
        double yOffset = PrimaryFace.Rotation == 90 || PrimaryFace.Rotation == 180 ? -panelY : 0;

        foreach (var operation in PrimaryFace.GetMachiningOperations()) {
            operation.AddToCode(code, xOffset, yOffset);
        }

        if (SecondaryFace is not null) {

            code.NestedPart(panelX, panelY, OriginType.CC_LL, SecondaryFace.ProgramName, AxisTypes.CC_AUTO_AXIS, (float)SecondaryFace.Rotation);

            xOffset = SecondaryFace.Rotation == 180 || SecondaryFace.Rotation == 270 ? -panelX : 0;
            yOffset = SecondaryFace.Rotation == 90 || SecondaryFace.Rotation == 180 ? -panelY : 0;

            foreach (var operation in SecondaryFace.GetMachiningOperations()) {
                operation.AddToCode(code, xOffset, yOffset);
            }

            throw new InvalidOperationException("Face 6 part not supported");

        }

    }

    internal void AddPrimaryFaceSinglePartToCode(CADCodeCodeClass code, UnitTypes units) {

        float panelX = (float)Length;
        float panelY = (float)Width;
        float thickness = (float)Thickness;

        code.Border(panelX, panelY, thickness, units, OriginType.CC_LL, PrimaryFace.ProgramName, AxisTypes.CC_X_AXIS);

        foreach (var operation in PrimaryFace.GetMachiningOperations()) {
            operation.AddToCode(code);
        }

        code.EndPanel();

    }

    internal PartLabel AddToLabels(InfoFields batchInfo, CADCodeLabelClass labels) {

        Dictionary<string, string> labelFields = [];

        labelFields["Face5Filename"] = PrimaryFace.ProgramName;
        if (SecondaryFace is not null) labelFields["Face5FileName"] = SecondaryFace.ProgramName;
        AddEdgeBandingLabelFields(labelFields, "Width", 2, Width2Banding);
        AddEdgeBandingLabelFields(labelFields, "Length", 1, Length1Banding);
        AddEdgeBandingLabelFields(labelFields, "Length", 2, Length2Banding);

        foreach (var (field, value) in InfoFields) {
            labelFields.Add(field, value);
        }
        foreach (var (field, value) in batchInfo) {
            labelFields.Add(field, value);
        }

        labels.NewLabel();
        foreach (var (field, value) in labelFields) {
            labels.AddField(field, value);
        }
        labels.EndLabel();

        return new PartLabel() {
            PartId = Id,
            Fields = labelFields
        };

    }

    private static void AddEdgeBandingLabelFields(Dictionary<string, string> labelFields, string edgeName, int edgeNum, EdgeBanding banding) {
        labelFields[$"{edgeName} Color {edgeNum}"] = banding.Color;
        labelFields[$"{edgeName} Material {edgeNum}"] = banding.Material;
    }

    internal CADCode.Part[] ToCADCodePart(UnitTypes units) {

        var parts = new List<CADCode.Part>();

        string width = Width.ToString();
        string length = Length.ToString();

        if (PrimaryFace.Rotation == 90 || PrimaryFace.Rotation == 270) {
            width = Length.ToString();
            length = Width.ToString();
        }

        for (int i = 0; i < Qty; i++) {

            bool containsShape = PrimaryFace.Tokens.Any(t => t is OutlineSegment);
            parts.Add(new() {
                QuantityOrdered = 1,
                Face5Filename = PrimaryFace.ProgramName,
                Width = width,
                Length = length,
                Thickness = (float)Thickness,
                Material = Material,
                Units = units,
                RotationAllowed = 90, // The increments which this part is allowed to be rotated by the panel optimizer
                Graining = IsGrained ? "Y" : "N",    // This is the important field required to make sure that the parts are oriented correctly on grained material. The Graining flag on the 'CutListInventory' class seems to have no affect.
                Face5Runfield = PrimaryFace.IsMirrored ? "Mirror On" : "Mirror Off",
                WidthColor1 = Width1Banding.Color,
                WidthMaterial1 = Width1Banding.Material,
                WidthColor2 = Width2Banding.Color,
                WidthMaterial2 = Width2Banding.Material,
                LengthColor1 = Length1Banding.Color,
                LengthMaterial1 = Length1Banding.Material,
                LengthColor2 = Length2Banding.Color,
                LengthMaterial2 = Length2Banding.Material,
                ContainsShape = containsShape,
                RouteShape = containsShape,
                PerimeterRoute = !containsShape, // If there is a face 6 part i think this should be set to false on one of the faces
                DoLabel = true
            });

            if (SecondaryFace is not null) {
                parts.Add(new() {
                    QuantityOrdered = 1,
                    Face5Filename = SecondaryFace.ProgramName,
                    Width = width,
                    Length = length,
                    Thickness = (float)Thickness,
                    Material = Material,
                    Units = units,
                    RotationAllowed = IsGrained ? 0 : 1,
                    Graining = IsGrained ? "Y" : "N",
                    Face5Runfield = SecondaryFace.IsMirrored ? "Mirror On" : "",
                    DoLabel = false
                });
            }

        }

        return [.. parts];

    }

    internal IEnumerable<CSVPart> ToCSVParts(string jobName) {

        yield return CreateCSVPart(jobName, PrimaryFace, false, SecondaryFace?.ProgramName ?? "");

        if (SecondaryFace is not null) {
            yield return CreateCSVPart(jobName, SecondaryFace, true, string.Empty);
        }

    }

    private CSVPart CreateCSVPart(string jobName, PartFace face, bool isFace6, string face6FileName) {

        string widthInches = AsInchFraction(Width).ToString();
        string lengthInches = AsInchFraction(Length).ToString();

        InfoFields.TryGetValue("CustomerInfo1", out string? customerInfo1);
        InfoFields.TryGetValue("Level1", out string? level1);
        InfoFields.TryGetValue("Comment1", out string? comment1);
        InfoFields.TryGetValue("Comment2", out string? comment2);
        InfoFields.TryGetValue("Side1Color", out string? side1Color);
        InfoFields.TryGetValue("Side1Material", out string? side1Material);
        InfoFields.TryGetValue("CabinetNumber", out string? cabNumber);
        InfoFields.TryGetValue("ProductName", out string? productName);
        InfoFields.TryGetValue("Description", out string? description);

        var partRecord = new PartRecord() {

            CabinetNumber = cabNumber ?? "",
            PartID = "",
            ProductName = productName ?? "",
            Description = description ?? "",
            JobName = jobName,
            Qty = Qty.ToString(),
            Width = Width.ToString(),
            Length = Length.ToString(),
            Thickness = Thickness.ToString(),
            Material = Material,
            Graining = IsGrained ? "Y" : "N",
            FileName = face.ProgramName,
            Face6FileName = face6FileName,
            Face6Flag = isFace6 ? "6" : "",
            Mirror = face.IsMirrored ? "Mirror On" : "",
            Rotation = face.Rotation == 0 ? "" : face.Rotation.ToString(),

            CustomerInfo1 = customerInfo1 ?? "",
            Level1 = level1 ?? "",
            Comment1 = comment1 ?? "",
            Comment2 = comment2 ?? "",

            WidthInches = widthInches,
            LengthInches = lengthInches,

            Side1Color = side1Color ?? "",
            Side1Material = side1Material ?? "",

            WidthColor1 = Width1Banding.Color,
            WidthMaterial1 = Width1Banding.Material,
            WidthColor2 = Width2Banding.Color,
            WidthMaterial2 = Width2Banding.Material,
            LengthColor1 = Length1Banding.Color,
            LengthMaterial1 = Length1Banding.Material,
            LengthColor2 = Length2Banding.Color,
            LengthMaterial2 = Length2Banding.Material,

        };

        var tokenRecords = face.Tokens.Select(t => t.ToTokenRecord()).ToList();

        return new() {
            PartRecord = partRecord,
            Tokens = tokenRecords
        };

    }

    private static Fraction AsInchFraction(double mm, double accuracy = 0.0001) {

        // https://stackoverflow.com/questions/5124743/algorithm-for-simplifying-decimal-to-fractions/42085412#42085412

        if (accuracy <= 0.0 || accuracy >= 1.0) {
            throw new ArgumentOutOfRangeException(nameof(accuracy), "Must be > 0 and < 1.");
        }

        double value = mm / 25.4;

        int sign = Math.Sign(value);

        if (sign == -1) {
            value = Math.Abs(value);
        }

        // Accuracy is the maximum relative error; convert to absolute maxError
        double maxError = sign == 0 ? accuracy : value * accuracy;

        int n = (int)Math.Floor(value);
        value -= n;

        if (value < maxError) {
            return new Fraction(sign * n, 1);
        }

        if (1 - maxError < value) {
            return new Fraction(sign * (n + 1), 1);
        }

        double z = value;
        int previousDenominator = 0;
        int denominator = 1;
        int numerator;

        do {
            z = 1.0 / (z - (int)z);
            int temp = denominator;
            denominator = denominator * (int)z + previousDenominator;
            previousDenominator = temp;
            numerator = Convert.ToInt32(value * denominator);
        } while (Math.Abs(value - (double)numerator / denominator) > maxError && z != (int)z);

        return new Fraction((n * denominator + numerator) * sign, denominator);
    }

}
