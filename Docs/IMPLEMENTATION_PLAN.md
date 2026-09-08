# Kế hoạch phát triển Gravity Box

Ngày 08 tháng 09 năm 2026. Căn cứ: GDD v1.0 của người dùng. Mục tiêu là một prototype Unity có thể chơi và đo lường, sau đó tiến tới sản phẩm qua các cổng chất lượng rõ ràng. Các prompt mẫu ở mục 15 của GDD là tài liệu tham khảo, không phải yêu cầu thay thế chỉ đạo hiện tại.

## Phạm vi và kết quả cần có

Prototype đầu tiên gồm một quả bóng, hộp acrylic 3D, xoay bằng chuột hoặc một ngón tay, Earth gravity và zero-G. Có 16 màn theo thứ tự 10 gravity rồi 6 zero-G, prefab cho từng màn, các cơ cấu wall/ramp, exit, hazard, plate, door, one-way gate, bumper và impulse pad. HUD có reset, chọn màn, pause; công cụ phát triển có thông tin vật lý, tốc độ mô phỏng và step. Giao diện game tạm dùng tiếng Anh để khớp GDD; tài liệu kỹ thuật dùng tiếng Việt.

Không triển khai magnetism, water, acid, wind, backend, tài khoản hay monetization trong Phase 1. Chỉ tạo hợp đồng mở rộng lực, không viết trước những hệ thống chưa được kiểm chứng bằng gameplay.

## Các quyết định nền tảng

- Unity 6.3 LTS 6000.3.19f1 đang cài trên máy; khóa phiên bản Editor và package lock.
- URP, màu Linear, hình khối đơn giản, một directional light chủ đạo; vật liệu dùng chung và shell hạn chế lớp trong suốt.
- Một scene Gameplay lâu dài, mỗi puzzle là LevelPrefab được tham chiếu bởi LevelDefinition. GameBootstrap là composition root.
- Ball là Rigidbody độc lập trong world space. LevelRoot là kinematic compound body và xoay bằng MoveRotation trong FixedUpdate. Không parent Ball vào LevelRoot.
- Force providers trả acceleration trong world space. Physics.gravity = zero, lực gravity áp riêng mỗi fixed tick. Zero-G không xoay velocity theo hộp.
- Reset có thứ tự và theo scope từng màn; không reload scene. Reset xóa transition đang chờ, trạng thái signal, pose, vận tốc, cooldown, quá trình đi qua cửa, trail và collision-ignore.
- asmdef thực thi hướng phụ thuộc. Không dùng static service locator, global event bus hoặc DI framework nặng.

Chi tiết: [ARCHITECTURE.md](ARCHITECTURE.md) và [DECISIONS.md](DECISIONS.md).

## Mốc triển khai và cổng nghiệm thu

| Mốc | Công việc và đầu ra | Điều kiện chuyển bước | Ước lượng người làm |
| --- | --- | --- | --- |
| M0 | Project, packages, asmdef, settings, Git hygiene, docs | Import sạch, project mở được | 0.5 ngày |
| M1 | Camera, shell, touch/mouse, free/assisted/90°, sensitivity | Drag rõ ràng, không nhảy khi chạm UI, release ổn định | 1–2 ngày |
| M2 | Ball, gravity provider, ramp, cửa thật và xác nhận bi thoát hẳn | Contact ổn định; gravity world-down; không steering ball | 1–2 ngày |
| M3 | State machine, reset registry, fail/bounds | 100 reset không drift; terminal event chỉ một lần | 1 ngày |
| M4 | Catalog/definition/prefab, lifecycle load/next, authoring | Load đúng profile/prefab, listener không nhân đôi | 1 ngày |
| M5 | Plate-door signal, one-way, spring, impulse | Từng cơ cấu hoạt động và reset trong lúc chuyển động | 2–3 ngày |
| M6 | Zero-G, velocity/trail, 6 layout | Không gia tốc tự phát; rotation chỉ ảnh hưởng qua contact | 1–2 ngày |
| M7 | 10 gravity + 6 zero-G, solution notes và kiểm tra khả giải | Mỗi màn có route tái hiện được; không bắt góc/khung hình cực hẹp | 3–5 ngày |
| M8 | HUD safe area, tutorial, audio, touch tune, profiling | Ma trận thiết bị và 30/60 FPS đạt; giao diện đọc được | 2–4 ngày |
| M9 | Dev build, playtest, triage và Go/No-Go | Android và iPhone thật; báo cáo hành vi người chơi | 2–3 ngày |

