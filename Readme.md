# AutoFarm Core

Project này tập trung xây dựng phần lõi (core) của game AutoFarm, tách biệt khỏi giao diện người dùng (UI) và các hiệu ứng hình ảnh.

## Khái niệm chính

*   **Entity:** Vật thể trên khu đất (ví dụ: cây trồng, công trình).
*   **Item:** Vật phẩm có thể nằm trong kho (inventory) hoặc cửa hàng (store).

## Hướng dẫn sử dụng

*   **Công cụ Farm Game:** Sử dụng thanh công cụ "Farm game" trên Editor để:
    *   Mở trình quản lý dữ liệu CSV (CSV Data Manager).
    *   Tạo dữ liệu mặc định.
*   **Dữ liệu Game:** Xem và chỉnh sửa dữ liệu game tại `Resources/GameData`.
*   **Scene Main:** Chạy scene `Main` để trải nghiệm game.

## Chức năng chính

*   **Điều khiển Plot:** Chuyển đổi giữa các khu đất bằng nút trái/phải.
*   **Thông tin:** Hiển thị cấp độ trang bị (equipment level), số lượng công nhân (available/total).
*   **Trồng trọt & Thu hoạch:**
    *   Nút "Trồng": Mở inventory để chọn loại cây trồng.
    *   Nút "Thu hoạch": Thu hoạch cây trồng.
*   **Cửa hàng (Shop):** Mua vật phẩm, khu đất mới, nâng cấp trang bị, thuê công nhân.
*   **Công nhân tự động:** Công nhân tự động thực hiện các công việc được giao.
*   **Offline Progress:** Khi offline, công nhân và các khu đất vẫn hoạt động. Khi online lại, trạng thái game được đồng bộ.
*   **Điều chỉnh tốc độ game:** Thay đổi giá trị "game speed" trong Inspector của `GameManager`.
*   **Giả lập Offline:** Sử dụng `GameDataManager` (Odin Inspector) để giả lập trạng thái offline ngay trong khi chơi.

## Lưu ý

*   **Entity Customization:** Dễ dàng tùy chỉnh các entity mà không ảnh hưởng đến core game, do các Manager đã xử lý toàn bộ dữ liệu liên quan.
*   **Dữ liệu người chơi:** Dữ liệu người chơi được lưu trữ dưới dạng JSON trong thư mục app data.