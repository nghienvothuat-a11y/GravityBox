# Đường trên kính — màn 66–71 (bản thử)

Mrk 18/10/2026: kính trơn là vật cản; bố trí đường đi và cơ quan trên các mặt kính để người chơi xoay camera tìm đường.
Chỉ xoay camera, hộp đứng yên. Trần không bấm được nên không có cơ quan, không có lỗ thoát trên trần.
Màn thử: 66 dễ, 67 vừa, 68 khó; 69 cực khó (Mrk: "68 chưa khó lắm, làm 69 cực khó với các cơ quan kết hợp").
70 và 71 (Mrk 19/10/2026: thêm 2 màn dạng này để thay các màn lặp thao tác, giữ tổng 65 màn) dự kiến vào vị trí 40 và 52;
thứ tự thay thế chờ Mrk chốt, hiện cả sáu màn vẫn nằm sau 65. Code: `Assets/_Game/Editor/COgheGlassRoadLevels.cs`.

## Luật chung

- **Đường:** dải kính trần trên vách. Hai bên mỗi đường có hàng rào trơn rộng 12 cm, hơn tầm với của COghe (~10 cm: nửa thân
  7,4 cm và 3,4 cm bám). Chân mọi vách có dải trơn 12 cm, trừ chỗ đường bắt đầu từ sàn.
  Hàng rào chạy tiếp qua góc sang vách bên cạnh, trừ khi có đường nối sang đó.
- **Phần kính còn lại để trong:** không có đường nào chạm tới nên không bò lên được. Trần cũng trong: mọi đường dừng thấp hơn
  trần 12 cm. Hộp nhìn sạch, đường là làn trong nằm giữa hai vạch tím.
- **Cơ quan:**
  - tấm phủ trơn che chân đường hoặc chắn ngang đường, kéo tay nắm thì trượt đi;
  - tay nắm trên kệ;
  - tay nắm treo trên kính (`GlassHandle`): COghe đứng trên đường ngay dưới để kéo. Thân tay nắm nằm dọc theo kính, cách
    kính 5 mm, trơn mọi mặt: không trèo lên được (70: đứng trên thân B thì với tới kính trần phía trên hàng rào);
  - thang máy chở COghe từ đường thấp lên đường cao (70);
  - nút cần nửa thân giữ cửa, tay nắm chốt cửa mở hẳn (71).
- **Lỗ thoát:** nằm trên vách, trong một đường, khoá bằng cửa trượt của cơ quan cuối. Riêng màn 66, lỗ thoát mở sẵn.

## Các màn

| Màn | Khoá | Tên | Lời giải |
| --- | --- | --- | --- |
| 66 | G6 | Đường trên kính (dễ) | A mở chân đường ở góc sau-phải → bò lên vách sau, qua góc sang vách phải → lỗ thoát mở sẵn. |
| 67 | G7 | Sau tấm chắn (vừa) | A mở chân đường khuất sau tấm chắn (xoay camera để thấy) → leo vách sau, rẽ sang vách trái → kéo B treo trên kính → qua góc sang vách trước → lỗ thoát. |
| 68 | G8 | Bốn mặt kính (khó) | A mở chân đường vách trước → leo, qua góc sang vách trái, lên kệ → B hạ tấm chắn ngang đường vách sau → theo vách sau sang vách phải → kéo C treo trên kính → lỗ thoát. Đường vách trước chạy về phía lỗ thoát nhưng cụt. |
| 69 | G9 | Hai nửa, bốn vách (cực khó) | Đường như 68. Cửa chắn giữa đường vách sau chỉ mở khi P1 (sàn) hoặc P2 (kệ phải) bị đè. Lỗ thoát cần cả thân. A mở chân đường vách trước → tách ở Q → nửa 1 đứng P1 → nửa 2 leo vách trước, vách trái, qua cửa, đứng P2 → nửa 1 rời P1, leo cùng đường, qua cửa → kéo C treo trên kính (cách lỗ thoát ~19 cm để lỗ không hút nửa thân trước khi nhập) → nhập trên kệ phải → ra. |
| 70 | G10 | Thang máy giữa hai đường (vừa–khó, cho vị trí 40) | Đường thấp lên vách trái; đường cao từ góc sau-phải chạy qua vách sau, vách phải tới lỗ thoát, cao hơn sàn thang máy 14 cm. Leo đường thấp, kéo B treo trên kính: mở khoá thang máy → xuống sàn → lên thang máy, thang đưa lên chân đường cao → theo vách sau, qua góc sang vách phải → lỗ thoát. |
| 71 | G11 | Nửa giữ, nửa qua (khó, cho vị trí 52) | Cửa chắn giữa đường vách sau chỉ mở khi nút P (dưới sàn, trước kệ trái, rộng 14 cm) có ít nhất nửa thân đè (một phần tư không đủ), hoặc mở hẳn khi kéo C. Lỗ thoát ở vách trái, cần cả thân. Tách ở Q → nửa 1 đứng P → nửa 2 leo vách trước, vách phải lên kệ phải, qua cửa sang kệ trái → kéo C treo trên kính: cửa chốt mở, lỗ thoát mở → nửa 1 rời P, đi cùng đường → nhập trên kệ trái → ra. 69 sau đó bỏ chốt: phải đổi người giữ cửa. |

