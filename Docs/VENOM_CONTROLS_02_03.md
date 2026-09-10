# Venom 02–03 — Điều khiển cơ thể trong hộp đứng yên

Hai thí nghiệm độc lập trên nhánh `Venom`, bên cạnh màn 01 nghiêng hộp. Cả hai giữ hộp nằm ngang, trọng lực thế giới 9,81 m/s², 32 hạt vật lý/96 g, lỗ tròn xuyên sàn và luật thoát đủ 100% vật chất. Không đổi nội dung 23 bàn bi thép.

## Chọn màn và điều khiển

App macOS `Builds/Venom/macOS/Venom.app` có ba nút chọn màn ở đầu màn hình; phím **1/2/3** tương ứng. Có thể mở riêng `Venom02.unity` hoặc `Venom03.unity` trong Unity. Game View portrait 9:16.

- Chạm/click một phần để chọn ở màn 02; cũng có thể bấm nút A/B dưới hộp hoặc **Tab** để chuyển phần.
- Giữ–kéo trong vùng hộp: joystick tương đối với điểm bắt đầu. Kéo xa tăng tốc độ yêu cầu, vẫn có gia tốc và quán tính. Trở về vùng chết hoặc thả tay để dừng bò và bám nhẹ. **WASD/mũi tên** điều khiển theo mặt phẳng sàn nhìn từ camera.
- Vòng xanh và nhãn `ĐANG CHỌN`/`CHỦ THỂ` chỉ phần nhận input. Nhãn phần còn lại cho biết đang bám, thời gian chờ, đang tìm về hoặc chờ lối mở.
- **R** reset toàn bộ, **P/Esc** pause. Pause dừng cả vật lý lẫn đồng hồ 3 giây. Reset xóa input, selection, đường đi và thời gian chờ.

## Camera theo cách điều khiển

- **Màn 02:** camera orthographic nhìn xuống **88° so với mặt sàn**, thẳng theo trục hộp. Hai công tắc và hai nửa hộp dễ so sánh, thuận tiện chạm chọn phần và căn vị trí. Góc gần thẳng đứng giúp thanh cửa đang nâng không che phần đứng trên công tắc.
- **Màn 03:** camera orthographic **góc nhìn 3/4**, nhìn xuống **45°** và nhìn chéo **45°** từ góc trái đầu xuất phát. Sinh vật bắt đầu ở gần người chơi và tiến về lỗ ở phía trên màn hình. Góc này bộc lộ phần đầu kéo, mô phía sau, độ cao của vách và hình dạng lúc nhập lại. Khung hình chứa cả hộp để người chơi theo dõi chủ thể lẫn phần nhỏ đang vòng về.
- Góc và khung bao cơ cấu cố định trong từng màn. Cắt/đổi phần/nhập lại không làm camera xoay hoặc zoom; màn 01 giữ góc quan sát cũ. Camera tính lại tỷ lệ theo kích thước cửa sổ để tránh HUD.
- Ánh xạ giữ–kéo bù độ co ngắn của mặt sàn khi nhìn chéo: hướng di chuyển chiếu lên màn hình khớp hướng ngón tay. Độ lớn joystick vẫn điều khiển tốc độ yêu cầu như trước.

`VenomCameraFraming` quản lý góc, khung hình và đổi hướng màn hình sang mặt sàn. Menu **Gravity Box → Venom → Update Control Cameras** chỉ cập nhật camera trong hai scene đã author, không tạo lại cơ quan.

## 02 — Hai phần, một kế hoạch

Lưỡi chém được giữ cao cho đến khi khối vào vùng cắt, sau đó hạ bằng lực motor, rồi nâng lại sau nhịp cắt 1,2 giây để không ghì mảnh nhỏ xuống sàn. Nếu nhập lại trước khi giải cửa và rời vùng cắt, trạm sẵn sàng cho lượt cắt tiếp theo. Trong hộp đứng yên, chỉ trọng lượng lưỡi có thể khiến nó nằm trên cơ thể mềm, nên trạm cắt này có motor; joint vẫn giới hạn hành trình trên mặt sàn.

