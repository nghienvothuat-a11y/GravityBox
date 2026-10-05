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
| 4 | Kéo là mở | Kéo A → cửa A mở | Tay nắm → cửa | 1 |
| 5 | Mở nắp trước | Kéo cần C dưới sàn → nắp kính trên tay A (trên vách) nâng lên → leo vách kéo A → cửa mở | Khoá có nắp, thứ tự mở | 2 |
| 6 | Leo lên, mở hộp | Leo bệ, lên bảng ngà trên vách kéo C → nắp kính trên tời A nâng lên → xuống quay tời A → cầu nâng → qua | Tay nắm trên cao mở khoá dưới thấp | 2 |
| 7 | Chìa khoá dưới hố | Chốt chặn cầu B; cần D đứng trên tấm khô dưới hố → xuống hố kéo D → leo lên kéo B → cầu trượt → qua | Chốt có cần riêng, phải xuống hố | 2 (+ xuống hố) |
| 8 | Lên, kéo, xuống | Bấm nút thang (lún xuống, giữ khi chạy) → lên gờ cao kéo A → cửa thoát dưới sàn mở → bấm lại nút (đã bật lên) → xuống → thoát | Nút bật lại, thang đi hai chiều | 3 |
| 9 | Cầu chắn giếng thang | Cầu đỗ ngang trên giếng thang nên thang không có điện; kéo C rút chốt → kéo B: cầu trượt khỏi giếng và bắc sang tháp thoát → đi thang lên → qua cầu → thoát | Một vật làm hai việc | 3 |
| 10 | BOSS · Cỗ máy thân quen | Đi thang lên ban công → kéo C (nắp kính trên tời A nâng) → xoay góc nhìn tìm D sau tấm chắn → kéo D (rút chốt kẹt) → bấm nút xuống → quay tời A → cửa thoát dưới ban công mở → thoát | Hai khoá trên một tời, một khoá bị giấu; phối hợp nắp, chốt, thang, xoay góc nhìn | 5 (+ tìm) |

Boss khác kế hoạch ban đầu ở một điểm. Ý "thang đi xuống hố" đổi thành "khoá bị giấu sau tấm chắn trên ban công":
bố cục ban công nổi cho đường đi rõ hơn và không cần hố thứ hai. Mẹo vẫn là thứ phải nhìn từ góc khác mới thấy.

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
