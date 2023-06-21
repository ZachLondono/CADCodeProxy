using CADCode;
using CADCodeProxy.CSV;

namespace CADCodeProxy.Machining;

public interface IToken {
    internal void AddToCode(CADCodeCodeClass code);
    internal TokenRecord ToTokenRecord();
}
