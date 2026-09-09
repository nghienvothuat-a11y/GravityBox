# Các quyết định kiến trúc

**Phạm vi hiện tại:** ADR 025 thêm luật tất cả bi thoát và màn phối hợp hai bi, tổng 16 màn. Hỗ trợ thoát ADR 023 áp dụng riêng cho từng bi. Các quyết định cũ bên dưới giữ lại để theo dõi lịch sử.

## ADR 025 Toàn bộ bi phải thoát, cơ quan phối hợp ở bàn 16

Thêm danh sách spawn/bi và trạng thái traversal/assist riêng cho mỗi bi vào runtime chung. Không hard-code điều kiện hai bi trong luật thắng. Bàn 16 dùng hai khoang và nút nhấn có tiếp xúc, lò xo thực, cửa trượt có collider và lực motor giới hạn. B giữ chốt mở hai cửa để A có đường rời hốc; bi nào thoát trước cũng không làm mất khả năng giải. Đây là cơ quan được cấp năng lượng, phân biệt với slider trọng lực thụ động ở bàn 09/10. [Thiết kế](LEVEL16_COOPERATIVE.md).

## ADR 024 Hình đầu sư tử là hình học của hộp

Màn 15 dùng một mặt nạ sư tử có chiều sâu: contour lõm tạo bờm/tai, năm gờ mặt vát cạnh tạo va chạm. Chọn hình cách điệu rõ đường lăn qua nắp kính. Vỏ và các khối nổi có collider theo mesh; inlay màu không collider. Dùng lại runtime và toàn bộ profile bi/gravity/rotation/exit, chỉ thêm authoring builder cùng shape ID nối cuối. [Thiết kế](LEVEL15_LION_HEAD.md).

## ADR 023 Hút bi gần lỗ và sàn trong theo hướng nhìn

Người dùng yêu cầu giảm độ khó khi đã đến gần lỗ: áp dụng motor lực ở vùng 40 mm cho mọi màn, căn giữa rồi đẩy ra. Đây là ngoại lệ gameplay được yêu cầu rõ ràng đối với nguyên tắc không hút bi trước đó. Không thay profile vật lý; giữ collider, khóa cửa và điều kiện toàn bộ bi phải thật sự ra ngoài. Trạng thái hỗ trợ không đồng nghĩa hoàn thành. Bi còn dynamic sau thắng. [Chi tiết](EXIT_ASSIST.md).

Để xem được bi khi lật hộp chất lỏng, sàn bàn 13/14 đổi opacity bằng property block theo hướng mặt sàn tới camera. Không tắt collider hoặc làm mờ bi/cube. Không sửa shared material trong mỗi frame.

## ADR 022 Thủy ngân dùng chung mô hình chất lỏng

Thêm màn riêng với hình học bàn 02/13 và profile thủy ngân ở 20°C. Giữ solver/bi/gravity: lực nổi lớn hơn trọng lượng thép nên bi nổi về mặt cao nhất. Khi chỉ nổi một phần ở cửa thì chưa hoàn thành; người chơi phải xoay tiếp để cả bi rời vành thật.

Thủy ngân đục sẽ che toàn bộ trò chơi trong hộp đầy. Chọn view bạc nhìn xuyên có nhãn rõ, bỏ caustic nước, giữ tracer chỉ để quan sát. Không thay tính chất vật lý để phục vụ hình ảnh. Root vẫn kinematic; không tuyên bố mô phỏng việc tay xoay khối thủy ngân khoảng 113 kg. [Công thức/giới hạn](LEVEL14_MERCURY.md).

## ADR 021 Cản lăn sát thành và quán tính nước

Phản hồi cho thấy cản nước chưa thuyết phục. Bản đầu áp mô hình cầu cô lập cả lúc bi lăn sát đáy. Bổ sung tương quan cản lăn gần mặt phẳng, tra collider thật để không đóng cửa thoát; giữ các thông số nước. Tách công thức vào WaterHydrodynamics, có benchmark lực và chuyển động theo miền Reynolds.

