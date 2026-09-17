# Venom — Chương mở đầu 01–10

Ngày cập nhật: 15/09/2026. Đã nhận bản vẽ đủ chín màn đầu và xác nhận các điều chỉnh 07–08. **01–09 là phần làm quen; 10 là Boss khó, tự khám phá, lấy cơ quan của Journey 04 hiện tại.** Thắng Boss lần đầu mở Collection **“Nhà của sinh vật”**. Luật campaign mới: phải hợp thể trước khi thoát; một phần ra trước khi hợp thể sẽ thua.

Đây là thiết kế đã đối chiếu, chưa phải catalog đã dựng đủ mười scene. Bản macOS hiện vẫn chạy năm màn `VenomJourney01`–`05` với luật thoát lần lượt cũ. [Kiến trúc và kế hoạch triển khai](VENOM_LEVEL_ARCHITECTURE.md) tách các quyết định đã chốt khỏi đề xuất kỹ thuật cần kiểm chứng. Chưa đổi code, scene, save hoặc build trong lần cập nhật này.

Tên và số màn dưới đây thuộc campaign mới. **Màn 04 mới “Trơn đấy” khác màn `VenomJourney04` đang chơi**, được chọn làm nguồn màn 10.

## Tư liệu đã đối chiếu

| Số màn campaign mới | Tư liệu có thể kiểm tra | Nội dung được hiểu ở mức bản vẽ | Trạng thái |
| --- | --- | --- | --- |
| 01 — Bò đi | `6412883e-1004-474c-811e-c028b980214e.jpeg` | Sinh vật trên sàn hộp vuông trong suốt, gần góc trước; lỗ ở góc xa trên sàn để học chạm hướng dẫn và quan sát sinh vật bò | Đã đối chiếu; **khác** scene `VenomJourney01` hiện tại về vị trí lỗ |
| 02 — Leo đi | `255d92cb-afa4-49e1-8c44-169d1f2acce2.jpeg` | Sinh vật trên sàn, vách thấp bịt kín hai đầu tới thành hộp, lỗ trên tường phía đối diện; dạy vượt vách và bám kính | **Đã xác nhận vách kín hai đầu**; khác scene `VenomJourney02` hiện tại có lỗ giữa trần |
| 03 — Xoay đi | `44ee8e3f-6dd9-4a27-a852-524091b7f1e4.jpeg` | Lỗ ở mặt sau; chạm theo hình chiếu khi chưa xoay chỉ chọn mặt trước. Xoay hộp để mặt sau hướng tới người chơi rồi chỉ dẫn | Đã đối chiếu; lấy chú thích “mặt sau” làm chuẩn, không suy ra lỗ trên trần từ phối cảnh vẽ |
| 04 — Trơn đấy | `d36aaf27-59af-4a63-90dd-7a2c516527ec.jpeg` | Sinh vật bám sẵn trên thành; lỗ ở tường đối diện, vùng trơn dưới lỗ. Dẫn thẳng sẽ trượt/rơi; tìm đường bám khác hoặc xoay hộp tận dụng trọng lực | **Đã xác nhận lời giải bằng xoay hộp/trọng lực hợp lệ** |
| 05 — Trượt đi | `809258102_1747237140737299_1541612142048880242_n.jpg` | Lỗ trên mặt nóc trơn toàn bộ; lật mặt này xuống dưới rồi nghiêng để sinh vật trượt tới lỗ | Đã đối chiếu; không thêm thao tác búng mới |
| 06 — Xoay tròn | `c6c1064b-0a85-45a8-aeb5-46bfb2abd934.jpeg` | Vỏ cầu trơn, sinh vật không tự bò; xoay lỗ tới sinh vật đang trượt/dao động | Đã đối chiếu; không gắn sinh vật vào chuyển động vỏ |
| 07 — Đẩy | `01642df5-3d08-4b31-88e2-35e4052e4626.jpeg` | Khóa xoay; đẩy/kéo hộp nhựa tới điểm chạm, đặt làm bậc vượt vùng trơn; 3 giây không điều khiển thì buông và Idle | Người dùng xác nhận có kéo để phục hồi khi đẩy sát kính |
| 08 — Chui qua lỗ | `e459b9c6-25d4-478e-bb15-3588ee700629.jpeg` | Khóa xoay; leo nóc, rơi khi chạm vùng trơn, căn rơi vào vành bám; cơ thể chảy qua ống nối sang hộp kia | Người dùng sửa rõ: **căn cú rơi**, không vươn xúc tu từ nóc tới vành |
| 09 — Lật đi | `0ef3e27a-53a5-4aa5-a931-9b8466ddd0f4.jpeg` | Nắp trong suốt dạng hộp úp che lỗ sàn; lật hộp lớn để nắp rời lỗ theo trọng lực | Đã đối chiếu; nắp là đồ vật rời có va chạm |
| 10 — Boss, chưa chốt tên riêng | Nguồn `Assets/_Game/Venom/VenomJourney04.unity` | Cắt → hai phần giữ A/B chốt cửa → rời nút, hợp thể → thoát. Không hướng dẫn; mở Collection khi thắng | Giữ cơ quan Journey 04, cập nhật luật thoát và cách trình bày Boss; không dùng lab `Venom04` |

