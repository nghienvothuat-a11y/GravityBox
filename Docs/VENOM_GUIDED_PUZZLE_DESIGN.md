# Venom — Dẫn đường cho một sinh vật đang lớn lên

Ngày thiết kế: 10/09/2026. Trạng thái: đề xuất sản phẩm dựa trên lựa chọn điều khiển màn 07 của người chơi; chưa triển khai các hệ thống và campaign bên dưới.

## 1. Trải nghiệm cần tạo ra

Người chơi quan sát chiếc hộp, hướng dẫn sinh vật giải quyết các cơ quan và đưa toàn bộ cơ thể ra ngoài. Sinh vật càng chơi càng biết làm nhiều việc, nhận ra những thứ quen thuộc và thể hiện sự gắn bó với người hướng dẫn.

Điều khiển giữ nguyên xuyên suốt: chạm để chỉ đích hoặc giao việc theo ngữ cảnh; kéo để xoay hộp. Chiều sâu đến từ lựa chọn phần cơ thể, mục tiêu, đường đi có hệ quả và thứ tự phối hợp. Cảm giác vật chất mềm, phản ứng sống động và khoảnh khắc hiểu ý nhau là phần thưởng thường xuyên.

Ba cam kết:

- Sinh vật tiếp nhận chỉ dẫn rõ ràng ngay từ đầu. Lớn lên không có nghĩa là những màn đầu cố tình điều khiển kém.
- Sinh vật tự thực hiện kỹ năng đã biết trong phạm vi việc được giao. Người chơi quyết định kế hoạch giải puzzle.
- Cơ quan chỉ thay đổi khi điều kiện gameplay/vật lý thực sự đạt. Animation thể hiện hành động và kết quả, không giả lập thành công.

Trong code hiện tại, màn 07–08 mới có đường đi trên sáu mặt hộp, trí nhớ tối đa 24 điểm theo từng màn và một cờ biết quan hệ nút–cửa. `VenomLifeAnimation` đã đọc chuyển động/tiếp xúc để tạo biến dạng, xúc tu và cử chỉ. Đây là nền thử nghiệm, chưa phải hệ học kỹ năng và cảm xúc mô tả ở tài liệu này.

## 2. Một ngôn ngữ điều khiển

| Thao tác | Ý nghĩa |
| --- | --- |
| Chạm mặt hộp | Phần đang chọn bò tới điểm đó rồi chờ |
| Chạm cơ quan | Xem mục tiêu được đánh dấu và giao hành động phù hợp: giữ nút, kéo chốt, đi vào vùng cắt… |
| Chạm một phần cơ thể | Chọn phần đó; các phần khác tiếp tục nhiệm vụ hiện tại |
| Kéo | Xoay hộp; không phát sinh lệnh di chuyển khi nhả |
| Chạm đích mới | Thay nhiệm vụ của phần đang chọn; hủy đúng các đăng ký chờ của nhiệm vụ cũ |
| Gọi về, chỉ hiện khi cần | Thu hồi những phần có thể rời vị trí; hiển thị rõ nếu việc rời nút sẽ đóng cửa |

Chạm cơ quan cần hiển thị ý định cụ thể, ví dụ vòng giữ quanh nút hoặc đường vào dao, trước khi cơ thể thực hiện phần không thể rút lại. Không thêm một hộp thoại xác nhận cho mỗi thao tác. Các cơ quan có nhiều cách dùng cần thiết kế các điểm tương tác phân biệt được trên màn hình.

Mỗi thời điểm phải nhìn rõ phần đang chọn, đích của nó và phần nào đang giữ nhiệm vụ. Điểm chạm trên mặt khuất không được âm thầm chọn xuyên qua nhiều mặt kính; cần làm nổi mặt nhận lệnh và xoay để nhìn vùng bị che. Cơ quan/nút nhỏ có vùng chạm lớn hơn hình hiển thị nhưng không chồng lấn mơ hồ.

Đề xuất cho campaign: sinh vật bám và giữ nhiệm vụ ổn định khi người chơi xoay để quan sát. Cơ quan chịu tác động trọng lực vẫn có thể thay đổi nếu đó là luật được giới thiệu rõ của màn. Camera/việc quan sát không được gây rơi hoặc cắt cơ thể ngoài dự đoán.

