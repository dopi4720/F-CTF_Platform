<?php
// Kiểm tra xem người dùng có tải lên tệp không
if ($_SERVER['REQUEST_METHOD'] == 'POST' && isset($_FILES['file'])) {
    $file = $_FILES['file'];

    // Lấy tên tệp và phần mở rộng
    $filename = $file['name'];
    $fileTmp = $file['tmp_name'];
    $fileError = $file['error'];

    // Kiểm tra lỗi tải lên
    if ($fileError === 0) {
        // Chỉ kiểm tra phần mở rộng mà không kiểm tra đúng MIME type
        $fileExt = strtolower(pathinfo($filename, PATHINFO_EXTENSION));


        $newFileName = uniqid('', true) . '.' . $fileExt;
        $fileDestination = 'uploads/' . $newFileName;

        // Lưu tệp vào thư mục uploads
        move_uploaded_file($fileTmp, $fileDestination);

        echo "File uploaded successfully! You can access your file here: <a href='uploads/$newFileName'>Download File</a>";
    } else {
        echo "Error uploading the file.";
    }
} else {
    echo "No file uploaded.";
}
