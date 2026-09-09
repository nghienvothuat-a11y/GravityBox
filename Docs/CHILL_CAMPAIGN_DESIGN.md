# Gravity Box — hệ thống campaign thư giãn và boss

Ngày: 09/09/2026. Đây là kế hoạch đã được người dùng duyệt để triển khai. Campaign 100 màn đã được author; trạng thái nghiệm thu và khác biệt so với mục tiêu thiết kế được ghi trong [CAMPAIGN_IMPLEMENTATION.md](CAMPAIGN_IMPLEMENTATION.md). Các mục tiêu độ khó/readability/recovery dưới đây vẫn cần playtest, không tự trở thành kết quả đã đo.

## 1. Hướng sản phẩm

Người chơi thấy một món đồ cơ khí đẹp, nghiêng thử, hiểu phản ứng của viên bi và dần điều khiển thành thạo. Thành công đem lại cảm giác khéo léo và khám phá. Mỗi mốc 10 màn có một boss với hình dáng, chuyển động và đoạn kết đáng nhớ.

Phạm vi bản đầu theo yêu cầu cập nhật: **100 màn campaign, 10 chương × 10 màn, gồm 90 màn thường và 10 boss**. Phát triển từ thư viện 23 prototype. Giữ nguyên thư viện trong Physics Lab để so sánh feeling và kiểm tra hồi quy. Số prototype P01–P23 không trở thành thứ tự campaign C01–C100. Ví dụ P15 là nền của boss C10; P23 là nền của boss C40 và một phần C100.

100 màn cho phép phân bố cơ chế thành các chương có thời gian học thật: cam ở C41–C50, con lắc/bay–đón ở C51–C60, nước ở C61–C70, thủy ngân ở C71–C80. Hai chương cuối đào sâu hình học và phối hợp những điều đã học. P12 nguyên bản được giữ làm Challenge tự chọn sau C30, ngoài 100 màn campaign; không dùng Challenge để bù đủ số lượng phát hành.

Đối tượng giả định để thiết kế và tuyển playtest: người chơi casual trên điện thoại dọc, phiên chơi khoảng 5–10 phút, chưa có kinh nghiệm với Gravity Box. Mac hiện tại dùng kiểm chứng và thử sớm; touch trên thiết bị thật là cổng nghiệm thu sản phẩm.

## 2. Các nguyên tắc bắt buộc

- Giữ profile bi thép, gravity thế giới, ma sát/va chạm và input nhất quán giữa các màn khô. Không tăng độ khó bằng cách bí mật thay lực hoặc độ nhạy.
- Giữ lỗ thoát thật và assist 40 mm ở lỗ cuối. Không thu nhỏ lỗ hoặc vùng assist để tăng khó. Mọi bi vẫn phải thoát hoàn toàn.
- Màn thường giới thiệu tối đa một ý mới. Boss không yêu cầu cơ chế chưa được học. Một kỹ năng chủ đạo cần ít nhất ba lần gặp có ý nghĩa: thử an toàn, tự thực hiện, áp dụng trong bố cục khác.
- Mỗi bước tăng khó ưu tiên một chiều: thêm quyết định, thêm hướng, hoặc thêm phối hợp. Tránh đồng thời đổi hình hộp, thêm bi, thêm cửa và siết timing.
- Không đồng hồ đếm ngược, giới hạn mạng hoặc phạt mất tiến độ dài trong campaign chính. Thời gian bên dưới là mục tiêu thiết kế, không phải điều kiện thua.
- Sai thao tác nên đưa bi về sân đỡ/hốc hồi phục gần đó. Chốt cơ khí giữ được một phần kết quả bằng trạng thái vật lý thực. Reset chủ động vẫn trả cả màn về ban đầu; không gọi teleport là checkpoint vật lý.
- Luôn nhìn được bi, mặt đỡ và vùng tiếp theo tại thời điểm phải quyết định. Kính mù, glare, rung camera và hiệu ứng che đường là lỗi readability, không phải điểm khó hợp lệ.
- Có thể dừng, quan sát và tiếp tục. Trong lúc điều khiển, camera ổn định; không tự orbit hoặc thay time scale. Cảnh quay khoe thiết kế nằm trong replay sau khi đã thoát.
- Chấp nhận cách giải vật lý hợp lệ khác với dự kiến. Waypoint kiểm thử không trở thành điều kiện thắng vô hình.

## 3. Đo độ khó: dự báo trước, hiệu chỉnh bằng người chơi

Không có công thức nào từ hình học tự chứng minh được độ khó cảm nhận. Hệ thống dùng hai lớp: **ước lượng để author** và **số đo playtest để sắp xếp lại**. Các trọng số, ngân sách và mục tiêu trong tài liệu này là giả thuyết thiết kế ban đầu, không phải số liệu đã kiểm chứng của Gravity Box.

### 3.1. Điểm ước lượng D, thang 0–100

Mỗi trục chấm 0–5, cho phép nửa điểm khi cần. Chấm tương đối với người đã học các kỹ năng tiên quyết; không coi một người lần đầu gặp cơ chế là đã biết cách giải.

`D = 20 × (0,22R + 0,20S + 0,18P + 0,18C + 0,12T + 0,10F)`

