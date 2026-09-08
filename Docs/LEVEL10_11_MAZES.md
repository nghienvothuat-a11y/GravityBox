# Hai mê cung vật lý: bàn 10–11

Hai bàn mở rộng catalog lên 11 và giữ nguyên bi thép đường kính 30 mm, khối lượng khoảng 111 g, gravity thế giới 9,81 m/s² và clock 120 Hz. Người chơi chỉ nghiêng hộp. Các đường mô tả dưới đây là một cách giải có thể kiểm chứng; cách đi khác nhờ quán tính, cửa mở một phần hoặc xoay tự do vẫn hợp lệ nếu bi thực sự đi qua hình học.

## 10 — Opposite ways

Hộp rộng 0,72 × 0,56 m, sâu 0,09 m. Hai vách chia khoang trái, giữa và phải; các vách ngang xen kẽ tạo hành lang phải đổi hướng. Bi xuất phát gần góc trái dưới và lỗ thoát nằm gần góc phải trên.

| Cơ cấu | Vị trí tâm khi đóng (local X, Z) | Chiều mở trong hộp | Hành trình |
| --- | --- | --- | --- |
| Cửa A | (−0,120; +0,030) m | +Z | 0,120 m |
| Cửa B | (+0,120; −0,080) m | −Z | 0,120 m |

Mỗi thanh chặn là PhysicalProp 0,18 kg, kích thước 0,026 × 0,080 × 0,076 m. ConfigurableJoint giữ một trục trượt, khóa các trục còn lại, không motor/drive/spring/projection. Cùng gia tốc thế giới tác động lên cả bi và hai thanh chặn. Ray là ràng buộc lý tưởng không ma sát dọc trục; vật liệu chỉ gây ma sát khi có contact thật với housing/end stop. Ray nhìn thấy gắn phía nắp; sàn lăn qua cửa phẳng.

Một cách đi là theo hành lang bên trái tới hốc A, nghiêng về +Z để thành hốc giữ bi trong khi A trượt mở. Chuyển sang nghiêng ngang để qua khoang giữa, tới hốc B rồi nghiêng về −Z để mở B. Cửa A có thể trượt đóng lại trong lúc đó. Qua B, dẫn bi theo các đoạn hành lang bên phải tới lỗ tròn. Các vạch hốc chỉ là dấu nhìn; không có trigger hoặc tín hiệu điều khiển cửa.

Test kiểm tra hai chiều gravity tạo hai trạng thái cửa đối nhau, cửa đóng ngăn toàn bộ cầu ở mọi độ sâu hợp lệ, reset cả ba body và thu hồi đủ force targets. Fixture đường giải bắt đầu ở spawn chuẩn, chỉ gửi rotation intent qua controller để qua cả hai cửa và thoát thật.

## 11 — Three dimensions

Hộp sâu 0,27 m có ba mê cung xếp theo Y. Mỗi mê cung có lưới 4 × 4, đường đi chính đổi hướng và các nhánh cụt. Sàn dày 0,006 m; vách cao 0,084 m chạm khoảng dưới mặt sàn/nắp phía trên, nên không thể đi xuyên tầng ở vùng sàn đặc.

| Tầng | Tâm sàn local Y | Điểm kết thúc tầng (local X, Z) | Bán kính lỗ |
| --- | --- | --- | --- |
| Trên | +0,045 m | (+0,220; +0,170) m | 0,038 m, xuống tầng giữa |
| Giữa | −0,045 m | (−0,220; −0,170) m | 0,038 m, xuống tầng dưới |
| Dưới | −0,135 m | (+0,230; +0,180) m | 0,023 m, ra khỏi hộp |

Bi bắt đầu trên tầng trên. Đưa bi qua mê cung tới lỗ thứ nhất để rơi xuống tầng giữa, tìm lỗ ở phía đối diện để xuống tầng dưới rồi đưa bi tới lỗ cuối. Các lỗ là khoảng rỗng thật trong mesh sàn và MeshCollider; viền mảnh không có collider hoặc gờ nâng. Không có trigger chuyển tầng, teleport, lực hút hoặc scene thay thế. Chuyển tầng chưa phải chiến thắng; toàn bộ cầu phải vượt mặt ngoài của lỗ cuối mới tính thoát.

HUD hiển thị tầng dựa trên chiều cao thật của bi trong hệ tọa độ hộp. Các tầng khác được làm mờ để đọc đường hiện tại; chạm dòng trạng thái để bật/tắt xem tổng thể. Chế độ này chỉ đổi vật liệu, mọi sàn/vách vẫn va chạm và Rigidbody vẫn dynamic. Khi bi đang rơi qua một sàn, chỉ số tầng có thể đổi trước khi toàn bộ cầu rời sàn; đó là chỉ báo hiển thị, không phải thao tác chuyển bi.

Test cô lập kiểm tra mỗi sàn đỡ bi ngoài lỗ và cho cả cầu rơi qua đúng lỗ. Fixture từ spawn đi qua đường của cả ba tầng tới exit cuối bằng controller hộp, theo dõi vị trí liên tục mỗi bước để phát hiện teleport. Test riêng đổi tầng/overview giữ nguyên collider, pose và vận tốc. Các fixture cô lập được đặt điều kiện đầu để đo hợp đồng; route từ spawn không đặt lại bi giữa hành trình.

## Phạm vi nghiệm thu

Hai mê cung bổ sung thử nghiệm điều khiển và đọc không gian; chưa coi số vách hoặc số tầng là bằng chứng độ khó phù hợp. Cần chơi bằng chuột/touch để đánh giá nhìn bi, đọc hốc/cửa, phân biệt tầng và lượng thao tác. Kết quả test, build và native QA được ghi riêng trong [bằng chứng hiện tại](Verification/README.md) và [nhật ký](DEVELOPMENT_LOG.md).
