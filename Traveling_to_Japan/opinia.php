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
$imie = $conn->real_escape_string($_POST['imie']);
$email = $conn->real_escape_string($_POST['email']);
$opinia = $conn->real_escape_string($_POST['opinia']);

// Zapytanie SQL do wstawienia danych
$sql = "INSERT INTO opinie (imie, email, opinia) VALUES ('$imie', '$email', '$opinia')";

// Wykonanie zapytania
if ($conn->query($sql) === TRUE) {
    echo "<div style='font-family: Arial, sans-serif; margin: 20px; padding: 20px; border-radius: 5px; background-color: #e8f5e9; color: #2e7d32;'>" .
            "<h2 style='text-align: center;'>Dziękujemy!</h2>" .
            "<p style='text-align: center;'>Twoja opinia została dodana.</p>" .
            "<div style='text-align: center;'>" .
            "<a href='index.html' style='display: inline-block; padding: 10px 20px; border: none; border-radius: 5px; background-color: #4CAF50; color: white; text-decoration: none;'>Powrót do strony głównej</a>" .
            "</div>" .
            "</div>";
} else {
    echo "<div style='font-family: Arial, sans-serif; margin: 20px; padding: 20px; border-radius: 5px; background-color: #ffebee; color: #c62828;'>" .
            "<h2 style='text-align: center;'>Błąd!</h2>" .
            "<p style='text-align: center;'>Nie udało się dodać opinii: " . $conn->error . "</p>" .
            "<div style='text-align: center;'>" .
            "<a href='index.html' style='display: inline-block; padding: 10px 20px; border: none; border-radius: 5px; background-color: #f44336; color: white; text-decoration: none;'>Spróbuj ponownie</a>" .
            "</div>" .
            "</div>";
}



// Zamknięcie połączenia
$conn->close();
?>
