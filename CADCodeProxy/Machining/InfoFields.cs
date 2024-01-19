using System.Collections;

namespace CADCodeProxy.Machining;

public class InfoFields : IEnumerable<KeyValuePair<string, string>> {

    internal readonly Dictionary<string, string> _fields;

    public InfoFields() {
        _fields = [];
    }

    public InfoFields Add(string name, string value) {
        _fields[name] = value;
        return this;
    }

    public IEnumerator<KeyValuePair<string, string>> GetEnumerator() => _fields.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public bool TryGetValue(string fieldName, out string? fieldValue) {
        return _fields.TryGetValue(fieldName, out fieldValue);
    }

}
