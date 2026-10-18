# COghe — luật bám kính, 14/10/2026

## Revision riêng màn 6 — 15/10/2026

Sửa theo yêu cầu mới nhất: riêng màn 6 (K01) có lỗ dưới sàn, nằm dưới khối đỏ.
Dời khối đỏ để lộ lỗ và thoát xuống; bỏ toàn bộ công tắc, nút, nắp và cửa.
Đây là ngoại lệ mới cho luật lỗ trên kính ở các màn đầu; kết quả bên dưới
là lịch sử của lượt kiểm tra 14/10, không đại diện cho bố cục K01 hiện tại.
[Thiết kế và kiểm chứng mới](../COgheCrateReveal6/README.md).
Kết quả bên dưới ghi nhận bản luật kính trước thay đổi cơ quan này.

Yêu cầu: kính để trần luôn bám được; chỉ vật liệu trơn làm mất bám. Phạm vi là
**65 màn trong `SpatialOrder`**, không đổi catalog lịch sử, ID hay save.

## Bố cục mở đầu

| Màn hiển thị | Content | Thay đổi | Chuỗi giải giữ lại |
|---|---|---|---|
| 1 | 01 | Lỗ ở cao 20 cm trên sàn, trên tường kính. Không dải trơn. | Chạm, bò trên sàn rồi leo kính ra lỗ. |
| 2 | 02 | Bỏ tấm bám phụ và mặt trơn của bậc; lỗ trên kính cao hơn hai bậc. | Leo bậc hoặc kính để tới lỗ. |
| 3 | 03 | Lỗ nâng lên kính, vẫn sau vách quan sát. | Đổi góc nhìn, đi quanh vách, leo tới lỗ. |
| 4 | 04 | Lỗ/cửa nâng lên kính; bỏ dải trơn quanh sàn. | Kéo tay nắm xanh, leo kính ra cửa đã mở. |
| 5 | 05 | Tay nắm và lỗ trên kính trần, bỏ hai tấm bám màu ngà. | Cần vàng mở hộp; tay nắm xanh mở cửa. |
| 6 | K01 | Lỗ chuyển từ sàn lên kính. Thùng đỏ nối cửa cùng màu, giữ bài toán xếp thùng. | Dời thùng cản, kéo thùng đỏ để mở cửa, leo kính. |
| 7 | 06 | Công tắc trên kính, bỏ panel bám. Dải phủ giữa hai phía nối qua nóc/mặt trước; chân kính phía nhận cầu có phủ trơn. | Leo kính kéo công tắc vàng, quay tời, qua cầu ròng rọc đã nâng, leo tới lỗ. |

Các mặt thùng có chủ ý trơn vẫn dùng vật liệu trơn; “bỏ quanh sàn” không biến các
thùng cản thành bậc leo mới. Các cơ quan vẫn dùng màu/circuit đã duyệt.

## Các màn sau

- G1–G5 giữ vùng phủ đã thiết kế riêng.
- Các tấm kính cũ bị cấm bám toàn mặt chuyển thành kính bám được. Lớp phủ thật đặt
  tại mép tiếp giáp sàn/bệ, đầu vách ngăn và đường chuyển lên nóc có thể bỏ qua cơ quan.
- Miệng lỗ luôn cắt tròn đúng hình học, không có mesh phủ ngang lỗ.
- Màn đu dây (hiển thị 33, content 18) có dải trơn nối mặt sau, nóc và mặt trước,
  ngăn bò qua kính giữa hai bến. Phải dùng dây để vượt khoảng trống.
- Nhóm bệ có bậc kéo: bệ cao 8 cm, bậc mở cao 5 cm (còn 3 cm tới mặt bệ).
  Việc tăng chiều cao bệ chặn bước leo trực tiếp từ sàn; cơ quan kéo bậc vẫn phải
  hoạt động trước. Bậc sâu 6 cm, thu vào cách mép hộc 7 cm, tránh tiếp xúc từ sàn
  và không va tường phía sau. Bài bánh răng đổi chiều chỉ để bậc hé 1,2 cm trong hộc lúc đầu,
  để nhìn được chiều chạy sai nhưng chưa tạo chỗ đứng. Kiểm chứng dùng thao tác
  thật; không có khóa thắng vô hình.
- Các hình chữ nhật phủ được hợp nhất thành vùng không chồng lấn, tránh nhiễu chiều sâu.
- Không thêm collider hay component chạy mỗi frame cho lớp phủ. Mesh ghép theo từng
  mặt kính, không tăng số hạt, tần số vật lý hoặc đèn realtime.

