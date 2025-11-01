using Xunit;
using FluentAssertions;
using Moq;
using Users.Infrastructure.Repository;

namespace Users.Infrastructure.Repository.Tests
{
    public class UserRepositoryTests
    {
        // TODO: Add mock dependencies
        // private readonly Mock<ISomeDependency> _someDependencyMock;
        
        // public UserRepositoryTests()
        // {
        //     _someDependencyMock = new Mock<ISomeDependency>();
        // }
        
        [Fact]
        public void UserRepository_Should_Be_Instantiated()
        {
            // Arrange
            
            // Act
            
            // Assert
            Assert.True(true);
        }
        
        // TODO: Add more test methods
        // [Theory]
        // [InlineData(...)]
        // public void UserRepository_Should_Handle_ValidInput()
        // {
        //     // Arrange
        //     
        //     // Act
        //     
        //     // Assert
        // }
    }
}
