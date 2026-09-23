# COghe — Chạm cơ quan và phối hợp, chapter 01–10

Hướng gameplay được Mrk yêu cầu triển khai ngày 23/09/2026. Chặng A/B đã kiểm chứng trước đó trong [báo cáo 5 màn](COGHE_TAP_VERIFICATION_2026_09_23.md). Yêu cầu mới lúc 05:24 UTC (`185e1e5d67f04a4c4d00919d9960accc7afb7cc4b3ef749ef9cb45155dfcf8d0`) cho phép mở rộng ngay 06–10 để người mới thử chung. Không coi đó là kết quả novice-playtest. **Chapter 01–10 đã hoàn thành kiểm chứng kỹ thuật: 382 PlayMode, 8 EditMode, Mac 10/10.**

## Phạm vi và quan hệ với campaign cũ

Scene `Assets/_Game/Venom/TapCampaign/COgheTap01..10.unity`, ID `coghe.tap.v1.01..10`. Giữ ID và nội dung 01–05; thêm năm ID mới, Boss 10 mở Nhà qua luật save hiện có. Tất cả mười màn khóa xoay. Origin giữ scene/ID/save cũ để đối chứng và hồi quy. Chưa chuyển các màn Origin thành tuyến chạm hoặc nhánh xoay; phạm vi yêu cầu mới là đủ mười màn thử chung.

Bản Mac chapter ở `Builds/COgheTapChapter/macOS/COghe.app`. Bản A/B và app Origin cũ vẫn ở đường dẫn trước. Kết quả mới xem [báo cáo chapter](COGHE_TAP_CHAPTER_VERIFICATION_2026_09_23.md).

## Hành vi

- Chạm cơ quan đang rảnh: phần đang chọn tự tiếp cận, bám vào tay nắm và tác động
  bằng lực hữu hạn tới điểm dừng. Hoàn tất theo vị trí thật và tốc độ đã ổn định.
- Chạm lần tiếp theo sau khi xong: đảo chiều. Chạm dồn không xếp hàng. Trong lúc
  tiếp cận có thể đổi đích; khi đã tác động thì chờ điểm dừng, hoặc pause/retry.
- Chọn phần khác không hủy tác vụ phần cũ. Một cơ quan có một chủ thể tại một thời
  điểm. Dao cắt thật và tự nhập theo khoảng cách/vật cản giữ nguyên luật Origin.
- Một chạm bàn đạp giao nhiệm vụ đứng giữ; chọn lại phần đó rồi chạm bàn đạp lần
  nữa để rời tới điểm an toàn. Khối lượng tiếp xúc thật, không việc chọn phần,
  quyết định sensor bật/tắt. Nhập cơ thể vẫn giữ lệnh hợp lệ mới nhất.
- Màn mới khóa xoay vật lý; camera không thay trạng thái puzzle. Cơ quan, vùng
  đi và cửa phải chọn được ở khung điện thoại dọc.
- Thắng vẫn cần một cơ thể đã nhập bên trong và đủ 32 hạt thoát đúng cửa. Quyền
  Nhà/tiến trình Origin giữ nguyên; thắng Boss 10 của chapter mở Nhà nếu chưa có.

## Nội dung

| Màn | Câu đố / bài học | Hồ sơ |
| --- | --- | --- |
| 01 · Khớp nối | Chạm bánh răng vào ổ, bộ truyền kéo cửa; rút ra thì cơ cấu hồi đóng | [01](LevelDesign/TapCampaign/Level01.md) |
| 02 · Đưa trở về | A nhả chốt B; B bật nguồn; đưa A trở lại mới nối bộ truyền | [02](LevelDesign/TapCampaign/Level02.md) |
| 03 · Mở từng bước | A vào ổ nhả chốt B; cả A và B ăn khớp mới truyền tới cửa | [03](LevelDesign/TapCampaign/Level03.md) |
| 04 · Tách và gặp lại | Phòng thực hành dao, chọn phần và nhập; không thêm chuỗi khóa mới | [04](LevelDesign/TapCampaign/Level04.md) |
| 05 · Cùng vận hành | A cần tải giữ khóa B mở; phần khác lắp B; cửa chốt để cả hai về nhập | [05](LevelDesign/TapCampaign/Level05.md) |
| 06 · Đón bạn trở về | Giữ A, lắp B rồi C, thu hồi phần giữ sau khi cửa chốt | [06](LevelDesign/TapCampaign/Level06.md) |
| 07 · Bốn điểm dừng | Chốt 2 nhả khóa nguồn B; chốt 4 nối bộ truyền; mỗi chạm đi một chốt | [07](LevelDesign/TapCampaign/Level07.md) |
| 08 · Chia việc đổi khớp | Giữ P và vận hành selector/nguồn bằng phần còn lại | [08](LevelDesign/TapCampaign/Level08.md) |
| 09 · Rút rồi nối | Rút A cho phép lắp B, đưa A lại để nối cả hai | [09](LevelDesign/TapCampaign/Level09.md) |
| 10 · Cỗ máy chung | Selector, nguồn, bánh răng C và hai phần; không tutorial/hint; mở Nhà | [10](LevelDesign/TapCampaign/Level10.md) |

Màn 04 là phòng luyện không có điều kiện lịch sử bí mật bắt buộc `CutCount` để
mở cửa. Người chơi có thể thoát nguyên khối; mục tiêu playtest là họ có hiểu và
thử dao từ lời hướng dẫn hay bỏ qua. Màn 05 kiểm tra nhu cầu phối hợp thực sự.

