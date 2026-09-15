# Venom — Chương mở đầu 01–10

Ngày cập nhật: 15/09/2026. Người dùng đã thiết kế chín màn đầu và quyết định **màn 10 lấy màn 4 hiện tại**. Chương này giúp người chơi làm quen với cách chỉ dẫn và nhận ra các khả năng của sinh vật; độ khó tăng từ dùng riêng từng kỹ năng tới phối hợp chúng.

Đây là bảng đối chiếu thiết kế, chưa phải catalog đã dựng đủ mười scene. Bản macOS hiện vẫn chạy năm màn `VenomJourney01`–`05`. Đã nhận và đối chiếu bản vẽ **01–04**; người dùng xác nhận vách 02 bịt kín hai đầu và lời giải xoay hộp/lợi dụng trọng lực ở 04 là một tính năng. Trước khi nhận bản vẽ **05–09**, không tự điền bố cục, cơ quan, vị trí lỗ hoặc đường giải của chúng.

Tên và số màn dưới đây thuộc campaign mới. **Màn 04 mới “Trơn đấy” khác màn `VenomJourney04` đang chơi**, được chọn làm nguồn màn 10.

## Tư liệu đã đối chiếu

| Số màn campaign mới | Tư liệu có thể kiểm tra | Nội dung được hiểu ở mức bản vẽ | Trạng thái |
| --- | --- | --- | --- |
| 01 — Bò đi | `6412883e-1004-474c-811e-c028b980214e.jpeg` | Sinh vật trên sàn hộp vuông trong suốt, gần góc trước; lỗ ở góc xa trên sàn để học chạm hướng dẫn và quan sát sinh vật bò | Đã đối chiếu; **khác** scene `VenomJourney01` hiện tại về vị trí lỗ |
| 02 — Leo đi | `255d92cb-afa4-49e1-8c44-169d1f2acce2.jpeg` | Sinh vật trên sàn, vách thấp bịt kín hai đầu tới thành hộp, lỗ trên tường phía đối diện; dạy vượt vách và bám kính | **Đã xác nhận vách kín hai đầu**; khác scene `VenomJourney02` hiện tại có lỗ giữa trần |
| 03 — Xoay đi | `44ee8e3f-6dd9-4a27-a852-524091b7f1e4.jpeg` | Lỗ ở mặt sau; chạm theo hình chiếu khi chưa xoay chỉ chọn mặt trước. Xoay hộp để mặt sau hướng tới người chơi rồi chỉ dẫn | Đã đối chiếu; lấy chú thích “mặt sau” làm chuẩn, không suy ra lỗ trên trần từ phối cảnh vẽ |
| 04 — Trơn đấy | `d36aaf27-59af-4a63-90dd-7a2c516527ec.jpeg` | Sinh vật bám sẵn trên thành; lỗ ở tường đối diện, vùng trơn dưới lỗ. Dẫn thẳng sẽ trượt/rơi; tìm đường bám khác hoặc xoay hộp tận dụng trọng lực | **Đã xác nhận lời giải bằng xoay hộp/trọng lực hợp lệ** |
| 05–09 | Người dùng báo đã thiết kế; chưa nhận bản vẽ/mô tả | Không gán kỹ năng, cơ quan hay đường giải khi chưa xem thiết kế | Chờ tư liệu để đối chiếu đúng ý người dùng |
| 10 | Màn 4 đang chơi mặc định: `Assets/_Game/Venom/VenomJourney04.unity` | Dẫn sinh vật tới máy chém; hai phần giữ riêng A/B để chốt cửa, rồi cả hai thoát thật | **Giả định nguồn màn 4** cho tới khi người dùng xác nhận; bộ thí nghiệm `Venom04.unity` là một màn khác về bò sáu mặt |

Các ảnh gốc mới nằm trong `/Users/mrk/Downloads/`; thứ tự người dùng đính kèm là **03, 02, 01, 04**, không phải thứ tự campaign. Hai bản vẽ 01–02 gửi trước (`798016963_1334366525439762_493405465279401791_n.jpg`, `796095026_1362977409238152_3256449574949989350_n.jpg`) là tư liệu trước của cùng ý tưởng. Tên màn dùng đúng yêu cầu trực tiếp của người dùng, kể cả khi tiêu đề trên giấy khác.

Concept “Glass Flower” trong `Docs/ArtDirection/Venom` thuộc đề xuất boss 10 trước quyết định này, chưa dựng trong Unity và không được tự thay vào màn 10 mới.

## Thiết kế đã đối chiếu của màn 01–04

### 01 — Bò đi

Hộp trong suốt, trống; sinh vật trên sàn gần góc trước, lỗ tròn ở góc xa trên cùng sàn. Người chơi chạm để chỉ điểm đến, sinh vật tự bò tới; đến gần lỗ thì thực hiện thoát tự động như hướng điều khiển đã chọn. Mục tiêu là nhận ra quan hệ giữa chạm, phản ứng của sinh vật và điểm đến. Không thêm cơ quan hoặc bắt người chơi leo để hoàn thành bài này.