| Trục | Điều cần đo | Mốc 0 / 3 / 5 |
| --- | --- | --- |
| R — suy luận đường đi | Quyết định có ý nghĩa, nhánh gây nhầm, chuỗi phụ thuộc | Đường trực tiếp / vài quyết định phụ thuộc / chuỗi dài, cần nhớ trạng thái hoặc quay lại |
| S — định hướng không gian | Thay mặt đỡ, độ sâu, lồng độc lập, số hướng phải hình dung | Một mặt đỡ / nhiều cao độ hoặc mặt đỡ / nhiều trục, không còn một hướng trên–dưới cố định |
| P — độ chính xác | Clearance theo đường kính bi, dung sai góc, vùng đặt bi | Sân rộng / khe hoặc hốc vừa / đòi hỏi căn rất sát |
| C — phối hợp | Số đối tượng/trạng thái phải giữ trong đầu cùng lúc | Một bi, từng việc độc lập / hai bi theo lượt có chỗ trú / hai bi phụ thuộc tải hoặc giữ cửa lẫn nhau |
| T — timing | Dung sai thời điểm bắt đầu/đổi hướng của input | Có thể giữ tư thế mở / cửa sổ khoảng 0,8–1,5 s / dưới khoảng 0,4 s |
| F — mất công khi sai | Thời gian và số chặng phải làm lại | Hồi phục gần như ngay / thường mất 15–30 s / thường mất hơn một phút hoặc reset toàn chuỗi |

Timing được đo bằng độ lệch input vẫn thành công; thời gian bi bay không đồng nghĩa thời gian phản xạ bắt buộc. R không tăng chỉ vì kéo dài hành lang. C không tăng chỉ vì có nhiều chi tiết trang trí. F là cảnh báo mức phạt, không phải nút chỉnh để nâng D.

P có thước hình học đầu tiên: tỷ lệ bề rộng thông thủy / đường kính bi. Với bi 30 mm, có thể dùng P≈1 ở tỷ lệ trên 2,2; P≈2 ở 1,8–2,2; P≈3 ở 1,5–1,8; P≈4 ở 1,2–1,5; dưới 1,2 cần xem lại cho campaign. Đây chỉ là thước sàng lọc: góc cua, vận tốc, mặt cong, nắp và contact vẫn phải thử bằng cả bán kính bi. Lỗ cuối có assist được loại khỏi thước này.

Ví dụ audit sơ bộ các **prototype hiện có**, chưa phải thứ tự campaign:

| Prototype | R | S | P | C | T | F | D làm tròn | Nhận định |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| P01 tròn | 1 | 0 | 1 | 0 | 0 | 0 | 8 | Nền để dạy nghiêng/phanh |
| P09 slider | 2 | 1 | 2 | 1 | 1 | 1 | 28 | Cần bản giới thiệu cửa và hốc riêng |
| P17 cầu | 3 | 2 | 2 | 1 | 0 | 2 | 36 | Giảm mất công khi rơi khỏi cầu trước khi đưa vào campaign |
| P19 lồng | 2 | 4 | 2 | 1 | 0 | 2 | 40 | Phải dạy lồng độc lập và giới hạn quay |
| P12 mê cung cầu đầy đủ | 5 | 5 | 3 | 0 | 0 | 4 | 61 | Chi phí nhớ đường và quay lại quá cao cho nhập môn |
| P21 bay–đón | 2 | 4 | 4 | 0 | 4 | 4 | 57 | Cần nới cửa sổ thao tác và dựng lại đường cứu bi |
| P23 hai bi trong cầu | 4 | 4 | 4 | 5 | 1 | 4 | 76 | Nới chỗ phối hợp, giảm hồi phục dài trước khi làm C40 |

Các điểm trên là đánh giá tác giả có căn cứ hình học/cơ cấu; chưa có khoảng tin cậy thống kê. Việc P21 có điểm thấp hơn P12 không chứng minh mọi người sẽ thấy nó dễ hơn: cần xem cả vector và kỹ năng đã học.

### 3.2. Điều kiện cứng trước khi xét tổng điểm

`Novelty`: 0 hoặc 1 ở màn thường; 0 ở boss về kỹ năng cần để giải. Bộ phối hợp có thể mới nhưng từng hành động đã quen.

`Readability`: không có quyết định bắt buộc khi không nhìn thấy bi/vùng đỡ. `Solvability`: có route bằng input chuẩn, collider thật và mọi bi thoát. `Recovery`: mục tiêu hồi phục thường trong khoảng 10 s, boss khoảng 15–20 s cho một lỗi cục bộ; không buộc lật cả hộp qua một tuyến dài mới quay lại điểm thử.

Không để các trục dễ bù trừ một lỗi nặng: màn D thấp nhưng bị che bi, cửa sổ quá hẹp hoặc mất toàn bộ tiến độ vẫn không được duyệt. Campaign chính ưu tiên T≤2; Challenge có thể cao hơn sau khi người chơi tự chọn.

### 3.3. Ngân sách độ khó theo nhịp chơi

Đường cong có đỉnh và khoảng nghỉ. Một chu kỳ tham khảo: **gặp ý mới → luyện → biến thể → nghỉ → mở rộng → luyện → phối hợp → thử thách → củng cố nhẹ → BOSS**. Không dùng mẫu cứng để nhồi thêm mechanic.

