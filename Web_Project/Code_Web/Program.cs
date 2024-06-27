using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Natsukasiy_Web.Services; 
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Natsukasiy_Web.Controllers;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddRazorPages();
builder.Services.AddControllers();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddHttpClient<Backend>();
builder.Services.AddSingleton<Backend>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDataProtection();
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = context => true;
    options.MinimumSameSitePolicy = SameSiteMode.None;
});
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = "Spotify";
})
.AddCookie(options =>
{
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
})
.AddOAuth("Spotify", options =>
{
    options.ClientId = "7eae723d9e134d2b9a4516d0ca88a7d9";
    options.ClientSecret = "2566165e0ddb48e4a6cbb2f1c4ecf2c8";
    options.CallbackPath = new PathString("/callback");
    options.AuthorizationEndpoint = "https://accounts.spotify.com/authorize";
    options.TokenEndpoint = "https://accounts.spotify.com/api/token";
    options.Scope.Add("user-read-private");
    options.Scope.Add("user-read-email");
    options.Scope.Add("user-modify-playback-state");
    options.Scope.Add("user-read-playback-state");
    options.SaveTokens = true;
    options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
    options.ClaimActions.MapJsonKey(ClaimTypes.Name, "display_name");
    options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
    options.Events = new OAuthEvents
    {
        OnCreatingTicket = async context =>
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.spotify.com/v1/me");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);
            var response = await context.Backchannel.SendAsync(request, context.HttpContext.RequestAborted);
            response.EnsureSuccessStatusCode();
            var user = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
            context.RunClaimActions(user);
            var accessToken = context.AccessToken;
            if (!string.IsNullOrEmpty(accessToken))
            {
                context.Identity?.AddClaim(new Claim("access_token", accessToken));
                context.Properties.StoreTokens(new[]
                {
                    new AuthenticationToken { Name = "access_token", Value = context.AccessToken },
                    new AuthenticationToken { Name = "refresh_token", Value = context.RefreshToken },
                    new AuthenticationToken { Name = "expires_at", Value = (DateTime.UtcNow + (context.ExpiresIn ?? TimeSpan.FromHours(1))).ToString("o") }
                });
                context.HttpContext.Session.SetString("SpotifyAccessToken", accessToken);
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogInformation($"Access token stored in session: {accessToken}");
            }
        },
        OnTicketReceived = context =>
        {
            return Task.CompletedTask;
        },
        OnRemoteFailure = context =>
        {
            context.HandleResponse();
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError($"Remote failure during authentication. Error: {context.Failure.Message}");
            context.Response.Redirect("/");
            return Task.CompletedTask;
        }
    };
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCookiePolicy();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapRazorPages();
app.MapControllers();


app.UseEndpoints(endpoints =>
{
    endpoints.MapPost("/save-registration", async context =>
    {
        using var reader = new StreamReader(context.Request.Body);
        var body = await reader.ReadToEndAsync();
        var registrationData = JsonSerializer.Deserialize<Natsukasiy_Web.Pages.Shared.RegisterInput>(body);

        if (registrationData != null)
        {
            var json = JsonSerializer.Serialize(registrationData, new JsonSerializerOptions { WriteIndented = true });
            var existingData = new List<Natsukasiy_Web.Pages.Shared.RegisterInput>();

            if (System.IO.File.Exists("registrationData.json"))
            {
                var existingJson = await System.IO.File.ReadAllTextAsync("registrationData.json");
                existingData = JsonSerializer.Deserialize<List<Natsukasiy_Web.Pages.Shared.RegisterInput>>(existingJson);
            }

            existingData.Add(registrationData);
            var updatedJson = JsonSerializer.Serialize(existingData, new JsonSerializerOptions { WriteIndented = true });
            await System.IO.File.WriteAllTextAsync("registrationData.json", updatedJson);

            context.Response.StatusCode = 200;
            await context.Response.WriteAsync("Registration successful");
        }
        else
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("Invalid registration data");
        }
    });
});

app.MapGet("/signin-spotify", async context =>
{
    var clientId = "7eae723d9e134d2b9a4516d0ca88a7d9";
    var redirectUri = "https://ivanbezushko.github.io/Uri/index_web.html";
    var scope = "user-read-private user-read-email user-modify-playback-state user-read-playback-state";
    var state = Guid.NewGuid().ToString("N");    
    var returnUrl = context.Request.Query["returnUrl"].ToString();     

    if (string.IsNullOrEmpty(returnUrl))
    {
        returnUrl = "/Privacy";        
    }

    var authorizeUrl = $"https://accounts.spotify.com/authorize?client_id={clientId}&response_type=code&redirect_uri={Uri.EscapeDataString(redirectUri)}&scope={Uri.EscapeDataString(scope)}&state={state}&show_dialog=true";
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

    context.Session.SetString("OAuthState", state);      
    context.Session.SetString("ReturnUrl", returnUrl);     
    logger.LogInformation($"Redirecting to {authorizeUrl}");
    context.Response.Redirect(authorizeUrl);
    await Task.CompletedTask;
});

app.MapGet("/callback", async context =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    var state = context.Request.Query["state"].ToString();
    var sessionState = context.Session.GetString("OAuthState");

    if (state != sessionState)
    {
        logger.LogError("Invalid state parameter.");
        context.Response.Redirect("/");
        return;
    }

    var code = context.Request.Query["code"].ToString();
    if (string.IsNullOrEmpty(code))
    {
        logger.LogError("No code found in callback URL.");
        context.Response.Redirect("/");
        return;
    }

    logger.LogInformation($"Code received in URL: {code}");

    var backendController = context.RequestServices.GetRequiredService<BackendController>();
    var accessToken = await backendController.ExchangeCodeForAccessTokenAsync(code);

    if (string.IsNullOrEmpty(accessToken))
    {
        logger.LogError("Failed to retrieve access token.");
        context.Response.Redirect("/");
        return;
    }

    logger.LogInformation($"Access token received: {accessToken}");

    
    context.Session.SetString("SpotifyAccessToken", accessToken);
    logger.LogInformation("Access token stored in session.");

   
    var returnUrl = context.Session.GetString("ReturnUrl");
    context.Response.Redirect(returnUrl);
});

app.Run();
