# COghe — Boss 10 và tiến trình gắn bó, 14/10/2026

Yêu cầu: Boss 10 khó hơn; “Hiểu COghe” bắt buộc trước chương tiếp;
trang trí mở sau **màn hiển thị 8**, hướng dẫn tiêm màu rõ từng bước.
Giữ Glass C, 65 màn, ID/save, luật kính bám được và màn thắng chờ người chơi bấm tiếp.

## Luồng sản phẩm

- Thắng màn 8 (`coghe.spatial.pilot.07`): Ocean miễn phí và Mint mở sớm.
  Sau khi bấm tiếp trên màn thắng, vào sân khấu Style với COghe 3D thật.
  Chọn Ocean → giữ trên da để tiêm, rê tay để đổi điểm → thả tay, màu tự lưu →
  giải thích Undo/Rinse → Done tiếp màn 9. Không mua hàng/quảng cáo để làm hướng dẫn.
- Style có nút riêng ở Main Menu, khóa trước khi hoàn thành màn 8. Nhà vẫn khóa đến Boss 10.
- Thắng Boss: sau phần giới thiệu vật phẩm vừa mở, tự vào “Understand COghe”.
  Không còn Skip. Back/Menu vẫn dùng được; Play quay lại câu chưa hoàn tất.
  Chỉ hoàn thành cả ba câu mới đi tiếp. Áp dụng sáu Boss 10/21/32/43/54/65.
- Save giữ nguyên các màn đã qua, vật phẩm và màu đang dùng. Reward cũ vẫn được coi
  là hoàn thành; các phần từng bỏ qua cần chơi trước khi tiếp tục chương.
  Checkpoint câu trả lời độc lập với Drops (kể cả Remote Config đặt thưởng bằng 0).
  Phần thưởng từng câu và phần thưởng cuối không trả lại lần hai.

## Boss 10

Xem [hồ sơ hiện hành](../../LevelDesign/COghe/SpatialPilot/Level10/README.md).
Thêm cầu nâng giữa hai ban công; bốn circuit: amber mở hộp điều khiển coral,
coral nâng cầu, green mở hộp điều khiển blue, blue mở cửa thoát.
Lời giải dùng thang hai lượt khứ hồi. Độ khó đến từ đọc liên kết và thứ tự,
không có bộ đếm thời gian hay khe chạm nhỏ. Không hiện hướng dẫn giải trong Boss.

## Kiến trúc

- Geometry/state: `COgheSpatialChapterOneBuilder`, scene `COgheSpatial10` và các
  `COgheTapRail`, `COgheRailSlider`, `COgheViewMechanism`, `COghePassengerLift` dùng lại.
- Style stage sử dụng habitat trình diễn, không mở khóa Home hay tăng chỉ số vật lý.
- `COgheShop.State.Understood` lưu checkpoint; nhận diện thêm paid keys cũ để tương thích.
- `InkGuideStep` lưu bước chọn màu/tiêm/kiểm tra/xong. Không tăng version campaign
  vì migration cũ của campaign có thể xóa tiến độ.
- Catalog test picker vẫn được phép nhảy màn trong bản test; Play/Next bản Product
  thực thi checkpoint bắt buộc. Test picker không ghi thắng.

## Kiểm chứng

**56 test cases khác nhau đều pass ở lượt kiểm tra cuối của từng case**:
43 UI/tiến trình và 13 gameplay. [Danh sách và XML nguồn](test-summary.json).
Các lượt phát hiện lỗi trong quá trình sửa vẫn giữ ở Artifacts; không gộp chúng
thành một lượt chạy xanh giả. Các lỗi va chạm khởi tạo, cầu không đạt độ cao và
điểm chạm bị cần/vách che đã được sửa, chạy lại solve/wander/reverse thành công.

- Boss 10: solve, wander tìm kẹt; chạm cơ quan sai thứ tự/đi thẳng lỗ;
  hạ cầu rồi nâng lại; Retry khôi phục khóa. 32/32 hạt thoát cùng một bản thể.
