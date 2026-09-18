# Màn 24 — chọn bánh răng và sửa kẹt khi chuyển cơ quan

18/09/2026. Màn hiển thị 24, content `venom.origin.17`, **Nối bánh răng**.

Người dùng yêu cầu tắt chọn nắp để dễ chọn bánh răng. Mặt `Laboratory ceiling`
được đặt `Selectable=false` trong builder của nội dung này. Tia chạm dùng bộ
lọc sẵn có của `TouchPoint`; không thêm ngoại lệ theo số màn vào runtime.

Giữ collider bật, mặt bám, vật liệu, khối lượng, ray, giới hạn lực, bản đồ đường
đi và camera. Đây là thay đổi chọn đích, không xóa nắp vật lý hoặc bỏ cửa cuối.
Màn khác vẫn có thể chọn nóc như trước.

Builder `RebuildGearSelection` dựng lại canonical 17 rồi đồng bộ slot 24;
không dựng các màn khác. Test mới chiếu điểm tay nắm A/B qua camera thật ở
720×1280 và 720×1612, xác nhận điểm chạm chọn đúng vật; sau đó chơi toàn bộ
bằng lệnh bò/bám/kéo tới khi cả 32 hạt thoát. Không teleport hoặc ép cửa mở
trong test giải trọn. Chưa đo FPS trên điện thoại cho thay đổi này.

Kết quả: **2/2 test đạt** trong
`Artifacts/COgheCampaign30/gear24-targeted.xml`: source 17 giải trọn; slot 24
chọn A/B qua camera ở hai tỷ lệ rồi giải trọn. Điều kiện kiểm tra chốt B dùng
`Latched && Position <= CatchTolerance` (3 mm hiện hành), thay cho ngưỡng
2 mm cũ đòi kéo quá chốt dù cửa đã mở. Không thay lực hoặc thuật toán chốt.

Build Mac thành công (`Artifacts/Venom01/build-macOS.log`,
`ORIGIN BUILD SUCCESS`). Đã mở slot 24 trên app và chạm tay nắm B ở vị trí
cao: dấu nhận lệnh nằm trên giá bánh răng, sinh vật bắt đầu tiếp cận cơ quan;
nắp không nhận lệnh bò. Lượt giải trọn được xác nhận bằng PlayMode ở trên.

## Phản hồi tiếp theo: không qua được màn

Đã tái hiện bằng chạm màn hình thật: nâng A xong, chạm thẳng B thì cả thân
mắc tại góc giữa kính bảo vệ và tấm kính phụ A. Test cũ chèn các điểm đi vòng
xuống sàn trước khi tới B nên bỏ sót lỗi sử dụng này. Bằng chứng lỗi trước sửa:
`Artifacts/COgheCampaign30/gear24-direct-before.xml` (1/1 thất bại ở tiếp cận B).

Sửa cục bộ bố cục source 17 / slot 24: bỏ hai mặt `A/B fixed grip cheek` vuông
góc, giữ nguyên kính bảo vệ liên tục tới sàn, tạo mặt bám thông suốt giữa hai
giá bánh răng. Giữ tay nắm, bốn tâm bánh, hành trình ray, khối lượng, lực, chốt,
truyền động cửa và lỗ thật. Không sửa thuật toán chung hoặc ép đường giải.
Hai collider/mặt bám ít hơn; chưa đo FPS trên điện thoại cho thay đổi này.

`Artifacts/COgheCampaign30/gear24-solvability.xml`: **4/4 đạt**:

- Chạm trực tiếp A → nâng → B → hạ → lỗ, thoát đủ 32 hạt. Không điểm đi vòng.
- Làm B trước A, chờ tự buông sau 3 giây ở mỗi tay nắm rồi chạm lỗ: hoàn tất.
- Canonical 17 giải trọn bằng lệnh chọn cơ quan, không còn điểm đi vòng trong test.
- Slot 24 chọn A/B qua camera ở 720×1280 và 720×1612; nắp không nhận chọn,
  vẫn có collider và độ bám; sau đó giải trọn.

Các test mô phỏng lực và va chạm thật; không dịch thân, gán chốt hoặc ép thắng.

Build Mac sau sửa thành công (`Artifacts/Venom01/build-macOS.log`,
`ORIGIN BUILD SUCCESS`). Đã chơi bằng chuột trong app: chọn A, chỉ lên,
chọn trực tiếp B, đưa B về hàng ăn khớp; cửa nâng, buông cơ quan rồi chạm lỗ.
Sinh vật thoát, chạy hết chuyển màn và tự sang **25 · Đổi chiều!**.
Trong lượt thao tác qua công cụ có dùng nút Tạm dừng/Tiếp tục để quan sát và
nắm lại tay cầm khi quá thời gian 3 giây; không thay luật hoặc vị trí vật lý.
Đã mở lại màn 24 ở trạng thái bắt đầu để người dùng test.
