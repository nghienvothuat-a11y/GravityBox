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

Màn mở đầu gây chú ý: menu vừa hiện (~1,2 s) là COghe diễn, sau đó cứ 3 vở diễn ở menu lại có một lần. Dài 7,2 s.
Mềm mại như chất lỏng, không dọa, không giọng nói (Mrk sửa lần 2). Xem: [`monster-menu.mp4`](monster-menu.mp4) (quay từ
bản Mac, có giao diện menu và tiếng), [`monster-menu.jpg`](monster-menu.jpg).

| Lúc | Diễn |
| --- | --- |
| 0–1,5 s | Chất lỏng phồng nhẹ, dâng lên thành cột rồi nặn thành dáng Venom; mắt trắng xếch và hàm răng hiện ra |
| 1,3–2,3 s | Quay nghiêng (thấy mặt nghiêng) |
| 1,6–4,5 s | Cả thân lượn uốn éo như chất lỏng; há miệng từ từ, thè lưỡi dài, lưỡi uốn lượn |
| 3,9–4,9 s | Rụt lưỡi, ngậm miệng, ngước nhìn ra xa |
| 4,5–6,6 s | Quay sang nhìn camera, chớp mắt, nghiêng đầu |
| 6,3–7,2 s | Tan về COghe |

- Như mọi vở diễn: chỉ là lớp da, không hạt nào di chuyển. Mắt, răng, miệng, lưỡi (`COgheMonsterFace`) nằm đúng trên mặt da
  (tìm trong trường của lớp da mỗi khung) và chỉ có trong vở này — ngoại lệ Mrk cho với quy tắc "không răng". Trong vở này
  đôi mắt thường của COghe tắt, mặt quái vật thay chỗ.
- Hình lấy cảm hứng từ Venom nhưng là thiết kế riêng, không dùng logo hay chi tiết nhận diện của Marvel.
- Camera menu: khung cả con quái vật, lại gần mặt một chút khi há miệng và nghiêng đầu (`COgheProductUI.FrameShowcase`).
- Tiếng: chỉ tiếng chất lỏng (dâng lên, lưỡi trượt ra) và tiếng "pốc" khi tan.

## Mắt (07/10, Mrk)

Mrk hỏi "Nếu COghe có mắt để diễn tả cảm xúc thì có được không?", duyệt clip so sánh có mắt và không mắt, rồi chốt
"tao chốt phương án có mắt" (bỏ luật "không mắt" trong `STYLE_RULES.md`). Sau lần duyệt đầu, Mrk yêu cầu "mắt đỡ rung" và
"lúc chiến thắng phải cho nó có mắt thể hiện sự vui vẻ". Clip duyệt: `OUTBOX/COGHE_EYES_REVIEW_2026_10_07` (workspace Buzz).

- Hai mắt hoạt hình trắng viền tối, nằm trên lớp da phía hướng về camera, nên co, dẹt, nhảy theo thân. Mỗi phần sau khi tách
  có đôi mắt riêng (tối đa 4 phần, phần dưới 4 hạt thì không có). Cỡ mắt theo bề ngang phần đó.
- Tự chớp mắt 2,2–5,7 s một lần, đưa mắt nhìn quanh, và nhìn vào thứ COghe đang làm: tay nắm, viên mồi, quả bóng, TV, bể cá,
  chỗ bị chặn.
- Cảm xúc (`COghePersonality.EyeMood`), đọc từ việc COghe đang làm, không thêm trạng thái mới:

| Cảm xúc | Khi nào | Mắt |
| --- | --- | --- |
| Vui | Được chạm, ăn xong, trả lời bonus đúng, thắng màn và cả màn ăn mừng | Cười híp (∩), to hơn 15% lúc ăn mừng |
| Yêu | Ôm, xong cả 3 câu bonus | Trái tim hồng |
| Buồn | Trả lời bonus sai, lệnh bị từ chối (1,3 s, nhìn về chỗ bị chặn) | Cụp mí phía ngoài |
| Dỗi | Dỗi trong nhà | Sụp nửa mí, lườm |
| Giật mình | Bị dẹp, né | Mở to, con ngươi nhỏ |
| Chăm chú | Kéo tay nắm, ăn, chơi bóng, xem TV | Mí hạ nhẹ, nhìn vào vật |
| Ngủ | Ngủ gật, nằm giường, võng | Nhắm (‿) |

- Mắt tắt khi: COghe nặn hình (hình là lời nói, nên câu hỏi bonus vẫn chỉ bằng hình), bị kéo dài mỏng để lách khe (cạnh dài
  gấp 2,6 lần cạnh giữa trong 0,2 s), trong ống của cơ quan, khi ra cửa, và trong vở quái vật. Dẹt (nằm giường, bò sát
  sàn) vẫn có mắt.
