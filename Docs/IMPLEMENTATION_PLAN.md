# Kế hoạch prototype: tám hộp thử và ba puzzle vật lý

Phạm vi hiện tại có tám hộp thử hình học, bàn 09 **Leave it behind**, bàn 10 với mê cung và hai thanh trượt đối nhau, bàn 11 với mê cung ba tầng và lỗ chuyển tầng vật lý. Hộp vuông tiếp tục có cube cố định. Mô hình bi thép và trọng lực dùng chung; cơ cấu mới phải mở đường bằng chuyển động thật. Kế hoạch 16 màn và cơ chế nắp rơi ở [tài liệu lịch sử](Archive/README.md).

## Mục tiêu nghiệm thu

Người chơi nghiêng nhẹ hộp và thấy bi tăng tốc; trả hộp về ngang thì bi tiếp tục lăn và giảm tốc dần; đổi hướng nghiêng thì bi cần thời gian mất vận tốc cũ. Khi va vào khối lập phương, bi đổi hướng và nảy với năng lượng giảm, đi kèm âm thanh kim loại tương ứng với lực va chạm. Chuyển động quay của bi phải đọc được qua bề mặt và phản xạ ánh sáng.

Chỉ tăng mass không đạt mục tiêu này: trọng lực tạo cùng gia tốc cho vật tự do. Scale, quán tính quay, ma sát tiếp xúc, độ nảy, vận tốc mặt hộp và feedback cùng quyết định cảm giác. Không giảm gravity, gắn bi vào hộp, kéo bi theo transform hoặc làm slow motion để giả sức nặng.

## Trình tự hoàn thiện

| Giai đoạn | Đầu ra | Cổng xác nhận |
| --- | --- | --- |
| 1. Mô hình vật lý | Bi thép đường kính 30 mm/111 g, hộp bàn nhỏ, 9,81 m/s², 120 Hz | Freefall, quán tính, dốc lăn, tổn hao năng lượng qua contact |
| 2. Tám hộp | Ba hình cơ bản và năm hình lõm/lõi rỗng; cube cố định; lỗ thoát phẳng | Spawn không chồng, sàn/nắp đúng contour, vỏ kín, lối đi đủ cả bán kính bi |
| 3. Đọc chuyển động | Vật liệu bi, camera, âm thanh lăn/va chạm, HUD 11 bàn | Nhìn được quay/tăng tốc; chọn được mọi hình trên màn hình nhỏ |
| 4. Lặp thí nghiệm | Reset, pause, chọn hộp, quan sát sau thoát ở thời gian thực | 100 reset mỗi bàn, không giữ state cũ, không auto advance |
| 5. Thử cảm giác | Quan sát trực tiếp các tình huống dưới đây | Người dùng xác nhận cảm giác bi có sức nặng và có thể dự đoán |
| 6. Puzzle 09 | Hốc giữ bi, thanh chặn trên ray không motor, cửa xuyên vách và lỗ thoát | Cửa đóng ngăn cả bi; gravity mở/đóng thật; giải được bằng nghiêng; reset hai body |
| 7. Mê cung 10 | Hành lang đổi hướng, hai thanh chặn trên ray ngược nhau và hai hốc giữ | Hai cửa đáp ứng gravity đối nhau, chặn toàn bộ chiều sâu; route từ spawn qua hai cửa đến thoát; reset ba body |
| 8. Mê cung 11 | Ba sàn, ba hệ vách, hai lỗ chuyển tầng lệch nhau, chế độ xem từng tầng/tổng thể | Sàn đỡ bi ngoài lỗ; cầu rơi qua lỗ; route liên tục qua cả ba tầng tới thoát; chế độ xem giữ mọi collider |

## Buổi thử cảm giác

1. Hộp tròn: nghiêng một góc nhỏ, giữ; quan sát bi tăng tốc và lăn dọc thành cong. Trả ngang rồi đảo nghiêng để đánh giá quán tính.
2. Hộp vuông: cho bi chạm cube trực diện ở tốc độ nhỏ và lớn; lặp với va chạm xiên. So sánh hướng nảy, độ cao nảy, spin và âm thanh.
3. Hộp tam giác: đưa bi vào một góc, giữ để bi ổn định, rồi nghiêng theo cạnh khác. Kiểm tra tiếp xúc hai mặt không rung hoặc phóng bi bất thường.
4. Chữ L/U: đưa bi qua các góc, đổi hướng nghiêng trước khi bi chạm thành để đánh giá quán tính.
5. Vành khuyên: lăn quanh lõi rỗng; quả tạ: đưa bi qua cổ giữa; ngôi sao: chuyển từ cánh qua vùng giữa sang cánh khác.
6. Bàn 09: đưa bi vào hốc giữ, quan sát thanh chặn trượt khi nghiêng; chuyển hướng để bi qua cửa và ra lỗ. Thử đổi chiều nghiêng để xem thanh chặn tự đóng lại.
7. Bàn 10: đi theo hành lang tới hốc thứ nhất; mở cửa A rồi chuyển sang hốc thứ hai và nghiêng ngược để mở B. Kiểm tra có thể đọc cửa đang trượt về đóng trong khi bi lăn.
8. Bàn 11: theo đường mê cung tới lỗ trên, quan sát bi rơi xuống tầng giữa rồi tầng dưới. Chuyển chế độ xem để đọc đường đi và xác nhận các tầng mờ vẫn va chạm.
9. Trong mỗi hộp: reset và lặp lại thao tác; đưa bi ra khỏi lỗ thật. Sau khi thoát, tốc độ mô phỏng không đổi và bàn không tự chuyển.

Ghi cảm nhận nhẹ/nặng, trơn/dính, nảy quá mức/thiếu phản hồi, khả năng thấy spin và điều khiển góc nhỏ. Mỗi vòng chỉ thay một nhóm thông số, lưu profile/commit để so sánh. Test tự động bảo vệ vật lý và lifecycle; việc đạt cảm giác cần được xác nhận bằng chơi trực tiếp.

## Sau prototype

Phạm vi hiện tại dừng ở 11 bàn đã yêu cầu. Hai mê cung mới thêm khả năng đổi hướng/đọc không gian, nhưng cần thử trực tiếp để đánh giá độ khó và khả năng nhìn tầng trên màn hình nhỏ. Zero-G, switch, cửa theo tín hiệu, pad, hazard hoặc nắp rơi tiếp tục nằm ngoài catalog. Các mở rộng sau cần dùng cùng mô hình vật lý đã chốt trước khi bổ sung onboarding, tiến trình, save, hiệu năng thiết bị thật và nội dung thương mại.

Kết quả từng lượt test/build/chạy được ghi tại [DEVELOPMENT_LOG.md](DEVELOPMENT_LOG.md); không suy diễn việc chạy trên thiết bị từ một lần build thành công.
