# Spatial 11–30 — kiểm chứng bản dựng

29/09/2026 · `NewGraphic` (sau sửa review Codex). Bố cục thực tế, khác biệt và số đo: [AS_BUILT](../../LevelDesign/COghe/SpatialNext20/AS_BUILT_2026_09_29.md).

## PlayMode

**87/87** — `COgheSpatialCampaignTests` (Spatial 01–30, hồi phục khi rơi, đường tắt 20, nửa thân ở 29 + probe), `COgheSpatialRecoveryTests`, `COgheViewCampaignTests`. [XML](playmode-results.xml).
Kịch bản chỉ chạm màn hình và chọn phần: không dịch chuyển mô, không gán cửa/chốt, không ép thắng.

Art pass Glass C: 30/30 scene — transform, collider, rigidbody, joint, surface, rail, input và tham số cơ quan giống nhau [trước](physics-before.txt.gz) / [sau](physics-after.txt.gz); definition không đổi.

## Native Mac

**20/20** màn 11–30 thắng trong build Mac thật (`COGHE_MOBILE_BENCHMARK`), InputSystem phát chạm. [JSON](mac-native-run.json).
Máy Mac16,10 / Apple M4; Mac OS X 26.5.1; Unity 6000.3.19f1; Metal; cửa sổ thực 720×1022 (macOS giới hạn chiều cao, không phải 720×1280); Glass C; cap 60 FPS.
Một lượt author replay, thời gian chụp ảnh loại khỏi mẫu. Không dùng số Mac để khẳng định FPS điện thoại; chưa đo OPPO/nhiệt.

| Màn | Kết quả | Hạt ra | Cơ thể | Giây | FPS TB | p95 ms | p99 ms | Max ms | Frame >33,3 ms |
| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| 11 | đạt | 32 | 1 | 8.5 | 60.0 | 16.67 | 16.69 | 16.8 | 0 |
| 12 | đạt | 32 | 1 | 22.0 | 59.9 | 16.77 | 17.22 | 27.4 | 0 |
| 13 | đạt | 32 | 1 | 33.7 | 58.4 | 16.87 | 33.31 | 33.4 | 1 |
| 14 | đạt | 32 | 1 | 14.8 | 53.3 | 33.34 | 33.41 | 34.3 | 47 |
| 15 | đạt | 32 | 1 | 42.7 | 59.8 | 16.74 | 17.21 | 33.3 | 0 |
| 16 | đạt | 32 | 1 | 10.6 | 59.4 | 16.76 | 17.55 | 33.3 | 1 |
| 17 | đạt | 32 | 1 | 28.5 | 59.6 | 16.70 | 17.05 | 33.3 | 1 |
| 18 | đạt | 32 | 1 | 2.9 | 60.0 | 16.67 | 16.70 | 16.7 | 0 |
| 19 | đạt | 32 | 1 | 9.3 | 59.3 | 16.80 | 22.42 | 33.3 | 0 |
| 20 | đạt | 32 | 1 | 29.7 | 59.2 | 16.82 | 32.59 | 33.9 | 1 |
| 21 | đạt | 32 | 1 | 10.3 | 58.7 | 16.69 | 33.24 | 33.3 | 1 |
| 22 | đạt | 32 | 1 | 28.5 | 59.1 | 16.81 | 32.78 | 33.4 | 3 |
| 23 | đạt | 32 | 1 | 21.2 | 56.2 | 33.30 | 33.38 | 34.3 | 33 |
| 24 | đạt | 32 | 1 | 26.1 | 59.5 | 16.77 | 17.58 | 33.3 | 2 |
| 25 | đạt | 32 | 1 | 21.6 | 59.1 | 16.67 | 32.94 | 50.0 | 3 |
| 26 | đạt | 32 | 1 | 27.0 | 57.5 | 16.80 | 33.39 | 50.6 | 32 |
| 27 | đạt | 32 | 1 | 45.9 | 58.8 | 16.88 | 33.02 | 33.3 | 0 |
| 28 | đạt | 32 | 1 | 28.7 | 58.5 | 16.86 | 32.66 | 50.1 | 3 |
| 29 | đạt | 32 | 1 | 36.9 | 58.6 | 16.70 | 32.43 | 50.0 | 13 |
| 30 | đạt | 32 | 1 | 60.8 | 58.6 | 16.70 | 33.61 | 50.5 | 42 |

## Ảnh

Mỗi màn: `NN-start.png` (mở màn), `NN-action.png` (giữa lời giải), `NN-won.png` (thắng) — ảnh chụp từ build Mac ở trên.
