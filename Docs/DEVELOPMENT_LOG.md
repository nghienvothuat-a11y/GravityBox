# Nhật ký phát triển Gravity Box

## Bổ sung năm hình dạng — 08/09/2026

Theo yêu cầu mới, ba hộp cơ bản được giữ và bổ sung chữ L, chữ U, vành khuyên, quả tạ, ngôi sao, tổng cộng tám bàn. Mô hình bi thép 30 mm/111 g, gravity 9,81 m/s², contact và clock 120 Hz tiếp tục dùng chung.

- LevelRuntime lưu contour ngoài và vùng rỗng bên trong. Sàn/nắp/thành được tạo theo hình học thật; vành khuyên có lõi rỗng, các góc lõm chữ L/U/ngôi sao và cổ quả tạ có khoảng trống cho cả bán kính bi.
- Mở rộng catalog, rest/reset/containment/exit và scene manual-next sang cả tám bàn. Test hình học đi theo từng cạnh contour thay vì giả định origin luôn nằm bên trong; các grid probe kiểm tra sàn/nắp không lấp phần khuyết/lõi rỗng.
- Năm fixture passage dùng sphere sweep đầy đủ rồi nghiêng hộp qua waypoint tại các góc/cổ nối; không lái ball bằng lực riêng hoặc đặt lại vị trí giữa các waypoint. Các phép đo thép và cube của ba hình đầu vẫn giữ nguyên.
- Passage chạy riêng **5/5 passed lúc 14:40:35 UTC**. Policy đầu quá mạnh so với độ trễ/giới hạn đổi góc của hộp, khiến bi qua sát mục tiêu rồi dao động; log xác nhận mặt đỡ và contact bình thường. Giảm riêng gain/cap của policy kiểm thử (`P=8`, `D=5`, cap 1 m/s²) cho phép đi qua đủ waypoint; không sửa geometry/vật lý hoặc nới điều kiện clearance để làm test qua.
- Toàn bộ **8/8 EditMode + 44/44 PlayMode = 52 tests passed**, không có failure hoặc skipped test. EditMode kết thúc 14:41:26 UTC; PlayMode chạy từ 14:41:33 đến 14:41:53 UTC. Bằng chứng: [EditMode.xml](Verification/EditMode.xml), [PlayMode.xml](Verification/PlayMode.xml). Cả tám bàn qua kiểm tra nằm yên: không impact lặp lại, biên độ độ cao dưới độ phân giải log; peak speed lớn nhất 0,000002 m/s ở vành khuyên, lực đỡ khoảng 1,08868 N.
- **Build/chạy thực tế:** bản macOS và APK Android ARM64 tám hộp đã build thành công (APK khoảng 56 MiB). Đã quan sát native bộ chọn tám bàn và cả năm hình mới; các thao tác Next đều chuyển bàn đúng. Kéo trên hộp ngôi sao làm hộp xoay và bi lăn từ hốc dưới lên nhánh phía trên; reset trả lại trạng thái đầu. [Chỉ mục ảnh native](Images/WeirdBoxes/README.md) ghi các hình được xem. Chưa chạy APK trên thiết bị Android hoặc xuất lại iOS cho mốc này. Kết quả build của mốc ba hộp bên dưới không tự xác nhận bản mở rộng.

## Đổi phạm vi sang ba hộp và bi thép — 08/09/2026

Theo yêu cầu mới, mục tiêu prototype là cảm giác tăng tốc, quán tính và va chạm của bi thép. Catalog chuyển về ba hộp tròn, vuông có cube cố định, tam giác. Các đoạn nhật ký 16 màn, nắp rơi và bằng chứng lời giải bên dưới thuộc baseline trước đó.

