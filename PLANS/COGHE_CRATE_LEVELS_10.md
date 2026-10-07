# Mười màn kéo hộp (thiết kế, 06/10/2026)

> Đã thay (06/10/2026): Mrk thấy thứ tự kéo vô lý (luật chỗ đứng ngầm). Bộ dựng trong game là
> [COGHE_CRATE_LEVELS_LOGIC.md](COGHE_CRATE_LEVELS_LOGIC.md). Phần dưới giữ lại làm lịch sử.

Mrk (06/10/2026): "mày tạo ra 10 màn kéo hộp như này có độ khó tăng dần" và "gửi tao thiết kế trước khi xây dựng màn chơi
trong unity". Đây là bản thiết kế để duyệt; chưa dựng trong Unity. Bộ này thay bộ 5 màn ở `COGHE_CRATE_PUZZLES.md`: thêm
luật chỗ đứng cho COghe để dựng được trong game.

![Mười màn](crate-levels-10/overview10.png)

## Luật chơi

- Lưới 6 × 5 ô (ô 12 cm) trong hộp 80 × 60 cm. Lỗ thoát trên sàn (vòng xanh bạc hà) bị **thùng đỏ** che.
- Mỗi thùng trượt theo một hướng trên ray riêng, giữa hai điểm dừng. Một lần chạm = trượt sang điểm dừng kia; chạm lần nữa
  thì về chỗ cũ. Thùng chỉ trượt khi cả đoạn đường trống.
- **Chỗ đứng của COghe:** muốn dời một thùng, phải có một ô trống ngay sau thùng (COghe đẩy), hoặc ngay sau chỗ thùng sẽ
  dừng (COghe kéo và lùi dần). Thùng sát kính hoặc bị kẹp giữa các thùng khác có thể không dời được theo hướng đó.
- Hình dạng: thùng dài 2 ô, dài 3 ô (từ màn 3), thùng vuông lớn 2 × 2 (từ màn 5). Thùng ngà; chỉ thùng đỏ khác màu.
- Không thế bày nào bị kẹt: mọi màn đều kiểm tra rằng từ bất kỳ thế bày nào người chơi tạo ra vẫn còn đường thắng, nên
  không cần nút Retry.

Kiểm tra bằng chương trình giải (BFS, có luật chỗ đứng): số lần kéo ít nhất, số thế bày người chơi có thể tạo ra, số thế
bày chết (bằng 0 ở cả 10 màn). Công cụ: `Tools/crate_puzzles` (`search10.py`, `levels10.py`).

| Màn | Kéo ít nhất | Thùng | Kéo lại thùng đã kéo | Thế bày | Ý mới |
| --- | --- | --- | --- | --- | --- |
| 1 · Thùng chặn đường | 2 | 3 | 0 | 8 | Thùng đỏ bị một thùng chặn; một thùng khác không liên quan. |
| 2 · Hai lớp chặn | 3 | 4 | 0 | 16 | Thùng chặn thùng đỏ lại bị một thùng khác chặn. |
| 3 · Thùng dài | 4 | 4 | 0 | 16 | Thùng dài 3 ô: cần khoảng trống dài hơn mới trượt được. |
| 4 · Gỡ từ ngoài vào | 5 | 5 | 0 | 32 | Chuỗi bốn thùng chặn nhau: gỡ thùng ngoài cùng trước. |
| 5 · Thùng vuông | 6 | 5 | 1 | 24 | Thùng vuông lớn; lần đầu phải dời một thùng rồi trả về. |
| 6 · Đi rồi trả lại | 7 | 6 | 1 | 40 | Sáu thùng; một thùng dời tạm để thùng khác đi qua. |
| 7 · Mượn chỗ | 8 | 6 | 2 | 36 | Hai thùng phải dời tạm rồi trả lại chỗ cũ. |
| 8 · Ngõ hẹp | 10 | 7 | 3 | 64 | Bảy thùng, ba lần trả thùng về. |
| 9 · Kho chật | 12 | 7 | 5 | 30 | Thùng vuông chắn giữa; năm lần trả thùng về. |
| 10 · Mê cung thùng | 15 | 8 | 7 | 32 | Tám thùng; thùng vuông phải dời ba lần. Bài cuối của bộ. |

## Đề xuất chèn

Hai màn mỗi chương, độ khó vẫn tăng trong từng chương (tổng 60 màn): 1–2 ở chương 1, 3–4 ở chương 2, 5–6 ở chương 3,
7–8 ở chương 4, 9–10 ở chương 5. Hoặc gom 10 màn thành một chương riêng. Mrk chọn rồi mới dựng.

## Từng màn

### Màn 1 · Thùng chặn đường (2 lần kéo)
![Màn 1](crate-levels-10/K01.png)

### Màn 2 · Hai lớp chặn (3 lần kéo)
![Màn 2](crate-levels-10/K02.png)

### Màn 3 · Thùng dài (4 lần kéo)
![Màn 3](crate-levels-10/K03.png)

### Màn 4 · Gỡ từ ngoài vào (5 lần kéo)
![Màn 4](crate-levels-10/K04.png)

### Màn 5 · Thùng vuông (6 lần kéo)
![Màn 5](crate-levels-10/K05.png)

### Màn 6 · Đi rồi trả lại (7 lần kéo)
![Màn 6](crate-levels-10/K06.png)

### Màn 7 · Mượn chỗ (8 lần kéo)
![Màn 7](crate-levels-10/K07.png)

