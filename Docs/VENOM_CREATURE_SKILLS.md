# Venom — Tám kỹ năng nền của sinh vật

Ngày concept: 14/09/2026; cập nhật 15/09/2026 theo [mười màn đầu](VENOM_CAMPAIGN_01_10.md). Danh sách là hướng thiết kế; các luật chi tiết được ghi là đề xuất, chưa đồng nghĩa đã triển khai hoặc đã cân bằng. Campaign tiếp tục là giải đố thuần, không phân điểm và không nhận buff từ đồ mua ở nhà. **Luật mới yêu cầu hợp thể trước khi thoát; Boss 10 tự khám phá, không tutorial; thắng mở Collection “Nhà của sinh vật”.**

## 1. Nguyên tắc chung

Mở dần kỹ năng → luyện một kỹ năng → kết hợp hai kỹ năng → đổi vai và thứ tự → thử thách tổng hợp. Độ khó đến từ quyết định của người chơi và quan hệ giữa cơ quan, không từ việc sinh vật ngẫu nhiên không nghe lời.

Năng lực vật lý được cấu hình chung. Khối lượng của phần cơ thể là tài nguyên có thể phân bổ trong màn: phần lớn kéo/đẩy mạnh hơn, phần nhỏ phù hợp những nhiệm vụ ở không gian hạn chế. Tụ lại khôi phục năng lực của một bản thể đầy đủ. Đây là hệ quả của lượng vật chất trong màn, không phải một chỉ số nâng cấp lâu dài.

Các trạng thái vật lý như rơi, va chạm, võng, biến dạng thụ động luôn tồn tại. “Học kỹ năng” mở khả năng nhận và thực thi một hành động có chủ đích; không tắt va chạm hoặc làm sinh vật cứng giả trước màn dạy biến dạng.

## 2. Danh mục và hợp đồng hành vi

| Kỹ năng | Ý định người chơi | Sinh vật tự thực hiện trong nhiệm vụ | Kết quả và phản hồi |
| --- | --- | --- | --- |
| Bò | Chạm vị trí, vùng tiếp cận cơ quan hoặc lỗ thoát | Tìm đường hợp lệ, khởi hành, hãm và dừng; tới vùng thoát hợp lệ thì căn qua lỗ | Dồn thân theo hướng đi, tới nơi nhìn lại; không có đường thì báo trở ngại |
| Leo trèo | Chỉ điểm trên mặt có thể bám | Tiếp cận mép, bám, kéo thân qua góc và tiếp tục trên vách/nóc | Xúc tu bám trước, thân/đuôi theo sau; bụng vẫn rủ theo trọng lực thế giới |
| Biến dạng luồn lách | Chỉ đích qua khe/ống phù hợp | Dò miệng vào, kéo dài/thu tiết diện, giữ liên tục vật chất và hồi hình sau khi qua | Nhìn thấy thân ép mỏng, kéo thành dòng; không đủ khả năng thì dừng/rút ra, không mất vật chất |
| Đẩy | Chọn vật có thể đẩy và hướng/đích tác động | Chọn vùng tì, bám chống, truyền lực tiếp xúc có giới hạn | Vật dịch chuyển/khớp quay thật; thân ép và căng theo tải; thiếu lực thì tì thử rồi báo |
| Kéo | Chọn đồ vật có thể bám/tay nắm/cần gạt/chốt và hướng/đích | Tới điểm bám, quấn/nối tiếp xúc, kéo và nhả theo nhiệm vụ; hộp 07 kéo được khỏi kính để đặt lại | Xúc tu căng theo lực; vật/chốt phải chuyển động đủ hành trình mới kích hoạt |
| Phân tách | Dẫn cơ thể qua cơ quan cắt, chọn vị trí/cách tiếp cận để phân lượng mô | Liên kết bị cắt khi lưỡi dao thực sự giao với cơ thể; các phần có danh tính/nhiệm vụ riêng | Thấy phần nào đang chọn, khối lượng và việc đang giữ; không sinh/mất vật chất; không tự tách theo nút lệnh |
| Tụ lại | Dẫn các phần đến gần nhau; có thể dùng gọi về để cùng tới một điểm | Khi mô của các phần đủ gần và thỏa điều kiện kết dính hiện có, tự nhập; không cần lệnh kích hoạt tụ riêng | Có cầu mô/kết dính nhìn thấy; lực tối đa của bản thể hồi theo khối lượng thực đã nhập |
| Copy vật thể | Chọn vật mẫu hợp lệ, rồi chỉ nơi cần sử dụng | Quan sát/tiếp xúc để học mẫu, biến dạng phần được chọn và thực hiện chức năng hình học phù hợp | Hình mẫu rõ, sinh vật tự trở thành vật thể; không tạo một vật mới độc lập hoặc giả báo mở khóa |

