# COghe — phản hồi điều khiển và nắp màn 09

Ngày 16/09/2026. Áp dụng cho 10 màn Origin.

- Chạm kính: vòng sóng tại điểm nhận lệnh, viền mặt kính và lớp sáng mờ trong khoảng
  một giây. Viền nằm vào phía trong để không bị khung kim loại che; lớp sáng mờ
  không phủ qua mặt có lỗ. Vòng đích nhỏ đi theo mặt kính hoặc đồ vật khi chúng chuyển động.
  Lệnh tới lỗ đánh dấu tâm lỗ thật; lệnh đẩy/kéo đánh dấu vị trí trên sàn.
- Màn 01: mũi tên hổ phách nhấp nhô nhẹ trên lỗ thoát.
- Từ màn 03: biểu tượng xoay đứng yên, cố định phía dưới hộp. Các màn
  07, 08, 10 hiện biểu tượng gạch chéo và chữ “Không thể xoay”.
- Màn 07: mũi tên trên thùng biến mất sau khi sinh vật bám vào thùng lần đầu.
- Màn 08: ký hiệu nhỏ trên phần nóc an toàn; sau khi sinh vật leo lên, ký hiệu
  chuyển tới mép trơn phía ống. Người chơi vẫn tự ra lệnh cho từng bước, căn rơi,
  rồi chạm miệng ống sau khi sinh vật bám vào vành.
- Khi phân tách: một mũi tên mint theo đúng phần đang được chọn, cả khi chọn bằng
  chạm lên cơ thể và bằng nút “Phần”. Đổi phần bỏ dấu đích của phần cũ.
- Retry xóa dấu chạm; thắng/thua/Nhà ẩn toàn bộ dấu hướng dẫn. Boss chỉ có phản hồi
  chọn phần/đích và trạng thái khóa xoay, không có gợi ý lời giải.

`COgheControlFeedback` chỉ đọc kết quả nhận lệnh của `VenomCampaign`. Nó không
raycast lần thứ hai, tự chọn mặt khác, đổi đường đi hay tạo collider. Tám đường
vẽ và một quad tô mặt được tạo một lần, dùng chung một material; bật những phần đang cần. Shader
alpha có depth test; không thêm đèn hoặc reflection realtime. Icon HUD được tạo
một lần từ nét vẽ, không phụ thuộc font ký hiệu trên Android.

Nắp màn 09 giữ nguyên kích thước ngoài 15×15×10 cm, đáy hở. Thành tăng từ 8 lên
14 mm, khối lượng từ 45 lên 180 g. Collider hộp từng thành, vật liệu tiếp xúc,
solver và CCD được cập nhật. `VenomPropContainment` khôi phục tiếp xúc còn xuyên
nhẹ sau khi thành hộp xoay qua nắp, đồng thời bỏ vận tốc tương đối hướng xuyên
tường; vẫn giữ chuyển động trượt tiếp tuyến và tác động của trọng lực. Ràng buộc
này chỉ dùng cho nắp lớn hơn đường kính lỗ thoát, không dùng cho sinh vật.

Tái áp dụng vào scene hiện có: **Gravity Box → COghe → Upgrade controls and
reinforced lid**. Không cần Generate lại thiết kế các màn. Bộ sinh campaign cũng
áp dụng cùng cấu hình để các lần tạo lại không làm mất thay đổi.

Kiểm thử: bộ `GravityBox.Tests.VenomOriginTests`, bao gồm nhận mặt chạm trong màn
03, hai bước theo dấu nóc màn 08, sinh vật leo lên nắp, xoay nhanh sáu chuỗi góc,
lật nắp để mở lỗ và hoàn thành màn. Báo cáo và render tại
`Artifacts/COgheControls/`; đây không phải số liệu đo FPS trên điện thoại.

Kết quả: **38/38 PlayMode tests đạt**. Bài xoay nắp ghi nhận sai lệch góc ngoài
lớn nhất khoảng 0,18 mm; độ dao động đứng khi sinh vật ở trên nắp dưới mức hiển
thị 0,01 mm của log. Đây là các tình huống kiểm thử xác định, không phải cam kết
cho mọi cách thao tác. Dữ liệu vật lý màn 01–08 và 10 không thay đổi.

[Ảnh render trong Unity](ArtDirection/COghe/Controls/README.md).

## Sửa chọn mặt nóc — 17/09/2026

Mặt nóc các hộp Origin nhận lệnh chạm khi xoay để nhìn thấy mặt trong.
Trước đây `Selectable = false` khiến nó bị bỏ qua ở mọi hướng xoay, dù kính
vẫn nhìn thấy. Các scene hiện có và bộ dựng `Cube` đã cùng bỏ giới hạn này.
Giữ quy tắc tia chọn: kính phía gần nhìn từ ngoài vẫn cho chạm xuyên vào trong,
trừ mặt được thiết kế nhận chạm từ ngoài (mặt trước bài 03 và nóc bài 08).
Nhờ vậy nóc không chặn lỗ sàn bài 01, còn bài 03 vẫn cần xoay để chọn lỗ phía sau.
Bài kiểm tra hồi quy chạm bằng tọa độ màn hình ở bài 01–03 sau khi xoay hộp.
