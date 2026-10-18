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

## Home / Main Menu: đầu quái vật trồi từ vũng nhầy (15/10/2026)

Thay dáng người cũ theo yêu cầu Mrk: chỉ còn đầu quái vật nối liền với phần nhầy,
không ngực/vai/tay. Vẫn dùng chung vở diễn ở Home và Main Menu; không đổi quyền
Plus hoặc nhịp tự diễn ở menu. Tổng thời gian **9,6 giây**.

| Lúc | Diễn |
| --- | --- |
| 0–1,6 s | Vũng nhầy tụ và phồng lên, chưa có mặt |
| 0,55–3 s | Một khối đầu từ từ nhô lên, phần gốc vẫn liền với vũng nhầy |
| 2,1–3,1 s | Mắt hình thành trên da; đầu giữ hướng 3/4, không quay mất mặt |
| 2,9–4,85 s | Miệng thành một nét cười rồi mở dần; nanh lộ ra sau mắt |
| 4,65–6,35 s | Lưỡi trườn ra từ miệng, uốn cong với chuyển động mềm |
| 6,35–7,75 s | Thu lưỡi, ngậm miệng, nhìn lại và chớp mắt |
| 7,8–9,6 s | Mặt thu lại trước, đầu chảy xuống vũng nhầy rồi về COghe |

- Chỉ thay lớp da và mặt, không di chuyển hạt, thêm collider hoặc ghi tiến độ.
- Lưới miệng/mắt lấy mẫu liên tục theo mặt da; răng/lưỡi dùng normal mượt.
  Sửa sai số nội suy trường da và normal chỉ khi diễn quái vật; giữ mật độ lưới cũ.
- Camera Home ở chế độ zoom theo tâm đầu/vũng nhầy; overview và thao tác xoay giữ nguyên.
- Tiếng vẫn là chất lỏng: cue lưỡi khớp nhịp mới ở 4,65 s. Không giọng nói/tiếng dọa.
- Lưỡi rộng gấp đôi bản đầu quái vật, giữ độ dày cũ. Theo phản hồi 15/10, giữ nhịp sóng uốn của bản cũ chạy dọc cả thân lưỡi; chỉ đoạn sát miệng giảm biên độ để gốc không giật. Giữa lưỡi có độ võng và đầu lưỡi lượn theo sóng, tránh giữ cố định tư thế cong.
- [Ảnh, video Unity và kiểm chứng](../../Verification/COgheMonsterHead/README.md).
- Các video và mô tả dáng người 02/10 là lịch sử, không còn là chuẩn hình dáng.

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

- Đội mũ thì đôi mắt của phần lớn nhất hạ thấp xuống dưới vành mũ (`COgheAccessories.HatShown`).
- Mắt tắt khi: COghe nặn hình (hình là lời nói, nên câu hỏi bonus vẫn chỉ bằng hình), bị kéo dài mỏng để lách khe (cạnh dài
  gấp 2,6 lần cạnh giữa trong 0,2 s), trong ống của cơ quan, khi ra cửa, và trong vở quái vật. Dẹt (nằm giường, bò sát
  sàn) vẫn có mắt.
- Chống rung: lớp da dựng lại mỗi khung, nên mắt không bám một đỉnh mà lấy trung bình lớp da ngoài quanh tia nhìn
  (`VenomSurface.SkinToward`). Rồi mỗi mắt được lọc one-euro (`COgheSteady`, mũ dùng chung) theo vị trí so với tâm các hạt của phần đó: đứng yên thì lọc
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

> **Bản 07/10, đã được thay một phần.** Hiện nay “Hiểu ra” là bắt buộc, không có Skip, đến sau Boss 10/21/32/43/54/65
> (Override 14/10). Từ chương 2, câu cuối là trò chơi với đồ chơi riêng; đáp sai thì COghe làm trò hài (Override 17/10).
> Chương 7 trở đi câu hỏi tự sinh. Đồ chơi đã thắng nằm trong menu Items. Xem hai mục Override ở cuối tài liệu. Các chi
> tiết dưới đây còn đúng cho chương 1 và cho cách hỏi, đếm và thưởng.

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