Giữ nút, chờ cửa, chọn phần và ghi nhớ là hành vi hỗ trợ các kỹ năng trên; chưa thêm chúng thành bốn kỹ năng mới cần mua/mở riêng.

## 3. Ngôn ngữ điều khiển

Giữ chạm để chỉ dẫn và kéo để xoay hộp. Bò, leo và luồn có thể chuyển tiếp theo đường đi sau khi được học. Đẩy/kéo/copy cần ý định rõ khi cùng đồ vật có nhiều cách dùng: đề xuất chạm đồ → hiện các điểm/hành động phù hợp → chọn mục tiêu, không dùng kéo ngón tay vì đã dành cho xoay hộp.

Màn 07 đã chốt cụ thể: chạm hộp để bám, sau đó điểm chạm là đích đưa hộp tới; sinh vật đẩy/kéo theo hướng đó. Sau 3 giây không điều khiển buông hộp về Idle, có thể chọn lại. Đây không phải thời hạn tự bỏ mọi nhiệm vụ; giữ nút ở Boss vẫn bền vững khi đổi phần. Màn 07–08 khóa xoay theo bản vẽ. Màn 08 yêu cầu người chơi căn rơi bắt vành, AI không tự chọn toàn bộ chuỗi hoặc vươn từ nóc thay cú rơi.

Các phần được chọn riêng, phần khác tiếp tục nhiệm vụ. “Điều khiển đồng thời” là nhiều nhiệm vụ cùng tồn tại, không yêu cầu giữ nhiều ngón để lái nhiều phần. AI không tự chọn đi qua dao, tự gọi các phần bỏ nhiệm vụ để tìm nhau hoặc tự copy ngoài ý định. Khi các phần đã đủ gần, tụ là phản ứng kết dính tự động theo điều kiện hiện có, không phải một quyết định mới cần người chơi bấm nút.

Các lệnh có thể hủy phải giải phóng điểm bám, lực điều khiển, chờ sự kiện và quyền giữ cơ quan. Nếu một hành động tạm thời không thể ngắt, phải thể hiện thời điểm đó và có thời hạn ngắn, hữu hạn. Không để animation kết thúc muộn kích hoạt vật thể sau khi đã đổi lệnh.

## 4. Đẩy/kéo phụ thuộc khối lượng

Yêu cầu người dùng: khối lượng lớn cho lực lớn hơn, lực lớn nhất ở bản thể có toàn bộ vật chất. Đề xuất mô hình điều khiển: lực chủ động tối đa của phần i là `F_motor,i = a_tissue × m_i`, với `a_tissue` là thông số thiết kế chung, đơn vị m/s²; `m_i` là khối lượng thực của phần đó. Đây là khả năng co chủ động được mô hình hóa cho sinh vật, không phải kết luận vật lý rằng vật nặng tự có khả năng đẩy mạnh.

Lực sử dụng thực còn bị giới hạn bởi điểm bám, sức chịu của liên kết và hình học cơ quan. Phản lực phải tác động lên cơ thể; đẩy vật nặng từ tư thế không có chỗ chống có thể làm sinh vật trượt thay vì vật tự dịch chuyển. Lực lên vật có vị trí tác dụng thật nên có thể tạo mô-men quay. Giữ trọng lực, ma sát, khối lượng vật và độ cứng lò xo nhất quán.

Tổng ngân sách lực chủ động các phần không vượt bản thể đầy đủ: `ΣF_motor,i = a_tissue × Σm_i = a_tissue × M`. Hai phần không nhận mỗi phần toàn bộ lực của cơ thể gốc. Khi cùng tác động một vật, xét hướng và mô-men thật; hai phần ở hai vị trí có thể tạo lợi thế hình học dù tổng lực không lớn hơn bản thể. Không cần cấm lợi thế vật lý hợp lệ này để ép tụ lại.

