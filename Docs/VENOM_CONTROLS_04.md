# Venom 04 — Bò khắp sáu mặt

Màn thử điều khiển trong hộp lập phương kính 0,5 m, trống bên trong. Sinh vật có thể bò liên tục từ sàn lên bốn tường và trần. Người chơi xoay **hộp vật lý** quanh tâm hình học để nhìn và tiếp cận mặt cần di chuyển. Lỗ tròn bán kính 35 mm nằm **giữa mặt trần**, đối diện sàn xuất phát; sàn bên dưới đã bịt kín. Sinh vật phải bò lên tường để đến trần; chỉ thắng khi đủ 32/32 hạt vật chất thực sự đi ra ngoài.

## Điều khiển

| Thiết bị | Bò | Xoay hộp |
| --- | --- | --- |
| Cảm ứng | Một ngón giữ–kéo, joystick xuất hiện tại điểm chạm | Hai ngón kéo cùng hướng |
| macOS | Chuột trái giữ–kéo, WASD hoặc phím mũi tên | Chuột phải giữ–kéo |

- Độ lệch joystick điều chỉnh tốc độ, có vùng chết để thả đứng yên dễ dàng.
- Hướng mới được căn theo camera và mặt đang bám. Giữ nguyên hướng khi tới mép sẽ chuyển hướng liên tục qua mép đó. Thả rồi kéo lại để căn hướng theo mặt mới.
- Thả điều khiển: giữ vị trí trên mặt bám, kể cả khi hộp quay. Trên Mac có thể vừa giữ phím di chuyển vừa kéo chuột phải.
- Khi ngón thứ hai chạm xuống, ngừng bò để xoay. Sau khi nhấc một ngón, phải nhấc ngón còn lại trước khi bắt đầu lượt bò mới; tránh vô tình lao đi.
- Nút **ZOOM IN · THEO SINH VẬT** hoặc **Z**: camera tiến gần trong 0,45 giây rồi theo vị trí sinh vật. Nhấn lại **ZOOM OUT · TOÀN HỘP** để trở về góc rộng. Hướng nhìn giữ ổn định, không tự xoay theo các góc tường. Zoom vẫn dùng được khi tạm dừng; thử lại màn sẽ trả về góc rộng.
- R thử lại; P/Esc tạm dừng; 1–4 chuyển màn. Trên HUD có nút tương ứng.
- Camera 3/4, cao 28°. Chế độ toàn hộp có khung hình cố định đủ chứa hộp ở mọi hướng. Mặt kính gần camera mờ hơn; viền mảnh, vòng đánh dấu mặt bám và nhãn mặt giúp định hướng.

## Cơ chế và kiến trúc

