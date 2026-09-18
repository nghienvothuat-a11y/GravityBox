# COghe — Quy trình thiết kế level và quy tắc xây dựng

Ngày lập: **17/09/2026**. Áp dụng cho campaign **COghe / Venom Origin 01–20 và các màn tiếp theo**.
Lưu theo yêu cầu người dùng sau quá trình thiết kế, chơi thử và tối ưu 20 màn.

Đây là quy chuẩn cho công việc tiếp theo, không phải chứng nhận mọi màn hiện tại đã đạt.
Các bộ Steel Ball Lab và thí nghiệm điều khiển Journey/Venom cũ là nội dung lịch sử;
không áp các luật của chúng vào Origin hoặc thay đổi chúng ngoài phạm vi được giao.

- Bắt đầu một màn mới bằng [mẫu hồ sơ level](LevelDesign/COghe/LEVEL_TEMPLATE.md).
- Ngoại hình tuân theo [Day Lab STYLE_RULES](ArtDirection/COghe/STYLE_RULES.md).
- Xem [kiến trúc cơ quan 11–20](COGHE_EXPANSION_11_20.md),
  [camera](COGHE_CAMERA.md) và [phản hồi điều khiển](COGHE_CONTROL_FEEDBACK.md).
- Yêu cầu mới nhất của người dùng và thiết kế riêng đã chốt được ưu tiên. Ghi lại
  thay đổi trong hồ sơ màn; không coi đề xuất cũ là tính năng đã triển khai.
- Phần nào đã được người dùng xác nhận thì tiếp tục thực hiện trong phạm vi đó.
  Các bước kiểm tra dưới đây không tạo thêm yêu cầu xin phép cho công việc đã được giao.

## 1. Luật sản phẩm phải giữ

1. **Giải đố thuần, cảm giác chill.** Không phân điểm chỉ số, không dùng đồ mua ở
   Nhà để tăng lực, tốc độ, sức bám hoặc mở khả năng giải màn. Khối lượng phân chia
   trong một màn là tài nguyên vật lý của puzzle, không phải nâng cấp lâu dài.
2. **Người chơi hướng dẫn sinh vật.** Chạm để giao đích/tương tác; sinh vật thực hiện
   tác vụ hợp lệ, không tự tìm và thực hiện toàn bộ lời giải. Nhiệm vụ của phần chưa
   được chọn vẫn tiếp tục theo luật tác vụ, không tự bỏ nút để chạy theo phần khác.
3. **Chọn mặt và bám mặt là hai việc riêng.** Nóc và mặt trơn vẫn nhận chạm khi
   chúng là đích hợp lệ trong chế độ hiện tại. Mặt trơn không tạo lực bò/bám chủ động;
   trọng lực, quán tính và tiếp xúc vẫn tồn tại. Trong chế độ chỉ đi trong ống như
   màn 15, điểm chạm trên vỏ hộp không được thay lệnh đi trong ống.
   Ngoại lệ theo yêu cầu 18/09/2026: màn hiển thị 24 (content 17) tắt chọn
   nắp trên để dễ chạm tay nắm bánh răng; nắp vẫn va chạm và bám được.
4. **Chỉ tách qua cơ quan cắt.** Dao cắt theo vị trí mô thực, không ép hai phần bằng
   nhau. Xung lực tách giúp các phần rời nhau; không đổi khối lượng hoặc sinh thêm mô.
5. **Đủ gần và không bị vật cản ngăn thì tự hợp thể.** Áp dụng cả khi đứng yên, đi
   khác đích hoặc giữ nút. Không cooldown sau chém, không bảo vệ nhiệm vụ để ngăn tụ.
   Khi nhập, giữ lệnh còn hiệu lực mới nhất theo hành vi hiện có; không nối xuyên vách.
6. **Hợp thể bên trong trước khi thoát.** Một phần ra lỗ cuối trước khi nhập hết thì
   thua, hiện `bạn phải hợp thể trước khi chui ra`. Chỉ thắng khi toàn bộ vật chất của
   bản thể đã đi qua lỗ cuối; hiện tại là 32 hạt. Ống chuyển khoang không phải cửa thắng.
