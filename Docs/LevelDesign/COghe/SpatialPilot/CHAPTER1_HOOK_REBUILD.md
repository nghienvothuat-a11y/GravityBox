# Chương 1 dựng lại theo kế hoạch "hook" (05/10/2026)

Theo phản hồi người mới chơi của Mrk (05/10/2026) và kế hoạch đã chốt `PLANS/COGHE_LEVEL_HOOK_PLAN.md` mục 5.1.
Mrk chốt: làm tầng A (cảm giác điều khiển) và chương 1 trước; màn được thay thì xoá luôn tiến trình test cũ.

Nguyên tắc của chương:
- Mỗi màn thêm một bước, hoặc một kiểu phụ thuộc mới, so với màn trước.
- Mọi khoá đều cho thấy được mở từ đâu: vạch mạch in, chốt nhô lên, nắp kính.
- Chạm vào thứ đang khoá thì hiện dấu ⊘ kèm lý do.

Code dựng màn: `Assets/_Game/Editor/COgheSpatialChapterOneBuilder.cs`. `BuildSpatial` trong
`COgheSpatialCampaignBuilder.cs` gọi `ChapterOne` cho mọi màn trừ 1 và 4. Lời giải mẫu nằm trong
`Assets/_Game/Venom/Runtime/ChapterProof/COgheSpatialScenario.cs`.

| Màn | Tên | Chuỗi bước | Ý mới | Thao tác |
| --- | --- | --- | --- | --- |
| 1 | Chạm để đi | Chạm cửa thoát | Chạm để đi | 0 |
| 2 | Đi vòng mặt tím | Mặt trước của hai bậc là tím trơn → leo bậc 1 từ mặt ngà bên trái → lên bậc 2 → thoát | Mặt tím không leo được | 0 (+ chọn đường) |
| 3 | Nhìn quanh vách | Vách che hẳn vòng thoát → xoay camera mới thấy | Xoay góc nhìn | 0 (+ xoay) |
| 4 | Kéo là mở | Kéo tay xanh (A) → cửa xanh mở | Tay nắm → cửa | 1 |
| 5 | Mở nắp trước | Kéo cần vàng (C) dưới sàn → hộp vàng úp trên tay xanh (A, trên vách) lật lên → leo vách kéo tay xanh → cửa mở | Hộp khoá, thứ tự mở | 2 |
| 6 | Leo lên, mở hộp | Leo bệ, lên bảng ngà trên vách kéo tay vàng (C) → hộp vàng trên tời xanh (A) lật nghiêng → xuống quay tời → cầu nâng → qua | Tay nắm trên cao mở khoá dưới thấp | 2 |
| 7 | Chìa khoá dưới hố | Chốt vàng chặn cầu; cần vàng (D) đứng trên tấm khô dưới hố → xuống hố kéo cần → leo lên kéo tay đỏ (B) → cầu trượt → qua | Chốt có cần riêng, phải xuống hố | 2 (+ xuống hố) |
| 8 | Lên, kéo, xuống | Bấm nút thang đỏ (lún xuống, giữ khi chạy) → lên gờ cao kéo tay xanh (A) → cửa xanh dưới sàn mở → bấm lại nút (đã bật lên) → xuống → thoát | Nút bật lại, thang đi hai chiều | 3 |
| 9 | Cầu chắn giếng thang | Cầu đỗ ngang trên giếng thang nên thang không có điện; kéo cần vàng (C) rút chốt vàng → kéo tay đỏ (B): cầu trượt khỏi giếng và bắc sang tháp thoát → đi thang (nút đỏ) lên → qua cầu → thoát | Một vật làm hai việc | 3 |
| 10 | BOSS · Cỗ máy thân quen | Đi thang (nút đỏ) lên ban công → kéo tay vàng (C) → xoay góc nhìn tìm tay xanh lá (D) sau tấm chắn → kéo (rút kẹp xanh lá trên hộp) → hộp vàng trên tời xanh (A) lật ra sau → bấm nút xuống → quay tời → cửa xanh dưới ban công mở → thoát | Hộp quen thuộc thêm một kẹp, tay mở kẹp bị giấu; phối hợp hộp, kẹp, thang, xoay góc nhìn | 5 (+ tìm) |

Boss khác kế hoạch ban đầu ở một điểm. Ý "thang đi xuống hố" đổi thành "khoá bị giấu sau tấm chắn trên ban công":
bố cục ban công nổi cho đường đi rõ hơn và không cần hố thứ hai. Mẹo vẫn là thứ phải nhìn từ góc khác mới thấy.

## Bản sửa theo góp ý của Mrk (05/10/2026, chiều)

Mrk: nắp kính vẫn khó hiểu; các chi tiết, cột màu đen thiếu tinh tế; bỏ chữ A, B, C, phân biệt kết nối bằng màu.

- **Hộp lật thay nắp kính.** Tay nắm bị khoá nằm dưới một hộp 5 mặt (mặt hở áp vào vách hoặc sàn), cùng màu với
  cần mở nó, có viền đặc và bản lề. Kéo cần thì hộp lật quanh bản lề như nắp ổ cắm: màn 5 lật lên (bản lề cạnh trên),
  màn 6 nghiêng sang trái (cạnh trái, tránh hướng kéo của tời), màn 10 lật ra sau. Hộp chỉ là hình
  (`COgheFlipCover`, đọc vị trí ray khoá); khoá thật vẫn là ray `RequiredRail`/`AlsoRequired` như trước, ray bị ẩn
  và tắt va chạm. Boss: hộp vàng quen thuộc thêm một kẹp xanh lá; nắp chờ kẹp trượt ra rồi mới lật.
