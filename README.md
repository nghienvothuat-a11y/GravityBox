# Venom — Living Matter Prototype

Nhánh **`Venom`** có ba màn thử nghiệm điều khiển sinh vật mềm. Chỉ thắng khi **100% vật chất** đã đi qua lỗ tròn thật.

| Màn | Điều khiển | Bài thử |
| --- | --- | --- |
| 01 — Một cơ thể, hai ý chí | Nghiêng hộp | Cắt, hai nút, hợp thể, thoát |
| 02 — Hai phần, một kế hoạch | Hộp đứng im; chọn từng phần | Bám giữ nút A, đổi phần sang nút B, mở cửa |
| 03 — Tìm về chủ thể | Điều khiển phần lớn nhất | Phần nhỏ chờ 3 giây rồi tự tìm đường vòng về để nhập lại |

- Chạy macOS: `Builds/Venom/macOS/Venom.app`. Nút ở đầu HUD hoặc **1/2/3** chọn màn; **R** thử lại; **P/Esc** tạm dừng.
- Màn 02/03: **giữ–kéo** để bò hoặc **WASD/mũi tên**. Màn 02 chạm phần muốn chọn, dùng nút A/B hoặc **Tab**. Màn 03 luôn chọn phần lớn nhất.
- Chạy Unity **6000.3.19f1**: mở `Assets/_Game/Venom/Venom01.unity`, `Venom02.unity` hoặc `Venom03.unity`, Game View **9:16**, Play.
- Build lại: `bash Tools/build-venom.sh` (đóng Unity đang mở project trước). Build macOS có cả ba màn; chưa xuất APK cho biến thể Venom.
- [Thiết kế màn 01](Docs/VENOM_PROTOTYPE_01.md) · [Điều khiển, navigation và kiến trúc màn 02–03](Docs/VENOM_CONTROLS_02_03.md).

Camera màn 02 nhìn gần thẳng từ trên xuống (**88°**) để dễ chọn hai phần/căn công tắc; màn 03 nhìn chéo từ đầu xuất phát (**58°, lệch 12°**) để thấy rõ chuyển động thân và phần đi theo. Cả hộp nằm trong khung hình; giữ–kéo được căn theo hướng nhìn mới.

Sinh vật dùng hạt vật lý liên kết nhớt/dẻo và bề mặt metaball liên tục. Đây là mô hình vật chất mềm phục vụ thử gameplay, chưa phải solver chất lỏng bảo toàn thể tích. Nội dung 23 bàn bi thép bên dưới vẫn có thể mở qua scene `Gameplay.unity`; build Venom dùng scene riêng.

Sinh vật có animation theo hướng symbiote: thân dồn và cuộn lệch, mô phía trên có độ trễ khi trượt, thỉnh thoảng dựng một đỉnh mềm để thăm dò. Các sợi bám mọc độc lập và căng mảnh rồi thu lại. Animation đọc vận tốc, tiếp xúc thật và ý định bò ở màn 02/03. Lực bám–kéo do hệ locomotion riêng tạo ở các hạt có tiếp xúc; mesh/xúc tua chỉ biểu diễn hình ảnh, không thay đổi lượng vật chất cần thoát. [Tư liệu và cách áp dụng](Docs/VENOM_MOTION_STUDY.md) · [Ảnh/animation cận cảnh](Docs/Images/VenomLife/README.md).

Bản cập nhật tiếp xúc sàn: cửa và lưỡi chém có chặn dưới đúng mặt sàn; skin không phình xuyên phần sàn đặc và cập nhật theo từng frame. Animation nhanh hơn **1,5×**, biên độ tăng khoảng **20–30%**. Đóng bản macOS đang chạy rồi mở lại app để nhận bản build mới. Kiểm chứng: rơi 2/5/10 m/s, lật hộp nhiều trục, chặn cơ cấu và đường giải thoát đủ vật chất.


## Gravity Box — Steel Ball Lab

Prototype Unity tập trung vào cảm giác bi thép lăn trong hộp: tăng tốc khi nghiêng, giữ quán tính, đổi hướng và nảy khi va chạm. Catalog có **23 bàn**: giữ 16 thí nghiệm trước và thêm bảy màn cơ khí 17–23. Hai môi trường chất lỏng dùng cùng hộp vuông/cube của bàn 02: bi chìm trong nước và nổi trong thủy ngân.