7. **Xoay và camera được khai báo riêng.** Đổi góc nhìn không đổi trọng lực hoặc tâm
   xoay vật lý. Màn khóa xoay vẫn cần các đích chạm tiếp cận được từ camera cho phép.
8. **Kỹ năng mở dần, độ khó tăng mượt.** Chuỗi màn giới thiệu → luyện tập → kết hợp
   → vận dụng khác ngữ cảnh. Mỗi mốc 10 màn là Boss; Boss không có hướng dẫn lời giải.
   Phản hồi chạm, chọn phần, trạng thái cơ quan vẫn phải rõ. Boss 10 mở Collection
   “Nhà của sinh vật”; không tự đặt thêm phần thưởng vĩnh viễn cho Boss mới.

Một số đoạn trong `VENOM_CREATURE_SKILLS.md`, `VENOM_PURE_PUZZLE_AND_HOME.md` và
tài liệu Journey còn mô tả bảo vệ nhiệm vụ hoặc thời gian chờ hợp thể. Với Origin,
luật 5–6 ở trên ghi lại quyết định mới hơn của người dùng và thay thế các đoạn đó.
Copy vật thể vẫn là hướng phát triển chưa có trong prototype; nếu màn mới cần nó,
phải triển khai và kiểm chứng kỹ năng trước, không giả định runtime đã hỗ trợ.

## 2. Quy trình chín bước

### Bước 1 — Mục tiêu trải nghiệm

- Ghi người chơi sẽ học/khám phá điều gì, kỹ năng đã biết, kỹ năng mới và khoảnh khắc
  nhận ra cách giải. Màn giới thiệu tập trung một kiến thức mới; tăng phối hợp sau.
- Xác định vị trí trong chương, màn thường/Boss, trạng thái kỹ năng thực sự có trong
  runtime và mức hướng dẫn. Không đặt một điều kiện ẩn mà người chơi chưa thể hiểu.
- Đầu ra: brief ngắn với mục tiêu và lý do màn cần tồn tại.

### Bước 2 — Phác thảo và diễn giải

- Đánh dấu spawn, lỗ cuối, ống chuyển, vật liệu bám/trơn, vật cố định/di động, các
  khoang, chiều chuyển động cơ quan, quyền xoay, điểm chạm và góc camera ban đầu.
- Màn nhiều khoang cần sơ đồ quan hệ nút/cần/cửa. Ống giao nhau phải thể hiện rõ
  có nối hay chỉ đi ngang nhau; không dùng hình trang trí để gợi một đường không có thật.
- Kèm đường giải dự kiến và góc phụ hoặc mặt cắt khi hình phối cảnh gây hiểu nhầm.
- Diễn giải lại bản vẽ để đối chiếu ý đồ; dùng xác nhận đã có. Chỉ hỏi những điểm
  chưa rõ có ảnh hưởng tới luật/bố cục, không hỏi lại toàn bộ phương án đã được duyệt.
- Đầu ra: phác thảo có phiên bản, các quyết định đã chốt và phần còn mở.

### Bước 3 — Lời giải, trạng thái và sửa sai

- Viết chuỗi hành động và điều kiện vật lý: ai tác động, cơ quan chuyển thế nào,
  phải giữ liên tục hay chốt lại, khi nào đường mở, điều kiện hoàn tất.
- Liệt kê cách giải thay thế hợp lý. Đánh giá đường tắt theo mục tiêu trải nghiệm;
  không vô hiệu hóa một hệ quả vật lý chỉ để buộc người chơi đi đúng kịch bản tác giả.
- Thử trên giấy: làm ngược thứ tự, cắt lệch, tụ quá sớm, kéo vật vào góc, buông cần,
  đi vào ngõ cụt, trượt khỏi đường, mở rồi đóng cửa, đổi phần trong lúc thao tác.
