namespace CADCodeProxy.Events;

public class CADCodeErrorEventArgs(string source, string type, int number, string message) {

    public string Source { get; } = source;

    public string Type { get; set; } = type;

    public int Number { get; set; } = number;

    public string Message { get; } = message;

}
