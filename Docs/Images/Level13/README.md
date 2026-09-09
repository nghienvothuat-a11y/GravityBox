# Bàn 13 — ảnh kiểm tra nước

Các ảnh được render từ prefab thật trong Unity, với bi bắt đầu tại spawn và tiếp tục mô phỏng vật lý. Fixture chỉ ra lệnh xoay hộp sau khi bi ổn định; không đặt lại pose hoặc vận tốc bi sau spawn. Đây không phải lượt chơi tay.

- [Đứng yên](Water13Still.png): bi thép dưới nước, đáy có caustic và khối cube cố định.
- [Bắt đầu chuyển động](Water13Moving.png): dòng nước và wake xuất hiện khi nghiêng.
- [Đổi hướng hộp](Water13Turned.png): thể tích nước được giữ đầy khi đổi tư thế.
- [Player macOS](Water13Native.png): ảnh F12 thật lúc 02:47:03 UTC ngày 09/09/2026, đã mở màn 13 và reset về spawn. Đây là kiểm tra hiển thị/HUD, không phải lượt hoàn thành bằng tay.

[Metadata](Water13RenderFixtures.json) lưu thời điểm, vị trí bi và số hạt wake đang sống. Caustic/tracer là hiệu ứng quang học xấp xỉ, không phải CFD hoặc khúc xạ ray tracing. Lệnh tạo lại: Unity batchmode có graphics, `-executeMethod GravityBox.Editor.WaterPreviewCapture.Capture`.
