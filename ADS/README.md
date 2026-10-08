# Clip quảng cáo COghe cho Google Ads

Ba clip, mỗi clip 3 khổ (dọc 9:16 1080×1920, vuông 1:1 1080×1080, ngang 16:9 1920×1080). H.264 + AAC 48 kHz, 30 fps,
faststart, không metadata. Tất cả là cảnh quay thật từ Unity, bản game commit 254a1366 (có mắt, mũ đứng yên, thùng cao).
Chữ tiếng Anh. Nhạc 120 BPM và tiếng động là bản gốc làm cho quảng cáo 03/10 (`OUTBOX/COGHE_AD_30S_2026_10_03/AUDIO`).

| Clip | Dài | Ý chính | Cảnh |
| --- | --- | --- | --- |
| `COGHE_AD1_PUZZLE_15S` | 15 s | Câu hỏi thử thách: "Can you get COghe out?" | Đu dây (màn 25) → kéo tay nắm mở cửa (màn 4) → tách đôi (màn 24) → bánh răng (màn 49) → đẩy thùng (màn 6) → nhảy mừng → logo |
| `COGHE_AD2_PET_17S` | 17 s | Thú cưng bằng chất lỏng | Gặp COghe → vuốt → ôm (mắt trái tim) → chọc nhiều quá thì giận → chơi bóng → ăn → tiêm mực xanh → đội mũ → logo |
| `COGHE_AD3_SHOWCASE_30S` | 30 s | Cả game: giải đố và nuôi | Gặp COghe → 5 loại câu đố → nhảy mừng "60 puzzles" → ôm, cho ăn → đổi màu, đội mũ → logo |

- 2 giây đầu mỗi clip có chữ to và cảnh động. Google khuyên móc người xem trong 2–3 giây đầu và có phụ đề, vì nhiều chỗ
  phát không tiếng.
- Chạm của người chơi hiện bằng vòng tròn lan ra, đúng chỗ ngón tay chạm trong game.
- Khung ngang 16:9 với cảnh dọc (mực, mũ): cảnh nằm trong khung bo tròn trên nền mờ của chính cảnh đó.
- Logo cuối: icon, chữ COghe, "PLAY FREE". Không có huy hiệu App Store, vì Google tự gắn nút cài đặt.

Cách đăng: mỗi file lên YouTube ở chế độ Unlisted (không công khai), rồi dán link vào nhóm quảng cáo. Tiêu đề YouTube gợi ý
ở `../APP_STORE/METADATA.md`.

Dựng lại: `SOURCE/adcut.py SOURCE/ads/<clip>.json <thư mục ra> <ffmpeg>` (xem `SOURCE/README.md`).