- Cơ quan 1–10: không chồng collider lúc đầu, không tự mở; hồi quy solve 4/5/8.
- Hiểu COghe: sáu bộ câu hỏi, thoát/resume, phần thưởng cũ, cấu hình thưởng bằng
  0, trả thưởng một lần. Câu hỏi đã làm được lưu riêng với ví tiền.
- Tiêm màu: thắng màn hiển thị 8 → Ocean miễn phí → chạm/giữ/thả trên cơ thể →
  Undo → tiêm lại → Back → nạp save → tiếp bước 3 → Done → màn 9. Nhà vẫn khóa.
- Layout: cả ba bước ở 720×1280, 1080×2340, 1536×2048; không trùng vùng chữ/nút.
  Hồi quy thắng chờ bấm tiếp, quảng cáo, vật phẩm, đổi màu và Home đều pass.

### Bản Mac và bằng chứng thật

Unity **6000.3.19f1**, app `Builds/SpatialLab/macOS/COghe.app`.
[Xem ảnh kiểm chứng](review.html). Ảnh gameplay dùng HUD kiểm thử của author replay;
ảnh Main Menu/Style/Hiểu COghe dùng UI Product thật. Không dùng concept render.

[Native Boss run](native-boss-summary.json): Mac17,2 / Apple M5, Metal,
720×1280, vòng lặp thời gian thực (không fast simulation). 11 lệnh chạm, 52.04 giây,
**32 hạt thoát, 1 bản thể, không error/exception**.

| Phép đo | Kết quả |
| --- | ---: |
| FPS trung bình | 57.56 |
| Frame p50 / p95 | 16.67 / 16.96 ms |
| Frame p99 / lớn nhất | 49.85 / 50.41 ms |
| Frame > 33.33 / > 50 ms | 59 / 18 trong 2.930 mẫu |
| Bộ nhớ allocated cao nhất | 76.4 MiB |

Loại thời gian chụp ảnh khỏi mẫu. Vẫn có khung hình chậm; đây **không phải chứng
minh 60 FPS ổn định trên mobile**. Chưa đo revision này trên OPPO/iPad.
Độ khó người mới cần thêm playtest; đường giải tự động không đo cảm nhận đó.

[UI native fixture](ui-result.json) đặt tiến độ đã qua 8/10 để chụp màn hình,
nhưng tiêm màu thật và đi qua UI thật, không ghi save của người chơi. Không dùng
fixture này để tuyên bố đã giải tám màn bằng tay. Bài solve Boss dùng InputSystem
Touchscreen thật, không teleport hay gán trạng thái thắng.

### Tái hiện

- PlayMode: `COgheProductUITests`, `COgheSpatialCampaignTests.Spatial10Solve`,
  `Spatial10Wander`, `Boss10RejectsShortcutsAndRecoversAfterReversingBridge`.
- Build: `GravityBox.Editor.VenomCampaignBuilder.BuildSpatialMac`.
- Player: `-coghe-view-proof <absolute-output> -coghe-spatial -coghe-view-first 10
  -coghe-view-last 10 -coghe-proof-quit`.
- UI fixture: `-coghe-chapter-review`; ảnh xuất ở
  `Builds/SpatialLab/macOS/Artifacts/ChapterProgression20261014/UI` trên Mac.
- Log, XML, snapshot trước sửa: `Artifacts/ChapterProgression20261014`.
  Native Boss build GUID nằm trong JSON; lượt polish UI sau đó không sửa gameplay.

Cầu nâng dùng tùy chọn `COgheViewMechanism.Counterbalance` chỉ bật cho cầu này:
bù trọng lượng dọc ray trong giới hạn lực 0.45 N, không gán transform/position.
Chốt cuối hành trình giữ tải; kéo ngược giải phóng chốt. Các cơ quan cũ giữ mặc định.