Boss có ngân sách cao hơn màn thử thách trước đó khoảng 5–8 điểm D. So sánh với đỉnh trước boss, không với màn nghỉ ngay trước nó. Sau boss, giảm tải và dạy ý mới trong không gian an toàn. Tăng dần đỉnh theo chương, không yêu cầu mọi màn sau luôn khó hơn màn trước.

Các target D trong [ma trận 100 màn](CAMPAIGN_LEVEL_MATRIX.md) là ngân sách authoring dự kiến. Chưa có prefab của các biến thể nên không được gọi đây là điểm đã tính từ hình học hoàn chỉnh. Không thêm việc thừa chỉ để đạt target; playtest có quyền hạ cả ngân sách và thứ tự. D của boss dự kiến là 22 → 34 → 43 → 54 → 60 → 65 → 68 → 72 → 75 → 80; đây là chỉ số thiết kế, không phải phần trăm khó hơn hay xác suất thất bại.

## 4. Cấu trúc 100 màn của bản đầu

| Campaign | Kỹ năng chủ đạo | Nguồn prototype | Boss |
| --- | --- | --- | --- |
| C01–C10 | Nghiêng, phanh, góc cua, quán tính | P01–P08, hình sư tử P15 | **C10 — Sư tử thức giấc**, D mục tiêu 22 |
| C11–C20 | Gravity làm cửa/cầu chuyển động; giữ bi trong hốc | P09, P10 rút ngắn, P17 | **C20 — Khu vườn cơ khí**, D mục tiêu 34 |
| C21–C30 | Đổi mặt đỡ, đọc 3D, lồng độc lập | P11, P19, các đoạn nhỏ của P12 | **C30 — Chòm sao thủy tinh**, D mục tiêu 43 |
| C31–C40 | Hai bi, tải đòn bẩy, giữ và giải phóng bạn đồng hành | P16, P18, P23 | **C40 — Trái tim cơ khí**, D mục tiêu 54 |
| C41–C50 | Đẩy, hồi, giữ trạng thái, chọn thứ tự đường mở | P22, nền chốt/cầu đã học | **C50 — Đóa sen ký ức**, D mục tiêu 60 |
| C51–C60 | Con lắc mở ổn định, lấy đà, bay và đón với cửa sổ rộng | P20, P21, lồng P19 đã học | **C60 — Vũ điệu quỹ đạo**, D mục tiêu 65 |
| C61–C70 | Chuyển động dưới nước, lực cản và đường lăn có nhịp chậm | P13, đối chiếu P02 | **C70 — Thủy cung ánh trăng**, D mục tiêu 68 |
| C71–C80 | Bi nổi, theo mặt cao, đổi mặt đỡ trong thủy ngân | P14, đối chiếu P13 | **C80 — Mặt trăng bạc**, D mục tiêu 72 |
| C81–C90 | Đọc topology, hộp kỳ dị, chọn đường và mặt đỡ trong không khí | P04–P08, P11, P12, P15, P19 | **C90 — Mê cung vạn hoa**, D mục tiêu 75 |
| C91–C100 | Phối hợp hai bi, trạng thái giữ, lồng và đón tuần tự | P16–P23, bố cục khô | **C100 — Vũ trụ trong lòng bàn tay**, D mục tiêu 80 |

Hình hộp là bộ từ vựng thị giác có thể lặp lại ở nhiều độ khó. Không coi hình kỳ dị tự động là màn khó. Không đổi từ khô sang chất lỏng cùng lúc giới thiệu hai bi hoặc camera mới.

P20/P21 có chương riêng, để dạy con lắc mở ổn định trước timing và dạy lấy đà/đón/hồi phục trước phối hợp. P22 học từng bước “đẩy → hồi → giữ kết quả”; ba lần lặp y hệt không tự tạo chiều sâu. C61 đối chiếu hộp khô quen thuộc rồi C62 mới đưa vào nước đơn giản. C71 đối chiếu nước rồi C72 mới giới thiệu thủy ngân trong một lượt khác; có chỉ dẫn rõ bi sẽ nổi. Không đổi chất lỏng bất ngờ giữa một lượt chơi. C81 trở lại môi trường khô với một bài hiệu chuẩn ngắn trước khi tăng topology.

Mỗi biến thể phải thay một quyết định hoặc kỹ năng vận dụng có ý nghĩa: hướng giữ bi, thứ tự mở, đường hồi phục, mặt đỡ, nhánh lựa chọn, điểm chuyển vai hai bi. Đổi màu, xoay nguyên prefab hoặc kéo dài hành lang không đủ để tính là một màn mới. Ma trận phải ghi được câu hỏi riêng của từng màn; review theo cặp để loại những màn có cùng lời giải và chỉ khác trang trí.

## 5. Mười boss cần thiết kế riêng

### C10 — Sư tử thức giấc

**Lời giải:** một viên bi đi từ bờm/trán, vòng một trong hai bên mõm, rồi ra lỗ miệng. Hai hoặc ba vùng rõ ràng; không cửa, cơ quan mới hoặc động tác bắt buộc phải làm nhanh. Dùng mặt nạ P15 nhưng làm lại đường vào và loại những hốc dễ giữ bi quá lâu.

