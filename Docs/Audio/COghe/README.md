# COghe — âm thanh bản đầu

30/09/2026 · Theo yêu cầu của Mrk: một bản nhạc nền chung cho cả bản này, SFX đơn giản mà tinh; cái nào chưa ổn thì
test rồi thay sau. Game hybrid casual, không cần phức tạp.

Cảm giác: chill, sâu, hơi cô đơn trong phòng lab yên; sinh vật nhỏ, tò mò, thích chơi, thân thiện.
Cách làm: **lab lạnh + sinh vật ấm**. Lab là tiếng máy chính xác và khoảng lặng; sinh vật có tiếng nhầy mềm và một
"giọng" phi ngôn ngữ nhỏ (ư?, líu lo). Không tiếng gắt; tiếng lấp lánh cao chỉ dành cho phần thưởng.

Sửa theo Mrk (30/09/2026, lần 2): bỏ room tone (thông gió, rè điện); nhạc nền to hơn (0,50 → 0,65); thêm tiếng cơ quan
đang chạy, tiếng "tới đích" khi cơ quan chạy tới đầu, và tiếng khối trượt trên sàn. Mọi tiếng nằm trong dải loa điện
thoại phát được (≥ 250 Hz là chính): bản đầu có tiếng máy ở 98 Hz, tiếng bò/đáp/chốt quá trầm nên trên điện thoại gần
như không nghe thấy.

## Nghe nhanh (không cần mở Unity)

- Nhạc nền: `Assets/_Game/Venom/Resources/COgheAudio/music_lab_loop.ogg`.
- Tất cả SFX nối liền: [`preview-sfx-reel.ogg`](preview-sfx-reel.ogg) (79 s, mỗi tiếng cách 0,7 s), theo thứ tự:
  ack 1–3, grab, hm, curious 1–2, happy, land, split, merge, merge_full, tube_in, tube_out, swing, exit, latch,
  arrive, pad_on, pad_off, gear_mesh, lift_ding, ui_tap, level_start, retry, win, fail, far beep/clink/thud, rồi 4 s
  tiếng bò, 4 s tiếng mô-tơ, 4 s tiếng khối trượt.

## Thay một âm thanh

Mọi file nằm ở `Assets/_Game/Venom/Resources/COgheAudio/`, game gọi theo **tên file**. Muốn thay: bỏ file mới (`.ogg`,
`.wav` hoặc `.mp3`) cùng tên (đuôi có thể khác, nhưng chỉ giữ một file cho mỗi tên), không cần sửa code. Thiết lập
import (nhạc stream, SFX giải nén khi nạp) tự áp theo tên: `COgheAudioImport`.

Âm lượng mặc định nằm ở đầu `COgheAudio.cs` (nhạc, room tone, tiếng bò, tiếng máy) và tại chỗ gọi `Play(...)` cho từng
sự kiện.

## Danh sách

