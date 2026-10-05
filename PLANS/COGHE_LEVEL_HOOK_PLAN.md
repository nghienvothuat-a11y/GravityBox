---
title: "COghe — kế hoạch làm lại 50 màn để giữ chân người chơi"
tags: [coghe, level-design, game-feel, playtest, plan]
status: draft
created: 2026-10-05
---

# COghe — kế hoạch làm lại 50 màn để giữ chân người chơi

05/10/2026 · Theo phản hồi chơi thử người mới của Mrk (10 điểm, 05/10/2026). **Bản nháp, chờ Mrk chốt mục 8.**

Cách làm: đọc lại code dựng màn, lời giải mẫu, ảnh chụp bản build và dossier của cả 50 màn theo đúng thứ tự trong game
(`VenomCampaignBuilder.SpatialOrder`), cùng code điều khiển (kéo tay nắm, thang máy, ống, lực). **Chưa chạy Unity, chưa
đo trên máy.** Mọi con số "đo" bên dưới lấy từ dữ liệu đã có trong repo; phần "mô hình" là mô phỏng lại phương trình trong
code, cần đo lại trên máy trước khi kết luận. Báo cáo gốc (tiếng Anh, có file:dòng) nằm cạnh file này, xem mục 10.

---

## 1. Tóm tắt

Code xác nhận cả 10 điểm Mrk nêu. Có bốn nguyên nhân gốc:

1. **Màn sau chép màn trước, nhiều chỗ chép nguyên code.**
   - Màn 4 và 5 chạy cùng một nhánh code (`if(n==4||n==5)`), lời giải giống hệt.
   - Màn 6, 9, 10 gọi cùng hàm dựng ròng rọc với cùng toạ độ; Boss 10 = màn 9 + thang máy của màn 8.
   - Mẫu "nửa A giữ nút, nửa B chốt" dùng cùng một bộ thông số ở màn 18, 19, 20, 38; ở chương 31–50 nó xuất hiện 9/20 màn.
   - Màn 32 có lời giải giống từng dòng màn 16; Boss 50 dùng lại tháp, vị trí Q, điểm xuất phát của màn 49.
2. **Ít quyết định thật.** Màn 1–10 có số lựa chọn cần suy nghĩ lần lượt 0, 0, 0, 1, 1, 1, 1, 1, 2, 3: từ màn 4 đến 8,
   cả lời giải là chạm vào tay nắm duy nhất trong hộp. Ở chương sau nhiều chạm không có nghĩa là khó: màn 42 (23 chạm)
   và 46 (27 chạm) chỉ có khoảng một quyết định, còn lại là đi lại và chạm lại.
3. **Cơ quan không "nói" trạng thái.**
   - Nút đen ở màn 13 không có va chạm, không lún, không nhãn.
   - Nút thang máy chỉ lún 4 mm (khoảng 3 px), lại bị chính COghe đứng che.
   - Chốt giữ cửa nằm trong sàn hoặc là khối 2 cm ở xa cửa (màn 33, 40, 45).
   - Cửa tô màu tím giống tường trơn.
   - Biển số màn che tay nắm/vòng dây ở ít nhất 16 màn.
4. **Cảm giác điều khiển hụt.**
   - Kéo tay nắm bị rung do lỗi vòng điều khiển (mục 3.1).
   - Ống chỉ nhận chạm ở miệng ống.
   - Phần thân yếu kéo vật nặng trông y như kéo được, đứng im 3–6 giây rồi buông. Bản game thật (Product UI) không hiện
     thông báo "chưa đủ lực" (chỉ HUD bản dev có) nên người chơi không biết vì sao.

Giải pháp gồm ba tầng, làm theo thứ tự:

- **A. Sửa cảm giác và độ rõ cho cả 50 màn (mục 3).** Làm trước vì sửa một chỗ thì mọi màn đều được lợi.
- **B. Bổ sung bộ "cơ quan logic" (mục 4):** chụp kính, hộp, chốt chặn, vật nặng đè, khoá giữ cửa, nút bật lại, đồng hồ cân.
  Phần lớn dựng được bằng component có sẵn.
- **C. Làm lại từng màn theo chu kỳ 10 màn (mục 5–6).** Làm chương 1 trước, cho người mới chơi thử, đạt mới làm tiếp
  chương 2–5.

---

## 2. Mười điểm của Mrk → nguyên nhân → hướng sửa

| # | Mrk nêu | Nguyên nhân tìm thấy | Hướng sửa |
| --- | --- | --- | --- |
| 1 | Kéo cơ quan bị rung, giật (màn 4…) | Vòng điều khiển tay nắm mất ổn định ở 43/59 tay nắm; cơ quan không nội suy hình; dựng lại bản đồ đường đi mỗi 0,35 s khi đang kéo | Mục 3.1 |
| 2 | Màn trước–sau ít thăng tiến; màn 4–5 cùng thao tác | Màn 4 và 5 cùng nhánh code; màn 5 chỉ đổi vị trí tay nắm | Màn 5 mới: công tắc sàn mở chụp kính che tay nắm trên vách (đúng ý Mrk) |
| 3 | Màn 6 cần hai thao tác; màn 7 thêm khoá | Màn 6, 7 mỗi màn một tay nắm, ngay cạnh chỗ xuất phát | Màn 6: leo lên kéo tay trên vách mở hộp → quay tời. Màn 7: chốt chặn cầu, cần mở chốt nằm dưới hố |
| 4 | Thang máy màn 8 phải có nút lún rõ, bật lại khi tới nơi | Nút lún 4 mm, chỉ lún sau khi lên khay + 0,2 s; COghe đứng che; không đèn, không tiếng | Mục 3.2; màn 8 mới cần bấm hai lần (lên rồi xuống) |
| 5 | Màn 9 giống 6, Boss 10 giống 9 | Cùng hàm dựng, cùng toạ độ | Màn 9, Boss 10 mới hoàn toàn; Boss có mẹo (mục 5.1) |
| 6 | Màn kiểu 12 (thứ tự) nên phát triển | Màn 12 là màn thứ tự hay nhất nhưng 15 màn sau mới có màn tiếp ý | Mạch "thứ tự" xuyên 5 chương (mục 6) |
| 7 | Nút đen màn 13 khó hiểu, bấm không được | Đó là "nút gọi thang tầng dưới": hình trang trí không va chạm, vật liệu kim loại hiện gần đen, không nhãn, không lún. Có ở màn 13, 28, 37, 39 | Mục 3.3 |
| 8 | Chạm vào thân ống là chui; ống rẽ nhánh chạm bên nào đi bên đó | Chỉ nhận chạm trong 6,8 cm quanh miệng ống; chạm thân ống không có phản ứng gì; ở ngã ba chỉ nhận một phần ba đầu nhánh | Mục 3.4 |
| 9 | Màn 18: kéo nút B thì cửa phải có khoá chặn | Code đã khoá cửa (logic OR), nhưng lúc kéo B cửa đang mở sẵn nên **không có gì thay đổi trên màn hình**: không chốt, không tiếng | Mục 3.5: chốt nhìn thấy cắm vào cửa (component `COgheSpringAccessDoor` có sẵn nhưng chưa màn nào dùng) |
| 10 | Phần nhỏ kéo vật nặng phải gồng mà không được | Không có kiểm tra lực lúc chạm; hoạt hình giống hệt lúc kéo được; vật đứng im; 3–6 s sau thì buông | Mục 3.6: trạng thái "gồng", nhãn % cần thiết |