- Chống rung: lớp da dựng lại mỗi khung, nên mắt không bám một đỉnh mà lấy trung bình lớp da ngoài quanh tia nhìn
  (`VenomSurface.SkinToward`). Rồi mỗi mắt được lọc one-euro theo vị trí so với tâm các hạt của phần đó: đứng yên thì lọc
  mạnh, chuyển động nhanh thì bám sát, nên đi theo thân không bị trễ. Đo trên clip duyệt: rung còn 0,2–0,5 mm mỗi khung,
  đứng yên hơn 2,4–3,3 lần so với chưa lọc. Mắt nhấc 3 mm về phía camera (camera trực giao nên không thấy) để gợn da không
  che mắt. Một khung thoáng nặn hình không làm mắt chớp tắt (phải ẩn đủ 0,08 s).
- Chỉ là phần nhìn: một mesh không collider, vẽ sau lớp da (Transparent+22); không ghi vào hạt, lực hay trạng thái puzzle.
  Chạy theo đồng hồ mô phỏng (dừng khi tạm dừng). `-coghe-no-eyes` hoặc `COgheEyes.Enabled = false` tắt mắt.

## Trong nhà (mở khi thắng Boss đầu, màn 12)

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

| Màn (×1,2 cho 60 màn, Mrk 06/10/2026) | Món | COghe làm gì |
| --- | --- | --- |
| 12 | Bóng (món đầu tiên, Mrk đổi 01/10) | háo hức lắc lư, đẩy bóng lăn một vòng rồi về (bóng lăn đúng theo quãng đường), hai xúc tu ôm bóng tung lên cao, chạy vào đỡ, đội đầu hai lần, đánh đầu bóng ra sau, nhảy biến thành trái tim trong khi bóng lăn về chỗ |
| 14 | Tạ | hai xúc tu nắm thanh, nâng ba lần, gồng |
| 17 | Đệm ngủ | nhảy lên nệm, cuộn tròn ngủ, thở, bong bóng ngáy |
| 19 | Gương | tạo dáng: biến hai hình trước gương |
| 22 | Xích đu | ngồi lên, đu qua lại |
| 24 | Cầu trượt | leo thang, trượt xuống về phía người chơi |
| 28 | TV | ngồi xem (hình trên màn chuyển động), nhảy lên phấn khích |
| 31 | Đàn gõ | xúc tu gõ một giai điệu 9 nốt |
| 36 | Bạt nhún | nhún 4 lần cao dần, lần cuối lộn vòng |
| 41 | Võng | nằm võng đung đưa, ngủ |
| 46 | Bánh xe | chạy trong bánh xe quay |
| 50 | Bể cá | áp vào kính ngắm cá bơi |
| 55 | Đèn chiếu bóng | biến hình trước đèn |
| 60 | Cúp | nhảy mừng quanh cúp |

## Màn thưởng "Hiểu ra" (07/10, Mrk)

Mrk: sau mỗi chương có một màn thưởng "Hiểu ra" bỏ qua được; làm xong được thưởng lớn, COghe diễn cảnh thân thiện có tim
bay để người chơi thấy có thành quả khi hiểu COghe. Chỉ nói bằng hình nặn (hình nào phức tạp thì bỏ), không sổ tay, bỏ
mọi thứ ảnh hưởng tới giải đố, tăng hoạt cảnh tình cảm. Clip: `Artifacts/Clips/bonus1.mp4` (bản Mac, có tiếng).

- **Khi nào:** sau khi thắng Boss của chương (vị trí 12, 24, 36, 48, 60). Thứ tự sau khi bấm "→ Level N": đồ mới mở →
  lời mời màn thưởng (Play / Skip) → quảng cáo nếu đến lượt → màn sau. Skip thì đi tiếp ngay; màn thưởng chờ trong Nhà
  (nút ✦ có chấm đỏ, góc trái phía trên nút quà). Không phạt, không khoá màn.
