---
title: "COghe thông minh dần: kế hoạch trí tuệ và tương tác"
tags: [coghe, personality, home, retention]
status: active
created: 2026-10-07
---

# COghe thông minh dần qua từng màn

> **Mrk chốt (07/10/2026, 02:26 UTC), thay cho các mục 3.1, 3.2, 3.4, 7 bên dưới:**
> 1. Sau mỗi chương một màn thưởng "Hiểu ra" bỏ qua được; làm xong thưởng lớn, COghe diễn cảnh thân thiện có tim bay.
> 2. Chỉ nói bằng hình nặn; hình quá phức tạp thì bỏ (không bong bóng biểu tượng).
> 3. Không làm sổ tay.
> 4. Bỏ những thứ ảnh hưởng tới giải đố (đã bỏ câu "kỹ năng quen giảm chỉ dẫn vụn").
> 5. Tăng hoạt cảnh tương tác, thể hiện tình cảm.
>
> Đã làm màn thưởng chương 1 (tim → chạm, bóng → chạm bóng, nấm → Feed): `Docs/Personality/COghe/README.md`,
> mục "Màn thưởng Hiểu ra". Còn lại: màn thưởng chương 2–5, thêm hoạt cảnh tình cảm khi chạm và khi chơi cùng.

Mrk (06/10/2026, 17:14 UTC): "tao muốn COghe có sự phát triển về trí tuệ, cảm giác COghe phải thông minh hơn sau mỗi màn
chơi nhưng lại không ảnh hưởng tới quá trình giải đố. Mức độ tương tác và đòi hỏi user tương tác cùng COghe cũng phải
tăng lên."

## 1. Ý chính

COghe học đúng thứ người chơi vừa giải. Chương 1 nó biết dùng tay, chương 2 biết chia việc, chương 3 biết cân nặng nhẹ,
chương 4 biết đếm và chia phần, chương 5 biết chế máy.

Trí tuệ chỉ được **thể hiện**, không được **dùng** trong màn. Nó lộ ra qua bốn chỗ:
- Lúc ăn mừng sau mỗi màn.
- Một cuốn sổ tay.
- Cách COghe "nói" bằng hình nặn.
- Các trò chơi ở Nhà.

Mạch cảm xúc: đầu game người chơi dạy COghe; cuối game COghe đố người chơi.

## 2. Ranh giới: không đụng vào giải đố

- Không gợi ý. COghe không chỉ, không nghiêng về phía cơ quan nào và không tự làm bước nào. Lực, khối lượng, tốc độ và
  số chạm không đổi.
- Trong màn chỉ đổi *nội dung* các act đang có (`COghePersonality`). Act chỉ chạy khi thân nguyên vẹn, đứng yên trên
  sàn, ngoài cơ quan. Nội dung act chỉ lấy từ các màn đã qua, không bao giờ từ màn đang chơi.
- Khoảnh khắc "hiểu ra" nằm gọn trong 4,8 giây ăn mừng hiện có (`VenomCelebration.Duration`). Thời gian chuyển màn không
  đổi.
- Lời rủ của COghe không khoá màn sau. Đây là đề xuất; Mrk chốt ở mục 7.
- Giữ luật hiện có: không mắt, không tiếng nói (chỉ tiếng chất lỏng). Riêng act quái vật được có mặt.
- Mâu thuẫn với tài liệu: `Docs/VENOM_PURE_PUZZLE_AND_HOME.md` mục 3 cho phép "kỹ năng quen có thể giảm chỉ dẫn vụn".
  Cho phép như vậy là làm đổi số chạm, tức là đụng vào giải đố. Đề xuất bỏ câu này.
- Mỗi bước tiến đều được báo cho người chơi qua trang sổ tay "COghe học được: …". Không lặng lẽ đổi hành vi.
- **Kiểm chứng:** chạy mọi test giải hai lần, một lần với COghe "mới sinh", một lần với COghe "học xong 60 màn". Kết quả,
  số chạm (`MeasureLevelMetrics`) và thời gian thoát phải trùng nhau. Thời gian ăn mừng cũng không đổi.

## 3. Bốn chỗ thể hiện trí tuệ

### 3.1 Sau mỗi màn: "Hiểu ra"

