# Nhật ký phát triển Gravity Box

## Bàn 12 — mê cung ván ghép — 08–09/09/2026

- Ghép các ván thành mạng lối đi ba chiều, thay bố cục 32 ván rải rác. Tuyến chính có 25 đoạn và 21 lần đổi hướng, thêm hai nhánh cụt; đường cuối nối khung vào lỗ thoát trên vỏ cầu. Khe nhìn 15 mm nhỏ hơn bi 30 mm nên không còn đường rơi ra ngoài khung rồi lăn sát vỏ.
- 1.309 ván mỏng được bake thành 54 cụm mesh/collider: 27 ngã rẽ và 27 đoạn nối. Lòng đường rộng 50 mm; ván dày 3 mm, rộng 10 mm, chia thành mảnh ngắn. Collider gồm chính các ván nhìn thấy, không có khối kín vô hình phủ khe. Vật liệu kính nhẹ và nét viền giảm độ che khuất.
- Giữ nguyên bi thép, trọng lực thế giới, input/clock 120 Hz và điều kiện toàn bộ bi ra khỏi lỗ mới thắng. Metadata graph chỉ phục vụ authoring và kiểm chứng, không điều khiển player. [Thiết kế](LEVEL12_SPATIAL_MAZE.md), ADR 019 và kiến trúc đã cập nhật.
- Generate/ContentValidator đạt 12 bàn. Kiểm tra hình học độc lập trên lưới 3 mm xác nhận cả 25 đoạn tuyến chính đều cần thiết để tới vùng cuối và không liên thông với khoảng trống sát vỏ. Lượt kiểm tra mở rộng không gian cho tâm bi dùng bán kính hiệu dụng 12,402 mm; lượt thu hẹp dùng 17,598 mm vẫn có đường từ spawn tới trước lỗ. Đây là kiểm chứng hình học tĩnh, không thay cho lượt giải bằng physics.
- Render bốn góc từ prefab thật lúc 16:55:03 UTC. [Ảnh/metadata](Images/Level12/README.md) là fixture đặt pose, không phải lượt giải. Bố cục 32 ván cũ được lưu bằng chứng riêng trong [Archive](Archive/Verification/SphereFreePlanks/README.md).
- Toàn bộ **8/8 EditMode + 63/63 PlayMode = 71/71 passed**, không failure hoặc skipped. EditMode kết thúc 17:10:40 UTC; PlayMode từ 17:10:53 tới 17:11:26 UTC. Lượt giải qua đủ 25 đoạn chính, có rẽ vào nhánh cụt và quay lại bằng xoay thật, tổng góc xoay 1.598,7°, khoảng bay dài nhất 0,142 giây. [XML và phạm vi kiểm chứng](Verification/README.md).
- Các lượt fixture đầu đã tìm ra giả định đo không đúng: vùng tâm bi ở ngã rẽ không phải tiết diện thu hẹp của đoạn thẳng, bi không cần dừng hẳn để được coi là đã qua nút, và Transform nội suy không phải frame vật lý hiện tại. Khi trôi vào nhánh cụt, policy xoay quay lại thay vì giữ mãi hướng xuống. Chỉ sửa cách quan sát/điều khiển fixture; geometry, profile bi và input của player giữ nguyên trong giai đoạn này.

- macOS development player và Android ARM64/IL2CPP build thành công sau lượt kiểm tra cuối. APK thực tế **59,208,154 bytes** (56.47 MiB), tại `Builds/Android/GravityBox.apk`; macOS tại `Builds/macOS/Gravity Box.app`. Chưa chạy APK trên thiết bị Android hoặc xuất lại iOS. Hoàn nguyên thay đổi blend do importer tự tạo ở ba vật liệu bàn 11 sau khi Unity kết thúc.
- Đã mở player macOS mới và chọn màn 12 lúc **17:14:48 UTC**. Quan sát trực tiếp đúng khung ván liên thông, bi thép và HUD mới; để player mở tại màn 12 cho người dùng thử. Đây là kiểm tra khởi chạy/hiển thị, chưa phải lượt giải bằng tay hay bằng chứng độ khó. Lượt giải bằng physics nằm trong XML PlayMode ở trên.

## Bàn 12 — bản 32 ván rời đã thay thế — 08/09/2026

