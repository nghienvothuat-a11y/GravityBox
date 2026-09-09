# Kiến trúc Gravity Box

Catalog hiện tại gồm 23 màn. Bảy cơ cấu mới được author qua `MechanicalAuthoring` và các builder riêng 17–23. Helper chỉ sinh prefab, collider và mesh; không xây hoặc điều khiển đường đi ở runtime. `PhysicalHinge` giữ bearing friction, đo góc xoắn tương đối từ quaternion Rigidbody và giao tải cho native HingeJoint. `ContactSeatLatch` khóa bậc tự do sau tiếp xúc với ngàm thật; `MemoryRatchetAssembly` quản lý hai cóc lý tưởng cùng lò xo hồi, không xoay cam bằng animation.

`LevelRuntime` thu danh sách component trước khi `PhysicalProp` tách khỏi cây hộp, đăng ký `IResettable` và `IForceProvider` một lần theo vòng đời màn. `IBallMechanism.Bind` truyền roster tường minh cho cơ cấu cần nhận biết contact của các bi. Nhờ vậy chốt nằm trên body tự do vẫn được reset; đổi màn thu hồi cả các body có joint nối nhau. `SphericalEnclosure` mô tả vỏ cầu độc lập với graph mê cung; boss không phải tạo metadata mê cung giả để dùng vỏ cong. [Thiết kế cơ khí](MECHANICAL_LEVELS_17_23.md).

Luật hiện tại quản lý danh sách `LevelManager.Balls`: mỗi bi có lifecycle, force, contact audio và trạng thái cửa riêng. `ExitSocket.BallExited` cập nhật bộ đếm; `Exited` chỉ phát khi toàn bộ roster đã ra ngoài. Các màn cũ có một spawn; `AdditionalBallSpawns` mở rộng nội dung. Camera sau thắng theo viên cuối vừa ra. `Ball` chỉ còn là truy cập bi chính cho công cụ cũ.

Bàn 16 dùng `CooperativeRelay` + hai `PressurePlunger` và bốn `PhysicalProp` (hai nút, hai cửa). Nút đo contact và hành trình thật; cửa được đẩy bằng lực motor giới hạn, không tắt collider. Chốt giữ mở hai cửa và được nhả khi reset. [Thiết kế](LEVEL16_COOPERATIVE.md).

Bàn 15 thêm hình hộp đầu sư tử có bờm/tai và năm gờ mặt thật. `LionHeadBuilder` sinh asset trong Editor; runtime dùng các thành phần chung, không thêm lực hay cơ cấu riêng. [Thiết kế](LEVEL15_LION_HEAD.md).

Prototype hiện tại tải 23 màn: tám thí nghiệm hình học, bốn puzzle vật lý, nước, thủy ngân, hộp đầu sư tử, bài phối hợp hai bi và bảy màn cơ khí mới; dùng chung mô hình bi thép. Bàn 09 có một thanh trượt, bàn 10 có hai thanh trượt ngược hướng, bàn 11 có ba tầng mê cung và bàn 12 có mê cung ba chiều trong khối cầu. Bi chuyển động trong world space; hộp là vật thể kinematic nhận ý định xoay từ người chơi. PhysX giải quyết va chạm giữa chúng. Không có đường điều khiển input trực tiếp tới vị trí hoặc vận tốc của bi.

## Phụ thuộc

```mermaid
flowchart TD
    App[App: composition root] --> Presentation
    App --> Gameplay
    App --> Simulation
    Presentation[Presentation: input, HUD, camera, audio] --> Gameplay
    Presentation --> Simulation
    Gameplay[Gameplay: catalog, load, reset, exit] --> Simulation
    Gameplay --> Foundation
    Simulation[Simulation: bodies, forces, contact, rotation] --> Foundation
    Foundation[Foundation: session and reset contracts]
    Editor[Editor: geometry, authoring, validation, builds] --> Gameplay
    Editor --> Simulation
```

Các lớp được cô lập bằng asmdef. Foundation không tham chiếu UnityEngine. Simulation không biết LevelManager/HUD. Gameplay không đọc touch/chuột hoặc xây giao diện. App truyền dependency tường minh. Editor và test assemblies không vào release player.

## Quyền sở hữu

