<?php
if ($_SERVER["REQUEST_METHOD"] == "POST") {
    // Database connection settings
    $servername = "localhost";
    $username = "root";
    $password = "";
    $dbname = "zamowienia_2";

    try {
        // Create a new PDO instance
        $conn = new PDO("mysql:host=$servername;dbname=$dbname", $username, $password);
        // Set PDO error mode to exception
        $conn->setAttribute(PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION);

        // Sanitize form data
        $imie = htmlspecialchars($_POST['imie']);
        $nazwisko = htmlspecialchars($_POST['nazwisko']);
        $email = htmlspecialchars($_POST['email']);
        $numer_telefonu = !empty($_POST['numer_telefonu']) ? $_POST['numer_telefonu'] : uniqid();
        $ilosc_osob = htmlspecialchars($_POST['ilosc_osob']);
        $destynacja = htmlspecialchars($_POST['destynacja']);
        $klasa = htmlspecialchars($_POST['klasa']);
        $data_wyjazdu = htmlspecialchars($_POST['data_wyjazdu']);

        // Insert data into klienci table
        $stmt_klienci = $conn->prepare("INSERT INTO klienci (imie, nazwisko, email, numer_telefonu) VALUES (:imie, :nazwisko, :email, :numer_telefonu)");
        $stmt_klienci->bindValue(':imie', $imie);
        $stmt_klienci->bindValue(':nazwisko', $nazwisko);
        $stmt_klienci->bindValue(':email', $email);
        $stmt_klienci->bindValue(':numer_telefonu', $numer_telefonu);
        $stmt_klienci->execute();

        // Get the ID of the last inserted row in klienci table
        $id_klienta = $conn->lastInsertId();

        // Insert data into destynacje table
        $stmt_destynacje = $conn->prepare("INSERT INTO destynacje (id_klienta, destynacja) VALUES (:id_klienta, :destynacja)");
        $stmt_destynacje->bindValue(':id_klienta', $id_klienta);
        $stmt_destynacje->bindValue(':destynacja', $destynacja);
        $stmt_destynacje->execute();

        // Get the ID of the last inserted row in destynacje table
        $id_destynacji = $conn->lastInsertId();

        // Insert data into zamowienia table
        $stmt_zamowienia = $conn->prepare("INSERT INTO zamowienia (id_klienta, id_destynacji, klasa, data_wyjazdu, ilosc_osob) VALUES (:id_klienta, :id_destynacji, :klasa, :data_wyjazdu, :ilosc_osob)");
        $stmt_zamowienia->bindValue(':id_klienta', $id_klienta);
        $stmt_zamowienia->bindValue(':id_destynacji', $id_destynacji);
        $stmt_zamowienia->bindValue(':klasa', $klasa);
        $stmt_zamowienia->bindValue(':data_wyjazdu', $data_wyjazdu);
        $stmt_zamowienia->bindValue(':ilosc_osob', $ilosc_osob);
        $stmt_zamowienia->execute();

        // Get the ID of the last inserted row in zamowienia table
        $id_zamowienia = $conn->lastInsertId();

        // Update the id_zamowienia column in klienci table
        $stmt_update_klienci = $conn->prepare("UPDATE klienci SET id_zamowienia = :id_zamowienia WHERE id_klienta = :id_klienta");
        $stmt_update_klienci->bindValue(':id_zamowienia', $id_zamowienia);
        $stmt_update_klienci->bindValue(':id_klienta', $id_klienta);
        $stmt_update_klienci->execute();

        // Update the id_zamowienia column in destynacje table
        $stmt_update_destynacje = $conn->prepare("UPDATE destynacje SET id_zamowienia = :id_zamowienia WHERE id_destynacji = :id_destynacji");
        $stmt_update_destynacje->bindValue(':id_zamowienia', $id_zamowienia);
        $stmt_update_destynacje->bindValue(':id_destynacji', $id_destynacji);
        $stmt_update_destynacje->execute();
        // Display the submitted data
        echo "<div style='" .
        "font-family: Arial, sans-serif; " .
        "margin: 20px; " .
        "padding: 20px; " .
        "border: 1px solid #ddd; " .
        "border-radius: 10px; " .
        "box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1); " .
        "background-color: #f9f9f9; " .
        "transition: all 0.3s ease;'" .
        "onmouseover=\"this.style.boxShadow='0 8px 16px rgba(0, 0, 0, 0.2)'; this.style.transform='scale(1.02)';\" " .
        "onmouseout=\"this.style.boxShadow='0 4px 8px rgba(0, 0, 0, 0.1)'; this.style.transform='scale(1.0)';\">" .
        "<h2 style='color: #333; margin-top: 0;'>Otrzymane dane:</h2>" .
        "<p>Imię: " . $imie . "</p>" .
        "<p>Nazwisko: " . $nazwisko . "</p>" .
        "<p>Email: " . $email . "</p>" .
        "<p>Destynacja: " . $destynacja . "</p>" .
        "<p>Klasa: " . $klasa . "</p>" .
        "<p>Data wyjazdu: " . $data_wyjazdu . "</p>" .
        "<p>Ilość osób: " . $ilosc_osob . "</p>" .
        "<p>ID klienta: " . $id_klienta . "</p>" .
        "<p>ID Zamóienia:". $id_zamowienia ."</p>".
        "</div>";

    } catch (PDOException $e) {
        echo "Błąd połączenia: " . $e->getMessage();
    }

    echo "<div style='text-align: center; margin-top: 20px;'>" . // Kontener dla przycisku
     "<a href='index.html' style='" .
     "display: inline-block; " .
     "padding: 15px 30px; " .
     "background-color: #4CAF50; " .
     "color: white; " .
     "text-align: center; " .
     "text-decoration: none; " .
     "font-size: 18px; " .
     "border-radius: 8px; " .
     "border: none; " .
     "cursor: pointer; " .
     "transition: background-color 0.3s ease, box-shadow 0.3s ease; " .
     "box-shadow: 0 4px 8px rgba(0, 0, 0, 0.2);'" .
     "onmouseover=\"this.style.backgroundColor='#66bb6a'; this.style.boxShadow='0 6px 12px rgba(0, 0, 0, 0.3)';\" " .
     "onmouseout=\"this.style.backgroundColor='#4CAF50'; this.style.boxShadow='0 4px 8px rgba(0, 0, 0, 0.2)';\">" .
     "Wróć do strony głównej</a>" .
     "</div>"; // Koniec kontenera

    // Close the database connection
    $conn = null;
}
?>