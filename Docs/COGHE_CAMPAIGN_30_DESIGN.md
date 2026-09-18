# COghe — 10 màn xen kẽ và tiến trình 30 màn

Ngày: 17/09/2026. **Trạng thái: đã dựng prototype Unity, xen đúng thứ tự 30 màn,
giữ ID save và đã chạy kiểm thử tự động trên macOS. Chưa thay cho playtest tay trên thiết bị di động.**

## Căn cứ và mục tiêu

Người dùng đã cho người chơi thật thử build gần nhất: màn 05, 08 và một số màn
11–20 quá khó. Thông tin làm rõ: **họ hiểu cách giải 05/08 nhưng khó xoay, căn trượt
hoặc rơi**. Chưa có số người, video thao tác hoặc số đo completion để kết luận tỷ lệ.

Thiết kế bổ sung hướng tới quyết định có thời gian suy nghĩ, thao tác rộng, kết quả
cơ quan nhìn thấy và được giữ lại. Tám màn dùng đẩy/kéo/ghép cơ khí/phối hợp đơn giản;
hai màn trượt/chui ống có vùng đón rộng. Không yêu cầu giữ nhiều ngón, bấm giữa lúc
rơi, căn pha răng hoặc sửa góc liên tục. Không thêm chỉ số nâng cấp.

Quy chuẩn: [quy trình chung](COGHE_LEVEL_DESIGN_RULES.md),
[mẫu hồ sơ](LevelDesign/COghe/LEVEL_TEMPLATE.md), [Day Lab](ArtDirection/COghe/STYLE_RULES.md).

## 1. Mười màn mới

[Xem 10 bản phác thảo bút bi xanh, phóng lớn từng hình](LevelDesign/COghe/Sketches21-30/review.html)
· [Danh mục ảnh và mô tả](LevelDesign/COghe/Sketches21-30/README.md).
Đây là bản duyệt ý tưởng v1; hình giản lược chi tiết lắp ráp, chưa thay thế kiểm chứng hình khối.

Các mã 21–30 dưới đây là **ID nội dung ổn định**, không phải vị trí chơi nối tiếp.
Mức suy luận/thao tác vẫn là đánh giá thiết kế, chưa phải kết quả playtest người chơi.

| ID / hồ sơ | Vị trí đề xuất | Ý tưởng và quyết định chính | Khoảnh khắc vui | Suy luận / thao tác |
| --- | ---: | --- | --- | --- |
| [21 · Nghiêng là tới](LevelDesign/COghe/Level21/README.md) | 5 | Nghiêng cho mô trượt theo máng rộng tới bệ đón; chọn hướng, rồi chờ | COghe dồn thành một cục ở khay, tự đứng dậy rồi tới lỗ | Nhẹ / nhẹ, cần đo góc dung sai |
| [22 · Đáp rồi chui](LevelDesign/COghe/Level22/README.md) | 11 | Chọn mép trơn rộng, rơi ngắn vào vành dẫn thật rồi tự chảy qua ống | Cơ thể kéo thành dòng và tụ lại ở hộp bên kia | Nhẹ / nhẹ, không bấm giữa cú rơi |
| [23 · Khớp rồi!](LevelDesign/COghe/Level23/README.md) | 9 | Đẩy một giá bánh răng vào cuối ray để nối nguồn với cửa | Ba bánh cùng quay, thanh răng nâng cửa | Nhẹ–vừa / nhẹ |
| [24 · Kéo ra mới qua](LevelDesign/COghe/Level24/README.md) | 14 | Nắp che trượt sang bên: đẩy sai thì gặp chặn, kéo đúng làm lỗ lộ ra | Cả cửa tròn hiện ra sau tấm che | Nhẹ / nhẹ, sửa hướng tự do |
| [25 · Hai nhịp một cửa](LevelDesign/COghe/Level25/README.md) | 16 | Mở che tay nắm, sau đó kéo cửa; hiểu kết quả bước đầu mở ra hành động bước sau | Hai cơ quan đáp lại nối tiếp, mỗi bước có tiến bộ giữ lại | Vừa / nhẹ |
| [26 · Bắc một nhịp](LevelDesign/COghe/Level26/README.md) | 18 | Đẩy một mô-đun cầu tới vị trí nối, rồi leo qua | Một nhịp cầu hoàn chỉnh, COghe bò qua chính vật vừa đặt | Nhẹ / nhẹ |
| [27 · Một bánh, hai việc](LevelDesign/COghe/Level27/README.md) | 21 | Đưa cùng một bánh răng tới hai trạm để mở hai cơ quan nối tiếp | Bánh răng được dùng lần hai, kết quả lần trước vẫn còn | Vừa / nhẹ |
| [28 · Nhường đường](LevelDesign/COghe/Level28/README.md) | 23 | Kéo vật cản vào hốc trước khi đẩy giá bánh răng | Vật từng chắn trở thành thứ được cất gọn; bộ máy chạy thông | Vừa / nhẹ |
| [29 · Bạn giữ, tớ kéo](LevelDesign/COghe/Level29/README.md) | 26 | Một phần giữ ly hợp, phần kia kéo cửa tới chốt; sau đó tụ lại | Cửa mở đủ và giữ nguyên, cả hai phần vui vẻ gặp lại | Vừa / nhẹ, dung sai chia mô cần đo |
| [30 · Nhà máy tí hon](LevelDesign/COghe/Level30/README.md) | 10 · BOSS | Một cơ thể nối bộ truyền để mở quyền tiếp cận tay nắm, rồi kéo cửa cuối | Cỗ máy sáng và chạy theo từng kết quả; chiến thắng mở Nhà | Vừa / nhẹ; không hướng dẫn lời giải |

