# Quan sát bi thoát khỏi thủy ngân

Render từ prefab thật lúc **03:36:07 UTC, 09/09/2026**, 796 × 1494. Hộp giữ nguyên góc Z=180° trong cả ba ảnh; sàn opacity 0,12. Đã xem ba ảnh và kiểm tra shader.

- [Đứng ở miệng lỗ](Mercury14Mouth.png): fixture đặt bi ngập một phần, tắt hỗ trợ và chờ cân bằng 3 s; chưa thắng.
- [Đang hút ra](Mercury14Assisting.png): bật motor gameplay, mô phỏng thêm 5 bước 120 Hz. Tâm bi y=0,062288 m, phần sau vẫn chưa vượt hết mặt ngoài nên chưa thắng.
- [Thoát hoàn toàn](Mercury14Released.png): ở t=3,133347 s, tâm bi y=0,074562 m, lệch khỏi trục lỗ và ở ngoài; đã thắng qua kiểm tra full-sphere.
- [Metadata](Mercury14ReleaseFixtures.json): vị trí, opacity, thời gian mô phỏng và trạng thái thoát.

Không đổi pose/vận tốc bi sau khâu chuẩn bị, không xoay hộp để hỗ trợ. Lực hút là tính năng gameplay đang kiểm thử. View bạc trong suốt chỉ giúp quan sát thủy ngân đục. Ảnh là fixture Editor, không phải ảnh native hay lượt chơi từ spawn.

Tạo lại bằng Unity batchmode có graphics: `-executeMethod GravityBox.Editor.WaterPreviewCapture.CaptureMercuryRelease`.

[Ảnh macOS](MacOSLevel14.png) chụp qua F12 lúc 10:40:18 giờ Việt Nam, sau khi mở bản build mới và chọn màn 14. Ảnh này xác nhận HUD/hướng dẫn/bản native; không phải ảnh thao tác thoát.
