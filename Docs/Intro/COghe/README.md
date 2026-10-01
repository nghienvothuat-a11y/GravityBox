# COghe — intro mở game (motion comic)

30/09/2026 · Theo yêu cầu của Mrk: khoảng 20 s, không chữ, chỉ hình, nhịp nhanh; style comic / Marvel; nhà khoa học nữ
người Mỹ, trẻ, xinh (không xuất hiện lại); kết bằng cú chuyển từ tranh comic sang hộp kính 3D. Tranh do Codex vẽ
(`OUTBOX/COGHE_INTRO_ASSETS_2026_09_30` trong workspace Buzz); chuyển động, hiệu ứng, âm thanh, tích hợp do Claude làm.

**Vẽ lại 01/10/2026** (Mrk: người dùng thấy tranh "hơi AI", chất liệu sần): Codex vẽ lại cả 18 lớp theo ảnh tham chiếu
của Mrk: nét comic sạch, viền mực đậm, mảng màu và bóng mượt, bỏ grain/halftone/hatching
(`OUTBOX/COGHE_INTRO_SMOOTH_2026_10_01`). Runtime bỏ lớp chấm in (halftone) từng phủ lên toàn bộ intro. Bố cục, nhịp,
nhân vật và cú chuyển cuối giữ nguyên. So sánh: [`before-after.jpg`](before-after.jpg). Dung lượng gần như không đổi (texture ASTC 8×8
trên máy 3,06 → 3,08 MB).

Xem nhanh: [`preview.mp4`](preview.mp4) (540×1170, có tiếng, render từ Unity: màn 1 thật nằm dưới tranh) và
[`contact-sheet.jpg`](contact-sheet.jpg) (24 khung).

## Kịch bản (giây)

| # | Thời gian | Cảnh | Chuyển động / hiệu ứng | Âm thanh |
| --- | --- | --- | --- | --- |
| S1 | 0–1,8 | Trời đêm, quả cầu bốc lửa lao xuống | Camera trôi, speed lines, loé trắng cuối | Pad tối, gió dâng, tiếng rít |
| S2 | 1,8–3,0 | Va chạm: hố, vụ nổ | Khung đảo mực (0,1 s), tia nổ, rung | Bùm + đất đá |
| S3 | 3,0–5,6 | Lính và nhà khoa học tới | Lính tiến lại, cô bước vào từ phải, parallax | Cánh quạt, trống, dây đàn nhịp |
| S4 | 5,6–7,6 | Quả cầu ở đáy hố → mở ra (7,0) | Đường nối sáng nhịp thở, loé khi mở, zoom chậm | Tim đập; xì hơi + chuông |
| S5 | 7,6–9,6 | COghe trồi lên từ khay; hai lính giật lùi | COghe nở ra (vượt đà + rung lỏng), lính trượt ra hai mép | "ư?" đầu tiên; hợp âm treo |
| S6 | 9,6–11,4 | Cô quỳ, hạ nòng súng, chìa tay; COghe nhảy lên lòng bàn tay | Nhảy vòng cung, đáp bẹp | Chuyển sang Rê trưởng ấm; "búp" + vui |
| S6 | 11,4–14,6 | Cận cảnh COghe trên tay | Đẩy camera vào lòng bàn tay, 3 đốm lấp lánh | Motif sinh vật (celesta, như nhạc nền) |
| S7 | 14,6–16,3 | Trong lab: hai tay hạ COghe xuống sàn kính | Khung trượt vào; nền hộp phóng to, mờ như lấy nét | Piano thưa |
| S7 | 16,3–18,0 | Toàn cảnh hộp kính, COghe nhảy xuống sàn | Khung khớp đúng camera thật của màn 1 | "hm?" tò mò |
| → 3D | 18,0–19,6 | Tranh tan thành màn 1 thật | Mực ăn dần từ chỗ COghe lan ra | Lấp lánh → hợp âm Dmaj9 (hợp âm mở đầu nhạc nền) |

Khung cuối: tranh phòng lab đã được **đăng ký hình học** lên ảnh render thật của màn 1 (lệch ≤ 3 px) và kéo màu gần
màu game; lúc chạy, vị trí tranh được tính lại từ 4 góc sàn hộp chiếu qua camera thật, nên khớp trên mọi tỉ lệ màn
hình (đã kiểm 1080×2340 và 1080×1920). COghe vẽ được đặt đúng chỗ COghe thật đang đứng.

## Khi nào phát

