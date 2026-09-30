# Spatial Plus — kiểm chứng bản dựng

30/09/2026 · nhánh `NewGraphic`. 18 màn dễ + 2 Boss mới, catalog Spatial 50 màn theo [thứ tự đã duyệt](../../LevelDesign/COghe/SpatialPlus20/PLACEMENT.md). Thiết kế, thay đổi khi dựng và lời giải: [SpatialPlus20](../../LevelDesign/COghe/SpatialPlus20/README.md).

Kịch bản chỉ chạm màn hình và chọn phần như người chơi: không dịch chuyển mô, không gán cửa/chốt, không ép thắng.

## PlayMode

**133/133** — `COgheSpatialCampaignTests` (Spatial 01–30, hồi phục khi rơi, Spatial Plus), `COgheSpatialRecoveryTests`, `COgheViewCampaignTests`. [XML](playmode-results.xml).

- Lời giải mẫu 20 màn mới bằng chạm thật, rồi Retry về đầu: **20/20**.
- Đi lang thang tìm chỗ kẹt (mỗi màn mới): **20/20**.
- Ca làm sai / huỷ / quá nhẹ / rời nút giữa chừng / Retry giữa chừng: **6/6**.
- 30 màn cũ ở vị trí mới: **30/30** (scene và ID giữ nguyên; tiêu đề, số trên biển và thứ tự chơi đổi).

Chạy đủ bộ này 5 lần trong lúc làm: `Spatial30FallRecovery` (màn cũ 30, rơi rồi leo lại) trượt 1 lần ở lượt thứ 4 rồi đạt ở lượt 5 và khi chạy riêng — test cũ phụ thuộc thứ tự chạy, không liên quan màn mới; ghi lại để theo dõi. `TracePlusLevel` (chẩn đoán, chỉ chạy khi đặt `COGHE_PLUS_TRACE`) bỏ qua.

Art pass Glass C: 50/50 scene — transform, collider, rigidbody, joint, surface, rail, input và tham số cơ quan giống nhau [trước](physics-before.txt.gz) / [sau](physics-after.txt.gz); definition không đổi.

## Đi lang thang: di chuyển sinh vật theo mọi cách để tìm chỗ kẹt

Từ điểm xuất phát, chạm lần lượt mọi góc (lùi 3,5 cm) và giữa của mọi mặt cố định đi được — sàn, bệ, sàn cao, đỉnh vách — theo đường gần nhất; mỗi lệnh chờ tới nơi hoặc sinh vật dừng. Bỏ qua Q (chạm Q là tách), tay nắm (là kéo) và vùng quanh lỗ thoát. Chạm trúng vật rời (thùng) thì buông. Cuối cùng phải **về lại điểm xuất phát**; nếu lệnh về bỏ cuộc thì chạm một điểm khác rồi về lại, như người chơi. Không về được = kẹt, test trượt tại đó. Sau đó Retry và giải trọn màn.

