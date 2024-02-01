using CADCodeProxy.Enums;
using CADCodeProxy.Machining;
using CADCodeProxy.Machining.Tokens;
using FluentAssertions;

namespace CADCodeProxy.Unit.Test;

public class RouteSequenceTests {

    private readonly TokenAccumulator _sut;

    public RouteSequenceTests() {
        _sut = new TokenAccumulator();
    }

    [Fact]
    public void AddToken_ShouldInsertSetMill_WhenPassedClosedSequenceOfRoutes() {

        var pointA = new Point(0, 0);
        var pointB = new Point(20, 20);
        var pointC = new Point(40, 0);

        var route1 = CreateRoute(pointA, pointB);
        var route2 = CreateRoute(pointB, pointC);
        var route3 = CreateRoute(pointC, pointA);

        _sut.AddToken(route1);
        _sut.AddToken(route2);
        _sut.AddToken(route3);

        var operations = _sut.GetMachiningOperations();

        operations.Should().HaveCount(4);
        operations[0].Should().BeOfType<SetMill>();
        operations[1].Should().Be(route1);
        operations[2].Should().Be(route2);
        operations[3].Should().Be(route3);

    }

    [Fact]
    public void AddToken_ShouldInsertMultipleSetMill_WhenPassedMultipleClosedSequencesOfRoutes() {

        var pointA = new Point(0, 0);
        var pointB = new Point(20, 20);
        var pointC = new Point(40, 0);

        var route1 = CreateRoute(pointA, pointB, toolName:"A");
        var route2 = CreateRoute(pointB, pointC, toolName:"A");
        var route3 = CreateRoute(pointC, pointA, toolName:"A");

        var route4 = CreateRoute(pointA, pointB, toolName:"B");
        var route5 = CreateRoute(pointB, pointC, toolName:"B");
        var route6 = CreateRoute(pointC, pointA, toolName:"B");

        _sut.AddToken(route1);
        _sut.AddToken(route2);
        _sut.AddToken(route3);
        _sut.AddToken(route4);
        _sut.AddToken(route5);
        _sut.AddToken(route6);

        var operations = _sut.GetMachiningOperations();

        operations.Should().HaveCount(8);
        operations[0].Should().BeOfType<SetMill>();
        operations[1].Should().Be(route1);
        operations[2].Should().Be(route2);
        operations[3].Should().Be(route3);
        operations[4].Should().BeOfType<SetMill>();
        operations[5].Should().Be(route4);
        operations[6].Should().Be(route5);
        operations[7].Should().Be(route6);

    }

    [Fact]
    public void AddToken_ShouldNotInsertSetMill_WhenPassedOpenSequencesOfRoutes() {

        var pointA = new Point(0, 0);
        var pointB = new Point(20, 20);
        var pointC = new Point(40, 0);

        var route1 = CreateRoute(pointA, pointB, toolName:"A");
        var route2 = CreateRoute(pointB, pointC, toolName:"A");

        _sut.AddToken(route1);
        _sut.AddToken(route2);
        
        var operations = _sut.GetMachiningOperations();

        operations.Should().HaveCount(2);
        operations[0].Should().Be(route1);
        operations[1].Should().Be(route2);

    }

    [Fact]
    public void AddToken_ShouldThrow_WhenTryingToInsertFilletBetweenRouteAndOutline() {

        var route = CreateRoute(new Point(0, 0), new Point(20, 20));
        var fillet = new Fillet() { Radius = 1 };
        var outline = new OutlineSegment() {
            ToolName = route.ToolName,
            Start = route.End,
            End = new Point(0, 20),
            StartDepth = route.StartDepth,
            EndDepth = route.EndDepth,
            FeedSpeed = route.FeedSpeed,
            NumberOfPasses = route.NumberOfPasses,
            SequenceNumber = route.SequenceNumber,
            SpindleSpeed = route.SpindleSpeed
        };

        _sut.AddToken(route);
        _sut.AddToken(fillet);
        var action = () => _sut.AddToken(outline);

        action.Should().Throw<InvalidOperationException>();

    }

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
