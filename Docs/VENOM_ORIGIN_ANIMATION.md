# Venom Origin — Animation theo hành động

Ngày 15/09/2026. Nghiên cứu và triển khai cho mười màn theo bản vẽ. Kết quả chơi/kiểm thử được ghi riêng khi nghiệm thu; bảng này mô tả ý đồ và cách gắn chuyển động với mô phỏng.

## Tham khảo

- [Arm Coordination in Octopus Crawling Involves Unique Motor Control Strategies](https://www.sciencedirect.com/science/article/pii/S0960982215002663): chuyển động bò đa hướng và phối hợp tay không theo một nhịp tuần hoàn cố định. Lấy ý tưởng các điểm bám lệch nhịp, không sao chép dáng bạch tuộc tám tay.
- [Small-scale soft-bodied robot with multimodal locomotion](https://www.nature.com/articles/nature25443): ví dụ vật mềm chuyển giữa nhiều cách di chuyển và dùng sóng biến dạng trong đường hẹp. Chỉ dùng làm tham khảo chuyển động; sinh vật trong game không có cơ chế dẫn động từ trường như robot nghiên cứu.

## Hành động và phản hồi

| Trạng thái | Chuyển động biểu diễn | Nguồn dữ liệu |
| --- | --- | --- |
| Idle | Mặt thân nhấp nhô lệch nhịp, ngóc một phần thân quan sát; thỉnh thoảng cử chỉ vui | Thời gian nghỉ, không có tác vụ chịu tải |
| Bò | Khối thân đi trước, nếp mô truyền dọc thân; xúc tu dò, bám, kéo rồi nhả không đồng nhịp | Vận tốc tương đối với mặt đỡ, ý định đi, điểm tiếp xúc |
| Leo | Chuyển hướng qua mép, giữ các điểm tì; phần bụng rủ theo hướng đất | Normal mặt bám, gravity thế giới và hướng đi |
| Trượt / rơi | Khi dấu chân còn lại không chịu nổi tải, nhả chân; thân kéo nhẹ theo vận tốc, xúc tu tìm bám ngắn rồi rút | Tỷ lệ dấu chân bám, quá tải bám, vận tốc, contact |
| Cố bò trên cầu trơn (06) | Chạm điểm: thân gợn theo hướng chỉ, xúc tu liên tục thử bám, trượt ngược rồi rút; cơ thể không tiến nhờ lệnh. Vỏ xoay vẫn tạo chuyển động tương đối và độ trễ của mô | Ý định từ điểm chạm, normal mặt cầu tại tiếp xúc và vận tốc tương đối; không thêm lực vật lý |
| Bắt vành / đáp đất | Nén thân ngắn, hồi lại; hai xúc tu giữ điểm đã chạm trên vành trong pha hãm, sau đó chuyển sang tư thế bám nghỉ | Tiếp xúc da thật với vành, trạng thái bắt rơi hữu hạn và biến thiên vận tốc |
| Đẩy | Thân nén theo hướng tác động, hai xúc tu tì vào hộp; các chân sau chống | Điểm tiếp xúc hộp, lực và đích đang kéo/đẩy |
| Kéo | Thân dài hơn giữa chỗ bám và hộp, xúc tu căng; nhả khi hết thời hạn | Điểm bám đi cùng Rigidbody của hộp, tác vụ kéo |
| Chui ống | Các hạt mô tiến qua lòng ống, skin thu tiết diện, gợn dọc dòng; đuôi theo sau, thu lại ở đầu ra | Vị trí/vận tốc hạt thực, trạng thái luồn; không thay sinh vật bằng VFX giả |
| Cắt | Liên kết bị lưỡi cắt, hai cụm mô riêng có phản ứng độc lập | Topology từ dao thật |
| Tụ | Mô gần nhau nối lại, cầu mô tăng độ kết dính; sắc sáng dịu ngắn | Sự kiện liên kết và FusionSeconds |
| Giữ cơ quan | Ổn định tư thế, không nhảy múa hoặc tự bỏ việc | Nhiệm vụ giữ A/B |
| Thắng | Một trong ba điệu vui, camera gần, ẩn vật che | Chỉ bắt đầu sau luật thắng hợp lệ |
| Ở nhà | Phản ứng chào/chơi và co người quanh món ăn | Tương tác Collection; không tăng lực hoặc khối lượng campaign |

## Ranh giới

Animation không dịch chuyển Rigidbody, tạo hạt, đổi nhóm hay mở cửa. Các biến dạng bổ sung của skin bị giới hạn ở biên mặt đỡ; collider vẫn quyết định chuyển động. Cơ thể dài qua ống cần giữ liên kết và vật chất thực, không chỉ nối hai đầu bằng một sợi vẽ. Ba điệu ăn mừng giữ nền đã có; camera chỉ được tập trung vào sinh vật sau khi toàn bộ một cơ thể đã qua lỗ.

Chi phí skin và raycast phải được đo khi thân trải dài và có nhiều phần. Không chạy đồng thời mọi hoạt cảnh; trạng thái tác vụ tắt các cử chỉ Idle không phù hợp. Chưa tuyên bố hiệu năng trên điện thoại trước khi đo bằng thiết bị.
