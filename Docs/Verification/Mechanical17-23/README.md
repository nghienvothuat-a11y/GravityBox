# Kiểm chứng bảy màn cơ khí 17–23

Unity 6000.3.19f1, macOS, mô phỏng 120 Hz. Ngày 09/09/2026.

## Bộ kiểm thử cuối

| Bộ kiểm thử | Kết quả | Thời gian UTC |
| --- | --- | --- |
| EditMode | 8/8 passed | 07:19:59 |
| PlayMode toàn bộ catalog 23 màn | 121/121 passed | 07:20:26–07:21:43 |
| Boss focused trước regression | 2/2 passed | 07:18:40–07:18:42 |

XML gốc được giữ trong thư mục này. Tổng cộng 129 kiểm thử trong lượt regression cuối, không có failed/skipped. 17 bài mới kiểm tra cơ cấu và đường giải của 17–23. Các kiểm tra dùng chung bao quát spawn, sàn/nắp/vỏ, cửa thoát, mọi viên bi, force/reset/unload, 100 reset mỗi màn, chọn màn cuối bằng input thật và vòng scene đủ 23 màn.

## Đường giải và các điểm kiểm chứng

- **17:** từ spawn vào chỗ trú, gập cầu bằng gravity, contact với ngàm rồi qua cầu tới thoát; chốt giữ ổn định khi đảo hộp.
- **18:** hai bi cùng khối lượng tạo mô-men khác nhau; A nâng B, chốt giữ kết quả, cả hai rời máng và thoát. Không điều khiển bi riêng.
- **19:** lồng treo quay độc lập, chịu giới hạn bản lề; xoay vỏ qua chặn để nghiêng sàn lồng, bi ra qua miệng và tới lỗ cuối.
- **20:** con lắc kín khe khi đóng, mở do gravity theo hướng nghiêng; đường từ spawn qua khe rồi thoát.
- **21:** máng sinh vận tốc bằng gravity/contact, bi có khoảng bay không hỗ trợ, xoay bộ hứng để đón rồi thoát. Bài riêng xác nhận cứu bi hụt, đưa qua mặt trong nắp về cốc xuất phát.
- **22:** ba lần ball–rack–finger–cam tiếp xúc đưa cam đến khoảng 31,47° → 61,50° → 91,50°. Rack phải hồi mới nhả nấc tiếp theo. Vòng chắn ngăn lối tắt qua khe ở nấc đầu; cuối cùng bi xuyên cam rồi thoát.
- **23:** B thực sự nhấn plunger để A được nhả. Từng bi được ghi nhận vào lồng, cắt mặt miệng trong tiết diện hợp lệ, thoát toàn bộ bán kính, bay không có contact rồi thoát khỏi cầu. A có thể đi trước trong khi B còn đang chuyển vào lồng; thắng vẫn đợi đủ hai bi.

Route fixture sau Load chỉ gửi rotation intent qua controller chung. Các fixture đo từng bộ phận được phép đặt điều kiện đầu để cô lập hợp đồng; không coi đó là lời giải từ spawn. Một route thành công chứng minh có đường vật lý, chưa chứng minh người dùng thấy dễ điều khiển hoặc màn đạt mức khó mục tiêu.

## Hình ảnh

`Level17.png` tới `Level23.png` và các bản `-top` là render hình học ban đầu từ prefab đã generate, không phải ảnh replay hay bằng chứng giải màn bằng tay. Kiểm tra trực quan xác nhận nhìn thấy bi/cơ cấu, kính không còn glare trắng chồng lớp, coil không còn đoạn kéo ra ngoài hộp và toàn bộ vỏ nằm trong khung hình.

## Giới hạn mô hình

Cóc giữ/chốt là ràng buộc cơ khí lý tưởng; không mô phỏng biến dạng hoặc ma sát răng rất nhỏ. Chúng chỉ giữ góc đã đạt. Gravity, tải tiếp xúc, bearing damping và lò xo tạo chuyển động. Hỗ trợ 4 cm chỉ ở lỗ cuối, theo yêu cầu game. Màn 20 cho phép giữ con lắc mở bằng tư thế; khoảng thả giữa module và lồng ở 23 có thể cho lời giải tắt vật lý. Giữ các lời giải hợp lệ đó trong bản thử này.


## macOS player

Build thành công, 330.623.858 bytes theo BuildReport. Đã mở player lúc 07:23 UTC, chọn màn 23 qua danh sách, kiểm tra HUD OUT 0/2 và reset bằng R; sau đó chọn/reset màn 17 để người dùng thử. `Mac-Level23.png` và `Mac-Level17.png` được lưu bằng F12 trong player, không phải ảnh Editor. Player.log trong lượt này không có exception/error. Không build APK.

Thao tác kéo qua công cụ CUA ghi nhận press nhưng tổng quãng kéo bằng 0; vì vậy lượt native này chỉ xác nhận chọn màn, render và reset, không được ghi là nghiệm thu drag hoặc giải bằng tay. Input chuột/touch và chuyển động xoay vẫn được bao phủ bởi PlayMode regression; cảm giác tay cần người dùng thử.

Sau build, hoàn nguyên các thay đổi ID và trường mảng rỗng mặc định trong 15 prefab cũ cùng scene: so sánh graph tham chiếu và nội dung serialized xác nhận tương đương. Hoàn nguyên thay đổi importer ở vật liệu kính cũ và Graphics/QualitySettings. Không thay đổi geometry/cơ cấu đã được kiểm tra của bảy màn mới.