Đưa sinh vật vào trạm cắt, chia cơ thể, chọn từng phần đến nút A/B. Phần bỏ chọn thu gọn và bám bằng lực hữu hạn khi có tiếp xúc. Nó vẫn có thể trượt, bị đẩy hoặc rơi khi mất mặt đỡ. Hai nhóm khác nhau cùng tạo đủ tải trên hai nút trong 0,22 giây làm cửa mở và chốt lại. Đưa các phần qua cửa, nhập lại nếu muốn, rồi đến lỗ tròn.

Selection gắn với một ID hạt vật chất bền vững, không gắn với số nhóm kết nối vốn thay đổi sau cắt/nhập. Khi tách, phần chứa điểm đã chọn tiếp tục nhận điều khiển. Khi nhập, giữ điều khiển trên khối hợp nhất. Khi phần đang chọn thoát hết, chọn phần lớn nhất còn ở trong hộp. Bản thử hỗ trợ nhiều nhóm, các nút HUD hiển thị tối đa bốn phần; có thể chọn trực tiếp hoặc Tab với nhiều phần hơn.

## 03 — Tìm về chủ thể

Lưỡi cắt lệch tâm tạo khối chính lớn hơn. Người chơi luôn điều khiển nhóm có nhiều vật chất chưa thoát nhất. Nếu hai nhóm bằng nhau, giữ chủ thể hiện tại để tránh đổi qua lại; nếu chủ thể không còn hợp lệ, chọn nhóm đầu tiên theo ID vật chất. Màn này không cho người chơi chọn điều khiển phần nhỏ.

Các phần ngoài chủ thể chờ **3 giây mô phỏng** kể từ khi tách khỏi chủ thể, sau đó tự tìm đường về. Đồng hồ không khởi động lại theo từng frame hay mỗi lần tìm đường. Trong thời gian chờ chúng vẫn có trọng lực, va chạm và bám sàn hữu hạn. Khi hợp thể, các hạt trở về nhóm chủ thể và xóa trạng thái chờ.

Một vách ngăn tách hai phía của trạm cắt; một vách ngang phía dưới có đường vòng rộng ở bên phải và khe hẹp 32 mm ở bên trái. Khe hẹp thay thế chỗ nối kín với thành hộp, cho phép thử luồn mô; cửa ra phía sau vẫn đóng cho đến khi giải đúng điều kiện cắt–hợp thể. Có vùng chờ rộng để người chơi đứng quan sát phần nhỏ vòng qua đầu vách rồi tiếp xúc, nhập lại. Cửa chỉ chốt mở sau một lần cắt thật và một lần hợp thể hoàn chỉnh, không yêu cầu một follower tự bỏ nhiệm vụ để đứng lâu trên nút.

## Kiến trúc và lực di chuyển

| Thành phần | Trách nhiệm |
| --- | --- |
| `VenomLocomotionProfile` | Tốc độ bò/đi theo, giới hạn gia tốc, lực bám, chờ 3 giây, chu kỳ tìm đường |
| `VenomLocomotion` | Snapshot nhóm theo ID hạt, selection/chủ thể lớn nhất, trạng thái chờ và bám, tác động lực ở tiếp xúc |
| `VenomSqueeze` | Dò khe ở gần theo hướng giữ, căn mô vào khe và giảm tốc phần đầu để đuôi theo qua |
| `VenomNavigator` | A* trên lưới mặt sàn, khoảng tránh vách, đường vòng và làm thẳng đoạn đi có kiểm tra vật cản |
| `VenomLevelController` | Chế độ điều khiển, trạm cắt, luật cửa riêng từng màn, pause/reset/thoát/chuyển scene |
| `VenomInput` / `VenomHud` | Joystick, phím điều hướng, chọn phần, dấu chủ thể và thông tin thời gian chờ |
| `VenomCameraFraming` | Góc riêng từng màn, khung bao hộp/cơ cấu, bù độ co ngắn hướng giữ–kéo |
| `VenomLifeAnimation` | Đọc thêm ý định bò để thân/xúc tua hướng theo lực đang cố tạo, kể cả khi bị vách chặn |

