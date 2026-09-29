# Spatial Pilot — mạch điện & ray satin

29/09/2026. Chỉnh trình bày theo phản hồi của người dùng. [So sánh 10 màn](review.html).

- Thay thanh liên kết chéo bằng nét mạch 3 mm trên sàn/mặt đỡ/kính sau, có góc vát và terminal nhỏ. Chỉ biểu diễn tín hiệu điều khiển, không thêm đường bò hay cơ quan vật lý.
- Ray satin xám dịu; bỏ các cục chặn trang trí rời, ray tay kéo sàn đặt sát sàn; thân cửa/cơ quan màu ngà. Tay nắm, dấu A/B và dải hẹp trên cửa giữ xanh / san hô.
- Cáp ròng rọc thật giữ nguyên đường dây và cơ chế lực; giảm độ nặng màu của puly/giá đỡ. Không thêm ánh sáng, shader, runtime update hay collider.
- Static art batch theo material; art động theo đúng rigidbody. Chưa đo draw calls riêng trên Android.

## Kiểm chứng

**41/41 PlayMode**: Spatial 15 và hồi quy V2 26. [XML](playmode-results.xml).

**10/10 native Mac**: 32/32 hạt thoát và 1 cơ thể mỗi màn, không lỗi runtime. InputSystem phát chạm thật; không gán trạng thái thắng. [JSON](mac-native-run.json).

So sánh [trước](physics-before.txt.gz) / [sau](physics-after.txt.gz) giống nhau: transform, collider, rigidbody, joint, surface, rail, input và tham số cơ quan. Definition assets không đổi. Builder cũng chạy lại trên scene đã trang trí mà không tạo bản sao vật lý.

Đã xem ảnh mở màn của đủ 10 màn; kiểm tra thêm ảnh cơ quan vận hành và thắng ở nhóm 04–10. Bản build Mac gồm đúng catalog Spatial Pilot 10 màn, không thay catalog V2 cũ.

## Frame time bản Mac

Máy Mac17,2 / Apple M5; Mac OS X 26.5.1; Unity 6000.3.19f1; Metal; 720×1280; Glass C; cap 60 FPS. Một lượt author replay ngắn, loại thời gian chụp ảnh khỏi mẫu. Chưa đo nhiệt kéo dài hoặc OPPO; không dùng số Mac để khẳng định FPS điện thoại.

| Màn | FPS TB | p95 ms | p99 ms | Max ms | Frame >33,333 ms |
| --- | ---: | ---: | ---: | ---: | ---: |
| 01 | 59.8 | 16.95 | 17.57 | 22.39 | 0 |
| 02 | 59.7 | 17.10 | 17.58 | 24.60 | 0 |
| 03 | 59.6 | 17.14 | 17.58 | 23.81 | 0 |
| 04 | 59.7 | 17.03 | 17.57 | 25.79 | 0 |
| 05 | 59.8 | 16.93 | 17.54 | 23.84 | 0 |
| 06 | 59.3 | 16.85 | 23.86 | 33.35 | 1 |
| 07 | 59.5 | 17.08 | 17.57 | 32.44 | 0 |
| 08 | 59.6 | 17.13 | 17.58 | 25.84 | 0 |
| 09 | 58.4 | 17.46 | 32.38 | 33.53 | 1 |
| 10 | 58.8 | 16.86 | 32.68 | 33.31 | 0 |

Build GUID: `db091d29eca64e8283e4b8f20e395e31`. SHA-256 `GravityBox.Venom.dll`: `89bffe75e19592989e07d92315da2d1b95df90089b1c578657a9c30f600529b8`.

## Chạy lại

Unity: **Gravity Box → COghe → Spatial pilot → Refine circuits and verify physics**, rồi **Build Mac test**. Generator 10 màn cũng gọi cùng art pass.

Mac: `Builds/SpatialLab/macOS/COghe.app`.

Test filter: `COgheSpatialCampaignTests;COgheViewCampaignTests` (PlayMode).

Native args: `-screen-width 720 -screen-height 1280 -screen-fullscreen 0 -coghe-view-proof <output directory> -coghe-spatial -coghe-depth c -coghe-proof-quit`.
