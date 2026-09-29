# COghe — Cơ quan dễ đọc, puzzle theo chiều cao

Ngày 29/09/2026 · **Đề xuất thiết kế, chưa triển khai vào campaign**.

Người dùng giữ đồ họa hộp kính C; yêu cầu phân biệt các liên kết bằng màu và tận dụng chiều cao, tường, leo trèo, xoay camera. Nghiên cứu này không thay scene, vật lý, thứ tự màn hay hướng art đã chốt. V01/V02 là mã thử nghiệm, không phải màn 01/02 hiện tại.

## 1. Quyết định đề xuất

**Màu cho biết “thuộc cụm nào”; hình dạng cho biết “làm gì”; chuyển động cho biết “đã xảy ra chưa”.** Chiều cao tạo đường tiếp cận và quan hệ không gian mới, không chỉ tăng số đồ vật.

**Làm rõ từ người dùng:** mở rộng thư viện cơ quan gồm ròng rọc, thang nâng/thang máy, các hộp xếp chồng và những cơ cấu khác; xen kẽ bố cục mặt sàn với chiều cao. V01/V02 chỉ minh họa hai khả năng, không đại diện cho toàn bộ hướng thiết kế. Không mặc định các màn sau đều cao hơn hoặc đều kết thúc bằng mở cửa.

Giữ hộp kính, khung nhôm mảnh, nhựa ngà, thân COghe đen ướt, ánh sáng C và bảng số dán vào kính. Chỉ thay ngôn ngữ trình bày của cơ quan và cách bố trí chúng. Ưu tiên hai cụm màu trong cùng một góc nhìn; màu thứ ba chỉ khi cần một liên kết độc lập thật sự. Đây là mục tiêu dễ đọc, không phải giới hạn engine.

## 2. Ngôn ngữ cơ quan

| Vai trò | Màu gợi ý, sRGB | Dấu phụ | Quy tắc |
|---|---|---|---|
| Liên kết A | Xanh lam dịu `#397FAD` | Vòng tròn + A | Tay tác động, đầu truyền động và viền đầu ra cùng mã |
| Liên kết B | Đỏ đất/coral `#C86C58` | Hình thoi + B | Khác xanh rõ, tránh đỏ cảnh báo quá gắt |
| Liên kết C, khi thật cần | Vàng đất `#A89245` | Ba vạch + C | Phân biệt với nhựa amber bằng nhãn và họa tiết; thử trên máy trước khi chốt |
| Bề mặt trơn | Lavender hiện có | Vân satin, ranh giới liên tục | Không bám/bò chủ động; vẫn nhận chạm, vẫn chịu trọng lực và quán tính |
| Lỗ thoát cuối | Mint hiện có | Lỗ tròn và viền mảnh sát mặt | Không dùng mint làm màu của một cụm cơ quan mới |
| Vỏ, kết cấu | Ngà, nhôm xanh xám | Hình khối và chân gá | Trung tính; không sơn toàn bộ cửa/phòng thành mảng màu bão hòa |

Màu A/B không quy định loại cơ quan: bánh răng xanh vẫn có thể điều khiển cửa xanh. Một cụm gồm nút + bánh răng + cầu dùng cùng mã nếu chúng thực sự truyền động cùng nhau. Một cửa có hai điều kiện phải có **hai dấu riêng**, không trộn thành màu thứ tư. Mỗi dấu báo riêng điều kiện của nó; cửa chỉ mở khi đủ điều kiện thật.

