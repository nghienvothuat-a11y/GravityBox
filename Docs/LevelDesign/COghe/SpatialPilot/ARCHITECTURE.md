# Spatial Pilot — kiến trúc và phạm vi

Catalog và definition riêng `coghe.spatial.pilot.NN`, save riêng. Builder sử dụng primitive/mesh Unity, không Blender. `COgheSpatialCampaignBuilder` chứa bố trí; runtime không biết số màn và không đọc lời giải thử nghiệm.

## Cơ quan dùng lại

- `COgheTapRail`: tiếp cận, đặt chân và tác động hữu hạn từ khối lượng thật. Tuỳ chọn `CompensateLoad` tích luỹ sai số vận tốc để kéo tải qua dây, vẫn giới hạn lực theo khối lượng. Mặc định tắt cho toàn bộ cơ quan cũ. Có thể đẩy/kéo ngược.
- `COghePulleyDrive`: cáp đàn hồi chỉ chịu kéo, lực bằng độ căng cộng giảm chấn, giới hạn hữu hạn. Đầu vào là ray kéo bộ tời thu dây, đầu ra là khay nâng trên ray; chốt giữ ở cuối. Lệnh kéo ngược nhả chốt, trọng lực hạ nhịp. Không dịch chuyển mô hay viết trạng thái thắng.
- `COghePassengerLift`: chạm bảng gọi → đi lên khay → kiểm tra đủ mô trong vùng khay → motor hữu hạn nâng → phanh tại điểm dừng. Chạm lại hạ; đứng ngoài khi khay trên cao có thể gọi về. Chạm chỗ khác khi đang tiếp cận huỷ việc lên thang. Nút lún là phản hồi hình ảnh của chuyển động thật.
- `VenomSurfacePatch.MotionFrame`: tham chiếu tuỳ chọn tới rigidbody đang chở. Vận tốc bò được tính tương đối với bề mặt này; null giữ hành vi cũ. Khay chở bằng tiếp xúc/bám, không parent hay teleport sinh vật.
- `COgheViewMechanism`: giữ liên kết cửa bằng lực hiện có. B trên Boss cấp quyền thang; vị trí khay ở điểm đến mới mở cửa B.

## Trình bày và authoring

`ApplyGlassPreview(..., spatial: true)` chỉ đổi vỏ và sàn của catalog mới; không biến mặt cơ quan thành kính hoặc lớp trơn thành mặt bám. Mesh frame, bảng số và art có thư mục riêng. Blue A và coral B, kèm nhãn chữ tương ứng, là nhận diện liên kết; lavender chỉ vật liệu trơn; mint chỉ thoát.

Giữ 32 hạt, fixed step 120 Hz, lực hữu hạn và khối lượng cũ. Cáp là một ràng buộc lực cùng LineRenderer, không chuỗi rigidbody. Không thêm ánh sáng realtime cho từng cơ quan.

## Chênh lệch được làm rõ sau minh hoạ

Minh hoạ không phải bản CAD. Ròng rọc nâng **nhịp giữa hai bờ có giá đỡ**, tránh một khay treo cô lập không thể tới sau khi nâng. Ray đầu vào kéo bộ tời thu dây; không giả dây chéo truyền lực ngang trực tiếp. Thang dùng motor dẫn động trên ray, không coi đối trọng trang trí là mô phỏng vật lý. Boss giữ tay B trên bệ giữa như phác thảo, rồi dùng nút B trên khay. Các lỗ cuối đặt trên vách đối diện, cùng cao độ bệ/khay, để thể hiện sinh vật chui khỏi hộp.

## Kiểm chứng

`COgheSpatialScenario` chỉ có trong Editor/build instrumentation. Nó phát lệnh chạm và quan sát trạng thái thật; gameplay không đọc kịch bản. PlayMode kiểm tra đường giải, bảo toàn mô, hồi quy, hủy lệnh, kéo ngược, lên/xuống thang, retry, không tự chạy khi idle và khoảng hở va chạm lúc bắt đầu. Native author replay phát InputSystem touch events để kiểm tra thêm UI/input và ghi frame time. Đánh giá người chơi mới và FPS Android vẫn cần thiết bị/người chơi thật.

## Chỉnh cơ quan tinh tế hơn — 29/09/2026

Theo phản hồi sau khi chơi: bỏ các thanh liên kết trang trí chéo, thay bằng nét mạch điện bám sàn/kính; ray satin xám nhạt, thân cơ quan màu ngà, màu A/B chỉ tập trung ở tay bấm và nhãn/dải nhận diện. Cáp ròng rọc thật vẫn giữ. Dùng `Refine circuits and verify physics` trước `Build Mac test`. [Ảnh và kết quả kiểm tra](../../../Verification/COgheSpatialCircuits/review.html).
