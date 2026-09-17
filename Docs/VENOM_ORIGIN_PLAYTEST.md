# Venom Origin — Hướng dẫn test 10 màn

Bản thử nghiệm macOS cập nhật ngày 16/09/2026. Chạy `Builds/Venom/macOS/Venom.app`. Nội dung mới nằm trong `Assets/_Game/Venom/Campaign/`; Journey và lab cũ vẫn giữ riêng để đối chiếu.

## Điều khiển

- Chạm một điểm trên bề mặt để hướng dẫn sinh vật. Kéo trên vùng chơi để xoay cả hộp quanh tâm, với tốc độ và gia tốc quay có giới hạn. Trọng lực luôn hướng xuống thế giới.
- 07, 08 và 10 khóa xoay. Riêng nóc của hộp đầu màn 08 nhận chạm trực tiếp để người chơi có thể chỉ đường lên đó từ camera cố định.
- Camera 07 nhìn 3/4 từ phía trái, nhìn xuống 27°, yaw 38°, khung hình gần hơn khoảng 5%. Góc này nhìn vào mặt thùng nơi sinh vật bám để đẩy/kéo, đồng thời thấy nóc thùng và mặt trong của vách có lỗ. Camera giữ nguyên hướng trong lúc thao tác để điểm chạm ổn định.
- Camera 08 nhìn 3/4 từ bên trái, góc nhìn xuống 30°, yaw 42°, khung hình gần hơn khoảng 10%. Nóc, vành bám/miệng ống và lỗ cuối cùng đều chọn được từ góc cố định này; nóc không chắn đường chọn miệng ống.
- Kính ngoài trong suốt không chặn chọn bề mặt bên trong. Màn 03 chủ động giữ mặt kính trước làm bề mặt nhận lệnh, nên phải xoay để chỉ vào lỗ phía sau. Vật cản và nắp thật vẫn chặn điểm chạm.
- Khi bị tách, chạm một phần hoặc nút “Phần …” để chọn. Chỉ phần được chọn nhận điểm đến mới; phần đang giữ cơ quan tiếp tục nhiệm vụ.
- `1`–`9`, `0`: chọn 01–10. `R`: thử lại. `P`/`Esc`: tạm dừng. `Z`: zoom. Các nút cùng chức năng nằm dưới màn hình.
- Bộ chọn mở sẵn tất cả màn để test, độc lập với quyền mở Collection.

## Các tình huống cần cảm nhận

| Màn | Hành động cần quan sát |
| --- | --- |
| 01 | Nhận điểm chạm, bò, chân bám lệch nhịp, cả đuôi đi qua lỗ |
| 02 | Thân chuyển từ sàn lên vách; vách bịt kín hai đầu. Đi qua rồi chỉ quay lại, hoặc đổi hướng khi đang ôm mép: cả đuôi phải qua được, không mắc vào kính. Có thể dùng bề mặt kính lân cận để leo |
| 03 | Mặt trước chặn chọn điểm ở mặt sau; xoay rồi chạm lại. Giữ điểm theo tọa độ hộp khi hộp quay |
| 04 | Trượt/rơi khi gặp vùng mất bám. Chỉ các điểm vòng tránh vùng đó hoặc dùng hướng trọng lực khác |
| 05 | Lật nóc trơn xuống; nghiêng nhẹ và điều chỉnh để sinh vật trượt tới lỗ. Chạm không tạo lực bò trên vật liệu trơn |
| 06 | Chạm vị trí trên mặt cầu: sinh vật quẫy thân và cố bò tại chỗ, xúc tu trượt rồi nhả vì không bám được. Điểm chỉ quay cùng vỏ cầu; thân vẫn trượt/dao động theo trọng lực và quán tính. Xoay đưa lỗ tới sinh vật để thoát |
| 07 | Chạm hộp nhựa để bám, chạm đích để đẩy/kéo. Đẩy kịch kính vẫn kéo ra được. 3 giây không lệnh mới thì buông; pause không ăn thời hạn. Khi đặt đúng bậc dưới vùng bám, sinh vật tiếp nối leo lên và đi tới lỗ |
| 08 | Chỉ lên phần nóc còn bám được, hướng tới mép có vật liệu trơn, căn cú rơi vào vành. Sinh vật bắt vành, hãm lại và chờ chỉ vào ống; nếu đã có lệnh chui ống thì giữ lệnh tới khi ổn định. Không cần chạm lần hai đúng khoảnh khắc đang rơi. Quan sát dòng mô xuyên ống, đuôi đi hết, rồi tụ hình trong khoang bên kia |
| 09 | Nắp trong suốt rơi, va chạm và nằm lại trong hộp. Không biến mất theo góc xoay |
| 10 | Hộp khóa xoay. Dao thép trên cao báo 1 giây sau khi mô vào vùng cảm biến; thử đổi vị trí hoặc né ra trong lúc báo. Dao phải cắt theo vị trí lúc rơi, nâng về vị trí chờ và không chém lặp khi còn mô trong vùng. Nút A/B lún và đổi đèn khi có tải; nhả lên khi rời. Boss không có tutorial; kiểm chứng phối hợp, hợp thể và Collection |

