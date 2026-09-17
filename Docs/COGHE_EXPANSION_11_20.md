# COghe — chương 11–20

Nội dung dựa trên các phác thảo trong `Docs/LevelDesign/COghe/Level11` đến `Level20`.
Các scene nằm trong `Assets/_Game/Venom/Campaign/VenomOrigin11.unity`…`VenomOrigin20.unity`.
Giữ nguyên chương 01–10, save `venom.origin.v2`, khối lượng mô và 32 hạt vật lý.

## Màn chơi và điều khiển

| Màn | Cơ quan chính | Xoay hộp |
| --- | --- | --- |
| 11 · Kê cao lên | Đẩy/kéo thùng cao làm bậc vượt dải trơn | Khóa |
| 12 · Trượt rồi bay | Leo bệ, trượt máng, bay và bám vùng đón | Khóa |
| 13 · Mở đường | Cần gạt → cửa phòng → nút bên trong → nắp ống uốn | Khóa |
| 14 · Đổi chiều | Cầu bản lề và cửa trượt theo hai hướng trọng lực | Có |
| 15 · Tìm lối ra | Mê cung ống có vòng nối, ngã ba và đầu cụt | Có |
| 16 · Sáu ngả | Cầu trơn, sáu ống cao su; năm đầu bịt và một đầu hở | Có |
| 17 · Nối bánh răng | Dịch hai ổ trượt để nối bộ truyền và nâng cửa | Khóa |
| 18 · Ghép đường | Ba thanh cầu trượt theo ba trục, tự khớp dưới trọng lực | Có |
| 19 · Cùng nhau | Tách cơ thể, giữ A, kéo B, mở lối hợp thể giữa hai khoang | Khóa |
| 20 · BOSS · TAM HỢP | Ba khoang, hai dao, hai ống, bánh răng và tời liên động | Khóa |

Menu chọn chương **01–10 / 11–20** ở trên cùng. Phím **1…9, 0** chọn chương đầu;
**Shift + 1…9, 0** chọn màn 11…20. Chạm phần cơ thể hoặc nút Phần để đổi chủ thể.
Chạm vật có thể thao tác rồi chạm đích để đẩy/kéo; sau ba giây không ra lệnh sẽ buông.
Trong ống, chạm nhánh nối tại ngã ba; chạm lại nhánh vừa đi để quay khỏi đầu cụt.

Boss không hiển thị lời giải. Các phần không được tự đi theo nhau: phần đang giữ
nút tiếp tục giữ khi người chơi đổi phần điều khiển. Cơ thể chỉ tách qua dao và tự
tụ khi đủ gần và không có vách ngăn ở giữa. Phải hợp thể trước khi bất kỳ phần nào thoát
qua lỗ cuối. Các ống nối khoang không tính là lỗ thắng.

## Kiến trúc triển khai

- `VenomCampaignExpansion`: khám phá, reset và cập nhật các `COgheMechanism` của
  scene; tổng hợp điều kiện cửa, chọn phần, trạng thái chảy và cản hợp thể qua vách.
- `COgheRailSlider`: Rigidbody/ConfigurableJoint, hành trình thật, trọng lực thế
  giới, lực cản và chốt đầu hành trình có thể kéo ngược để nhả.
- `COgheTissueSensor`: đo tải mô trên vùng tiếp xúc; mặt nút và đèn phản ánh tải thật.
- `COgheGuillotine`: cảm biến mô → cảnh báo một giây → rơi → cắt liên kết thực tế
  theo vị trí; xung lực sau chém chỉ tác động nhóm vừa bị cắt.
- `COgheGearTrain` và `COgheCooperativeWinch`: kiểm tra vị trí ăn khớp, truyền chuyển
  động, motor có lực hữu hạn, khóa tải và chốt cửa khi đã mở đủ hành trình.
