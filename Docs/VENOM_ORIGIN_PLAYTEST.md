# Venom Origin — Hướng dẫn test 10 màn

Bản thử nghiệm macOS ngày 15/09/2026. Chạy `Builds/Venom/macOS/Venom.app`. Nội dung mới nằm trong `Assets/_Game/Venom/Campaign/`; Journey và lab cũ vẫn giữ riêng để đối chiếu.

## Điều khiển

- Chạm một điểm trên bề mặt để hướng dẫn sinh vật. Kéo trên vùng chơi để xoay cả hộp quanh tâm, với tốc độ và gia tốc quay có giới hạn. Trọng lực luôn hướng xuống thế giới.
- 07–08 khóa xoay. Riêng nóc của hộp đầu màn 08 nhận chạm trực tiếp để người chơi có thể chỉ đường lên đó từ camera cố định.
- Kính ngoài trong suốt không chặn chọn bề mặt bên trong. Màn 03 chủ động giữ mặt kính trước làm bề mặt nhận lệnh, nên phải xoay để chỉ vào lỗ phía sau. Vật cản và nắp thật vẫn chặn điểm chạm.
- Khi bị tách, chạm một phần hoặc nút “Phần …” để chọn. Chỉ phần được chọn nhận điểm đến mới; phần đang giữ cơ quan tiếp tục nhiệm vụ.
- `1`–`9`, `0`: chọn 01–10. `R`: thử lại. `P`/`Esc`: tạm dừng. `Z`: zoom. Các nút cùng chức năng nằm dưới màn hình.
- Bộ chọn mở sẵn tất cả màn để test, độc lập với quyền mở Collection.

## Các tình huống cần cảm nhận

| Màn | Hành động cần quan sát |
| --- | --- |
| 01 | Nhận điểm chạm, bò, chân bám lệch nhịp, cả đuôi đi qua lỗ |
| 02 | Thân chuyển từ sàn lên vách; vách bịt kín hai đầu. Có thể dùng bề mặt kính lân cận để leo |
| 03 | Mặt trước chặn chọn điểm ở mặt sau; xoay rồi chạm lại. Giữ điểm theo tọa độ hộp khi hộp quay |
| 04 | Trượt/rơi khi gặp vùng mất bám. Chỉ các điểm vòng tránh vùng đó hoặc dùng hướng trọng lực khác |
| 05 | Lật nóc trơn xuống; nghiêng nhẹ và điều chỉnh để sinh vật trượt tới lỗ. Chạm không tạo lực bò trên vật liệu trơn |
| 06 | Sinh vật chỉ trượt/dao động; người chơi đưa lỗ tới nó bằng xoay vỏ cầu |
| 07 | Chạm hộp nhựa để bám, chạm đích để đẩy/kéo. Đẩy kịch kính vẫn kéo ra được. 3 giây không lệnh mới thì buông; pause không ăn thời hạn. Khi đặt đúng bậc dưới vùng bám, sinh vật tiếp nối leo lên và đi tới lỗ |
| 08 | Chỉ lên phần nóc còn bám được, hướng tới mép có vật liệu trơn, căn cú rơi vào vành. Lệnh tới miệng ống được giữ tới lúc bắt vành. Quan sát dòng mô xuyên ống, đuôi đi hết, rồi tụ hình trong khoang bên kia |
| 09 | Nắp trong suốt rơi, va chạm và nằm lại trong hộp. Không biến mất theo góc xoay |
| 10 | Boss không có dòng tutorial hoặc lời giải trong HUD. Kiểm chứng sự phối hợp và luật hợp thể mới |

Mỗi màn dùng chung profile sinh vật. Khoảng cách, hình hộp, vùng bám và cơ quan tạo khác biệt; không có chỉ số nâng cấp làm một màn mất khả năng giải.

## Thắng, thua và Collection

Mọi phần phải hợp thể **trước khi cơ thể bắt đầu thoát qua lỗ cuối**. Một phần ra trước sẽ thua và hiện đúng dòng `bạn phải hợp thể trước khi chui ra`. Không cứu được bằng việc tụ ngoài hộp. Một cơ thể đi đầu trước, đuôi sau vẫn hợp lệ; camera ăn mừng chỉ bắt đầu sau khi toàn bộ vật chất đã thoát.

Hỗ trợ gần miệng chỉ khởi động cho một cơ thể thống nhất khi đường không bị chắn. Khi thân đã chui qua, phần đuôi được kéo tiếp theo cơ thể qua cùng miệng, bằng lực có giới hạn và giữ va chạm. Không xóa hạt hoặc teleport để thắng. Ống ở 08 là chuyển khoang, không ghi nhận thắng/thua do lỗ cuối.

Sau thắng, camera gần, cảnh bị ẩn, sinh vật chọn một trong ba điệu vui trong 4,8 giây. Màn 01–09 tự chuyển. Boss dừng lại ở phần thưởng mở Collection. Nhà có chào/chơi, món ăn và đồ đạc mẫu; chưa có mua bán hoặc trình biên tập nhà. Save mới giữ quyền Collection qua lần mở app sau và không cấp lại phần thưởng khi replay.

## Mô phỏng và animation

