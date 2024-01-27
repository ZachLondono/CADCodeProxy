namespace CADCodeProxy.Events;

public class ErrorEventArgs(string message, Exception? exception = null) {

    public string Message { get; } = message;

    public Exception? Exception { get; } = exception;

}
