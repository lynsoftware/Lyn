using PasswordGenerator.Core.Configuration;
using PasswordGenerator.Core.Models;
using PasswordGenerator.Core.Services;

namespace Lyn.Tests.PasswordGenerator.Services;

public class PasswordServiceTests
{
    private readonly PasswordService _sut = new();

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
    public async Task GeneratePassword_GivenInput_AlwaysReturnsSamePassword(
        string masterPassword, string seed, int length, bool includeSpecialChars, string expectedPassword)
    {
        var request = new PasswordGenerationRequest
        {
            MasterPassword = masterPassword,
            Seed = seed,
            Length = length,
            IncludeSpecialChars = includeSpecialChars
        };

        var result = await _sut.GeneratePasswordAsync(request);

        Assert.Equal(expectedPassword, result.Value);
    }

    [Fact]
    public async Task GeneratePassword_TooShortLength_ThrowsArgumentException()
    {
        var request = new PasswordGenerationRequest
        {
            MasterPassword = "test", Seed = "test", Length = 3, IncludeSpecialChars = true
        };

        await Assert.ThrowsAsync<ArgumentException>(() => _sut.GeneratePasswordAsync(request));
    }

    [Fact]
    public async Task GeneratePassword_TooHighLength_ThrowsArgumentException()
    {
        var request = new PasswordGenerationRequest
        {
            MasterPassword = "test", Seed = "test", Length = 580, IncludeSpecialChars = true
        };

        await Assert.ThrowsAsync<ArgumentException>(() => _sut.GeneratePasswordAsync(request));
    }

    [Fact]
    public async Task GeneratePassword_EmptyMasterPassword_ThrowsArgumentException()
    {
        var request = new PasswordGenerationRequest
        {
            MasterPassword = "", Seed = "test", Length = 16, IncludeSpecialChars = true
        };

        await Assert.ThrowsAsync<ArgumentException>(() => _sut.GeneratePasswordAsync(request));
    }

    [Fact]
    public async Task GeneratePassword_EmptySeed_ThrowsArgumentException()
    {
        var request = new PasswordGenerationRequest
        {
            MasterPassword = "test", Seed = "", Length = 16, IncludeSpecialChars = true
        };

        await Assert.ThrowsAsync<ArgumentException>(() => _sut.GeneratePasswordAsync(request));
    }

    [Fact]
    public async Task GeneratePassword_DifferentMasterPassword_ReturnsDifferentPassword()
    {
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

        Assert.NotEqual(result1.Value, result2.Value);
    }

    [Fact]
    public async Task GeneratePassword_DifferentSeed_ReturnsDifferentPassword()
    {
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

        Assert.NotEqual(result1.Value, result2.Value);
    }

    [Theory]
    [InlineData(8)]
    [InlineData(16)]
    [InlineData(64)]
    [InlineData(256)]
    public async Task GeneratePassword_GivenLength_ReturnsCorrectLength(int length)
    {
        var request = new PasswordGenerationRequest
        {
            MasterPassword = "password", Seed = "seed", Length = length, IncludeSpecialChars = true
        };

        var result = await _sut.GeneratePasswordAsync(request);

        Assert.Equal(length, result.Value.Length);
    }

    [Fact]
    public async Task GeneratePassword_WithoutSpecialChars_ContainsOnlyAlphanumeric()
    {
        var request = new PasswordGenerationRequest
        {
            MasterPassword = "password", Seed = "seed", Length = 256, IncludeSpecialChars = false
        };

        var result = await _sut.GeneratePasswordAsync(request);

        Assert.Matches("^[a-zA-Z0-9]+$", result.Value);
    }

    [Fact]
    public async Task GeneratePassword_WithSpecialChars_ContainsAllSpecialCharTypes()
    {
        var request = new PasswordGenerationRequest
        {
            MasterPassword = "Gif!zE*Lkf@I&!DH", Seed = "GeirOlavDenStoreHellige", Length = 250,
            IncludeSpecialChars = true
        };

        var result = await _sut.GeneratePasswordAsync(request);

        Assert.All(
            AppConstants.SpecialChars.ToCharArray(),
            specialChar => Assert.Contains(specialChar.ToString(), result.Value)
        );
    }

    [Fact]
    public async Task GeneratePassword_SameInputMultipleTimes_AlwaysReturnsSameResult()
    {
        var request = new PasswordGenerationRequest
        {
            MasterPassword = "same", Seed = "seed1", Length = 16, IncludeSpecialChars = true
        };

        var results = new List<string>();
        for (int i = 0; i < 5; i++)
        {
            var result = await _sut.GeneratePasswordAsync(request);
            results.Add(result.Value);
        }

        Assert.All(results, password => Assert.Equal(results[0], password));
    }
}