# Venom — nếp khối và sợi bám

Capture cận cảnh từ PlayMode/PhysX ngày 10/09/2026 sau lần chỉnh tiếp xúc sàn và tăng nhịp diễn xuất 1,5×. Camera phóng gần để kiểm tra chi tiết, không phải kích thước mặc định trong app. Không đặt lại vị trí hạt để tạo tư thế.

- [Yên, dồn vai lên rồi thu lại — 10 giây](idle.gif).
- [Trượt ban đầu, sợi bám và nhả khi dừng ở cơ cấu — 3,4 giây](moving.gif).
- [Hình đỉnh mềm lúc thăm dò](life-01-curious.png).
- [Hình sợi tiếp xúc ở rìa khối](life-02-gripping.png).

GIF dùng 10 frame/giây theo thời gian mô phỏng, không đại diện FPS thiết bị. Chuyển động ingame dựng hình mỗi frame, mục tiêu 60 fps. Chuỗi trượt ở đây kiểm tra tiếp xúc và nhả, không phải đường giải toàn màn.

Tái tạo PNG bằng test PlayMode `GravityBox.Tests.VenomPrototypeTests`, bật graphics và đặt `VENOM_CAPTURE_DIR` cùng `VENOM_CAPTURE_MOTION=1`. Các PNG `life-motion-idle-*` và `life-motion-moving-*` được lấy mỗi 0,1 giây mô phỏng.

[Nghiên cứu, nguồn tham chiếu và giới hạn](../../VENOM_MOTION_STUDY.md). Mở bản macOS để đánh giá feeling ở góc chơi thực tế.
