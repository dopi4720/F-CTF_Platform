import express from "express";
import path from "path";
import { json } from "stream/consumers";
import { fileURLToPath } from "url";
import morgan from "morgan"; // Import morgan để log request
import { createProxyMiddleware } from "http-proxy-middleware";

const app = express();
const PORT = process.env.PORT || 3000;

// Đường dẫn tĩnh tới thư mục build
const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);
//app.use(express.json()); // Để Express parse JSON trong body
//app.use(express.urlencoded({ extended: true })); // Để parse dữ liệu form

// Middleware để log tất cả các yêu cầu
app.use(morgan("combined")); // Sử dụng format "combined" để log chi tiết

// Serve các tệp tĩnh từ thư mục build
app.use(express.static(path.join(__dirname, "dist")));

app.use("/src/assets", express.static(path.join(__dirname, "src/assets")));

// Endpoint mặc định để xử lý mọi request khác
app.get("/", (req, res) => {
  console.log(`Serving file: ${path.join(__dirname, "dist", "index.html")}`);
  res.sendFile(path.join(__dirname, "dist", "index.html"));
});

// Cấu hình proxy cho yêu cầu đến /api
app.use(
  "/api",
  createProxyMiddleware({
    target: `http://127.0.0.1/8000/api`, // Địa chỉ backend mà bạn muốn proxy yêu cầu tới
    changeOrigin: true, // Thay đổi header "Origin" trong request
    pathRewrite: {
      "^/api": "", // Viết lại URL, nếu cần
    },
    onProxyReq: (proxyReq) => {
      console.log("Proxying request to:", proxyReq.host);
    },
  })
);
// Khởi động server
app.listen(PORT, () => {
  console.log(`Server đang chạy tại http://localhost:${PORT}`);
});