## Kiểm tra

Với cả sáu màn, các test sau đều qua (69 có thêm `GlassRoadG9SolveAt30Fps`):
- `SpatialPlusG6–G11Solve`, `SpatialPlusG6–G11Wander`, `GlassRoadG6–G11SolveAtFrameRate` (60 khung/s);
- `GlassRoadG6–G11NoShortcut`: chạm cao trên kính, lên kệ, vào lỗ thoát mà chưa làm cơ quan thì không thắng (70: bỏ qua B,
  vì B nằm trên đường và được kéo ngay từ đầu);
- `GlassExitTapSweep`: đặt COghe lên mọi chỗ, rồi chạm lỗ thoát. Ở 66 chỉ "thắng" khi đặt COghe lên đỉnh tấm phủ, chỗ người chơi
  không tới được. Ở 67–71 không chỗ nào thắng
  (19/10, `Artifacts/GlassAudit/exit-sweep-g6to11.tsv`).

Phim: Explicit `GlassRoadFilm` (env `GLASSROAD_FILM`).
Clip: `OUTBOX/COGHE_GLASS_ROADS_2026_10_18/COGHE_GLASS_ROADS_66_68.mp4`, `COGHE_GLASS_ROAD_69.mp4` và
`COGHE_GLASS_ROADS_70_71.mp4` (máy của Claude).

Bài học khi dựng: bộ tìm đường nối các điểm cách nhau tới 10 cm, kể cả qua chỗ trống. Vì vậy chỗ bước từ kệ sang kính phải có
kệ đỡ bên dưới (màn 68 và 69: kệ chạy dọc theo vách sau).
- Hai nửa đi ngang qua gần nhau thì tự nhập lại, nên nút giữ cửa phải nằm ngoài đường nửa kia đi (69: P1 dưới sàn).
- Chạm thẳng vào đích ở xa thì bộ tìm đường có thể chọn leo đoạn kính trơn ngắn (rồi tuột) thay vì đi đường dài. Lời giải mẫu chạm từng chặng, như người chơi.
  Từ 19/10, ở các màn đường kính (cờ `NoSlickClimb` trên bốn vách và thân tay nắm), bộ tìm đường (`VenomCampaignMotion.FindPath`)
  không đi lên điểm trên vách cao hơn chân vách 6 cm nếu chỗ đó trơn. Các màn cũ giữ nguyên: bài 4 của Origin và màn 04 cố ý
  để COghe leo kính trơn rồi tuột cho người chơi tự học (áp luật cho mọi màn làm hỏng 10 test, gồm 14, 28, 30 và N29).
- Thân tay nắm và các khối khác đặt sát kính là bậc đứng: nếu trần, COghe đứng lên đó và với qua hàng rào (70, thân B).
  Đường phải chạy tới sát trần (hàng rào chạm trần) và thân tay nắm phải trơn.
- Nút cần đúng nửa thân phải rộng: nửa thân dừng ở điểm cuối đường đi, có thể lệch tâm nút vài cm. Nút 10 cm ở 71 bị lệch 4 cm,
  chỉ đo được 0,024–0,027 (cần 0,030). Nút 14 cm đủ chỗ, một phần tư thân (0,024) vẫn không mở được.
- Lỗ thoát mở ra hút phần ở gần, nên tay nắm cuối phải cách lỗ thoát đủ xa khi còn tách đôi.
