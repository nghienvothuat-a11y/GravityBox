# C đã chọn — nhãn mẫu vật trên hộp

29/09/2026 · NewGraphic · Unity 6000.3.19f1.

[Xem ảnh](review.html). Mac: `Builds/GlassLab/macOS/COghe.app`.

- Người dùng chọn **C** cho hướng hộp kính. Màn mẫu 01 dùng C mặc định, ẩn các nút so sánh. Chưa chuyển hình học/art của các màn khác sang hộp kính trong lượt này.
- **30/30 màn V2** có một bảng nhựa màu ngà áp trực tiếp ngoài mặt kính, dưới góc trên trái; số xanh xám 01–30 theo `Definition.Order` và gạch nhỏ. Theo concept Day Lab gốc; thay thế mẫu ngàm trên khung đã bị từ chối. Không billboard hoặc HUD giả.
- Bảng dày 4 mm; mặt sau chạm mặt ngoài pane (8 mm ở hộp kính 01, 34 mm ở các vỏ V2 còn lại), không có khoảng hở hoặc ngàm.
- Không collider, Rigidbody, logic tương tác hoặc Update mới. Ba renderer mỗi màn (nhựa, gạch in, chữ), dùng chung vật liệu/font atlas. Victory tự ẩn cả nhãn. Retry tải lại đúng số.
- Builder xoá nhãn cũ trước khi dựng, dùng thư mục mesh riêng theo màn; không làm tăng bản sao khi chạy lại. Art generator V2 và glass pilot đều gọi builder nhãn.

## Kiểm tra

- **102/102 PlayMode pass**, gồm suite V2 01–10 và expansion 11–30: [XML](playmode.xml). Giải màn, retry, input/camera, hợp thể/cooperation và các tình huống bất lợi có trong suite. Sau suite chỉ chỉnh geometry/typography của bảng theo phản hồi về concept và thêm mục menu build; không sửa code runtime hay gameplay.
- Builder xác minh đúng số ở mọi cảnh và physics snapshot của cả 30 cảnh giống trước/sau, gồm lần chạy lại builder.
- Native Mac level 01: giải bằng touch replay; ảnh native cuối và report lưu cùng báo cáo này. Ảnh native 30 dùng replay tăng tốc để kiểm tra chữ hai chữ số; không dùng làm chứng cứ FPS.
- Không đo performance OPPO hoặc benchmark mới trong lượt này. Số FPS của thử nghiệm C trước không đại diện cho bản mới trên điện thoại.
- Giữ nguyên các thay đổi đã có trước task; không commit/push.

## Dựng lại

Unity → **Gravity Box → COghe → Glass preview → Build approved C and numbered levels Mac**.

Chỉ gắn lại nhãn và chốt C: **Apply approved C and specimen numbers**. Code: `Assets/_Game/Editor/COgheSpecimenPlateBuilder.cs`. Màu C tuân theo `Docs/ArtDirection/COghe/STYLE_RULES.md`.
