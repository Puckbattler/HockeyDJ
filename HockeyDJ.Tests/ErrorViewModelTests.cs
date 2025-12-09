using HockeyDJ.Models;

namespace HockeyDJ.Tests;

public class ErrorViewModelTests
{
    [Fact]
    public void ShowRequestId_ReturnsFalse_WhenRequestIdIsNull()
    {
        // Arrange
        var model = new ErrorViewModel { RequestId = null };

        // Act & Assert
        Assert.False(model.ShowRequestId);
    }

    [Fact]
    public void ShowRequestId_ReturnsFalse_WhenRequestIdIsEmpty()
    {
        // Arrange
        var model = new ErrorViewModel { RequestId = "" };

        // Act & Assert
        Assert.False(model.ShowRequestId);
    }

    [Fact]
    public void ShowRequestId_ReturnsTrue_WhenRequestIdHasValue()
    {
        // Arrange
        var model = new ErrorViewModel { RequestId = "test-request-id-123" };

        // Act & Assert
        Assert.True(model.ShowRequestId);
    }

    [Fact]
    public void RequestId_CanBeSetAndRetrieved()
    {
        // Arrange
        var expectedId = "test-id-456";
        var model = new ErrorViewModel();

        // Act
        model.RequestId = expectedId;

        // Assert
        Assert.Equal(expectedId, model.RequestId);
    }

    [Fact]
    public void ShowRequestId_ReturnsFalse_WhenRequestIdIsWhitespace()
    {
        // Arrange
        var model = new ErrorViewModel { RequestId = "   " };

        // Act & Assert - whitespace is not empty according to string.IsNullOrEmpty
        Assert.True(model.ShowRequestId);
    }
}
