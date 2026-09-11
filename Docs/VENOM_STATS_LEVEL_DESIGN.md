# Venom — Chỉ số, cơ quan và sự trưởng thành nhìn thấy được

Ngày: 11/09/2026. Trạng thái: đặc tả thiết kế cho giai đoạn tiếp theo, chưa triển khai hệ chỉ số hoặc các màn 06–10. Bản chơi vẫn có năm màn Journey hiện tại. Tài liệu này cập nhật hướng phát triển sau năm màn, thay cho chuỗi 06–10 sơ bộ trong `VENOM_GUIDED_PUZZLE_DESIGN.md`; mốc boss mỗi 10 màn được giữ lại.

## 1. Hợp đồng thiết kế

Mỗi nâng cấp phải thay đổi một hành vi quan sát được và tạo lợi ích trong một tình huống thật. Chuỗi giới thiệu: gặp cơ quan trong vùng an toàn → thấy giới hạn hiện tại → nhận nâng cấp → thử lại một cơ quan tương đương → kết hợp với luật đã biết. Phản ứng của cơ thể, cơ quan và phản hồi chỉ dẫn phải cho cùng một kết quả.

- Thay đổi đầu tiên phải nhìn thấy ngay ở tình huống thử sau nâng cấp. Người chơi chưa mở bảng chỉ số vẫn mô tả được sinh vật vừa làm tốt hơn điều gì.
- Mỗi màn giới thiệu tối đa một luật mới. Không đồng thời tăng độ rối bố cục và buộc người chơi hiểu một chỉ số mới.
- Màn bắt buộc chỉ yêu cầu năng lực đã được bảo đảm cấp trước đó. Nếu sau này cho tự phân điểm, phải có đường giải với mọi cách phân điểm hợp lệ hoặc cho điều chỉnh miễn phí tại màn. Chưa chốt kinh tế nâng cấp/trả phí.
- Chỉ số thấp vẫn nhận lệnh chính xác, nhớ kỹ năng cơ bản, dừng an toàn và có thể hoàn thành phần chơi bắt buộc. Khác biệt nằm ở hiệu quả, cách tiếp cận và mức tự thực hiện.
- Chỉ số cao mở cách làm hoặc giảm thao tác vụn. Các quyết định phân chia vật chất, chọn cơ quan, phân vai và thứ tự vẫn thuộc người chơi.

## 2. Chỉ số phải nối được với hành vi và vật lý

| Chỉ số | Thay đổi thực thi | Cơ quan thử | Biểu hiện trực quan | Ranh giới |
| --- | --- | --- | --- | --- |
| Nhanh nhẹn | Gia tốc tiếp tuyến, khả năng hãm và đổi hướng; tăng tốc tối đa có giới hạn | Đường cong chữ S, bệ dừng, cửa theo chu kỳ đã học | Thân dồn về trước khi khởi hành, rút đuôi khi rẽ, tới bệ nhanh và dừng gọn | Không đổi nhịp thời gian của thế giới; không xuyên collider; mức cơ bản không trượt lệnh |
| Sức mạnh | Lực kéo/đẩy chủ động và tải bám tối đa trên vùng tiếp xúc thật | Chốt lò xo, tay kéo có bản lề, vật cản có ma sát | Xúc tu bám, thân căng, lò xo giãn và kim cơ khí dịch chuyển; nâng cấp giúp kéo xa hơn với cùng bố trí | Khối lượng, trọng lực, độ cứng lò xo và ma sát cơ quan không đổi để giả tiến bộ |
| Trí tuệ | Học quy tắc nhanh hơn, ghi nhớ kỹ năng theo loại và ghép chuỗi thao tác trong việc được giao | Vùng chờ và cửa đóng/mở định kỳ | Mới học cần chỉ từng bước; thành thạo nhìn cửa, tự chờ rồi đi; các dấu nhiệm vụ thể hiện bước đang thực hiện | Không tự chọn dao, phân tải hay bỏ phần đang giữ nút; điều kiện hiện tại được kiểm tra lại |
| Cảm nhận | Phát hiện tín hiệu sớm hơn, định hướng nguồn và báo tín hiệu rõ hơn | Bệ rung báo trước một cơ quan chuyển động, bề mặt trơn có dấu hiệu nhìn thấy | Xúc tu chạm dò, quay đầu về nguồn rung, co thân/ra dấu trước khi cơ quan chuyển động | Chỉ nhận thông tin từ nguồn được thiết kế; không biết lời giải hoặc nhìn xuyên vật cản vô cớ |
| Gắn bó | Cử chỉ đáp lại, chào, chia sẻ thành công và nhìn người chơi khi cần giúp | Khoảng nghỉ, reunion, kết thúc thử thách | Cử chỉ quen thuộc xuất hiện đúng sự kiện; mỗi phần hướng về nhau rồi hướng về người chơi | Không làm tăng lực hay khối lượng, không mở cửa bằng ngưỡng tình cảm; không giảm vì nghỉ chơi |