Tương tác vuốt ve, chạm xúc tu hoặc chào lại nên xuất hiện ở khoảng nghỉ hay sau khi thoát. Không chiếm cùng thao tác kéo đang dùng để xoay giữa puzzle.

## 3. Ranh giới giữa trí thông minh và lời giải

| Sinh vật được chủ động | Quyết định thuộc người chơi |
| --- | --- |
| Bò vòng một chướng ngại trong vùng đi lại đang thông | Chọn cửa hoặc nhánh đi có hậu quả khác nhau |
| Căn thân, luồn qua khe phù hợp với khả năng vật chất | Có cần cắt, cắt ở đâu và phân bổ bao nhiêu cơ thể |
| Giữ nút đã được giao và điều chỉnh tư thế cho đủ tải | Phần nào giữ nút nào; lúc nào cho phép rời nút |
| Chờ thời điểm an toàn của cơ quan có chu kỳ đã học | Chọn tuyến có cơ quan đó và phối hợp các nhiệm vụ |
| Nhận ra không thể đến, nhìn chướng ngại và báo lại | Mở đường bằng cơ quan nào, theo thứ tự nào |
| Tự căn và thoát khi đã tới vùng thoát hợp lệ | Đưa các phần tới đó, mở cửa và giải phóng những phần còn giữ nhiệm vụ |

Một đường đi được coi là tương đương khi không đổi trạng thái puzzle: không chạm nút ngoài nhiệm vụ, không đi qua dao, không tự nhập với phần đang giữ việc, không làm một nguồn lực rời vị trí. Bộ tìm đường phải xét các tác động này, không chỉ tìm đường ngắn nhất.

Khi một đích có đường đi qua vùng gây phân tách hoặc cơ quan chưa được giao, sinh vật dừng ở vùng tiếp cận và biểu đạt trở ngại. Người chơi có thể chạm đúng vùng tương tác để giao bước tiếp theo. Đường đi trong mê cung có lựa chọn chiến lược cần chia thành các đoạn có điểm quyết định nhìn thấy được; một lần chạm lỗ không tự chọn toàn bộ chuỗi lời giải.

Trí nhớ không chứa bảng lời giải của campaign. Tự mở tất cả cửa rồi thoát sau một lần chỉ lỗ chỉ phù hợp màn hướng dẫn hoặc lượt trình diễn đã được người chơi cho phép.

## 4. Các họ puzzle có thể phát triển lâu dài

| Họ puzzle | Quyết định cốt lõi | Những biến thể có ý nghĩa |
| --- | --- | --- |
| Phân bổ cơ thể | Bao nhiêu vật chất dành cho mỗi việc? | Tải nút khác nhau, chia lệch, hợp lại để đủ sức, mang một phần qua đường riêng |
| Giữ và giải phóng | Ai giữ lối cho ai; khi nào được rời? | Cửa cần giữ liên tục, chốt giữ vĩnh viễn, đổi vai hai phần, đường vòng về |
| Thứ tự và trạng thái | Việc nào phải xảy ra trước? | Đòn bẩy đảo đường, cửa một chiều, một hành động làm mất tuyến khác, khôi phục trạng thái |
| Không gian biến đổi | Sau khi đổi hình hộp, các vùng nối với nhau thế nào? | Cầu bản lề, mặt trượt, khoang xoay, vỏ bung thành cánh; điểm nối đều là hình học thật |
| Môi trường và cơ thể | Tuyến nào phù hợp trạng thái hiện tại? | Vùng trơn giảm bám, dòng khí đẩy, vùng làm đổi độ kết dính; mỗi tác động có luật rõ và màn dạy riêng |
| Phối hợp theo chu kỳ | Giao ai chờ ở đâu để hành động đúng nhịp? | Cửa đóng/mở, bệ nâng, quạt nghỉ theo chu kỳ; sinh vật xử lý thời điểm sau khi được giao tuyến |

Không coi đổi màu, tăng hành lang hoặc giấu nút sau nhiều kính là một dạng chiều sâu mới. Mỗi màn cần ít nhất một quyết định mà lựa chọn khác tạo ra hệ quả khác.