| Vị trí | Màn | Mã | Lệnh đi | Tới nơi | Đi vòng khi về | Kết thúc |
| --- | --- | --- | ---: | ---: | ---: | --- |
| 13 | Thang chở hàng | E01 | 12 | 8 | 0 | về điểm xuất phát |
| 15 | Hai ống, một đích | E02 | 12 | 2 | 0 | về điểm xuất phát |
| 18 | Giữ cửa cho bạn | E03 | 9 | 3 | 0 | về điểm xuất phát |
| 21 | Cõng thùng | E04 | 12 | 7 | 0 | về điểm xuất phát |
| 23 | Hai nhịp dây | E05 | 16 | 4 | 0 | về điểm xuất phát |
| 26 | Bập bênh | E06 | 10 | 2 | 0 | ra lỗ thoát (đã mở), chạm lỗ là thắng |
| 29 | Chồng hai tầng | E07 | 13 | 9 | 0 | về điểm xuất phát |
| 30 | BOSS · Tháp khối | B1 | 22 | 10 | 0 | về điểm xuất phát |
| 31 | Bánh răng đầu tiên | E08 | 17 | 11 | 0 | về điểm xuất phát |
| 33 | Đủ nặng mới mở | E09 | 11 | 3 | 0 | về điểm xuất phát |
| 36 | Đu rồi luồn | E10 | 19 | 10 | 0 | về điểm xuất phát |
| 37 | Giữ thang cho bạn | E11 | 6 | 3 | 0 | về điểm xuất phát |
| 41 | Khớp một bánh | E12 | 8 | 6 | 0 | về điểm xuất phát |
| 42 | Hai ống, hai nửa | E13 | 16 | 3 | 2 | về điểm xuất phát |
| 44 | Hai tầng răng | E14 | 16 | 4 | 0 | về điểm xuất phát |
| 46 | Người chạy máy | E15 | 9 | 4 | 0 | về điểm xuất phát |
| 47 | Bàn xoay | E16 | 18 | 13 | 1 | về điểm xuất phát |
| 48 | Hai máy nối nhau | E17 | 11 | 5 | 0 | về điểm xuất phát |
| 49 | Ba lớp răng | E18 | 17 | 3 | 0 | về điểm xuất phát |
| 50 | BOSS · Tháp bánh răng | B2 | 19 | 3 | 0 | về điểm xuất phát |

“Tới nơi” thấp là bình thường: nhiều mặt cao chỉ lên được khi đã giải cơ quan. Nhật ký từng màn: `wander-KEY.txt`.

### Chỗ kẹt và đường tắt test tìm ra (đã sửa)

| Màn | Phát hiện | Sửa |
| --- | --- | --- |
| 15 · Hai ống, một đích | Ống ra chạy sát sàn băng ngang phòng 1: đi từ trước ra sau là kẹt dưới ống. Nắp ống ngà: leo lên nắp rồi rơi vào khe sau nắp, không ra được. | Ống dời sát vách ngăn, dựng lên ngay sau miệng; nắp trơn, cách miệng 5 mm. |
| 18, 33 · cửa | Cửa ngà là cái thang: leo cửa lên đỉnh vách, sang phòng thoát không cần giải. | Mọi cửa/nắp/chốt/cửa chớp trơn. |
| 26 · Bập bênh | Chui vào khe hình nêm dưới ván đã nghiêng và kẹt. | Bậc chêm trơn dưới hai đầu hạ; khe dưới ván ≤ 3 cm. |
| 42 · Hai ống, hai nửa | Đoạn ống thấp cạnh miệng làm phần cơ thể kẹt khi đi vòng qua. | Miệng ống quay vào giữa phòng, ống dựng lên sau miệng. |
| 44, 49, 50 · tháp | Nằm trên bậc đầu thì kẹt dưới cửa bậc thang đã nâng; đi chéo từ bậc 3 vào sàn giữa hụt xuống bậc 2 rồi lặp lại; đường đi trèo qua bàn răng/xe bánh răng rồi mắc bánh răng. | Cửa nâng 17 cm; mặt ngà trên bậc 1–2; bàn răng và xe trơn. |

### Khi dựng lời giải (chạy lời giải mẫu)

Khối trượt trên khối mang (21, 29, 30, bậc tháp 49/50) dừng thiếu 8 mm: góc khối vướng mép tấm cuối của mặt tĩnh (tấm dày 8 mm) — khối trên nay nổi 2 mm (ray giữ độ cao). Xe đẩy ra khỏi chỗ đứng (30), đảo làm hụt dây (23), bậc bị kẹp (46), thanh răng đâm bậc (50)… — chi tiết từng màn ở mục “Đã dựng: thay đổi so với thiết kế” trong hồ sơ màn.

### Còn lại, không phải kẹt