- Tỷ lệ mới: bi R=0,015 m, mass khoảng 0,111 kg; hộp rộng khoảng 0,34 m, sâu 0,09 m; gravity thế giới 9,81 m/s² và clock 120 Hz.
- Bộ kiểm tra catalog/lifecycle được chuyển sang ba bàn: spawn clearance, 100 reset mỗi bàn, vỏ/cửa thật, contact cube, full-sphere escape, manual next và thời gian thực sau thoát. Physics fixture riêng kiểm tra luật gia tốc, lăn và va chạm.
- Kế hoạch và triết lý thiết kế đã viết lại quanh cảm giác bi. Kế hoạch 16 màn/nắp rơi được lưu trong `Docs/Archive`; GDD và bằng chứng cũ vẫn giữ để truy vết.
- Generate và ContentValidator đã qua cho đúng ba hộp. Toàn bộ **8/8 EditMode + 32/32 PlayMode = 40 tests passed**, không có failure; EditMode kết thúc 14:13:35 UTC, PlayMode chạy từ 14:13:43 đến 14:13:54 UTC ngày 08/09/2026. Bộ mới kiểm tra profile thép, hình học/clearance/containment của prefab thật, rebound từ cube, full-sphere escape và lifecycle/manual next. Bằng chứng mốc ba hộp: [EditMode.xml](Archive/Verification/ThreeBoxes/EditMode.xml), [PlayMode.xml](Archive/Verification/ThreeBoxes/PlayMode.xml).
- Regression lăn trên mặt phẳng: vận tốc giảm từ **0,30000 xuống 0,28599 m/s trong 0,25 s**; động năng gồm tịnh tiến và quay giảm từ **0,0069915 xuống 0,0063536 J**, khoảng 9,1%. Lực đỡ đo được **1,08868 N**, phù hợp trọng lượng viên bi khoảng 111 g. Đây là phép đo trong fixture kiểm soát, không phải kết luận về cảm giác của mọi lượt chơi.
- Đã sửa lỗi điểm contact lưu từ callback chậm một bước so với tâm bi: tính vận tốc quay tại điểm cũ tạo ra thành phần tách mặt giả, khiến rolling resistance bị bỏ qua dù bi vẫn lăn trên sàn. Kiểm tra tách mặt hiện dùng vận tốc tương đối theo pháp tuyến tại vị trí tiếp xúc hình học của cầu; bi đang bay vẫn không bị rolling resistance làm giảm spin.
- Native review của bản thử phát hiện bi rung/nảy rất nhỏ khi phải nằm yên: ngưỡng bounce 0,05 m/s thấp hơn vận tốc trọng lực tích lũy trong một bước 120 Hz (~0,08175 m/s). Đã đặt ngưỡng 0,2 m/s và bổ sung regression trên cả ba prefab thực: sau 2 giây ổn định, đo liên tục 5 giây được vận tốc đỉnh 0, biên độ độ cao 0, lực đỡ không đổi 1,08868 N và không có impact mới. Restitution vật liệu 0,38 vẫn được kiểm tra tạo rebound với cube ở vận tốc 0,35 / 1 / 3 m/s.
- Bổ sung input regression cho chuỗi nhấn/kéo/thả và trả con trỏ về chỗ cũ trong cùng frame; góc xoay root thực tế phải vượt 5°. Điều này bảo vệ các thao tác kéo nhanh khỏi bị mất khi lấy mẫu input.
- **Build/chạy thực tế:** bản cuối chứa sửa resting/input đã build macOS và Android ARM64 thành công (APK khoảng 56 MiB). Đã quan sát native macOS: hộp tròn, bi phản chiếu, lỗ cắt phẳng và thao tác xoay làm bi chuyển động. Lần quan sát này phát hiện micro-bounce; bản sửa được xác minh bằng các phép thử nằm yên trên cả ba prefab và rebound với vật liệu thật ở trên. Chưa khởi động lại bản cuối vì người dùng đang chơi; cần thoát rồi mở lại app để nhận sửa resting/input. Chưa chạy APK trên thiết bị Android hoặc xuất lại iOS cho mốc này. Kết quả mobile và chơi L06 ở các mục lịch sử không xác nhận bản mới.

## Baseline ngày 08 tháng 09 năm 2026

