using CADCode;

namespace CADCodeProxy.Machining;

public interface IMachiningOperation {
    internal void AddToCode(CADCodeCodeClass code, double xOffset = 0, double yOffset = 0);
}
