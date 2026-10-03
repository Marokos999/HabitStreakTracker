using System.Net;
using System.Net.Http.Headers;

namespace HabitTracker.Mobile.Services;

public class AuthHeaderHandler(ICognitoAuthService authService) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken ct)
    {
        var token = await authService.GetAccessTokenAsync();
        if (token is not null)
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await base.SendAsync(request, ct);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
            await authService.HandleUnauthorizedAsync();

        return response;
    }
}
