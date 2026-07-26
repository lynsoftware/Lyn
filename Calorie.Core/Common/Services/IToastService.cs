namespace Calorie.Core.Common.Services;

/// <summary>
/// Toasts sett fra ViewModels. Appen mapper suksess til grønn snackbar
/// og notis til nøytral toast; testene mocker den.
/// </summary>
public interface IToastService
{
    Task ShowSuccessAsync(string message);

    Task ShowNoticeAsync(string message);

    Task ShowErrorAsync(string message);
}