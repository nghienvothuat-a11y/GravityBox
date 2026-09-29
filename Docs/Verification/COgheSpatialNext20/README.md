# Spatial 11–30 — kiểm chứng bản dựng

29/09/2026 · nhánh `claude/spatial-next20`. Bố cục thực tế, khác biệt và số đo: [AS_BUILT](../../LevelDesign/COghe/SpatialNext20/AS_BUILT_2026_09_29.md).

## PlayMode

**76/76** — `COgheSpatialCampaignTests` (Spatial 01–30 + probe), `COgheSpatialRecoveryTests`, `COgheViewCampaignTests`. [XML](playmode-results.xml).
Hai lượt liên tiếp trên cùng scene cho kết quả giống hệt. Kịch bản chỉ chạm màn hình và chọn phần: không dịch chuyển mô, không gán cửa/chốt, không ép thắng.

Art pass Glass C: 30/30 scene — transform, collider, rigidbody, joint, surface, rail, input và tham số cơ quan giống nhau [trước](physics-before.txt.gz) / [sau](physics-after.txt.gz); definition không đổi.

## Native Mac

**20/20** màn 11–30 thắng trong build Mac thật (`COGHE_MOBILE_BENCHMARK`), InputSystem phát chạm. [JSON](mac-native-run.json).
Máy Mac16,10 / Apple M4; Mac OS X 26.5.1; Unity 6000.3.19f1; Metal; cửa sổ thực 720×1022 (macOS giới hạn chiều cao, không phải 720×1280); Glass C; cap 60 FPS.
Một lượt author replay, thời gian chụp ảnh loại khỏi mẫu. Không dùng số Mac để khẳng định FPS điện thoại; chưa đo OPPO/nhiệt.

| Màn | Kết quả | Hạt ra | Cơ thể | Giây | FPS TB | p95 ms | p99 ms | Max ms | Frame >33,3 ms |
| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| 11 | đạt | 32 | 1 | 8.5 | 59.7 | 17.09 | 17.58 | 22.8 | 0 |
| 12 | đạt | 32 | 1 | 22.0 | 59.7 | 17.02 | 17.57 | 27.1 | 0 |
| 13 | đạt | 32 | 1 | 33.7 | 58.4 | 17.03 | 33.29 | 33.4 | 2 |
| 14 | đạt | 32 | 1 | 16.7 | 54.4 | 33.33 | 34.06 | 34.3 | 41 |
| 15 | đạt | 32 | 1 | 30.9 | 59.6 | 16.90 | 17.46 | 33.3 | 0 |
| 16 | đạt | 32 | 1 | 10.6 | 59.4 | 16.79 | 17.57 | 33.4 | 1 |
| 17 | đạt | 32 | 1 | 29.0 | 59.4 | 16.83 | 22.04 | 50.0 | 1 |
| 18 | đạt | 32 | 1 | 2.9 | 59.9 | 16.81 | 17.03 | 17.3 | 0 |
| 19 | đạt | 32 | 1 | 9.7 | 60.0 | 16.71 | 16.96 | 17.2 | 0 |
| 20 | đạt | 32 | 1 | 30.9 | 59.2 | 16.93 | 32.34 | 33.1 | 0 |
| 21 | đạt | 32 | 1 | 10.3 | 58.5 | 16.99 | 33.31 | 33.3 | 0 |
| 22 | đạt | 32 | 1 | 28.7 | 59.1 | 16.76 | 32.77 | 33.3 | 1 |
| 23 | đạt | 32 | 1 | 21.7 | 56.2 | 33.30 | 33.56 | 34.3 | 37 |
| 24 | đạt | 32 | 1 | 25.7 | 59.5 | 16.81 | 17.58 | 33.3 | 4 |
| 25 | đạt | 32 | 1 | 21.7 | 58.9 | 16.78 | 33.30 | 33.4 | 4 |
| 26 | đạt | 32 | 1 | 28.4 | 57.3 | 17.50 | 33.34 | 50.6 | 23 |
| 27 | đạt | 32 | 1 | 48.6 | 58.8 | 16.94 | 32.90 | 33.5 | 1 |
| 28 | đạt | 32 | 1 | 29.3 | 58.5 | 17.26 | 32.59 | 49.9 | 1 |
| 29 | đạt | 32 | 1 | 34.5 | 59.0 | 16.84 | 32.64 | 47.3 | 2 |
| 30 | đạt | 32 | 1 | 57.8 | 58.5 | 16.78 | 49.25 | 50.6 | 41 |

## Ảnh

Mỗi màn: `NN-start.png` (mở màn), `NN-action.png` (giữa lời giải), `NN-won.png` (thắng) — ảnh chụp từ build Mac ở trên.
