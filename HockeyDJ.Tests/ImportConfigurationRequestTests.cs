using HockeyDJ.Controllers;

namespace HockeyDJ.Tests;

public class ImportConfigurationRequestTests
{
    [Fact]
    public void ConfigData_DefaultsToEmptyString()
    {
        // Arrange & Act
        var request = new ImportConfigurationRequest();

        // Assert
        Assert.Equal(string.Empty, request.ConfigData);
    }

    [Fact]
    public void ConfigData_CanBeSetAndRetrieved()
    {
        // Arrange
        var expectedData = "{\"clientId\":\"test\"}";
        var request = new ImportConfigurationRequest();

        // Act
        request.ConfigData = expectedData;

        // Assert
        Assert.Equal(expectedData, request.ConfigData);
    }

    [Fact]
    public void ConfigData_CanBeSetViaInitializer()
    {
        // Arrange
        var expectedData = "{\"clientId\":\"test\",\"redirectUri\":\"http://localhost\"}";

        // Act
        var request = new ImportConfigurationRequest { ConfigData = expectedData };

        // Assert
        Assert.Equal(expectedData, request.ConfigData);
    }
}
