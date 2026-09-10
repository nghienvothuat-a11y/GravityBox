# Venom 01 — Một cơ thể, hai ý chí

Prototype trên nhánh `Venom`, phát triển từ bản 23 thí nghiệm trước campaign 100. Scene riêng tại `Assets/_Game/Venom/Venom01.unity`. Mục tiêu là kiểm tra một sinh vật mềm có thể tách để phối hợp rồi nhập lại dưới cùng một thao tác nghiêng hộp.

## Vòng chơi

1. Khối vật chất ở đầu hộp. Nghiêng nhẹ về phía máy chém; quan sát khối chảy, bẹp và kéo dài.
2. Lưỡi chém trượt trên ray theo trọng lực, cắt các liên kết đi qua mặt lưỡi. Vách giữa giữ hai phần cách nhau khi đi vào khu vực nút A/B.
3. Hai phần khác nhau phải cùng đè đủ vật chất lên hai nút trong 0,22 giây. Cửa có motor trượt lên và chốt giữ mở. Người chơi có thể tập trung đưa cả hai ra sau cửa.
4. Nghiêng để đưa hai phần sát nhau trong khoang thu hẹp sau cửa. Hai vách kính có viền mảnh dẫn các phần về cùng một chỗ bằng lực tiếp xúc. Bề mặt nối lại, lực liên kết tăng dần trong 0,65 giây, có ánh xanh rất nhẹ tại thời điểm nhập.
5. Đưa khối tới lỗ tròn có viền sáng mảnh. Hỗ trợ cục bộ căn khối vào cửa; hạt vẫn có collider và phải đi qua lỗ thật. Hoàn thành khi cả 32 hạt đã thoát.

Cửa được chốt sau khi giải nên không yêu cầu một phần ở lại nút mãi. Nhập lại là hành vi vật lý có thể thử; điều kiện thắng vẫn cho phép các phần thoát lần lượt nếu người chơi tìm được cách. Bài đầu cố ý bố trí hai nút đối xứng để giới thiệu mechanic, chưa phải bài kiểm tra phối hợp khó.

## Điều khiển và cách thử

Kéo chuột trái hoặc một ngón tay trong vùng hộp để xoay; thả tay giữ hướng. Tâm xoay ở chính giữa hộp. Root kinematic quay bằng `MoveRotation`; sinh vật, lưỡi chém, nút và cửa là Rigidbody động trong không gian thế giới. Trọng lực luôn hướng xuống thế giới; xoay hộp không xoay trọng lực.

Nắp và cửa dùng kính để nhìn xuyên. Khi nhìn từ mặt dưới hộp, sàn giảm độ đục để tiếp tục theo dõi sinh vật; collider không thay đổi.

**R** thử lại toàn bộ; **P/Esc** tạm dừng; **F12** lưu `venom.png` vào thư mục persistent data của app. Bản macOS: `Builds/Venom/macOS/Venom.app`. Chưa xuất APK cho biến thể này.

Nên thử các tình huống: nghiêng chậm rồi đảo chiều để cảm nhận độ trễ, ép khối sát vách, đưa hai phần về hai bên khác nhau, giữ một nút mà bỏ nút còn lại, ghép hai phần sau cửa, và quan sát khối kéo thành cổ khi xuống lỗ. Thử lại sau khi đã tách hoặc thoát một phần để kiểm tra reset.

## Kiến trúc

Assembly `GravityBox.Venom` dùng lại rotation/timing của nền tảng, không phụ thuộc `BallController` hay luật roster bi thép.

| Thành phần | Trách nhiệm |
| --- | --- |
| `VenomProfile` | Thông số vật chất, vật liệu tiếp xúc, độ phân giải bề mặt |
| `CohesiveOrganism` | 32 hạt, lực liên kết, đồ thị kết nối, tách/nhập, bảo toàn khối lượng |
| `VenomContact` / `VenomPressurePlate` | Nhận tiếp xúc PhysX, lượng vật chất trên nút, chuyển động nút lò xo |
| `VenomLevelController` | Quy tắc hai nút, cửa, đường thoát, reset và hoàn thành |
| `VenomSurface` | Metaball + marching tetrahedra; chỉ dựng hình, không điều khiển vị trí hạt |
| `VenomLifeAnimation` | Bề mặt nhấp nhô, đầu tò mò, xúc tua bám/nhả và chuyển trạng thái theo chuyển động/tiếp xúc |
| `VenomInput` / `VenomHud` | Một nguồn input nghiêng, trạng thái chia/nhập/thoát |
| `VenomPrototypeBuilder` | Tạo scene, mesh lỗ tròn, profile và build macOS riêng |