- Lần đầu người chơi mở **vị trí 1 của catalog Spatial** (màn 1), trước khi chơi. Màn 1 được tạm dừng bên dưới; HUD ẩn;
  nhạc nền và SFX của game tắt (intro có nhạc riêng). Hết intro, game tự chạy tiếp.
- Nút **Skip** (dưới, giữa màn hình, sau 1 s; Mrk 01/10: bấm bằng ngón cái): nhảy tới đoạn COghe đã ngồi trong hộp rồi
  tan vào game — bỏ qua vẫn thấy cú chuyển.
- Xem lại: màn 1 → **Tạm dừng** → **Xem lại phần mở đầu**.
- Đã xem thì lưu `PlayerPrefs "coghe.intro.seen"`. Không phát khi chạy test, batchmode hay bản proof tự chạy.

## File

- Runtime: `Assets/_Game/Venom/Runtime/Intro/COgheIntro.cs` (vẽ immediate mode bằng `Graphics.DrawTexture`, không thêm
  canvas/camera/scene) và shader `Resources/COgheIntro/IntroLayer.shader` (mờ dần, đảo mực, mờ lấy nét, tan mực).
- Tranh: `Assets/_Game/Venom/Resources/COgheIntro/*.jpg|png` + `layout.txt` (vùng cắt của từng lớp trong khung gốc, vị
  trí COghe trong các lớp có COghe). Nền JPG, nhân vật PNG có alpha.
- Nhạc: `Assets/_Game/Venom/Resources/COgheAudio/intro_score.ogg` (20 s, stream).
- Import: `Assets/_Game/Editor/COgheIntroImport.cs` (ASTC 8×8 trên Android/iOS, không mipmap: trên Android texture
  không lũy thừa 2 có mipmap bị lưu không nén).

## Thay tranh

1. Bỏ PNG mới (tên như `LAYER_MANIFEST.md`, cùng tỉ lệ khung) vào thư mục LAYERS.
2. Chạy:
   ```
   python3 -m venv .venv && .venv/bin/pip install numpy pillow opencv-python-headless
   .venv/bin/python Tools/intro/prepare_intro_layers.py <LAYERS> <REFERENCES/level01_start_1080x2340.png>
   ```
   Script chuẩn hoá kích thước, làm sạch alpha, cắt viền, đăng ký tranh lab lên ảnh màn 1, tìm COghe, ghi `layout.txt`.
   Bộ quả cầu (`S4_SPHERE_CLOSED`, `S4_SPHERE_OPEN`, `S5_COGHE_RISE`) được dời và co giãn chung một phép biến đổi, đo trên
   quả cầu đóng, về đúng chỗ runtime đặt quả cầu (thân rộng 0,479 canvas, tâm x 0,499, đáy y 0,888); ba lớp phải vẽ cùng
   khung đăng ký với nhau.
3. Nhịp và vị trí từng cảnh nằm trong các hàm `Fall`, `Impact`, `Arrival`, `Sphere`, `Emerge`, `Kneel`, `CloseUp`,
   `Place`, `Seated` của `COgheIntro.cs`.

Nhạc intro: `Tools/audio/synth_coghe_audio.py <out_dir> --intro` (tổng hợp bằng code, điểm nhấn khớp thời gian ở trên).

## Dung lượng (đo trên APK Android Spatial, 30/09/2026)

| | APK |
| --- | ---: |
| Không có intro | 41,40 MB |
| Có intro | 43,84 MB |
| **Tăng thêm** | **+2,33 MB** (mục tiêu ≤ 5 MB) |

Trong bản build: 18 lớp tranh ASTC 8×8 ≈ 2,9 MB chưa nén (nền 267 KB/cảnh), nhạc intro 140 KB, shader 8 KB. Tranh chỉ
được nạp khi intro chạy và giải phóng ngay sau đó. Lưu ý khi thay tranh: đừng bật mipmap — trên Android texture không
lũy thừa 2 có mipmap bị lưu không nén (tranh lab từng lên 5,5 MB vì vậy).

## Kiểm tra

- `COgheIntroTests` (PlayMode): đủ 18 lớp + layout + shader + nhạc; không tự phát trong test; phát trên màn 1 thì game
  tạm dừng, nhạc game nhường, tự kết thúc và trả game đang chạy; bỏ qua nhảy đúng tới đoạn cuối; xem lại từ màn tạm dừng
  cũng trả game đang chạy; chỉ khớp màn 1.
- `RenderIntroFrames` (explicit): khung xem trước ở 1080×2340 và 1080×1920 (`Artifacts/Intro/`), đặt
  `COGHE_INTRO_FPS=30` để xuất khung cho video.