### 02 — Leo đi

Sinh vật xuất phát trên sàn. Một vách thấp chắn đường tới phía lỗ, **hai đầu vách nối kín tới thành hộp**, không có khe để bò vòng qua đầu vách trên sàn. Lỗ nằm cao hơn sàn trên tường phía đối diện. Đường giải được giới thiệu là bò tới vách, leo vượt vách, rồi bám kính đi lên lỗ. Người chơi chỉ mục tiêu; sinh vật thực hiện việc chuyển tiếp giữa các mặt tiếp xúc.

Vách có hình học và va chạm thật; cơ thể phải vượt được cạnh trên, không xuyên qua vách bằng animation. Bịt kín hai đầu không đồng nghĩa thêm luật vô hình bắt buộc “đã trèo đúng vách”: nếu một đường leo khác trên thành hộp hợp lệ theo cùng luật vật lý thì không đánh trượt lời giải đó. Chiều cao/độ dày vách sẽ được chỉnh khi dựng để thao tác leo đọc rõ và dễ ở đầu game.

### 03 — Xoay đi

Sinh vật bắt đầu trên sàn. Lỗ ở **mặt sau** của hộp theo chú thích bản vẽ. Từ góc nhìn ban đầu, chạm vào hình chiếu của lỗ sẽ nhận điểm trên mặt trước; người chơi xoay hộp đưa mặt có lỗ ra trước rồi chạm đúng bề mặt để hướng dẫn sinh vật.

Điểm chỉ dẫn phải hiển thị trên mặt thực sự được chọn để người chơi hiểu kết quả chạm. Quy tắc chọn bề mặt phải nhất quán giữa các màn; không tự xuyên qua mặt trước để chọn lỗ phía sau chỉ vì cùng tọa độ màn hình. Cách trình bày mặt kính và lựa chọn mặt sàn ở 01–02 cần được đối chiếu cùng quy tắc này khi thiết kế input, tránh luật riêng gây mâu thuẫn.

### 04 — Trơn đấy

Sinh vật xuất phát **đã bám trên thành hộp**. Lỗ ở tường đối diện. Vùng vật liệu trơn nằm dưới lỗ và ôm phần dưới của nó như bản vẽ; không tự mở rộng thành một vòng kín chắn mọi hướng tới lỗ.

Nếu người chơi chỉ thẳng vào lỗ, sinh vật đi theo hướng đó, chạm vùng trơn, mất khả năng bám ở phần tiếp xúc và trượt/rơi dưới **trọng lực Trái Đất**. Người chơi có thể chỉ các điểm trung gian để đi vòng trên phần kính bám được, tiếp cận từ bên hoặc phía trên. Trong bài này AI không tự tìm sẵn đường vòng tránh vùng trơn khi người chơi chỉ thẳng vào lỗ, vì chọn đường là nội dung giải đố.

Người dùng xác nhận **xoay hộp để đổi tương quan giữa vùng trơn, lỗ và hướng trọng lực là lời giải hợp lệ**, kể cả khi sinh vật trượt hoặc rơi vào lỗ. Đích kiểm tra là toàn bộ sinh vật thoát thật qua lỗ, không phải đã đi đủ các điểm của đường giải mẫu.

Về yêu cầu mô phỏng khi triển khai: vùng trơn cần tác động tới khả năng bám chủ động, không chỉ giảm ma sát nhưng vẫn cho bộ điều khiển giữ cơ thể trên tường. Hướng rơi giữ theo world gravity khi hộp xoay. Hỗ trợ thoát gần lỗ phải được chỉnh để không hút qua vật cản hoặc xóa luôn thử thách vùng trơn. Các thông số vật liệu, kích thước và bán kính hỗ trợ chưa chốt; cần chơi thử để cân bằng.

## Nguyên tắc công nhận lời giải

**Lời giải khác dự kiến nhưng tuân thủ các quy luật vật lý và cơ quan của game vẫn được công nhận.** Xác nhận màn 04 cụ thể hóa nguyên tắc này cho campaign mới: không khóa xoay hay thêm cờ bắt buộc đi đúng đường mẫu chỉ để giữ một đáp án. Kiểm tra thắng dựa trên trạng thái thoát thật của toàn bộ vật chất và các điều kiện cơ quan thực sự có trong màn. Lỗi xuyên collider hoặc vật chất bật khỏi vỏ ở ngoài lỗ vẫn là lỗi mô phỏng cần sửa.

Khi nghiệm thu 01–04, cần kiểm tra tương ứng: bò tới góc xa; vách kín nhưng có thể leo qua; dấu chạm ở đúng mặt trước/sau khi xoay; vùng trơn gây trượt đúng hướng trọng lực và cả đường vòng lẫn lời giải xoay hộp đều hoàn thành được. Đây là tiêu chí kiểm thử sắp tới, chưa phải kết quả đã chạy trên scene mới.

## Vai trò học và tăng độ khó

