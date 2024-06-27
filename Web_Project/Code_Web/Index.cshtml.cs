using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Threading.Tasks;
using Natsukasiy_Web.Services;
using Microsoft.AspNetCore.Http;

public class IndexModel : PageModel
{
    private readonly Backend _backend;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public IndexModel(Backend backend, IHttpContextAccessor httpContextAccessor)
    {
        _backend = backend;
        _httpContextAccessor = httpContextAccessor;
        PopularPlaylists = new List<Backend.Playlist>();
    }

    public List<Backend.Playlist> PopularPlaylists { get; set; }

    public async Task OnGetAsync()
    {
        var accessToken = _httpContextAccessor.HttpContext?.User.FindFirst("access_token")?.Value;
        if (accessToken != null)
        {
            var playlists = await _backend.GetPopularPlaylistsAsync(accessToken);
            PopularPlaylists = playlists ?? new List<Backend.Playlist>();
        }
    }
}
