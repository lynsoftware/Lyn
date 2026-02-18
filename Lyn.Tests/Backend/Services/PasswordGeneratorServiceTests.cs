using Lyn.Backend.Services;
using Lyn.Shared.Configuration;
using Lyn.Shared.Models.Request;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace Lyn.Tests.Backend.Services;

public class PasswordGeneratorServiceTests
{
    // Servicen vi tester
    private readonly PasswordGeneratorService _sut;

    public PasswordGeneratorServiceTests()
    {
        // Mocket logger, og mocket scopefactory
        var mockLogger = new Mock<ILogger<PasswordGeneratorService >>();
        var mockScopeFactory = new Mock<IServiceScopeFactory>();
        
        // Servicen vi tester
        _sut = new PasswordGeneratorService(mockLogger.Object, mockScopeFactory.Object);
    }
    
    /// <summary>
    /// Vi tester at vi generer samme passordet med korrekt masterPassword, seed, tegn og bool mot includeSpecialChars
    /// Dette krever at DegreeOfParallelism = 4, MemorySize = 131072, Iterations = 4
    /// </summary>
    /// <param name="masterPassword">Master Password</param>
    /// <param name="seed">Seed</param>
    /// <param name="length">Lengde på passordet</param>
    /// <param name="includeSpecialChars">Bool om special chars skal være med</param>
    /// <param name="expectedPassword">Forventet passord</param>
    [Theory]
    [InlineData("321", "321", 16, true, "Cat1txGIY7_jTeoe")]
    [InlineData("321", "321", 16, false, "YltnETdfYtiQfAoe")]
    [InlineData("321", "321", 256, true, "@rPtiMkUbY+o+6$dKAciGD8wFkE8YPyuEvRV+HtqLJlW&@ecC1tEO8jR#oElga0cs5d52h9acde+$JXYiKfJ6pn896bMi1BAPptEtLEAEKy8md1TCW1yKyvBDceDGO*k@%nmbMzsnwHyD4&8epsrlACfFmarCeKt5sp6AwVI=bgN%aSTKq=OtEhU=ew-*71*ztcKh&*JHC0rIkFihcD=g90h!RWEEQznrAt3SDGYpsVcDmh-_cDzQ02e+AejFFLI")]
    [InlineData("master", "github.com", 20, true, "zSodc@ceYzCRW_K!5a#f")]
    [InlineData("master", "github.com", 20, false, "KezzJbcekVCdiEhwrlyf")]
    [InlineData("test", "test", 8, true, "%E1zokGl")]
    [InlineData("longpassword123", "netflix.com", 32, true, "-ywFI7MWk$lWckYaY-cS%lAcxT4+76Ic")]
    [InlineData("passord123!@PO", "pornshop", 200, true, "xERAgifJ-y43rodCMBVYpfsidgycD2VNixB#3rSNmpBwu5yhCbr5MP0uyI4drqKjtywR$gYKtV&VpJkDx91Xpn=IK@FdFHKmidyRjQwM8YB+1XSkzY0FhKKz3AkPzAAcy8F5JzWe6A56H0Lzs05_GE4EM&$*$%vKryM1MoC7c2h%mILql41HgAvZH0S&9&9vebDot4kD")]
    [InlineData("321", "DjMikkel", 16, true, "fTm&oqZVOUyLmN+t")]
    [InlineData("321", "djMikkel", 16, true, "mk&VQtI4EIjFW4vG")]
    [InlineData("storeEkleFøtter", "321", 16, true, "6%#gQ3GSrn8YPGxJ")]
    [InlineData("storeekleFøtter", "321", 16, true, "Gif!zE*Lkf@I&!DH")]
    [InlineData("Gif!zE*Lkf@I&!DH", "GeirOlavDenStoreHellige", 256, true, "7+ieyUK&6PC7WeI&6fHaM_ZBEwxQk00$bIqavH!TupMIUbKynjV9m=&%baLl!EonfK%aizeI!$pUUQrT&uTAuE@rKuSnApeDmy1rp*CDSrt_EsiEqxc-%P2U5ngIGAnxfDefC6vzKZ4aK9ekH7iZ=AjFK3LtqwLeshnt44VBcx4y8sSvx=efsrnmoM@p**jDnfEpVfkz0qnkz6h=Bwc2Sq5*nCQRlL%gmW55_f!DkEx7sgwJ0DcwMgXibdZC*$!c")]
    public async Task GeneratePasswordService_GivenInput_AlwaysReturnsSamePassword(string masterPassword, string seed,
        int length, bool includeSpecialChars, string expectedPassword)
    {
        // Arrange
        var request = new PasswordGenerationRequest
        {
            MasterPassword = masterPassword,
            Seed = seed,
            Length = length,
            IncludeSpecialChars = includeSpecialChars
        };
        
        // Act
        var result = await _sut.GeneratePasswordAsync(request);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedPassword, result.Value!.Value);
    }

    
    /// <summary>
    /// Tester at length er for lav
    /// </summary>
    [Fact]
    public async Task GeneratePasswordService_TooShortLength_ThrowsArgumentException()
    {
        // Arrange
        var request = new PasswordGenerationRequest
        {
            MasterPassword = "test",
            Seed = "test",
            Length = 3,
            IncludeSpecialChars = true
        };
        
        // Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _sut.GeneratePasswordAsync(request));
    }
    
    /// <summary>
    /// Tester at length er for høy
    /// </summary>
    [Fact]
    public async Task GeneratePasswordService_TooHighLength_ThrowsArgumentException()
    {
        // Arrange
        var request = new PasswordGenerationRequest
        {
            MasterPassword = "test",
            Seed = "test",
            Length = 580,
            IncludeSpecialChars = true
        };
        
        // Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _sut.GeneratePasswordAsync(request));
    }
    
    /// <summary>
    /// Tester at forskjellige master password gir forskjellige resultatet når de andre egenskapene er like
    /// </summary>
    [Fact]
    public async Task GeneratePasswordService_DifferentMasterPassword_ReturnsDifferentPassword()
    {
        // Arrange
        var request1 = new PasswordGenerationRequest
        {
            MasterPassword = "password1", Seed = "seed", Length = 16, IncludeSpecialChars = true
        };
        var request2 = new PasswordGenerationRequest
        {
            MasterPassword = "password2", Seed = "seed", Length = 16, IncludeSpecialChars = true
        };

        var result1 = await _sut.GeneratePasswordAsync(request1);
        var result2 = await _sut.GeneratePasswordAsync(request2);
        
        // Assert
        Assert.NotEqual(result1.Value!.Value, result2.Value!.Value);
    }
    
    /// <summary>
    /// Tester at forskjellige seed gir forskjellige resultatet når de andre egenskapene er like
    /// </summary>
    [Fact]
    public async Task GeneratePasswordService_DifferentSeed_ReturnsDifferentPassword()
    {
        // Arrange
        var request1 = new PasswordGenerationRequest
        {
            MasterPassword = "same", Seed = "seed1", Length = 16, IncludeSpecialChars = true
        };
        var request2 = new PasswordGenerationRequest
        {
            MasterPassword = "same", Seed = "seed2", Length = 16, IncludeSpecialChars = true
        };

        var result1 = await _sut.GeneratePasswordAsync(request1);
        var result2 = await _sut.GeneratePasswordAsync(request2);
        
        // Assert
        Assert.NotEqual(result1.Value!.Value, result2.Value!.Value);
    }
    
    /// <summary>
    /// Tester at forskjellige length-input gir riktig lengde på passordene
    /// </summary>
    /// <param name="length">Forskjellige length-inputs</param>
    [Theory]
    [InlineData(8)]
    [InlineData(16)]
    [InlineData(64)]
    [InlineData(256)]
    public async Task GeneratePasswordService_GivenLength_ReturnsPasswordWithDifferentLengths(int length)
    {
        // Arrange
        var request = new PasswordGenerationRequest
        {
            MasterPassword = "password", Seed = "seed", Length = length, IncludeSpecialChars = true
        };
        
        // Act
        var result1 = await _sut.GeneratePasswordAsync(request);
        
        // Assert
        Assert.Equal(length, result1.Value!.Value.Length);
    }
    
    /// <summary>
    /// Tester at det er ingen symboler når IncludeSpecialChars = false
    /// </summary>
    [Fact]
    public async Task GeneratePasswordService_WithoutSpecialChars_ReturnsPasswordWithNoSpecialChars()
    {
        // Arrange
        var request = new PasswordGenerationRequest
        {
            MasterPassword = "password", Seed = "seed", Length = 256, IncludeSpecialChars = false
        };
        
        // Act
        var result = await _sut.GeneratePasswordAsync(request);
        
        // Assert
        Assert.Matches("^[a-zA-Z0-9]+$", result.Value!.Value);
    }
    
    /// <summary>
    /// Tester at alle symboler er med når IncludeSpecialChars = true
    /// </summary>
    [Fact]
    public async Task GeneratePasswordService_WithSpecialChars_ReturnsPasswordWithAllSpecialChars()
    {
        // Arrange
        var request = new PasswordGenerationRequest
        {
            MasterPassword = "Gif!zE*Lkf@I&!DH", Seed = "GeirOlavDenStoreHellige", Length = 250, 
            IncludeSpecialChars = true
        };
        
        // Act
        var result = await _sut.GeneratePasswordAsync(request);
        
        // Assert
        Assert.All(
            AppConstants.SpecialChars.ToCharArray(),
            specialChar => Assert.Contains(specialChar.ToString(), result.Value!.Value)
        );
    }
    
    /// <summary>
    /// Tester at generinger av samme passord alltid gir samme resultat
    /// </summary>
    [Fact]
    public async Task GeneratePasswordService_GenerateSamePasswordSeveralTimes_ReturnsSamePassword()
    {
        // Arrange
        var request = new PasswordGenerationRequest
        {
            MasterPassword = "same", Seed = "seed1", Length = 16, IncludeSpecialChars = true
        };

        var results = new List<string>();
        for (int i = 0; i < 5; i++)
        {
            var result = await _sut.GeneratePasswordAsync(request);
            results.Add(result.Value!.Value);
        }
        
        // Assert
        Assert.All(results, password => Assert.Equal(results[0], password));
    }
}
