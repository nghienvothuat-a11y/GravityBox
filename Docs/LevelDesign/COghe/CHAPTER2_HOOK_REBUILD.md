# Chương 2 dựng lại theo kế hoạch "hook" (05/10/2026), phần 1

Theo `PLANS/COGHE_LEVEL_HOOK_PLAN.md` mục 5.2 và lời Mrk (05/10/2026): "bắt đầu chương 2 xong đăng clip như chương 1".
Màn mới theo quy tắc đã chốt ở chương 1: không chữ, mỗi cặp một màu, không chi tiết tối.

Phần 1 (lượt này): thứ tự mới, ba màn mới 13, 15, 18. Phần 2 (lượt sau): màn 19 mới và Boss 20 làm lại.

## Thứ tự chương 2

| Vị trí | Nội dung (key) | Tên | Ghi chú |
| --- | --- | --- | --- |
| 11 | `16` | Một thành hai | Dạy máy Q, đưa từ 17 lên ngay sau Boss đầu |
| 12 | `12` | Khối lớn đi trước | Giữ |
| 13 | `N13` | Khối chặn lò xo | **Mới** |
| 14 | `14` | Luồn một vòng | Giữ (dạy ống) |
| 15 | `N15` | Chuẩn bị trước khi đi | **Mới** |
| 16 | `15` | Gặp nhau ở ngã ba | Giữ |
| 17 | `E03` | Giữ cửa cho bạn | Tạm, dời từ 18: bước đệm ngay trước 18 mới |
| 18 | `N18` | Nửa thân không đủ sức | **Mới** (ý 9 + 10 của Mrk) |
| 19 | `17` | Bạn giữ, mình luồn | Tạm, chờ màn 19 mới |
| 20 | `20` | Hai nửa một máy (Boss) | Chờ làm lại |

Bỏ khỏi game: `11` (Kê một bậc), `E01` (Thang chở hàng), `E02` (Hai ống, một đích). File cảnh và test của chúng còn
giữ tới khi chương 2 xong, vì vài test chung (thang máy, âm thanh) còn dùng cảnh `E01`.

Tiến trình đã lưu của các bản test bị xoá một lần (`VenomCampaignSave.CurrentVersion` = 4). Biển số trên kính của các
màn đổi chỗ được đánh số lại (`RenumberSpatialPlaques`).

## Màn mới

Code: `Assets/_Game/Editor/COgheSpatialPlusLevels.cs` (`PlusN13`, `PlusN15`, `PlusN18`), dựng bằng
`VenomCampaignBuilder.GenerateChapterTwoLevels` với `-coghe-plus-levels N13,N15,N18` (chỉ dựng các màn được nêu, không
động tới cảnh khác). Lời giải mẫu: `COgheSpatialPlusScenario` (`N13`, `N15`, `N18`).

| Màn | Chuỗi bước | Ý mới |
| --- | --- | --- |
| 13 · Khối chặn lò xo | Vào Q tách đôi → nửa 1 kéo khối đỏ (thấp, có lò xo) về bãi và giữ → nửa 2 đẩy khối xanh (cao) sang sát đảo → nửa 1 buông, khối đỏ bật về nằm cạnh khối xanh → nhập → leo đỏ, xanh, đảo → thoát | Khối có lò xo: phải có người giữ; buông ra thành bậc thấp |
| 15 · Chuẩn bị trước khi đi | Kéo tay xanh (chỉ có ở phòng này) → bậc 3 cm trượt qua khe 3,4 cm dưới vách sang sát kệ phòng bên → chui ống qua vách → leo bậc → kệ → thoát. Chui ống trước thì phòng bên không có tay nắm, quay lại bằng chính ống đó | Làm việc ở phòng này trước khi rời đi |
| 18 · Nửa thân không đủ sức | Tách đôi → nửa 1 đứng nút xanh, cửa nâng → nửa 2 sang phòng bên, đẩy thử khối vàng "100%": gồng rồi buông ("chưa đủ sức") → kéo tay đỏ chốt cửa → nửa 1 đi qua → nhập → cả thân đẩy khối vàng sát kệ 9 cm → leo → thoát | Khối 100% (cản 0,45 N; cả thân đẩy tối đa 0,67 N, nửa thân 0,34 N) |

Gợi ý: màn 13 và 18 có gợi ý thao tác (lò xo; khối 100%), cả ba có lời giải theo màu trong nút Gợi ý.

## Kiểm chứng

- `SpatialPlusN13Solve`, `SpatialPlusN15Solve`, `SpatialPlusN18Solve` và ba test kẹt `…Wander`: đạt.
- Lời giải màn 18 kiểm tra nửa thân bỏ cuộc vì quá nặng (`GaveUp`) và khối không nhúc nhích trước khi cả thân đẩy.
- Sửa trong lúc dựng: máy Q sát kính phải không tách được (dời 4 cm); tay đỏ màn 18 sát vách không tới được (dời ra
  giữa phòng); khe dưới vách màn 15 vừa khít làm kẹt bậc (nới 1 cm mỗi bên); miệng ống màn 15 quay về phía bậc để
  thân ra khỏi ống không vướng góc kệ.
