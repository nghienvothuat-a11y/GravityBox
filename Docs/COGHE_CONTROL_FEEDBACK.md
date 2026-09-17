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
  chuyển tới mép trơn phía ống. Người chơi chỉ đường lên nóc và căn cú rơi.
  Sau khi bắt được vành và hãm lại, sinh vật tự chui vào ống, không cần chạm thêm.
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

## Quy tắc mặt nóc và vật liệu trơn — 17/09/2026

Áp dụng cho cả 20 màn: mặt nóc nhận chạm từ ngoài lẫn trong, kể cả màn khóa xoay.
Kính và vùng trơn đều là mặt nhận lệnh; tính chọn được không phụ thuộc lực bám.
Bộ dựng scene và dữ liệu đã lưu cùng áp dụng quy tắc này.
Các mặt kính khác vẫn dùng hướng nhìn để chọn mặt trong; mặt trước bài 03 nhận
chạm từ ngoài để giữ bài học xoay tới đúng mặt có lỗ.

Khi chỉ tiếp xúc vật liệu trơn, COghe có thể chạy động tác cố bò và trượt xúc tu,
nhưng lệnh không sinh lực kéo, lực đẩy hay bù trọng lực. Chuyển động thực vẫn do
trọng lực, quán tính và va chạm. Vành bám quanh ống vẫn có lực bám đúng phạm vi.
Kiểm tra gồm chạm nóc bằng tọa độ màn hình ở 20 màn, đường giải bài 01/08,
và đối chiếu quỹ đạo có/không ra lệnh trên mặt trơn phẳng và cầu.

Đã xác nhận trực tiếp trên bản Mac: chạm nóc màn 01 và nóc trơn màn 07 (khóa xoay) hiện dấu đích/viền đúng trên nóc, sinh vật nhận lệnh tiếp cận.
