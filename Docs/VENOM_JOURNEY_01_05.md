# Venom Journey — Năm bài đầu để chơi thử

Ngày: 10/09/2026. Campaign thử nghiệm riêng, sử dụng một cách điều khiển xuyên suốt: chạm để hướng dẫn, kéo để xoay hộp. Các scene thử nghiệm điều khiển cũ được giữ lại.

## Chơi trên macOS

Mở `Builds/Venom/macOS/Venom.app`. Phím 1–5 hoặc hàng nút đầu màn chọn bài. R thử lại; P/Esc tạm dừng; Z phóng gần và theo sinh vật. Sau khi toàn bộ 32 hạt thoát thật qua lỗ, camera tiến gần và theo sinh vật ăn mừng 4,8 giây rồi sang bài tiếp theo. Bài 5 kết thúc chuỗi thử nghiệm.

- Chạm một điểm nhìn thấy trên mặt trong: sinh vật bò đến rồi chờ.
- Chạm nút A/B: giao việc tới giữ nút, không cần giữ ngón tay.
- Chạm sinh vật hoặc nút khối lượng phía dưới: chọn phần đó. Chọn phần khác không hủy việc đang giữ.
- Kéo một ngón/chuột trái để xoay; hai ngón hoặc chuột phải cũng xoay. Thao tác xoay không phát thêm lệnh đi lúc nhả.
- Chạm đích mới: thay nhiệm vụ của phần đang chọn.
- Gọi về hợp thể: các phần rời nhiệm vụ, tìm về vùng gặp nhau trên sàn; có thể làm cơ quan đóng lại nếu chưa chốt.
- Cùng ra ngoài: chỉ xuất hiện khi lỗ đã mở; giao đích thoát cho mọi phần. Không giải cơ quan thay người chơi.

## Nội dung và đường giải

| Bài | Thử thách | Đường giải để đối chiếu |
| --- | --- | --- |
| 01 — Chào bạn nhỏ | Làm quen với phản hồi và điểm đến | Chạm một điểm sàn; đưa sinh vật tới lỗ ở giữa sàn |
| 02 — Một thế giới sáu mặt | Đọc không gian và đổi mặt bám | Chỉ điểm trên tường, xoay hộp để nhìn trần, chỉ lỗ giữa trần |
| 03 — Giữ thêm một chút | Một nhiệm vụ tồn tại sau khi thả tay | Chạm A, giữ đủ 1,8 giây để chốt mở cửa; sau đó chỉ lỗ. Rời sớm thì tiến độ giảm về 0 |
| 04 — Chờ nhau qua cửa | Tách, giữ và phối hợp hai phần | Chạm dao. Chọn một phần tới A; A nâng nắp che B. Chọn phần còn lại tới B. Hai phần giữ đồng thời 0,65 giây để chốt cửa. Dẫn tất cả ra |
| 05 — Mỗi phần một nhiệm vụ | Phân bổ khối lượng và thử lại một giả thuyết | Cắt giữa thường để lại hai phần chưa đủ 60 g cho B. Gọi về hợp thể, đổi vị trí dao sang cắt lệch rồi chạm dao. Phần nhẹ giữ A cần 24 g; phần nặng giữ B cần 60 g |

Kết quả cắt có thể lệch vài hạt vì cơ thể biến dạng thật. Đọc khối lượng mỗi phần trên HUD thay vì giả định luôn đúng 50/50. Sinh vật căn một mặt cắt của cơ thể với lưỡi dao; không sinh ra hai khối cầu thay thế hay dịch chuyển hạt tới hai vị trí đặt sẵn. Hai phần bò tách ra sau khi liên kết thực sự bị cắt.

Ở bài 04–05, đến B khi nắp chưa mở chỉ nhận phản hồi bị chặn. A phải tiếp tục có đủ vật chất khi B được kích hoạt để chốt cửa cuối. Nắp có bảo vệ khi còn cơ thể nằm bên dưới, tránh ép xuyên sàn; điều này không thay điều kiện giữ A để giải puzzle.

Máy chém dùng phần sống dao có collider để cơ cấu vẫn có khối lượng hình học rõ ràng; đoạn mép sắc phía dưới cắt liên kết vật chất mà không hoạt động như một khối tù ép hạt vào sàn. Khi chu trình chém bắt đầu, xoay hộp được khóa trong 1,1 giây rồi tự mở lại. Việc này loại bỏ xung lực ngang giả khi một Rigidbody kinematic vừa chém vừa quay cùng hộp. Kiểm tra hồi quy chạy ba hướng xoay khác nhau, yêu cầu đủ hai phần, phần nhỏ nhất ít nhất 10/32 hạt và tâm của mọi hạt giữ trong mặt va chạm của hộp.

## Trí nhớ và phản ứng

Các sự kiện tới đích, bò sang mặt khác, giữ đủ thời gian, chia và phối hợp thành công ghi nhận kỹ năng. Tiến trình được lưu trên thiết bị qua các lần mở app; retry giữ kiến thức nhưng xóa nhiệm vụ và trạng thái cơ quan.