Mỗi màn dùng chung profile sinh vật. Khoảng cách, hình hộp, vùng bám và cơ quan tạo khác biệt; không có chỉ số nâng cấp làm một màn mất khả năng giải.

## Nhận diện vật liệu

- Màn 02: thanh nhựa cứng trong màu hổ phách, thấy được độ dày và mép; khác kính hộp.
- Vật liệu trơn: tím lavender có vân satin, gồm mảng, nóc và cầu. Vành bám vẫn trong và đúng kích thước vật lý.
- Màn 08: ống cyan trong, vòng nối sứ và mép kim loại xanh; nhìn được dòng mô qua lòng ống.
- Boss: dao thép có cạnh mài, bốn đèn báo thời gian và vùng cảm biến; nút tròn hổ phách có chữ A/B, đế tối và viền sứ. Không dùng chữ mô tả lời giải.

## Thắng, thua và Collection

Mọi phần phải hợp thể **trước khi cơ thể bắt đầu thoát qua lỗ cuối**. Một phần ra trước sẽ thua và hiện đúng dòng `bạn phải hợp thể trước khi chui ra`. Không cứu được bằng việc tụ ngoài hộp. Một cơ thể đi đầu trước, đuôi sau vẫn hợp lệ; camera ăn mừng chỉ bắt đầu sau khi toàn bộ vật chất đã thoát.

**Hợp thể tự động theo khoảng cách:** mô hai phần đủ gần và không bị vật cản ngăn thì nối lại, không phụ thuộc trạng thái di chuyển/Idle/giữ nút hoặc điểm đích. Không có thời gian khóa hợp thể sau chém ở Origin; khi lưỡi dao còn nằm giữa hai phần, chính vùng cắt ngăn chúng nối xuyên dao. Nếu đưa hai phần gặp nhau rồi để dao nâng khỏi chỗ tiếp xúc, chúng có thể nhập lại ngay. Lệnh còn hiệu lực mới nhất được giữ sau hợp thể; khối lượng không đổi.

Cú chém hất hai phần sang hai bên bằng lực vật lý để khoảng cách giữa mô lớn hơn ngưỡng hợp thể. Tốc độ tương đối bổ sung tối đa 0,8 m/s, giới hạn mỗi bên 0,6 m/s; chia theo khối lượng để tổng động lượng của cú hất bằng 0. Không thêm vận tốc theo chiều đứng hoặc teleport. Chơi thử: để yên sau chém phải thấy hai phần tách biệt; dẫn chúng lại gần phải nhập ngay, kể cả một phần còn giữ nút.