Riêng khe hẹp: chất lỏng có thể kéo dài qua khe, nên không dùng riêng chiều rộng khe để ngầm khẳng định cơ thể lớn bắt buộc phải cắt. Muốn cắt có ý nghĩa, dùng nhiệm vụ cần đồng thời ở hai nơi, giới hạn tải thật, buồng chuyển vật chất có dung tích hữu hạn hoặc quy tắc vật chất có giới hạn kéo giãn đã được dạy và kiểm chứng. Không thay collider một cách vô hình để ép lời giải.

Các phần chưa có nhiệm vụ có thể tự nhìn nhau hoặc vươn xúc tu. Chúng không tự nhập nếu điều đó làm mất một nhiệm vụ người chơi đã bố trí.

## 5. Sinh vật lớn lên bằng những năng lực quan sát được

| Giai đoạn | Năng lực mới | Cách người chơi nhận ra | Thử thách tiếp theo |
| --- | --- | --- | --- |
| Làm quen | Nhìn điểm chỉ, đi tới, chờ | Quay đầu theo chạm; tới nơi nhìn lại người hướng dẫn | Chọn mặt và đường tiếp cận |
| Hiểu một hành động | Giữ nút, căn thân, luồn khe | Tự giữ tư thế sau khi được giao việc | Chọn đúng nút và điều kiện cần giữ |
| Học phối hợp | Hai phần giữ hai nhiệm vụ độc lập | Phần nhỏ chờ; phần lớn hướng xúc tu về nó khi thông đường | Đổi vai, chia tải, tránh nhập sớm |
| Nhớ quy tắc | Nhận ra cùng loại cơ quan ở hình dạng/vị trí mới | Thăm dò ngắn hơn; nhìn phản ứng của cửa sau khi tác động | Kết hợp quy tắc quen trong bố cục mới |
| Chuẩn bị và chờ | Biết đứng ở điểm chờ của nhiệm vụ được giao | Quan sát chuyển động và tự căn thời điểm đi | Phối hợp nhiều bước có chu kỳ |
| Chủ động giúp trong phạm vi cho phép | Thực hiện chuỗi con đã được dạy, chờ ở ranh giới quyết định | Làm việc quen rồi quay lại chờ chỉ dẫn tiếp | Kế hoạch dài hơn, nhiều ràng buộc hơn |

Level tạo cơ hội học; sự kiện thực hiện thành công mới ghi nhận kiến thức. Nếu người chơi đi đường khác, level giới thiệu tiếp theo phải kiểm tra và dạy bù kỹ năng bắt buộc.

Tách bốn loại dữ liệu:

1. **Khả năng đã học:** quy tắc và kỹ năng theo loại cơ quan, dùng được qua các màn.
2. **Trạng thái màn hiện tại:** cửa nào mở, phần nào giữ nút, đường nào bị chặn; cập nhật bằng quan sát thực tế.
3. **Ký ức trải nghiệm:** các điểm/việc từng hoàn thành, có phiên bản màn và điều kiện áp dụng; không phát lại tọa độ cũ vào một hình học mới.
4. **Mức thân quen:** lịch sử những lần cùng vượt thử thách và tương tác tự nguyện; không điều khiển tính đúng đắn của đường đi.

Kiến thức lâu dài không bị xóa khi retry. Trạng thái cơ quan và nhiệm vụ được reset. Ký ức lượt cũ có thể dùng làm gợi ý hoặc phát lại khi người chơi yêu cầu, nhưng phải kiểm tra lại điều kiện hiện tại. Người chơi quay lại màn cũ với sinh vật đã giỏi hơn vẫn giữ các quyết định chiến lược của màn.

Không cần mô hình ngôn ngữ trực tuyến để xây dựng giai đoạn này. Đề xuất dùng kỹ năng có điều kiện, trí nhớ sự kiện và lựa chọn hành vi có giới hạn, chạy trên thiết bị. Đây là AI gameplay; chưa phải sinh vật tự học mọi quy luật tùy ý.

## 6. Cảm xúc phải phản ứng với sự kiện

Cảm xúc ngắn hạn gồm tò mò, dè chừng, tập trung, nhẹ nhõm và vui. Mức thân quen phát triển dài hạn. Những trạng thái này thay đổi tư thế, ánh nhìn, chất giọng và cử chỉ, không làm nó ngẫu nhiên bỏ lệnh hợp lệ.

