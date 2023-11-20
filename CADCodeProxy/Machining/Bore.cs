using CADCode;
using CADCodeProxy.CSV;

namespace CADCodeProxy.Machining;

public class Bore : IBoringToken, IMachiningOperation {

    public string ToolName { get; set; } = string.Empty;
    public double ToolDiameter { get; set; } = 0;
    public Point Position { get; set; }
    public double Depth { get; set; }
    public int SequenceNumber { get; set; } = 0;
    public int NumberOfPasses { get; set; } = 0;

    public Bore(string toolName, Point position, double depth, int sequenceNumber = 0, int numberOfPasses = 0) {
        ToolName = toolName;
        ToolDiameter = 0;
        Position = position;
        Depth = depth;
        SequenceNumber = sequenceNumber;
        NumberOfPasses = numberOfPasses;
    }

    public Bore(double toolDiameter, Point position, double depth, int sequenceNumber = 0, int numberOfPasses = 0) {
        ToolName = string.Empty;
        ToolDiameter = toolDiameter;
        Position = position;
        Depth = depth;
        SequenceNumber = sequenceNumber;
        NumberOfPasses = numberOfPasses;
    }

    void IMachiningOperation.AddToCode(CADCodeCodeClass code) {

        code.Bore((float)Position.X,
                    (float)Position.Y,
                    (float)Depth,
                    FaceTypes.CC_UPPER_FACE,
                    (float) ToolDiameter,
                    ToolName,
                    "",
                    "",
                    0f,
                    0f,
                    "",
                    SequenceNumber: SequenceNumber,
                    NumberOfPasses: NumberOfPasses);

    }

    TokenRecord IToken.ToTokenRecord() {

        return new() {
            Name = "Bore",
            StartX = Position.X.ToString(),
            StartY = Position.Y.ToString(),
            StartZ = Depth.ToString(),
            ToolName = ToolName,
            ToolDiameter = ToolDiameter.ToString(),
            SequenceNum = SequenceNumber == 0 ? "" : SequenceNumber.ToString(),
            NumberOfPasses = NumberOfPasses == 0 ? "" : NumberOfPasses.ToString()
        };

    }

    internal static Bore FromTokenRecord(TokenRecord tokenRecord) {

        if (!tokenRecord.Name.Equals("bore", StringComparison.InvariantCultureIgnoreCase)) {
            throw new InvalidOperationException($"Can not map token '{tokenRecord.Name}' to bore.");
        }

        if (!double.TryParse(tokenRecord.StartX, out double startX)) {
            throw new InvalidOperationException("Start X value not specified or invalid for Bore operation");
        }

        if (!double.TryParse(tokenRecord.StartY, out double startY)) {
            throw new InvalidOperationException("Start Y value not specified or invalid for Bore operation");
        }

        if (!double.TryParse(tokenRecord.StartZ, out double depth)) {
            throw new InvalidOperationException("Start Z value not specified or invalid for Bore operation");
        }

        if (!int.TryParse(tokenRecord.SequenceNum, out int sequenceNum)) {
            sequenceNum = 0;
        }

        if (!int.TryParse(tokenRecord.NumberOfPasses, out int numberOfPasses)) {
            numberOfPasses = 0;
        }

        Point position = new(startX, startY);

        if (string.IsNullOrEmpty(tokenRecord.ToolName)) {

            if (!double.TryParse(tokenRecord.ToolDiameter, out double toolDiameter)) {
                throw new InvalidOperationException("Tool value not specified or invalid for Bore operation");
            }

            return new(toolDiameter, position, depth, sequenceNum, numberOfPasses);

        } else {

            return new(tokenRecord.ToolName, position, depth, sequenceNum, numberOfPasses);

        }

    }

}