Các ảnh gốc mới nằm trong `/Users/mrk/Downloads/`; thứ tự người dùng đính kèm là **03, 02, 01, 04**, không phải thứ tự campaign. Hai bản vẽ 01–02 gửi trước (`798016963_1334366525439762_493405465279401791_n.jpg`, `796095026_1362977409238152_3256449574949989350_n.jpg`) là tư liệu trước của cùng ý tưởng. Tên màn dùng đúng yêu cầu trực tiếp của người dùng, kể cả khi tiêu đề trên giấy khác.

Đợt ảnh 05–09 được đính kèm theo thứ tự **09, 07, 05, 06, 08**. Nội dung xác nhận trực tiếp sau ảnh có ưu tiên: 07 có đẩy/kéo và thời gian buông; 08 dùng rơi bắt vành; 10 là Boss không tutorial. Không coi các mũi tên trong bản vẽ là lệnh camera hay lệnh điều khiển ngoài phần giải nghĩa.

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

## Thiết kế đã đối chiếu của màn 05–09

### 05 — Trượt đi

Sinh vật bám trên thành hộp. Toàn bộ mặt nóc, nơi có lỗ, là vật liệu trơn. Khi bò lên đó ở tư thế ban đầu, sinh vật mất bám và rơi. Người chơi lật mặt trơn xuống phía dưới rồi nghiêng hộp để sinh vật trượt tới lỗ. Cùng vật liệu của màn 04, nhưng giờ người chơi tận dụng thay vì đi vòng tránh nó. Cảm giác như căn đường trượt trên bàn; không tự bổ sung thao tác búng hoặc lực di chuyển chủ động trên mặt không có độ bám.

### 06 — Xoay tròn

Vỏ hình cầu có toàn bộ mặt trong trơn. Sinh vật không thể chủ động bò/leo; chỉ trượt và dao động theo trọng lực, quán tính và tiếp xúc. Người chơi xoay vỏ để đưa lỗ đến vùng sinh vật có thể thoát. Xoay cầu không được mang cơ thể xoay cùng như đang dính lên vỏ. Các nét cong trong hình biểu diễn bề mặt cầu, không phải các tầng hoặc ván mê cung.

### 07 — Đẩy

**Khóa xoay hộp ở màn này theo yêu cầu người dùng.** Có một hộp nhựa nhỏ di chuyển được và vùng trơn ngăn leo trực tiếp tới lỗ. Người chơi chọn hộp; sinh vật tiếp cận, bám và chuyển sang tư thế tác động. Khi đang bám hộp, điểm chạm tiếp theo là **đích muốn đưa hộp tới**. Sinh vật đẩy hoặc kéo tùy hướng đích, tự bố trí tiếp xúc trong phạm vi nhiệm vụ đó.