Locomotion không ghi transform hoặc gán vận tốc hạt. Vận tốc yêu cầu được đổi thành lực có giới hạn, đặt trên các hạt tiếp xúc mặt đỡ. Lực được chiếu lên tiếp tuyến; mặt đỡ động nhận lực phản ứng ngược lại. Hạt đang bay không nhận lực bò. Khi số hạt có tiếp xúc ít đi, tổng lực bám cũng giảm; hạt sát lỗ thoát nhả lực bò/bám để hỗ trợ thoát và trọng lực kéo phần còn lại qua lỗ.

Thông số ban đầu: bò 0,14 m/s, follower 0,16 m/s; gia tốc cơ thể tối đa 5 m/s², bám 3 m/s²; phản hồi vận tốc 16/s; bù ma sát chủ động 1,3 m/s². Tải lực được phân bổ lên hạt có tiếp xúc, tối đa 2,5 lần phần khối lượng một hạt. Đây là động lực học của sinh vật giả tưởng có thể tự tạo công, không phải chất lỏng thụ động.

A* dùng ô 12,5 mm và ưu tiên khoảng tránh vách 29 mm. Nếu không có đường rộng, nó thử lại với bán kính hạt + 2 mm (11 mm), để phần nhỏ có thể tự luồn khe. Nó xét collider thật của hộp, cửa/lưỡi hiện tại và lỗ sàn, không xét thân các phần như vách tĩnh. Replan mỗi 0,35 giây trong lúc theo; các cạnh chéo và đoạn rút gọn được kiểm tra để tránh cắt góc vách. Đích ở sát vách có thể cần một đoạn tiếp cận bằng khoảng hở cỡ hạt. Không tìm được đường thì phần nhỏ bám/chờ và thử lại, không xuyên vách hay dịch chuyển tức thời. Vật chất vẫn phải đi qua khoảng trống bằng PhysX.

Giới hạn: đây là navigation trên mặt sàn phẳng trong môi trường 3D, chưa hỗ trợ leo tường/trần hoặc đường đi nhiều tầng. Mesh và sợi xúc tua vẫn là biểu diễn hình ảnh, không phải từng cơ riêng có solver lực. Navigation thử hai mức khoảng hở; chưa có bộ chọn đường tối ưu theo mọi hình dạng biến dạng hoặc thể tích của khối lớn.

## Luồn khe hẹp

Giữ hướng vào khe gần cơ thể để tự luồn; không có nút chuyển dạng. `VenomSqueeze` dò một làn ngắn theo hướng đang giữ, kiểm tra hai mép đối diện và khoảng hở xuyên qua cả mép trước lẫn mép sau. Nó chỉ hỗ trợ khe ở gần, không tự giải mê cung cho người chơi. Mỗi phần được chọn hoặc đang tự tìm về dùng cùng cơ chế.

- Mô chạm mép đặc trượt ngang vào làn trước khi kéo tới. Các hạt có tiếp xúc sàn nhận lực riêng, giới hạn theo locomotion; hạt đang bay không nhận lực bò.
- Liên kết mềm hơn, thay đổi độ dài nghỉ nhanh hơn khi luồn. Những liên kết dài dư thừa được thay bằng liên kết lân cận; chỉ bỏ liên kết cũ khi còn một đường nối khác, nên việc thu hẹp không tự tính thành một lần chém.
- Tiếp xúc **giữa các hạt mô trong cùng phần** chuyển sang lực áp suất đàn hồi có damping, tránh hiện tượng hạt cứng chen nhau thành vòm bị kẹt. Lực áp suất tác động bằng cặp lực bằng nhau và ngược chiều. Chỉ cặp collider nội bộ này được bỏ tiếp xúc cứng tạm thời; collider với sàn, vách, lưỡi và cửa vẫn giữ nguyên bán kính 9 mm và CCD. Khi mô hết chịu nén, tiếp xúc cứng trở lại sau khi cặp hạt đã tách đủ, tránh bật tung vì bật collider trong trạng thái chồng lấn.
- Làn được giữ đến khi phần đuôi qua mép. Phần đầu giảm tốc để mô phía sau theo kịp nhưng vẫn tiến để chừa chỗ cho mô mới. Khi ra khỏi khe, đặc tính kết dính bình thường trở lại. Thả tay hủy kéo chủ động; đổi hướng có thể rút lại. Reset phục hồi toàn bộ liên kết và tiếp xúc.
- Không tạo/xóa hạt: luôn 32 hạt, tổng 96 g. Mesh đi theo biến dạng thật của các hạt.