**Khoảnh khắc thị giác:** ban đầu là mặt nạ kính và đồng yên tĩnh. Nét sáng rất mảnh chạy qua các vùng vừa đi, làm hiện rõ khuôn mặt theo tiến độ. Khi bi thực sự ra khỏi miệng, toàn bộ đường nét tạo thành chòm sao sư tử trong khoảng hai giây. Lỗ thoát vẫn là đường sáng yếu; phần thưởng ánh sáng ở hoa văn khác.

**Hồi phục:** các túi ở bờm có mặt dẫn trở lại tuyến; lỗi ở mõm không đưa bi ngược hết về trán. Cần thử bằng người mới để tránh họ tưởng phải kích hoạt cả hai mắt: đèn phản hồi tiến độ, không thêm luật khóa vô hình.

**Nghiệm thu riêng:** hiểu mục tiêu bằng một cái nhìn, thấy bi lăn ra khỏi miệng, cả hai đường quanh mõm hợp lệ; boss đầu ưu tiên thành công và ngạc nhiên hơn thử thách khắc nghiệt.

### C20 — Khu vườn cơ khí

**Lời giải:** ba sân nối nhau, hai cửa gravity mở khác hướng và một cầu bản lề. Mỗi sân có hốc trú nhìn rõ. Người chơi hoàn thành từng thao tác; không cần giữ hai cửa ở những tư thế mâu thuẫn cùng lúc. Rút bỏ hành lang lặp dài của P10.

**Khoảnh khắc thị giác:** cấu trúc trông như một khu vườn bằng kính mờ và đồng. Khi nghiêng, cửa trượt và cầu gập theo gravity, làm xuất hiện những hướng đi mới giữa các “cánh lá”. Một nhịp click kim loại và nét sáng ngắn xác nhận cầu đã tựa ngàm. Khi thoát, những đường vân ghép thành một hoa văn lớn.

**Hồi phục:** cầu đã gài chốt giữ trạng thái thực; sân thấp hứng bi trượt khỏi mép, có đường trở lại sân hiện tại. Phải chứng minh bằng physics rằng sân hứng không trở thành đường tắt bỏ qua toàn bộ puzzle; lời giải khác hợp lệ vẫn được chấp nhận.

**Nghiệm thu riêng:** nhìn thấy lý do cửa mở/đóng, đọc được hướng tiếp theo, chỉ có một việc đòi hỏi căn chỉnh tại mỗi thời điểm. Hiệu ứng chỉ quan sát contact/trạng thái đã đạt; không dùng animation mở collider thay physics.

### C30 — Chòm sao thủy tinh

**Lời giải:** một lồng treo nối với mạng ván ba chiều có khoảng 8–10 khúc đổi hướng, chia ba vùng dễ nhận biết. Kế thừa kỹ năng C21–C29. Không đưa nguyên P12 với 21 khúc đổi hướng và hai nhánh cụt vào boss bắt buộc.

**Khoảnh khắc thị giác:** một mô hình thiên văn trong khối cầu; vỏ xoay trong khi lồng treo có hướng riêng. Các đoạn đường đã đi tạo thành một chòm sao. Mỗi vùng có hình dáng/mốc riêng, không chỉ khác màu. Khi bi ra ngoài, replay có thể cho thấy cả tuyến dưới góc khác, để người xem nhận ra đây là đường đi thật trong 3D.

**Hồi phục và quan sát:** điểm nối có sân nhỏ hoặc hốc nghỉ vật lý; ván lỡ tay đưa về nút gần, không về đầu tuyến. Mặt cầu gần camera mờ đi, nét cạnh/bi giữ độ rõ; collider không đổi. Lồng nằm ngoài các đường nhìn quyết định. Không tự xoay camera trong lúc điều khiển.

**Nghiệm thu riêng:** người chơi chỉ được đúng nút tiếp theo trước khi xoay; không lạc do transparency sorting; rơi về nút gần có thể phục hồi; vẫn hiểu không gian khi xem ở kích thước điện thoại.

### C40 — Trái tim cơ khí

**Lời giải:** A giữ tải đầu xa → B qua phía nâng và nhấn chốt → lấy A ra → đưa từng bi qua lồng và bộ hứng → đủ hai bi thoát. Bộ hứng phải đủ rộng để đoạn rơi này củng cố kỹ năng đổi mặt đỡ đã biết; chưa yêu cầu thao tác bắt bi giữa không trung như P21.

**Thay đổi cần thử so với P23:** hành lang B hiện 46 mm và hốc A dài 42 mm cho bi 30 mm. Dựng biến thể thử với lối B khoảng 60–70 mm và hốc A khoảng 55–65 mm. Phải tính lại chiều dài cánh tay đòn, tải giữ, vị trí ngàm và collider; không scale đồng loạt rồi giả định cơ cấu vẫn hoạt động. Kiểm tra mô-men `m × g × cánh tay đòn vuông góc` ở các tư thế giữ/nhả, sau đó xác nhận bằng rigidbody thật.

**Khoảnh khắc thị giác:** hai bi có tông và ký hiệu riêng; các đường dẫn trong lõi giống một cỗ máy đang được khởi động. Ánh sáng tăng nhẹ theo chốt thực đã gài. Khi A ra trước, HUD/hoa văn cho thấy còn B; nhịp kết chỉ xảy ra sau khi cả hai đã thoát. Màn kết thấy được hai quỹ đạo thật rời khối cầu.

