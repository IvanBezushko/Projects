using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Natsukasiy_Web.Services
{
    public class Backend
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<Backend> _logger;

        public Backend(HttpClient httpClient, ILogger<Backend> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<Playlist>> GetPopularPlaylistsAsync(string accessToken)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var response = await _httpClient.GetAsync("https://api.spotify.com/v1/browse/featured-playlists");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var json = JsonDocument.Parse(content).RootElement;

                var playlists = new List<Playlist>();
                foreach (var item in json.GetProperty("playlists").GetProperty("items").EnumerateArray())
                {
                    playlists.Add(new Playlist
                    {
                        Id = item.GetProperty("id").GetString(),
                        Name = item.GetProperty("name").GetString(),
                        ImageUrl = item.GetProperty("images")[0].GetProperty("url").GetString()
                    });
                }

                return playlists;
            }

            return null;
        }

        public async Task<List<Track>> GetPopularTracksAsync(string accessToken)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.GetAsync("https://api.spotify.com/v1/recommendations?limit=30&seed_genres=pop,rock");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to fetch recommendations: " + response.ReasonPhrase);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Recommendations response: " + content);

            var json = JsonDocument.Parse(content).RootElement;

            var tracks = new List<Track>();
            foreach (var item in json.GetProperty("tracks").EnumerateArray())
            {
                var album = item.GetProperty("album");
                var contextUri = album.GetProperty("uri").GetString();

                tracks.Add(new Track
                {
                    Name = item.GetProperty("name").GetString(),
                    Artist = item.GetProperty("artists")[0].GetProperty("name").GetString(),
                    ImageUrl = album.GetProperty("images")[0].GetProperty("url").GetString(),
                    Duration = item.GetProperty("duration_ms").GetInt32(),
                    Uri = item.GetProperty("uri").GetString(),
                    ContextUri = contextUri 
                });
            }

            return tracks;
        }


        public async Task<List<Show>> GetRandomShowsAsync(string accessToken)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var showIds = new List<string>
            {
                "5CfCWKI5pZ28U0uOzXkDHe",
                "5as3aKmN2k11yfDDDSrvaZ",
                "4wEuac2C7cpuvy8HBjfvW7",
                "6Yw8IYbenhAwzXKtXfJBB9",
                "2OwAg0ckS0mUDPi6HW8FNt",
                "26qYIYvP3SrHJBB6BQ6wZh",
                "7CgoVR2exzCeBsuaysOXOR",
                "2Eqx1kHjpiAOQ9oSxo8Tcg"
            };

            var shows = new List<Show>();

            foreach (var showId in showIds)
            {
                var showResponse = await _httpClient.GetAsync($"https://api.spotify.com/v1/shows/{showId}");
                if (showResponse.IsSuccessStatusCode)
                {
                    var showContent = await showResponse.Content.ReadAsStringAsync();
                    var showJson = JsonDocument.Parse(showContent).RootElement;

                    shows.Add(new Show
                    {
                        Id = showJson.GetProperty("id").GetString(),
                        Name = showJson.GetProperty("name").GetString(),
                        ImageUrl = showJson.GetProperty("images")[0].GetProperty("url").GetString()
                    });
                }
            }

            return shows;
        }

        public async Task<List<Playlist>> GetRandomPlaylistsByGenresAsync(string accessToken)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var categoriesResponse = await _httpClient.GetAsync("https://api.spotify.com/v1/browse/categories");
            if (!categoriesResponse.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to fetch categories: " + categoriesResponse.ReasonPhrase);
                return null;
            }

            var categoriesContent = await categoriesResponse.Content.ReadAsStringAsync();
            var categoriesJson = JsonDocument.Parse(categoriesContent).RootElement;
            var categories = categoriesJson.GetProperty("categories").GetProperty("items").EnumerateArray().ToList();

            var random = new Random();
            var playlists = new List<Playlist>();

            for (int i = 0; i < 5; i++)
            {
                var randomCategory = categories[random.Next(categories.Count)].GetProperty("id").GetString();
                var playlistsResponse = await _httpClient.GetAsync($"https://api.spotify.com/v1/browse/categories/{randomCategory}/playlists");
                if (playlistsResponse.IsSuccessStatusCode)
                {
                    var playlistsContent = await playlistsResponse.Content.ReadAsStringAsync();
                    var playlistsJson = JsonDocument.Parse(playlistsContent).RootElement;

                    foreach (var item in playlistsJson.GetProperty("playlists").GetProperty("items").EnumerateArray())
                    {
                        playlists.Add(new Playlist
                        {
                            Id = item.GetProperty("id").GetString(),
                            Name = item.GetProperty("name").GetString(),
                            ImageUrl = item.GetProperty("images")[0].GetProperty("url").GetString()
                        });

                        if (playlists.Count >= 30) break;
                    }

                    if (playlists.Count >= 30) break;
                }
            }

            return playlists;
        }

        public async Task<List<Genre>> GetPopularGenresAsync(string accessToken)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var response = await _httpClient.GetAsync("https://api.spotify.com/v1/browse/categories");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Spotify categories response: " + content);

                try
                {
                    var json = JsonDocument.Parse(content).RootElement;

                    var genres = new List<Genre>();
                    foreach (var item in json.GetProperty("categories").GetProperty("items").EnumerateArray())
                    {
                        genres.Add(new Genre
                        {
                            Id = item.GetProperty("id").GetString(),
                            Name = item.GetProperty("name").GetString(),
                            ImageUrl = item.GetProperty("icons")[0].GetProperty("url").GetString()
                        });
                    }

                    return genres;
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error parsing Spotify categories response: " + ex.Message);
                    return null;
                }
            }
            else
            {
                _logger.LogError("Failed to fetch Spotify categories: " + response.ReasonPhrase);
                return null;
            }
        }

        public async Task<GenreDetails> GetGenreDetailsAsync(string accessToken, string genreId)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var response = await _httpClient.GetAsync($"https://api.spotify.com/v1/browse/categories/{genreId}/playlists");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Spotify genre playlists response: " + content);

                try
                {
                    var json = JsonDocument.Parse(content).RootElement;
                    var genreDetails = new GenreDetails
                    {
                        Id = genreId,
                        Name = genreId, 
                        Tracks = new List<Track>()
                    };

                    var playlists = json.GetProperty("playlists").GetProperty("items")
                                        .EnumerateArray()
                                        .Select(item => item.GetProperty("id").GetString())
                                        .ToList();

                    var trackFetchTasks = playlists.Select(async playlistId =>
                    {
                        var tracksResponse = await _httpClient.GetAsync($"https://api.spotify.com/v1/playlists/{playlistId}/tracks");
                        if (tracksResponse.IsSuccessStatusCode)
                        {
                            var tracksContent = await tracksResponse.Content.ReadAsStringAsync();
                            var tracksJson = JsonDocument.Parse(tracksContent).RootElement;
                            var tracks = tracksJson.GetProperty("items").EnumerateArray().Select(item =>
                            {
                                var track = item.GetProperty("track");

                               
                                _logger.LogInformation("Track data: " + track.ToString());

                                var uri = track.TryGetProperty("uri", out var uriElement) ? uriElement.GetString() : null;

                                return new Track
                                {
                                    Id = track.GetProperty("id").GetString(),
                                    Name = track.GetProperty("name").GetString(),
                                    Artist = track.GetProperty("artists")[0].GetProperty("name").GetString(),
                                    ImageUrl = track.GetProperty("album").GetProperty("images")[0].GetProperty("url").GetString(),
                                    Duration = track.GetProperty("duration_ms").GetInt32(),
                                    Uri = uri,
                                    ContextUri = track.GetProperty("album").GetProperty("uri").GetString()
                                };
                            }).ToList();

                            lock (genreDetails.Tracks)
                            {
                                genreDetails.Tracks.AddRange(tracks);
                            }
                        }
                    });

                    await Task.WhenAll(trackFetchTasks);

                    return genreDetails;
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error parsing Spotify genre playlists response: " + ex.Message);
                    return null;
                }
            }
            else
            {
                _logger.LogError($"Failed to fetch Spotify genre playlists: {response.StatusCode} - {response.ReasonPhrase}");
                return null;
            }
        }





        public async Task<List<Track>> SearchTracksAsync(string accessToken, string query)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var response = await _httpClient.GetAsync($"https://api.spotify.com/v1/search?q={Uri.EscapeDataString(query)}&type=track");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to search tracks: " + response.ReasonPhrase);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(content).RootElement;

            var tracks = new List<Track>();
            foreach (var item in json.GetProperty("tracks").GetProperty("items").EnumerateArray())
            {
                var album = item.GetProperty("album");
                var contextUri = album.GetProperty("uri").GetString();

                tracks.Add(new Track
                {
                    Name = item.GetProperty("name").GetString(),
                    Artist = item.GetProperty("artists")[0].GetProperty("name").GetString(),
                    ImageUrl = album.GetProperty("images")[0].GetProperty("url").GetString(),
                    Duration = item.GetProperty("duration_ms").GetInt32(),
                    Uri = item.GetProperty("uri").GetString(),
                    ContextUri = contextUri,
                    AlbumId = album.GetProperty("id").GetString()
                });
            }

            return tracks;
        }




        public async Task<List<Track>> GetPlaylistTracksAsync(string accessToken, string playlistId)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var response = await _httpClient.GetAsync($"https://api.spotify.com/v1/playlists/{playlistId}/tracks");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to fetch playlist tracks: " + response.ReasonPhrase);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(content).RootElement;

            var tracks = new List<Track>();
            foreach (var item in json.GetProperty("items").EnumerateArray())
            {
                var track = item.GetProperty("track");
                tracks.Add(new Track
                {
                    Name = track.GetProperty("name").GetString(),
                    Artist = track.GetProperty("artists")[0].GetProperty("name").GetString(),
                    ImageUrl = track.GetProperty("album").GetProperty("images")[0].GetProperty("url").GetString(),
                    Duration = track.GetProperty("duration_ms").GetInt32(),
                    Uri = track.GetProperty("uri").GetString(),
                    ContextUri = track.GetProperty("album").GetProperty("uri").GetString() 
                });
            }

            return tracks;
        }


        public async Task<ShowDetails> GetShowDetailsAsync(string accessToken, string showId)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var response = await _httpClient.GetAsync($"https://api.spotify.com/v1/shows/{showId}?market=US");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var showJson = JsonDocument.Parse(content).RootElement;

                var showDetails = new ShowDetails
                {
                    Id = showJson.GetProperty("id").GetString(),
                    Name = showJson.GetProperty("name").GetString(),
                    ImageUrl = showJson.GetProperty("images")[0].GetProperty("url").GetString(),
                    Episodes = new List<Episode>(),
                    Uri = showJson.GetProperty("uri").GetString() 
                };

                foreach (var episode in showJson.GetProperty("episodes").GetProperty("items").EnumerateArray())
                {
                    showDetails.Episodes.Add(new Episode
                    {
                        Id = episode.GetProperty("id").GetString(),
                        Name = episode.GetProperty("name").GetString(),
                        Description = episode.GetProperty("description").GetString(),
                        ImageUrl = episode.GetProperty("images")[0].GetProperty("url").GetString(),
                        Uri = episode.GetProperty("uri").GetString(),
                        DurationMs = episode.GetProperty("duration_ms").GetInt32() 
                    });
                }

                return showDetails;
            }

            return null;
        }





        public async Task<bool> PlayTrackAsync(string uri, string contextUri, string deviceId, string accessToken)
        {
            try
            {
                var requestBody = new
                {
                    context_uri = contextUri,
                    offset = new { uri = uri },
                    position_ms = 0
                };

                string json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.PutAsync($"https://api.spotify.com/v1/me/player/play?device_id={deviceId}", content);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Nie udało się rozpocząć odtwarzania: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                    return false;
                }
                _logger.LogInformation("Pomyślnie rozpoczęto odtwarzanie z URI: " + uri);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Wystąpił błąd podczas odtwarzania: {ex.Message}");
                return false;
            }
        }








        public async Task<bool> StopPlaybackAsync(string accessToken)
        {
            try
            {
                var response = await _httpClient.PutAsync("https://api.spotify.com/v1/me/player/pause", null);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Failed to stop playback: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                    return false;
                }
                _logger.LogInformation("Playback stopped successfully.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred during stop playback: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetVolumeAsync(string accessToken, int volume)
        {
            try
            {
                string endpoint = $"https://api.spotify.com/v1/me/player/volume?volume_percent={Math.Round((double)volume)}";
                var response = await _httpClient.PutAsync(endpoint, null);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Failed to set volume. Status code: {response.StatusCode}");
                    return false;
                }

                _logger.LogInformation("Volume set successfully.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception when setting volume: {ex.Message}");
                return false;
            }
        }

        public async Task<List<Device>> GetAvailableDevicesAsync(string accessToken)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var response = await _httpClient.GetAsync("https://api.spotify.com/v1/me/player/devices");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var json = JsonDocument.Parse(content).RootElement;

                var devices = new List<Device>();
                foreach (var item in json.GetProperty("devices").EnumerateArray())
                {
                    devices.Add(new Device
                    {
                        Id = item.GetProperty("id").GetString(),
                        Name = item.GetProperty("name").GetString(),
                        IsActive = item.GetProperty("is_active").GetBoolean(),
                        IsPrivateSession = item.GetProperty("is_private_session").GetBoolean(),
                        IsRestricted = item.GetProperty("is_restricted").GetBoolean(),
                        Type = item.GetProperty("type").GetString(),
                        VolumePercent = item.GetProperty("volume_percent").GetInt32()
                    });
                }

                return devices;
            }

            _logger.LogError($"Failed to fetch devices: {response.StatusCode} - {response.ReasonPhrase}");
            return null;
        }



        public async Task<bool> TogglePlayPauseAsync(string accessToken)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                var response = await _httpClient.PutAsync("https://api.spotify.com/v1/me/player/play", null);
                if (!response.IsSuccessStatusCode)
                {
                    response = await _httpClient.PutAsync("https://api.spotify.com/v1/me/player/pause", null);
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error toggling play/pause: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SkipToPreviousAsync(string accessToken)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                var response = await _httpClient.PostAsync("https://api.spotify.com/v1/me/player/previous", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error skipping to previous: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SkipToNextAsync(string accessToken)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                var response = await _httpClient.PostAsync("https://api.spotify.com/v1/me/player/next", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error skipping to next: {ex.Message}");
                return false;
            }
        }






        public class ShowDetails : Show
        {
            public List<Episode> Episodes { get; set; }
            public string Uri { get; set; } 
        }


        public class Episode
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string ImageUrl { get; set; }
            public string Uri { get; set; } 
            public int DurationMs { get; set; } 
        }


        public class Playlist
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string ImageUrl { get; set; }
        }

        public class Track
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Artist { get; set; }
            public string ImageUrl { get; set; }
            public int Duration { get; set; }
            public string ContextUri { get; set; }
            public string Uri { get; set; } 
            public string AlbumId { get; set; } 
        }


        public class Show
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string ImageUrl { get; set; }
        }

        public class Genre
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string ImageUrl { get; set; }
        }

        public class GenreDetails : Genre
        {
            public List<Track> Tracks { get; set; }
        }

        public class Device
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public bool IsActive { get; set; }
            public bool IsPrivateSession { get; set; }
            public bool IsRestricted { get; set; }
            public string Type { get; set; }
            public int VolumePercent { get; set; }
        }

    }
}