Màn 03: vách ngang dài 318 mm, từ x = −218 mm đến +100 mm; thành trái ở −250 mm nên có khe 32 mm. Đường vòng bên phải giữ nguyên. Generator và scene đã author có cùng kích thước. Khe chỉ giúp vượt vách ngang; nó không đi vòng qua cửa khóa ở phía sau.

Thông số trong `Living matter.asset`: `FlowStiffness` = 0,16 lần độ cứng thường, `FlowPlasticity` = 12/s, `TissuePressure` = 8 N/m, `TissueDamping` = 0,06 N·s/m. Các lực bò vẫn bị giới hạn 5 m/s² trước khi phân bổ tải tiếp xúc. Màn 01 nghiêng hộp giữ mô hình trước đó vì không có điều khiển bò chủ động.

Giới hạn mô hình: đây là mô mềm có thể chịu nén, chưa phải SPH bảo toàn thể tích hoặc chất lỏng liên tục. Bán kính collider với môi trường không thu nhỏ: khe hẹp hơn đường kính hạt 18 mm vẫn không thể đi qua; bộ dò còn dành 2 mm dự phòng mỗi bên. Khe đang kiểm chứng cho cả cơ thể là 32 mm. Muốn chảy qua vết nứt vài mm cần tăng độ phân giải vật chất hoặc thay solver, không tắt va chạm với vách.

## Build và kiểm chứng

`Gravity Box → Venom → Generate Experiments 02 and 03` tạo hai scene từ scene 01 đã author và thiết lập ba scene trong build. Không cần chạy generator để chơi. `bash Tools/build-venom.sh` build app gồm cả ba màn.

`VenomControlTests` kiểm tra lực bò/dừng, hộp đứng im, không có lực bò khi đang bay, selection, hai nút thực, toàn bộ đường cắt–phối hợp–nhập–thoát, chủ thể lớn nhất, chờ 3 giây/pause/reset, đường vòng quanh vách và chặn bởi cửa đóng. Các bài đường giải điều khiển bằng input lực; không đặt vị trí hạt trong quá trình giải. Test bay chỉ đặt điều kiện ban đầu để kiểm tra không có lực điều khiển giữa không trung.

Kết quả ngày 10/09/2026:

- 19/19 kiểm tra Venom qua trong lần chạy có đồ họa: 12 kiểm tra màn 01 và 7 kiểm tra điều khiển mới. Sau khi bổ sung khả năng quay lại trạm cắt, chạy lại đủ **8/8 kiểm tra điều khiển** thành công; tổng ở thời điểm đó là 20 trường hợp Venom.
- Hai đường giải thực đều cắt thành nhiều nhóm, mở cửa đúng luật, nhập lại và thoát đủ 32/32 hạt. Màn 02 giữ được công tắc khi điều khiển phần khác; màn 03 thực sự đi vòng qua vách.
- Kiểm tra riêng mốc 3 giây: chưa tìm đường ở thời điểm ngay trước 3 giây, bắt đầu sau mốc đó, pause không tiêu hao thời gian. Có thêm kiểm tra chọn chủ thể lớn nhất sau nhiều lần tách và khi chủ thể cũ đã thoát.
- Build macOS gồm đủ ba scene thành công. Đã mở bản native và kiểm tra bố cục; vòng chọn dùng material được tham chiếu để shader không bị loại khi build. Camera màn 02/03 dành khoảng trống cho HUD ở cửa sổ thấp. Sửa lỗi thay đổi danh sách nhóm ngay khi đang vẽ nút A/B bằng cách áp dụng selection sau vòng lặp GUI.
- [Ảnh từ mô phỏng](Images/VenomControls/README.md). XML/log tại `Artifacts/VenomControls-final.*`, `Artifacts/VenomControls-capture.*`, `Artifacts/VenomControls-retry.*`. Feeling trên thiết bị cảm ứng cần được đánh giá riêng; không suy ra từ đường giải tự động.

