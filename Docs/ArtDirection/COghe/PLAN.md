# COghe — Định hướng đồ hoạ và kế hoạch triển khai

Ngày: 15/09/2026. Nhánh tham chiếu: `Venom`, commit `c103b31`.
Trạng thái cập nhật 16/09/2026: người dùng đã duyệt bản Unity màn 07. Đã áp bộ Day Lab cho cả 10 màn; xem [quy chuẩn đã chốt](STYLE_RULES.md) và [bản dựng, gallery, kiểm tra](Campaign/README.md). Nội dung dưới là kế hoạch ban đầu: Night Trial, Nhà theo concept và đo mobile vẫn là phần việc tương lai. Khi thông số khác nhau, STYLE_RULES là chuẩn hiện hành (cubemap 128 px, không post-processing bắt buộc, Boss dùng Day Lab sáng).

## 1. Quyết định đề xuất

Chọn **3D cách điệu với phòng thí nghiệm vũ trụ sáng, gọn và ấm áp** làm phong cách chính. COghe là một khối vật chất sống đen, mềm và có biểu cảm nằm giữa các thiết bị nghiên cứu sáng màu, có cấu tạo dễ hiểu. Cảm giác cần đạt: tò mò → tin tưởng → thân thiết → tự hào khi cùng vượt thử thách.

Nguồn cốt truyện là yêu cầu của người dùng ngày 15/09: vật thể lạ rơi xuống Trái Đất; NASA phát hiện sinh vật bên trong, đưa về nghiên cứu; qua thí nghiệm thấy nó có trí tuệ và cảm xúc, nhân viên trở nên gắn bó rồi đặt tên **COghe**. Giữ chính xác cách viết tên này.

Đề xuất diễn giải vai trò người chơi: người hướng dẫn/người chăm sóc trong nhóm nghiên cứu. Các hộp là bài thử khả năng; hoàn thành thử nghiệm dẫn tới một tương tác tích cực. Không tự thêm xung đột chạy trốn, trừng phạt hoặc tổ chức phản diện vào cốt truyện.

Sinh vật giữ nguyên bản sắc runtime: mô đen vô định hình, mép bám mềm, nếp khối bất đối xứng, xúc tu mảnh không mọc đều như chân máy, đỉnh thân chỉ là biến dạng tạm thời. Không thêm mắt, mặt, đầu cố định, tai, trang phục hay giải phẫu siêu anh hùng. Sự thân thiện đến từ cử chỉ và quan hệ với con người.

## 2. Vì sao chọn hướng này

| Hướng cân nhắc | Đánh giá cho COghe |
| --- | --- |
| Phòng nghiên cứu 3D cách điệu, ánh sáng mềm | Đề xuất chính: thân đen nổi rõ trên đồ vật sáng, hợp cảm giác chill, nối tự nhiên sang Collection, có thể dựng bằng bộ asset và shader gọn |
| Khoa học viễn tưởng tả thực, tối và phản chiếu mạnh | Có chất khám phá nhưng dễ khiến kính lẫn cơ thể khó đọc. Cần nhiều kiểm soát ánh sáng; chọn một phần không khí cho Boss |
| Hoạt hình phẳng hoặc low-poly góc cạnh | Có thể đơn giản hoá rendering nhưng giảm cảm giác mô mềm, sức nặng và tiếp xúc vốn là điểm mạnh hiện tại |

Phong cách không tự bảo đảm tốc độ. Lợi thế hiệu năng chỉ xuất hiện khi thực hiện bằng ít lớp kính, phản xạ chuẩn bị sẵn, shader gọn, model vừa đủ và VFX có giới hạn.