Cảm xúc tức thời (tò mò, tập trung, dè chừng, vui) được tính từ tình huống; gắn bó là tiến trình dài hạn. Không ép cả hai thành một thanh điểm. Cảm nhận trả lời “đang xảy ra gì”, trí tuệ trả lời “với việc đã được giao thì làm bước nào”. Hai hệ phải có phản hồi riêng để người chơi phân biệt được nâng cấp.

Không đổi tất cả thông số liên quan cùng một lúc. Lần thử đầu chỉ chọn một khác biệt chính của mỗi chỉ số; các giá trị min/max được chốt sau đo và playtest. Nâng cấp không cộng vô hạn và không thay đổi số hạt vật lý.

## 3. Vật lý và điều kiện cơ quan

### Khối lượng khác sức mạnh

Tổng vật chất hiện tại là 96 g, chia từ 32 hạt. Phân tách/hợp thể phải bảo toàn tổng này. Một phần 30 g khỏe hơn vẫn là 30 g; không được kích hoạt cảm biến cần 60 g bằng cách tăng Strength.

Các nút Journey 04–05 hiện dùng tiếp xúc và lượng vật chất trong vùng nút, không phải phép đo trọng lượng tổng quát ở mọi góc hộp. Trong thiết kế mới, gọi rõ chúng là **cảm biến lượng vật chất**, dùng vòng vùng nhận và số gram để biểu đạt. Nếu làm cân thật, dùng lực theo trục mặt cân; khi xoay hộp, số đo phải đổi tương ứng và sự khác biệt này phải được dạy.

Chốt sức mạnh dùng lực theo trục kéo, lò xo, giảm chấn và giới hạn hành trình. Với trạng thái tĩnh đơn giản, lực giữ cân bằng lò xo là F = kx; khi chuyển động cần tính thêm giảm chấn, trọng lực và tiếp xúc. Latch chỉ khóa khi chốt thực sự tới hành trình yêu cầu. Không dùng điều kiện `Strength >= N` để tự mở cửa trước khi cơ cấu chuyển động đủ.

### Lực bám và chuyển động chủ động

Sinh vật là mô mềm có thể chủ động co và bám. Lực tác động cơ quan phải có đầu bám thật, giới hạn lực và phản lực lên cơ thể; không để xúc tu chỉ là hình ảnh kéo vật từ xa. Phần thân phía ngoài vẫn chịu trọng lực và võng xuống, kể cả khi Strength cao.

Sức mạnh của một phần phụ thuộc lượng mô và vùng tiếp xúc khả dụng. Tách hai phần không được nhân đôi tổng ngân sách lực; mỗi phần không nhận toàn bộ sức mạnh của cơ thể gốc. Kiến thức và gắn bó là của cùng một sinh vật, được dùng chung sau tách/hợp; nhiệm vụ vẫn riêng từng phần.

Nhanh nhẹn tăng đồng thời khả năng khởi hành và hãm theo giới hạn đã kiểm chứng. Khoảng hãm v²/(2a) chỉ là ước lượng cho gia tốc hãm không đổi, dùng để dự trù khoảng trống; kiểm tra vật lý thật vẫn quyết định khả năng dừng trên tường, góc và mặt trơn. Không tăng tốc vượt mức mà đường đi/collision có thể xử lý.

Cửa chu kỳ là cơ cấu có động lực hoặc truyền động được thể hiện rõ. Điều kiện thông đường lấy từ vị trí thực của cửa. Chỗ chờ phải an toàn, có thể đợi vòng sau, không nghiền hoặc xóa vật chất khi bỏ lỡ. Nâng cấp trí tuệ không làm cửa chạy chậm đi; phản ứng cảm nhận không làm thay đổi pha cửa.

## 4. Chuỗi thử nghiệm đầu và boss

Năm bài đang chạy giữ vai trò nền. Các dòng 06–10 là đề xuất phải dựng và kiểm chứng, không phải màn đã có. Trong giai đoạn test dùng profile chuẩn và nâng cấp theo kịch bản, chưa thêm phân điểm tự do.

