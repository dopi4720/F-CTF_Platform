<!DOCTYPE html>
<html lang="en">

<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>File Upload Challenge</title>
</head>

<body>
    <h1>Welcome to the File Upload Challenge</h1>
    <form action="upload.php" method="POST" enctype="multipart/form-data">
        <label for="file">Select a file to upload:</label>
        <input type="file" name="file" id="file">
        <input type="submit" value="Upload">
    </form>
</body>

</html>