---

## 3. Sửa cảm giác và độ rõ (áp dụng cho cả 50 màn)

### 3.1 Kéo tay nắm bị rung, giật (điểm 1)

Ba nguyên nhân, xếp theo mức chắc chắn:

1. **Vòng điều khiển tay nắm mất ổn định (chắc chắn nhất).**
   - Lực tay nắm áp vào ray trễ một nhịp vật lý: ray chạy trước tay nắm trên cùng object, ở cả 59 tay nắm.
   - Hệ số lực tính theo khối lượng tối thiểu 0,18 kg, trong khi xe tay nắm chỉ nặng 30–35 g. Hệ số vòng lặp thành
     khoảng 1,07, lớn hơn 1.
   - Kết quả (mô hình): tay nắm dao động khoảng 20 lần/giây. Lực đập qua lại ±0,672 N, có khung hình tay nắm còn lùi lại.
     Phản lực làm cả thân COghe rung theo.
   - 43/59 tay nắm rơi vào vùng này, gồm màn 4, 5, 6, 7, 9, 10. Tốc độ trung bình không đổi nên các test giải màn hiện có
     không bắt được.
   - **Sửa:** tính hệ số theo khối lượng thật của xe, thêm bù ma sát. Mô hình hết dao động, vẫn kéo được tải 0,22 N.
     Sau đó đổi thứ tự cho ray chạy sau mọi cơ quan đẩy nó, rồi chạy lại toàn bộ test hồi quy.
2. **Cơ quan không nội suy hình, COghe thì có.**
   - 234/241 vật thể vật lý ở các màn Spatial không nội suy.
   - Vật lý chạy 120 Hz. Trên OPPO khung hình xen kẽ 16,6 ms và 33,2 ms, nên cửa, xe, thang nhảy bước không đều cạnh một
     COghe chạy mượt; trông như giật hoặc COghe trượt trên khay.
   - **Sửa:** bật nội suy cho ray và vật, sau khi kiểm tra code đường đi vẫn đọc đúng vị trí vật lý.
3. **Dựng lại bản đồ đường đi mỗi 0,35 s khi đang kéo.**
   - Mỗi lần tốn 7–19 ms trên Mac M5 ở màn 4–10, điện thoại chậm hơn nhiều lần. Đó là khung khựng "thỉnh thoảng".
   - **Sửa:** không dựng lại trong lúc chỉ có ray đang được kéo; dựng một lần khi kéo xong.

Thêm: xúc tu bật ra/biến mất trong một khung, thân giãn 1 → 1,16 đột ngột lúc nắm/buông → làm mềm 0,12 s.

**Kiểm chứng:**
- Test mới `TapRailsPullSmoothly` kéo cả 59 tay nắm. Tiêu chí: lực tối đa ≤ 0,1 N ở màn 4, độ lệch bước tay nắm ≤ 0,2 mm.
- Test giả lập nhịp khung 60/30 fps.
- Đo OPPO trước/sau ở màn 4, 6, 7, 8, 9.

### 3.2 Nút bấm ra nút bấm, thang máy đi hai chiều (điểm 4)

- **Hiện tại:**
  - Chạm nút thì COghe đi vào giữa khay và đứng che nút.
  - Nút lún 4 mm, chỉ trong lúc khay chạy; không có đèn, không có tiếng bấm.
  - Thang đã đi xuống được (chạm lại nút), nhưng không màn nào cần, cũng không có gì gợi ý.
- **Làm lại:**
  - Nút có năm trạng thái theo vật lý thật: *sẵn sàng → nhận lệnh → đang bấm → giữ lún khi chạy → bật lên khi tới nơi*.
  - Nút đặt trên trụ ở góc khay, phía camera; COghe đứng ở nửa kia nên không che nút.
  - Nút lún 8–10 mm. Có một xúc tu vươn ra ấn nút.
  - Vòng đèn: trắng/mint khi sẵn sàng, có mũi tên ▲ hoặc ▼ cho chiều đi tiếp; hổ phách khi đang chạy; nháy khi tới.
  - Tiếng "cạch" khi bấm, "ting" khi tới, "tách" khi nút bật lên. Các file âm thanh đã có sẵn.
  - Chạm khi thang chưa có điện hoặc đang chạy: phản hồi "từ chối" rõ (rung + ổ khoá), không im lặng như bây giờ.

### 3.3 Nút đen màn 13 (điểm 7)

