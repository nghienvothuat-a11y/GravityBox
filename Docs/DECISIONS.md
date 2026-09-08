# Các quyết định kiến trúc

## ADR 001 Unity 6.3 LTS và URP

Chọn 6000.3.19f1 có sẵn trên máy và URP 17.3.0. Unity xác định dòng 6000.3 là LTS trong tài liệu API. Cố định versions và packages-lock.json; không tự nâng giữa các phép so sánh vật lý. URP tránh phải chuyển toàn bộ vật liệu khi tiến tới mobile product; không thêm post-processing hoặc shader nặng ở prototype.

## ADR 002 Hộp kinematic và bóng world space

Chọn physical-root convention đầu tiên như GDD đề xuất. MoveRotation theo fixed tick, bounded angular velocity và input backlog. Ball không parent vào root và gravity luôn world-down. Zero-G chỉ đổi momentum qua contact/explicit force. Logical gravity là phương án nghiên cứu dự phòng, chưa triển khai song song để tránh hai truth sources.

## ADR 003 Physics không phải deterministic solver

Reset state rõ ràng và repeatability trong cùng cấu hình là yêu cầu hiện tại. Không hứa tính tái lập bit-for-bit across PhysX/platform. Dùng fixed timestep, primitive colliders, CCD cho ball có impulse, max depenetration speed; không tăng solver mặc định khi chưa đo.

## ADR 004 Giới hạn rotation và các chế độ snap

Assisted/free đều xoay liên tục, release chỉ snap nếu gần một trong 24 orientations. Chế độ QuarterTurn hiện snap đến canonical sau release, có chuyển động liên tục trong drag; là chế độ so sánh authoring, chưa phải gesture discrete chỉ cho phép một bước 90°. Cần product test nếu muốn semantics hard-snap nghiêm ngặt hơn.

## ADR 005 Reset registry theo màn

Không singleton global registry. Capture một lần sau bind, reset root trước ball; restore launch velocity của zero-G. Signal bus và IgnoreCollision đều được đưa vào lifecycle reset. Transition dùng deadline có thể hủy ngay, tránh coroutine tiếp tục sau reset.

## ADR 006 Tín hiệu cơ cấu và door

Plate publish channel, door subscribe qua bus instance. Exit có RequiredChannel cho puzzle sequencing. Door collision chuyển ngay theo signal, visual trượt dần; tradeoff prototype để không di chuyển collider con tương đối trong compound đang xoay. Product có thể chuyển door sang kinematic body riêng, cần kiểm tra ordering, reset và contact mới.

## ADR 007 Content baseline và authoring

16 prefab data-driven được tạo bởi editor builder để tái lập baseline và có thể mở Inspector chỉnh. Không encode level ID trong physics/input/runtime mechanism. Baseline dùng hành lang extrude theo depth để dễ đọc; vẫn là Rigidbody 3D tự do trong cube. Lối ra hiện dùng cửa xuyên vỏ hộp theo ADR 011; quyết định dùng trigger xuyên depth của baseline cũ đã bị thay thế theo yêu cầu người dùng.

## ADR 008 Build và sản phẩm

Desktop development build là đầu ra kiểm thử ngay trên máy hiện có. Android/iOS build được tự động hóa nếu toolchain sẵn. Chỉ ghi 'verified on device' sau khi cài/chạy thật. Save game, addressables, DI framework, backend và Phase 2 chưa có lý do đủ mạnh để triển khai trong baseline.

## ADR 009 Bóng bị che khuất

Một sphere overlay nhỏ dùng ZTest Greater để chỉ hiển thị silhouette khi geometry đặc che bóng. Đây là tín hiệu vị trí, không thay đổi collision hoặc velocity. Chi phí thêm một renderer/mesh sphere và shader unlit nhỏ; cần xác nhận readability trên màn hình thật. Shell vẫn hạn chế transparency; không làm toàn bộ cơ cấu trong suốt vì sẽ mất cảm nhận đường đi.

## ADR 010 Kiến trúc simulator

Build iOS Simulator chọn `PlayerSettings.iOS.simulatorSdkArchitecture = AppleMobileArchitectureSimulator.ARM64` cho máy Apple Silicon; Device SDK giữ setting ban đầu. Nếu để mặc định x86_64 rồi ép Xcode `-arch arm64`, UnityRuntime và baselib không link được. Tham chiếu: [Unity simulator architecture](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/AppleMobileArchitectureSimulator.html).

Trên simulator iOS 26.5 đã gặp lỗi Metal render attachment 1 sample so với yêu cầu MSAA 4. Export simulator tạm đặt URP MSAA = 1 (disabled), rồi phục hồi asset về 4 sau build. Device/macOS/Android vẫn dùng cấu hình baseline và cần profiling riêng; không dùng performance simulator làm số liệu thiết bị.

## ADR 011 Chỉ thắng khi bi thoát thật khỏi hộp

Yêu cầu cập nhật của người dùng ngày 08/09/2026 có ưu tiên so với quy tắc capture trong GDD gốc. Bỏ trigger xuyên depth và teleport đến CapturePoint. Mỗi màn có một cửa vuông 1,44 × 1,44 trên mặt đáy hoặc mặt phải, nằm ở depth -1,15 và gần đích cũ. Vỏ tại mặt đó gồm bốn collider quanh cửa; viền có collider riêng, lòng cửa hoàn toàn rỗng. Dùng cửa vuông để hình hiển thị khớp chính xác với primitive collider.

Local +Z của ExitSocket hướng ra ngoài; ApertureHalfSize và WallHalfDepth là hợp đồng giữa geometry với detector. Detector theo dõi đoạn chuyển động tương đối của tâm bi từ phía trong, qua tiết diện cửa, tới khi toàn bộ bán kính vượt mép ngoài thêm 0,02 m. Kiểm tra sweep tránh bỏ sót bước nhanh; chạm mép, đi từ ngoài vào, hoặc vượt mặt hộp ở vị trí khác đều không thắng. RequiredChannel còn khóa bằng shutter vật lý tái sử dụng SignalDoor.

Event Exited chỉ phát một lần. LevelManager khóa input xoay, giữ Rigidbody dynamic và vận tốc thực; TimeScale 0,35 trong 1,8 giây thực để quan sát thoát ra. Camera chỉ mở rộng sau khi thắng để giữ hộp và bi cùng khung. Chuyển màn dùng unscaled deadline; reset hủy deadline, trả tốc độ bình thường và khởi tạo lại mẫu vị trí sau khi reset ball/root. Chỉ ở màn cuối, sau khoảng quan sát, ball mới được giữ tại vị trí hiện tại để hiện màn tổng kết.

Nâng cấp dùng menu Upgrade Physical Exits để thay vỏ và cửa, giữ nguyên platforms, spawn, cơ cấu khác và tuning. Test khả giải phải ghi lại với điều kiện thắng mới, sau đó replay nghiêm ngặt ở lần kiểm tra bình thường.
