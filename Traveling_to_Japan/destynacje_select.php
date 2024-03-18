<?php

ini_set('display_errors', 1);
ini_set('display_startup_errors', 1);
error_reporting(E_ALL);
// Ustaw połączenie z bazą danych
$servername = "localhost";
$username = "root";
$password = "";
$dbname = "zamowienia_2";

// Połącz z bazą danych
$conn = new mysqli($servername, $username, $password, $dbname);

// Sprawdź połączenie
if ($conn->connect_error) {
    die("Connection failed: " . $conn->connect_error);
}

// Pobierz listę destynacji
$destynacje_html = '';
$result = $conn->query("SELECT id_destynacji, nazwa FROM destynacje");

if ($result) {
    while($row = $result->fetch_assoc()) {
        $destynacje_html .= '<option value="'. htmlspecialchars($row['id_destynacji']) .'">' . htmlspecialchars($row['nazwa']) . '</option>';
    }
    $result->free();
}

// Zamknij połączenie
$conn->close();

// Wyświetl opcje dla select
echo $destynacje_html;
?>