Hỗ trợ gần miệng chỉ khởi động cho một cơ thể thống nhất khi đường không bị chắn. Khi thân đã chui qua, phần đuôi được kéo tiếp theo cơ thể qua cùng miệng, bằng lực có giới hạn và giữ va chạm. Không xóa hạt hoặc teleport để thắng. Ống ở 08 là chuyển khoang, không ghi nhận thắng/thua do lỗ cuối.

Sau thắng, camera gần, cảnh bị ẩn, sinh vật chọn một trong ba điệu vui trong 4,8 giây. Màn 01–09 tự chuyển. Boss dừng lại ở phần thưởng mở Collection. Nhà có chào/chơi, món ăn và đồ đạc mẫu; chưa có mua bán hoặc trình biên tập nhà. Save mới giữ quyền Collection qua lần mở app sau và không cấp lại phần thưởng khi replay.

## Mô phỏng và animation

- 32 phần tử Rigidbody, mỗi phần 3 g, tổng 96 g, bán kính collider 9 mm. Bước mô phỏng 1/120 giây. Project tắt `Physics.gravity`; runtime áp gia tốc 9,81 m/s² riêng cho mô, đồ vật rời và dao trong pha rơi để không tính trọng lực hai lần. Liên kết mềm, tính dẻo và thay đổi láng giềng tạo khả năng chảy; chỉ cơ quan cắt làm tách cơ thể.
- Tốc độ mục tiêu bò/leo: **0,126 m/s**, tăng 20% từ 0,105 m/s; sinh vật giảm tốc khi tới gần điểm được chỉ.
- Bộ vận động chủ động truyền lực qua các điểm bám của cùng cơ thể. Mất bám thì rơi theo trọng lực; vật trơn không nhận lực bò. Vành bắt dùng vùng tiếp xúc của lớp da mềm quanh các phần tử, tối đa 21 mm từ tâm hạt, không kéo từ nóc cách miệng hàng chục centimet.
- Ở mép vách, bộ vận động xét đủ 32 phần tử và mở rộng đoạn ôm mép theo độ dày thực của cơ thể cùng bán kính collider. Cả thân được dẫn vòng qua mép rồi mới bò xuống phía bên kia, tránh để đuôi ở một bên còn đầu kéo sang bên đối diện. Lệnh đến đích chỉ kết thúc khi đường từ toàn bộ cơ thể tới đích không bị vật cản chắn. Giữ nguyên độ mềm, lực vận động và collider; không teleport, tách mô hoặc thu nhỏ va chạm để vượt kính.
- Khi cơ thể đi từ kính thường vào vùng trơn, lực giữ giảm theo tỷ lệ dấu chân còn bám: `F_max = khối lượng × 36 m/s² × tỷ lệ bám`. Các hạt nằm trong thân hoặc đang ở trong không khí không bị tính nhầm thành chân mất bám. Đây là tham số mô mềm của prototype, chưa phải số đo sinh học. Nếu lực giữ không đủ chịu trọng lượng trong một khoảng ngắn, các chân còn lại tuột ra, lệnh leo bị ngắt và có 0,45 giây nhả bám **chỉ trên những mặt vừa tuột** để tránh móc lại ngay cùng mép. Chạm một mặt bám mới có thể bám ngay; không khóa toàn cơ thể trong lúc đang rơi.
- Vành ống có phản xạ bắt rơi sau ít nhất hai điểm tiếp xúc da thực. Pha giữ điểm bám và hãm kéo dài tối đa 0,55 giây, giới hạn tầm với 0,15 m từ điểm đã chạm và giữ giới hạn lực vận động hiện có. Hai xúc tu biểu diễn chỗ bám trong lúc hãm, HUD hiện “Bám vành ống”. Sau khi ổn định, lực bám thường tiếp quản; luồng chui ống chỉ tiếp quản khi tốc độ đã giảm. Không mở rộng vùng bắt từ xa hoặc teleport sinh vật.
- Hộp đẩy có khối lượng 180 g và trọng tâm hạ trong phần đế để giảm lật. Nó vẫn có chuyển động, quay, trọng lực và va chạm tự do. Khi thao tác, điểm tì đi theo pose vật lý của hộp; lực phản ứng được truyền về sinh vật.
- Màn 06 nhận chạm vào mặt cầu gần người xem. Tiếp xúc và normal được tính theo mặt cầu cong; chạm không tạo lực bò, lực bám hay triệt tiêu trọng lực. Animation đọc ý định độc lập với lực: xúc tu thử bám rồi trượt trên mặt cong, thân có nếp chuyển động tại chỗ và độ trễ theo vận tốc tương đối của vỏ. Khi không tiếp xúc vỏ, chân rút lại.
- Animation đọc điểm bám, ý định, vận tốc, lực tương tác và trạng thái cắt/luồn/tụ. Skin có thêm độ rủ, nén khi tiếp đất, xúc tu chống khi đẩy và căng khi kéo; các phần này không sửa khối lượng hay tự mở cơ quan.
- [Ma trận animation và nguồn nghiên cứu](VENOM_ORIGIN_ANIMATION.md).

