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
| `VenomLifeAnimation` | Nếp khối, đầu tò mò, xúc tua bám/nhả; tốc độ diễn xuất riêng với tốc độ vật lý |
| `VenomFloorBoundary` | Giới hạn sàn/lỗ thật dùng cho chặn hành trình cơ cấu và vùng tiếp xúc của skin |
| `VenomInput` / `VenomHud` | Một nguồn input nghiêng, trạng thái chia/nhập/thoát |
| `VenomPrototypeBuilder` | Tạo scene, mesh lỗ tròn, profile và build macOS riêng |

Tách/nhập không tạo sinh vật thay thế và không teleport hạt. ID, khối lượng và vận tốc được giữ. Cắt xoá các cạnh liên kết giao với vùng lưỡi; mặt cắt chờ 1,5 giây trước khi có thể hàn lại. Hai hạt đủ gần và không có cơ cấu nằm giữa mới nối lại. Mesh dựng theo nhóm kết nối; phần đang chui qua cửa vẫn được thể hiện liên tục.

### Animation sống

Bề mặt chính có nếp khối chạy chậm và bất đối xứng. Khi trượt, thân kéo theo hướng dòng chảy, mô phía trên có độ trễ. Lúc yên, thỉnh thoảng một vai dồn lên thành đỉnh mềm để thăm dò rồi thu lại; không còn cổ với hai mắt sáng hoặc hướng nhìn cố định về camera.

Mỗi phần có ngân sách tối đa 5 xúc tua trang trí (3 với mảnh nhỏ), không phải bộ chân cố định. Các lần mọc chọn góc mới thiên theo hướng chuyển động, có một số điểm giữ ở phía sau. Đầu sợi raycast lên collider thật và lưu trong tọa độ collider; khi khối dịch chuyển, sợi căng mảnh rồi thu về. Thời gian vươn, giữ, thu và chờ giữa các lần khác nhau. Khi rời mặt đỡ, gặp vật cản, kéo quá xa hoặc vào vùng thoát thì nhả.

Animation **không tạo lực, không có collider, không đè nút và không bổ sung khối lượng**. Vẫn có đúng 32 hạt vật lý/96 g. Tất cả nhịp dùng đồng hồ mô phỏng, đóng băng khi pause và xóa khi reset. Biến dạng hình ảnh có giới hạn, xét khoảng trống quanh cơ cấu; chưa bảo toàn thể tích mesh hay mô phỏng tương tác cơ học riêng cho từng sợi.

Các tham số nằm trong `Living matter.asset`: `AnimationSpeed = 1.5`, `IdleBulge = 0.0036 m`, `CuriousHeadLift = 0.041 m`, `TendrilReach = 0.034 m`. So với lượt animation trước, nhịp nhanh hơn 50%, biên độ phồng tăng khoảng 29%, độ ngóc và tầm vươn tăng khoảng 21%. [Nghiên cứu tư liệu phim, chẩn đoán và lựa chọn animation](VENOM_MOTION_STUDY.md).

Mỗi nút nhận tải từ các hạt thực sự có tiếp xúc trên mặt nút; lượng tiếp xúc tối thiểu 9 g. Logic hai nút kiểm tra nhóm vật chất khác nhau, không chỉ kiểm tra tổng tải. Motor/chốt cửa là cơ cấu có nguồn năng lượng được mô hình hoá chủ ý; sinh vật không nhận lực đẩy từ thao tác mở cửa ngoài tiếp xúc thông thường.

## Tham số và giới hạn

- 1 Unity unit = 1 m; trọng lực 9,81 m/s²; mô phỏng 120 Hz; mesh dựng lại mỗi khung hình hiển thị, mục tiêu 60 fps.
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

- Lần chỉnh theo nghiên cứu symbiote: 9/9 kiểm tra Venom chạy lại thành công, bổ sung kiểm tra render lúc trượt không đổi vật lý và toàn bộ mesh đứng yên khi pause/đổi camera. Các con số 130 PlayMode/8 EditMode phía trên là lần chạy toàn dự án trước đó.

## Sửa tiếp xúc sàn và tăng sức sống — 10/09/2026

- Joint của cửa/lưỡi chém giờ có chặn dưới dựa trên mặt sàn và đáy collider. Trước đó giới hạn đối xứng quanh vị trí nghỉ cho phép cơ cấu đi xuyên sàn; cửa còn hạ khoảng 1 cm dưới vị trí nghỉ do tải trọng. Giới hạn trên của cơ cấu được giữ nguyên. Không tạo một mặt sàn vô hình để bắt vật chất.
- Các hạt vẫn dùng Continuous Dynamic CCD ở 120 Hz. Giới hạn tốc độ giải xuyên tiếp xúc tăng từ 0,6 lên 2 m/s để khối bị ép phục hồi sớm hơn; đây không phải giới hạn vận tốc chuyển động hay giảm trọng lực.
- Skin có mặt tiếp xúc giới hạn theo sàn đặc. Trong phép thử va sàn, hạt thấp nhất nằm khoảng y = −0,058 m nhưng mesh cũ xuống khoảng −0,071 m, trong khi mặt sàn ở −0,067 m. Sau sửa, đáy skin trên sàn ở −0,0668 m. Mô ở lỗ thật và mô đã thoát vẫn được dựng phía dưới sàn.
- Mesh dựng lại mỗi LateUpdate để theo đúng tư thế nội suy của hộp. Ngân sách làm mới 30 Hz trước đó có thể để bề mặt trong không gian thế giới chậm hơn hộp đang xoay.
- Nhịp animation có hệ số riêng 1,5; vẫn đóng băng khi pause, xóa khi reset và không ghi vị trí/vận tốc vật lý.

Kiểm tra hiện tại gồm 12 trường hợp Venom: bổ sung rơi hai phần với tốc độ 2/5/10 m/s, đo riêng hạt/skin, lật nhiều trục trong 32 giây và giới hạn đáy cơ cấu. Đường giải vẫn dùng thao tác xoay và PhysX, giữ nghiêng 26° qua đoạn thu hẹp trước khi giảm góc ở miệng lỗ; hoàn thành đủ 32/32 hạt, mesh vẫn hiện phần thoát. Log/XML và capture nằm trong `Artifacts/VenomCollision`. Đây là kiểm tra các tình huống cụ thể, không phải chứng minh mọi va chạm có thể xảy ra đều không xuyên.