### Override 14/10/2026 — “Hiểu COghe” bắt buộc

Áp dụng Boss hiển thị 10/21/32/43/54/65 trong catalog 65 màn. Sau bấm tiếp trên màn
thắng và giới thiệu phần mới mở, tự vào phần hiểu COghe; không có Skip. Back về menu,
Play tiếp câu chưa hoàn tất. Checkpoint câu và hoàn tất lưu độc lập với thưởng Drops;
paid keys cũ vẫn hợp lệ, không xóa tiến độ level. Các mô tả “optional/skippable” ở
phần lịch sử phía trên không còn áp dụng. Style tách khỏi Home, mở sau màn 8; Home
vẫn sau Boss 10. [Biên bản](../../Verification/COgheChapterProgression/README.md).

### Override 17/10/2026 — Đồ chơi và trò hài trong “Hiểu COghe” (Mrk)

Mrk chốt: dòng đồ chơi riêng có animation riêng; đáp sai thì COghe làm trò hài; câu cuối mỗi chương là chơi cùng COghe.
Mrk duyệt từng đợt qua clip; clip ở `OUTBOX/COGHE_HIEURA_TOYS_2026_10_17` (máy của Claude).

- **Đồ chơi (`COgheToys`):** không bán, không nằm trong 14 món nội thất, không mở theo màn. Thắng trò chơi trong
  “Hiểu ra” thì đồ chơi thành của người chơi (`COgheShop.Grant`, nguồn `bonus_toy`), ở lại trong Nhà. Chạm vào nó ở Nhà
  thì COghe ra chơi. Trống (`TOY_DRUM`) nằm cạnh chỗ COghe đứng hỏi, hơi lùi về sau.
- **Câu cuối chương 2 = trò trống** (`COgheBonusRound.Game`, thay câu “nấm + bóng”):
  - Trống bật ra cạnh COghe, nó gõ một tiếng rồi giật mình.
  - Ba nhịp 2 / 3 / 4 tiếng; người chơi chạm trống gõ lại. Chỉ tính đủ số tiếng, không tính nhanh chậm, không đếm giờ.
  - Gõ thừa: COghe bịt tai rồi gõ lại nhịp đó. Chạm COghe hay bấm Feed thay vì trống: một trò hài, rồi gõ lại nhịp.
  - Đúng cả ba: cùng gõ tự do 5,4 giây (mỗi lần người chơi gõ, COghe nhảy), rồi vui, trả thưởng câu 3 như cũ, rồi cảm ơn.
  - Bỏ ngang: trống cất đi; vào lại thì chơi lại từ trò trống.
  - Chạm trống được ưu tiên hơn chạm COghe (`VenomCampaign.TouchPoint` → `ToyTouch`); vùng chạm rộng hơn trống 3,5 cm.
- **Đáp sai = trò hài theo thứ COghe nhận được** (`COgheGag`, mọi chương), thay cho cái lắc đầu:
  - Bị cho ăn: bắt viên bi, phồng má, nhè ra (`Spit`).
  - Chạm quả bóng: bóng nảy tới cộc vào đầu (`Bonk`).
  - Bị chạm khi muốn ăn: cười nhột rồi bụng sôi (`GiggleRumble`).
  - Bị chạm khi muốn bóng hay trống: cười nhột rồi chỉ vào món đó (`GigglePoint`).
  - Cho ăn thừa: nấc cụt rồi ợ bong bóng (`Hiccup`). Vuốt thừa: cười lăn lộn (`GiggleRoll`).
  - Món đồ khác: lắc đầu như cũ (`Nope`).
  - Camera lại gần trong lúc diễn (`GagFrame`). Sai 2 lần vẫn hiện gợi ý chữ.
- **Chương 1 giữ như cũ** (tim → bóng → nấm). Mrk 17/10: chương 1 phải dễ.
  - Trò chơi bóng (`Game("BALL")`) vẫn có trong code để dùng cho chương sau, chưa chương nào dùng.
  - Trò chơi bóng: COghe nặn hình quả bóng, bóng thật lăn tới, rồi ba lượt chuyền qua lại với người chơi (chạm vào bóng để
    chuyền lại). Cuối cùng COghe tâng bóng bằng đầu.