Đây là mô hình mô mềm phục vụ gameplay, chưa phải solver chất lỏng bảo toàn thể tích hoặc mô phỏng sinh học chính xác. Chưa kết luận hiệu năng Android/iOS; đợt này không build APK. Art và UI đang ở mức prototype.

## Kiểm chứng và phát triển tiếp

Chạy `bash Tools/verify-venom.sh` để kiểm tra Origin và các suite Venom cũ. Script đăng ký scene Journey trước khi Unity vào Play Mode và phục hồi danh sách 10 scene khi kết thúc; build macOS vẫn chỉ chứa Origin.

`VenomOriginTests` kiểm tra các đường giải bằng lực, lệnh di chuyển và bộ quay thật. Các phép dịch chuyển trực tiếp chỉ xuất hiện trong fixture riêng kiểm tra bộ phát hiện thua; không dùng làm đường giải màn. Test có tình huống không dùng bậc không thắng ở 07, kéo hộp khỏi kính, chọn nóc bằng điểm màn hình, tách–giữ–tụ–thoát ở Boss và mở Nhà.

Kết quả chạy Origin riêng được lưu ở `Artifacts/Venom01/origin-tests.xml`, kết quả hồi quy ở `Artifacts/Venom01/origin-regression.xml`, ảnh khung hình ở `Artifacts/Venom01/OriginFrames/`; đây là dữ liệu QA local, không đưa vào Git. Kiến trúc hiện tại và những phần còn là đề xuất được phân biệt trong [tài liệu kiến trúc](VENOM_LEVEL_ARCHITECTURE.md).

## Hợp thể theo khoảng cách và cú hất sau chém — 16/09/2026

- **35/35 kiểm thử Origin đạt**, kết thúc 2026-09-16 05:33:24Z. Kiểm tra mới: cú hất giữ hai phần tách biệt khi không ra lệnh, khoảng cách mô vượt ngưỡng tự nối, không văng lên/ra vỏ; dẫn lại gần thì hợp thể. Ba tình huống nhập khi đang giữ nút, Idle và hai đích khác nhau đều đạt; giữ lệnh mới nhất sau nhập, mở cửa và thoát hợp lệ.
- Fixture tiếp xúc riêng kiểm tra phần xa không nhập, phần gần không nhập xuyên vách, và phần gần không bị ngăn thì nhập ngay trong tick mô phỏng kế tiếp, không chờ cooldown. Fixture này sắp vị trí hạt để cô lập phép kiểm tra; đường giải Boss dùng lực và lệnh thật.
- Báo cáo: `Artifacts/COgheFusion/origin-35.xml`; ảnh cú hất: `Artifacts/Venom01/OriginFrames/10-cut-separated.png`.
- Build macOS thành công; đã mở binary mới, chạm vùng chém ở Boss và xác nhận khi để yên sau lượt chém vẫn có hai phần riêng cùng hai nút chọn phần. Để sẵn màn 10 ở trạng thái này cho người chơi thử; R bắt đầu lại nếu muốn quan sát cú hất từ đầu.

