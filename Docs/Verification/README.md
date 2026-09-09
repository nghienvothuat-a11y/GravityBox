# Bằng chứng kiểm thử mê cung ván ghép

Trang này giữ bằng chứng của phiên bản 12 bàn trước khi thêm nước. Bộ kiểm tra mới của 13 bàn nằm tại [Water13](Water13/README.md).

Bản hiện tại ghép các ván nhỏ thành mê cung liên tục thay cho 32 ván rải rác. Kết quả của bản cũ, gồm lượt native 16,305 giây, ở [Archive](../Archive/Verification/SphereFreePlanks/README.md).

## Kiểm tra Unity của bố cục hiện tại

Unity 6000.3.19f1, ngày 08/09/2026 theo UTC:

- [EditMode.xml](EditMode.xml): **8/8 passed**, kết thúc 17:10:40 UTC.
- [PlayMode.xml](PlayMode.xml): **63/63 passed**, từ 17:10:53 tới 17:11:26 UTC.

Tổng **71/71**, không failure hoặc skipped. Bốn test riêng xác nhận graph 25 đoạn/21 khúc ngoặt và nhánh cụt, collider thật của mọi lối nối/mặt bịt/khe nhìn, một lượt giải từ spawn và reset/unload. Phép thử hình học dùng cả bán kính bi, gồm 648 sphere sweep ngang các khe của đoạn nối, các mặt mở/đóng tại ngã rẽ và 408 hướng radial của vỏ cầu.

Lượt giải đi qua đủ 25 đoạn chính, chủ động vào nhánh cụt `21 → 18` rồi quay lại `18 → 21` trước khi tiếp tục tới lỗ thoát. Tổng góc xoay đo được **1.598,7°**, khoảng bay tự do dài nhất **0,142 giây**. Chỉ gửi ý định xoay qua controller; không đặt lại vị trí/vận tốc bi, đổi gravity, sửa collider hoặc thêm lực lái. Bi giữ trạng thái dynamic, đi liên tục trong khung và chỉ phát thắng khi toàn bộ bi thoát qua cửa thật.

Fixture đọc frame Rigidbody thay vì Transform nội suy để đo/điều khiển chính xác, và tính một nút đã được ghé khi bi đi vào ngã rẽ thật; không yêu cầu bi dừng hẳn hoặc nằm trong tiết diện đã thu hẹp của một đoạn thẳng. Nếu quán tính đưa bi vào nhánh bên, policy kiểm thử xoay để quay về ngã rẽ trước khi tiếp tục. Policy này không có trong player. Đây là kiểm chứng khả giải và hồi phục vật lý, không phải số đo độ khó với người chơi.

Các bài chung giữ kiểm tra bi thép, nằm yên, va chạm cube, input, reset, retention/exit của cả 12 bàn; đường giải bàn 09–11 và chế độ xem tầng của 11 tiếp tục pass.

## Hình học tĩnh

- [Kiểm tra đường tắt](connected-sphere-topology-over-3mm.json): lưới 3 mm, nới không gian cho tâm bi bằng nửa đường chéo voxel (2,598 mm). Bán kính kiểm tra hiệu dụng 12,402 mm; không tìm được đường từ spawn ra khoảng trống sát vỏ. Khóa lần lượt mỗi đoạn của tuyến chính đều cắt đường tới vùng trước cửa: **25/25 đoạn cần thiết**.
- [Kiểm tra độ rộng](connected-sphere-topology-under-3mm.json): tăng bán kính kiểm tra lên 17,598 mm và thu hẹp vỏ tương ứng; vẫn liên thông từ spawn tới trước cửa. Đây là kiểm tra có biên dự phòng cho bi thật R=15 mm.

Dữ liệu lấy từ 1.309 hộp mỏng tạo 54 mesh section; mỗi báo cáo lưu SHA-256 của prefab, thời điểm và phạm vi. Công cụ [audit-sphere-topology.py](../../Tools/audit-sphere-topology.py) chỉ đọc prefab, dùng Python 3 và NumPy:

```sh
python3 Tools/audit-sphere-topology.py --mode over --cuts
python3 Tools/audit-sphere-topology.py --mode under
```

Lưới chỉ xét geometry tĩnh từ các descriptor hộp. Mục tiêu là phía trong, trước lỗ cầu; không dùng kết quả này để xác nhận va chạm tốc độ cao, khả giải bằng input hoặc toàn bộ bi xuyên qua lỗ cuối. Những hành vi đó do PlayMode/native kiểm tra riêng. Độ khó đối với người chơi cũng cần lượt thử trực tiếp.

## Build và kiểm tra player

Sau bộ test cuối, macOS development player và Android ARM64/IL2CPP build đều thành công. APK thực tế 59,208,154 bytes (56.47 MiB). Chưa kiểm tra trên thiết bị Android hoặc build lại iOS.

Player macOS mới đã khởi chạy và mở màn 12 lúc 17:14:48 UTC ngày 08/09/2026; quan sát trực tiếp khung mê cung ván ghép cùng HUD mới. Đây là kiểm tra hiển thị/khởi chạy, không chứng minh hoàn thành bằng tay hoặc độ khó khi chơi.
