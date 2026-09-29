# Hợp đồng cơ quan — Spatial 11–30

29/09/2026 · **Thiết kế, chưa triển khai runtime**. Bộ 10 Spatial hiện tại vẫn giữ nguyên.

## 1. Máy lượng tử Q thay cơ quan cắt

Yêu cầu mới của người dùng: một sinh vật/phần cơ thể đi vào máy sẽ tự chia thành **hai phần bằng nhau theo khối lượng**. Áp dụng cho khối lượng phần đi vào, không phải luôn tạo hai phần bằng 50% tổng ban đầu:

- 100 → 50 + 50.
- Một phần 50 vào tiếp → 25 + 25; phần 50 đang ở ngoài giữ nguyên. Tổng trạng thái là 50 + 25 + 25.
- Cho nửa 50 còn lại vào → bốn phần 25.
- Hợp 25 + 25 → 50; hợp tất cả → 100. Không sinh thêm vật chất.

Trong **lời giải của bộ 20 màn này** tối đa bốn phần; không đồng nghĩa đã chốt giới hạn toàn game. Bản nháp sử dụng 4 phần theo giả định thiết kế; có thể điều chỉnh sau khi người dùng xem, chưa nâng lên 8 phần ở Boss 30. Ngưỡng tách nhỏ nhất là quyết định kỹ thuật còn mở, phải công bố rõ trước triển khai. Không âm thầm chia gần đúng vì thiếu hạt: baseline 32 hạt hỗ trợ chính xác 32→16→8. Nhập các phần khác kích thước rồi tách tiếp cần kiểm tra riêng; nếu không đủ độ phân giải để chia đúng, máy phải báo không nhận và trả phần ra an toàn. Không đổi số hạt/khối lượng giữa các cấu hình máy để làm đẹp hình.

### Hình học và trạng thái

Một cửa vào rộng, khoang quét nhìn xuyên được, hai khay ra, vách phân luồng thật ở giữa. Khoảng hai khay > khoảng tự tụ hiện hành cộng dung sai chuyển động; mặt chặn ngăn nối mô xuyên qua. Không dùng cooldown hoặc trạng thái nhiệm vụ để ngăn hợp thể.

`Idle → Receiving → Gathered → Separating → Clearing → Idle`.

- Chỉ xử lý nhóm đang vào, sau khi toàn bộ mô của nhóm đã qua vùng nhận và hai lối ra còn trống. Hai nhóm cùng tiến vào: nhóm đến sau chờ ở ngoài; không nuốt mô hoặc trộn ID ngẫu nhiên.
- Tự chạy khi vào đủ; không thêm nút “cắt”. Một lượt vào chỉ kích hoạt một lần. Mô đứng ở khay ra không bị chia liên tiếp mỗi frame. Phải đi ra khỏi vùng nhận và quay vào để tách tiếp; đây là điều kiện hình học, không phải thời gian chờ hợp thể.
- Dùng lực hữu hạn để gom và tạo hai thuỳ, rồi đổi topology thành hai nhóm cân bằng. Không teleport hạt vào hai vị trí đặt trước. Phân vùng phải giữ mỗi nhóm liên thông, đúng khối lượng, không có hạt kẹt trong vách. Đây là rủi ro cần prototype trước, chưa có bằng chứng solver hiện tại làm được.
- Các đường ra vật lý giữ hai thuỳ riêng trong lúc phân luồng. Nếu gặp nhau ngoài máy và không có vật cản, tự nhập ngay theo luật chung.
- Khi nhận lệnh tách, chỉ huỷ tác vụ của nhóm vào máy; nhóm khác giữ nút hoặc tiếp tục tác vụ. Sau tách chọn nhóm ở khay trái theo ID ổn định; UI đánh số phần và hiện % khối lượng, chạm phần/nút phần để đổi chọn.
- Đổi đích trước pha đổi topology: cho rút lui. Sau pha đổi topology: hoàn tất đưa ra khay an toàn bằng lực, rồi nhận lệnh mới. Pause không chạy tiếp; Retry khôi phục đủ vật chất và một nhóm ban đầu.

### Ngoại hình và animation

Vỏ ngà bo nhỏ, kính quan sát, hai biểu tượng giọt bằng nhau, nhãn Q. Ánh quét trắng lạnh nhẹ bằng material/particle, không thêm realtime light, không tím trơn/mint thoát. COghe tò mò chạm máy → trải mềm vào khoang → kéo thành hai thuỳ → hai phần rung nhẹ, nhìn hướng người chơi bằng dáng cơ thể. Không cưa/dao, không máu, không nổ văng.

Animation đọc các pha thật; không “đến cuối animation thì gán chia xong”. Âm thanh ngắn, êm, tránh ù lặp liên tục.

## 2. Đẩy/kéo và lắp khối

Tái dùng `COgheTapRail`, `COgheRailSlider`, `VenomMovableProp`. Khối đầu chương đi theo ray/hốc đủ rộng, tay nắm còn tiếp cận được ở cả hai đầu; không mở ngay điều khiển xoay tự do nhiều trục. Chạm tay nắm → chạm vị trí hợp lệ → tự đến đứng trên mặt bám → tác động lực hữu hạn. Kéo ngược để sửa sai.