## Sửa rơi khi leo sau phân tách — 16/09/2026

- Đã tái hiện hai lỗi bằng thử nghiệm đầy đủ từ dao và A/B: phần nhỏ rơi khi đường đi cắt chéo góc tường–trần; sau khi sửa góc, phần chưa hợp thể vẫn rơi ở miệng lỗ do hỗ trợ thoát bị tắt.
- Navigation thêm điểm chuyển ở góc lõm, đi sát cả hai mặt; không tăng lực bám, tắt trọng lực, thay vật liệu trơn hay dịch chuyển trực tiếp sinh vật. Mép lồi của vật cản vẫn dùng xử lý đưa cả đuôi qua mép.
- Hỗ trợ miệng lỗ áp dụng cho từng phần ở gần cửa đã mở. Điều kiện hợp thể vẫn được kiểm tra khi mô thực sự xuyên qua cửa; một phần ra trước báo `bạn phải hợp thể trước khi chui ra`, không ghi chiến thắng hoặc mở Collection.
- **30/30 kiểm thử Origin đạt**, kết thúc 2026-09-16 05:21:38Z. Hai kiểm tra mới: cả hai phần leo/giữ trần 3 giây, hợp thể ngay trên trần rồi thắng; đưa một phần từ nút tới lỗ khi chưa hợp thể thì thua đúng luật và Retry phục hồi. Đường giải hợp thể trên sàn, các màn vật liệu trơn, qua vách và bắt vành ống vẫn đạt.
- Báo cáo trước/sau: `Artifacts/COgheBossClimb/`; ảnh thực tế `Artifacts/Venom01/OriginFrames/10-split-ceiling.png`.
- Build macOS thành công; đã đóng tiến trình cũ, mở binary mới và để sẵn màn 10. Đường leo/hợp thể/thoát được kiểm tra tự động trong Unity; lần mở binary này chỉ xác nhận nạp màn và giao diện.

## Kết quả kiểm tra 15/09/2026

- Hồi quy: **93/93 test đạt**, gồm 25 test Origin và 68 test Venom cũ; kết thúc 2026-09-15 14:32:02Z. Màn 02 được kiểm tra đi qua–quay lại bốn lượt, quay đầu trên mép ba lượt rồi thoát; mỗi lượt phải đưa đủ 32 phần tử sang cùng một bên vách. Màn 04 vẫn tuột khỏi vùng trơn và qua được bằng đường vòng. Màn 08 được kiểm tra rơi trước khi có lệnh chui ống tại chính giữa và lệch hai bên 35 mm: bắt vành, giữ ít nhất 0,6 giây, nhận lệnh sau đó và chui qua. Cú rơi lệch ngoài vành không bị hút vào và phải xuống sàn.
- MacOS: Unity 6000.3.19f1 báo `ORIGIN BUILD SUCCESS`, binary mới tại `Builds/Venom/macOS/Venom.app`.
- Bản build chứa 10 scene Origin; danh sách scene tạm cho test đã được phục hồi.
- Kiểm tra trực tiếp macOS từng phát hiện kính bên hông chặn chọn lỗ ở 01. Đã sửa và thêm test chọn điểm màn hình rồi thoát đủ cơ thể; test 03 vẫn bảo đảm mặt trước chặn chọn mặt sau.
- Binary macOS cập nhật lúc 17:28:57 ngày 15/09/2026 đã mở và kiểm tra trực tiếp màn 04: chọn lỗ từ phía dưới vùng trơn, sinh vật rơi xuống sàn và trở về Idle; không bị treo ở mép. Các đường giải còn lại của binary này được kiểm tra tự động, chưa chơi tay lại toàn bộ.
- Cập nhật camera 08: 3/3 kiểm tra liên quan đạt lúc 2026-09-15 10:38:51Z, gồm nạp 10 scene, chọn nóc và hoàn thành 08 bằng các lệnh điểm màn hình. Test đường giải 08 hiện dùng cùng `TouchPoint` với người chơi ở cả nóc, mép rơi, miệng ống và lỗ cuối, không gọi tắt `EnterTube`.
- Bản macOS camera 08 build lúc 17:39:49 đã mở, kiểm tra khung hình và chạm lên nóc trực tiếp; sinh vật nhận đúng bề mặt và leo tới điểm chọn.
- Bản sửa bắt vành build lúc 17:52:54 đã thử trực tiếp trên macOS: chỉ lên nóc, chỉ ra mép trơn mà chưa chỉ vào ống; sinh vật rơi rồi giữ lại trên vành ở trạng thái Idle. Chạm miệng ống sau đó chuyển sang “Chảy qua ống”.

