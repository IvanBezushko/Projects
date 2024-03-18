<?php
// Sprawdzenie, czy formularz został przesłany
if ($_SERVER["REQUEST_METHOD"] == "POST") {
    // Ustawienia połączenia z bazą danych
    $servername = "localhost";
    $username = "root";
    $password = "";
    $dbname = "zamowienia_2";

    try {
        // Utworzenie połączenia z bazą danych
        $conn = new PDO("mysql:host=$servername;dbname=$dbname", $username, $password);
        // Ustawienie trybu raportowania błędów PDO na wyjątki
        $conn->setAttribute(PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION);

        // Pobranie danych z formularza edycji
        $id_klienta = htmlspecialchars($_POST['id_klienta']);
        $imie = htmlspecialchars($_POST['imie']);
        $nazwisko = htmlspecialchars($_POST['nazwisko']);
        $email = htmlspecialchars($_POST['email']);
        $destynacja = htmlspecialchars($_POST['destynacja']);
        $klasa = htmlspecialchars($_POST['klasa']);
        $data_wyjazdu = htmlspecialchars($_POST['data_wyjazdu']);
        $ilosc_osob = htmlspecialchars($_POST['ilosc_osob']);
        $numer_telefonu = htmlspecialchars($_POST['numer_telefonu']);

        // Sprawdzenie, czy klient o podanym ID istnieje
        $stmt_check_client = $conn->prepare("SELECT * FROM klienci WHERE id_klienta = :id_klienta LIMIT 1");
        $stmt_check_client->bindParam(':id_klienta', $id_klienta);
        $stmt_check_client->execute();
        $client = $stmt_check_client->fetch();

        if (!$client) {
            throw new PDOException("Nie znaleziono klienta o podanym ID.");
        }

        // Aktualizacja rekordu w tabeli klienci
        $stmt_klienci = $conn->prepare("UPDATE klienci SET imie = :imie, nazwisko = :nazwisko, email = :email, numer_telefonu = :numer_telefonu WHERE id_klienta = :id_klienta");
        $stmt_klienci->bindParam(':id_klienta', $id_klienta);
        $stmt_klienci->bindParam(':imie', $imie);
        $stmt_klienci->bindParam(':nazwisko', $nazwisko);
        $stmt_klienci->bindParam(':email', $email);
        $stmt_klienci->bindParam(':numer_telefonu', $numer_telefonu);
        $stmt_klienci->execute();

        

        // Pobranie ID destynacji z bazy danych
        $stmt_destynacje_id = $conn->prepare("SELECT id_destynacji FROM destynacje WHERE destynacja = :destynacja LIMIT 1");
        $stmt_destynacje_id->bindParam(':destynacja', $destynacja);
        $stmt_destynacje_id->execute();
        $id_destynacji = $stmt_destynacje_id->fetchColumn();

        // Aktualizacja rekordu w tabeli destynacje
        $stmt_destynacje = $conn->prepare("UPDATE destynacje SET destynacja = :destynacja WHERE id_klienta = :id_klienta");
        $stmt_destynacje->bindParam(':id_klienta', $id_klienta);
        $stmt_destynacje->bindParam(':destynacja', $destynacja);
        $stmt_destynacje->execute();


        // Sprawdzenie, czy aktualizacja się powiodła
        if ($stmt_destynacje->rowCount() == 0) {
            throw new PDOException("Błąd aktualizacji rekordu w tabeli destynacje.");
        }

        // Pobranie ID zamówienia dla klienta
        $stmt_zamowienia_id = $conn->prepare("SELECT id_zamowienia FROM zamowienia WHERE id_klienta = :id_klienta LIMIT 1");
        $stmt_zamowienia_id->bindParam(':id_klienta', $id_klienta);
        $stmt_zamowienia_id->execute();
        $id_zamowienia = $stmt_zamowienia_id->fetchColumn();

        // Aktualizacja rekordu w tabeli zamowienia
        $stmt_zamowienia = $conn->prepare("UPDATE zamowienia SET klasa = :klasa, data_wyjazdu = :data_wyjazdu, ilosc_osob = :ilosc_osob WHERE id_zamowienia = :id_zamowienia");
        $stmt_zamowienia->bindParam(':id_zamowienia', $id_zamowienia);
        $stmt_zamowienia->bindParam(':klasa', $klasa);
        $stmt_zamowienia->bindParam(':data_wyjazdu', $data_wyjazdu);
        $stmt_zamowienia->bindParam(':ilosc_osob', $ilosc_osob);
        $stmt_zamowienia->execute();


        // Sprawdzenie, czy aktualizacja się powiodła
        if ($stmt_zamowienia->rowCount() == 0) {
            throw new PDOException("Błąd aktualizacji rekordu w tabeli zamowienia.");
        }

        // Wyświetlenie komunikatu o powodzeniu edycji
        echo "<div style='font-family: Arial, sans-serif; margin: 20px; padding: 20px; border-radius: 5px; background-color: #e8f5e9; color: #2e7d32;'>" .
                "<h2 style='text-align: center;'>Zamówienie zaktualizowane</h2>" .
                "<p style='text-align: center;'>Zmiany w zamówieniu zostały zapisane.</p>" .
                "<div style='text-align: center;'>" .
                "<a href='index.html' style='display: inline-block; padding: 10px 20px; border: none; border-radius: 5px; background-color: #4CAF50; color: white; text-decoration: none;'>Powrót do strony głównej</a>" .
                "</div>" .
                "</div>";
    } catch (PDOException $e) {
        // Obsługa błędów połączenia z bazą danych
        if ($conn) {
            echo "Błąd połączenia: " . $e->getMessage() . "<br>";
            $conn = null;
        }

        // Obsługa błędów aktualizacji rekordów
        if (isset($stmt_klienci)) {
            echo "Błąd aktualizacji rekordu w tabeli klienci: " . $e->getMessage() . "<br>";
        }
        if (isset($stmt_destynacje)) {
            echo "Błąd aktualizacji rekordu w tabeli destynacje: " . $e->getMessage() . "<br>";
        }
        if (isset($stmt_zamowienia)) {
            echo "Błąd aktualizacji rekordu w tabeli zamowienia: " . $e->getMessage() . "<br>";
        }
    }

    // Zamknięcie połączenia
    $conn = null;
}
?>