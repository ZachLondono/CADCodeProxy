using CADCodeProxy.Machining;
using CsvHelper;
using System.Globalization;

namespace CADCodeProxy.CSV;

public class CSVTokenWriter {

    public string WriteBatchCSV(Batch batch, string directory) {

        if (!Directory.Exists(directory)) {
            throw new DirectoryNotFoundException($"CSV output directory does not exist {directory}");
        }

        string filePath = GetAvailableFileName(directory, batch.Name, "csv");

        using var writer = new StreamWriter(filePath);
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

        csv.Context.RegisterClassMap<PartRecordMap>();
        csv.Context.RegisterClassMap<TokenRecordMap>();

        csv.WriteHeader<PartRecord>();
        csv.NextRecord();

        foreach (var part in batch.Parts) {

            foreach (var csvPart in part.ToCSVParts(batch.Name)) {
                csv.WriteRecord(csvPart.PartRecord);
                csv.NextRecord();
                foreach (var tokenRecord in csvPart.Tokens) {
                    csv.WriteRecord(tokenRecord);
                    csv.NextRecord();
                }
            }

        }
        
        csv.Flush();

        return filePath;

    }

    private static string GetAvailableFileName(string directory, string fileName, string fileExtension) {

        int index = 0;
        while (true) {

            string file = Path.Combine(directory, $"{fileName}{(index == 0 ? "" : $" ({index})")}.{fileExtension}");

            if (File.Exists(file)) {

                index++;
                continue;

            }

            return file;

        }

    }

}
