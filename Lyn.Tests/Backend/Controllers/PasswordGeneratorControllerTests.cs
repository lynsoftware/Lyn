using Lyn.Backend.Controllers;
using Lyn.Backend.Services.Interface;
using Lyn.Shared.Models.Request;
using Lyn.Shared.Models.Response;
using Lyn.Shared.Result;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Lyn.Tests.Backend.Controllers;

public class PasswordGeneratorControllerTests
{   
    
    private readonly PasswordGeneratorController _sut;
    private readonly Mock<IPasswordGeneratorService> _mockService;
    
    public PasswordGeneratorControllerTests()
    {
        // Mocket logger, og mocket scopefactory
        _mockService = new Mock<IPasswordGeneratorService>();
        
        // Controlleren vi tester
        _sut = new PasswordGeneratorController(_mockService.Object);
    }

    /// <summary>
    /// Sjekker at kontrolleren returner riktig Ok 200 med et PasswordGenerationResponse
    /// </summary>
    [Fact]
    public async Task PasswordGeneratorController_ValidRequest_ReturnsOkWithPassword()
    {
        // Arrange
        var request = new PasswordGenerationRequest
        {
            MasterPassword = "test",
            Seed = "test",
            Length = 16,
            IncludeSpecialChars = true
        };

        var expectedResponse = new PasswordGenerationResponse { Value = "generatedPassword" };
        
        // Oppretter et svar fra servicen
        _mockService
            .Setup(s => s.GeneratePasswordAsync(It.IsAny<PasswordGenerationRequest>()))
            .ReturnsAsync(Result<PasswordGenerationResponse>.Success(expectedResponse));
        
        // Act
        var result = await _sut.GeneratePassword(request);
        
        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<PasswordGenerationResponse>(okResult.Value);
        Assert.Equal("generatedPassword", response.Value);
    }
    
    
}