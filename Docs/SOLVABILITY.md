# Phạm vi kiểm chứng prototype ba hộp

Mục tiêu hiện tại là chuyển động và va chạm của bi thép. Không yêu cầu tạo lại hoặc replay lời giải 16 màn cũ. Các route trong `VERIFIED_ROUTES.json` và [bảng lời giải cũ](Archive/SOLVABILITY_16_LEVELS.md) chỉ là bằng chứng lịch sử của scale và catalog trước đó.

## Kiểm tra tự động

- Catalog: đúng ba hình Circle/Square/Triangle, Earth gravity, free rotation, không cơ cấu phụ, kích thước/khối lượng tương ứng thép đặc.
- Mô phỏng độc lập: freefall 9,81 m/s², quán tính cầu, gia tốc lăn trên dốc, contact và mất năng lượng, rebound, spin và rolling resistance.
- Prefab thật: spawn clearance, thành hộp quanh chu vi, contact với cube cố định, containment khi nghiêng và lỗ thoát có khoảng trống thực.
- Exit: hướng đi từ trong ra ngoài, clearance tròn, toàn bộ bán kính vượt vỏ, một lần phát event, reset xóa passage cũ. Không teleport hoặc capture lúc thoát.
- Lifecycle/input: một body được áp gravity một lần, 100 reset mỗi bàn, manual next, pause, mouse/touch và tốc độ mô phỏng không đổi sau thoát.

Test exit có thể đặt bi vào vị trí thử và cấp vận tốc khởi đầu để cô lập detector/collider. Đây là fixture kiểm tra một hợp đồng vật lý, không phải chứng minh người chơi đã giải bàn từ spawn. Test dốc/va chạm cũng dùng điều kiện ban đầu kiểm soát được; player không có các thao tác thử nghiệm này.

## Giới hạn của bằng chứng

120 Hz và PhysX không hứa tái lập bit-for-bit trên mọi nền tảng. Dung sai nhỏ được đặt theo scale; contact có thể khác về thời điểm giữa các lượt chạy. Geometry/tốc độ đầu vào vẫn phải bảo toàn containment và không sinh năng lượng bất thường.

Tests passing không xác nhận cảm giác đã thuyết phục. Buổi thử trực tiếp cần kiểm tra tăng tốc, đảo nghiêng, va chạm cube, góc tam giác và âm thanh lăn/va chạm theo [kế hoạch](IMPLEMENTATION_PLAN.md). Kết quả thực tế và platform đã chạy được ghi riêng trong [nhật ký](DEVELOPMENT_LOG.md).