Workspace ban đầu `/Users/mrk/GravityBox` trống, chưa có Unity project hoặc Git repository. Đã đọc đầy đủ GDD Word do người dùng cung cấp; nội dung prompt mẫu được giữ dưới dạng nguồn tham khảo, không dùng để giới hạn yêu cầu hiện tại thành riêng M1.

## Mốc và kết quả

| Mốc | Thay đổi | Kiểm chứng | Trạng thái |
| --- | --- | --- | --- |
| M0 | Unity 6000.3.19f1, URP 17.3, Input System 1.17, asmdef, portrait, project settings | Unity import/compile và build | Đã thực hiện |
| M1 | Camera cố định, acrylic shell, mouse/touch, assisted/free/quarter-turn, tuning asset | Rotation cap, 24 orientation; integration mouse/touch và reset button | Đạt automated; feel cần người chơi |
| M2 | Ball world-space, explicit force provider, ramp, physical exit | Gravity acceleration; zero-G không steering; render native | Đạt kiểm thử |
| M3 | Session state, reset registry, fail, transition cancellation | 100 resets; exit/fail event một lần; reset khi completing | Đạt kiểm thử |
| M4 | Catalog/definition/prefab, persistent Gameplay scene | Load 16 definition đúng profile; advance tới Finished; replay | Đạt kiểm thử |
| M5 | Plate, signal door, one-way gate, spring/impulse, kill volume | Trigger, channel prerequisite, cooldown reset, collision-ignore reset | Đạt kiểm thử |
| M6 | Zero-G profile, launch velocity, momentum trail | 120 physics ticks không mất momentum; root rotation không đổi velocity khi không contact | Đạt kiểm thử |
| M7 | 10 gravity + 6 zero-G layouts, solution records | 16/16 physics-only solve routes được replay; spawn validator | Khả giải kỹ thuật; cần cân bằng qua playtest |
| M8 | HUD, level selector, pause/debug, audio, safe area, ball occlusion cue | Native screenshot; automated UI input; kiểm tra shader trong build | Một phần; chưa nghiệm thu thiết bị thật |
| M9 | macOS player, Android APK, iOS Simulator export, CLI commands | macOS và Android build thành công; iOS xem phần nền tảng | Chưa đạt GDD device/playtest gate |

## Test suite

7 Edit Mode + 30 Play Mode = **37 bài test**, đều pass sau khi cập nhật nắp vật lý. Lượt đầy đủ với replay nghiêm ngặt kết thúc 08/09/2026 lúc 05:59:20 UTC. Baseline trước thay đổi cửa có 26 bài test; bản lỗ tròn có 31. Phần hình ảnh được kiểm tra thêm bằng build và ảnh native.

Edit Mode bao gồm reset registry deduplication, terminal transition, pause gate, gravity profiles, 24 canonical rotations, instance signal isolation và catalog integrity.

Play Mode bao gồm mouse drag, fast drag giữa frame, one-finger touch, vuốt chỉ có begin/end vẫn giữ đoạn di chuyển cuối, UI reset không xoay hộp, gravity acceleration, zero-G drift và rotation invariance, speed limit, 100 resets, launch-velocity restore, exit/hazard, plate-door prerequisite, one-way ignore pair, bumper cooldown, load mọi level, cancel pending next, toàn bộ progression và replay solve routes.

Kết quả XML/log nằm tại `Artifacts/editmode-results.xml`, `Artifacts/playmode-results.xml`, `Artifacts/editmode.log`, `Artifacts/playmode.log`. Chạy lại bằng `bash Tools/verify.sh` khi Editor không giữ project lock.

Các phép reset kiểm tra state khôi phục, không tuyên bố bitwise deterministic trên mọi PhysX/platform. Bài khả giải điều khiển rotation intent quanh world Z; chứng minh có đường vật lý tới exit, chưa chứng minh người mới thực hiện dễ dàng bằng camera-relative drag. Chi tiết trong [SOLVABILITY.md](SOLVABILITY.md).

## Lỗi đã tìm và sửa