| Tình huống | Khi mới gặp người chơi | Khi đã thân |
| --- | --- | --- |
| Nhận điểm chỉ | Nhô đầu dò hướng, đưa một xúc tu ra | Nghiêng đầu đáp lại rồi bò, động tác tự tin hơn |
| Gặp cơ quan lạ | Thăm dò, nhìn cơ quan rồi nhìn điểm chỉ | Chủ động thử động tác đã biết trong nhiệm vụ; báo lại nếu luật mới |
| Một phần được giải phóng | Rung nhẹ, tìm phần còn lại | Vươn về phần kia rồi hướng về người chơi trước khi hợp thể |
| Vượt thử thách khó | Thả lỏng thân | Một cử chỉ ăn mừng quen thuộc, biến thể theo những trải nghiệm chung |
| Người chơi quay lại | Nhận ra điểm tương tác | Chào bằng xúc tu, áp gần mặt kính; không cần thông báo ép quay lại |

Không giảm sự gắn bó vì người chơi nghỉ vài ngày, retry hoặc chưa giải được puzzle. Không yêu cầu vuốt ve để sinh vật chịu làm việc. Khi thất bại, phản ứng là bối rối/thử lại và chờ hướng dẫn, tránh trách móc người chơi.

Muốn cảm giác được thấu hiểu, ưu tiên một phản ứng đúng lúc hơn nhiều animation ngẫu nhiên. Các tín hiệu như quay đầu từ nút sang cửa vừa mở giúp người chơi nhìn thấy cả sự hiểu biết lẫn quan hệ nhân quả.

## 7. Hệ animation và tương tác

Thiết kế thư viện hành động theo ngữ cảnh, không làm một clip riêng cho từng level. Mỗi hành động gồm chú ý → chuẩn bị → tiếp xúc/thực hiện → phản ứng kết quả; có biến thể theo kích thước phần cơ thể, mặt bám, chuyển động và cảm xúc.

| Nhóm | Các hành động nền |
| --- | --- |
| Với người chơi | Nhìn điểm chạm, xác nhận, nhìn lại khi cần giúp, chào, chạm xúc tu ở khoảng nghỉ |
| Di chuyển | Dồn thân khởi hành, bám kéo, quấn mép, hãm, xoay đầu, vượt góc, mất bám và lấy lại bám |
| Cơ quan | Thăm dò, ấn và giữ nút, tì thân đẩy, quấn xúc tu kéo, nhìn cửa phản ứng |
| Cơ thể | Chuẩn bị qua dao, tách đàn hồi, nhận ra phần còn lại, luồn khe, chạm và nhập lại |
| Môi trường | Né và thu xúc tu, nghiêng chống dòng khí, trượt, co lại rồi hồi phục, quan sát vật chuyển động |
| Cảm xúc và hoàn tất | Tò mò, tập trung, dè chừng, nhẹ nhõm, cử chỉ thân quen, căn lỗ và thoát |

Thứ tự ưu tiên: phản ứng tiếp xúc/vật lý cần thiết → hành động đang thực hiện → phản hồi lệnh mới → phản ứng cảm xúc → idle. Hành động cảm xúc không chen vào khi đang luồn khe, giữ nút hoặc đi qua cơ quan có nhịp.

Phản hồi nhận lệnh phải xuất hiện ngay; clip chuẩn bị không bắt người chơi chờ hết mới có thể đổi lệnh. Hủy hành động phải giải phóng quyền giữ cơ quan và các callback cũ. Không để animation hoàn tất muộn mở cửa sau khi cơ thể đã rời nút.

`VenomLifeAnimation` hiện chỉ biểu diễn, không sửa hạt/collider/cơ quan; giữ ranh giới này. Với hành động kéo/đẩy thực sự, một bộ thực thi tương tác riêng điều khiển lực hoặc ràng buộc có giới hạn, đọc điểm bám thật. Animation đi theo tiến độ và lực đó. Xúc tu hình ảnh đơn thuần không được làm công tắc hoạt động từ xa.

Mỗi hành động cần có kết quả nhìn thấy được: thành công, thiếu tải, đường bị chặn, mất bám hoặc bị người chơi thay lệnh. Vùng chạm phát sáng nhẹ và âm thanh ngắn, mềm; âm lặp theo nhịp bò cần giới hạn mật độ để không gây khó chịu. Cử chỉ dài có khoảng nghỉ và có thể bị cắt ngắn; không cố làm sinh vật hoạt động lớn liên tục.