- **Màn 01** là bài nhận lệnh: chỉ một việc dễ thấy, sinh vật đáp lại và di chuyển tới lỗ. Không đặt mức độ khó bằng thời gian bò dài.
- **Màn 02** thêm khả năng leo qua vách và bám mặt kính; hai đầu vách kín, không có đường vòng trên sàn.
- **Màn 03** dạy xoay hộp để quan sát và chỉ đúng bề mặt; tiếp tục dùng kỹ năng bò/leo đã biết.
- **Màn 04** phối hợp chỉ đường, leo và xoay; giới thiệu vật liệu trơn, yêu cầu người chơi tự chọn đường hoặc tận dụng trọng lực.
- **Màn 05–09** do người dùng định nghĩa. Khi nhận bản vẽ, đánh dấu mỗi màn dạy kỹ năng nào trong [tám kỹ năng nền](VENOM_CREATURE_SKILLS.md), đã học gì trước đó và kết hợp kỹ năng nào. Không giả định cần ép đủ tám kỹ năng vào chín màn nếu tiến độ học không mượt.
- **Màn 10** là bài phối hợp của chương mở đầu. Dùng cơ quan và đường giải của màn 4 nguồn: đến dao để phân tách (chỉ tách bằng cơ quan cắt); một phần giữ A làm mở lối tới B; phần còn lại giữ B đủ điều kiện để chốt cửa; người chơi đưa **toàn bộ** vật chất ra lỗ. Đó là bài kiểm tra cách phân vai và nhiệm vụ bền vững, không cần phát minh chỉ số hoặc copy để qua.

Màn 10 hiện tại có A/B cùng yêu cầu ít nhất 12 g mô thực sự ở vùng cảm biến, trong hai phần khác nhau, giữ đồng thời đủ 0,65 giây. A che/mở nắp tiếp cận B; sau khi B chốt, cửa thoát mở. Người chơi được xoay hộp trong phần dẫn đường nhưng dao khóa xoay trong chu trình cắt để tránh xung lực giả. Sau khi cắt, hai phần đủ gần có thể tự tụ theo điều kiện kết dính và nhiệm vụ hiện có; **không cần tụ để hoàn thành nguồn màn 4**, vì luật thắng là mọi phần ra ngoài.

Để màn 10 không gây sốc, các màn trước cần cho người chơi biết hoặc có bài nhắc ngắn về: vị trí dao cắt, chọn từng phần, giữ một nhiệm vụ khi đổi phần, cơ quan A ảnh hưởng B, và việc phải đưa mọi phần ra ngoài. Màn nào cụ thể dạy từng điều sẽ đối chiếu tiếp với các bản vẽ 05–09; không sửa bố cục của người dùng để lấp chỗ trống trước khi xem.

Mốc mỗi 10 màn trước đây được định hướng là một màn boss có khoảnh khắc thị giác. Quyết định mới chọn **gameplay màn 4 hiện tại làm màn 10**; nếu tiếp tục gọi đây là boss đầu, đầu tư camera, ánh sáng, khoảnh khắc dao cắt, A mở B và đoạn ăn mừng trên cùng cơ quan thật. Không thêm luật chưa được dạy hoặc dùng concept “Glass Flower” cũ để đổi lời giải.

## Cách chuyển thành bản chơi sau khi đủ tư liệu

1. Nhận và đối chiếu 05–09 từ bản vẽ, hoàn thiện bảng kỹ năng/quan hệ cơ quan/đường giải. Sau khi chốt thiết kế, lập kiến trúc authoring/runtime có thể mở rộng, rồi mới sinh scene theo yêu cầu người dùng. Giữ các quy tắc đã xác nhận của 01–04 ở trên.
2. Dựng 01–09 theo catalog mới rồi dùng **cơ quan và luật của `VenomJourney04`** làm nguồn cho scene campaign 10. Giữ source hiện tại để hồi quy; không chỉ đổi `LevelNumber` từ 4 sang 10 trên scene cũ.
3. Khi renumbering, cập nhật tên scene, title/hint, hàng chọn màn, tiến trình hoàn thành/save, tự sang màn, build scene list và tests. Runtime Journey hiện dùng mảng 01–05, `Completed` chỉ giữ năm bit; đổi số đơn lẻ sẽ làm truy cập sai hoặc mất tiến trình người chơi cũ. Cần tách ID campaign mới với nguồn scene prototype và có chuyển đổi save tương thích.
4. Kiểm thử đường giải của từng màn với kỹ năng đã học, sai lệch cắt, xoay hộp, retry và khả năng giải phóng mọi phần. Màn 10 phải giữ điều kiện tiếp xúc A/B và 100% vật chất thoát thật; hiệu ứng chỉ biểu diễn kết quả cơ quan.

Tài liệu này ghi nhận quyết định và những khoảng thông tin còn thiếu. Chưa xây thêm scene, chưa đổi thứ tự màn trong bản macOS, chưa chạm save người chơi hoặc build APK/macOS mới.
