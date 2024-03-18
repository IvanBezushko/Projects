<?php
// Ustawienia połączenia z bazą danych
$host = "localhost";
$db_user = "root";
$db_password = "";
$db_name = "zamowienia_2";

// Łączenie z bazą danych
$conn = new mysqli($host, $db_user, $db_password, $db_name);

// Sprawdzanie połączenia
if ($conn->connect_error) {
    die("Connection failed: " . $conn->connect_error);
}

// Pobieranie danych z formularza
$nazwisko = $conn->real_escape_string($_POST['nazwisko']);
$telefon = $conn->real_escape_string($_POST['telefon']);
$wiadomosc = $conn->real_escape_string($_POST['wiadomosc']);

// Zapytanie SQL do wstawienia danych
$sql = "INSERT INTO konsultacje (nazwisko, telefon, wiadomosc) VALUES ('$nazwisko', '$telefon', '$wiadomosc')";

// Wykonanie zapytania
if ($conn->query($sql) === TRUE) {
    echo "<div style='font-family: Arial, sans-serif; margin: 20px; padding: 20px; border-radius: 5px; background-color: #e8f5e9; color: #2e7d32;'>" .
            "<h2 style='text-align: center;'>Dziękujemy za zgłoszenie</h2>" .
            "<p style='text-align: center;'>Twoja prośba o konsultację została przyjęta. Skontaktujemy się z Tobą wkrótce.</p>" .
            "<div style='text-align: center;'>" .
            "<a href='index.html' style='display: inline-block; padding: 10px 20px; border: none; border-radius: 5px; background-color: #4CAF50; color: white; text-decoration: none;'>Powrót do strony głównej</a>" .
            "</div>" .
            "</div>";
} else {
    echo "<div style='font-family: Arial, sans-serif; margin: 20px; padding: 20px; border-radius: 5px; background-color: #ffebee; color: #c62828;'>" .
            "<h2 style='text-align: center;'>Błąd!</h2>" .
            "<p style='text-align: center;'>Nie udało się zarejestrować Twojej prośby o konsultację: " . $conn->error . "</p>" .
            "<div style='text-align: center;'>" .
            "<a href='index.html' style='display: inline-block; padding: 10px 20px; border: none; border-radius: 5px; background-color: #f44336; color: white; text-decoration: none;'>Spróbuj ponownie</a>" .
            "</div>" .
            "</div>";
}

// Zamknięcie połączenia
$conn->close();
?>
