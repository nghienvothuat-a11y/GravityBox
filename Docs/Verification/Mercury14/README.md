# Kiểm chứng bàn 14 — thủy ngân

Unity 6000.3.19f1, ngày 09/09/2026. `bash Tools/verify.sh` đạt **89/89**, không failure/skipped:

- [EditMode.xml](EditMode.xml): 8/8, kết thúc 03:11:53 UTC.
- [PlayMode.xml](PlayMode.xml): 81/81, từ 03:12:00 tới 03:12:34 UTC.

Sáu test thủy ngân mới và 83 kiểm tra trước đó đều đạt. Scene integration đi qua đủ 14 màn; selector dùng click thật trên thẻ cuối và kiểm tra hint vừa khung. Bằng chứng riêng bản nước hiệu chỉnh trước khi thêm thủy ngân ở [Water13](../Water13/README.md).

## Kết quả

| Phép thử | Kết quả |
|---|---|
| Lực nổi khi ngập hết | 1,878635 N hướng lên |
| Added mass | 0,09575103 kg; khối lượng thép và inertia quay không đổi |
| Gia tốc nổi ban đầu | +3,821224 m/s² |
| Vị trí nghỉ ở nắp | y=+0,027000 m, đúng tiếp xúc bi R=15 mm với nắp y=42 mm |
| Tải lên nắp | 0,789953 N; ổn định trong 5 s đo, không phát sinh va chạm lặp |
| Coasting không gravity/không biên | 0,200 → 0,065833 m/s sau 1 s; trùng nghiệm cản bậc hai trong ngưỡng 0,0001 m/s |
| Nổi một phần tại lỗ hướng lên | Cân bằng tại phần ngập khoảng 0,5795; không thắng |
| Ra hết lỗ | Thắng qua swept-sphere clearance; bi còn dynamic và mass trở về thép |
| Reset/đổi môi trường | Xóa dòng/added mass/wake; trở về bàn 13 có gia tốc chìm −8,050707 m/s² |

Đường giải kiểm thử bắt đầu tại spawn, không đặt lại position/velocity: xoay hộp 180° trong 3,5 s; đi tới waypoint vòng cube trong 1,167 s, tới miệng lỗ trong 1,617 s, rồi nghiêng sang bên để rời vành trong 1,583 s. Chỉ một sự kiện thắng. Thời gian là controller kiểm thử, không dùng đánh giá độ khó chơi tay.

Lượt tập trung đầu tìm thấy policy chỉ giữ lỗ hướng lên sẽ để bi nổi lưng chừng. Không đổi lực/độ nổi/điều kiện thắng để vượt test: thêm động tác nghiêng hộp thật trong policy. Test cân bằng một phần vẫn giữ để bảo vệ hành vi này.

Ba ảnh render từ mô phỏng liên tục đã được xem, shader không có lỗi. [Ảnh/metadata](../../Images/Level14/README.md). Đây là hình bạc nhìn xuyên để quan sát, không phải tính trong suốt vật lý của thủy ngân.

## Giới hạn

Kiểm tra xác nhận lực theo mô hình và khả năng chơi, chưa phải hiệu chuẩn với thủy ngân thật. Không có CFD, sức căng bề mặt, meniscus hoặc squeeze-film va chạm; root vẫn kinematic và không mô phỏng sức tay xoay khối chất lỏng khoảng 113 kg. [Thông số/nguồn/giới hạn](../../LEVEL14_MERCURY.md).

Chỉ xuất macOS theo yêu cầu hiện tại; không build APK/iOS mới.

`Tools/build.sh macOS` thành công, player cập nhật **10:13:50 giờ Việt Nam** (03:13:50 UTC), tại `Builds/macOS/Gravity Box.app`. Đã khởi chạy, mở selector, chọn màn 14 và quan sát bi nổi, HUD 111 g và nhãn nhìn xuyên. Player.log không có exception/error trong lượt khởi chạy đã đọc. Sau đó người dùng bắt đầu xoay hộp; tác vụ dừng điều khiển UI để giữ nguyên lượt chơi. Kiểm tra native này không phải lượt giải trọn màn. Không gán ảnh native cũ của bàn 13 cho bằng chứng bàn 14.