Trong lúc ăn mừng, COghe dùng chính thân mình nặn lại thứ nó vừa dùng. Thân thật vẫn đứng yên tại chỗ; chỉ lớp da đổi
hình, như các hình nặn hiện có. Các mô hình nặn:
- tay nắm, nút;
- chữ Y của ống, hai nửa đập tay;
- bập bênh, quả cân;
- số 1/4, 3/4;
- bánh răng quay.

Độ khéo tăng theo số lần COghe đã gặp cơ quan đó:
- Lần đầu (màn dạy): nặn vụng, méo, đổ, phải nặn lại.
- Các lần sau: nặn nhanh và gọn hơn, có thêm chi tiết. Ví dụ bánh răng có răng, quay khớp với một bánh thứ hai.

Người chơi so được hai lần: lần trước nó nặn vụng, lần này nặn khéo. Đó là cảm giác "thông minh hơn sau mỗi màn".

Khái niệm của từng màn lấy từ các cơ quan có trong màn, giống bộ đo độ khó (`COgheLevelMetrics`, trường
`mechanisms`). Không cần gắn tay cho 60 màn.

### 3.2 Sổ tay của COghe (60 trang)

- Mỗi màn thắng thêm một trang. Trên trang, COghe "vẽ" lại màn vừa chơi.
- Nét vẽ lớn lên theo chương:
  - Chương 1: nét nguệch ngoạc như trẻ con.
  - Chương 2: có màu, có hai hình COghe nhỏ.
  - Chương 3: có mũi tên và quả cân.
  - Chương 4: ghi phân số.
  - Chương 5: thành bản vẽ kỹ thuật.
- Trang được vẽ tự động từ hình học của màn (nhìn từ trên xuống, áp kiểu nét theo chương), nên không phải vẽ tay 60 trang.
- Xem sổ tay ở màn thắng (trang lật vào) và trong Nhà. Trang mỗi màn chỉ hiện sau khi đã thắng màn đó, nên không lộ lời
  giải trước.

### 3.3 Nói bằng hình nặn

Hiện có 8 hình mở dần theo màn (`COghePersonality.ShapeUnlocks`: tim, sao, dấu hỏi, nấm, người tuyết, ngón cái, tên lửa,
ô). Mở rộng thành "câu":
- Chương 1: một hình (cảm xúc).
- Chương 2: hai hình. Ví dụ dấu hỏi + quả bóng nghĩa là "chơi bóng không?".
- Chương 3: chuỗi ba hình để kể lại một chuyện.
- Chương 4: có số (1–4). Ví dụ đòi đúng 3 viên.
- Chương 5: đặt câu đố cho người chơi.

Câu dùng ở menu, Nhà và màn thắng. Trong màn vẫn chỉ dùng hình cảm xúc như bây giờ.

### 3.4 Ở Nhà: mỗi chương một trò, đòi người chơi nhiều hơn

| Chương | COghe biết | Trò | Người chơi phải làm |
| --- | --- | --- | --- |
| 1 (Nhà chưa mở) | dùng tay | Vẫy lại: COghe vẫy ở đầu và cuối màn | 1 chạm vào kính để vẫy lại |
| 2 (Nhà mở ở Boss 12) | chia việc | Dạy trò | Chạm 2–3 món đồ theo thứ tự. COghe nhớ và làm lại; lần sau nó tự thêm một bước |
| 3 | cân nặng nhẹ | Trốn tìm | COghe trốn sau đồ đạc; xoay camera tìm (đúng kỹ năng nhìn quanh vách). Nó trốn khéo dần |
| 4 | đếm, chia phần | Đếm cho đúng; đàn theo | COghe nặn số 3, cho ăn đúng 3 viên. Ở đàn xylophone nó chơi giai điệu, người chơi chơi lại; giai điệu dài dần |
| 5 | chế máy | Chế máy cùng COghe | COghe dựng một cỗ máy nhỏ từ đồ trong Nhà (cầu trượt → bóng → chuông) còn thiếu một món; người chơi đặt món đó. Về sau máy dài hơn và nó đố lại |

- COghe tự rủ bằng câu hình. Nhận lời có thưởng nhỏ: Drops, hình nặn mới hoặc trang sổ tay đặc biệt.
- Từ chối thì không mất gì. Hành vi ở Nhà vẫn tuân theo `VENOM_PURE_PUZZLE_AND_HOME.md` mục 5.