Muốn một puzzle cần tụ lại, thiết kế một điểm tiếp cận/điểm kéo hữu hạn, yêu cầu tải tập trung hoặc đường tiếp cận mà nhiều phần không thể đồng thời tác dụng đủ theo cùng hướng. Một bản thể đủ mô có thể bám và phát lực tại đó. Không cho cơ quan tự kiểm tra “đã nhập” rồi mở bất kể lực.

Điểm tham chiếu hiện tại: 32 hạt × 3 g = 96 g; 48 g có ngân sách lực chủ động bằng một nửa 96 g nếu cùng hệ số. Các giá trị lực cụ thể và biên an toàn cần đo; không chốt ngưỡng cơ quan sát đúng một tỷ lệ cắt giả định vì khối lượng các phần có thể lệch vài hạt.

## 5. Luồn khe/ống và giới hạn khối lượng

Yêu cầu người dùng: có giới hạn khối lượng có thể chui qua. Cần làm giới hạn này quan sát được và có nguyên nhân nhất quán. Một chất lỏng chảy qua một khe mở có thể lần lượt đưa nhiều vật chất qua; chỉ đường kính khe thường không tạo ra một ngưỡng tổng khối lượng cho cả phần cơ thể.

Hai cách thiết kế đề xuất, phải chọn theo loại cơ quan và dạy rõ:

1. **Khoang chuyển hữu hạn:** cả phần phải nằm trong khoang thì cửa đầu vào mới đóng và cửa đầu ra mới mở. Thể tích hữu dụng giới hạn phần có thể chuyển: với mật độ gần như cố định, `m_max = ρ × V_usable`. Có thể cho thấy phần lớn tràn ngoài miệng và cửa chưa đóng được; sau chia, phần nhỏ nằm trọn và qua được. Miệng vào, cửa, vật cản và khoảng hở đều là hình học thật. Đây là cơ quan có dung tích, không phải một ống mở thông hai đầu.
2. **Giới hạn biến dạng của mô sinh vật:** không coi nó là chất lỏng lý tưởng; có độ dài kéo giãn, tiết diện nhỏ nhất và khả năng duy trì liên kết hữu hạn. Phải mô phỏng và kiểm chứng đường đi cụ thể, cho thấy cơ thể căng tới giới hạn và rút lại. Chỉ đặt giới hạn khối lượng khi hình học thực buộc đủ phần cơ thể vào cấu hình giới hạn đó; không suy ra từ đường kính một khe ngắn.

Trường hợp chỉ muốn giới thiệu khả năng luồn: dùng khe đủ cho toàn bộ cơ thể đi qua, chưa thêm điều kiện phải chia. Chỉ giới thiệu cơ quan có giới hạn lượng vật chất sau khi phân tách và tụ lại đã được dạy. Không biến một animation kéo dài thành bằng chứng rằng vật lý đã cho vật chất qua.

## 6. Phân tách và tụ lại

**Đã được người dùng chốt: chỉ phân tách khi đi qua cơ quan cắt.** Không có kỹ năng tự tách tại vị trí bất kỳ. “Học phân tách” là biết tiếp cận và sử dụng cơ quan cắt, nhận biết các phần sau cắt và chờ phân công; AI không tự tìm dao khi người chơi chưa giao ý định đó.

**Đã được người dùng chốt: các phần ở gần nhau tự tụ lại, giữ nguyên nguyên tắc hiện có.** “Học tụ” là người chơi/sinh vật nhận biết và tận dụng sự kết dính; không mở một nút kích hoạt khả năng mới. Có thể chỉ dẫn các phần về gần nhau bằng lệnh di chuyển thông thường. Nút gọi về chỉ thay nhiệm vụ di chuyển để đưa các phần tới điểm gặp, không trực tiếp nhập mô và không bắt buộc để tụ.

