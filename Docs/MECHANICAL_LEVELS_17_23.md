# Bảy thí nghiệm cơ khí 17–23

Mục tiêu của loạt này là kiểm chứng sáu họ cơ chế và một bài phối hợp, trước khi tạo biến thể hoặc cân bằng độ khó cho sản phẩm. Trọng lực thế giới 9,81 m/s², bi thép Ø30 mm khoảng 111 g, contact và input xoay giữ cùng profile với 16 màn trước. Toàn bộ bi bắt buộc phải ra khỏi lỗ cuối. Lỗ chuyển nội bộ không hút bi.

| Màn | Cơ chế muốn kiểm chứng | Khi chơi cần quan sát |
| --- | --- | --- |
| 17 — Build your own bridge | Trọng lực gập cầu vào ngàm; bi trú rồi đi qua | Cầu có tự rơi, va ngàm và giữ ổn định; đường lăn có bị gờ làm mắc |
| 18 — Two balls, one balance | Hai bi bằng nhau, cánh tay đòn khác nhau, chốt giữ kết quả nâng | A có tạo tải rõ để nâng B; cả hai có được giải phóng |
| 19 — A box within a box | Lồng có đối trọng quay độc lập với vỏ, chặn góc để nghiêng sàn | Quán tính và độ trễ lồng; miệng lồng đủ dễ nhìn để căn hướng |
| 20 — Between the beats | Con lắc thật che một khe, kết hợp hai trục nghiêng để qua | Nhịp dao động, va chạm, thời điểm thả bi; giữ con lắc mở bằng tư thế là lời giải hợp lệ |
| 21 — Catch me | Máng lấy đà, đoạn bay tự do, máng hứng lệch hướng | Gia tốc, quỹ đạo bay và cảm giác xoay hộp đón; khả năng quay lại sau hụt |
| 22 — The box remembers | Bi đẩy rack, cam tiến từng nấc, hai cóc giữ/nhả khi rack trở về | Một stroke có đổi đúng một trạng thái; spring hồi; đọc được đường đã mở |
| 23 — Mechanical heart | Hai bi phối hợp đòn bẩy, chốt, lồng treo và đoạn đón trong cầu kính | Giữ A an toàn trong lúc B gài chốt; quan sát cả hai và đưa hết bi ra |

## Cách mô phỏng

Các phần chuyển động là body động trong world space, nối bằng joint với hộp hoặc với body khác. Hộp kinematic nhận chuyển động từ input như các màn cũ. Không ghi vị trí/vận tốc của bi trong runtime để giải cơ cấu, không tắt collider khi cửa được coi là mở, không thêm lực phóng hoặc hút trong các chặng bay.

Ổ trục được mô hình hóa bằng joint lý tưởng và mômen cản nhớt nhỏ. Ngàm ở 17/18 khóa joint tại góc tiếp xúc đã đạt; đây là xấp xỉ chốt cứng, không mô phỏng biến dạng răng hoặc ma sát từng chi tiết nhỏ. Màn 22 dùng giới hạn một chiều cho cóc giữ và escapement chống vượt nhiều nấc; tiếp xúc ball–rack–finger–cam cung cấp chuyển động. Những giới hạn này hấp thụ chuyển động bị cấm, không ra lệnh đẩy bánh cam về góc đích.

Màn 19 dùng một trục lồng treo. Nhiều lồng/trục khác nhau là biến thể tương lai, chưa có trong 19. Màn 20 cho phép giữ cửa ở góc mở bằng trọng lực; đó là kết quả hợp lệ của mô hình. Màn 21 có thể có các lời giải dùng đổi mặt đỡ hoặc quán tính ngoài đường dự kiến. Chỉ test cảm giác với chuột/touch mới đánh giá được mức khó và tính dễ đọc; một route tự động thành công không chứng minh màn đã cân bằng cho người chơi.

## Sinh nội dung và kiểm chứng

`PhysicsLabBuilder.Generate` tạo 23 prefab và catalog, rồi `ContentValidator` kiểm tra spawn, lỗ, hình học và liên kết. Mỗi họ có builder riêng, dùng chung `MechanicalAuthoring`; root scale luôn 1, kích thước author bằng mét.

PlayMode kiểm tra riêng cơ cấu và route. Fixture cơ cấu được phép đặt điều kiện ban đầu để đo tải, chặn và clearance; fixture route bắt đầu từ spawn và chỉ gửi rotation intent. Các fixture render chỉ là ảnh hình học ban đầu, không được coi là replay lời giải.

Bản test cho người chơi là `Builds/macOS/Gravity Box.app`. Chọn **SHAPES**, kéo xuống màn 17–23; **R** reset, **P/Esc** pause. Mỗi lần reset trả cả bi, lò xo, cam và chốt về trạng thái gốc. APK không được build lại trong đợt này.