Kéo là phần bắt buộc của bài: khi đã đẩy kịch vào kính vẫn có thể kéo hộp ra và đặt lại đúng chỗ. Không cần sinh vật chui ra phía ngoài kính để tìm tư thế đẩy; dùng điểm bám còn tiếp cận được trên mặt hộp phía trong. Sau **3 giây không điều khiển**, sinh vật buông hộp và về **Idle**. Cách triển khai thời gian: tính từ lệnh điều khiển tương tác gần nhất, dừng khi pause; hết hạn thì hủy lực đẩy/kéo và nhả bám dù còn chưa tới đích. Điểm chạm mới khi đang bám đặt lại thời hạn. Quy tắc 3 giây chỉ áp dụng tương tác đẩy/kéo hộp này, không làm các phần ở Boss tự rời nút.

Đặt hộp sát thành đúng vùng giúp sinh vật leo lên như một bậc để tới lỗ. Buông hộp rồi chọn đích leo; sau Idle có thể chạm hộp để bám lại nếu cần sửa vị trí. Khối lượng, lực, ma sát và tư thế tiếp xúc phải cho việc kéo ra khỏi kính khả thi. Đồ vật giữ va chạm thật, không snap tới đích. Ngưỡng đặt đủ gần để leo và kích thước vùng an toàn cần chỉnh qua chơi thử.

### 08 — Chui qua lỗ

**Khóa xoay.** Hai hộp nối bằng một ống nhỏ; sinh vật ở hộp đầu, lỗ thoát cuối ở hộp sau. Mặt quanh đầu ống bên hộp đầu là vật liệu trơn, ngoại trừ **vành có thể bám quanh miệng ống**.

Người chơi dẫn sinh vật lên nóc và chọn vị trí tiến tới vùng trơn. Nó mất bám, rơi dưới trọng lực; người chơi phải căn điểm rời nóc/quỹ đạo rơi để cơ thể **tiếp xúc được vành bám**. Không vươn xúc tu từ nóc để bỏ qua cú rơi, không hút từ xa vào vành và không tự chọn điểm rơi tối ưu thay người chơi. Việc bắt vành là phản ứng bám khi tiếp xúc hợp lệ; không yêu cầu thao tác bấm đúng một frame để bắt.

Từ vành, cơ thể kéo dài và **chảy qua ống như một dòng nước**, toàn bộ vật chất lần lượt sang khoang thứ hai rồi thu lại thành một sinh vật. Đây là biểu diễn biến dạng liên tục, không tự phân tách thành các phần độc lập và không dịch chuyển tức thời sang đầu ống. Ống là đường chuyển khoang, **không phải cửa thoát cuối**; đi qua đây không kích hoạt thắng/thua do thoát.

Nếu rơi hụt vành, sinh vật rơi xuống sàn hộp đầu và có đường leo lại để thử. Vành phải nhìn rõ, có bề rộng và độ bám đủ cho một khoảng sai số chơi được; độ khó nằm ở chọn điểm rơi. Không cho kỹ năng đã nhớ tự chọn toàn bộ chuỗi rơi–bắt–qua ống ở lần chơi lại.

### 09 — Lật đi

Sinh vật bắt đầu bám thành. Lỗ sàn bị một nắp trong suốt dạng hộp úp che. Chạm theo vị trí lỗ khi bị che sẽ đưa sinh vật tới bề mặt nắp, chưa thoát được. Nắp là đồ vật rời; người chơi lật hộp lớn để nó rơi khỏi lỗ theo trọng lực, rồi sinh vật tới lỗ thoát.

Nắp có khối lượng và va chạm, không biến mất, không có motor nhả ở một góc đặt sẵn. Hình dạng úp và miệng đủ rộng để nắp không lọt vào hoặc kẹt trong lỗ; lực bám/phản lực của sinh vật không được mặc nhiên coi nắp là thành cố định. Khoang còn lại phải đủ chỗ cho nắp rơi mà không khóa vĩnh viễn đường thoát.

