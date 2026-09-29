# COghe V2 04 — Nhìn gần hơn

## 1. Định danh, phạm vi và trạng thái
ID `coghe.view.v2.04`; slot 4/30; phiên bản3. Scene `Assets/_Game/Venom/ViewCampaign/COgheView04.unity`, definition `Definitions/View04.asset`; builder `COgheViewCampaignBuilder.cs`. Người thực hiện Codex, 28/09/2026. Thiết kế dựa bộ concept10 màn và yêu cầu triển khai của Mrk ngày28/09 (event09cb747e27dc1fbdfb6763a0a443dba3db980f474115b1d5e2612518ef279825).
Trạng thái: **đã qua kiểm chứng tự động trên Mac sau thay đổi ngày 28/09/2026**. Mrk yêu cầu màn 4 khác màn 2 và làm rõ việc phóng gần để đọc đường đi (event `097510c4952b8b89b116cd0b42bd447a16807fe946cf3c59b976acda5618e4cd`). Bản cũ dùng bậc dốc giống màn 2; kết quả 441/441 tại `bd0ca04` chỉ thuộc bố cục cũ. Native diagnostic 22 cùng cả hai replay 30 màn đã giải được bố cục mới. Full PlayMode 513/513, EditMode 8/8 đạt; [bằng chứng và giới hạn](../../../Verification/COgheViewExpansion/README.md).

## 2. Mục tiêu và tiến trình
Ý chính: phóng gần để đọc rãnh dẫn qua các khúc rẽ. Màn 2 học leo mặt nghiêng; màn 4 đi trên cùng một sàn, quan sát hai khe ở hai đầu đối diện của vách thấp. Một cơ thể, không cắt. Hướng dẫn ngắn trong definition; không cấp sức mạnh/chỉ số. Không mở phần thưởng vĩnh viễn mới.

## 3. Hình học, camera và thao tác
Buồng .8×.6m, sàn y=-.30m, vỏ cao .34m. Tâm spawn y=-.250m, tính cả bán kính hạt. Hai vách thấp trơn cao .07m, dày .035m, dài .56m: vách I tâm x=-.12/z=-.10, khe phải; vách II tâm x=.12/z=.10, khe trái. Rãnh bạc rộng .0025m chạy trên sàn qua hai khe; khoảng trống đầu vách rộng .24m để người chơi không cần chạm chính xác. Lỗ thoát trên sàn tại x=.22/z=.23, bán kính .046m. Không còn bậc hoặc dốc.
Camera trực giao, yaw bằng kéo ngang, pitch cố định; hộp khóa vật lý. Pinch zoom và Toàn cảnh. Vỏ ngoài phía nhìn vào không chọn; cả vách và viền hiện/ẩn dần theo góc xoay; vách/cửa/nắp thật vẫn cản chọn. Mục tiêu trên màn hình phải được kiểm tra cả720×1280 và720×1612, không dựa vào kích thước world để kết luận vùng chạm đủ lớn.

## 4. Lời giải, kết thúc và phục hồi
Pinch để đọc rãnh; dùng Toàn cảnh khi cần. Đi tới đầu phải vách I, qua khe vào hành lang giữa, đi tới đầu trái vách II, qua khe ra hành lang sau, rồi tới vòng xanh trên sàn. Zoom chỉ giúp quan sát: người đã nhìn rõ đường có thể giải mà không pinch; không có cờ zoom ẩn khóa lối thoát.
Chỉ dùng screen picking và simulation; không gán pose sinh vật/cửa hoặc trạng thái thắng. Đủ32 hạt của một cơ thể thoát lỗ cuối mới thắng. Cửa kín không nhận thắng xuyên vật cản. Chạm một đích khác trên sàn sẽ thay lệnh đi. Có thể đi ngược qua cả hai khe hoặc quay về điểm xuất phát; không có cửa chốt làm mắc kẹt. Chạm mặt trơn không cấp lực bám để leo tắt qua vách. Mất điểm bám/kẹt có timeout và thông báo, Retry luôn đặt lại scene. Pause giữ trạng thái; Retry huỷ input/tác vụ/camera. Không áp dụng tách/transfer vì màn không có chúng.

## 5. Cơ quan và trạng thái
Không có cơ quan động.

Navigation rebuild theo pose thực; không dùng số màn để đổi lực hoặc luật thắng. Lời giải chỉ nằm trong author replay, không được Boss HUD đọc.

## 6. Hình ảnh và phản hồi
Day Lab: nền kem, vách xanh, cơ quan amber, trạng thái mint, COghe đen. Mesh bo cạnh riêng không collider, dùng shared materials và mesh batching. Đèn đầu ra đọc vị trí thật. Phản hồi chạm → đang tới → đang chuyển → khớp/kẹt. Mẫu tham chiếu source là Day Lab07; concept V2 định hướng bố cục, không chứng minh hình học hoặc khả giải.

## 7. Độ khó và chi phí
Không suy luận chuỗi cơ quan, tập trung quan sát hai khúc rẽ. Vật liệu satin tím báo mặt trơn; vách có bo cạnh thị giác cùng bộ art V2. Rãnh trang trí không có collider hay vùng nhận chạm.
Giữ32 hạt/120Hz; chi phí rail, graph, skin và kính phải đo. Không chốt trần renderer/memory khi chưa có baseline thiết bị. Không biến điểm chạm nhỏ thành độ khó.

## 8. Kiểm chứng
Test lời giải `COgheViewCampaignTests.View04Zoom`, `View04HasAnInspectionRouteWhoseDetailsEnlargeWithZoom` và `View04KnownRouteCanBeCompletedWithoutAHiddenZoomFlag`; assertions đủ32 hạt, một body, không Lost, hộp không xoay và retry reset. Chạy cùng toàn bộ PlayMode và EditMode. Thêm gesture/input/occlusion/catalog tests dùng chung. Native HUD, touch trên điện thoại và người chưa biết lời giải cần bằng chứng riêng. Xem báo cáo cuối của đợt 11–30 để đối chiếu đúng source, XML và ảnh; kết quả cũ không chứng nhận thay đổi này.

## 9. Hiệu năng thiết bị
Mục tiêu60FPS, p95≤20ms, p99≤33.3ms; ghi spikes>50ms, memory/GC, CPU/GPU và phiên15–20 phút. Máy/SoC/OS, build/hash, chất lượng, thời lượng, số lượt: chờ báo cáo đo. ADB lúc bắt đầu không có thiết bị. Không dùng số đo Mac để nghiệm thu Android.

## 10. Tích hợp và tương thích
CatalogV2 mặc định có30 entry, ID mới; key save production giữ nguyên nên completion legacy và Nhà không bị xoá. Next/selector dùng SceneSequenceV2; không phụ thuộc cờ pilot. Chưa gọi nghiệm thu mobile trước khi có thiết bị. Hồ sơ này bổ sung lịch sử; không thay file thiết kế level cũ.


## Lịch sử — bố cục cũ, được thay theo yêu cầu mới
Bậc có mặt chuyển nghiêng45°, cao .10m, rộng .80m; sàn phía trước kết thúc ở z=.08m, mặt trên bắt đầu z=.18m. Giữ bài học đổi mặt bám, giảm mép vuông trong bài đầu.

## NewGraphic — 29/09/2026

Thử nghiệm art Blender: vỏ sứ bo cạnh, panel xanh nhạt, cơ quan amber, viền thoát mint; giữ bố cục và đường giải của hồ sơ này. Không đổi collider, input, rail, solver hay điều kiện thắng. Nguồn và kiểm tra A/B xem [báo cáo](../../../Verification/COgheNewGraphic/README.md).