Các case study từ nhà phát hành nhấn mạnh bố cục sạch, vật liệu phù hợp hành động và phản hồi rõ. Đó là căn cứ cho cách trình bày; không chứng minh rằng một bảng màu sẽ thành công trên thị trường. [Supersonic, 18/11/2025](https://supersonic.com/learn/blog/how-ux-design-drives-player-engagement-in-eogames-hit-hybrid-puzzles/), [Rollic, 12/06/2025](https://rollicgames.com/highlights/hit-stories-is-back-the-success-story-of-color-block-jam).

Bài viết về động lực người chơi tháng 2/2026 gợi ý chọn động lực chính cho core rồi dùng meta bổ sung động lực khác. Với COghe, đề xuất là giải quyết thử thách trong puzzle, chăm sóc và cá nhân hoá trong Nhà; đây là suy luận thiết kế riêng cho game. [Supersonic, 16/02/2026](https://supersonic.com/?p=107139).

## 3. Ba concept trong cùng một hệ hình ảnh

| Concept | Vai trò | Trọng tâm |
| --- | --- | --- |
| 01 — Day Lab | Chuẩn chính cho gameplay thường, minh hoạ màn 7 | Kính trong, sinh vật bám thùng, vùng trơn, nóc thùng và lỗ thoát cùng đọc được |
| 02 — Night Trial | Biến thể ánh sáng cho Boss 10 | Nền phòng tối dịu nhưng sàn và cơ quan vẫn sáng; hai phần COghe phối hợp; nhìn được nút, dao, cửa |
| 03 — A Place to Belong | Nhà của COghe sau Boss đầu tiên | Tái sử dụng hộp nghiên cứu thành nơi chăm sóc; COghe đáp lại ngón tay người chăm sóc qua kính |

Concept là đích hình ảnh, không phải ảnh chụp bản build, phép đo hiệu năng hay bản vẽ cơ khí có kích thước. Các đường ray, vỏ bọc và đồ trang trí trong concept phải được đối chiếu collider khi dựng asset. Hình Boss không thay bố cục hoặc luật hiện hành.

## 4. Màu, vật liệu và cơ quan

Bảng màu gợi ý: nền `#F2EFE8`, vỏ thiết bị `#DCE5E2`, chữ/cơ cấu `#34454F`, tương tác đang hoạt động `#2A978E`, vật di chuyển `#D6A16F`, vùng trơn `#A6C4DE`, thân `#202630`. Màu phản xạ trên thân là kết quả ánh sáng, không tô sinh vật thành một màu mới. Hệ thống trạng thái dùng thêm hình, ký hiệu và chuyển động.

| Thành phần | Thiết kế | Dấu hiệu gameplay phải đọc được |
| --- | --- | --- |
| Kính bám được | Mặt trong sạch, cạnh mảnh bắt sáng, gá ngoài nhỏ; giảm phản xạ/độ đục mặt gần camera khi che cảnh | Biên hộp và điểm bám còn thấy được ở mọi góc; việc làm trong không đổi mục tiêu raycast |
| Vùng trơn | Lớp phủ xanh băng, dải phản xạ rộng, hoa văn mảnh theo bề mặt, ranh giới rõ | Nhận diện được khi đứng yên và xoay; xúc tu hụt bám/trượt là phản hồi thực |
| Thùng đẩy/kéo | Resin mờ màu ấm, mép bo nhỏ, mặt lớn sạch | Điểm tì, độ nén khi đẩy và độ căng khi kéo; vỏ không che phần thân |
| Nắp rời | Viền và độ dày rõ, vật liệu khác vùng kính bên dưới | Là một vật thể rời chịu trọng lực; bóng, khe hở và va chạm cho thấy nó đã rơi khỏi lỗ |
| Nút/cảm biến | Đế thấp, vùng nhận lực, ký hiệu A/B; bộ phận chuyển động biểu diễn đúng cơ chế | Chưa kích hoạt / đang giữ / đã chốt phân biệt bằng tư thế và ánh sáng; không chỉ màu |
| Dao/cửa/chốt | Ray, trục và bộ phận dịch chuyển có cấu tạo rõ, vỏ màu nhạt với điểm kim loại nhỏ | Chỉ chuyển trạng thái khi simulation cho phép; không thêm trang trí làm người chơi hiểu sai đường đi |
| Lỗ thoát | Lỗ tròn thật, viền phẳng sáng nhỏ và yếu | Nhìn được miệng, cổ mô và đuôi đang chui; không dựng gờ portal chặn đường |
| COghe | Phản xạ rộng dịu, mép thân có tương phản, xúc tu có gốc và đầu rõ | Thấy mô mềm, khối lượng, hướng cố gắng và tiếp xúc; không phản chiếu trắng kín thân |

Mọi nâng cấp dùng chung tổng vật chất, tốc độ, tách/hợp và lực hiện có. Cơ thể chỉ tách qua cơ quan cắt, các phần gần nhau tụ lại; phải hợp thể trước lỗ cuối. Boss không hiện chỉ dẫn lời giải. Đồ ăn/đồ đạc/animation ở Nhà không thay khả năng giải puzzle.

## 5. Ánh sáng và hiệu ứng có thể chạy trên mobile

Thiết lập cơ sở dự kiến:

- Một đèn chính tạo khối và bóng trong vùng chơi; ánh bù và viền ưu tiên ambient cùng thành phần đơn giản trong shader.
- Phản xạ studio từ cubemap chuẩn bị sẵn, khởi điểm 256 px/mặt; thử 128/512 tuỳ máy và độ rõ của thân khi zoom. Không cập nhật reflection probe mỗi frame.
- Kính một lớp vỏ hình học vừa đủ, xử lý đúng mặt trước/sau và thứ tự trong suốt; giảm các lớp phủ chồng toàn màn hình. Giới hạn quang học ở mức không làm lệch cảm nhận điểm chạm.
- Mặt cơ quan chủ yếu opaque. Dùng normal/roughness và mép model thay cho nhiều lớp kính hoặc hàng loạt đèn.
- Bóng tiếp xúc của vật chuyển động phải theo vị trí/mặt đỡ. Với hộp xoay, chỉ bake những chi tiết gắn với model như AO cục bộ; không bake một bóng sàn cố định lên hộp rồi để nó quay sai ánh sáng.
- Background gameplay là gradient, hình nền hoặc vài khối đơn giản. Kể chuyện ở khoảnh khắc chuyển cảnh, tránh chạy nguyên phòng NASA cùng nhân viên trong mọi màn.
- Bloom nhẹ là tuỳ chọn sau khi đo. Bản cơ sở ưu tiên LDR và phản xạ có kiểm soát; độ rõ cơ quan không phụ thuộc bloom. Giữ hiệu ứng toàn màn hình ở mức tối thiểu.
- Giữ mô phỏng 32 phần tử hiện có. Đo riêng CPU skin, physics, tìm đường và GPU kính trước khi quyết định nâng mật độ mesh; thêm art không tăng hạt vật lý.

Unity nêu rõ cần đo ảnh hưởng từng tuỳ chọn URP, đồng thời cảnh báo chi phí của lớp trong suốt và post-processing trên mobile. Đây là lý do phải làm bản mẫu chạy thật trước khi nhân rộng. [Unity URP 6.3](https://docs.unity3d.com/6000.3/Documentation/Manual/urp/configure-for-better-performance.html), [Unity mobile graphics](https://unity.com/blog/games/optimize-your-mobile-game-performance-expert-tips-on-graphics-and-assets).

| Cấu hình dự kiến | Hình ảnh | Mục tiêu kiểm tra, chưa đo |
| --- | --- | --- |
| Mobile cơ sở | Reflection sẵn, một nguồn bóng giới hạn, VFX cục bộ, không bloom bắt buộc | 60 FPS trên máy mục tiêu tầm trung; kiểm tra frame-time và nhiệt sau 15 phút |
| Mobile thấp | Giảm độ phân giải render/bóng, đơn giản hoá phản xạ và VFX | 30 FPS ổn định; giữ nguyên độ đọc trạng thái và toàn bộ logic vật lý |
| Mac/chất lượng cao | Có thể tăng bóng, reflection và post nhẹ khi còn ngân sách | Dùng để kiểm tra art; không lấy kết quả Mac để kết luận điện thoại |

Chốt ngân sách ms/triangles/batches/bộ nhớ sau mốc mẫu đầu tiên trên thiết bị mục tiêu. Chưa chọn máy đại diện nên không cam kết FPS trên mọi điện thoại.

## 6. Kể chuyện bằng hình ảnh

- Mở đầu: chuỗi vài hình minh hoạ về vật thể rơi, phát hiện và đưa vào phòng nghiên cứu. Có thể dùng tranh có chuyển động camera nhẹ để tiết kiệm chi phí dựng cinematic 3D.
- Các màn đầu: nhãn mẫu nghiên cứu, vật dụng sạch và cử chỉ đáp lại điểm chỉ. Tên COghe xuất hiện như một cái tên thân mật trên thẻ nhỏ.
- Càng tiến xa: thêm ghi chú, hình vẽ và cử chỉ chào đón ở khu vực ngoài gameplay; tránh trang trí kín hộp.
- Boss: thay nhiệt độ ánh sáng và nhịp phản hồi cơ quan, giữ sinh vật/sàn đủ sáng. Thử thách lớn vẫn là bài giải đố.
- Sau Boss 10: mở Nhà; một góc phòng nghiên cứu được cá nhân hoá. Tái dùng khung kính và vật liệu chính, thêm đồ ăn, đồ chơi và tương tác. Đây là tiến triển quan hệ bằng không gian, không thêm thanh chỉ số bắt buộc.

## 7. Kiến trúc art và thứ tự triển khai

Các tên dưới đây là module đề xuất, chưa tồn tại như API được triển khai:

| Module | Trách nhiệm |
| --- | --- |
| ArtThemeProfile | Bảng màu, thư viện vật liệu, thông số phản xạ; preset Day / Night / Home |
| LightingProfile | Đèn chính, nền, ambient, cubemap và tuỳ chọn theo chất lượng |
| MechanismVisual | Vỏ model và animation nhận trạng thái từ nút, cửa, nắp, dao; không ghi lực hay luật thắng |
| GlassPresentation | Giữ biên kính rõ và xử lý mặt đang che cảnh; tách khỏi collider và chọn điểm |
| CreaturePresentation | Material, ánh viền, chất lượng skin và tín hiệu animation; không đổi danh tính/khối lượng |
| QualityProfile | Điều chỉnh chi phí hình ảnh theo cấu hình đã đo; không đổi lực, route hoặc điều kiện vượt màn |

**Mốc A — bản mẫu màn 7:** dựng kính, thùng, vật trơn, vật liệu sinh vật và ánh sáng. So sánh trước/sau cùng camera và cùng chuỗi thao tác. Đích bàn giao là build macOS nhìn rõ bám → đẩy/kéo → leo → thoát.

**Mốc B — kiểm tra vật liệu ở tình huống khó:** màn 4–6 khi trượt và xoay, màn 8 khi nhìn qua hai hộp và ống, màn 9 khi nắp rơi. Sửa kính chồng lớp, sai phản xạ, mất tương phản và vùng chọn khó bấm.

**Mốc C — bộ cơ quan và Boss:** hoàn thiện nút, dao, cửa và các trạng thái vật lý; áp preset Night có kiểm soát vào Boss 10. Kiểm tra đường giải, các phần cơ thể và khoảnh khắc hợp thể/thoát.

**Mốc D — Nhà và thương hiệu:** đổi phần trình bày sang COghe, làm bối cảnh chăm sóc và vài tương tác mẫu; đồng bộ menu/HUD. Dữ liệu save cần giữ khả năng đọc tiến trình cũ nếu đổi định danh ứng dụng.

**Mốc E — nhân rộng và đo mobile:** áp dụng cho 10 màn, kiểm tra xoay/zoom ở khung điện thoại, regression gameplay, đo CPU/GPU/memory/nhiệt. Sau đó mới chốt profile phát hành.

Mỗi mốc có capture hoặc video cùng bản chạy. Không dùng tranh concept làm bằng chứng rằng gameplay, animation hay hiệu năng đã đạt.

## 8. Nghiệm thu

1. Nhìn ở khung 360×640 vẫn phân biệt sinh vật, vật trơn, đồ vật rời, nút và lỗ.
2. Bám/đẩy/kéo/leo/luồn/thoát đọc được bằng dáng và điểm tiếp xúc khi tắt HUD.
3. Kiểm tra grayscale để bảo đảm mã màu không là tín hiệu duy nhất.
4. Xoay sáu hướng, nghiêng liên tục và zoom không gây kính nhấp nháy, mất lỗ hoặc chọn sai mặt do art.
5. Không thay collider, khối lượng, tốc độ hoặc đường giải chỉ để làm đẹp; nếu model thay hình học có ý nghĩa thì phải chạy lại test liên quan.
6. Chơi thử 10–15 phút trên điện thoại đại diện; báo p95 frame-time, CPU/GPU, peak memory và giảm hiệu năng do nhiệt.
7. Các KPI cảm nhận/retention/viral cần playtest; chưa khẳng định từ concept.

## 9. Bàn giao concept

Ảnh được tạo bằng **imagegen tích hợp** từ screenshot runtime, không dùng CLI/API riêng. Prompt chính xác được lưu trong [Prompts](Prompts/). Ảnh cuối, nguồn tham chiếu, giới hạn và kết quả kiểm tra bằng mắt được ghi trong [README](README.md) và [QA](QA.md).

Đợt concept ngày 15/09 chỉ tạo tài liệu và ảnh. Đợt 16/09 dựng art màn 07, được duyệt rồi mở rộng cho 10 màn Origin; giữ nguyên physics. Bộ dựng hiện có là `COgheDayLabBuilder` (hai file partial), `COgheDayLabPresentation` và các shader/asset dùng chung. Các tên profile/module ở phần 7 là hướng mở rộng, không phải API hiện hành.
