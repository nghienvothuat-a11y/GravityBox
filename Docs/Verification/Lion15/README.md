# Kiểm chứng bàn 15 — The lion's mouth

Unity 6000.3.19f1, ngày 09/09/2026. `bash Tools/verify.sh` đạt **104/104**, không failure/skipped:

- [EditMode.xml](EditMode.xml): 8/8, kết thúc 03:54:19 UTC.
- [PlayMode.xml](PlayMode.xml): 96/96, từ 03:54:26 tới 03:55:03 UTC.

Bốn trường hợp mới kiểm tra trọng lực 9,81 m/s² và profile bi chung, năm collider phù điêu, va chạm mũi và hai đường giải vòng qua má trái/phải. Trong phép thử va chạm, bi lao về mũi ở 0,45 m/s, dừng tiến tại tâm z=0,02433 m và có vận tốc bật ngược; hỗ trợ thoát chưa hoạt động.

Hai đường giải bắt đầu tại spawn thật. Fixture chỉ gửi ý định xoay tới controller, không ghi pose/velocity bi trên đường đi. Bi đi từ bờm trái tới trán, giữa mắt, vòng ngoài một bên mõm, rồi tới lỗ miệng. Mỗi lượt có một sự kiện thoát hoàn toàn, body vẫn dynamic; reset trả về spawn và xóa trạng thái hỗ trợ. Đây là kiểm chứng đường vật lý khả thi bằng bộ điều khiển test, không phải lượt chơi tay hoặc phép đo độ khó.

Các kiểm tra dùng catalog đã chạy với đủ 15 màn: vỏ kín, selector và chuyển màn, cùng hỗ trợ ở ba hướng lỗ (45 tình huống đoạn cuối). 100 test trước vẫn đạt, gồm lực nước/thủy ngân và các cơ quan của màn cũ. Generate/ContentValidator đạt 15 màn; enum cũ được giữ nguyên.

Bốn [ảnh prefab](../../Images/Level15/README.md) được render bằng Metal ở 796 × 1494 lúc 03:53:48 UTC. Đã xem hình và kiểm tra shader kính không có lỗi; đây là fixture hình ảnh, không phải bằng chứng giải màn.

`bash Tools/build.sh macOS` thành công. Player tại `Builds/macOS/Gravity Box.app`, cập nhật **10:58:52 giờ Việt Nam / 03:58:52 UTC**. Đã mở player mới, chọn màn 15 qua selector, quan sát đúng hình dáng/HUD/bi thép và lưu [ảnh native](../../Images/Level15/MacOSLevel15.png) qua F12 lúc **04:00:07 UTC**. Không có exception/error trong Player.log của lượt khởi chạy đã đọc. Kiểm tra native xác nhận khởi chạy và hiển thị; chưa đánh giá cảm giác chơi tay. Không build APK mới.

[Thiết kế và thông số](../../LEVEL15_LION_HEAD.md).