- **Nó là gì:** nút gọi thang ở tầng dưới.
  - Tạo bằng hàm hình trang trí, hàm này xoá va chạm. Vật liệu `Blade.mat` (kim loại 0,86) hiện gần như đen.
  - Lượt dựng art bỏ sót nó: không màu mạch, không chữ A, không lún.
  - Hình dạng giống hệt nút sàn "đứng lên là bấm", nhưng đứng lên thì không có gì xảy ra.
  - Chạm vào lúc đầu màn thì COghe lại **đi ra xa nó** để lên khay.
  - Nếu đang nắm thùng thì mọi chạm vào cơ quan đều bị bỏ qua.
- **Có ở màn 13, 28, 37, 39.**
- **Sửa:**
  - Màn 13 bỏ nút gọi tầng dưới (khay đã nằm sẵn ở dưới).
  - Ở các màn khác: nút gọi thành nút gắn tường, cùng màu mạch với thang, có chữ, mũi tên ▲/▼, lún khi bấm, có vạch nối
    tới thang. Không dùng hình đĩa phẳng trên sàn, vì hình đó đã có nghĩa là "nút đứng".
- Chạm vào cơ quan khi đang nắm vật → tự buông vật rồi làm, hoặc báo "Buông vật trước".

### 3.4 Ống: chạm thân ống, chạm nhánh (điểm 8)

Ống có ở 11 màn: 14, 15, 16, 19, 20, 32, 36, 38, 40, 42, 43. Ngã ba chỉ có ở 16, 32, 43.

- **Chạm thân ống từ bên ngoài:** COghe đi tới miệng ống gần nhất *theo đường đi bộ được* (không theo khoảng cách
  thẳng), chui vào và đi hết đoạn được chạm. Ống một đoạn: chạm đâu cũng chui qua.
- **Ở ngã ba:** chạm bất kỳ đâu trên nhánh nào thì đi nhánh đó. Có thể chạm trước khi tới ngã ba: COghe vẫn dừng gom đủ
  thân ở ngã ba như luật hiện tại rồi đi tiếp ngay, không bắt chạm lần nữa.
- **Nhánh đóng:** đi tới ngã ba cuối còn mở thì dừng; nhánh đóng mờ đi, có ổ khoá, báo "Nhánh đang khoá". Hiện tại chạm
  nhánh đóng bị nuốt im lặng.
- **Phản hồi:** vòng chạm hiện ở chỗ chạm (hiện ống không có). Tuyến đã chọn sáng lên màu ngọc của ống (không dùng mint
  vì mint là cửa thoát; không dùng xanh/san hô vì đó là mạch A/B).
- **Ai được chạm trước:** vật bị tia chạm trúng đầu tiên. Thứ tự đó sửa luôn lỗi cũ: hộp chạm của Q "ăn" chạm sàn phía sau
  Q, miệng ống bắt chạm sau vật cản 6 cm.
- **Rủi ro:**
  - Phải chạy lại toàn bộ test các chiến dịch dùng chung runtime ống.
  - Test lang thang của E02, E10, E13 phải lọc lại điểm chạm.
  - Hiệu ứng sáng tuyến cần art direction duyệt, vì `STYLE_RULES` hiện ghi ống "không có hành vi runtime".

### 3.5 Khoá nhìn thấy được (điểm 9)

- **Màn 18:**
  - Thay khoá OR vô hình bằng `COgheSpringAccessDoor` (có sẵn, đang nằm ở chiến dịch cũ).
  - Nửa A đứng nút thì cửa kéo lên ngược lò xo. Tay B đặt **ngay cạnh cửa** phía bên kia, chỉ kéo được khi cửa đã lên hết.
  - Khi B kéo xong, chốt trượt vào khung cửa kèm tiếng "cạch" và dòng "Cửa đã chốt — có thể rời A".
  - Nửa A đi được, cửa vẫn đứng.
- **Mọi khoá khác:**
  - Chốt và lẫy đặt *trên vật bị khoá*, đủ to để thấy từ camera mặc định: màn 33, 35, 37, 39, 40, 42, 43, 45, 49, 50.
  - `COgheLoadLatch` đã hỗ trợ chốt và lẫy; màn 34 đang làm đúng, lấy làm mẫu.
- **Cửa và nắp:** vẫn trơn nhưng không tô tím như tường. Dùng khung ngà và dải màu mạch.
- **Đèn trạng thái cửa:** `COgheViewMechanism` có sẵn đèn nhưng các màn Spatial chưa nối.

### 3.6 Cảm giác lực khi phần nhỏ kéo vật nặng (điểm 10)

- **Lực:** vẫn 7 × khối lượng phần thân (cả thân 0,672 N; nửa 0,336 N; một phần tư 0,168 N). Không chặn lệnh: COghe
  vẫn tới và thử.
- **Khi kẹt:** lực tăng dần tới tối đa trong 0,6 s; "thất bại" nghĩa là đã dốc hết sức mà không đủ.
- **Trạng thái mới "gồng" (TooHeavy):**
  - Xúc tu căng thẳng và mảnh lại, thêm một xúc tu.
  - Thân giãn tới 1,3 và ngả ra sau.
  - Hai–ba nhịp rướn cách nhau 0,5 s, run 8–12 Hz theo mức gắng.
  - Vật nhích 2–4 mm rồi bật về (lúc đầu làm phần hình, không đụng vật lý).
  - Sau khoảng 1,5 s thì buông: xẹp xuống, lắc đầu, tiếng "hừm".
  - Phân biệt với "bị kẹt" (đang khoá/vướng): kẹt chỉ có tiếng "hm" như hiện tại.
- **Nhãn cân trên vật nặng:**
  - Biểu tượng quả cân và % cần thiết (ví dụ ●●○○ = 50%).
  - Khi phần đang chọn gồng: thanh đo tô phần hiện có so với phần cần.
  - Product UI hiện thông báo cho người chơi, ví dụ "Too heavy for this part — merge and try again". Hiện Product UI
    không hiện thông báo nào cả.
- **Nút sàn có đồng hồ cân:** dùng cho chương 4 (mục 5.4).

### 3.7 Sửa nhanh khác