**Hồi phục:** A có hốc giữ an toàn trong lúc điều khiển B, chốt giữ kết quả hợp lệ, sân hứng lớn. Giữ khoảng dừng giữa ba chặng; không cần phối hợp hai cú bắt bi chính xác đồng thời. Khoảng hở từ module vào lồng hiện có thể tạo nghiệm tắt; hoặc nhận đó là lời giải vật lý, hoặc dựng vỏ truyền thật nếu nó phá hủy toàn bộ ý tưởng. Không thêm kiểm tra waypoint để buộc người chơi đi tuyến tác giả.

**Nghiệm thu riêng:** người chơi hiểu “A giúp B, B giải phóng A”; không giữ sai bi do màu/che khuất; sau lỗi cục bộ vẫn giữ phần lớn công sức. Boss phải hấp dẫn ngay cả khi không xem VFX kết màn.

### C50 — Đóa sen ký ức

**Lời giải:** ba sân quanh tâm sen: cam thứ nhất mở một đường trung gian → gập và chốt cầu để sang sân kế → cam cuối nối đường về tâm. Mỗi cam giữ các nấc đã đạt; trạng thái trung gian tạo hướng tiếp cận hoặc đường hồi có ý nghĩa. Chỉ thao tác một cơ cấu mỗi chặng. Tiếp xúc rack–cam, nấc giữ, cầu và chuỗi hai cam đều đã học ở C41–C49. Không làm boss bằng việc yêu cầu đẩy một nút giống hệt nhiều lần hơn.

**Khoảnh khắc thị giác:** bánh cam nhìn như đài sen bằng đồng trong kính. Mỗi nấc khớp một phần hoa văn; cả đường đi và hoa văn hoàn chỉnh khi cam thẳng cửa. Cảm giác thỏa mãn đến từ bộ máy thật nhớ hành động của mình, với tiếng click và phản xạ kim loại nhẹ.

**Hồi phục:** cam giữ tiến độ khi bi quay về, các đường hồi ngắn và rộng. Whitebox phải chứng minh từng nấc có một ý nghĩa đường đi thực, đồng thời không có khe chéo cho bi bỏ qua cơ cấu. Phần này cần author collider cam mới và kiểm chứng, chưa được coi là khả thi chỉ vì P22 đã qua test.

### C60 — Vũ điệu quỹ đạo

**Lời giải:** tạo tư thế con lắc mở → dừng ở sân chuẩn bị rồi nghiêng bộ hứng để đón → dừng an toàn trước khi qua lồng dẫn về đích. Ba chặng có chỗ nghỉ, không ép căn con lắc và bắt bi trong cùng một cửa sổ ngắn. C51–C59 đã luyện phần con lắc/bay và đường cứu bi; lồng dùng hành vi đã học ở C23–C30.

**Khoảnh khắc thị giác:** những vòng trang trí gợi một mô hình quỹ đạo; con lắc đung đưa thật, bi đi qua một khoảng trống đủ rõ để thấy quỹ đạo của nó khác chuyển động của hộp. Ánh sáng ở sân hứng xác nhận tiếp xúc; trail chỉ mô tả chuyển động đã xảy ra, không giả đường hút.

**Hồi phục:** làm lại sân hứng/máng hồi để một lần hụt đưa bi về gần điểm lấy đà. Không dùng nguyên đường cứu bi P21 phải lật hộp và dẫn qua mặt trong nắp. Thử nhiễu thời điểm/góc để cửa sổ thành công đủ rộng; nếu người mới cần phản xạ dưới một giây, phải đổi hình học hoặc cách chuẩn bị.

### C70 — Thủy cung ánh trăng

**Lời giải:** dẫn một bi chìm qua các đường vách tĩnh, dùng độ chậm và khả năng phanh đã học trong nước. Chốt một bi cho toàn chương nước của bản đầu; không thêm cơ cấu động ngập nước khi chưa có mô hình lực tương ứng cho prop.

**Khoảnh khắc thị giác:** không gian nước đầy tĩnh lặng, các vách cong tạo hình trăng và những đường caustic dịu trên vật liệu. Wake nhỏ cho thấy viên bi đang đẩy nước. Không thêm mặt nước rút xuống, bọt tràn hoặc nước thoát lỗ khi mô hình hiện tại giữ chất lỏng đầy hộp.

**Hồi phục:** hốc rộng và vách bo tránh kẹt; các ngã rẽ dễ đảo hướng. Bản đầu ưu tiên miền chất lỏng đã được mô hình hóa và kiểm chứng, sau đó mới thay vỏ nếu thể tích/biên của force model vẫn khớp. Không coi việc chờ bi đi rất chậm là tăng chiều sâu câu đố.

### C80 — Mặt trăng bạc

**Lời giải:** người chơi đã quen bi nổi, đưa nó men theo các mặt cao và đổi mặt đỡ qua các vùng trong hộp thủy ngân. Đích cuối vẫn có assist chung. Từng vùng có chỗ nghỉ; không bắt giữ bi chìm bằng một lực không tồn tại.