Added mass bằng nửa khối lượng nước bị chiếm được đưa vào Rigidbody.mass để solver dùng cùng quán tính cho lực và tiếp xúc. Khối lượng thép trong profile/HUD và mô-men quán tính thép giữ nguyên. WaterVolume bù gravity của added mass, bổ sung lực gia tốc từ trường dòng, phục hồi mass khi ra nước/disable/reset. Tránh phản hồi gia tốc đo từ frame trước của bi vì impulse tiếp xúc có thể bị nhầm thành lực thủy động lực và gây rung.

Giới hạn: added mass đẳng hướng của cầu trong môi trường rộng; cản thành nội suy cho gần mặt phẳng và lăn ít trượt; chưa giải squeeze-film va chạm hoặc áp suất CFD. Hệ số nối miền và độ nhám là giả định công khai. Bộ kiểm chứng đối chiếu nghiệm mô hình và hội tụ bước thời gian, chưa chứng minh trùng thiết bị thật. Chỉ build macOS theo yêu cầu; APK trước đó giữ bản nước đầu tiên. [Công thức, nguồn và giới hạn](LEVEL13_WATER.md).

## ADR 020 Hộp đầy nước để so sánh bi thép

Bàn 13 dùng cùng hình học hộp vuông, cube, spawn và lỗ thoát như bàn 02. Nước có lực nổi theo thể tích bị chiếm và lực cản cầu theo Reynolds/vận tốc tương đối. Mật độ nước và độ nhớt được tách vào WaterProfile; không dùng giảm gravity hay damping toàn cục để giả cảm giác nước.

WaterVolume đăng ký với EnvironmentForceSystem trong một lần load. Provider chuẩn bị dòng khối và lực mỗi fixed step; ball vẫn độc lập với hộp và không bị đặt lại pose/vận tốc. Tại cửa, tỷ lệ ngập giảm liên tục cho tới không còn lực nước. Nước được giữ đầy theo quy tắc thí nghiệm do người dùng yêu cầu; không tạo collider bịt lỗ hoặc mô phỏng tháo nước.

VFX gồm thể tích nhuộm màu, kính nhẹ, caustic thủ tục và mesh tracer/wake có giới hạn. Presentation không tạo lực. Bản đầu chưa có added mass/cản sát thành; ADR 021 bổ sung mô hình này. Caustic và dòng khối vẫn là xấp xỉ. [Thiết kế và giới hạn](LEVEL13_WATER.md).

## ADR 019 Ghép các ván thành mê cung nối ba chiều

Phản hồi sau khi chơi yêu cầu các mảnh ván gắn kết thành mê cung thực sự. Bản 32 ván rải rác có đường thoát nhanh, vì vậy tăng số vật cản ngẫu nhiên không giải quyết được vấn đề. Chọn mạng đoạn nối hẹp trong cầu, với 25 đoạn trên tuyến chính, 21 lần đổi hướng và hai nhánh cụt. Những hướng không nối tại mỗi ngã rẽ có ván chặn thật.

Mỗi mặt được ghép từ ván mỏng, để khe nhìn nhỏ hơn viên bi. Lối đi rộng 50 mm; khe nhìn 15 mm giữ bi 30 mm ở trong mạng đường đi. Các ngã rẽ/đoạn nối dùng MeshCollider theo đúng hình học ván, cùng root kinematic. Đoạn cuối nối với lỗ thật trên vỏ nên không thể rơi ra ngoài mạng rồi lăn sát cầu để đi tắt. Không thêm lực, collider vô hình hoặc khóa tiến độ.

SpatialMaze lưu graph và descriptor để kiểm chứng; không có xử lý graph trong vòng mô phỏng. Kiểm tra geometry phải xác nhận không có đường tắt giữa các đoạn không nối và mọi đoạn cần thiết đều cho cả viên bi qua được; kiểm tra PlayMode bắt đầu từ spawn, chỉ gửi ý định xoay. Đánh giá độ khó vẫn cần lượt chơi thật; không dùng số nút hoặc thời gian chạy policy làm kết luận “siêu khó”.