Hốc chỉ chốt khi pose/tiếp xúc thực nằm trong dung sai; silhouette không tự hút/teleport cả khối. Một sàn ghép chỉ cho navigation đi qua khi mặt thật khớp và khe nhỏ đủ cho mô. Trường hợp tải lớn ở 25/29/30: hiệu chỉnh lực cản/đối trọng bằng thử nghiệm để phần 25% không đủ, 50% đủ cho khối; tời cuối 100% thắng tải. Không đặt `if(level == ...)` hoặc mở khóa chỉ vì đếm số phần. Ngưỡng 40%/75% là mục tiêu thiết kế lực, chưa phải số tuning đã kiểm chứng.

## 3. Ròng rọc, tải và thang

Tái dùng `COghePulleyDrive`, `COghePassengerLift`; mở rộng khả năng chở/đo tải props cho màn 13 và đối trọng màn 21. Tời dây truyền lực thật; chốt giữ tải nhìn thấy rõ. Có gọi thang về ở hai bến. Chỉ khóa chuyển động khi đủ mô/đồ nằm trên khay; không parent sinh vật để giả chở.

Nút giữ (`Hold`) và chốt (`Latch`) dùng hai hình dạng khác nhau: mặt cap lún theo tải và móc chốt cơ học kèm dấu khóa. Không đổi luật nút âm thầm giữa các màn. Mọi bài nhiều phần phải có bước giải phóng người giữ trước khi nhập.

## 4. Đu dây

`COgheSwingTransfer` là **component mới cần prototype**. Một đầu neo, một dây/constraint chiều dài hữu hạn, vòng bám ở đầu; không chuỗi hàng chục rigidbody. Sinh vật bám bằng các điểm tiếp xúc/xúc tu. Có cơ cấu tời kéo về tư thế xuất phát rồi nhả, lực và động lượng quyết định cung đu.

Người chơi chọn bến đến thay vì canh nhả đúng frame. Bộ thực thi chỉ chuyển sang bám bến khi mô thực nằm trong vùng tiếp xúc hợp lệ, còn dây giữ an toàn trước đó. Bến rộng và gối đón thật; không bật dính từ xa/teleport hoặc phóng vận tốc theo kịch bản. Không đủ tầm: ở lại dây, quay về bến hoặc rơi xuống sàn cứu hộ có đường lên. Màn 19 yêu cầu đưa bến vào cung đu trước, không vừa kéo bến vừa căn nhảy.

Độ dễ điều khiển đến từ hình học và cơ cấu hỗ trợ, không sửa trọng lực. Hệ tự canh chuyển bám là hỗ trợ thi hành lệnh; không tự chọn bến đúng thay người chơi.

## 5. Ống và chuyển tuyến

Tái dùng `COgheTubeNetwork`, mở rộng van định tuyến nếu cần. Miệng/đường tâm/va chạm phải liên tục; lỗ chuyển khoang dùng loại `Transfer`, cuối mới là `FinalExit`. Trong ống chạm trên vỏ hộp không kéo mô ra ngoài. Tại junction, dừng toàn bộ nhóm trong khoang đủ rộng rồi chọn nhánh; có lối quay lại. Hai ống chéo chỉ nối nếu có cổ nối/cửa sổ junction thật.

Van có interlock chống kẹp: hoãn chuyển khi mô còn trong đoạn động, có phản hồi “đang có mô”. Ống không tự phân chia nhóm. Cơ thể nguyên khối có thể chảy qua ống hẹp nếu thông tuyến; **không dùng tiết diện ống đơn thuần để giả giới hạn tổng khối lượng**. Khi cần giới hạn tải phải dùng cơ cấu cân/tải thấy được.

## 6. Luật chung, không thay bằng art

- Hộp đứng yên, kéo đổi góc camera; pinch zoom. Đây là phần tiếp nối Spatial ViewOnly, không đưa lại bài căn nghiêng/rơi vốn gây khó điều khiển.
- Mặt trơn vẫn chọn được nhưng không có lực bò/bám. Dải trơn quanh đích và mặt phân khoang phải chặn cả đường tắt qua tường/nóc; đường cứu hộ không được vô tình nối thẳng tới đích.
- Gần nhau và không vật cản thì tự tụ, kể cả đang giữ nút hoặc khác nhiệm vụ.
- Hợp thể hoàn toàn **bên trong** trước khi bất kỳ phần nào đi qua lỗ cuối. Ra sớm: “bạn phải hợp thể trước khi chui ra”. Ống chuyển không kích hoạt luật thắng/thua này.
- Đèn/vệt mạch biểu diễn trạng thái cơ quan thật. Nét mạch cố định bám sàn/kính; dây chịu lực vẫn là dây. Hai circuit xanh A/coral B tối đa trong vùng quan sát; chữ/glyph bổ sung cho màu. Không đổi màu trạng thái làm mất liên hệ A/B.
- Mọi blueprint là dữ liệu authoring. Runtime không đọc lời giải, không thay lực/luật theo số level.
