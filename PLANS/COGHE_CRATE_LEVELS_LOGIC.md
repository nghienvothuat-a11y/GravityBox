---
title: "Mười màn thùng làm lại cho đúng logic (51–60)"
tags: [coghe, crate-levels, level-design]
status: active
created: 2026-10-06
---

# Mười màn thùng làm lại cho đúng logic (51–60)

Mrk (06/10/2026), xem màn 52 của bộ cũ: "về mặt hiển thị chỉ cần kéo miếng màu đỏ xuống phía dưới là xong. Tại sao phải đẩy
xong mới đẩy miếng đó. Mày phải sửa lại những màn kiểu này cho có tính logic".

Bản này thay mười thiết kế trong [COGHE_CRATE_LEVELS_10.md](COGHE_CRATE_LEVELS_10.md). Mrk duyệt (06/10/2026): "Ok vậy mày triển khai levels 51-60 theo phương án này".

## Vì sao bộ cũ trông vô lý

Bộ cũ có một luật không nhìn thấy được: muốn đẩy thùng, COghe phải có một ô trống ngay sau thùng; muốn kéo, phải có ô trống
ngay sau chỗ thùng dừng. Một thùng có đường trượt trống vẫn có thể "không kéo được". Kiểm lại: cả mười màn cũ, ngay từ đầu,
đều có thùng trượt được mà không kéo được; ở 51–54 đó chính là thùng đỏ. Nếu chỉ tính thùng chắn đường nhau thì cả mười màn cũ giải được trong 1–3 lần kéo, trong
khi lời giải thiết kế cần 2–15 lần.

## Luật mới (`Tools/crate_puzzles/logic.py`)

Kiểm trên mọi thế bày người chơi có thể tới, không chỉ trên lời giải:

1. Thùng nào có đường trượt trống thì có chỗ cho COghe đứng đẩy hoặc kéo (mặt thùng rộng hai ô cần cả hai ô). Chỉ thùng chắn
   đường mới giữ một thùng lại.
2. COghe chỉ đi trên sàn, không trèo qua thùng. Thùng cao hơn COghe để nhìn là biết. COghe bị thùng vây thì thấy ngay trên màn.
3. Thùng đỏ lúc đầu bị thùng khác chắn đường trượt (ý gốc của Mrk). Lần kéo thùng đỏ là lần kéo cuối.
4. Không có thế bày nào bị kẹt: kéo sai luôn kéo lại được.

## Mười màn

![Tổng quan](crate-levels-logic/overview_logic.png)

"Do chắn" là số lần kéo cần nếu COghe đi được khắp nơi: phần độ khó đến từ thùng chắn nhau. Phần còn lại đến từ việc phải mở
lối cho COghe.

| Màn | Tên | Lần kéo | Thùng | Kéo lại | Do chắn | Ý |
|-----|-----|---------|-------|---------|---------|---|
| 51 | Một thùng chắn | 2 | 3 | 0 | 2 | Thùng đỏ trượt được, nhưng một thùng đứng ngay trên đường trượt |
| 52 | Chuỗi ba thùng | 3 | 4 | 0 | 3 | Thùng chắn thùng đỏ lại bị thùng khác chắn: gỡ từ ngoài vào |
| 53 | Thùng dài | 4 | 4 | 0 | 4 | Thùng dài 3 ô cần một khoảng trống dài |
| 54 | Gỡ từng lớp | 5 | 5 | 0 | 5 | Năm thùng chắn nhau thành chuỗi |
| 55 | Thùng vuông | 6 | 5 | 1 | 5 | Thùng vuông lớn; một thùng dời đi rồi trả về |
| 56 | Dây chuyền | 6 | 6 | 0 | 6 | Sáu thùng chắn nhau thành một chuỗi |
| 57 | Mở lối cho COghe | 7 | 6 | 1 | 5 | COghe bị thùng vây trong góc: mở lối cho nó trước |
| 58 | Đi rồi trả lại | 8 | 6 | 2 | 4 | Hai thùng dời tạm rồi trả về |
| 59 | Kho chật | 9 | 6 | 3 | 4 | Một thùng dời qua lại ba lần |
| 60 | Mê cung thùng | 10 | 6 | 4 | 3 | Bài cuối của bộ |

Từng màn: `crate-levels-logic/K01.png` … `K10.png`. Dữ liệu chính xác (thùng, điểm dừng, chỗ COghe bắt đầu, từng lần kéo đẩy
hay kéo và ô đứng): `crate-levels-logic/summary_logic.json`.

## Đổi so với bộ cũ

- Khó nhất 10 lần kéo, không phải 15. Sàn 6 × 5 ô mà COghe phải đi được: thêm thùng thì COghe bị cắt đường nhiều hơn. Với
  bảy, tám thùng, bộ tìm chỉ ra bài tối đa 7–8 lần kéo; bài sâu nhất (10 lần) có 6 thùng. Muốn khó hơn thì phải tăng sàn,
  ví dụ 7 × 6 ô, và hộp kính to hơn.
- Ở 51–56, gần như toàn bộ thứ tự kéo đến từ thùng chắn nhau. Ở 57–60, một phần đến từ việc mở lối cho COghe.

## Đã dựng trong Unity (06/10/2026)

Code: `Assets/_Game/Editor/COgheSpatialCrateLevels.cs` (dựng cảnh), `COgheSpatialCrateDesigns.cs` và
`Venom/Runtime/ChapterProof/COgheCrateRoutes.cs` (sinh bằng `Tools/crate_puzzles/export_unity.py` từ `summary_logic.json`).

- Ô 14 cm, không phải 12 cm. Lối đi rộng một ô giữa hai thùng cao phải đủ cho COghe (~11 cm) chui qua; 12 cm thì kẹt. Hộp
  kính 84 × 70 cm, vách kính đứng đúng mép lưới (dải sàn hẹp ngoài lưới khiến đường đi chui vào chỗ COghe không lọt). Camera tự
  canh khung theo hộp.
- Thùng cao 6 cm (thùng vuông 7 cm), cách mép ô 1 mm (hai thùng cạnh nhau cách 2 mm, không có đường đi lọt khe), nổi 2 mm trên
  sàn: đáy sát sàn thì vấp vành lỗ thoát khi trượt qua (thùng đỏ màn 59).
- Mặt thùng là mặt trơn (`Slippery`): COghe không bám để trèo, đường đi luôn vòng trên sàn khi có lối. Theo quy ước màu của game,
  mặt trơn có màu tím nhạt; thùng đỏ vẫn đỏ.
- `COgheTapRail.SeatAtStops` (mới, chỉ màn thùng bật): hết lần kéo, thùng được đặt đúng điểm dừng. Lực kéo nhả khi thùng vào
  vùng bắt (6 mm), thùng thường dừng thiếu vài mm và cấn thùng bên cạnh.
- COghe bắt đầu ở ô `spawn`; lời giải mẫu đi tới ô đứng của từng lần kéo, chạm thùng; thùng trượt, COghe đi theo hoặc lùi lại.

Kiểm tra: 10 test giải + 10 test đi lang thang (`SpatialPlusK01…K10Solve/Wander`) qua, hai lượt liên tiếp. Clip cả 10 màn:
`Artifacts/Clips/crates2/crates-51-60.mp4` (tốc độ x2).

Tìm lại: `python3 logic.py <levels> <seed from> <seed to> [append]` (biến `LOGIC_OUT` chọn file kết quả). Vẽ bảng:
`uv run --with pillow python3 levels_logic.py`.
