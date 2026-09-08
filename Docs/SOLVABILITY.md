# Bằng chứng khả giải các màn

Unity 6000.3.19f1; fixed timestep 1/60 s. Các route dưới đây đạt exit bằng PhysX qua BoxRotationController, gravity/zero-G và cơ cấu chuẩn. Không teleport, không steering ball, không thêm lực trong phép kiểm tra khả giải.

Đây là kiểm chứng khả giải ở cấp rotation intent; chưa thay thế việc người mới giải bằng touch/mouse, chưa chứng minh độ vui hoặc tính ổn định trên mọi thiết bị. Route xoay quanh world Z, dùng cùng giới hạn/smoothing của controller. Input người chơi dùng camera yaw/pitch nên đường thao tác bằng ngón tay cần được ghi riêng trong playtest.

| Màn | Thời gian mô phỏng | Các đích xoay Z và thời gian giữ |
| --- | --- | --- |
| gb-01 | 2.52 giây | -25° (2.52s) |
| gb-02 | 16.78 giây | -30° (7.00s) → 30° (7.00s) → -35° (2.78s) |
| gb-03 | 16.30 giây | -30° (7.00s) → 35° (7.00s) → -35° (2.30s) |
| gb-04 | 1.95 giây | -25° (1.95s) |
| gb-05 | 2.23 giây | -45° (2.23s) |
| gb-06 | 21.85 giây | 35° (2.93s) → 55° (3.33s) → -140° (1.60s) → 180° (2.02s) → -35° (4.37s) → 55° (2.03s) → -140° (3.10s) → -55° (2.47s) |
| gb-07 | 1.88 giây | -35° (1.88s) |
| gb-08 | 6.30 giây | -140° (2.08s) → 90° (1.92s) → -20° (2.30s) |
| gb-09 | 9.23 giây | 40° (7.00s) → -40° (2.23s) |
| gb-10 | 14.17 giây | 140° (2.83s) → -20° (3.30s) → 0° (3.00s) → 35° (2.73s) → -35° (2.30s) |
| gb-11 | 1.88 giây | 0° (1.88s) |
| gb-12 | 8.68 giây | 35° (3.58s) → 180° (1.67s) → 20° (3.13s) → 90° (0.30s) |
| gb-13 | 4.90 giây | -140° (1.85s) → 180° (3.00s) → -140° (0.05s) |
| gb-14 | 18.10 giây | 20° (3.10s) → 180° (4.33s) → 20° (2.52s) → 180° (1.93s) → 140° (3.28s) → 140° (1.83s) → 180° (1.10s) |
| gb-15 | 9.72 giây | 0° (7.00s) → 90° (2.72s) |
| gb-16 | 5.82 giây | 0° (5.82s) |

Dữ liệu gốc: [VERIFIED_ROUTES.json](VERIFIED_ROUTES.json). Test All16Levels_HaveReproduciblePhysicsOnlySolveRoutes bắt buộc replay route đã lưu và fail nếu route thay đổi. Chỉ khi author chủ ý đặt biến môi trường `GRAVITYBOX_RECORD_SOLUTIONS=1`, test mới được tìm kiếm có seed/giới hạn và ghi lại route mới. Review thay đổi JSON trước khi nhận làm baseline; việc này không thay thế playtest.

Các route dùng một số lần xoay lớn. Đây là dấu hiệu cần tuning input và độ rõ đường đi, đặc biệt L06, L10, L14. L11 chủ ý cho bóng tự trôi tới exit để dạy quán tính.