- **Trống là một từ từ chương 3** (`COgheShape.Drum`: thân trống tròn và hai dùi bắt chéo, kèm tiếng trống nhỏ):
  - chương 3: vuốt → trống → ăn;
  - chương 4: trống đúng 3 lần;
  - chương 5: 2 trống → ăn → vuốt;
  - chương 6: bóng → 3 trống → ăn.
  - Chạm trống đúng lượt thì COghe ra gõ. Gõ trống lúc không muốn thì COghe giật mình nhảy dựng, lườm trống rồi nhún vai
    (`Startle`). Lúc nào chạm trống cũng có tiếng.
- **Save cũ:** đã xong “Hiểu ra” chương 2 trước khi có trống thì được tặng trống (`COgheToys.GrantEarned`, lúc dựng Nhà).
- **Câu cuối chương 3 = kèn** (đợt 2, `TOY_HORN`):
  - Kèn nằm bên trái chỗ COghe đứng, miệng thổi hướng về nó. Có 3 nút to: san hô, xanh lục lam, xanh nhạt; mỗi nút một nốt.
  - COghe bấm một giai điệu, người chơi bấm lại đúng thứ tự. Ba giai điệu: 0-2, 2-0-1, 1-2-0.
  - Chạm vào vùng kèn thì tính là bấm nút gần nhất.
  - Bấm sai nút: kèn kêu "oẹt", COghe giật lùi, mắt tròn, rồi cười (`Blooper`). Sau đó COghe bấm lại giai điệu.
  - Đủ ba giai điệu: hai bên cùng thổi kèn; mỗi lần người chơi chạm thì COghe nhảy. Ở Nhà, chạm kèn thì COghe ra thổi một
    bài.
- **Câu cuối chương 4 = bóng bay** (đợt 2, `TOY_BALLOON`, chùm bóng sau cái trống):
  - COghe lấy ra một quả bóng xẹp và giơ số xúc tu bằng số lần cần thổi: 2, rồi 3, rồi 3. Mỗi lần người chơi chạm vào bóng
    là một lần thổi, bóng to dần.
  - Đủ số rồi chờ một chút: COghe buộc bóng, bóng bay về chùm.
  - Thổi thừa: bóng nổ "bụp" vào mặt COghe, COghe bị thổi bẹp rồi cười (`Pop`), rồi lấy quả mới, vẫn đếm số cũ.
  - Đủ ba quả: COghe cầm dây cả chùm bay lên; mỗi lần người chơi chạm thì bay cao hơn. Ở Nhà, chạm chùm bóng thì COghe bay
    lên cùng bóng.
- **Câu cuối chương 5 = con quay** (đợt 3, `TOY_TOP`, phía trước bên phải chỗ COghe đứng):
  - COghe quấn xúc tu vào cán rồi giật, con quay quay tít. Một lúc sau nó chậm dần và lắc lư mỗi lúc một mạnh (khoảng
    4 giây), người chơi chạm vào nó để nó quay thẳng lại. Không bấm giờ gắt.
  - Lắc lư mà chưa đổ thì chạm lúc nào cũng được. Chạm khi nó còn quay đều thì chỉ quay nhanh thêm, không bị phạt.
  - Để nó đổ: tiếng lạch cạch, COghe hốt hoảng, tự xoay như con quay rồi chóng mặt (`TopFall`). Sau đó COghe dựng con
    quay lên và giật lại.
  - Cứu ba lần thì COghe xoay tít bên con quay. Ở Nhà, chạm con quay thì COghe giật cho nó quay rồi xoay theo.
