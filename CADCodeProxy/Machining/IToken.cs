using CADCode;

namespace CADCodeProxy.Machining;

public interface IToken {
    internal void AddToCode(CADCodeCodeClass code);
}
