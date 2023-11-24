using CADCodeProxy.Enums;
using CADCodeProxy.Machining;
using FluentAssertions;

namespace CADCodeProxy.Unit.Test.FilletTests;

public class FilletSequenceTests {

    private readonly TokenAccumulator _sut;

    public FilletSequenceTests() {
        _sut = new TokenAccumulator();
    }

    [Fact]
    public void AddToken_ShouldThrow_WhenRoutesDoNotConnect() {

        // Arrange
        var route1 = CreateRoute(new(0, 0), new(10, 0));
        var route2 = CreateRoute(new(15, 0), new(20, 20));
        var fillet = new Fillet() { Radius = 10 };

        // Act
        _sut.AddToken(route1);
        _sut.AddToken(fillet);
        var action = () => _sut.AddToken(route2);

        // Assert
        action.Should().Throw<InvalidOperationException>();

    }

    [Fact]
    public void AddToken_ShouldThrow_WhenSequencesDoNotMatch() {

        // Arrange
        var route1 = CreateRoute(new(0, 0), new(10, 0), sequenceNumber: 1);
        var route2 = CreateRoute(new(10, 0), new(20, 20), sequenceNumber: 2);
        var fillet = new Fillet() { Radius = 10 };

        // Act
        _sut.AddToken(route1);
        _sut.AddToken(fillet);
        var action = () => _sut.AddToken(route2);

        // Assert
        action.Should().Throw<InvalidOperationException>();

    }

    [Fact]
    public void AddToken_ShouldThrow_WhenToolsDoNotMatch() {

        // Arrange
        var route1 = CreateRoute(new(0, 0), new(10, 0), toolName: "Tool A");
        var route2 = CreateRoute(new(10, 0), new(20, 20), toolName: "Tool B");
        var fillet = new Fillet() { Radius = 10 };

        // Act
        _sut.AddToken(route1);
        _sut.AddToken(fillet);
        var action = () => _sut.AddToken(route2);

        // Assert
        action.Should().Throw<InvalidOperationException>();

    }

    [Fact]
    public void AddToken_ShouldThrow_WhenPassCountsDoNotMatch() {

        // Arrange
        var route1 = CreateRoute(new(0, 0), new(10, 0), numberOfPasses: 1);
        var route2 = CreateRoute(new(10, 0), new(20, 20), numberOfPasses: 2);
        var fillet = new Fillet() { Radius = 10 };

        // Act
        _sut.AddToken(route1);
        _sut.AddToken(fillet);
        var action = () => _sut.AddToken(route2);

        // Assert
        action.Should().Throw<InvalidOperationException>();

    }

    [Fact]
    public void AddToken_ShouldThrow_WhenDepthsDoNotMatch() {

        // Arrange
        var route1 = CreateRoute(new(0, 0), new(10, 0), endDepth: 10);
        var route2 = CreateRoute(new(10, 0), new(20, 20), startDepth: 15);
        var fillet = new Fillet() { Radius = 10 };

        // Act
        _sut.AddToken(route1);
        _sut.AddToken(fillet);
        var action = () => _sut.AddToken(route2);

        // Assert
        action.Should().Throw<InvalidOperationException>();

    }

    [Fact]
    public void AddToken_ShouldThrow_WhenOffsetsDoNotMatch() {

        // Arrange
        var route1 = CreateRoute(new(0, 0), new(10, 0), offset: Offset.Left);
        var route2 = CreateRoute(new(10, 0), new(20, 20), offset: Offset.Right);
        var fillet = new Fillet() { Radius = 10 };

        // Act
        _sut.AddToken(route1);
        _sut.AddToken(fillet);
        var action = () => _sut.AddToken(route2);

        // Assert
        action.Should().Throw<InvalidOperationException>();

    }

    // TODO: Check if CADCode allows two routes with different tool speeds to be linked together

    public Route CreateRoute(Point start, Point end,
                            string toolName = "",
                            double startDepth = 0,
                            double endDepth = 0,
                            Offset offset = Offset.None,
                            double feedSpeed = 0,
                            double spindleSpeed = 0,
                            int numberOfPasses = 0,
                            int sequenceNumber = 0) {

        return new Route() {
            Start = start,
            End = end,
            ToolName = toolName,
            StartDepth = startDepth,
            EndDepth = endDepth,
            Offset = offset,
            FeedSpeed = feedSpeed,
            SpindleSpeed = spindleSpeed,
            NumberOfPasses = numberOfPasses,
            SequenceNumber = sequenceNumber
        };

    }

}
