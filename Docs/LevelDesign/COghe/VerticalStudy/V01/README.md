# COghe — Hồ sơ V01 — Trạm trên vách

Theo [LEVEL_TEMPLATE](../../LEVEL_TEMPLATE.md), [quy tắc level](../../../../COGHE_LEVEL_DESIGN_RULES.md) và [art C hiện hành](../../../../ArtDirection/COghe/STYLE_RULES.md).

## 1. Định danh, phạm vi và trạng thái

- ID đề xuất `study.vertical.wall-station`; phiên bản 0.1, 29/09/2026, Codex. Không đổi catalog hoặc màn 01 hiện có.
- **Nháp** phục vụ yêu cầu nghiên cứu của người dùng; chưa chốt scene, chưa dựng prototype. Dùng luật V2 camera-only và Origin cắt/tụ/thoát hiện hành.
- [Concept](../../../../ArtDirection/COghe/VerticalMechanisms/02-wall-station.png), [kế hoạch](../../../../ArtDirection/COghe/VerticalMechanisms/PLAN.md). Tranh mô tả ngoại hình, không xác nhận tỷ lệ vật lý.
- Mốc: thiết kế đã diễn giải; prototype, art Unity, kiểm thử và nghiệm thu đều chưa thực hiện.

## 2. Mục tiêu trải nghiệm và tiến trình

Người chơi nhận ra có thể bám vách để vận hành cơ quan. Chỉ một cụm xanh A, một cửa, không thêm vật liệu trơn hoặc tách cơ thể. Dùng sau bài bò và leo, trước tổ hợp nhiều cơ quan. Đây là màn giới thiệu; có phản hồi cặp A khi chạm, không cần chỉ đường toàn tuyến. Không tăng chỉ số, buff hoặc mở thưởng vĩnh viễn mới.

## 3. Hình học, camera, thao tác

- Hộp đứng, sàn rộng. Spawn dưới trái; tay kéo A trên vách trái ở khoảng nửa chiều cao; lỗ cuối trên vách phải khoảng 3/4 chiều cao, có cửa xanh A và bệ nghỉ bên dưới.
- Một dải nhựa ngà bám được ở chân tay kéo giúp đọc đường leo, nhưng kính thường vẫn leo được. Dải không phải thang bắt buộc. Cả trần và mặt sau được dùng làm đường vòng hợp lệ.
- Lấy D là bề rộng cơ thể nghỉ đầy đủ trong profile thực. Vùng thao tác dự kiến rộng ít nhất 2D; mặt chân phải đỡ mô trên đúng WorkingSurface khi tay chạm quai. Khoảng hở mép, hành trình và lực chưa chốt cho tới thử mô thật.
- Góc đầu 3/4 thấy tay kéo và cửa; drag chỉ orbit camera, pinch/Toàn cảnh giữ hiện có. Kiểm tra tay nắm không chiếu trùng rail trước, bảng số hoặc thân COghe ở 9:16 và màn dài.
- Chạm kính → Move; chạm quai A → tiếp cận và kéo tới chốt; chạm lại sau hoàn tất → kéo ngược. Đổi lệnh hủy tác vụ và lực đang sở hữu theo cơ chế chung. Tương tác không đòi hold đúng pixel.

## 4. Lời giải, kết thúc và phục hồi

| Bước | Lệnh và thay đổi thật | Phản hồi | Phục hồi |
|---|---|---|---|
| 1 | Chạm quai A; bò từ sàn lên mặt đỡ bên tay nắm | Vòng đích ở đúng vách; xúc tu tìm chỗ bám | Chạm nơi khác hủy, có thể leo lại từ mọi hướng |
| 2 | Cơ thể chống chân, kéo ray A tới chốt | Quai chuyển, truyền động chạy, cửa A trượt khỏi lỗ | Thiếu tiếp xúc thì dừng và báo bằng tư thế; không giật mô tới quai |
| 3 | Chạm vùng bám cạnh lỗ; bò dọc kính tới bệ và thoát | Cửa vẫn mang màu A; vòng mint xác định lỗ cuối | Đi ngược hoặc rơi xuống vẫn leo lại; chốt giữ cửa |

