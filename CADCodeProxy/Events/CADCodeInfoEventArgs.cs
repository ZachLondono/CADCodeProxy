namespace CADCodeProxy.Events;

public class CADCodeInfoEventArgs(string source, string type, string message) {

    public string Source { get; } = source;

    public string Type { get; set; } = type;

    public string Message { get; } = message;

}