Ưu tiên sản xuất: hoàn thiện bộ nhận lệnh, báo trở ngại, giữ/nhả nút, tách/hợp, nhìn lại và thoát trước; sau đó mở rộng biến thể cảm xúc và môi trường. Chưa ấn định số clip lớn trước khi kiểm chứng sự liên quan của chúng với gameplay.

## 8. Kiến trúc phát triển từ prototype

Luồng chính: chỉ dẫn → ý định của một phần → kỹ năng có điều kiện → đường đi/tương tác → vật lý → quan sát kết quả → trí nhớ và biểu cảm.

| Thành phần đề xuất | Trách nhiệm | Liên hệ hiện tại |
| --- | --- | --- |
| Guidance input | Tap/drag, chọn phần, mặt nhận lệnh, phản hồi mục tiêu | Mở rộng `VenomInput` |
| Intent controller | Một nhiệm vụ bền vững mỗi phần, hủy/thay/gọi về | Tách trách nhiệm khỏi `VenomGuidance` |
| Skill executor | Tiền điều kiện, chạy, bị chặn, thành công, bị hủy | Trích logic giữ nút/tự thoát thành kỹ năng tái sử dụng |
| World observation + route planner | Vùng đi được, cơ quan động, cạnh có hậu quả, tính lại đường | Mở rộng `VenomSurfaceRoute`; sáu mặt hiện tại chưa đủ cho mê cung tổng quát |
| Interaction executor | Tiếp xúc, lực, tải và quyền sử dụng cơ quan | Phối hợp locomotion, organism và cơ quan hiện có |
| Knowledge + memory | Quy tắc theo loại, dữ kiện màn, ký ức và phiên bản lưu | Thay dần cấu trúc điểm/cờ trong `VenomGuidanceMemory` |
| Emotion + presentation | Đánh giá sự kiện, trạng thái thân quen, chọn và trộn hành động | Cung cấp ngữ cảnh cho `VenomLifeAnimation` |
| Level definition + validation | Cơ quan, điều kiện, kỹ năng cần có, lời giải kiểm chứng, checkpoint | Bổ sung dữ liệu dùng lại; không nhồi thêm nhánh theo số level |

Mỗi tương tác khai báo điều kiện tải/tiếp xúc, tác động, điều kiện kết thúc, quyền ngắt và mối liên hệ với cơ quan khác. Dữ liệu phải dùng ID ổn định của cơ quan/phần; tách/hợp cần chuyển giao hoặc hủy nhiệm vụ theo quy tắc rõ, không bám vào index tạm thời.

Tách kỹ năng dùng lại khỏi bố cục riêng của màn. Điều kiện puzzle được kiểm tra bằng trạng thái cơ quan và vật chất thực, không bằng việc phát xong animation. Mọi phần còn vật chất đều được theo dõi tới cửa thoát; không coi chủ thể thoát là toàn bộ đã thắng.

Bộ lập kế hoạch chỉ đi trong phạm vi nhiệm vụ được giao. Không dùng bộ giải toàn màn làm AI của sinh vật. Bộ giải toàn màn có thể phục vụ công cụ tác giả để phát hiện trạng thái bế tắc và đường tắt.

Lựa chọn này chưa đòi hỏi thay engine animation hay thêm dịch vụ AI. Cần đo hiệu năng trên điện thoại mục tiêu trước khi quyết định độ dày mesh, số xúc tu, tần suất cập nhật nhận thức và số tác vụ tính đường đồng thời.

## 9. Thiết kế độ khó có thể kiểm chứng

Ghi một hồ sơ cho từng level thay vì chỉ gán số độ khó:

- Số luật mới phải hiểu.
- Số quyết định có hệ quả và độ dài chuỗi phụ thuộc giữa chúng.
- Số phần/nhiệm vụ phải giữ đồng thời.
- Số tuyến hoặc trạng thái phải phân biệt; mức độ nhìn rõ các quan hệ.
- Độ chính xác thời điểm; người chơi hay sinh vật chịu trách nhiệm căn nhịp.
- Chi phí sửa sai: có thể đảo hành động, gọi về, reset một cơ quan hoặc dùng checkpoint không.

Đây là các trục để so sánh và chẩn đoán; chưa có trọng số thực nghiệm để cộng thành một điểm chính xác. Đường đi dài, nhiều vật cản hoặc thời gian bò lâu không mặc nhiên là puzzle khó.

