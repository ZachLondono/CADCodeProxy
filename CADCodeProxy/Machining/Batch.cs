namespace CADCodeProxy.Machining;

public record Batch {

    public required string Name { get; init; }
    public required Part[] Parts { get; init; }
    public InfoFields InfoFields { get; init; } = new();

}
