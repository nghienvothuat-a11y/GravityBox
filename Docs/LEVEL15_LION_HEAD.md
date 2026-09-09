# Bàn 15 — The lion's mouth

Hộp khô hình đầu sư tử cách điệu, bằng đồng và kính. Trọng lực Trái Đất, bi thép 30 mm / 111 g, ma sát, độ nảy và thao tác xoay dùng nguyên cấu hình chung. Màn này thử đường bi lăn trong một hình dạng có ý nghĩa thị giác, với các hốc bờm, tai và các khối nổi trên mặt.

## Hình học có thể chơi

- Lòng hộp theo contour đối xứng 50 đỉnh, rộng khoảng 714 mm, cao 698 mm trong mặt XZ; chiều dày hộp 90 mm, khoảng trong 84 mm. Vỏ dày 6 mm. Đây là hộp dạng mặt nạ có chiều sâu và phù điêu, không phải đầu thú giải phẫu hoặc một tấm ảnh đặt lên hộp vuông.
- Bờm răng cưa và hai tai là thành vỏ thật. Mesh sàn khoét lỗ, nắp và thành bên đều dùng cùng contour; không có sàn vô hình bắc qua các khoảng lõm.
- Năm khối nổi cố định: hai mắt, mũi tam giác và hai phần mõm. Mỗi khối dùng mesh vát cạnh và một convex MeshCollider từ chính mesh đó; gắn vào root kinematic, cùng vật liệu tiếp xúc với hộp vuông. Khe từ đỉnh gờ tới nắp nhỏ hơn đường kính bi, nên không thể đi xuyên phía trên gờ.
- Màu mặt, các múi bờm, mảng tai và điểm sáng mắt là inlay không collider. Inlay mặt chừa cửa R=23 mm cùng 0,8 mm quanh vành để không che lỗ thật và vòng sáng mảnh bên dưới.
- Lỗ tròn ở phần miệng, local `(0, -0,045, -0,215)` m; dùng hỗ trợ hút 40 mm đã áp dụng cho mọi màn. Vẫn chỉ thắng khi toàn bộ bi ra ngoài.

Bi xuất phát gần tai trái `(-0,23, -0,024, 0,22)` m. Đường kiểm thử đi từ bờm lên trán, giữa hai mắt, vòng bên trái hoặc phải của mõm rồi tới cửa miệng. Hai nhánh đều được kiểm tra từ spawn bằng cách chỉ đổi ý định xoay hộp; không ghi pose/velocity bi trên đường giải. Không có công tắc, motor vật cản, chất lỏng hay thay gravity riêng ở màn này.

## Hình ảnh và kiến trúc

`LionHeadBuilder` chỉ chạy lúc authoring: sinh contour, các mesh phù điêu, inlay, vật liệu và lưu prefab. Gameplay dùng `LevelRuntime`, `BoxRotationController`, `BallController`, `ExitSocket` sẵn có; không thêm bộ điều khiển riêng cho sư tử. Enum `LionHead` được nối cuối để giữ nguyên giá trị của 14 màn trước.

Kính dùng shader `Inspection Glass` với alpha thấp và viền tăng theo góc nhìn, giữ tương phản bi/gờ thay vì phủ một lớp phản sáng đặc lên mặt. Đây là hiển thị kính đơn giản, không mô phỏng khúc xạ quang học. Root vẫn xoay quanh origin như các hộp trước; phần phù điêu có va chạm, phần màu trang trí không tạo lực.

Nghiệm thu gồm content validation, vỏ kín, lỗ thoát ở ba tư thế, đường giải cả hai bên, va chạm mũi, reset, selector 15 màn và bản macOS. [Kết quả](Verification/Lion15/README.md), [ảnh](Images/Level15/README.md).
