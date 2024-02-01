using CADCodeProxy.CSV;
using CADCodeProxy.Machining;
using CADCodeProxy.Machining.Tokens;
using FluentAssertions;

namespace CADCodeProxy.Unit.Test.RecordToTokenTests;

public class FreePocketSegmentMappingTests {

    [Fact]
    public void MapTokenToFreePocketSegment() {

        var toolName = "3-8Comp";
        var start = new Point(1, 2);
        var end = new Point(3, 4);
        var startDepth = 6;
        var endDepth = 7;
        var sequenceNumber = 8;
        var numberOfPasses = 9;
        var feedSpeed = 10;
        var spindleSpeed = 11;

        IToken pocket = new FreePocketSegment() {
            ToolName = toolName,
            Start = start,
            End = end,
            StartDepth = startDepth,
            EndDepth = endDepth,
            SequenceNumber = sequenceNumber,
            NumberOfPasses = numberOfPasses,
            FeedSpeed = feedSpeed,
            SpindleSpeed = spindleSpeed
        };

        var token = pocket.ToTokenRecord();

        token.Name.Should().BeEquivalentTo("FreePocket");
        token.ToolName.Should().Be(toolName);
        token.StartX.Should().Be(start.X.ToString());
        token.StartY.Should().Be(start.Y.ToString());
        token.EndX.Should().Be(end.X.ToString());
        token.EndY.Should().Be(end.Y.ToString());
        token.StartZ.Should().Be(startDepth.ToString());
        token.EndZ.Should().Be(endDepth.ToString());
        token.SequenceNum.Should().Be(sequenceNumber.ToString());
        token.NumberOfPasses.Should().Be(numberOfPasses.ToString());
        token.FeedSpeed.Should().Be(feedSpeed.ToString());
        token.SpindleSpeed.Should().Be(spindleSpeed.ToString());

    }

    [Fact]
    public void MapFreePocketSegmentToToken() {

        var toolName = "3-8Comp";
        var start = new Point(1, 2);
        var end = new Point(3, 4);
        var startDepth = 6;
        var endDepth = 7;
        var sequenceNumber = 8;
        var numberOfPasses = 9;
        var feedSpeed = 10;
        var spindleSpeed = 11;

        var token = new TokenRecord() {
            Name = "FreePocket",
            ToolName = toolName,
            StartX = start.X.ToString(),
            StartY = start.Y.ToString(),
            EndX = end.X.ToString(),
            EndY = end.Y.ToString(),
            StartZ = startDepth.ToString(),
            EndZ = endDepth.ToString(),
            SequenceNum = sequenceNumber.ToString(),
            NumberOfPasses = numberOfPasses.ToString(),
            FeedSpeed = feedSpeed.ToString(),
            SpindleSpeed = spindleSpeed.ToString()
        };

        var pocket = FreePocketSegment.FromTokenRecord(token);
        pocket.ToolName.Should().BeEquivalentTo(toolName);
        pocket.Start.Should().Be(start);
        pocket.End.Should().Be(end);
        pocket.StartDepth.Should().Be(startDepth);
        pocket.EndDepth.Should().Be(endDepth);
        pocket.SequenceNumber.Should().Be(sequenceNumber);
        pocket.NumberOfPasses.Should().Be(numberOfPasses);
        pocket.FeedSpeed.Should().Be(feedSpeed);
        pocket.SpindleSpeed.Should().Be(spindleSpeed);

    }

    [Fact]
    public void FromTokenRecord_Should_ThrowException_WhenTokenNameDoesNotMatch() {

        var token = new TokenRecord() {
            Name = "Route"
        };

        var action = () => FreePocketSegment.FromTokenRecord(token);

        action.Should().Throw<InvalidOperationException>();

    }

}
