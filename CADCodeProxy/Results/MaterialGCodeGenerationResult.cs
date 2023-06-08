namespace CADCodeProxy.Results;

public class MaterialGCodeGenerationResult {
    public required string MaterialName { get; init; }
    public required double MaterialThickness { get; init; }
    public required string[] ProgramNames { get; init; }
    public required UnplacedPart[] UnplacedParts { get; init; }
    public required PlacedPart[] PlacedParts { get; init; }
    public required UsedInventory[] UsedInventory { get; init; }
}