### 3.5 (Phụ) Act lúc chờ trong màn

Kho act lớn dần theo những gì COghe đã học: vẽ lên kính hình cơ quan của *chương trước*, tung hứng một quả bóng ảo.
Act chỉ chạy khi người chơi đứng chờ lâu, theo đúng điều kiện act hiện có, và chỉ dùng lớp da.

## 4. Kỹ thuật

- **`COgheMind`** (dữ liệu): suy ra phần lớn từ tiến trình đã lưu. Gồm:
  - khái niệm đã học, theo các màn đã thắng;
  - số lần đã gặp mỗi khái niệm;
  - cấp ngôn ngữ, theo chương.
  Chỉ trạng thái các trò ở Nhà là dữ liệu lưu mới. Dùng ID ổn định, có phiên bản.
- **Chỗ gắn vào code:**
  - `VenomCelebration` / màn thắng của `COgheProductUI`: hiểu ra, trang sổ tay.
  - `COghePersonality` và `VenomLifeAnimation`: hình nặn mới, câu hình, act lúc chờ.
  - `COghePersonality.Home` và `COgheHomeRoom`: các trò ở Nhà.
- **Rủi ro cần prototype trước:**
  - Lớp da nặn được hình phức tạp như bánh răng, bập bênh, phân số. Các hình tên lửa và ô cho thấy là làm được.
  - Lớp da tách thành hai hình nhỏ đập tay.
  - Tốc độ khung hình trên điện thoại khi nặn.
- **Test:**
  - Test bất biến giải đố ở mục 2.
  - Thời gian ăn mừng không đổi.
  - Act không chạy khi đang trong cơ quan.
  - Mỗi trò ở Nhà lưu đúng và thoát giữa chừng không để lại lực treo.

## 5. Các bước (mỗi bước gửi clip cho Mrk duyệt)

1. **Prototype "hiểu ra" và sổ tay** cho 5 màn dạy cơ quan, mỗi chương một màn. Chạy test bất biến. Clip so sánh lần nặn
   vụng với lần nặn khéo.
2. **Đủ 60 màn:** tự gắn khái niệm cho từng màn; độ khéo theo số lần gặp; sổ tay tự vẽ với 5 kiểu nét.
3. **Ngôn ngữ hình:** thêm hình mới (số 1–4, bóng, bánh răng, mũi tên); câu hai và ba hình; COghe rủ chơi ở menu và
   Nhà.
4. **Trò ở Nhà,** mỗi đợt một trò: vẫy lại → dạy trò → trốn tìm → đếm và đàn theo → chế máy.
5. **Act lúc chờ** lớn dần.
6. **Đo** bằng `COgheAnalytics`:
   - tỉ lệ nhận lời rủ;
   - số lần vào Nhà mỗi phiên và thời gian ở Nhà;
   - D1/D7 trước và sau.

## 6. Không làm

- Không có điểm IQ, thanh chỉ số hay sức mạnh tăng theo trí tuệ.
- Không có gợi ý tự động. Gợi ý trả phí (`COgheProductUI.Hints`) giữ nguyên.
- Không phạt khi người chơi lâu không vào Nhà: COghe không quên bài, không buồn kéo dài.

## 7. Cần Mrk chốt

1. Lời rủ của COghe không bao giờ khoá màn (đề xuất), hay có cổng mềm cuối chương?
2. Chỉ nói bằng hình nặn (giữ luật không mắt, không tiếng), hay thêm bong bóng biểu tượng nhỏ ở Nhà?
3. Sổ tay vẽ tự động từ hình màn (rẻ, đủ 60 trang), hay vẽ tay 5–10 trang đẹp cho các mốc?
4. Bỏ câu "kỹ năng quen giảm chỉ dẫn vụn" trong tài liệu Nhà?

## Nguồn

- `Docs/VENOM_PURE_PUZZLE_AND_HOME.md` (mục 1, 3, 5)
- `Assets/_Game/Venom/Runtime/Personality/COghePersonality.cs` (act, `ShapeUnlocks`)
- `COghePersonality.Home.cs` (trò ở Nhà, sulk, ăn)
- `VenomCelebration.cs` (`Duration` = 4,8 s)
- `Assets/_Game/Tests/PlayMode/COgheLevelMetrics.cs`
