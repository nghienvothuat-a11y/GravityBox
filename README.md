# Gravity Box — Campaign 100

Prototype Unity có **100 màn campaign, 10 chương và 10 boss ở các mốc 10–100**, cùng **23 màn Physics Lab** để so sánh cơ chế. Màn khô giữ chung bi thép 30 mm / 111 g, trọng lực 9,81 m/s² và mô phỏng 120 Hz. Mọi bi phải đi qua lỗ thật mới thắng; assist 40 mm chỉ hỗ trợ đoạn thoát cuối.

Campaign dạy từng kỹ năng, xen màn nghỉ ở nhịp x4/x9, rồi kết hợp trong boss. Các chương lần lượt là nghiêng/phanh, cửa/cầu, không gian 3D, hai bi, cam nhớ, con lắc/bay–đón, nước, thủy ngân, hình học lạ và phối hợp tổng hợp. 55 màn do GPT-5.6 Sol triển khai; 35 màn khó và 10 boss do GPT-6 Astra triển khai theo phân công của người dùng.

Có chọn chương/màn, đánh dấu boss và màn đã hoàn thành, lưu tiến độ bằng ID ổn định, chuyển sang Lab, zoom quan sát chủ động và xem lại chuyển động thực 15 giây cuối sau khi hoàn thành boss. Prototype cho phép chọn cả 100 màn để test; không bắt mở khóa tuần tự. Điểm độ khó là ngân sách thiết kế **chưa được hiệu chỉnh bằng playtest người thật**. [Thiết kế campaign](Docs/CHILL_CAMPAIGN_DESIGN.md), [ma trận 100 màn](Docs/CAMPAIGN_LEVEL_MATRIX.md), [kiến trúc triển khai và phạm vi kiểm chứng](Docs/CAMPAIGN_IMPLEMENTATION.md).

Các đoạn bên dưới mô tả **Physics Lab P01–P23**, không phải thứ tự campaign C001–C100.

**17–23:** cầu bản lề tự dựng, cân hai bi, lồng treo độc lập, cổng con lắc, lăn–bay–đón, bánh cam có cóc nhớ trạng thái, và boss hai bi trong khối cầu kính. Các cơ cấu dùng Rigidbody/joint, lực tiếp xúc và trọng lực; ball không được điều khiển bằng đường chạy hoặc xung phóng. Chốt/cóc giữ được mô hình hóa bằng ràng buộc lý tưởng tại trạng thái cơ khí đã đạt. [Thiết kế và cách test](Docs/MECHANICAL_LEVELS_17_23.md).

**Luật chung: tất cả bi phải thoát qua lỗ mới thắng.** Bàn 16 có hai bi cùng chịu một thao tác xoay: A giữ nút lò xo để B qua cửa; B nhấn chốt giữ mở hai cửa, rồi cả hai tới lỗ chung. Bàn 18 và 23 cũng có hai bi. HUD đếm `OUT 0/2 → 1/2 → 2/2`; những màn còn lại có một bi. [Thiết kế phối hợp](Docs/LEVEL16_COOPERATIVE.md), [kiểm chứng hiện tại](Docs/Verification/Cooperative16/README.md).

Màn 15 là hộp đầu sư tử bằng đồng và kính: bờm/tai là thành hộp thật, mắt/mũi/mõm là các gờ va chạm. Trọng lực bình thường; dẫn bi vòng qua khuôn mặt đến lỗ ở miệng. [Thiết kế](Docs/LEVEL15_LION_HEAD.md), [ảnh](Docs/Images/Level15/README.md).

Cả campaign và 23 màn Lab có **hỗ trợ hút trong bán kính 4 cm quanh lỗ cuối**: căn từng bi rồi đẩy ra qua cửa thật, chỉ thắng khi đủ số bi đã thoát hoàn toàn. Các lỗ chuyển nội bộ không có lực hút. Đây là hỗ trợ gameplay theo yêu cầu, không thay mô hình vật lý ở phần còn lại. [Thiết kế](Docs/EXIT_ASSIST.md).

Bàn 14 giữ luật chất lỏng đầy, không chảy qua cửa; dùng hình ảnh bạc nhìn xuyên có nhãn để thấy bi bên trong thủy ngân đục. Hỗ trợ thoát xử lý cả bi nổi đứng yên ở miệng lỗ. Sàn bàn 13/14 trong hơn khi lật về phía camera. [Thiết kế/giới hạn](Docs/LEVEL14_MERCURY.md). Bản mới xuất macOS; APK cũ chưa có campaign 100 màn.