| Màn | Mục tiêu | Khoảnh khắc chứng minh tiến bộ / quyết định còn lại |
| --- | --- | --- |
| 01–02 hiện tại | Chỉ đích, bám và đọc sáu mặt | Chuẩn cơ bản về nghe lời, lực bám và phản hồi; chưa giới thiệu điểm số |
| 03 hiện tại | Giữ nút và nhớ việc | Làm nền cho khái niệm ghi nhớ; kỹ năng đã biết không mất khi retry |
| 04 hiện tại | Phối hợp hai phần | Chuyển lựa chọn không hủy việc phần kia; chưa gán việc này cho chỉ số IQ |
| 05 hiện tại | Phân bổ 96 g cho hai cảm biến | Dạy rõ lượng vật chất, làm đối chứng khi Strength xuất hiện sau đó |
| 06 — Kéo được rồi | Giới thiệu sức mạnh | Kéo chốt lò xo trong khu thử: thấy giới hạn hành trình → nâng cấp → kéo tới chốt khóa. Lối tập luôn có thể rời; nhận nâng cấp trước đoạn bắt buộc. Cuối màn chọn nơi bám và hướng tiếp cận |
| 07 — Bước chân mềm | Giới thiệu nhanh nhẹn | Hai đoạn chữ S tương đương trước/sau nâng cấp; dồn thân, rẽ và hãm rõ hơn. Không dùng đồng hồ loại người chơi. Cuối màn tự chọn tuyến tiếp cận tay kéo đã học |
| 08 — Nghe chiếc hộp | Giới thiệu cảm nhận | Bệ rung nối với cơ cấu cửa đã nhìn rõ; sau nâng cấp sinh vật quay đầu và báo sớm hơn với cùng pha cửa. Người chơi vẫn chỉ điểm chờ/đích. Luôn có tín hiệu công khai và vùng chờ an toàn ở mức cơ bản |
| 09 — Mình nhớ rồi | Giới thiệu trí tuệ, khoảng nghỉ gắn bó | Dùng lại cửa và vùng chờ của 08: được dạy chuỗi, rồi tại cơ quan tương đương chỉ một đích là tự tiếp cận–chờ–đi. Người chơi vẫn chọn cửa/tuyến. Kết thúc có tương tác chào tự nguyện, không có khóa điểm tình cảm |
| 10 — Bông hoa thủy tinh, boss | Tổng hợp, không thêm luật mới | Hai phần phân tải, kéo chốt và chờ cửa theo nhiệm vụ; khi chốt trung tâm mở, các mặt hộp bung bằng bản lề. Giữ nguyên kiến thức nhưng người chơi phải đổi vai để thoát |

Boss 10 đề xuất dùng cấu trúc hai giai đoạn:

1. Chia khoảng 30/66 g bằng cơ chế cắt lệch đã học. Phần nhỏ giữ cảm biến A cần 24 g; phần lớn tới tay kéo. A cho phép tay kéo hoạt động, tay kéo tới hành trình sẽ chốt cầu và giải phóng nhiệm vụ A. Lực cần được hiệu chỉnh để phần lớn đạt ở Strength đã bảo đảm trước boss, với sai lệch khối lượng cắt hợp lệ.
2. Hộp bung thành cánh bằng bản lề có truyền động nhìn thấy được, để lộ hai tuyến. Người chơi đưa phần lớn tới cảm biến C cần 60 g để cho tuyến nhỏ hoạt động; phần nhỏ chờ cửa chu kỳ, qua và kéo chốt D. D giữ đường mở để giải phóng C; người chơi gọi cả hai tới lỗ. Không tự lập toàn bộ chuỗi này sau một lần chạm lỗ.

Cần kiểm chứng vùng mô không bị nghiền khi bung, các mặt được nối thật trong bộ tìm đường, có lối quay về sau mọi bước và tổng lực của phần nhỏ đủ kéo D. Checkpoint ở chốt giữa hai giai đoạn; không có timer toàn màn. Hiệu ứng boss tập trung vào cấu trúc đổi hình và hành vi phối hợp, không che mục tiêu bằng kính/lấp lánh dày đặc.

Đây là đường giải tham chiếu để dựng thử, chưa chứng minh hình học/độ khó. Nếu phiên bản tuyến tính này chưa đủ thú vị, bổ sung lựa chọn đổi vai có hệ quả sau playtest; không tăng khó bằng làm AI chậm hoặc không nghe lời.

