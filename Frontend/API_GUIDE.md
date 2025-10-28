# Hướng Dẫn Sử Dụng API Cho Frontend

Tài liệu này mô tả cách tương tác với các API của backend để quản lý sản phẩm hoa và tích hợp cổng thanh toán Momo.

**Base URL:** `[YOUR_API_DOMAIN]` (Ví dụ: `https://localhost:7000`)

## Mục Lục
1.  [Xác Thực (Authentication)](#xác-thực-authentication)
2.  [Quản Lý Hình Ảnh](#quản-lý-hình-ảnh)
3.  [Quản Lý Hoa (CRUD)](#quản-lý-hoa-crud)
4.  [Tích Hợp Thanh Toán Momo](#tích-hợp-thanh-toán-momo)

---

## Xác Thực (Authentication)

Một số API yêu cầu quyền admin. Để gọi các API này, bạn cần gửi một `Bearer Token` trong header `Authorization`.

**Header:**
```
Authorization: Bearer [YOUR_JWT_TOKEN]
```

---

## Quản Lý Hình Ảnh

Các API này dùng để tải lên, xóa và quản lý hình ảnh của các loài hoa trên Firebase Storage. Yêu cầu quyền **Admin**.

### 1. Tải Lên Hình Ảnh Mới

Sử dụng API này để tải ảnh lên và nhận lại URL. URL này sẽ được dùng khi tạo hoặc cập nhật thông tin hoa.

-   **Endpoint:** `POST /api/Upload/image`
-   **Method:** `POST`
-   **Authorization:** `Admin`
-   **Request:** `multipart/form-data`
    -   Key: `file`
    -   Value: File ảnh cần tải lên.

-   **Success Response (200 OK):**
    -   Nội dung: JSON chứa URL và tên file của ảnh đã tải lên.
    -   **Quan trọng:** Lưu lại `fileName` để có thể xóa hoặc cập nhật ảnh sau này.

    ```json
    {
      "imageUrl": "https://firebasestorage.googleapis.com/...",
      "fileName": "your-image-file-name.jpg"
    }
    ```

### 2. Xóa Hình Ảnh

-   **Endpoint:** `DELETE /api/Upload/image/{fileName}`
-   **Method:** `DELETE`
-   **Authorization:** `Admin`
-   **URL Params:**
    -   `fileName`: Tên file ảnh đã nhận được khi tải lên.

-   **Success Response (200 OK):**
    ```json
    {
      "message": "File deleted successfully"
    }
    ```

---

## Quản Lý Hoa (CRUD)

### Workflow Đề Xuất cho Admin:
1.  **Tải ảnh lên** bằng API `POST /api/Upload/image` để lấy `imageUrl`.
2.  **Tạo hoa mới** bằng API `POST /api/Flower` và gắn `imageUrl` vào body request.

### 1. Lấy Danh Sách Tất Cả Hoa

-   **Endpoint:** `GET /api/Flower`
-   **Method:** `GET`
-   **Authorization:** Không yêu cầu.
-   **Success Response (200 OK):**
    -   Mảng các đối tượng hoa.

    ```json
    [
      {
        "id": 1,
        "name": "Hoa Hồng",
        "description": "Một loài hoa đẹp",
        "imageUrl": "https://...",
        "price": 250000,
        "stock": 100,
        "category": 1
      },
      ...
    ]
    ```

### 2. Lấy Thông Tin Một Bông Hoa

-   **Endpoint:** `GET /api/Flower/{id}`
-   **Method:** `GET`
-   **Authorization:** Không yêu cầu.
-   **Success Response (200 OK):**
    -   Đối tượng hoa tương ứng.

### 3. Tạo Hoa Mới

-   **Endpoint:** `POST /api/Flower`
-   **Method:** `POST`
-   **Authorization:** `Admin`
-   **Request Body:**
    -   Đối tượng JSON chứa đầy đủ thông tin của hoa.

    ```json
    {
      "name": "Hoa Tulip",
      "description": "Hoa từ Hà Lan",
      "imageUrl": "https://... (Lấy từ API Upload)",
      "price": 350000,
      "stock": 50,
      "category": 2
    }
    ```
-   **Success Response (201 Created):**
    -   Trả về đối tượng hoa vừa tạo.

### 4. Cập Nhật Thông Tin Hoa

-   **Endpoint:** `PUT /api/Flower/{id}`
-   **Method:** `PUT`
-   **Authorization:** `Admin`
-   **Request Body:**
    -   Đối tượng JSON chứa thông tin hoa cần cập nhật. **Lưu ý:** `id` trong body phải khớp với `id` trên URL.

    ```json
    {
      "id": 5,
      "name": "Hoa Tulip Vàng",
      "description": "Hoa từ Hà Lan, màu vàng",
      "imageUrl": "https://...",
      "price": 360000,
      "stock": 45,
      "category": 2
    }
    ```
-   **Success Response (204 No Content):**
    -   Không có nội dung trả về.

### 5. Xóa Hoa

-   **Endpoint:** `DELETE /api/Flower/{id}`
-   **Method:** `DELETE`
-   **Authorization:** `Admin`
-   **Success Response (204 No Content):**
    -   Không có nội dung trả về.

### 6. Thay Đổi / Xóa Ảnh Của Hoa

Đây là một điểm quan trọng. Vì tên file ảnh trên server (`fileName`) khác với tên file gốc, backend đã cung cấp một API chuyên dụng để frontend không cần quan tâm đến `fileName` khi muốn xóa ảnh.

#### A. Xóa ảnh khỏi một bông hoa

Sử dụng endpoint này để xóa file ảnh trên Firebase và đồng thời xóa đường dẫn `imageUrl` khỏi thông tin hoa trong database.

-   **Endpoint:** `DELETE /api/Flower/{id}/image`
-   **Method:** `DELETE`
-   **Authorization:** `Admin`
-   **URL Params:**
    -   `id`: ID của bông hoa cần xóa ảnh.
-   **Success Response (200 OK):**
    ```json
    {
        "message": "Image deleted and flower updated successfully."
    }
    ```
-   **Luồng thực hiện của Frontend:**
    1.  Người dùng nhấn nút xóa ảnh cho bông hoa có ID, ví dụ `5`.
    2.  Frontend gọi `DELETE /api/Flower/5/image`.
    3.  Sau khi nhận được response thành công, frontend tải lại dữ liệu của hoa (hoặc cập nhật state) để không hiển thị ảnh nữa.

#### B. Thay đổi ảnh của một bông hoa

1.  **Bước 1: Tải ảnh mới lên**
    -   Gọi `POST /api/Upload/image` với file ảnh mới.
    -   Nhận về `imageUrl` mới.
2.  **Bước 2 (Khuyến khích): Xóa ảnh cũ**
    -   Gọi `DELETE /api/Flower/{id}/image` để xóa file ảnh cũ trên Firebase, tránh lưu file rác.
3.  **Bước 3: Cập nhật hoa với ảnh mới**
    -   Gọi `PUT /api/Flower/{id}`.
    -   Trong body request, đặt giá trị `imageUrl` là `imageUrl` mới nhận được từ Bước 1.

---

## Tích Hợp Thanh Toán Momo

Đây là luồng thanh toán dành cho người dùng đã đăng nhập.

### Luồng Thanh Toán
1.  **Frontend:** Người dùng chọn thanh toán qua Momo. Frontend gửi yêu cầu tạo thanh toán đến backend.
2.  **Backend:** Tạo một yêu cầu đến Momo và trả về một `payUrl`.
3.  **Frontend:** Tự động chuyển hướng người dùng đến `payUrl` của Momo.
4.  **Momo:** Người dùng thực hiện thanh toán trên trang/app của Momo. Sau khi hoàn tất, Momo sẽ chuyển hướng người dùng trở lại `returnUrl` (đã được cấu hình trên backend, thường là một trang trên web của bạn, ví dụ: `https://your-frontend.com/payment/callback`).
5.  **Frontend:** Trang callback (`/payment/callback`) sẽ nhận được một loạt các query parameters từ Momo. **Ngay lập tức**, frontend phải gọi API `PaymentCallBack` của backend và **chuyển tiếp toàn bộ các query parameters này**.
6.  **Backend:** Xác thực chữ ký từ Momo, kiểm tra kết quả giao dịch và cập nhật trạng thái đơn hàng.

### 1. Tạo Yêu Cầu Thanh Toán

-   **Endpoint:** `POST /api/Payment`
-   **Method:** `POST`
-   **Authorization:** Yêu cầu (người dùng đã đăng nhập).
-   **Request Body:**
    -   `orderId`: Mã đơn hàng của bạn.
    -   `amount`: Tổng số tiền cần thanh toán.

    ```json
    {
      "orderId": "123",
      "amount": 50000
    }
    ```
-   **Success Response (200 OK):**
    -   Đối tượng JSON từ Momo chứa `payUrl`. Frontend phải chuyển hướng người dùng đến URL này.

    ```json
    {
      "partnerCode": "MOMO",
      "requestId": "...",
      "orderId": "...",
      "amount": 50000,
      "responseTime": ...,
      "message": "Success",
      "resultCode": 0,
      "payUrl": "https://test-payment.momo.vn/v2/gateway/pay?id=...",
      "deeplink": "momo://..._app",
      "qrCodeUrl": ""
    }
    ```

### 2. Xử Lý Dữ Liệu Trả Về Từ Momo (Callback)

-   **Endpoint:** `GET /api/Payment/PaymentCallBack`
-   **Method:** `GET`
-   **Authorization:** Không yêu cầu.
-   **Hành động của Frontend:**
    1.  Sau khi Momo redirect người dùng về trang của bạn (ví dụ: `https://your-frontend.com/payment/callback?partnerCode=...&orderId=...&resultCode=0...`), trang này phải lấy toàn bộ URL query string.
    2.  Thực hiện gọi API `GET /api/Payment/PaymentCallBack` với toàn bộ query string đó.
    
    **Ví dụ (JavaScript):**
    ```javascript
    // Giả sử bạn đang ở trang /payment/callback
    const queryString = window.location.search; // Lấy "?partnerCode=...&..."
    
    fetch(`[YOUR_API_DOMAIN]/api/Payment/PaymentCallBack${queryString}`)
      .then(response => response.json())
      .then(data => {
        // data = { orderId: "...", message: "Payment successful!" }
        console.log(data.message);
        // Hiển thị thông báo thành công/thất bại cho người dùng
      });
    ```

-   **Success Response (200 OK):**
    -   Backend sẽ trả về kết quả cuối cùng của giao dịch.

    ```json
    {
      "orderId": "...",
      "amount": "50000",
      "orderInfo": "Thanh toan don hang #123",
      "message": "Payment successful!"
    }
    ```
    -   Nếu `message` không phải là "Payment successful!", nghĩa là giao dịch đã thất bại.