`COgheGlassRuleAuthoring.Configure` ghi dữ liệu lúc author/build scene;
`COgheSoftEdges` chỉ vẽ đúng dữ liệu `SlipRegions`. Không có luật vật lý theo số màn.
Luật/ngoại hình này thay thế phương án A ngày 09/10. Cấu hình builder và scene được
cập nhật cùng nhau. `MigrateSavedScenes` cập nhật scene đã có mà không dựng lại cơ quan.

## Kiểm chứng

Các kết quả theo từng lượt nằm trong `Artifacts/GlassRule20261014`:

- `campaign-v2.xml`: 65/65 lời giải qua màn. Kiểm tra kính toàn catalog, leo/xuống
  kính trần và chống bò vòng ở màn 7 đạt. Quét chạm thẳng lỗ phát hiện 5 đường tắt.
- Đường bò kính qua hai bến ở màn 33 được chặn bằng dải phủ liên tục. Bốn đường
  còn lại (18/28/38/45) liên quan bậc trong hộc; đã sửa hình học tại helper dùng chung.
- `platform-probe-v6.xml`: bệ mẫu qua kiểm tra lời giải và không thể đi thẳng tới lỗ
  khi bậc đóng. Bài chẩn đoán tạm được lưu trong Artifacts, bỏ khỏi bộ test chính;
  kiểm tra quét cả catalog vẫn được giữ trong `COgheGlassRuleTests`.
- `recovery.xml` và `diagnostic.xml`: kiểm tra đi lạc rồi quay lại các màn mở đầu,
  vật liệu trơn G2 và giải G1 với nhịp render mô phỏng 30 fps. Đã sửa giới hạn hộp
  trong test đi lạc để dùng đúng kích thước hộp thùng K01.

Kết quả cuối của kiểm tra Play Mode:

- `final-checks.xml`: quy tắc kính đủ 65 màn; quét chạm thẳng lỗ ở 62 màn có cơ
  quan **không còn đường tắt**; lời giải content 16/18/22 và wander N34 đạt.
- `final-recovery.xml`: **6/6 đạt**, gồm wander rồi Retry + solve ở
  N40/N41/N42/N44/N47, và solve N15.
- Các lời giải sau thay đổi chiều cao bậc lấy điểm chạm từ mặt trên đang hiện của
  bậc/bệ. Không đặt lại thân sinh vật, cơ quan, trạng thái cửa hay thắng trong test.
  Cách này xử lý cả bậc đang chốt và mặt tĩnh sau khi chốt.
- Toàn bộ 65 màn có lời giải đã đạt: lượt toàn catalog ban đầu cộng lượt chạy lại
  các màn chịu ảnh hưởng của thay đổi sau cùng. Không dùng các lượt thăm dò thất bại
  làm bằng chứng hoàn tất.

## Bản Mac đã chạy thực

Build thành công: `Builds/SpatialLab/macOS/COghe.app` (log `build-mac.log`).
Replay dùng Input System touch events, vòng lặp player thường, không tăng tốc,
không thay vị trí thân hay trạng thái cơ quan. Cả **7/7 màn đạt**, không có lỗi runtime.
Không ghi tiến trình của lượt replay vào save của người chơi.

Thiết bị: Apple M5, Metal, 540 × 960, Glass C. Kết quả này là trên Mac, không phải
số đo điện thoại. Thời gian chụp ảnh và chuyển scene không tính vào mẫu frame.

| Màn | Qua màn | FPS TB | P95 frame |
|---|---|---|---|
| 1 | 32/32 hạt, 1 bản thể | 59.6 | 16.9 ms |
| 2 | 32/32 hạt, 1 bản thể | 60.0 | 16.7 ms |
| 3 | 32/32 hạt, 1 bản thể | 59.7 | 17.0 ms |
| 4 | 32/32 hạt, 1 bản thể | 59.8 | 16.8 ms |
| 5 | 32/32 hạt, 1 bản thể | 59.2 | 17.1 ms |
| 6 | 32/32 hạt, 1 bản thể | 59.5 | 16.9 ms |
| 7 | 32/32 hạt, 1 bản thể | 59.5 | 17.0 ms |

[Xem ảnh các màn](review.html) · [Số đo player](native-summary.json).
Các ảnh 1–7 trong gallery lấy từ bản Mac; ảnh leo kính ở cuối lấy từ bài test Unity.

## Sửa đường tắt qua kính, 17/10/2026

Tìm ra từ 6 test hỏng tồn đọng (Mrk: "trong quá trình test có lỗi thì fix bugs"). Dải phủ 9 cm hẹp hơn tầm với
của COghe: đứng ở mép một bệ, thân vẫn bám được kính trần ngay cạnh dải. Hai màn sửa riêng, không đổi luật chung:

