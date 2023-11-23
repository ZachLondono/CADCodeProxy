using CADCode;

namespace CADCodeProxy.CNC;

public record InventoryItem {

    public required string MaterialName { get; init; }
    public required bool IsGrained { get; init; }
    public required int AvailableQty { get; init; }
    public required int Priority { get; init; }
    public required double PanelWidth { get; init; }
    public required double PanelLength { get; init; }
    public required double PanelThickness { get; init; }

    internal CutlistInventory AsCutlistInventory() {

        var inv = new CutlistInventory();
        Console.WriteLine(inv.Graining);

        return new CutlistInventory() {
            Description = MaterialName,
            Width = PanelWidth.ToString(),
            Length = PanelLength.ToString(),
            Thickness = PanelThickness.ToString(),
            Priority = Priority.ToString(),
            Graining = IsGrained ? "1" : "0",
            Supply = AvailableQty.ToString(),

            Trim1 = "7",
            Trim2 = "7",
            Trim3 = "4",
            Trim4 = "4",
            TrimDrop = false
        };
    }

}
