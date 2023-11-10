using CADCode;
using CADCodeProxy.CSV;

namespace CADCodeProxy.Machining;

public interface IToken {
    public string ToolName { get; }
    internal void AddToCode(CADCodeCodeClass code);
    internal TokenRecord ToTokenRecord();
}