Tách/nhập không tạo sinh vật thay thế và không teleport hạt. ID, khối lượng và vận tốc được giữ. Cắt xoá các cạnh liên kết giao với vùng lưỡi; mặt cắt chờ 1,5 giây trước khi có thể hàn lại. Hai hạt đủ gần và không có cơ cấu nằm giữa mới nối lại. Mesh dựng theo nhóm kết nối; phần đang chui qua cửa vẫn được thể hiện liên tục.

### Animation sống

Khi một phần có tiếp xúc đỡ và vận tốc trượt thấp, bề mặt phồng/lõm không đồng pha (biên độ cấu hình 2,8 mm). Sau khoảng 2–3 giây yên ban đầu, một cổ và đầu nhỏ nhô lên tối đa 3,4 cm, nhìn trái/phải rồi thu lại trong khoảng 3 giây. Những lần nhìn tiếp theo cách nhau lâu hơn, có lệch nhịp theo từng phần. Hai đốm mắt nhỏ giúp thấy hướng liếc; đầu và cổ là phần nối liền của field metaball.

Khi chuyển động tương đối với mặt đỡ tăng, đầu thu xuống và 4–6 xúc tua xuất hiện ở rìa mỗi phần. Mỗi xúc tua chạy vòng **vươn → giữ → co/nhả** lệch pha. Đầu bám được raycast lên collider hộp/cơ cấu thật và lưu trong tọa độ collider đó, nên nó theo đúng sàn khi người chơi xoay hộp hoặc nút trượt. Đầu bám đứng tại một điểm trong pha giữ, trong khi gốc tiếp tục theo khối; nhịp tăng khi trượt nhanh. Không đặt chân qua vách hoặc trên khoảng rỗng của lỗ. Khi rời mặt đỡ, bị kéo quá xa hoặc vào vùng thoát, xúc tua nhả ra.

Những chi tiết này là animation thể hiện ý định sinh vật đang cố bò: **không tạo lực, không có collider, không đè nút và không tính là vật chất bổ sung**. Vẫn có đúng 32 hạt vật lý/96 g. Cổ, đầu và xúc tua chưa có tương tác cơ học độc lập. Animation dùng đồng hồ mô phỏng: pause đóng băng cả nhịp sống, reset xoá đầu/điểm bám, tách/nhập cập nhật animation theo nhóm hạt ổn định.

Các tham số hình ảnh nằm trong `Living matter.asset`: `IdleBulge`, `CuriousHeadLift`, `TendrilReach`. Giới hạn khoảng trống được kiểm tra trước khi nhô đầu; kích thước và số xúc tua được giữ nhỏ để nhìn rõ đường đi của khối.

Mỗi nút nhận tải từ các hạt thực sự có tiếp xúc trên mặt nút; lượng tiếp xúc tối thiểu 9 g. Logic hai nút kiểm tra nhóm vật chất khác nhau, không chỉ kiểm tra tổng tải. Motor/chốt cửa là cơ cấu có nguồn năng lượng được mô hình hoá chủ ý; sinh vật không nhận lực đẩy từ thao tác mở cửa ngoài tiếp xúc thông thường.

## Tham số và giới hạn