Đối chiếu runtime hiện có: xét khoảng cách giữa các hạt mô, có thời gian hồi sau cắt và không nối xuyên vật cản. Journey còn bảo vệ phần đang giữ nút/chờ dao hoặc có nhiệm vụ không tương thích; hai phần tự do hoặc được giao cùng vùng đích có thể kết dính khi đủ gần. Giữ các điều kiện này trong lần làm rõ thiết kế, không thay bằng hút từ xa hoặc bỏ bảo vệ nhiệm vụ.

Vị trí dao, đường tới dao và vị trí có thể tụ lại là cấu trúc puzzle. Một màn cần phần nhỏ ở phía sau khe phải cho tiếp cận cơ quan cắt trước khi bị chặn; một màn cần cắt lại sau khi tụ phải có đường tới dao hoặc cơ quan cắt tiếp theo. Không thêm hành động tự tách vô hình để cứu một bố trí thiếu đường giải.

Tách và tụ cần có cơ hội trải nghiệm cùng nhau và có đường tập hợp lại để sửa sai. **Boss 10 là nơi người chơi tự khám phá dao, chọn phần, giữ cơ quan và tụ; không chèn hướng dẫn trước hoặc trong Boss để đáp ứng đề xuất tutorial cũ.** Năng lực dùng dao/chọn phần có sẵn để thử dù chưa được ghi vào trí nhớ; ghi nhận sau sự kiện thật. Baseline Boss dùng hai phần; số phần tối đa, khối lượng nhỏ nhất điều khiển được và cách chọn tỷ lệ tách cho các màn sau chưa chốt; không cho từng hạt vụn thành một nhân vật cần thao tác riêng.

Sau tách, lượng vật chất và vị trí thực quyết định lực/khả năng nhận việc. Nhiệm vụ thuộc về hạt/ID vật chất ổn định; cần quy tắc rõ khi chủ thể tách hoặc nhiều chủ thể nhập. Không tự hợp thể nếu làm mất nhiệm vụ giữ cơ quan; người chơi gọi về phải thấy việc nào sẽ được nhả.

Tụ không dịch chuyển các phần qua vật cản. Chỉ những phần có đường về mới tiếp cận được; gặp nhau mới nhập. Không nhập từ xa hoặc khôi phục phần thất lạc bằng animation. **Tất cả phần phải nhập thành một cơ thể bên trong trước khi qua cửa thoát cuối**; bất kỳ phần nào ra trước khi nhập hết thì thua và hiện “bạn phải hợp thể trước khi chui ra”. Nhập ngoài hộp không cứu được lần thua. Một cơ thể chảy qua ống liên tục không phải phân tách, ống chuyển khoang không phải cửa cuối. Xem [hợp đồng kết thúc](VENOM_LEVEL_ARCHITECTURE.md).

## 7. Copy vật thể: biến thành hình dạng có công dụng

Người dùng đề xuất sinh vật biến thành đồ vật trong hộp, ví dụ chìa khóa. Đề xuất quy tắc đầu tiên: **copy hình dạng và chức năng hình học có thể thực hiện bằng cơ thể**, giữ danh tính/khối lượng của sinh vật. Chạm vật mẫu hợp lệ để học, sau đó có thể dùng mẫu đó theo luật màn; phạm vi nhớ mẫu trong màn hay xuyên màn còn cần chốt.

- Vật gốc vẫn ở đó. Một phần sinh vật chuyển hình, không tạo thêm một chiếc chìa có khối lượng ngoài tổng vật chất. Phần đang hóa chìa vẫn được theo dõi tới khi thoát.
- Mẫu có kích thước/khối lượng phù hợp; phần quá nhỏ không tự sinh thêm mô. Phần dư không bị xóa để khớp hình; cần mẫu cho phép dùng đủ mô hoặc yêu cầu người chơi chọn phần phù hợp trước.
- Copy không tự cấp vật liệu, điện năng, nam châm, trọng lượng hay toàn bộ công dụng của vật gốc. Copy hình dạng một viên pin không làm sinh vật có điện. Mỗi mẫu cần định nghĩa chức năng được hỗ trợ và tín hiệu nhận biết.
- Để chìa truyền mô-men thay vì oằn như chất lỏng, đề xuất sinh vật **giữ cứng tạm thời khi hóa hình**. Đây là khả năng giả tưởng cần giới thiệu bằng biến đổi dáng/chất liệu và phản ứng thật, không tuyên bố là tính chất chất lỏng thông thường.
- Với ví dụ ổ khóa: mẫu phù hợp → căn và đưa phần chìa vào → có tiếp xúc hợp lệ → xoay đủ góc → cơ cấu nhả. Có thể dùng ổ khóa hình học đơn giản có nhận dạng mẫu để prototype; cần ghi rõ là mô hình khóa đơn giản, không giả lập toàn bộ cơ cấu khóa thực. Chỉ có ID đúng hoặc animation kết thúc không đủ để mở khi sinh vật còn ở xa.
- Vật mẫu/ổ khóa có dấu hiệu tương ứng, đúng/sai được phản hồi trước khi gây kẹt. Đề xuất mỗi phần chỉ giữ một hình dạng đang dùng; trong bản thử đầu trở về dạng mềm trước khi tách/tụ. Chưa chốt đây là giới hạn sản phẩm vĩnh viễn.

