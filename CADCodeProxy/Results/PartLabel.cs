namespace CADCodeProxy.Results;

public class PartLabel {

    public required Guid PartId { get; init; }
    public required Dictionary<string, string> Fields { get; init; }

}
