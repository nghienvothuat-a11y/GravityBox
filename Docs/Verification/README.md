# Bằng chứng kiểm thử

Kết quả toàn bộ catalog 11 bàn ngày 08/09/2026, Unity 6000.3.19f1:

- [EditMode.xml](EditMode.xml): **8/8 passed**, kết thúc 15:40:05 UTC.
- [PlayMode.xml](PlayMode.xml): **58/58 passed**, từ 15:40:18 tới 15:40:47 UTC.

Tổng **66 tests**, không failure hoặc skipped. Lượt tập trung vào hai mê cung cũng **6/6 passed**, từ 15:39:19 tới 15:39:20 UTC.

Màn 10 có bằng chứng từ spawn qua cả hai cửa trượt ngược hướng rồi ra lỗ thật. Màn 11 có bằng chứng đi qua mê cung cả ba tầng, rơi liên tục qua hai lỗ chuyển tầng và thoát qua lỗ cuối. Các đường giải chỉ gửi ý định xoay hộp qua controller chuẩn; không đặt lại vị trí/vận tốc bi, không thêm lực điều khiển riêng và không teleport giữa tầng.

Các fixture riêng kiểm tra cửa đóng kín cả chiều sâu, gravity mở/đóng ngược hướng, 100 reset và thu hồi đủ force targets; sàn tầng chắn bi ngoài lỗ, chuyển tầng chưa phải chiến thắng. Layer view và thao tác chọn màn 11/đổi overview được kiểm tra không sửa vị trí, vận tốc hoặc collider.

Build và native QA được ghi riêng trong [nhật ký](../DEVELOPMENT_LOG.md). Các test không đo độ khó đối với người chơi hay xác nhận trải nghiệm đã cân bằng.

Mốc [chín bàn](../Archive/Verification/NineLevels/README.md), [tám hộp](../Archive/Verification/EightBoxes/README.md), [ba hộp](../Archive/Verification/ThreeBoxes/README.md) và [16 màn/L06](../Archive/Verification/README.md) được giữ riêng trong Archive.