## Kiến trúc và nguyên nhân cơ khí

`COgheTapRail` mở rộng hợp đồng `COgheMechanism`, dùng `COgheRailSlider` hiện có.
Tác vụ giữ anchor hạt và đối tượng lệnh của `VenomCampaignMotion`; việc đổi lựa
chọn không đổi chủ sở hữu tác vụ. Mất lệnh, cắt mất mô, mất điểm bám hoặc kẹt có
phản hồi/hủy lực; không teleport sinh vật hoặc ép rail vào chốt.

`COgheTapPad` giao lệnh đứng giữ, còn `COgheTissueSensor` đọc khối lượng thực.
Khóa rail do tải/vị trí đầu vào quyết định, thể hiện bằng pin nối tới cơ quan.
`COgheGearTrain` kiểm tra tiếp xúc vòng chia và truyền lực hữu hạn. Các tùy chọn
PowerRail/ReturnWhenDisconnected/LatchOutput mặc định không bật ở scene cũ.

Điểm dừng của bánh răng khác chốt cửa. Màn 01–03 có cơ cấu hồi; 05 có nắp sàn nâng bằng thanh răng và chốt giữ
đầu ra sau khi mở hết, cho phép bỏ bàn đạp và hợp thể. Animation/skin/đèn phải đọc
trạng thái vật lý, không tạo thành công qua sự kiện kết thúc clip.

`VenomCampaignDefinition.SceneSequence` là danh sách scene tùy chọn. Khi trống,
Origin dùng catalog cũ. Khi có, nút chọn màn/auto-next dùng đúng danh sách mười màn,
không đi nhầm vào Origin06. Completion được lưu dưới ID mới trong save hiện có;
không xóa, gán lại hoặc coi completion cũ là thắng prototype. `COgheTapLesson` chỉ đọc trạng thái để đổi lời nhắc; hoàn thành bài hoặc vào Nhà thì ẩn hướng dẫn.

`COgheTapRail.Stops` mở rộng ray thành chu trình [0,.08,.16,.24] m. Chỉ tiến tới chốt kế sau một lệnh; điểm đang dở được tiếp tục khi giao lại. Chốt trung gian khóa tại vị trí đo được, không đặt pose. `RequiredPosition` mô tả cam nhả chốt ở một vị trí thật. `COgheTapStopPresentation` chỉ chỉ điểm dừng kế, không mở khóa hay đổi vật lý.

## Tái tạo và kiểm chứng

Unity **6000.3.19f1**. Editor menu:

- `Gravity Box → COghe → Generate Tap Campaign · Ten Levels`
- `Gravity Box → COghe → Build Tap Campaign · macOS`

Từ thư mục project, build đúng chapter 10 màn:

```bash
bash Tools/build-venom.sh --tap
# Khi bạn build bản Android cho người thử:
bash Tools/build-venom-android.sh --tap
```

Mac: `Builds/COgheTapChapter/macOS/COghe.app`. Android: `Builds/COgheTapChapter/Android/COghe.apk`. Không truyền `--tap` sẽ build Origin theo hành vi cũ. Android cần Unity Android Build Support/SDK/NDK sẵn có; lượt này chưa build hoặc cài Android. Builder chọn chính xác mười scene và giữ bundle/save namespace hiện có. Không bật `COGHE_BENCHMARK` cho bản user test.

Nếu dùng Build Profiles thủ công, chọn menu `Gravity Box → COghe → Use Tap Campaign for Player Build` trước khi Build để scene 01 đứng đầu và chỉ mười scene được enable. Menu này đổi EditorBuildSettings; trước khi chạy lại **full regression** cần enable các scene Origin/legacy trong Build Profiles như trước. Các lệnh build `--tap` ở trên không cần đổi danh sách Editor dùng cho regression.

Tương đương executeMethod:
`GravityBox.Editor.VenomCampaignBuilder.GenerateTapCampaign` và
`GravityBox.Editor.VenomCampaignBuilder.BuildTapCampaignMac`.

Generator chỉ quản lý scene/definition/mesh prototype riêng; giữ scene cũ trong
EditorBuildSettings để chạy toàn bộ suite. Build chapter chỉ lấy mười scene mới.

Chạy toàn bộ `GravityBox.Tests.PlayMode` và EditMode, không chỉ fixture mới.
`COgheTapCampaignTests` dùng screen picking và mô phỏng thật cho hành trình, cửa,
pause/retry, vật cản, dao/tụ và phối hợp. Báo cáo phải nêu exact HEAD + diff, XML,
thời gian, hình thật và build đã chạy. Các kết quả chưa ghi ở báo cáo không được
suy ra là đã đạt.

## Thử người mới và thiết bị

Thử đề xuất 5–8 người mới/bản, không chỉ lời giải. Đo hiểu mục tiêu, thực hiện ý
định, đọc đường truyền, hiểu cần hai phần, chạm sai, trợ giúp và tự chọn chơi tiếp.
Mục tiêu ban đầu: ít nhất 80% tự qua 01–03; báo kèm số người thực. Kiểm tra riêng
người chơi bỏ qua bài dao 04 rồi mắc ở 05. Chưa có bằng chứng người mới hoặc hiệu
năng điện thoại. Dùng [phiếu quan sát](COGHE_TAP_PLAYTEST.md) cho vòng tiếp theo.