- **Biển số màn:**
  - Dời khỏi đường nhìn mặc định hoặc mờ đi khi đang chơi.
  - Hiện đang che tay nắm/vòng dây/khối ở 4, 11, 12, 17, 21, 22, 24, 26, 27, 29, 30, 36, 44, 45, 49, 50.
  - Màn 22 và 24 bị che mất vòng dây, tức chính thứ cần chạm.
- **Dòng gợi ý:**
  - Dữ liệu màn có câu gợi ý nói luôn lời giải (ví dụ màn 12 "Khối cao vào trước, khối thấp vào sau.", màn 27
    "Nhịp xa vào trước."). Product UI hiện **không** hiện các câu này (chỉ HUD bản dev), nên người chơi thử không bị lộ.
  - Viết lại thành gợi ý thao tác cho màn dạy cơ quan mới. Giữ lời giải cho nút "Gợi ý" theo yêu cầu, có thể nối với
    "gợi ý bằng quảng cáo thưởng" còn trong kế hoạch kiếm tiền.
- **Lỗi hiển thị nhỏ:**
  - Màn 25 vừa vào đã báo "Đối trọng đang kéo cầu" (khay rỗng tự võng).
  - Mặt trơn màu tím chưa từng được dạy trước màn 6.
  - Màn 3 lộ mép vòng thoát.
  - Màn 4 vòng thoát bị cửa che hết; nên lộ mép để thấy đích.

---

## 4. Bộ cơ quan logic (đọc là hiểu cái gì trước, cái gì sau)

Nguyên tắc chung:
- Mọi khoá có nguồn nhìn thấy, có vạch nối từ nguồn tới khoá.
- Chạm vào thứ đang khoá → từ chối rõ (rung + ổ khoá) chứ không im lặng.
- Cửa mang màu của thứ mở nó.
- Chữ cái thống nhất: A xanh, B san hô là mạch chính; C, D màu trung tính là cần mở khoá.

| Cơ quan | Ý của Mrk / dùng ở | Dựng bằng | Trạng thái |
| --- | --- | --- | --- |
| Chụp kính che công tắc | Màn 5: công tắc sàn mở chụp che công tắc vách | Cửa trượt trong suốt (`PlusGate`) + `ViewLink` hoặc `COgheLoadLatch`; tay nắm có `RequiredRail` + chốt `InterlockPin` | Có sẵn (mẫu `ChapterCover` ở chiến dịch cũ) |
| Hộp chứa công tắc | Màn 6: kéo tay vách mở hộp chứa tời dưới sàn | Nắp trượt + `ViewLink`; tời có `RequiredRail` = nắp | Có sẵn |
| Chốt chặn cầu | Màn 7, 9, Boss 10 | Chốt = cửa trượt nhỏ có tay riêng; cầu kéo tay dùng `RequiredRail`. Cầu do `ViewLink` đẩy cần thêm `RequiredRail` cho `COgheViewMechanism` | Có sẵn; cần mở rộng nhỏ cho `ViewLink` |
| Chốt dưới cầu | Boss 10 | Logic như trên; phải thấy được (đầu chốt đỏ lộ ra mép cầu, xoay camera thấy cả chốt) | Có sẵn; cần làm hình |
| Khối nặng đè cầu | Boss 10 (biến thể), chương 3 | Component mới `COghePropZone`: cầu chỉ nâng khi vùng trên cầu trống; đèn/nhãn "Cầu đang bị đè" | **Cần làm mới** |
| Cửa giữ bằng lò xo + chốt | Màn 18, 17, 19, 20, 38 | `COgheSpringAccessDoor`; thêm chữ trạng thái và tiếng cạch | Có sẵn; thêm phản hồi |
| Nút bật lại | Màn 8, 9, Boss 10, 13, 28, 37 | Trạng thái nút trong `COghePassengerLift` + phần hình mới | Cần làm (mục 3.2) |
| Khối chặn lò xo | Màn 13, 19 | `COgheTapRail.HoldAtEnd` + `ReturnForce` (vật lý có sẵn); hình lò xo mới | Thiếu hình |
| Nút sàn có đồng hồ cân | Chương 4 | `COgheTissueSensor.Load` đã có; thêm hình đồng hồ | Cần làm hình |
| Thanh răng hai chiều, mô tơ mạnh theo khối lượng người đứng | Chương 5 | Mở rộng `COgheGearTrain`, bật theo cờ để màn cũ không đổi | Cần làm |
| Cân hai đĩa có lẫy khi thăng bằng | Boss 40 | Dựng thử từ `PlusSeesaw` / `COgheSeesawBridge` | Cần dựng thử |
| Điều kiện chung "cần / mở" | Nối mọi đầu ra (thùng đã vào ổ, cầu đã chốt, bàn xoay đã khớp…) vào mọi đầu vào | Adapter `COgheCondition` | Không bắt buộc; làm khi cần |

---

## 5. Kế hoạch 50 màn

Chu kỳ 10 màn áp cho mọi chương:
1. Dạy một cơ quan mới.
2. Hai–ba màn luyện, mỗi màn thêm một bước hoặc một kiểu phụ thuộc mới.
3. Kết hợp với cơ quan cũ.
4. Một màn thứ tự kiểu màn 12.
5. Màn chuẩn bị Boss.
6. Boss: bố cục mới, nhiều bước hơn, có một mẹo.
7. Sau Boss là màn dạy cơ quan mới của chương sau.

Luật đi kèm:
- Không màn nào chơi trước "aha" của màn kế tiếp.
- Không lặp bố cục trong ba màn liền nhau.
- Mục tiêu tối đa khoảng 18–20 chạm.

"QĐ" = số quyết định (lựa chọn mà chọn sai là hợp lý), ước lượng. "Chạm" = ước lượng số chạm.

### 5.1 Chương 1 · Cơ quan đầu tiên (1–10)