- Mỗi tình huống có đường phục hồi hoặc thất bại có chủ đích với phản hồi rõ.
  Mắc kẹt không lối sửa và không thông báo là lỗi thiết kế; Retry luôn phải hoạt động.
- Đầu ra: lời giải mẫu, quan hệ trạng thái, đường phục hồi và trường hợp biên.
  Lời giải dành cho tác giả/test; runtime và HUD Boss không đọc nó.

### Bước 4 — Độ khó và ngân sách hiệu năng

- Đánh giá riêng: số quyết định cần suy luận, độ dài chuỗi phụ thuộc, số vai trò
  đồng thời, yêu cầu căn thời điểm/độ chính xác, mức quan sát và chi phí thử lại.
- Ước lượng riêng: bề mặt tìm đường, cơ quan động/khớp, phần cơ thể hoạt động,
  tần suất hình học thay đổi, lớp kính chồng, bóng đổ và công việc khi nhận lệnh.
- So với màn trước/sau và một màn tham chiếu đã đo. Không gộp mọi trục thành một
  điểm “độ khó khoa học” chưa được kiểm chứng, không suy ra chi phí chỉ từ số object.
- Chọn thiết bị, cấu hình, mục tiêu khung hình và tình huống nặng để đo. Các con số
  renderer/collider/khớp tối đa phải có căn cứ đo; chưa có thì ghi chưa xác định.
- Đầu ra: mục tiêu thử nghiệm và rủi ro ưu tiên. Số liệu độ khó cần hiệu chỉnh qua
  người chơi chưa biết lời giải; đồ họa nhỏ/khó chạm không phải độ khó puzzle mong muốn.

### Bước 5 — Dựng hình khối đơn giản, chơi trọn màn

- Dựng collider, mặt bám/trơn, cơ quan, đường đi và camera bằng bộ hình đơn giản.
  Dùng đơn vị mét và kích thước phù hợp mô sinh vật; đo clearance tại mép và cửa.
- Chơi toàn bộ bằng API/lệnh điều khiển mà người chơi có, chờ mô phỏng thật. Kiểm
  tra thêm chạm màn hình; không dịch chuyển mô, gán cửa mở hoặc ép thắng để chứng minh khả giải.
- Thử đi ngược, quay đầu, rời vật, tiếp cận từ mặt khác và phần nhỏ sau cắt.
- Đầu ra: bản chơi được có đường giải hoàn chỉnh, input rõ và kết quả phục hồi.
  Chưa đầu tư art chi tiết khi vẫn phải sửa bố cục để giải được.

### Bước 6 — Tích hợp vào bộ cơ quan dùng chung

- Cấu hình bố cục/camera/quyền xoay/liên kết qua dữ liệu level và component có sẵn.
  Cơ chế mới có hợp đồng trạng thái, lực, reset, hủy lệnh và khả năng ảnh hưởng đường đi.
- Không thêm luật vật lý hoặc luật thắng dựa vào số màn. Tách logic mới thành phần
  tái sử dụng khi cần; không bắt buộc viết lại toàn bộ kiến trúc để thêm một màn.
- Đăng ký ID ổn định trong catalog, scene/build và bộ chọn; giữ save cũ. Builder
  dựng lại phải giữ cấu hình đã chốt, tài nguyên sinh ra nằm riêng theo màn.
- Đầu ra: scene/cấu hình tái tạo được, cơ quan có phạm vi rõ và test tương ứng.

### Bước 7 — Art, animation và phản hồi

- Áp Day Lab sau khi phần chơi ổn. Màu/chất liệu thể hiện chức năng, khung không che
  đích chạm; màu không phải tín hiệu duy nhất cho vùng trơn và trạng thái cơ quan.
- Với mỗi tương tác, thể hiện nhận lệnh → tiếp cận/bám → tác động → kết quả/hủy/thất bại.
  Xúc tu, thân, đèn và âm thanh đọc trạng thái thật; không mở cửa theo cuối animation.