Màn 30 là Boss đầu tiên vừa sức: thử kết hợp những điều đã dùng, không tăng yêu cầu
độ chính xác hoặc thêm kỹ năng tách cơ thể. Màn 27 sau Boss phối hợp là một nhịp nghỉ
có kết quả cơ khí rõ. Những lần lắp ghép dùng ray/chặn thật, không kéo thả đồ từ xa.

## 2. Thứ tự đề xuất đủ 30 màn

Giữ ID của 20 màn cũ. Không thay ID bằng số hiển thị, không lấy tên thư mục làm
điều kiện vật lý/tiến trình. Cột “cũ” luôn chỉ bản 20 màn người dùng vừa test.

| Chơi ở vị trí | Nội dung | Vai trò trong nhịp học/chơi |
| ---: | --- | --- |
| 1 | Cũ 01 · Bò đi | Chạm đích và thoát |
| 2 | Cũ 02 · Leo đi | Qua vách, leo kính |
| 3 | Cũ 03 · Xoay đi | Nhìn/chọn mặt khác |
| 4 | Cũ 04 · Trơn đấy | Chạm lỗ lần đầu đi đường ngắn qua lớp trơn và rơi; người chơi tự chỉ tuyến kính khô để vòng qua |
| 5 | **Mới 21 · Nghiêng là tới** | Nghiêng–chờ với hình học thu mô rộng |
| 6 | Cũ 05 · Trượt đi — đề xuất chỉnh dung sai | Lật mặt trơn; giảm yêu cầu phanh/căn hai trục |
| 7 | Cũ 06 · Xoay tròn | Thử cùng trọng lực trên mặt cầu; theo dõi khó thao tác |
| 8 | Cũ 07 · Đẩy | Bám vật, chỉ đích để đẩy/kéo |
| 9 | **Mới 23 · Khớp rồi!** | Một giá bánh răng, một kết quả rõ |
| **10** | **Mới 30 · BOSS Nhà máy tí hon** | Một cơ thể, hai bước cơ khí quen; đề xuất mở Nhà |
| 11 | **Mới 22 · Đáp rồi chui** | Nhịp nghỉ; cú rơi ngắn và dòng mô qua ống |
| 12 | Cũ 08 · Chui qua lỗ — đề xuất chỉnh dung sai | Tăng không gian hai hộp, vẫn không thao tác giữa cú rơi |
| 13 | Cũ 09 · Lật đi | Vận dụng trọng lực làm nắp rơi |
| 14 | **Mới 24 · Kéo ra mới qua** | Một thao tác kéo dễ sửa; nghỉ sau xoay |
| 15 | Cũ 11 · Kê cao lên | Dùng vật làm bậc |
| 16 | **Mới 25 · Hai nhịp một cửa** | Chuỗi hai trạng thái giữ lại được |
| 17 | Cũ 13 · Mở đường | Cần → cửa → nút → ống; dùng mẫu chuỗi đã biết |
| 18 | **Mới 26 · Bắc một nhịp** | Nghỉ bằng một mô-đun cầu |
| 19 | Cũ 16 · Ghép cầu | Ba mô-đun, không thêm kiểu input |
| **20** | **Cũ 10 · BOSS Chờ nhau** | Tự khám phá cắt, đổi phần, phối hợp, tụ; không gợi ý lời giải |
| 21 | **Mới 27 · Một bánh, hai việc** | Cơ khí một cơ thể, nghỉ sau Boss |
| 22 | Cũ 12 · Trượt rồi bay | Đổi nhịp sau cơ khí; theo dõi riêng thao tác lên máng và đón |
| 23 | **Mới 28 · Nhường đường** | Suy nghĩ thứ tự thay cho độ chính xác |
| 24 | Cũ 17 · Nối bánh răng | Bộ truyền dài hơn, phối hợp đẩy và kéo |
| 25 | Cũ 14 · Đổi chiều | Xoay làm cầu/cửa hoạt động; cần chơi thử thao tác lại |
| 26 | **Mới 29 · Bạn giữ, tớ kéo** | Quay về thao tác tĩnh; luyện hai vai trò sau Boss |
| 27 | Cũ 18 · Ghép đường | Cơ quan tự khớp theo trọng lực |
| 28 | Cũ 15 · Tìm lối ra | Nghỉ tay xoay, tập trung chọn nhánh ống |
| 29 | Cũ 19 · Cùng nhau | Hai khoang và hai vai trò |
| **30** | **Cũ 20 · BOSS Tam hợp** | Thử thách tổng hợp cuối chương |

