# COghe — venom.origin.30 — Nhà máy tí hon

## 1. Định danh, phạm vi và trạng thái

- ID ổn định `venom.origin.30`; **Boss đầu tiên ở vị trí hiển thị 10**, không phải màn cuối 30.
- Boss cũ `venom.origin.10` ở vị trí 20; Boss cũ `venom.origin.20` ở vị trí 30; tất cả ID cũ giữ nguyên.
- Prototype Unity v2, 17/09/2026; scene, definition, builder, bộ truyền G và tời B đã tồn tại.
- Origin hiện hành, 32 hạt/120 Hz. Yêu cầu: Boss vừa sức, một thân, kỹ năng đã học, 2–3 quyết định, không tách/tilt/timing mới.
- Chuỗi cơ quan, trạng thái mở cửa, reset, camera chạm và ngân sách mô phỏng đã qua test macOS; chưa playtest tay mobile.
Nguồn: [quy tắc level](../../../COGHE_LEVEL_DESIGN_RULES.md), [template](../LEVEL_TEMPLATE.md), [Day Lab](../../../ArtDirection/COghe/STYLE_RULES.md), [kiến trúc](../../../COGHE_EXPANSION_11_20.md).

## 2. Mục tiêu trải nghiệm và tiến trình

- Biến kỹ năng đã học thành một “nhà máy” nhỏ: nối nguồn bằng một bánh rồi vận hành cửa bằng động tác kéo quen thuộc.
- Đã học đẩy/kéo và một bánh trung gian ở màn mới 23; hành động thứ hai chỉ là kéo một tay nắm trên ray, không cần kỹ năng màn cũ 13.
- Khoảnh khắc khám phá: nối G làm máy hoạt động và mở nắp nhỏ, lộ tay B; người chơi tự nhận ra động tác kế tiếp.
- **Boss, Hints=None**: không hướng dẫn lời giải, số bước, mũi tên từ G sang B hoặc popup gợi thứ tự; vẫn có phản hồi chạm/trạng thái.
- Không nâng chỉ số/buff Nhà; thắng ghi hoàn tất và đề xuất mở Nhà tại mốc Boss hiển thị 10, chi tiết ở mục 10.

## 3. Phác thảo, hình học, camera và thao tác

![Phác thảo 30 · Nhà máy tí hon](mockup-v1.png)

[Xem cả 10 bản phác thảo](../Sketches21-30/review.html). Bản v1 để duyệt bố cục và thao tác;
chi tiết tỷ lệ, vách kín, ray/chốt và tiếp xúc cơ khí được giản lược, cần kiểm chứng khi dựng.

- Sơ đồ tác giả v0.1: `Spawn → motor O — [G] — O → nắp hốc B; B ─cáp→ cửa cuối → Exit`.
- Hộp khoảng 0,65 m, sàn bám; spawn trước-trái, G giữa-trước, hốc B giữa-phải, lỗ cuối vách phải phía sau.
- Khoang thoát có vách kín từ sàn tới nóc; aperture duy nhất bị cửa cuối đặc che. B không nằm trực tiếp trên nắp ngoài để bò vòng lấy tay.
- Hốc B có vách/đáy/mái thật; nắp nhỏ trượt sang bên, không nhấc thành mái thấp tạo kẹt hoặc che tay B khi mở.
- G trên ray ngang 50–70 mm, tay 60 mm; B trên ray kéo 60–80 mm; vùng bám thao tác ít nhất khoảng 0,12 m, số liệu chưa thử.
- Không cửa định giờ: nắp hốc và cửa cuối có catch khi mở; đường tới B và Exit rộng, cữ/rail không nằm trước đích chạm.
- Khóa xoay; camera tĩnh khoảng 37°/15°, overview fit bounds tĩnh; cả nguồn/G/hốc/Exit nhìn được, follow không đổi luật chơi.
- Chạm tay G/B → tiếp cận/bám; chạm đích rộng → kéo; buông hoặc 3 giây không lệnh chỉ buông prop, giữ chốt đã gài.
- Sau buông chạm sàn/Exit để bò; nóc/trơn hợp lệ vẫn nhận đích, không bám giả. Thử 720×1280/720×1612 và safe area.

## 4. Lời giải, kết thúc và phục hồi

| Bước | Lệnh tác giả/test, không hiện trong Boss | Trạng thái / tín hiệu | Bỏ dở hoặc làm ngược |
| --- | --- | --- | --- |
| 1 | Đưa G vào ổ | Bộ truyền quay, nắp hốc B trượt mở và chốt | Lùi G giữa chừng: phanh nắp; gài lại tiếp tục |
| 2 | Buông G, tới B rồi kéo | COghe truyền lực kéo cửa cuối tới chốt | Buông sớm phanh giữ tải, bám lại kéo tiếp |
| 3 | Buông và dẫn thân ra Exit | Cửa thật thông, toàn thân thoát | Không cần giữ G hoặc B khi đi ra |

- Thắng chỉ khi một cơ thể hợp nhất bên trong rồi đủ 32 hạt qua `FinalExit`; không dao/Transfer trong Boss này.
- Cửa kín tự chặn đường tắt bò tường/nóc. Không dùng cờ “đã chọn G, đã chọn B” hoặc số màn để hợp thức hóa nắp hở.
- B trước G: tay bị nắp/hốc che thật; chạm nắp không xuyên tới tay. Sau nắp chốt, rút G vẫn có thể dùng B — trạng thái hợp lệ.
- Cửa cuối chưa mở đủ: kéo tiếp được. Không đòi thao tác đồng thời hoặc đi qua trước lúc nắp tự đóng.
- Tay luôn còn phía tiếp cận được; vật không rời ray, không khe kẹp không lối gỡ; reverse nhả catch có kiểm soát, retry mọi lúc.
- Pause dừng mô phỏng; reset trả G/nắp/B/cửa/phanh/chốt/vận tốc/lệnh về ban đầu, không xóa quyền Nhà đã có.
- Nếu fixture tạo nhiều phần, vẫn tụ tự nhiên không cooldown và chưa tụ mà thoát dùng lỗi “bạn phải hợp thể trước khi chui ra”.

