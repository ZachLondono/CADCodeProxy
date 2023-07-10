namespace CADCodeProxy.CSV;

internal class CSVPart {

    public required PartRecord PartRecord { get; set; }

    public required IEnumerable<TokenRecord> Tokens { get; set; }

}
