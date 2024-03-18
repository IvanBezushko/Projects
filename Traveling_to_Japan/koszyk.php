<?php
// Sprawdzanie, czy formularz został wysłany
if ($_SERVER["REQUEST_METHOD"] == "POST") {
    // Ustawienia połączenia z bazą danych
    $host = 'localhost';
    $username = 'root';
    $password = '';
    $dbname = 'zamowienia_2';

    // Łączenie z bazą danych
    $conn = new mysqli($host, $username, $password, $dbname);

    // Sprawdzanie połączenia
    if ($conn->connect_error) {
        die("Connection failed: " . $conn->connect_error);
    }

// Pobieranie danych z formularza
$imie = $conn->real_escape_string($_POST['imie']);
$nazwisko = $conn->real_escape_string($_POST['nazwisko']);
$email = $conn->real_escape_string($_POST['email']); // Pobranie email

// Pobieranie ilości każdego produktu
$hoodie = isset($_POST['hoodie']) ? (int)$_POST['hoodie'] : 0;
$jukata = isset($_POST['jukata']) ? (int)$_POST['jukata'] : 0;
$panama = isset($_POST['panama']) ? (int)$_POST['panama'] : 0;
$dlugopis = isset($_POST['dlugopis']) ? (int)$_POST['dlugopis'] : 0;
$kubek = isset($_POST['kubek']) ? (int)$_POST['kubek'] : 0;

// Przygotowanie i wykonanie zapytania SQL
$sql = "INSERT INTO koszyk (imie, nazwisko, email, hoodie, jukata, panama, dlugopis, kubek) VALUES (?, ?, ?, ?, ?, ?, ?, ?)";
$stmt = $conn->prepare($sql);

if ($stmt === false) {
    die("Error preparing statement: " . $conn->error);
}

$stmt->bind_param("sssiiiii", $imie, $nazwisko, $email, $hoodie, $jukata, $panama, $dlugopis, $kubek);

if ($stmt->execute()) {
    echo "Zamówienie zostało zapisane!";
} else {
    echo "Błąd podczas zapisywania zamówienia: " . $stmt->error;
}

$stmt->close();
$conn->close();
// Przekierowanie po zapisaniu zamówienia
header("Location: index.html"); // Zmień na odpowiednią stronę
exit;
} else {
// Opcjonalnie: wyświetlenie wiadomości błędu lub przekierowanie
echo "Formularz nie został poprawnie wysłany.";
// Możesz tutaj dodać przekierowanie, jeśli to konieczne
}
?>