| Đối tượng | Chủ sở hữu / tuổi thọ | Trách nhiệm |
| --- | --- | --- |
| GameBootstrap | Gameplay scene / phiên chạy | Khởi tạo clock 120 Hz và kết nối hệ thống |
| LevelManager | Bootstrap / phiên chạy | Load một hộp, reset, chuyển hộp thủ công, session |
| LevelRuntime | LevelManager / một lần load | Root, spawn, exit, registry theo hộp |
| ExitSocket / ExitSocket.Assist | LevelRuntime / một lần load | Kiểm tra bi ra hoàn toàn; force provider hỗ trợ trong 40 mm, có kiểm tra lối đi và reset theo session |
| BallController | LevelManager / một lần load | Rigidbody độc lập, contact/rolling state, reset, dữ liệu feedback |
| PhysicalProp | LevelRuntime / một lần load | Body thanh trượt độc lập, đăng ký lực, lưu/khôi phục pose/vận tốc, thu hồi khi đổi bàn |
| GravitySliderGuide | PhysicalProp / một lần load | Đọc độ dịch chuyển/vận tốc theo ray; không điều khiển chuyển động hoặc gửi unlock |
| LayeredMaze | LevelRuntime / một lần load | Metadata sàn, lỗ chuyển tầng và đường authoring; xác định tầng từ vị trí thật của bi |
| SpatialMaze | LevelRuntime / một lần load | Metadata vỏ cầu, nút/đoạn nối, tuyến kiểm chứng và descriptor ván ghép; không điều khiển bi |
| WaterVolume / WaterProfile / WaterHydrodynamics | LevelRuntime / một lần load; asset read-only | Mô hình chất lỏng dùng chung: lực nổi, cản lăn/cầu, added mass, phần ngập và dòng khối ở bàn 13/14 |
| WaterVisuals | HUD khởi tạo / một lần load | Mesh tracer, wake và shader nước; đọc simulation, không tạo lực |
| MazeLayerView | HUD khởi tạo / một lần load | Làm mờ các tầng không hoạt động hoặc hiện tổng thể bằng vật liệu, giữ nguyên physics |
| EnvironmentForceSystem | Bootstrap / phiên chạy | Áp gia tốc thế giới một lần cho mỗi body đã đăng ký |
| BoxRotationController | LevelRuntime / một lần load | Đổi input intent thành chuyển động kinematic bị giới hạn |
| Catalog / profile | Asset / read-only runtime | Hình dạng, vật liệu, kích thước và thông số chung |
| HUD / camera / audio | Bootstrap / phiên chạy | Thể hiện trạng thái vật lý và nhận thao tác |

Ball không nằm dưới transform của hộp. Khi đổi hộp, manager xóa force targets/providers và vô hiệu hóa root/ball/props cũ trước Destroy cuối frame, tránh collider của hai bàn cùng hoạt động. Số force targets là `1 + Props.Length`: hai ở bàn 09, ba ở bàn 10, một ở tám hộp hình học và bàn 11–13. Nước là force provider, không tạo body nước riêng. Các cơ cấu tín hiệu cũ vẫn nằm ngoài catalog.

## Clock, scale và contact

Một đơn vị Unity bằng một mét. Bi bán kính 0,015 m, khối lượng khoảng 0,111 kg; quán tính cầu đặc phải nhất quán với `I = 2/5 m r²`. Vỏ hộp dày 0,006 m. `LevelRuntime.InteriorDepth` lưu khoảng cách giữa tâm sàn ngoài và tâm nắp: 0,09 m mặc định, 0,27 m cho hộp ba tầng. Với SphereMaze, giá trị 0,732 m là đường kính ngoài; `SpatialMaze.InnerRadius` và `ShellThickness` mới là metadata có thẩm quyền cho vỏ cong. Tỷ lệ hiển thị đến từ camera, không scale phóng đại Rigidbody.

Input gửi góc mong muốn tới controller; controller dùng MoveRotation trong fixed step. Gia tốc Trái Đất là `(0, -9.81, 0)` trong world space. Hệ lực dùng ForceMode.Acceleration và không nhân timestep lần thứ hai. Ball nhận lực/contact; renderer đọc pose nội suy để hiển thị. Profile rolling resistance xử lý tổn hao khi có mặt đỡ, không dùng damping trong không khí để thay thế ma sát lăn.

Mô phỏng dùng 1/120 s. Contact offset, bounce threshold, solver và giới hạn tốc độ quay của bi được đặt theo scale bàn nhỏ. Giới hạn góc xoay của hộp hạn chế input gây vận tốc bề mặt quá lớn. Việc chọn giá trị phải được đo bằng test dốc/va chạm; không chốt cảm giác chỉ từ Inspector.

