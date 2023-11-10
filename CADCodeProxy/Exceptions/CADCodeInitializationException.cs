namespace CADCodeProxy.Exceptions;

public class CADCodeInitializationException : InvalidOperationException {

    public int ErrorNumber { get; init; }

    public CADCodeInitializationException(int errorNumber, string message) : base(message) {
        ErrorNumber = errorNumber;
    }

}