Màu đi cùng hình/nhãn nhằm giúp nhận biết khi đổi góc, giảm bão hòa hoặc khó phân biệt màu. Đây cũng là nguyên tắc trong [Xbox Accessibility Guideline 103](https://learn.microsoft.com/en-us/xbox/accessibility/xbox-accessibility-guidelines/103). Không coi bộ màu đề xuất là đã đạt kiểm thử mù màu.

### Hình dạng phải nói rõ thao tác

- **Nút chịu tải:** mặt tròn rộng, đế thấp, khe lún thấy rõ. Đặt ngang trên sàn hoặc bệ; lún và nhả theo tải thật. Nếu cần giữ thì hiện biểu tượng giữ, không giả thành nút chốt.
- **Tay kéo/đẩy:** quai chữ U đủ lớn, ray có đầu hành trình rõ, một vùng bám chân cố định ngay cạnh. COghe bám vào mặt đỡ rồi tác động vào quai; không kéo qua khoảng không.
- **Bánh răng lắp ghép:** vành răng thép/nhựa trung tính, màu tập trung ở moay-ơ và vị trí khớp. Chỗ trống có vòng nét đứt cùng mã. Răng phải ăn khớp theo hình học thật; bánh trang trí không được quay giả một đường truyền không có thật.
- **Cửa:** viền/băng màu cùng mã đầu vào, khe mở và hướng ray rõ. Màu nhận diện giữ nguyên khi mở; không biến mọi cửa thành xanh lá.
- **Cầu/tấm bám:** mặt đi ngà dễ thấy thân đen, đầu nối mang mã màu, có chốt và dấu vị trí đỗ. Phần ray là kết cấu riêng, không mặc nhiên trở thành lối bám.
- **Thang nâng, giai đoạn sau:** khay có chân đỡ, ray, đối trọng/phanh, điểm đỗ. COghe phải được chở bằng tiếp xúc thật; không dời transform để làm giả chuyển động.

Tín hiệu trạng thái dùng hành trình, chốt cơ khí và một đèn nhỏ; âm “cạch” dịu bổ sung. Khi chạm đầu vào, cho dấu A/B ở đầu ra nháy nhẹ đồng bộ trong thời gian ngắn. Đó là phản hồi quan hệ, không bật sẵn lời giải hay thứ tự cho Boss. Đường nối màu ngắn đi sát gá/tường, tránh mạng dây che kín kính. Nếu dùng đường in thay truyền động vật lý, phải đọc như ký hiệu dây tín hiệu; không vẽ trục giả truyền lực.

## 3. Không gian ba chiều có ý nghĩa

Một bố cục nên có **vùng chân hộp → trạm trên vách → đường nối trên cao → đích**, với khoảng trống đủ quan sát. Không chia hộp thành nhiều sàn kín đè lên nhau. Dùng bệ lệch, trụ, thanh nối và vùng bám trên kính; bố trí góc trước thoáng.

Tường và trần kính thường đều bò được. Bởi vậy một cây cầu chỉ bắc qua sàn trống chưa tạo puzzle: COghe có thể bò vòng trên kính. Mọi đường bắt buộc phải dựa trên hình học hoặc vật liệu thấy được: dải trơn liên tục, vách thật, cửa thật, khoảng hở vượt khả năng với tới. **Không thêm tường vô hình hoặc khóa kỹ năng để ép lời giải.**

Vùng trơn phải liên tục qua mép nếu dùng để chặn đường vòng; các thanh gá, dây, mặt dưới bệ cũng phải được kiểm tra. Trang trí không được trông như bề mặt đủ rộng để bò nhưng lại không có va chạm. Gá nhỏ có thể làm mảnh rõ ràng; gá lớn phải có mặt bám/trơn hợp lý trong gameplay.

Sự thú vị đến từ thay đổi quan hệ không gian:

| Họ puzzle | Quyết định của người chơi | Khoảnh khắc vui | Điều kiện chống khó thao tác |
|---|---|---|---|
| Cơ quan trên vách | Tìm mặt tiếp cận, leo và kéo | Nhận ra cả vách cũng dùng được | Một tay nắm lớn, chỗ bám rộng |
| Lắp đường bám | Đặt tấm nối vào vị trí làm đường liên tục | Đường chữ Z hiện ra, COghe bò qua | Vị trí đỗ có chốt, không đòi kéo pixel chính xác |
| Đổi hướng cầu | Chọn nhánh nối để tới trạm kế rồi đổi lại | Cùng vật thể có hai công dụng | Mỗi trạng thái ổn định, có đường quay lại |
| Truyền động khác độ cao | Gài bánh ở chân hộp để giải phóng cơ quan ở vách | Thấy chuyển động lan lên cao | Truyền động nhìn được, không giấu điều kiện |
| Vận chuyển | Đưa sinh vật/vật lên bệ, dùng phanh/đối trọng | Cả cụm nâng lên bằng lực | Không bước nhảy căn thời điểm; có mép lên xuống rộng |
| Phối hợp hai vách | Một phần giữ phanh, phần kia điều chỉnh đường | Hai vị trí cùng làm một hệ chạy | Khoang giữ hai phần cách nhau tự nhiên; sau chốt có đường tụ |

Hai minh họa cụ thể: [V01 — Trạm trên vách](../../../LevelDesign/COghe/VerticalStudy/V01/README.md) và [V02 — Ghép đường lên cao](../../../LevelDesign/COghe/VerticalStudy/V02/README.md). Hai hồ sơ giữ các mục của LEVEL_TEMPLATE; chưa có scene hoặc bằng chứng chơi trọn.

## 4. Camera và điều khiển

- Bản V2 hiện tại kéo để **xoay camera**; không xoay hộp, không đổi trọng lực. Giữ điều này trong các thử nghiệm mới.
- Góc đầu phải thấy COghe, ít nhất một điểm tác động, hướng đường đi và lỗ cuối. Nhìn mặt cơ quan ở góc xiên vừa phải, tránh nằm sát đường viền.
- Chạm là giao tác vụ; cơ quan tự đi tới vị trí đỗ hợp lệ bằng lực hữu hạn hiện có. Điểm chạm cần lớn ở ảnh điện thoại, tránh hai tay nắm chồng nhau theo phép chiếu. Dùng khoảng 48 đơn vị UI logic làm mục tiêu thử ban đầu, không coi 48 pixel vật lý là chuẩn chung.
- V2 đã có orbit ngang, pinch và Toàn cảnh. Ban đầu bố trí cơ quan để dùng được bằng các điều khiển đó; chưa cần camera tự đổi góc. Camera xoay không được đổi trạng thái cơ quan hoặc rebuild graph.
- Nếu thử nghiệm thật cho thấy bệ trên che bệ dưới, thêm lựa chọn góc nhìn tác giả cho từng vùng. Chọn vùng chỉ đổi camera, không điều khiển COghe hoặc tiết lộ thứ tự giải.
- Chỉ làm mờ/ẩn phần trang trí che view theo quy tắc trình bày. Cửa, vật cản và đích chạm thực phải còn đọc được; không raycast xuyên cơ quan kín để dễ chọn.

## 5. Đối chiếu code hiện tại

| Quan sát từ repository | Ý nghĩa thiết kế và việc cần làm |
|---|---|
| `COgheTapRail.StandPoint` dùng `WorkingSurface.Normal`; `COgheRailSlider.WorldAxis` đổi trục từ Frame | Có nền tảng cho tay kéo trên vách. Cần cấu hình mặt đứng/stance/handle và test phản lực, không chỉ quay prefab 90° rồi coi là xong |
| `COgheTapRail` đếm mô đang bám đúng collider và dùng lực hữu hạn theo khối lượng | Bệ thao tác phải đủ rộng và nằm đúng WorkingSurface. Tránh giật vì điểm chân nằm trên vật đang bị đẩy; thử hủy lệnh, đi ngược, phần nhỏ |
| `COgheTissueSensor` nhân tải với `Clamp01(Dot(transform.up, Vector3.up))` | Nút chịu tải xoay lên tường đứng cho tải bằng 0. Dùng tay kéo cho vách; nút chủ động trên vách phải có cơ chế nén theo pháp tuyến mới, không sửa sensor tải để nhận mọi chạm |
| `COgheViewTransmission` kiểm tra hình học ăn khớp và giữ đầu ra khi rail đã ổn định | Tái dùng cho gài bánh/đỗ cầu; không chỉ đổi màu hoặc phát animation để mở cửa |
| `VenomNavigationRevision` giữ revision của toàn bộ surfaces/props; `BuildGraph` dựng lại toàn graph khi khác | Có cache, nhưng nhiều mặt chuyển động trên cao vẫn có thể làm invalidation đắt. Đo trước; nếu cần tách graph phần tĩnh và cập nhật cụm động/link giao tiếp. Đây là mở rộng đề xuất, chưa có sẵn |
| `VenomCampaignCamera` có ViewOnly orbit yaw, pinch và Overview | Camera không phải cơ quan vật lý. Màn cần nhìn mặt dưới phải được kiểm tra với pitch hiện có; camera vùng là bước sau nếu cần |
| `COgheViewMechanism` đang thay material Waiting/Ready cho lamp | Đặt màu liên kết ở trim riêng; đèn trạng thái tách khỏi mã màu để tránh mất liên hệ sau kích hoạt |

Kiến trúc đề xuất: thêm dữ liệu trình bày `CircuitId`, mã màu, glyph, nhãn và danh sách đầu vào/đầu ra. **Dependency graph gameplay hiện tại vẫn quyết định hoạt động**; dữ liệu màu không tạo điều kiện mở cửa. Builder lấy liên kết thực để sinh trình bày, kiểm tra tham chiếu rỗng/trùng mã gây nhầm. Một đầu vào điều khiển nhiều đầu ra dùng cùng mã; hai điều kiện độc lập không được che thành một màu.

Cơ quan dùng lại cần khai báo rõ local frame, trục lực, WorkingSurface, điểm chân, điểm tay, hành trình/chốt, liên kết đầu ra, reset/hủy và hình học ảnh hưởng đường đi. Animation đọc Approaching/Operating/force/contact/latched: xúc tu chạm đúng quai, thân co trước kéo, võng theo trọng lực, rồi nhả khi đổi lệnh. Không kích hoạt trạng thái bằng sự kiện cuối animation.

## 6. Hiệu năng và tính khả thi

Cao hơn không đồng nghĩa đắt hơn; chi phí phụ thuộc số bề mặt navigation, vật động, contact và lớp trong suốt chồng lên nhau. Không làm mỗi răng bánh răng thành một collider hoặc mỗi tấm trang trí thành mặt tìm đường. Tách ray/khung tĩnh với phần động; dùng collider đơn giản nhưng đúng khe chức năng; gộp trang trí tĩnh theo vật liệu.

Dùng ít vật liệu màu chung, atlas glyph và một nguồn bóng như C. Tránh đèn thật riêng cho mỗi nút, kính nhiều lớp bên trong, phản xạ realtime, bloom/SSAO. Không mặc định MaterialPropertyBlock luôn tối ưu: [Unity ghi rõ nó làm mất tương thích SRP Batcher](https://docs.unity.com/en-us/engine/6000.3/manual/analysis/graphics-performance-profiling/reduce-draw-calls/draw-call-batching/properties). Chọn shared materials/atlas hay property block sau khi xem Frame Debugger và đo pipeline thực.

Giữ 32 hạt và nhịp mô phỏng đã chốt; không giảm mô để đổi lấy FPS. Không ngừng mô phỏng cơ quan giữ tải chỉ vì nó khuất camera. Dựng V01 trước vì chỉ một cụm, rồi V02 hai cụm để đo chi phí tăng thực tế; đây không phải tuyên bố engine chỉ chịu được hai cụm.

Đo baseline và prototype cùng OPPO, cùng cấu hình: warm-up, chạy đường giải, liên tục orbit/pinch/đổi lệnh khi ray đang chạy, retry và chơi 15–20 phút. Lưu p50/p95/p99/max frame time, CPU/GPU, GC, số lần/thời gian BuildGraph, renderers/batches/overdraw. Mục tiêu thử là 60 FPS ổn định ở cấu hình chọn; **chưa có đo mới, chưa chứng nhận đạt**. Chụp ảnh thực ở góc đầu, giữa và kết thúc để so concept.

## 7. Kế hoạch triển khai tiếp theo

1. Làm lớp nhận diện cho một cặp cơ quan hiện có: màu + glyph + hành trình + phản hồi liên kết. So trước/sau, giữ vật lý nguyên vẹn.
2. Dựng V01 khối đơn giản, kiểm tra chân bám trên vách, phản lực, hủy và kéo ngược. Áp art C khi chơi trọn được.
3. Dựng V02 hai tấm nối; kiểm tra vòng quanh bốn vách, trần, khung gá, rơi về sàn và đi ngược. Chốt kích thước bằng cơ thể thật, không dựa vào tranh.
4. Cho người chưa biết lời giải chơi: ghi nhận họ đoán đúng cặp điều khiển–đầu ra không, số chạm sai, điểm bị che, có phải hỏi thao tác không. Với V02 đo riêng suy luận vị trí ghép và khó chạm. Điều chỉnh hình/góc trước khi thêm hướng dẫn.
5. Đo trên OPPO, so ảnh thực với art C, rồi mới cân nhắc thang nâng và phối hợp hai vách. Xếp thử nghiệm vào tiến trình sau dữ liệu playtest; không tự thay số màn đã chốt.

Hướng này giữ cảm giác chill: người chơi khó ở **nhìn ra liên kết và đặt đúng trạng thái**, được vui nhờ thấy cả hệ chuyển động và COghe bò lên con đường mình vừa tạo. Không đánh đổi bằng thao tác nhỏ, cú rơi phải căn chuẩn hoặc đồ vật khuất.

## 8. Tài liệu và giới hạn hình minh họa

Ảnh ở thư mục này được tạo bằng imagegen tích hợp; prompt và tham chiếu lưu trong `Prompts/`. Đây là concept phục vụ trao đổi hình ảnh. Ray/khớp, kích thước và đường bám phải theo hồ sơ và được kiểm chứng bằng Unity; tranh không chứng minh khả giải hoặc FPS. Các nét đứt/mũi tên trong bảng minh họa là chú thích cho tác giả, không mặc định xuất hiện khi chơi hoặc trong Boss.


## 9. Mở rộng cơ quan và nhịp giới thiệu — cập nhật theo người dùng

### Hai trục độc lập

Thiết kế theo hai trục: **cơ quan làm gì** và **nó nằm ở đâu**. Cùng một cơ quan có thể xuất hiện trên sàn, trên bệ hoặc trên vách với mặt đỡ phù hợp. Một màn trên sàn vẫn có thể khó vì thứ tự và phối hợp; một màn cao vẫn có thể dễ nếu chỉ một hành động rõ ràng. Không dùng chiều cao hay số object làm thước đo độ khó.

| Họ cơ quan | Lần đầu: một quan hệ dễ nhìn | Biến thể sau khi đã hiểu | Yêu cầu khi dựng |
|---|---|---|---|
| Khối/hộp làm đường leo | Các hộp cố định xếp lệch thành bậc, leo tới đích | Đẩy một khối vào khe; chọn thứ tự các khối; dùng nâng hạ để đưa khối lên vị trí | Không mặc định COghe có kỹ năng nhấc hộp. Xếp lại theo cao độ phải có dốc, ray hoặc thiết bị nâng thật; kiểm tra lật/kẹt, đường quay lại |
| Ròng rọc và đối trọng | Kéo một đầu dây, thấy bệ ở đầu kia nâng lên | Đưa khối lên đĩa đối trọng; chọn lượng tải hoặc vị trí neo; kết hợp giữ phanh | Dây, bánh dẫn hướng, neo và đường truyền tải rõ. Dây chỉ kéo, có trạng thái chùng; không giả dây mềm thành thanh đẩy. Lực/hành trình phải theo cấu hình, không cho lực vô hạn |
| Thang nâng/thang máy | Bước lên khay, chọn điểm dừng gần, khay chở tới bệ | Mang theo khối; dừng ở nhiều cao độ; gọi khay từ trạm khác; chọn đường tiếp nối | Có nguồn nâng đọc được: đối trọng hoặc mô-tơ lab; phanh/chốt thật, tiếp xúc chở mô ổn định. Có cơ chế gọi lại tránh khay bỏ sinh vật ở ngõ cụt |
| Cầu, ván, tấm bám | Đẩy một tấm nối đúng hai mép | Đổi hướng, thay độ cao, dùng cùng tấm cho hai chặng | Vị trí đỗ ổn định; kiểm tra mặt dưới, mép, khe và đường vòng qua kính |
| Bánh răng, cần gạt, cửa | Một đầu vào và một đầu ra cùng màu | Chọn đường truyền, đổi chế độ, chốt thứ tự | Là một họ trong thư viện, không phải mẫu kết thúc bắt buộc của mọi màn |
| Cơ cấu phối hợp | Các tác vụ nối tiếp bằng một cơ thể | Sau khi học cắt/tụ: một phần giữ phanh, phần kia chuyển tải rồi cùng tụ | Phải có đường rời nhiệm vụ và hợp thể bên trong; bố trí giữ khoảng cách bằng hình học, không dùng cooldown tụ |

Ngoại hình nhận diện riêng cho từng họ: ròng rọc có dây và đĩa dẫn; thang có khay, ray và điểm dừng; khối có mặt leo và tay đẩy; cầu có hai đầu nối. Mã màu gắn ở đầu tác động, dây/cụm liên quan và đầu ra, giữ phần lớn kết cấu trung tính. Không sơn mọi đồ có thể đẩy cùng một màu nếu chúng thuộc các liên kết khác nhau.

### Nhịp học: giới thiệu → luyện → kết hợp → nghỉ nhịp

Mỗi màn giới thiệu chỉ thêm một nguyên lý. Khi dạy cơ quan mới, dùng điều khiển và góc nhìn người chơi đã quen; không đồng thời thêm loại cơ quan, điều khiển mới, góc bị che và yêu cầu timing. Sau đó tái dùng cơ quan trong bố cục khác, rồi mới phối hợp với kỹ năng đã học.

Ví dụ nhịp cho **một chương sau khi người chơi đã biết bò/leo/đẩy**, không phải quyết định thay 10 màn đầu:

| Vị trí tương đối | Bố cục | Nội dung học/thử |
|---|---|---|
| 1 | Sàn | Đẩy một khối vào vị trí làm đường; một thao tác rõ |
| 2 | Cao thấp nhẹ | Leo các hộp xếp sẵn; luyện leo, không thêm cơ quan mới |
| 3 | Sàn | Giới thiệu ròng rọc đơn, kéo dây để dịch một bệ nhìn thấy ngay |
| 4 | Sàn kết hợp bệ thấp | Dùng lại ròng rọc để nâng bệ thành lối leo; không thêm điều khiển |
| 5 | Sàn | Màn nghỉ nhịp: biến thể ngắn của đẩy khối đã biết |
| 6 | Chiều cao | Giới thiệu thang nâng tự giữ ở điểm dừng; khay chở COghe lên bệ rộng |
| 7 | Chủ yếu sàn | Luyện gọi/trả khay và mang một khối qua khoảng trống; không căn nhảy |
| 8 | Sàn + vách | Kết hợp khối với cơ cấu nâng, tới một trạm đã nhìn thấy từ đầu |
| 9 | Bố cục gọn | Củng cố thao tác khó nhất ghi nhận từ playtest; ít bước, dễ sửa sai |
| 10 | Sàn + chiều cao | Boss kết hợp các nguyên lý đã học thành vấn đề mới; không đưa cơ chế chưa từng dạy |

Nhịp này cần playtest, không là tỷ lệ sàn/cao bắt buộc. Nếu người chơi còn lúng túng với ròng rọc, thêm bài luyện và lùi thang nâng; không nhồi đủ thư viện vào một chương. Boss khó nhờ phối hợp và suy luận, không vì chi tiết nhỏ/khó chạm; giữ chính sách không gợi lời giải.

### Hệ quả cho triển khai

V01/V02 tiếp tục là mẫu thử cho mặt đứng và ghép đường. Song song về mặt kế hoạch, cần mẫu ròng rọc, khay nâng và khối đẩy trên sàn trước khi xếp thành campaign. Khi triển khai mỗi mẫu mới phải có hồ sơ theo LEVEL_TEMPLATE, đường giải, đường phục hồi và bằng chứng Unity/thiết bị; danh sách trên chưa chứng nhận runtime đã hỗ trợ.

Cơ quan nâng không cần mô phỏng dây bằng hàng chục rigidbody ngay từ đầu: có thể nghiên cứu ràng buộc chiều dài/truyền lực với dây hiển thị theo các điểm dẫn nếu đúng nhu cầu puzzle. Nếu gameplay cần dây mắc, chùng hoặc va vật khác, mô hình phải hỗ trợ tình huống đó; không dùng đường dây trang trí để hứa hẹn tương tác chưa có. Khối tự do cần vật lý thật khi có thể lật/đổ; khối chạy ray phải có ray và chốt nhìn thấy. Đo contact/graph/khớp trên OPPO trước khi tăng số khối hoặc tầng.
