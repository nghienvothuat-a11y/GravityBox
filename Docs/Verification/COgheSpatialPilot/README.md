# COghe Spatial Pilot — kiểm chứng 10 màn

Ngày đo 29/09/2026, nhánh NewGraphic. Lượt đo chạy trên các thay đổi Spatial chưa commit tại thời điểm đó; nền repo `2c25830654269f7630a21d5e975dafac7cca2373`. Source và bằng chứng này được lưu cùng đợt bàn giao Spatial. Bản art sau cùng được ghi tại [SpatialCircuits](../COgheSpatialCircuits/README.md); không coi số dưới đây là phép đo mới của 20 màn thiết kế tiếp theo.

- [Minh hoạ trước dựng](../../LevelDesign/COghe/SpatialPilot/review.html) · [Hồ sơ](../../LevelDesign/COghe/SpatialPilot/README.md) · [Đối chiếu ảnh game](review.html).
- Mac: `Builds/SpatialLab/macOS/COghe.app`; catalog riêng 10 màn, save `coghe.spatial.pilot`. V2 30 scene cũ giữ nguyên.
- PlayMode: **41/41 đạt** — Spatial 15/15, V2 26/26. [XML](playmode-results.xml).
- Native Mac: **10/10 đạt**, 32/32 hạt thoát, một cơ thể mỗi màn, 0 lỗi runtime. [Dữ liệu gốc](mac-native-run.json).

## Đã kiểm tra

Đường giải đủ 10 màn bằng chạm/camera thật; Boss mở Nhà; reset sau thắng; hộp không xoay khi kéo camera; không có props chồng vào cảnh tĩnh lúc bắt đầu; cơ quan không tự chạy khi idle; ròng rọc kéo thuận/ngược; thang chở đủ mô lên và xuống; đổi đích huỷ boarding; catalog/save riêng. Hồi quy bao gồm chạm, kéo, pinch, pause/focus, cơ quan, các đường giải V2.

Các lỗi tìm và sửa: cửa thiếu khoảng trượt; cửa B cọ bệ cao; cáp dao động và không tới chốt dưới tải; khay nâng cần mốc vận tốc tương đối cho sinh vật; chốt cáp cần nhả khi kéo ngược. Không teleport sinh vật để chứng minh qua màn.

## Số liệu native Mac

Máy Mac17,2 / Apple M5 / macOS 26.5.1; Unity 6000.3.19f1; Metal; 720×1280; cap 60 FPS; Glass C; 32 hạt, bước vật lý 1/120 s. Release player kèm instrumentation `COGHE_MOBILE_BENCHMARK`, không profiler nối ngoài. Mỗi scene chờ 2 giây trước đo, đo đường giải thực; thời gian screenshot và các frame kế được loại khỏi frame sample. Một lượt, chưa có kiểm tra nhiệt kéo dài. GC counter bao gồm công việc chụp hình nên không coi nó là GC gameplay thuần.

| Màn | Kết quả | FPS TB | p95 ms | p99 ms | Max ms | Frame >33,333 ms |
| --- | --- | ---: | ---: | ---: | ---: | ---: |
| 01 | Đạt · 32/32 · 1 cơ thể | 59.3 | 17.52 | 21.79 | 23.38 | 0 |
| 02 | Đạt · 32/32 · 1 cơ thể | 60.0 | 16.73 | 16.90 | 16.97 | 0 |
| 03 | Đạt · 32/32 · 1 cơ thể | 59.8 | 16.81 | 17.50 | 23.82 | 0 |
| 04 | Đạt · 32/32 · 1 cơ thể | 59.8 | 16.77 | 17.52 | 23.09 | 0 |
| 05 | Đạt · 32/32 · 1 cơ thể | 59.6 | 17.02 | 17.58 | 29.56 | 0 |
| 06 | Đạt · 32/32 · 1 cơ thể | 59.2 | 17.10 | 24.89 | 33.33 | 0 |
| 07 | Đạt · 32/32 · 1 cơ thể | 59.7 | 16.84 | 17.57 | 30.52 | 0 |
| 08 | Đạt · 32/32 · 1 cơ thể | 59.7 | 16.85 | 17.55 | 28.39 | 0 |
| 09 | Đạt · 32/32 · 1 cơ thể | 58.8 | 17.02 | 33.28 | 33.47 | 2 |
| 10 | Đạt · 32/32 · 1 cơ thể | 58.8 | 16.98 | 33.00 | 33.33 | 0 |

Build GUID: `7a91b28954c342bf987bcdf0564797b8`. SHA-256 `GravityBox.Venom.dll`: `89bffe75e19592989e07d92315da2d1b95df90089b1c578657a9c30f600529b8`.

Đây là lượt author replay ngắn (khoảng 5–19 giây/màn), không phải bằng chứng độ khó với người mới hoặc hiệu năng OPPO. OPPO không hiện trong `adb devices` lúc kiểm chứng. Chưa đo GPU time riêng, throttling, pin hoặc tải lâu dài. Màn 09 có 2 frame vượt 33,333 ms; không mô tả toàn bộ lượt đo là tuyệt đối không spike.

## Tái chạy

```sh
Unity -batchmode -projectPath /Users/mrk/GravityBox -runTests -testPlatform PlayMode -testFilter 'COgheSpatialCampaignTests;COgheViewCampaignTests' -testResults Artifacts/spatial-regression.xml -logFile Artifacts/spatial-regression.log
```

Build: `GravityBox.Editor.VenomCampaignBuilder.BuildSpatialMac`.

```sh
'Builds/SpatialLab/macOS/COghe.app/Contents/MacOS/Gravity Box' -screen-width 720 -screen-height 1280 -screen-fullscreen 0 -coghe-view-proof /Users/mrk/GravityBox/Artifacts/COgheSpatial/Native -coghe-spatial -coghe-depth c -coghe-proof-quit -logFile /Users/mrk/GravityBox/Artifacts/spatial-native.log
```

HUD mờ trong ảnh tự chạy vì khoá nút người dùng để phép đo không bị can thiệp; bản chơi bình thường không khoá HUD. Bố trí có một số điều chỉnh kỹ thuật so với concept (bờ đỡ cầu, vị trí B, thang motor); [giải thích](../../LevelDesign/COghe/SpatialPilot/ARCHITECTURE.md). Chưa thay bộ 30 màn mặc định hoặc xoá nội dung đã duyệt.
