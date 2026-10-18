# Đường trên kính — màn 66–68 (bản thử)

Mrk 18/10/2026: kính trơn là vật cản; bố trí đường đi và cơ quan trên các mặt kính để người chơi xoay camera tìm đường.
Chỉ xoay camera, hộp đứng yên. Trần không bấm được nên không có cơ quan, không có lỗ thoát trên trần.
Ba màn thử: 66 dễ, 67 vừa, 68 khó. Code: `Assets/_Game/Editor/COgheGlassRoadLevels.cs`.

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

## Kiểm tra

Với cả ba màn, các test sau đều qua:
- `SpatialPlusG6–G8Solve`, `SpatialPlusG6–G8Wander`, `GlassRoadG6–G8SolveAtFrameRate` (60 khung/s);
- `GlassRoadG6–G8NoShortcut`: chạm cao trên kính, lên kệ, vào lỗ thoát mà chưa làm cơ quan thì không thắng;
- `GlassExitTapSweep`: đặt COghe lên mọi chỗ, rồi chạm lỗ thoát. Ở 66 chỉ "thắng" khi đặt COghe lên đỉnh tấm phủ, chỗ người chơi
  không tới được. Ở 68 không chỗ nào thắng.

Phim: Explicit `GlassRoadFilm` (env `GLASSROAD_FILM`).
Clip: `OUTBOX/COGHE_GLASS_ROADS_2026_10_18/COGHE_GLASS_ROADS_66_68.mp4` (máy của Claude).

Bài học khi dựng: bộ tìm đường nối các điểm cách nhau tới 10 cm, kể cả qua chỗ trống. Vì vậy chỗ bước từ kệ sang kính phải có
kệ đỡ bên dưới (màn 68: kệ chạy dọc theo vách sau).
