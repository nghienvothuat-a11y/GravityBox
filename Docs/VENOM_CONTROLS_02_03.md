# Venom 02–03 — Điều khiển cơ thể trong hộp đứng yên

Hai thí nghiệm độc lập trên nhánh `Venom`, bên cạnh màn 01 nghiêng hộp. Cả hai giữ hộp nằm ngang, trọng lực thế giới 9,81 m/s², 32 hạt vật lý/96 g, lỗ tròn xuyên sàn và luật thoát đủ 100% vật chất. Không đổi nội dung 23 bàn bi thép.

## Chọn màn và điều khiển

App macOS `Builds/Venom/macOS/Venom.app` có ba nút chọn màn ở đầu màn hình; phím **1/2/3** tương ứng. Có thể mở riêng `Venom02.unity` hoặc `Venom03.unity` trong Unity. Game View portrait 9:16.

- Chạm/click một phần để chọn ở màn 02; cũng có thể bấm nút A/B dưới hộp hoặc **Tab** để chuyển phần.
- Giữ–kéo trong vùng hộp: joystick tương đối với điểm bắt đầu. Kéo xa tăng tốc độ yêu cầu, vẫn có gia tốc và quán tính. Trở về vùng chết hoặc thả tay để dừng bò và bám nhẹ. **WASD/mũi tên** điều khiển theo mặt phẳng sàn nhìn từ camera.
- Vòng xanh và nhãn `ĐANG CHỌN`/`CHỦ THỂ` chỉ phần nhận input. Nhãn phần còn lại cho biết đang bám, thời gian chờ, đang tìm về hoặc chờ lối mở.
- **R** reset toàn bộ, **P/Esc** pause. Pause dừng cả vật lý lẫn đồng hồ 3 giây. Reset xóa input, selection, đường đi và thời gian chờ.

## 02 — Hai phần, một kế hoạch

Lưỡi chém được giữ cao cho đến khi khối vào vùng cắt, sau đó hạ bằng lực motor, rồi nâng lại sau nhịp cắt 1,2 giây để không ghì mảnh nhỏ xuống sàn. Nếu nhập lại trước khi giải cửa và rời vùng cắt, trạm sẵn sàng cho lượt cắt tiếp theo. Trong hộp đứng yên, chỉ trọng lượng lưỡi có thể khiến nó nằm trên cơ thể mềm, nên trạm cắt này có motor; joint vẫn giới hạn hành trình trên mặt sàn.

Đưa sinh vật vào trạm cắt, chia cơ thể, chọn từng phần đến nút A/B. Phần bỏ chọn thu gọn và bám bằng lực hữu hạn khi có tiếp xúc. Nó vẫn có thể trượt, bị đẩy hoặc rơi khi mất mặt đỡ. Hai nhóm khác nhau cùng tạo đủ tải trên hai nút trong 0,22 giây làm cửa mở và chốt lại. Đưa các phần qua cửa, nhập lại nếu muốn, rồi đến lỗ tròn.

Selection gắn với một ID hạt vật chất bền vững, không gắn với số nhóm kết nối vốn thay đổi sau cắt/nhập. Khi tách, phần chứa điểm đã chọn tiếp tục nhận điều khiển. Khi nhập, giữ điều khiển trên khối hợp nhất. Khi phần đang chọn thoát hết, chọn phần lớn nhất còn ở trong hộp. Bản thử hỗ trợ nhiều nhóm, các nút HUD hiển thị tối đa bốn phần; có thể chọn trực tiếp hoặc Tab với nhiều phần hơn.

## 03 — Tìm về chủ thể

Lưỡi cắt lệch tâm tạo khối chính lớn hơn. Người chơi luôn điều khiển nhóm có nhiều vật chất chưa thoát nhất. Nếu hai nhóm bằng nhau, giữ chủ thể hiện tại để tránh đổi qua lại; nếu chủ thể không còn hợp lệ, chọn nhóm đầu tiên theo ID vật chất. Màn này không cho người chơi chọn điều khiển phần nhỏ.

Các phần ngoài chủ thể chờ **3 giây mô phỏng** kể từ khi tách khỏi chủ thể, sau đó tự tìm đường về. Đồng hồ không khởi động lại theo từng frame hay mỗi lần tìm đường. Trong thời gian chờ chúng vẫn có trọng lực, va chạm và bám sàn hữu hạn. Khi hợp thể, các hạt trở về nhóm chủ thể và xóa trạng thái chờ.

Một vách ngăn tách hai phía của trạm cắt; một vách ngang phía dưới buộc cả khối đi vòng tới cửa ra. Có vùng chờ rộng để người chơi đứng quan sát phần nhỏ vòng qua đầu vách rồi tiếp xúc, nhập lại. Cửa chỉ chốt mở sau một lần cắt thật và một lần hợp thể hoàn chỉnh, không yêu cầu một follower tự bỏ nhiệm vụ để đứng lâu trên nút.

## Kiến trúc và lực di chuyển