Đường trở về dạng mềm phải có không gian hoặc quy trình tháo ra hợp lệ. Cần thử trường hợp chìa ở trong khóa khi phần khác mở cửa, bị gọi về, pause hoặc retry; không tạo collider mới xuyên vật cản. Năng lực copy và mẫu cần để giải màn thuộc campaign, không phụ thuộc đồ đã mua trong nhà.

Về triển khai, cần thư viện mẫu do thiết kế khai báo và bộ chuyển trạng thái hình học/vật lý có bảo toàn khối lượng. Hệ 32 hạt và skin hiện tại không tự cung cấp collider chìa có răng, khả năng giữ cứng hay suy luận công dụng của vật bất kỳ. Bắt đầu bằng một mẫu chìa và một ổ khóa trước khi mở rộng.

## 8. Cách mở kỹ năng và tăng độ khó

Người dùng đã chốt [01–09 làm quen, 10 là Boss](VENOM_CAMPAIGN_01_10.md), đã nhận đủ bản vẽ. Chuỗi: bò → leo → xoay chọn mặt → vùng trơn → tận dụng trượt → vỏ cầu trơn → đẩy/kéo làm bậc → rơi bắt vành/chảy qua ống → nắp rơi theo trọng lực → Boss dao/hai phần/A–B/hợp thể. Boss lấy cơ quan Journey04 cũ nhưng sửa luật thoát. Copy và giới hạn khối lượng qua khoang chưa có màn trong chuỗi này, giữ cho các thiết kế sau.

Nhịp làm quen tham khảo: thử đơn lẻ → luyện trong bố trí khác → kết hợp → đổi vai/thứ tự → ôn nhẹ. Quyết định cụ thể của người dùng cho Boss 10 có ưu tiên: **thử thách mới, khó và không hướng dẫn**, không buộc giới thiệu dao trước đó. Không ép Boss dùng đủ tám kỹ năng. Độ khó đo bằng quyết định có hệ quả, nhiệm vụ cùng giữ, phụ thuộc thứ tự, khả năng phục hồi và độ rõ tín hiệu; kiểm chứng bằng lượt chơi thật, không chỉ đếm kỹ năng hoặc kéo dài đường đi.

Các chuỗi minh họa, cần dựng hình học và kiểm chứng trước khi coi là level:

- **Luồn + kéo:** sinh vật thu thân qua khe, vào phía sau cửa để kéo chốt mở lối lớn.
- **Tách + giữ + luồn:** phần nhỏ vào khoang giới hạn dung tích, phần còn lại giữ cơ quan cho nó qua.
- **Tụ + đẩy:** hai phần hoàn thành hai nhiệm vụ, có đường gặp nhau; bản thể đủ mô đẩy vật nặng ở vùng tì hữu hạn.
- **Copy + phối hợp:** một phần hóa chìa và giữ khóa đang xoay, phần khác đi qua để chốt cửa mở; khi cửa đã chốt, phần chìa được rút về, tụ và cùng thoát.

Chuỗi cuối phải có bước giải phóng phần đang làm chìa; nếu biến thành chìa tiêu hao hoặc bị bỏ lại trong ổ thì vi phạm luật toàn bộ sinh vật phải thoát. Chấp nhận lời giải khác dùng đúng các luật vật lý và kỹ năng, không khóa cứng đúng một chuỗi thao tác.

