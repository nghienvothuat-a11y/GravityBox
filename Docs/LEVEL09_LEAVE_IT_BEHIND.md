# 09 — Leave it behind

Đây là puzzle đầu tiên trên mô hình bi thép hiện tại. Ý chính: cho hai vật cùng chịu trọng lực, nhưng giữ một vật lại bằng hình học để vật kia dời chỗ.

## Bố trí

Hộp chữ nhật có mặt bằng 0,56 × 0,38 m và chiều sâu 0,09 m. Một vách chia khoang trái/phải, chỉ chừa cửa rộng 0,08 m theo trục Z. Thanh chặn hổ phách ở cửa có kích thước 0,026 × 0,080 × 0,076 m, khối lượng 0,18 kg, trượt 0,12 m dọc ray về phía +Z. Hai ray dẫn gắn phía nắp có hình học/collider nhìn thấy được. Sàn tại cửa giữ phẳng, không đặt mép ray dưới đường lăn. Chiều cao thanh chặn cùng vách ngăn khiến bi không thể lách trên hoặc dưới cửa.

Trong khoang trái có hốc mở về phía -Z. Thành sau giữ bi khi nghiêng theo +Z; cạnh bên giúp đưa bi vào vị trí phù hợp trước cửa. Khoang phải chứa lỗ thoát tròn phẳng giống các bàn thử trước.

## Cách suy luận

1. Đưa bi xuống phía -Z của khoang trái, rồi sang phải tới miệng hốc.
2. Nghiêng +Z để bi vào hốc. Thành sau giữ bi lại, trong khi thanh chặn tiếp tục trượt trên ray và mở cửa.
3. Chuyển độ nghiêng sang +X để bi đi ngang qua cửa vào khoang phải.
4. Đưa bi tới lỗ tròn ở (+0,19; -0,10) trên mặt XZ và để toàn bộ bi ra ngoài.

Đây là một đường suy luận, không phải chuỗi thao tác bắt buộc. Thanh chặn có thể đóng lại nếu nghiêng theo chiều ngược. Người chơi được dùng mọi chuyển động hợp lệ của hộp để giải, gồm việc đi qua khi cửa chỉ mở một phần đủ cho bi hoặc tận dụng quán tính. Trạng thái guide không quyết định quyền đi qua; collider thật quyết định.

## Hợp đồng vật lý

- Một PhysicalProp luôn dynamic, dùng chung EnvironmentForceSystem với bi. Khối lượng không làm vật rơi chậm hơn; sự khác biệt đến từ ray và các mặt tiếp xúc.
- ConfigurableJoint giới hạn một trục, không có drive/motor/spring hoặc projection. Ray được mô hình hóa lý tưởng, không ma sát dọc trục; các va chạm với housing/end stop vẫn dùng vật liệu vật lý.
- GravitySliderGuide chỉ đo displacement và báo khi hình học đủ thoáng (0,078 m). Giá trị này không bật/tắt collider, không gửi tín hiệu và không khóa trạng thái mở.
- Reset trả root trước, sau đó thanh trượt và bi về pose/vận tốc đầu. Đổi bàn hủy đăng ký body thanh trượt, không giữ force target hoặc collider cũ.

## Kiểm chứng

Các test kiểm tra cả bán kính bi ở mọi độ cao hợp lệ của cửa đóng, va chạm thật với thanh chặn, trượt đi/về theo trọng lực, không có signal, 100 reset và thu hồi body. Một fixture bắt đầu từ spawn, chỉ gửi rotation intent, đưa bi vào hốc, mở thanh chặn rồi qua lỗ thoát thật. Kết quả từng lượt và native review được ghi trong [nhật ký](DEVELOPMENT_LOG.md), không suy diễn từ các test tám hộp trước đó.