## 5. Mẫu bắt buộc cho từng cơ quan và level

Mỗi cơ quan khai báo: loại cơ quan; điểm tiếp cận/bám; đại lượng tác động và đơn vị; giới hạn vật lý; điều kiện thành công; trạng thái thất bại; cách nhả/hủy; tín hiệu quan sát; kỹ năng được phép dùng; quan hệ mở/khóa với cơ quan khác. Tránh logic riêng theo số level.

Mỗi level ghi: năng lực tối thiểu; đúng một bài học mới nếu có; khác biệt trước/sau cần nhìn thấy; đường giải với profile tối thiểu; lợi ích của nâng cấp; quyết định không được AI tự lấy; phương án phục hồi; hành vi khi tách/hợp; hướng hộp hợp lệ; dữ liệu cần đo. Boss ghi thêm từng giai đoạn, checkpoint và đường giải phóng mọi phần.

Những cơ quan chưa có lời giải ở profile tối thiểu hoặc buộc tăng một nhánh tùy chọn để qua sẽ chưa được đưa vào tuyến bắt buộc.

## 6. Cách chứng minh thay đổi có giá trị

- So sánh cùng cơ quan, hình học, khối lượng, hướng hộp và điều kiện ban đầu, chỉ đổi một chỉ số. Khi thử trí tuệ, tách thử tốc độ học (cùng lịch sử trải nghiệm) và mức tự thực hiện (cùng kỹ năng đã biết); không nhập nhằng nâng cấp chỉ số với vừa được dạy một luật mới.
- Đo Strength bằng lực/hành trình thực; Agility bằng thời gian đến và sai số dừng; Perception bằng thời điểm báo so với sự kiện; Intelligence bằng số lệnh người chơi cần cho cùng nhiệm vụ và số hành động ngoài ý định; gắn bó bằng phản ứng đúng ngữ cảnh và đánh giá của người chơi, không gọi đó là cảm xúc thật.
- Cho người mới chơi và hỏi trung tính “lần này sinh vật làm khác gì?”. Nếu chỉ đọc số mới biết nâng cấp, sửa cơ quan/animation trước khi thêm nội dung. Không đặt ngưỡng thành công thống kê khi chưa có mẫu playtest.
- Kiểm tra min/max và cách phân bổ lệch; mọi profile hợp lệ đều có đường giải. Test chuyển nhiệm vụ, thiếu tải, mất bám, tách/hợp, pause/retry và đủ toàn bộ vật chất thoát. Cảm xúc không được làm gián đoạn thao tác cơ quan.
- Đo bản build trên nhóm điện thoại mục tiêu, gồm máy yếu nhất dự kiến và chơi kéo dài để thấy nhiệt. Không hạ trí thông minh hay đổi luật theo cấu hình; ưu tiên giảm chi tiết hình ảnh. 30/60 fps là điều kiện kiểm thử, không phải lời hứa hiệu năng hiện tại.
- Giới hạn việc tìm đường và độ dài chuỗi hành vi; kiến thức dùng chung cho các phần. Chỉ số không mở một phép tìm kiếm vô hạn hoặc buộc chạy AI mỗi bước vật lý. Phản hồi lệnh vẫn xuất hiện ngay, các bước suy xét nặng có ngân sách riêng.

## 7. Thứ tự triển khai

1. Tách skill, dữ liệu chỉ số, ký ức và nhiệm vụ khỏi nhánh logic theo số màn; giữ bộ kiểm tra năm Journey làm hồi quy.
2. Dựng khu thử cùng cơ quan trước/sau nâng cấp, bắt đầu bằng chốt sức mạnh và một đoạn đường nhanh nhẹn. Làm lực tương tác thật trước rồi gắn animation theo lực và hành trình.
3. Dựng cửa chu kỳ/vùng chờ, tín hiệu cảm nhận và kỹ năng có trí nhớ; đo số lệnh giảm mà quyền quyết định vẫn giữ nguyên.
4. Chỉ sau khi các khu thử cho khác biệt đọc được mới ghép thành 06–09 và boss 10; kiểm chứng trên điện thoại trước khi nhân thành campaign dài.

Chưa thay đổi code, scene, save hoặc build trong lần cập nhật đặc tả này. Các ngưỡng nâng cấp, lực kéo và phần thưởng cần được hiệu chỉnh trên prototype, không coi mô tả thiết kế là kết quả vật lý đã đạt.