| Màn | Bây giờ | Làm lại thành | QĐ / chạm |
| --- | --- | --- | --- |
| 1 | Chạm để đi | Giữ | 0 / 1 |
| 2 | Leo hai bậc | **"Đi vòng mặt tím":** mặt khối nhìn thấy trước là tím trơn, phải vòng sang mặt ngà để leo. Dạy vật liệu trơn trước khi màn 6 cần | 1 / 2 |
| 3 | Xoay camera | Giữ; giấu hẳn vòng thoát (hiện lộ một mép) để phải xoay thật | 1 / 2–3 |
| 4 | Kéo tay A mở cửa | Giữ một thao tác (màn dạy); sửa rung; dời biển số khỏi tay A; vòng thoát lộ mép sau cửa; đèn cửa | 1 / 2 |
| 5 | Màn 4 trên vách | **"Mở nắp trước" (ý Mrk):** cần C dưới sàn → chụp kính trượt khỏi tay A trên vách → leo lên kéo A → cửa mở. Chạm A trước: A lắc, hiện ổ khoá | 2 / 3–4 |
| 6 | Ròng rọc, một tay nắm | **"Leo lên, mở hộp, quay tời" (ý Mrk):** leo bệ, kéo tay C trên vách → nắp hộp quanh tời A dưới sàn mở → quay A → cầu nâng → qua. Ngược thứ tự màn 5 (trên trước, dưới sau) | 2 / 4–5 |
| 7 | Một tay trượt cầu | **"Chìa khoá dưới hố" (ý Mrk):** chốt chặn xuyên ray cầu, vạch nối chạy xuống hố tới cần D → xuống hố kéo D → chốt rút → leo lên kéo B → cầu trượt vào → qua | 2 + xuống hố / 5–6 |
| 8 | Bấm nút lên thang | **"Lên, kéo, xuống" (ý Mrk):** bấm nút (lún, giữ khi chạy) → lên cao, nút bật lên → kéo A trên cao mở cửa thoát ở dưới đất → bấm lại nút → thang xuống → thoát | 3 / 4–5 |
| 9 | Màn 6 + một tay nắm | **"Cây cầu vừa chắn vừa là đường":** cầu B nằm ngang trên giếng thang nên thang từ chối ("giếng bị chắn"); chốt chặn cầu, mở bằng cần C → kéo B: cầu trượt khỏi giếng **và** bắc sang tháp thoát → đi thang lên → qua cầu | 3 / 5–6 |
| 10 | BOSS = màn 9 + thang | **BOSS "Cỗ máy thân quen" (bố cục mới):** xem bên dưới | 5 + 1 phát hiện / 8–10 |

**Boss 10.** Bố cục mới: sàn xuất phát | hố giữa chứa cầu ròng rọc | phía nhận có cửa thoát cao. Thang B nối sàn với đáy hố
và **đang đậu ở trên**.

1. Leo vách kéo tay C → chụp kính trên tời A mở (màn 5–6).
2. Chạm tời A → bị từ chối. Đầu chốt đỏ lộ ra dưới mặt cầu; xoay camera thấy chốt khoá cầu và vạch nối xuống cần D
   dưới hố (màn 3 + màn 7).
3. Bấm nút thang B → thang đi **xuống** hố, ngược mọi lần trước (màn 8 đảo chiều).
4. Kéo D → chốt rút.
5. Bấm nút (đã bật lên) → thang lên.
6. Quay tời A → cầu nâng và chốt, đồng thời kéo cửa thoát mở: cây cầu là chìa khoá.
7. Qua cầu → thoát.

**Mẹo:** nguyên nhân kẹt chỉ thấy khi đổi góc nhìn; thang chỉ quen đi lên lại là đường xuống.

**Biến thể theo ý Mrk:** thay bước 3–5 bằng khối nặng đè lên cần D, phải kéo khối ra trước. Khối chỉ là vật cản, không
làm bậc, để màn 11 vẫn giữ được ý mới.

### 5.2 Chương 2 · Một thành hai: máy Q, ống, giữ và chốt (11–20)

Đưa máy Q (điểm hấp dẫn nhất của game) từ màn 17 lên màn 11, ngay sau Boss đầu.

| Màn | Bây giờ | Làm lại thành | QĐ / chạm |
| --- | --- | --- | --- |
| 11 | Kê một bậc (lặp ý màn 7 cũ) | **Dạy máy Q** (đưa màn 17 cũ lên): tách đôi, hai nút cùng lúc, tự nhập | 4 / 6–8 |
| 12 | Khối lớn đi trước | Giữ (màn thứ tự mẫu); dời biển số | 4 / 7 |
| 13 | Thang chở hàng (= màn 28 bỏ bớt bước) | **"Khối chặn lò xo":** giao lộ như màn 12 nhưng khối chặn bị lò xo kéo về chỗ cũ → một nửa giữ khối chặn, nửa kia đẩy khối cao qua, buông ra thì khối chặn bật về làm bậc thấp | 5 / 8–9 |
| 14 | Dạy ống | Giữ; ống đậm hơn, chạm thân ống được, chỗ xuất phát không đè lên tay nắm | 3 / 3 |
| 15 | Hai ống một đích (chơi trước màn 16) | **"Chuẩn bị trước khi đi":** khối bậc cho phòng bên kia đẩy qua khe dưới vách, tay nắm chỉ có ở phòng này → phải đẩy trước rồi mới chui ống. Đi trước thì quay lại được | 4 / 5–6 |
| 16 | Ngã ba | Giữ; phản hồi nhánh đóng; tuỳ chọn: tay nắm hai nấc (mở nhánh này đóng nhánh kia) | 5 / 6 |
| 17 | Dạy Q | **"Bạn giữ, mình luồn"** (đưa màn 19 cũ lên): giữ tạm, bạn chốt vĩnh viễn; chốt nhìn thấy trên nắp ống | 6 / 8 |
| 18 | Giữ cửa cho bạn (= màn 19 cũ chơi trước) | **"Nửa thân không đủ sức" (điểm 9 + 10):** nửa A đứng nút mở cửa lò xo → nửa B qua; thử kéo khối 100% thì **gồng không nổi** → kéo chốt B cạnh cửa (cạch, chốt cắm vào cửa) → nửa A đi qua → nhập → cả thân kéo khối làm bậc → thoát | 7 / 10 |
| 19 | Bạn giữ, mình luồn | **"Người giữ có việc thứ hai":** giải phóng người giữ bằng chốt, rồi giao người đó giữ khối chặn lò xo (màn 13) trong khi bạn đẩy khối qua giao lộ ở phòng bên kia | 9 / 12–14 |
| 20 | BOSS = màn 19 + thùng + tời | **BOSS "Hai nửa một máy" (làm lại):** tách mở đường → nhập để quay tời 100% (khối 100% có nhãn; nửa thân gồng không nổi) → qua cầu, **tách lại** ở Q thứ hai để đứng hai nút hai bên vách trơn mở cửa thoát. Mẹo: lên kế hoạch lúc nào cần cả thân, lúc nào cần hai nửa | 12 / 18–20 |

