# COghe 30 — BOSS · Tháp khối

`coghe.spatial.plus.b1` · Theo [LEVEL_TEMPLATE](../../LEVEL_TEMPLATE.md). Nguồn: [quy tắc](../../../../COGHE_LEVEL_DESIGN_RULES.md), [style](../../../../ArtDirection/COghe/STYLE_RULES.md), [đề xuất 50 màn](../PLACEMENT.md), [cơ quan](../MECHANICS.md).

![30 · BOSS · Tháp khối](../Illustrations/30-B1.png)

## 1. Định danh, phạm vi và trạng thái

- ID `coghe.spatial.plus.b1` · vị trí đề xuất **30/50** · chương 3 (Dây, đối trọng và khối chồng) · **Boss** · hồ sơ v0.1, 30/09/2026.
- Trạng thái: **Đã dựng — chơi được** (30/09/2026). Scene `COgheSpatialPlusB1.unity`, vị trí 30/50 trong catalog Spatial (save theo ID). Mục 4 là lời giải như đã dựng; thay đổi so với thiết kế ở cuối mục 1.
- Hình: chụp từ scene thật (dựng bằng `B1` trong [COgheSpatialPlusLevels.cs](../../../../../Assets/_Game/Editor/COgheSpatialPlusLevels.cs)) bằng `COgheSpatialPlusDesignRender` (góc camera trong game + mặt bằng). Đường màu và số là lời giải mẫu, không phải hướng dẫn trong game.
- Được yêu cầu (Mrk, 29/09/2026): 18 màn dễ xen giữa 30 màn có sẵn để độ khó mượt hơn, 2 Boss rất khó bằng khối/bánh răng xếp nhiều lớp; có hình mô tả; đề xuất thứ tự tổng 50 màn.

| Mốc | Điều kiện chuyển bước | Trạng thái |
| --- | --- | --- |
| Thiết kế | Mục tiêu, bố cục, lời giải, phục hồi đủ rõ để dựng | Mrk duyệt 30/09/2026 |
| Prototype | Chơi trọn bằng chạm thật; các lỗi dự kiến có đường sửa | Xong: lời giải chạm thật + test đi lang thang tìm chỗ kẹt |
| Hoàn thiện | Cơ quan dùng chung, Glass C, camera, phản hồi | Glass C/mạch in như 11–30; bánh răng dùng art bánh răng hiện có |
| Nghiệm thu | PlayMode + native, OPPO, người chơi mới | PlayMode + native Mac xong; OPPO và người chơi mới: chưa |

### Đã dựng: thay đổi so với thiết kế

- C2 rộng 22 cm, trượt 8 cm tới đầu tháp của C1; tay nắm ở mép trên bên trái, đẩy từ lùi 12 cm trên kệ: bản cũ người đẩy bị C2 kéo ra khỏi kệ và mất điểm bám.
- C3 nằm nửa phải C2, chừa dải trống bên trái để đứng đẩy C3.
- C2, C3 nổi 2 mm trên khối mang (như E07).
- Mái chìa nâng 6 mm: C2 lọt dưới, C3 vẫn bị chặn nếu chưa đẩy ra sau.

## 2. Mục tiêu trải nghiệm và vị trí trong tiến trình

**Tháp khối vừa là dụng cụ vừa là đích: dùng nó để lên, rồi sắp lại nó để thoát.**

- Vai trò: Boss chương 3 (rất khó). Trước: 29 · Chồng hai tầng (mới) · sau: 31 · Bánh răng đầu tiên (mới).
- Vì sao ở đây: Boss mới thay chỗ Boss 30 cũ (nay ở 40). Khối xếp chồng ba tầng; tháp vừa là bậc để lên cần gạt vừa là cầu thang cuối.
- Cơ quan: Không (Boss chỉ phối hợp): ray trên khối (21, 29), khối nặng cần cả thân (35), giữ + chốt (18), Q.
- Gợi ý: không có hướng dẫn lời giải (Boss). Giải đố thuần, không cần nâng chỉ số hay mua đồ.

## 3. Phác thảo, hình học, camera và thao tác

