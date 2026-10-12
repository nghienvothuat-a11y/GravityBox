# COghe — 5 màn bò trên kính (G1–G5) — bản thiết kế

Trạng thái: **Nháp**, chờ duyệt (12/10/2026). Chưa dựng màn chơi: hình là blockout dựng trong Unity
(`GlassCrawlMocks`), không phải scene chơi được.

Yêu cầu (Mrk, 12/10/2026): *"thiết kế thêm 5 màn mà COghe có thể bò lên kính, nhưng vẫn phải điều khiển các cơ quan
mới có thể đến được lỗ thoát. Có thể thiết kế theo nhiều hướng trong không gian 3D."*

![Tổng quan](GLASS_CRAWL_overview.png)

## Luật chung cho 5 màn

- Kính bò được, kể cả trần. Băng hologram tím (băng dính trơn) là mặt trơn thật: không bám. Hình và vật lý khớp nhau.
- Lỗ thoát luôn bị khoá bởi cơ quan cuối. Thứ tự giải do khoá quyết định (RequiredRail, chốt), không do tầm với.
- Cơ quan đặt trên sàn và kệ (loại đã có: TapRail, nút nặng, chốt, thang máy, tấm trượt, Q). Tường và trần là đường đi.
  Chưa có tay nắm làm việc từ mặt đứng hoặc trần, nên không dùng.
- Rơi ở đâu cũng leo lại được; không có chỗ kẹt không lối ra.
- Hộp 40 cm, sàn 0,8 × 0,6 m. Băng chân tường cao 7 cm (COghe cao ~5 cm, với tối đa ~6 cm).
- Dải trơn dọc trên kính trước/sau và trên trần dùng để chặn đường vòng qua kính khi hộp chia đôi.

## Đã kiểm tra trong engine

`GlassCrawlProbe` (Explicit), màn 1, cho mọi tấm kính bám:

| Đích chạm | Kết quả |
| --- | --- |
| Giữa trần | tới (cách 2,6 cm), 28/32 hạt bám trần |
| Góc trần xa | tới (cách 3,2 cm), 29/32 hạt bám trần |
| Thấp trên tường phải | tới (cách 2,4 cm) |
| Cao trên tường sau | tới (cách 2,8 cm) khi trần không nhận chạm |
| Về sàn | tới |

- Màn Spatial ở chế độ `ViewOnly` không cho chạm kính từ phía ngoài, kể cả trần. Bản thử sửa 1 dòng trong
  `VenomCampaign.CanPick` (chưa commit; khi dựng phải kiểm lại các catalog cũ có trần bật `InterceptExterior`):
  `if(Definition.ViewOnly)return !face.ExteriorGlass||face.InterceptExterior||Vector3.Dot(ray.direction,face.Normal)<-.001f;`
  Không màn Spatial nào hiện bật cờ này. Không có dòng này, probe không tới được trần.
- Khi trần nhận chạm, chạm vào phần trên tường sau (nằm sau trần nhìn từ camera) sẽ rơi vào trần. Đặt đích chạm quan
  trọng ở chỗ không bị trần che, hoặc chỉ cho phần trần cần bò nhận chạm.
- Chưa có màn nào dùng lỗ thoát trên trần (`Outward` hướng lên); 5 thiết kế không dùng.

## G1 · Leo kính lấy chìa (nhập môn)

![G1](GLASS_CRAWL_G1.png)

- Học: kính bò được; băng trơn thì không.
- Bố cục: băng chân tường trước, sau, phải. Tường trái để trống: chỗ leo. Kệ 18 cm sát tường trái, tay nắm A (xanh).
  Lỗ thoát chân tường phải, cửa trượt đóng (vạch xanh: A mở).
- Lời giải: chạm kệ (bò tường trái lên kệ) → kéo A (cửa mở) → chạm lỗ thoát.
- Đi tắt: lỗ thoát khoá tới khi kéo A. Phục hồi: rơi thì leo lại tường trái.

## G2 · Kệ cao (luyện: vào từ phía trên)

![G2](GLASS_CRAWL_G2.png)

