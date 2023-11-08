using CADCodeProxy.Machining;

namespace CADCodeProxy.Results;

public class PlacedPart {

    public required Guid PartId { get; init; }
    public required string Name { get; init; }
    public required double Width { get; init; }
    public required double Length { get; init; }
    public required int UsedInventoryIndex { get; init; }
    public required int ProgramIndex { get; init; }
    public required double Area { get; set; }
    public required bool IsRotated { get; set; }
    public required Point InsertionPoint { get; set; }

    internal static PlacedPart FromPart(CADCode.Part part, Guid partId) {
        return new() {
            PartId = partId,
            Name = part.Face5Filename,
            Width = double.Parse(part.Width),
            Length = double.Parse(part.Length),
            Area = (double) part.Area,
            IsRotated = part.Rotated,
            UsedInventoryIndex = part.ParentInventoryItem - 1,
            ProgramIndex = part.PatternNumber - 1,
            InsertionPoint = new(part.InsertionX, part.InsertionY)
        };
    }

}
