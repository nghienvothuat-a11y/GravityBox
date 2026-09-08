# Bằng chứng kiểm thử

Kết quả chạy toàn bộ catalog chín bàn ngày 08/09/2026, Unity 6000.3.19f1:

- [EditMode.xml](EditMode.xml): **8/8 passed**, kết thúc 15:07:44 UTC.
- [PlayMode.xml](PlayMode.xml): **49/49 passed**, chạy từ 15:07:51 đến 15:08:14 UTC. Bao gồm cả chín prefab, physics/containment/exit/reset, bộ chọn cuộn tới màn 09 và bốn bài kiểm tra thanh trượt/puzzle mới.

Tổng **57 tests**, không failure hoặc skipped. Đường giải màn 09 bắt đầu từ spawn và chỉ gửi ý định xoay hộp; bi đi vào hốc, thanh trượt mở theo gravity, bi qua cửa và toàn bộ bán kính ra khỏi lỗ. Không dịch chuyển bi/thanh chặn bằng test harness trong đường giải này. Policy giải tự động chỉ nằm trong test, không có trong player.

Ray giữ dưới sàn trong bản thử đã làm passage thất bại với bi lăn chậm. Bản được kiểm chứng ở trên chỉ có hai ray phía nắp và sàn cửa phẳng; giữ nguyên policy, thông số bi và điều kiện qua test.

Kết quả build/native review được ghi riêng trong [nhật ký](../DEVELOPMENT_LOG.md). Kiểm tra tự động chưa phải kết luận về độ khó hoặc cảm giác chơi.

Các mốc [tám hộp](../Archive/Verification/EightBoxes/README.md), [ba hộp](../Archive/Verification/ThreeBoxes/README.md) và [16 màn/L06](../Archive/Verification/README.md) được giữ riêng trong Archive.
