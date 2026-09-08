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

16 prefab data-driven được tạo bởi editor builder để tái lập baseline và có thể mở Inspector chỉnh. Không encode level ID trong physics/input/runtime mechanism. Baseline dùng hành lang extrude theo depth để dễ đọc; vẫn là Rigidbody 3D tự do trong cube. Exit là socket dạng đường hầm với trigger xuyên depth được thể hiện bằng ring/rail, không phải một lỗ điểm ở riêng mặt trước.

## ADR 008 Build và sản phẩm

Desktop development build là đầu ra kiểm thử ngay trên máy hiện có. Android/iOS build được tự động hóa nếu toolchain sẵn. Chỉ ghi 'verified on device' sau khi cài/chạy thật. Save game, addressables, DI framework, backend và Phase 2 chưa có lý do đủ mạnh để triển khai trong baseline.

## ADR 009 Bóng bị che khuất

Một sphere overlay nhỏ dùng ZTest Greater để chỉ hiển thị silhouette khi geometry đặc che bóng. Đây là tín hiệu vị trí, không thay đổi collision hoặc velocity. Chi phí thêm một renderer/mesh sphere và shader unlit nhỏ; cần xác nhận readability trên màn hình thật. Shell vẫn hạn chế transparency; không làm toàn bộ cơ cấu trong suốt vì sẽ mất cảm nhận đường đi.

## ADR 010 Kiến trúc simulator

Build iOS Simulator chọn `PlayerSettings.iOS.simulatorSdkArchitecture = AppleMobileArchitectureSimulator.ARM64` cho máy Apple Silicon; Device SDK giữ setting ban đầu. Nếu để mặc định x86_64 rồi ép Xcode `-arch arm64`, UnityRuntime và baselib không link được. Tham chiếu: [Unity simulator architecture](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/AppleMobileArchitectureSimulator.html).

Trên simulator iOS 26.5 đã gặp lỗi Metal render attachment 1 sample so với yêu cầu MSAA 4. Export simulator tạm đặt URP MSAA = 1 (disabled), rồi phục hồi asset về 4 sau build. Device/macOS/Android vẫn dùng cấu hình baseline và cần profiling riêng; không dùng performance simulator làm số liệu thiết bị.