- asmdef test ban đầu khai báo runner trùng với TestAssemblies; đã bỏ tham chiếu trùng.
- Tên SessionState xung đột UnityEditor trong tests; đã dùng alias tường minh.
- Input integration trong headless Editor bị focus routing chặn; harness tạm đặt IgnoreFocus/AllDeviceInputAlwaysGoesToGameView và phục hồi trong teardown. Không đổi focus policy của game để làm test pass.
- Mouse polling có thể mất thao tác đã hoàn tất giữa hai frame; chuyển capture press/release sang InputAction events và có regression test.
- Touch kết thúc vẫn có thể mang vị trí mới; áp delta cuối trước EndDrag, có regression test cho gesture không có moved sample.
- HUD fixed width có thể tràn trên điện thoại dài; CanvasScaler hiện match theo chiều ngang, phần cao bổ sung phân phối giữa các anchor.
- Bóng có thể bị vách đặc che; thêm một silhouette pass chỉ hiện phần bị occlude, dùng cùng mesh sphere và không thêm collider/force. Camera framing radius nâng lên 5.3 để tính tới corner khi xoay tự do.
- Xcode Simulator lần đầu lỗi link do Unity export x86_64 nhưng compile arm64. BuildIOSSimulator hiện chọn ARM64 tường minh, đồng thời phục hồi setting sau export.

## Nền tảng và giới hạn

**macOS:** development player build thành công, chạy native trên Apple M5. Đã xem L01/L03/L14, level selector đủ 16 màn, reset keyboard và UI, pause/diagnostics, lưu ảnh bằng F12. Chuột/touch được kiểm tra end-to-end bằng Input System events; macro kéo của công cụ điều khiển desktop không cho một cử chỉ kéo liên tục đáng tin cậy, nên không dùng macro đó để tuyên bố feel đã đạt.

**Android:** APK development ARM64/IL2CPP được tạo tại `Builds/Android/GravityBox.apk`, khoảng 73 MB ở bản cuối. ADB không có thiết bị kết nối; SDK không có system image emulator. Chưa cài/chạy hoặc profile Android thật.

**iOS:** Xcode build ARM64 thành công và đã cài/chạy trên iPhone 17 Simulator với iOS 26.5. Đã kiểm tra bố cục portrait/safe area và chạm nút Reset. Export simulator tắt MSAA để tránh mismatch attachment 1/4 samples của Metal; asset baseline được phục hồi về MSAA 4 sau export. Không còn lỗi render attachment ở bản đã sửa. URP còn một warning fallback shadow depth 16-bit từ package trên simulator, không chặn gameplay. Chưa có iPhone thật hoặc signing được xác minh. Script tái lập: `Tools/run-ios-simulator.sh`.

Không đo FPS/CPU/GPU p95/GC allocations trên Android/iPhone thật; chưa có thermal/battery test hoặc dữ liệu từ người mới. M8/M9 vì vậy chưa được đánh dấu hoàn tất theo Definition of Done toàn phần của GDD.

## Tuning hiện tại

| Tham số | Giá trị |
| --- | --- |
| Fixed timestep | 1/60 giây |
| Target FPS | 60 |
| Base gravity | 9.81 m/s² |
| Zero-G damping | 0 linear, 0 angular |
| Ball mass/radius | 1 kg / 0.27 m |
| Max speed / angular speed | 12 m/s / 35 rad/s |
| Max depenetration speed | 3 m/s |
| Rotation sensitivity | 200° / chiều ngắn màn hình |
| Rotation cap / smoothing | 100°/s / 0.09 giây |
| Rotation backlog / snap cone | 14° / 10° |
| Completion / failure feedback | 1,8 / 0,7 giây thực; completion ở time scale 0,35 |

## Việc tiếp theo có ưu tiên

1. Chơi touch trên một Android tầm trung và một iPhone thật, profile 15–20 phút ở 30/60 FPS và thao tác nhanh.
2. Ghi route từ thao tác người chơi, làm rõ L06/L10/L14 và giảm khả năng giải do quay ngẫu nhiên; thu phản hồi zero-G trước khi thêm lực mới.
3. Tuning tiếng động/impact; haptics, sensitivity settings, localization/accessibility chưa triển khai đầy đủ.
4. Đo contact/CCD của ball và props trên thiết bị; chỉnh hình học, timestep và solver khi có bằng chứng, giữ trọng lực thế giới.
5. Qua GO gate mới triển khai save schema, loading bất đồng bộ nếu cần, material profiles và Phase 2. Kế hoạch chi tiết ở [IMPLEMENTATION_PLAN.md](IMPLEMENTATION_PLAN.md).