- Học: chỗ bị băng trơn bao quanh thì vào từ phía trên.
- Bố cục: băng chân mọi tường. Khe leo ở chân tường trái bị tấm trơn che; tay nắm A (xanh) kéo tấm ra. Kệ cao trên tường
  sau có nút P (hồng); kính dưới kệ trơn. P chốt cửa thoát mở (PlusLatch, retain).
- Lời giải: kéo A → leo tường trái, bò theo tường sau, xuống kệ, đứng P → bò sang tường phải, xuống sàn, ra.
- Đi tắt: khe leo đóng tới khi A; cửa thoát đóng tới khi P. Phục hồi: A giữ nguyên, leo lại qua khe.

## G3 · Qua trần nhà (kỹ năng mới: trần)

![G3](GLASS_CRAWL_G3.png)

- Học: treo trên trần để vượt vách ngăn.
- Bố cục: vách trơn chia hai phòng, cách trần 10 cm. Dải trơn trên kính trước/sau và trên trần ngay trên vách. Tay nắm B
  (san hô) trượt tấm bám được dưới trần, phủ dải trơn. Thang máy phòng trái chỉ chạy khi đã kéo B (RequiredRail).
- Lời giải: kéo B → lên thang máy, bấm nút → bò tường sau lên trần, qua tấm, xuống tường phải, ra lỗ thoát.
- Đi tắt: sàn không leo được (băng); vách trơn; dải trơn chặn đường vòng qua kính; thang khoá bởi B.
- Rủi ro: COghe trượt ở mép dải trơn trên trần có thể rơi lên đỉnh vách. Dải trơn đặt hẳn về phía phòng trái để rơi
  luôn về phòng trái; kiểm bằng test đi tắt.

## G4 · Ba kệ (kết hợp)

![G4](GLASS_CRAWL_G4.png)

- Học: chuỗi 3 cơ quan ở 3 độ cao, đi qua 3 mặt và trần.
- Bố cục: A (xanh, sàn) mở khe leo tường trái. Kệ sau-trái có P (hồng) chốt tấm trần phủ dải trơn giữa trần. Kệ phải có
  tay nắm B (san hô) mở cửa thoát. Kính dưới kệ phải trơn: chỉ tới kệ phải qua trần.
- Lời giải: A → leo, xuống kệ trái, đứng P → lên trần, qua tấm, xuống kệ phải, kéo B → xuống tường phải, ra.
- Đi tắt: dải trơn trên trần và kính chia đôi hộp; mỗi bước khoá bước sau. Phục hồi: P đã chốt, leo lại qua khe.

## G5 · Hai nửa trên trần (Boss)

![G5](GLASS_CRAWL_G5.png)

- Học: tách đôi bằng Q; một nửa giữ nút, nửa kia đi đường trên trần.
- Bố cục: Q tách 50/50. Nút P (hồng) trên sàn: tấm trần chỉ phủ dải trơn khi có người đứng (không chốt). Góc sau-trái
  không có băng: chỗ leo. Kệ phải cao có tay nắm B (san hô) mở cửa thoát ở chân tường trái.
- Lời giải: chạm Q → nửa 1 đứng P → nửa 2 leo góc sau-trái, qua trần, xuống kệ phải, kéo B → nửa 2 xuống tường phải về
  sàn, nửa 1 rời P, hợp thể → ra.
- Đi tắt: cả thân không vừa giữ P vừa qua trần, nên phải tách; cửa khoá tới khi B; phải hợp thể mới thắng.
- Cần kiểm chứng: nửa thân bò trần và kéo B được; P đặt xa đường leo để hai nửa không tự hợp thể giữa chừng.

## Khi dựng (sau khi duyệt)

1. Dựng 5 màn bằng builder Plus (kính bám, tấm trơn trên kính đúng chỗ băng, cửa thoát khoá), bước soft-edges vẽ băng
   theo mặt trơn thật.
2. Mỗi màn: test giải, test đi tắt (đi tới cạnh lỗ thoát qua từng mặt kính, không chạm cơ quan), test lang thang, quét
   chạm ngẫu nhiên.
3. Chọn vị trí trong 60 màn sau khi chơi thử.

Ước lượng: khoảng 2 ngày cho 5 màn.
