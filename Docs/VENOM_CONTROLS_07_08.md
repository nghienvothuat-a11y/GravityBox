# Venom 07–08 — Hướng dẫn một sinh vật biết ghi nhớ

Hai màn này giữ hộp kính sáu mặt, sinh vật bò bằng lực bám và lỗ tròn giữa trần. Người chơi chỉ đích, sinh vật tự chọn đường bò trên mặt trong của hộp. Một đích trên mặt đối diện đòi hỏi nó bò qua tường, không bay xuyên lòng hộp.

## Cách test

- **Chạm / click chuột trái:** đặt đích trên mặt trong đang nhìn thấy xuyên qua kính. Vòng sáng nhỏ đánh dấu nơi đã chọn. Muốn chỉ mặt khác, xoay hộp để nhìn rõ mặt đó.
- **Giữ–kéo một ngón / chuột trái:** xoay hộp. Chuột phải hoặc hai ngón cũng xoay. Một thao tác đã kéo để xoay sẽ không phát thêm lệnh chạm lúc thả.
- **Phóng gần / Z:** camera theo sinh vật; bấm lại để nhìn toàn hộp.
- **Thử lại / R:** đưa sinh vật và cơ cấu về đầu màn, giữ kiến thức đã học. Sinh vật chờ lệnh mới.
- **Nhớ lại:** lặp lại những điểm đã được hướng dẫn thành công. Ở màn 08, nếu đã biết mở cửa, nó tự đến nút rồi đi ra.
- **Quên:** xóa trí nhớ của riêng màn đang chơi và dừng đích hiện tại. Đích mới sẽ thay thế lệnh đang làm.

Bản Mac chứa các màn 01–05, 07 và 08; phím số hoặc hàng nút trên cùng để chọn. Scene 06 không được thêm vào phạm vi này.

## Màn 07 — Chạm để dẫn đường

Bắt đầu trên sàn hộp. Chỉ vào một điểm trên tường để xem thân bò lên, rồi xoay hộp và chỉ điểm trên trần. Đích được lưu trong hệ tọa độ hộp, nên xoay không làm điểm trôi ra không gian thế giới.

Khi chỉ gần lỗ và sinh vật đã đến vùng lân cận, nó tự căn vào lỗ và thoát. Nếu chỉ một điểm khác trên trần, nó chọn đường vòng tránh vùng hút quanh lỗ. Thắng vẫn yêu cầu đủ 32 hạt đi qua lỗ thật, với phần cơ thể hiện ra bên ngoài.

Các điểm đã tới được ghi nhớ. Sau khi thoát thành công, vị trí lỗ được thêm vào chuỗi đã học. Có thể thử lại rồi bấm Nhớ lại để xem nó tự đi theo các chỉ dẫn cũ.

## Màn 08 — Học cách mở lối ra

Lỗ trần bị một tấm cửa trượt che kín. Trên sàn có nút sáng gần phẳng với mặt sàn:

1. Người chơi chạm vị trí nút để dẫn sinh vật đến đó.
2. Hạt vật lý thực sự tiếp xúc với nút; ít nhất 9 g thuộc cùng một phần giữ tiếp xúc trong 0,25 giây để nhả cửa.
3. Motor kéo tấm cửa sang bên. Sinh vật đợi cửa di chuyển đủ để thông lỗ rồi tự đổi mục tiêu sang lỗ thoát.
4. Nó bò từ sàn qua tường lên trần và chui ra, không cần chỉ thêm từng điểm.
5. Quan hệ nút → cửa được ghi nhớ sau khi kích hoạt thật. Thử lại rồi Nhớ lại sẽ lặp lại chuỗi tự động.

Chạm lỗ lúc cửa đóng hiện nhắc nhở tới nút trước. Cửa mở được chốt lại cho đến khi reset. Việc chỉ vào nút tự nó không mở cửa: cần tải tiếp xúc vật lý.

## Kiến trúc

