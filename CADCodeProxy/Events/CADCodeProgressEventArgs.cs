namespace CADCodeProxy.Events;

public class CADCodeProgressEventArgs(string source, int value) {

    public string Source { get; set; } = source;

    public int Value { get; set; } = value;

}