- `COgheTubeNetwork`: đồ thị ống rõ nhánh nối; điều khiển mô bằng lực dọc lòng ống,
  dừng để chọn nhánh, quay lại đầu bịt, ống hai chiều và tâm tuyến cao su biến dạng.
  Trạng thái chảy thuộc từng nhóm mô, không vô hiệu hóa phần đang giữ nút ở nơi khác.
  Lỗ nối khoét theo đường cong thật; toàn bộ mô phải vào ngã ba trước khi đổi nhánh.
  `AllowsExitAssist` chặn lực hút lỗ cuối khi đường kéo thẳng còn xuyên thành ống;
  mỗi hạt chỉ chuyển sang lực hỗ trợ thoát khi đoạn cuối đã thông thoáng.
- `COgheSlideLaunchMonitor`: chỉ quan sát trượt, bay và bám; không mở cửa bằng một
  điều kiện lịch sử vô hình. Đường bay do hình máng, tiếp xúc và trọng lực tạo ra.
- `COgheLatchedAccessSequence`, `COgheGravityBridgeAssembly`: cơ quan cửa có chốt
  của màn 13 và phép đo cầu/cửa chuyển động thụ động của màn 14.
- `COgheExpansionArtBuilder`: áp Day Lab, dùng chung vật liệu và cubemap, mesh riêng
  từng scene; trang trí không tạo va chạm hoặc quyết định kết quả giải đố.

Mô phỏng này vẫn là prototype mô mềm 32 hạt, không phải solver chất lỏng bảo toàn
thể tích. Bộ truyền dùng ràng buộc bánh răng lý tưởng; ống mềm dùng tâm tuyến có
ràng buộc, không phải mô phỏng đầy đủ vật liệu cao su. Cần đo hiệu năng trên điện
thoại trước khi đưa ra cam kết FPS.

## Animation và phản hồi

Dùng cùng bộ animation procedural của COghe: xúc tu bám theo tiếp xúc thật khi
leo, thân kéo dài khi kéo vật, dồn thân khi đẩy, mất bám và chùng xuống theo trọng
lực khi trượt/rơi. Máng trượt 12 không dùng animation để phóng cơ thể. Trong ống,
mô chảy theo lòng ống và co gọn; nhóm đang ở ngoài vẫn giữ animation tiếp xúc riêng.
Ngã ba dừng để nhận lệnh chọn nhánh. Nút, cửa, bánh răng và đèn đọc trạng thái cơ
quan thực tế. Sau thắng giữ ba điệu nhảy ngẫu nhiên và camera cận cảnh hiện có.

## Checklist chơi thử

- Thử chỉ sai đích, đổi hướng, bỏ vật sau ba giây và reset giữa lúc cơ quan chạy.
- Màn 12: trượt khỏi mép máng, có khoảng bay thật rồi bám được vùng đón.
- Màn 14/18: xoay hộp làm cơ quan chuyển động do trọng lực; đứng sai vị trí có thể
  rơi và phải tìm đường quay lại. Không dùng một góc xoay như mã mở cửa vô hình.
- Màn 15: thử cả ba ngõ cụt và quay về ngã ba, đi hai chiều vòng nối; chỉ ống cuối
  dẫn tới lỗ thắng. Chạm đường ống tại ngã ba phải chọn được nhánh đang thấy.
- Màn 16: đi vào ống bịt, chọn quay lại rồi xoay miệng ống về phía trên để đưa cơ
  thể về cầu; tìm một trong sáu ống có đầu ra thật.
- Màn 19/20: chọn phần khác không làm phần đang giữ nút tự bỏ vị trí; không hợp
  thể xuyên vách; đủ khối lượng mới kéo được tải nặng. Reset phục hồi toàn bộ dao,
  nút, cửa, bộ truyền và các phần cơ thể.
- Kiểm tra tab 11–20, zoom, chọn từng phần, lỗ thoát, cảnh thắng và chuyển màn.

Boss 20 không hiện checklist hay lời giải trong game. Bộ test giải màn đi qua các
lệnh điều khiển có sẵn và chờ mô phỏng; người chơi vẫn phải khám phá thứ tự dùng
khối lượng, chia cơ thể và phối hợp giữa ba khoang.

## Dựng lại và kiểm chứng