### 5.3 Chương 3 · Dây, sức nặng và khối chồng (21–30)

| Màn | Bây giờ | Làm lại thành | QĐ / chạm |
| --- | --- | --- | --- |
| 21 | Cõng thùng | **Dạy đu dây** (đưa màn 22 cũ lên); dời biển số khỏi vòng dây | 2 / 3 |
| 22 | Đu dây | **"Chất hàng trước"** (E04 sửa): khi xe đã cập bờ thì tay nắm thùng bị kẹt vào mép → phải trượt thùng trên xe trước rồi mới kéo xe | 3 / 5 |
| 23 | Hai nhịp dây (= màn 22 hai lần) | **"Đưa bến lại gần"** (màn 24 cũ lên): cần B đặt xa chỗ xuất phát để người chơi đu hụt trước rồi mới nghĩ ra; hụt thì tời kéo về như hiện có. Bỏ "Hai nhịp dây" | 3–4 / 5–6 |
| 24 | Đưa bến lại gần | **Dạy đối trọng** (màn 25 cũ lên); sửa chữ báo sai lúc vào màn | 2 / 4–6 |
| 25 | Kéo đối trọng | **"Chưa đủ nặng":** thùng chỉ nâng ván nửa chừng → COghe tự leo lên khay làm thêm đối trọng → ván thăng bằng, lẫy bắt → leo ra, qua | 4 / 6 |
| 26 | Bập bênh (hai ván giống nhau) | **"Nhẹ quá không nghiêng":** Q + hai nút mở cổng vào phòng bập bênh; nửa thân đi qua trục thì ván kẽo kẹt không nghiêng → nhập lại → cả thân nghiêng ván, xuống phòng bên | 5 / 8 |
| 27 | Ba mảnh thành đường | Giữ ý, đào sâu: bốn mảnh, bãi chờ chỉ chứa một → phải xếp lần lượt hai khối chặn | 6 / 10 |
| 28 | Thùng đi thang | **"Gửi hàng lên trước":** khay chỉ chở được thùng **hoặc** COghe → đẩy thùng lên khay, bước ra, bấm nút gọi tầng trên gửi thùng lên, gọi khay về, rồi mới lên. Nút gọi tầng (nút đen cũ) thành mấu chốt | 6 / 10 |
| 29 | Chồng hai tầng (thứ tự nào cũng được) | **"Xếp tầng trên trước":** khối trên phải đẩy ra đầu xa *trước khi* khối dưới cập bờ, vì sau đó tay nắm bị kẹt; mái nhô chặn nếu làm sai thứ tự. Dạy nhỏ ý chính của Boss 30 | 4–5 / 7 |
| 30 | BOSS Tháp khối | **BOSS làm lại:** nửa 1 ngồi lên khay nặng, dây kéo chốt của khối C1 lộ ra trước → nửa 2 đu dây sang kệ cần → kéo B, chốt cắm (nửa 1 được thả) → đẩy C2, đẩy C3 lui khỏi mái nhô → dùng chồng khối làm cầu thang đi xuống → nhập → cả thân kéo C1 (100%) → leo C1 → C2 → C3 → thoát. Mẹo: tháp vừa là cầu thang lên, vừa phải xếp ngược lại | 11 / 18 |

### 5.4 Chương 4 · Chia đúng cỡ: 25%, 75%, đồng hồ cân (31–40)

Chương 2 dạy "nửa thân yếu hơn", chương 3 dạy "sức nặng là công cụ", chương 4 dạy **chia chính xác**. Bánh răng dồn hết
sang chương 5; hiện màn 31 dạy bánh răng rồi chín màn sau không dùng.

| Màn | Bây giờ | Làm lại thành | QĐ / chạm |
| --- | --- | --- | --- |
| 31 | Bánh răng đầu tiên | **"Cân ở cửa":** dạy chia hai lần (25%) và nút có đồng hồ cân; 25% chỉ đầy nửa đồng hồ, hai phần tư cùng đứng mới đủ | 4 / 7 |
| 32 | Đổi tuyến trên vách (= màn 16) | **"Nặng đi trước":** giao lộ màn 12 + khối nặng đỗ giữa giao lộ → phải đẩy khi còn nguyên thân, rồi mới tách. Tách trước thì gồng không nổi, nhập lại sửa được | 6–7 / 10 |
| 33 | Đủ nặng mới mở | **"Hai phần tư thành một nửa":** tách cho cổng hai nút, nhập lại cho đủ cân để chốt cửa; chốt hiện trên khung cửa | 7 / 11 |
| 34 | Hai rồi bốn (= màn 17 bốn nút) | **"Bập bênh nâng bạn":** phần 50% làm đối trọng nâng phần 25% lên bờ cao; 25 với 25 hay 50 với 50 thì cân, không lên | 6 / 10 |
| 35 | Giữ lại phần lớn | **"Ba phần tư":** Q chỉ chia đôi, nhưng nhập 25 + 50 = 75 để kéo khối ●●●; phần 25 còn lại giữ chốt | 7 / 11 |
| 36 | Đu rồi luồn | Màn nghỉ; dùng điều khiển ống mới (chạm thân ống, chọn nhánh trước) | 2–3 / 5 |
| 37 | Giữ thang cho bạn (lặp 18, 19) | **"Thang chở thùng nặng":** cả thân đẩy thùng lên khay trước → tách → một nửa giữ điện, nửa kia lên cùng thùng, gồng không đẩy nổi thùng → chốt điện → nửa dưới lên → nhập → đẩy thùng vào ổ | 8 / 12 |
| 38 | Đu và luồn | Giữ chuỗi (aha tốt); sửa độ rõ: khay gấp lộ mép, chốt trên nắp ống, ống đậm hơn | 6 / 9 |
| 39 | Bốn trạm (= màn 37 + hai nút) | **"Xưởng lắp cầu"** (màn 45 cũ lên làm chuẩn bị Boss): mảnh xa vào trước, khoá khung nhìn thấy, cả thân quay tời nâng cả khung. Bỏ "Bốn trạm" | 8 / 13 |
| 40 | BOSS Hộp cộng hưởng (ghép các ý cũ, 29 chạm) | **BOSS "Cân ba phần tư":** cân chỉ nhận đúng ●●●. Cả thân lên thì nghiêng sai chiều, 50% thì không đủ → phải tạo 75% (nhập 25 vào 50), 25% còn lại giữ chốt trục cân ở phòng bên. Mẹo: cả chương "nặng hơn là tốt", Boss đòi đúng cân | 10–11 / 15 |