Mỗi cụm 10 màn: giới thiệu luật → luyện → kết hợp → tình huống đảo vai → nhịp nhẹ → boss tổng hợp. Nhịp cụ thể có thể đổi theo playtest. Boss đầu tiên chỉ kiểm tra những gì đã học; độ khó rất cao dành cho các chương sau hoặc biến thể thử thách tùy chọn. Boss vẫn dùng chạm/kéo, không giới thiệu joystick hay luật mới ở giữa màn.

Màn rất khó tăng sự phụ thuộc giữa các quyết định, phân bổ vật chất và thay đổi kết nối. Không tăng khó bằng việc AI ngẫu nhiên không nghe lời, dấu mục tiêu không rõ hoặc yêu cầu chạm cực chính xác. Có checkpoint theo giai đoạn đối với boss dài.

## 10. Chương thử nghiệm 10 màn đề xuất

Đây là số thứ tự campaign thiết kế mới, không đổi số hay ghi đè các scene thử nghiệm 01–08 hiện có.

| Màn | Bài học/thử thách | Biểu hiện sinh vật |
| --- | --- | --- |
| 1 | Chỉ đích, tới gần lỗ, tự thoát | Nhìn điểm chỉ và đáp lại khi đến |
| 2 | Xoay để thấy một mặt khác, vượt mép tường | Dò góc rồi bám qua mép |
| 3 | Giữ nút đủ một nhịp để chốt cửa mở; nhả sớm thì cửa đóng, có vùng thử an toàn | Dồn thân giữ, nhìn cửa thay đổi rồi rời nút khi đã chốt |
| 4 | Cắt thành hai phần để một phần giữ và một phần đi qua | Hai phần nhận biết nhau, chờ được chọn |
| 5 | Hai tải khác nhau, chọn vị trí cắt và phân vai | Phần thiếu tải tì thử rồi nhìn lại, không giả báo thành công |
| 6 | Phần qua cửa gạt chốt để giải phóng phần giữ | Phần được giải phóng đáp lại, hợp thể khi được gọi |
| 7 | Học chờ một cửa chạy theo chu kỳ với vùng đứng an toàn | Quan sát, chờ rồi tự đi đúng nhịp trong tuyến đã giao |
| 8 | Cơ quan quen ở hình dạng và vị trí mới; thứ tự khác | Nhận ra cách dùng nhưng chờ người chơi chọn nhiệm vụ |
| 9 | Một lời giải ngắn dùng kỹ năng quen, ít áp lực | Khoảnh khắc phối hợp trôi chảy và cử chỉ thân quen |
| 10 — Boss | Hộp bung thành các cánh, đổi vai hai phần qua hai giai đoạn | Giữ nhiệm vụ khi hình học đổi, nhìn người chơi giữa các giai đoạn, cùng thoát |

Boss 10, ví dụ nguyên tắc để dựng và kiểm chứng: tổng khối lượng hiện tại 96 g, thử chia 36/60 g. Phần nhỏ giữ A cần ít nhất 25 g để mở cầu; phần lớn qua cầu và nhấn B cần ít nhất 50 g. B chốt cầu ở trạng thái mở, giải phóng nhiệm vụ A và triển khai các cánh hộp. Sau đó người chơi dẫn phần nhỏ đến C để mở tuyến cho phần lớn tới D; D chốt cửa cuối. C có thể được giải phóng, hai phần được gọi về và thoát.

Các giá trị này là điểm khởi đầu thử nghiệm, chưa là cấu hình vật lý đã kiểm tra. Tải đo trên cơ quan cần dựa tiếp xúc thực và hướng lực được thiết kế, tránh việc xoay hộp làm sai ý nghĩa tải. Hình học biến đổi phải giữ các bề mặt đỡ, không nghiền/đánh rơi phần đang giữ. Cần chứng minh còn đường gọi về sau từng bước và đủ vật chất qua lỗ cuối.

Điểm bất ngờ là chiếc hộp đổi cấu trúc và hai phần phối hợp những việc đã học. Không cần thêm một thao tác điều khiển mới để tạo cao trào. Chưa thể khẳng định ví dụ tuyến tính này đủ khó cho người chơi thành thạo; bản thử sẽ cho biết cần thêm lựa chọn trạng thái hay đổi vai ở đâu.