| Thành phần | Vai trò |
| --- | --- |
| `VenomInput` | Phân biệt tap và drag; chọn mặt nhìn thấy bằng ray; chặn tap thừa khi đổi số ngón |
| `VenomGuidance` | Đích hiện tại, đến đích, lặp chỉ dẫn, quan sát nút/cửa và quyết định tự thoát |
| `VenomSurfaceRoute` | Đường qua các mặt kề nhau; đồ thị nhìn thấy trên trần để vòng tránh lỗ và tấm cửa đang trượt |
| `VenomGuidanceMemory` | Tối đa 24 điểm đã tới và quan hệ nút/cửa; lưu riêng từng màn bằng PlayerPrefs, schema v1 |
| `VenomLocomotion` / `VenomWallClimb` | Đổi ý định thành lực tiếp tuyến và lực bám lên những hạt có mặt đỡ |
| `VenomLevelController` | Vật lý, reset, cơ cấu và xác nhận toàn bộ vật chất thoát qua lỗ |

Đây là trí nhớ chỉ dẫn và logic quyết định có trạng thái, chưa phải mô hình học máy tự suy ra luật của cơ cấu bất kỳ. Điểm chỉ nhưng bị bỏ dở không được ghi thành một lần đến thành công. Đường bò hiện dành cho hộp lập phương trống của hai thí nghiệm; để thêm mê cung trên vỏ cần bổ sung bản đồ vật cản trên từng mặt và cạnh nối.

Bộ nhớ không dùng tọa độ màn hình hoặc đường hoạt hình. Sau reset/đổi hướng hộp, sinh vật tính lại đường tới các điểm trong tọa độ hộp. Physics vẫn quyết định vị trí, vận tốc, tiếp xúc và lượng vật chất; việc học không di chuyển hạt trực tiếp.

## Kiểm chứng

`VenomGuidanceTests` kiểm tra đi tới đích trên các mặt thật, giữ đích khi xoay, tránh lỗ khi chưa định thoát, tự thoát, phân biệt tap/drag/hai ngón, pause, ghi nhớ–lặp lại–quên, và chuỗi tiếp xúc nút → cửa thật → thoát → reset → lặp điều đã học.

Scene generator: **Gravity Box → Venom → Generate Experiments 07 and 08**. Generator lấy scene 04 làm nền và lưu riêng hai scene mới. Build Mac: `bash Tools/build-venom.sh`. XML/log và ảnh thử nằm trong `Artifacts/Venom01`; bản chạy ở `Builds/Venom/macOS/Venom.app`.

Kết quả kiểm chứng: **54/54 PlayMode Venom đạt**, gồm **12 trường hợp màn 07/08**. Lần chạy cuối bật đồ họa và chụp các trạng thái trong `Artifacts/VenomGuidance`; XML/log tại `Artifacts/Venom01/guidance-final-tests.*`. Màn 08 đã hoàn thành cả lần chỉ dẫn đầu và lần Nhớ lại khi hộp đang xoay. Đích mới từ trần có thể dẫn sinh vật trở lại tường/sàn mà không tự rơi vào lỗ giữa đường.

Ghi nhận đi qua miệng lỗ dùng cùng sai số tiếp xúc 1 mm với bước kiểm tra bên trong lỗ, tránh bỏ sót một hạt đã thật sự vượt qua mép trong dung sai PhysX. Điều kiện hoàn thành vẫn là đủ 32 hạt qua mặt thoát, không dựa vào một lần chạm đích. Nút màn 08 có vị trí reset lưu đúng trên sàn ngay ở frame đầu, không cần solver kéo về từ tâm hộp.

Bản macOS đã build thành công và được kiểm tra trực tiếp: chạm trên màn 07 làm sinh vật tới đích, đóng/mở app vẫn giữ điểm đã học, Nhớ lại hoạt động, kéo xoay hộp không đổi đích; màn 08 hoàn thành 100% từ một lần chạm vào nút. Dữ liệu hướng dẫn dùng cho QA trên app đã được xóa bằng Quên, để hai màn bắt đầu chưa được dạy khi bàn giao.