Có thể tới cửa trước, nhưng cửa thật chặn lỗ; không thắng vì chỉ chạm mục tiêu. Đường leo bất kỳ trên kính sau mở đều hợp lệ. Một cơ thể, không dao: toàn bộ 32 hạt phải ra đúng FinalExit. Vẫn giữ thất bại thoát sớm khi chưa tụ của runtime chung nếu trạng thái bị thử qua công cụ test; không tạo ngoại lệ riêng.

Retry reset rail, latch, cửa, lệnh/force và spawn; pause không tiếp tục đồng hồ tác vụ. Cửa chuyển động phải có bảo vệ kẹt mô thật, không cắt/chèn hạt. Không có timer bắt người chơi vội.

## 5. Cơ quan, trạng thái, kiến trúc

| ID | Phần dùng lại | Điều kiện → kết quả | Reset |
|---|---|---|---|
| A.handle | COgheTapRail + RailSlider | Chân bám vách + tay tới quai → lực lên trục ray | Về đầu ray, hủy sở hữu tác vụ |
| A.gate | COgheViewMechanism hoặc output rail tương đương | Vị trí input truyền sang shutter; chỉ mở khi thật hết hành trình | Đóng, aperture blocked |
| exit | FinalExit hiện có | Cửa thông + đầy đủ vật chất thoát | Trạng thái chưa thắng |

Chọn cấu hình latch có thể đảo và lực chống trọng lực phù hợp, không gán mở bằng animation. Navigation thay khi cửa đổi hình học, không khi màu/nhãn/camera đổi. Art chỉ đọc ID A và trạng thái thực; không thêm luật theo số màn. Chưa chứng minh cấu hình wall rail hiện tại hoạt động trọn, cần test.

## 6. Hình ảnh, animation và phản hồi

Giữ glass C, số V01 trên kính trong concept; số thật lấy catalog khi tích hợp. Quai/cửa có xanh lam + vòng tròn A, vỏ ngà. Truyền động phải có cơ sở cơ khí hoặc actuator lab rõ ràng, không chỉ trục quay trang trí. Thân co khi kéo, xúc tu giữ quai, phần thân treo võng theo trọng lực. Đèn nhỏ chốt sáng sau khi rail ổn định; cửa không đổi sang màu khác. Victory dùng hệ hiện có.

## 7. Độ khó và ngân sách

- Dự kiến: 1 quan hệ nhân quả, 1 tác vụ tại một thời điểm, không căn rơi/nhảy, không nhiều phần. Chưa có thời gian giải thực đo.
- Chi phí thêm dự kiến: một input rail, một output shutter và các bề mặt đỡ; chưa xác định số renderer/collider tối đa. Ưu tiên cấu kiện tĩnh, glyph dùng atlas, không đèn thật bổ sung.
- Không đổi 32 hạt hoặc nhịp vật lý. Trần frame time, graph và rendering phải lấy theo phép đo cùng OPPO.

## 8. Chơi thử và hồi quy

**Chưa chạy Unity cho đề xuất này.** Cần kiểm tra: tiếp cận từ dưới/trên/hai bên, chân tiếp xúc đúng mặt, kéo và đẩy ngược, spam/hủy lệnh, orbit trong thao tác, vật cản tay–quai, cửa chèn mô, pause/retry, lỗ thoát đủ mô. Chơi tay trên điện thoại và người chưa biết lời giải để kiểm tra ghép cặp A. Hồi quy các scene dùng TapRail và gate trước khi tích hợp.

## 9. Bằng chứng hiệu năng

Chưa có build ID, APK, thiết bị, mẫu frame time hay kết quả. Đo như PLAN: baseline cùng máy, 60 FPS là mục tiêu thử, không phải kết quả; ghi CPU/GPU/GC/graph khi rail chuyển, camera quay và chơi kéo dài. Tranh không phải artifact runtime.

## 10. Tích hợp và nghiệm thu

Chưa thêm catalog/save/build list; không ảnh hưởng tiến trình hiện có. Dùng ID ổn định khi prototype, không ghi đè scene cũ. Chỉ xếp thứ tự sau chơi thử. Kết luận: **đủ brief để dựng thử, chưa chứng minh khả giải, chưa nghiệm thu**. Việc tiếp theo: đo D/độ với/độ bám, dựng mặt đứng và thử chuỗi lệnh thật.
