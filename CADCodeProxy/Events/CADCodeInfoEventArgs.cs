namespace CADCodeProxy.Events;

public class CADCodeInfoEventArgs(Type source, string type, string message) {

    public Type Source { get; } = source;

    public string Type { get; set; } = type;

    public string Message { get; } = message;

}
