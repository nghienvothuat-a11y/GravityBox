# Phạm vi kiểm chứng prototype 14 bàn

Bản hiện tại có hỗ trợ thoát trong 40 mm quanh cửa cho **cả 14 màn**. Đường puzzle vẫn do người chơi xoay hộp; motor lực xử lý đoạn cuối qua lỗ thật. 100/100 kiểm tra gồm 42 tình huống ở miệng lỗ, ba môi trường, vách/cửa chặn và bi thủy ngân đứng yên tại cửa hướng lên. [Kiểm chứng hiện tại](Verification/ExitAssist/README.md).

Bàn 14 tiếp tục đo nổi về nắp, cản theo nghiệm, reset/đổi môi trường và trạng thái nổi cân bằng khi tắt hỗ trợ. Đường giải phải nghiêng thêm để tự vượt vành là bằng chứng của bản đầu trước khi thêm motor. [Thiết kế](LEVEL14_MERCURY.md), [bằng chứng lịch sử](Verification/Mercury14/README.md).

Bàn 13 thêm phép thử lực nổi, lực cản với nghiệm tham chiếu, nghỉ ổn định dưới nước, chuyển sang không khí ở lỗ thoát, đường giải quanh cube bằng xoay và lifecycle/VFX. Xem [thiết kế nước](LEVEL13_WATER.md) và [kết quả bộ test hiện tại](Verification/Water13/README.md).

Mục tiêu hiện tại là chuyển động và va chạm của bi thép. Không yêu cầu tạo lại hoặc replay lời giải 16 màn cũ. Các route trong `VERIFIED_ROUTES.json` và [bảng lời giải cũ](Archive/SOLVABILITY_16_LEVELS.md) chỉ là bằng chứng lịch sử của scale và catalog trước đó.

## Kiểm tra tự động

- Catalog: tám hình Circle/Square/Triangle/LShape/UShape/Annulus/Dumbbell/Star, GravityLock ở bàn 09, MechanicalMaze ở 10, LayeredMaze ở 11 và SphereMaze ở 12; Earth gravity, free rotation, cùng profile bi thép. Bàn 09 có một PhysicalProp, bàn 10 có hai; không có cơ cấu mở bằng tín hiệu.
- Mô phỏng độc lập: freefall 9,81 m/s², quán tính cầu, gia tốc lăn trên dốc, contact và mất năng lượng, rebound, spin và rolling resistance.
- Prefab thật: spawn clearance, thành hộp theo từng cạnh contour gồm cả thành trong vành khuyên, sàn/nắp hỗ trợ đúng vùng có thể chơi và để trống phần khuyết/lõi rỗng; contact với cube, nằm yên, containment khi nghiêng và lỗ thoát thật. Scene integration kiểm tra cả 14 bàn. Phép kiểm tra vỏ dùng chiều sâu của từng prefab, gồm hộp 0,27 m ở bàn 11.
- Lối đi mới: sphere sweep theo toàn bộ bán kính 0,015 m qua góc chữ L/U, nửa vòng vành khuyên, cổ quả tạ và vùng giữa ngôi sao; sau đó đưa bi qua các waypoint bằng gravity và rotation intent.
- Exit: hướng đi từ trong ra ngoài, clearance tròn, toàn bộ bán kính vượt vỏ, một lần phát event, reset xóa passage cũ. Không teleport hoặc capture lúc thoát.
- Puzzle 09: thanh chặn kín toàn bộ chiều sâu cửa, body/joint không drive hoặc signal, trượt mở/đóng do gravity; fixture từ spawn chỉ xoay hộp để giữ bi, mở cửa, đi sang khoang phải và thoát thật.
- Mê cung 10: hai trục ray đối nhau; nghiêng một chiều mở A và giữ B đóng, nghiêng ngược đảo trạng thái. Cả hai cửa chặn toàn bộ cầu ở mọi chiều sâu hợp lệ. Route từ spawn đi qua hành lang, hai hốc/cửa và lỗ thoát chỉ bằng rotation intent; không phát signal. Reset khôi phục riêng từng body/anchor và unload thu hồi cả hai targets.
- Mê cung 11: ba sàn thật đỡ bi ngoài lỗ, sphere cast và body thật đi qua hai lỗ chuyển tầng cùng lỗ cuối. Route từ spawn đi qua waypoint trên cả ba tầng, kiểm tra tính liên tục của vị trí từng bước để phát hiện teleport; chỉ exit cuối phát thắng. Chế độ xem theo tầng/tổng thể chỉ đổi vật liệu, giữ pose/vận tốc, body dynamic và mọi collider.
- Mê cung cầu 12: các ván mỏng ghép thành tuyến ba chiều có nhánh cụt; khe nhìn nhỏ hơn viên bi để chặn thoát ra ngoài mạng. Kiểm tra collider thật ở các đoạn nối/ngã rẽ/cap và đường đi đủ 25 đoạn qua 21 khúc đổi hướng từ spawn tới lỗ thật chỉ bằng rotation intent.
- Lifecycle/input: mỗi body được áp gravity một lần (hai targets ở bàn 09, ba ở bàn 10), 100 reset mỗi bàn gồm cả root/prop, cleanup, manual next, pause, mouse/touch và tốc độ mô phỏng không đổi sau thoát. Bộ chọn tới được bàn 12 và quay lại bàn 11; nút xem tầng không tạo rotation input.

