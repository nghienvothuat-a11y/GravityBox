# Tự chuyển màn và âm thanh lăn

Ngày kiểm tra: 09/09/2026.

## Hành vi đã xác minh

- Campaign tự tải màn kế tiếp sau 1,15 giây kể từ khi toàn bộ bi thoát; Physics Lab dùng khoảng chờ 1,8 giây.
- Màn cuối C100 giữ trạng thái hoàn thành và không quay về C001.
- Boss replay giữ việc chuyển màn trong lúc phát và bắt đầu lại khoảng chờ sau khi kết thúc.
- Reset xóa bộ đếm chuyển màn cũ; luật nhiều bi vẫn chỉ hoàn thành sau khi toàn bộ roster thoát.
- Âm thanh lăn lấy vận tốc và tải tiếp xúc làm đầu vào, có ngưỡng tắt rung nhỏ, giới hạn âm lượng/pitch và loop procedural đã lọc/crossfade. Chất lượng nghe cuối cùng vẫn cần đánh giá bằng tai trên bản macOS.

## Kết quả

- [EditMode XML](editmode.xml): 11/11 passed, 0 failed, 0 skipped.
- [PlayMode XML](playmode.xml): 380/380 passed, 0 failed, 0 skipped.
- Tổng: **391/391 passed**.
- macOS Campaign build thành công: 351.510.201 bytes. Player mở ở C002, resume mô phỏng bình thường; log khởi động không có exception/error.

Log thô được giữ ngoài Git; XML là kết quả NUnit do Unity Test Runner tạo.
