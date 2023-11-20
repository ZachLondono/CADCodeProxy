using CADCode;

namespace CADCodeProxy.Machining;

public interface IMachiningOperation {
    public string ToolName { get; }
    internal void AddToCode(CADCodeCodeClass code);
}
