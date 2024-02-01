using CADCodeProxy.Enums;

namespace CADCodeProxy.Machining;

public interface IRouteSequenceSegment : IMachiningOperation {

    public string ToolName { get; }
    public Point Start { get; }
    public Point End { get; }
    public double StartDepth { get; }
    public Offset Offset { get; }
    public int SequenceNumber { get; }
    public int NumberOfPasses { get; }
    public double FeedSpeed { get; }
    public double SpindleSpeed { get; }

}
