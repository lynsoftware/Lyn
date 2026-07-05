namespace Lyn.Backend.Common.ProblemDetails;

/// <summary>
/// Utvidet ProblemDetails med domenespesifikk feilkode.
/// Brukes i stedet for standard ProblemDetails i alle API-feilsvar.
/// </summary>
public class AppProblemDetails : Microsoft.AspNetCore.Mvc.ProblemDetails
{
    /// <summary>
    /// Domenespesifikk feilkode. Speilet i frontend som AppErrorCode-enum.
    /// </summary>
    public int Code { get; set; }
}
