# Cơ quan cho 20 màn Spatial Plus

30/09/2026 · **Đã dựng.** Chỉ ghi phần **mới hoặc mở rộng** so với [hợp đồng Spatial 11–30](../SpatialNext20/MECHANICS.md). Cột Runtime ghi component đang dùng.

| Cơ quan | Dùng ở | Runtime | Hợp đồng / ghi chú |
| --- | --- | --- | --- |
| Khối trên khối (ray gắn trên vật đang chạy) | 21, 29, Boss 30 | Có (khung cầu màn 29: `COgheRailSlider.Frame` là vật động, khớp nối vào thân vật mang) | Khối trên đi theo khối dưới; mặt ghế ngồi (docked deck) của khối trên là con của khối dưới và dùng box collider. Tay nắm chỉ nhận lệnh khi vật mang đứng yên. Ba tầng lồng nhau (Boss 30) cần kiểm độ cứng khớp và chỗ đứng khi đẩy. |
| Khối nặng cần cả thân | Boss 30 | Có (`CompensateLoad`, lực tay 7 × khối lượng) | Ma sát/khối lượng chọn để 50% kẹt, 100% kéo được — như màn 25/29. |
| Then AND có chốt | Boss 30 | Có (`COgheLoadLatch` Any=false, Retain=true) | Then chỉ rút khi nút có tải **và** cần đã kéo, rồi pawl giữ; cả hai được rời. |
| Nút ngưỡng khối lượng | 33 | Có (`COgheTissueSensor` threshold) | Nút “50%” cần ≥ 12 hạt; phần 25% (8 hạt) không đủ. In rõ % trên nút; phản hồi “chưa đủ nặng”. |
| Ván bập bênh tự do | 26 | Đã dựng (prop có `HingeJoint` giới hạn ±27°, trọng tâm lệch về chân; không động cơ) | Thân đi qua trục thì ván nghiêng thả xuống phòng bên; không tải thì về vị trí nghỉ. Bậc chêm trơn dưới hai đầu hạ: không còn khe cao quá 3 cm dưới ván (sinh vật từng kẹt trong khe nêm). Lên từ chân ván. |
| Bộ bánh răng nằm ngang | 31, 41, 44, 46–50 | Có (`COgheGearTrain`); thêm cờ `GatesExit` (đầu ra không phải cửa thoát thì không khoá lỗ thoát) | Bánh cố định màu hổ phách, bánh trên xe màu ngà. Thiếu một bánh thì chỉ phần trước khe quay. Bàn răng và xe bánh răng trơn: đường đi vòng quanh máy. Lực kẹt thanh răng = MotorTorque/r ở tốc độ đặt; nâng người cần MotorSpeed cao hơn (46: 3; 50: 2,5). |
| Xe bánh răng | 41, 44, 49, 50 | Có (xe ray mang bánh, như catalog cũ màn 37/38) | Kéo tay A/B/C đưa bánh vào khe; chốt ở cuối. |
| Trục nối nhiều tầng | 44, 49, 50 | Đã dựng (`COgheGearTrain.ShaftLinks`: hai bánh đồng trục quay cùng tốc độ) | Trục đứng là hình vẽ; chuỗi bánh chứa bánh của các tầng theo thứ tự. Tầng trên chỉ quay khi mọi tầng dưới đã khớp. |
| Thang bánh răng chỉ chạy khi có tải | 46, 50 | Có (`LatchOutput=false`, `ReturnWhenDisconnected`) | Rời nút thì bệ hạ về bằng lực hữu hạn; cần cảm biến chống kẹp khi có mô dưới bệ. |
| Máy đẩy máy | 48, 50 | Có (thanh răng máy 1 → `ViewLink` → xe bánh máy 2) | Máy 1 chỉ di chuyển bánh của máy 2, không nâng gì. |
| Bàn xoay nấc 90° | 47 | Đã dựng (`COgheTurntable`, mới) | Đầu ra quay từng nấc 90° khi máy chạy, chốt ở mỗi nấc; chỉ quay khi vùng mặt bàn không có mô; graph cập nhật sau mỗi nấc. |
| Đầu ra cần hai động cơ | Boss 50 | Đã dựng (`COgheGearTrain.ExtraClutches`: mọi nút phải có tải) | Cửa chớp lỗ thoát chỉ quay khi P1 **và** P2 cùng có tải, rồi chốt; ba phần chia vai. |
| Khối nổi trên khối mang | 21, 29, 30 | Đã dựng (ray giữ độ cao; khối trên cách mặt khối mang 2 mm) | Nằm sát mặt, góc khối trên vướng mép tấm cuối của khối mang (mặt tĩnh là tấm dày 8 mm) và dừng thiếu 8 mm. |
| Cửa, nắp, chốt trơn | 15, 18, 33, 44, 49, 50 | Đã dựng (`PlusGate` = ViewGate trơn mọi mặt) | Cửa ngà là cái thang: sinh vật leo cửa qua vách (test đi lang thang). |

Mọi cơ quan: lực hữu hạn, không dịch chuyển mô, không đọc số màn, Retry trả về đầu, cửa/đầu ra đã chốt không mất khi rời nút.
