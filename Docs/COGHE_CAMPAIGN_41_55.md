# COghe — campaign 01–55 cho user test

Yêu cầu 23/09/2026: mười màn Tap thành 41–50; thêm năm màn đẩy hộp ghép đường theo màn hiển thị 19 tại 51–55. Source ở worktree `GravityBox-macos-preview`, Unity 6000.3.19f1.

| Màn | Nội dung |
| --- | --- |
| 41–50 | Giữ nguyên mười bài Tap; Boss 50 không gợi ý lời giải |
| 51 | Hai khối rộng: tập nối cầu |
| 52 | Ba khối lệch: đọc mép và ổ đích |
| 53 | Ba khối cần kéo trở về đường ngang |
| 54 | A/C đẩy, B kéo |
| 55 | Bốn khối, đường lệch, kết hợp đẩy/kéo |

Hồ sơ từng màn: `Docs/LevelDesign/COghe/Level41` đến `Level55`. Các màn 01–40 và bản Tap riêng vẫn được giữ. ID Tap không đổi khi đưa vào campaign, nên completion cũ tiếp tục nhận đúng nội dung. Save key/schema không đổi; không tự xóa dữ liệu người chơi.

## Build đúng bản 55 màn

```bash
cd /Users/tommynguyen/.buzz/REPOS/GravityBox-macos-preview
bash Tools/build-venom-android.sh
```

APK đầu ra: `Builds/Venom/Android/COghe.apk`. **Không truyền `--tap`**: cờ đó vẫn chọn bản thử mười màn riêng trước đây.

Build Mac mặc định: `bash Tools/build-venom.sh`, đầu ra `Builds/Venom/macOS/Venom.app`. Builder truyền đủ 55 scene vào BuildPipeline. Khi build bằng Unity UI, danh sách Editor còn các scene legacy phục vụ regression; ưu tiên wrapper để lấy chính xác campaign.

Tái tạo phần mới bằng menu `Gravity Box → COghe → Generate Campaign 41–55` (Editor method `GravityBox.Editor.VenomCampaignBuilder.GenerateCampaign55`). Method giữ 01–40 và Tap nguồn, viết lại riêng scene/definition 41–55, dùng `.meta` đã có. Không gọi GenerateCampaign30/40 để dựng phần mới này.

## Thử trên điện thoại

Vào trang `41–50` để chơi cơ quan Tap, hoặc trang `51–55` để thử ghép hộp. Ở các màn hộp, chạm tay nắm rồi chạm hướng cần đẩy/kéo; chạm mặt trên hộp để ra lệnh bò. Buông vật trước khi leo bờ trái và qua cầu. Không yêu cầu xoay hộp, cắt hoặc nâng chỉ số trong 51–55.

Ghi riêng từng màn: thời gian tự hiểu, số chạm nhầm, có nhận ra tay nắm/ổ đích không, có tìm được đường leo lên bờ trái không, hướng kéo có dễ hiểu không, có cần gợi ý không, Retry có phục hồi không. Không cho người mới xem lời giải/video trước buổi thử. Ghi model điện thoại, OS, build SHA, nóng máy/FPS nếu đo; Mac replay không thay cho các dữ liệu này.

## Kiểm chứng

Đã qua 393/393 PlayMode, 8/8 EditMode và 15/15 màn mới trên Mac. Xem [báo cáo đầy đủ](COGHE_CAMPAIGN_41_55_VERIFICATION_2026_09_23.md), bằng chứng tại `Artifacts/COgheCampaign55`. Android chưa build/cài trong nhiệm vụ này; người dùng sẽ build để test.
