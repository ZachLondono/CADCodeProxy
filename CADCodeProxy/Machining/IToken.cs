using CADCodeProxy.CSV;

namespace CADCodeProxy.Machining;

public interface IToken {
    internal TokenRecord ToTokenRecord();
}