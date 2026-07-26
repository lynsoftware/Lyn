using Calorie.Core.Common;
using Calorie.Core.Common.Services;
using Calorie.Infrastructure.Services;
using CommunityToolkit.Maui.Alerts;

namespace Calorie.Common.Services;

/// <summary>
/// Toasts for ViewModels: suksess = grønn temasnackbar, notis = nøytral
/// native toast.
/// </summary>
public class ToastService : IToastService
{
    public Task ShowSuccessAsync(string message) => AppToast.SuccessAsync(message);

    public Task ShowNoticeAsync(string message) => Toast.Make(message).Show();

    public Task ShowErrorAsync(string message) => AppToast.ErrorAsync(message);
}