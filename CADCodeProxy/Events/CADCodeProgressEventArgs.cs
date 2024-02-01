namespace CADCodeProxy.Events;

public class CADCodeProgressEventArgs(Type source, int value) {

    public Type Source { get; set; } = source;

    public int Value { get; set; } = value;

}