Nhận lệnh làm sinh vật hướng phần đầu/xúc tu về mục tiêu. Đến nơi hoặc cần giúp thì nhìn lại người chơi. Khi giữ nút hay chờ dao, động tác nhảy múa idle được ngắt để biểu đạt đang tập trung. Mở được cửa thì chuyển chú ý về lối ra. Những lần hoàn thành trước được nhận ra khi bắt đầu màn khác.

Đây là bước đầu của hành vi học có trạng thái: kỹ năng và phản hồi theo sự kiện, chưa có hệ học máy tự suy luận cơ quan tùy ý hay đầy đủ thư viện cảm xúc của thiết kế dài hạn. Phần đang giữ việc không tự nhập hoặc tự đi theo phần khác sau 3 giây.

## Ăn mừng khi thoát

Khi đủ 100% vật chất đã qua lỗ, camera chuyển liên tục từ góc đang chơi sang cận cảnh trong khoảng 1 giây. Hộp, kính, cơ quan, viền chỉ dẫn và HUD puzzle được ẩn để chỉ còn sinh vật trên nền tối. Mỗi lần thắng chọn ngẫu nhiên một trong ba animation: **vẫy chào**, **nhảy bật và vỗ xúc tu trên đầu**, hoặc **xoay thân rồi bung xúc tu như pháo hoa**. Hệ thống tránh lặp lại đúng animation của lần thắng liền trước. Các phần còn tách được lấy chung vào khung hình và có thể nhập lại sau khi thoát.

`VenomCelebration` quản lý camera, thời gian và trạng thái hiển thị; `VenomCelebrationHud` giữ thông báo ngắn ở rìa màn hình. `VenomLifeAnimation` biến dạng skin cho điệu nhảy, không thay vị trí hạt hoặc tạo vật chất mới. Lực gom sẵn có ở cửa thoát tiếp tục giữ mô động lực học ngoài hộp cho đoạn ăn mừng; đây là hỗ trợ trình diễn sau chiến thắng. Collider vẫn hoạt động, không đổi điều kiện thắng.

Tạm dừng dừng cả animation và thời gian chuyển màn; R khôi phục hộp/cơ quan và camera chơi. Z không ghi đè camera ăn mừng. Cơ chế này dùng chung cho Journey và các thí nghiệm Venom cũ; chỉ Journey 01–04 tự chuyển tiếp, bài 05 giữ đoạn ăn mừng để xem hoặc thử lại.

## Cơ quan và vật lý

- Giữ 32 hạt động lực học, mỗi hạt 3 g, tổng 96 g. Lực bám và di chuyển tác dụng lên tiếp xúc bề mặt, mô mềm tiếp tục dùng liên kết có độ nhớt/dẻo.
- Nút phải có tiếp xúc vật lý thật. Khối lượng kích hoạt tính từ mô cùng một phần đang thực sự nằm trên vùng nút, sau khi xác nhận tiếp xúc; không cộng khối lượng phần ở xa chỉ vì một đầu xúc tu chạm tới.
- Đây là cảm biến tải gameplay dựa vào tiếp xúc và lượng mô trên vùng nút, không phải cân đo lực pháp tuyến chính xác. Cách này giữ luật phân bổ rõ ràng khi người chơi xoay hộp.
- Dao và nắp là cơ cấu chủ động điều khiển bằng chuyển động Rigidbody kinematic có collider; chúng không được mô tả là vật rơi tự do. Sinh vật vẫn dùng Rigidbody động lực học.
- Tìm đường tránh các vùng cơ quan chưa được giao. Các bề mặt sàn/trần có đường vòng quanh dao, nút và nắp; đường bò qua các mặt dựa trên hộp sáu mặt. Chưa phải bộ tìm đường cho mê cung 3D bất kỳ.
- Chỉ thắng khi tất cả vật chất đi qua lỗ thật. Chạm mục tiêu hoặc phát xong animation không được tính là thoát.

## Mã nguồn và cách dựng

`VenomJourney` quản lý nhiệm vụ gắn với hạt đại diện ổn định, đọc tải nút và vận hành các bài học. `VenomJourneyRoute` tìm đường sàn có vùng tương tác cần tránh. `VenomJourneyProgress` lưu kỹ năng và các bài đã hoàn thành. `VenomJourneyHud` hiển thị mục tiêu, phần đang chọn, tải và phản hồi.

`VenomGuidance` chuyển các scene Journey sang bộ hướng dẫn mới; các scene thử nghiệm cũ giữ logic trước đó. `VenomLifeAnimation` đọc sự chú ý và nhiệm vụ để biểu diễn, không ghi đè vị trí vật lý. Khi bò qua góc, lực kéo của tiếp xúc phía trước tính tới phần cơ thể còn ở mặt cũ.

Unity menu: **Gravity Box → Venom → Generate Guided Journey 01–05**. Scene mới: `Assets/_Game/Venom/VenomJourney01.unity` đến `VenomJourney05.unity`.