## Cập nhật cửa thoát thật — 08/09/2026

- Thay socket trigger và CapturePoint ở 16 prefab + mechanism library bằng lỗ vuông xuyên vỏ, collider viền và shutter theo channel.
- ExitSocket kiểm tra hướng đi từ trong ra, tiết diện cửa, bán kính toàn bi và sweep qua bước physics. Không đặt position hoặc velocity khi thắng.
- Giữ cảnh thắng 1,8 giây thực ở 0,35×, camera mở khung theo bi; reset hủy payoff và mẫu traversal cũ.
- EditMode: **7/7 passed**. PlayMode: **22/22 passed**, bao gồm kiểm tra bi xuyên collider cửa thật trên cả 16 màn, partial/inward/missed/swept traversal, shutter, giữ momentum và chuỗi scene hoàn chỉnh.
- Ghi lại 16 route từ spawn chỉ bằng xoay hộp; lần chạy tiếp theo replay nghiêm ngặt (không bật tìm route) cũng **22/22 passed**, kết thúc 04:24:51 UTC. Fixture gần cửa trong integration test chỉ kiểm tra aperture/lifecycle; không được tính là bằng chứng giải màn.

- macOS và Android đã build lại thành công; APK khoảng 53 MB. Quan sát native L11 xác nhận đủ ba trạng thái: trong hộp, đang qua cửa (chưa thắng), ngoài cửa (BALL ESCAPED). Ảnh tại `Docs/Images/Exit-Inside.png`, `Exit-Through.png`, `Exit-Escaped.png`.
- iOS Simulator ARM64: Xcode build succeeded, cài và khởi động bản mới trên iPhone 17 / iOS 26.5; native render xác nhận cửa thật và HUD mới (`Docs/Images/iOS-PhysicalExit.png`). Đây là kiểm tra khởi động/render, không phải nghiệm thu touch hoặc hiệu năng trên iPhone thật. Simulator được trả về trạng thái tắt sau kiểm tra.

## Cập nhật lỗ tròn khoét phẳng — 08/09/2026

- Bỏ toàn bộ gờ vuông cao 0,60 m. Lỗ tròn bán kính 0,78 m được cắt trực tiếp qua mặt vỏ dày 0,18 m; dùng cùng mesh cho render và va chạm. Ring phẳng rộng 0,018 m, không collider, màu xanh dịu.
- Shutter tròn nằm chìm trong vỏ. Điều kiện thoát chuyển từ tiết diện vuông sang clearance tròn, đồng bộ mép ngoài thực; giữ nguyên động lượng và phần quan sát sau thắng.
- **7/7 EditMode và 24/24 PlayMode passed**. Bài mới xác nhận bi lăn chậm 1,2 m/s trên sàn tự rơi qua lỗ, không cần nhảy/leo gờ; góc của cửa vuông cũ không còn là vùng thắng.
- Tạo lại 16 route bằng xoay hộp từ spawn dưới hình học mới. Kết quả và thời gian ở SOLVABILITY/VERIFIED_ROUTES; không dùng fixture gần cửa để tính bằng chứng giải màn.

- Sau tinh chỉnh độ rộng/offset riêng của nét sáng, replay nghiêm ngặt 16 route vẫn pass (kết thúc 2026-09-08 04:44:55Z). Geometry va chạm và logic gameplay không thay đổi trong bước tinh chỉnh nét sáng.
- Đã xem native: bi trong hộp → qua lỗ tròn → ngoài hộp/đã thắng. Ảnh ở `Exit-Round-Inside/Through/Escaped.png`; ảnh `Exit-RoundFlush.png` là shutter tròn đang khóa ở L06. Nét sáng sau đó được tăng nhẹ từ 0,012 lên 0,018 m và dịch render 0,003 m để tránh đứt nét ở góc xiên; không thay collider.
- macOS và Android đã build lại bản cuối; iOS Simulator export và Xcode ARM64 build succeeded. Kiểm tra native tương tác của thay đổi này thực hiện trên macOS; chưa chạy lại bản lỗ tròn trên simulator hoặc thiết bị mobile thật.