Tháp thoát 27 cm bên phải có mái chìa ở dải 18–27 cm phía trước. Xe C1 (9 cm, nặng: chỉ cả thân kéo nổi) chở C2 (9 cm, rộng 22 cm, trượt 8 cm tới đầu tháp của C1) chở C3 (9 cm, nằm nửa phải C2, trượt trước → sau). Lúc đầu C2 sát kệ cần gạt 18 cm bên trái (mặt kệ phía C1 ngà). Then C1 chỉ rút khi nút A có tải VÀ cần B đã kéo. Tay nắm C2 ở mép trên bên trái, đẩy từ kệ; khối trên nổi 2 mm trên khối mang.

- Hộp cố định 0,8 × 0,6 m như Spatial 11–30; kéo đổi góc nhìn, pinch zoom; không nghiêng trọng lực. Camera đầu 3/4 như hình.
- Mặt ngà leo được, mặt tím trơn (bậc trơn ≤3,5 cm vượt được, ≥5 cm chặn); mint chỉ ở lỗ thoát. Mọi đường tắt phải bị chặn bằng hình học công khai (mặt trơn, khe), không bằng số màn.
- Chạm sàn/bệ → đi tới; chạm tay nắm/nút/vòng → tiếp cận và tác động; chạm Q → vào máy; chạm miệng ống → vào ống; chạm phần hoặc ô phần → đổi chọn.

## 4. Lời giải, kết thúc và phục hồi

| Bước | Lệnh / ý định | Trạng thái trước → sau | Tín hiệu thật | Bỏ dở / làm ngược |
| --- | --- | --- | --- | --- |
| 1 | Vào Q: 50% + 50% | Mốc 0 → mốc 1; cơ quan/tiếp xúc thật xác nhận | Vật thật đổi trạng thái (chốt, bậc, cửa, dây) | Đổi đích huỷ tiếp cận; cơ quan chưa chốt thì kéo ngược/làm lại được |
| 2 | Nửa 1 đứng nút A (bên phải) | Mốc 1 → mốc 2; cơ quan/tiếp xúc thật xác nhận | Vật thật đổi trạng thái (chốt, bậc, cửa, dây) | Đổi đích huỷ tiếp cận; cơ quan chưa chốt thì kéo ngược/làm lại được |
| 3 | Nửa 2 leo đầu hở của C1 → C2 → kệ, kéo B: then C1 bắt chốt | Mốc 2 → mốc 3; cơ quan/tiếp xúc thật xác nhận | Vật thật đổi trạng thái (chốt, bậc, cửa, dây) | Đổi đích huỷ tiếp cận; cơ quan chưa chốt thì kéo ngược/làm lại được |
| 4 | Nửa 2 đứng trên kệ, đẩy C2 (tay nắm mép trên) sang đầu tháp của C1 | Mốc 3 → mốc 4; cơ quan/tiếp xúc thật xác nhận | Vật thật đổi trạng thái (chốt, bậc, cửa, dây) | Đổi đích huỷ tiếp cận; cơ quan chưa chốt thì kéo ngược/làm lại được |
| 5 | Nửa 2 xuống dải trống bên trái C2, đẩy C3 ra sau (C3 chỉ mở khi C2 đã sang) | Mốc 4 → mốc 5; cơ quan/tiếp xúc thật xác nhận | Vật thật đổi trạng thái (chốt, bậc, cửa, dây) | Đổi đích huỷ tiếp cận; cơ quan chưa chốt thì kéo ngược/làm lại được |
| 6 | Hai nửa xuống sàn trước, tự nhập 100% | Mốc 5 → mốc 6; cơ quan/tiếp xúc thật xác nhận | Vật thật đổi trạng thái (chốt, bậc, cửa, dây) | Đổi đích huỷ tiếp cận; cơ quan chưa chốt thì kéo ngược/làm lại được |
| 7 | Cả thân kéo C1 sát tháp (nửa thân kéo không nổi) | Mốc 6 → mốc 7; cơ quan/tiếp xúc thật xác nhận | Vật thật đổi trạng thái (chốt, bậc, cửa, dây) | Đổi đích huỷ tiếp cận; cơ quan chưa chốt thì kéo ngược/làm lại được |
| 8 | Leo C1 → C2 → C3 → tháp, chui ra | Mốc 7 → mốc 8; cơ quan/tiếp xúc thật xác nhận | Vật thật đổi trạng thái (chốt, bậc, cửa, dây) | Đổi đích huỷ tiếp cận; cơ quan chưa chốt thì kéo ngược/làm lại được |

