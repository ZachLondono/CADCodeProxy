namespace CADCodeProxy.Exceptions;

public class InvalidInventoryException : InvalidOperationException {
    public InvalidInventoryException(string message) : base(message) { }
}
