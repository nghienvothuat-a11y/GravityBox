# Kiểm chứng hỗ trợ thoát và sàn trong

Unity 6000.3.19f1, 09/09/2026. `bash Tools/verify.sh` đạt **100/100**, không failure/skipped:

- [EditMode.xml](EditMode.xml): 8/8, kết thúc 03:37:50 UTC.
- [PlayMode.xml](PlayMode.xml): 92/92, từ 03:37:56 tới 03:38:31 UTC.

Phạm vi mới:

- Cả 14 prefab thật, ở ba góc root Z=0°/90°/180°: bi bắt đầu đứng yên bên trong miệng lỗ, force provider kéo bi ra hoàn toàn, không teleport hay ghi velocity. 42 trường hợp đều qua, 0,058–0,083 s từ vị trí chuẩn gần tâm lỗ. Đây là phép thử đoạn cuối, không phải 42 lượt giải toàn màn.
- Căn bi lệch trục 29 mm trước khi thoát trong hộp vuông, nước và thủy ngân.
- Bi thủy ngân cân bằng đứng yên ở lỗ hướng lên: bật hỗ trợ, giữ nguyên góc hộp, bi thoát thật; 0,5 s sau vẫn ở ngoài chất lỏng.
- Ngoài phạm vi, tiếp cận từ ngoài, khóa tín hiệu và shutter thật: không kích hoạt. Reset/disable/đổi màn không giữ lực cũ. Không thắng ngay khi bắt đầu hút; chỉ một sự kiện khi toàn bộ cầu ra ngoài; body vẫn dynamic.
- HUD đọc trạng thái motor, hiện hỗ trợ kịp thời và trả hint cũ sau reset.
- Sàn bàn 13/14 đổi opacity 1 → 0,12 → 1 khi lật rồi reset; collider, shared material và số thành phần vật lý không đổi.

89 kiểm tra cũ vẫn đạt. Test cân bằng thủy ngân riêng tắt `AssistEnabled` để đo lực tự nhiên; gameplay mặc định có hỗ trợ theo yêu cầu mới. Các phép đo mật độ, cản và lực nổi không dùng motor để che sai số.

Ba [ảnh render](../../Images/Level14/ExitAssist/README.md) đã được xem: bi nổi lưng chừng, bi đang được đẩy qua lỗ nhưng chưa thắng, bi đã ở hoàn toàn bên ngoài. Cả ba giữ nguyên hộp lật ngược và floor opacity 0,12. Không có lỗi shader trong lượt render. Đây là fixture có chuẩn bị trạng thái ban đầu, không phải lượt chơi tay.

Chỉ xuất macOS theo yêu cầu hiện tại. [Thiết kế/thông số](../../EXIT_ASSIST.md). Hỗ trợ thoát là lực gameplay có chủ ý; các giới hạn của mô hình chất lỏng vẫn giữ nguyên.

Build macOS thành công, player cập nhật **10:39:04 giờ Việt Nam** (03:39:04 UTC), tại `Builds/macOS/Gravity Box.app`. Đã mở player mới, dùng selector chọn màn 14, kiểm tra hướng dẫn có exit assist và bi nổi. Ảnh native chỉ xác nhận bản mới khởi chạy; việc thoát ở tư thế lật được kiểm tra bằng fixture/test ở trên, không gán thành lượt chơi tay.
