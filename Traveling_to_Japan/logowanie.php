

<?php
// Połączenie zazą dany
$host = "localhost";
$db_user = "root";
$db_password = "";
$db_name = "zamowienia_2";

$conn = new mysqli($host, $db_user, $db_password, $db_name);

// Sprawdzenie połączenia
if ($conn->connect_error) {
    die("Connection failed: " . $conn->connect_error);
}

// Oczyszczenie danych wejściowych
function test_input($data) {
    $data = trim($data);
    $data = stripslashes($data);
    $data = htmlspecialchars($data);
    return $data;
}

$orders_stmt = null;

if ($_SERVER["REQUEST_METHOD"] == "POST") {
    // Tutaj można dodać logikę logowania użytkownika i sprawdzenie hasła

    // Przykładowy kod logowania (proszę dostosować do własnych potrzeb)
    $imie = test_input($_POST['imie']);
    $nazwisko = test_input($_POST['nazwisko']);
    $email = test_input($_POST['email']);
    $haslo = test_input($_POST['haslo']);

    $stmt = $conn->prepare("SELECT * FROM uzytkownicy WHERE email = ?");
    $stmt->bind_param("s", $email);
    $stmt->execute();
    $result = $stmt->get_result();

    if ($result->num_rows > 0) {
        $user = $result->fetch_assoc();
        if (password_verify($haslo, $user['haslo'])) {
            // Użytkownik zalogowany

            // Pobierz dane użytkownika
            $imie = $user['imie'];
            $nazwisko = $user['nazwisko'];
            $email = $user['email'];

            echo "<div class='frame'>";
            echo "<div class='box'>";
            echo "<h2><b>Witamy użytkownika!<b></h2>";
            echo "<p><strong>Imię:</strong> $imie</p>";
            echo "<p><strong>Nazwisko:</strong> $nazwisko</p>";
            echo "<p><strong>Email:</strong> $email</p>";
            echo "</div>";
            echo "</div>";

            // Pobierz dane o zamówieniach użytkownika
            $user_id = $user['id'];

           // Pobierz ostatnie zamówienie użytkownika na podstawie imienia i nazwiska
           $last_order_stmt = $conn->prepare("SELECT zamowienia.data_wyjazdu, zamowienia.ilosc_osob, destynacje.destynacja, zamowienia.klasa
           FROM zamowienia
           JOIN destynacje ON zamowienia.id_destynacji = destynacje.id_destynacji
           JOIN klienci ON zamowienia.id_klienta = klienci.id_klienta
           WHERE klienci.imie = ? AND klienci.nazwisko = ?
           ORDER BY zamowienia.id_zamowienia DESC
           LIMIT 1");
        $last_order_stmt->bind_param("ss", $imie, $nazwisko);
        $last_order_stmt->execute();
        $last_order_result = $last_order_stmt->get_result();

        if ($last_order_result->num_rows > 0) {
        // Wyświetl ostatnie zamówienie użytkownika
        $last_order_data = $last_order_result->fetch_assoc();
        $data_wyjazdu = $last_order_data["data_wyjazdu"];
        $ilosc_osob = $last_order_data["ilosc_osob"];
        $destynacja = $last_order_data["destynacja"];
        $klasa = $last_order_data["klasa"];

        echo "<div class='frame'>";
        echo "<div class='box'>";
        echo "<h2 class='orders-title'><b>Twoje ostatnie zamowienie:</b></h2>";
        echo "<div class='order-box'>";
        echo "<ul class='orders-list'>";
        echo "<li><span class='order-details'><b>Data wyjazdu:</b></span> $data_wyjazdu</li>";
        echo "<li><span class='order-details'><b>Ilość osób:</b></span> $ilosc_osob</li>";
        echo "<li><span class='order-details'><b>Destynacja:</b></span> $destynacja</li>";
        echo "<li><span class='order-details'><b>Klasa:</b></span> $klasa</li>";
        echo "</ul>";
        echo "</div>";
        echo "</div>";
        echo "</div>";
        } else {
        echo '<div style="text-align: center; margin-top: 20px;">Brak zamówień dla tego użytkownika.</div>';
        }
        } else {
        echo '<div style="text-align: center; margin-top: 20px;">Błąd logowania: Nieprawidłowe hasło.</div>';
        }
        } else {
        echo '<div style="text-align: center; margin-top: 20px;">Błąd logowania: Nieprawidłowy email.</div>';
        }

        echo '<div style="text-align: center; margin-top: 20px;">';
        echo '<a href="index.html" style="background-color: #007BFF; color: white; padding: 10px 20px; text-decoration: none; font-size: 16px;">Powrót do strony głównej</a>';
        echo '</div>';

        $stmt->close();
        if ($orders_stmt !== null) {
            // Teraz możesz bezpiecznie wywołać close() na zmiennej $orders_stmt
            $orders_stmt->close();
        }
}

$conn->close();
?>

<style>
.index-link {
    display: block;
    margin-top: 20px;
    text-align: center;
    text-decoration: none;
    color: #fff;
    background-color: #007BFF;
    padding: 10px 20px;
    border-radius: 5px;
    font-size: 16px;
    transition: background-color 0.3s ease-in-out;
}

.index-link:hover {
    background-color: #0056b3;
}

.frame {
    display: flex;
    background-color: #ffffff;
    flex-direction: column;
    justify-content: center;
    align-items: center;
    width: 100%;
    height: auto;
    background-color: #f5f5f5;
    padding: 20px;
    box-sizing: border-box;
}
   /* Styl dla głównego kontenera */
.box {
    width: 35%;
    align-items: center;
    text-align: center;
    background-color: #f0f0f0;
    border: 1px solid #ddd;
    padding: 20px;
    background-color: aquamarine;
    margin: 20px;
    border-radius: 5px;
    box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
}

/* Styl dla tytułów */
h2 {
    font-size: 20px;
    color: #333;
}

/* Styl dla danych użytkownika */
p {
    font-size: 16px;
    color: #555;
    margin: 10px 0;
}

/* Styl dla zamówień */
.orders-title {
    margin-top: 20px;
    font-size: 18px;
    color: #007BFF;
}

.orders-list {
    list-style: none;
    padding: 0;
}

.order-details {
    font-size: 16px;
    color: #333;
}

/* Styl dla poszczególnych elementów zamówienia */
.orders-list li {
    margin-bottom: 10px;
    border-left: 4px solid #007BFF;
    padding-left: 10px;
}

.data-wyjazdu::before {
    content: "Data wyjazdu: ";
    font-weight: bold;
}

.ilosc-osob::before {
    content: "Ilość osób: ";
    font-weight: bold;
}

.destynacja::before {
    content: "Destynacja: ";
    font-weight: bold;
}

.klasa::before {
    content: "Klasa: ";
    font-weight: bold;
}

    </style>