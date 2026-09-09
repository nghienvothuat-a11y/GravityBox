# Kiểm chứng bàn 13 — nước

Unity 6000.3.19f1, ngày 09/09/2026 theo UTC. Bộ kiểm tra cuối đạt **77/77**, không failure hoặc skipped:

- [EditMode.xml](EditMode.xml): 8/8, kết thúc 02:43:21 UTC.
- [PlayMode.xml](PlayMode.xml): 69/69, từ 02:43:28 đến 02:44:00 UTC.

Sáu phép thử riêng: lực nổi và cùng hình học bàn 02; giảm tốc theo nghiệm lực cản; tải đỡ ổn định; thoát nước trở lại gravity thế giới; xoay từ spawn tới lỗ; flow/wake/pause/reset/unload.

Kết quả lực nước: lực nổi 0,138436 N; gia tốc chìm ban đầu −8,56257 m/s². Tải đỡ ở trạng thái nghỉ 0,95025 N và không có va chạm lặp. Thử coasting 1 m/s còn 0,41510 m/s sau một giây, so với nghiệm liên tục 0,41688 m/s. Route từ spawn vòng qua cube rồi ra lỗ chỉ bằng xoay: 0,908 s tới waypoint đầu, tiếp 1,083 s tới thoát. Đây là controller kiểm thử, không phải thời gian chơi tay.

Render có graphics đạt ba góc; shader không có lỗi. [Ảnh và metadata](../../Images/Level13/README.md) ghi chuyển động vật lý liên tục từ spawn, với 0/8/42 hạt wake ở ba thời điểm. Fixture không lưu thay đổi vào Gameplay scene.

Lượt kiểm tra chung đầu tiên tìm thấy test chọn màn 11 giả định thẻ đó vẫn nằm trong viewport khi cuộn tới cuối. Thêm hàng màn 13 làm giả định này sai; fixture đã cuộn đúng thẻ vào vùng nhìn trước khi gửi mouse click. Không đổi hành vi selector của player để vượt test.

Các bằng chứng cũ ở thư mục Verification cha ghi nhận riêng phiên bản mê cung cầu trước khi thêm nước, không phải bộ test của bàn 13. Mức độ giống nước khi chơi tay và hiệu năng Android cần kiểm tra trên thiết bị thật.

## Build và native

macOS development player và Android ARM64/IL2CPP đều build thành công. Player macOS đã mở màn 13, reset và chụp [ảnh native](../../Images/Level13/Water13Native.png) lúc 02:47:03 UTC. Chưa xác minh trên thiết bị Android hoặc xuất lại iOS.

APK `Builds/Android/GravityBox-Level13-Water.apk` là bản sao chính xác của output `GravityBox.apk`, **59.217.936 bytes**, build lúc 09:46:20 ngày 09/09/2026 theo giờ Việt Nam. ZIP integrity check đạt. SHA-256: `918f759ee5cd9902e1823b8d25c02e87368ae0f88050b5e373bf3aeeb7aff1c7`.
