using System.Collections;

namespace CADCodeProxy.Machining;

public class InfoFields : IEnumerable<KeyValuePair<string, string>> {

    internal readonly Dictionary<string, string> Fields;

    public InfoFields() {
        Fields = new();
    }

    public InfoFields Add(string name, string value) {
        Fields[name] = value;
        return this;
    }

    public IEnumerator<KeyValuePair<string, string>> GetEnumerator() => Fields.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public bool TryGetValue(string fieldName, out string? fieldValue) {
        return Fields.TryGetValue(fieldName, out fieldValue);
    }

}
