using CADCodeProxy.CSV;
using CADCodeProxy.Machining;
using FluentAssertions;

namespace CADCodeProxy.Unit.Test.RecordToTokenTests;

public class CircularPocketMappingTests {

    [Fact]
    public void MapTokenToCircularPocket() {

        var toolName = "Pocket";
        var center = new Point(1,2);
        var depth = 3;
        var radius = 4;
        var sequenceNumber = 5;
        var numberOfPasses = 6;
        var feedSpeed = 7;
        var spindleSpeed = 8;

        var token = new TokenRecord() {
            Name = "Pocket",
            ToolName = toolName,
            CenterX = center.X.ToString(),
            CenterY = center.Y.ToString(),
            StartZ = depth.ToString(),
            Radius = radius.ToString(),
            SequenceNum = sequenceNumber.ToString(),
            NumberOfPasses = numberOfPasses.ToString(),
            FeedSpeed = feedSpeed.ToString(),
            SpindleSpeed = spindleSpeed.ToString(),
        };

        var pocket = CircularPocket.FromTokenRecord(token);

        pocket.ToolName.Should().BeEquivalentTo(toolName);
        pocket.Center.Should().Be(center);
        pocket.Depth.Should().Be(depth);
        pocket.Radius.Should().Be(radius);
        pocket.SequenceNumber.Should().Be(sequenceNumber);
        pocket.NumberOfPasses.Should().Be(numberOfPasses);
        pocket.FeedSpeed.Should().Be(feedSpeed);
        pocket.SpindleSpeed.Should().Be(spindleSpeed);

    }

    [Fact]
    public void MapCircularPocketToToken() {

        var toolName = "Pocket";
        var center = new Point(1,2);
        var depth = 3;
        var radius = 4;
        var sequenceNumber = 5;
        var numberOfPasses = 6;
        var feedSpeed = 7;
        var spindleSpeed = 8;

        IToken pocket = new CircularPocket() {
            ToolName = toolName,
            Center = center,
            Depth = depth,
            Radius = radius,
            SequenceNumber = sequenceNumber,
            NumberOfPasses = numberOfPasses,
            FeedSpeed = feedSpeed,
            SpindleSpeed = spindleSpeed,
        };

        var token = pocket.ToTokenRecord();

        token.Name.Should().BeEquivalentTo("Pocket");
        token.ToolName.Should().BeEquivalentTo(toolName);
        token.CenterX.Should().Be(center.X.ToString());
        token.CenterY.Should().Be(center.Y.ToString());
        token.StartZ.Should().Be(depth.ToString());
        token.Radius.Should().Be(radius.ToString());
        token.SequenceNum.Should().Be(sequenceNumber.ToString());
        token.NumberOfPasses.Should().Be(numberOfPasses.ToString());
        token.FeedSpeed.Should().Be(feedSpeed.ToString());
        token.SpindleSpeed.Should().Be(spindleSpeed.ToString());

    }

    [Fact]
    public void FromTokenRecord_Should_ThrowException_WhenTokenNameDoesNotMatch() {
        
        var token = new TokenRecord() {
            Name = "Route"
        };

        var action = () => CircularPocket.FromTokenRecord(token);

        action.Should().Throw<InvalidOperationException>();

    }

}