## Luật thắng/thua đã thay đổi

Áp dụng chung cho gameplay Venom: **mọi phần phải hợp thể thành một sinh vật trước khi bắt đầu thoát qua lỗ cuối**, sau đó toàn bộ vật chất phải ra ngoài thật mới thắng. Không yêu cầu tất cả vật chất qua mặt lỗ cùng một thời điểm; một cơ thể liền mạch kéo dài qua lỗ vẫn hợp lệ.

Nếu còn các phần độc lập mà bất kỳ phần nào thoát ra trước, chuyển sang thua và hiện nguyên văn:

> bạn phải hợp thể trước khi chui ra

Chạm gần lỗ hoặc vào vùng hỗ trợ chưa đồng nghĩa đã thoát. Khi mô thực sự vượt ra ngoài qua cửa cuối, kiểm tra toàn bộ cơ thể, gồm cả vật chất đã ra ngoài; không dùng số phần còn trong hộp để suy ra hợp thể. Thua được giữ tới khi người chơi thử lại, không thể để các phần ra sau nhập ngoài hộp rồi đổi thành thắng. Đầu/đuôi của **một** cơ thể đi qua ống hoặc qua cửa không được nhầm là hai phần. Tiêu chí phát hiện cụ thể và thứ tự xử lý nằm trong [kiến trúc kết thúc màn](VENOM_LEVEL_ARCHITECTURE.md).

Luật này thay thế việc cho các phần thoát lần lượt trong prototype cũ, kể cả nguồn Journey 04 của Boss. Giữ phân tách chỉ bằng dao; tụ là tự động khi tới gần và thỏa điều kiện kết dính hiện có. Không tự gọi các phần rời nhiệm vụ chỉ vì cửa đã mở.

## Nguyên tắc công nhận lời giải

**Lời giải khác dự kiến nhưng tuân thủ các quy luật vật lý và cơ quan của game vẫn được công nhận.** Không thêm cờ buộc đi đúng đường mẫu. Màn 07, 08 và 10 khóa xoay là ràng buộc màn được người dùng chỉ định rõ, không phải giới hạn tự thêm; màn 04 vẫn cho giải bằng xoay/trọng lực. Kiểm tra thắng dựa trên luật hợp thể trước khi thoát, trạng thái thoát thật của toàn bộ vật chất và các điều kiện cơ quan có trong màn. Lỗi xuyên collider hoặc vật chất bật khỏi vỏ ở ngoài lỗ vẫn là lỗi mô phỏng cần sửa.

Khi nghiệm thu 01–04, cần kiểm tra tương ứng: bò tới góc xa; vách kín nhưng có thể leo qua; dấu chạm ở đúng mặt trước/sau khi xoay; vùng trơn gây trượt đúng hướng trọng lực và cả đường vòng lẫn lời giải xoay hộp đều hoàn thành được. Đây là tiêu chí kiểm thử sắp tới, chưa phải kết quả đã chạy trên scene mới.

## Vai trò học và tăng độ khó

- **Màn 01** là bài nhận lệnh: chỉ một việc dễ thấy, sinh vật đáp lại và di chuyển tới lỗ. Không đặt mức độ khó bằng thời gian bò dài.
- **Màn 02** thêm khả năng leo qua vách và bám mặt kính; hai đầu vách kín, không có đường vòng trên sàn.
- **Màn 03** dạy xoay hộp để quan sát và chỉ đúng bề mặt; tiếp tục dùng kỹ năng bò/leo đã biết.
- **Màn 04** phối hợp chỉ đường, leo và xoay; giới thiệu vật liệu trơn, yêu cầu người chơi tự chọn đường hoặc tận dụng trọng lực.
- **Màn 05** đổi cách dùng vật liệu: biến mặt trơn thành đường trượt dưới trọng lực.
- **Màn 06** luyện xoay trên hình cầu, đưa lỗ tới sinh vật thụ động.
- **Màn 07** thêm đẩy/kéo đồ vật và sửa vị trí đã đặt sai; phối hợp với leo.
- **Màn 08** phối hợp leo, rơi bắt điểm bám và biến dạng chảy qua ống.
- **Màn 09** dùng trọng lực tác động lên đồ vật rời để mở đường.
- **Màn 10** là Boss khó tự khám phá, đưa thêm thử thách cắt, phân vai, giữ A/B và hợp thể trước khi thoát. Không tutorial, không hướng dẫn bù khi chưa biết kỹ năng. Copy chưa xuất hiện trong chín bản vẽ và không được tự thêm vào Boss.