- **Màu thay chữ.** Màn 1–10 không còn huy hiệu chữ trên tay nắm, cửa, thang. Mỗi cặp dùng một màu riêng cho tay
  nắm, vạch mạch, điểm tiếp xúc và vật nhận: xanh dương (A), đỏ cam (B), vàng (C, và cần D của màn 7), xanh lá
  (D của boss). Nút thang luôn đỏ cam (màn 9 thang chạy nhờ cầu của tay đỏ). Cáp ròng rọc màn 6 màu xanh của tời.
  Màn 6 và 10 vẽ vạch mạch tới tận hộp/kẹp; màn 7 và 9 chốt chặn tô màu vàng của cần.
- **Bỏ chi tiết tối.** Bỏ chốt khoá kim loại cạnh tay nắm (`TapLock`) và thanh nối, cột dẫn quanh nắp màn 6, cột
  đứng của thang, khung kính, thanh dẫn lơ lửng của thang/cầu/chốt, vạch điện lơ lửng của thang màn 9. Kiểm tra
  độ sáng mọi vật thể hiển thị ở 10 màn (log `CHAPTER 1 AUDIT` khi dựng): không còn vật kim loại hay màu tối; dưới độ
  sáng 0,5 chỉ còn màu mạch (0,44–0,49), chữ số trên biển số màn và bóng đổ.
- Màn 6 xuất phát lệch phải (−.13, −.20) để đường lên bệ không đi xuyên hộp; tời dời ra sau 1,5 cm cho hở hộp.
- Gợi ý: màn 5 có gợi ý thao tác (hộp và màu); lời giải 5, 6, 7, 9 nói theo màu.
- Màn 11–50 chưa đổi (vẫn chữ A/B), chờ Mrk duyệt màn 1–10.

## Kiểm chứng (PlayMode, bản dựng 05/10/2026)

- Test giải 1–10 (`Spatial01Solve`…`Spatial10Solve`): đạt.
- Test kẹt mới: `Spatial02Wander`, `Spatial05Wander`…`Spatial10Wander`. Đi khắp mọi mặt đứng được rồi về chỗ xuất
  phát, sau đó Retry và giải lại.
- `MechanismsStartClearOfFixedScenery`, `NoIdleMechanismOpensAndCatalogIsIsolated`.
- Các test thang: `ElevatorRoundTripCarriesAllTissue`, `BoardingCanBeCancelledByAnotherTap`,
  `LiftButtonPressesHoldsAndPopsUp`, `UnpoweredLiftRefusesWithAReason`.
- `PulleyCanBeReversedAndReset` sửa theo màn 6 mới: C mở nắp trước, sau Retry tời lại bị khoá.
- `COgheRailFeelTests.HandlesPullSmoothly01to10`: mọi tay nắm kéo mượt.

Những điểm đã sửa trong lúc dựng:
- Cửa thoát của màn 8 và 10 nằm sát sàn, vì vách kính trơn thì COghe không leo vào lỗ cao được.
- Nút thang đặt ở góc sau-phải để gờ cao bên cạnh hay rào phía trước không che mất.
- Mô tơ thang có thêm thành phần tích phân tốc độ. Lý do: ước lượng sức nặng của người đứng trên khay thừa thì khay
  kẹt ở điểm dừng trên.

## Chưa làm

- Người mới chơi thử chương 1.
- Đo trên máy Android.
- Hình minh hoạ mới cho dossier từng màn (các hình `Illustrations/0X-0Y.png` là của bản cũ).

## Màn 7 "Chìa khoá dưới hố" (vị trí 8): bịt hố, đáy hố bám được (09/10/2026)

Mrk cho người chơi test (09/10/2026): chạm thẳng vào lỗ thoát ngay từ đầu thì COghe xuống rãnh và kẹt; một lần khác COghe
rơi ra khỏi hộp và màn báo thua.

- **Nguyên nhân:** đáy hố (`Recovery basin`) thấp hơn hai bờ 10 cm, nhưng vách kính chỉ chạy tới mặt bờ (y −.30). Đầu trước,
  đầu sau và phía phải của hố để hở, nên COghe trượt ra ngoài. Đáy hố lại trơn: chỉ tấm đặt cần D đứng được, rơi xuống chỗ
  khác thì không bò được nữa. Lệnh chạm lỗ thoát cố ý đi cả qua mặt trơn, nên COghe tự xuống hố.
- **Sửa** (`COgheSpatialChapterOneBuilder`, n==7):
  - Đáy hố bám được.
  - Vách trơn bịt đầu trước, đầu sau, phía phải của hố, và khoảng trống dưới bờ nhận.
  - Vách bờ nhận chỉ dài bằng bờ đó, nên cả hố là một mặt sàn: rơi xuống đâu, COghe cũng bò về vách leo bên trái được.
  - Lời giải giữ nguyên.
- **Kiểm** (`COgheLevel07ProbeTests`, Explicit, ghi vào `Artifacts/L07`):
  - `Level07ExitTapAtStart`: chạm lỗ thoát ngay từ đầu, rồi đưa COghe về chỗ xuất phát.
  - `Level07RandomTaps`: 12 lượt × 25 lần chạm ngẫu nhiên, có cả chạm lỗ thoát và chạm vào hố.
    - Bản cũ: 6/6 lượt COghe ra ngoài hộp.
    - Bản mới: 0/12, và lượt nào cũng về được chỗ xuất phát.
  - `RandomTapsSweep`: cùng cách chạm ngẫu nhiên cho cả 60 vị trí. Không màn nào để COghe ra ngoài hộp; chỗ kẹt tìm thấy ở
    các màn khác được sửa riêng.
- Dựng lại một màn chương 1: `GenerateSpatialCampaign -coghe-spatial-levels 7`, rồi `COgheProductUIBuilder.Prepare`.