- Kiểm tra hình thực trong Unity ở portrait, safe area, góc xoay, cận cảnh, từng
  khoang và ăn mừng. Art không thêm collider hoặc thay đổi đường giải đã chốt.
- Đầu ra: ảnh/render thật, bảng phản hồi theo hành động, không chỉ concept đẹp.

### Bước 8 — Chơi thử và đo trên thiết bị

- Chạy test đường giải, các ca biên và hồi quy hệ dùng chung; xem mục 4. Chơi tay
  kiểm tra điểm chạm, chuyển camera, độ hiểu cơ quan và cảm giác sinh vật.
- Với người chưa biết lời giải, ghi thời gian, điểm do dự/chạm sai, lần thử, nhu cầu
  gợi ý và cách giải họ tìm ra. Ghi số người/lượt; test tác giả không thay cho bằng chứng này.
- Đo baseline và bản thay đổi với cùng thiết bị/kịch bản/chất lượng. Bao gồm frame
  khi nhận lệnh và cơ quan đổi trạng thái, không chỉ đứng yên. Kiểm tra lượt chơi dài
  khoảng 15–20 phút trên máy mục tiêu để quan sát nóng máy; ghi thời lượng thực tế.
- Đầu ra: kết quả test/playtest/performance, lỗi còn lại và chỉnh sửa có lý do.

### Bước 9 — Nghiệm thu và xếp vào tiến trình

- Chốt vị trí dựa trên mức học và chơi thử, tránh nhiều bước khó mới xuất hiện cùng
  lúc. Boss có thử thách phối hợp và khoảnh khắc chiến thắng; màn sau có nhịp nghỉ.
- Cập nhật hồ sơ, README/catalog và bằng chứng theo đúng bản build. Chỉ ghi đạt khi
  có bằng chứng tương ứng; gameplay đạt không tự chứng minh mobile performance đạt.
- Trạng thái hồ sơ: **Nháp → Thiết kế đã chốt → Prototype → Đang kiểm chứng → Đạt**.
  Đánh dấu riêng gameplay, input/art, hiệu năng và lỗi còn tồn tại. Có thể bàn giao
  prototype chưa đạt với giới hạn rõ; không đổi nhãn thành “Đạt” để hoàn tất báo cáo.

## 3. Quy tắc code và chi phí chạy

### 3.1 Phân tách trách nhiệm

- Simulation sở hữu mô, khối lượng, lực, va chạm, cắt/tụ; cơ quan sở hữu chuyển động
  và trạng thái thật; navigation tìm đường thực thi lệnh; presentation đọc để hiển thị.
- Mesh trang trí không có collider, không đăng ký mặt dẫn đường, không chặn input.
  Collider đơn giản phải đủ đúng cho khe, ống và cạnh chức năng; không lấp lỗ bằng hộp bao.
- Chỉ cấp Rigidbody/joint cho vật cần mô phỏng. Cơ quan ngoài camera vẫn chạy nếu
  giữ tải, truyền lực hoặc điều kiện puzzle; không tắt collider/logic theo visibility.

### 3.2 Giới hạn phạm vi truy vấn

- Dùng snapshot transform/bounds/collider chung trong một lượt xử lý; lọc không gian
  theo vùng từng phần cơ thể trước khi kiểm tra chính xác. Không quét toàn scene ở
  từng đỉnh mesh, tia xúc tu, hạt hoặc cặp nút tìm đường.
- Bounds chỉ là bộ lọc bảo thủ: chấp nhận dư ứng viên, không bỏ sót collider mỏng,
  mặt đồng phẳng, vành bám và lỗ. Collider/mặt thật vẫn quyết định tiếp xúc/đường trống.
- Phân nhóm khả năng cơ quan sau initialization (`HasSkinConstraint`, `TransportsTissue`,
  `SeparatesTissue`, `ControlsExit`). Khả năng thay đổi hoặc thêm/bớt cơ quan runtime
  phải cập nhật nhóm; trạng thái active vẫn được kiểm tra khi dùng.

