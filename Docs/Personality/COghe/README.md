# COghe — tính cách: animation tương tác trong màn và trong nhà

30/09–01/10/2026 · Theo yêu cầu của Mrk: xây dựng cá tính cho sinh vật (tò mò, vui vẻ, hài hước) qua animation tương tác.
Kế hoạch và các quyết định: `PLANS/COGHE_PERSONALITY_PLAN.md` (workspace Buzz). Concept và icon đồ vật do Codex vẽ
(`OUTBOX/COGHE_HOME_ITEMS_2026_09_30`); model, animation, âm thanh, tích hợp do Claude làm.

Xem nhanh: [`levels-preview.mp4`](levels-preview.mp4) (40 s: vẫy tay, 8 hình, tan chảy, gõ kính, ngủ gật, cơn bực),
[`home-preview.mp4`](home-preview.mp4) (72 s: đồ bật ra, COghe chơi từng món, phản ứng khi chạm, dỗi),
[`shapes.jpg`](shapes.jpg), [`home-screens.jpg`](home-screens.jpg) (ảnh chụp bản Mac: nhà, menu đồ vật, hình mờ, hai trò chơi).
Video có tiếng (tiếng của vở diễn + nhạc nền), render từ Unity.
Đợt 01/10 (Mrk): [`home-round3.mp4`](home-round3.mp4) (26 s, quay từ bản Mac: xoay phòng, trò chơi bóng, Feed bi thép,
Zoom in khi COghe ăn), [`home-round3.jpg`](home-round3.jpg).

## Nguyên tắc

- **Không xương, không clip animation, không texture.** COghe là 32 hạt vật lý và lớp da (metaball) được dựng lại mỗi frame.
  Mọi animation là "vở diễn" viết bằng code trên lớp da; mỗi hình biến hình chỉ là vài chục điểm dữ liệu.
- **Trong màn: chỉ là phần nhìn.** Lớp da rời thân rồi quay về; không ghi vào hạt, lực hay trạng thái puzzle. Test chứng minh
  quỹ đạo hạt giống hệt khi có và không có diễn. Riêng cơn bực giữ điều khiển (Mrk chốt).
- **Trong nhà: đi lại thật** (không có puzzle), bằng hệ di chuyển sẵn có.
- Một vở diễn một lúc; chỉ khi thân liền một khối (Mrk chốt).

## Trong màn

| Vở diễn | Khi nào | Mô tả | Âm thanh |
| --- | --- | --- | --- |
| Vẫy tay | Lần đầu đứng yên ~6,5 s trong màn | Thân mọc thành bàn tay, lòng bàn tay hướng camera, vẫy từ cổ tay | "hi-i!" |
| Hình hài | Đứng yên, lần lượt | Tim (màn 2), sao (4), dấu hỏi (6), nấm (8), người tuyết (12), like (16), tên lửa (22), ô (28). Mở ẩn: hình mới hiện đầu tiên ở đúng màn mở | "ta-da" |
| Tan chảy | Đứng yên | Chảy thành vũng, gợn sóng, bật lại hơi cao rồi lắc về | "uuuh" + "pốc" |
| Gõ kính | Có vách kính gần (≤ 12 cm) | Nghiêng về kính, xúc tu gõ 3 cái | "cộc cộc cộc" |
| Ngủ gật | Để yên ≥ 45 s | Xẹp xuống, thở chậm, bong bóng ngáy | tiếng ngáy khẽ |
| **Cơn bực** | Bấm ≥ 6 lần trong 3 s | Người chơi mất điều khiển; khi thân đứng yên: nhún tức, bật nhảy lộn một vòng, bẹp vào vách kính bên cạnh, trượt xuống, nhảy về chỗ cũ, lắc "hừ". 3,1 s rồi trả điều khiển; 25 s mới bực lại | hừ, vút, bẹp, trượt, hừ |

- Lệnh của người chơi dừng mọi vở diễn ngay (0,16 s), trừ cơn bực.
- Chỉ tính chạm thật trên màn hình: bản giải tự động và proof không bao giờ kích hoạt cơn bực.
- Không diễn khi ở trong cơ quan, đang nắm vật, trên tường/dốc, hoặc thân đang tách mảnh.

## Main menu: quái vật (02/10, Mrk)

Màn mở đầu gây chú ý: menu vừa hiện (~1,2 s) là COghe diễn, sau đó cứ 3 vở diễn ở menu lại có một lần. Dài 6,6 s.
Xem: [`monster-menu.mp4`](monster-menu.mp4) (quay từ bản Mac, có giao diện menu và tiếng), [`monster-menu.jpg`](monster-menu.jpg).

