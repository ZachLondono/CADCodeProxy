using CADCodeProxy.CSV;
using CADCodeProxy.Machining;
using FluentAssertions;

namespace CADCodeProxy.Unit.Test.RecordToTokenTests;

public class FilletMappingTests {

    [Fact]
    public void MapTokenRecordToFillet() {

        // Arrange
        var tokenRecord = new TokenRecord() {
            Name = "Fillet",
            Radius = "123"
        };

        // Act
        var fillet = Fillet.FromTokenRecord(tokenRecord);

        // Assert
        fillet.Radius.Should().Be(123);

    }

    [Fact]
    public void MapFilletToTokenRecord() {

        // Arrange
        IToken fillet = new Fillet() {
            Radius = 123
        };

        // Act
        var token = fillet.ToTokenRecord();

        // Assert
        token.Radius.Should().Be("123");

    }

    [Fact]
    public void FromTokenRecord_ShouldThrowException_WhenTokenNameDoesNotMatch() {

        // Arrange
        var tokenRecord = new TokenRecord() {
            Name = "Rectangle"
        };

        // Act
        var mapAction = () => Fillet.FromTokenRecord(tokenRecord);

        // Assert
        mapAction.Should().Throw<InvalidOperationException>();

    }

}
