# COghe — âm thanh bản đầu

30/09/2026 · Theo yêu cầu của Mrk: một bản nhạc nền chung cho cả bản này, SFX đơn giản mà tinh; cái nào chưa ổn thì
test rồi thay sau. Game hybrid casual, không cần phức tạp.

Cảm giác: chill, sâu, hơi cô đơn trong phòng lab yên; sinh vật nhỏ, tò mò, thích chơi, thân thiện.
Cách làm: **lab lạnh + sinh vật ấm**. Lab là room tone thấp, tiếng máy chính xác và khoảng lặng; sinh vật có tiếng
nhầy mềm và một "giọng" phi ngôn ngữ nhỏ (ư?, líu lo). Không tiếng gắt; tiếng lấp lánh cao chỉ dành cho phần thưởng.

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
| `ambience_lab_loop` | Suốt màn | Room tone lab (thông gió, rè điện nhẹ), 30 s. |
| `creature_crawl_loop` | Khi sinh vật bò/leo | Âm lượng và cao độ theo tốc độ thật của cơ thể đang chọn. |
| `mech_machine_loop` | Khi cơ quan đang chạy | Theo tổng tốc độ các ray, thang đang chạy, bánh răng có điện. |
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
| `mech_latch` | Ray chốt, bàn xoay khoá nấc | |
| `mech_gear_mesh` | Bộ bánh răng vừa khớp | |
| `mech_pad_on` / `mech_pad_off` | Nút có tải / hết tải | |
| `mech_lift_ding` | Thang tới nơi | |
| `game_level_start` | Vào màn | |
| `game_win` / `game_fail` | Thắng / thua | |
| `game_retry` | Làm lại / Thử lại | |
| `ui_tap` | Mọi nút HUD | |
| `lab_far_beep/clink/thud` | Ngẫu nhiên 25–55 s một lần, rất nhỏ | Tiếng xa trong lab. |

## Cài đặt

Màn hình **Tạm dừng** có hai nút: *Nhạc nền: bật/tắt* và *Âm thanh: bật/tắt* (lưu giữa các lần chơi).

## Kỹ thuật

- `COgheAudio` (`Assets/_Game/Venom/Runtime/Audio/`) tự khởi động khi chạy game, sống xuyên các màn (nhạc không bị cắt),
  chỉ gắn vào catalog Spatial. Nó **chỉ đọc** trạng thái: số lệnh đã nhận (`COgheControlFeedback.CommandCount`), số
  phần cơ thể, ray/chốt, nút tải, bánh răng, thang, bàn xoay, dây đu, ống, thắng/thua. Không đổi vật lý, input, đường đi
  hay cơ quan.
- Giới hạn lặp theo từng tiếng và pool 10 nguồn phát: 32 hạt hay 4 phần cùng bò không làm loạn tiếng.
- Mobile: nhạc Vorbis stream; room tone nén trong RAM; SFX ngắn giải nén khi nạp. Tổng file ≈ 3 MB.
- Test: `COgheAudioTests` (PlayMode) — nạp đủ clip, một nguồn nhạc xuyên màn, sự kiện đúng khi giải thật màn 23
  (dây), 42 (Q, ống, nhập), 44 (nút, bánh răng, chốt), Retry là một tiếng tua chứ không phải "nhập", tắt âm thanh thì
  không phát.

## Tạo lại

Toàn bộ âm thanh bản đầu được tổng hợp bằng code (không dùng sample có bản quyền):

```
python3 -m venv .venv && .venv/bin/pip install numpy scipy soundfile
.venv/bin/python Tools/audio/synth_coghe_audio.py
```

Script ghi đè các file trong `Resources/COgheAudio/`; seed cố định nên chạy lại cho kết quả như cũ. Đổi nhạc cụ, hợp
âm, nhịp hay mix ngay trong script.

## Chưa làm / nên làm tiếp

- Nghe và chỉnh mix trên loa OPPO và tai nghe (bản này cân bằng bằng số đo: đỉnh, RMS, phổ, mối nối loop).
- Thay dần bằng âm thanh thu/soạn thật: ưu tiên nhạc nền và giọng sinh vật (hai thứ làm nên bản sắc).
- Rung (haptic) — chưa có.
