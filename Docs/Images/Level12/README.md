# Hình ảnh mê cung ván ghép — màn 12

Bốn ảnh chụp từ prefab **Lost in glass** lúc 16:55:03 UTC ngày 08/09/2026, Unity 6000.3.19f1:

- [Góc xuất phát](Sphere12Front.png)
- [Xoay sang hướng khác](Sphere12Turned.png)
- [Mặt sau](Sphere12Back.png)
- [Góc nhìn lỗ thoát](Sphere12Exit.png)

Khung mê cung gồm các ván mỏng ghép thành đoạn nối và ngã rẽ, cùng tồn tại trong một thể tích cầu. Vật liệu trong suốt nhẹ, nét viền mảnh và khe nhìn giúp quan sát các đường theo chiều sâu. Các khe nhìn là geometry trống thật nhưng nhỏ hơn bi; lối đi lớn hơn nối liên tục qua khung. Chỉ có một lỗ tròn trên vỏ cầu.

Các PNG là fixture đặt pose để kiểm tra hình ảnh, không phải lượt chơi: root/bi được đặt tại góc khai báo và không chạy physics. [Metadata](Sphere12RenderFixtures.json) lưu pose/camera/thời điểm. [Kiểm chứng](../../Verification/README.md) tách riêng hình học, lượt giải bằng physics và native.

Tạo lại bằng `Gravity Box > Capture Sphere Maze Render Fixtures` hoặc execute method `GravityBox.Editor.MazePreviewCapture.CaptureSphere` với graphics, không dùng `-nographics`. Các thay đổi vật liệu/settings do importer sinh khi khởi động renderer đã hoàn nguyên về source; fixture không lưu scene. Độ rõ khi xoay và trên màn hình điện thoại vẫn cần chơi trực tiếp.