Đây là thứ tự **đã được áp dụng vào scene tích hợp**, không chỉ cộng 10 màn vào cuối.
Mục đích là giữ mốc Boss 10/20/30 và dời thử thách nhiều phần cơ thể khỏi đầu game.
Catalog runtime mặc định dùng 30 scene `COgheOrigin01`…`COgheOrigin30` theo thứ tự này.

Nhà theo ý đồ sản phẩm tiếp tục mở ở Boss đầu tại vị trí 10. Code hiện tại
`VenomCampaignSave.Win` mở Nhà khi thắng một definition có `Boss=true` và chưa có
quyền này; không hardcode ID 10. Giữ cơ chế đó, đánh dấu nội dung mới 30 là Boss,
để hành trình thông thường mở Nhà tại mốc 10 mới. Quyền đã có của người chơi cũ
phải giữ nguyên, replay không cấp trùng. Không đổi “đã thắng màn 10 cũ” thành
“đã thắng màn mới 30”; `Completed` hiện đã lưu theo ID. Cần kiểm tra catalog,
next/auto-advance và màn đã mở theo thứ tự mới, không tự đổi save schema khi chưa cần.

## 3. Màn 05/08 cần sửa thao tác, không chỉ thêm màn tập

### Những gì đã đọc được từ code

- Test 05 đảo hộp rồi cập nhật góc dựa trên vị trí/vận tốc mỗi 24 bước vật lý
  (0,2 giây ở 120 Hz). Nó chứng minh có đường giải vật lý, chưa chứng minh chơi tay dễ.
- 08 hiện đã tự chui sau khi bắt vành thật; vành bám bán kính 82 mm, lòng ống 21 mm.
  Test có tâm và lệch ngang ±35 mm. Không cần thêm yêu cầu bấm giữa lúc đang rơi.
- Kết quả trên là đọc code/test, không phải đo lại thiết bị hoặc test người chơi mới.
  Nguồn: `VenomCampaignBuilder`, `VenomOriginTests`, cơ chế bắt vành/ống trong Campaign.

