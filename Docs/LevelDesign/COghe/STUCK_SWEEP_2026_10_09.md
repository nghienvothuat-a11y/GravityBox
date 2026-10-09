# Quét kẹt bằng chạm ngẫu nhiên (09/10/2026)

Mrk cho người chơi test màn 8 (09/10/2026): chạm thẳng lỗ thoát lúc đầu thì COghe xuống rãnh và kẹt; một lần khác COghe
rơi ra khỏi hộp. Sau khi sửa màn 8, cả 60 màn được chạy lại bằng cùng một kiểu chạm ngẫu nhiên.

## Cách quét

`COgheLevel07ProbeTests.RandomTapsSweep` (Explicit). Mỗi vị trí chạy `COGHE_PROBE_RUNS` lượt (mặc định 3). Mỗi lượt:

1. 20 lần chạm ngẫu nhiên trong khung hộp; cứ lần thứ 2 trong mỗi 4 lần thì chạm lỗ thoát. Giữa các lần chờ 0,5–4 giây.
2. Hai loại lỗi:
   - **Ra ngoài hộp:** màn báo "ra ngoài vỏ hộp".
   - **Kẹt:** thử 10 điểm đi được (giữa và góc các mặt cố định, bám được, hướng lên; tránh lỗ cắt) và giữa mọi nhánh ống. Không
     lần nào làm COghe dịch quá 3 cm thì là kẹt.

Kết quả ghi ở `Artifacts/L07/sweep.txt`. Khi kẹt, log thêm:
- thứ ở gần COghe;
- trạng thái ống;
- ảnh cận cảnh (`ShotClose`);
- `COGHE_PROBE_STUCKCLIP=1` để có thêm chuỗi ảnh cận cảnh lúc chạm thử sau khi kẹt.

`COGHE_PROBE_LEVELS=16,27` chỉ chạy các vị trí đó. Hạt ngẫu nhiên cố định theo màn và lượt, nên lỗi lặp lại được.

## Kết quả và sửa

Không màn nào để COghe ra ngoài hộp, sau khi đã sửa màn 8.

| Vị trí (nội dung) | Kẹt ở đâu | Sửa |
| --- | --- | --- |
| 8 (07) | Rơi ra ngoài qua hai đầu hố; kẹt trên đáy hố trơn | Bịt hố, đáy hố bám được (xem `SpatialPilot/CHAPTER1_HOOK_REBUILD.md`) |
| 16 (14) | Dưới chân ống chữ U, chỗ ống cách sàn 1–9 cm | Đệm trơn dưới chân ống, như N43 |
| 27 (N23) | Trong khe 7 cm dưới khay đáp B | Khay thành khối đặc, đáy cách sàn 8 mm; mặt khay vẫn ở −.20 |
| 51 (N43) | Một hạt lọt vào miệng ống từ phía trước, thân nằm cạnh ống, vách ống ở giữa: hạt không về được, thân không đi được | `COgheTubeNetwork.ReleaseSnaggedParticles`: phần ít hạt trong miệng ống quá 1 giây mà không vào ống thì về giữa thân |
| 28 (21) | COghe nặng hơn thùng (0,096 so với 0,040): đứng lên khay đối trọng là khay chìm 6,6 cm và cầu hạ; vách hố trơn cao hơn 5 cm nên không ra được | Chờ Mrk chọn: thêm dải bám (hết kẹt, nhưng màn giải được không cần thùng), hoặc làm khay để COghe không đứng lên được |

Các cảnh báo "kẹt" ban đầu ở 10 màn thùng là do bộ quét: sàn của màn thùng là một mặt có lỗ thoát cắt sẵn. Màn 19 kẹt ở ngã
ba ống: chạm một nhánh là đi tiếp, đúng thiết kế.