### 3.3 Cache đúng vòng đời

- Reference/config tĩnh có thể cache theo scene; pose/bounds cơ quan động chỉ dùng
  trong lượt snapshot thích hợp. Không giữ bounds cũ qua chuyển động rồi cho mô xuyên vật.
- Dùng lại graph khi hình học/trạng thái lỗ chưa đổi. Tọa độ graph theo Root giúp
  việc xoay toàn hộp không tự tạo công việc vô ích; cơ quan dịch tương đối vẫn phải invalidate.
- Khi nhận lệnh sau khi xoay, điểm đến và truy vấn độ bám phải dùng tọa độ world
  được chuyển từ graph local bằng pose Root hiện tại. Không dùng world snapshot
  lúc dựng graph để chọn điểm bám; chỉ cần cập nhật tọa độ, không dựng lại các cạnh.
- Đổi hình học, aperture, kích thước/enable mặt hoặc di chuyển vật phải được phát hiện.
  Thêm/xóa collider hay thay mesh collider runtime phải vô hiệu cache ngay; hiện dùng
  `Motion.BuildGraph(true)` cho trường hợp không được revision tự theo dõi.
- Code hiện có vẫn kiểm tra revision/polling; không mô tả là hoàn toàn theo sự kiện
  hoặc không làm việc mỗi frame. Mọi tối ưu tiếp theo phải kiểm tra với graph dựng mới.

### 3.4 Thuật toán và cấp phát

- Ưu tiên giảm công việc lặp trước khi giảm chất lượng: spatial grid cho cặp gần,
  cây bounds cho truy vấn, hàng đợi ưu tiên cho đường đi. Giữ thứ tự xử lý đường đồng
  chi phí để kết quả không đổi theo thứ tự hash hoặc bản build.
- Tái sử dụng mesh, arrays/lists và bộ đệm tạm; tránh `Find*`, LINQ/cấp phát và tạo
  material trong vòng lặp nóng. Đo allocation thực tế, không tuyên bố toàn game zero-GC.
- Công việc có đỉnh lớn khi chạm/mở cửa cũng cần ngân sách. Chỉ cập nhật graph cục bộ
  hoặc chia việc qua nhiều frame khi đã xác định cần; không dùng route cũ xuyên cửa
  trong lúc chờ. Hai hướng này là đề xuất tiếp theo, chưa mặc định đã triển khai.

### 3.5 Vật lý, xoay và animation

- Giữ trọng lực thế giới, tỷ lệ và lực theo khối lượng. Không thay luật theo máy,
  không giảm physics tick hoặc hạt chỉ để nâng FPS mà chưa kiểm chứng tác động gameplay.
  Baseline hiện tại là 32 hạt và 120 Hz; không coi đó là tuyên bố solver chất lỏng đầy đủ.
- Cơ quan động không đồng thời bị kéo bởi parent Transform xoay và Rigidbody tự mô
  phỏng. Theo builder hiện có: đặt theo local frame rồi tách vào Apparatus, joint
  nối rigidbody pivot; đọc trục/vị trí/giới hạn trong hệ quy chiếu đúng.
- Chọn CCD và solver phù hợp từng cơ cấu; đo sai lệch neo/ray khi xoay, lật, chịu tải.
  Không tăng solver toàn scene hoặc ép pose mỗi frame để che cấu trúc khớp sai.
- Skin/animation không tạo lực hoặc thay topology để đẹp hơn. Hiện skin dựng mỗi
  frame; tối ưu bộ đệm/truy vấn, không tự giảm nhịp khiến mô hiển thị trễ va chạm.
- Chi phí animation phụ thuộc việc đang chạy, đỉnh mô và truy vấn tiếp xúc. Không
  chạy toàn bộ thư viện cùng lúc. Hủy tác vụ/reset/pause không để callback animation
  cũ bật cơ quan hoặc giữ lực; tài nguyên động cần giải phóng đúng vòng đời scene.