- **Phục hồi riêng:** Kéo C1 khi C3 còn phía trước: mái chìa chặn, C1 dừng ngắn — kéo C1 về, đẩy C3 ra sau, kéo lại. Mọi khối kéo ngược được trừ then đã chốt.
- Dòng khối lượng: `100% → 50% + 50% → 100%`. Tách chỉ qua Q; đủ gần và không vật cản thì tự nhập, không cooldown.
- Thắng: toàn bộ mô nhập thành một cơ thể **trong hộp** rồi qua lỗ cuối; ra trước khi nhập hết thì thua đúng câu hiện hành. Retry trả mọi vật về đầu.

## 5. Cơ quan, trạng thái và kiến trúc dùng lại

| MechanismId | Loại | Hợp đồng | Reset |
| --- | --- | --- | --- |
| b1.1 | Ray trên xe hai tầng (có) | Xem [MECHANICS](../MECHANICS.md) | Reset pose, lực, chốt, tác vụ và người giữ |
| b1.2 | COgheLoadLatch AND + pawl (có) | Xem [MECHANICS](../MECHANICS.md) | Reset pose, lực, chốt, tác vụ và người giữ |
| b1.3 | Khối nặng + CompensateLoad (có) | Xem [MECHANICS](../MECHANICS.md) | Reset pose, lực, chốt, tác vụ và người giữ |
| b1.4 | COgheQuantumSplitter (có) | Xem [MECHANICS](../MECHANICS.md) | Reset pose, lực, chốt, tác vụ và người giữ |

Rủi ro cần prototype: Ba tầng ray lồng nhau cần prototype: khớp nối xe–xe, chỗ đứng khi đẩy C2/C3, lực kéo C1 có cả tháp.

## 6. Hình ảnh, animation và phản hồi

Glass C / mạch in / ray satin theo STYLE_RULES như Spatial 11–30: A xanh tròn, B san hô; tím = trơn; mint = lỗ cuối; bánh răng cố định màu nhựa hổ phách của bộ bánh răng cũ, bánh trên xe trượt màu ngà (cần art pass riêng cho bánh răng Glass C). Nhận ý định → tiếp cận → tác động → kết quả theo trạng thái thật.

## 7. Độ khó và ngân sách runtime

| Trục | Dự kiến | Thực đo |
| --- | --- | --- |
| Phần cơ thể tối đa | 2 | 2 (lời giải mẫu) |
| Số chạm lời giải mẫu | khoảng 18 | 16 (native Mac, tác giả) |
| Thời gian lần đầu | 4–7 phút | Tác giả 30 s; người mới: chưa đo |
| Độ chính xác | Thấp: đích rộng, không canh thời điểm | Không cần canh thời điểm |

Không gộp thành điểm khó tổng. So sánh vị trí dùng số chạm đo được của các màn cũ (xem [PLACEMENT](../PLACEMENT.md)).

## 8. Chơi thử và hồi quy

- `SpatialPlusB1Solve`: lời giải mẫu bằng chạm thật từ đầu tới lỗ thoát, rồi Retry về đầu (PlayMode, 120 Hz).
- `SpatialPlusB1Wander`: đi lang thang — chạm mọi góc và giữa mọi mặt cố định đi được (sàn, bệ, sàn cao), rồi về điểm xuất phát; kẹt ở đâu là trượt tại đó. Sau đó Retry và giải trọn.

- Native Mac (build 50 màn, chạy theo thứ tự catalog): qua, 16 chạm, 30 s, p95 16.7 ms.
- Bằng chứng: [báo cáo kiểm thử Spatial Plus](../../../../Verification/COgheSpatialPlus/README.md).

## 9. Bằng chứng hiệu năng trên thiết bị

Mac (M4, author replay): p95 16.7 ms/khung. OPPO: chưa đo — đo như PLAN của Spatial 11–30.

## 10. Tích hợp, tương thích và nghiệm thu

- ID mới, không đụng save của 30 màn đã có; thứ tự hiển thị đổi theo [PLACEMENT](../PLACEMENT.md), ID màn cũ giữ nguyên.
- Kết luận: **đã dựng và kiểm thử tự động**; chờ Mrk chơi thử, đo OPPO và người chơi mới.
