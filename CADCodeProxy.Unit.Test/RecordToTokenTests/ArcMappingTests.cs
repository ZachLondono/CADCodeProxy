using CADCodeProxy.CSV;
using CADCodeProxy.Enums;
using CADCodeProxy.Machining;
using CADCodeProxy.Machining.Tokens;
using FluentAssertions;

namespace CADCodeProxy.Unit.Test.RecordToTokenTests;

public class ArcMappingTests {

    [Fact]
    public void MapTokenRecordToArc_WithStandardTokenName() {

        // Arrange
        var toolName = "3-8Comp";
        var start = new Point(1, 2);
        var end = new Point(3, 4);
        var radius = 5;
        var startDepth = 6;
        var endDepth = 7;
        var expectedDirection = ArcDirection.CounterClockWise;
        var directionStr = "CCW";
        var expectedOffset = Offset.Left;
        var offsetStr = "L";
        var sequenceNumber = 8;
        var numberOfPasses = 9;
        var feedSpeed = 10;
        var spindleSpeed = 11;

        var record = new TokenRecord() {
            Name = "Arc",
            ToolName = toolName,
            StartX = start.X.ToString(),
            StartY = start.Y.ToString(),
            EndX = end.X.ToString(),
            EndY = end.Y.ToString(),
            Radius = radius.ToString(),
            StartZ = startDepth.ToString(),
            EndZ = endDepth.ToString(),
            ArcDirection = directionStr,
            OffsetSide = offsetStr,
            SequenceNum = sequenceNumber.ToString(),
            NumberOfPasses = numberOfPasses.ToString(),
            FeedSpeed = feedSpeed.ToString(),
            SpindleSpeed = spindleSpeed.ToString()
        };

        // Act
        var arc = Arc.FromTokenRecord(record);

        // Assert
        arc.Start.Should().Be(start);
        arc.StartDepth.Should().Be(startDepth);
        arc.End.Should().Be(end);
        arc.EndDepth.Should().Be(endDepth);
        arc.Radius.Should().Be(radius);
        arc.Direction.Should().Be(expectedDirection);
        arc.Offset.Should().Be(expectedOffset);
        arc.SequenceNumber.Should().Be(sequenceNumber);
        arc.NumberOfPasses.Should().Be(numberOfPasses);
        arc.FeedSpeed.Should().Be(feedSpeed);
        arc.SpindleSpeed.Should().Be(spindleSpeed);

    }

    [Fact]
    public void MapTokenRecordToArc_WithCWArcDirectionInTokenName() {

        // Arrange
        var toolName = "3-8Comp";
        var start = new Point(1, 2);
        var end = new Point(3, 4);
        var radius = 5;
        var startDepth = 6;
        var endDepth = 7;
        var expectedDirection = ArcDirection.CounterClockWise;
        var directionStr = "";
        var expectedOffset = Offset.Left;
        var offsetStr = "L";
        var sequenceNumber = 8;
        var numberOfPasses = 9;
        var feedSpeed = 10;
        var spindleSpeed = 11;

        var record = new TokenRecord() {
            Name = "CCWArc",
            ToolName = toolName,
            StartX = start.X.ToString(),
            StartY = start.Y.ToString(),
            EndX = end.X.ToString(),
            EndY = end.Y.ToString(),
            Radius = radius.ToString(),
            StartZ = startDepth.ToString(),
            EndZ = endDepth.ToString(),
            ArcDirection = directionStr,
            OffsetSide = offsetStr,
            SequenceNum = sequenceNumber.ToString(),
            NumberOfPasses = numberOfPasses.ToString(),
            FeedSpeed = feedSpeed.ToString(),
            SpindleSpeed = spindleSpeed.ToString()
        };

        // Act
        var arc = Arc.FromTokenRecord(record);

        // Assert
        arc.Start.Should().Be(start);
        arc.StartDepth.Should().Be(startDepth);
        arc.End.Should().Be(end);
        arc.EndDepth.Should().Be(endDepth);
        arc.Radius.Should().Be(radius);
        arc.Direction.Should().Be(expectedDirection);
        arc.Offset.Should().Be(expectedOffset);
        arc.SequenceNumber.Should().Be(sequenceNumber);
        arc.NumberOfPasses.Should().Be(numberOfPasses);
        arc.FeedSpeed.Should().Be(feedSpeed);
        arc.SpindleSpeed.Should().Be(spindleSpeed);

    }

