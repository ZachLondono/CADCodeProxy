using CADCodeProxy.Machining;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace CADCodeProxy.CSV;

public class CSVTokenReader {

    public Batch[] ReadBatchCSV(string filePath) {

        var config = new CsvConfiguration(CultureInfo.InvariantCulture) {
            IgnoreBlankLines = true,
            MissingFieldFound = null
        };

        List<CSVPart> parts = [];

        var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using (var reader = new StreamReader(fileStream))
        using (var csv = new CsvReader(reader, config)) {

            csv.Context.RegisterClassMap<PartRecordReadMap>();
            csv.Context.RegisterClassMap<TokenRecordReadMap>();

            csv.Read();
            csv.ReadHeader();

            while (csv.Read()) {

                var currentToken = csv.GetField(5)?.ToLowerInvariant() ?? "";
                switch (currentToken) {
                    case "border":
                        var part = csv.GetRecord<PartRecord>() ?? throw new InvalidOperationException("Unable to read part from csv record");
                        parts.Add(new() {
                            PartRecord = part,
                            Tokens = []
                        });
                        break; ;

                    case "":
                        break;

                    default:
                        var tokenRecord = csv.GetRecord<TokenRecord>() ?? throw new InvalidOperationException($"Unable to read token from csv record '{currentToken}'");
                        parts.Last().Tokens.Add(tokenRecord);
                        break;

                }

            }

        }

        return parts.GroupBy(part => part.PartRecord.JobName)
            .Select(group => {

                var parts = group.Select(record => {

                    var tokens = record.Tokens
                                        .Select(MapRecordToToken)
                                        .ToArray();

                    // TODO: check if part has a face6

                    return new Part() {
                        Qty = int.Parse(record.PartRecord.Qty),
                        Width = double.Parse(record.PartRecord.Width),
                        Length = double.Parse(record.PartRecord.Length),
                        Thickness = double.Parse(record.PartRecord.Thickness),
                        Material = record.PartRecord.Material,
                        IsGrained = record.PartRecord.Graining == "Y",
                        Width1Banding = new(record.PartRecord.WidthColor1, record.PartRecord.WidthMaterial1),
                        Width2Banding = new(record.PartRecord.WidthColor2, record.PartRecord.WidthMaterial2),
                        Length1Banding = new(record.PartRecord.LengthColor1, record.PartRecord.LengthMaterial1),
                        Length2Banding = new(record.PartRecord.LengthColor2, record.PartRecord.LengthMaterial2),
                        PrimaryFace = new() {
                            ProgramName = record.PartRecord.FileName,
                            IsMirrored = (record.PartRecord.Mirror.Equals("Y", StringComparison.InvariantCultureIgnoreCase) || record.PartRecord.Mirror.Equals("mirr on", StringComparison.InvariantCultureIgnoreCase)),
                            Rotation = ParseDoubleFromStringOrDefault(record.PartRecord.Rotation, 0),
                            Tokens = tokens
                        },
                        InfoFields = new() {
                            { "CustomerInfo1", record.PartRecord.CustomerInfo1 },
                            { "Level1", record.PartRecord.Level1 },
                            { "Comment1", record.PartRecord.Comment1 },
                            { "Comment2", record.PartRecord.Comment2 },
                            { "Side1Color", record.PartRecord.Side1Color },
                            { "Side1Material", record.PartRecord.Side1Material },
                            { "CabinetNumber", record.PartRecord.CabinetNumber },
                            { "ProductName", record.PartRecord.ProductName },
                            { "Description", record.PartRecord.Description }
                        }
                    };
                }).ToArray();

                return new Batch() {
                    Name = group.Key,
                    Parts = parts,
                    InfoFields = []
                };

            })
            .ToArray();


    }

    internal static double ParseDoubleFromStringOrDefault(string input, double defaultValue = 0) {
        if (double.TryParse(input, out var result)) {
            return result;
        }
        return defaultValue;
    }

    internal IToken MapRecordToToken(TokenRecord record) {
        string tokenName = record.Name;
        if (record.Name.Contains('*')) {
            tokenName = record.Name.Split('*')[0];
        }
        return tokenName.ToLower() switch {
            "bore" => Bore.FromTokenRecord(record),
            "multibore" => MultiBore.FromTokenRecord(record),
            "arc" or "cwarc" or "ccwarc" => Arc.FromTokenRecord(record),
            "shape" or "outline" => OutlineSegment.FromTokenRecord(record),
            "pocket" => MapRecordToPocket(record),
            "freepocket" => MapRecordToFreePocket(record),
            "route" => Route.FromTokenRecord(record),
            "rectangle" => Rectangle.FromTokenRecord(record),
            "fillet" => Fillet.FromTokenRecord(record),
            _ => throw new NotImplementedException($"Token not supported '{record.Name}'"),
        };
    }

    internal static IToken MapRecordToPocket(TokenRecord record) {

        if (double.TryParse(record.StartX, out _)
            || double.TryParse(record.StartX, out _)) {

            return Pocket.FromTokenRecord(record);
            
        }

        return CircularPocket.FromTokenRecord(record);

    }

    internal static IToken MapRecordToFreePocket(TokenRecord record) {

        if (double.TryParse(record.Radius, out _)
            || double.TryParse(record.ArcDirection, out _)) {

            return FreePocketArcSegment.FromTokenRecord(record); 

        }

        return FreePocketSegment.FromTokenRecord(record);

    }

}
