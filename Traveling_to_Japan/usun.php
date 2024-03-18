<?php
// Get the order ID from the form submission
$order_id = $_POST['order_id'];

// Connect to the database
$host = 'localhost';
$user = 'root';
$password = '';
$dbname = 'zamowienia_2';

$conn = mysqli_connect($host, $user, $password, $dbname);

// Check for errors
if (!$conn) {
    die('Connection failed: ' . mysqli_connect_error());
}

// Begin a transaction
mysqli_begin_transaction($conn);

// Attempt to delete records from 'klienci' and 'destynacje'
$delete_klienci_query = "DELETE FROM klienci WHERE id_zamowienia = ?";
$delete_destynacje_query = "DELETE FROM destynacje WHERE id_zamowienia = ?";

$stmt_klienci = mysqli_prepare($conn, $delete_klienci_query);
$stmt_destynacje = mysqli_prepare($conn, $delete_destynacje_query);

mysqli_stmt_bind_param($stmt_klienci, "i", $order_id);
mysqli_stmt_bind_param($stmt_destynacje, "i", $order_id);

if (mysqli_stmt_execute($stmt_klienci) && mysqli_stmt_execute($stmt_destynacje)) {
    // If deletion from 'klienci' and 'destynacje' is successful, delete from 'zamowienia'
    $delete_zamowienia_query = "DELETE FROM zamowienia WHERE id_zamowienia = ?";
    $stmt_zamowienia = mysqli_prepare($conn, $delete_zamowienia_query);
    mysqli_stmt_bind_param($stmt_zamowienia, "i", $order_id);

    if (mysqli_stmt_execute($stmt_zamowienia)) {
        // Commit the transaction if all deletions are successful
        mysqli_commit($conn);
        echo '<p class="success">Order and related records deleted successfully</p>';
    } else {
        // Rollback the transaction if deletion from 'zamowienia' fails
        mysqli_rollback($conn);
        echo 'Error deleting order: ' . mysqli_error($conn);
    }
} else {
    // Rollback the transaction if deletion from 'klienci' or 'destynacje' fails
    mysqli_rollback($conn);
    echo 'Error deleting related records: ' . mysqli_error($conn);
}

// Close the statements and the connection
mysqli_stmt_close($stmt_klienci);
mysqli_stmt_close($stmt_destynacje);
mysqli_stmt_close($stmt_zamowienia);
mysqli_close($conn);

// Redirect to index.html
header('Location: index.html');
exit;
?>