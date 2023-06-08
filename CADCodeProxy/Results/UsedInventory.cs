using CADCode;

namespace CADCodeProxy.Results;

public class UsedInventory {

    public required string Name { get; init; }
    public required double Width { get; init; }
    public required double Length { get; init; }
    public required double Thickness { get; init; }
    public required int Qty { get; init; }
    public required bool IsGrained { get; init; }

    internal static UsedInventory FromCutlistInventory(CutlistInventory inventory) {

        int qty;
        double width, length, thickness;

        _ = int.TryParse(inventory.SheetsUsed, out qty);
        _ = double.TryParse(inventory.Width, out width);
        _ = double.TryParse(inventory.Length, out length);
        _ = double.TryParse(inventory.Thickness, out thickness);

        return new() {
            Name = inventory.Description,
            IsGrained = (inventory.Graining == "Y"),
            Width = width,
            Length = length,
            Thickness = thickness,
            Qty = qty
        };

    }

}