### Kiểm tra góc camera riêng cho hai màn

Đã chạy 8/8 kiểm tra điều khiển với camera có đồ họa, sau đó chạy lại riêng cả hai đường giải đầy đủ ở góc trước lần thử 3/4 (88° và 58°/12°): 2/2 qua, đủ 32/32 hạt thoát mỗi màn. Đã xem ảnh khi hai nút cùng được giữ, phần nhỏ tìm về và nhập lại để chọn góc ít bị thanh cửa/lưỡi chém che. Log/XML/capture tại `Artifacts/VenomCamera`. Camera cố định, giữ cả hộp và các phần trong cùng khung hình. Bản macOS đã build lại thành công với hai góc này.

### Màn 03: thử góc nhìn 3/4

Theo yêu cầu tiếp theo, đổi riêng màn 03 sang camera 3/4 từ góc trái đầu xuất phát: cao 45°, chéo 45° (rotation Unity 45°, 135°, 0°). Góc này cho thấy mặt sàn và độ sâu hai thành hộp, đồng thời đưa lưỡi chém sang bên trong ảnh quan sát cảnh hợp thể. Đã xem ảnh tìm về/nhập lại và chạy lại đường giải đầy đủ: 1/1 qua, đủ 32/32 hạt thoát. Capture/XML/log tại `Artifacts/VenomCamera/ThreeQuarter` và `Artifacts/VenomCamera/three-quarter.*`.

### Kiểm chứng luồn khe — 10/09/2026

**25/25 trường hợp qua**, trong hai lượt có đồ họa: 12 kiểm tra điều khiển (`Artifacts/VenomSqueeze/controls-final.xml`), rồi 12 kiểm tra màn 01 và 1 bài follower qua khe (`Artifacts/VenomSqueeze/regression.xml`).

- Cắt bằng lưỡi thật, nhập lại, mở cửa, luồn đủ 32/32 hạt qua khe 32 mm trong khoảng **3,37 giây** ở lượt thử, rồi thoát đủ qua lỗ. Test theo dõi từng hạt khi cắt qua mặt phẳng khe và vị trí so với sàn; không đặt vị trí hạt trong đường giải.
- Thả tay lúc đang luồn: dừng kéo và hạ trạng thái mềm; đổi hướng rút được thân ra, reset khôi phục tiếp xúc nội bộ.
- Khe 12 mm vẫn chặn các hạt đường kính 18 mm. Cửa khóa vẫn chặn cả navigation với khoảng hở nhỏ.
- Khi đóng riêng đường vòng rộng trong bài thử, follower tìm đường qua khe, biến dạng và tiếp xúc nhập lại với chủ thể. Bài này thiết lập hai nhóm ban đầu ở hai phía để kiểm tra độc lập; sau đó chỉ mô phỏng bằng lực và đường tìm tự động.
- Toàn bộ kiểm tra cũ về sàn, lật hộp, chém, hợp thể, giữ công tắc, chờ 3 giây và thoát vẫn qua.

[Ảnh trước/trong/sau khe](Images/VenomControls/README.md). Feeling với thao tác tay cần thử trong app; các bài tự động không đánh giá được cảm giác điều khiển.

Bản macOS đã build thành công với cơ chế luồn khe; đóng app đang chạy rồi mở lại `Builds/Venom/macOS/Venom.app` để thử. Không xuất APK trong lượt cập nhật này.
