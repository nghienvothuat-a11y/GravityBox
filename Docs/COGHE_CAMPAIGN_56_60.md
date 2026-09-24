# COghe — Màn xoay vật thể trơn 56–60

Thêm 5 màn theo màn 7 hiện tại (content 06), nối vào campaign thành 60 màn.

| Màn | Hình | Ý tưởng |
| --- | --- | --- |
|56|Bình cổ hẹp|Lật bụng, căn cổ xuống|
|57|Mặt nạ thủy tinh|Khoang dẹt, lỗ lệch trên trán|
|58|Ấm nghiêng|Bụng nối vòi cong|
|59|Đầu lâu pha lê|Từ sọ xuống lỗ hàm, bắt đầu nằm nghiêng|
|60|BOSS · Vỏ ốc|Đổi hướng qua đường cong, không hint|

Mọi mặt trong trơn; kéo xoay như màn 7. Chạm vẫn phản hồi nhưng không bám. Vòng mint là lỗ thoát thật duy nhất. Đủ 32 hạt trong một cơ thể mới thắng.
Vật thể là vỏ rỗng có collider đúng hình, không phải hình trang trí bọc hộp.

Builder: `GravityBox.Editor.VenomCampaignBuilder.GenerateCampaign60` chỉ sinh 56–60.
Build Mac: `GravityBox.Editor.VenomCampaignBuilder.BuildCampaign60Mac` → `Builds/COgheCampaign60/macOS/COghe.app`.
Build Android tại worktree: `bash Tools/build-venom-android.sh`, không thêm `--tap`.
APK đầu ra theo wrapper hiện tại: `Builds/Venom/Android/COghe.apk`. Lượt này chưa build APK.

Hồ sơ từng màn: `Docs/LevelDesign/COghe/Level56` đến `Level60`.
Đã qua 403/403 PlayMode, 8/8 EditMode và 5/5 màn trong app Mac. Xem [báo cáo kiểm chứng](COGHE_CAMPAIGN_56_60_VERIFICATION_2026_09_24.md) để biết source, bằng chứng và giới hạn kiểm chứng mobile/người mới.
