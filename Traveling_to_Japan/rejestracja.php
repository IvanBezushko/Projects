<?php
if ($_SERVER["REQUEST_METHOD"] == "POST") {
    // Pobierz dane z formularza
    $imie = $_POST['imie'];
    $nazwisko = $_POST['nazwisko'];
    $email = $_POST['email'];
    $haslo = $_POST['haslo']; // Pamiętaj, aby zahashować hasło przed zapisaniem!

    // Przygotuj połączenie z bazą danych
    $servername = "localhost";
    $username = "root";
    $password = "";
    $dbname = "zamowienia_2";

    try {
        $conn = new PDO("mysql:host=$servername;dbname=$dbname", $username, $password);
        // Ustaw tryb błędu PDO na wyjątek
        $conn->setAttribute(PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION);

        // Sprawdź, czy użytkownik już istnieje w bazie danych
        $stmt = $conn->prepare("SELECT id FROM uzytkownicy WHERE email = ?");
        $stmt->execute([$email]);
        $existing_user = $stmt->fetch(PDO::FETCH_ASSOC);

        if ($existing_user) {
            echo '<div style="background-color: #FF0000; color: white; padding: 10px; text-align: center; font-size: 18px;">Użytkownik o podanym adresie email już istnieje!</div>';
        } else {
            // Wstaw nowego użytkownika do bazy danych
            $stmt = $conn->prepare("INSERT INTO uzytkownicy (imie, nazwisko, email, haslo) VALUES (?, ?, ?, ?)");
            $stmt->execute([$imie, $nazwisko, $email, password_hash($haslo, PASSWORD_DEFAULT)]);

            // Przekierowanie do strony "logowanie.html" po zakończeniu rejestracji
            header("Location: logowanie.html");
            exit();
        }
    } catch(PDOException $e) {
        echo "Błąd: " . $e->getMessage();
    }

    $conn = null;
}

// Przycisk "Powrót do strony głównej" zostaje wyświetlony niezależnie od warunków powyżej
echo '<div style="text-align: center; margin-top: 20px;">';
echo '<a href="index.html" style="background-color: #007BFF; color: white; padding: 10px 20px; text-decoration: none; font-size: 16px;">Powrót do strony głównej</a>';
echo '</div>';
?>