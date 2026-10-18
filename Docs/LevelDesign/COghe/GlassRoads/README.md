# Đường trên kính — màn 66–69 (bản thử)

Mrk 18/10/2026: kính trơn là vật cản; bố trí đường đi và cơ quan trên các mặt kính để người chơi xoay camera tìm đường.
Chỉ xoay camera, hộp đứng yên. Trần không bấm được nên không có cơ quan, không có lỗ thoát trên trần.
Màn thử: 66 dễ, 67 vừa, 68 khó; 69 cực khó (Mrk: "68 chưa khó lắm, làm 69 cực khó với các cơ quan kết hợp"). Code: `Assets/_Game/Editor/COgheGlassRoadLevels.cs`.

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
    kính 5 mm.
- **Lỗ thoát:** nằm trên vách, trong một đường, khoá bằng cửa trượt của cơ quan cuối. Riêng màn 66, lỗ thoát mở sẵn.

## Ba màn

| Màn | Khoá | Tên | Lời giải |
| --- | --- | --- | --- |
| 66 | G6 | Đường trên kính (dễ) | A mở chân đường ở góc sau-phải → bò lên vách sau, qua góc sang vách phải → lỗ thoát mở sẵn. |
| 67 | G7 | Sau tấm chắn (vừa) | A mở chân đường khuất sau tấm chắn (xoay camera để thấy) → leo vách sau, rẽ sang vách trái → kéo B treo trên kính → qua góc sang vách trước → lỗ thoát. |
| 68 | G8 | Bốn mặt kính (khó) | A mở chân đường vách trước → leo, qua góc sang vách trái, lên kệ → B hạ tấm chắn ngang đường vách sau → theo vách sau sang vách phải → kéo C treo trên kính → lỗ thoát. Đường vách trước chạy về phía lỗ thoát nhưng cụt. |
| 69 | G9 | Hai nửa, bốn vách (cực khó) | Đường như 68. Cửa chắn giữa đường vách sau chỉ mở khi P1 (sàn) hoặc P2 (kệ phải) bị đè. Lỗ thoát cần cả thân. A mở chân đường vách trước → tách ở Q → nửa 1 đứng P1 → nửa 2 leo vách trước, vách trái, qua cửa, đứng P2 → nửa 1 rời P1, leo cùng đường, qua cửa → kéo C treo trên kính (cách lỗ thoát ~19 cm để lỗ không hút nửa thân trước khi nhập) → nhập trên kệ phải → ra. |

## Kiểm tra

Với cả bốn màn, các test sau đều qua (69 có thêm `GlassRoadG9SolveAt30Fps`):
- `SpatialPlusG6–G9Solve`, `SpatialPlusG6–G9Wander`, `GlassRoadG6–G9SolveAtFrameRate` (60 khung/s);
- `GlassRoadG6–G9NoShortcut`: chạm cao trên kính, lên kệ, vào lỗ thoát mà chưa làm cơ quan thì không thắng;
- `GlassExitTapSweep`: đặt COghe lên mọi chỗ, rồi chạm lỗ thoát. Ở 66 chỉ "thắng" khi đặt COghe lên đỉnh tấm phủ, chỗ người chơi
  không tới được. Ở 68 và 69 không chỗ nào thắng.

Phim: Explicit `GlassRoadFilm` (env `GLASSROAD_FILM`).
Clip: `OUTBOX/COGHE_GLASS_ROADS_2026_10_18/COGHE_GLASS_ROADS_66_68.mp4` và `COGHE_GLASS_ROAD_69.mp4` (máy của Claude).

Bài học khi dựng: bộ tìm đường nối các điểm cách nhau tới 10 cm, kể cả qua chỗ trống. Vì vậy chỗ bước từ kệ sang kính phải có
kệ đỡ bên dưới (màn 68 và 69: kệ chạy dọc theo vách sau).
- Hai nửa đi ngang qua gần nhau thì tự nhập lại, nên nút giữ cửa phải nằm ngoài đường nửa kia đi (69: P1 dưới sàn).
- Chạm thẳng vào đích ở xa thì bộ tìm đường có thể chọn leo đoạn kính trơn ngắn (rồi tuột) thay vì đi đường dài. Lời giải mẫu chạm từng chặng, như người chơi.
- Lỗ thoát mở ra hút phần ở gần, nên tay nắm cuối phải cách lỗ thoát đủ xa khi còn tách đôi.