## 10 — Boss và Collection

Nguồn Journey 04 có A/B cùng yêu cầu ít nhất **12 g** mô thực sự ở vùng cảm biến, trong **hai phần khác nhau**, giữ đồng thời **0,65 giây**. A nâng nắp để tiếp cận B; khi đạt cả hai, cửa thoát được **chốt mở**. Dùng đây làm baseline cơ quan, không sao chép điều kiện 24/60 g của Journey 05. Khối lượng và thời gian là thông số nguồn chưa được chứng minh cân bằng Boss; có thể chỉnh sau khi đo lượt chơi, không thay lời giải đã chọn.

Lời giải để người thiết kế/kiểm thử đối chiếu: vào dao → có hai phần → một phần giữ A để phần còn lại tới B → cả hai chốt cửa → người chơi dẫn chúng tới vùng gặp an toàn → tự hợp thể → một cơ thể thoát. **Cửa đã chốt phải tiếp tục mở khi các phần rời A/B để hợp thể**. Không tự gọi về hoặc tự thoát khi cửa mở. Theo cập nhật ngày 16/09/2026, **khóa xoay toàn bộ Boss**. Lưỡi thép chờ trên cao; mô đi vào vùng cảm biến thì báo bằng đèn trong **1 giây mô phỏng**, rồi dao rơi theo trọng lực. Người chơi vẫn được chỉ vị trí mới trong lúc báo. Tỷ lệ các phần phụ thuộc vị trí mô thật khi lưỡi dao cắt qua; không ép về tâm hoặc chia 50/50. Dao nâng trở lại sau lượt chém và chỉ sẵn sàng khi vùng cảm biến đã trống. Né được lượt chém là hợp lệ. Nút A/B có mặt nhấn lún, nhả lên khi rời, và đèn theo lượng mô thực; giữ nguyên ngưỡng 12 g cùng chốt cửa 0,65 giây.

Các phần tách rời vẫn leo và bám kính thường; không giảm khả năng leo theo tỷ lệ khối lượng. Khi đổi mặt tại góc lõm, đường đi phải giữ tiếp xúc để phần nhỏ không rời tường trước khi bắt trần. Người chơi có thể dẫn các phần gặp nhau trên sàn hoặc trần. Hỗ trợ miệng lỗ không bị vô hiệu khi còn nhiều phần: mô ra ngoài trước khi hợp thể phải kích hoạt thông báo thua, không rơi lặp lại ở miệng lỗ.

Luật hợp thể chốt ngày 16/09: **cứ đủ gần là nhập lại**. Không yêu cầu cùng đích, cùng đứng yên hay nhả nút trước; không có cooldown sau chém. Cú chém tạo xung lực sang hai phía của lưỡi dao, cân bằng động lượng theo khối lượng để hai phần văng ra đủ xa và không tự nối lại ngay. Không thêm lực hất lên hoặc dịch chuyển tức thời; va chạm và lực bám làm chúng dừng lại. Kiểm tra tiếp xúc giữa mô và vật cản thực, gồm mặt cắt của lưỡi dao khi nó vẫn nằm giữa hai phần. Nếu tới gần một phần đang giữ nút, chúng vẫn nhập; cảm biến sau đó đo khối lượng của cơ thể đã nhập theo luật cơ quan. Các lệnh cũ được gộp, giữ lệnh còn hiệu lực được đưa ra gần nhất. Khoảng hình thành liên kết mềm vẫn giữ 0,65 giây để biểu diễn tụ dần.

