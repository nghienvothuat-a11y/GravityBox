# Venom 05 — Chia ra để lọt vào

Màn 05 giữ hộp kính, bò tường, xoay hộp và Zoom của màn 04. Lỗ tròn ở giữa trần được bọc trong một hộp kính nhỏ, kín phía dưới và các cạnh, chỉ có một cửa rộng 34 mm ở phía trước.

## Chuỗi chơi

1. Bò từ sàn lên tường và trần, tới lưỡi dao có mép sáng màu hổ phách.
2. Đưa cơ thể qua dao. Dao cắt các liên kết vật chất tại mặt cắt thật; tỷ lệ các mảnh phụ thuộc vị trí tiếp cận, không đặt sẵn 50/50.
3. Quyền điều khiển tự chuyển sang mảnh lớn nhất. Các mảnh còn lại bám chờ trên trần.
4. Dẫn phần lớn vào khe cửa. Mô đã cắt có thể thu hẹp và kéo dài để luồn qua cửa. Cơ thể nguyên vẹn giữ liên kết chặt, không bật chế độ luồn khe chủ động.
5. Khi **toàn bộ phần lớn đã vào hộp nhỏ**, các phần còn lại tự tìm đường qua khe. Chúng không chạy theo sau 3 giây; thời gian chờ không có giới hạn.
6. Dẫn chủ thể tới lỗ tròn. Các mảnh đi sau tiếp tục tự vào và thoát kể cả khi chủ thể đã ra ngoài. Chỉ thắng khi đủ 32/32 hạt đã thực sự xuyên qua lỗ.

Chuột trái giữ–kéo hoặc WASD/mũi tên để bò; chuột phải kéo để xoay hộp. Cảm ứng: một ngón bò, hai ngón xoay. Zoom In/Out hoặc Z; R thử lại; P/Esc tạm dừng; 1–5 chọn màn.

## Kiến trúc và phạm vi

- Scene riêng `Venom05.unity`, sinh từ `VenomPrototypeBuilder.GenerateVault`. Scene và cơ quan màn 04 được giữ nguyên.
- `VenomSplitVault`: dao, hình học khoang, xác nhận toàn bộ chủ thể đi vào, kích hoạt đi theo và lưu anchor chủ thể. Reset xóa trạng thái này.
- `VenomLocomotion`: chọn mảnh lớn nhất sau khi cắt; giữ chủ thể từ khi bắt đầu đi vào khoang (kể cả khi các hạt đầu đã thoát và phần còn lại tạm nhỏ hơn mảnh đang chờ), tự điều khiển các mảnh đi sau đến lỗ. Không yêu cầu chạm chọn từng mảnh.
- `VenomNavigator` dùng mặt trần trong hệ tọa độ hộp cho màn 05. A* xét các collider thật của hộp nhỏ và dao, thử clearance cơ thể rồi clearance nhỏ cho cửa hẹp. Không tìm được đường thì mảnh chờ, không đi xuyên vách.
- `VenomSqueeze` dùng mặt di chuyển tương ứng, giữ bán kính và collider hạt khi tiếp xúc với cơ quan. Mô bên trong được phép biến dạng sau khi cắt; không teleport qua cửa. Tầm bám trần tăng lên 55 mm khi mô kéo dài, còn đổi mặt ở mép dùng ngưỡng riêng 25 mm để không bị hút sang tường sớm.
- Các mảnh bò bằng lực của `VenomWallClimb`, bám theo hộp khi xoay. Pathfinding của màn này dành cho các mảnh đã bị cắt **trên trần**, không phải bộ tìm đường tự do trên mọi mesh 3D.
- Không thêm timer, mất máu hay phạt sai tỷ lệ cắt. Nếu các mảnh gặp lại nhau ngoài hộp và hợp nhất, người chơi có thể đưa cơ thể về dao để cắt lại.

Hộp nhỏ rộng 200 mm, sâu 150 mm, cao 100 mm ở mặt trong trần. Cửa 34 mm có hai mép sáng nhẹ; dao đứng riêng bên phải cửa. Các mặt kính giữ collider thật, gồm cả tấm đáy để không thể vào từ bên dưới.

## Kiểm chứng

**36/36 bài kiểm tra PlayMode của Venom đã pass**, gồm **5/5 bài màn 05**: cả đường giải từ điểm xuất phát đến thoát 32/32 hạt; thử lệch dao cho 27/4/1 và 8/24 hạt; mẩu nhỏ sát dao cũng tìm được đường rời mép; cơ thể nguyên vẹn không qua được cửa bằng thao tác đẩy liên tục.

`VenomVaultTests` kiểm tra tiếp cận dao bằng lực từ điểm xuất phát, tỷ lệ cắt và lựa chọn mảnh lớn, chờ quá 3 giây, đi qua cửa rồi kích hoạt follower, thoát đủ vật chất, hình học kín của khoang và reset.

Build macOS dùng `bash Tools/build-venom.sh`, chọn **05 · CẮT** hoặc phím **5**. Chưa build APK cho biến thể Venom.