**17–23:** cầu bản lề tự dựng, cân hai bi, lồng treo độc lập, cổng con lắc, lăn–bay–đón, bánh cam có cóc nhớ trạng thái, và boss hai bi trong khối cầu kính. Các cơ cấu dùng Rigidbody/joint, lực tiếp xúc và trọng lực; ball không được điều khiển bằng đường chạy hoặc xung phóng. Chốt/cóc giữ được mô hình hóa bằng ràng buộc lý tưởng tại trạng thái cơ khí đã đạt. [Thiết kế và cách test](Docs/MECHANICAL_LEVELS_17_23.md).

**Luật chung: tất cả bi phải thoát qua lỗ mới thắng.** Bàn 16 có hai bi cùng chịu một thao tác xoay: A giữ nút lò xo để B qua cửa; B nhấn chốt giữ mở hai cửa, rồi cả hai tới lỗ chung. Bàn 18 và 23 cũng có hai bi. HUD đếm `OUT 0/2 → 1/2 → 2/2`; những màn còn lại có một bi. [Thiết kế phối hợp](Docs/LEVEL16_COOPERATIVE.md), [kiểm chứng hiện tại](Docs/Verification/Cooperative16/README.md).

Màn 15 là hộp đầu sư tử bằng đồng và kính: bờm/tai là thành hộp thật, mắt/mũi/mõm là các gờ va chạm. Trọng lực bình thường; dẫn bi vòng qua khuôn mặt đến lỗ ở miệng. [Thiết kế](Docs/LEVEL15_LION_HEAD.md), [ảnh](Docs/Images/Level15/README.md).

Cả 23 màn có **hỗ trợ hút trong bán kính 4 cm quanh lỗ cuối**: căn từng bi rồi đẩy ra qua cửa thật, chỉ thắng khi đủ số bi đã thoát hoàn toàn. Các lỗ chuyển nội bộ không có lực hút. Đây là hỗ trợ gameplay theo yêu cầu, không thay mô hình vật lý ở phần còn lại. [Thiết kế](Docs/EXIT_ASSIST.md).

Bàn 14 giữ luật chất lỏng đầy, không chảy qua cửa; dùng hình ảnh bạc nhìn xuyên có nhãn để thấy bi bên trong thủy ngân đục. Hỗ trợ thoát xử lý cả bi nổi đứng yên ở miệng lỗ. Sàn bàn 13/14 trong hơn khi lật về phía camera. [Thiết kế/giới hạn](Docs/LEVEL14_MERCURY.md). Bản mới xuất macOS; APK cũ chưa có màn 14–23 và hỗ trợ thoát.

Bản nước hiệu chỉnh bổ sung cản khi bi lăn sát thành và added mass; giữ nước 20°C và bi thép 111 g. Có benchmark giảm tốc, hội tụ 60/120/240 Hz và kiểm tra lỗ thật. [Công thức/giới hạn](Docs/LEVEL13_WATER.md), [kiểm chứng](Docs/Verification/Water13/README.md). Bản hiệu chỉnh chỉ xuất macOS; APK hiện có vẫn là bản nước trước đó.

Ở bàn 09, một thanh chặn có khối lượng trượt trên ray theo trọng lực. Đưa bi vào hốc giữ, nghiêng để thanh chặn rời cửa trong khi thành hốc giữ bi lại, rồi chuyển hướng nghiêng để bi đi qua khoang bên phải và ra lỗ. Thanh chặn luôn là vật thể va chạm; không có công tắc hoặc tín hiệu mở khóa. Xem [thiết kế bàn 09](Docs/LEVEL09_LEAVE_IT_BEHIND.md).

Bàn 10 kết hợp các hành lang đổi hướng với hai cửa trượt được mở bởi hai chiều nghiêng đối nhau. Bàn 11 có mê cung riêng trên từng tầng; bi phải lăn tới các lỗ chuyển tầng lệch nhau rồi rơi xuống theo trọng lực, cuối cùng ra lỗ tròn ở đáy. Xem [thiết kế hai mê cung](Docs/LEVEL10_11_MAZES.md).

Bàn 12 ghép các ván kính nhỏ thành một mê cung liên tục trong khối cầu: 25 đoạn, 21 khúc đổi hướng theo cả ba trục và hai nhánh cụt. Khe nhìn giữa các ván nhỏ hơn bi, chặn đường tắt sát vỏ; các lối đi thật nối tới lỗ tròn duy nhất. Xem [thiết kế mê cung trong khối cầu](Docs/LEVEL12_SPATIAL_MAZE.md).

Bàn 13 giữ đầy nước ngay cả khi bi thoát qua lỗ. Bi thép chịu lực nổi và lực cản theo tốc độ tương đối; VFX thể hiện màu nước, caustic, hạt lơ lửng và wake quanh bi. Đây là mô hình lực và dòng khối xấp xỉ, chưa phải fluid solver đầy đủ. Xem [thiết kế và giới hạn bàn 13](Docs/LEVEL13_WATER.md).

