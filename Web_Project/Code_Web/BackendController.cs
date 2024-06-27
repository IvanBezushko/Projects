using Microsoft.AspNetCore.Mvc;
using Natsukasiy_Web.Services;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace Natsukasiy_Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BackendController : ControllerBase
    {
        private readonly Backend _backend;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<BackendController> _logger;

        public BackendController(Backend backend, IHttpContextAccessor httpContextAccessor, ILogger<BackendController> logger)
        {
            _backend = backend;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        [HttpGet("access-token")]
        public IActionResult GetAccessToken()
        {
            var accessToken = HttpContext.Session.GetString("SpotifyAccessToken");
            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogWarning("No access token found in session.");
                return Unauthorized(new { error = "No access token found in session." });
            }

            return Ok(new { accessToken });
        }

        [HttpGet("playlists")]
        public async Task<IActionResult> GetPlaylists()
        {
            var accessToken = HttpContext.Session.GetString("SpotifyAccessToken");

            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogError("No access token found in session.");
                return Unauthorized(new { error = "No access token found in session." });
            }

            _logger.LogInformation("Access token found: " + accessToken);

            try
            {
                var playlists = await _backend.GetPopularPlaylistsAsync(accessToken);
                return Ok(playlists);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError("Error fetching playlists: " + ex.Message);
                return StatusCode(500, new { error = "Failed to fetch playlists" });
            }
        }

        [HttpGet("tracks")]
        public async Task<IActionResult> GetTracks()
        {
            var accessToken = HttpContext.Session.GetString("SpotifyAccessToken");

            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogError("No access token found in session.");
                return Unauthorized(new { error = "No access token found in session." });
            }

            _logger.LogInformation("Access token found: " + accessToken);

            try
            {
                var tracks = await _backend.GetPopularTracksAsync(accessToken);
                if (tracks == null)
                {
                    return StatusCode(500, new { error = "Failed to fetch tracks" });
                }
                return Ok(tracks);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError("Error fetching tracks: " + ex.Message);
                return StatusCode(500, new { error = "Failed to fetch tracks" });
            }
        }

        [HttpGet("shows")]
        public async Task<IActionResult> GetShows()
        {
            var accessToken = HttpContext.Session.GetString("SpotifyAccessToken");

            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogError("No access token found in session.");
                return Unauthorized(new { error = "No access token found in session." });
            }

            _logger.LogInformation("Access token found: " + accessToken);

            try
            {
                var shows = await _backend.GetRandomShowsAsync(accessToken);
                if (shows == null)
                {
                    _logger.LogError("Failed to fetch shows.");
                    return StatusCode(500, new { error = "Failed to fetch shows" });
                }
                return Ok(shows);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError("Error fetching shows: " + ex.Message);
                return StatusCode(500, new { error = "Failed to fetch shows" });
            }
        }

        [HttpGet("shows/{showId}")]
        public async Task<IActionResult> GetShowDetails(string showId)
        {
            var accessToken = HttpContext.Session.GetString("SpotifyAccessToken");

            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogError("No access token found in session.");
                return Unauthorized(new { error = "No access token found in session." });
            }

            _logger.LogInformation("Access token found: " + accessToken);

            try
            {
                var showDetails = await _backend.GetShowDetailsAsync(accessToken, showId);
                if (showDetails == null)
                {
                    _logger.LogError("Failed to fetch show details.");
                    return StatusCode(500, new { error = "Failed to fetch show details" });
                }
                return Ok(showDetails);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError("Error fetching show details: " + ex.Message);
                return StatusCode(500, new { error = "Failed to fetch show details" });
            }
        }



        [HttpGet("random-playlists")]
        public async Task<IActionResult> GetRandomPlaylists()
        {
            var accessToken = HttpContext.Session.GetString("SpotifyAccessToken");

            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogError("No access token found in session.");
                return Unauthorized(new { error = "No access token found in session." });
            }

            _logger.LogInformation("Access token found: " + accessToken);

            try
            {
                var playlists = await _backend.GetRandomPlaylistsByGenresAsync(accessToken);
                if (playlists == null)
                {
                    _logger.LogError("Failed to fetch playlists.");
                    return StatusCode(500, new { error = "Failed to fetch playlists" });
                }
                return Ok(playlists);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError("Error fetching playlists: " + ex.Message);
                return StatusCode(500, new { error = "Failed to fetch playlists" });
            }
        }







        [HttpPost("save-token")]
        public IActionResult SaveAccessToken([FromBody] TokenRequest request)
        {
            if (string.IsNullOrEmpty(request.AccessToken))
            {
                return BadRequest(new { error = "Access token is required." });
            }

            HttpContext.Session.SetString("SpotifyAccessToken", request.AccessToken);
            _logger.LogInformation("Access token saved in session.");

            return Ok();
        }

        public class TokenRequest
        {
            public string AccessToken { get; set; }
        }


        [HttpPost]
        public IActionResult SetSpotifyToken([FromBody] TokenInfo tokenInfo)
        {
            HttpContext.Session.SetString("SpotifyAccessToken", tokenInfo.AccessToken);
            HttpContext.Session.SetString("SpotifyRefreshToken", tokenInfo.RefreshToken);
            HttpContext.Session.SetString("SpotifyTokenExpiresAt", tokenInfo.ExpiresAt.ToString());
            return Ok();
        }

        public class TokenInfo
        {
            public string AccessToken { get; set; }
            public string RefreshToken { get; set; }
            public long ExpiresAt { get; set; }
        }


        public class RegistrationInput
        {
            public string UserNick { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
        }


        [ApiController]
        [Route("api/[controller]")]
        public class RegistrationController : ControllerBase
        {
            [HttpPost]
            public IActionResult Post([FromBody] RegistrationInput input)
            {
                if (input == null)
                {
                    return BadRequest("Invalid input");
                }

                
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "registrations.json");

               
                List<RegistrationInput> registrations;
                if (System.IO.File.Exists(filePath))
                {
                    var jsonData = System.IO.File.ReadAllText(filePath);
                    registrations = JsonSerializer.Deserialize<List<RegistrationInput>>(jsonData);
                }
                else
                {
                    registrations = new List<RegistrationInput>();
                }

              
                registrations.Add(input);

               
                var updatedJsonData = JsonSerializer.Serialize(registrations);
                System.IO.File.WriteAllText(filePath, updatedJsonData);

                return Ok(new { message = "Registration successful" });
            }
        }


        [HttpGet("genres")]
        public async Task<IActionResult> GetPopularGenres()
        {
            var accessToken = HttpContext.Session.GetString("SpotifyAccessToken");

            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogError("No access token found in session.");
                return Unauthorized(new { error = "No access token found in session." });
            }

            _logger.LogInformation("Access token found: " + accessToken);

            try
            {
                var genres = await _backend.GetPopularGenresAsync(accessToken);
                if (genres == null)
                {
                    _logger.LogError("Failed to fetch genres.");
                    return StatusCode(500, new { error = "Failed to fetch genres" });
                }
                return Ok(genres);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError("Error fetching genres: " + ex.Message);
                return StatusCode(500, new { error = "Failed to fetch genres" });
            }
        }

        [HttpGet("genres/{genreId}/details")]
        public async Task<IActionResult> GetGenreDetails(string genreId)
        {
            var accessToken = HttpContext.Session.GetString("SpotifyAccessToken");

            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogError("No access token found in session.");
                return Unauthorized(new { error = "No access token found in session." });
            }

            _logger.LogInformation("Access token found: " + accessToken);

            try
            {
                var genreDetails = await _backend.GetGenreDetailsAsync(accessToken, genreId);
                if (genreDetails == null)
                {
                    _logger.LogError("Failed to fetch genre details.");
                    return StatusCode(500, new { error = "Failed to fetch genre details" });
                }
                return Ok(genreDetails);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error fetching genre details: " + ex.Message);
                return StatusCode(500, new { error = "Failed to fetch genre details" });
            }
        }







        [HttpGet("playlists/{playlistId}/tracks")]
        public async Task<IActionResult> GetPlaylistTracks(string playlistId)
        {
            var accessToken = HttpContext.Session.GetString("SpotifyAccessToken");

            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogError("No access token found in session.");
                return Unauthorized(new { error = "No access token found in session." });
            }

            _logger.LogInformation("Access token found: " + accessToken);

            try
            {
                var tracks = await _backend.GetPlaylistTracksAsync(accessToken, playlistId);
                if (tracks == null)
                {
                    return StatusCode(500, new { error = "Failed to fetch playlist tracks" });
                }
                return Ok(tracks);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError("Error fetching playlist tracks: " + ex.Message);
                return StatusCode(500, new { error = "Failed to fetch playlist tracks" });
            }
        }


        [HttpGet("search")]
        public async Task<IActionResult> SearchTracks([FromQuery] string query)
        {
            var accessToken = HttpContext.Session.GetString("SpotifyAccessToken");

            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogError("No access token found in session.");
                return Unauthorized(new { error = "No access token found in session." });
            }

            _logger.LogInformation("Access token found: " + accessToken);

            try
            {
                var tracks = await _backend.SearchTracksAsync(accessToken, query);
                if (tracks == null)
                {
                    return StatusCode(500, new { error = "Failed to fetch tracks" });
                }
                return Ok(tracks);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError("Error fetching tracks: " + ex.Message);
                return StatusCode(500, new { error = "Failed to fetch tracks" });
            }
        }



        

        [HttpPost("playback/stop")]
        public async Task<IActionResult> StopPlayback()
        {
            var accessToken = HttpContext.Session.GetString("SpotifyAccessToken");

            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogError("No access token found in session.");
                return Unauthorized(new { error = "No access token found in session." });
            }

            var success = await _backend.StopPlaybackAsync(accessToken);
            if (!success)
            {
                return StatusCode(500, new { error = "Failed to stop playback" });
            }
            return Ok();
        }

        [HttpPost("volume")]
        public async Task<IActionResult> SetVolume([FromBody] VolumeRequest request)
        {
            var accessToken = HttpContext.Session.GetString("SpotifyAccessToken");

            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogError("No access token found in session.");
                return Unauthorized(new { error = "No access token found in session." });
            }

            var success = await _backend.SetVolumeAsync(accessToken, request.Volume);
            if (!success)
            {
                return StatusCode(500, new { error = "Failed to set volume" });
            }
            return Ok();
        }

        [HttpGet("devices")]
        public async Task<IActionResult> GetAvailableDevices()
        {
            var accessToken = HttpContext.Session.GetString("SpotifyAccessToken");

            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogError("No access token found in session.");
                return Unauthorized(new { error = "No access token found in session." });
            }

            var devices = await _backend.GetAvailableDevicesAsync(accessToken);
            if (devices == null)
            {
                return StatusCode(500, new { error = "Failed to fetch devices" });
            }
            return Ok(devices);
        }

        [HttpPost("playback/toggle")]
        public async Task<IActionResult> TogglePlayPause()
        {
            var accessToken = HttpContext.Session.GetString("SpotifyAccessToken");

            if (string.IsNullOrEmpty(accessToken))
            {
                return Unauthorized(new { error = "No access token found in session." });
            }

            var success = await _backend.TogglePlayPauseAsync(accessToken);
            if (!success)
            {
                return StatusCode(500, new { error = "Failed to toggle playback" });
            }

            return Ok();
        }

        [HttpPost("playback/previous")]
        public async Task<IActionResult> SkipToPrevious()
        {
            var accessToken = HttpContext.Session.GetString("SpotifyAccessToken");

            if (string.IsNullOrEmpty(accessToken))
            {
                return Unauthorized(new { error = "No access token found in session." });
            }

            var success = await _backend.SkipToPreviousAsync(accessToken);
            if (!success)
            {
                return StatusCode(500, new { error = "Failed to skip to previous" });
            }

            return Ok();
        }

        [HttpPost("playback/next")]
        public async Task<IActionResult> SkipToNext()
        {
            var accessToken = HttpContext.Session.GetString("SpotifyAccessToken");

            if (string.IsNullOrEmpty(accessToken))
            {
                return Unauthorized(new { error = "No access token found in session." });
            }

            var success = await _backend.SkipToNextAsync(accessToken);
            if (!success)
            {
                return StatusCode(500, new { error = "Failed to skip to next" });
            }

            return Ok();
        }




        [HttpPost("playback/track")]
        public async Task<IActionResult> PlayTrack([FromBody] PlayTrackRequest request)
        {
            _logger.LogInformation("Otrzymano żądanie odtworzenia utworu z URI: {Uri} i Context URI: {ContextUri} na urządzeniu ID: {DeviceId}", request.Uri, request.ContextUri, request.DeviceId);

            if (string.IsNullOrEmpty(request.Uri) || string.IsNullOrEmpty(request.ContextUri) || string.IsNullOrEmpty(request.AccessToken))
            {
                _logger.LogWarning("Nieprawidłowe żądanie: Brak Uri, ContextUri lub AccessToken");
                return BadRequest("Uri, ContextUri i AccessToken są wymagane.");
            }

            var success = await _backend.PlayTrackAsync(request.Uri, request.ContextUri, request.DeviceId, request.AccessToken);
            if (success)
            {
                _logger.LogInformation("Pomyślnie rozpoczęto odtwarzanie dla URI: {Uri}", request.Uri);
                return Ok();
            }

            _logger.LogError("Nie udało się odtworzyć utworu z URI: {Uri}", request.Uri);
            return BadRequest("Nie udało się odtworzyć utworu");
        }

        public class PlayTrackRequest
        {
            public string Uri { get; set; }
            public string ContextUri { get; set; }
            public string DeviceId { get; set; }
            public string AccessToken { get; set; } 
        }



        public class VolumeRequest
    {
        public int Volume { get; set; }
    }


    public async Task<string> ExchangeCodeForAccessTokenAsync(string code)
        {
            var tokenEndpoint = "https://accounts.spotify.com/api/token";
            var redirectUri = "https://ivanbezushko.github.io/Uri/index_web.html"; 
            var clientId = "7eae723d9e134d2b9a4516d0ca88a7d9";
            var clientSecret = "2566165e0ddb48e4a6cbb2f1c4ecf2c8";

            var requestBody = new Dictionary<string, string>
    {
        { "grant_type", "authorization_code" },
        { "code", code },
        { "redirect_uri", redirectUri },
        { "client_id", clientId },
        { "client_secret", clientSecret },
    };

            var requestContent = new FormUrlEncodedContent(requestBody);

            using (var client = new HttpClient())
            {
                var response = await client.PostAsync(tokenEndpoint, requestContent);
                var responseContent = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var tokenData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(responseContent);
                    return tokenData["access_token"].GetString(); 
                }
                else
                {
                    _logger.LogError($"Failed to exchange token: HTTP {response.StatusCode} - {responseContent}");
                    return null;
                }
            }
        }

    }




    [ApiController]
    [Route("api/[controller]")]
    public class RegistrationController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> SaveRegistration([FromBody] RegisterInput input)
        {
            if (input == null || !ModelState.IsValid)
            {
                return BadRequest("Invalid input");
            }

           
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "registrations.json");

           
            List<RegisterInput> registrations;
            if (System.IO.File.Exists(filePath))
            {
                var jsonData = await System.IO.File.ReadAllTextAsync(filePath);
                registrations = JsonSerializer.Deserialize<List<RegisterInput>>(jsonData) ?? new List<RegisterInput>();
            }
            else
            {
                registrations = new List<RegisterInput>();
            }

           
            registrations.Add(input);

           
            var updatedJsonData = JsonSerializer.Serialize(registrations);
            await System.IO.File.WriteAllTextAsync(filePath, updatedJsonData);

            return Ok(new { message = "Registration successful" });
        }
    }

    public class RegisterInput
    {
        [Required]
        public string UserNick { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Hasła nie są zgodne.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }



}
