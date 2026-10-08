# COghe: nội dung App Store và Google Ads (Mỹ, tiếng Anh)

Dán thẳng vào App Store Connect và Google Ads. Đã đếm ký tự bằng script (`SOURCE/meta_check.py`). Quy tắc và nguồn:
`RESEARCH/COGHE_APP_STORE_ASO_2026_10_08.md` và `RESEARCH/COGHE_GOOGLE_ADS_IOS_2026_10_08.md`.

## App Store Connect

| Ô | Nội dung | Độ dài |
| --- | --- | --- |
| Name | `COghe: Physics Puzzle Pet` | 25/30 |
| Subtitle | `Stretch, Grab & Solve Levels` | 28/30 |
| Keywords | `virtual,brain,teaser,logic,slime,blob,goo,jelly,squishy,kawaii,cute,gravity,rope,lever,gears,offline` | 100/100 byte |
| Promotional Text | `Meet COghe, a living liquid with big curious eyes. Guide it through 60 glass-box physics puzzles, then feed it, hug it and dress it up at Home.` | 143/170 |
| Primary category | Games → Puzzle | |
| Secondary | Games → Casual | |
| Price | Free | |

Vì sao chọn như vậy:
- **Tên** có "Physics Puzzle": các game vật lý dẫn đầu (Cut the Rope, Royal Smash) đều đặt cụm này trong tên. Có thêm "Pet"
  cho phần Nhà.
- **Phụ đề** nói đúng cách chơi: bò, vươn, bám. Phương án khác: `Cute Blob Brain Teasers` (23).
- **Từ khoá** không lặp lại chữ đã có trong tên, phụ đề hay danh mục, vì Apple đã tính những chữ đó. Không có tên game khác
  (Apple cấm). "offline" đúng vì cả 60 màn chơi được khi không có mạng. Chưa có số liệu lượt tìm thật (phải dùng công cụ trả
  phí). App Store Connect chỉ cho biết bao nhiêu lượt cài đến từ tìm kiếm, không tách theo từng từ khoá. Muốn đo từng từ
  phải dùng công cụ ASO trả phí hoặc Apple Ads.
- **Promotional Text** không ảnh hưởng tìm kiếm, nhưng sửa được bất cứ lúc nào mà không cần gửi bản mới.

### Description (1.158/4.000)

```
Meet COghe, a little creature made of living liquid, with big curious eyes.

Guide it through 60 glass-box physics puzzles. Tap where you want it to go and COghe crawls, stretches and grips its way there. Pull handles, swing on ropes, ride lifts, turn gears and push tall crates to open the way out. Split COghe in two so each half can do a job, then merge it back together.

Between puzzles, COghe lives at Home. Feed it, play ball, tuck it into bed and give it a hug. Poke it too much and it sulks! After each chapter, a bonus round asks you to understand what COghe is saying with the shapes of its body.

Make it one of a kind: inject colored inks, try on hats and add little friends that float inside it. Solve puzzles to earn Drops and unlock new things for COghe and its Home.

- 60 hand-made physics puzzles in 5 chapters, each ending with a boss puzzle
- No timers: think at your own pace
- A pet that reacts to every touch
- Inks, hats and floating friends to collect
- Plays offline

COghe is free to play and shows ads. Buy No Ads to remove banner and full-screen ads, or COghe Plus to also double your Drops and get rewards without watching ads.
```

Mô tả chỉ nói những gì bản hiện tại có. Bộ đồ độc quyền của Plus chưa làm nên không nhắc tới. Nếu lúc phát hành mua trong
app chưa xong thì xoá câu cuối.

### Screenshot (thứ tự upload)

| # | File | Chữ trên ảnh | Cảnh |
| --- | --- | --- | --- |
| 1 | `01_SWING_GRAB_SOLVE` | SWING, GRAB & SOLVE! | Màn 25: COghe đu dây qua khe, ô tròn phóng to COghe |
| 2 | `02_SPLIT_IN_TWO` | SPLIT IN TWO. MERGE BACK. | Màn 24 (Boss): COghe đã tách đôi |
| 3 | `03_HUG_ME` | HUG ME! | Nhà: được ôm, mắt hình trái tim |
| 4 | `04_TURN_THE_GEARS` | TURN THE GEARS | Màn 49: bánh răng |
| 5 | `05_PLAY_WITH_ME` | PLAY WITH ME! | Nhà: chơi bóng |
| 6 | `06_PUSH_THE_CRATES` | PUSH & PULL THE CRATES | Màn 6: COghe đẩy thùng đỏ |
| 7 | `07_DRESS_ME_UP` | DRESS ME UP | Mực xanh lá, mực vàng, mũ len |
| 8 | `08_60_LEVELS` | 60 BRAIN-TEASING LEVELS | Màn 48 (Boss) |