- Với tay nắm trên ray, vị trí chân bám và điểm tay nắm phải được tính riêng.
  Chân cần mặt bám cố định song song hành trình, đủ khoảng trống cho thân; không
  kéo toàn thân ra trước tay nắm rồi dựa vào lực vô hạn để giữ nó trên không.
  Kiểm tra một lượt đẩy/kéo liên tục, đổi chiều và buông; test tự bám lại nhiều lần
  có thể chứng minh khả giải nhưng không chứng minh thao tác ổn định.

### 3.6 Dựng hình và camera

- Dùng shared material/cubemap và property block thích hợp. Batch theo material và
  cùng chuyển động; không gộp nắp động vào vỏ cố định. Tài nguyên sinh ra riêng từng scene.
- Theo ngân sách Day Lab về đèn/bóng; tránh thêm nhiều lớp kính chồng, reflection
  realtime và hậu kỳ nặng. Đo GPU trước khi kết luận đồ họa là nguyên nhân lag.
- Camera lấy bounds tĩnh lúc vào màn, tính góc cần thiết mỗi frame; không quét toàn
  bộ renderer/collider để fit. Vật di động không làm toàn cảnh liên tục phóng/thu.
- Kiểm tra toàn cảnh/follow/khoang và tỷ lệ dọc dài. Nền phải phủ viewport ở mọi góc,
  gần/xa clip và ray chạm phải phù hợp; camera không đổi vị trí Rigidbody để căn hình.

## 4. Kiểm chứng và hồ sơ hiệu năng

### Ma trận kiểm tra tối thiểu theo phạm vi thay đổi

| Phạm vi | Bằng chứng cần có |
| --- | --- |
| Mỗi màn mới | Tải/reset, input thật, giải trọn bằng lệnh hợp lệ, luật thắng/thua, phục hồi các lỗi dự kiến |
| Mặt/khe/ống | Hai chiều, quay đầu, mép chuyển sàn–tường–trần, vành lỗ, phần nhỏ/lớn, ngõ cụt |
| Cơ quan động | Chạm/đổi lệnh/buông, tải thật, vật kẹt góc, giới hạn hành trình; rotation stress nếu cho xoay |
| Cắt/tụ/phối hợp | Cắt lệch, nhiều phần, giữ nhiệm vụ khi đổi chọn, tụ gần, vật cản ngăn tụ, tụ rồi thoát |
| Cache/thuật toán | Kết quả trước/sau tương đương, cache vs dựng mới, invalidation khi cửa/lỗ/prop thay đổi |
| Camera/art | Portrait 720×1280 và 720×1612 làm mốc, safe area, toàn cảnh/follow/khoang, xoay, ăn mừng |
| Thay runtime dùng chung | Hồi quy toàn Origin hiện hành và các test mới; lỗi cũ phải được báo riêng |
| Save/catalog | ID không đổi, tải/chọn màn, pause/retry, mở Collection hợp lệ, tiến trình được giữ |

Fixture đơn vị có thể sắp đặt trạng thái để thử một luật và phải ghi rõ. Test giải
trọn màn không teleport, gán topology, ép cửa mở hoặc sửa thắng. Script lời giải qua
API không thay thế kiểm tra chạm pixel và cảm giác chơi tay trên điện thoại.

Mỗi phép đo cần: ngày, device/OS, độ phân giải, chất lượng, build type, commit và
diff nếu working tree chưa commit, SHA APK/binary, kịch bản/thời lượng, điều kiện
sạc/nhiệt nếu biết, đường dẫn dữ liệu và lỗi đang có. Baseline/after phải so tương đương.

- Ghi FPS thực, p95/p99 thời gian frame, frame chậm nhất, số frame trên 33,3/50 ms,
  độ trễ nhận lệnh, chi phí skin/gameplay/navigation; thêm GPU/GC/memory khi thu được.
