using System;
using Auth.Domain;
using FluentAssertions;
using Xunit;

namespace Domain.Tests.Entities
{
    public class UserTests
    {
        [Fact]
        public void User_Should_Initialize_With_Default_Role_User()
        {
            // Arrange & Act
            var user = new User { Id = Guid.NewGuid(), Email = "test@example.com", PasswordHash = "hash123" };

            // Assert
            user.Role.Should().Be(RoleUser.User);
        }

        [Fact]
        public void User_Should_Set_Email_Correctly()
        {
            // Arrange
            var email = "john@example.com";

            // Act
            var user = new User { Email = email };

            // Assert
            user.Email.Should().Be(email);
        }
    }
}