- Đi từ sau Q ra trước Q, đường ngắn nhất đôi khi lách qua dưới vỏ Q và dừng ở vách sau Q; chạm sang bên là đi tiếp (có sẵn ở các màn Q cũ). Test đi lang thang tính các ca này là “đi vòng khi về”.
- Chạm nửa xa của ván bập bênh từ cạnh ván: sinh vật có thể dừng cạnh ván; chạm chân ván là lên.
- Lỗ thoát sát sàn (26): đi dọc vách có lỗ thì trượt vào ống ra khi lỗ đã mở — chạm lỗ là thắng.

## Native Mac

**50/50** vị trí 1–50 thắng trong build Mac thật (`COGHE_MOBILE_BENCHMARK`), InputSystem phát chạm, chạy theo thứ tự catalog. [JSON](mac-native-run.json).
Máy Mac16,10 / Apple M4; Mac OS X 26.5.1; Unity 6000.3.19f1; Metal; cửa sổ thực 720×1022; Glass C; cap 60 FPS. Một lượt author replay, thời gian chụp ảnh loại khỏi mẫu. Không dùng số Mac để khẳng định FPS điện thoại; chưa đo OPPO/nhiệt.

| Vị trí | Màn | Nguồn | Kết quả | Hạt ra | Cơ thể | Chạm | Giây | FPS TB | p95 ms | p99 ms | Max ms | Frame >33,3 ms |
| ---: | --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| 1 | Chạm để đi | cũ 1 | đạt | 32 | 1 | 1 | 4.6 | 60.0 | 16.67 | 16.67 | 16.7 | 0 |
| 2 | Leo từng bậc | cũ 2 | đạt | 32 | 1 | 3 | 7.9 | 60.0 | 16.67 | 16.68 | 17.2 | 0 |
| 3 | Nhìn quanh vách | cũ 3 | đạt | 32 | 1 | 3 | 6.2 | 60.0 | 16.67 | 16.69 | 17.1 | 0 |
| 4 | Kéo là mở | cũ 4 | đạt | 32 | 1 | 2 | 6.6 | 60.0 | 16.70 | 16.82 | 16.9 | 0 |
| 5 | Tay kéo trên vách | cũ 5 | đạt | 32 | 1 | 2 | 8.8 | 60.0 | 16.67 | 16.70 | 17.1 | 0 |
| 6 | Một sợi dây | cũ 6 | đạt | 32 | 1 | 5 | 10.9 | 59.9 | 16.74 | 17.28 | 23.2 | 0 |
| 7 | Ghép một nhịp | cũ 7 | đạt | 32 | 1 | 4 | 7.6 | 60.0 | 16.67 | 16.70 | 17.1 | 0 |
| 8 | Đi thang nâng | cũ 8 | đạt | 32 | 1 | 2 | 8.1 | 60.0 | 16.67 | 16.67 | 17.0 | 0 |
| 9 | Nâng rồi kéo | cũ 9 | đạt | 32 | 1 | 6 | 12.8 | 59.6 | 16.67 | 17.06 | 33.3 | 0 |
| 10 | BOSS · Cỗ máy thân quen | cũ 10 | đạt | 32 | 1 | 6 | 18.5 | 58.9 | 16.67 | 33.30 | 33.3 | 1 |
| 11 | Kê một bậc | cũ 11 | đạt | 32 | 1 | 4 | 8.5 | 59.9 | 16.74 | 17.53 | 22.6 | 0 |
| 12 | Khối lớn đi trước | cũ 12 | đạt | 32 | 1 | 7 | 22.0 | 59.9 | 16.71 | 17.17 | 27.6 | 0 |
| 13 | Thang chở hàng | mới | đạt | 32 | 1 | 7 | 14.4 | 58.5 | 16.69 | 33.30 | 33.3 | 1 |
| 14 | Luồn một vòng | cũ 14 | đạt | 32 | 1 | 3 | 15.7 | 54.2 | 33.33 | 33.35 | 33.6 | 50 |
| 15 | Hai ống, một đích | mới | đạt | 32 | 1 | 6 | 30.4 | 52.2 | 33.34 | 33.36 | 34.0 | 125 |
| 16 | Gặp nhau ở ngã ba | cũ 15 | đạt | 32 | 1 | 6 | 33.2 | 59.9 | 16.71 | 17.02 | 32.0 | 0 |
| 17 | Một thành hai | cũ 16 | đạt | 32 | 1 | 8 | 10.6 | 59.5 | 16.67 | 16.97 | 33.3 | 0 |
| 18 | Giữ cửa cho bạn | mới | đạt | 32 | 1 | 8 | 15.9 | 59.5 | 16.67 | 17.20 | 33.3 | 0 |
| 19 | Bạn giữ, mình luồn | cũ 17 | đạt | 32 | 1 | 8 | 28.3 | 59.4 | 16.75 | 27.59 | 33.3 | 2 |
| 20 | BOSS · Hai nửa một máy | cũ 20 | đạt | 32 | 1 | 16 | 31.0 | 59.3 | 16.72 | 32.67 | 33.2 | 0 |
| 21 | Cõng thùng | mới | đạt | 32 | 1 | 5 | 11.3 | 59.9 | 16.69 | 16.86 | 30.3 | 0 |
| 22 | Bám dây sang bờ | cũ 18 | đạt | 32 | 1 | 3 | 2.9 | 60.0 | 16.67 | 16.69 | 17.3 | 0 |
| 23 | Hai nhịp dây | mới | đạt | 32 | 1 | 5 | 4.4 | 60.0 | 16.67 | 16.69 | 16.7 | 0 |
| 24 | Đưa bến lại gần | cũ 19 | đạt | 32 | 1 | 5 | 9.3 | 59.4 | 16.67 | 17.17 | 33.3 | 0 |
| 25 | Kéo đối trọng | cũ 21 | đạt | 32 | 1 | 6 | 10.3 | 58.7 | 16.69 | 33.24 | 33.3 | 2 |
| 26 | Bập bênh | mới | đạt | 32 | 1 | 6 | 8.5 | 59.3 | 16.68 | 32.44 | 33.0 | 0 |
| 27 | Ba mảnh thành đường | cũ 22 | đạt | 32 | 1 | 9 | 28.5 | 59.5 | 16.67 | 17.17 | 33.3 | 1 |
| 28 | Thùng đi thang | cũ 13 | đạt | 32 | 1 | 14 | 33.7 | 58.5 | 16.68 | 33.30 | 33.6 | 6 |
| 29 | Chồng hai tầng | mới | đạt | 32 | 1 | 6 | 11.2 | 60.0 | 16.67 | 16.69 | 17.8 | 0 |
| 30 | BOSS · Tháp khối | mới | đạt | 32 | 1 | 16 | 29.6 | 59.2 | 16.69 | 32.48 | 49.9 | 1 |
| 31 | Bánh răng đầu tiên | mới | đạt | 32 | 1 | 4 | 10.2 | 59.5 | 16.67 | 17.24 | 33.3 | 0 |
| 32 | Đổi tuyến trên vách | cũ 23 | đạt | 32 | 1 | 6 | 21.7 | 55.9 | 33.33 | 33.36 | 33.4 | 42 |
| 33 | Đủ nặng mới mở | mới | đạt | 32 | 1 | 9 | 22.3 | 59.7 | 16.67 | 17.27 | 33.3 | 0 |
| 34 | Hai rồi bốn | cũ 24 | đạt | 32 | 1 | 15 | 25.0 | 59.6 | 16.67 | 16.87 | 33.3 | 1 |
| 35 | Giữ lại phần lớn | cũ 25 | đạt | 32 | 1 | 19 | 21.4 | 59.1 | 16.70 | 33.27 | 33.3 | 2 |
| 36 | Đu rồi luồn | mới | đạt | 32 | 1 | 4 | 10.4 | 60.0 | 16.67 | 16.70 | 16.8 | 0 |
| 37 | Giữ thang cho bạn | mới | đạt | 32 | 1 | 12 | 29.2 | 58.6 | 16.71 | 33.22 | 33.3 | 0 |
| 38 | Đu và luồn | cũ 26 | đạt | 32 | 1 | 10 | 27.7 | 58.0 | 16.77 | 33.34 | 50.4 | 28 |
| 39 | Bốn trạm tiếp sức | cũ 27 | đạt | 32 | 1 | 19 | 45.8 | 58.8 | 16.77 | 33.03 | 33.3 | 0 |
| 40 | BOSS · Hộp cộng hưởng | cũ 30 | đạt | 32 | 1 | 29 | 59.3 | 58.4 | 16.72 | 33.73 | 83.2 | 42 |
| 41 | Khớp một bánh | mới | đạt | 32 | 1 | 5 | 11.9 | 59.8 | 16.73 | 17.29 | 31.9 | 0 |
| 42 | Hai ống, hai nửa | mới | đạt | 32 | 1 | 23 | 43.6 | 59.8 | 16.71 | 17.04 | 33.4 | 1 |
| 43 | Đường ống ba chiều | cũ 28 | đạt | 32 | 1 | 14 | 28.3 | 58.4 | 17.10 | 32.62 | 66.6 | 2 |
| 44 | Hai tầng răng | mới | đạt | 32 | 1 | 11 | 28.6 | 59.1 | 16.67 | 33.28 | 33.3 | 0 |
| 45 | Xưởng lắp cầu | cũ 29 | đạt | 32 | 1 | 16 | 36.5 | 58.8 | 16.70 | 32.43 | 50.0 | 10 |
| 46 | Người chạy máy | mới | đạt | 32 | 1 | 27 | 23.0 | 59.3 | 16.68 | 33.04 | 33.3 | 0 |
| 47 | Bàn xoay | mới | đạt | 32 | 1 | 4 | 11.1 | 59.4 | 16.67 | 17.23 | 33.5 | 1 |
| 48 | Hai máy nối nhau | mới | đạt | 32 | 1 | 5 | 8.0 | 59.5 | 16.67 | 17.25 | 33.3 | 0 |
| 49 | Ba lớp răng | mới | đạt | 32 | 1 | 18 | 38.5 | 58.7 | 16.68 | 32.36 | 50.0 | 15 |
| 50 | BOSS · Tháp bánh răng | mới | đạt | 32 | 1 | 20 | 42.4 | 58.6 | 16.68 | 49.83 | 50.1 | 28 |