- 60 FPS tương ứng khoảng 16,7 ms/frame, 30 FPS khoảng 33,3 ms/frame. **60 FPS là mục
  tiêu, chưa phải kết quả đã đạt ổn định trên OPPO.** Profile 30 FPS cho máy yếu chỉ
  là lựa chọn cần đo và ghi trong cấu hình sản phẩm; không tự thay vật lý theo profile.
- Đặt tiêu chí p95/p99, đỉnh khựng và memory cho từng nhóm thiết bị trong hồ sơ trước
  khi nghiệm thu. Chưa đặt hoặc chưa đo thì ghi chưa kiểm chứng, không tự coi là đạt.
- Không cộng thời gian các vùng CPU lồng nhau. Mac/Editor không chứng minh Android
  mượt; phép đo đứng/bò vài giây không chứng minh đã chơi giải trọn hoặc ổn định khi nóng.
- Benchmark/auto-play chỉ tồn tại hoặc bật trong bản đo theo cơ chế build hiện có;
  bản chơi không được tự chạy đo, không ghi đè tiến trình người dùng.

### Các mốc đã đo để tham khảo

| Mốc lịch sử | Kết quả | Giới hạn |
| --- | --- | --- |
| OPPO 11–20, 17/09 | Màn 13 bò 8,2 → 45,9 FPS; Boss 20 bò 4,5 → 44,9 FPS | Trước đợt camera mới; lượt mẫu ngắn; vẫn còn frame khựng |
| Mac Boss 20, 17/09 | Graph cao nhất/frame 913,52 → 55,47 ms; cả hai lượt giải thắng | Không thay số đo điện thoại; 55 ms vẫn có thể gây khựng |

Nguồn: [OPPO và dữ liệu gốc](COGHE_MOBILE_PERFORMANCE.md),
[lời giải/đo Boss 20 Mac](COGHE_BOSS20_PLAYTEST.md).
Đây là bài học kỹ thuật, không phải ngân sách đạt sẵn cho level mới hoặc build mới.

## 5. Cách lưu và bàn giao

1. Sao chép [LEVEL_TEMPLATE](LevelDesign/COghe/LEVEL_TEMPLATE.md) thành
   `Docs/LevelDesign/COghe/LevelNN/README.md` cho màn mới; không ghi đè hồ sơ đã có.
   Cập nhật liên kết tương đối theo vị trí hồ sơ mới. Lưu phác thảo, phiên bản chỉnh
   và bằng chứng cùng thư mục hoặc liên kết tới Verification.
2. Ghi trạng thái thật và lỗi còn lại. Không đánh dấu checklist bằng kế hoạch dự định.
   Nếu quy tắc sản phẩm đổi, cập nhật nguồn chung và hồ sơ bị ảnh hưởng, giữ lịch sử rõ ràng.
3. Test hiện hành: `bash Tools/verify-coghe-expansion.sh`. Khi thêm suite mới, đăng ký
   vào script; không dùng tên 11–20 để bỏ qua màn mới. Không chạy `--generate` chỉ để
   kiểm tra tài liệu: tùy chọn đó dựng lại scene và phải đúng phạm vi công việc.
4. Build Mac cho thay đổi playable: `bash Tools/build-venom.sh`. Build Android khi
   được yêu cầu trong nhiệm vụ: `bash Tools/build-venom-android.sh`; bản đo dùng
   `COGHE_BENCHMARK=1` theo [hướng dẫn đo](COGHE_MOBILE_PERFORMANCE.md).
   Cập nhật catalog/kịch bản benchmark khi mở rộng số màn; hiện các lượt mẫu tập trung 11–20.
5. Bàn giao nêu thay đổi, đường giải cho người test nếu cần, test và số đo thực tế,
   đường dẫn build, trạng thái cài thiết bị và giới hạn. Build xong không đồng nghĩa
   đã cài; APK/Mac và kết quả đo phải nhận diện được đúng phiên bản.

Thay đổi chỉ tài liệu như lần lập quy chuẩn này chỉ cần rà nội dung/liên kết/diff;
không cần dựng scene, chạy lại Unity hoặc tạo APK để chứng nhận văn bản.
