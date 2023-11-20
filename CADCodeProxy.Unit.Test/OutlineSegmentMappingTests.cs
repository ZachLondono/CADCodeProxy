using CADCodeProxy.CSV;
using CADCodeProxy.Machining;
using FluentAssertions;

namespace CADCodeProxy.Unit.Test;

public class OutlineSegmentMappingTests {

    [Fact]
    public void MapTokenRecordToRoute() {

        // Arrange
        var toolName = "3-8Comp";
        var start = new Point(10, 20);
        var end = new Point(30, 40);
        var startDepth = 1;
        var endDepth = 2;
        var sequenceNumber = 3;
        var numberOfPasses = 4;
        var feedSpeed = 5;
        var spindleSpeed = 6;
        var tokenRecord = new TokenRecord() {
            Name = "Shape",
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

        // Act
        var shape = OutlineSegment.FromTokenRecord(tokenRecord);

        // Assert
        shape.ToolName.Should().Be(toolName);
		shape.Start.Should().Be(start);
		shape.End.Should().Be(end);
		shape.StartDepth.Should().Be(startDepth);
		shape.EndDepth.Should().Be(endDepth);
		shape.SequenceNumber.Should().Be(sequenceNumber);
		shape.NumberOfPasses.Should().Be(numberOfPasses);
		shape.FeedSpeed.Should().Be(feedSpeed);
		shape.SpindleSpeed.Should().Be(spindleSpeed);

	}

    [Fact]
    public void MapRouteToTokenRecord() {

        // Arrange
        var toolName = "3-8Comp";
        var start = new Point(10, 20);
        var end = new Point(30, 40);
        var startDepth = 1;
        var endDepth = 2;
        var sequenceNumber = 3;
        var numberOfPasses = 4;
        var feedSpeed = 5;
        var spindleSpeed = 6;
        IToken shape = new OutlineSegment() {
            ToolName = toolName,
            Start = start,
            End = end,
            StartDepth = startDepth,
            EndDepth = endDepth,
            SequenceNumber = sequenceNumber,
            NumberOfPasses = numberOfPasses,
            FeedSpeed = feedSpeed,
            SpindleSpeed = spindleSpeed,
        };

        // Act
        var record = shape.ToTokenRecord();

        // Assert
        record.Name.Should().Match(name => name.Equals("shape", StringComparison.InvariantCultureIgnoreCase) || name.Equals("outline", StringComparison.InvariantCultureIgnoreCase));
        record.ToolName.Should().Be(toolName);
        record.StartX.Should().Be(start.X.ToString());
        record.StartY.Should().Be(start.Y.ToString());
        record.EndX.Should().Be(end.X.ToString());
        record.EndY.Should().Be(end.Y.ToString());
        record.StartZ.Should().Be(startDepth.ToString());
        record.EndZ.Should().Be(endDepth.ToString());
        record.SequenceNum.Should().Be(sequenceNumber.ToString());
        record.NumberOfPasses.Should().Be(numberOfPasses.ToString());
        record.FeedSpeed.Should().Be(feedSpeed.ToString());
        record.SpindleSpeed.Should().Be(spindleSpeed.ToString());

    }

    [Fact]
    public void FromTokenRecord_ShouldThrowException_WhenTokenNameDoesNotMatch() {

        // Arrange
        var tokenRecord = new TokenRecord() {
            Name = "Rectangle"
        };

        // Act
        var mapAction = () => Route.FromTokenRecord(tokenRecord);

        // Assert
        mapAction.Should().Throw<InvalidOperationException>();

    }

}
