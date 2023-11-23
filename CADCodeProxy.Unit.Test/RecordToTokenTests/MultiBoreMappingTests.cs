using CADCodeProxy.CSV;
using CADCodeProxy.Machining;
using FluentAssertions;

namespace CADCodeProxy.Unit.Test.RecordToTokenTests;

public class MultiBoreMappingTests {

    [Fact]
    public void MapTokenRecordToMultiBore_WhenToolNameIsProvided() {

        // Arrange
        var toolName = "3-8Comp";
        var start = new Point(1, 2);
        var end = new Point(3, 4);
        var spacing = 5;
        var depth = 6;
        var sequenceNumber = 7;
        var numberOfPasses = 8;
        IToken bore = new MultiBore(toolName, start, end, spacing, depth, sequenceNumber, numberOfPasses);

        // Act
        var token = bore.ToTokenRecord();

        // Assert
        token.Name.Should().BeEquivalentTo("multibore");
        token.ToolName.Should().Be(toolName);
        token.StartX.Should().Be(start.X.ToString());
        token.StartY.Should().Be(start.Y.ToString());
        token.StartZ.Should().Be(depth.ToString());
        token.EndX.Should().Be(end.X.ToString());
        token.EndY.Should().Be(end.Y.ToString());
        token.Pitch.Should().Be(spacing.ToString());
        token.SequenceNum.Should().Be(sequenceNumber.ToString());
        token.NumberOfPasses.Should().Be(numberOfPasses.ToString());

    }

    [Fact]
    public void MapTokenRecordToMultiBore_WhenToolDiameterIsProvided() {

        // Arrange
        var toolDiameter = 6;
        var start = new Point(1, 2);
        var end = new Point(3, 4);
        var spacing = 5;
        var depth = 6;
        var sequenceNumber = 7;
        var numberOfPasses = 8;
        IToken bore = new MultiBore(toolDiameter, start, end, spacing, depth, sequenceNumber, numberOfPasses);

        // Act
        var token = bore.ToTokenRecord();

        // Assert
        token.Name.Should().BeEquivalentTo("multibore");
        token.ToolDiameter.Should().Be(toolDiameter.ToString());
        token.StartX.Should().Be(start.X.ToString());
        token.StartY.Should().Be(start.Y.ToString());
        token.StartZ.Should().Be(depth.ToString());
        token.EndX.Should().Be(end.X.ToString());
        token.EndY.Should().Be(end.Y.ToString());
        token.Pitch.Should().Be(spacing.ToString());
        token.SequenceNum.Should().Be(sequenceNumber.ToString());
        token.NumberOfPasses.Should().Be(numberOfPasses.ToString());

    }

    [Fact]
    public void MapMultiBoreToTokenRecord_WhenToolNameIsProvided() {

        // Arrange
        var toolName = "3-8Comp";
        var start = new Point(1, 2);
        var end = new Point(3, 4);
        var spacing = 5;
        var depth = 6;
        var sequenceNumber = 7;
        var numberOfPasses = 8;
        var token = new TokenRecord() {
            Name = "multibore",
            ToolName = toolName.ToString(),
            StartX = start.X.ToString(),
            StartY = start.Y.ToString(),
            StartZ = depth.ToString(),
            EndX = end.X.ToString(),
            EndY = end.Y.ToString(),
            Pitch = spacing.ToString(),
            SequenceNum = sequenceNumber.ToString(),
            NumberOfPasses = numberOfPasses.ToString()
        };

        // Act
        var bore = MultiBore.FromTokenRecord(token);

        // Assert
        bore.ToolName.Should().Be(toolName);
        bore.Start.Should().Be(start);
        bore.End.Should().Be(end);
        bore.Spacing.Should().Be(spacing);
        bore.Depth.Should().Be(depth);
        bore.SequenceNumber.Should().Be(sequenceNumber);
        bore.NumberOfPasses.Should().Be(numberOfPasses);

    }

    [Fact]
    public void MapMultiBoreToTokenRecord_WhenToolDiameterIsProvided() {

        // Arrange
        var toolDiameter = 6;
        var start = new Point(1, 2);
        var end = new Point(3, 4);
        var spacing = 5;
        var depth = 6;
        var sequenceNumber = 7;
        var numberOfPasses = 8;
        var token = new TokenRecord() {
            Name = "multibore",
            ToolDiameter = toolDiameter.ToString(),
            StartX = start.X.ToString(),
            StartY = start.Y.ToString(),
            StartZ = depth.ToString(),
            EndX = end.X.ToString(),
            EndY = end.Y.ToString(),
            Pitch = spacing.ToString(),
            SequenceNum = sequenceNumber.ToString(),
            NumberOfPasses = numberOfPasses.ToString()
        };

        // Act
        var bore = MultiBore.FromTokenRecord(token);

        // Assert
        bore.ToolDiameter.Should().Be(toolDiameter);
        bore.Start.Should().Be(start);
        bore.End.Should().Be(end);
        bore.Spacing.Should().Be(spacing);
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
        var mapAction = () => MultiBore.FromTokenRecord(token);

        // Assert
        mapAction.Should().Throw<InvalidOperationException>();

    }

}
