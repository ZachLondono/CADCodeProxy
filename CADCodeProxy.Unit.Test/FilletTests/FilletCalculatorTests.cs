using CADCodeProxy.Machining;
using FluentAssertions;
using static CADCodeProxy.Machining.TokenAccumulator;

namespace CADCodeProxy.Unit.Test.FilletTests;

public class FilletCalculatorTests {

    [Fact]
    public void GetFilletPoints_ShouldThrow_WhenAngleBetweenRoutesIs0() {

        // A,C-------B

        var start = new Point(0, 0);
        var center = new Point(10, 0);
        var end = new Point(0, 0);

        var action = () => FilletCalculator.GetFilletPoints(start, center, end, 10);

        action.Should().Throw<InvalidOperationException>();

    }

    [Fact]
    public void GetFilletPoints_ShouldThrow_WhenAngleBetweenRoutesIs180() {

        // A-------B------C

        var start = new Point(0, 0);
        var center = new Point(10, 0);
        var end = new Point(20, 0);

        var action = () => FilletCalculator.GetFilletPoints(start, center, end, 10);

        action.Should().Throw<InvalidOperationException>();

    }

    [Theory]
    [InlineData(5, 5, 20, 5, false)]
    [InlineData(5, 5, 5, 20, true)]
    [InlineData(20, 5, 20, 20, false)]
    [InlineData(20, 5, 5, 5, true)]
    [InlineData(20, 20, 5, 20, false)]
    [InlineData(20, 20, 20, 5, true)]
    [InlineData(5, 20, 5, 5, false)]
    [InlineData(5, 20, 20, 20, true)]
    public void GetFilletPoints_ShouldReturnCorrectArcDirection(double aX, double aY, double cX, double cY, bool expectedIsCCW) {

        var start = new Point(aX, aY);
        var center = new Point(10, 10);
        var end = new Point(cX, cY);

        var (_, _, ccw) = FilletCalculator.GetFilletPoints(start, center, end, 5);

        ccw.Should().Be(expectedIsCCW);

    }

}
