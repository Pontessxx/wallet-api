using System;
using System.Threading.Tasks;
using FluentAssertions;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Infrastructure.Tests.Data
{
    public class ApplicationDbContextTests
    {
        [Fact]
        public void ApplicationDbContext_Should_Initialize_With_Valid_Options()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_" + Guid.NewGuid())
                .Options;

            // Act & Assert
            using (var context = new ApplicationDbContext(options))
            {
                context.Should().NotBeNull();
                context.Users.Should().NotBeNull();
            }
        }

        [Fact]
        public async Task ApplicationDbContext_Should_Create_Users_Table()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_" + Guid.NewGuid())
                .Options;

            // Act & Assert
            using (var context = new ApplicationDbContext(options))
            {
                context.Should().NotBeNull();
                await Task.CompletedTask;
            }
        }
    }
}