- Bản sửa qua vách màn 02 build lúc 18:20:46 ngày 15/09/2026 đã thử trực tiếp trên macOS: chạm sàn phía bên kia vách, đợi sinh vật qua và về Idle, chạm quay về phía ban đầu; cả thân và đuôi quay lại được, không còn mắc vào vách. Chạm lỗ sau đó hoàn thành màn 02 và tự chuyển sang màn 03; đã đưa app về màn 02 để test tiếp.
- Tăng tốc bò/leo 20%: **24/24 test Origin đạt** lúc 2026-09-15 11:52:49Z với tốc độ 0,126 m/s, gồm qua vách rồi quay lại và bắt vành ống. Build macOS lúc 18:53:08 ngày 15/09/2026 thành công, đã mở sẵn màn 02 để cảm nhận tốc độ mới.
- Tương tác cầu trơn màn 06: kiểm tra điểm chạm chọn mặt cầu gần người xem, xúc tu trượt có animation, không có chân bám cố định, không đổi vị trí/vận tốc hạt khi render. So sánh 150 khung mô phỏng có/không chạm (gồm xoay 65°) với sai số vị trí từng hạt dưới 0,5 mm; điểm đích giữ tọa độ theo vỏ, thử lại xóa ý định cũ.
- Bản macOS màn 06 build lúc 21:32:22 ngày 15/09/2026 đã mở, nhận điểm chạm và hiện “Cố bò / trượt”; quan sát ở chế độ zoom thấy thân đổi dáng và xúc tu thử bám khi vẫn ở đáy cầu. Thao tác kéo qua công cụ UI chưa xác nhận được xoay trực tiếp; quán tính và đường giải xoay lỗ xuống đã được kiểm tra tự động. App để sẵn màn 06 cho chơi thử.
- Camera 07: **4/4 kiểm tra màn 07 đạt** lúc 2026-09-15 14:48:52Z. Đường giải chọn thùng và đích bằng điểm màn hình, đẩy tới vách, leo lên rồi thoát; đích trên phần vách nhìn thấy phía trên thùng tránh việc chạm vào chính thùng đang chắn phía trước. Đã xem ảnh riêng tại các pha bám, đẩy, kéo, leo và chui qua lỗ ở khung dọc 720×1280: sinh vật và chỗ tiếp xúc nằm phía camera, miệng lỗ không bị nóc che. Các kiểm tra kéo khỏi kính và tự buông sau 3 giây vẫn đạt.
- Bản macOS camera 07 build lúc 21:49:19 ngày 15/09/2026 thành công. Đã mở và chạm thùng trực tiếp: sinh vật tiếp cận, bám ở phía nhìn thấy, HUD chuyển sang “Đẩy” và hiện nút buông hộp. App được đưa về đầu màn 07 để test góc mới.
