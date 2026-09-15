# Venom — Chương mở đầu 01–10

Ngày cập nhật: 15/09/2026. Người dùng đã thiết kế chín màn đầu và quyết định **màn 10 lấy màn 4 hiện tại**. Chương này giúp người chơi làm quen với cách chỉ dẫn và nhận ra các khả năng của sinh vật; độ khó tăng từ dùng riêng từng kỹ năng tới phối hợp chúng.

Đây là bảng đối chiếu thiết kế, chưa phải catalog đã dựng đủ mười scene. Bản macOS hiện vẫn chạy năm màn `VenomJourney01`–`05`. Trước khi nhận bản vẽ 03–09, không tự điền bố cục, cơ quan, vị trí lỗ hoặc đường giải của chúng.

## Tư liệu đã đối chiếu

| Số màn campaign mới | Tư liệu có thể kiểm tra | Nội dung được hiểu ở mức bản vẽ | Trạng thái |
| --- | --- | --- | --- |
| 01 | Bản vẽ giấy người dùng gửi `798016963_1334366525439762_493405465279401791_n.jpg` | Sinh vật trên sàn hộp vuông trong suốt, gần góc; lỗ ở xa để học chạm hướng dẫn và quan sát sinh vật bò | Ý tưởng đã được giải nghĩa; **khác** scene `VenomJourney01` hiện tại về vị trí lỗ |
| 02 | Bản vẽ giấy người dùng gửi `796095026_1362977409238152_3256449574949989350_n.jpg` | Sinh vật xuất phát trên sàn, một vách thấp, lỗ ở tường phía đối diện; cho thấy cách vượt vách và bám mặt kính | Ý tưởng đã được giải nghĩa; **khác** scene `VenomJourney02` hiện tại có lỗ giữa trần; đầu vách còn cần đối chiếu xem có đường vòng hay kín |
| 03–09 | Người dùng báo đã thiết kế; chưa thấy bản vẽ/mô tả của các màn này trong tư liệu được gửi hoặc workspace | Không gán kỹ năng, cơ quan hay đường giải khi chưa xem thiết kế | Chờ tư liệu để đối chiếu đúng ý người dùng |
| 10 | Màn 4 đang chơi mặc định: `Assets/_Game/Venom/VenomJourney04.unity` | Dẫn sinh vật tới máy chém; hai phần giữ riêng A/B để chốt cửa, rồi cả hai thoát thật | **Giả định nguồn màn 4** cho tới khi người dùng xác nhận; bộ thí nghiệm `Venom04.unity` là một màn khác về bò sáu mặt |

Các file `3.jpg`, `4.jpg` trong Downloads đã kiểm tra là ảnh một trò ghép thú khác, không phải bản vẽ Venom màn 03–04. Concept “Glass Flower” trong `Docs/ArtDirection/Venom` thuộc đề xuất boss 10 trước quyết định này, chưa dựng trong Unity và không được tự thay vào màn 10 mới.

## Vai trò học và tăng độ khó

- **Màn 01** là bài nhận lệnh: chỉ một việc dễ thấy, sinh vật đáp lại và di chuyển tới lỗ. Không đặt mức độ khó bằng thời gian bò dài.
- **Màn 02** thêm một việc mới: hiểu mép/vách và khả năng leo, bám mặt kính. Vách phải có va chạm và cơ thể cần thật sự vượt nó; nếu có đường vòng, level phải chủ ý cho phép hoặc loại bỏ nó theo thiết kế người dùng.
- **Màn 03–09** do người dùng định nghĩa. Khi nhận bản vẽ, đánh dấu mỗi màn dạy kỹ năng nào trong [tám kỹ năng nền](VENOM_CREATURE_SKILLS.md), đã học gì trước đó và kết hợp kỹ năng nào. Không giả định cần ép đủ tám kỹ năng vào chín màn nếu tiến độ học không mượt.
- **Màn 10** là bài phối hợp của chương mở đầu. Dùng cơ quan và đường giải của màn 4 nguồn: đến dao để phân tách (chỉ tách bằng cơ quan cắt); một phần giữ A làm mở lối tới B; phần còn lại giữ B đủ điều kiện để chốt cửa; người chơi đưa **toàn bộ** vật chất ra lỗ. Đó là bài kiểm tra cách phân vai và nhiệm vụ bền vững, không cần phát minh chỉ số hoặc copy để qua.

Màn 10 hiện tại có A/B cùng yêu cầu ít nhất 12 g mô thực sự ở vùng cảm biến, trong hai phần khác nhau, giữ đồng thời đủ 0,65 giây. A che/mở nắp tiếp cận B; sau khi B chốt, cửa thoát mở. Người chơi được xoay hộp trong phần dẫn đường nhưng dao khóa xoay trong chu trình cắt để tránh xung lực giả. Sau khi cắt, hai phần đủ gần có thể tự tụ theo điều kiện kết dính và nhiệm vụ hiện có; **không cần tụ để hoàn thành nguồn màn 4**, vì luật thắng là mọi phần ra ngoài.

Để màn 10 không gây sốc, các màn trước cần cho người chơi biết hoặc có bài nhắc ngắn về: vị trí dao cắt, chọn từng phần, giữ một nhiệm vụ khi đổi phần, cơ quan A ảnh hưởng B, và việc phải đưa mọi phần ra ngoài. Màn nào cụ thể dạy từng điều sẽ lấy từ các bản vẽ 03–09; không sửa bố cục của người dùng để lấp chỗ trống trước khi xem.

Mốc mỗi 10 màn trước đây được định hướng là một màn boss có khoảnh khắc thị giác. Quyết định mới chọn **gameplay màn 4 hiện tại làm màn 10**; nếu tiếp tục gọi đây là boss đầu, đầu tư camera, ánh sáng, khoảnh khắc dao cắt, A mở B và đoạn ăn mừng trên cùng cơ quan thật. Không thêm luật chưa được dạy hoặc dùng concept “Glass Flower” cũ để đổi lời giải.

## Cách chuyển thành bản chơi sau khi đủ tư liệu

1. Đối chiếu 03–09 từ bản vẽ, làm bảng kỹ năng/quan hệ cơ quan/đường giải và sửa những chỗ tao giải nghĩa sai trước khi sinh scene.
2. Dựng 01–09 theo catalog mới rồi dùng **cơ quan và luật của `VenomJourney04`** làm nguồn cho scene campaign 10. Giữ source hiện tại để hồi quy; không chỉ đổi `LevelNumber` từ 4 sang 10 trên scene cũ.
3. Khi renumbering, cập nhật tên scene, title/hint, hàng chọn màn, tiến trình hoàn thành/save, tự sang màn, build scene list và tests. Runtime Journey hiện dùng mảng 01–05, `Completed` chỉ giữ năm bit; đổi số đơn lẻ sẽ làm truy cập sai hoặc mất tiến trình người chơi cũ. Cần tách ID campaign mới với nguồn scene prototype và có chuyển đổi save tương thích.
4. Kiểm thử đường giải của từng màn với kỹ năng đã học, sai lệch cắt, xoay hộp, retry và khả năng giải phóng mọi phần. Màn 10 phải giữ điều kiện tiếp xúc A/B và 100% vật chất thoát thật; hiệu ứng chỉ biểu diễn kết quả cơ quan.

Tài liệu này ghi nhận quyết định và những khoảng thông tin còn thiếu. Chưa xây thêm scene, chưa đổi thứ tự màn trong bản macOS, chưa chạm save người chơi hoặc build APK/macOS mới.
