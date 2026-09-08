# Bằng chứng hiện tại: prototype tám hộp

Kết quả chạy toàn bộ bộ kiểm tra ngày 08/09/2026, Unity 6000.3.19f1:

- [EditMode.xml](EditMode.xml): 8/8 passed, kết thúc 14:41:26 UTC.
- [PlayMode.xml](PlayMode.xml): 44/44 passed, chạy từ 14:41:33 đến 14:41:53 UTC; gồm vật lý, cả tám prefab, resting/rebound, contour/floor/void, năm passage bằng nghiêng hộp, exit/reset/manual next và input/bộ chọn tám hình.

Tổng 52 tests, không có failure hoặc skipped test. Năm passage cũng qua lượt chạy riêng lúc 14:40:35 UTC. Kết quả build và native review được ghi riêng trong [nhật ký](../DEVELOPMENT_LOG.md); kiểm tra tự động không tự chứng minh cảm giác đã đạt.

Bằng chứng [ba hộp trước khi mở rộng](../Archive/Verification/ThreeBoxes/README.md) và [16 màn/L06](../Archive/Verification/README.md) được giữ riêng trong Archive.