## 11. Phạm vi triển khai đầu tiên

Trước khi sản xuất một campaign dài, tạo một nhóm sáu bài kiểm chứng: dẫn đường và phản hồi; giữ/nhả nút; tách và giao hai việc; thiếu tải và thử lại; dùng kỹ năng quen trong bố cục mới; boss nhỏ đổi vai với phần thưởng cảm xúc. Có thể tái sử dụng bài 07–08 làm nền, giữ scene thử nghiệm cũ để đối chiếu.

Thứ tự công việc:

1. Chốt chọn mặt/chọn phần và ý định bền vững, đặc biệt giữ nút và thay lệnh.
2. Làm route planner hiểu vật cản và các ranh giới có tác động puzzle.
3. Hoàn thiện tương tác vật lý giữ/nhả, chia tải, chốt và gọi về.
4. Thêm kỹ năng được học từ sự kiện thật và lưu qua level; tách trí nhớ phiên chơi.
5. Gắn phản hồi nhận lệnh, không tới được, học được, được giải phóng và hợp thể vào animation.
6. Dựng sáu bài, thử trên điện thoại rồi mới mở rộng thư viện cơ quan và biểu cảm.

Không lấy việc hoàn thành vài test vật lý hoặc có nhiều animation làm bằng chứng đã có gameplay chiều sâu.

## 12. Kiểm chứng trước khi mở rộng

Kiểm tra tự động cần tập trung vào hợp đồng hành vi: giữ nút không tự bỏ; gọi về có hậu quả đúng; đổi lệnh không còn callback cũ; tách/hợp giữ đúng danh tính nhiệm vụ; kiến thức không tự mở cơ quan ngoài lệnh; reset giữ kiến thức nhưng xóa trạng thái màn; mọi phần thực sự thoát mới thắng.

Với mỗi màn, có chuỗi lời giải, các thao tác sai tiêu biểu, trạng thái bế tắc và cách phục hồi. Kiểm chứng sơ đồ logic trước, sau đó chạy lại với solver vật lý thật ở các hướng hộp và sai lệch đầu vào hợp lý. Không giả định replay vật lý hoàn toàn giống nhau giữa thiết bị.

Playtest cần ghi: thời gian hiểu một luật mới; số lệnh nhầm mặt/phần; lần sinh vật làm ngoài ý định; quyết định người chơi thực sự đưa ra; lý do retry; cảm nhận sinh vật có hiểu mình không; mong muốn chơi tiếp. Dùng quan sát và câu hỏi trung tính, không chỉ hỏi họ có thấy sinh vật dễ thương không.

Thử cùng một bố cục với sinh vật mới học và đã thành thạo. Nếu bản thành thạo tự loại bỏ quyết định chính, thu hẹp quyền tự hành hoặc thiết kế lại puzzle. Nếu người chơi hiểu lời giải nhưng mất nhiều thời gian vì sinh vật kẹt, đó là vấn đề thực thi, không ghi nhận như độ khó tốt.

## 13. Tham khảo và giới hạn suy luận

Supersonic mô tả các puzzle của EOGAMES có vòng chơi đơn giản, quyết định chiến lược, khả năng nhìn rõ lớp bên trong và phản hồi vật lý/âm thanh phù hợp cảm giác game. Đây là cơ sở tham khảo cho nguyên tắc dễ hiểu và phản hồi rõ, không phải bằng chứng chế độ chạm đích sẽ đạt retention cụ thể. [Bài phân tích UX, 18/11/2025](https://supersonic.com/learn/blog/how-ux-design-drives-player-engagement-in-eogames-hit-hybrid-puzzles/).

genDESIGN mô tả Projection Trico phản ứng theo người xem và đồ vật được đưa ra, với nhiều phản ứng trực tiếp. Tham khảo cách liên kết kích thích → phản ứng nhân vật để tạo cảm giác tương tác; không suy diễn rằng dự án mình cần cùng quy mô AI hoặc có thể đạt kết quả gắn bó tương đương. [Trao đổi với đội ngũ The Last Guardian, phần 1](https://gendesign.co.jp/qa_01/interview01en.html).

Các cấu trúc puzzle, nhịp học và kiến trúc trong tài liệu là đề xuất cho Venom, cần kiểm chứng bằng bản chơi. Không có cam kết viral hay số liệu retention suy ra từ các nguồn trên.
