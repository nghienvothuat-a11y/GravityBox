# NewGraphic — profile thử nghiệm đã triển khai

Phạm vi riêng: V2 01–10 trên nhánh `NewGraphic`, theo yêu cầu triển khai concept Blender ngày 29/09/2026. Không thay quy chuẩn Day Lab cho các màn khác.

- Đế/vỏ sứ ivory bo cạnh, panel xanh nhạt, chân graphite nhỏ. Cơ quan thao tác amber, ray kim loại; mặt trơn lavender. Không dùng màu amber cho phần nền không tương tác.
- Aperture mint là vòng phẳng rộng 6 mm trong thử nghiệm này, không collider, không bóng, không thay kích thước lỗ. Profile Current giữ vòng 3,2 mm cũ. Không biến cửa thành cổng phát sáng chói.
- COghe giữ nguyên mesh sống, xúc tu và mô phỏng; skin tối ướt, không thêm mắt/miệng. Không giảm số hạt hay bước vật lý để bù chi phí art.
- Vỏ được tách từng mặt, dùng cutaway hiện hành; đồ trang trí không chặn raycast. Renderer mới bám transform thật của cơ quan.
- Một directional key ấm và fill xanh nhẹ. URP key dùng bias riêng cho mô hình nhỏ, không dùng bias toàn pipeline. Giữ shadow map/resolution và renderer của dự án, không thêm hậu kỳ hoặc reflection realtime.
- Đường ghép sàn là chi tiết mảnh trên mặt, không ảnh hưởng đi lại. Texture panel chỉ tạo sắc độ nhẹ. Lỗ thật luôn thông, không đặt tấm trang trí ngang lỗ.
- Sau phản hồi của người dùng về độ lệch concept: nền lab dùng cảnh Blender dựng và làm mờ sẵn thành PNG 768×320. Shader bàn studio trộn nền ở phần trên màn hình; không thêm DOF hay blur thời gian thực trên điện thoại.
- Kiểm tra portrait, overview, pinch, orbit, tay nắm bị che và ăn mừng. A/B reload cùng màn; không so hai trạng thái cơ quan khác nhau rồi quy chênh lệch cho art.

Nguồn editable và cách tái tạo: [ArtSource](../../../../ArtSource/COghe/NewGraphic/README.md). Ảnh concept không phải cam kết mọi chi tiết giống render offline; ảnh runtime là bằng chứng nghiệm thu.

## Vòng sửa theo phản hồi trực tiếp — 29/09/2026

Bản cài đầu chưa đạt hình ảnh concept. Vòng sửa dùng khung sứ liền bo góc thay các thanh ghép, mặt ốp cửa, sàn ghép panel, pale blue sáng hơn và key shadow rõ hơn. Các mặt mỏng dùng material hai mặt; không suy rằng shader của mesh gốc có thể dùng culling mặc định. Viền mint mới thay hiển thị đường cũ để tránh hai nét chồng. Camera có padding riêng cho vỏ mới, không sửa collider hoặc đổi puzzle.

Nền dựng trong `Laboratory_Backdrop.blend`, tái tạo bằng `build_backdrop.py`. Nó chỉ gợi bối cảnh nghiên cứu, không chứa cơ quan chạm được. Nghiệm thu phải dựa trên ảnh Unity/OPPO mới; không coi việc tạo xong file Blender là đạt art direction.