Test exit có thể đặt bi vào vị trí thử và cấp vận tốc khởi đầu để cô lập detector/collider. Đây là fixture kiểm tra một hợp đồng vật lý, không phải chứng minh người chơi đã giải bàn từ spawn. Test dốc/va chạm cũng dùng điều kiện ban đầu kiểm soát được; player không có các thao tác thử nghiệm này.

Fixture lối đi đặt một trạng thái khởi đầu, sau đó chỉ nghiêng hộp bằng controller chuẩn; không di chuyển bi giữa các waypoint và không thêm lực điều khiển riêng. Bàn 09–12 có fixture route từ spawn mặc định tới thoát thật; riêng bàn 11 phải qua đủ ba tầng; bàn 12 phải đi qua các đoạn nối vật lý tới cửa, với các mặt đỡ theo nhiều hướng. Đây là bằng chứng có đường vật lý hợp lệ, không phải lời giải bắt buộc trong player hoặc cam kết về thời gian giải bằng touch. Các fixture cô lập sàn/lỗ/tầng và reset vẫn có quyền đặt trạng thái để kiểm tra đúng hợp đồng của chúng.

Policy kiểm thử dùng điều khiển vị trí/vận tốc bảo thủ (`P=8`, `D=5`, gia tốc tiếp tuyến mong muốn tối đa 1 m/s²), chuyển thành góc nghiêng và đi qua giới hạn gia tốc/tốc độ của controller hộp. Bản policy đầu quá mạnh khiến hộp đảo góc muộn và bi dao động quanh waypoint; đã chỉnh riêng policy kiểm thử. Geometry, mô hình bi và tiêu chí clearance/đến waypoint không thay đổi vì lỗi điều khiển này.

Policy thử mê cung cầu gửi ý định xoay để gravity tương đối đưa bi dọc từng đoạn, đổi mặt đỡ ở ngã rẽ và đi qua cửa cuối. Không viết pose/vận tốc, không căn điểm đáp bằng lực riêng. Kiểm chứng hình học độc lập tìm đường trong không gian dành cho tâm toàn bộ viên bi, để kiểm tra các khe nhìn không tạo đường tắt. Bằng chứng và giới hạn của phiên bản cụ thể nằm trong XML/báo cáo hiện tại.

## Giới hạn của bằng chứng

120 Hz và PhysX không hứa tái lập bit-for-bit trên mọi nền tảng. Dung sai nhỏ được đặt theo scale; contact có thể khác về thời điểm giữa các lượt chạy. Geometry/tốc độ đầu vào vẫn phải bảo toàn containment và không sinh năng lượng bất thường.

Tests passing không xác nhận cảm giác đã thuyết phục. Buổi thử trực tiếp cần kiểm tra tăng tốc, đảo nghiêng, va chạm cube, góc tam giác và âm thanh lăn/va chạm theo [kế hoạch](IMPLEMENTATION_PLAN.md). Kết quả thực tế và platform đã chạy được ghi riêng trong [nhật ký](DEVELOPMENT_LOG.md).
