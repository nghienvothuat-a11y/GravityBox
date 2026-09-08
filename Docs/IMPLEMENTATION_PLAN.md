# Kế hoạch prototype: ba hộp, một viên bi thép

Yêu cầu mới của người dùng đặt cảm giác chuyển động của bi lên trên số lượng màn và độ phức tạp puzzle. Phạm vi đang áp dụng là ba hộp tròn, vuông, tam giác; trong hộp vuông có một khối lập phương cố định. Kế hoạch 16 màn và cơ chế nắp rơi đã chuyển thành [tài liệu lịch sử](Archive/README.md).

## Mục tiêu nghiệm thu

Người chơi nghiêng nhẹ hộp và thấy bi tăng tốc; trả hộp về ngang thì bi tiếp tục lăn và giảm tốc dần; đổi hướng nghiêng thì bi cần thời gian mất vận tốc cũ. Khi va vào khối lập phương, bi đổi hướng và nảy với năng lượng giảm, đi kèm âm thanh kim loại tương ứng với lực va chạm. Chuyển động quay của bi phải đọc được qua bề mặt và phản xạ ánh sáng.

Chỉ tăng mass không đạt mục tiêu này: trọng lực tạo cùng gia tốc cho vật tự do. Scale, quán tính quay, ma sát tiếp xúc, độ nảy, vận tốc mặt hộp và feedback cùng quyết định cảm giác. Không giảm gravity, gắn bi vào hộp, kéo bi theo transform hoặc làm slow motion để giả sức nặng.

## Trình tự hoàn thiện

| Giai đoạn | Đầu ra | Cổng xác nhận |
| --- | --- | --- |
| 1. Mô hình vật lý | Bi thép đường kính 30 mm/111 g, hộp bàn nhỏ, 9,81 m/s², 120 Hz | Freefall, quán tính, dốc lăn, tổn hao năng lượng qua contact |
| 2. Ba hộp | Thành tròn/vuông/tam giác thật; cube cố định; lỗ thoát phẳng | Spawn không chồng, vỏ kín, cube tạo xung/nảy, lỗ thông |
| 3. Đọc chuyển động | Vật liệu bi, camera, âm thanh lăn/va chạm, HUD ba bàn | Nhìn được quay/tăng tốc; âm thanh phản ánh va chạm thực |
| 4. Lặp thí nghiệm | Reset, pause, chọn hộp, quan sát sau thoát ở thời gian thực | 100 reset mỗi bàn, không giữ state cũ, không auto advance |
| 5. Thử cảm giác | Quan sát trực tiếp các tình huống dưới đây | Người dùng xác nhận cảm giác bi có sức nặng và có thể dự đoán |

## Buổi thử cảm giác

1. Hộp tròn: nghiêng một góc nhỏ, giữ; quan sát bi tăng tốc và lăn dọc thành cong. Trả ngang rồi đảo nghiêng để đánh giá quán tính.
2. Hộp vuông: cho bi chạm cube trực diện ở tốc độ nhỏ và lớn; lặp với va chạm xiên. So sánh hướng nảy, độ cao nảy, spin và âm thanh.
3. Hộp tam giác: đưa bi vào một góc, giữ để bi ổn định, rồi nghiêng theo cạnh khác. Kiểm tra tiếp xúc hai mặt không rung hoặc phóng bi bất thường.
4. Trong mỗi hộp: reset và lặp lại thao tác; đưa bi ra khỏi lỗ thật. Sau khi thoát, tốc độ mô phỏng không đổi và bàn không tự chuyển.

Ghi cảm nhận nhẹ/nặng, trơn/dính, nảy quá mức/thiếu phản hồi, khả năng thấy spin và điều khiển góc nhỏ. Mỗi vòng chỉ thay một nhóm thông số, lưu profile/commit để so sánh. Test tự động bảo vệ vật lý và lifecycle; việc đạt cảm giác cần được xác nhận bằng chơi trực tiếp.

## Sau prototype

Chưa đưa thêm màn, zero-G, switch, door, pad, hazard hoặc nắp rơi vào catalog. Sau khi người dùng chốt cảm giác, dùng cùng mô hình để thiết kế puzzle mới, rồi mới cân nhắc onboarding, tiến trình, save, hiệu năng thiết bị thật và nội dung thương mại.

Kết quả từng lượt test/build/chạy được ghi tại [DEVELOPMENT_LOG.md](DEVELOPMENT_LOG.md); không suy diễn việc chạy trên thiết bị từ một lần build thành công.