Bản nước hiệu chỉnh bổ sung cản khi bi lăn sát thành và added mass; giữ nước 20°C và bi thép 111 g. Có benchmark giảm tốc, hội tụ 60/120/240 Hz và kiểm tra lỗ thật. [Công thức/giới hạn](Docs/LEVEL13_WATER.md), [kiểm chứng](Docs/Verification/Water13/README.md). Bản hiệu chỉnh chỉ xuất macOS; APK hiện có vẫn là bản nước trước đó.

Ở bàn 09, một thanh chặn có khối lượng trượt trên ray theo trọng lực. Đưa bi vào hốc giữ, nghiêng để thanh chặn rời cửa trong khi thành hốc giữ bi lại, rồi chuyển hướng nghiêng để bi đi qua khoang bên phải và ra lỗ. Thanh chặn luôn là vật thể va chạm; không có công tắc hoặc tín hiệu mở khóa. Xem [thiết kế bàn 09](Docs/LEVEL09_LEAVE_IT_BEHIND.md).

Bàn 10 kết hợp các hành lang đổi hướng với hai cửa trượt được mở bởi hai chiều nghiêng đối nhau. Bàn 11 có mê cung riêng trên từng tầng; bi phải lăn tới các lỗ chuyển tầng lệch nhau rồi rơi xuống theo trọng lực, cuối cùng ra lỗ tròn ở đáy. Xem [thiết kế hai mê cung](Docs/LEVEL10_11_MAZES.md).

Bàn 12 ghép các ván kính nhỏ thành một mê cung liên tục trong khối cầu: 25 đoạn, 21 khúc đổi hướng theo cả ba trục và hai nhánh cụt. Khe nhìn giữa các ván nhỏ hơn bi, chặn đường tắt sát vỏ; các lối đi thật nối tới lỗ tròn duy nhất. Xem [thiết kế mê cung trong khối cầu](Docs/LEVEL12_SPATIAL_MAZE.md).

Bàn 13 giữ đầy nước ngay cả khi bi thoát qua lỗ. Bi thép chịu lực nổi và lực cản theo tốc độ tương đối; VFX thể hiện màu nước, caustic, hạt lơ lửng và wake quanh bi. Đây là mô hình lực và dòng khối xấp xỉ, chưa phải fluid solver đầy đủ. Xem [thiết kế và giới hạn bàn 13](Docs/LEVEL13_WATER.md).

Bi có đường kính 30 mm, khối lượng khoảng 111 g; hộp rộng khoảng 34–101 cm tùy hình, sâu 9 cm ở bàn 01–10 và 27 cm ở bàn 11; cầu ở bàn 12 có đường kính ngoài 73,2 cm. Một đơn vị Unity là một mét. Trọng lực thế giới 9,81 m/s² và mô phỏng 120 Hz được giữ nhất quán; cảm giác nặng đến từ tỷ lệ, quán tính quay, contact, tổn hao năng lượng và âm thanh tương ứng với va chạm.

## Chạy và quan sát

1. Mở project bằng Unity **6000.3.19f1**.
2. Mở `Assets/_Game/Scenes/Campaign.unity`. Scene `Gameplay.unity` giữ riêng Physics Lab cũ.
3. Chọn Game View portrait **9:16**, nhấn Play.

Kéo chuột trái hoặc một ngón tay trong vùng hộp để nghiêng. Nghiêng nhẹ rồi giữ để quan sát gia tốc; trả mặt hộp về ngang để quan sát quán tính và giảm tốc. Trong hộp vuông, đưa bi vào mặt phẳng và góc của khối lập phương để so sánh hướng nảy. **R** reset; **P/Esc** pause. Chọn hộp và chuyển tiếp bằng HUD. Ở bàn 11, chạm dòng trạng thái tầng để bật/tắt xem toàn bộ tầng; chế độ xem chỉ đổi vật liệu hiển thị, mọi sàn và vách vẫn va chạm.

Lỗ thoát vẫn là lỗ tròn xuyên mặt hộp, với viền sáng mảnh không có collider hoặc gờ nổi. Chỉ khi toàn bộ bi đi qua lỗ mới ghi nhận thoát. Sau khoảnh khắc thắng ngắn, game tự tải màn kế tiếp; nút NEXT vẫn được giữ để bỏ qua nhanh khi test. Mô phỏng tiếp tục ở tốc độ thực và không slow motion.

Âm thanh lăn dùng lớp rumble đã lọc bớt dải cao, không phát khi bi chỉ rung rất nhỏ, vào chậm và tắt nhanh theo tiếp xúc. Pitch được giới hạn trong vùng trầm; âm lượng tổng được cân lại khi có nhiều bi để tránh tiếng rít và tiếng ồn liên tục.