Xem [thiết kế hiện tại](LEVEL12_SPATIAL_MAZE.md) và [bằng chứng bản ván rời đã thay thế](Archive/Verification/SphereFreePlanks/README.md).

## ADR 018 Các thanh rời và khoảng rơi tự do trong cầu thủy tinh — đã thay thế

Bàn 12 dùng 32 thanh kính nhỏ, cố định và tách rời trong một vỏ cầu trong suốt. Người chơi có thể lăn trên thanh, rơi qua khoảng không, chạm thanh khác hoặc thành cầu. Không dùng các tấm vách lớn chia phòng, hành lang kín hoặc lỗ chuyển tiếp. Lỗ tròn duy nhất là cửa thoát trên vỏ. Nhiều đường đi và cách bỏ qua thanh bằng động lượng đều hợp lệ.

Bán kính trong cầu 0,36 m, vỏ dày 0,006 m; các thanh chủ yếu dày 0,004 m. Bố trí gồm thanh xuất phát, thanh đón đầu tiên, 24 thanh phân bố theo seed authoring, 4 thanh gần vỏ và 2 thanh lệch gần cửa. Mỗi thanh là một BoxCollider thuộc root kinematic. Bi vẫn dynamic trong world space, gravity vẫn 9,81 m/s²; không có lực hỗ trợ đáp xuống hoặc logic chuyển bi giữa các thanh.

SpatialMaze lưu metadata vỏ/thanh/seed, không lưu graph phòng hoặc lời giải để điều khiển player. Kiểm thử dành riêng đo khoảng rơi tự do và gia tốc, contact khi đáp, một route vật lý hợp lệ và reset/unload. Các kiểm tra vỏ cong và điều kiện toàn bộ bi thoát vẫn áp dụng. Độ khó cần được đánh giá riêng bằng chơi thực tế, vì bố trí mở cho phép nhiều đường và không bắt buộc chạm mọi vật cản.

Xem [thiết kế và kiểm chứng bàn 12](LEVEL12_SPATIAL_MAZE.md).

## ADR 017 Hai cửa ngược hướng và mê cung ba tầng thật

Bàn 10 **Opposite ways** dùng hai bản của thanh trượt bàn 09, đặt ở hai vách ngăn liên tiếp. Cửa A trượt theo local +Z của hộp, cửa B được xoay 180° quanh Y để trượt theo −Z. Hai hốc giữ bi và hành lang đổi hướng tạo chuỗi thao tác vật lý; không thêm trạng thái unlock, motor hoặc logic cửa phụ thuộc cửa trước. Có ba force targets: bi và hai thanh chặn. Reset/cleanup dùng registry hiện có.

Bàn 11 **Three dimensions** tăng khoảng cách tâm sàn ngoài–nắp lên 0,27 m và có ba sàn tại Y=+0,045; −0,045; −0,135 m, mỗi tầng một mê cung 4 × 4 có nhánh cụt. Hai lỗ chuyển tầng R=0,038 m đặt lệch nhau; lỗ ra cuối giữ R=0,023 m. Bi tự rơi qua sàn theo gravity; runtime không đổi pose hoặc vận tốc khi chuyển tầng. Vách từng tầng cao đủ khoảng trống giữa hai sàn, ngăn đi xuyên hoặc vượt qua đường phân cách chỉ bằng một phần bán kính.

LevelRuntime công khai InteriorDepth để authoring, kiểm tra vỏ và framing không giả định mọi hộp sâu 9 cm. LayeredMaze lưu metadata của các sàn/lỗ/route và đọc tầng từ vị trí bi. MazeLayerView chỉ đổi vật liệu để đọc tầng hiện tại hoặc tổng thể; mọi collider hoạt động đồng thời. Bằng chứng gồm route từ spawn tới thoát chỉ xoay hộp, liên tục qua cả ba tầng, cộng fixture sàn/lỗ và kiểm tra chế độ xem không đổi physics. Độ khó và khả năng nhìn đường trên thiết bị thật vẫn cần đánh giá trực tiếp; không suy diễn từ route tự động.