| Lúc | Diễn |
| --- | --- |
| 0–0,9 s | Chất lỏng rung, phồng, gai đâm ra ("gừ" trầm) |
| 0,8–1,7 s | Vọt lên thành cột rồi nặn thành quái vật: ngực vai to, tay chất lỏng có vuốt, xúc tu sau lưng; mắt trắng xếch và hàm răng hiện ra |
| 1,9–2,35 s | Lấy đà: ngửa ra, hít mạnh, hé miệng, thè lưỡi |
| 2,35–3,3 s | Há to mồm, đầu phình to và chồm về phía người xem, hai tay vươn tới, GẦM; camera dí sát mặt và rung |
| 3,8–5,6 s | Ngả người, hai tay ôm bụng, cười khằng khặc: hàm đóng mở theo từng tiếng "khặc", vai nảy, đầu gật và lắc kiểu đắc ý, mắt híp |
| 5,9–6,6 s | Tan về COghe ("pốc") |

- Như mọi vở diễn: chỉ là lớp da, không hạt nào di chuyển. Mắt, răng, miệng, lưỡi (`COgheMonsterFace`) nằm đúng trên mặt da
  (tìm trong trường của lớp da mỗi khung) và chỉ có trong vở này — ngoại lệ Mrk cho với quy tắc "không mắt, không răng".
- Hình lấy cảm hứng từ Venom nhưng là thiết kế riêng, không dùng logo hay chi tiết nhận diện của Marvel.
- Camera menu: khung cả con quái vật, khi gầm thì dí vào miệng (giữa logo và nút Play) và rung (`COgheProductUI.FrameShowcase`).

## Trong nhà (mở ở màn 10)

- **Phòng:** sàn 0,9 × 1,68 m, tường sau màu mint, vách kính hai bên như hộp màn chơi. Đồ cao đứng phía sau, đồ thấp phía
  trước để camera luôn thấy COghe chơi; lối đi ở giữa. Camera thấy cả phòng và nghiêng lại gần món đang chơi hoặc món đang
  xem trước. Đồ vật dựng theo đơn vị W = 10 cm (bề ngang thân COghe khi thả lỏng, đo trên da thật; brief ban đầu ghi 7 cm).
- **Menu chính:** vẫn dùng phòng này nhưng ẩn; COghe chỉ diễn tại chỗ (vẫy, biến hình, tan chảy), không đi lại.
- **Tự do:** COghe tự đi lang thang, ghé chơi đồ đã mở, thỉnh thoảng vẫy tay/biến hình. Người chơi chạm sàn thì nó đi tới
  (tạm dừng tự do vài giây); Play cho một phản ứng vui như khi chạm vào nó.
- **Feed (01/10, Mrk):** mỗi lần bấm ném 3 viên bi thép nhỏ (đường kính 2,2 cm) từ phía người chơi vào phòng, hướng và chỗ
  rơi ngẫu nhiên. Bi là vật lý thật: nảy, lăn, va kính và đồ đạc rồi dừng (không bao giờ đụng vào thân COghe). Viên nào dừng
  thì COghe đi tới, vươn xúc tu cuốn bi về, nuốt "ực" (thân phồng lên rồi xẹp), ăn hết thì nhảy vui. Tối đa 9 viên trong
  phòng. Tiếng bi kêu "keng" theo lực va.
- **Xoay phòng (01/10, Mrk):** kéo ngang để xoay góc nhìn quanh phòng như trong màn chơi (xoay được cả vòng). Khi nhìn từ phía
  sau, vách sau tự ẩn để không che phòng; khung hình tự thu phóng để vẫn thấy cả phòng.
- **Zoom in (01/10, Mrk):** nút thứ tư ở thanh dưới. Camera tiến sát COghe và đi theo nó (cả khi nó đang chơi đồ); bấm
  "Zoom out" để về toàn cảnh.
- **Chạm vào COghe:** 6 phản ứng (nhột, nảy, biến tim, lăn, bẹp, né). Chạm dồn dập (4 lần/3 s) thì dỗi: bò vào góc xa, quay
  đi một lúc rồi "hừ" và quay lại.
- **Menu đồ vật (Items):** 14 món với icon của Codex; đã mở ghi "Play" (chọn: COghe tới chơi), chưa mở có khóa + "Level N"
  (chọn: hiện hình mờ của món đó ngay chỗ đặt, camera nghiêng tới).
- **Món mới:** lần đầu vào nhà sau khi mở, các món mới lần lượt "bật" ra tại chỗ và COghe chạy tới chơi thử món mới nhất.