- **Boss 10 (`10`):** từ sàn thang máy, COghe bò lên kính sau tới ban công xa và kéo D khi cầu B chưa nâng.
  Bộ dựng (`COgheGlassRuleAuthoring.LiftCoatings`) giờ phủ kính dọc mép sàn thang suốt hành trình, rộng thêm
  9 cm mỗi bên. `Boss10RejectsShortcutsAndRecoversAfterReversingBridge`, solve và wander màn 10 đạt.
- **E10:** trượt khỏi dải ở chỗ vách chuồng thoát chạm kính trước, thân rơi vắt qua vách, một giọt lọt vào chuồng
  nên kẹt. Từ góc bờ đầu, COghe với qua dải ở kính sau rồi bò kính sau và kính phải vào chuồng, bỏ qua dây đu.
  Cả hai dải rộng hơn về phía phòng (`RoomStripe`: kính trước x 0–0,07 m, kính sau x −0,115 – −0,045 m).
  `SpatialPlusE10Wander` và `SpatialPlusE10Solve` đạt.
- Scene đã lưu cập nhật bằng `MigrateSavedScenes -coghe-glass-levels 10` và `E10`.
- Các màn khác vẫn dùng dải 9 cm; wander của chúng đạt, nhưng wander không dò hết mọi đường tắt.

## Rà dải phủ 65 màn và sửa 6 màn, 18/10/2026

Mrk: "rà soát lại một lượt 65 màn", rồi "sửa những màn này đi". Bài rà là các test Explicit trong
`COgheGlassStripeAuditTests`, chỉ đo, không đổi màn:

- `GlassStripeReachAudit`: đặt COghe ở hai đầu và giữa mỗi mép bệ sát kính, kể cả nóc thùng/khay/sàn di động lúc đầu màn.
  Ra lệnh leo lên kính trần ngay trên hoặc cạnh dải. Tính "lên kính" khi treo trên kính trần bằng ít nhất 3 hạt, không đứng
  trên gì. Sau đó thử tới các bệ cao hơn, tới lỗ thoát, và quay về.
- `GlassStripeShortcutCheck` / `Detail`: làm lại mỗi chỗ leo được từ đầu màn thật (đi tới, leo, chạm lỗ thoát).
- `GlassStripeShortcutWins`: thắng thật hay không theo `Owner.Completed` (có thể quay phim, `AUDIT_FILM`).
- `GlassExitTapSweep`: đặt COghe lên mọi chỗ và mọi mặt bệ, chạm lỗ thoát, chờ 40 giây.

Kết quả trước khi sửa: 40/59 màn (7–65) có chỗ COghe lên được kính. 6 màn thắng được mà không giải đố, 7 đường:
9 (từ sàn thang máy), 34/N23 (từ sàn; từ bậc thang đầu), 36/21 và 37/N25 (từ bậc cứu hộ), 43/20 (từ góc sàn), 62/29 (từ bậc
thang đầu). Nguyên nhân: sàn di động không có dải riêng; hạt bám kính trần ngay ngoài đầu dải của một bậc hay khối thấp (3 mm–3 cm);
mặt bên ngà của bờ đầu chạm kính (dải vách chỉ ±4,5 cm); khối C nghiêng cách kính 8 cm; khe kính trần 3,7 cm giữa hai dải.

Sửa trong `COgheGlassRuleAuthoring`, chỉ cho các màn đó (màn khác giữ dải đã duyệt):
- `09`: `LiftCoatings` như Boss 10 (phủ dọc đường thang chạy).
- `WideEnds` = N23, 21, N25, 20, 29:
  - dải trên bệ và dải ở chỗ vách chạm kính dài thêm 9 cm mỗi bên;
  - mặt bệ cách kính tới 10 cm cũng được phủ, tính từ điểm cao nhất của mặt (khối nghiêng);
  - khe kính trần hẹp hơn 10 cm giữa hai dải được phủ (`CloseGaps`). Vòng quanh lỗ thoát vẫn để trần.
- Scene cập nhật bằng `MigrateSavedScenes -coghe-glass-levels 09,N23,21,N25,20,29`. Chạy lại bằng code cũ trước khi đổi thì
  `SlipRegions` y hệt, chỉ mesh băng dán được dựng lại.

Sau khi sửa: 7 đường cũ không thắng, không hạt nào bám kính trần. Quét chạm lỗ thoát chỉ thắng khi COghe đã ở khu lỗ thoát
(tháp/bệ thoát, khay đáp cạnh lỗ), tức bước cuối đúng thiết kế. Solve, wander và climb-back của 6 màn đạt. Test toàn bộ 890/0.
Clip trước/sau: `OUTBOX/COGHE_GLASS_STRIPE_2026_10_18/COGHE_GLASS_FIX_BEFORE_AFTER.mp4` (máy của Claude).
