namespace Domain.Tests.Entities
{
    public class UserTests
    {
        [Fact]
        public void User_Should_Initialize_With_Default_Role_User()
        {
            // Arrange & Act
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = "testuser",
                PasswordHash = "hash123"
            };

            // Assert
            user.Role.Should().Be(RoleUser.User);
        }

        [Fact]
        public void User_Should_Set_Username_Correctly()
        {
            // Arrange
            var username = "john";

            // Act
            var user = new User { Username = username };

            // Assert
            user.Username.Should().Be(username);
        }
    }
}