| Thành phần | Trách nhiệm |
| --- | --- |
| `VenomLocomotionProfile` | Tốc độ bò/đi theo, giới hạn gia tốc, lực bám, chờ 3 giây, chu kỳ tìm đường |
| `VenomLocomotion` | Snapshot nhóm theo ID hạt, selection/chủ thể lớn nhất, trạng thái chờ và bám, tác động lực ở tiếp xúc |
| `VenomNavigator` | A* trên lưới mặt sàn, khoảng tránh vách, đường vòng và làm thẳng đoạn đi có kiểm tra vật cản |
| `VenomLevelController` | Chế độ điều khiển, trạm cắt, luật cửa riêng từng màn, pause/reset/thoát/chuyển scene |
| `VenomInput` / `VenomHud` | Joystick, phím điều hướng, chọn phần, dấu chủ thể và thông tin thời gian chờ |
| `VenomLifeAnimation` | Đọc thêm ý định bò để thân/xúc tua hướng theo lực đang cố tạo, kể cả khi bị vách chặn |

Locomotion không ghi transform hoặc gán vận tốc hạt. Vận tốc yêu cầu được đổi thành lực có giới hạn, đặt trên các hạt tiếp xúc mặt đỡ. Lực được chiếu lên tiếp tuyến; mặt đỡ động nhận lực phản ứng ngược lại. Hạt đang bay không nhận lực bò. Khi số hạt có tiếp xúc ít đi, tổng lực bám cũng giảm; hạt sát lỗ thoát nhả lực bò/bám để hỗ trợ thoát và trọng lực kéo phần còn lại qua lỗ.

Thông số ban đầu: bò 0,14 m/s, follower 0,16 m/s; gia tốc cơ thể tối đa 5 m/s², bám 3 m/s²; phản hồi vận tốc 16/s; bù ma sát chủ động 1,3 m/s². Tải lực được phân bổ lên hạt có tiếp xúc, tối đa 2,5 lần phần khối lượng một hạt. Đây là động lực học của sinh vật giả tưởng có thể tự tạo công, không phải chất lỏng thụ động.

A* dùng ô 12,5 mm và khoảng tránh vách 29 mm. Nó xét collider thật của hộp, cửa/lưỡi hiện tại và lỗ sàn, không xét thân các phần như vách tĩnh. Replan mỗi 0,35 giây trong lúc theo; các cạnh chéo và đoạn rút gọn được kiểm tra để tránh cắt góc vách. Đích ở sát vách có thể cần một đoạn tiếp cận bằng khoảng hở cỡ hạt. Không tìm được đường thì phần nhỏ bám/chờ và thử lại, không xuyên vách hay dịch chuyển tức thời. Vật chất vẫn phải đi qua khoảng trống bằng PhysX.

Giới hạn: đây là navigation trên mặt sàn phẳng trong môi trường 3D, chưa hỗ trợ leo tường/trần hoặc đường đi nhiều tầng. Mesh và sợi xúc tua vẫn là biểu diễn hình ảnh, không phải từng cơ riêng có solver lực. Khoảng tránh vách cố định phù hợp với hai phần trong thí nghiệm; chưa có bộ chọn đường tối ưu theo mọi hình dạng biến dạng của khối lớn.

## Build và kiểm chứng

`Gravity Box → Venom → Generate Experiments 02 and 03` tạo hai scene từ scene 01 đã author và thiết lập ba scene trong build. Không cần chạy generator để chơi. `bash Tools/build-venom.sh` build app gồm cả ba màn.

`VenomControlTests` kiểm tra lực bò/dừng, hộp đứng im, không có lực bò khi đang bay, selection, hai nút thực, toàn bộ đường cắt–phối hợp–nhập–thoát, chủ thể lớn nhất, chờ 3 giây/pause/reset, đường vòng quanh vách và chặn bởi cửa đóng. Các bài đường giải điều khiển bằng input lực; không đặt vị trí hạt trong quá trình giải. Test bay chỉ đặt điều kiện ban đầu để kiểm tra không có lực điều khiển giữa không trung.

Kết quả ngày 10/09/2026:

- 19/19 kiểm tra Venom qua trong lần chạy có đồ họa: 12 kiểm tra màn 01 và 7 kiểm tra điều khiển mới. Sau khi bổ sung khả năng quay lại trạm cắt, chạy lại đủ **8/8 kiểm tra điều khiển** thành công; tổng hiện có 20 trường hợp Venom.
- Hai đường giải thực đều cắt thành nhiều nhóm, mở cửa đúng luật, nhập lại và thoát đủ 32/32 hạt. Màn 02 giữ được công tắc khi điều khiển phần khác; màn 03 thực sự đi vòng qua vách.
- Kiểm tra riêng mốc 3 giây: chưa tìm đường ở thời điểm ngay trước 3 giây, bắt đầu sau mốc đó, pause không tiêu hao thời gian. Có thêm kiểm tra chọn chủ thể lớn nhất sau nhiều lần tách và khi chủ thể cũ đã thoát.
- Build macOS gồm đủ ba scene thành công. Đã mở bản native và kiểm tra bố cục; vòng chọn dùng material được tham chiếu để shader không bị loại khi build. Camera màn 02/03 dành khoảng trống cho HUD ở cửa sổ thấp. Sửa lỗi thay đổi danh sách nhóm ngay khi đang vẽ nút A/B bằng cách áp dụng selection sau vòng lặp GUI.
- [Ảnh từ mô phỏng](Images/VenomControls/README.md). XML/log tại `Artifacts/VenomControls-final.*`, `Artifacts/VenomControls-capture.*`, `Artifacts/VenomControls-retry.*`. Feeling trên thiết bị cảm ứng cần được đánh giá riêng; không suy ra từ đường giải tự động.