## 5. Cơ quan và kiến trúc dùng lại

| Cơ quan | Lực / hành trình ước lượng | Trạng thái, phụ thuộc và reset |
| --- | --- | --- |
| G / `COgheRailSlider` | Ngang 50–70 mm, cản 0,03–0,05 N | Catch ở ổ, kéo ngược nhả; reset rời nguồn |
| `COgheGearTrain` + nắp B | Motor hiện rõ, khoảng 0,03 N·m; nắp dịch 70 mm | Nguồn–G–tải nối mới dịch, ngắt phanh, cuối chốt |
| B + cửa cuối | Tay kéo/cáp có tỉ số cố định, cửa khoảng 0,10 m | COghe cấp lực hữu hạn, phanh giữ khi buông, cuối catch; reset đóng |

- Rail/gear/catch có sẵn; truyền kéo–cáp và phanh cần cấu hình/component dùng lại như màn 25, không tuyên bố đã chạy trong scene mới.
- Nguồn thứ nhất là motor có thân/trục, nguồn thứ hai là mô kéo qua cáp; không animation tự cung cấp lực/mở cửa.
- Nắp hốc/cửa/ổ động làm đổi route; không kiểm tra lịch sử lời giải, không tự chọn G hoặc B thay người chơi.

## 6. Hình ảnh, animation và phản hồi

- Day Lab màn 07 với chi tiết lắp ráp vừa đủ: vỏ sứ, ổ hổ phách, trục/ray bạc, đèn mint theo trạng thái thật.
- Nhận lệnh → tới tay → thân kéo → bánh/nắp/catch phản hồi; âm máy/nắp là thông tin trạng thái, không lời giải bằng giọng nói.
- Không nhãn “bước 1/2”; cửa cuối sáng rõ khi thật sự mở. Thắng dùng ba điệu nhảy/camera hiện có; chưa có render thực.

## 7. Độ khó và ngân sách runtime

- Dự kiến 2 quyết định phụ thuộc + chọn lối ra, một cơ thể/không nhiệm vụ đồng thời; mục tiêu thử 2–4 phút, không precision/timing.
- Một carriage, nắp, tay B và cửa; hai hệ truyền hữu hạn. Ngân sách contact/route/skin/GC/GPU chưa đo/chốt.
- Dùng kit/ánh sáng Day Lab, không tăng collider răng hay phản xạ realtime để “trông giống Boss”; không tuyên bố FPS từ hình đơn giản.

## 8. Chơi thử và hồi quy

- Tuyến Boss G→B→cửa cuối→lỗ đã qua full-solution test bằng cơ quan thật. B trước G, reverse và playtest mobile vẫn là checklist hiệu chỉnh.
- Thử bỏ cả hai cơ quan rồi bò mọi mặt tới Exit; aperture phải bị chắn thật. Thử chạm nắp trước tay; không teleport hay ép nắp mở.
- Người mới chưa biết lời giải: ghi số người/lượt, thời gian, do dự, chạm sai và có tự hiểu tác dụng motor không; Boss không được gợi chuỗi.
- Hồi quy rail/gear/truyền cáp, màn mới 23/25 và màn cũ 17/20; thêm tests catalog/save/House theo mục 10.

## 9. Bằng chứng hiệu năng trên thiết bị

- Chưa có máy/OS/build/SHA/log, p50/p95/p99/max/spike/CPU/GPU/GC/memory hoặc phiên nhiệt; chưa nghiệm thu playable/mobile.
- Đo OPPO được xác nhận cùng baseline/profile 32 hạt/120 Hz; 60 FPS là mục tiêu, ngưỡng p95/p99 phải đặt trước nghiệm thu.
- Đo G→nắp→B→cửa→ăn mừng/House và phiên 15–20 phút; ghi cấu hình/resolution/warm-up/commit/diff và thời lượng thực.

## 10. Tích hợp, tương thích và nghiệm thu

- **Đề xuất tiến trình mới, chưa triển khai:** Nhà mở sau Boss đầu tiên ở **vị trí hiển thị 10**, tức ID mới `venom.origin.30`.
- Đây là thay đổi nội dung/thứ tự Boss đã tích hợp trong runtime và catalog campaign 30 màn.
- Người cũ có `HomeUnlocked=true` giữ quyền; ID cũ 10 chuyển vị trí 20 vẫn giữ Completed, replay không cấp trùng; Boss cũ 20 ở vị trí 30.
- Runtime hiện tại `VenomCampaignSave.Win` mở Nhà khi thắng **bất kỳ definition.Boss** nếu chưa có quyền. Giữ hành vi đó: Boss mới 30 ở vị trí 10 sẽ là lần mở đầu trên đường chơi thường; không hardcode ID 30.
- `Completed` lưu danh sách ID; mapping display→ID/next đã tích hợp mà không đổi schema. Save cũ/mới, replay, next và build catalog đã có hồi quy tự động; thoát app đúng lúc lưu–ăn mừng vẫn cần test tay.
- Kết luận Nháp v0.1; còn dựng gameplay/input, xác nhận độ khó với người mới, hiệu năng và migration trước nghiệm thu.