- 32 phần tử Rigidbody, mỗi phần 3 g, tổng 96 g, bán kính collider 9 mm. Bước mô phỏng 1/120 giây. Project tắt `Physics.gravity`; runtime áp gia tốc 9,81 m/s² riêng cho mô, đồ vật rời và dao trong pha rơi để không tính trọng lực hai lần. Liên kết mềm, tính dẻo và thay đổi láng giềng tạo khả năng chảy; chỉ cơ quan cắt làm tách cơ thể.
- Bộ vận động chủ động truyền lực qua các điểm bám của cùng cơ thể. Mất bám thì rơi theo trọng lực; vật trơn không nhận lực bò. Vành bắt dùng vùng tiếp xúc của lớp da mềm quanh các phần tử, tối đa 21 mm từ tâm hạt, không kéo từ nóc cách miệng hàng chục centimet.
- Khi cơ thể đi từ kính thường vào vùng trơn, lực giữ giảm theo tỷ lệ dấu chân còn bám: `F_max = khối lượng × 36 m/s² × tỷ lệ bám`. Các hạt nằm trong thân hoặc đang ở trong không khí không bị tính nhầm thành chân mất bám. Đây là tham số mô mềm của prototype, chưa phải số đo sinh học. Nếu lực giữ không đủ chịu trọng lượng trong một khoảng ngắn, các chân còn lại tuột ra, lệnh leo bị ngắt và có 0,45 giây nhả bám để tránh móc lại ngay cùng mép. Người chơi có thể chỉ đường vòng mới sau cú trượt; không mất mô và không phải reset màn.
- Hộp đẩy có khối lượng 180 g và trọng tâm hạ trong phần đế để giảm lật. Nó vẫn có chuyển động, quay, trọng lực và va chạm tự do. Khi thao tác, điểm tì đi theo pose vật lý của hộp; lực phản ứng được truyền về sinh vật.
- Animation đọc điểm bám, ý định, vận tốc, lực tương tác và trạng thái cắt/luồn/tụ. Skin có thêm độ rủ, nén khi tiếp đất, xúc tu chống khi đẩy và căng khi kéo; các phần này không sửa khối lượng hay tự mở cơ quan.
- [Ma trận animation và nguồn nghiên cứu](VENOM_ORIGIN_ANIMATION.md).

Đây là mô hình mô mềm phục vụ gameplay, chưa phải solver chất lỏng bảo toàn thể tích hoặc mô phỏng sinh học chính xác. Chưa kết luận hiệu năng Android/iOS; đợt này không build APK. Art và UI đang ở mức prototype.

## Kiểm chứng và phát triển tiếp

Chạy `bash Tools/verify-venom.sh` để kiểm tra Origin và các suite Venom cũ. Script đăng ký scene Journey trước khi Unity vào Play Mode và phục hồi danh sách 10 scene khi kết thúc; build macOS vẫn chỉ chứa Origin.

`VenomOriginTests` kiểm tra các đường giải bằng lực, lệnh di chuyển và bộ quay thật. Các phép dịch chuyển trực tiếp chỉ xuất hiện trong fixture riêng kiểm tra bộ phát hiện thua; không dùng làm đường giải màn. Test có tình huống không dùng bậc không thắng ở 07, kéo hộp khỏi kính, chọn nóc bằng điểm màn hình, tách–giữ–tụ–thoát ở Boss và mở Nhà.

Kết quả chạy Origin riêng được lưu ở `Artifacts/Venom01/origin-tests.xml`, kết quả hồi quy ở `Artifacts/Venom01/origin-regression.xml`, ảnh khung hình ở `Artifacts/Venom01/OriginFrames/`; đây là dữ liệu QA local, không đưa vào Git. Kiến trúc hiện tại và những phần còn là đề xuất được phân biệt trong [tài liệu kiến trúc](VENOM_LEVEL_ARCHITECTURE.md).

## Kết quả kiểm tra 15/09/2026

- Hồi quy: **88/88 test đạt**, gồm 20 test Origin và 68 test Venom cũ; kết thúc 2026-09-15 10:28:36Z. Test mới xác nhận bám ổn định trên kính thường, leo từ dưới vào dải trơn thì rơi, không tách mô và vẫn qua màn khi nhận đường vòng mới.
- MacOS: Unity 6000.3.19f1 báo `ORIGIN BUILD SUCCESS`, binary mới tại `Builds/Venom/macOS/Venom.app`.
- Bản build chứa 10 scene Origin; danh sách scene tạm cho test đã được phục hồi.
- Kiểm tra trực tiếp macOS từng phát hiện kính bên hông chặn chọn lỗ ở 01. Đã sửa và thêm test chọn điểm màn hình rồi thoát đủ cơ thể; test 03 vẫn bảo đảm mặt trước chặn chọn mặt sau.
- Binary macOS cập nhật lúc 17:28:57 ngày 15/09/2026 đã mở và kiểm tra trực tiếp màn 04: chọn lỗ từ phía dưới vùng trơn, sinh vật rơi xuống sàn và trở về Idle; không bị treo ở mép. Các đường giải còn lại của binary này được kiểm tra tự động, chưa chơi tay lại toàn bộ.