Ước lượng trên là effort tham khảo cho một lập trình viên Unity có khả năng làm level, không phải cam kết lịch. Thời gian playtest, phần cứng và vòng chỉnh level nằm trên critical path. Việc tạo code và prefab nhanh không thay thế thời gian tuning hay kiểm thử thiết bị.

## Quy trình kiểm chứng

1. Chạy Unity import/compile với Editor đúng phiên bản.
2. Edit Mode kiểm tra state transition, reset registry, environment acceleration, 24 orientation của cube và dữ liệu catalog.
3. Play Mode kiểm tra PhysX thực: gravity, zero-G drift, root xoay không kéo bóng, exit, hazard, mechanism và 100 lần reset.
4. Dùng Play Mode kiểm tra load/advance/complete, pause/reset giữa các trạng thái, input tương tác UI và độ đọc của cube.
5. Tạo development build desktop để người dùng chơi ngay; Android APK và iOS project khi toolchain cho phép. Build thành công không tương đương đã chạy trên thiết bị.
6. Ghi kết quả thực tế và giới hạn vào DEVELOPMENT_LOG.md; không đánh dấu đạt performance/fun/solvability chỉ từ việc compile.

## Ma trận màn chơi

| Màn | Tên | Mục tiêu học | Điều cần quan sát |
| --- | --- | --- | --- |
| 01 | First principles | Ramp, trọng lực | Nhìn thấy bóng và exit ngay; thử drag trong 10 giây |
| 02 | Around the corner | Xoay nhiều trục | Đổi hướng để đi qua góc |
| 03 | Small steps | Shelf sequencing | Rơi xuống đúng khe và phục hồi được |
| 04 | A little caution | Hazard | Nguy hiểm nổi bật, fail giải thích được |
| 05 | Carry the motion | Momentum + ramp | Biết lấy đà rồi đổi hướng |
| 06 | Let it fall | Nắp tự do rơi theo trọng lực | Đưa lỗ lên trên để nắp rời mặt tựa; nắp vẫn va chạm sau khi rơi |
| 07 | No turning back | One-way | Đi xuôi, không xuyên gate theo chiều ngược |
| 08 | Spring theory | Bumper/impulse | Impulse là nguồn lực được đánh dấu rõ |
| 09 | Chain reaction | Plate + ramp + gate | Thứ tự kích hoạt có ý nghĩa |
| 10 | Gravity graduate | Tổng hợp | Không chỉ random spin là giải được |
| 11 | Weightless | Momentum ban đầu | Bóng không rơi khi thả tay |
| 12 | Equal and opposite | Redirect | Đổi trajectory bằng contact |
| 13 | A gentle push | Bumper | Hiểu nguồn impulse và hồi tiếp |
| 14 | Moving the walls | Align trước contact | Xoay không uốn trajectory từ xa |
| 15 | Permission to pass | Gate zero-G | Điều hướng để mở lối ra |
| 16 | Orbital mechanics | Hai redirect và pad | Kết hợp, không đòi chính xác từng frame |

Layout khởi tạo là nội dung thử nghiệm cần tuning với người chơi. Mỗi LevelDefinition lưu DesignerSolution để designer ghi route; checklist nghiệm thu phân biệt route dự kiến với bằng chứng đã giải bằng mô phỏng/hand play.

## Kiến trúc sản phẩm và lộ trình sau prototype

**Cổng P0 — xác thực trò chơi:** ít nhất 5–8 người mới, 2 thiết bị thật, đo tutorial completion và thời gian/reset mỗi màn. Quan sát hiểu nguyên nhân thất bại, sự khác biệt mental model gravity/zero-G, mong muốn chơi lại. GDD chưa cung cấp ngưỡng thống kê; các ngưỡng dưới đây là đề xuất để thảo luận.

Đề xuất GO: 80% người mới hiểu thao tác không cần giải thích thêm ở L02; không có bug physics/reset nghiêm trọng trong phiên 20 phút; phần lớn puzzle standard được giải 30–120 giây; tối thiểu 5 màn zero-G tạo lựa chọn trajectory có chủ ý. Không dùng số lượng asset hoặc số test xanh thay cho fun signal.

**Cổng P1 — vertical slice sản phẩm:** chốt thiết bị tối thiểu, art direction, accessibility, localization, settings audio/haptics và save schema có migration. Chuyển authoring sang prefab variants/custom inspector và validation; thêm Addressables khi số lượng asset/loading memory thực sự cần. Tránh một scene khổng lồ hay Resources.Load phân tán.

