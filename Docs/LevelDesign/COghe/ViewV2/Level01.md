# COghe V2 01 — Chạm để đi

## 1. Định danh, phạm vi và trạng thái
ID `coghe.view.v2.01`; slot 1/10; phiên bản2. Scene `Assets/_Game/Venom/ViewCampaign/COgheView01.unity`, definition `Definitions/View01.asset`; builder `COgheViewCampaignBuilder.cs`. Người thực hiện Codex, 28/09/2026. Thiết kế dựa bộ concept10 màn và yêu cầu triển khai của Mrk ngày28/09 (event09cb747e27dc1fbdfb6763a0a443dba3db980f474115b1d5e2612518ef279825).
Trạng thái: Prototype đã đạt kiểm tra Unity và author replay; chưa nghiệm thu thiết bị mobile/người mới. Toàn bộ PlayMode441/441 và EditMode8/8 tại commit `bd0ca04`; [báo cáo và giới hạn](../../../Verification/COgheViewV2/README.md).

## 2. Mục tiêu và tiến trình
Ý chính: Chạm để đi. Vai trò: giới thiệu hoặc kết hợp một ý chính. Một cơ thể, không cắt. Hướng dẫn ngắn trong definition; không cấp sức mạnh/chỉ số. Không mở phần thưởng vĩnh viễn mới.

## 3. Hình học, camera và thao tác
Buồng .8×.6m, sàn y=-.30m, vỏ cao .34m. Tâm spawn y=-.250m: tính cả bán kính và lớp hạt thấp nhất để không chồng vào sàn lúc bắt đầu. Đường sàn/bậc rộng theo builder.
Camera trực giao, yaw bằng kéo ngang, pitch cố định; hộp khóa vật lý. Pinch zoom và Toàn cảnh. Vỏ ngoài phía nhìn vào không chọn/không render; vách/cửa/nắp thật vẫn cản chọn. Mục tiêu trên màn hình phải được kiểm tra cả720×1280 và720×1612, không dựa vào kích thước world để kết luận vùng chạm đủ lớn.

## 4. Lời giải, kết thúc và phục hồi
Chạm lỗ thoát.
Chỉ dùng screen picking và simulation; không gán pose sinh vật/cửa hoặc trạng thái thắng. Đủ32 hạt của một cơ thể thoát lỗ cuối mới thắng. Cửa kín không nhận thắng xuyên vật cản. Làm ngược hoặc bỏ dở: ray hai nấc có thể gọi tiếp/đảo khi hoàn tất; lệnh đang chạy không xếp hàng. Mất điểm bám/kẹt có timeout và thông báo, Retry luôn đặt lại scene. Pause giữ trạng thái; Retry huỷ input/tác vụ/camera. Không áp dụng tách/transfer vì màn không có chúng.

## 5. Cơ quan và trạng thái
Không có cơ quan động.

Navigation rebuild theo pose thực; không dùng số màn để đổi lực hoặc luật thắng. Lời giải chỉ nằm trong author replay, không được Boss HUD đọc.

## 6. Hình ảnh và phản hồi
Day Lab: nền kem, vách xanh, cơ quan amber, trạng thái mint, COghe đen. Mesh bo cạnh riêng không collider, dùng shared materials và mesh batching. Đèn đầu ra đọc vị trí thật. Phản hồi chạm → đang tới → đang chuyển → khớp/kẹt. Mẫu tham chiếu source là Day Lab07; concept V2 định hướng bố cục, không chứng minh hình học hoặc khả giải.

## 7. Độ khó và chi phí
Không suy luận chuỗi cơ quan, tập trung một cử chỉ hoặc mặt đi.
Giữ32 hạt/120Hz; chi phí rail, graph, skin và kính phải đo. Không chốt trần renderer/memory khi chưa có baseline thiết bị. Không biến điểm chạm nhỏ thành độ khó.

## 8. Kiểm chứng
Test lời giải `COgheViewCampaignTests.View01Tap`; assertions đủ32 hạt, một body, không Lost, hộp không xoay và retry reset. Chạy cùng toàn bộ PlayMode và EditMode. Thêm gesture/input/occlusion/catalog tests dùng chung. Native HUD, touch trên điện thoại và người chưa biết lời giải cần bằng chứng riêng. XML cuối đạt; xem báo cáo chung để đối chiếu artifact và phạm vi thực sự đã chạy.

## 9. Hiệu năng thiết bị
Mục tiêu60FPS, p95≤20ms, p99≤33.3ms; ghi spikes>50ms, memory/GC, CPU/GPU và phiên15–20 phút. Máy/SoC/OS, build/hash, chất lượng, thời lượng, số lượt: chờ báo cáo đo. ADB lúc bắt đầu không có thiết bị. Không dùng số đo Mac để nghiệm thu Android.

## 10. Tích hợp và tương thích
CatalogV2 mặc định có10 entry, ID mới; key save production giữ nguyên nên completion legacy và Nhà không bị xoá. Next/selector dùng SceneSequenceV2; không phụ thuộc cờ pilot. Chưa gọi nghiệm thu mobile trước khi có thiết bị. Hồ sơ này bổ sung lịch sử; không thay file thiết kế level cũ.

## NewGraphic — 29/09/2026

Thử nghiệm art Blender: vỏ sứ bo cạnh, panel xanh nhạt, cơ quan amber, viền thoát mint; giữ bố cục và đường giải của hồ sơ này. Không đổi collider, input, rail, solver hay điều kiện thắng. Nguồn và kiểm tra A/B xem [báo cáo](../../../Verification/COgheNewGraphic/README.md).


## Thay đổi phần nhìn — 29/09/2026

Theo yêu cầu bỏ phương án Blender: màn 01 trở lại hộp kính trong, khung nhôm mảnh, góc sứ, sàn xanh nhạt và viền lỗ mint. Giữ layout/kích thước/physics/luật thắng/ID V2. Mặt kính gần camera vẫn nhìn thấy rất nhẹ bằng shader kính; bỏ cơ chế che hoàn toàn vách đặc của màn này. Thao tác chạm chọn mặt và orbit không đổi. Builder: `COgheGlassPreviewBuilder`, được gọi từ bộ dựng art V2 cho màn 01. Bằng chứng và bản Mac: [Glass preview](../../../Verification/COgheGlassPreview/README.md).

## Thử chiều sâu đồ hoạ — 29/09/2026

Thêm bộ so sánh Gốc/A/B/C ở màn 01: ánh sáng và bóng tiếp xúc; phản xạ kính và chất liệu; nền lab nhẹ. Đây là thử nghiệm theo yêu cầu người dùng, chưa chọn thành style chung. Đổi phương án tải lại màn 01. Chỉ đọc vị trí mô để vẽ bóng tiếp xúc, không sửa collider, solver, mục tiêu chạm hay luật thắng. Xem [kết quả thật](../../../Verification/COgheGlassDepth/README.md).

## Chốt C và nhãn mẫu vật — 29/09/2026

Người dùng chọn C. Màn mẫu 01 dùng C mặc định và ẩn nút so sánh. Thêm nhãn nhựa nhỏ ghi số **1**, ngàm gắn lên mép trước phía trên hộp. Builder áp nhãn tương ứng cho cả 30 màn V2, không sửa bố cục hay cơ quan. Xem [kiểm tra](../../../Verification/COgheSpecimenPlates/README.md).

Hiệu chỉnh theo concept gốc: bảng nhựa màu ngà ghi **01** và gạch nhỏ, áp trực tiếp ngoài mặt kính ở góc trên trái, dưới mép khung. Bỏ kiểu bảng có ngàm dựng trên khung đã bị người dùng từ chối.