## Hình học và cửa thoát

LevelDefinition khai báo `ContainerShape`: Circle, Square, Triangle, LShape, UShape, Annulus, Dumbbell, Star, GravityLock, MechanicalMaze, LayeredMaze, SphereMaze và WaterBox. `LevelRuntime.Footprint` lưu contour ngoài CCW trên mặt XZ; `FootprintVoids` chứa các contour lõi rỗng, mỗi phần tử có `Points`. Vành khuyên có một contour trong, các hình khác có thể lõm nhưng không có lõi rỗng khép kín.

Editor triangulate sàn/nắp theo miền polygon thật; không dựng quạt từ origin cho polygon lõm hoặc chứa lỗ. Thành vỏ đi theo cả contour ngoài lẫn contour trong. Cùng mesh được dùng cho hiển thị/collision. Collider **Fixed cube**, cạnh 0,064 m, thuộc compound kinematic của hộp vuông và không di chuyển tương đối với hộp.

Cửa giữ quy ước local +Z hướng ra ngoài, bán kính 0,023 m và wall half-depth 0,003 m. Renderer/collider dùng cùng hình học mặt cắt. Ring chỉ có renderer, không collider. ExitSocket kiểm tra sweep từ phía trong qua bán kính tròn và đợi toàn bộ cầu vượt mặt ngoài. Sai số clearance/completion tính theo bán kính bi; không giữ các dung sai vài centimet từ prototype cũ.

Bounds của LevelRuntime là hàng rào kiểm tra lỗi sau vùng vỏ và ngưỡng thoát, không phải collider. Kích thước bounds/framing tăng theo contour của từng hộp. Kiểm thử hình học tiếp cận từng cạnh từ phía có thể chơi, kiểm tra sàn/nắp bên trong và khoảng trống bên ngoài polygon, sau đó sweep toàn bộ cầu và nghiêng bi qua các góc/cổ nối. Không giả định origin luôn nằm trong hộp: origin của vành khuyên nằm trong vùng rỗng.

## Session và reset

Active có thể chuyển Paused, Completing khi bi thoát, hoặc Failed nếu phát hiện lọt ra ngoài sai đường. Completing giữ time scale 1; manager không tự chuyển bàn theo timer. Next được người chơi gọi và quay vòng 13 bàn. Reset có thể gọi từ trạng thái đang chơi hoặc đã thoát để lặp cùng điều kiện.

Reset khôi phục root pose trước, xóa input backlog, khôi phục exit/traversal state, rồi world pose/vận tốc của props và contact state của bi. Cuối cùng SyncTransforms và BeginTracking lấy mẫu mới. Registry chỉ capture trạng thái ban đầu một lần; reset không ghi đè trạng thái chuẩn bằng kết quả thử trước đó.

## Ray của thanh trượt bàn 09–10

ConfigurableJoint nối body thanh chặn với root hộp. Một trục tịnh tiến cho phép hành trình 0–0,12 m dọc local +Z; các trục tịnh tiến khác và ba trục quay bị khóa. Anchor đặt giữa hành trình để giới hạn đối xứng của joint biểu diễn được khoảng này. Không có spring, damper drive, motor hoặc projection kéo body về vị trí.

PhysicalProp tách khỏi hierarchy lúc load, nhưng liên kết vật lý của joint vẫn gắn với hộp. GravitySliderGuide chỉ báo displacement/speed và clearance hình học; collider luôn bật. Hệ lực áp cùng gia tốc thế giới cho bi và thanh chặn. Ray là mô hình lý tưởng không ma sát dọc ray; vật liệu tiếp xúc chỉ tác động khi body thật sự chạm housing/end stop. Bàn 10 tái sử dụng cùng authoring với pose riêng; cửa B xoay 180° quanh Y nên cùng trục ray của body hướng về local −Z của hộp. Không thêm controller riêng cho cửa thứ hai. Xem [bàn 09](LEVEL09_LEAVE_IT_BEHIND.md) và [hai mê cung](LEVEL10_11_MAZES.md).

## Mê cung ba tầng và chế độ xem

LayeredMazeBuilder tạo ba sàn có tâm Y lần lượt +0,045; −0,045; −0,135 m. Sàn trên/giữa có lỗ chuyển tầng R=0,038 m tại hai vị trí XZ lệch nhau; sàn dưới dùng ExitSocket tròn R=0,023 m hiện có. Mỗi tầng có hệ vách riêng cao 0,084 m nối sàn với mặt dưới của tầng/nắp phía trên. Toàn bộ collider hoạt động đồng thời; bi tự rơi qua lỗ, không có trigger chuyển pose, lực chuyển tầng hoặc scene phụ.

