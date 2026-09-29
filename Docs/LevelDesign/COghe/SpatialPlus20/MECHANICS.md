# Cơ quan cho 20 màn Spatial Plus

30/09/2026 · Đề xuất. Chỉ ghi phần **mới hoặc mở rộng** so với [hợp đồng Spatial 11–30](../SpatialNext20/MECHANICS.md). “Có runtime” = component đã có trong code hiện tại; “mới” = phải prototype trước khi dựng màn.

| Cơ quan | Dùng ở | Runtime | Hợp đồng / ghi chú |
| --- | --- | --- | --- |
| Khối trên khối (ray gắn trên vật đang chạy) | 21, 29, Boss 30 | Có (khung cầu màn 29: `COgheRailSlider.Frame` là vật động, khớp nối vào thân vật mang) | Khối trên đi theo khối dưới; mặt ghế ngồi (docked deck) của khối trên là con của khối dưới và dùng box collider. Tay nắm chỉ nhận lệnh khi vật mang đứng yên. Ba tầng lồng nhau (Boss 30) cần kiểm độ cứng khớp và chỗ đứng khi đẩy. |
| Khối nặng cần cả thân | Boss 30 | Có (`CompensateLoad`, lực tay 7 × khối lượng) | Ma sát/khối lượng chọn để 50% kẹt, 100% kéo được — như màn 25/29. |
| Then AND có chốt | Boss 30 | Có (`COgheLoadLatch` Any=false, Retain=true) | Then chỉ rút khi nút có tải **và** cần đã kéo, rồi pawl giữ; cả hai được rời. |
| Nút ngưỡng khối lượng | 33 | Có (`COgheTissueSensor` threshold) | Nút “50%” cần ≥ 12 hạt; phần 25% (8 hạt) không đủ. In rõ % trên nút; phản hồi “chưa đủ nặng”. |
| Ván bập bênh tự do | 26 | **Mới** (chỉ bản lề + khối lượng, không động cơ) | Đối trọng giữ đầu gần chạm sàn; thân đi qua trục thì ván nghiêng thả xuống phòng bên; không tải thì về vị trí nghỉ. Cần kiểm tìm đường trên mặt đang nghiêng và ngưỡng lật cho 25/50/100%. |
| Bộ bánh răng nằm ngang | 31, 41, 44, 46–50 | Có (`COgheGearTrain`: chuỗi bánh, kiểm khớp theo vòng lăn, nút tải làm động cơ, đầu ra thanh răng) | Bánh cố định màu hổ phách, bánh trên xe màu ngà. Thiếu một bánh thì chỉ phần trước khe quay. Lưu ý code: `GearTrain` có `Rack` đang tính là cơ quan khoá lối thoát (`ControlsExit`) — cần cờ tắt cho các đầu ra không phải cửa thoát. |
| Xe bánh răng | 41, 44, 49, 50 | Có (xe ray mang bánh, như catalog cũ màn 37/38) | Kéo tay A/B/C đưa bánh vào khe; chốt ở cuối. |
| Trục nối nhiều tầng | 44, 49, 50 | Có (một chuỗi `Wheels` qua các tầng) | Trục đứng là hình vẽ; chuỗi bánh chứa bánh của các tầng theo thứ tự. Tầng trên chỉ quay khi mọi tầng dưới đã khớp. |
| Thang bánh răng chỉ chạy khi có tải | 46, 50 | Có (`LatchOutput=false`, `ReturnWhenDisconnected`) | Rời nút thì bệ hạ về bằng lực hữu hạn; cần cảm biến chống kẹp khi có mô dưới bệ. |
| Máy đẩy máy | 48, 50 | Có (thanh răng máy 1 → `ViewLink` → xe bánh máy 2) | Máy 1 chỉ di chuyển bánh của máy 2, không nâng gì. |
| Bàn xoay nấc 90° | 47 | **Mới** | Đầu ra quay từng nấc 90°, có chốt ở mỗi nấc; chỉ quay khi mặt bàn không có mô; graph cập nhật sau mỗi nấc. |
| Đầu ra cần hai động cơ | Boss 50 | **Mới** (mở rộng nhỏ: `GearTrain` nhận nhiều `InputClutch`, cần tất cả có tải) | Cửa thoát nặng: chỉ quay khi P1 **và** P2 cùng có tải; bốn phần phải chia vai. |

Mọi cơ quan: lực hữu hạn, không dịch chuyển mô, không đọc số màn, Retry trả về đầu, cửa/đầu ra đã chốt không mất khi rời nút.
