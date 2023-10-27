namespace CADCodeProxy.Machining;

public class Batch {

    public required string Name { get; set; }
    public required Part[] Parts { get; set; }
    public InfoFields InfoFields { get; set; } = new();

}
