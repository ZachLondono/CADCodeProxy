using CADCodeProxy.CSV;
using CADCodeProxy.Enums;
using CADCodeProxy.Machining;
using FluentAssertions;

namespace CADCodeProxy.Unit.Test.RecordToTokenTests;

public class RouteMappingTests {

    [Fact]
    public void MapTokenRecordToRoute() {

        // Arrange
        var toolName = "3-8Comp";
        var start = new Point(10, 20);
        var end = new Point(30, 40);
        var startDepth = 1;
        var endDepth = 2;
        var expectedOffset = Offset.Left;
        var offsetStr = "L";
        var sequenceNumber = 3;
        var numberOfPasses = 4;
        var feedSpeed = 5;
        var spindleSpeed = 6;
        var tokenRecord = new TokenRecord() {
            Name = "Route",
            ToolName = toolName,
            StartX = start.X.ToString(),
            StartY = start.Y.ToString(),
            EndX = end.X.ToString(),
            EndY = end.Y.ToString(),
            StartZ = startDepth.ToString(),
            EndZ = endDepth.ToString(),
            OffsetSide = offsetStr,
            SequenceNum = sequenceNumber.ToString(),
            NumberOfPasses = numberOfPasses.ToString(),
            FeedSpeed = feedSpeed.ToString(),
            SpindleSpeed = spindleSpeed.ToString()
        };

        // Act
        var route = Route.FromTokenRecord(tokenRecord);

        // Assert
        route.ToolName.Should().Be(toolName);
        route.Start.Should().Be(start);
        route.End.Should().Be(end);
        route.StartDepth.Should().Be(startDepth);
        route.EndDepth.Should().Be(endDepth);
        route.Offset.Should().Be(expectedOffset);
        route.SequenceNumber.Should().Be(sequenceNumber);
        route.NumberOfPasses.Should().Be(numberOfPasses);
        route.FeedSpeed.Should().Be(feedSpeed);
        route.SpindleSpeed.Should().Be(spindleSpeed);

    }

    [Fact]
    public void MapRouteToTokenRecord() {

        // Arrange
        var toolName = "3-8Comp";
        var start = new Point(10, 20);
        var end = new Point(30, 40);
        var startDepth = 1;
        var endDepth = 2;
        var offset = Offset.Left;
        var expectedOffsetStr = "L";
        var sequenceNumber = 3;
        var numberOfPasses = 4;
        var feedSpeed = 5;
        var spindleSpeed = 6;
        IToken route = new Route() {
            ToolName = toolName,
            Start = start,
            End = end,
            StartDepth = startDepth,
            EndDepth = endDepth,
            Offset = offset,
            SequenceNumber = sequenceNumber,
            NumberOfPasses = numberOfPasses,
            FeedSpeed = feedSpeed,
            SpindleSpeed = spindleSpeed,
        };

        // Act
        var record = route.ToTokenRecord();

        // Assert
        record.Name.Should().BeEquivalentTo("route");
        record.ToolName.Should().Be(toolName);
        record.StartX.Should().Be(start.X.ToString());
        record.StartY.Should().Be(start.Y.ToString());
        record.EndX.Should().Be(end.X.ToString());
        record.EndY.Should().Be(end.Y.ToString());
        record.StartZ.Should().Be(startDepth.ToString());
        record.EndZ.Should().Be(endDepth.ToString());
        record.OffsetSide.Should().Be(expectedOffsetStr);
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
