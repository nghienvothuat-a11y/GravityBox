# 60 màn: xen 10 màn thùng vào 5 chương (06/10/2026)

Mrk (06/10/2026, 12:32): "Sửa tiếp Chương 4 và 5 sau đó thêm các level của 51-60 xen kẽ trong các chương. Xếp levels
theo độ khó tăng dần". Làm sau khi chương 5 dựng lại xong.

## Cách xếp

- 5 chương × 12 màn. Mỗi chương nhận 2 màn thùng; K01–K10 vẫn theo thứ tự dễ → khó.
- Trong chương giữ đường răng cưa của kế hoạch (`PLANS/COGHE_LEVEL_HOOK_PLAN.md` mục 5.6): chỉ trũng ở màn dạy cơ quan
  mới và màn nghỉ, tăng dần vào Boss. Boss vẫn đứng cuối chương.
- Màn thùng đặt vào chỗ số quyết định của nó vừa với hai màn bên cạnh. K01 đứng sau màn 4–5 (đã học kéo tay nắm).

## Thứ tự (key · số quyết định)

| Chương | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9 | 10 | 11 | 12 (Boss) |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 1 (1–12) | 01 · 0 | 02 · 1 | 03 · 1 | 04 · 1 | 05 · 2 | **K01 · 2** | 06 · 2 | 07 · 2 | 08 · 3 | **K02 · 3** | 09 · 3 | 10 · 6 |
| 2 (13–24) | 16 · 4 | 12 · 4 | N13 · 5 | 14 · 3 | N15 · 4 | **K03 · 4** | 15 · 5 | **K04 · 5** | 17 · 6 | N18 · 7 | N19 · 9 | 20 · 12 |
| 3 (25–36) | 18 · 2 | N22 · 3 | N23 · 3 | 21 · 2 | N25 · 4 | N26 · 5 | **K05 · 6** | 22 · 4 | 13 · 3 | **K06 · 6** | N29 · 4 | B1 · 7 |
| 4 (37–48) | N31 · 5 | N32 · 7 | N33 · 8 | N34 · 8 | N35 · 7 | **K07 · 7** | E10 · 3 | E11 · 6 | 26 · 6 | **K08 · 8** | 29 · 8 | N40 · 9 |
| 5 (49–60) | N41 · 3 | N42 · 5 | N43 · 7 | N44 · 4 | N45 · 8 | **K09 · 9** | N46 · 4 | N47 · 5 | N48 · 4 | N49 · 7 | **K10 · 10** | N50 · 12 |

Chỗ trũng và lý do:
- Chương 2: 14 (3) dạy ống.
- Chương 3: 21 (2) dạy đối trọng. 22 (4) và 13 (3) là màn 27, 28 chưa làm lại (kế hoạch 6 và 6); K05 và K06 tạm giữ
  đỉnh 6 ở hai chỗ đó. N29 (4) chuẩn bị Boss.
- Chương 4: E10 (3) là màn nghỉ sau đỉnh N35 + K07.
- Chương 5: N44 (4) dạy bánh đệm; N46 (4) là màn nghỉ sau đỉnh N45 + K09; N48 (4) theo kế hoạch 4–5.

Biểu đồ: `Tools/level_metrics/order60.py` (dữ liệu `Artifacts/Metrics/levels.jsonl`), ảnh `Tools/level_metrics/out/order60.png`.

## Thay đổi

- Chỉ đổi `SpatialOrder` trong `COgheSpatialCampaignBuilder.cs`. ID, scene và thư mục art đi theo nội dung, nên save cũ
  không bị ảnh hưởng.
- Áp dụng: `GenerateChapterTwoLevels -coghe-plus-levels NONE` (không dựng màn nào; chạy `ApplySpatialOrder` và
  `RenumberSpatialPlaques`, 55 biển số được đánh lại), rồi `MoveChapterThreePlaques` và `COgheProductUIBuilder.Prepare`.

## Những thứ gắn với vị trí

- Boss: cờ `Boss` nằm trong definition của nội dung (10, 20, B1, N40, N50), không theo vị trí. Danh sách màn trong game
  và phần giới thiệu Boss đọc cờ này.
- Nhà mở khi thắng Boss đầu tiên (`VenomCampaignSave`), nay là màn 12.
- Đồ trong Nhà, mũ, vật bay quanh và màu mực mở theo số màn. Các mốc đặt cho 50 màn (Bóng 10 … Cúp 50), nên Bóng mở
  trước Nhà và Cúp không đi cùng Boss cuối. **Đã đổi** (Mrk 06/10/2026, 17:34 UTC: "nhân mọi mốc với 1,2"): 40 mốc nhân
  1,2 rồi làm tròn (Bóng 12 … Cúp 60). Đường giá chuyển từ 10–50 sang 12–60, nên giá của cả 40 món giữ nguyên. Hình nặn
  của COghe (`ShapeUnlocks`, mở ngầm ở màn 2–28) không phải đồ mở khoá nên giữ nguyên. Danh sách mốc mới nằm trong
  `Docs/Customize/COghe/README.md` và `Docs/Personality/COghe/README.md`.
- Hộp "Home khoá" trước ghi cứng "Complete level 10"; nay lấy vị trí Boss đầu từ danh mục (hiện là 12).
- Hai bảng chọn màn kiểu cũ dùng OnGUI (`VenomCampaign` dòng 744, `COgheDayLabPresentation` dòng 139) ghi "B" cho mỗi màn
  thứ 10. Chúng dùng chung cho các campaign cũ 10 màn một chương, nên chưa sửa; bảng chọn màn của giao diện sản phẩm
  đọc cờ `Boss`.

## Kiểm tra

- Sau khi áp thứ tự: K01 ở vị trí 6, K02 ở 10, K05 ở 31, K10 ở 59; 10 ở 12, B1 ở 36, N50 ở 60 (đọc trong definition).
  Biển số của 55 màn đổi vị trí đã được đánh lại.
- Bộ PlayMode đầy đủ (06/10/2026, 16:47–17:06 UTC, `Artifacts/Tests/order60b.xml`): 793 đạt, 0 trượt, 16 bỏ qua. Gồm test
  giải và test đi lang thang của cả 10 màn thùng và các màn Plus, Product UI và Spatial recovery.
- Lần chạy đầu bị Unity sập trong Burst lúc nạp lại script (không liên quan code), nên định nghĩa chưa nhận thứ tự mới và
  bước Prepare báo lỗi. Đã chạy lại từ đầu.
