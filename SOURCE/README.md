# Cách dựng lại clip, screenshot và ảnh quảng cáo

Python với Pillow, numpy, soundfile, scipy (đang dùng `.scratch/audio-venv`), ffmpeg (`/opt/homebrew/bin/ffmpeg`).
Chạy với `python -I` từ một thư mục khác thư mục chứa script.

## Cảnh quay gốc (Unity, repo `REPOS/GravityBox-spatial20`, commit 254a1366)

Các cảnh nằm trong `Artifacts/` (không vào git):
- `Clips/03, 04, 06, 12, 13, 24, 25, 28, 36, 48, 49, 59, 60`: lời giải mẫu của từng màn, 1320×2347, 30 fps. Có `track.txt`
  (vị trí COghe theo khung hình) và `taps.txt` (chỗ chạm).
- `Clips/25-eyes, 13-eyes`: như trên, kèm cả điệu nhảy mừng 5 giây.
- `Clips/eyes-home/eyes`: cận cảnh ở Nhà, 1200×1600. Các mốc trong `marks.txt`: nghỉ, vuốt, ôm, giận, bóng, ngủ, ăn, màn thưởng, mực.
- `Inks/singles`, `Inks/mixes`, `Accessories/hats`, `Accessories/inclusions`: tiêm mực, mũ, vật trôi; 1080×2340.

Lệnh quay (các test Explicit, xem `Tools/run-coghe-tests.sh`):

```sh
COGHE_CLIP_LEVELS="3,4,6,12,13,24,25,28,36,48,49,59,60" COGHE_CLIP_SIZE=1320x2347 \
  bash Tools/run-coghe-tests.sh ua-levels "GravityBox.Tests.COgheSpatialCampaignTests.RecordReviewClips"
# Các cảnh Nhà, mực, mũ ở độ phân giải gấp đôi: cần áp recorder-scale.patch trước (git apply), sau đó revert
COGHE_CLIP_SCALE=2 COGHE_CLIP_PLAIN=0 COGHE_EYES=1 COGHE_CLIP_LEVELS="25,13" COGHE_CLIP_SIZE=1320x2347 \
  bash Tools/run-coghe-tests.sh ua-reels "GravityBox.Tests.COgheSpatialCampaignTests.RecordEyesHome;GravityBox.Tests.COgheSpatialCampaignTests.RenderInkInjections;GravityBox.Tests.COgheSpatialCampaignTests.RenderAccessoryReels;GravityBox.Tests.COgheSpatialCampaignTests.RecordReviewClips"
```

`recorder-scale.patch` chỉ sửa test: thêm biến `COGHE_CLIP_SCALE` (nhân kích thước khung quay các cảnh Nhà, mực, mũ) và
`COGHE_CLIP_PLAIN=0` (bỏ bản không mắt). Chưa commit; repo đang sạch.

**Lưu ý:** `RecordReviewClips` xoá `Clips/NN` trước khi quay. Cảnh gốc của clip hướng dẫn 07/10 đã được dời sang
`Clips/tutorial-0710/`.

## Dựng

| Việc | Lệnh |
| --- | --- |
| Clip quảng cáo | `adcut.py ads/ad1_puzzle_15.json <ra> <ffmpeg> 9x16,1x1,16x9` (tương tự `ad2_pet_17.json`, `ad3_showcase_30.json`) |
| App Preview | `adcut.py ads/app_preview_30.json <ra> <ffmpeg> 886x1920` (mã hoá theo quy cách Apple) |
| Screenshot | `shots.py shots_spec.json <ra> 1320 2868`, rồi `1206 2622` và `2064 2752` |
| Ảnh Google Ads | `gads_images.py gads_spec.json <ra>` |
| Icon đổi màu nền | `icon_variants.py <icon 1024> <ra>` |
| Đếm ký tự nội dung | `python3 meta_check.py` (đọc `desc.txt`) |
| Kiểm khung hình | `review.py <video> <ảnh ra> <t1,t2,…>`, `sheet.py`, `strip.py` |

Mỗi spec JSON liệt kê các cảnh: cảnh gốc, khung vào/ra, tốc độ, độ phóng theo từng khổ, điểm nhìn ("track" là đi theo COghe),
chữ, tiếng động và logo cuối. Muốn sửa chữ hay đổi cảnh thì chỉ cần sửa JSON rồi chạy lại.
