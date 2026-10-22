# Màn 26 — đường qua trần và phục hồi thang (22/10/2026)

Màn hiển thị **26** là G3, `coghe.spatial.plus.g3`, “Qua trần nhà”. Người chơi
báo rơi khi chạm sang phía bên kia **sau khi thang đã dừng trên**, rồi không lên
lại được. Giữ nguyên bố cục, vùng trơn, lực bám, ID và tiến trình lưu.

## Nguyên nhân tái hiện được

- Chạm sàn phòng phải dùng đường ưu tiên bề mặt bám: thang → kính sau → trần →
  mặt dưới tấm trượt → phòng phải. Chạm thẳng lỗ thoát lại bỏ ưu tiên này, tìm
  đường ngắn qua vách trơn. COghe rời mép thang, mất bám và không vượt được vách.
- Khi thang ở trên, chạm nút thang từ sàn chỉ tạo lệnh leo lên khay đang ngoài
  tầm với. Màn này chưa bật khả năng gọi khay trống về bến dưới.

Hai ca lỗi đều thất bại trước khi sửa; chạm sàn và chạm cao trên kính đã chạy
được. Dấu vết trước sửa: `Artifacts/Ceiling26/before/` và
`Artifacts/Tests/ceiling26-before.xml` (2 đạt / 2 lỗi được tái hiện).

## Sửa

1. Thêm cấu hình `VenomCampaignDefinition.PreferGripForExit`, mặc định tắt.
   Chỉ G3 bật: lệnh chạm lỗ thoát dùng cùng chi phí chọn đường bám như lệnh đi
   thông thường. Lệnh vẫn có cờ Exit, giữ đích lỗ và tái lập đường sau va chạm.
   Không thay lực bám, bỏ trọng lực hay cho bám vật liệu trơn. Các bài cố ý dạy
   trượt khi chạm thẳng lỗ giữ hành vi cũ.
2. Bật khả năng đã có `EmptyTrayCallFromLowerLanding` cho riêng thang G3. Sau
   khi rơi về bến dưới, chạm nút trên khay trống gọi khay xuống. Chiều lên vẫn
   phải đưa toàn bộ COghe lên khay; rời thang giữa chuyến vẫn dừng thang. Khóa
   cấp nguồn B vẫn có tác dụng. Không bật điều khiển từ xa cho thang Boss 10.
3. Cập nhật cả builder, asset định nghĩa và scene lưu sẵn. Không dựng lại hoặc
   làm thay đổi hình học, màu sắc và mạch điện của các màn khác.

Chỉ bật cấm leo vách trơn là chưa đủ: đường thoát vẫn có thể rời trần sớm,
đi xuống dải trơn ở phía trái vách. Thử nghiệm đó không được giữ trong bản sửa.

## Kiểm chứng

- Nhóm chuyên biệt: **12/12 đạt**, 0 lỗi, 145 giây
  (`Artifacts/Tests/ceiling26-regression.xml`).
- Lệnh chạm thẳng lỗ sau khi thang dừng, ở nhịp 60/30 khung; kiểm tra cơ thể
  thực sự vượt qua phía dưới tấm trần và thoát đủ 32 hạt.
- Lệnh chạm sàn và chạm cao bên kia; rơi về sàn → gọi thang → đi lên lại → thắng;
  đổi lệnh giữa chuyến lên → thang dừng → gọi xuống → đi lên và giải trọn màn.
- Các bài G3 đã có: Solve, Wander, NoShortcut, lời giải mẫu ở 60/30 khung.
- Clip `ceiling26-exit.mp4`: camera Unity thật, từ thang đã dừng đến thắng,
  điều khiển bằng tọa độ chạm qua camera; không teleport hoặc ép trạng thái thắng.
- Hồi quy toàn bộ: đã khởi chạy nhưng bị ngắt khi tác vụ bị dừng; Unity đã thoát
  trước khi ghi XML cuối. Không tính lượt này là đạt. Log còn tại
  `Artifacts/Tests/ceiling26-full-regression.log`. Bộ 989 test đạt của bản Clever
  trước đó không chứng nhận phần thay đổi đường đi mới ở màn 26.

Các lượt 30/60 ở đây là nhịp mô phỏng và cập nhật khung, **không phải số đo FPS
trên iPhone**. Clip ghép từ 15 ảnh/giây để xem đường di chuyển. Thử nghiệm tự động
không thay thế cảm giác điều khiển khi người chơi test trên máy thật.

Chạy lại:

```sh
bash Tools/run-coghe-tests.sh ceiling26-regression 'Ceiling26|GlassCrawlG3|SpatialPlusG3'
bash Tools/run-coghe-tests.sh ceiling26-full-regression
```

[Xem clip và giải thích](review.html). Manifest trạng thái source tại lúc chạy:
`Artifacts/Ceiling26/source-manifest.json`.

## Bản iPhone

Đã build thành công Release ARM64, đủ 67 scene, ký bằng team đang dùng. Build
từ bản sao riêng được đối chiếu hash với source kiểm thử để không làm gián đoạn
bộ hồi quy. App tại `Builds/SpatialLab/iOS-Ceiling26/COghe.app`; log và bản sao
preferences trước cài ở `Artifacts/Ceiling26/ios/`.

Đã cài thành công lên iPhone 13 Pro Max. Toàn bộ 34 khóa preferences trước và
sau cài trùng khớp; không gỡ ứng dụng hoặc xóa tiến trình. Sau khi người dùng mở
khóa, lệnh mở ứng dụng thành công. Chưa đo FPS hoặc chạy lời giải tự động trên
iPhone; kết quả lời giải ở trên là từ Unity PlayMode.
