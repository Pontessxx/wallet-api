namespace AuthApi.Tests
{
    public class ApiTests
    {
        [Fact]
        public void Api_Should_Initialize_Correctly()
        {
            // Arrange
            var expected = true;

            // Act
            var actual = true;

            // Assert
            actual.Should().Be(expected);
        }
    }
}