### 5.5 Chương 5 · Bánh răng (41–50)

| Màn | Bây giờ | Làm lại thành | QĐ / chạm |
| --- | --- | --- | --- |
| 41 | Khớp một bánh | **Dạy bánh răng** (gộp ý màn 31 cũ): máy quay suông → đưa bánh G vào khe (vòng khe trống hiện trong cảnh, đèn khớp) → máy chạy | 3 / 5 |
| 42 | Hai ống hai nửa (23 chạm, ~0 quyết định) | **"Hai xe chéo nhau":** màn 12 bằng xe bánh răng: xe K2 đỗ chắn đường K1 → lùi K2, đẩy K1 vào, rồi đẩy K2 vào khe của nó | 5 / 8 |
| 43 | Đường ống ba chiều | **"Ống theo hộp số":** bánh răng chọn van nào quay; đặt tuyến trước khi bạn chui; ống tô màu theo tuyến, bỏ khối "chọn van" giả | 7 / 11 |
| 44 | Hai tầng răng (bắt đi ngược) | **"Bánh đệm đổi chiều":** mũi tên trên bánh răng; chèn bánh đệm thì cửa đổi chiều; thứ tự chèn quyết định cái gì chạy | 4–5 / 7 |
| 45 | Xưởng lắp cầu (đã lên 39) | **"Ai chạy máy":** lực mô tơ theo khối lượng người đứng nút; thang dừng nửa chừng → người nặng chạy máy, người nhẹ đi thang (75/25) | 8 / 12 |
| 46 | Người chạy máy (27 chạm) | **"Bàn xoay chở hàng"** (màn nghỉ): đẩy thùng lên bàn, bước ra, bàn xoay mang thùng thành bậc | 4–5 / 7 |
| 47 | Bàn xoay (= bố cục màn 31) | **"Mượn bánh":** chỉ có một bánh G; chạy máy 1 cho cổng chốt lại rồi mới rút G sang máy 2 | 5 / 7 |
| 48 | Hai máy nối nhau (= màn 41) | **"Hai tầng trục":** ý trục của màn 44 cũ nhưng không bắt đi ngược; máy dưới đẩy xe bánh răng tầng trên vào khớp | 4–5 / 8 |
| 49 | Ba lớp răng | **"Hai động cơ một cửa"** (chuẩn bị Boss): hai mô tơ cộng lực; phải chọn cỡ chia (50 + 25) để vẫn còn một phần đi chốt | 7 / 12 |
| 50 | BOSS = tháp màn 49 + thêm phần | **BOSS "Hộp số" (bố cục mới):** lắp bánh đệm để mở cầu thang → chuyển bộ chọn sang cửa, một phần tư chạy máy gồng không nổi → nhập 75% chạy máy → cầu cuối cần chiều cũ nên phải **tháo bánh đệm ra** rồi chạy lại. Mẹo: lắp vào rồi phải tháo ra; phần nặng nhất chạy máy cuối | 11–12 / 18 |

### 5.6 Đường cong mới (số quyết định, ước lượng)

- **Chương 1:** 0 · 1 · 1 · 1 · 2 · 2 · 2 · 3 · 3 · **6** (hiện tại: 0 0 0 1 1 1 1 1 2 3)
- **Chương 2:** 4 · 4 · 5 · 3 · 4 · 5 · 6 · 7 · 9 · **12**
- **Chương 3:** 2 · 3 · 3–4 · 2 · 4 · 5 · 6 · 6 · 4–5 · **11**
- **Chương 4:** 4 · 6–7 · 7 · 6 · 7 · 2–3 · 8 · 6 · 8 · **10–11**
- **Chương 5:** 3 · 5 · 7 · 4–5 · 8 · 4–5 · 5 · 4–5 · 7 · **11–12**

Mỗi chương một đường răng cưa: chỉ trũng ở màn dạy và màn nghỉ, tăng dần vào Boss. Không màn nào quá khoảng 20 chạm
(hiện màn 40 cần 29 chạm).

---

## 6. Mạch "thứ tự" kiểu màn 12 xuyên suốt

Màn 12 hay vì bốn điều: hình đơn giản, các khối chặn nhau thật, mọi bước làm lại được, thứ tự đúng phải suy ra. Các màn mới
phát triển ý đó:

| Màn | Kiểu thứ tự |
| --- | --- |
| 12 | Khối chặn tránh sang rồi quay lại làm bậc (mẫu gốc) |
| 13 | Khối chặn có lò xo nên phải tách đôi: giữ → đẩy → buông |
| 15 | Đẩy bậc sang phòng bên trước rồi mới chui ống |
| 19 | Chốt trước để giải phóng người giữ, rồi giao người giữ việc thứ hai |
| 22 | Trượt thùng trên xe trước khi xe cập bờ |
| 27 | Bốn mảnh, bãi chờ một chỗ |
| 28 | Gửi thùng lên trước, gọi khay về, rồi mới lên |
| 29 | Xếp tầng trên trước khi khối dưới cập bờ |
| 32 | Việc nặng làm khi còn nguyên thân, rồi mới tách |
| 42 | Hai xe bánh răng chéo nhau |
| 44 | Thứ tự chèn bánh đệm |
| 47 | Chốt đầu ra trước rồi mới mượn bánh |
| Boss 30, 50 | Dùng xong phải tháo hoặc xếp ngược lại |

