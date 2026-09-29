# COghe Spatial 02 — Leo từng bậc

## 1. Định danh, phạm vi và trạng thái

`coghe.spatial.pilot.02` — Leo từng bậc. Catalog riêng 10 màn trên NewGraphic, giữ nguyên V2 30 màn.
Scene: `Assets/_Game/Venom/SpatialCampaign/COgheSpatial02.unity`. Builder: `COgheSpatialCampaignBuilder`. Ngày 2026-09-29. Trạng thái: prototype đã kiểm chứng đường giải trên PlayMode và native Mac; mobile và người chơi mới chưa kiểm chứng.
[Minh hoạ trước dựng](../Illustrations/01-02.png). Ảnh định hướng hình học, kích thước scene quyết định va chạm; không dùng ảnh làm bằng chứng chạy game.

## 2. Mục tiêu trải nghiệm và vị trí trong tiến trình

Leo hai khối cố định. Giới thiệu/luyện tập một ý chính, không thao tác căn thời gian hẹp. Không tăng chỉ số, không yêu cầu mua đồ.

## 3. Phác thảo, hình học, camera và thao tác

Hai bậc rộng nối sàn với lỗ cao; không yêu cầu nhảy.
Hộp kính khoảng 0.8 × 0.6 m; chiều cao theo cơ quan. Camera 3/4, kéo để quan sát, pinch để phóng; hộp đứng yên. Nóc và vật liệu trơn vẫn nhận chạm; trơn không cấp sức bám. Mục tiêu thử: 720×1280 và màn dài, không chạm xuyên cơ quan. Bề rộng đường dự kiến ≥0.14m cho cơ thể ~0.07m.

## 4. Lời giải, kết thúc và phục hồi

Leo hai khối cố định. Chạm đích → tiếp cận thật → tác động bằng lực → thấy hình học mở đường → chạm vòng thoát. Có thể đổi lệnh và làm lại cơ quan. Rơi về sàn có đường leo lại; khay nâng phải có cách gọi về.
Thắng khi toàn bộ 32 hạt đã hợp thể trong hộp trước lần ra đầu tiên, rồi thoát đúng lỗ cuối. Không dao trong bộ này; luật tách/tụ chung giữ nguyên. Retry huỷ lệnh và reset ray/cáp/khay. Không dùng teleport để giải.

## 5. Cơ quan, trạng thái và kiến trúc dùng lại

Dùng `COgheTapRail`, `COgheRailSlider` cho thao tác và ray hữu hạn; pulley/lift là component dữ liệu độc lập. Cáp truyền lực giữa hai ray, khay nâng có motor, điểm dừng và kiểm tra người đứng trên khay. Lực không phụ thuộc số màn. Route đổi khi hình học đổi; không tạo route bằng trạng thái mỹ thuật. Không đọc lời giải trong runtime.

## 6. Hình ảnh, animation và phản hồi

Glass C đã chốt: kính trong, khung nhôm mảnh, bảng số nhựa ngà dán mặt kính, nền phòng lab dịu. Sinh vật đen giữ tạo hình; bò/leo/ghì kéo từ tiếp xúc và lực thật. A xanh tròn, B san hô hình thoi, tím trơn, mint lỗ cuối. Dây đi qua bánh, ray có cột đỡ. Đèn trạng thái phụ, không đổi màu nhận diện circuit. [Ảnh native Mac](../../../../Verification/COgheSpatialPilot/02-start.png).

## 7. Độ khó và ngân sách runtime

Số thao tác suy luận tăng dần: màn 2/10. Không cần căn rơi/nhảy. Giữ 32 hạt, fixed timestep 1/120; không mô phỏng dây bằng chuỗi rigidbody. Dùng chung material, mesh tĩnh gộp, một nguồn bóng mềm. Ngân sách và FPS cần đo thật; chưa tuyên bố đạt.

## 8. Chơi thử và hồi quy

Đã chạy đường giải bằng chạm/camera trên native Mac và PlayMode. Bộ test chung kiểm tra đổi lệnh, thang lên/xuống, kéo ngược ròng rọc, idle, va chạm khởi tạo, retry và mở Nhà; hồi quy V2 đạt. Rơi tự do ở mọi góc, thử ngẫu nhiên kéo dài và người chơi mới chưa được phủ toàn diện. Test không thay thế đánh giá người chơi mới.

## 9. Bằng chứng hiệu năng trên thiết bị

Đã đo native Mac M5 / macOS 26.5.1 / Metal / 720×1280: TB 60.0 FPS, p50 16.67 ms, p95 16.73 ms, p99 16.90 ms, max 16.97 ms. Chờ 2 giây trước lượt đường giải ngắn; không phải thermal soak. [Raw data và cấu hình](../../../../Verification/COgheSpatialPilot/mac-native-run.json). OPPO chưa kết nối; Android chưa đo.

## 10. Tích hợp, tương thích và nghiệm thu

Catalog/scene/save riêng `coghe.spatial.pilot`; không ghi đè tiến độ V2. Menu/build Mac riêng để so sánh. Prototype đạt các đường giải và ca kiểm chứng đã liệt kê trên Mac/PlayMode. Chưa nghiệm thu Android hoặc độ khó người chơi mới. Sai khác hình minh hoạ có giải thích trong ARCHITECTURE.md của bộ test.


### Kết quả dựng và kiểm chứng 29/09/2026

- Scene đã dựng và qua màn bằng input thực: 32/32 hạt thoát, 1 cơ thể, không lỗi runtime.
- [Ảnh + báo cáo Mac](../../../../Verification/COgheSpatialPilot/README.md). PlayMode 15/15 ca của catalog mới; hồi quy V2 26/26.
- Mac M5 720×1280: FPS trung bình 60.0; p95 16.73 ms; đây là lượt author replay ngắn, không đại diện Android hay người mới.
- Các mục “chưa kiểm chứng” trong kế hoạch ban đầu được cập nhật ở kết quả này đối với đường giải chính, reset, camera và cơ quan đã chạy; chưa có xác nhận người chơi mới/Android.
