# Bằng chứng khả giải các màn

Unity 6000.3.19f1; fixed timestep 1/60 s. Các route được ghi lại sau thay đổi nắp vật lý ở L06 ngày 08/09/2026: toàn bộ bi đi qua cửa thật và thoát khỏi hộp bằng PhysX qua BoxRotationController, gravity/zero-G và cơ cấu chuẩn. Không teleport, không steering ball, không thêm lực trong phép kiểm tra khả giải.

Đây là kiểm chứng khả giải ở cấp rotation intent; chưa thay thế việc người mới giải bằng touch/mouse, chưa chứng minh độ vui hoặc tính ổn định trên mọi thiết bị. Mười lăm màn dùng route xoay quanh world Z. L06 có vật tự do thứ hai có thể đẩy bi lệch theo chiều sâu, nên phần cuối dùng policy quan sát bi và điều chỉnh cả hai trục của hộp. Tất cả dùng cùng giới hạn/smoothing của BoxRotationController. Input người chơi dùng camera yaw/pitch nên đường thao tác bằng ngón tay cần được ghi riêng trong playtest.

| Màn | Ngân sách thời gian | Chương trình xoay hộp |
| --- | --- | --- |
| gb-01 | 3.90 giây | -25° (3.90s) |
| gb-02 | 18.05 giây | -30° (7.00s) → 30° (7.00s) → -35° (4.05s) |
| gb-03 | 15.42 giây | -20° (3.93s) → -90° (3.63s) → 20° (1.77s) → 35° (1.87s) → -20° (2.82s) → -140° (1.40s) |
| gb-04 | 3.45 giây | -25° (3.45s) |
| gb-05 | 3.53 giây | -45° (3.53s) |
| gb-06 | 31.00 giây | 0° (2.00s) → 180° (4.00s) → 90° (4.00s) → Nghiêng hai trục theo vị trí bi (21.00s) |
| gb-07 | 3.33 giây | -35° (3.33s) |
| gb-08 | 7.70 giây | -140° (2.08s) → 90° (1.92s) → -20° (3.70s) |
| gb-09 | 15.40 giây | 140° (3.70s) → 55° (4.33s) → 20° (3.68s) → -90° (2.18s) → 0° (1.50s) |
| gb-10 | 15.77 giây | 140° (2.83s) → -20° (3.30s) → 0° (3.00s) → 35° (2.73s) → -35° (2.48s) → -20° (1.42s) |
| gb-11 | 3.90 giây | 0° (3.90s) |
| gb-12 | 3.52 giây | 55° (2.35s) → -20° (1.17s) |
| gb-13 | 10.22 giây | -140° (1.85s) → 180° (3.00s) → -140° (2.83s) → 140° (2.53s) |
| gb-14 | 19.30 giây | 20° (3.10s) → 180° (4.33s) → 20° (2.52s) → 180° (1.93s) → 140° (3.28s) → 140° (1.83s) → 180° (2.30s) |
| gb-15 | 11.02 giây | 0° (7.00s) → 90° (4.02s) |
| gb-16 | 8.28 giây | 0° (7.00s) → 45° (1.28s) |

Dữ liệu gốc: [VERIFIED_ROUTES.json](VERIFIED_ROUTES.json). Test All16Levels_HaveReproduciblePhysicsOnlySolveRoutes bắt buộc replay chương trình xoay đã lưu và fail nếu không giải được trong ngân sách. Với L06, `aimAtExit` là policy kiểm thử được khai báo tường minh trong JSON; không phải hỗ trợ tự lái trong game. Chỉ khi author chủ ý đặt biến môi trường `GRAVITYBOX_RECORD_SOLUTIONS=1`, test mới được tìm kiếm có seed/giới hạn và ghi lại route mới. Review thay đổi JSON trước khi nhận làm baseline; việc này không thay thế playtest.

Khi ghi baseline, đích xoay cuối có thêm 1 giây giữ nguyên để chịu sai lệch nhỏ về thời điểm contact giữa các lượt PhysX. Những đích xoay trung gian không được nới; replay không tìm lời giải mới hoặc sửa lực. Bảng thể hiện ngân sách input, không phải cam kết bi thoát đúng từng tick.

Các route dùng một số lần xoay lớn. Đây là dấu hiệu cần tuning input và độ rõ đường đi, đặc biệt L03, L10, L13, L14. L11 chủ ý cho bóng tự trôi tới exit để dạy quán tính.

L06 cần đưa lỗ lên trên để nắp rơi, nghiêng cho nắp dời sang bên, rồi đưa bi tới cửa. Route mô phỏng cả nắp và bi từ trạng thái chuẩn, không gọi signal mở cửa. Nắp có thể tự rơi lại vào lỗ nếu quay ngược ngay, nên người chơi cần quan sát và điều chỉnh cả hai vật. Các mốc thời gian trong bảng là bằng chứng kỹ thuật, không phải bộ đếm mở khóa.

Policy L06 chỉ dùng vị trí/vận tốc quan sát được của bi để chọn orientation mục tiêu. Nó tính sai lệch tiếp tuyến với mặt lỗ, giảm nghiêng khi bi tiến gần cửa và vẫn chịu giới hạn xoay 100°/s. Không sửa vị trí/vận tốc của bi hoặc nắp, không thêm lực, không mở collider, không gọi thắng. Policy nằm trong assembly kiểm thử, không có trong player. Đây là bằng chứng khả giải bằng thao tác xoay có phản hồi, không phải cam kết cùng một chuỗi góc sẽ cho kết quả bitwise giống nhau trong bài toán nhiều vật va chạm.
