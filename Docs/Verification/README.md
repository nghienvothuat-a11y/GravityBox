# Bằng chứng kiểm thử

Kết quả toàn bộ catalog 12 bàn với **bản ván kính rời của màn 12**, ngày 08/09/2026, Unity 6000.3.19f1:

- [EditMode.xml](EditMode.xml): **8/8 passed**, kết thúc 16:38:06 UTC.
- [PlayMode.xml](PlayMode.xml): **63/63 passed**, từ 16:38:19 tới 16:38:52 UTC.

Tổng **71 tests**, không failure hoặc skipped.

Bốn test riêng của bàn 12 xác nhận 32 ván nhỏ cố định, khoảng không đủ cho bi, nhiều hướng mặt đỡ, rơi tự do rồi va chạm ván khác, một route từ spawn tới thoát thật và reset/unload. Các phép thử dùng prefab cuối cùng, với một MeshCollider vỏ cầu và 32 BoxCollider của ván; không có collider hành lang, vách khoang hoặc lỗ chuyển tiếp ẩn.

Trong phép đo cú rơi đầu, bi có 0,117 giây không tiếp xúc, giảm cao 0,1192 m trước khi chạm ván đón. Gia tốc world giữa các bước bay sai khác dưới 0,05 m/s² so với `(0, −9.81, 0)`. Route hoàn chỉnh chạm hai ván, có khoảng bay dài nhất 0,333 giây rồi toàn bộ bi đi qua lỗ thoát. Fixture tăng ý định nghiêng dần, sau đó điều chỉnh góc cầu qua controller chuẩn; không viết pose/vận tốc bi, thêm lực lái riêng hoặc thay gravity. Các đường khác và việc bỏ qua ván vẫn hợp lệ; bài test không chứng minh mức “siêu khó” hay thứ tự 32 bước.

Các bài chung kiểm tra vỏ cầu bằng sphere cast ở 408 hướng, silhouette, lỗ thật, spawn, nằm yên với tải đỡ 0,95–1,05 mg, reset, containment, thoát và bộ chọn đủ 12 bàn. Test reset so pose Rigidbody với pose ban đầu đã lưu để tránh nhầm Transform nội suy với trạng thái vật lý; giữ dung sai và kiểm tra cả bước mô phỏng đầu tiên sau reset.

Bàn 10 tiếp tục có bằng chứng từ spawn qua cả hai cửa trượt ngược hướng rồi ra lỗ thật. Bàn 11 có bằng chứng đi qua mê cung cả ba tầng, rơi liên tục qua hai lỗ chuyển tầng và thoát qua lỗ cuối. Layer view và thao tác chọn lại màn 11/đổi overview giữ nguyên các collider và Rigidbody.

Phiên chơi native macOS sau khi mở bản mới ghi nhận một lượt hoàn thành màn 12 trong 16,305 giây, 5 lượt kéo, 0 reset: [CSV](Native-Level12.csv). Người dùng điều khiển trong lượt này; đây không phải replay do công cụ desktop thực hiện. Dữ liệu này không chứng minh mức “siêu khó”.

Build và quan sát hình ảnh được ghi riêng trong [nhật ký](../DEVELOPMENT_LOG.md). Các test chứng minh đường đi và hợp đồng vật lý, chưa đo độ khó với người chơi hay xác nhận trải nghiệm đã cân bằng.

Mốc [11 bàn](../Archive/Verification/ElevenLevels/README.md), [chín bàn](../Archive/Verification/NineLevels/README.md), [tám hộp](../Archive/Verification/EightBoxes/README.md), [ba hộp](../Archive/Verification/ThreeBoxes/README.md) và [16 màn/L06](../Archive/Verification/README.md) được giữ riêng trong Archive.