- `VenomControlMode.SurfaceCrawl` được thêm cuối enum để giữ nguyên dữ liệu màn 01–03.
- `VenomWallClimb` quản lý bề mặt, lực bám và chuyển góc. Sáu collider thật bao quanh khoang; thứ tự dữ liệu: sàn, trái, phải, trước, sau, trần.
- Đầu dò hỗ trợ dài 35 mm, chỉ tạo lực trên mô nằm sát một mặt hộp. Sinh vật ở giữa khoảng không vẫn rơi tự do. Không dịch chuyển trực tiếp Rigidbody và không vô hiệu hóa collider hạt.
- Tốc độ mục tiêu 0,28 m/s (gấp đôi bản đầu màn 04 để test feeling); điều khiển lực dựa trên vận tốc **tương đối với hộp**, có bù thành phần trọng lực dọc mặt. Lực pháp tuyến mô phỏng khả năng kết dính của sinh vật. Đây là năng lực hư cấu phục vụ gameplay, không phải chất lỏng thụ động.
- Khi ít nhất ba hạt tiếp cận tường theo hướng bò, hướng di chuyển được xoay sang mặt mới. Hạt đầu leo lên; mô phía sau còn chạm mặt cũ được kéo về góc.
- Khi dừng, điểm bám lưu trong tọa độ hộp. Hộp quay bằng Rigidbody kinematic, tối đa 65°/s và gia tốc 280°/s²; trọng lực vẫn hướng xuống thế giới.
- Miệng lỗ ở tọa độ hộp `(0, +0,253, 0)`, hướng ra ngoài là +Y. Gần lỗ và đường không bị chắn, lực bám nhường cho hỗ trợ thoát có bù trọng lực để vật chất chui lên được. Hạt vẫn giữ collider, phải đi qua miệng và ra hết mặt ngoài mới được tính. Vật chất đã thoát được gom bằng lực ở vị trí cách tâm lỗ 10 cm phía ngoài, tránh rơi ngược vào lỗ khi phần còn lại đang chui qua.
- `VenomFollowCamera` kết hợp góc rộng hiện có với camera theo cơ thể, nội suy vị trí và kích thước orthographic; không điều khiển vật lý hay đổi hướng camera. Nút Zoom nằm trong vùng HUD, ngoài vùng kéo di chuyển.
- `VenomGlassVisibility` điều chỉnh độ trong từng mặt bằng MaterialPropertyBlock. Collider luôn tồn tại. Bề mặt sinh vật được giới hạn ở phía trong kính; vùng xuyên lỗ vẫn dùng xử lý xuyên lỗ thật.
- Animation thân, xúc tu và điệu nhảy nhận pháp tuyến bề mặt đang tiếp xúc. Chúng là hình ảnh; lực bám nằm riêng trong locomotion.
- Scene, mesh sàn, kính và profile xoay/bò của màn 04 là asset riêng. Không tái sinh các màn cũ khi xây dựng màn này.

## Phạm vi prototype

Một cơ thể, không máy chém hay công tắc mới; mục tiêu là kiểm tra feeling di chuyển sáu mặt và xoay hộp. Bộ điều khiển đang hỗ trợ sáu mặt phẳng của hộp lập phương, chưa phải bộ bò tùy ý trên mọi mesh cong. Mặt nhìn cạnh có hướng chiếu khó đọc; xoay hộp để nhìn rõ hơn trước khi đổi hướng.

## Kiểm chứng

Đã pass **31/31** bài kiểm tra PlayMode của Venom (26 bài cũ và 5 bài màn 04); build macOS thành công bằng Unity 6000.3.19f1. Đã mở bản native và xác nhận HUD/camera màn 04.

Bản tốc độ **0,28 m/s**, lỗ trần và Zoom follow đã pass **5/5** bài `VenomClimbTests`.

`VenomClimbTests` chạy bước vật lý 120 Hz, dùng lực điều khiển thật:
- Giữ một hướng theo màn hình để bò sàn → tường → trần, giữ trần, rồi xoay hộp khi đang bám.
- Kiểm tra hạt còn trong hộp, cơ thể vẫn nối thành một phần, và vị trí bám tương đối khi quay.
- Vật chất không được bay khi chưa có mặt bám; pause/reset xóa trạng thái phù hợp.
- Chuyển một ngón → hai ngón → nhấc tay không tạo lệnh bò thừa.
- Sàn cũ kín, lỗ mới xuyên giữa trần, đường điều khiển bò lên tường rồi đưa đủ 32 hạt ra ngoài trần.
- Zoom gần từ từ, theo cơ thể di chuyển, giữ hướng nhìn, không thay vị trí hạt; Zoom Out khôi phục đúng góc rộng cả khi pause; reset thoát chế độ follow.

Bài kiểm tra cử chỉ phát lại dữ liệu ngón tay qua cùng bộ xử lý dùng lúc chơi. Cần test feeling bằng tay trên thiết bị cảm ứng trước khi chốt thông số cho mobile.

Build macOS: `bash Tools/build-venom.sh`; ứng dụng `Builds/Venom/macOS/Venom.app`, chọn **04 · BÒ TƯỜNG**. Không xuất APK trong lần thay đổi này.
