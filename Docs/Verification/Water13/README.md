# Kiểm chứng bàn 13 — hiệu chỉnh cản nước

Unity 6000.3.19f1, ngày 09/09/2026. `bash Tools/verify.sh` đạt **83/83**, không failure hoặc skipped:

- [EditMode.xml](EditMode.xml): 8/8, kết thúc 02:59:15 UTC.
- [PlayMode.xml](PlayMode.xml): 75/75, từ 02:59:22 đến 02:59:54 UTC.

Có 12 trường hợp test nước; 71 kiểm tra còn lại tiếp tục đạt. Bản nước đầu ở commit `08525a4` được giữ trong [Baseline](Baseline/README.md), không dùng làm bằng chứng của mô hình mới.

## Kết quả định lượng

| Kiểm tra | Kết quả | Đối chiếu |
|---|---:|---|
| Khối lượng thép / added mass khi ngập hết | 110,97676 / 7,05586 g | Inertia tịnh tiến 118,03262 g; inertia quay vẫn là thép |
| Lực nổi | 0,138436 N | Thể tích nước bị bi chiếm |
| Gia tốc chìm ban đầu | −8,05071 m/s² | `(m−mf)g/(m+ma)` |
| Tải nghỉ dưới nước | 0,95025 N | `mg−Fb`; không có xung va chạm lặp trong 5 s đo |
| Coasting trong bể rộng, không gravity | 1 → 0,43194 m/s sau 1 s | Nghiệm cản bậc hai 0,43194 m/s |
| Lăn trên tấm phẳng, 60 Hz | 0,3 → 0,179662 m/s sau 1 s | Nghiệm liên tục 0,179418 m/s |
| Lăn trên tấm phẳng, **120 Hz** | 0,3 → **0,179540 m/s** sau 1 s | Sai số tích phân khoảng 0,068% |
| Lăn trên tấm phẳng, 240 Hz | 0,3 → 0,179479 m/s sau 1 s | Sai số giảm khi chia đôi bước |
| Cản trên sàn thật tại 0,2 m/s | 0,014534 N | Sau tích phân; luật liên tục 0,014609 N; bản cũ 0,006209 N |

Tấm phẳng hiệu chuẩn giữ gravity, lực nổi, contact solver, mass và inertia; bỏ riêng cản lăn do biến dạng khô để đối chiếu phương trình `v' = −b v −k v²`, có mẫu số `m+ma+I/r²`. Động năng giảm ở mọi bước, trượt <0,001 m/s. Kiểm tra hệ số tại Re=70/100/150/5.000 và các mối nối miền; miền tốc độ rất lớn chỉ kiểm tra tính hữu hạn, không xác nhận đúng vật lý ở đó.

Raycast sàn và cạnh cube đều nhận đúng mặt lăn. Tại tâm lỗ, hiệu chỉnh thành bằng không. Tách việc di chuyển bi khỏi gia tốc phần tử nước: lấy mẫu vị trí khác trong nước đứng yên không phát sinh lực áp suất; dòng tịnh tiến đều có gia tốc bằng không. Disable phục hồi mass mà không đổi vận tốc. Reset/unload không để lực hoặc added mass lan sang bàn khô.

Route bắt đầu tại spawn, vòng cube rồi thoát hoàn toàn chỉ bằng điều khiển xoay: 0,950 s tới waypoint đầu và tiếp 1,150 s tới thoát. Đây là thời gian controller kiểm thử, không phải người chơi. Khi ra hết nước, bi vẫn dynamic và trở lại rơi với 9,81 m/s².

Lượt chạy tập trung đầu tiên tìm ra lỗi lấy mẫu VFX giữa pose trước và sau MoveRotation: pháp tuyến dòng tại thành lệch 0,00018 m/s. Đã đồng bộ việc lấy mẫu với pose Rigidbody hiện tại; test giữ nguyên ngưỡng, không nới để vượt kiểm tra.

## Phạm vi kết luận

Các kết quả xác nhận triển khai mô hình, hội tụ tích phân và gameplay; **không phải sai số so với một hộp nước thật**. Những giới hạn về flow/áp suất, trượt, squeeze-film va chạm và vật liệu được ghi tại [thiết kế nước](../../LEVEL13_WATER.md). Hình ảnh/VFX giữ riêng khỏi lực mô phỏng.

## Build

Theo yêu cầu mới, chỉ xuất macOS. APK hiện có vẫn là bản nước đầu (thông tin/hash trong [Baseline](Baseline/README.md)); không có APK hiệu chỉnh.

`Tools/build.sh macOS` thành công, player cập nhật lúc **10:02:15 giờ Việt Nam** (03:02:15 UTC). Đường dẫn: `Builds/macOS/Gravity Box.app`. Đã khởi chạy native, chọn màn 13, reset và kiểm tra HUD/hiển thị; [ảnh F12](../../Images/Level13/Refined/Water13Native.png) lúc 03:03:12 UTC. Không phát hiện exception trong Player.log ở lượt kiểm tra này. Player được để mở tại màn 13 cho người dùng chơi thử; chưa xác nhận cảm giác bằng một lượt chơi tay.
