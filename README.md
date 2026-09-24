# COghe — Day Lab Puzzle Prototype

[Xem 10 bản phác thảo level mới 21–30](Docs/LevelDesign/COghe/Sketches21-30/review.html) · [Danh mục và mô tả](Docs/LevelDesign/COghe/Sketches21-30/README.md). Mười thiết kế đã được dựng và xen vào campaign theo thứ tự đã duyệt.

Bản mặc định là **Venom Origin — 30 màn**, trên nhánh `Venom`. Chạm để hướng dẫn sinh vật; kéo để xoay ở những màn cho phép. Biểu tượng dưới hộp cho biết quyền xoay của từng màn. Sinh vật dùng mô mềm có khối lượng, lực bám, va chạm và trọng lực thế giới.

**Phong cách đã chốt: Day Lab**, lấy bản Unity màn 07 ngày 16/09 làm chuẩn: kính sạch, vỏ sứ ấm, viền nhôm, resin hổ phách, vùng trơn tím satin và COghe đen bóng. Bộ này áp dụng cho cả **30 vị trí chơi Origin**, gồm khối cầu, ống nối, nắp rơi, bánh răng, ray và cơ quan Boss. Toàn bộ dựng bằng Unity.

Đọc [STYLE_RULES — quy chuẩn đồ hoạ bắt buộc](Docs/ArtDirection/COghe/STYLE_RULES.md) trước khi sửa art, shader, ánh sáng hoặc HUD. [AGENTS.md](AGENTS.md) dẫn tới quy chuẩn này cho các lần phát triển sau. [Bản mẫu màn 07](Docs/ArtDirection/COghe/Runtime/README.md) · [Gallery và kiểm chứng 10 màn](Docs/ArtDirection/COghe/Campaign/README.md).

**Thiết kế màn tiếp theo:** đọc [quy trình level design và quy tắc code/performance](Docs/COGHE_LEVEL_DESIGN_RULES.md), rồi dùng [mẫu hồ sơ từng level](Docs/LevelDesign/COghe/LEVEL_TEMPLATE.md). Quy trình gồm chín bước từ mục tiêu, phác thảo, lời giải và phục hồi đến dựng bản chơi, cơ quan dùng chung, art/animation, đo trên thiết bị và nghiệm thu. Hồ sơ tách riêng độ khó, khả năng giải, độ rõ điều khiển và hiệu năng; quy tắc hiện hành về cắt/tụ/thoát được ghi rõ để không dùng lại luật Journey cũ. Áp dụng cho Origin 01–20 và các màn mới; chưa coi các màn hiện có là đã đạt mọi tiêu chí chỉ vì có tài liệu này.

**Campaign sau playtest:** [10 màn xen kẽ và thứ tự 30 màn](Docs/COGHE_CAMPAIGN_30_DESIGN.md), gồm tám màn cơ khí/phối hợp và hai màn trượt–chảy có vùng đón rộng. Nội dung mới dùng ID ổn định 21–30, tách khỏi vị trí hiển thị; các màn cũ giữ ID save. Scene tích hợp là `COgheOrigin01`…`COgheOrigin30`, với Boss ở mốc 10/20/30.
[Kết quả test cấu trúc, cơ quan và performance batch](Docs/Verification/COgheCampaign30/README.md).
[Rà soát màn 14/16/18: hình học theo phác thảo và kiểm thử đường giải](Docs/Verification/COgheCampaign30/review-14-16-18.md).
[Sửa cơ thể bị rời khi ra ống màn 17; 63/63 kiểm tra đạt](Docs/Verification/COgheCampaign30/tube-exit-17.md).
[Màn 21: bánh răng cùng trục, khớp răng ở hai trạm và kiểm tra truyền lực](Docs/Verification/COgheCampaign30/gears-21.md).
[Màn 22: bệ leo rộng, tiếp cận từ mép trước và kiểm thử leo–trượt–bay](Docs/Verification/COgheCampaign30/slide-22.md).
[Màn 24: chọn tay nắm xuyên nắp và sửa kẹt khi chuyển giữa hai bánh răng](Docs/Verification/COgheCampaign30/gear-selection-24.md).
[Màn 30: bố cục Boss rõ hơn, nút lớn và thao tác phối hợp dễ hơn](Docs/Verification/COgheCampaign30/boss-30.md).

Áp lại art vào các scene hiện có: **Gravity Box → COghe → Apply Day Lab · All 10 Origin levels**. Lệnh giữ collider, cơ quan và cấu hình màn; mesh trang trí nằm trong thư mục riêng theo màn. Bộ Generate Origin cũng tự áp style. Sàn trong hơn khi lật về camera; chi tiết khung được ẩn khi zoom. Boss không có hướng dẫn lời giải. Tên app/bundle/save vẫn giữ `Venom` để duy trì tiến trình hiện có.