- Ba ảnh đầu hiện trong kết quả tìm kiếm khi chưa có video, nên mỗi bên có mặt: hai ảnh giải đố và một ảnh thú cưng.
- Ảnh phần thú cưng viết theo giọng COghe ("HUG ME!"), theo cách game nuôi thú ảo dẫn đầu (Bruno) đang làm.
- Không ghi giá, không ghi "for kids", không dùng khung máy.
- Đồ trang trí mua bằng Giọt kiếm trong game, không cần trả tiền thật, nên không phải ghi chú mua thêm (quy định 2.3.2).
- Ba bộ kích thước:
  - `IPHONE_6.3_1206x2622`: trang quy cách của Apple ghi bộ này là bắt buộc.
  - `IPHONE_6.9_1320x2868`: các hướng dẫn 2026 của bên thứ ba ghi bộ này là bắt buộc. Chính trang của Apple cũng ghi
    lẫn lộn, nên upload cả hai cho chắc.
  - `IPAD_13_2064x2752`: chỉ cần khi bản build hỗ trợ iPad, mà bản hiện tại đang để iPhone + iPad. Cảnh cận trên iPad mềm
    hơn một chút vì phải phóng to từ khung quay.

### Video xem trước (App Preview)

`PREVIEW/COGHE_APP_PREVIEW_30S_886X1920.mp4`: 29,5 giây, 886×1920, 30 fps, H.264 khoảng 11 Mbps, AAC 256 kbps, đúng quy
cách Apple.
- Chỉ có cảnh quay trong game và chữ mô tả. Không có giá, không có nút "Play free".
- Video tự chạy không tiếng, nên 2 giây đầu là COghe nhìn thẳng ra.
- Đặt ảnh bìa (poster frame) ở giây 6: COghe đang đu dây ("Swing across"). Mặc định của Apple là giây 5, đúng lúc chuyển cảnh.

### Icon

`ICON/COGHE_APP_ICON_1024.png`: icon đã duyệt 07/10. Ba bản màu nền khác (`CORAL`, `LAVENDER`, `SUN`) dùng cho thử nghiệm icon
sau tháng test; xem mục 6.4 của kế hoạch.

### Quyền riêng tư (App Privacy)

Khai báo theo cách thận trọng; kiểm lại bằng báo cáo quyền riêng tư của Xcode trên bản archive.

| Loại dữ liệu | Từ | Mục đích | Gắn với người dùng | Dùng để theo dõi |
| --- | --- | --- | --- | --- |
| Coarse Location | AdMob, Firebase (từ IP) | Third-Party Advertising, Analytics | Có | Có, nếu người chơi cho phép theo dõi |
| Device ID | AdMob (IDFA), Firebase (app instance ID) | Third-Party Advertising, Analytics | Có | Có (IDFA) |
| Purchase History | Firebase (sự kiện mua) | Analytics | Có | Không |
| Product Interaction | Firebase, AdMob | Analytics, Third-Party Advertising | Có | Có (phần AdMob) |
| Advertising Data | AdMob | Third-Party Advertising, Analytics | Có | Có |
| Crash Data, Performance Data, Other Diagnostic Data | AdMob | Analytics, App Functionality, Third-Party Advertising | Crash: không (AdMob ghi "non-user related"); Performance: có | Không |

Độ tuổi: dự kiến **4+**; quảng cáo vẫn hợp với 4+. **Không** chọn "Made for Kids", vì không đổi lại được sau khi duyệt, và
Kids Category gần như cấm AdMob và Firebase.

## Google Ads (nhóm quảng cáo "Evergreen")

**Headlines** (dùng 5, ≤ 30 ký tự):
1. `A pet made of liquid` (20)
2. `60 physics puzzles` (18)
3. `Can you get COghe out?` (22)
4. `Cute blob, clever puzzles` (25)
5. `Tap. Stretch. Solve.` (20)

Dự phòng để thay khi một dòng bị "Low": `Split it. Merge it. Escape.` (27), `Feed it, hug it, dress it up` (28), `Play free, offline` (18).

**Descriptions** (dùng 5, ≤ 90 ký tự):
1. `Guide COghe, a living liquid, through 60 glass-box puzzles of levers, ropes and gears.` (86)
2. `Tap to move. COghe crawls, stretches and grips to pull handles and open the way out.` (84)
3. `Split COghe in two so each half does a job, then merge back. Brainy, cute and calm.` (83)
4. `Between puzzles, COghe lives at Home: feed it, play ball, hug it and earn Drops.` (80)
5. `Color it with inks, add hats and little floating friends. Make COghe one of a kind.` (83)

Dự phòng: `Push and pull tall crates to clear a path. No timers, no rush: think it through.` (80)

**Video** (đăng lên YouTube ở chế độ Unlisted, rồi dán link vào nhóm quảng cáo): 9 file trong `ADS/`.

| File | Tiêu đề YouTube gợi ý |
| --- | --- |
| `COGHE_AD1_PUZZLE_15S_*` | COghe — Can you get it out? |
| `COGHE_AD2_PET_17S_*` | COghe — A pet made of liquid |
| `COGHE_AD3_SHOWCASE_30S_*` | COghe — Physics puzzles with a liquid pet |

**Ảnh**: 9 file trong `ADS_IMAGES/` (3 cảnh × 1200×1200, 1200×628, 1200×1500). Chữ chiếm 3–17% diện tích, dưới mức 20%
Google khuyên.