`LayeredMaze.Decks` giữ floor height, collider, renderers và route authoring; `TransferPorts` chỉ đánh dấu vị trí hình học. `GetLayerIndex` đọc chiều cao bi trong hệ tọa độ hộp. MazeLayerView dùng metadata để đổi shared materials giữa bản gốc và bản mờ, phục hồi vật liệu khi thu hồi; không bật/tắt collider hoặc thay trạng thái Rigidbody. HUD cho xem toàn bộ tầng bằng một nút và chặn thao tác này khỏi input xoay hộp.

## Mê cung ván ghép trong cầu

SphereMazeBuilder tạo một mạng nối ba chiều bên trong vỏ cầu R=0,36 m, dày 6 mm. Các đoạn có tiết diện trống 50 mm, ghép từ ván mỏng với khe nhìn 15 mm. Ngã rẽ đóng những phía không kết nối; hai nhánh cụt có đầu chặn thật. Khung nối vào lỗ thoát cuối, chặn đường vòng bên ngoài. Mỗi đoạn/ngã rẽ dùng một MeshCollider chứa đúng hình học các ván, gắn với root kinematic; không dùng collider kín phủ khe.

SpatialMaze giữ `NodesLocal`, `Edges`, `MainPath`, `JunctionColliders`, `Planks` và kích thước. Descriptor ván chỉ phục vụ authoring/validation; tuyến chính chỉ phục vụ kiểm chứng. Runtime không đi theo graph để di chuyển bi. Footprint XZ là dữ liệu tương thích; vỏ cầu dùng bán kính và collider cong làm hợp đồng hình học.

Input chỉ xoay root. Bi chuyển động trong world space, có thể rơi dọc một đoạn rồi chạm mặt đỡ tại ngã rẽ. Test kiểm tra các passage/cap bằng cả bán kính bi và replay tuyến 25 đoạn chỉ qua rotation intent; các phép đo không thay đổi model vật lý. Xem [thiết kế bàn 12](LEVEL12_SPATIAL_MAZE.md).

## Mở rộng sau khi cảm giác đạt

Bàn 13 mở rộng hệ lực bằng `IForceStepProvider.PrepareStep(dt)` trước lượt cộng gia tốc. `WaterVolume` tính phần ngập, vận tốc nước và lực lên bi, áp mô-men nhớt qua Rigidbody; Earth gravity vẫn do provider chung quản lý. `LevelRuntime` chỉ phụ thuộc Simulation. HUD khởi tạo `WaterVisuals`, giống seam presentation của mê cung tầng; reset registry thu hồi trạng thái, `EnvironmentForceSystem.Clear` bỏ provider cũ khi đổi bàn. [Mô hình và giới hạn nước](LEVEL13_WATER.md).

`WaterHydrodynamics` tách công thức cản khỏi Unity lifecycle. WaterVolume sở hữu added mass: Rigidbody.mass là quán tính tịnh tiến thép+nước, profile vẫn chứa mass/inertia thép; provider bù trọng lượng phần nước và trả mass khi ra nước/disable/reset. Không ghi mass lặp khi phần ngập không đổi, tránh đánh thức bi đang nghỉ. Trường dòng lưu hai snapshot để tính gia tốc phần tử nước, không dùng gia tốc đo của bi. Hiệu chỉnh cản lăn raycast collider thật và không tạo collider bổ sung.

Bàn 14 thay profile bằng thủy ngân; không có nhánh lực riêng hoặc bản sao solver. Tên lớp `Water*` giữ để tương thích các asset/GUID hiện có; menu authoring đổi thành Liquid Profile. `WaterVisuals.MercuryCutaway` chỉ chọn màu tracer và bộ material/shader khác, không tham gia tính lực. ContentValidator kiểm tra từng chất lỏng và view tương ứng. [Thiết kế thủy ngân](LEVEL14_MERCURY.md).

Giữ seam giữa simulation, level content và presentation để tinh chỉnh từng phần. Chỉ thêm bàn/cơ cấu sau khi cảm giác bi đạt qua thử trực tiếp. Force providers, PhysicalProp, save adapter hoặc loader async có thể được mở rộng khi có yêu cầu cụ thể; số lượng lớp không phải mục tiêu. Các rule tín hiệu/zero-G cũ không được coi là nền tảng cần bật lại.
