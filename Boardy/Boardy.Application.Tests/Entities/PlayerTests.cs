using Boardy.Domain.Entities;
using System;
using Xunit;

namespace Boardy.Domain.Tests.Entities
{
    public class PlayerTests
    {
        [Fact]
        public void Constructor_WithValidName_CreatesPlayerWithNewGuid()
        {
            var expectedName = "Marharyta";
            var player = new Player(expectedName);

            Assert.Equal(expectedName, player.Name);
            Assert.NotEqual(Guid.Empty, player.Id);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void Constructor_WithEmptyName_ThrowsArgumentException(string invalidName)
        {
            var exception = Assert.Throws<ArgumentException>(() => new Player(invalidName));
            
            Assert.Contains("Player's name cannot be empty", exception.Message);
        }
    }
}