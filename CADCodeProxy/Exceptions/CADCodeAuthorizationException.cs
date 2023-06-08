namespace CADCodeProxy.Exceptions;

public class CADCodeAuthorizationException : InvalidOperationException {
    public CADCodeAuthorizationException(string message) : base(message) { }
}
