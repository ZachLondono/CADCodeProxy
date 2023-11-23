using CADCodeProxy.CSV;
using CADCodeProxy.Machining;
using FluentAssertions;

namespace CADCodeProxy.Unit.Test.RecordToTokenTests;

public class PocketMappingTests {

    [Fact]
    public void MapTokenRecordToPocket() {

        // Arrange
        var toolName = "3-8Comp";
        var cornerA = new Point(60, 60);
        var cornerB = new Point(315, 60);
        var cornerC = new Point(315, 1000);
        var cornerD = new Point(60, 1000);
        var startDepth = 1;
        var endDepth = 2;
        var sequenceNum = 3;
        var numOfPasses = 4;
        var feedSpeed = 5;
        var spindleSpeed = 6;
        var tokenRecord = new TokenRecord() {
            Name = "Pocket",
            ToolName = toolName,
            StartX = cornerA.X.ToString(),
            StartY = cornerA.Y.ToString(),
            EndX = cornerC.X.ToString(),
            EndY = cornerC.Y.ToString(),
            CenterX = cornerB.X.ToString(),
            CenterY = cornerB.Y.ToString(),
            PocketX = cornerD.X.ToString(),
            PocketY = cornerD.Y.ToString(),
            StartZ = startDepth.ToString(),
            EndZ = endDepth.ToString(),
            SequenceNum = sequenceNum.ToString(),
            NumberOfPasses = numOfPasses.ToString(),
            FeedSpeed = feedSpeed.ToString(),
            SpindleSpeed = spindleSpeed.ToString(),
        };

        // Act
        var pocket = Pocket.FromTokenRecord(tokenRecord);

        // Assert
        pocket.ToolName.Should().Be(toolName);
        pocket.CornerA.Should().Be(cornerA);
        pocket.CornerB.Should().Be(cornerB);
        pocket.CornerC.Should().Be(cornerC);
        pocket.CornerD.Should().Be(cornerD);
        pocket.StartDepth.Should().Be(startDepth);
        pocket.EndDepth.Should().Be(endDepth);
        pocket.SequenceNumber.Should().Be(sequenceNum);
        pocket.NumberOfPasses.Should().Be(numOfPasses);
        pocket.FeedSpeed.Should().Be(feedSpeed);
        pocket.SpindleSpeed.Should().Be(spindleSpeed);

    }

    [Fact]
    public void MapPocketToTokenRecord() {

        // Arrange
        var toolName = "3-8Comp";
        var cornerA = new Point(60, 60);
        var cornerB = new Point(315, 60);
        var cornerC = new Point(315, 1000);
        var cornerD = new Point(60, 1000);
        var startDepth = 1;
        var endDepth = 2;
        var sequenceNum = 3;
        var numOfPasses = 4;
        var feedSpeed = 5;
        var spindleSpeed = 6;
        IToken pocket = new Pocket() {
            ToolName = toolName,
            CornerA = cornerA,
            CornerB = cornerB,
            CornerC = cornerC,
            CornerD = cornerD,
            StartDepth = startDepth,
            EndDepth = endDepth,
            SequenceNumber = sequenceNum,
            NumberOfPasses = numOfPasses,
            FeedSpeed = feedSpeed,
            SpindleSpeed = spindleSpeed,
        };

        // Act
        var record = pocket.ToTokenRecord();

        // Assert
        record.Name.Should().BeEquivalentTo("pocket");
        record.ToolName.Should().Be(toolName);
        record.StartX.Should().Be(cornerA.X.ToString());
        record.StartY.Should().Be(cornerA.X.ToString());
        record.EndX.Should().Be(cornerC.X.ToString());
        record.EndX.Should().Be(cornerC.X.ToString());
        record.CenterX.Should().Be(cornerB.X.ToString());
        record.CenterX.Should().Be(cornerB.X.ToString());
        record.PocketX.Should().Be(cornerD.X.ToString());
        record.PocketX.Should().Be(cornerD.X.ToString());
        record.StartZ.Should().Be(startDepth.ToString());
        record.EndZ.Should().Be(endDepth.ToString());
        record.SequenceNum.Should().Be(sequenceNum.ToString());
        record.NumberOfPasses.Should().Be(numOfPasses.ToString());
        record.FeedSpeed.Should().Be(feedSpeed.ToString());
        record.SpindleSpeed.Should().Be(spindleSpeed.ToString());

    }

    [Fact]
    public void FromTokenRecord_ShouldThrowException_WhenTokenNameDoesNotMatch() {

        // Arrange
        var tokenRecord = new TokenRecord() {
            Name = "Route"
        };

        // Act
        var mapAction = () => Pocket.FromTokenRecord(tokenRecord);

        // Assert
        mapAction.Should().Throw<InvalidOperationException>();

    }

}
