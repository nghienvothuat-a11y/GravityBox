# COghe 50 — BOSS · Tháp bánh răng

`coghe.spatial.plus.b2` · Theo [LEVEL_TEMPLATE](../../LEVEL_TEMPLATE.md). Nguồn: [quy tắc](../../../../COGHE_LEVEL_DESIGN_RULES.md), [style](../../../../ArtDirection/COghe/STYLE_RULES.md), [đề xuất 50 màn](../PLACEMENT.md), [cơ quan](../MECHANICS.md).

![50 · BOSS · Tháp bánh răng](../Illustrations/50-B2.png)

## 1. Định danh, phạm vi và trạng thái

- ID `coghe.spatial.plus.b2` · vị trí đề xuất **50/50** · chương 5 (Bánh răng) · **Boss** · hồ sơ v0.1, 30/09/2026.
- Trạng thái: **Nháp — thiết kế + greybox minh hoạ**. Chưa có scene trong catalog/build, chưa chơi thử; mọi số liệu là ước lượng.
- Hình minh hoạ: greybox Unity dựng bằng `PlusDesign B2` trong [builder](../../../../../Assets/_Game/Editor/COgheSpatialPlusDesignLevels.cs) với cùng helper, kích thước và art Glass C của Spatial 11–30, chụp bằng `COgheSpatialPlusDesignRender` (góc camera trong game + mặt bằng). Đường màu và số là lời giải dự kiến, không phải hướng dẫn trong game.
- Được yêu cầu (Mrk, 29/09/2026): 18 màn dễ xen giữa 30 màn có sẵn để độ khó mượt hơn, 2 Boss rất khó bằng khối/bánh răng xếp nhiều lớp; có hình mô tả; đề xuất thứ tự tổng 50 màn.

| Mốc | Điều kiện chuyển bước | Trạng thái |
| --- | --- | --- |
| Thiết kế | Mục tiêu, bố cục, lời giải, phục hồi đủ rõ để dựng | Nháp, chờ Mrk duyệt |
| Prototype | Chơi trọn bằng chạm thật; các lỗi dự kiến có đường sửa | Chưa làm |
| Hoàn thiện | Cơ quan dùng chung, Glass C, camera, phản hồi | Chưa làm |
| Nghiệm thu | PlayMode + native, OPPO, người chơi mới | Chưa làm |

## 2. Mục tiêu trải nghiệm và vị trí trong tiến trình

**Mỗi tầng mở đường lên tầng sau; cửa cuối cần cả hai động cơ cùng lúc nên bốn phần phải chia vai đúng.**

- Vai trò: Boss cuối (rất khó). Trước: 49 · Ba lớp răng (mới) · sau: —.
- Vì sao ở đây: Boss cuối của bản 50 màn: bánh răng xếp ba tầng, hai động cơ, thang giữ tay, máy đẩy máy và cửa cần cả hai động cơ.
- Cơ quan: Không (tổ hợp 31, 41, 44, 46, 48, 49 + bốn phần 34/39).
- Gợi ý: không có hướng dẫn lời giải (Boss). Giải đố thuần, không cần nâng chỉ số hay mua đồ.

## 3. Phác thảo, hình học, camera và thao tác

Tháp bánh răng trên một hàng trục: tầng 1 bàn sàn (xe A, động cơ P1), tầng 2 trên sàn giữa 18 cm (xe B, động cơ P2, cần L), tầng 3 trên sàn trên 36 cm (xe C do tầng 2 đẩy vào). Thang giữa chỉ chạy khi P1 có tải; cầu lên sàn trên do tầng 2 nâng; cửa thoát cần P1 và P2 cùng có tải.

- Hộp cố định 0,8 × 0,6 m như Spatial 11–30; kéo đổi góc nhìn, pinch zoom; không nghiêng trọng lực. Camera đầu 3/4 như hình.
- Mặt ngà leo được, mặt tím trơn (bậc trơn ≤3,5 cm vượt được, ≥5 cm chặn); mint chỉ ở lỗ thoát. Mọi đường tắt phải bị chặn bằng hình học công khai (mặt trơn, khe), không bằng số màn.
- Chạm sàn/bệ → đi tới; chạm tay nắm/nút/vòng → tiếp cận và tác động; chạm Q → vào máy; chạm miệng ống → vào ống; chạm phần hoặc ô phần → đổi chọn.

## 4. Lời giải, kết thúc và phục hồi

