using CADCodeProxy.CSV;
using CADCodeProxy.Machining;
using FluentAssertions;

namespace CADCodeProxy.Unit.Test;

public class BoreMappingTests {

    [Fact]
    public void MapTokenRecordToBore_WhenToolNameIsProvided() {

        // Arrange
        var toolName = "3-8Comp";
        var position = new Point(1, 2);
        var depth = 3;
        var sequenceNumber = 4;
        var numberOfPasses = 5;
        IToken bore = new Bore(toolName, position, depth, sequenceNumber, numberOfPasses);

        // Act
        var token = bore.ToTokenRecord();

        // Assert
        token.Name.Should().BeEquivalentTo("bore");
        token.ToolName.Should().Be(toolName);
        token.StartX.Should().Be(position.X.ToString());
        token.StartY.Should().Be(position.Y.ToString());
        token.StartZ.Should().Be(depth.ToString());
        token.SequenceNum.Should().Be(sequenceNumber.ToString());
        token.NumberOfPasses.Should().Be(numberOfPasses.ToString());

    }

    [Fact]
    public void MapTokenRecordToBore_WhenToolDiameterIsProvided() {

        // Arrange
        var toolDiameter = 6;
        var position = new Point(1, 2);
        var depth = 3;
        var sequenceNumber = 4;
        var numberOfPasses = 5;
        IToken bore = new Bore(toolDiameter, position, depth, sequenceNumber, numberOfPasses);

        // Act
        var token = bore.ToTokenRecord();

        // Assert
        token.Name.Should().BeEquivalentTo("bore");
        token.ToolDiameter.Should().Be(toolDiameter.ToString());
        token.StartX.Should().Be(position.X.ToString());
        token.StartY.Should().Be(position.Y.ToString());
        token.StartZ.Should().Be(depth.ToString());
        token.SequenceNum.Should().Be(sequenceNumber.ToString());
        token.NumberOfPasses.Should().Be(numberOfPasses.ToString());

    }

    [Fact]
    public void MapBoreToTokenRecord_WhenToolNameIsProvided() {

        // Arrange
        var toolName = "3-8Comp";
        var position = new Point(1, 2);
        var depth = 3;
        var sequenceNumber = 4;
        var numberOfPasses = 5;
        var token = new TokenRecord() {
            Name = "bore",
            ToolName = toolName.ToString(),
            StartX = position.X.ToString(),
            StartY = position.Y.ToString(),
            StartZ = depth.ToString(),
            SequenceNum = sequenceNumber.ToString(),
            NumberOfPasses = numberOfPasses.ToString()
        };

        // Act
        var bore = Bore.FromTokenRecord(token);

        // Assert
        bore.ToolName.Should().Be(toolName);
        bore.Position.Should().Be(position);
        bore.Depth.Should().Be(depth);
        bore.SequenceNumber.Should().Be(sequenceNumber);
        bore.NumberOfPasses.Should().Be(numberOfPasses);

    }

    [Fact]
    public void MapBoreToTokenRecord_WhenToolDiameterIsProvided() {

        // Arrange
        var toolDiameter = 6;
        var position = new Point(1, 2);
        var depth = 3;
        var sequenceNumber = 4;
        var numberOfPasses = 5;
        var token = new TokenRecord() {
            Name = "bore",
            ToolDiameter = toolDiameter.ToString(),
            StartX = position.X.ToString(),
            StartY = position.Y.ToString(),
            StartZ = depth.ToString(),
            SequenceNum = sequenceNumber.ToString(),
            NumberOfPasses = numberOfPasses.ToString()
        };

        // Act
        var bore = Bore.FromTokenRecord(token);

        // Assert
        bore.ToolDiameter.Should().Be(toolDiameter);
        bore.Position.Should().Be(position);
        bore.Depth.Should().Be(depth);
        bore.SequenceNumber.Should().Be(sequenceNumber);
        bore.NumberOfPasses.Should().Be(numberOfPasses);

    }

    [Fact]
    public void FromTokenRecord_ShouldThrowException_WhenTokenNameDoesNotMatch() {

        // Arrange
        var token = new TokenRecord() {
            Name = "Route"
        };

        // Act
        var mapAction = () => Bore.FromTokenRecord(token);

        // Assert
        mapAction.Should().Throw<InvalidOperationException>();

    }

}