Unity **6000.3.19f1**. Menu **Gravity Box → COghe → Generate Expansion 11–20** chỉ
dựng chương mới và đăng ký đủ 20 scene. Không ghi đè scene 01–10. Các định nghĩa
camera, hình học, trạng thái cơ quan và quyền xoay được tác giả đặt trong các partial
builder; runtime không đọc lời giải từ tests.

Các suite `COgheEarlyExpansionTests`, `COghePipeExpansionTests`,
`COgheMechanismExpansionTests` kiểm tra đường giải bằng lệnh điều khiển và mô phỏng
vật lý. `COgheExpansionIntegrationTests` kiểm tra ID, trạng thái ban đầu, reset và
chụp ảnh cả chương. Các fixture lực/cơ quan riêng không thay thế full solution tests.

Chạy `bash Tools/verify-coghe-expansion.sh` để kiểm tra cả chương mới và hồi quy
01–10. Thêm `--generate` nếu cần dựng lại scene 11–20 trước khi kiểm tra.

Kết quả chạy và ảnh thật nằm trong `Artifacts/COgheExpansion`; những tệp có tên
`first`, `second`, `third` là bằng chứng các lượt tìm lỗi, không phải chứng nhận đạt.
Build macOS bằng `bash Tools/build-venom.sh`, đầu ra `Builds/Venom/macOS/Venom.app`.
APK/iOS cũ không được coi là đã cập nhật chỉ vì có scene mới trong repository.

## Kết quả kiểm chứng — 17/09/2026

Lượt cuối trong `Artifacts/COgheExpansion/verification.xml`: **67/67 đạt, 0 lỗi**
(Unity PlayMode, 234,0 giây).

| Nhóm | Đạt | Phạm vi |
| --- | ---: | --- |
| Origin 01–10 và đầu vào | 44/44 | Hồi quy di chuyển, trơn, ống, dao, hợp thể, thắng/thua, Collection; chém lần hai không hất phần giữ nút khác; kéo nhanh, cấm xoay, chạm, pause/reset/disable |
| Màn 11–14 | 4/4 | Đường giải hoàn chỉnh bằng điều khiển và tiếp xúc vật lý, đủ mô qua lỗ cuối |
| Màn 15–16 | 7/7 | Ba ngõ cụt và quay lại, vòng nối, chạm chọn nhánh, sáu ống mềm, thoát đủ 32 hạt |
| Cơ quan 17–20 | 11/11 | Tải, truyền động, ghép cầu, phối hợp, chạm cơ quan và đường giải hoàn chỉnh tới thắng |
| Tích hợp | 1/1 | Đủ 20 scene/ID, không thiếu script; 10 màn mới đứng yên, reset, quyền xoay và render portrait |

Ảnh Unity thật của lượt kiểm tra nằm ở `Artifacts/COgheExpansion/Frames` và `Pipes`.
Các phép thử giải màn không đặt lại vị trí cơ thể hoặc mở cửa thay cho người chơi.
Fixture riêng cho dao/tải có sắp đặt trạng thái để kiểm tra từng luật và được ghi
rõ trong test. Kết quả này xác nhận các đường giải đã kiểm tra, không thay cho
playtest độ khó với người chơi mới hoặc phép đo hiệu năng trên điện thoại.

Kiểm tra trực tiếp trên bản Mac: đổi tab 11–20, chọn màn 11, chạm thùng và zoom;
chọn nhánh ống màn 15 tới nút A; kéo nhanh màn 16 làm quả cầu đổi hướng;
màn 20 hiện ba khoang, khóa xoay và không hiện lời giải. Kiểm tra chọn nóc
01–03 dùng fixture xoay vỏ hộp và chạm tọa độ màn hình; không phải phép thử
hoàn thành màn bằng cách leo nóc.

Hồi quy màn 07 phát hiện thùng xoay chéo đã chạm kính nhưng điều kiện trèo còn
đo bằng tâm thùng và nửa chiều rộng cố định. Điều kiện nay đo mép mặt trên thật
với khoảng với tới 15 mm như trước; vẫn yêu cầu căn ngang và thùng gần đứng yên.
Hai đường thử giải màn kiểm tra thùng ban đầu thẳng và lệch 12°; không dịch
chuyển vật thể thay cho người chơi trong quá trình giải.
