# Màn 6 — lỗ thoát dưới khối đỏ

Sửa theo yêu cầu mới nhất ngày 15/10/2026. [Xem hình](review.html) ·
[Hồ sơ](../../LevelDesign/COghe/CrateReveal6/README.md) · [Kết quả](results.json).

## Bố cục hiện hành

Bỏ toàn bộ công tắc, nút, nắp, hốc công tắc, cửa và mạch nối. Khối đỏ che một
lỗ thật trên sàn. Dời thùng cản, kéo khối đỏ sang bên rồi chạm lỗ để COghe thoát
xuống. Giữ ID K01, vị trí hiển thị 6, tiến độ cũ và phong cách Glass C.
Đây là ngoại lệ mới thay lỗ trên tường ở màn 6; không thay các màn 1–5 hay Boss 10.

## Kiểm chứng bản sửa

9/9 PlayMode đạt (`Artifacts/Tests/crate-floor-exit.xml`):

- `CrateRevealExposesFloorExitAndResets`: không còn control phụ; khối đỏ che trọn
  lỗ; lỗ hình học trùng outlet hướng xuống; chạm lỗ khi bị che không thắng;
  dời khối bằng screen tap, thoát đủ 32 hạt trong một phần; Retry che lỗ lại.
- `SpatialPlusK01Solve` và `SpatialPlusK01Wander`: giải và phục hồi sau đi lạc.
- Ba bài crate input: chạm cùng đầu để đẩy/kéo; chạm mặt bên; từ chối vật bị chặn.
- `Spatial05Solve`, `Spatial06Solve`, `Spatial10Solve`: hồi quy nội dung cạnh bên
  và Boss 10. Mã 06 là màn hiển thị 7, không nhầm với K01.

Authoring và scene K01 đã tái tạo đồng bộ. Ảnh `start.png` và `revealed.png` lấy
trực tiếp từ test với camera portrait. Không dùng hình minh hoạ để chứng nhận chạy.

Bản Mac (`Builds/COgheProduct/macOS/COghe.app`) chạy với vòng lặp player bình thường,
InputSystem touch: **3 lần chạm, đủ 32/32 hạt, 1 bản thể, không lỗi**. Lượt giải
13,59 giây trên Apple M5: trung bình 59,48 FPS; p95 16,72 ms, p99 17,31 ms;
2/792 frame vượt 33 ms, không frame vượt 50 ms. Bỏ frame chụp ảnh khỏi thống kê.
Đây là replay của tác giả, không phải playtest người mới hoặc đo trên mobile.
Raw evidence: `Artifacts/COgheCrateReveal6/FloorNative/20261015T140733Z/run.json`.

Đối chiếu hash với đầu lượt sửa màn 6: chỉ scene K01 đổi, ánh sáng dùng chung và
các scene còn lại không đổi. Đã giữ công việc có sẵn ngoài phạm vi.
Chưa cài bản sửa này lên iPad/OPPO.