Xem [bố trí và cách kiểm chứng hai mê cung](LEVEL10_11_MAZES.md).

## ADR 016 Thanh trượt trọng lực và hốc giữ bi

Bàn 09 **Leave it behind** dùng một PhysicalProp 0,18 kg trên ConfigurableJoint nối root hộp, hành trình 0–0,12 m dọc local +Z. Joint khóa các trục còn lại; không spring, motor, drive hoặc projection. Thanh chặn luôn dynamic và luôn va chạm. GravitySliderGuide chỉ đọc vị trí/vận tốc tương đối và clearance hình học, không có logic unlock.

Hốc giữ bi dùng vách vật lý: khi nghiêng +Z, bi bị thành sau giữ trong khi thanh trượt tiếp tục mở cửa. Chuyển sang +X đưa bi qua cửa; hướng nghiêng ngược có thể đóng thanh chặn lại. Tám hộp thử giữ nguyên nội dung và physics tuning. Số force targets là một ở các hộp thử và hai ở bàn 09; reset/release được quản lý qua PhysicalProp/LevelRuntime có sẵn.

Ray là ràng buộc lý tưởng không ma sát dọc trục; contact material áp dụng cho tiếp xúc thật với housing/end stop. Đây là giới hạn mô hình được khai báo, không bù bằng điều kiện góc hoặc xung lực riêng. Bằng chứng cần gồm cửa đóng ngăn toàn bộ cầu, mở/đóng dưới trọng lực, reset hai body và một route từ spawn chỉ điều khiển hộp.

## ADR 015 Năm hình mới dùng polygon lõm và lõi rỗng thật

Theo yêu cầu người dùng, giữ ba hình cơ bản và thêm chữ L, chữ U, vành khuyên, quả tạ, ngôi sao. Chỉ mở rộng nội dung hình học; ball mass/radius/inertia, gravity, rolling resistance, restitution, clock 120 Hz và điều khiển hộp dùng chung profile.

Mỗi LevelRuntime lưu Footprint ngoài và FootprintVoids để authoring, validation và framing có cùng nguồn hình học. Sàn/nắp được triangulate theo miền polygon có thể lõm/có lõi rỗng; không dùng convex hull hoặc quạt tam giác từ origin. Renderer và MeshCollider chia sẻ mesh đã bake. Contour trong vành khuyên có thành riêng, phần giữa hoàn toàn trống.

Chữ L/U kiểm tra đổi hướng ở góc lõm; vành khuyên có biên trong/ngoài; quả tạ có hai buồng nối bằng cổ rộng 0,10 m; ngôi sao nối các cánh qua vùng giữa. Lối đi phải đủ toàn bộ cầu đường kính 0,03 m với clearance. Test dữ liệu, floor/void và sweep được bổ sung bằng fixture chỉ xoay hộp để đưa bi qua waypoint thật; không thêm logic hỗ trợ trong player.

Catalog và bộ chọn bàn có đúng tám lựa chọn, Next quay vòng. Camera/bounds theo kích thước footprint để không cắt mất hộp mới. Bộ mở rộng đã qua 52 tests; bằng chứng 40 tests của mốc ba hộp được giữ riêng trong Archive để đối chiếu.

## ADR 014 Prototype bi thép với ba hình học

Người dùng xác định chuyển động của bi thép là yếu tố cốt lõi cần nghiệm thu trước khi phát triển puzzle. Catalog chỉ gồm Circle, Square và Triangle; Square có một cube cố định. Asset cũ được giữ ngoài catalog. Không mở rộng nội dung khi chưa xác nhận cảm giác lăn, gia tốc và va chạm.

Chọn scale bàn nhỏ: ball radius 0,015 m và mass khoảng 0,111 kg, khớp thép đặc khoảng 7.850 kg/m³; hộp rộng khoảng 0,34 m và sâu 0,09 m. Một Unity unit bằng một mét. Tăng mass riêng lẻ không thay đổi gia tốc rơi dưới trọng lực, nên việc tạo cảm giác nặng phải đồng bộ scale, inertia, contact, restitution, rolling resistance và feedback.