Bi có đường kính 30 mm, khối lượng khoảng 111 g; hộp rộng khoảng 34–101 cm tùy hình, sâu 9 cm ở bàn 01–10 và 27 cm ở bàn 11; cầu ở bàn 12 có đường kính ngoài 73,2 cm. Một đơn vị Unity là một mét. Trọng lực thế giới 9,81 m/s² và mô phỏng 120 Hz được giữ nhất quán; cảm giác nặng đến từ tỷ lệ, quán tính quay, contact, tổn hao năng lượng và âm thanh tương ứng với va chạm.

## Chạy và quan sát

1. Mở project bằng Unity **6000.3.19f1**.
2. Mở `Assets/_Game/Scenes/Gameplay.unity` hoặc menu **Gravity Box → Open Gameplay**.
3. Chọn Game View portrait **9:16**, nhấn Play.

Kéo chuột trái hoặc một ngón tay trong vùng hộp để nghiêng. Nghiêng nhẹ rồi giữ để quan sát gia tốc; trả mặt hộp về ngang để quan sát quán tính và giảm tốc. Trong hộp vuông, đưa bi vào mặt phẳng và góc của khối lập phương để so sánh hướng nảy. **R** reset; **P/Esc** pause. Chọn hộp và chuyển tiếp bằng HUD. Ở bàn 11, chạm dòng trạng thái tầng để bật/tắt xem toàn bộ tầng; chế độ xem chỉ đổi vật liệu hiển thị, mọi sàn và vách vẫn va chạm.

Lỗ thoát vẫn là lỗ tròn xuyên mặt hộp, với viền sáng mảnh không có collider hoặc gờ nổi. Chỉ khi toàn bộ bi đi qua lỗ mới ghi nhận thoát. Mô phỏng tiếp tục ở tốc độ thực; không slow motion hoặc tự chuyển bàn. Người chơi chọn khi nào reset hoặc chuyển hộp.

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

`Scripts/Foundation` giữ session/reset; `Simulation` giữ Rigidbody, lực, rolling contact và rotation; `Gameplay` quản lý catalog, load/reset/exit; `Presentation` nhận input và thể hiện hình/âm thanh; `App` kết nối các lớp. Editor tạo prefab/mesh và kiểm tra nội dung trước build. Scene duy nhất tải một thí nghiệm tại một thời điểm.

Profile đang dùng nằm trong `Assets/_Game/PhysicsLab/Profiles`: `Solid steel.asset`, `Earth.asset`, `Hand rotation.asset`, `Room temperature water.asset` và `Room temperature mercury.asset`. Prefab/mesh nằm trong `PhysicsLab/Prefabs` và `PhysicsLab/Meshes`; `ScriptableObjects/LevelCatalog.asset` tham chiếu 23 level. Khi đổi kích thước bi phải đổi mass/inertia/contact offset và kiểm tra clearance theo cùng đơn vị. Không tăng riêng mass hoặc giảm gravity để tạo cảm giác nặng.

Prefab và profile đã có sẵn; không cần chạy generator để chơi. Generator tạo lại nội dung đã author, nên lưu thay đổi bằng Git hoặc prefab variant trước khi regenerate.

## Kiểm thử và build

Đóng Unity đang mở project trước khi chạy CLI:

```bash
bash Tools/verify.sh
bash Tools/build.sh macOS
bash Tools/build.sh Android
bash Tools/build.sh iOS
bash Tools/build.sh iOS-Simulator
```

Có thể đặt `UNITY_EDITOR` tới executable đúng phiên bản. XML/log vào `Artifacts/`, build vào `Builds/`; hai thư mục này không commit. Validator kiểm tra catalog, prefab, scale, spawn và aperture; tests kiểm tra luật chuyển động, contact, đường biên/lõi rỗng, độ rộng lối đi và lifecycle. Đường giải 16 màn cũ không còn là cổng nghiệm thu của physics lab.

Android cần module và SDK/NDK/JDK tương ứng. iOS export cần module iOS; compile/cài cần Xcode và signing phù hợp. `bash Tools/run-ios-simulator.sh` export, compile và cài lên simulator đang boot trên Apple Silicon; có thể truyền UDID làm tham số đầu. Telemetry chỉ lưu CSV local. Trạng thái build/chạy thực tế được ghi riêng trong nhật ký; tests không tự chứng minh cảm giác chơi đã đạt.
