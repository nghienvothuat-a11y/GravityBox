# COghe — Mở đầu và chạm cơ quan

Revision 15/10/2026, working tree NewGraphic trên `627298b7`. Giữ các thay đổi
nhịp 65 màn/VFX đã làm trước đó; đây là phần bổ sung theo yêu cầu mới.

## Hành vi

- Lần mở đầu: intro tự chạy, kết thúc vào màn 1. Skip vào màn 1 ngay; thao tác Skip
  không lọt xuống mặt kính. Xem hết vẫn giữ dissolve cuối clip.
- Lần mở sau giữ Main Menu/Play tiếp tiến độ. Save cũ có màn đã qua nhưng thiếu
  cờ intro không bị coi là người chơi mới. Xem lại Intro từ menu vẫn trở về menu.
- Hướng dẫn đầu màn fade in 0,4 giây. Bắt đầu chạm/kéo/pinch trong vùng chơi hoặc
  dùng điều khiển camera sẽ fade out 0,3 giây; chính thao tác đó vẫn có hiệu lực.
  Đóng, hết thời gian và Pause cũng fade out. Đổi kích thước không phát lại fade.
  Hướng dẫn vẫn mở lại được từ Hint. Mũi tên gameplay giữ cơ chế riêng.
- **50 công tắc trong 33 scene hiện hành** nhận cả vỏ vật lý màu ngà lẫn tay nắm
  màu. COghe tự đến vị trí đứng và thực hiện đúng tác vụ đẩy/kéo hiện có.
  [Danh sách](control-targets.json). Chạm vào công tắc còn khóa vẫn bị từ chối;
  vật cản đặc che phía trước vẫn chặn chạm. Điểm trên kính cạnh đó vẫn là lệnh đi.
- Cầu, bệ leo, khối xếp và cửa đầu ra không bị biến thành nút bấm toàn khối.
  Giữ lực, đường đi, giới hạn hành trình, interlock và luật hợp thể/thắng.
  **Boss 10 giữ bố trí và cách giải**; bốn công tắc chỉ nhận vùng chạm rộng hơn.

## Triển khai

`COgheProductUI` chọn luồng đầu tiên bằng cờ intro và tiến độ đã có, không thêm
một loại save mới. `COgheIntro.Skip` kết thúc và xóa trạng thái pointer cũ.
Hint dùng một CanvasGroup và thời gian UI; không coroutine/texture/material mới.

`COgheTapRail.TouchControlBody` là opt-in cho công tắc gọn. Collider được cache khi
khởi tạo, raycast chỉ khi chạm. Chạm chính xác vỏ được ưu tiên trước vùng dung sai
của tay nắm cạnh bên. Vẫn chạy `Request` cũ để COghe tiếp cận và thao tác thật.
`TapRail()` trong builder lưu cờ cho lần dựng sau. Công cụ
`Tools/update-coghe-control-targets.py` cập nhật các scene trong catalog65 bằng
cách chỉ thêm cờ này; tự kiểm tra nội dung khác không thay đổi. Không dựng lại
hình học, không đổi collider hoặc navigation.

## Kiểm chứng

- **58/58 test đạt**, không lỗi/skip, gồm toàn bộ Product UI, luồng mở lần đầu,
  xem hết/Skip, save cũ, fade/đóng/Pause/resize, chạm vỏ công tắc, interlock,
  vật che phía trước và hồi quy thao tác đẩy/kéo khối. [Kết quả từng test](tests.json).
- **65/65 màn được giải thành công** qua lời giải tác giả bằng lệnh gameplay,
  trong cùng lượt hồi quy. [Dữ liệu từng màn](levels.json). Điều này kiểm tra
  khả năng giải, không thay cho playtest độ khó với người chơi mới.
- **Mac player, 720×1280:** intro tự chạy, Skip vào màn 1, chụp được các bước
  fade in/out; lệnh chạm đầu vẫn đến game. Chạm phần vỏ ngà ở màn 5 qua Input
  System khiến COghe tới vận hành công tắc thật và mở nắp. [Kết quả native](native-result.json),
  [ảnh thực tế](review.html), [định danh build](build.json).
- Đã kiểm tra ảnh: chữ không bị cắt, hướng dẫn mờ đi để trả lại vùng quan sát,
  khung kính/công tắc giữ art Glass C. Build Mac thành công tại
  `Builds/COgheProduct/macOS/COghe.app`.
- Không cài lên điện thoại hoặc đo FPS mobile trong đợt này. Kiểm chứng native
  dùng input chuột; kiểm tra cảm giác ngón tay trên thiết bị là lượt riêng.

Chạy lại: `bash Tools/run-coghe-tests.sh firstplay-targeted
"FreshLaunch;ReturningLaunch;OpeningHint;HintClose;WholeCasing;CasingCannot;IntroSkip"`.
Bản Mac dùng `COgheProductUIBuilder.BuildMac`. Tham số
`-coghe-firstplay-review` chỉ bật trong build kiểm chứng, dùng save và cờ intro
trong bộ nhớ, không sửa tiến độ của người chơi.