**Khoảnh khắc thị giác:** khung máy bạc và các vách tạo hình lưỡi liềm, bi lăn dưới mặt “trần” như một cảnh trọng lực lạ nhưng có quy tắc rõ. Chế độ nhìn xuyên/cắt quan sát được báo rõ là hỗ trợ hình ảnh vì thủy ngân thực không trong suốt. Không làm hình phản chiếu giả gây nhầm với viên bi thật.

**Hồi phục:** luôn có mặt cao liên tục dẫn bi trở lại vùng gần. Hình học boss phải được thử ở nhiều hướng, gồm cân bằng nổi tại lỗ; không biến đoạn cuối thành bài chống buoyancy quá khó. D mục tiêu chỉ là trần: nếu trải nghiệm nổi dễ hiểu và hấp dẫn, không thêm công việc thừa để đẩy điểm lên.

### C90 — Mê cung vạn hoa

**Lời giải:** trở lại môi trường khô, đi qua một hộp có các nhánh hình học khác nhau nhưng nhận biết được. Kết hợp đổi mặt đỡ và một lồng đã biết; chọn đường theo vị trí thực. Độ sâu đến từ cách các vùng nối nhau, không từ hàng chục ngõ cụt dài.

**Khoảnh khắc thị giác:** vỏ và các nhánh tạo một hình khác khi đổi hướng; sau chiến thắng, góc replay làm lộ hoa văn đối xứng đã được viên bi đi qua. Hình phản chiếu/đối xứng nằm ở trang trí, không tạo đường giả trông giống lối có thể đi.

**Hồi phục:** nhánh sai vòng lại nút gần thay vì bắt đi ngược toàn bộ tuyến. Các mốc có hình dáng riêng để người chơi giữ được bản đồ trong đầu. C81–C89 đã dạy bố cục và đối chiếu môi trường khô; không đổi lại physics mà không có nhịp làm quen.

### C100 — Vũ trụ trong lòng bàn tay

**Lời giải:** hai bi trong một cỗ máy cầu, ba hồi liên tục: giữ tải và mở đường bằng cam → giải phóng cả hai qua lồng → đưa từng bi qua sân đón và lỗ cuối. C91–C99 phải luyện chính các cặp phụ thuộc được dùng ở đây. Mỗi hồi chỉ có một hoặc hai việc đang cần quan tâm; không bắt giữ tất cả trạng thái của game cùng lúc.

**Khoảnh khắc thị giác:** hình dạng tổng thể gợi một thiên thể cơ khí chứa các “chòm sao” đã gặp. Các bộ phận dùng chuyển động thật, ánh sáng đáp lại chốt/góc/contact đã đạt. Đoạn kết giữ khối cầu trong hình khi hai bi thoát; replay cho thấy toàn bộ cỗ máy vận hành và khoảnh khắc lời giải hoàn chỉnh. Không đưa thêm nước, thủy ngân hoặc luật thắng mới vào màn cuối chỉ để chứa đủ mechanic.

**Hồi phục:** sau mỗi hồi có vị trí nghỉ và trạng thái cơ khí giữ kết quả. Bắt buộc có sân dừng sau lồng và trước dốc bay; người chơi được chuẩn bị lại tư thế đón, không phải vừa rời lồng là lập tức xoay bắt bi. Phần khó nhất là thứ tự phối hợp, không phải khe siêu nhỏ hoặc một sai lầm làm lại vài phút. Mastery C100 có thể là biến thể rất khó sau campaign; bản C100 bắt buộc vẫn phải đạt chuẩn thư giãn và khả năng hồi phục.

**Nghiệm thu riêng:** lời giải có thể giải thích bằng ba câu, nhìn rõ hai bi tại các quyết định, đủ số lần luyện trước đó, mỗi hồi có đường cứu bi. Cần kiểm chứng whitebox, cả nhiễu input và playtest, trước khi sản xuất art đắt tiền.

### Chuẩn đầu tư cho mọi boss

Mỗi boss có bản whitebox, kịch bản dạy trước, hai hoặc ba chặng, kế hoạch hồi phục, phương án vật liệu/ánh sáng, điểm nhấn âm thanh và đường quay replay riêng. Dự trù khoảng một phần ba công sức level/art của mỗi chặng cho boss; không duyệt boss như một màn thường có thêm effect.

Thử hình thu nhỏ và video không lời: người xem có hiểu mục tiêu và nhìn thấy khoảnh khắc giải được hay không. Chỉ thêm nghệ thuật sau khi đường chơi và readability đã qua test. VFX không được làm frame time hoặc input tệ đi ở đoạn đòi hỏi chính xác.

## 6. Playtest và điều chỉnh thứ tự

### Mục tiêu ban đầu, chưa phải benchmark thị trường

| Loại màn | Mục tiêu trải nghiệm | Thời gian giải tham khảo | Hoàn thành không reset ở lần gặp đầu |
| --- | --- | --- | --- |
| Giới thiệu/nghỉ | Hiểu và thấy mình điều khiển được | 15–45 s | Khoảng 90–95% |
| Màn thường | Một việc vừa sức để khám phá | 30–90 s | Khoảng 80–90% |
| Phối hợp/thử thách | Vận dụng điều đã học | 60–120 s | Khoảng 70–85% |
| Boss | Có cao trào nhưng có thể hồi phục | C10: 90–150 s; boss sau: 120–240 s | Khoảng 60–80%; trên 90% sau tối đa ba lần thử là mục tiêu tiếp theo |