### Đề xuất điều chỉnh 05

Giữ đảo nóc trơn xuống dưới và trọng lực thế giới. Thử mặt thu mô rộng với các
mặt dốc nông và thành dẫn thật tới lỗ, để nghiêng về một vùng đúng rồi chờ. Không
tăng lực hút từ xa, ma sát hoặc lực bám trên lớp trơn. Lỗ vẫn cắt phẳng qua vỏ;
phần thu là hình học dẫn bên trong, không tạo gờ chặn ngay miệng thoát.

Mục tiêu dựng thử: một khoảng góc hữu ích quanh tư thế úp, thử lệch ±15° và giữ ít
nhất 3 giây, không cần kéo sửa liên tục. Các số này là mục tiêu thử nghiệm, không
phải dung sai đã đạt. Kiểm tra đủ vật chất thoát và mọi hướng sai vẫn phục hồi được.

### Đề xuất điều chỉnh 08

Rút ngắn cú rơi bằng cách thử nâng **cả tuyến ống và hai miệng** 60–80 mm, kèm vùng
tiếp cận rộng và hai má dẫn thật. Cú rơi vẫn phải xảy ra; sau tiếp xúc vành thật,
hành vi tự chảy hiện có tiếp tục. Rơi ngoài vùng đón xuống sàn an toàn và đường leo
thử lại phải nhìn thấy từ camera cố định.

Thử lệch ngang 0, ±35, ±60 mm; không ra lệnh mới sau khi rời mép. Không chỉ tăng
`GripRadius`: auto-enter hiện còn kiểm tra tiếp xúc, tâm trong 0,11 m và vận tốc
≤0,25 m/s. Nếu bám được ở ngoài phạm vi vào ống, phải bò tới miệng bằng đường thật
hoặc sửa hình học dẫn; không để tạo trạng thái “bắt được nhưng đứng mãi”. Kích thước
đề xuất cần được xác nhận bằng prototype, chưa thay scene đã duyệt trong lượt này.

Giữ phác thảo cũ làm lịch sử. Ghi phiên bản chỉnh trong hồ sơ 05/08 khi dựng, không
tự chuyển hai màn thành bộ sưu tập thử thách trả phí hoặc đặt thêm chế độ chơi.

## 4. Hợp đồng điều khiển và vật lý cho màn mới

- Chạm tay nắm không quay để bám, chạm vùng đích để cơ thể đẩy/kéo vật thật; không
  chạm kéo trực tiếp vật thể bằng tay người chơi. Ray dẫn một bậc tự do, có chặn và
  chốt cuối hành trình. Đích rộng dẫn về cùng hành trình, không cần “đúng pixel”.
- Chốt chỉ giữ sau khi cơ quan thực tới vị trí; cơ thể truyền lực có giới hạn. Không
  snap/teleport bánh răng vào ổ, không mở cửa chỉ từ ID “đặt đúng ô”.
- Tự buông sau 3 giây không lệnh áp dụng tác vụ prop; giữ nút khi chọn phần khác
  vẫn tiếp tục. Các thao tác dài phải đo thời gian/thể hiện đang kéo, tránh timeout
  giữa một hành trình bình thường; kết quả dở dang phải đứng lại hoặc hồi phục rõ.
- Không giảm luật trơn, dùng khóa vô hình để cấm leo tắt, hoặc chặn tụ theo nhiệm vụ.
  Vách/cửa thật bảo vệ lối ra; cơ cấu cần bám và sức lực tương ứng khối lượng.
- Bánh nguồn có động cơ mô-men hữu hạn được thể hiện. Bánh trung gian quay chỉ khi
  thật sự ăn khớp; cửa chuyển theo tải và bộ truyền, dừng/chốt ở cuối hành trình.
- Boss 30 mới không có dấu đáp án, số thứ tự giải hoặc tự gợi ý sau vài lần sai.
  Biểu tượng thao tác, vật liệu và phản hồi cơ quan vẫn nhất quán với các màn trước.

## 5. Dùng lại kiến trúc và kiểm chứng

