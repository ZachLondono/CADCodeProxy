namespace CADCodeProxy.Events;

public class CADCodeErrorEventArgs(Type source, string type, int number, string message) {

    public Type Source { get; } = source;

    public string Type { get; set; } = type;

    public int Number { get; set; } = number;

    public string Message { get; } = message;

}
