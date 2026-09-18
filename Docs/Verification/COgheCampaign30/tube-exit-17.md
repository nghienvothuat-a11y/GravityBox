# Màn 17 — giữ cơ thể liền khi ra ống

Ngày 18/09/2026. Màn hiển thị 17 là content 13 **Mở đường!** trong campaign 30
màn, không phải content 17 bánh răng. Giữ nguyên bố cục và lời giải được duyệt.

## Nguyên nhân và thay đổi

Trước sửa, liên kết dữ liệu vẫn báo một bản thể nhưng lực hút lỗ cuối tiếp quản
từng hạt đầu với tốc độ lớn hơn dòng trong ống. Sau khi một hạt được tính đã
thoát, nó lại chịu lực gom ngoài hộp; phần đuôi vẫn theo đường cong. Hình học da
metaball có thể bị đứt dù còn nối bởi lò xo dài. Khoảng nối lớn nhất quan sát
được trong lượt baseline là khoảng 28 mm.

`COgheTubeNetwork` giờ giữ quyền dẫn cả đầu đã ra và đuôi chưa ra cho tới khi
toàn khối qua miệng. `VenomCampaign` không cộng lực gom lên phần đầu còn do ống
dẫn. Hạt giữ collider trong quá trình này; không teleport, không sửa khối lượng,
không ép hợp thể và không đổi luật thắng. Máy dò lỗ vẫn đếm riêng đủ 32 hạt thật.
Fix nằm trong cơ chế ống chung, không kiểm tra số level.

## Kiểm chứng

- Lời giải qua chạm màn hình thật trong bản integrated 17 và source 13: đến
  cần A, kéo qua nấc, vào cửa, đè B, chạm miệng ống, đi hết đường cong, thắng,
  Retry phục hồi cửa và nắp. Không gán vị trí mô hoặc trạng thái cửa để giải.
- Lượt integrated có thêm chạm nhầm sàn khi đầu đang ra. Ống tiếp tục dẫn đúng.
- Kiểm tra từng bước vật lý: đủ khối lượng, không có lần cắt, luôn một bản thể,
  collider còn bật khi ống điều khiển; khoảng nối lớn nhất giới hạn dưới 24 mm
  (bản sửa đo 20.9 mm trong hai đường giải).
- Dựng lại mesh da thật mỗi 0.1 giây mô phỏng. Hàn các đỉnh trùng vị trí, đếm
  thành phần liên thông theo tam giác; luôn một mảnh đáng kể. Bỏ qua mảnh cực
  nhỏ dưới 2% diện tích da để không coi chi tiết xúc tu là cơ thể bị chém.
- Kiểm tra đường ống nhiều nhánh, ngõ cụt/quay đầu, nhận chạm, vách chặn hợp thể,
  cùng các hồi quy Origin về cắt/hợp thể/lỗ cuối. Kết quả suite ghi bên dưới.

Suite cuối: **63/63 Passed**, `Artifacts/COgheCampaign30/tube17-regression.xml`,
kết thúc `2026-09-18 06:13:09Z`. Phạm vi: `COgheEarlyExpansionTests`,
`COghePipeExpansionTests`, `VenomOriginTests`. Đây không phải toàn campaign 30;
lỗi màn 5 đã ghi trong lượt kiểm trước nằm ngoài thay đổi này.

`bash Tools/build-venom.sh` đã build thành công, log
`Artifacts/Venom01/build-macOS.log` có `Build Finished, Result: Success` và
`ORIGIN BUILD SUCCESS`. Đã mở player mới, vào màn 17 và kiểm tra Retry bằng chuột.
Chứng cứ đi trọn đường giải/đo liên tục lớp da ở trên đến từ PlayMode; chưa xác
nhận thêm một lượt giải trọn màn bằng chuột trên native player. Player được để
sẵn tại trạng thái đầu màn 17 cho người dùng test.

Đây là kiểm tra chức năng và hình ảnh trên Mac, không phải đo FPS trên OPPO.
Không thêm hạt, mesh hoặc collider runtime; số hạt vẫn là 32. Chưa build APK
hay đo lại trên điện thoại cho thay đổi này.

![Phần đầu và đuôi cùng dòng chuyển động](level17-continuous-head.png)
![Phần đuôi ra khỏi miệng ống](level17-continuous-tail.png)
