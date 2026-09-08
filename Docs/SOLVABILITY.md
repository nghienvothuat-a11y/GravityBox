# Bằng chứng khả giải các màn

Unity 6000.3.19f1; fixed timestep 1/60 s. Các route được ghi lại sau thay đổi lỗ tròn phẳng ngày 08/09/2026: toàn bộ bi đi qua cửa thật và thoát khỏi hộp bằng PhysX qua BoxRotationController, gravity/zero-G và cơ cấu chuẩn. Không teleport, không steering ball, không thêm lực trong phép kiểm tra khả giải.

Đây là kiểm chứng khả giải ở cấp rotation intent; chưa thay thế việc người mới giải bằng touch/mouse, chưa chứng minh độ vui hoặc tính ổn định trên mọi thiết bị. Route xoay quanh world Z, dùng cùng giới hạn/smoothing của controller. Input người chơi dùng camera yaw/pitch nên đường thao tác bằng ngón tay cần được ghi riêng trong playtest.

| Màn | Thời gian mô phỏng | Các đích xoay Z và thời gian giữ |
| --- | --- | --- |
| gb-01 | 2.90 giây | -25° (2.90s) |
| gb-02 | 17.05 giây | -30° (7.00s) → 30° (7.00s) → -35° (3.05s) |
| gb-03 | 14.42 giây | -20° (3.93s) → -90° (3.63s) → 20° (1.77s) → 35° (1.87s) → -20° (2.82s) → -140° (0.40s) |
| gb-04 | 2.45 giây | -25° (2.45s) |
| gb-05 | 2.53 giây | -45° (2.53s) |
| gb-06 | 22.18 giây | 35° (2.93s) → 55° (3.33s) → -140° (1.60s) → 180° (2.02s) → -35° (4.37s) → 55° (2.03s) → -140° (3.10s) → -55° (2.80s) |
| gb-07 | 2.33 giây | -35° (2.33s) |
| gb-08 | 6.72 giây | -140° (2.08s) → 90° (1.92s) → -20° (2.72s) |
| gb-09 | 9.93 giây | 40° (7.00s) → -40° (2.93s) |
| gb-10 | 14.77 giây | 140° (2.83s) → -20° (3.30s) → 0° (3.00s) → 35° (2.73s) → -35° (2.48s) → -20° (0.42s) |
| gb-11 | 2.90 giây | 0° (2.90s) |
| gb-12 | 2.52 giây | 55° (2.35s) → -20° (0.17s) |
| gb-13 | 9.22 giây | -140° (1.85s) → 180° (3.00s) → -140° (2.83s) → 140° (1.53s) |
| gb-14 | 18.30 giây | 20° (3.10s) → 180° (4.33s) → 20° (2.52s) → 180° (1.93s) → 140° (3.28s) → 140° (1.83s) → 180° (1.30s) |
| gb-15 | 10.02 giây | 0° (7.00s) → 90° (3.02s) |
| gb-16 | 7.28 giây | 0° (7.00s) → 45° (0.28s) |

Dữ liệu gốc: [VERIFIED_ROUTES.json](VERIFIED_ROUTES.json). Test All16Levels_HaveReproduciblePhysicsOnlySolveRoutes bắt buộc replay route đã lưu và fail nếu route thay đổi. Chỉ khi author chủ ý đặt biến môi trường `GRAVITYBOX_RECORD_SOLUTIONS=1`, test mới được tìm kiếm có seed/giới hạn và ghi lại route mới. Review thay đổi JSON trước khi nhận làm baseline; việc này không thay thế playtest.

Các route dùng một số lần xoay lớn. Đây là dấu hiệu cần tuning input và độ rõ đường đi, đặc biệt L03, L10, L13, L14. L11 chủ ý cho bóng tự trôi tới exit để dạy quán tính.