**Không hướng dẫn ở Boss:** không bàn tay chỉ, không mũi tên đường giải, không chuỗi “cắt rồi A rồi B”, không gợi ý xuất hiện sau vài lần thua. Dao cắt, phần được chọn, nút bị đè, cửa mở và tụ phải có phản hồi nhìn thấy được; đây là thông tin hành động thực sự, không tiết lộ lời giải. Thông báo thua do chưa hợp thể vẫn hiển thị nguyên văn theo yêu cầu người dùng, kể cả ở Boss.

Định hướng thị giác đề xuất: nhận diện Boss ngay khi vào màn, nhấn khoảnh khắc cơ thể bị chia, liên kết ánh sáng thể hiện A tác động B và đợt sáng khi cửa chốt. Không che dao/nút/đường về bằng VFX. Giữ đoạn thoát thật, camera tiến gần, ẩn vật che và ngẫu nhiên một trong ba điệu vui đã có. Hiệu ứng cụ thể cần thử trực quan; không tự thay hộp bằng “Glass Flower”.

**Thắng Boss 10 lần đầu mở Collection “Nhà của sinh vật”.** Ghi quyền mở khóa cùng kết quả thắng trước khi chạy thông báo; replay Boss không cấp lại. Đề xuất sau ăn mừng hiển thị thẻ mở Collection và lựa chọn vào nhà; không tự next quá nhanh làm mất khoảnh khắc này. Vào lại game sau khi thoát app giữa đoạn ăn mừng vẫn giữ quyền mở. Trước Boss, nhà chưa mở; không bắt mua hoặc chăm sóc để tiếp tục campaign. Danh mục đồ, kinh tế, cách trang trí và nội dung nhà ban đầu chưa chốt.

## Chuyển thành bản chơi

1. Dùng [kiến trúc đề xuất](VENOM_LEVEL_ARCHITECTURE.md) để dựng từng cơ chế dùng chung trước khi ghép đủ mười màn. Không còn chờ bản vẽ 05–09; phần chưa chốt là thông số hình học, tuning và trình bày cụ thể.
2. Dựng 01–09 theo catalog mới rồi dùng **cơ quan và luật của `VenomJourney04`** làm nguồn cho scene campaign 10. Giữ source hiện tại để hồi quy; không chỉ đổi `LevelNumber` từ 4 sang 10 trên scene cũ.
3. Khi renumbering, cập nhật tên scene, title/hint, hàng chọn màn, tiến trình hoàn thành/save, tự sang màn, build scene list và tests. Runtime Journey hiện dùng mảng 01–05, `Completed` chỉ giữ năm bit; đổi số đơn lẻ sẽ làm truy cập sai hoặc mất tiến trình người chơi cũ. Cần tách ID campaign mới với nguồn scene prototype và có chuyển đổi save tương thích.
4. Kiểm thử đường giải của từng màn, sai lệch cắt, xoay hộp, retry và khả năng giải phóng mọi phần. Màn 10 giữ điều kiện tiếp xúc A/B, cửa chốt, hợp thể trước cửa cuối và 100% vật chất thoát thật. Kiểm tra Boss không hiện tutorial và Collection chỉ mở sau thắng hợp lệ; hiệu ứng chỉ biểu diễn kết quả cơ quan.

Cập nhật triển khai 15/09/2026: mười scene Origin đã được dựng theo thiết kế này. Runtime, luật hợp thể, animation theo hành động và phòng Collection thử nghiệm được mô tả trong [hướng dẫn prototype](VENOM_ORIGIN_PLAYTEST.md). Journey và save cũ được giữ riêng. Không có build APK trong đợt này.