Màn 16 được thay hoàn toàn bằng **Ghép cầu**: COghe đẩy/kéo ba mô-đun trên ray rồi leo qua dải trơn tới lỗ. [Thiết kế và phác thảo mới](Docs/LevelDesign/COghe/Level16/README.md).

Màn 15 dùng thân ống xanh ngọc, gân đồng mảnh và khớp nối màu sứ để phân biệt với kính hộp. Dựng lại riêng bằng **Gravity Box → COghe → Rebuild Day Lab · Pipe Maze 15**; xem [thiết kế và kiểm thử](Docs/COGHE_EXPANSION_11_20.md).

Chỉnh riêng ngoại hình Boss: **Gravity Box → COghe → Rebuild Day Lab · Boss 10**. Đế nút A/B là xám bạc nhạt; dao có mặt thép bạc xước, pháp tuyến phẳng và cạnh vát đánh bóng. Lệnh này giữ nguyên cơ chế chém, lực bám, hợp thể và độ lún nút.

[Phản hồi điều khiển](Docs/COGHE_CONTROL_FEEDBACK.md): vòng sóng và viền sáng xác nhận mặt vừa chạm, dấu đích theo mặt kính, mũi tên chọn phần sinh vật. Các màn 01/07/08 có dấu hướng dẫn riêng; biểu tượng xoay hoặc cấm xoay nằm dưới hộp từ màn 03. Nắp màn 09 có thành dày 14 mm, khối lượng 180 g và xử lý tiếp xúc khi xoay để tránh rung/xuyên vỏ hộp; vẫn rơi tự do khi lật hộp.

[Camera gần và xem từng khoang](Docs/COGHE_CAMERA.md): toàn cảnh của cả 30 vị trí chơi tự vừa vùng chơi dọc, chừa HUD và biểu tượng xoay. **Theo COghe** theo phần đang chọn; bấm **Toàn cảnh** để quay lại. Các nút khu vực chỉ đổi góc quan sát, không xoay vật lý hoặc giải cơ quan thay người chơi.

Nền Day Lab tự phủ đủ khung hình khi xoay và nhìn gần, tránh lộ mép nền xám ở màn 03 trên màn hình dọc dài. Giữ nguyên mặt bàn, bóng đổ và kích thước hiển thị của hộp; xem kiểm chứng trong tài liệu camera phía trên.

**Chọn mặt và lực bám là hai quy tắc riêng:** nóc hộp ở mọi màn đều nhận chạm, kể cả màn khóa xoay. Mặt trơn vẫn nhận lệnh và hiện animation cố bò/trượt xúc tu, nhưng không sinh lực bám; sinh vật chỉ trượt hoặc rơi theo trọng lực, quán tính và va chạm.

| Màn | Trọng tâm |
| --- | --- |
| 01 — Bò đi | Chỉ đường tới lỗ ở góc sàn |
| 02 — Leo đi | Vách thấp bịt kín hai đầu, leo qua rồi bám tường |
| 03 — Xoay đi | Xoay để chọn đúng mặt có lỗ phía sau |
| 04 — Trơn đấy | Chỉ đường tránh vùng mất bám; có thể dùng trọng lực |
| 05 — Trượt đi | Lật nóc trơn xuống dưới, nghiêng để trượt vào lỗ |
| 06 — Xoay tròn | Sinh vật trượt thụ động trong cầu; đưa lỗ tới nó |
| 07 — Đẩy | Đẩy/kéo hộp làm bậc, tự buông sau 3 giây không điều khiển |
| 08 — Chui qua lỗ | Căn rơi bắt vành bám, tự chảy qua ống sang hộp thứ hai |
| 09 — Lật đi | Lật hộp để nắp rời rơi khỏi lỗ theo trọng lực |
| 10 — BOSS · Chờ nhau | Phối hợp các phần cơ thể; không hướng dẫn trong màn |

[Chương mới 11–20: cơ quan, điều khiển, kiến trúc và kiểm chứng](Docs/COGHE_EXPANSION_11_20.md). Dựng riêng chương này bằng **Gravity Box → COghe → Generate Expansion 11–20**.

**Kiểm chứng hiệu năng 17/09/2026:** đã đo trực tiếp các màn 11–20 trên OPPO CPH2591,
đối chiếu APK trước/sau tối ưu. Giữ 32 hạt, vật lý 120 Hz, độ mịn mô và animation Day Lab.
[Báo cáo hiệu năng, số đo và giới hạn kiểm chứng](Docs/COGHE_MOBILE_PERFORMANCE.md).
Chạy lại bộ kiểm thử bằng
`bash Tools/verify-coghe-expansion.sh`.

