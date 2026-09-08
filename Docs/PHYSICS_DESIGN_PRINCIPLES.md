# Triết lý thiết kế: giải đố bằng vật lý

Quyết định của người dùng sau khi chơi màn 6: trải nghiệm phải tập trung vào vật lý. Người chơi xoay hộp để thay đổi điều kiện tiếp xúc, hướng của bề mặt so với trọng lực và đường đi của các vật. Một lời giải tốt xuất hiện từ việc hiểu chuyển động; không cần ghi nhớ chuỗi công tắc có tác dụng ở xa.

## Nguyên tắc bắt buộc cho cơ chế mới

1. **Nguyên nhân nhìn thấy được.** Nắp được mặt tựa đỡ, rơi vì trọng lực, bị đẩy khi va chạm. Hình dạng, khối lượng, ma sát, độ đàn hồi và lực giải thích được chuyển động.
2. **Trọng lực ở hệ tọa độ thế giới.** Trên Trái Đất, mọi vật tự do nhận cùng gia tốc hướng xuống. Xoay hộp không xoay trọng lực và không kéo các vật tự do bằng hierarchy. Trong zero-G không tự tạo lực rơi để mở một cơ cấu.
3. **Hình học hiển thị khớp va chạm.** Một lỗ thật phải có khoảng trống thật; nắp có mặt tựa nhìn thấy được. Vật cản đã rơi vẫn tồn tại và va chạm.
4. **Không kịch bản hóa cách mở.** Không dùng điều kiện góc để bật collider, tween nắp, teleport, hút bi hoặc tự thêm lực để bảo đảm lời giải. Va chạm, lực, khớp và giới hạn vật lý quyết định kết quả.
5. **Chấp nhận lời giải phát sinh.** Nếu người chơi đẩy hoặc làm nghiêng nắp bằng cách khác nhưng vẫn tuân thủ vật lý, đó là lời giải hợp lệ. Không ép đúng một chuỗi xoay đã soạn.
6. **Một màn dạy một ý chính.** Màn 6 dạy đổi hướng mặt tựa so với trọng lực. Độ khó đến từ bố trí vật thể và dự đoán chuyển động, không từ luật ẩn.
7. **Điều kiện thắng là kết quả quan sát được.** Toàn bộ bi phải đi qua lỗ và ra khỏi hộp. Nắp rơi không tự hoàn thành màn. Reset, pause, chọn màn, âm thanh và phần quan sát sau thắng là luật trình bày/vòng đời, không được sửa kết quả vật lý để cứu người chơi.

## Màn 6 — Let it fall

Nắp là một đĩa tròn rời ở mặt trong, bán kính 0,83 m và dày 0,09 m. Nó lớn hơn lỗ 0,78 m nên mặt hộp đỡ nó khi lỗ hướng xuống. Nắp có khối lượng 2 kg, một collider lồi và một Rigidbody tự do; không chốt lồng, bản lề, motor hoặc khóa ẩn.

Khi người chơi đưa lỗ lên phía trên, trọng lực kéo nắp rời khỏi chỗ tựa và rơi vào trong hộp. Lỗ được mở bằng việc vật cản thực sự dời chỗ. Người chơi tiếp tục xoay để đưa bi ra ngoài; nắp có thể va vào bi, thành hộp hoặc rơi trở lại gần lỗ. Không gắn cờ “đã mở vĩnh viễn”.

Đã bỏ pressure plate, cửa trượt bằng tín hiệu và RequiredChannel ở màn 6. Nắp được author trong prefab rồi tách ra world space khi load. Hệ lực và reset quản lý cả bi lẫn nắp, đồng bộ khôi phục pose và vận tốc khi reset, thu hồi đúng các body khi đổi màn.

## Kiểm chứng

- Lỗ ở dưới: nắp tự nằm yên trên mặt tựa và chặn bi bằng collision.
- Lỗ xoay lên: nắp tự rơi khỏi lỗ; không phát sự kiện mở cửa hoặc tự thắng.
- Body tự do chịu gia tốc Trái Đất một lần mỗi bước, không bị xoay theo transform của hộp; zero-G giữ động lượng.
- Nắp vẫn có collider sau khi rơi. Reset lặp lại không thêm body/force target; đổi màn không để lại vật thể cũ.
- Xoay hộp qua nhiều góc lớn liên tiếp: nắp vẫn nằm trong vỏ và tiếp tục được mô phỏng, không xuyên thành.
- Đường giải bắt đầu từ spawn chuẩn, chỉ điều khiển xoay hộp. Fixture gần cửa dùng trong test lifecycle không được coi là bằng chứng giải màn.

## Phạm vi hiện tại và hướng chuyển đổi nội dung

Màn 6 là cơ chế đầu tiên được thay theo triết lý này. Các màn cũ còn dùng tín hiệu/cơ cấu quy ước vẫn là nội dung prototype để rà soát; không xem chúng là mẫu chuẩn cho nội dung mới.

| Nội dung cũ | Hướng thiết kế tiếp theo | Tiêu chí trước khi thay |
| --- | --- | --- |
| Plate/SignalDoor tại L09, L10, L15 | Chuyển động của vật cản, đối trọng hoặc khớp cơ học trực tiếp; zero-G dùng quán tính/contact | Có hình học giải thích được cơ cấu và route mới bằng physics |
| OneWayGate tại L07/L09 | Cánh cửa bản lề có chặn góc, không đổi IgnoreCollision theo phía bi | Có test tiếp xúc hai chiều, mô-men và reset |
| Spring/ImpulsePad | Lò xo/bề mặt đàn hồi với năng lượng và lực có giới hạn | Không bơm thêm vận tốc từ cooldown; có bài đo năng lượng/động lượng phù hợp |
| Hazard và failure | Phân biệt luật thất bại với lực tác động lên vật | Không dùng fail/teleport để che lỗi collision |

Không thay hàng loạt bằng cơ cấu chưa kiểm chứng. Mỗi lần chuyển đổi cần sửa bố cục/hint, kiểm tra reset và xác nhận lại đường giải. Các giới hạn tốc độ, timestep và solver của prototype là ổn định số; phải được đo và chỉnh để không trở thành luật giải đố vô hình.
