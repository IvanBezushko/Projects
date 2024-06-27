using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Globalization;
using System.Threading;
using static Natsukasiy.MainWindow;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Threading;
using System.IO;
using System.Diagnostics;
using System.Collections.Specialized;
using System.Net;
using System.Web;
using Mono.Web;

using WebSocketSharp;
using WebSocketSharp.Server;
using System.Windows.Controls.Primitives;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace Natsukasiy
{


    


        public partial class MainWindow : Window, INotifyPropertyChanged
    {

        private HttpListener _httpListener;
        private CancellationTokenSource _httpCts;
        private bool _isAuthorized = false;
        private SpotifyService spotifyService;
        private readonly ObservableCollection<object> _items = new ObservableCollection<object>();
        private readonly ObservableCollection<TrackViewModel> searchResults = new ObservableCollection<TrackViewModel>();
        private DispatcherTimer _timer = new DispatcherTimer();
        private SpotifyAuthenticator _authenticator;
        public ObservableCollection<PlaylistItem> RandomPlaylists { get; set; } = new ObservableCollection<PlaylistItem>();

        private readonly string clientId = "7eae723d9e134d2b9a4516d0ca88a7d9";
        private readonly string clientSecret = "2566165e0ddb48e4a6cbb2f1c4ecf2c8";
        private readonly string redirectUri = "https://ivanbezushko.github.io/Uri/index.html";

       
        private TrackViewModel1 _viewModel;
        private string currentTrackUri;
        private bool _isPlaying = false;     
       

        private string _statusMessage;
        public string StatusMessage
        {
            get { return _statusMessage; }
            set
            {
                _statusMessage = value;
                OnPropertyChanged(nameof(StatusMessage));       
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }







        public async void UpdateCurrentTrackDisplay()
        {
            while (!_isAuthorized)
            {
                await Task.Delay(500);
                Console.WriteLine("Waiting for authorization...");
            }

            TrackViewModel1 viewModel = Na.DataContext as TrackViewModel1;
            if (viewModel == null)
            {
                Console.WriteLine("ViewModel is not set or wrong type.");
                return;
            }

            var currentTrackInfo = await spotifyService.GetCurrentTrackInfoAsync();
            if (currentTrackInfo != null)
            {
                Dispatcher.Invoke(() =>
                {
                    
                    if (viewModel != null)
                    {
                        viewModel.Name = currentTrackInfo.TrackName;
                        viewModel.Artist = currentTrackInfo.ArtistName;
                        viewModel.ImageUrl = currentTrackInfo.ImageUrl;     
                    }
                    else
                    {
                        Console.WriteLine("ViewModel is not set or wrong type.");
                    }
                });
            }
            else
            {
                Console.WriteLine("No track info available.");
            }
        }





        private async void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var slider = sender as Slider;
            if (slider != null && spotifyService != null)
            {
                int volume = (int)slider.Value;
                await spotifyService.SetVolumeAsync(volume);
            }
            else
            {
                Debug.WriteLine("spotifyService is not initialized.");
            }
        }





















        public static readonly DependencyProperty DateTimeNowProperty =
            DependencyProperty.Register(
                nameof(DateTimeNow),
                typeof(DateTime),
                typeof(MainWindow),
                new PropertyMetadata(default(DateTime)));





        private async void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            bool result = await spotifyService.SkipToPreviousAsync();
            if (result)
            {
               
            }
            else
            {
                MessageBox.Show("Failed to skip to the previous track.");
            }
        }

        private async void NextButton_Click(object sender, RoutedEventArgs e)
        {
            bool result = await spotifyService.SkipToNextAsync();
            if (result)
            {
                
            }
            else
            {
                MessageBox.Show("Failed to skip to the next track.");
            }
        }






        private DispatcherTimer _progressTimer;

        public void StartProgressTimer()
        {
            _progressTimer = new DispatcherTimer();
            _progressTimer.Interval = TimeSpan.FromSeconds(1);    
            _progressTimer.Tick += async (sender, e) => await UpdateSliderPosition();
            _progressTimer.Start();
        }

       


        private async Task UpdateSliderPosition()
        {
            var isPlaying = await spotifyService.CheckIfPlaying();
            if (!isPlaying)
            {
                Console.WriteLine("No track is currently playing.");
                return;
            }

            var (duration, position) = await spotifyService.GetCurrentTrackProgressAsync();
            slider.Maximum = duration;
            slider.Value = position;
        }

        private void Slider_DragStarted(object sender, DragStartedEventArgs e)
        {
            _progressTimer.Stop();         
        }

        private async void Slider_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            _progressTimer.Start();   
            await spotifyService.SeekTo((int)slider.Value);      
        }

        public int CurrentPosition { get; set; }    
        public int MaxDuration { get; set; }     



        private async void InitializeHttpListener()         
        {
            _httpListener = new HttpListener();
            _httpListener.Prefixes.Add("http://localhost:8081/");      
            _httpCts = new CancellationTokenSource();

            try
            {
                _httpListener.Start();
                await ListenForHttpRequests(_httpCts.Token);       
            }
            catch (HttpListenerException ex)
            {
                MessageBox.Show($"HttpListener exception: {ex.Message}");
            }
        }


        private async Task ListenForHttpRequests(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    var context = await _httpListener.GetContextAsync();
                    HandleHttpRequest(context);
                }
                catch (Exception ex) when (ex is HttpListenerException || ex is OperationCanceledException)
                {
                    Console.WriteLine($"Wystąpił wyjątek: {ex.Message}");
                    MessageBox.Show($"Wystąpił wyjątek: {ex.Message}");
                }
            }
        }






        private void HandleHttpRequest(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;

            Console.WriteLine("Request URL: " + request.Url);
            Console.WriteLine("Request Query: " + request.Url.Query);

            if (request.QueryString["code"] != null)
            {
                string code = request.QueryString["code"];
                Console.WriteLine("Authorization code received: " + code);
               
                this.HandleAuthorizationResponse(code);       
            }
            else if (request.QueryString["error"] != null)
            {
                string error = request.QueryString["error"];
                Console.WriteLine("Authorization error received: " + error);
               
                _isAuthorized = false;
            }
            else
            {
                Console.WriteLine("No authorization code or error received.");
               
            }

            string responseString = "<html><body>You can close this window now.</body></html>";
            byte[] buffer = Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
            response.OutputStream.Close();
            response.StatusCode = 200;
            response.Close();
        }



        private async void HandleAuthorizationResponse(string code)
        {
            var accessToken = await ExchangeCodeForAccessTokenAsync(code);

            if (accessToken != null)
            {
                spotifyService = new SpotifyService(accessToken);       

                InitializeSpotifyService(accessToken);       

                Console.WriteLine("Authorization successful. Access token set.");
                _isAuthorized = true;
            }
            else
            {
                MessageBox.Show("Failed to exchange the authorization code for an access token.");
            }
        }










        private async Task<string> ExchangeCodeForAccessTokenAsync(string code)
        {
            var tokenEndpoint = "https://accounts.spotify.com/api/token";
            var requestBody = new Dictionary<string, string>
    {
        { "grant_type", "authorization_code" },
        { "code", code },
        { "redirect_uri", redirectUri },
        { "client_id", clientId },
        { "client_secret", clientSecret }
    };

            var requestContent = new FormUrlEncodedContent(requestBody);

            using (var client = new HttpClient())
            {
                var response = await client.PostAsync(tokenEndpoint, requestContent);
                var responseContent = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var tokenData = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(responseContent);
                    return tokenData["access_token"].GetString();      
                }
                else
                {
                    Console.WriteLine($"Failed to exchange token: HTTP {response.StatusCode} - {responseContent}");
                    return null;
                }
            }
        }











        private void Window_Closing(object sender, CancelEventArgs e)
        {
            _httpCts?.Cancel();
            _httpListener?.Close();
        }


       

        public DateTime DateTimeNow
        {
            get => (DateTime)GetValue(DateTimeNowProperty);
            set => SetValue(DateTimeNowProperty, value);
        }

        public MainWindow()
        {
            InitializeComponent();
            StartAuthorizationProcess();
            InitializeHttpListener();
            _authenticator = SpotifyAuthenticator.Instance;


            searchTextBox.TextChanged += SearchTextBox_TextChanged;
            randomPlaylistsListBox.SelectionChanged += RandomPlaylistsListBox_SelectionChanged;
            playlistsListBox.ItemsSource = _items;
            tracksListBox1.ItemsSource = searchResults;
            popularShowsListBox.SelectionChanged += PopularShowsListBox_SelectionChanged;


            


            var trackViewModel = new TrackViewModel1();
            Na.DataContext = trackViewModel;


            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += (sender, args) => DateTimeNow = DateTime.Now;
            _timer.Start();

            this.DataContext = this;


            
            var trackUpdateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)         
            };
            trackUpdateTimer.Tick +=  (sender, e) =>  UpdateCurrentTrackDisplay();
            trackUpdateTimer.Start();


        }



        public async void TogglePlayPause(object sender, RoutedEventArgs e)
        {
            bool isCurrentlyPlaying = await spotifyService.CheckIfPlaying();

            if (isCurrentlyPlaying)
            {
                bool stopped = await spotifyService.StopPlaybackAsync();
                if (stopped)
                {
                    _isPlaying = false;
                    PlayPauseIconKind = "Play";
                    Console.WriteLine("Playback stopped.");
                }
                else
                {
                    Console.WriteLine("Failed to stop playback.");
                }
            }
            else
            {
                var trackUri = await spotifyService.GetCurrentPlayingTrackAsync();
                if (string.IsNullOrEmpty(trackUri))
                {
                    Console.WriteLine("No track is currently playing or failed to fetch track.");
                    return;
                }
                currentTrackUri = trackUri;
                bool started = await spotifyService.ResumePlaybackAsync(currentTrackUri);
                if (started)
                {
                    _isPlaying = true;
                    PlayPauseIconKind = "Pause";
                    Console.WriteLine("Playback resumed.");
                }
                else
                {
                    Console.WriteLine("Failed to resume playback.");
                }
            }
        }






        public async void OnTrackSelected(string selectedTrackUri)
        {
            currentTrackUri = selectedTrackUri;
            bool isCurrentlyPlaying = await spotifyService.CheckIfPlaying();
            _isPlaying = isCurrentlyPlaying;
            PlayPauseIconKind = _isPlaying ? "Pause" : "Play";

            Console.WriteLine($"Track selected: {currentTrackUri}, playing: {_isPlaying}");
        }











        private async Task LoadRandomPlaylists()
        {
            string query = "tag:pop, k-pop, j-pop, disco";       
            var playlistsJson = await spotifyService.SearchPlaylistsAsync(query);
            Console.WriteLine($"Response: {playlistsJson}");   
            if (playlistsJson != null)
            {
                var playlists = ParsePlaylistsFromJson(playlistsJson);
                Console.WriteLine($"Parsed {playlists.Count} playlists.");     
                Application.Current.Dispatcher.Invoke(() =>
                {
                    RandomPlaylists.Clear();     
                    foreach (var playlist in playlists)
                    {
                        RandomPlaylists.Add(new PlaylistItem
                        {
                            Id = playlist.Id,
                            Name = playlist.Name,
                            ImageUrl = playlist.ImageUrl
                        });
                    }
                    randomPlaylistsListBox.ItemsSource = RandomPlaylists;       
                    Console.WriteLine($"Items set to list: {RandomPlaylists.Count}");      
                });
            }
            else
            {
                MessageBox.Show("No playlists found. Try a different query.");
            }
        }




        private async void RandomPlaylistsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (randomPlaylistsListBox.SelectedItem is PlaylistItem selectedPlaylist)
            {
                var tracks = await GetTracksForPlaylist(selectedPlaylist.Id);
                Tracks1ListBox.ItemsSource = tracks;


                randomPlaylistsListBox.Visibility = Visibility.Collapsed;
                Tracks1ListBox.Visibility = Visibility.Visible;
                toggleViewButton.Visibility = Visibility.Visible;
            }
        }



        private void DisplayTracks1(List<Track> tracks)
        {
            ClearItems();
            foreach (var track in tracks)
            {
                _items.Add(new TrackViewModel
                {
                    Id = track.Id,        
                    Name = track.Name,
                    Artist = track.Artist,
                    DurationFormatted = track.DurationFormatted,
                    ImageUrl = track.ImageUrl
                });
            }


            Tracks1ListBox.ItemsSource = tracks;

        }



        private List<PlaylistItem> ParsePlaylistsFromJson(string json)
        {
            var playlists = new List<PlaylistItem>();
            var jsonDoc = JsonDocument.Parse(json);
            var items = jsonDoc.RootElement.GetProperty("playlists").GetProperty("items").EnumerateArray();
            foreach (var item in items)
            {
                playlists.Add(new PlaylistItem
                {
                    Id = item.GetProperty("id").GetString(),
                    Name = item.GetProperty("name").GetString(),
                    ImageUrl = item.GetProperty("images").EnumerateArray().FirstOrDefault().GetProperty("url").GetString()
                });
            }
            return playlists;
        }







        private void Timer_Tick(object sender, EventArgs e)
        {
            CommandManager.InvalidateRequerySuggested();
           
        }



        private void ClearItems()
        {
            _items.Clear();
        }


        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();    
            }
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            HomeSection.Visibility = Visibility.Visible;
            BrowseSection.Visibility = Visibility.Collapsed;
            PlaylistSection.Visibility = Visibility.Collapsed;
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            HomeSection.Visibility = Visibility.Collapsed;
            BrowseSection.Visibility = Visibility.Visible;
            PlaylistSection.Visibility = Visibility.Collapsed;
        }

        private void PlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            HomeSection.Visibility = Visibility.Collapsed;
            BrowseSection.Visibility = Visibility.Collapsed;
            PlaylistSection.Visibility = Visibility.Visible;
        }

        private async void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool musicPaused = await spotifyService.StopMusicAsync();
                if (musicPaused)
                {
                    Debug.WriteLine("Music paused successfully.");
                }
                else
                {
                    Debug.WriteLine("Failed to pause music or no music was playing.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An error occurred while trying to pause music: {ex.Message}");
            }

            this.Close();
        }


        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }


        



        private void StartAuthorizationProcess()
        {
            string clientId = "7eae723d9e134d2b9a4516d0ca88a7d9";
            string scopes = "user-read-private user-read-email user-modify-playback-state user-read-playback-state";
            string redirectUri = "https://ivanbezushko.github.io/Uri/index.html";
            string authorizeUrl = $"https://accounts.spotify.com/authorize?client_id={clientId}&response_type=code&redirect_uri={Uri.EscapeDataString(redirectUri)}&scope={Uri.EscapeDataString(scopes)}&show_dialog=true";

            Process.Start(new ProcessStartInfo
            {
                FileName = authorizeUrl,
                UseShellExecute = true
            });
        }

        private async void InitializeSpotifyService(string accessToken)
        {
            if (!string.IsNullOrEmpty(accessToken))
            {
                spotifyService = new SpotifyService(accessToken);
               

                await LoadRandomPlaylists();
                await LoadRecommendationTracks();
                LoadAndDisplayShows();
                await DisplayRecommendationsGenres();
                StartProgressTimer();
                _isAuthorized = true;
                UpdateCurrentTrackDisplay();
            }
            else
            {
                MessageBox.Show("Access token is not available.");
            }
        }

        public class SpotifyAuthenticator
        {
            private readonly string clientId;
            private readonly string clientSecret;
            private HttpClient _httpClient = new HttpClient();
            private static SpotifyAuthenticator _instance;
            private static readonly object _lock = new object();

            private SpotifyAuthenticator(string clientId, string clientSecret)
            {
                this.clientId = clientId;
                this.clientSecret = clientSecret;
            }

            public static SpotifyAuthenticator Instance
            {
                get
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new SpotifyAuthenticator("7eae723d9e134d2b9a4516d0ca88a7d9", "2566165e0ddb48e4a6cbb2f1c4ecf2c8");
                        }
                        return _instance;
                    }
                }
            }

           



           
        }




        private async void SearchResultsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Console.WriteLine("SelectionChanged event triggered for SearchResultsListBox");

            if (!(SearchResultsListBox.SelectedItem is TrackViewModel selectedTrack))
            {
               
                Console.WriteLine("No track selected or incorrect data type from SearchResultsListBox.");
                return;
            }

            Console.WriteLine($"Selected track ID from SearchResultsListBox: {selectedTrack.Id}");

            if (spotifyService == null)
            {
               
                Console.WriteLine("Spotify Service is not initialized.");
                return;
            }

            string trackUri = $"spotify:track:{selectedTrack.Id}";
            Console.WriteLine($"Attempting to start playback with URI: {trackUri}");

            string contextUri = await spotifyService.GetContextUriForTrackAsync(selectedTrack.Id);
            if (string.IsNullOrWhiteSpace(contextUri))
            {
                MessageBox.Show("Failed to obtain album context for the selected track.");
                return;
            }

            bool success = await spotifyService.StartPlaybackAsync(trackUri, contextUri);
            if (!success)
            {
                
                Console.WriteLine("Failed to start the playback with selected track URI.");
            }
            else
            {
               
                Console.WriteLine("Playback started successfully with selected track URI.");
            }
        }







        private async void TracksListBox_SelectionChanged1(object sender, SelectionChangedEventArgs e)
        {
           
           


            if (!(tracksListBox1.SelectedItem is TrackViewModel selectedTrack))
            {
                MessageBox.Show("Incorrect item type selected.");
                return;
            }

            if (spotifyService == null)
            {
                MessageBox.Show("Spotify service is not initialized.");
                return;
            }

            string trackUri = $"spotify:track:{selectedTrack.Id}";
            string contextUri = await spotifyService.GetContextUriForTrackAsync(selectedTrack.Id);
            if (string.IsNullOrWhiteSpace(contextUri))
            {
                MessageBox.Show("Failed to obtain album context for the selected track.");
                return;
            }

            bool success = await spotifyService.StartPlaybackAsync(trackUri, contextUri);
            if (!success)
            {
                MessageBox.Show("Failed to start the playback. Please check your network connection and ensure the track URI is correct.");
            }
           
        }





        private async void TracksListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(tracksListBox.SelectedItem is TrackViewModel selectedTrack))
            {
                Console.WriteLine("TracksListBox_SelectionChanged: Incorrect item type selected.");
                
                return;
            }

            Console.WriteLine($"TracksListBox_SelectionChanged: Selected track ID: {selectedTrack.Id}");

            if (spotifyService == null)
            {
                Console.WriteLine("TracksListBox_SelectionChanged: Spotify service is not initialized.");
                MessageBox.Show("Spotify service is not initialized.");
                return;
            }

            string trackUri = $"spotify:track:{selectedTrack.Id}";
            Console.WriteLine($"TracksListBox_SelectionChanged: Attempting to start playback with URI: {trackUri}");

            string contextUri = await spotifyService.GetContextUriForTrackAsync(selectedTrack.Id);
            if (string.IsNullOrWhiteSpace(contextUri))
            {
                Console.WriteLine("TracksListBox_SelectionChanged: Failed to obtain album context.");
               
                return;
            }

            bool success = await spotifyService.StartPlaybackAsync(trackUri, contextUri);
            if (!success)
            {
                Console.WriteLine("TracksListBox_SelectionChanged: Failed to start the playback.");
               
            }
            else
            {
                Console.WriteLine("TracksListBox_SelectionChanged: Playback started successfully.");
                
            }
        }




        private async void EpisodesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(episodesListBox.SelectedItem is Episode selectedEpisode))
            {
               
                return;
            }

            if (spotifyService == null)
            {
                MessageBox.Show("Spotify service is not initialized.");
                return;
            }

            string episodeUri = $"spotify:episode:{selectedEpisode.Id}";
            Console.WriteLine($"Attempting to start playback with URI: {episodeUri}");

            bool success = await spotifyService.StartPlaybackAsync(episodeUri, null);         
            if (!success)
            {
                MessageBox.Show("Failed to start the playback. Please check your network connection and ensure the episode URI is correct.");
            }
            else
            {
               
            }
        }


        private async void Tracks1ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(Tracks1ListBox.SelectedItem is TrackViewModel selectedTrack))
            {
                Console.WriteLine("Tracks1ListBox_SelectionChanged: Incorrect item type selected.");
               
                return;
            }

            Console.WriteLine($"Tracks1ListBox_SelectionChanged: Selected track ID: {selectedTrack.Id}");

            if (spotifyService == null)
            {
                Console.WriteLine("Tracks1ListBox_SelectionChanged: Spotify service is not initialized.");
                MessageBox.Show("Spotify service is not initialized.");
                return;
            }

            string trackUri = $"spotify:track:{selectedTrack.Id}";
            Console.WriteLine($"Tracks1ListBox_SelectionChanged: Attempting to start playback with URI: {trackUri}");

            string contextUri = await spotifyService.GetContextUriForTrackAsync(selectedTrack.Id);
            if (string.IsNullOrWhiteSpace(contextUri))
            {
                Console.WriteLine("Tracks1ListBox_SelectionChanged: Failed to obtain album context.");
                MessageBox.Show("Failed to obtain album context.");
                return;
            }

            bool success = await spotifyService.StartPlaybackAsync(trackUri, contextUri);
            if (!success)
            {
                Console.WriteLine("Tracks1ListBox_SelectionChanged: Failed to start the playback.");
                MessageBox.Show("Failed to start the playback. Please check your network connection and ensure the track URI is correct.");
            }
            else
            {
                Console.WriteLine("Tracks1ListBox_SelectionChanged: Playback started successfully.");
                
            }
        }




        private async void PlaylistsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (playlistsListBox.SelectedItem is PlaylistItem selectedPlaylist)
            {
                var tracks = await GetTracksForPlaylist(selectedPlaylist.Id);
                if (tracks != null && tracks.Any())
                {
                    playlistsListBox.ItemsSource = tracks;
                    playlistsListBox.ItemTemplate = (DataTemplate)Resources["TrackTemplate"];
                }
                else
                {
                    MessageBox.Show("No tracks found for this playlist.");
                }
            }
            else if (playlistsListBox.SelectedItem is TrackViewModel selectedTrack)
            {
                Console.WriteLine($"Selected track ID: {selectedTrack.Id}");

                if (spotifyService == null)
                {
                    MessageBox.Show("Spotify service is not initialized.");
                    return;
                }

                string trackUri = $"spotify:track:{selectedTrack.Id}";
                Console.WriteLine($"Attempting to start playback with URI: {trackUri}");

                string contextUri = await spotifyService.GetContextUriForTrackAsync(selectedTrack.Id);
                if (string.IsNullOrWhiteSpace(contextUri))
                {
                    MessageBox.Show("Failed to obtain album context.");
                    return;
                }

                bool success = await spotifyService.StartPlaybackAsync(trackUri, contextUri);
                if (!success)
                {
                    MessageBox.Show("Failed to start the playback. Please check your network connection and ensure the track URI is correct.");
                }
            }
            else
            {
                
            }
        }




        public class SpotifyService
        {
            public  HttpClient client = new HttpClient();
            
            private string _accessToken;
            private long _pausePositionMs = 0;
            public bool _isAuthorized = false;



            public async Task<bool> StopMusicAsync()
            {
                try
                {
                    string endpoint = "https://api.spotify.com/v1/me/player/pause";
                    var response = await client.PutAsync(endpoint, null);
                    if (!response.IsSuccessStatusCode)
                    {
                        Debug.WriteLine($"Failed to stop music. Status code: {response.StatusCode}");
                        return false;
                    }
                    Debug.WriteLine("Music stopped successfully.");
                    return true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Exception when trying to stop music: {ex.Message}");
                    return false;
                }
            }



            public async Task<bool> StartPlaybackAsync(string uri, string contextUri = null, string deviceId = null)
            {
                try
                {
                    Console.WriteLine($"Access Token: {_accessToken}");
                    if (!Uri.TryCreate(uri, UriKind.Absolute, out var parsedUri))
                    {
                        Console.WriteLine("Invalid URI format: " + uri);
                        return false;
                    }

                    var availableDevices = await GetAvailableDevices();
                    deviceId = deviceId ?? availableDevices.FirstOrDefault()?.Id;

                    if (string.IsNullOrEmpty(deviceId))
                    {
                        Console.WriteLine("No active devices found.");
                        return false;
                    }

                    object requestBody;
                    if (contextUri != null)
                    {
                        requestBody = new
                        {
                            context_uri = contextUri,
                            offset = new { position = 0 },
                            position_ms = 0,
                            device_ids = new string[] { deviceId }
                        };
                    }
                    else
                    {
                        requestBody = new
                        {
                            uris = new string[] { uri },
                            offset = new { position = 0 },
                            position_ms = 0,
                            device_ids = new string[] { deviceId }
                        };
                    }

                    string json = System.Text.Json.JsonSerializer.Serialize(requestBody);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

                    var response = await client.PutAsync("https://api.spotify.com/v1/me/player/play", content);
                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"Failed to start playback: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                        return false;
                    }
                    Console.WriteLine("Playback started successfully with URI: " + uri);
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred during playback: {ex.Message}");
                    return false;
                }
            }




            public async Task<bool> StopPlaybackAsync()
            {
                try
                {
                    await Task.Delay(500);

                    var playerStateResponse = await client.GetAsync("https://api.spotify.com/v1/me/player");
                    if (playerStateResponse.IsSuccessStatusCode)
                    {
                        var content = await playerStateResponse.Content.ReadAsStringAsync();
                        var playerState = JsonDocument.Parse(content);
                        if (playerState.RootElement.TryGetProperty("is_playing", out JsonElement isPlaying) && isPlaying.GetBoolean())
                        {
                            if (playerState.RootElement.TryGetProperty("progress_ms", out JsonElement progressMs))
                            {
                                _pausePositionMs = progressMs.GetInt64();
                            }

                            var pauseResponse = await client.PutAsync("https://api.spotify.com/v1/me/player/pause", null);
                            if (pauseResponse.IsSuccessStatusCode)
                            {
                                Console.WriteLine("Playback stopped successfully at position: " + _pausePositionMs);
                                return true;
                            }
                            else
                            {
                                Console.WriteLine($"Failed to stop playback: {pauseResponse.StatusCode} - {await pauseResponse.Content.ReadAsStringAsync()}");
                                return false;
                            }
                        }
                        else
                        {
                            Console.WriteLine("No active playback to stop.");
                            return false;
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Failed to get player state: HTTP {playerStateResponse.StatusCode} - {await playerStateResponse.Content.ReadAsStringAsync()}");
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred during stop playback: {ex.Message}");
                    return false;
                }
            }





            public async Task<bool> ResumePlaybackAsync(string uri)
            {
                try
                {
                    var body = new
                    {
                        uris = new[] { uri },
                        position_ms = _pausePositionMs
                    };

                    var json = System.Text.Json.JsonSerializer.Serialize(body);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

                    var response = await client.PutAsync("https://api.spotify.com/v1/me/player/play", content);
                    Console.WriteLine($"Attempting to resume: {json}");    
                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"Failed to resume playback: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                        return false;
                    }
                    Console.WriteLine("Playback resumed successfully from position: " + _pausePositionMs);
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred during resume playback: {ex.Message}");
                    return false;
                }
            }


            public async Task SeekTo(int positionMs)
            {
                var response = await client.PutAsync($"https://api.spotify.com/v1/me/player/seek?position_ms={positionMs}", null);
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Failed to seek track.");
                }
            }


            private DateTime lastVolumeUpdate = DateTime.MinValue;
            private int lastVolumeLevel = -1;

            public async Task<bool> SetVolumeAsync(int volume)
            {
                int currentVolume = await GetCurrentVolumeAsync();
                if (currentVolume == -1)
                {
                    Debug.WriteLine("Unable to retrieve current volume.");
                    return false;         
                }

                if (currentVolume == volume)
                {
                    Debug.WriteLine($"Volume is already set to {volume}.");
                    return true;         
                }

                if (DateTime.Now - lastVolumeUpdate < TimeSpan.FromSeconds(1))
                {
                    Debug.WriteLine("Volume update request is too frequent.");
                    return false;      
                }

                try
                {
                    string endpoint = $"https://api.spotify.com/v1/me/player/volume?volume_percent={Math.Round((double)volume)}";
                    var response = await client.PutAsync(endpoint, null);
                    if (!response.IsSuccessStatusCode)
                    {
                        Debug.WriteLine($"Failed to set volume. Status code: {response.StatusCode}");
                        return false;
                    }

                    lastVolumeUpdate = DateTime.Now;
                    lastVolumeLevel = volume;
                    Debug.WriteLine("Volume set successfully.");
                    return true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Exception when setting volume: {ex.Message}");
                    return false;
                }
            }

            private async Task<int> GetCurrentVolumeAsync()
            {
                try
                {
                    string endpoint = "https://api.spotify.com/v1/me/player";
                    var response = await client.GetAsync(endpoint);
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var playerState = JsonConvert.DeserializeObject<dynamic>(content);
                        if (playerState != null && playerState.device != null)
                        {
                            int currentVolume = playerState.device.volume_percent;
                            return currentVolume;
                        }
                        Debug.WriteLine("Current player state or device information is null.");
                        return -1;
                    }
                    Debug.WriteLine($"Failed to retrieve player information. Status code: {response.StatusCode}");
                    return -1;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Exception when getting current volume: {ex.Message}");
                    return -1;
                }
            }






            public async Task<PlaybackContext> GetCurrentPlaybackContextAsync()
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync("https://api.spotify.com/v1/me/player");
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var playerInfo = System.Text.Json.JsonSerializer.Deserialize<JsonElement>(content);

                        if (playerInfo.TryGetProperty("is_playing", out JsonElement isPlaying) &&
                            playerInfo.TryGetProperty("item", out JsonElement item) &&
                            item.TryGetProperty("uri", out JsonElement trackUri) &&
                            playerInfo.TryGetProperty("context", out JsonElement context) &&
                            context.TryGetProperty("uri", out JsonElement contextUri))
                        {
                            return new PlaybackContext
                            {
                                IsPlaying = isPlaying.GetBoolean(),
                                TrackUri = trackUri.GetString(),
                                ContextUri = contextUri.GetString()
                            };
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error in GetCurrentPlaybackContextAsync: " + ex.Message);
                }
                return null;
            }


            public async Task<CurrentTrackInfo> GetCurrentTrackInfoAsync()
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync("https://api.spotify.com/v1/me/player");
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var playerInfo = System.Text.Json.JsonSerializer.Deserialize<JsonElement>(content);

                        if (playerInfo.TryGetProperty("item", out JsonElement item) &&
                            item.TryGetProperty("name", out JsonElement name) &&
                            item.TryGetProperty("artists", out JsonElement artists) &&
                            artists.EnumerateArray().FirstOrDefault().TryGetProperty("name", out JsonElement artistName) &&
                            item.TryGetProperty("album", out JsonElement album) &&
                            album.TryGetProperty("images", out JsonElement images) &&
                            images.EnumerateArray().FirstOrDefault().TryGetProperty("url", out JsonElement imageUrl))
                        {
                            return new CurrentTrackInfo
                            {
                                TrackName = name.GetString(),
                                ArtistName = artistName.GetString(),
                                ImageUrl = imageUrl.GetString(),     
                            };
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error in GetCurrentTrackInfoAsync: " + ex.Message);
                }
                return null;
            }




            public async Task<bool> SkipToPreviousAsync()
            {
                var currentContext = await GetCurrentPlaybackContextAsync();
                if (currentContext != null && currentContext.IsPlaying)
                {
                    try
                    {
                        HttpResponseMessage response = await client.PostAsync("https://api.spotify.com/v1/me/player/previous", null);
                        return response.IsSuccessStatusCode;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Error in SkipToPreviousAsync: " + ex.Message);
                    }
                }
                else
                {
                    Debug.WriteLine("No active or valid playback context to skip tracks within.");
                }
                return false;
            }


            public async Task<bool> SkipToNextAsync()
            {
                var currentContext = await GetCurrentPlaybackContextAsync();
                if (currentContext != null && currentContext.IsPlaying)
                {
                    try
                    {
                        HttpResponseMessage response = await client.PostAsync("https://api.spotify.com/v1/me/player/next", null);
                        return response.IsSuccessStatusCode;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Error in SkipToNextAsync: " + ex.Message);
                    }
                }
                else
                {
                    Debug.WriteLine("No active or valid playback context to skip tracks within.");
                }
                return false;
            }




           






            public async Task<bool> CheckIfPlaying()
            {
                var response = await client.GetAsync("https://api.spotify.com/v1/me/player");
                if (response.IsSuccessStatusCode && response.Content != null)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrWhiteSpace(content))
                    {
                        try
                        {
                            var playerState = JsonDocument.Parse(content);
                            if (playerState.RootElement.TryGetProperty("is_playing", out JsonElement isPlaying))
                            {
                                return isPlaying.GetBoolean();
                            }
                        }
                        catch (System.Text.Json.JsonException ex)
                        {
                            Console.WriteLine("Failed to parse JSON: " + ex.Message);
                        }
                    }
                }
                return false;          
            }


            public async Task<bool> CheckIfTrackIsPlaying(string trackUri)
            {
                try
                {
                    var response = await client.GetAsync("https://api.spotify.com/v1/me/player/currently-playing");
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var playerInfo = JsonDocument.Parse(content);
                        if (playerInfo.RootElement.TryGetProperty("currently_playing_type", out JsonElement type))
                        {
                            if (type.GetString() == "track" && playerInfo.RootElement.TryGetProperty("item", out JsonElement item) &&
                                item.TryGetProperty("uri", out JsonElement uri))
                            {
                                return uri.GetString() == trackUri && playerInfo.RootElement.GetProperty("is_playing").GetBoolean();
                            }
                            else if (type.GetString() == "episode" && playerInfo.RootElement.TryGetProperty("item", out JsonElement episodeItem) &&
                                     episodeItem.TryGetProperty("uri", out JsonElement episodeUri))
                            {
                                return episodeUri.GetString() == trackUri && playerInfo.RootElement.GetProperty("is_playing").GetBoolean();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred while checking if track or episode is playing: {ex.Message}");
                }
                return false;        
            }



            public async Task<string> GetCurrentPlayingTrackAsync()
            {
                try
                {
                    var response = await client.GetAsync("https://api.spotify.com/v1/me/player/currently-playing");
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        using (var json = JsonDocument.Parse(content))
                        {
                            if (!json.RootElement.TryGetProperty("item", out JsonElement item) || item.ValueKind == JsonValueKind.Null)
                            {
                                Console.WriteLine("No current track information available.");
                                return null;
                            }

                            if (item.TryGetProperty("uri", out JsonElement uri))
                            {
                                return uri.GetString();
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Failed to get current playing track: HTTP {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred during fetching current playing track: {ex.Message}");
                }
                return null;                
            }



            public async Task<(double Duration, double Position)> GetCurrentTrackProgressAsync()
            {
                var response = await client.GetAsync("https://api.spotify.com/v1/me/player/currently-playing");
                if (response.IsSuccessStatusCode && response.Content != null)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(jsonContent))
                    {
                        var document = JsonDocument.Parse(jsonContent);
                        var root = document.RootElement;

                        string type = root.GetProperty("currently_playing_type").GetString();

                        if (type == "track" && root.TryGetProperty("item", out JsonElement item) && item.TryGetProperty("duration_ms", out JsonElement durationElement))
                        {
                            double duration = durationElement.GetDouble();
                            double position = root.TryGetProperty("progress_ms", out JsonElement positionElement) ? positionElement.GetDouble() : 0;
                            return (duration, position);
                        }
                        else if (type == "episode")
                        {
                            Debug.WriteLine("Currently playing content is an episode, which might not support duration and position in the same way as tracks.");
                        }
                    }
                }

                return (0, 0);            
            }








            public async Task<string> GetContextUriForTrackAsync(string trackId)
            {
                try
                {
                    var response = await client.GetAsync($"https://api.spotify.com/v1/tracks/{trackId}");
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();

                        var json = JObject.Parse(content);
                        var album = json["album"];
                        var albumId = album["id"].ToString();

                        return $"spotify:album:{albumId}";
                    }
                    else
                    {
                        Console.WriteLine($"Failed to get track details. Status code: {response.StatusCode}");
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred while getting track details: {ex.Message}");
                    return null;
                }
            }





            public async Task<List<Device>> GetAvailableDevices()
            {
                var response = await client.GetAsync("https://api.spotify.com/v1/me/player/devices");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    using (JsonDocument doc = JsonDocument.Parse(json))
                    {
                        var devices = new List<Device>();
                        foreach (var item in doc.RootElement.GetProperty("devices").EnumerateArray())
                        {
                            devices.Add(new Device
                            {
                                Id = item.GetProperty("id").GetString(),
                                IsActive = item.GetProperty("is_active").GetBoolean(),
                                IsPrivateSession = item.GetProperty("is_private_session").GetBoolean(),
                                Name = item.GetProperty("name").GetString(),
                                Type = item.GetProperty("type").GetString(),
                                VolumePercent = item.GetProperty("volume_percent").GetInt32()
                            });
                        }
                        return devices;
                    }
                }
                else
                {
                    Console.WriteLine("Failed to fetch devices.");
                    return null;
                }
            }




            public void SetAccessToken(string accessToken)
            {
                _accessToken = accessToken;
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                Console.WriteLine("Token set: " + accessToken);      
            }














            public SpotifyService(string accessToken)
            {
                _accessToken = accessToken;
                client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }

            public async Task<string> SearchPlaylistsAsync(string query)
            {
                var response = await client.GetAsync($"https://api.spotify.com/v1/search?q={query}&type=playlist&limit=15");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }

                return null;
            }

            public async Task<string> SearchTracksAsync(string query)
            {
                var encodedQuery = Uri.EscapeDataString(query);
                var response = await client.GetAsync($"https://api.spotify.com/v1/search?q={encodedQuery}&type=track&limit=20");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return content;
                }
                else
                {
                    return null;
                }
            }

            public async Task<List<string>> SearchRecommendationsGenresAsync()
            {
                var response = await client.GetAsync("https://api.spotify.com/v1/recommendations/available-genre-seeds");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var jsonDoc = JsonDocument.Parse(content);

                    var genres = jsonDoc.RootElement.GetProperty("genres").EnumerateArray()
                        .Select(genre => genre.GetString())
                        .ToList();

                    return genres;
                }
                else if (response.StatusCode == (HttpStatusCode)429)
                {
                    int waitTime = int.Parse(response.Headers.RetryAfter?.Delta?.TotalSeconds.ToString() ?? "60");
                    await Task.Delay(waitTime * 1000);
                    return await SearchRecommendationsGenresAsync();   
                }

                return null;
            }



            public async Task<List<SpotifyShow>> GetShowsAsync(string[] showIds)
            {
                var ids = string.Join(",", showIds);
                var response = await client.GetAsync($"https://api.spotify.com/v1/shows?ids={ids}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var jsonDoc = JsonDocument.Parse(content);
                    var showsList = new List<SpotifyShow>();

                    foreach (var item in jsonDoc.RootElement.GetProperty("shows").EnumerateArray())
                    {
                        showsList.Add(new SpotifyShow
                        {
                            Id = item.GetProperty("id").GetString(),
                            Name = item.GetProperty("name").GetString(),
                            Publisher = item.GetProperty("publisher").GetString(),
                            Description = item.GetProperty("description").GetString(),
                            ImageUrl = item.GetProperty("images").EnumerateArray().FirstOrDefault().GetProperty("url").GetString(),
                        });
                    }

                    return showsList;
                }

                return new List<SpotifyShow>();
            }


            public async Task<List<Episode>> GetEpisodesForShow(string showId)
            {
                List<Episode> episodesList = new List<Episode>();
                int limit = 50;
                int offset = 0;
                bool moreEpisodesAvailable = true;

                while (moreEpisodesAvailable)
                {
                    var response = await client.GetAsync($"https://api.spotify.com/v1/shows/{showId}/episodes?limit={limit}&offset={offset}");

                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var jsonDoc = JsonDocument.Parse(content);
                        var episodes = jsonDoc.RootElement.GetProperty("items").EnumerateArray();

                        foreach (var item in episodes)
                        {
                            string imageUrl = "default_image_url";        

                            if (item.TryGetProperty("images", out JsonElement imagesElement) && imagesElement.EnumerateArray().Any())
                            {
                                imageUrl = imagesElement.EnumerateArray().FirstOrDefault().GetProperty("url").GetString();
                            }

                            episodesList.Add(new Episode
                            {
                                Id = item.GetProperty("id").GetString(),
                                Name = item.GetProperty("name").GetString(),
                                Description = item.GetProperty("description").GetString(),
                                ImageUrl = imageUrl,
                            });
                        }


                        offset += episodes.Count();
                        moreEpisodesAvailable = episodes.Count() == limit;
                    }
                    else
                    {
                        Console.WriteLine("Failed to retrieve episodes: " + response.StatusCode);
                        moreEpisodesAvailable = false;
                    }
                }

                return episodesList;
            }








        }



        private string _playPauseIconKind = "Pause";
        public string PlayPauseIconKind
        {
            get => _playPauseIconKind;
            set
            {
                _playPauseIconKind = value;
                OnPropertyChanged(nameof(PlayPauseIconKind));        
            }
        }

       


        private async void PausePlayback()
        {
            await spotifyService.StopPlaybackAsync();       
        }

        private async void ResumePlayback()
        {
            await spotifyService.StartPlaybackAsync(currentTrackUri);       
        }

















        private async Task<List<TrackViewModel>> GetTracksForPlaylist(string playlistId)
        {
            var tracks = new List<TrackViewModel>();

            try
            {
                var response = await spotifyService.client.GetAsync($"https://api.spotify.com/v1/playlists/{playlistId}/tracks");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var jsonDoc = JsonDocument.Parse(content);

                    var items = jsonDoc.RootElement.GetProperty("items").EnumerateArray();
                    foreach (var item in items)
                    {
                        var trackJson = item.GetProperty("track");
                        var artists = trackJson.GetProperty("artists").EnumerateArray();
                        var images = trackJson.GetProperty("album").GetProperty("images").EnumerateArray();

                        var trackViewModel = new TrackViewModel
                        {
                            Id = trackJson.GetProperty("id").GetString(),
                            Name = trackJson.GetProperty("name").GetString(),
                            Artist = artists.Any() ? artists.First().GetProperty("name").GetString() : "Unknown Artist",
                            DurationFormatted = TimeSpan.FromMilliseconds(trackJson.GetProperty("duration_ms").GetInt64()).ToString(@"mm\:ss"),
                            ImageUrl = images.Any() ? images.First().GetProperty("url").GetString() : string.Empty
                        };
                        tracks.Add(trackViewModel);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Wystąpił błąd podczas pobierania traków: {ex.Message}");
            }

            return tracks;
        }





        private async Task<List<TrackViewModel>> GetTracksForGenre(string genre)
        {
            var tracks = new List<TrackViewModel>();
            try
            {
                var searchResult = await spotifyService.SearchTracksAsync($"genre:\"{genre}\"");
                if (searchResult != null)
                {
                    var jsonDoc = JsonDocument.Parse(searchResult);
                    var items = jsonDoc.RootElement.GetProperty("tracks").GetProperty("items").EnumerateArray();
                    foreach (var item in items)
                    {
                        var trackViewModel = new TrackViewModel
                        {
                            Id = item.GetProperty("id").GetString(),
                            Name = item.GetProperty("name").GetString(),
                            Artist = item.GetProperty("artists").EnumerateArray().FirstOrDefault().GetProperty("name").GetString(),
                            DurationFormatted = TimeSpan.FromMilliseconds(item.GetProperty("duration_ms").GetInt64()).ToString(@"mm\:ss"),
                            ImageUrl = item.GetProperty("album").GetProperty("images").EnumerateArray().FirstOrDefault().GetProperty("url").GetString()
                        };
                        tracks.Add(trackViewModel);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error fetching tracks for genre {genre}: {ex.Message}");
            }
            return tracks;
        }



        private async void GenresListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (genresListBox.SelectedItem is string selectedGenre)
            {
                var tracks = await GetTracksForGenre(selectedGenre);
                DisplayTracksForGenre(tracks);
                genresListBox.Visibility = Visibility.Collapsed;    
                tracksListBox.Visibility = Visibility.Visible;    
                backButton1.Visibility = Visibility.Visible;      
            }
        }


        private void BackButton1_Click(object sender, RoutedEventArgs e)
        {
            genresListBox.Visibility = Visibility.Visible;
            tracksListBox.Visibility = Visibility.Collapsed;
            backButton1.Visibility = Visibility.Collapsed;
        }

        private void ToggleViewButton_Click(object sender, RoutedEventArgs e)
        {
            if (randomPlaylistsListBox.Visibility == Visibility.Visible)
            {
                randomPlaylistsListBox.Visibility = Visibility.Collapsed;
                Tracks1ListBox.Visibility = Visibility.Visible;
            }
            else
            {
                randomPlaylistsListBox.Visibility = Visibility.Visible;
                Tracks1ListBox.Visibility = Visibility.Collapsed;
            }
            toggleViewButton.Visibility = Visibility.Collapsed;
        }


        private void DisplayTracksForGenre(List<TrackViewModel> tracks)
        {
            tracksListBox.ItemsSource = tracks;
            tracksListBox.Visibility = Visibility.Visible;
            backButton1.Visibility = Visibility.Visible;
        }















        private async void LoadAndDisplayShows()
        {
            var showIds = new[]
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

            try
            {
                var shows = await spotifyService.GetShowsAsync(showIds);

                popularShowsListBox.ItemsSource = shows;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error occurred while fetching show data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        private CancellationTokenSource cancellationTokenSource;

        private CancellationTokenSource _searchCancellationTokenSource;

        private async void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(searchTextBox.Text))
            {
                ClearItems();
                return;
            }

            if (spotifyService == null)
            {
                MessageBox.Show("Spotify Service is not initialized. Please authorize.");
                return;
            }

            _searchCancellationTokenSource?.Cancel();
            _searchCancellationTokenSource = new CancellationTokenSource();

            try
            {
                await Task.Delay(500, _searchCancellationTokenSource.Token);

                var searchResults = await spotifyService.SearchPlaylistsAsync(searchTextBox.Text);
                if (_searchCancellationTokenSource.Token.IsCancellationRequested)
                    return;

                var playlists = ConvertSearchResultsToPlaylistItems(searchResults);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    playlistsListBox.ItemsSource = playlists;
                });
            }
            catch (TaskCanceledException)
            {
                return;
            }
        }

        private List<PlaylistItem> ConvertSearchResultsToPlaylistItems(string json)
        {
            var playlists = new List<PlaylistItem>();

            if (string.IsNullOrEmpty(json))
            {
                return playlists;           
            }

            var jsonDoc = JsonDocument.Parse(json);
            var items = jsonDoc.RootElement.GetProperty("playlists").GetProperty("items").EnumerateArray();
            foreach (var item in items)
            {
                playlists.Add(new PlaylistItem
                {
                    Id = item.GetProperty("id").GetString(),
                    Name = item.GetProperty("name").GetString(),
                    ImageUrl = item.GetProperty("images").EnumerateArray().FirstOrDefault().GetProperty("url").GetString()
                });
            }
            return playlists;
        }














        private void DisplaySearchResults(string searchResult)
        {
            ClearItems();

            var jsonDoc = JsonDocument.Parse(searchResult);
            var playlists = jsonDoc.RootElement.GetProperty("playlists").GetProperty("items").EnumerateArray();

            foreach (var item in playlists)
            {
                var playlistItem = new PlaylistItem
                {
                    Id = item.GetProperty("id").GetString(),
                    Name = item.GetProperty("name").GetString(),
                    ImageUrl = item.GetProperty("images").EnumerateArray().FirstOrDefault().GetProperty("url").GetString()
                };

                _items.Add(playlistItem);
            }

            playlistsListBox.ItemsSource = _items;

        }







       



       

        private void DisplayTracks(List<Track> tracks)
        {
            ClearItems();
            foreach (var track in tracks)
            {
                _items.Add(new TrackViewModel
                {
                    Id = track.Id,        
                    Name = track.Name,
                    Artist = track.Artist,
                    DurationFormatted = track.DurationFormatted,
                    ImageUrl = track.ImageUrl
                });
            }

            playlistsListBox.ItemTemplate = FindResource("TrackTemplate") as DataTemplate;

        }




        
        public class TrackViewModel
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Artist { get; set; }
            public string DurationFormatted { get; set; }
            public string ImageUrl { get; set; }
            public string Uri { get; internal set; }
        }

        public class GenreItem
        {
            public string Name { get; set; }   
            public string ImageUrl { get; set; }    
        }

        public class SpotifyShow
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Publisher { get; set; }
            public string Description { get; set; }
            public string ImageUrl { get; set; }
        }

        public class Episode
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string ImageUrl { get; set; }
        }

        public class PlaylistItem
        {
            public string Name { get; set; }
            public string Id { get; set; }
            public string ImageUrl { get; set; }
            public string Description { get; set; }
        }

        public class Track
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Artist { get; set; }
            public TimeSpan Duration { get; set; }
            public string DurationFormatted => Duration.ToString(@"mm\:ss");
            public string ImageUrl { get; set; }           
            public string Description { get; set; }
            public string Uri { get; internal set; }
        }

        public class CurrentPlaybackState
        {
            public bool IsPlaying { get; set; }
            public Track Item { get; set; }
            public int ProgressMs { get; set; }
            public Device Device { get; set; }
        }

        public class Artist
        {
            public string Name { get; set; }
        }

        public class Album
        {
            public string Name { get; set; }
            public Image[] Images { get; set; }
        }

        public class Image
        {
            public string Url { get; set; }
        }

        public class Device
        {
            public string Id { get; set; }
            public bool IsActive { get; set; }
            public bool IsPrivateSession { get; set; }
            public string Name { get; set; }
            public string Type { get; set; }
            public int VolumePercent { get; set; }
        }


        public class PlaybackRequest
        {
            public string[] Uris { get; set; }
            public long PositionMs { get; set; }
        }

        public class PlaybackContext
        {
            public bool IsPlaying { get; set; }
            public string TrackUri { get; set; }
            public string ContextUri { get; set; }
        }

        public class CurrentTrackInfo
        {
            public bool IsPlaying { get; set; }
            public string TrackName { get; set; }
            public string ArtistName { get; set; }
            public string TrackUri { get; set; }
            public string ContextUri { get; set; }
            public string ImageUrl { get; set; }
        }






        private async void SearchTracks(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                ClearItems();
                return;
            }

            cancellationTokenSource?.Cancel();
            cancellationTokenSource = new CancellationTokenSource();
            CancellationToken token = cancellationTokenSource.Token;

            try
            {
                await Task.Delay(500, token);
                if (token.IsCancellationRequested) return;

                var searchResult = await spotifyService.SearchTracksAsync(query);
                if (!token.IsCancellationRequested && searchResult != null)
                {
                    DisplayTrackSearchResults(searchResult);
                }
            }
            catch (TaskCanceledException)
            {
            }
        }

        private void DisplayTrackSearchResults(string searchResult)
        {
            var searchResults = new ObservableCollection<TrackViewModel>();
            var jsonDoc = JsonDocument.Parse(searchResult);
            var tracks = jsonDoc.RootElement.GetProperty("tracks").GetProperty("items").EnumerateArray();

            if (string.IsNullOrWhiteSpace(searchResult))
            {
                ClearItems();
                return;
            }

            foreach (var item in tracks)
            {
                var trackItem = new TrackViewModel
                {
                    Id = item.GetProperty("id").GetString(),
                    Name = item.GetProperty("name").GetString(),
                    Artist = item.GetProperty("artists").EnumerateArray().FirstOrDefault().GetProperty("name").GetString(),
                    DurationFormatted = TimeSpan.FromMilliseconds(item.GetProperty("duration_ms").GetInt64()).ToString(@"mm\:ss"),
                    ImageUrl = item.GetProperty("album").GetProperty("images").EnumerateArray().FirstOrDefault().GetProperty("url").GetString(),
                };

                searchResults.Add(trackItem);
            }

            SearchResultsListBox.ItemsSource = searchResults;
           
        }


       


        private async void NewSearchTracksTextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = (sender as TextBox)?.Text.Trim();

            if (string.IsNullOrEmpty(searchText))
            {
                ClearItems();
                SearchResultsListBox.ItemsSource = null;
                SearchResultsListBox.Visibility = Visibility.Collapsed;
                return;
            }
            else
            {
                SearchResultsListBox.Visibility = Visibility.Visible;
            }

            if (spotifyService == null)
            {
                Console.WriteLine("spotifyService is null at the time of search.");
                MessageBox.Show("Please ensure you are authorized before searching.");
                return;
            }

            cancellationTokenSource?.Cancel();
            cancellationTokenSource = new CancellationTokenSource();
            CancellationToken token = cancellationTokenSource.Token;

            try
            {
                await Task.Delay(500, token);      
                if (token.IsCancellationRequested) return;

                var searchResult = await spotifyService.SearchTracksAsync(searchText);
                if (!token.IsCancellationRequested)
                {
                    DisplayTrackSearchResults(searchResult);
                }
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("The task has been cancelled.");
            }
        }




        private async Task DisplayRecommendationsGenres()
        {
            var genres = await spotifyService.SearchRecommendationsGenresAsync();
            if (genres != null)
            {
                genresListBox.Items.Clear();

                foreach (var genre in genres)
                {
                    genresListBox.Items.Add(genre);
                }
            }
        }





        private async void ShowsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (popularShowsListBox.SelectedItem is SpotifyShow selectedShow)
            {
                var episodes = await spotifyService.GetEpisodesForShow(selectedShow.Id);
                DisplayEpisodes(episodes);       
            }
        }



        private async void PopularShowsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (popularShowsListBox.SelectedItem is SpotifyShow selectedShow)
            {
                backButton.Visibility = Visibility.Visible;

                popularShowsListBox.Visibility = Visibility.Collapsed;

                episodesListBox.Visibility = Visibility.Visible;

                var episodes = await spotifyService.GetEpisodesForShow(selectedShow.Id);
                DisplayEpisodes(episodes);
            }
        }


        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            backButton.Visibility = Visibility.Collapsed;

            popularShowsListBox.Visibility = Visibility.Visible;

            episodesListBox.Visibility = Visibility.Collapsed;

            popularShowsListBox.SelectedItem = null;
        }



        private void DisplayEpisodes(List<Episode> episodes)
        {
            episodesListBox.ItemsSource = episodes;
        }





        private async Task LoadRecommendationTracks()
        {
            string seedGenres = "pop,rock,j-pop,k-pop";    
            try
            {
                if (spotifyService == null)
                {
                    MessageBox.Show("SpotifyService nie jest zainicjowany.");
                    return;
                }

                var result = await spotifyService.client.GetAsync($"https://api.spotify.com/v1/recommendations?limit=50&seed_genres={seedGenres}");
                if (result.IsSuccessStatusCode)
                {
                    var content = await result.Content.ReadAsStringAsync();
                    var tracks = ParseRecommendationTracksFromJson(content);
                    UpdateUIWithTracks(tracks);
                }
                else
                {
                    MessageBox.Show("Nie udało się pobrać rekomendowanych traków z Spotify.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Wystąpił błąd: {ex.Message}");
            }
        }



        private List<TrackViewModel> ParseRecommendationTracksFromJson(string json)
        {
            var tracksList = new List<TrackViewModel>();
            using (var document = JsonDocument.Parse(json))
            {
                var tracks = document.RootElement.GetProperty("tracks").EnumerateArray();
                foreach (var track in tracks)
                {
                    var artists = string.Join(", ", track.GetProperty("artists").EnumerateArray().Select(a => a.GetProperty("name").GetString()));
                    var imageUrl = track.GetProperty("album").GetProperty("images").EnumerateArray().FirstOrDefault().GetProperty("url").GetString();
                    var trackViewModel = new TrackViewModel
                    {
                        Id = track.GetProperty("id").GetString(),     
                        Name = track.GetProperty("name").GetString(),
                        Artist = artists,
                        ImageUrl = imageUrl,
                        DurationFormatted = TimeSpan.FromMilliseconds(track.GetProperty("duration_ms").GetInt64()).ToString(@"mm\:ss")
                    };
                    tracksList.Add(trackViewModel);
                }
            }
            return tracksList;
        }




        private void UpdateUIWithTracks(List<TrackViewModel> tracks)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                searchResults.Clear();
                foreach (var track in tracks)
                {
                    searchResults.Add(track);
                }
            });
        }

        protected bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (!Equals(field, newValue))
            {
                field = newValue;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                return true;
            }

            return false;
        }

        private ImageSource imageUrl1;

        public ImageSource ImageUrl { get => imageUrl1; set => SetProperty(ref imageUrl1, value); }

        private string artist;

       




    }









    public class PlaylistTrackTemplateSelector : DataTemplateSelector
    {
        public DataTemplate PlaylistTemplate { get; set; }
        public DataTemplate TrackTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is System.Windows.Controls.Primitives.Track)
                return TrackTemplate;
            else if (item is PlaylistItem)
                return PlaylistTemplate;
            else
                return base.SelectTemplate(item, container);
        }
    }



    public class ToUpperConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string text)
            {
                return text.ToUpper();
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class ProgressToWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is double trackWidth && value is double currentValue && parameter is Slider slider)
            {
                return (currentValue / slider.Maximum) * trackWidth;
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }




    public class TrackViewModel1 : INotifyPropertyChanged
    {
        private string _name;
        private string _artist;
        private string _imageUrl;

        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Artist
        {
            get => _artist;
            set
            {
                if (_artist != value)
                {
                    _artist = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ImageUrl
        {
            get => _imageUrl;
            set
            {
                if (_imageUrl != value)
                {
                    _imageUrl = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }


}