    [Fact]
    public void MapTokenRecordToArc_WithCCWArcDirectionInTokenName() {

        // Arrange
        var toolName = "3-8Comp";
        var start = new Point(1, 2);
        var end = new Point(3, 4);
        var radius = 5;
        var startDepth = 6;
        var endDepth = 7;
        var expectedDirection = ArcDirection.ClockWise;
        var directionStr = "";
        var expectedOffset = Offset.Left;
        var offsetStr = "L";
        var sequenceNumber = 8;
        var numberOfPasses = 9;
        var feedSpeed = 10;
        var spindleSpeed = 11;

        var record = new TokenRecord() {
            Name = "CWArc",
            ToolName = toolName,
            StartX = start.X.ToString(),
            StartY = start.Y.ToString(),
            EndX = end.X.ToString(),
            EndY = end.Y.ToString(),
            Radius = radius.ToString(),
            StartZ = startDepth.ToString(),
            EndZ = endDepth.ToString(),
            ArcDirection = directionStr,
            OffsetSide = offsetStr,
            SequenceNum = sequenceNumber.ToString(),
            NumberOfPasses = numberOfPasses.ToString(),
            FeedSpeed = feedSpeed.ToString(),
            SpindleSpeed = spindleSpeed.ToString()
        };

        // Act
        var arc = Arc.FromTokenRecord(record);

        // Assert
        arc.Start.Should().Be(start);
        arc.StartDepth.Should().Be(startDepth);
        arc.End.Should().Be(end);
        arc.EndDepth.Should().Be(endDepth);
        arc.Radius.Should().Be(radius);
        arc.Direction.Should().Be(expectedDirection);
        arc.Offset.Should().Be(expectedOffset);
        arc.SequenceNumber.Should().Be(sequenceNumber);
        arc.NumberOfPasses.Should().Be(numberOfPasses);
        arc.FeedSpeed.Should().Be(feedSpeed);
        arc.SpindleSpeed.Should().Be(spindleSpeed);

    }

    [Fact]
    public void MapArcToTokenRecord() {

        // Arrange
        var toolName = "3-8Comp";
        var start = new Point(1, 2);
        var end = new Point(3, 4);
        var radius = 5;
        var startDepth = 6;
        var endDepth = 7;
        var direction = ArcDirection.ClockWise;
        var expectedDirectionStr = "CW";
        var offset = Offset.Left;
        var expectedOffsetStr = "L";
        var sequenceNumber = 8;
        var numberOfPasses = 9;
        var feedSpeed = 10;
        var spindleSpeed = 11;

        IToken arc = new Arc() {
            ToolName = toolName,
            Start = start,
            End = end,
            Radius = radius,
            StartDepth = startDepth,
            EndDepth = endDepth,
            Direction = direction,
            Offset = offset,
            SequenceNumber = sequenceNumber,
            NumberOfPasses = numberOfPasses,
            FeedSpeed = feedSpeed,
            SpindleSpeed = spindleSpeed
        };

        // Act
        var token = arc.ToTokenRecord();

        // Assert
        token.Name.Should().BeEquivalentTo("arc");
        token.StartX.Should().Be(start.X.ToString());
        token.StartY.Should().Be(start.Y.ToString());
        token.StartZ.Should().Be(startDepth.ToString());
        token.EndX.Should().Be(end.X.ToString());
        token.EndY.Should().Be(end.Y.ToString());
        token.EndZ.Should().Be(endDepth.ToString());
        token.Radius.Should().Be(radius.ToString());
        token.ArcDirection.Should().Be(expectedDirectionStr);
        token.OffsetSide.Should().Be(expectedOffsetStr);
        token.SequenceNum.Should().Be(sequenceNumber.ToString());
        token.NumberOfPasses.Should().Be(numberOfPasses.ToString());
        token.FeedSpeed.Should().Be(feedSpeed.ToString());
        token.SpindleSpeed.Should().Be(spindleSpeed.ToString());

    }

    [Fact]
    public void FromTokenRecord_ShouldThrowException_WhenTokenNameDoesNotMatch() {

        // Arrange
        var token = new TokenRecord() {
            Name = "Route"
        };

        // Act
        var mapAction = () => Arc.FromTokenRecord(token);

        // Assert
        mapAction.Should().Throw<InvalidOperationException>();

    }

}