**Boss 20 trên macOS:** [lời giải, bản trình diễn và kết quả tối ưu](Docs/COGHE_BOSS20_PLAYTEST.md). Đã giải trọn màn bằng lệnh điều khiển thông thường; tối ưu bộ tìm đường và việc cập nhật khi cơ quan chuyển động. Số đo Mac tách riêng khỏi phép đo OPPO phía trên.

**Màn 19 — Cùng nhau:** camera gần hơn, cơ quan A/B và hai cửa có hình dáng/đèn rõ ràng, HUD nhắc theo trạng thái. Tách đôi → giữ A bằng một phần → dùng phần kia bám B và chạm mũi tên để kéo → đưa hai phần chạm nhau → thoát. [Chi tiết màn 19](Docs/LevelDesign/COghe/Level19/README.md).

Mở `Builds/Venom/macOS/Venom.app`. **1–9, 0** chọn màn 01–10; **Shift + 1–9, 0** chọn màn 11–20; **Ctrl + 1–9, 0** chọn màn 21–30; **R** thử lại; **P/Esc** tạm dừng; **Z** zoom. Khi có nhiều phần, chạm một phần hoặc nút chọn phần ở cuối màn hình.

**iPhone:** `bash Tools/build-venom-ios.sh` xuất project Xcode từ catalog 30 màn; cung cấp team/device qua biến môi trường để tự động ký, cài và mở trực tiếp. Profile thủ công là tuỳ chọn. Xem [hướng dẫn build iOS](Docs/COGHE_IOS_PLAYTEST.md). App trên điện thoại có tên **COghe**.

[Ảnh cập nhật cơ quan](Docs/ArtDirection/COghe/Readability/README.md). **Cập nhật cơ quan 16/09:** vật cản 02 là nhựa hổ phách trong; vùng trơn màu tím satin; ống nối 08 có thành cyan và vòng nối rõ. Boss 10 khóa xoay, dao thép báo 1 giây rồi rơi và cắt theo vị trí cơ thể; nút A/B có mặt nhấn lún theo tải.

**Phải hợp thể trong hộp trước khi thoát.** Một phần ra trước sẽ thua và hiện: `bạn phải hợp thể trước khi chui ra`. Chỉ thắng khi toàn bộ 32 phần tử vật chất đã ra ngoài qua lỗ cuối. Ống nối giữa hai khoang không phải lỗ thoát cuối. Camera sau thắng tiến sát, ẩn cảnh, chạy một trong ba điệu vui trong 4,8 giây; các màn thường tự chuyển tiếp, Boss chờ người chơi chọn tiếp tục.

Các phần sau khi cắt vẫn leo và bám kính thường theo khối lượng của mình. Đường chuyển giữa sàn, tường và trần đi sát góc để phần nhỏ không hụt điểm bám. Hỗ trợ ở miệng lỗ áp dụng cả cho phần tách rời; luật hợp thể được kiểm tra khi vật chất thực sự đi qua lỗ, thay vì khiến phần nhỏ rơi lặp lại.

**Hợp thể: cứ đủ gần là tự nhập**, kể cả đang di chuyển, đứng yên, giữ nút hoặc đi khác đích. Cú chém hất hai phần sang hai phía bằng xung lực vật lý để chúng nằm xa hơn khoảng tự nối; không còn bộ đếm chờ sau chém. Kính, vật cản và lưỡi dao đang nằm giữa mô vẫn ngăn nối xuyên qua. Sau khi nhập, cơ thể giữ lệnh còn hiệu lực mới nhất, không bị kéo về lệnh giữ nút cũ.

Thắng Boss lần đầu mở **Collection “Nhà của sinh vật”** với phòng thử nghiệm, chào/chơi và cho ăn. Đây là tương tác mẫu; chưa có cửa hàng, giao dịch hoặc hệ trang trí đầy đủ. Đồ ở nhà không tăng chỉ số hay thay khả năng giải đố. Tiến trình dùng ID nội dung ổn định `venom.origin.01`…`30`; đổi vị trí chơi không đổi khóa save và dữ liệu Journey cũ vẫn được giữ riêng.

[Hướng dẫn test và giới hạn](Docs/VENOM_ORIGIN_PLAYTEST.md) · [Thiết kế 10 màn](Docs/VENOM_CAMPAIGN_01_10.md) · [Kiến trúc và lộ trình](Docs/VENOM_LEVEL_ARCHITECTURE.md) · [Animation theo hành động](Docs/VENOM_ORIGIN_ANIMATION.md).

