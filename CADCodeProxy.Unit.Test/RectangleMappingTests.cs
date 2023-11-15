using CADCodeProxy.CSV;
using CADCodeProxy.Enums;
using CADCodeProxy.Machining;
using FluentAssertions;

namespace CADCodeProxy.Unit.Test;

public class RectangleMappingTests {

    [Fact]
    public void MapTokenRecordToRectangle() {

        // Arrange
        var toolName = "";
        var cornerA = new Point(60, 60);
        var cornerB = new Point(315, 60);
        var cornerC = new Point(315, 1000);
        var cornerD = new Point(60, 1000);
        var startDepth = 0;
        var endDepth = 0;
        var offsetStr = "I";
        var expectedOffset = Offset.Inside;
        var sequenceNum = 0;
        var numOfPasses = 0;
        var tokenRecord = new TokenRecord() {
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
            NumberOfPasses = numOfPasses.ToString()
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

    }

}