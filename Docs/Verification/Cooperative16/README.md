# Kiểm chứng luật nhiều bi và bàn 16

Unity 6000.3.19f1, ngày 09/09/2026. `bash Tools/verify.sh` đạt **112/112**, không failure/skipped:

- [EditMode.xml](EditMode.xml): 8/8, kết thúc 04:25:35 UTC.
- [PlayMode.xml](PlayMode.xml): 104/104, từ 04:25:42 tới 04:26:21 UTC.

Tám trường hợp mới kiểm tra:

- Hai thứ tự thoát A→B và B→A: không hoàn thành ở 1/2, không đếm trùng, toàn bộ cầu phải ra ngoài; reset và chuyển lại màn một bi không giữ body/force cũ.
- Pause/reset sau đúng một bi thoát: cả hai bi và trạng thái cửa/nút/bộ đếm được khôi phục.
- Nút không kích hoạt bởi tự trọng khi xoay hộp; B không đi xuyên cửa đóng nếu A chưa giữ nút.
- Hai bi cùng profile truyền động lượng khi va chạm.
- Lượt giải từ cả hai spawn: A nén nút, B qua cửa, B nén nút thứ hai, A được giải phóng và cả hai thoát qua một lỗ. Chỉ gửi rotation intent, không ghi pose/velocity bi trên đường giải. Bi đã ra ngoài vẫn dynamic.
- Xoay/lật qua nhiều góc bằng controller thật: hai bi được giữ trong vỏ tới khi thoát, các body trên ray không phát nổ hoặc rời housing.
- Kiểm tra API trigger cũ với hai mục tiêu: plate chờ occupant cuối rời đi; one-way collision clearance và impulse cooldown riêng từng bi. Đây là fixture API, không phải cơ quan sử dụng trong màn 16.

Các test catalog chạy với đủ 16 màn và 17 bi được authoring, gồm lỗ thoát tại ba góc (51 trường hợp bi/cửa), containment, scene integration, selector, hỗ trợ hút, nước/thủy ngân và reset. Scene integration xác nhận HUD `OUT 1/2`, giữ session Active và không xử thua khi viên đã thoát rơi ra xa, rồi hoàn tất khi bi còn lại ra ngoài.

Fixture lỗ đổi góc trực tiếp đặt toàn bộ body đã tách hierarchy về cùng tư thế; chờ viên trước rời bore rồi chuẩn bị viên sau. Một bi đang trong đường sphere sweep vẫn là vật cản thật. Đường phối hợp từ spawn kiểm tra riêng hành vi trong gameplay bằng controller, không dùng phép đặt pose của fixture lỗ.

Sáu [ảnh mô phỏng liên tục](../../Images/Level16/README.md) được chụp trong Play Mode lúc 04:19:55 UTC, có callback tiếp xúc thật. Mặt kính của nút giúp nhìn thấy bi đang giữ, cửa giữ collider khi di chuyển. Ảnh không phải lượt chơi tay; thời gian policy không đo độ khó với người chơi.

[Thiết kế, kích thước và giới hạn cơ quan](../../LEVEL16_COOPERATIVE.md). Ở mốc kiểm chứng này chỉ xuất macOS; cơ chế hai bi hiện có trong [APK Campaign 100](../Campaign100/Android/README.md).

`bash Tools/build.sh macOS` thành công, player cập nhật **11:27:05 giờ Việt Nam / 04:27:05 UTC** tại `Builds/macOS/Gravity Box.app`. Đã mở bản mới, chọn bàn 16 qua selector, quan sát hai bi cyan/hổ phách, hướng dẫn và `OUT 0/2`, reset rồi lưu [ảnh native](../../Images/Level16/MacOSLevel16.png) qua F12 lúc **04:29:04 UTC**. Player.log của lượt khởi chạy đã đọc không có exception/error. Đây là kiểm tra khởi chạy/hiển thị/reset; chưa xác nhận lượt giải bằng thao tác tay trên player.
