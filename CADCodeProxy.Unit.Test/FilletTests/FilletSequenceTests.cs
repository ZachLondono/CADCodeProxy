using CADCodeProxy.Enums;
using CADCodeProxy.Machining;
using CADCodeProxy.Machining.Tokens;
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

    [Fact]
    public void GetOperations_ShouldReturnSequenceOfSegments() {

        // Arrange
        var segment1 = CreateRoute(new(25, 0), new(50, 0));
        var fillet1 = new Fillet() { Radius = 5 };
        var segment2 = CreateRoute(new(50, 0), new(50, 50));
        var fillet2 = new Fillet() { Radius = 5 };
        var segment3 = CreateRoute(new(50, 50), new(0, 50));
        var fillet3 = new Fillet() { Radius = 5 };
        var segment4 = CreateRoute(new(0, 50), new(0, 0));
        var fillet4 = new Fillet() { Radius = 5 };
        var segment5 = CreateRoute(new(0, 0), new(25, 0));

        // Act
        _sut.AddToken(segment1);
        _sut.AddToken(fillet1);
        _sut.AddToken(segment2);
        _sut.AddToken(fillet2);
        _sut.AddToken(segment3);
        _sut.AddToken(fillet3);
        _sut.AddToken(segment4);
        _sut.AddToken(fillet4);
        _sut.AddToken(segment5);
        var operations = _sut.GetMachiningOperations();

        // Assert
        operations[0].Should().BeOfType<SetMill>();
        operations[1].Should().BeOfType<Route>();
        operations[2].Should().BeOfType<Arc>();
        operations[3].Should().BeOfType<Route>();
        operations[4].Should().BeOfType<Arc>();
        operations[5].Should().BeOfType<Route>();
        operations[6].Should().BeOfType<Arc>();
        operations[7].Should().BeOfType<Route>();
        operations[8].Should().BeOfType<Arc>();
        operations[9].Should().BeOfType<Route>();

        Point? lastPos = null;
        foreach (var operation in operations) {

            Point start;
            Point end;

            if (operation is IRouteSequenceSegment seg) {
                start = seg.Start;
                end = seg.End;
            } else continue;

            if (lastPos is not null) {
                start.Should().Be(lastPos);
            }

            lastPos = end;

        }

    }

    [Fact]
    public void GetOperations_ShouldReturnSameSequence_WhenRunMultipleTimes() {

        // Arrange
        var route1 = CreateRoute(new(151.0625, 0), new(302.125, 0));
        var fillet1 = new Fillet() { Radius = 5 };
        var route2 = CreateRoute(new(302.125, 0), new(302.125, 1521.325));
        var fillet2 = new Fillet() { Radius = 5 };
        var route3 = CreateRoute(new(302.125, 1521.325), new(0, 1521.325));
        var fillet3 = new Fillet() { Radius = 5 };
        var route4 = CreateRoute(new(0, 1521.325), new(0, 0));
        var fillet4 = new Fillet() { Radius = 5 };
        var route5 = CreateRoute(new(0, 0), new(151.0625, 0));


        // Act
        var accumulator1 = new TokenAccumulator();
        accumulator1.AddToken(route1);
        accumulator1.AddToken(fillet1);
        accumulator1.AddToken(route2);
        accumulator1.AddToken(fillet2);
        accumulator1.AddToken(route3);
        accumulator1.AddToken(fillet3);
        accumulator1.AddToken(route4);
        accumulator1.AddToken(fillet4);
        accumulator1.AddToken(route5);
        var operations1 = accumulator1.GetMachiningOperations();

        var accumulator2 = new TokenAccumulator();
        accumulator2.AddToken(route1);
        accumulator2.AddToken(fillet1);
        accumulator2.AddToken(route2);
        accumulator2.AddToken(fillet2);
        accumulator2.AddToken(route3);
        accumulator2.AddToken(fillet3);
        accumulator2.AddToken(route4);
        accumulator2.AddToken(fillet4);
        accumulator2.AddToken(route5);
        var operations2 = accumulator2.GetMachiningOperations();

        // Assert
        operations1.Should().BeEquivalentTo(operations2);

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
