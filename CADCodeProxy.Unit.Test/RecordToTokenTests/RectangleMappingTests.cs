using CADCodeProxy.CSV;
using CADCodeProxy.Enums;
using CADCodeProxy.Machining;
using CADCodeProxy.Machining.Tokens;
using FluentAssertions;

namespace CADCodeProxy.Unit.Test.RecordToTokenTests;

public class RectangleMappingTests {

    [Fact]
    public void MapTokenRecordToRectangle() {

        // Arrange
        var toolName = "3-8Comp";
        var cornerA = new Point(60, 60);
        var cornerB = new Point(315, 60);
        var cornerC = new Point(315, 1000);
        var cornerD = new Point(60, 1000);
        var startDepth = 1;
        var endDepth = 2;
        var offsetStr = "I";
        var expectedOffset = Offset.Inside;
        var sequenceNum = 3;
        var numOfPasses = 4;
        var feedSpeed = 5;
        var spindleSpeed = 6;
        var radius = 7;
        var tokenRecord = new TokenRecord() {
            Name = "Rectangle",
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
            OffsetSide = offsetStr,
            SequenceNum = sequenceNum.ToString(),
            NumberOfPasses = numOfPasses.ToString(),
            FeedSpeed = feedSpeed.ToString(),
            SpindleSpeed = spindleSpeed.ToString(),
            Radius = radius.ToString(),

            ArcDirection = "",
            StartAngle = "",
            EndAngle = "",
            Pitch = "",
            ToolDiameter = ""
        };

        // Act
        var rectangle = Rectangle.FromTokenRecord(tokenRecord);

        // Assert
        rectangle.ToolName.Should().Be(toolName);
        rectangle.CornerA.Should().Be(cornerA);
        rectangle.CornerB.Should().Be(cornerB);
        rectangle.CornerC.Should().Be(cornerC);
        rectangle.CornerD.Should().Be(cornerD);
        rectangle.StartDepth.Should().Be(startDepth);
        rectangle.EndDepth.Should().Be(endDepth);
        rectangle.Offset.Should().Be(expectedOffset);
        rectangle.SequenceNumber.Should().Be(sequenceNum);
        rectangle.NumberOfPasses.Should().Be(numOfPasses);
        rectangle.FeedSpeed.Should().Be(feedSpeed);
        rectangle.SpindleSpeed.Should().Be(spindleSpeed);
        rectangle.Radius.Should().Be(radius);

    }

    [Fact]
    public void MapRectangleToTokenRecord() {

        // Arrange
        var toolName = "3-8Comp";
        var cornerA = new Point(60, 60);
        var cornerB = new Point(315, 60);
        var cornerC = new Point(315, 1000);
        var cornerD = new Point(60, 1000);
        var startDepth = 1;
        var endDepth = 2;
        var expectedOffsetStr = "I";
        var offset = Offset.Inside;
        var sequenceNum = 3;
        var numOfPasses = 4;
        var feedSpeed = 5;
        var spindleSpeed = 6;
        var radius = 7;
        IToken rectangle = new Rectangle() {
            ToolName = toolName,
            CornerA = cornerA,
            CornerB = cornerB,
            CornerC = cornerC,
            CornerD = cornerD,
            StartDepth = startDepth,
            EndDepth = endDepth,
            Offset = offset,
            SequenceNumber = sequenceNum,
            NumberOfPasses = numOfPasses,
            FeedSpeed = feedSpeed,
            SpindleSpeed = spindleSpeed,
            Radius = radius
        };

        // Act
        var record = rectangle.ToTokenRecord();

        // Assert
        record.Name.Should().BeEquivalentTo("rectangle");
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
        record.OffsetSide.Should().Be(expectedOffsetStr);
        record.SequenceNum.Should().Be(sequenceNum.ToString());
        record.NumberOfPasses.Should().Be(numOfPasses.ToString());
        record.FeedSpeed.Should().Be(feedSpeed.ToString());
        record.SpindleSpeed.Should().Be(spindleSpeed.ToString());
        record.Radius.Should().Be(radius.ToString());

    }

    [Fact]
    public void FromTokenRecord_ShouldThrowException_WhenTokenNameDoesNotMatch() {

        // Arrange
        var tokenRecord = new TokenRecord() {
            Name = "Route"
        };

        // Act
        var mapAction = () => Rectangle.FromTokenRecord(tokenRecord);

        // Assert
        mapAction.Should().Throw<InvalidOperationException>();

    }

}
