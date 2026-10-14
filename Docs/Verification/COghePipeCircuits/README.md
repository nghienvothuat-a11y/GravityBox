# COghe — tốc độ chui ống và mạch nằm trên bề mặt

Ngày 14/10/2026, nhánh NewGraphic. Áp dụng yêu cầu tăng tốc độ chui ống 1,5 lần và sửa mạch lơ lửng.

## Thay đổi

- `COghePipeFlow.SpeedMultiplier = 1.5`: dùng chung cho mạng ống `COgheTubeNetwork` và ống chuyển thẳng cũ. Vận tốc dọc ống tăng từ 0,13 lên 0,195 m/s; đoạn ra miệng ống từ 0,11 lên 0,165 m/s, trước các hệ số giảm tốc/giữ đuôi. Không thay tốc độ bò, lực căn tâm, giới hạn gia tốc, va chạm hoặc luật hợp thể.
- Mạch, tiếp điểm và ray trang trí được bám vào mặt kính/sàn/bệ thật lúc dựng asset. Đường nối từ bệ cao đi theo các mặt liên thông; tránh lỗ thoát. Không dùng mặt đỡ di động hoặc mặt chỉ xuất hiện khi cầu cập bến làm nền cho mạch cố định.
- Chân tiếp điểm có lề để không nhô khỏi mép. Dây ròng rọc vẫn là dây cơ khí. Không thêm collider, tấm kính giả hay tính toán tìm đường mạch lúc chơi.
- Sửa cả authoring chung và các scene đã lưu trong catalog 65 màn. Không đổi thứ tự màn, ID, cơ quan hay lời giải.

## Kiểm chứng

- Audit toàn bộ **65 màn**, **319 renderer mạch/ray**, **64.836 mẫu hình học**: **0 mẫu thiếu mặt đỡ**, ngưỡng khoảng cách 4 mm. Kiểm tra mẫu đỉnh và tâm tam giác trên các mặt cố định, loại mặt đỡ di động/có điều kiện; đây là kiểm tra hình học, không thay kiểm tra hình ảnh.
- Snapshot vật lý trước/sau giống hệt: collider, Rigidbody, joint, mặt bám, rail, điều khiển, lift, pulley và cấu hình liên kết. Hash và kết quả ở [verification.json](verification.json).
- **16/16 PlayMode tests passed**: giải 8 màn có mạng ống (số hiển thị **15, 19, 29, 34, 41, 43, 49, 53**), đo tốc độ/độ liền khối, hồi quy khóa một chiều và một số màn có mạch trên bệ/ở độ cao khác nhau.
- Cùng tuyến ống tại màn hiển thị 15, từ lúc vào đến lúc toàn thân được thả khỏi ống: **10,658 → 5,850 giây mô phỏng**, số phần tối đa **1**. Thời gian tổng không tỷ lệ tuyến tính chính xác với vận tốc đặt vì còn va chạm, ổn định thân và điều tiết phần đuôi.
- Bản Mac standalone: replay bằng touch thật tại màn hiển thị 15 **PASS**, thoát đủ 32/32 hạt, còn 1 khối; FPS trung bình 41.98 trên Apple M5. Dùng FixedUpdate bình thường, không chế độ chạy nhanh; chỉ là số đo Mac, không đại diện thiết bị di động. [Run JSON](native-run.json).
- Ảnh trong gallery chụp bằng Unity/Metal từ scene thật, giữ Glass C. Chưa đo FPS mới trên OPPO/iPad trong đợt sửa này.

## Cách kiểm tra lại

Unity menu **Gravity Box → COghe → Spatial → Audit circuit surface support**. Audit báo lỗi nếu còn hình học mạch không có mặt đỡ.

Muốn áp dụng lại art vào scene có sẵn: **Repair printed circuits on physical surfaces**. Công cụ so snapshot vật lý và dừng nếu dữ liệu vật lý thay đổi. Không tái sinh toàn bộ gameplay để sửa mạch.

Test chính: `COgheSpatialCampaignTests.Spatial14PipePaceKeepsTheWholeBody`; các solve tests trong `Artifacts/COghePipeCircuit20261014/regression.xml`. Báo cáo log đầy đủ nằm ở `Artifacts/COghePipeCircuit20261014` (không đưa build/cache lên Git).

[Gallery runtime](review.html).