Ý dự phòng: một khối phải đỗ tạm vào chỗ cuối của khối khác rồi rút ra; một tay nắm đẩy khối X tới đồng thời kéo khối Y lui
(nối bằng `ViewLink`); dùng `COgheExitRailLock` để "khối đã leo lên phải kéo ra vì nó chặn cửa sập".

---

## 7. Thứ tự triển khai và kiểm chứng

| Giai đoạn | Nội dung | Điều kiện qua |
| --- | --- | --- |
| **0. Cảm giác và độ rõ** | Mục 3 toàn bộ: rung khi kéo, nút thang, nút đen, chạm ống, khoá nhìn thấy, gồng + nhãn cân, biển số, thông báo trong Product UI | Test `TapRailsPullSmoothly` đạt; toàn bộ PlayMode + replay native 50 màn đạt; đo OPPO màn 4, 6, 7, 8, 9 trước/sau |
| **1. Chương 1** | Màn 2, 3, 5–10 mới (mục 5.1) + cơ quan logic dùng tới (chụp kính, hộp, chốt, nút bật lại) | Mỗi màn: dossier, lời giải mẫu, test giải + test lang thang, ảnh build. Rồi **3–5 người mới chơi thử** |
| **2. Chương 2** | Q lên màn 11, màn 13, 15, 17–20 mới | Như trên + người mới chơi lại từ đầu |
| **3. Chương 3** | Đổi chỗ dây/đối trọng, màn 22, 25, 26, 28–30 mới, `COghePropZone` | Như trên |
| **4. Chương 4–5** | Đồng hồ cân, thanh răng hai chiều, mô tơ theo khối lượng, cân hai đĩa (dựng thử trước), các màn mới | Như trên |

**Đo xem đã "hook" chưa:**
- Game đã gửi `level_start`, `level_complete`, `level_fail`, `level_abandon`, `level_retry` kèm thời gian.
- Thêm: số chạm, số chạm bị từ chối, số lần gồng không nổi, lần mở gợi ý, thời gian tới thao tác đúng đầu tiên.
- Khi chơi thử, ghi:
  - màn nào người chơi do dự, chạm sai;
  - có "à ra thế" không;
  - câu hỏi cuối: *"Màn nào làm bạn thấy mình thông minh?"*. Mỗi chương phải có ít nhất 2–3 màn được nhắc tên.
- Mục tiêu:
  - không ai bỏ cuộc ở chương 1;
  - thời gian trung vị mỗi màn tăng dần trong chương, giảm ở màn sau Boss;
  - không màn thường nào có tỉ lệ bỏ cuộc cao hơn Boss của chương đó.

---

## 8. Cần Mrk chốt

1. **Thứ tự chương mới:**
   - Q lên màn 11; dây lên màn 21.
   - Chương 4 thành "chia đúng cỡ", bánh răng dồn sang chương 5.
   - Bỏ 8 màn trùng ý: Kê một bậc (11), Thang chở hàng (E01), Hai ống một đích (E02), Giữ cửa cho bạn (E03),
     Hai nhịp dây (E05), Đổi tuyến trên vách (23), Bốn trạm (27), Hai ống hai nửa (E13).
   - Thay bằng 8 màn mới: 13, 15, 18, 19, 25, 32, 42, 44 (mục 5). Tổng vẫn 50 màn; các màn còn lại sửa trên nội dung cũ
     hoặc đổi chỗ.
2. **Gợi ý:** Product UI chỉ có mũi tên ở màn 1, 3, 4. Đề xuất: màn dạy cơ quan mới có gợi ý thao tác, còn lại có nút
   "Gợi ý" theo yêu cầu (có thể xem quảng cáo thưởng để mở). Đồng ý không?
3. **ID và save:** game chưa phát hành. Đề xuất dựng lại nội dung trên ID cũ khi cùng ý, ID mới khi là màn mới hẳn, và xoá
   tiến trình của các bản test.
4. **Đồng ý làm Giai đoạn 0 + chương 1 trước, cho người mới chơi thử, rồi mới làm chương 2–5?**

---

## 9. Ghi chú về độ chắc chắn

- Lặp code, thông số, nút đen, cách chạm ống, ống ở màn nào, khoá ẩn ở đâu: **đã xác nhận** bằng đọc code và file scene.
- Rung khi kéo: thứ tự component, khối lượng, hệ số đã xác nhận bằng đọc. Biên độ dao động là **mô hình**, cần đo trên máy.
- Tác động của việc không nội suy và dựng lại bản đồ đường đi trên Android: **giả thuyết**; Spatial chưa từng được đo trên
  Android.
- Các màn mới chưa dựng: số quyết định và số chạm là ước lượng thiết kế. Bập bênh nâng bạn (34), cân hai đĩa (40),
  thang chở 50% + thùng (37), bàn xoay chở thùng (46) phải dựng thử trước khi chốt.

## 10. Nguồn

Báo cáo chi tiết (tiếng Anh, có file:dòng) trong `PLANS/level-hook-audit/`:
- `L01_10.md`, `L11_30.md`, `L31_50.md`: rà từng màn theo vị trí chơi.
- `TECH_FEEL.md`: rung khi kéo, nút thang, cảm giác lực.
- `TECH_INPUT_LOCKS.md`: chạm ống, bộ cơ quan khoá/cửa, nút đen màn 13.

Tài liệu repo: `Docs/COGHE_LEVEL_DESIGN_RULES.md`, `Docs/LevelDesign/COghe/SpatialPlus20/PLACEMENT.md`, builder
`Assets/_Game/Editor/COgheSpatial*.cs`, lời giải `Assets/_Game/Venom/Runtime/ChapterProof/COgheSpatial*Scenario.cs`.