Kiểm tra Origin bằng Unity Test Runner, gồm `COgheCampaign30IntegrationTests` và toàn bộ PlayMode/EditMode. Kiểm tra bộ Journey cũ: `bash Tools/verify-venom.sh`. Build mặc định: `bash Tools/build-venom.sh`. Sinh lại campaign tích hợp trong Unity: **Gravity Box → COghe → Generate Integrated Campaign 01–30**. Lệnh Generate tạo lại scene và hình học của campaign, vì vậy cần lưu các chỉnh sửa tay trước khi chạy.

Build Android từ catalog 30 màn: `bash Tools/build-venom-android.sh` → `Builds/Venom/Android/COghe.apk`, ứng dụng **COghe**, package riêng `com.gravityboxlab.venom`. [Cài và test trên điện thoại](Docs/COGHE_ANDROID_PLAYTEST.md).

Bộ Journey 5 màn cũ vẫn giữ để đối chiếu: `bash Tools/build-venom.sh --journey`; [tài liệu Journey](Docs/VENOM_JOURNEY_01_05.md). [Kỹ năng nền](Docs/VENOM_CREATURE_SKILLS.md) và [định hướng giải đố thuần / ngôi nhà](Docs/VENOM_PURE_PUZZLE_AND_HOME.md) tiếp tục áp dụng; Copy vật thể và hệ nội thất đầy đủ thuộc giai đoạn sau.

## Bộ thí nghiệm điều khiển trước campaign

Nhánh vẫn giữ bảy scene thử nghiệm **01–05, 07–08**. Dùng `bash Tools/build-venom.sh --lab` để build bộ này thay cho Origin vào cùng đường dẫn app.

| Màn | Điều khiển | Bài thử |
| --- | --- | --- |
| 01 — Một cơ thể, hai ý chí | Nghiêng hộp | Cắt, hai nút, hợp thể, thoát |
| 02 — Hai phần, một kế hoạch | Hộp đứng im; chọn từng phần | Bám giữ nút A, đổi phần sang nút B, mở cửa |
| 03 — Tìm về chủ thể | Điều khiển phần lớn nhất | Phần nhỏ chờ 3 giây rồi tự tìm đường vòng về để nhập lại |
| 04 — Bò khắp sáu mặt | Bò trên mặt hộp, xoay hộp độc lập | Hộp lập phương trong suốt; bò sàn → tường → lỗ giữa trần; Zoom theo sinh vật |
| 05 — Chia ra để lọt vào | Điều khiển mảnh lớn sau khi cắt | Dao cắt theo vị trí; phần lớn luồn vào hộp nhỏ, phần còn lại tự chui theo qua khe |
| 07 — Chạm để dẫn đường | Chạm đích trên mặt trong; kéo để xoay | Tự bò qua các mặt; gần lỗ tự thoát; ghi nhớ các điểm đã tới |
| 08 — Học cách mở lối ra | Chạm nút để hướng dẫn | Tiếp xúc nút mở cửa; tự tìm lỗ và thoát; nhớ quan hệ nút–cửa |

- Trong bản build `--lab`: nút ở đầu HUD hoặc **1/2/3/4/5/7/8** chọn màn; **R** thử lại; **P/Esc** tạm dừng.
- Màn 01: dao chờ ở vị trí nâng, tự thả xuống theo trọng lực khi sinh vật trượt vào dưới lưỡi. Vùng nhả dao rộng hơn để dễ cắt khi nghiêng nhẹ hoặc căn hơi lệch; chém hụt thì dao tự nâng lại sau khi thân rời vùng dao.
- Màn 02/03: **giữ–kéo** để bò hoặc **WASD/mũi tên**. Màn 02 chạm phần muốn chọn, dùng nút A/B hoặc **Tab**. Màn 03 luôn chọn phần lớn nhất.
- Màn 04: **giữ–kéo một ngón / chuột trái** hoặc **WASD/mũi tên** để bò; **kéo hai ngón / chuột phải** để xoay hộp. Giữ hướng để bò vòng qua mép; thả tay thì bám tại chỗ. Xoay hộp để nhìn rõ mặt đang bò. Lỗ thoát ở **giữa trần**; nút **Zoom In / Z** tiến gần và theo sinh vật, bấm lại để về góc toàn hộp.
- Màn 05 giữ điều khiển bò/xoay/Zoom của màn 04. Đưa cơ thể tới dao trên trần, dẫn mảnh lớn vào khe hộp nhỏ; mảnh còn lại bám chờ đến khi chủ thể vào hẳn mới tự chui theo.
- Màn 07/08: **chạm/click đặt đích**, **kéo để xoay hộp**. **Nhớ lại** lặp điều đã học; **R** giữ trí nhớ khi thử lại, **Quên** xóa trí nhớ của màn hiện tại. Trí nhớ được giữ giữa các lần mở app. [Thiết kế điều khiển và trí nhớ](Docs/VENOM_CONTROLS_07_08.md).
- Chạy Unity **6000.3.19f1**: mở `Assets/_Game/Venom/Venom01.unity`, `Venom02.unity`, `Venom03.unity`, `Venom04.unity`, `Venom05.unity`, `Venom07.unity` hoặc `Venom08.unity`, Game View **9:16**, Play.
- Build bộ cũ: `bash Tools/build-venom.sh --lab` (đóng Unity đang mở project trước). APK mặc định là bản 30 màn Origin; các bộ thí nghiệm cũ được build riêng.
- [Thiết kế màn 01](Docs/VENOM_PROTOTYPE_01.md) · [Điều khiển, navigation và kiến trúc màn 02–03](Docs/VENOM_CONTROLS_02_03.md) · [Bò tường và xoay hộp màn 04](Docs/VENOM_CONTROLS_04.md) · [Dao, khe hẹp và phối hợp màn 05](Docs/VENOM_CONTROLS_05.md).