- 1 Unity unit = 1 m; trọng lực 9,81 m/s²; mô phỏng 120 Hz; mesh làm mới tối đa 30 Hz.
- 32 hạt, bán kính collider 9 mm, mỗi hạt 3 g; tổng 96 g. Hộp bên trong rộng 50 × 64 cm, sâu khoảng 14 cm; lỗ thoát đường kính 7 cm.
- Lực đàn hồi liên kết 2,6 N/m; cản vận tốc tương đối 0,009 N·s/m; chiều dài nghỉ biến đổi dẻo theo cấu hình. Lực cặp bằng nhau và ngược chiều; không điều khiển một tâm blob thay cho từng phần.
- Hỗ trợ thoát trong phạm vi 5,5 cm theo phương mặt lỗ và 6 cm phía trong; gia tốc hỗ trợ tối đa 4,5 m/s². Đây là hỗ trợ gameplay theo yêu cầu trước đó, chỉ hoạt động sau khi cửa mở.
- Mô hình là khối hạt có tính kết dính/nhớt/dẻo, chưa giải Navier–Stokes/SPH, không bảo toàn thể tích bề mặt, không mô phỏng sức căng bề mặt đã hiệu chuẩn. Khối lượng được bảo toàn; thể tích nhìn thấy có thể thay đổi khi biến dạng.
- Bề mặt và collider có sai số ở quy mô từng hạt; vật chất không thể luồn qua khe nhỏ hơn đường kính collider. Không hỗ trợ hàng nghìn hạt hoặc vết cắt tuỳ ý ở độ phân giải cao.
- Các thông số được chọn cho sinh vật giả tưởng. Chưa thể gọi là mô phỏng một chất lỏng ngoài đời với độ nhớt đo được. Đánh giá feeling cuối cùng cần chơi bằng tay; đường giải tự động chỉ xác minh tính khả thi cơ học.

## Tạo lại, build và kiểm chứng

Scene/profile/mesh đã được lưu trong Git; không cần Generate để chơi. **Gravity Box → Venom → Generate Experiment 01** sẽ tạo lại nội dung, ghi đè thay đổi author trong scene và thông số builder quản lý. **Gravity Box → Venom → Build macOS** hoặc `bash Tools/build-venom.sh` xuất riêng app Venom. `Tools/build.sh` vẫn dành cho lab bi thép.

`VenomPrototypeTests` kiểm tra khối nghỉ ổn định, khối lượng qua cắt, vùng cắt, ngăn hàn ngay lập tức, đường giải bằng rotation/PhysX thực, hai nút không thể được giải bởi một nhóm, không thắng khi mới thoát một phần và reset. Đường giải tự động không đặt vị trí sinh vật: chỉ đổi orientation mục tiêu của hộp, gồm đoạn nghiêng ban đầu rồi phản hồi vị trí/vận tốc để đưa khối về khoang nhập và lỗ.

Log/XML/ảnh QA đặt trong `Artifacts/Venom01`, build trong `Builds/Venom`; các thư mục này không commit. Chạy test với `VENOM_CAPTURE_DIR` và graphics enabled để lưu sáu ảnh từ nguyên khối tới thoát. Ảnh test là render camera từ mô phỏng, không bao gồm HUD native.

### Kết quả ngày 10/09/2026

- **130/130 PlayMode** qua, gồm 121 kiểm tra của lab bi thép và 9 kiểm tra Venom. **8/8 EditMode** qua.
- Đường giải dùng PhysX/rotation thực đã ghi nhận cắt, hai nút cùng giữ, cửa mở, nhập lại và đủ 32/32 hạt thoát. Các lần thử chỉ nghiêng quá nhẹ dừng ở vách thu hẹp; bản kiểm chứng cuối tăng độ nghiêng ở đoạn này như một thao tác người chơi thực hiện được.
- Build macOS thành công bằng Unity 6000.3.19f1. Đã mở app riêng, kiểm tra HUD, kéo chuột làm hộp và sinh vật di chuyển, reset và chụp ảnh native. Chưa kiểm thử cảm giác dài hạn, Android hoặc touchscreen cho biến thể này.
- Bản cập nhật animation đã build lại macOS: kiểm tra đủ vòng ngóc đầu/thu xuống, đầu bám xúc tua giữ đúng điểm trên collider, nhả khi không còn mặt đỡ, pause/reset và bảo toàn vị trí/vận tốc vật lý qua bước dựng hình. [Ảnh cận cảnh animation](Images/VenomLife/README.md).
- [Ảnh trạng thái và nguồn ảnh](Images/Venom01/README.md).