| Bước | Lệnh / ý định | Trạng thái trước → sau | Tín hiệu thật | Bỏ dở / làm ngược |
| --- | --- | --- | --- | --- |
| 1 | Vào Q hai lượt: bốn phần 25% | Mốc 0 → mốc 1; cơ quan/tiếp xúc thật xác nhận | Vật thật đổi trạng thái (chốt, bậc, cửa, dây) | Đổi đích huỷ tiếp cận; cơ quan chưa chốt thì kéo ngược/làm lại được |
| 2 | Phần 1 kéo A: tầng 1 khớp | Mốc 1 → mốc 2; cơ quan/tiếp xúc thật xác nhận | Vật thật đổi trạng thái (chốt, bậc, cửa, dây) | Đổi đích huỷ tiếp cận; cơ quan chưa chốt thì kéo ngược/làm lại được |
| 3 | Phần 2 đứng P1: thang bánh răng chạy (chỉ khi có tải) | Mốc 2 → mốc 3; cơ quan/tiếp xúc thật xác nhận | Vật thật đổi trạng thái (chốt, bậc, cửa, dây) | Đổi đích huỷ tiếp cận; cơ quan chưa chốt thì kéo ngược/làm lại được |
| 4 | Phần 3 đứng lên thang, lên sàn giữa | Mốc 3 → mốc 4; cơ quan/tiếp xúc thật xác nhận | Vật thật đổi trạng thái (chốt, bậc, cửa, dây) | Đổi đích huỷ tiếp cận; cơ quan chưa chốt thì kéo ngược/làm lại được |
| 5 | Phần 3 kéo B: tầng 2 khớp | Mốc 4 → mốc 5; cơ quan/tiếp xúc thật xác nhận | Vật thật đổi trạng thái (chốt, bậc, cửa, dây) | Đổi đích huỷ tiếp cận; cơ quan chưa chốt thì kéo ngược/làm lại được |
| 6 | Phần 3 kéo L: thang có điện thường trực, phần 1 và 4 đi thang lên | Mốc 5 → mốc 6; cơ quan/tiếp xúc thật xác nhận | Vật thật đổi trạng thái (chốt, bậc, cửa, dây) | Đổi đích huỷ tiếp cận; cơ quan chưa chốt thì kéo ngược/làm lại được |
| 7 | Phần 4 đứng P2: tầng 2 đẩy xe C vào tầng 3 và nâng cầu lên sàn trên | Mốc 6 → mốc 7; cơ quan/tiếp xúc thật xác nhận | Vật thật đổi trạng thái (chốt, bậc, cửa, dây) | Đổi đích huỷ tiếp cận; cơ quan chưa chốt thì kéo ngược/làm lại được |
| 8 | Phần 3 qua cầu; khi P1 và P2 cùng có tải, cửa thoát mở và chốt | Mốc 7 → mốc 8; cơ quan/tiếp xúc thật xác nhận | Vật thật đổi trạng thái (chốt, bậc, cửa, dây) | Đổi đích huỷ tiếp cận; cơ quan chưa chốt thì kéo ngược/làm lại được |
| 9 | Hai phần đứng máy rời nút, đi lên; bốn phần nhập trên sàn trên, chui ra | Mốc 8 → mốc 9; cơ quan/tiếp xúc thật xác nhận | Vật thật đổi trạng thái (chốt, bậc, cửa, dây) | Đổi đích huỷ tiếp cận; cơ quan chưa chốt thì kéo ngược/làm lại được |

- **Phục hồi riêng:** Mỗi đầu ra đã chốt giữ nguyên khi rời nút; thang có đường xuống; khớp sai tầng chỉ làm bánh quay suông. Tụ sớm: vào lại Q.
- Dòng khối lượng: `100% → 50% + 50% → 25% + 25% + 25% + 25% → 100%`. Tách chỉ qua Q; đủ gần và không vật cản thì tự nhập, không cooldown.
- Thắng: toàn bộ mô nhập thành một cơ thể **trong hộp** rồi qua lỗ cuối; ra trước khi nhập hết thì thua đúng câu hiện hành. Retry trả mọi vật về đầu.

## 5. Cơ quan, trạng thái và kiến trúc dùng lại

| MechanismId | Loại | Hợp đồng | Reset |
| --- | --- | --- | --- |
| b2.1 | COgheGearTrain nhiều chuỗi (có) | Xem [MECHANICS](../MECHANICS.md) | Reset pose, lực, chốt, tác vụ và người giữ |
| b2.2 | Đầu ra cần hai động cơ (mới: hai InputClutch cho một chuỗi) | Xem [MECHANICS](../MECHANICS.md) | Reset pose, lực, chốt, tác vụ và người giữ |
| b2.3 | Thang bánh răng giữ tay (như 46) | Xem [MECHANICS](../MECHANICS.md) | Reset pose, lực, chốt, tác vụ và người giữ |
| b2.4 | Q (có) | Xem [MECHANICS](../MECHANICS.md) | Reset pose, lực, chốt, tác vụ và người giữ |

Rủi ro cần prototype: Đầu ra hai động cơ là mở rộng nhỏ của GearTrain; bốn phần + nhiều bánh quay: đo hiệu năng OPPO sớm.

## 6. Hình ảnh, animation và phản hồi

Glass C / mạch in / ray satin theo STYLE_RULES như Spatial 11–30: A xanh tròn, B san hô; tím = trơn; mint = lỗ cuối; bánh răng cố định màu nhựa hổ phách của bộ bánh răng cũ, bánh trên xe trượt màu ngà (cần art pass riêng cho bánh răng Glass C). Nhận ý định → tiếp cận → tác động → kết quả theo trạng thái thật.

## 7. Độ khó và ngân sách runtime

| Trục | Dự kiến | Thực đo |
| --- | --- | --- |
| Phần cơ thể tối đa | 4 | Chưa chơi thử |
| Số chạm lời giải mẫu | khoảng 30 | Chưa đo |
| Thời gian lần đầu | 6–10 phút | Chưa đo |
| Độ chính xác | Thấp: đích rộng, không canh thời điểm | Chưa đo |

Không gộp thành điểm khó tổng. So sánh vị trí dùng số chạm đo được của các màn cũ (xem [PLACEMENT](../PLACEMENT.md)).

## 8. Chơi thử và hồi quy

Chưa dựng. Khi dựng: đường giải chính bằng chạm thật (PlayMode + native Mac), các ca ở mục 4, Retry/pause, phần nhỏ/lớn, đường tắt qua kính/mặt trơn; người chơi mới chưa biết lời giải.

## 9. Bằng chứng hiệu năng trên thiết bị

Chưa có. Đo trên OPPO như PLAN của Spatial 11–30 khi có build.

## 10. Tích hợp, tương thích và nghiệm thu

- ID mới, không đụng save của 30 màn đã có; thứ tự hiển thị đổi theo [PLACEMENT](../PLACEMENT.md), ID màn cũ giữ nguyên.
- Kết luận: **đề xuất thiết kế**, chờ Mrk duyệt trước khi dựng.
