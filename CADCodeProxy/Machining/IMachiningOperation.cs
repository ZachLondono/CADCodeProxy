using CADCode;

namespace CADCodeProxy.Machining;

public interface IMachiningOperation {
    internal void AddToCode(CADCodeCodeClass code);
}