| Màn | Món | COghe làm gì |
| --- | --- | --- |
| 10 | Bóng (món đầu tiên, Mrk đổi 01/10) | háo hức lắc lư, đẩy bóng lăn một vòng rồi về (bóng lăn đúng theo quãng đường), hai xúc tu ôm bóng tung lên cao, chạy vào đỡ, đội đầu hai lần, đánh đầu bóng ra sau, nhảy biến thành trái tim trong khi bóng lăn về chỗ |
| 12 | Tạ | hai xúc tu nắm thanh, nâng ba lần, gồng |
| 14 | Đệm ngủ | nhảy lên nệm, cuộn tròn ngủ, thở, bong bóng ngáy |
| 16 | Gương | tạo dáng: biến hai hình trước gương |
| 18 | Xích đu | ngồi lên, đu qua lại |
| 20 | Cầu trượt | leo thang, trượt xuống về phía người chơi |
| 23 | TV | ngồi xem (hình trên màn chuyển động), nhảy lên phấn khích |
| 26 | Đàn gõ | xúc tu gõ một giai điệu 9 nốt |
| 30 | Bạt nhún | nhún 4 lần cao dần, lần cuối lộn vòng |
| 34 | Võng | nằm võng đung đưa, ngủ |
| 38 | Bánh xe | chạy trong bánh xe quay |
| 42 | Bể cá | áp vào kính ngắm cá bơi |
| 46 | Đèn chiếu bóng | biến hình trước đèn |
| 50 | Cúp | nhảy mừng quanh cúp |

## File

- Tính cách: `Assets/_Game/Venom/Runtime/Personality/COghePersonality.cs` (trong màn), `COghePersonality.Home.cs` (trong nhà).
- Vẽ vở diễn: `Assets/_Game/Venom/Runtime/VenomLifeAnimation.Acts.cs` (hình dạng, cơn bực, tư thế trong nhà),
  `VenomLifeAnimation.Monster.cs` (quái vật ở menu) và mặt của nó `Personality/COgheMonsterFace.cs`.
- Nhà: `Assets/_Game/Venom/Runtime/Home/` — `COgheLowPoly` (bộ dựng mesh low-poly), `COgheHomeItems` (14 món theo
  `ITEMS.json` của Codex), `COgheHomeRoom` (phòng, bố trí, mở khóa, hình mờ). `VenomHabitat` dựng phòng khi vào nhà.
- Menu đồ vật và camera nhà: `Assets/_Game/Venom/Runtime/Product/COgheProductUI.Items.cs`.
- Icon: `Assets/_Game/Venom/Resources/COgheHome/Icons/<ID>.png` (256 px, ASTC 6×6).
- Âm thanh: `Tools/audio/synth_coghe_audio.py --personality` (11 tiếng vở diễn), `--home` (6 nốt đàn gõ), `--feed` (bi
  thép, tiếng nuốt), `--monster` (quái vật: gừ, vọt lên, hít, gầm, cười khằng khặc).
- Bi thép: `Assets/_Game/Venom/Runtime/Home/COgheFeedBalls.cs` (ném, va chạm chỉ cho bi, nằm yên); ăn bi và trò chơi bóng
  trong `COghePersonality.Home.cs`. Xoay/zoom: `COgheProductUI.Items.cs` (`FrameHome`).

## Thêm hình hoặc món mới

- Hình: thêm vào `COgheShape`, `ShapeUnlocks` (màn mở) và một `case` trong `BuildShape` (điểm theo bán kính thân: x phải,
  y lên, z về camera; nét mảnh dùng `Straight`/`LimbQ`).
- Món: thêm vào `COgheHomeItems.Catalog` + một `case` dựng model (đơn vị W = 7 cm), chỗ đặt trong `COgheHomeRoom.Layout`,
  trò chơi trong `COghePersonality.PlayPose`, icon 256 px trong `Resources/COgheHome/Icons`.

## Dung lượng

Đồ vật dựng bằng code nên không có file model/texture. Theo báo cáo build Android: 14 icon × 29 KB = 406 KB, 17 tiếng mới
≈ 100 KB; tổng thêm ≈ 0,5 MB. CPU: vở diễn chỉ nội suy ≤ 38 điểm và thêm vài ống xúc tu trên lưới da vốn có; trong nhà
thêm ~14 món low-poly (vật liệu dùng chung). Chưa đo trên OPPO.

## Kiểm tra

- `COghePersonalityTests`: diễn không đổi vị trí hạt nào; cơn bực khóa rồi trả điều khiển, thân ở chỗ cũ, có thời gian chờ;
  vở diễn tự bắt đầu khi để yên và dừng khi có lệnh.
- `COgheHomeTests`: chỉ hiện món đã mở, hình mờ cho món khóa; COghe tự đi, chơi đồ, không rời sàn; chạm có phản ứng, chạm
  dồn thì dỗi.
- Quái vật: không đổi hạt nào; có mặt khi gầm, mặt mất khi xong (`MonsterGrowsAFaceAndLosesIt`); menu mở là quái vật,
  mặt nằm trong khung hình (`TheMenuOpensWithTheMonster`).
- Explicit: `RenderPersonalityActs` (khung từng vở diễn), `RenderHome` (toàn cảnh, nhìn từ trên, từng trò chơi),
  `RenderMonsterStills`, `RenderMenuMonster`. Bản Mac: `-coghe-monster-reel <thư mục>` quay menu thật (PNG 30 khung/s + `sounds.txt`).