Tận dụng `VenomMovableProp`, `COgheRailSlider`, `COgheGearTrain`, `COgheTissueSensor`,
`COgheGuillotine`, camera vùng và đường đi bề mặt. Mỗi hồ sơ phân biệt cấu hình dùng
lại với driver còn phải viết. `COgheAssemblyBridge.Ready` hiện mặc định đúng ba ray;
không giả định ghép cầu một ray đã được hỗ trợ đầy đủ. Không đổi core để đáp ứng
đẹp hình mà chưa có test tương ứng.

Đây là đợt giảm độ khó thao tác; **người đã hiểu lời giải** là nhóm test chính bên
cạnh người chơi mới. Ghi số lần kéo sửa, rơi hụt, chạm nhầm, thời gian quay lại thử
và cảm giác hoàn tất; tách khỏi thời gian suy luận. Các mốc đích thử nghiệm:

- Với cơ quan ray: sau khi biết phải làm gì, một lệnh đích rộng đưa được cơ quan
  tới đầu hành trình; không phải spam tap/bám lại vì timeout. Thử kéo ngược/hủy.
- Với 21/22 và 05/08 chỉnh: đo một tập góc/đích chạm lệch, không dùng bộ điều khiển
  tự hiệu chỉnh để chứng minh “dễ”. Thao tác sai nhẹ phải thử lại tại chỗ được.
- Với 29: nhiều tỷ lệ cắt quanh vùng hợp lệ đều đủ giữ/kéo; ba vai trò khối lượng,
  giữ nút và khoảng cách chống một cơ thể làm cả hai phải được đo, không đoán.
- Khi giải xong, cơ quan đã đạt giữ trạng thái bằng chốt; sinh vật có đường thoát
  rõ, không thêm thao tác khó ở bước cuối. Toàn bộ bản thể phải thoát mới thắng.
- Benchmark có idle/move/ra lệnh/đổi cơ quan/cắt tụ/đổi scene và lượt chơi dài;
  so với cùng thiết bị/build. Chưa có FPS hay completion rate cho 10 màn này.

Các rủi ro còn cần theo dõi: 06/09 vẫn cần xoay, 12/14/18 vẫn có thao tác không gian,
Boss cũ 10 vẫn giới thiệu cắt/phân vai cùng lúc. Dời muộn không chứng minh chúng đã
vừa sức. Nếu playtest tiếp tục báo khó điều khiển, sửa vùng thao tác/hình học trước
khi tăng gợi ý lời giải hoặc thêm màn dạy.

## 6. Trạng thái triển khai và kiểm chứng

- `VenomCampaign30Builder` tạo 30 scene tích hợp, clone nội dung cũ theo stable ID và
  dựng nội dung mới 21–30 ở đúng vị trí hiển thị. Boss nằm tại 10/20/30.
- Cơ quan mới dùng driver nhỏ, tái sử dụng ray, prop, bánh răng, nút, máy chém và
  cửa vật lý hiện có. Trạng thái mở lối ra đọc từ hành trình thật của cơ quan.
- `COgheCampaign30IntegrationTests` kiểm tra mapping/save ID, hợp đồng từng thiết kế,
  reset/idle, chọn cơ quan qua camera thật, trạng thái giải cơ khí và ngân sách mô phỏng.
- Bộ test riêng có **7/7** bài qua trên Unity macOS ngày 17/09/2026. Mười màn mới
  có thời gian mô phỏng script trung bình khoảng **0,26–0,41 ms/bước** trong phép đo
  batch hiện tại và không tăng bộ nhớ managed vượt ngưỡng 2 MB của test.
- Hồi quy cuối đạt **278/278 PlayMode**, **8/8 EditMode** và build macOS Development
  thành công với đúng 30 scene tích hợp.
- Cần tiếp tục playtest tay trên Android/iOS để hiệu chỉnh vùng chạm, camera và độ khó;
  số đo batch không được dùng thay cho FPS thực trên thiết bị.

Danh sách máy đọc và thứ tự đầy đủ: [campaign-30-proposal.json](LevelDesign/COghe/campaign-30-proposal.json).