Build mặc định: `bash Tools/build-venom.sh`. Muốn build lại bộ thí nghiệm điều khiển cũ: `bash Tools/build-venom.sh --lab`. Cả hai ghi cùng đường dẫn app; lệnh cuối cùng quyết định nội dung app đó.

Kiểm chứng tự động: `GravityBox.Tests.VenomJourneyTests`. Kết quả và ảnh kiểm chứng được ghi dưới `Artifacts/Venom01` và `Artifacts/VenomJourney`. Báo cáo hoàn tất trong README sẽ ghi kết quả của lần chạy cuối, thay vì xem danh sách test là bằng chứng chúng đã đạt.

## Kết quả kiểm chứng

Lần chạy cuối đạt **67/67 PlayMode Venom**, gồm **13 trường hợp Journey**: đường giải đủ năm bài; giữ/rời nút, thay lệnh, lựa chọn phần, xoay khi đang giữ; chia giữa chưa đủ tải → hợp thể → chia lệch → giải bài 5; lưu kỹ năng qua scene mà không phát lại lệnh cũ; chuột kéo nhanh rồi trả con trỏ về chỗ cũ; cảm ứng kết thúc một lần vuốt ở vị trí mới. Máy chém màn 4 còn được kiểm tra ở ba hướng xoay, khóa xoay xuyên cả thao tác pause/resume, giữ mọi tâm hạt trong mặt va chạm và không tạo phần nhỏ hơn 10/32 hạt. Đuôi cơ thể vẫn giữ lệnh thoát khi hạt đại diện đã đi qua lỗ.

XML: `Artifacts/Venom01/celebration-full-tests.xml`; log cùng tên. Kiểm tra Journey cuối sau khi bổ sung chặn chuyển scene lúc tạm dừng: `Artifacts/Venom01/celebration-final-journey-tests.xml`. Ảnh đoạn thắng tại lỗ sàn, lỗ trần xoay nghiêng và bài phối hợp: `Artifacts/VenomCelebration/victory-*.png`.

Kiểm tra tổng thể sau khi thêm ba animation: `Artifacts/Venom01/three-celebrations-full-tests.xml`, **66/66 đạt**. Ba ảnh cùng thời điểm để so sánh silhouette nằm tại `Artifacts/VenomCelebrationVariants/victory-variant-1.png` đến `victory-variant-3.png`.

Kiểm tra tổng thể sau sửa máy chém: `Artifacts/Venom01/knife-fix-full-tests.xml`, **67/67 đạt**. Kiểm tra đồ họa riêng và ảnh hai phần còn trong hộp: `Artifacts/Venom01/knife-fix-visual.xml` và `Artifacts/VenomKnifeFix/04-contained-after-knife.png`.

Kiểm tra đoạn thắng bao gồm: phần đầu qua lỗ chưa được kích hoạt ăn mừng; đủ 32/32 mới bắt đầu; camera cận cảnh giữ mọi phần trong khung; cả ba biến thể có tên và pose riêng, skin vẫy ít nhất bốn xúc tu mà không sửa vị trí vật lý; kính/cơ quan/marker bị ẩn nhưng collider giữ nguyên; pause giữ nhịp và hoãn chuyển scene kể cả ở nhịp cuối; retry trả lại cả renderer đã bị ẩn sẵn trước đó và camera; không chuyển scene ở mốc 3 giây cũ, sang bài mới sau 4,8 giây và không mang trạng thái ẩn sang bài sau.

Bản macOS cập nhật ăn mừng đã được kiểm tra trực tiếp: chạm lỗ bài 1, camera tiến gần, toàn bộ kính/cơ quan biến mất, thân và bốn xúc tu đổi nhịp nhảy; tạm dừng/tiếp tục giữa đoạn thắng rồi tự sang bài 2 với hộp hiện đầy đủ. Kết quả kiểm tra Journey cuối: **12/12 đạt**.

Đã chơi trực tiếp bản macOS trước đó: chạm một điểm ở bài 1, chạm lỗ và tự sang bài 2; ở bài 4 chạm lưỡi dao đang nâng, chọn từng phần, giữ A mở B, chốt cửa rồi cả hai thoát và tự sang bài 5. Bản sửa đầu vào xử lý từng sự kiện theo thứ tự để tránh gom một lần kéo nhanh thành một lần chạm.

Giới hạn QA thao tác kéo cửa sổ Mac: công cụ UI tự động đã gửi sự kiện nhấn và nhả cùng ở điểm cuối, không có sự kiện giữ–di chuyển. Trace F9 trong Development Build xác nhận điều này; không coi lần diễn lại đó là kiểm chứng vuốt tay trên thiết bị. Các test thiết bị Input System đã kiểm tra chuỗi nhấn → di chuyển → nhả, cả trường hợp toàn bộ chuỗi nằm giữa hai frame và cảm ứng đổi vị trí ở frame nhả. Cần playtest vuốt tay trên máy/điện thoại để đánh giá feeling.