Đây là dải theo dõi, không phải quota thất bại. Nếu một boss rất dễ nhưng vẫn khiến người chơi vui và muốn xem lại, không tăng khó chỉ để ép tỷ lệ xuống. Không đặt timer theo thời gian tham khảo.

1. **Đợt định tính:** khoảng 12–20 người mới cho mỗi vòng thử trọng điểm, chia theo kinh nghiệm puzzle; quan sát từng chặng ngắn, hỏi họ dự đoán phản ứng trước một số thao tác. Tìm sai hiểu, chỗ không thấy bi và thao tác phục hồi khó chịu. Mẫu này không đủ để tuyên bố tỷ lệ cho toàn bộ người chơi.
2. **Đợt hiệu chỉnh:** khoảng 40–60 lượt người đủ tiên quyết cho mỗi nhóm biến thể quan trọng, chia các chặng giữa người chơi; không yêu cầu một người chơi liên tiếp cả 100 màn. Chia nhóm so sánh A/B hoặc đổi thứ tự trên các màn tương đương; tránh cho người đã biết lời giải thử lại rồi tính là người mới. Bổ sung nhóm chơi liên tục qua nhiều phiên để đo tích lũy mệt mỏi và khả năng nhớ kỹ năng.
3. Ghi thời gian giải, reset, mất tiến độ cục bộ, gợi ý, bỏ dở, thời gian hồi phục và pha gặp vấn đề. Tách thời gian pause/AFK xác nhận khỏi thời gian suy nghĩ thật. Thống kê người bỏ dở cùng mẫu người đã bắt đầu; không chỉ tính thời gian của nhóm thắng.
4. Sau mỗi chặng hỏi riêng mức thư giãn, mức hiểu cơ chế và khoảnh khắc ngạc nhiên. Tỷ lệ hoàn thành cao vẫn có thể là một màn nhàm hoặc khó chịu.
5. Báo cáo median/P90, tỷ lệ và khoảng bất định theo người chơi. Khi đủ dữ liệu, hiệu chỉnh trọng số bằng mô hình đơn giản có kiểm tra trên người chưa dùng để fit; không để một composite score thay thế các trục.

Cờ cần xem lại: tỷ lệ hoàn thành của màn thường giảm khoảng 15 điểm phần trăm so với ba màn liên quan trước đó, median giải tăng quá khoảng 1,5 lần, hoặc nhiều người sai cùng một dự đoán vật lý. Với mẫu nhỏ cần thêm bằng chứng, không sửa theo một lượt bất thường. Boss được so với ngân sách boss, không coi đỉnh có chủ ý là regression.

Thứ tự sửa: **readability → kỹ năng còn thiếu → đường hồi phục → precision/timing → vị trí trong campaign**. Không âm thầm thay gravity hoặc lực hút để làm đẹp thống kê. Gợi ý có thể lần lượt nhắc mục tiêu, chỉ chỗ trú, cho xem cơ cấu cần chú ý; người chơi vẫn điều khiển bi.

## 7. Viral phù hợp với cảm giác thư giãn

Thiết kế boss để đoạn video 8–15 giây có đủ ba ý: thấy bài toán, thấy một hành động khiến máy thay đổi, thấy viên bi thoát thật. Đây là mục tiêu nội dung, không phải bảo đảm viral.

Sau chiến thắng cho phép xem lại và chủ động lưu/chia sẻ. Replay dùng chuyển động đã ghi, không giả định replay input sẽ tái lập PhysX chính xác trên mọi máy. Chỉ đưa slow motion/góc quay điện ảnh vào replay; phần chơi vẫn giữ nhịp vật lý.

Sau mỗi boss có thể mở một bản **Mastery** tự chọn: thêm nhánh, cửa sổ nhỏ hơn hoặc phối hợp sâu hơn trên đúng cơ chế đã học. Không chặn campaign bằng bản siêu khó. Bản P12 đầy đủ là ứng viên tốt cho nhánh này. Theo dõi xem lại/lưu clip và phản hồi “muốn thử”, đồng thời theo dõi người chơi có bực hoặc rời game; không tối ưu chia sẻ bằng việc làm người chơi thất bại liên tục.

## 8. Kiến trúc nội dung đề xuất

Các kiểu dữ liệu bên dưới là kế hoạch, chưa tồn tại trong bản build hiện tại.

