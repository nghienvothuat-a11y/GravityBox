# Triết lý thiết kế: cảm giác của bi thép

Đối tượng quan trọng nhất là viên bi: người chơi phải cảm thấy nó có quán tính, đang quay, nhận gia tốc khi nghiêng và truyền xung lực khi va vào hộp. Tám hộp thử hình học quan sát hành vi này; bàn 09 đưa thêm một thanh chặn trượt để tạo puzzle bằng cùng trọng lực và contact.

## Các nguyên tắc đang áp dụng

1. **Tỷ lệ vật lý nhất quán.** Một Unity unit là một mét. Bi đường kính 30 mm có mass khoảng 111 g, tương ứng thép đặc. Hộp có kích thước cầm tay/bàn nhỏ. Camera được chỉnh theo vật thể.
2. **Sức nặng không đến từ tăng mass đơn lẻ.** Trọng lực thế giới vẫn 9,81 m/s². Quán tính cầu đặc, năng lượng, ma sát và restitution quyết định gia tốc lăn và phản ứng va chạm. Không giảm gravity để làm bi chuyển động chậm giả.
3. **Người chơi tác động lên hộp.** Root kinematic chuyển động trong fixed step có giới hạn tốc độ/gia tốc góc; ball là body tự do trong world space. Xoay hộp không xoay trọng lực hoặc kéo ball bằng hierarchy.
4. **Contact là nguồn đổi chuyển động.** Mặt cong dẫn đường; mặt phẳng/cạnh đổi hướng; cube cố định nhận xung và làm bi nảy. Không dùng trigger để thêm cú nảy, tween, hút bi hoặc chỉnh vận tốc theo một lời giải.
5. **Năng lượng có lý do để mất hoặc tăng.** Rolling resistance tác động khi có mặt đỡ; va chạm không đàn hồi hoàn toàn mất năng lượng. Chỉ trọng lực và công từ hộp đang chuyển động có thể cấp năng lượng trong các bàn này. Air damping không được dùng để thay thế contact.
6. **Hình và tiếng diễn tả trạng thái thật.** Spin dễ nhận biết; âm lăn theo contact/tốc độ tiếp tuyến và âm va chạm theo xung thực. Không phát tiếng va chạm khi không có collision hoặc rung camera để che mô phỏng thiếu ổn định.
7. **Quan sát ở thời gian thực.** Không đổi time scale khi bi thoát. Người chơi tự reset/chọn hộp, đủ thời gian quan sát sự giảm tốc và kết quả va chạm.
8. **Hình học nhìn thấy khớp collision.** Mỗi silhouette là một vỏ thật. Sàn/nắp không lấp khoảng khuyết hoặc lõi rỗng; đường nối giữa hai vùng phải đủ rộng cho cả bi, không chỉ tâm bi. Lỗ tròn khoét xuyên, viền sáng mảnh không tạo gờ; toàn bộ bi ra ngoài mới ghi nhận thoát.

## Mỗi hộp kiểm chứng điều gì

| Hộp | Điểm quan sát chính |
| --- | --- |
| Tròn | Tăng tốc khi nghiêng, lăn liên tục dọc thành cong, giảm tốc khi trả ngang |
| Vuông + cube cố định | Va chạm trực diện/xiên, xung lực, độ nảy, spin và năng lượng sau va chạm |
| Tam giác | Hướng phản xạ ở cạnh xiên, tiếp xúc góc và khả năng thoát góc khi đổi độ nghiêng |
| Chữ L | Chuyển hướng qua góc vuông bên trong, đổi trục nghiêng trong khi còn quán tính |
| Chữ U | Đi qua hai góc liên tiếp và tránh khoảng khuyết mở ở giữa |
| Vành khuyên | Lăn quanh một lõi rỗng thật, tiếp xúc cả thành trong lẫn thành ngoài |
| Quả tạ | Đi từ buồng rộng qua cổ hẹp sang buồng còn lại |
| Ngôi sao | Đổi hướng ở các góc lồi/lõm và đưa bi từ một cánh qua vùng giữa |
| 09 — Leave it behind | Giữ bi bằng thành hốc trong khi trọng lực dời thanh chặn, rồi đi qua khoảng trống thật |

Thanh trượt ở bàn 09 luôn tự do dọc ray, nhận gia tốc thế giới và giữ collider suốt hành trình. Mở cửa nghĩa là vật cản đã dời đủ chỗ; đổi độ nghiêng có thể khiến nó trượt về đóng lại. Không dùng trigger/hốc giữ để gửi unlock, không cho latch vô hình hoặc thêm lực hỗ trợ lời giải. Nếu bi đi qua được một cửa mở một phần hoặc dùng quán tính để giải, đó là kết quả vật lý hợp lệ. Các công tắc, zero-G và nắp của baseline cũ tiếp tục nằm ngoài catalog. Đường giải tự động không chứng minh cảm giác nặng; kiểm tra định lượng phải đi kèm thử bằng chuột/touch.