### Màn 8 · Ngõ hẹp (10 lần kéo)
![Màn 8](crate-levels-10/K08.png)

### Màn 9 · Kho chật (12 lần kéo)
![Màn 9](crate-levels-10/K09.png)

### Màn 10 · Mê cung thùng (15 lần kéo)
![Màn 10](crate-levels-10/K10.png)

## Khi dựng trong Unity (sau khi Mrk duyệt)

- Mỗi thùng: rail hai điểm dừng (`NextCrate`), thùng vuông là khối 24 × 24 cm cao 6 cm, các thùng khác cao 4,5 cm, mặt ngà
  leo được. Vị trí, hướng, điểm dừng lấy đúng từ `crate-levels-10/summary10.json`. Lỗ thoát: cửa sập sàn như màn 7.
- Tay nắm ở cả hai đầu thùng theo hướng trượt: COghe chọn đẩy hay kéo tuỳ chỗ đứng còn trống.
- Lời giải mẫu theo đúng thứ tự trong bảng; test giải và test đi lang thang cho từng màn.

## Đã dựng trong Unity — màn 51–60 (06/10/2026, đang làm dở)

Mrk (06/10/2026): "triển khai làm 10 màn mới - từ 51 đến 60 theo thiết kế này. Tao sẽ sắp xếp level xen kẽ sau".

- `SpatialOrder` thêm `K01`…`K10` sau Boss 50 (`B2`) → màn 51–60. Cảnh `COgheSpatialPlusK01…K10.unity`.
- Bố cục và lời giải mẫu sinh từ dữ liệu thiết kế bằng `Tools/crate_puzzles/export_unity.py` →
  `Assets/_Game/Editor/COgheSpatialCrateDesigns.cs` (bố cục) và `Assets/_Game/Venom/Runtime/ChapterProof/COgheCrateRoutes.cs`
  (thứ tự kéo + ô COghe đứng). Dựng cảnh: `Assets/_Game/Editor/COgheSpatialCrateLevels.cs`.
- Mỗi thùng là một rail hai điểm dừng (`COgheTapRail.Stops`), chạm vào mặt thùng thì thùng trượt sang điểm dừng kia; tay nắm ở
  cả hai đầu, COghe đẩy (đứng sau) hoặc kéo (đứng trước) tuỳ chỗ trống. Thùng cao 3 cm (vuông 3,5 cm) thay vì 4,5/6 cm như dự
  kiến: với ô 12 cm, COghe phải trèo qua thùng (xem dưới); mặt trên và hông thùng dùng vật liệu bám như sàn.
- Sửa chung trong `COgheTapRail.TryTouch`: hộp chạm quanh tay nắm phụ (`AlternateHandle`) chỉ dùng khi `PickHandleOnly`; trước đó
  hộp này lấn ra sàn sau thùng và "nuốt" lệnh đi tới ô đó. Chỉ màn View 07 có tay nắm phụ khác, và nó dùng `PickHandleOnly`.

### Kiểm tra (bộ PlayMode đầy đủ, 06/10/2026: 739/765 qua, 15 bỏ qua như trước, 11 trượt = 3 test đếm số màn (đã sửa, chạy lại riêng: qua) + 8 test màn thùng)

| Màn | Test giải | Test lang thang | Ghi chú |
|-----|-----------|-----------------|---------|
| 51 Thùng chặn đường | qua | qua | |
| 52 Hai lớp chặn | qua | qua | |
| 53 Thùng dài | qua | qua | |
| 54 Gỡ từ ngoài vào | trượt | qua | chập chờn: thùng đỏ bị đẩy về tường trước, COghe đứng mép lỗ — lúc rơi xuống lỗ (thắng), lúc đứng lại và thùng kẹt |
| 55 Thùng vuông | trượt | trượt | COghe kẹt khi trèo qua thùng đỏ (kéo 1) |
| 56 Đi rồi trả lại | trượt | trượt | kẹt khi trèo qua thùng (kéo 6) |
| 57 Mượn chỗ | qua | qua | |
| 58 Ngõ hẹp | qua | qua | |
| 59 Kho chật | trượt | trượt | kẹt khi trèo qua thùng (kéo 4) |
| 60 Mê cung thùng | qua | trượt | chập chờn: có lần thùng vuông dừng thiếu > 6 mm trước điểm dừng, lần kéo không kết thúc |

Danh mục sản phẩm (`Resources/COgheUI/Catalog.asset`) dựng lại: 60 màn; ba test đếm số màn sửa 50 → 60.

Clip các màn qua: `Artifacts/Clips/crates/crates-51-58.mp4` (51, 52, 53, 57, 58).

### Vấn đề mở: COghe trèo qua thùng

Kiểm theo lưới (BFS trên ô trống, `Tools/crate_puzzles`): chỉ màn 51 và 53 giải được mà COghe luôn đi trên sàn; tám màn còn lại
có bước mà ô cần đứng bị thùng vây kín, COghe phải trèo qua thùng. Trèo qua thùng 3 cm lúc được lúc không ("Bò / leo" không tới
nơi). Đã thử: thùng 2 cm, thùng 6 cm, chạm từng đoạn đường sàn — không cải thiện rõ. Hai hướng:

1. Sửa cho COghe trèo qua bậc thấp ổn định (điều hướng/leo bậc) — giữ nguyên mười thiết kế đã duyệt.
2. Tìm lại thiết kế với luật "COghe luôn có đường sàn tới chỗ đứng" — đổi bố cục, cần Mrk duyệt lại.