- **Cách chơi (diễn trong Nhà):** COghe ra giữa phòng, nặn một hình và giữ, đung đưa, thỉnh thoảng nâng lên "hỏi" (kêu
  khẽ). Người chơi đáp bằng việc làm: chạm COghe, chạm một món đồ, hoặc bấm Feed. Trong lúc đó COghe không đi lang thang và
  chạm sàn không làm nó đi; chạm vào hình nó đang nặn (cao hơn thân) cũng tính là chạm COghe.
  - Đúng (Mrk 07/10: thưởng ngay, cảm xúc ngay): ngay lúc đáp, COghe thu mình, bật nhảy xoay một vòng, nặn trái tim đập
    hai nhịp. Ở đỉnh cú nhảy, "+10" hiện lên, tim nhỏ bay ra, Giọt bay từ COghe vào ô Giọt (ô đếm lên và nảy theo từng giọt).
    Thưởng câu tăng dần: 10, 20, 30 Giọt (`drops_bonus_round` × số câu). Sau đó nó làm luôn việc đó: chơi bóng, ăn viên bi
    (một viên, thả ngay trước mặt nó), rồi câu tiếp.
  - Sai: hình tan, thân lắc qua lại "không phải", rồi hỏi lại. Feed sai không ném bi. Sai 2 lần thì dòng chữ dưới tiêu đề
    gợi ý ("It wants a cuddle: tap COghe.").
- **Câu hỏi là một "câu" gồm các từ** (`COgheBonusRound`): mỗi từ là một hình và một việc, có thể kèm số (hình `One`–
  `Three`: giơ chừng ấy xúc tu như ngón tay). COghe diễn lần lượt từng hình (mỗi hình ~1,25 s, nghỉ 0,7 s rồi lặp), chỉ diễn
  những từ còn lại. Câu trả lời chỉ dùng thứ ai cũng có (chạm, Feed, quả bóng quà), nên không phụ thuộc đồ đã mua.
  - Chương 1: một từ (tim → chạm; bóng nảy → chạm quả bóng; nấm → Feed).
  - Chương 2: hai từ, thứ tự nào cũng được (tim + nấm, bóng + tim, nấm + bóng).
  - Chương 3: đúng thứ tự (nấm → tim; tim → bóng → nấm; bóng → nấm → tim). Sai thứ tự = lắc nhẹ.
  - Chương 4: đúng số lượng (2 nấm = cho ăn đúng 2 viên; 3 tim = chạm đúng 3 lần; 3 nấm). Mỗi lần đáp, COghe giơ số đã
    nhận; đủ số thì nó nghiêng qua lại chờ một chút để "kiểm"; thêm một lần nữa là thừa: lắc, đếm lại.
  - Chương 5: ghép (2 tim rồi bóng; nấm rồi 3 tim; 2 nấm, tim, rồi bóng).
  - Xong một từ (chưa hết câu): nhảy nhẹ + hình ngón cái. Feed trong màn thưởng: một viên bi rơi ngay trước mặt, COghe
    bắt bằng xúc tu và nuốt tại chỗ (không đi lại), nên đếm nhanh.
- **Cảm ơn (6,2 s, phần thưởng lớn nhất):** hiểu hết thì COghe ra chỗ trống nhất của phòng (`StageSpot`), camera lại
  gần. Nó áp bẹp vào màn hình rồi bật ra, nhảy cao xoay một vòng, nặn trái tim lớn đập nhịp, tim bay lên từ đó
  (`COgheHearts`). Sau đó tim chuyển thành ngôi sao có pháo giấy (`COgheConfetti`). Nó nhảy múa qua lại trong khi "+100" và
  một chùm Giọt bay vào ô Giọt (`COgheDropsFly`). Cuối cùng hiện hộp "You understood COghe!": "+100 Drops" đếm lên từ 0,
  dưới là tổng cả màn thưởng ("+160 Drops in all").
- **Thưởng:** câu 1–3 trả ngay lúc đúng (10/20/30, khoá `bonus:<chương>:<câu>`); xong cả màn trả `drops_bonus` (mặc
  định 100, khoá `bonus:<chương>`). Tất cả ×2 với Plus, mỗi khoá trả một lần (vào lại sau khi bỏ dở không trả lại câu đã
  trả).
- **Tình cảm trong Nhà (07/10, Mrk: tăng hoạt cảnh tình cảm):**
  - Chạm nhẹ liên tiếp (cách nhau dưới 6 s, không dồn dập) làm COghe "ấm" dần: lần 1 một phản ứng vui; lần 2 nó dụi vào
    tay (nghiêng về phía người chơi, rung "rừ rừ") hoặc nặn tim có tim bay; lần 3 nó lao tới áp bẹp vào màn hình như ôm,
    tim bay. Chạm dồn dập (4 lần / 3 s) vẫn là dỗi như cũ.
  - Chơi một món người chơi chọn xong: nó nặn tim đáp lại, tim bay ("cảm ơn đã chơi cùng").
  - Mỗi lần vào Nhà: nó chạy ra phía trước vẫy chào (sau khi đồ mới bật ra).
  - Tim bay hiện cả ở menu chính (COghe ở menu cũng ôm được).
