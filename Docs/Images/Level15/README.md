# Hộp đầu sư tử — bàn 15

Bốn ảnh prefab thật, render lúc **03:53:48 UTC ngày 09/09/2026**, 796 × 1494:

- [Góc chơi mặc định](Lion15Front.png): bờm, tai, năm khối phù điêu và lỗ miệng.
- [Góc nghiêng](Lion15Turned.png): chiều sâu hộp và kính trong.
- [Mặt sau](Lion15Back.png): sàn đồng và cửa tròn xuyên sàn.
- [Nhìn thẳng khuôn mặt](Lion15Face.png): contour sư tử cách điệu và lối quanh các gờ.
- [Metadata](Lion15RenderFixtures.json): các tư thế hộp/bi; camera toàn cục ghi góc cuối nhìn thẳng khuôn mặt.

Bi được đặt ở spawn và giữ kinematic trong các ảnh fixture để xem hình. Không dùng những ảnh này làm bằng chứng di chuyển hay giải màn. Shader kính dùng alpha thấp, không mô phỏng khúc xạ. Tạo lại bằng Unity batchmode có graphics: `-executeMethod GravityBox.Editor.MazePreviewCapture.CaptureLion`.

[Ảnh macOS đang chạy](MacOSLevel15.png) được chụp qua F12 lúc **04:00:07 UTC**, sau khi mở bản build lúc 03:58:52 UTC và chọn màn 15. Đây là ảnh native, bi dùng physics thật; chỉ xác nhận khởi chạy/hiển thị. Đường giải và va chạm được kiểm chứng riêng trong [PlayMode](../../Verification/Lion15/README.md).
