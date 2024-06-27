const rangeInput = document.querySelector('.volume-control input[type="range"]');
if (rangeInput) {
    rangeInput.addEventListener('input', function () {
        this.style.setProperty('--value', `${this.value}%`);
        this.style.setProperty('background-size', `${this.value}% 100%`);
    });
}

function loadContent(url) {
    $.get(url, function (data) {
        const homepageContent = $(data).find('#homepage').html();
        if (homepageContent) {
            $('#homepage').html(homepageContent);
        }
    });
}

$(function () {
    $('#homeButton').on('click', function () {
        loadContent('/Index');
    });

    $('#privacyButton').on('click', function () {
        loadContent('/Privacy');
    });
});

const authorizeButton = document.getElementById('authorizeButton');
if (authorizeButton) {
    authorizeButton.addEventListener('click', function () {
        window.location.href = '/api/backend/authorize';
    });
}





async function loadRandomPlaylists() {
    logMessage("Attempting to load random playlists...");

    try {
        const accessToken = localStorage.getItem('spotifyAccessToken');
        if (!accessToken) {
            throw new Error('No access token found.');
        }

        logMessage("Access token: " + accessToken);

        const response = await fetch('/api/backend/random-playlists', {
            method: 'GET',
            headers: {
                'Authorization': `Bearer ${accessToken}`,
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            }
        });

        if (!response.ok) {
            throw new Error('Failed to load random playlists');
        }

        const playlists = await response.json();
        logMessage("Random Playlists data: " + JSON.stringify(playlists));                   

        const playlistsContainer = document.querySelector('.playlist');
        playlistsContainer.innerHTML = '';          

        playlists.forEach(playlist => {
            const playlistElement = document.createElement('div');
            playlistElement.className = 'random-playlist-item';
            playlistElement.innerHTML = `
                                <img src="${playlist.imageUrl}" alt="${playlist.name}" class="random-playlist-image" />
                                <div class="random-playlist-name">${playlist.name}</div>
                            `;
            playlistsContainer.appendChild(playlistElement);
        });

        logMessage("Random playlists loaded successfully.");
    } catch (error) {
        console.error('Failed to load random playlists:', error);
        logMessage('Failed to load random playlists: ' + error.message);
    }
}

document.addEventListener('DOMContentLoaded', () => {
    logMessage("DOM content loaded. Starting authentication check.");
    handleCallback();
    
    loadRandomPlaylists();
});
