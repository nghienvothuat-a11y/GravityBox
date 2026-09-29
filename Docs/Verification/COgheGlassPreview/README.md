# COghe — màn 01 trở lại hộp kính

Ngày 29/09/2026, theo yêu cầu dừng hướng Blender và dựng lại riêng màn 1 để xem hình ảnh. [Xem ảnh](review.html).

- V2 01 trở lại kính trong, khung nhôm mảnh, góc nối sứ, sàn xanh nhạt, viền mint phẳng quanh lỗ thật. Giữ cơ thể/animation hiện có.
- Chỉ thay phần nhìn của màn 01; các màn khác dùng profile trước Blender mặc định. Tắt nút A/B thử nghiệm trong luồng chơi. Source Blender và các kết quả cũ được giữ lại, không xóa.
- Giữ kích thước buồng V2 .8×.6×.34m, ID/save, spawn, aperture và chạm/orbit/pinch. Không khôi phục luật xoay vật lý của Origin.
- Kính dùng shader Day Lab sẵn có; khung được gộp theo material. Không thêm collider, Rigidbody, đèn hoặc hậu kỳ.

## Dựng và mở

Unity: **Gravity Box → COghe → Glass preview → Rebuild level 01**. Bộ dựng art V2 cũng gọi adapter này cho màn 01. Builder `Assets/_Game/Editor/COgheGlassPreviewBuilder.cs`.

Bản Mac: `Builds/GlassPreview/macOS/COghe.app`. Đã mở trực tiếp để xem. Không build/cài Android trong yêu cầu này.

## Kiểm tra

- Snapshot collider, Rigidbody, joint, surface, rail và input của cả 30 scene trước/sau giống nhau: `0f57d8866c1e3cd37b62dcf06fdd61cc4c5cb821e0c758e1fb4bfa533a495356`. Raw trong `Artifacts/COgheGlassPreview/physics-*.txt`.
- Native Mac ở 720×1280: đường giải màn 01 qua bằng touch replay trong vòng lặp bình thường; đủ32 hạt thoát trong một thể, không lỗi log. [Report](native-replay.json). Không dùng lượt này để chứng nhận FPS điện thoại.
- Đã xem ảnh render Unity, native Mac ban đầu, cảnh ăn mừng và cửa sổ chơi bình thường. Hình ảnh là bản review thực tế, chưa coi là user đã duyệt.
- Lượt PlayMode đầu:22/24. Hai test cũ còn giả định vách đặc ở màn 01 đã được chuyển sang màn02 (nơi thiết kế đó vẫn dùng); thêm hồi quy kính màn01 giữ renderer/collider khi orbit, không sinh collider trang trí. Kết quả lượt cuối ghi bên dưới.

Lượt cuối: **25/25 PlayMode passed**, gồm đường giải 10 màn V2, input/camera/retry, hai bài cutaway và hồi quy kính mới. [XML](playmode-final.xml). Mac build thành công; native màn01 passed.