Clock vật lý chuyển từ 60 lên 120 Hz, gravity vẫn 9,81 m/s² trong world space. Bi độc lập với transform root; cube cố định thuộc compound kinematic của hộp. Input chỉ tạo chuyển động hộp có giới hạn, không lái ball. Contact offset, ngưỡng bounce, solver và angular velocity được đặt theo kích thước bi nhỏ và kiểm tra bằng dốc/va chạm có điều kiện đầu xác định.

Cửa tròn phẳng tiếp tục là lỗ thật, R=0,023 m và wall half-depth=0,003 m. Sai số traversal tỷ lệ với radius thay cho dung sai 0,015/0,02 m của scale cũ. Escape giữ body dynamic và time scale 1; bỏ tự advance để người chơi tự quan sát rồi reset/chuyển bàn. Next quay vòng ba thí nghiệm.

Acceptance chuyển từ replay 16 lời giải sang định lượng luật chuyển động, collider của ba hình, va chạm cube, reset/input và chơi thử cảm giác. Kết quả test không tự chứng minh cảm giác đã đạt; phải ghi riêng việc đã chơi và phản hồi người dùng. Kiến trúc module hiện tại được giữ, tránh xây thêm hệ thống product trước khi mô hình vật lý được chốt.

## ADR 001 Unity 6.3 LTS và URP

Chọn 6000.3.19f1 có sẵn trên máy và URP 17.3.0. Unity xác định dòng 6000.3 là LTS trong tài liệu API. Cố định versions và packages-lock.json; không tự nâng giữa các phép so sánh vật lý. URP tránh phải chuyển toàn bộ vật liệu khi tiến tới mobile product; không thêm post-processing hoặc shader nặng ở prototype.

## ADR 002 Hộp kinematic và bóng world space

Chọn physical-root convention đầu tiên như GDD đề xuất. MoveRotation theo fixed tick, bounded angular velocity và input backlog. Ball không parent vào root và gravity luôn world-down. Zero-G chỉ đổi momentum qua contact/explicit force. Logical gravity là phương án nghiên cứu dự phòng, chưa triển khai song song để tránh hai truth sources.

## ADR 003 Physics không phải deterministic solver

Reset state rõ ràng và repeatability trong cùng cấu hình là yêu cầu hiện tại. Không hứa tính tái lập bit-for-bit across PhysX/platform. Dùng fixed timestep, primitive colliders, CCD cho ball có impulse, max depenetration speed; không tăng solver mặc định khi chưa đo.

## ADR 004 Giới hạn rotation và các chế độ snap

Assisted/free đều xoay liên tục, release chỉ snap nếu gần một trong 24 orientations. Chế độ QuarterTurn hiện snap đến canonical sau release, có chuyển động liên tục trong drag; là chế độ so sánh authoring, chưa phải gesture discrete chỉ cho phép một bước 90°. Cần product test nếu muốn semantics hard-snap nghiêm ngặt hơn.

## ADR 005 Reset registry theo màn

Không singleton global registry. Capture một lần sau bind, reset root trước ball; restore launch velocity của zero-G. Signal bus và IgnoreCollision đều được đưa vào lifecycle reset. Transition dùng deadline có thể hủy ngay, tránh coroutine tiếp tục sau reset.

## ADR 006 Tín hiệu cơ cấu và door

Plate publish channel, door subscribe qua bus instance. Exit có RequiredChannel cho puzzle sequencing. Door collision chuyển ngay theo signal, visual trượt dần; tradeoff prototype để không di chuyển collider con tương đối trong compound đang xoay. Product có thể chuyển door sang kinematic body riêng, cần kiểm tra ordering, reset và contact mới.

## ADR 007 Content baseline và authoring

16 prefab data-driven được tạo bởi editor builder để tái lập baseline và có thể mở Inspector chỉnh. Không encode level ID trong physics/input/runtime mechanism. Baseline dùng hành lang extrude theo depth để dễ đọc; vẫn là Rigidbody 3D tự do trong cube. Lối ra hiện dùng cửa xuyên vỏ hộp theo ADR 011; quyết định dùng trigger xuyên depth của baseline cũ đã bị thay thế theo yêu cầu người dùng.