Camera màn 02 nhìn gần thẳng từ trên xuống (**88°**) để dễ chọn hai phần/căn công tắc; màn 03 nhìn **3/4 từ góc trái đầu xuất phát**, cao **45°** và chéo **45°** để thấy rõ chuyển động thân và phần đi theo. Cả hộp nằm trong khung hình; giữ–kéo được căn theo hướng nhìn mới.

Màn 02/03 có **luồn khe chủ động**: giữ hướng vào khe để mô thu hẹp, kéo dài và chảy qua; thả tay để dừng, đổi hướng để rút lại. Màn 03 có khe thật **32 mm** ở đầu trái vách ngang để thử tính năng này, bên cạnh đường vòng rộng ở đầu phải. Cửa ra vẫn yêu cầu cắt rồi hợp thể. [Cách hoạt động và giới hạn](Docs/VENOM_CONTROLS_02_03.md#luồn-khe-hẹp).

Sinh vật dùng hạt vật lý liên kết nhớt/dẻo và bề mặt metaball liên tục. Đây là mô hình vật chất mềm phục vụ thử gameplay, chưa phải solver chất lỏng bảo toàn thể tích. Nội dung 23 bàn bi thép bên dưới vẫn có thể mở qua scene `Gameplay.unity`; build Venom dùng scene riêng.

Sinh vật có thêm **bốn xúc tu giơ lên, cuộn và quẫy lệch nhịp**; thỉnh thoảng vươn thân cao rồi lắc lư như nhảy múa. Bắt đầu bò thì thu động tác lớn; đang luồn khe thì giữ hình dáng gọn. [Xem animation 10 giây](Docs/Images/VenomLife/idle.gif).

Sinh vật có animation theo hướng symbiote: thân dồn và cuộn lệch, mô phía trên có độ trễ khi trượt, thỉnh thoảng dựng một đỉnh mềm để thăm dò. Các sợi bám mọc độc lập và căng mảnh rồi thu lại. Animation đọc vận tốc, tiếp xúc thật và ý định bò ở màn 02/03. Lực bám–kéo do hệ locomotion riêng tạo ở các hạt có tiếp xúc; mesh/xúc tua chỉ biểu diễn hình ảnh, không thay đổi lượng vật chất cần thoát. [Tư liệu và cách áp dụng](Docs/VENOM_MOTION_STUDY.md) · [Ảnh/animation cận cảnh](Docs/Images/VenomLife/README.md).

Bản cập nhật tiếp xúc sàn: cửa và lưỡi chém có chặn dưới đúng mặt sàn; skin không phình xuyên phần sàn đặc và cập nhật theo từng frame. Animation nhanh hơn **1,5×**, biên độ tăng khoảng **20–30%**. Đóng bản macOS đang chạy rồi mở lại app để nhận bản build mới. Kiểm chứng: rơi 2/5/10 m/s, lật hộp nhiều trục, chặn cơ cấu và đường giải thoát đủ vật chất.


## Gravity Box — Steel Ball Lab

Prototype Unity tập trung vào cảm giác bi thép lăn trong hộp: tăng tốc khi nghiêng, giữ quán tính, đổi hướng và nảy khi va chạm. Catalog có **23 bàn**: giữ 16 thí nghiệm trước và thêm bảy màn cơ khí 17–23. Hai môi trường chất lỏng dùng cùng hộp vuông/cube của bàn 02: bi chìm trong nước và nổi trong thủy ngân.

**17–23:** cầu bản lề tự dựng, cân hai bi, lồng treo độc lập, cổng con lắc, lăn–bay–đón, bánh cam có cóc nhớ trạng thái, và boss hai bi trong khối cầu kính. Các cơ cấu dùng Rigidbody/joint, lực tiếp xúc và trọng lực; ball không được điều khiển bằng đường chạy hoặc xung phóng. Chốt/cóc giữ được mô hình hóa bằng ràng buộc lý tưởng tại trạng thái cơ khí đã đạt. [Thiết kế và cách test](Docs/MECHANICAL_LEVELS_17_23.md).

**Luật chung: tất cả bi phải thoát qua lỗ mới thắng.** Bàn 16 có hai bi cùng chịu một thao tác xoay: A giữ nút lò xo để B qua cửa; B nhấn chốt giữ mở hai cửa, rồi cả hai tới lỗ chung. Bàn 18 và 23 cũng có hai bi. HUD đếm `OUT 0/2 → 1/2 → 2/2`; những màn còn lại có một bi. [Thiết kế phối hợp](Docs/LEVEL16_COOPERATIVE.md), [kiểm chứng hiện tại](Docs/Verification/Cooperative16/README.md).

Màn 15 là hộp đầu sư tử bằng đồng và kính: bờm/tai là thành hộp thật, mắt/mũi/mõm là các gờ va chạm. Trọng lực bình thường; dẫn bi vòng qua khuôn mặt đến lỗ ở miệng. [Thiết kế](Docs/LEVEL15_LION_HEAD.md), [ảnh](Docs/Images/Level15/README.md).

Cả 23 màn có **hỗ trợ hút trong bán kính 4 cm quanh lỗ cuối**: căn từng bi rồi đẩy ra qua cửa thật, chỉ thắng khi đủ số bi đã thoát hoàn toàn. Các lỗ chuyển nội bộ không có lực hút. Đây là hỗ trợ gameplay theo yêu cầu, không thay mô hình vật lý ở phần còn lại. [Thiết kế](Docs/EXIT_ASSIST.md).

Bàn 14 giữ luật chất lỏng đầy, không chảy qua cửa; dùng hình ảnh bạc nhìn xuyên có nhãn để thấy bi bên trong thủy ngân đục. Hỗ trợ thoát xử lý cả bi nổi đứng yên ở miệng lỗ. Sàn bàn 13/14 trong hơn khi lật về phía camera. [Thiết kế/giới hạn](Docs/LEVEL14_MERCURY.md). Bản mới xuất macOS; APK cũ chưa có màn 14–23 và hỗ trợ thoát.

Bản nước hiệu chỉnh bổ sung cản khi bi lăn sát thành và added mass; giữ nước 20°C và bi thép 111 g. Có benchmark giảm tốc, hội tụ 60/120/240 Hz và kiểm tra lỗ thật. [Công thức/giới hạn](Docs/LEVEL13_WATER.md), [kiểm chứng](Docs/Verification/Water13/README.md). Bản hiệu chỉnh chỉ xuất macOS; APK hiện có vẫn là bản nước trước đó.

Ở bàn 09, một thanh chặn có khối lượng trượt trên ray theo trọng lực. Đưa bi vào hốc giữ, nghiêng để thanh chặn rời cửa trong khi thành hốc giữ bi lại, rồi chuyển hướng nghiêng để bi đi qua khoang bên phải và ra lỗ. Thanh chặn luôn là vật thể va chạm; không có công tắc hoặc tín hiệu mở khóa. Xem [thiết kế bàn 09](Docs/LEVEL09_LEAVE_IT_BEHIND.md).

Bàn 10 kết hợp các hành lang đổi hướng với hai cửa trượt được mở bởi hai chiều nghiêng đối nhau. Bàn 11 có mê cung riêng trên từng tầng; bi phải lăn tới các lỗ chuyển tầng lệch nhau rồi rơi xuống theo trọng lực, cuối cùng ra lỗ tròn ở đáy. Xem [thiết kế hai mê cung](Docs/LEVEL10_11_MAZES.md).

Bàn 12 ghép các ván kính nhỏ thành một mê cung liên tục trong khối cầu: 25 đoạn, 21 khúc đổi hướng theo cả ba trục và hai nhánh cụt. Khe nhìn giữa các ván nhỏ hơn bi, chặn đường tắt sát vỏ; các lối đi thật nối tới lỗ tròn duy nhất. Xem [thiết kế mê cung trong khối cầu](Docs/LEVEL12_SPATIAL_MAZE.md).

Bàn 13 giữ đầy nước ngay cả khi bi thoát qua lỗ. Bi thép chịu lực nổi và lực cản theo tốc độ tương đối; VFX thể hiện màu nước, caustic, hạt lơ lửng và wake quanh bi. Đây là mô hình lực và dòng khối xấp xỉ, chưa phải fluid solver đầy đủ. Xem [thiết kế và giới hạn bàn 13](Docs/LEVEL13_WATER.md).

Bi có đường kính 30 mm, khối lượng khoảng 111 g; hộp rộng khoảng 34–101 cm tùy hình, sâu 9 cm ở bàn 01–10 và 27 cm ở bàn 11; cầu ở bàn 12 có đường kính ngoài 73,2 cm. Một đơn vị Unity là một mét. Trọng lực thế giới 9,81 m/s² và mô phỏng 120 Hz được giữ nhất quán; cảm giác nặng đến từ tỷ lệ, quán tính quay, contact, tổn hao năng lượng và âm thanh tương ứng với va chạm.

## Chạy và quan sát

1. Mở project bằng Unity **6000.3.19f1**.
2. Mở `Assets/_Game/Scenes/Gameplay.unity` hoặc menu **Gravity Box → Open Gameplay**.
3. Chọn Game View portrait **9:16**, nhấn Play.

Kéo chuột trái hoặc một ngón tay trong vùng hộp để nghiêng. Nghiêng nhẹ rồi giữ để quan sát gia tốc; trả mặt hộp về ngang để quan sát quán tính và giảm tốc. Trong hộp vuông, đưa bi vào mặt phẳng và góc của khối lập phương để so sánh hướng nảy. **R** reset; **P/Esc** pause. Chọn hộp và chuyển tiếp bằng HUD. Ở bàn 11, chạm dòng trạng thái tầng để bật/tắt xem toàn bộ tầng; chế độ xem chỉ đổi vật liệu hiển thị, mọi sàn và vách vẫn va chạm.

Lỗ thoát vẫn là lỗ tròn xuyên mặt hộp, với viền sáng mảnh không có collider hoặc gờ nổi. Chỉ khi toàn bộ bi đi qua lỗ mới ghi nhận thoát. Mô phỏng tiếp tục ở tốc độ thực; không slow motion hoặc tự chuyển bàn. Người chơi chọn khi nào reset hoặc chuyển hộp.

**D** bật diagnostics trong Editor/development build. **F12** lưu ảnh native vào `Application.persistentDataPath/gravity-box.png`.

## Tài liệu đang áp dụng

- [Triết lý vật lý và cách đánh giá cảm giác](Docs/PHYSICS_DESIGN_PRINCIPLES.md)
- [Kế hoạch hoàn thiện và cổng nghiệm thu](Docs/IMPLEMENTATION_PLAN.md)
- [Kiến trúc và quyền sở hữu trạng thái](Docs/ARCHITECTURE.md)
- [Phạm vi kiểm chứng](Docs/SOLVABILITY.md)
- [Quyết định kỹ thuật](Docs/DECISIONS.md)
- [Nhật ký và kết quả xác minh](Docs/DEVELOPMENT_LOG.md)
- [Ảnh native của năm hình mới](Docs/Images/WeirdBoxes/README.md)
- [Màn 09: trạng thái cửa đóng/mở](Docs/Images/Level09/README.md)
- [Bàn 10–11: cửa ngược hướng và mê cung ba tầng](Docs/LEVEL10_11_MAZES.md)
- [Bàn 12: mê cung không gian trong cầu thủy tinh](Docs/LEVEL12_SPATIAL_MAZE.md)

Ba hộp cơ bản đã được bổ sung năm hình dạng khác thường theo yêu cầu người dùng. Mỗi hình có sàn, nắp và thành đúng đường biên thật; khoảng khuyết của chữ L/U, lõi rỗng của vành khuyên và cổ hẹp của quả tạ ảnh hưởng trực tiếp tới đường lăn. Asset của catalog 16 màn cũ vẫn được giữ ngoài catalog đang chơi. [Tài liệu lịch sử](Docs/Archive/README.md) và [GDD gốc đã trích xuất](Docs/GDD_REFERENCE.md) không phải danh sách tính năng cần đưa trở lại prototype này.

## Cấu trúc và tuning

`Scripts/Foundation` giữ session/reset; `Simulation` giữ Rigidbody, lực, rolling contact và rotation; `Gameplay` quản lý catalog, load/reset/exit; `Presentation` nhận input và thể hiện hình/âm thanh; `App` kết nối các lớp. Editor tạo prefab/mesh và kiểm tra nội dung trước build. Scene duy nhất tải một thí nghiệm tại một thời điểm.

Profile đang dùng nằm trong `Assets/_Game/PhysicsLab/Profiles`: `Solid steel.asset`, `Earth.asset`, `Hand rotation.asset`, `Room temperature water.asset` và `Room temperature mercury.asset`. Prefab/mesh nằm trong `PhysicsLab/Prefabs` và `PhysicsLab/Meshes`; `ScriptableObjects/LevelCatalog.asset` tham chiếu 23 level. Khi đổi kích thước bi phải đổi mass/inertia/contact offset và kiểm tra clearance theo cùng đơn vị. Không tăng riêng mass hoặc giảm gravity để tạo cảm giác nặng.

Prefab và profile đã có sẵn; không cần chạy generator để chơi. Generator tạo lại nội dung đã author, nên lưu thay đổi bằng Git hoặc prefab variant trước khi regenerate.

## Kiểm thử và build

Đóng Unity đang mở project trước khi chạy CLI:

```bash
bash Tools/verify.sh
bash Tools/build.sh macOS
bash Tools/build.sh Android
bash Tools/build.sh iOS
bash Tools/build.sh iOS-Simulator
```

Có thể đặt `UNITY_EDITOR` tới executable đúng phiên bản. XML/log vào `Artifacts/`, build vào `Builds/`; hai thư mục này không commit. Validator kiểm tra catalog, prefab, scale, spawn và aperture; tests kiểm tra luật chuyển động, contact, đường biên/lõi rỗng, độ rộng lối đi và lifecycle. Đường giải 16 màn cũ không còn là cổng nghiệm thu của physics lab.

Android cần module và SDK/NDK/JDK tương ứng. iOS export cần module iOS; compile/cài cần Xcode và signing phù hợp. `bash Tools/run-ios-simulator.sh` export, compile và cài lên simulator đang boot trên Apple Silicon; có thể truyền UDID làm tham số đầu. Telemetry chỉ lưu CSV local. Trạng thái build/chạy thực tế được ghi riêng trong nhật ký; tests không tự chứng minh cảm giác chơi đã đạt.

Màn 12 cập nhật đường nhận chạm trên máng cong và camera bên hông; lối leo hổ phách, máng tím và thành cyan trong giúp quan sát leo–trượt–bay–bám. [Chi tiết thao tác và kiểm chứng](Docs/COGHE_EXPANSION_11_20.md).

Màn 13: xuất phát xa cần gạt; cần A mở cửa hộp nhỏ, nút B mở nắp ống. Đã sửa cần tự đổ mở cửa; thêm khung hộp, ray cửa, nút nhấn và đèn A/B. Hai kiểm thử riêng màn 13 đạt. [Thiết kế và cách thử](Docs/COGHE_EXPANSION_11_20.md).

## Campaign 01–55 (23/09/2026)

The ten Tap levels are now integrated at 41–50; five block-assembly puzzles based on displayed level 19 occupy 51–55. Build the full campaign with the ordinary `Tools/build-venom-android.sh` / `Tools/build-venom.sh` commands, without `--tap`. See [build and playtest guide](Docs/COGHE_CAMPAIGN_41_55.md). The standalone Tap variant remains available for earlier comparisons; original scene and save identities are preserved.

## Campaign01–60 (24/09/2026)

Five hollow slippery vessels follow displayed level7 at56–60: vase, mask, teapot, skull and a spiral-shell Boss. Drag rotates the real container; the creature moves under gravity and escapes through the mint opening. Use the ordinary build wrappers for all60; `--tap` retains its standalone chapter. See [vessel chapter guide](Docs/COGHE_CAMPAIGN_56_60.md).

### COghe onboarding pilot — 24 September 2026

The isolated ten-level learning pilot uses source slots `1 → 2 → 41 → 8 → 14 → 18 → 9 → 16 → 42 → 10`, with state-driven visual guidance and its own save. Build it with `bash Tools/build-venom.sh --onboarding` or `bash Tools/build-venom-android.sh --onboarding`. Outputs: `Builds/COgheOnboarding/macOS/COghe Learn.app` and `Builds/COgheOnboarding/Android/COghe-Learn.apk`. Android installs alongside the main game.

The normal build still contains the existing60-level campaign. The full progression mapping remains a design hypothesis pending novice playtests. [Pilot and test sheet](Docs/COGHE_ONBOARDING_PILOT_2026_09_24.md) · [60-level candidate](Docs/LevelDesign/COghe/ONBOARDING_PROGRESSION_DRAFT.md).