## Nắp tự do và thiết kế dựa trên vật lý — 08/09/2026

- L06 đổi thành **Let it fall**, bỏ plate, cửa tín hiệu và khóa exit. Nắp là một đĩa tròn rời tựa phía trong, một Rigidbody 2 kg với một collider lồi. Khi lỗ lên trên, nắp rơi vào hộp theo gia tốc Trái Đất; sau đó vẫn va chạm và có thể bịt lại lỗ nếu rơi về đúng vị trí.
- PhysicalProp tách khỏi root từ lúc load, đăng ký vào hệ lực chung với ball, lưu/khôi phục pose và vận tốc, được thu hồi khi đổi màn. Không có điều kiện góc, tween, lực kéo nắp hay tắt collider khi mở.
- Nắp dùng ContinuousSpeculative; kiểm tra resting, blocked ball, gravity/zero-G, không kế thừa phép xoay của hộp, nhiều vòng xoay mạnh, 100 reset và unload.
- Cập nhật triết lý thiết kế, ADR, kiến trúc và kế hoạch authoring. Các signal door/one-way/pad cũ ngoài L06 được ghi rõ là nội dung kế thừa cần rà soát, chưa chuyển đổi hàng loạt.
- Phát hiện route lật lên rồi trả ngay có thể khiến nắp tự rơi về bịt lỗ. Giữ nguyên hành vi vật lý này; hướng dẫn thêm bước nghiêng nắp sang bên. Route được kiểm tra từ spawn, chỉ xoay hộp, không di chuyển bi hoặc nắp bằng test harness.
- **7/7 EditMode và 30/30 PlayMode passed** ở lượt replay nghiêm ngặt, kết thúc 05:59:20 UTC. L06 có chương trình giữ nguyên ban đầu → 180° → 90° → nghiêng hai trục theo vị trí bi, mô phỏng cả bi và nắp. Policy kiểm thử được thử qua ba lần tải màn mới trước khi lưu; chỉ gửi rotation intent, không có trong player. Ngân sách input và giới hạn bằng chứng ở SOLVABILITY.md.
- Replay riêng bài khả giải cũng **passed** lúc 05:58:14 UTC. Kiểm tra toàn bộ và chạy riêng đều dùng fixed timestep 1/60 s; TimeManager asset được đồng bộ với GameBootstrap. Fixture lưu/phục hồi timing, không phụ thuộc bài test khác đã mở Gameplay.
- Nắp có thể đẩy bi lệch theo chiều sâu nên lời giải một trục không đủ làm regression cho L06. Đổi phần kết thúc bằng policy quan sát vị trí bi và nghiêng hộp trên hai trục. Quy tắc, thời hạn và giới hạn bằng chứng được khai báo trong SOLVABILITY/VERIFIED_ROUTES; không biến policy kiểm thử thành hỗ trợ tự lái trong game.
- Log từ player macOS đang chạy ghi nhận L06 hoàn thành lúc 05:49:33 UTC, **22,316 giây / 11 lượt kéo / 0 reset**. Dữ liệu lịch sử ở [Native-Level06.csv](Archive/Verification/Native-Level06.csv). Đây là một lượt chơi thành công của catalog cũ, không thay thế playtest nhiều người hoặc nghiệm thu thiết bị mobile.
- macOS player, Android ARM64 APK và iOS Simulator export đã build lại thành công. Xcode ARM64 simulator **BUILD SUCCEEDED**. Bản nắp vật lý được chơi trên macOS; chưa chạy lại bản cuối trên simulator hoặc thiết bị Android/iPhone thật.