| File | Khi nào | Ghi chú |
| --- | --- | --- |
| `music_lab_loop` | Suốt game, liền mạch giữa các màn | 160 s, 72 BPM, piano nỉ + pad ấm + celesta; motif sinh vật D–F#–A–G# ở vòng giữa; vòng cuối thưa hơn (cô đơn). Nhỏ lại khi tạm dừng và khi thắng. |
| `creature_crawl_loop` | Khi sinh vật bò/leo | Âm lượng và cao độ theo tốc độ thật của cơ thể đang chọn. |
| `mech_motor_loop` | Cơ quan đang vận hành: cửa, cổng, sàn nâng, thang, bàn xoay, bộ bánh răng có điện | Mô-tơ bánh răng nhỏ. To nhỏ theo tốc độ thật; bộ bánh răng có điện mà đầu ra đã tới thì chỉ rì nhỏ. Pan theo vị trí vật đang chạy. |
| `block_slide_loop` | Khối trượt trên sàn: thùng, xe, khối trên ray, xe bánh răng, bậc ngăn kéo, thùng rời bị đẩy | Tiếng ma sát mềm, to nhỏ theo tốc độ thật. |
| `mech_arrive` | Cơ quan chạy tới một đầu (cuối hoặc về đầu) sau khi đi ≥ 1 cm | "Cộp + tinh" — biết cơ quan đã tới đích. Thang dùng tiếng ding riêng. |
| `creature_ack_1..3` | Mỗi lệnh được nhận (chạm đi, chạm cơ quan) | "ư?" — sinh vật trả lời. |
| `creature_grab` | Bắt đầu kéo/đẩy tay nắm, nắm vật | |
| `creature_hm` | Tay nắm không tới được / bị chặn | "hm?" — phản hồi, không trách. |
| `creature_land` | Rơi rồi chạm đất; đáp sau khi đu | Nhỏ hay lớn theo tốc độ rơi. |
| `creature_split` | Tách qua Q | |
| `creature_merge` / `creature_merge_full` | Hai phần nhập / cơ thể liền lại đủ | |
| `tube_in` / `tube_out` | Vào / ra ống | |
| `creature_swing` | Bắt đầu đu dây | |
| `creature_exit` | Phần đầu tiên chui ra lỗ thoát | |
| `creature_curious_1..2` | Đứng yên lâu (16–28 s một lần) | Ngó quanh, ngân nga. |
| `creature_happy` | Nhà của COghe: Cho ăn, Chơi cùng | |
| `mech_latch` | Bàn xoay khoá nấc | |
| `mech_gear_mesh` | Bộ bánh răng vừa khớp | |
| `mech_pad_on` / `mech_pad_off` | Nút có tải / hết tải | |
| `mech_lift_ding` | Thang tới nơi | |
| `game_level_start` | Vào màn | |
| `game_win` / `game_fail` | Thắng / thua | |
| `game_retry` | Làm lại / Thử lại | |
| `ui_tap` | Mọi nút HUD | |
| `lab_far_beep/clink/thud` | Ngẫu nhiên 25–55 s một lần, rất nhỏ | Tiếng xa trong lab (bíp, lọ thuỷ tinh, cửa). |

## Cài đặt

Màn hình **Tạm dừng** có hai nút: *Nhạc nền: bật/tắt* và *Âm thanh: bật/tắt* (lưu giữa các lần chơi).

## Kỹ thuật

- `COgheAudio` (`Assets/_Game/Venom/Runtime/Audio/`) tự khởi động khi chạy game, sống xuyên các màn (nhạc không bị cắt),
  chỉ gắn vào catalog Spatial. Nó **chỉ đọc** trạng thái: số lệnh đã nhận (`COgheControlFeedback.CommandCount`), số
  phần cơ thể, ray/chốt, nút tải, bánh răng, thang, bàn xoay, dây đu, ống, thắng/thua. Không đổi vật lý, input, đường đi
  hay cơ quan.
- Giới hạn lặp theo từng tiếng và pool 10 nguồn phát: 32 hạt hay 4 phần cùng bò không làm loạn tiếng.
- Mobile: nhạc Vorbis stream; SFX ngắn giải nén khi nạp. Tổng file ≈ 3 MB.
- Test: `COgheAudioTests` (PlayMode) — nạp đủ clip, một nguồn nhạc xuyên màn, sự kiện đúng khi giải thật màn 13
  (thang, thùng rời trượt), 21 (xe và thùng trượt, tới đích, không có mô-tơ), 23 (dây), 42 (Q, ống, nhập), 44 (nút,
  bánh răng, cửa và cầu chạy mô-tơ, xe trượt, tới đích), Retry là một tiếng tua chứ không phải "nhập", tắt âm thanh thì
  không phát.

## Tạo lại

Toàn bộ âm thanh bản đầu được tổng hợp bằng code (không dùng sample có bản quyền):

```
python3 -m venv .venv && .venv/bin/pip install numpy scipy soundfile
.venv/bin/python Tools/audio/synth_coghe_audio.py
```

Script ghi đè các SFX trong `Resources/COgheAudio/`. Nhạc nền chỉ tạo lại khi thêm `--music` (bản trong repo là bản đã
nghe duyệt; nhạc dùng dòng ngẫu nhiên riêng nên sửa SFX không làm đổi nhạc). Đổi nhạc cụ, hợp âm, nhịp hay mix ngay
trong script.

## Chưa làm / nên làm tiếp

- Nghe và chỉnh mix trên loa OPPO và tai nghe (bản này cân bằng bằng số đo: đỉnh, RMS, phổ, mối nối loop).
- Thay dần bằng âm thanh thu/soạn thật: ưu tiên nhạc nền và giọng sinh vật (hai thứ làm nên bản sắc).
- Rung (haptic) — chưa có.