## 9. Kiến trúc và phạm vi hiện tại

Xem [kiến trúc level và kế hoạch triển khai](VENOM_LEVEL_ARCHITECTURE.md). Mỗi kỹ năng là bộ thực thi gồm điều kiện bắt đầu, bước tiếp cận, trạng thái đang làm, kiểm tra kết quả, điều kiện bị chặn và hủy. Dữ liệu cơ quan cung cấp điểm bám/tì, hướng tác động, ngưỡng vật lý, vùng hình học và các tác động puzzle. Ý định của người chơi, bộ thực thi, mô phỏng và biểu diễn có trách nhiệm riêng; animation không tự thay trạng thái cơ quan.

Kiến thức được ghi qua sự kiện thực hiện thật. Level khai báo tập kỹ năng đã học/cần dạy; không dùng level number để rải logic cho từng cơ quan. Các trạng thái chỉ có một phần đang chọn nhưng nhiều phần đang thực hiện phải được HUD thể hiện nhất quán.

| Kỹ năng | Bằng chứng có trong prototype hiện tại | Công việc còn thiếu cho đặc tả |
| --- | --- | --- |
| Bò | Chạm đích, nhiệm vụ riêng và đường giải năm Journey | Tìm đường tổng quát qua cơ quan/hình học mới |
| Leo | Bò trên sáu mặt vỏ hộp, đổi mặt, bám/võng theo trọng lực | Leo qua vách thấp và bề mặt cơ quan nội bộ một cách tổng quát |
| Luồn | Có `VenomSqueeze` cho các bố trí thử | Ống/khoang mới và giới hạn lượng vật chất có nguyên nhân rõ |
| Đẩy/kéo | Có tiếp xúc vật lý; xúc tu chủ yếu là biểu diễn | Lệnh tương tác đồ vật, điểm bám và truyền lực có giới hạn theo khối lượng |
| Phân tách | Máy chém cắt liên kết vật chất, đúng hướng chỉ tách bằng cơ quan đã chốt | Mở rộng cơ quan cắt, phản hồi vị trí cắt và kiểm chứng các bố trí mới |
| Tụ lại | Có tiếp cận, kết dính và bảo vệ nhiệm vụ | Luật tổng quát cho nhiều phần và trạng thái copy |
| Copy | Chưa có | Mẫu, học/nhớ mẫu, chuyển hình vật lý, tương tác khóa và khôi phục dạng mềm |

## 10. Kiểm chứng trước khi thiết kế campaign dài

- Mỗi kỹ năng có bài đơn lẻ chứng minh ý định, kết quả vật lý, phản hồi và hủy/thử lại. Test phối hợp chỉ thêm sau khi các kỹ năng thành phần chạy đúng.
- Đẩy/kéo: khối lượng lớn có ngân sách lực lớn hơn trong cùng điều kiện; phản lực, ma sát và mô-men hợp lý; tổng lực không nhân đôi sau tách.
- Luồn: chênh lệch khả năng qua có nguyên nhân thể tích/biến dạng đã khai báo; không xuyên collider, mất mô hoặc tạo phần vụn không điều khiển được.
- Tách/tụ/copy: tổng khối lượng và ID vật chất được bảo toàn; đổi hình không tạo va chạm giả hoặc khóa vĩnh viễn các phần còn lại.
- Mỗi đường giải phải giải phóng được mọi phần, kể cả phần đang giữ nút hoặc làm chìa. Kiểm tra cả người mới học và đã biết kỹ năng; không phụ thuộc sở hữu đồ nhà.
- Đo vật lý, dựng skin và nhiều tương tác đồng thời trên điện thoại mục tiêu. Không tạo thêm hạt vật lý cho từng kiểu animation hay chạy mọi mẫu copy cùng lúc.

Lần cập nhật này chỉ ghi nhận concept và thiết kế quy tắc. Không sửa code, scene, save hoặc build. Cách kích hoạt phân tách đã chốt là qua cơ quan cắt; thông số lực, số phần/tỷ lệ cắt, giới hạn luồn và cách copy cần tiếp tục chốt/kiểm chứng.