- Theo phản hồi trực tiếp, bỏ các vách lớn chia khoang và lỗ chuyển tiếp. Bố cục hiện tại gồm **32 ván kính nhỏ, cố định, tách rời** trong một khối cầu bán kính trong 0,36 m, vỏ 6 mm. Ván đặt tại nhiều vị trí và góc, không chia tầng; bi có thể lăn, rơi khỏi mép, va vào ván khác hoặc thành cầu. [Thiết kế bàn 12](LEVEL12_SPATIAL_MAZE.md).
- Thanh xuất phát và thanh đón cách nhau 14,5 cm theo chiều cao, giữa chúng có khoảng rơi thật. Có thêm 24 ván trong lòng cầu, 4 ván gần vỏ và 2 ván lệch nhau gần cửa. Vỏ có duy nhất lỗ tròn 46 mm, viền xanh lá mảnh và không gờ; toàn bộ bi phải thoát ra ngoài mới thắng.
- `SpatialMaze` chỉ lưu metadata vỏ và mảng ván; `SphereMazeBuilder`/`SphereMazeGeometry` tạo collider và mesh ở Editor. Không còn đồ thị khoang, hành lang giữ bi hoặc chuỗi ván bắt buộc. Bi 30 mm/~111 g, gravity thế giới 9,81 m/s² và mô phỏng 120 Hz giữ nguyên.
- Generate/ContentValidator đạt 12 bàn. Phép thử nghiêng nhanh ban đầu khiến bi bỏ qua ván đón rồi thoát bằng đường khác, nên không phù hợp để đo cú đáp định trước. Fixture sau dùng ý định nghiêng tăng dần để bi lăn khỏi mép; không sửa production physics, geometry hoặc ép quỹ đạo bi. Lượt tập trung **4/4 passed** lúc 16:36:59 UTC: rơi tự do 0,117 giây, giảm cao 0,1192 m, gia tốc đo giữa các bước bay phù hợp 9,81 m/s², rồi chạm ván đón thật.
- Route kiểm thử từ spawn chạm hai ván, có khoảng bay dài nhất 0,333 giây và thoát qua lỗ thật bằng rotation intent. Test không đặt lại pose/vận tốc bi, thêm lực điều khiển hoặc đổi gravity. Đây là một đường vật lý hợp lệ; không chứng minh phải đi qua cả 32 ván hay mức “siêu khó”.
- Bốn góc nhìn đã render từ prefab/camera thật, nhìn rõ ván rời và khoảng không; viền ván nhẹ, tắt phản chiếu trên ván để giảm các mảng trắng chồng nhau. [Ảnh/metadata](https://github.com/nghienvothuat-a11y/GravityBox/tree/34bb783/Docs/Images/Level12) là fixture đặt pose, không phải bằng chứng giải màn. Đã xóa 58 asset vách/lỗ/vật liệu cũ không còn được tham chiếu.
- Toàn bộ **8/8 EditMode + 63/63 PlayMode = 71/71 passed**, không failure hoặc skipped. EditMode kết thúc 16:38:06 UTC; PlayMode từ 16:38:19 tới 16:38:52 UTC. Bao gồm vỏ cầu ở 408 hướng, nghỉ ổn định với tải đỡ 0,95–1,05 mg, spawn/exit/reset và bộ chọn 12 bàn. [XML/phạm vi kiểm chứng](Archive/Verification/SphereFreePlanks/README.md).
- macOS development player và Android ARM64/IL2CPP build thành công. APK thực tế **59.207.939 bytes** (~56,46 MiB), lưu tại `Builds/Android/GravityBox.apk`; macOS tại `Builds/macOS/Gravity Box.app`. Chưa chạy APK trên Android thật hoặc xuất lại iOS. Các giá trị blend do importer tự thay ở ba vật liệu kính bàn 11 đã hoàn nguyên sau build về giá trị source trước đó; vật liệu riêng bàn 12 giữ nguyên cấu hình alpha.
- Đã mở native macOS, chọn màn 12 và nhìn thấy đúng ván rời/bi/lỗ thoát. Sau đó người dùng tiếp tục thao tác; trạng thái cuối quan sát là **BALL OUTSIDE**. [Log của bản ván rời](Archive/Verification/SphereFreePlanks/Native-Level12.csv) ghi hoàn thành lúc 16:41:11 UTC, **16,305 giây / 5 lượt kéo / 0 reset**. Không lưu được screenshot native mới vì người dùng đang điều khiển. Lượt hoàn thành nhanh này cho thấy bố cục hiện tại chưa đạt mục tiêu “siêu khó”; đây là bản thử ván rời và chuyển động, cần tinh chỉnh bố trí từ playtest nếu tiếp tục tăng độ khó.

## Mê cung bàn 10–11 — 08/09/2026

- Bàn 10 **Opposite ways**: ba khoang, vách ngang xen kẽ, hai hốc giữ và hai cửa trượt có hướng mở đối nhau. Tái sử dụng cửa trượt thụ động từ bàn 09, cho phép khai báo vị trí và góc đặt; cả hai body nhận cùng gravity thế giới, không motor hoặc unlock signal.
- Bàn 11 **Three dimensions**: hộp sâu 0,27 m với ba mê cung 4×4 riêng biệt. Sàn ở Y=+0,045/−0,045/−0,135 m, hai lỗ chuyển tầng thật đặt lệch nhau và lỗ thoát tròn ở tầng thấp nhất. Tất cả vách chạm sàn/nắp của tầng; đổi tầng là chuyển động Rigidbody liên tục.
- `InteriorDepth` và chiều cao spawn được author theo bàn. Metadata tầng cung cấp hình học/renderer/đường kiểm chứng; lớp hiển thị đọc vị trí bi và làm mờ tầng khác, có nút xem tổng thể. Việc đổi vật liệu không sửa collider, pose hoặc vận tốc.
- Generate/ContentValidator đạt 11 bàn. Lượt tập trung hai mê cung **6/6 passed** lúc 15:39:19–20 UTC. Toàn bộ **8/8 EditMode + 58/58 PlayMode = 66/66 passed**, không failure/skipped; kết thúc 15:40:05 và 15:40:47 UTC. [XML mốc 11 bàn](Archive/Verification/ElevenLevels/README.md).
- Cả hai đường giải từ spawn tới lỗ cuối đều chỉ gửi rotation intent qua controller chuẩn, dùng chung policy P8/D5/cap 1 m/s². Bài tầng còn theo dõi vị trí mỗi bước để phát hiện teleport. Các fixture đặt điều kiện đầu để đo lỗ/cửa riêng không được tính là lời giải.
- macOS và Android ARM64 đã build thành công; APK 59.202.143 bytes (khoảng 56,46 MiB). Native macOS đã mở bộ chọn 11 bàn, chọn 10 và quan sát bố cục/hai cửa, gồm trạng thái A đã mở / B đóng. Giữ nguyên lượt chơi 10 của người dùng; đã render riêng cả ba tầng và chế độ tổng thể của 11 với prefab/camera/layer view thật. Tầng hiện tại và bi đọc được khi các tầng khác mờ đi. Đây là fixture đặt bi để xem hình, không phải lượt giải native. [Ảnh và phạm vi quan sát](Images/Mazes/README.md). Chưa chạy APK trên Android thật hoặc xuất lại iOS.

## Bàn 09 — Leave it behind — 08/09/2026

Thêm một puzzle sau tám hộp thử hình học. Vách chia hai khoang, một thanh chặn trượt do gravity và hốc giữ bi tạo chuỗi suy luận: giữ bi bằng thành hốc, để thanh chặn rời cửa, rồi nghiêng bi qua khoang phải tới lỗ thoát.

- Bi dùng nguyên profile thép; thanh chặn PhysicalProp 0,18 kg, kích thước 0,026 × 0,080 × 0,076 m, hành trình +Z 0,12 m. Joint một trục không motor/drive/spring/projection; collider luôn bật và không phát unlock signal.
- Chuyển catalog/scene suites sang chín bàn, force registry theo số body thật. Bổ sung test cửa đóng ở toàn bộ chiều sâu, va chạm thật, gravity mở/đóng, 100 reset root/prop, thu hồi force target và route chỉ xoay từ spawn tới thoát thật.
- Lượt test với ray giữ sát sàn phát hiện mép nhô khoảng 1 mm cản bi đang lăn nhẹ dù thanh chặn đã mở. Đã bỏ hai ray dưới và giữ hai ray gắn phía nắp, để sàn qua cửa phẳng. Đây là sửa hình học từ lỗi passage thật, không nới ngưỡng test hoặc thêm lực đẩy bi.
- Generate và ContentValidator đã qua cho chín bàn. **8/8 EditMode + 49/49 PlayMode = 57 tests passed**, không failure/skipped; EditMode kết thúc 15:07:44 UTC, PlayMode từ 15:07:51 tới 15:08:14 UTC. [XML mốc chín bàn](Archive/Verification/NineLevels/README.md). Đường giải từ spawn tới thoát thật chỉ dùng rotation intent, giữ nguyên policy sau sửa ray.
- **Build/native:** macOS player và Android ARM64 APK đã build thành công; APK 59.194.461 bytes (56,45 MiB). Native macOS đã kiểm tra bộ chọn cuộn tới 09, bố cục, kéo xoay hộp/bi lăn, thanh chặn trượt mở theo gravity và reset trả root/bi/thanh chặn về đầu. [Ảnh cửa đóng/mở](Images/Level09/README.md). Native QA chưa hoàn thành một lượt giải từ spawn; bằng chứng lời giải đầy đủ ở fixture rotation-only. Chưa chạy APK trên Android thật hoặc xuất lại iOS.
- Một số thao tác kéo tự động native bị gộp sự kiện: callback nhấn/thả đều nhận tọa độ cuối và delta 0. Log tạm xác nhận các sự kiện có đủ đầu/cuối làm hộp xoay đúng; không sửa gameplay để đoán dữ liệu đã mất. Đã bỏ toàn bộ log input tạm khỏi source bàn giao.

## Bổ sung năm hình dạng — 08/09/2026

Theo yêu cầu mới, ba hộp cơ bản được giữ và bổ sung chữ L, chữ U, vành khuyên, quả tạ, ngôi sao, tổng cộng tám bàn. Mô hình bi thép 30 mm/111 g, gravity 9,81 m/s², contact và clock 120 Hz tiếp tục dùng chung.

- LevelRuntime lưu contour ngoài và vùng rỗng bên trong. Sàn/nắp/thành được tạo theo hình học thật; vành khuyên có lõi rỗng, các góc lõm chữ L/U/ngôi sao và cổ quả tạ có khoảng trống cho cả bán kính bi.
- Mở rộng catalog, rest/reset/containment/exit và scene manual-next sang cả tám bàn. Test hình học đi theo từng cạnh contour thay vì giả định origin luôn nằm bên trong; các grid probe kiểm tra sàn/nắp không lấp phần khuyết/lõi rỗng.
- Năm fixture passage dùng sphere sweep đầy đủ rồi nghiêng hộp qua waypoint tại các góc/cổ nối; không lái ball bằng lực riêng hoặc đặt lại vị trí giữa các waypoint. Các phép đo thép và cube của ba hình đầu vẫn giữ nguyên.
- Passage chạy riêng **5/5 passed lúc 14:40:35 UTC**. Policy đầu quá mạnh so với độ trễ/giới hạn đổi góc của hộp, khiến bi qua sát mục tiêu rồi dao động; log xác nhận mặt đỡ và contact bình thường. Giảm riêng gain/cap của policy kiểm thử (`P=8`, `D=5`, cap 1 m/s²) cho phép đi qua đủ waypoint; không sửa geometry/vật lý hoặc nới điều kiện clearance để làm test qua.
- Toàn bộ **8/8 EditMode + 44/44 PlayMode = 52 tests passed**, không có failure hoặc skipped test. EditMode kết thúc 14:41:26 UTC; PlayMode chạy từ 14:41:33 đến 14:41:53 UTC. Bằng chứng mốc tám hộp: [EditMode.xml](Archive/Verification/EightBoxes/EditMode.xml), [PlayMode.xml](Archive/Verification/EightBoxes/PlayMode.xml). Cả tám bàn qua kiểm tra nằm yên: không impact lặp lại, biên độ độ cao dưới độ phân giải log; peak speed lớn nhất 0,000002 m/s ở vành khuyên, lực đỡ khoảng 1,08868 N.
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