## ADR 008 Build và sản phẩm

Desktop development build là đầu ra kiểm thử ngay trên máy hiện có. Android/iOS build được tự động hóa nếu toolchain sẵn. Chỉ ghi 'verified on device' sau khi cài/chạy thật. Save game, addressables, DI framework, backend và Phase 2 chưa có lý do đủ mạnh để triển khai trong baseline.

## ADR 009 Bóng bị che khuất

Một sphere overlay nhỏ dùng ZTest Greater để chỉ hiển thị silhouette khi geometry đặc che bóng. Đây là tín hiệu vị trí, không thay đổi collision hoặc velocity. Chi phí thêm một renderer/mesh sphere và shader unlit nhỏ; cần xác nhận readability trên màn hình thật. Shell vẫn hạn chế transparency; không làm toàn bộ cơ cấu trong suốt vì sẽ mất cảm nhận đường đi.

## ADR 010 Kiến trúc simulator

Build iOS Simulator chọn `PlayerSettings.iOS.simulatorSdkArchitecture = AppleMobileArchitectureSimulator.ARM64` cho máy Apple Silicon; Device SDK giữ setting ban đầu. Nếu để mặc định x86_64 rồi ép Xcode `-arch arm64`, UnityRuntime và baselib không link được. Tham chiếu: [Unity simulator architecture](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/AppleMobileArchitectureSimulator.html).

Trên simulator iOS 26.5 đã gặp lỗi Metal render attachment 1 sample so với yêu cầu MSAA 4. Export simulator tạm đặt URP MSAA = 1 (disabled), rồi phục hồi asset về 4 sau build. Device/macOS/Android vẫn dùng cấu hình baseline và cần profiling riêng; không dùng performance simulator làm số liệu thiết bị.

## ADR 011 Chỉ thắng khi bi thoát thật khỏi hộp

Điều kiện thắng và phần quan sát sau thắng vẫn áp dụng; hình học cửa vuông bên dưới đã được thay bằng cửa tròn phẳng theo ADR 012.

Yêu cầu cập nhật của người dùng ngày 08/09/2026 có ưu tiên so với quy tắc capture trong GDD gốc. Bỏ trigger xuyên depth và teleport đến CapturePoint. Mỗi màn có một cửa vuông 1,44 × 1,44 trên mặt đáy hoặc mặt phải, nằm ở depth -1,15 và gần đích cũ. Vỏ tại mặt đó gồm bốn collider quanh cửa; viền có collider riêng, lòng cửa hoàn toàn rỗng. Dùng cửa vuông để hình hiển thị khớp chính xác với primitive collider.

Local +Z của ExitSocket hướng ra ngoài; ApertureHalfSize và WallHalfDepth là hợp đồng giữa geometry với detector. Detector theo dõi đoạn chuyển động tương đối của tâm bi từ phía trong, qua tiết diện cửa, tới khi toàn bộ bán kính vượt mép ngoài thêm 0,02 m. Kiểm tra sweep tránh bỏ sót bước nhanh; chạm mép, đi từ ngoài vào, hoặc vượt mặt hộp ở vị trí khác đều không thắng. RequiredChannel còn khóa bằng shutter vật lý tái sử dụng SignalDoor.

Event Exited chỉ phát một lần. LevelManager khóa input xoay, giữ Rigidbody dynamic và vận tốc thực; TimeScale 0,35 trong 1,8 giây thực để quan sát thoát ra. Camera chỉ mở rộng sau khi thắng để giữ hộp và bi cùng khung. Chuyển màn dùng unscaled deadline; reset hủy deadline, trả tốc độ bình thường và khởi tạo lại mẫu vị trí sau khi reset ball/root. Chỉ ở màn cuối, sau khoảng quan sát, ball mới được giữ tại vị trí hiện tại để hiện màn tổng kết.