- **Ranh giới:** chỉ là phần nhìn và trong Nhà; không đổi vật lý, hạt hay lời giải màn nào. Tài liệu Nhà
  (`VENOM_PURE_PUZZLE_AND_HOME.md` mục 3) đã bỏ "kỹ năng quen giảm chỉ dẫn vụn".
- **File:** `Home/COgheBonus.cs` (câu hỏi từng chương, đã xong/đang chờ), `Personality/COghePersonality.Bonus.cs` (hỏi,
  lắc, mừng, cảm ơn), `Product/COgheProductUI.Bonus.cs` (lời mời, màn trong Nhà, hộp thưởng), `Product/COgheHearts.cs`,
  quay clip `Product/COgheBonusReel.cs` (`-coghe-bonus-reel <thư mục>`, bản phát triển).
- **Test:** `COgheBonusTests` (lời mời sau Boss rồi Skip, màn thưởng chờ trong Nhà; chơi đủ 3 câu kể cả trả lời sai, chạm
  sàn không làm nó đi, thưởng ngay từng câu và một lần; Play từ lời mời rồi "Later" sang màn 13), `COgheBonusChapterTests`
  (chương nào cũng có 3 câu chỉ dùng chạm/Feed/bóng; hai việc thứ tự nào cũng được; sai thứ tự; đếm đúng và thừa một;
  câu ghép số + bóng), `COgheAffectionTests` (ba lần chạm nhẹ thành cái ôm có tim, không dỗi; vào Nhà là chào).
  Clip: `Artifacts/Clips/bonus1b.mp4` (chương 1 trọn vẹn), `bonus2.mp4` (tình cảm + câu đầu chương 2–5;
  `-coghe-bonus-reel2 <thư mục>`).

## File

- Tính cách: `Assets/_Game/Venom/Runtime/Personality/COghePersonality.cs` (trong màn), `COghePersonality.Home.cs` (trong nhà).
- Vẽ vở diễn: `Assets/_Game/Venom/Runtime/VenomLifeAnimation.Acts.cs` (hình dạng, cơn bực, tư thế trong nhà),
  `VenomLifeAnimation.Monster.cs` (quái vật ở menu) và mặt của nó `Personality/COgheMonsterFace.cs`.
- Mắt: `Personality/COgheEyes.cs` (vẽ, chớp, nhìn, chống rung), `Personality/COghePersonality.Eyes.cs` (cảm xúc),
  `VenomSurface.FragmentShape` / `SkinToward` (chỗ đặt mắt trên da).
- Nhà: `Assets/_Game/Venom/Runtime/Home/` — `COgheLowPoly` (bộ dựng mesh low-poly), `COgheHomeItems` (14 món theo
  `ITEMS.json` của Codex), `COgheHomeRoom` (phòng, bố trí, mở khóa, hình mờ). `VenomHabitat` dựng phòng khi vào nhà.
- Menu đồ vật và camera nhà: `Assets/_Game/Venom/Runtime/Product/COgheProductUI.Items.cs`.
- Icon: `Assets/_Game/Venom/Resources/COgheHome/Icons/<ID>.png` (256 px, ASTC 6×6).
- Âm thanh: `Tools/audio/synth_coghe_audio.py --personality` (11 tiếng vở diễn), `--home` (6 nốt đàn gõ), `--feed` (bi
  thép, tiếng nuốt), `--monster` (quái vật: chất lỏng dâng lên, lưỡi trượt ra).
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
- Quái vật: không đổi hạt nào; nghiêng mặt khi thè lưỡi, rồi nhìn camera nghiêng đầu; mặt mất khi xong
  (`MonsterGrowsAFaceAndLosesIt`); menu mở là quái vật, mặt nằm trong khung hình (`TheMenuOpensWithTheMonster`).
- `COgheEyesTests`: mắt bật mặc định, có trên da lúc đứng yên, cười trong màn ăn mừng, tắt khi `Enabled = false`.
- Mắt, Explicit: `RenderEyesStills` (ảnh từng cảm xúc, `Artifacts/Eyes`), `RecordEyesHome` (clip trong nhà, mỗi khung có
  bản có mắt và không mắt). `COGHE_EYES=1` cho `RecordReviewClips` / `RecordCrateTapDemo` lưu thêm bản không mắt (`plain/`)
  và độ rung vào `track.txt`; `COGHE_EYES=0` quay không mắt.
- Explicit: `RenderPersonalityActs` (khung từng vở diễn), `RenderHome` (toàn cảnh, nhìn từ trên, từng trò chơi),
  `RenderMonsterStills`, `RenderMenuMonster`. Bản Mac: `-coghe-monster-reel <thư mục>` quay menu thật (PNG 30 khung/s + `sounds.txt`).