- **Câu cuối chương 6 = hộp nhạc** (đợt 3, `TOY_MUSICBOX`, phía trước bên trái):
  - COghe mở nắp, búp bê nhỏ trồi lên. COghe quay tay quay một nấc (2 nốt).
  - Đến lượt người chơi: mỗi lần chạm hộp là quay một nấc, ra 2 nốt; búp bê xoay, COghe nhảy một bước. Ba lần chạm là
    hết bài, ba bài.
  - Ba bài xong thì hộp tự chơi tiếp, hai bên cùng nhảy, rồi nắp đóng lại. Chỉ chạm sai chỗ mới có trò hài. Ở Nhà, chạm hộp
    thì COghe mở hộp, quay nhạc và nhảy.
- Âm thanh: `top_whirr`, `top_clack`, `mbox_0..7` (chuông hộp nhạc, thang ngũ cung).
- **Chương 7 trở đi** (đợt 4, 18/10, cho tới 300 màn): `COgheBonus.Rounds` tự sinh câu hỏi từ số chương, tối đa
  `MaxChapters` = 60. Lần nào vào cũng ra đúng câu đó, nên bỏ ngang rồi vào lại vẫn khớp, và Boss nào cũng có Hiểu ra kèm
  thẻ Ability.
  - Câu 1: 3 từ theo thứ tự, có số lượng (2 hoặc 3) ở 1 từ.
  - Câu 2: 4 từ, số lượng ở 1 từ; từ chương 10 thì 2 từ.
  - Trong một câu không từ nào lặp lại.
  - Từ lấy trong vốn mọi người chơi đều có: vuốt, cho ăn, bóng, trống, và 4 từ mới là kèn, bóng bay, con quay, hộp nhạc
    (`COgheShape.Horn/Balloon/Top/MusicBox`).
  - Từ đồ chơi: chạm đúng lượt thì COghe ra chơi món đó. Chạm sai lượt thì COghe giật mình (`Startle`), món đó vẫn kêu tiếng
    của nó.
  - Câu cuối: lần lượt 5 trò (trống, kèn, bóng bay, con quay, hộp nhạc), cứ 5 chương thì khó thêm một bậc
    (`COgheBonusRound.Level`):
    - trống: mỗi nhịp thêm một tiếng, tối đa 6;
    - kèn: mỗi giai điệu thêm một nốt, tối đa 5;
    - bóng bay: 3 lần thổi mỗi quả;
    - con quay: lắc nhanh hơn (tối thiểu 2,8 s) và phải cứu thêm, tối đa 5 lần.
  - Chương 1–6 giữ nguyên.
- **Bộ sưu tập đồ chơi** (đợt cuối, 18/10): menu Items ở Nhà có phần “Toys n/5” trên cùng, nội thất ở dưới.
  - Đồ đã có: hình món đồ, tên, “Play”. Chạm vào thì menu đóng và COghe ra chơi ngay, không chào trước
    (`PlayWith` bỏ lời chào đang chờ).
  - Đồ chưa có: hình mờ, dấu “?”, tên “???”, dòng “Chapter N”. Chạm vào thì báo chương nào tặng, menu vẫn mở.
  - Hình món đồ: `Resources/COgheHome/Icons/TOY_*.png`, chụp từ mô hình bằng test `RenderToyIcons` (Explicit).
- **Kiểm tra:** `COgheToyTests`, `COgheProductUITests.ToysAreACollectionWithEmptySlots`, `ChapterTwoIsUnderstoodEndToEnd` (chơi trọn chương 2 qua màn hình thật: câu hỏi, trò trống, thưởng, thẻ Ability, đồ chơi trong bộ sưu tập). Explicit `EveryChapterIsUnderstoodEndToEnd`: chương 1–6 như vậy, chương 7–9 câu tự sinh. Bản Mac: `-coghe-toy-reel <thư mục>` quay trò trống và các trò hài,
  `-coghe-toy-reel2 <thư mục>` quay trống trong câu hỏi, `-coghe-toy-reel3 <thư mục>` quay kèn và bóng bay, `-coghe-toy-reel4 <thư mục>` quay con quay và hộp nhạc, `-coghe-chapter7-reel <thư mục>` chơi hết chương 7, `-coghe-collection-reel <thư mục>` mở bộ sưu tập rồi chơi kèn (PNG 30 khung/s, `sounds.txt`, `marks.txt`; tối đa
  9000 khung).
