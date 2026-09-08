# Kiểm tra hình ảnh bàn 12

Bốn góc dưới đây là prefab **Lost in glass** với 32 ván kính nhỏ tách rời, render ngày 08/09/2026 lúc 16:34:57 UTC trong Unity 6000.3.19f1:

- [Góc xuất phát](Sphere12Front.png)
- [Xoay sang hướng khác](Sphere12Turned.png)
- [Mặt sau](Sphere12Back.png)
- [Hướng nhìn vào lỗ thoát](Sphere12Exit.png)

Vỏ cầu và các ván cùng hiển thị trong một thể tích. Khoảng không giữa các ván giúp nhìn thấy bi và những mặt đỡ ở các hướng khác nhau. Chỉ có một lỗ tròn trên vỏ, với viền xanh lá mảnh; không có lỗ chuyển tiếp trong các ván. Các hình này thay thế bố cục vách lớn đã bị loại bỏ.

Đây là fixture hình ảnh, không phải ảnh ghi lại một lượt giải: mỗi góc đặt root và bi tại pose đã khai báo, không chạy vật lý. [Metadata](Sphere12RenderFixtures.json) ghi camera, vị trí/góc và thời điểm. [PlayMode XML](../../Verification/README.md) cung cấp bằng chứng riêng về rơi tự do và đường thoát bằng thao tác xoay.

Tạo lại qua `Gravity Box > Capture Sphere Maze Render Fixtures` hoặc execute method `GravityBox.Editor.MazePreviewCapture.CaptureSphere` với graphics, không dùng `-nographics`. Ảnh xuất vào `Artifacts/`. Các thay đổi vật liệu/settings tự sinh khi Unity khởi động renderer đã được hoàn nguyên về cấu hình source; fixture không lưu scene.

Native macOS đã mở đúng bố cục này, bộ chọn đủ 12 bàn và quan sát trạng thái **BALL OUTSIDE** sau khi người dùng thao tác. [Log native](../../Verification/Native-Level12.csv) ghi một lượt hoàn thành; không có ảnh native mới được lưu. Các PNG ở trên vẫn chỉ là fixture hình ảnh Editor.

Độ rõ khi cầu chuyển động, cảm giác căn điểm rơi và mức khó vẫn cần đánh giá khi chơi trực tiếp, đặc biệt trên màn hình điện thoại.