### Các lượt native trước lượt cuối

Chạy native lộ ra những gì PlayMode (bước vật lý 120 Hz theo script) không thấy; mỗi lỗi được xử lý rồi chạy lại:

| Lượt | Kết quả | Trượt | Xử lý |
| --- | --- | --- | --- |
| 1 | 46/50 | 13, 15, 29, 35 | 13: chạm nắm thùng lúc sinh vật sát thùng — lời giải lùi về góc khay rồi mới nắm. 15: khúc cua gắt mới của ống ban công làm sinh vật dừng trong ống — trả ống về hình cũ. 29: khối B chốt đúng ngưỡng rồi lùi dưới tải — bỏ chốt cuối của B. 35 (màn cũ 25): sinh vật dừng trên bậc — `Go` của kịch bản chạm lại tối đa 2 lần khi sinh vật dừng giữa đường (như `Walk` đã làm, như người chơi chạm lại). |
| 2 | 46/50 | 13, 37, 39, 46 | 46: điểm đích nằm dưới HUD ở cửa sổ thấp — đích dời ra mép trước của sàn cao. 37, 39: đạt khi chạy lại (dao động thời gian thực của vòng lặp player). |
| 3 (chỉ các màn trượt) | 13, 37, 38, 39, 46 đạt | — | — |
| cuối | xem bảng trên | | Build cuối, cả 50 vị trí theo thứ tự. |

## Ảnh

Mỗi màn mới: `NN-KEY-start.png`, `NN-KEY-action.png` (giữa lời giải), `NN-KEY-won.png` — chụp từ build Mac ở trên.