- **CampaignCatalog:** thứ tự C01–C100 tham chiếu LevelId/prefab variant; tách khỏi Physics Lab. ID ổn định, không suy ra danh tính từ vị trí mảng hoặc đổi ý nghĩa enum shape. Dùng schema lưu tiến độ có phiên bản, hỗ trợ người chơi tiếp tục qua nhiều phiên.
- **LevelDesignProfile:** SourcePrototypeId, IntroducedSkills, RequiredSkills, vai trò màn, sáu trục D, TargetBand, Novelty, phương án hồi phục, phiên bản hiệu chỉnh và liên kết bằng chứng playtest.
- **BossDesignProfile:** cờ Boss, số chương, các pha quan sát, chỗ nghỉ, tiêu chí đọc hình và cấu hình âm thanh/VFX/replay. Đánh dấu rõ BOSS tại mọi mốc 10 đến 100 trong bộ chọn. Màu/ký hiệu không phải dấu hiệu duy nhất.
- **Progression validator:** boss đúng mốc 10, không thiếu tiên quyết, đã có các lần luyện trước đó, biến thể có đường vật lý, lỗ cuối và roster hợp lệ. Gợi ý kỹ năng chưa dạy phải được bắt ở Editor.
- **Kiểm chứng vật lý:** route từ spawn, perturb góc/timing quanh route để ước lượng dung sai, containment, contact, softlock, hồi phục, reset, toàn bộ bi thoát. Không dùng thời gian solver tự động làm thời gian người chơi.
- **Playtest recorder:** mở rộng log local hiện có với LevelVersion, nhóm kỹ năng, hint, phase đã quan sát, lần mất tiến độ/hồi phục và dữ liệu replay. Tránh hard-code logic campaign trong hệ lực.

Các pha boss quan sát trạng thái vật lý đã đạt để hiển thị tiến độ. Lực, collision và thắng vẫn thuộc runtime vật lý hiện tại. Powered gate của P16 cần được thể hiện là cơ cấu có nguồn năng lượng hỗ trợ; không mô tả nó là cửa thụ động tự mở do gravity.

## 9. Thứ tự triển khai

1. **Chốt hệ thống cho đủ 100:** ma trận kỹ năng, câu hỏi riêng của mỗi màn, cách chấm D, các biến thể dự kiến, mười bản thiết kế boss và tiêu chí nghiệm thu. Lập campaign riêng, giữ Lab làm đối chứng. Hoàn thành đủ 100 màn gồm 10 boss là điều kiện nội dung của bản đầu.
2. **Làm trọn C01–C10:** whitebox cả chặng và boss sư tử trước, thử bằng người mới, rồi mới hoàn thiện art/âm thanh của boss. Đây là mẫu chuẩn về độ rõ, nhịp nghỉ và mức phạt sai.
3. **Làm C11–C20:** dạy slider/cầu theo các bước nhỏ, đo khả năng hiểu nguyên nhân–kết quả và đường hồi phục; hoàn thiện boss khu vườn.
4. **Làm C21–C40:** chốt readability 3D trước phối hợp hai bi; chỉnh P23 theo tải/contact thực, không mang nguyên độ hẹp của bài stress test vào campaign.
5. **Làm C41–C60:** cam có các nấc mang ý nghĩa, cửa con lắc có thể mở ổn định, đường cứu bi bay được thiết kế lại. Duyệt boss 50/60 bằng whitebox trước art.
6. **Làm C61–C80:** hai chương môi trường có bài đối chiếu rõ ràng. Kiểm tra collider/biên chất lỏng/VFX cùng nhau; không thêm động học chất lỏng cho prop khi chưa được mô hình hóa.
7. **Làm C81–C100:** topology và chuỗi phối hợp có chủ đích, không cộng dồn chiều dài/thất bại. Dành vòng playtest riêng cho nhịp mệt mỏi và boss cuối.
8. **Nghiệm thu bản đầu:** đủ 100 màn/10 boss, không thiếu bài dạy, save/continue ổn định, kiểm thử cơ cấu/lifecycle và route trên nội dung thật, chơi chặng trên thiết bị mục tiêu, hiệu năng và replay/share tự chọn đạt. Mastery là nội dung thêm, không thay thế các màn còn thiếu.

Triển khai theo lô 10 màn để học từ playtest, nhưng phạm vi bản phát hành đầu vẫn là 100 màn. Mỗi chặng phải đạt trải nghiệm mục tiêu trước khi nhân số lượng. Chưa ước lượng ngày công chính xác khi chưa có số người, ngân sách art và thiết bị mục tiêu.

## 10. Cơ sở tham khảo và giới hạn

[GameFlow — Sweetser & Wyeth, 2005](https://www.valuesatplay.org/wp-content/uploads/2007/09/sweetser.pdf) cung cấp khung xem xét thử thách phù hợp kỹ năng, mục tiêu/feedback rõ, cảm giác kiểm soát và phục hồi sai. Bài gốc thử bằng expert review hai game RTS; không cung cấp công thức D hoặc tỷ lệ hoàn thành cho Gravity Box.

[The AI Systems of Left 4 Dead — Michael Booth, Valve, 2009](https://steamcdn-a.akamaihd.net/apps/valve/2009/ai_systems_of_l4d_mike_booth.pdf) trình bày nhịp cường độ có đỉnh và khoảng nghỉ. Việc áp dụng nguyên tắc nhịp chơi vào các chặng 10 màn ở đây là suy luận thiết kế cho game khác thể loại; không đề xuất bê AI Director hoặc độ căng của game kinh dị vào Gravity Box.

Tất cả trọng số, target D, kích thước biến thể, thời gian, tỷ lệ và ngân sách công sức trong tài liệu là đề xuất cần kiểm chứng. Bằng chứng 129 test của prototype xác nhận các hợp đồng đã thử; không đo độ thư giãn, cân bằng campaign hoặc khả năng viral.
