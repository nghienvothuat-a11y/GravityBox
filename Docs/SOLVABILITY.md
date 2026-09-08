# Phạm vi kiểm chứng prototype chín bàn

Mục tiêu hiện tại là chuyển động và va chạm của bi thép. Không yêu cầu tạo lại hoặc replay lời giải 16 màn cũ. Các route trong `VERIFIED_ROUTES.json` và [bảng lời giải cũ](Archive/SOLVABILITY_16_LEVELS.md) chỉ là bằng chứng lịch sử của scale và catalog trước đó.

## Kiểm tra tự động

- Catalog: tám hình Circle/Square/Triangle/LShape/UShape/Annulus/Dumbbell/Star và GravityLock ở bàn 09, Earth gravity, free rotation, cùng profile bi thép. Chỉ bàn 09 có thêm một PhysicalProp; không có cơ cấu mở bằng tín hiệu.
- Mô phỏng độc lập: freefall 9,81 m/s², quán tính cầu, gia tốc lăn trên dốc, contact và mất năng lượng, rebound, spin và rolling resistance.
- Prefab thật: spawn clearance, thành hộp theo từng cạnh contour gồm cả thành trong vành khuyên, sàn/nắp hỗ trợ đúng vùng có thể chơi và để trống phần khuyết/lõi rỗng; contact với cube, nằm yên, containment khi nghiêng và lỗ thoát thật trên cả chín bàn.
- Lối đi mới: sphere sweep theo toàn bộ bán kính 0,015 m qua góc chữ L/U, nửa vòng vành khuyên, cổ quả tạ và vùng giữa ngôi sao; sau đó đưa bi qua các waypoint bằng gravity và rotation intent.
- Exit: hướng đi từ trong ra ngoài, clearance tròn, toàn bộ bán kính vượt vỏ, một lần phát event, reset xóa passage cũ. Không teleport hoặc capture lúc thoát.
- Puzzle 09: thanh chặn kín toàn bộ chiều sâu cửa, body/joint không drive hoặc signal, trượt mở/đóng do gravity; fixture từ spawn chỉ xoay hộp để giữ bi, mở cửa, đi sang khoang phải và thoát thật.
- Lifecycle/input: mỗi body được áp gravity một lần (hai targets ở bàn 09), 100 reset mỗi bàn gồm cả root/prop, cleanup, manual next, pause, mouse/touch và tốc độ mô phỏng không đổi sau thoát.

Test exit có thể đặt bi vào vị trí thử và cấp vận tốc khởi đầu để cô lập detector/collider. Đây là fixture kiểm tra một hợp đồng vật lý, không phải chứng minh người chơi đã giải bàn từ spawn. Test dốc/va chạm cũng dùng điều kiện ban đầu kiểm soát được; player không có các thao tác thử nghiệm này.

Fixture lối đi đặt một trạng thái khởi đầu, sau đó chỉ nghiêng hộp bằng controller chuẩn; không di chuyển bi giữa các waypoint và không thêm lực điều khiển riêng. Đến vùng cuối hoặc thoát hợp lệ được coi là chứng minh passage. Đây không phải lời giải bắt buộc trong player hoặc cam kết về thời gian giải bằng touch.

Policy kiểm thử dùng điều khiển vị trí/vận tốc bảo thủ (`P=8`, `D=5`, gia tốc tiếp tuyến mong muốn tối đa 1 m/s²), chuyển thành góc nghiêng và đi qua giới hạn gia tốc/tốc độ của controller hộp. Bản policy đầu quá mạnh khiến hộp đảo góc muộn và bi dao động quanh waypoint; đã chỉnh riêng policy kiểm thử. Geometry, mô hình bi và tiêu chí clearance/đến waypoint không thay đổi vì lỗi điều khiển này.

## Giới hạn của bằng chứng

120 Hz và PhysX không hứa tái lập bit-for-bit trên mọi nền tảng. Dung sai nhỏ được đặt theo scale; contact có thể khác về thời điểm giữa các lượt chạy. Geometry/tốc độ đầu vào vẫn phải bảo toàn containment và không sinh năng lượng bất thường.

Tests passing không xác nhận cảm giác đã thuyết phục. Buổi thử trực tiếp cần kiểm tra tăng tốc, đảo nghiêng, va chạm cube, góc tam giác và âm thanh lăn/va chạm theo [kế hoạch](IMPLEMENTATION_PLAN.md). Kết quả thực tế và platform đã chạy được ghi riêng trong [nhật ký](DEVELOPMENT_LOG.md).