**Cổng P2 — cơ chế mới:** material profile → magnetism → wind → water → acid → multiple balls, đúng thứ tự GDD. Mỗi force implement IForceProvider; volume có lifecycle đăng ký/hủy; level definition tham chiếu data, không kiểm tra level ID trong runtime. Khi có nhiều ball, mở rộng target registry trong ForceSystem và goal conditions, giữ Input/Rotation không biết đến mục tiêu.

**Cổng P3 — nội dung và vận hành:** level editor, smoke routes được ghi lại, asset validation trên CI, build theo platform, crash reporting và analytics có consent khi cần. Chỉ quyết định backend, monetization, account và live ops khi product requirements rõ.

## Ngân sách kỹ thuật đề xuất

| Hạng mục | Baseline | Cách đo / hành động |
| --- | --- | --- |
| Physics | 60 Hz, giới hạn speed 12 m/s, rotation 100°/s | Unity Profiler trên máy thật; tăng solver chỉ khi có bằng chứng |
| Presentation | 60 FPS ưu tiên | CPU/GPU p95 và nhiệt trong phiên 15–20 phút |
| Reset | <0.5 giây | Profile marker hoặc thời gian frame khi reset chuyển động |
| Allocations | 0 B recurring trong force/rotation hot path | Profiler GC.Alloc; HUD debug 4 Hz được tách khỏi physics |
| Memory | Đo baseline rồi đặt budget theo máy thấp nhất | Snapshot sau load tất cả level và sau reset 100 lần |
| Assets | Primitive colliders, shared materials | Frame Debugger, count colliders/materials và transparency |

## Rủi ro cần xử lý trước khi gọi là sản phẩm

Rủi ro lớn nhất là xoay compound body gây contact bùng lực. Biện pháp hiện tại là giới hạn angular speed/backlog, continuous collision cho bóng, speculative collision cho nắp, thành đủ dày và cap depenetration. Nếu test thiết bị thất bại, đo contact, timestep, hình dạng collider và solver để sửa nguyên nhân; giữ trọng lực thế giới và quán tính của các body tự do.

16 level prefab không chứng minh 16 puzzle đã cân bằng. Cần lưu solve route có input/timing và đánh dấu từng màn đã kiểm chứng, rồi playtest để người chơi hiểu nguyên nhân của chuyển động. Với nội dung mới, hình học vật cản quyết định đường đi; chấp nhận lời giải khác nếu đúng vật lý. Exit prerequisite trong các màn plate cũ là cơ chế kế thừa cần chuyển đổi, không phải quy tắc cho màn mới.

Build Android/iOS phụ thuộc module, SDK, signing và phần cứng. Giữ build script tái lập được; báo rõ build nào đã compile, build nào đã cài/chạy, và phép đo nào chưa thực hiện.

## Thay đổi yêu cầu ngày 08/09/2026: bi phải thoát ra ngoài

Yêu cầu trực tiếp của người dùng thay thế capture trong GDD gốc. Tiêu chí nghiệm thu: vỏ có lỗ thật, bi đi xuyên bằng Rigidbody, chưa thắng khi còn chồng mép cửa, chỉ thắng từ phía trong ra, không teleport/hút bi, và thấy chuyển động ngoài hộp trước khi đổi màn. Màn có prerequisite phải khóa cả vật lý lẫn điều kiện thắng. Reset phải xóa quá trình đi qua cửa và hủy chuyển màn đang chờ. Kiểm tra lại toàn bộ 16 đường giải sau khi sửa geometry.

Phản hồi tiếp theo: cửa phải là lỗ tròn khoét phẳng trên thành hộp, viền sáng mảnh/dịu; không có collider nhô lên gây vướng khi lăn. Tiêu chí bổ sung: bi lăn chậm trên sàn tự rơi qua lỗ mà không cần leo gờ hoặc được thêm lực nâng; hình dạng va chạm và vùng thắng cùng dùng tiết diện tròn.

## Định hướng vật lý sau phản hồi màn 6

Ưu tiên nguyên nhân vật lý nhìn thấy được, hình học/contact, khối lượng, ma sát, quán tính và trọng lực thế giới. [PHYSICS_DESIGN_PRINCIPLES.md](PHYSICS_DESIGN_PRINCIPLES.md) là chuẩn cho cơ chế mới, có ưu tiên so với các công tắc/tween quy ước của baseline GDD. L06 được chuyển trước thành nắp tự do; các tín hiệu, one-way gate và impulse pad cũ cần rà soát theo bảng chuyển đổi trong tài liệu đó, từng cơ chế kèm route/reset/performance validation.
