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
  - bánh răng dựng trên kính, tay nắm treo trên kính đẩy bánh thiếu vào dàn; bến đáp do bánh răng đẩy; dây đu (71).
- **Lỗ thoát:** nằm trên vách, trong một đường, khoá bằng cửa trượt của cơ quan cuối. Riêng màn 66, lỗ thoát mở sẵn.

## Các màn

| Màn | Khoá | Tên | Lời giải |
| --- | --- | --- | --- |
| 66 | G6 | Đường trên kính (dễ) | A mở chân đường ở góc sau-phải → bò lên vách sau, qua góc sang vách phải → lỗ thoát mở sẵn. |
| 67 | G7 | Sau tấm chắn (vừa) | A mở chân đường khuất sau tấm chắn (xoay camera để thấy) → leo vách sau, rẽ sang vách trái → kéo B treo trên kính → qua góc sang vách trước → lỗ thoát. |
| 68 | G8 | Bốn mặt kính (khó) | A mở chân đường vách trước → leo, qua góc sang vách trái, lên kệ → B hạ tấm chắn ngang đường vách sau → theo vách sau sang vách phải → kéo C treo trên kính → lỗ thoát. Đường vách trước chạy về phía lỗ thoát nhưng cụt. |
| 69 | G9 | Hai nửa, bốn vách (cực khó) | Đường như 68. Cửa chắn giữa đường vách sau chỉ mở khi P1 (sàn) hoặc P2 (kệ phải) bị đè. Lỗ thoát cần cả thân. A mở chân đường vách trước → tách ở Q → nửa 1 đứng P1 → nửa 2 leo vách trước, vách trái, qua cửa, đứng P2 → nửa 1 rời P1, leo cùng đường, qua cửa → kéo C treo trên kính (cách lỗ thoát ~19 cm để lỗ không hút nửa thân trước khi nhập) → nhập trên kệ phải → ra. |
| 70 | G10 | Thang máy giữa hai đường (vừa–khó, cho vị trí 40) | Đường thấp lên vách trái; đường cao từ góc sau-phải chạy qua vách sau, vách phải tới lỗ thoát, cao hơn sàn thang máy 14 cm. Leo đường thấp, kéo B treo trên kính: mở khoá thang máy → xuống sàn → lên thang máy, thang đưa lên chân đường cao → theo vách sau, qua góc sang vách phải → lỗ thoát. |
| 71 | G11 | Bánh răng và dây đu (khó, cho vị trí 52) | Mrk 19/10: bản đầu "Nửa giữ, nửa qua" giống 69 quá → làm lại với bánh răng và đu dây, bỏ tách đôi. Hộp cao hơn (trần 0,30) để có chỗ đu. Ba bánh răng dựng trên kính vách sau, bánh giữa G thiếu. Leo đường vách sau tới A treo trên kính → kéo A, G trượt vào dàn → xuống đứng nút P dưới sàn: bánh răng quay, bến đáp B trượt dọc vách phải vào dưới dây (chưa có G thì chỉ bánh đầu quay) → leo đường vách trái lên kệ trái → bám vòng B, chạm bến: đu sang (bến chưa vào thì đu hụt, tời kéo về) → từ bến leo đường vách phải lên lỗ thoát. Đường vách trước vòng sang vách phải là đường cụt dưới tầm lỗ thoát. |

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
- Nút cần đúng nửa thân phải rộng: nửa thân dừng ở điểm cuối đường đi, có thể lệch tâm nút vài cm. Ở bản đầu của 71, nút 10 cm
  bị lệch 4 cm, chỉ đo được 0,024–0,027 (cần 0,030); nút 14 cm thì đủ.
- Kệ và bến đặt sát vách có đường thì phần kính quanh nó phải là đường hoặc hàng rào. Kệ, bến đứng cách kính trần ít nhất 12 cm
  (71: kệ trái và bến đáp dừng trước kính vách sau 12 cm, nên từ đường vách sau không với tới).
- Biển số màn ở góc trên-trái kính trước. Camera nhìn chéo qua biển xuống phía sau-trái; ở 71 (hộp cao) đường nhìn rơi đúng chỗ
  COghe bám dây, nên biển được lật sang góc trên-phải (`plusPlaqueRight`).
- Lỗ thoát mở ra hút phần ở gần, nên tay nắm cuối phải cách lỗ thoát đủ xa khi còn tách đôi.
