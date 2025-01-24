using CADCodeProxy.Machining;
using CADCodeProxy.Machining.Tokens;
using FluentAssertions;

namespace CADCodeProxy.Unit.Test.FilletTests;

public class FilletOutlineTests {

    private readonly TokenAccumulator _sut;

    public FilletOutlineTests() {
        _sut = new TokenAccumulator();
    }

    [Fact]
    public void AddToken_ShouldThrow_WhenSegmentsDoNotConnect() {

        // Arrange
        var segment1 = CreateSegment(new(0, 0), new(10, 0));
        var segment2 = CreateSegment(new(15, 0), new(20, 20));
        var fillet = new Fillet() { Radius = 10 };

        // Act
        _sut.AddToken(segment1);
        _sut.AddToken(fillet);
        var action = () => _sut.AddToken(segment2);

        // Assert
        action.Should().Throw<InvalidOperationException>();

    }

    [Fact]
    public void AddToken_ShouldThrow_WhenSequencesDoNotMatch() {

        // Arrange
        var segment1 = CreateSegment(new(0, 0), new(10, 0), sequenceNumber: 1);
        var segment2 = CreateSegment(new(10, 0), new(20, 20), sequenceNumber: 2);
        var fillet = new Fillet() { Radius = 10 };

        // Act
        _sut.AddToken(segment1);
        _sut.AddToken(fillet);
        var action = () => _sut.AddToken(segment2);

        // Assert
        action.Should().Throw<InvalidOperationException>();

    }

    [Fact]
    public void AddToken_ShouldThrow_WhenToolsDoNotMatch() {

        // Arrange
        var segment1 = CreateSegment(new(0, 0), new(10, 0), toolName: "Tool A");
        var segment2 = CreateSegment(new(10, 0), new(20, 20), toolName: "Tool B");
        var fillet = new Fillet() { Radius = 10 };

        // Act
        _sut.AddToken(segment1);
        _sut.AddToken(fillet);
        var action = () => _sut.AddToken(segment2);

        // Assert
        action.Should().Throw<InvalidOperationException>();

    }

    [Fact]
    public void AddToken_ShouldThrow_WhenPassCountsDoNotMatch() {

        // Arrange
        var segment1 = CreateSegment(new(0, 0), new(10, 0), numberOfPasses: 1);
        var segment2 = CreateSegment(new(10, 0), new(20, 20), numberOfPasses: 2);
        var fillet = new Fillet() { Radius = 10 };

        // Act
        _sut.AddToken(segment1);
        _sut.AddToken(fillet);
        var action = () => _sut.AddToken(segment2);

        // Assert
        action.Should().Throw<InvalidOperationException>();

    }

    [Fact]
    public void AddToken_ShouldThrow_WhenDepthsDoNotMatch() {

        // Arrange
        var segment1 = CreateSegment(new(0, 0), new(10, 0), endDepth: 10);
        var segment2 = CreateSegment(new(10, 0), new(20, 20), startDepth: 15);
        var fillet = new Fillet() { Radius = 10 };

        // Act
        _sut.AddToken(segment1);
        _sut.AddToken(fillet);
        var action = () => _sut.AddToken(segment2);

        // Assert
        action.Should().Throw<InvalidOperationException>();

    }

    [Fact]
    public void GetOperations_ShouldReturnSequenceOfSegments() {

        // Arrange
        var segment1 = CreateSegment(new(25, 0), new(50, 0));
        var fillet1 = new Fillet() { Radius = 5 };
        var segment2 = CreateSegment(new(50, 0), new(50, 50));
        var fillet2 = new Fillet() { Radius = 5 };
        var segment3 = CreateSegment(new(50, 50), new(0, 50));
        var fillet3 = new Fillet() { Radius = 5 };
        var segment4 = CreateSegment(new(0, 50), new(0, 0));
        var fillet4 = new Fillet() { Radius = 5 };
        var segment5 = CreateSegment(new(0, 0), new(25, 0));

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
        operations[0].Should().BeOfType<OutlineSegment>();
        operations[1].Should().BeOfType<ArcOutlineSegment>();
        operations[2].Should().BeOfType<OutlineSegment>();
        operations[3].Should().BeOfType<ArcOutlineSegment>();
        operations[4].Should().BeOfType<OutlineSegment>();
        operations[5].Should().BeOfType<ArcOutlineSegment>();
        operations[6].Should().BeOfType<OutlineSegment>();
        operations[7].Should().BeOfType<ArcOutlineSegment>();
        operations[8].Should().BeOfType<OutlineSegment>();

        Point? lastPos = null;
        foreach (var operation in operations) {

            Point start;
            Point end;

            if (operation is OutlineSegment seg) {
                start = seg.Start;
                end = seg.End;
            } else if (operation is ArcOutlineSegment arc) {
                start = arc.Start;
                end = arc.End;
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
        var segment1 = CreateSegment(new(151.0625, 0), new(302.125, 0));
        var fillet1 = new Fillet() { Radius = 5 };
        var segment2 = CreateSegment(new(302.125, 0), new(302.125, 1521.325));
        var fillet2 = new Fillet() { Radius = 5 };
        var segment3 = CreateSegment(new(302.125, 1521.325), new(0, 1521.325));
        var fillet3 = new Fillet() { Radius = 5 };
        var segment4 = CreateSegment(new(0, 1521.325), new(0, 0));
        var fillet4 = new Fillet() { Radius = 5 };
        var segment5 = CreateSegment(new(0, 0), new(151.0625, 0));

        // Act
        var accumulator1 = new TokenAccumulator();
        accumulator1.AddToken(segment1);
        accumulator1.AddToken(fillet1);
        accumulator1.AddToken(segment2);
        accumulator1.AddToken(fillet2);
        accumulator1.AddToken(segment3);
        accumulator1.AddToken(fillet3);
        accumulator1.AddToken(segment4);
        accumulator1.AddToken(fillet4);
        accumulator1.AddToken(segment5);
        var operations1 = accumulator1.GetMachiningOperations();

        var accumulator2 = new TokenAccumulator();
        accumulator2.AddToken(segment1);
        accumulator2.AddToken(fillet1);
        accumulator2.AddToken(segment2);
        accumulator2.AddToken(fillet2);
        accumulator2.AddToken(segment3);
        accumulator2.AddToken(fillet3);
        accumulator2.AddToken(segment4);
        accumulator2.AddToken(fillet4);
        accumulator2.AddToken(segment5);
        var operations2 = accumulator2.GetMachiningOperations();

        // Assert
        operations1.Should().BeEquivalentTo(operations2, options => options.RespectingRuntimeTypes());

    }

    // TODO: Check if CADCode allows two outline segments with different tool speeds to be linked together

    public OutlineSegment CreateSegment(Point start, Point end,
                            string toolName = "",
                            double startDepth = 0,
                            double endDepth = 0,
                            double feedSpeed = 0,
                            double spindleSpeed = 0,
                            int numberOfPasses = 0,
                            int sequenceNumber = 0) {

        return new OutlineSegment() {
            Start = start,
            End = end,
            ToolName = toolName,
            StartDepth = startDepth,
            EndDepth = endDepth,
            FeedSpeed = feedSpeed,
            SpindleSpeed = spindleSpeed,
            NumberOfPasses = numberOfPasses,
            SequenceNumber = sequenceNumber,
        };

    }

}