**D** bật diagnostics trong Editor/development build. **F12** lưu ảnh native vào `Application.persistentDataPath/gravity-box.png`.

## Tài liệu đang áp dụng

- [Triết lý vật lý và cách đánh giá cảm giác](Docs/PHYSICS_DESIGN_PRINCIPLES.md)
- [Kế hoạch hoàn thiện và cổng nghiệm thu](Docs/IMPLEMENTATION_PLAN.md)
- [Kiến trúc và quyền sở hữu trạng thái](Docs/ARCHITECTURE.md)
- [Phạm vi kiểm chứng](Docs/SOLVABILITY.md)
- [Quyết định kỹ thuật](Docs/DECISIONS.md)
- [Nhật ký và kết quả xác minh](Docs/DEVELOPMENT_LOG.md)
- [Ảnh native của năm hình mới](Docs/Images/WeirdBoxes/README.md)
- [Màn 09: trạng thái cửa đóng/mở](Docs/Images/Level09/README.md)
- [Bàn 10–11: cửa ngược hướng và mê cung ba tầng](Docs/LEVEL10_11_MAZES.md)
- [Bàn 12: mê cung không gian trong cầu thủy tinh](Docs/LEVEL12_SPATIAL_MAZE.md)

Ba hộp cơ bản đã được bổ sung năm hình dạng khác thường theo yêu cầu người dùng. Mỗi hình có sàn, nắp và thành đúng đường biên thật; khoảng khuyết của chữ L/U, lõi rỗng của vành khuyên và cổ hẹp của quả tạ ảnh hưởng trực tiếp tới đường lăn. Asset của catalog 16 màn cũ vẫn được giữ ngoài catalog đang chơi. [Tài liệu lịch sử](Docs/Archive/README.md) và [GDD gốc đã trích xuất](Docs/GDD_REFERENCE.md) không phải danh sách tính năng cần đưa trở lại prototype này.

## Cấu trúc và tuning

`Scripts/Foundation` giữ session/reset; `Simulation` giữ Rigidbody, lực, rolling contact và rotation; `Gameplay` quản lý catalog, load/reset/exit; `Presentation` nhận input và thể hiện hình/âm thanh; `App` kết nối các lớp. Editor tạo prefab/mesh và kiểm tra nội dung trước build. Scene Campaign tải một màn tại một thời điểm, chuyển catalog qua HUD; scene Gameplay giữ điểm chạy Lab cũ.

Profile đang dùng nằm trong `Assets/_Game/PhysicsLab/Profiles`: `Solid steel.asset`, `Earth.asset`, `Hand rotation.asset`, `Room temperature water.asset` và `Room temperature mercury.asset`. Prefab/mesh nằm trong `PhysicsLab/Prefabs` và `PhysicsLab/Meshes`; `ScriptableObjects/LevelCatalog.asset` tham chiếu 23 level. Khi đổi kích thước bi phải đổi mass/inertia/contact offset và kiểm tra clearance theo cùng đơn vị. Không tăng riêng mass hoặc giảm gravity để tạo cảm giác nặng.

Prefab và profile đã có sẵn; không cần chạy generator để chơi. Generator tạo lại nội dung đã author, nên lưu thay đổi bằng Git hoặc prefab variant trước khi regenerate.

## Kiểm thử và build

Đóng Unity đang mở project trước khi chạy CLI:

```bash
bash Tools/verify.sh
bash Tools/build.sh macOS
```

Có thể đặt `UNITY_EDITOR` tới executable đúng phiên bản. XML/log vào `Artifacts/`, build vào `Builds/`; hai thư mục này không commit. Validator kiểm tra catalog, prefab, scale, spawn và aperture; tests kiểm tra luật chuyển động, contact, đường biên/lõi rỗng, độ rộng lối đi và lifecycle. Đường giải 16 màn cũ không còn là cổng nghiệm thu của physics lab.

Các lệnh Android/iOS cũ vẫn build scene Lab, chưa chuyển sang campaign và không được chạy trong lượt triển khai này. Android cần module và SDK/NDK/JDK tương ứng. iOS export cần module iOS; compile/cài cần Xcode và signing phù hợp. `bash Tools/run-ios-simulator.sh` export, compile và cài lên simulator đang boot trên Apple Silicon; có thể truyền UDID làm tham số đầu. Telemetry chỉ lưu CSV local. Trạng thái build/chạy thực tế được ghi riêng trong nhật ký; tests không tự chứng minh cảm giác chơi đã đạt.
