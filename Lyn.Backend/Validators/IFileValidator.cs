using Lyn.Shared.Enum;
using Lyn.Shared.Result;

namespace Lyn.Backend.Validators;

public interface IFileValidator
{
    /// <summary>
    /// Validerer en AppRelease-fil for riktig filstørrelse, extension er korrekt, content type er korrekt og at
    /// Magic Byte stemmer (hvis vi ikke må skippe den pga type fil)
    /// </summary>
    /// <param name="file">Filen som skal valideres</param>
    /// <param name="releaseType">Validerer slik at extensions og content type stemmer med typen fil</param>
    /// <returns>Result med Success eller en feilmelding</returns>
    public Result ValidateReleaseFile(IFormFile file, ReleaseType releaseType);

    /// <summary>
    /// Validerer en Support Ticket File (pdf, doc, txt eller bilde filer). Sjekker filstørrelse, extensions, content
    /// type og Magic Byte
    /// </summary>
    /// <param name="file">Filen som skal valideres</param>
    /// <returns>Result med Success eller en feilmelding</returns>
    Result ValidateSupportAttachment(IFormFile file);
}