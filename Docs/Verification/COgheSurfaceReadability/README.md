# Spatial 50 — surface and tube readability · 30/09/2026

Các số dưới đây là **vị trí hiển thị trong catalog 50 màn**, không phải số scene
lịch sử. Yêu cầu: nhiễu bề mặt khi đổi góc nhìn trên OPPO ở 12, 24, 26, 27;
miệng và thân ống khó nhìn.

## Nguyên nhân và phạm vi sửa

Đã tái hiện trên OPPO CPH2591: các dải ngà/tím nhấp nháy ở mép khối, khay và
ván. Mỗi khối ghép từ sáu tấm va chạm dày 8 mm; mặt đóng mép của tấm này trùng
mặt trước của tấm kia. Đây là hai bề mặt tranh cùng độ sâu, không phải lỗi ảnh texture.

Pass chỉ thay `MeshFilter` bằng lưới hiển thị bỏ những tam giác mép được một mặt
cùng thân che kín. Giữ mặt trước/sau, toàn bộ MeshCollider/BoxCollider, transform,
Rigidbody, joint, bề mặt bò và luật chơi. Không tăng độ dày, đổi lực hay sửa camera.
Mesh giống nhau dùng chung asset; pass được gọi từ builder dùng chung để tránh tái
phát khi dựng màn mới. Các catalog V2/Origin không được dựng lại.

- Rà 50/50 scene; sửa 770 bề mặt, bỏ 6.160 tam giác thừa trong 44 màn.
- Vị trí có sửa: **4–21, 24–35, 37–50**. Màn 36 chỉ cập nhật ống.
- Ống: **14, 15, 16, 19, 20, 32, 36, 38, 40, 42, 43**; 13 mạng, 30 miệng vào.
- Thân ống cyan trong, hai gân teal mảnh, cổ ngà và gioăng teal ở miệng thực.
  Không che lòng ống, không thêm collider/điểm bấm hoặc dùng màu mint của lỗ thoát.
  Art tĩnh gộp thành hai renderer mỗi mạng; không thêm cập nhật mỗi frame.

## Tái tạo và bằng chứng

Unity: **Gravity Box → COghe → Spatial pilot → Repair surface rendering and clarify tubes**.
Không cần Generate lại gameplay. `COgheSpatialReadabilityBuilder` ghi
`Artifacts/COgheSurfaceReadability/audit.csv`, snapshot physics trước/sau và log.
So sánh physics cùng definition của 50/50 scene đã đạt, không có khác biệt.

Lượt PlayMode đầu dùng `-nographics` gặp crash native Unity tại `Camera.Render`
của hàm chụp ảnh kiểm thử; không phải crash bản OPPO. Đã chuyển sang chạy có Metal.

- **183 passed / 0 failed / 1 skipped**: Spatial, V2 view controls và Origin,
  gồm đủ 50 lời giải Spatial, retry, wander/recovery, tách–tụ, chui ống, zoom,
  Collection. Test bỏ qua là `TracePlusLevel`, cần biến môi trường chẩn đoán;
  không phải một màn chưa qua. [XML](playmode-results.xml).
- **50/50 lời giải trong native Mac, 0 lỗi player** qua replay InputSystem.
  Đây là replay tăng tốc 120 Hz để kiểm tra chức năng/hình ảnh, **không phải số
  đo FPS thời gian thực**. [Kết quả](mac-native-run.json). Đã xem khung thắng,
  cơ thể thoát đủ và đồ vật che phía trước được ẩn.
- **OPPO CPH2591:** APK release đã cài và mở; SHA256 bản cài khớp APK vừa build.
  Rà 50 màn ở hai góc nhìn, thêm góc thứ ba cho 12/24/26/27; 104 ảnh sau sửa.
  Không còn các dải ngà/tím tranh độ sâu ở các mép trong các góc kiểm tra.
  Không có Unity exception/fatal trong log lượt rà. Đây là kiểm tra hình ảnh
  trên điện thoại, không thay cho playtest qua hết 50 màn trên điện thoại hay
  benchmark FPS kéo dài. [Biên bản thiết bị](oppo-verification.json).
- Đối chiếu YAML: chỉ đổi 770 MeshFilter, 27 MeshRenderer, 5 LineRenderer và
  thêm nhánh art vào 13 Transform; không đổi component gameplay hoặc ánh sáng.
  [Đối chiếu](scene-diff-audit.json). Snapshot physics trước/sau có cùng SHA256:
  `a4dbd28d2bc43fa677bdd783763ccfb67ab31e747c6badbcbc71b69bccc4961b`.

Ảnh so sánh: [mở gallery trước/sau trên OPPO](review.html). Ảnh tổng quan sau xoay:
[01–10](overview-01.jpg), [11–20](overview-11.jpg), [21–30](overview-21.jpg),
[31–40](overview-31.jpg), [41–50](overview-41.jpg).

## Bản test

- macOS: `Builds/SpatialLab/macOS/COghe.app`.
- Android: `Builds/SpatialLab/Android/COghe-Spatial.apk`, **50 scene**, ARM64 IL2CPP
  release, không bật benchmark tự chạy. Dung lượng 39.782.019 byte.
- SHA256: `a9f05e9388e8b10e9ac3289ed2c3849f83ce80d90826c878e0b76081bd4c4958`.
  [Catalog và bản build](build-record.json).
- Bản cuối dùng alpha thân ống 0.27; 120 lưới mặt dùng chung và 26 lưới art ống
  đã gộp, ba material dùng chung. Không thêm logic chạy mỗi frame.