Nâng cấp dùng menu Upgrade Physical Exits để thay vỏ và cửa, giữ nguyên platforms, spawn, cơ cấu khác và tuning. Test khả giải phải ghi lại với điều kiện thắng mới, sau đó replay nghiêm ngặt ở lần kiểm tra bình thường.

## ADR 012 Lỗ tròn khoét phẳng, bỏ gờ cản đường lăn

Theo phản hồi tiếp theo của người dùng, bỏ hoàn toàn Port side/Port sill và viền khối. Mặt vỏ chứa lỗ được bake thành một mesh kín có tiết diện tròn 64 đoạn, bán kính 0,78 m và chiều dày đúng 0,18 m của vỏ. MeshCollider dùng chính mesh hiển thị, giữ non-convex để không lấp mất lỗ; root vẫn kinematic và ball là SphereCollider. Unity cho phép concave MeshCollider trên body kinematic khi va chạm với collider convex: [Unity Mesh colliders](https://docs.unity3d.com/6000.3/Documentation/Manual/mesh-colliders-introduction.html).

Mesh được tạo tại Editor và lưu thành asset, không dựng lại hoặc cook mỗi frame. Bổ sung góc của đường biên ngoài vào tập góc chia để mặt vuông không bị hở ở các góc khi lỗ lệch tâm. Năm mặt còn lại vẫn dùng BoxCollider. Cần profiling thiết bị thật khi chốt product; không suy diễn chi phí mesh từ simulator.

Viền là vành phẳng rộng 0,018 m nằm sát hai bề mặt vỏ, lệch 0,003 m chỉ để tránh z-fighting. Material unlit màu xanh dịu (0,33; 0,52; 0,39), không bloom, đèn, emission boost hay collider. Không có vành dạng ống hoặc phần nâng mặt sàn. Shutter các màn có switch dùng đĩa tròn chìm trong độ dày vỏ.

ExitSocket kiểm tra clearance theo bán kính tròn; WallHalfDepth giảm từ 0,30 xuống 0,09 để khớp mép thật của mặt cắt. Chưa thắng cho đến khi toàn bộ bi ra ngoài. Bổ sung regression test bi lăn chậm từ mặt sàn bên cạnh lỗ chỉ với gravity và vận tốc tiếp tuyến 1,2 m/s; test phải qua mà không thêm lực nâng. Test riêng loại vùng góc từng hợp lệ của cửa vuông.

## ADR 013 Vật thể tự do và nắp rơi bằng trọng lực

Phản hồi của người dùng thay thế cơ chế công tắc ở L06. Xem [Triết lý thiết kế vật lý](PHYSICS_DESIGN_PRINCIPLES.md). Màn này bỏ Plate/SignalDoor/RequiredChannel; dùng đĩa tròn rời tựa phía trong. Nắp rời lỗ vì lực và contact, không kiểm tra orientation để quyết định mở. Đĩa nắp là một convex MeshCollider trên body tự do; sàn có lỗ vẫn là mesh non-convex trên root kinematic.

EnvironmentForceSystem chuyển từ một target sang danh sách đăng ký theo scope màn. IPhysicsAffectable chỉ yêu cầu Rigidbody, không buộc props phụ thuộc BallPhysicsProfile. Ball giữ profile riêng; prop dùng khối lượng/shape/material trong prefab và cùng EnvironmentProfile để nhận gia tốc. PhysicalProp chỉ quản lý world-space initialization/reset, không có Update/FixedUpdate điều khiển chuyển động.

LevelRuntime giữ danh sách Props trước khi tách khỏi LevelRoot, đăng ký force/reset một lần. Reset root trước các body; ReleaseProps vô hiệu hóa object và hủy đăng ký trước Destroy để load liên tiếp không tạo va chạm/lực ma. Việc tách khỏi hierarchy là thao tác khởi tạo, không phải thời điểm nắp rơi. Nắp luôn dynamic, không có trạng thái unlock hoặc animation mở.

Các cơ cấu tín hiệu ở màn cũ khác chưa được chuyển đổi trong thay đổi L06; được đánh dấu trong kế hoạch chuyển nội dung. Không tuyên bố toàn bộ 16 màn đã tuân thủ triết lý mới.
