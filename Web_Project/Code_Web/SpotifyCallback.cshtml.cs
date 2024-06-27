using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

public class SpotifyCallbackModel : PageModel
{
    public async Task OnGetAsync(string code, string state)
    {
        if (string.IsNullOrEmpty(code))
        {
            // Handle error
            return;
        }

        var tokenResponse = await ExchangeCodeForTokensAsync(code);
        if (tokenResponse != null)
        {
            HttpContext.Session.SetString("access_token", tokenResponse.AccessToken ?? string.Empty);
            // Redirect to the main page or wherever
            Response.Redirect("/");
        }
    }

    private async Task<TokenResponse?> ExchangeCodeForTokensAsync(string code)
    {
        // Your code to exchange the code for tokens
        return await Task.FromResult<TokenResponse?>(null);
    }

    public class TokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;
    }
}
