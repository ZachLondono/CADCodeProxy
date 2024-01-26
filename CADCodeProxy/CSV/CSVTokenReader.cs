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

                // TODO: Handle error where there are two parts in batch with the same face 6 file name
                var face6Parts = group.Where(p => !string.IsNullOrWhiteSpace(p.PartRecord.Face6Flag))
                                        .ToDictionary(p => p.PartRecord.FileName);

                var parts = group.Where(p => string.IsNullOrWhiteSpace(p.PartRecord.Face6Flag))
                                .Select(record => MapRecordToPart(record, face6Parts))
                                .ToArray();

                return new Batch() {
                    Name = group.Key.Trim(),
                    Parts = parts,
                    InfoFields = []
                };

            })
            .ToArray();


    }

    private Part MapRecordToPart(CSVPart record, Dictionary<string, CSVPart> face6Parts) {

        var tokens = record.Tokens
                            .Select(MapRecordToToken)
                            .ToArray();

        PartFace? secondaryFace = null;
        bool hasFace6 = !string.IsNullOrWhiteSpace(record.PartRecord.Face6FileName);
        if (hasFace6) {

            if (face6Parts.TryGetValue(record.PartRecord.Face6FileName, out var face6Part)) {

                var face6Tokens = face6Part.Tokens
                                           .Select(MapRecordToToken)
                                           .ToArray();
        
                var face6Rotation = ParseDoubleFromStringOrDefault(face6Part.PartRecord.Rotation, 0);

                secondaryFace = new() {
                    ProgramName = face6Part.PartRecord.FileName,
                    Tokens = face6Tokens,
                    Rotation = face6Rotation,
                    IsMirrored = IsMirrored(face6Part.PartRecord.Mirror)
                };

            }

            //throw new InvalidOperationException("Face 6 parts not supported");

        }

        var rotation = ParseDoubleFromStringOrDefault(record.PartRecord.Rotation, 0);
        if (rotation != 0) {
            throw new InvalidOperationException("Part rotation is not supported");
        }

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
                IsMirrored = IsMirrored(record.PartRecord.Mirror),
                Rotation = rotation,
                Tokens = tokens
            },
            SecondaryFace = secondaryFace,
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
    }

    internal static bool IsMirrored(string recordValue) {
        return (recordValue.Equals("Y", StringComparison.InvariantCultureIgnoreCase) || recordValue.Equals("mirr on", StringComparison.InvariantCultureIgnoreCase));
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
