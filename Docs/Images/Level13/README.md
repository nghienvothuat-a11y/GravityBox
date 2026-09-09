# Bàn 13 — ảnh kiểm tra nước

## Bản hiệu chỉnh cản nước

Render ngày 09/09/2026 lúc 03:01:12 UTC, cùng fixture và trình tự xoay. Đã xem cả ba ảnh; không có lỗi shader. VFX giữ cách dựng cũ nhưng chuyển động dùng mô hình cản lăn/added mass mới. Ở góc cuối, cube có thể che bi; đây không phải bằng chứng bi đã ra khỏi hộp.

- [Đứng yên](Refined/Water13Still.png).
- [Bắt đầu lăn](Refined/Water13Moving.png).
- [Đổi hướng](Refined/Water13Turned.png).
- [Metadata hiệu chỉnh](Refined/Water13RenderFixtures.json): 0/7/41 wake ở ba thời điểm.
- [Player macOS hiệu chỉnh](Refined/Water13Native.png): F12 lúc 03:03:12 UTC ngày 09/09/2026, sau khi mở bàn 13 và reset. Xác minh hiển thị/HUD, chưa phải lượt giải bằng tay.

## Bản nước đầu — lưu đối chiếu

Các ảnh được render từ prefab thật trong Unity, với bi bắt đầu tại spawn và tiếp tục mô phỏng vật lý. Fixture chỉ ra lệnh xoay hộp sau khi bi ổn định; không đặt lại pose hoặc vận tốc bi sau spawn. Đây không phải lượt chơi tay.

- [Đứng yên](Water13Still.png): bi thép dưới nước, đáy có caustic và khối cube cố định.
- [Bắt đầu chuyển động](Water13Moving.png): dòng nước và wake xuất hiện khi nghiêng.
- [Đổi hướng hộp](Water13Turned.png): thể tích nước được giữ đầy khi đổi tư thế.
- [Player macOS](Water13Native.png): ảnh F12 thật lúc 02:47:03 UTC ngày 09/09/2026, đã mở màn 13 và reset về spawn. Đây là kiểm tra hiển thị/HUD, không phải lượt hoàn thành bằng tay.

[Metadata](Water13RenderFixtures.json) lưu thời điểm, vị trí bi và số hạt wake đang sống. Caustic/tracer là hiệu ứng quang học xấp xỉ, không phải CFD hoặc khúc xạ ray tracing. Lệnh tạo lại: Unity batchmode có graphics, `-executeMethod GravityBox.Editor.WaterPreviewCapture.Capture`.
