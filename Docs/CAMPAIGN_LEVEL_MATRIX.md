# Ma trận campaign 001–100 — thiết kế và triển khai

**Trạng thái: toàn bộ C001–C100 đã được author vào Campaign riêng; chưa có dữ liệu playtest người thật để chốt độ khó.** Physics Lab với 23 màn vẫn được giữ để đối chứng. Số campaign không đổi ID, prefab hay thứ tự của Lab. “Nguồn Pxx” chỉ prototype làm nền; prefab biến thể và đường hồi phục đã được dựng theo ma trận, nhưng mức dễ/khó, readability và feeling vẫn cần được thử với người chơi.

Campaign hướng tới cảm giác thư thái, tiến bộ đều và một boss mỗi 10 màn, từ **010 đến 100**. Mỗi chương có ít nhất hai màn thở ở vị trí **x4 và x9**, có thể đổi hình dạng hoặc thẩm mỹ nhưng không thêm hành động cần học. Mỗi màn giới thiệu tối đa một mechanic hoặc một hành động mới; boss không có mechanic mới. Không tự sinh 100 màn bằng việc đổi màu, thay hình vỏ hoặc chỉ thêm số lần lặp.

## Cách đọc và giới hạn của kế hoạch

- **D mục tiêu** là trần mềm cho authoring trên thang 0–100, chưa phải điểm đo hay dự báo tỷ lệ hoàn thành. Không thêm vật cản để đủ điểm. Có thể hạ ngân sách hoặc đổi biến thể theo playtest; đặc biệt giữ các màn giới thiệu hai bi và chất lỏng thật đơn giản.
- 40 màn đầu được dàn lại để nhường không gian phát triển cho bản 100 màn. Các đỉnh boss lần lượt là **22, 34, 43, 54, 60, 65, 68, 72, 75, 80**; đỉnh màn thường trong mỗi chương thấp hơn boss 5–8 điểm. Các màn x4/x9 có ngân sách giảm 3–7 điểm so với màn ngay trước, nhưng số thấp chưa tự chứng minh cảm giác nghỉ: phải đo thời gian thao tác, suy luận và hồi phục thực.
- Cột **Nguồn và biến thể** nêu sự thay đổi về bố trí, liên kết đường đi hoặc chuỗi quyết định. Một mechanic có nhiều biến thể nhưng hành vi vật lý giữ nhất quán. VFX không quyết định quyền đi qua.
- Cột **Hồi phục** là yêu cầu bằng hình học, trọng lực và chốt có hành vi rõ ràng. Không teleport bi, đóng băng bi giữ hộ, vô hiệu collider hoặc tự đưa bi về sân. Các sân/hốc/đường thu hồi phải được xây và kiểm thử, không mặc định đã có vì kế thừa prototype.
- Giữ bi, trọng lực và điều khiển chung; môi trường có tác động khác phải được giới thiệu bằng đối chứng. Giữ hỗ trợ tiếp cận 40 mm ở lỗ cuối, không dùng ở lỗ chuyển nội bộ. Tất cả bi phải thoát hoàn toàn qua lỗ thật mới thắng.
- “Giới thiệu” = một nội dung mới; “Luyện” = biến thể kỹ năng đã biết; “Kết hợp” = nối nội dung đã học; “Thở” = ngắn, ít yêu cầu; “Đối chứng” = tái kiểm tra cảm giác quen để so sánh; “Boss” = tổng hợp, không luật mới.

## Chương 1 — Làm quen với trọng lượng và hình dạng

Một bi, môi trường khô. Học nghiêng, gia tốc, phanh và đổi hướng; không có cơ cấu chuyển động mới. Boss chỉ kết hợp điều hướng quanh hình học đã biết.

| Màn | Tên | Vai trò | Nguồn và biến thể cần dựng | Kỹ năng mới hoặc luyện | Hồi phục vật lý cần có | D mục tiêu |
| --- | --- | --- | --- | --- | --- | ---: |
| 001 | Lăn nhẹ thôi | Giới thiệu | P01 — hộp tròn, đoạn đi ngắn, sân thoát rộng | Giới thiệu: nghiêng nhẹ để bi bắt đầu lăn; quan sát bi đi hẳn ra khỏi hộp | Một lòng hộp mở, thành cong giữ bi; đi quá chỉ cần nghiêng lại | 6 |
| 002 | Chậm lại một chút | Giới thiệu | P01 — đổi hướng xuất phát và đặt vùng tiếp cận rộng trước lỗ | Giới thiệu: giảm nghiêng rồi nghiêng ngược để hãm; quán tính vẫn tiếp tục khi ngừng thao tác | Vùng quá đà rộng bên cạnh đích, không hốc kẹt hoặc ngõ cụt hẹp | 8 |
| 003 | Chạm khối vuông | Giới thiệu | P02 — một khối lập phương cố định, lối đi rộng hai bên | Giới thiệu: đọc phản ứng va chạm; chọn đi vòng hoặc chạm nhẹ để đổi hướng | Bi bật lại vào sân mở; cả hai bên vật cản đều nối được về đích | 10 |
| 004 | Theo cạnh nghiêng | Thở | P03 — tam giác, đích dễ tiếp cận, không bắt buộc chạm góc nhọn | Luyện, màn nghỉ: để một cạnh hướng bi, dùng ít lần đổi hướng | Các góc liên quan đường chơi có khoảng quay lại rõ; không yêu cầu mắc bi vào đỉnh nhọn | 7 |
| 005 | Một khúc quanh | Giới thiệu | P04 — chữ L với góc trong rộng | Giới thiệu: hãm trước góc và đổi độ nghiêng sang hướng mới | Sân nhỏ ở chỗ rẽ nhận bi đi quá; luôn có thể quay lại nhánh trước | 12 |
| 006 | Đi quanh khoảng trống | Luyện | P06 — vòng, bề rộng đều và vùng thoát rộng | Luyện: đổi hướng nghiêng liên tục theo đường cong | Bi đi quá được vòng tiếp hoặc quay ngược; không có cổng một chiều hay hình phạt | 14 |
| 007 | Qua bên kia | Luyện | P05 — chữ U, đáy chữ U có sân xoay rộng | Luyện: nối hai lần đổi hướng, kiểm soát tốc độ ở đoạn đáy | Hai nhánh đều mở về đáy; trượt ngược chỉ lùi về sân giữa | 15 |
| 008 | Cổ chai | Giới thiệu | P07 — hai khoang, cổ nối nới rộng cho lần học đầu | Giới thiệu: căn bi vào một lối hẹp hơn sân; dùng vận tốc vừa đủ | Hai đầu cổ đều có sân căn lại; không cần lấy đà chính xác để vượt gờ | 17 |
| 009 | Ngôi sao êm | Thở | P08 — sao rút gọn, hốc nông và tuyến ngắn | Luyện, màn nghỉ: rời một hốc rồi chọn hướng về đích; không phải mê cung ghi nhớ | Hốc dễ rút bi ra bằng nghiêng ngược; không thêm vật cản hoặc nhánh cụt sâu | 12 |
| **010 · BOSS** | **Sư tử thức giấc** | **BOSS** | P15 — mặt nạ sư tử, bố trí lại thành 2–3 chặng nhìn rõ | Tổng hợp: phanh, đi vòng vật cản, đổi hướng và căn lối; không có cửa hoặc cơ cấu động mới | Các sân ở trán và má giữ bi khi đi quá; hai phía mõm đều là nghiệm hợp lệ; không mất toàn bộ hành trình vì một va chạm cuối | **22** |

## Chương 2 — Giữ bi để cơ cấu chuyển động

Môi trường khô. Hai nhóm chính là thanh trượt và cầu bản lề. Cùng trọng lực tạo chuyển động khác nhau nhờ ray, bản lề và hốc giữ; mỗi chặng cho thấy rõ một quan hệ nguyên nhân–kết quả.

| Màn | Tên | Vai trò | Nguồn và biến thể cần dựng | Kỹ năng mới hoặc luyện | Hồi phục vật lý cần có | D mục tiêu |
| --- | --- | --- | --- | --- | --- | ---: |
| 011 | Cửa cũng rơi | Giới thiệu | P09 — một thanh trượt, bỏ đường vòng vào hốc, cho thấy trọn hành trình ray | Giới thiệu: thanh trượt cũng chịu trọng lực; nghiêng một hướng để mở lối | Bi nằm trong sân rộng khi thử nghiêng; cửa đóng lại không kẹp bi vào khoang không thể thoát | 16 |
| 012 | Đợi trong hốc | Giới thiệu | P09 — hốc nông, miệng rộng, đặt gần cửa | Giới thiệu: dùng thành hốc giữ bi trong khi thanh trượt đi tiếp | Bi tuột khỏi hốc vẫn ở sân xuất phát; thử lại bằng một đoạn quay ngắn | 18 |
| 013 | Để nó đi trước | Luyện | P09 — biến thể gần đường giải hiện có, rút bớt quãng lăn thừa | Luyện: vào hốc → mở thanh trượt → đổi hướng để qua cửa | Hai phía cửa có vùng chờ; bị cửa đẩy trở lại không rơi xuống một tầng hoặc phải reset | 21 |
| 014 | Qua sân nhỏ | Thở | P10 — chỉ giữ một cửa và một khúc ngoặt sau cửa | Luyện, màn nghỉ: đọc cửa và góc rẽ trong một tuyến ngắn quen thuộc | Sân sau cửa rộng; bi đã qua có thể dừng an toàn, không phải lập tức xử lý cơ cấu khác | 17 |
| 015 | Cầu tự gập | Giới thiệu | P17 — cầu ngắn, hốc giữ rộng, chỗ tiếp nhận dễ quan sát | Giới thiệu: trọng lực quay bản lề; cầu chỉ được giữ sau tiếp xúc thật với chốt | Sàn thu hồi dưới khoảng trống dẫn về sân đầu; cầu đã vào chốt vẫn giữ trạng thái khi bi lùi lại | 22 |
| 016 | Cho cầu ngồi xuống | Luyện | P17 — chuyển vị trí hốc giữ, giữ nguyên hành vi bản lề và chốt | Luyện: bố trí bi an toàn trước khi làm cầu gập, nhìn cầu chạm điểm tựa | Rời hốc quá sớm đưa bi xuống sân thu hồi gần; không phải làm lại phần gập cầu đã hoàn tất | 24 |
| 017 | Tự bắc đường đi | Luyện | P17 — tuyến gần bản đầy đủ, điều chỉnh bề rộng theo playtest | Luyện: giữ → gập → chốt → qua cầu; điều tiết tốc độ khi chuyển mặt đỡ | Mép và sân đón dẫn bi đi quá về vùng thu hồi; không cho bi kẹt dưới cầu hoặc chốt | 26 |
| 018 | Cửa trước, cầu sau | Kết hợp | P09 + P17 — một cửa dẫn tới một sân thao tác cầu | Kết hợp đã học: hai cơ cấu nối tiếp, mỗi lần chỉ tập trung vào một cơ cấu | Sân trung gian tách khỏi vùng quét thanh trượt; chốt cầu giữ công việc đã làm, có đường trở lại khi hụt bước | 29 |
| 019 | Một vòng thư giãn | Thở | P10 rút gọn + hình học P06 — tuyến cong ngắn với một thanh trượt quen thuộc | Luyện, màn nghỉ: đọc nhanh hình học, đi một vòng ngắn; không tăng số cửa hoặc độ hẹp | Có sân chờ hai phía cửa, không chuỗi thao tác giữ chính xác kéo dài | 23 |
| **020 · BOSS** | **Khu vườn cơ khí** | **BOSS** | P10 + P17 — hai cửa mở ngược chiều và một cầu, chia thành ba sân | Tổng hợp: đọc chiều trượt, đỗ bi, thay hướng trọng lực và gập cầu; không ra mắt cam hoặc con lắc | Mỗi cơ cấu có sân trước và sau; giữ tiến độ cầu bằng chốt đã học; rút ngắn hành lang lặp lại của P10 để lỗi cuối không buộc chạy lại một mê cung dài | **34** |

## Chương 3 — Đổi mặt đỡ trong không gian

Môi trường khô. Học đổi cao độ trước, rồi lồng treo có giới hạn bản lề và mạng đường ba chiều. Độ sâu phải dễ đọc; không dùng lớp kính chồng hoặc mất dấu bi để tăng khó.

| Màn | Tên | Vai trò | Nguồn và biến thể cần dựng | Kỹ năng mới hoặc luyện | Hồi phục vật lý cần có | D mục tiêu |
| --- | --- | --- | --- | --- | --- | ---: |
| 021 | Xuống một bậc | Giới thiệu | P11 — hai sàn ngắn, một khoảng chuyển rộng và nhìn thấy sân đón | Giới thiệu: bi rời một mặt đỡ và rơi xuống mặt dưới; lỗ chuyển khác lỗ thoát | Sân dưới hứng toàn bộ vùng rơi hợp lý; không đòi hỏi xoay để đón bi đang bay | 25 |
| 022 | Hai mặt của chiếc hộp | Luyện | P11 — hai tầng với hai hướng đi ngắn, bỏ tầng thứ ba | Luyện: nhận biết bi đang ở độ cao nào, đổi hướng sau khi xuống tầng | Hai tầng có vùng dừng rộng trước và sau chỗ chuyển; không có lỗ rơi sai đưa về đầu màn | 27 |
| 023 | Lồng muốn đứng yên | Giới thiệu | P19 — lồng lớn, lối ra rộng và sân ngoài gần miệng | Giới thiệu: lồng treo giữ hướng khác vỏ và chỉ nghiêng theo vỏ khi chạm giới hạn bản lề; thử một trục trong sân mở | Bi được giữ trong lồng khi thử nghiêng; ra khỏi miệng có sân ngoài hứng rộng | 26 |
| 024 | Chạm tới giới hạn | Thở | P19 — lồng bắt đầu gần tư thế giới hạn đã học ở 23, đường từ miệng tới lỗ ngắn và rộng | Luyện, màn nghỉ: lặp lại một lần chạm chặn rồi ra sân, không thêm hành động hoặc yêu cầu ghi nhớ | Dừng hoặc xoay ngược vẫn giữ bi trong lồng; không cần tạo xung xoay hoặc căn một cửa sổ thời gian | 22 |
| 025 | Rời chiếc lồng | Kết hợp | P19 — chuyển sang sân ngoài rồi đi thêm một khúc ngoặt | Kết hợp đã học: đổi mặt đỡ từ lồng động sang vỏ cố định, sau đó phanh trên sân đón | Sân đón có thành chắn đủ cao và vùng giảm tốc; rơi lệch vẫn tới đường có thể đưa về lỗ | 32 |
| 026 | Ba hướng, một đường | Giới thiệu | P12 — mạng rút gọn khoảng ba khúc đổi hướng, không nhánh cụt | Giới thiệu hình học mạng 3D: xoay để một vách trở thành mặt đỡ tiếp theo | Mỗi khúc rẽ có khoang nối đủ để dừng và quay lại; không rơi khỏi mạng vào khoảng kín | 29 |
| 027 | Đường qua chiều sâu | Luyện | P12 — khoảng sáu khúc đổi hướng, chỉ một đoạn chiều sâu nổi bật mỗi góc nhìn | Luyện: theo dấu tuyến qua ba trục, giảm tốc trước điểm đổi mặt đỡ | Các đoạn đều đi ngược được; ngã nối có vùng giữ, không yêu cầu nhớ đường quá dài | 34 |
| 028 | Đổi mặt để đi tiếp | Luyện | P12 — khoảng tám khúc đổi hướng và một nhánh phụ ngắn, nhìn rõ điểm kết thúc | Luyện: chọn nhánh dựa trên hình học; nhận biết và tự quay lại khi chọn sai | Nhánh phụ có chỗ xoay và đường rút ngắn; không giấu đầu cụt sau nhiều lớp kính | 38 |
| 029 | Qua một nhịp kính | Thở | P19 + P12 — một lần rời lồng, sau đó tuyến ba chiều rất ngắn | Luyện, màn nghỉ: nối hai hành vi quen thuộc mà không thêm giới hạn mới | Sân sau miệng lồng rộng, mạng chỉ vài đoạn; nếu đi quá có thể trở lại sân này | 32 |
| **030 · BOSS** | **Chòm sao thủy tinh** | **BOSS** | P12 + P19 — một lồng và mạng khoảng 8–10 khúc đổi hướng trong cầu kính | Tổng hợp: đọc chiều sâu, đổi mặt đỡ và dùng giới hạn bản lề; không thêm cú bắt bi trên không | Chia tuyến thành các vùng có khoang nghỉ; nhánh sai ngắn và quay lại được; không dùng toàn bộ 21 khúc ngoặt của P12 gốc | **43** |

## Chương 4 — Hai viên bi cùng một trọng lực

Môi trường khô. Tăng số bi trước khi thêm phối hợp. Màn 31–32 chỉ cần dạy hai vị trí dưới cùng thao tác, không nhồi cơ quan để đạt D. Bước 33 giới thiệu cánh tay đòn, 34 lặp lại ngắn để nghỉ; chốt và nút được đưa vào sau khi giữ bi đã dễ hiểu.

| Màn | Tên | Vai trò | Nguồn và biến thể cần dựng | Kỹ năng mới hoặc luyện | Hồi phục vật lý cần có | D mục tiêu |
| --- | --- | --- | --- | --- | --- | ---: |
| 031 | Có thêm một người bạn | Giới thiệu | P02 + danh sách hai bi của P16 — sàn mở, bỏ khối cản, lỗ tiếp cận rộng | Giới thiệu duy nhất: điều khiển hai bi cùng lúc và chỉ thắng khi cả hai đã ra; chưa có công tắc hoặc giữ tải | Hai bi luôn có đường tới lỗ; bi thoát trước không khiến bi còn lại mất điều kiện mở đường | 31 |
| 032 | Mỗi bi một làn | Luyện | P16 — hai làn ngắn không khóa nhau, cùng dẫn về sân thoát | Luyện: hai bi ở hai làn rộng; quan sát thành cuối làn tự giữ một bi trong khi bi còn lại tiếp tục lăn | Mỗi làn có túi dừng rộng; một bi lùi lại không kéo bi kia về đầu hoặc gây khóa đường | 34 |
| 033 | Khoảng cách tạo lực | Giới thiệu | P18 — đòn bẩy ngắn, A đã nằm trong vùng dễ giữ, chỉ di chuyển A từ gần tâm ra xa | Giới thiệu duy nhất: khoảng cách tới tâm quay làm thay đổi mô men của hai bi bằng nhau; chưa thêm nút hoặc cửa | Hai máng rộng có thành cuối giữ bi; điểm nâng có chốt đã học, không yêu cầu căn cửa sổ hẹp | 36 |
| 034 | Hai đầu đòn bẩy | Thở | P18 — giữ hành vi của 33, giảm quãng di chuyển và bỏ lối ra hẹp; hai sân thoát nhìn thấy cùng lúc | Luyện, màn nghỉ: đọc lại hai cánh tay đòn đã thấy ở 33, di chuyển ngắn rồi cùng ra sân rộng; không thêm hành động | Hai bi ở các máng kín bên; chưa yêu cầu rút cả hai qua cửa sổ hẹp ngay sau khi nâng | 32 |
| 035 | Đứng xa hơn một chút | Luyện | P18 — di chuyển A ra xa tâm để nâng B, điểm tiếp nhận và chốt nhìn rõ | Luyện: đặt A vào vùng tải và điều chỉnh nghiêng khi đòn bẩy thay đổi hướng | Hốc tải rộng giữ A; chốt tiếp xúc lưu trạng thái nâng, sai thao tác chỉ cần căn lại vị trí A | 38 |
| 036 | Giữ đường cho bạn | Giới thiệu | P16 + chốt P23 — một mặt nút tiếp xúc rõ, hành trình ngắn, chỉ một mối liên hệ cần đọc | Giới thiệu quan hệ giữ/chốt: một bi tới vị trí tiếp xúc để giữ đường cho bi còn lại; phải cho thấy điều gì đang được giữ | Kích hoạt giữ thành công không phụ thuộc bi tiếp tục đè mãi; không cho bi đầu thoát trước khi còn một đường giải cho bi sau | 41 |
| 037 | Cả hai cùng rời đi | Kết hợp | P18 — hai sân nhận rộng, cùng dẫn về một cửa thoát | Kết hợp đã học: nâng, chốt, rút A và B rồi lần lượt đưa tới lỗ | Sân hai phía đủ giữ bi khi nghiêng phục vụ phía còn lại; không có hốc kẹt giữa đầu đòn bẩy và thành hộp | 43 |
| 038 | Đổi vai | Luyện | P16 + phần phối hợp P23 — A giữ vị trí, B tới chốt, sau đó lấy lại A | Luyện: chuỗi phụ thuộc có thể nhìn thấy; giải phóng bi đang làm nhiệm vụ giữ | Có túi giữ B sau thao tác chốt; tiến độ đã chốt được bảo toàn bằng cơ cấu, không buộc làm lại cả chuỗi vì lăn quá sân | 48 |
| 039 | Dạo qua trái tim | Thở | P23 rút gọn — đòn bẩy dễ giữ, lối B rộng, lồng và phễu tiếp nhận đơn giản | Luyện, màn nghỉ: diễn tập thứ tự quen thuộc với hai bi; bỏ các đoạn căn hẹp của boss | Mỗi bước có sân dừng rõ; miệng lồng và sân đón rộng; không yêu cầu thao tác đúng nhịp hoặc xoay bắt bi | 42 |
| **040 · BOSS** | **Trái tim cơ khí** | **BOSS** | P23 — bố trí lại để nhìn rõ đòn bẩy → chốt → lồng → phễu cuối | Tổng hợp: giữ A tạo mô men, đưa B tới chốt, rút A và đưa cả hai qua lồng; không giới thiệu cơ chế mới trong boss | Nới đường phối hợp và túi giữ theo dữ liệu thử; chia chặng bằng các sân an toàn; chốt giữ tiến độ; đoạn rơi cuối có sân đón rộng, bi được phép thoát khác thời điểm | **54** |

## Chương 5 — Cơ khí giữ ký ức

Môi trường khô. Một nấc giữ trước, chu kỳ đẩy–hồi sau, rồi trạng thái trung gian và thứ tự tiếp cận. Các biến thể thay đường nối, vị trí mặt tác động hoặc phụ thuộc cơ cấu; không chỉ tăng số lần đẩy giống nhau.

| Màn | Tên | Vai trò | Nguồn và biến thể cần dựng | Kỹ năng mới hoặc luyện | Hồi phục vật lý cần có | D mục tiêu |
| --- | --- | --- | --- | --- | --- | ---: |
| 041 | Chiếc hộp nhớ một nhịp | Giới thiệu | P22 — một rack, một nấc giữ và một cửa cam ở ngay sau mặt đẩy | Giới thiệu: lực tiếp xúc đẩy cam tới nấc mới; cam giữ vị trí sau khi bi rời rack | Một sân mở trước rack; lùi bi và thử lại không xóa nấc đã đạt | 43 |
| 042 | Buông ra rồi đẩy tiếp | Giới thiệu | P22 — hai nấc, khoảng hồi rack nhìn trọn, cửa chỉ thẳng sau lần đẩy thứ hai | Giới thiệu một bước: rack phải hồi đủ trước lần đẩy kế; không thể giữ bi đè mãi để đi tiếp | Vùng lùi bi rộng và có vạch vị trí rack; sai nhịp chỉ dừng tiến độ, không mất nấc | 45 |
| 043 | Rời đi rồi quay lại | Luyện | P22 + P04 — sân rack ở nhánh chữ L, lối trở lại ngắn dẫn tới cửa cam ở nhánh kia | Luyện: rời cơ cấu để đọc lối ra rồi quay về bổ sung một nấc; nhớ trạng thái, không nhớ đường dài | Hai nhánh đều quay về sân chung; thêm nấc chỉ có thể mở rộng đường đi, không khóa bi ở nhánh cũ | 48 |
| 044 | Một tiếng tách êm | Thở | P22 — một nấc giữ, mặt đẩy lớn, lỗ thoát nhìn thấy ngay qua cam | Luyện, màn nghỉ: một thao tác đẩy–rời quen thuộc, không thêm luật hoặc đường vòng | Sân trước và sau cam rộng; bi trượt khỏi mặt đẩy vẫn nằm ngay cạnh vị trí thử lại | 42 |
| 045 | Đúng cửa, đúng nấc | Giới thiệu | P22 — cam có hai lối trung gian nối tiếp; chỉ tiếp cận được mặt đẩy kế sau khi qua lối thứ nhất | Giới thiệu: một trạng thái trung gian là một phần đường đi, không chỉ là tiến độ tới cửa cuối | Hình học chặn việc bỏ qua nấc cần đi; các nấc đã qua giữ lối quay lại, không tạo khóa mềm vì đẩy quá | 49 |
| 046 | Đẩy từ phía bên kia | Luyện | P22 + P07 — hai khoang dẫn tới hai phía tiếp cận của cùng cụm cơ cấu; chuỗi mặt đẩy được bố trí theo tuyến | Luyện: tìm mặt tác động tiếp theo sau khi cam đã đổi đường nối; cùng tiếp xúc và chu kỳ hồi đã học | Mỗi khoang có sân xoay rộng; rack không nằm ở đáy ngõ cụt, bi luôn có thể rời mặt đẩy để hồi lò xo | 51 |
| 047 | Cầu qua ký ức | Kết hợp | P17 + P22 — cam mở đường tới sân cầu, cầu đã chốt đưa bi trở lại phía sau cam | Kết hợp: lưu trạng thái cam trước, sau đó gập cầu để tạo một tuyến vòng về đích | Chốt cam và cầu giữ phần đã làm; bi hụt cầu về sân thu hồi gần, không trở lại rack đầu | 53 |
| 048 | Hai điều cần nhớ | Kết hợp | P22 — hai cam nối tiếp ở hai sân riêng, cam đầu cần một nấc, cam sau dùng một trạng thái trung gian | Luyện: đọc hai trạng thái cơ khí độc lập; chỉ một cam ở vị trí thao tác tại mỗi chặng | Sân giữa hai cam là vùng dừng; cả hai giữ tiến độ, mọi trạng thái trung gian có đường quay về sân tương ứng | 55 |
| 049 | Lối mở đã chờ | Thở | P22 + P06 — một nấc rồi một cung vòng rộng về lỗ, không có cơ cấu thứ hai | Luyện, màn nghỉ: nhìn cam giữ đường mở và lăn một đoạn ngắn đã quen | Vòng cho phép đi quá rồi quay lại; không thêm nhánh cụt hoặc số lần đẩy | 49 |
| **050 · BOSS** | **Đóa sen ký ức** | **BOSS** | P22 + P17 — ba sân quanh tâm sen: cam mở lối trung gian, cầu nối sang sân kế, cam cuối nối về tâm | Tổng hợp kỹ năng đã học: đọc nấc, rời rack, dùng đường trung gian và cầu; chuyển động trang trí không thêm luật | Mỗi sân hứng lỗi tại chỗ; chốt giữ các phần đã mở; không phải lặp toàn bộ ba sân khi trượt ở cửa cuối | **60** |

## Chương 6 — Nhịp chuyển động và quỹ đạo

Môi trường khô. Con lắc được dạy bằng tư thế mở ổn định trước khi kết hợp hai trục. Cú bay đầu có sân đón thẳng và rộng, sau đó mới yêu cầu xoay sân nhận. Hồi phục cú hụt phải được dựng lại ngắn hơn P21 hiện tại.

| Màn | Tên | Vai trò | Nguồn và biến thể cần dựng | Kỹ năng mới hoặc luyện | Hồi phục vật lý cần có | D mục tiêu |
| --- | --- | --- | --- | --- | --- | ---: |
| 051 | Cánh cửa biết đung đưa | Giới thiệu | P20 — một con lắc, bi ở túi chờ rộng; giữ tư thế mở ổn định là lời giải chính | Giới thiệu: thay độ nghiêng làm con lắc rời cửa; không đòi canh nhịp ngay lần đầu | Hai phía cửa có sàn hứng; bị chặn đẩy bi về sân chờ, không rơi mất tiến độ | 46 |
| 052 | Mở sang bên, đi về trước | Luyện | P20 — cửa và vùng chờ lệch nhau để tách hướng mở con lắc khỏi hướng lăn qua | Luyện: giữ góc mở rồi thêm độ nghiêng theo trục còn lại; cửa sổ đi qua rộng | Vách dẫn không kẹp bi vào vùng quét; có thể ngừng cấp độ nghiêng tiến và trở lại túi chờ | 49 |
| 053 | Chờ ở mép an toàn | Luyện | P20 + P04 — lối chữ L tới cửa, túi chờ nằm cạnh mép hành trình con lắc | Luyện: tiếp cận, dừng, quan sát trạng thái rồi đi; hãm con lắc cũng là nghiệm hợp lệ | Túi chờ ở ngoài vùng quét bob; hụt lượt qua vẫn ở cạnh cửa, không chạy lại hành lang dài | 52 |
| 054 | Một cánh cửa êm | Thở | P20 — tuyến thẳng ngắn, con lắc dễ giữ ở tư thế mở, sân thoát lớn | Luyện, màn nghỉ: một lần mở ổn định rồi qua cửa; không thêm con lắc hoặc siết thời điểm | Sân hai phía cửa rộng; bi bị bob đẩy chỉ lùi một đoạn ngắn | 46 |
| 055 | Một khoảng không | Giới thiệu | P21 — dốc lấy đà ngắn, sân nhận cố định rất rộng và nằm thẳng dưới đường bay | Giới thiệu: bi giữ quỹ đạo trong thế giới khi rời dốc; lần này chưa cần xoay để bắt | Một tầng hứng ngay dưới sân nhận nối lại túi phóng bằng vòng nghiêng ngắn; không dùng đường cứu trên toàn bộ nắp | 51 |
| 056 | Đưa sân đón tới | Giới thiệu | P21 — sân nhận lệch nhẹ sang bên, dốc dài vừa đủ để thấy và bắt đầu xoay trước khi bi rời mép | Giới thiệu một hành động: xoay hộp để đưa sân nhận vào dưới quỹ đạo bay; không tác động lực trực tiếp lên bi | Hụt sân nhận rơi xuống tầng hứng cục bộ, trở lại túi phóng bằng một đoạn chuyển mặt đỡ đã học | 54 |
| 057 | Chạm đất rồi đổi hướng | Luyện | P21 + P04 — sân nhận nối góc L có vùng phanh trước đoạn rẽ | Luyện: đón bi, giảm tốc sau chạm đất rồi chuyển hướng; không yêu cầu hai cú bắt liên tiếp | Thành sân nhận hứng bi đáp nhanh; trượt góc về cùng sân, không rơi ngược xuống dốc đầu | 57 |
| 058 | Cửa mở, đường bay | Kết hợp | P20 + P21 — cửa dẫn tới túi chuẩn bị tách biệt trước dốc phóng | Kết hợp: hoàn tất qua con lắc, dừng ở túi rồi chuẩn bị cú đón; không phải vừa canh cửa vừa bắt bi | Sân sau con lắc giữ chặng đầu; hụt cú bay chỉ quay lại túi phóng, không qua con lắc lần nữa | 60 |
| 059 | Đáp xuống thật nhẹ | Thở | P21 — một cú bay ngắn sang sân rộng, lỗ gần vị trí đáp, không có cửa | Luyện, màn nghỉ: một cú đón dễ đọc bằng hành động đã học; giảm thời gian hồi phục | Sàn hứng cạnh sân nhận đưa bi trở lại điểm chuẩn bị bằng một vòng ngắn | 54 |
| **060 · BOSS** | **Vũ điệu quỹ đạo** | **BOSS** | P20 + P21 + P19 — cổng con lắc, sân dừng, một cú đón và lồng dẫn về đích | Tổng hợp không luật mới: giữ mở, chuẩn bị, đón rồi đổi mặt đỡ; không buộc xử lý ba cơ cấu cùng lúc | Từng chặng có sân thu hồi; lồng không đẩy bi trở lại cú bay; sau cú đón thành công có sân giữ tiến độ | **65** |

## Chương 7 — Chuyển động dưới nước

61 là đối chứng KHÔ; 62 dùng đúng hình học đó để giới thiệu NƯỚC. Từ 62–70, mỗi hộp đầy nước đồng nhất, một bi; không đổi chất lỏng trong cùng màn, không thêm dòng chảy hoặc độ nhớt biến thiên. Trình tự thay topology và mặt đỡ, không chỉ kéo dài thời gian chìm.

| Màn | Tên | Vai trò | Nguồn và biến thể cần dựng | Kỹ năng mới hoặc luyện | Hồi phục vật lý cần có | D mục tiêu |
| --- | --- | --- | --- | --- | --- | ---: |
| 061 | Nhớ cảm giác khô | Đối chứng | P02 — bản khô có đúng kích thước, spawn, khối cản và đích sẽ dùng ở 62 | Đối chứng: nhớ gia tốc, va chạm và quãng đi dưới cùng thao tác; chưa có chất lỏng | Một sân mở và hai phía đi vòng khối cản; đường ngắn để việc đối chiếu không thành bài khó | 43 |
| 062 | Lần đầu dưới nước | Giới thiệu | P13 — giữ nguyên toàn bộ hình học của 61, chỉ đổi sang hộp đầy nước với thông số cố định | Giới thiệu duy nhất: lực nổi và sức cản làm thay đổi phản ứng của bi; thép vẫn chìm; không đổi môi trường giữa màn | Sân rộng như đối chứng; đi quá hoặc đổi hướng muộn chỉ cần nghiêng lại, không có cơ cấu thời điểm | 45 |
| 063 | Qua khoảng lặng | Luyện | P13 + P04 — lối L với sân quan sát trước góc, không thêm vật chuyển động | Luyện: chọn thời điểm đổi độ nghiêng theo phản ứng dưới nước, dừng rồi rẽ thay vì tăng lực điều khiển | Vùng nhận sau góc rộng; bi lùi vẫn về sân ngay trước góc | 49 |
| 064 | Một cung nước êm | Thở | P13 + P06 — một cung vòng rộng, đường ngắn và lỗ thấy rõ | Luyện, màn nghỉ: đi theo thành cong dưới nước; không thêm hành động hoặc đổi thông số chất lỏng | Có khoảng vượt đích để quay lại; không có nhánh kín hoặc hành lang dài | 43 |
| 065 | Hai cách vòng qua | Luyện | P13 + P02 — hai vật cản cố định tạo đường trong ngắn và đường ngoài rộng cùng tới một sân | Luyện: chọn giữa căn lối và đi vòng theo cảm giác nước đã biết; hai nghiệm đều hợp lệ | Cả hai đường quay về cùng sân; chọn nhánh không khóa bi hoặc gây mất tiến độ | 52 |
| 066 | Đổi mặt đỡ dưới nước | Luyện | P13 + P11 — hai cao độ nối bằng khoảng chuyển rộng, cả hộp cùng đầy nước | Luyện kỹ năng đổi mặt đỡ đã học trong môi trường nước; không có dòng chảy hoặc mặt nước mới | Sân dưới hứng toàn bộ khoảng chuyển; không cần căn thời điểm rơi hoặc bắt bi | 55 |
| 067 | Cùng một lối, khác hướng | Kết hợp | P13 + P05 — tuyến U có hai sân ở cao độ khác nhau, nối bằng đoạn chuyển mặt đỡ đã học | Kết hợp: đổi hướng trong U rồi chuyển xuống sân nhận; hành vi nước không thay đổi giữa chặng | Các sân có vùng dừng rộng; lùi về sân gần nhất, không tụt qua nhiều tầng | 59 |
| 068 | Vườn nước hai tầng | Kết hợp | P13 + P11 — hai tầng ngắn, đường tầng dưới nằm lệch khỏi hình chiếu tầng trên | Luyện: theo dõi vị trí trong chiều sâu dưới nước và quyết định hướng tiếp theo sau mỗi lần chuyển | Tầng dưới có sân nhận và đường quay lại cửa đi; không bố trí lỗ rơi sai dẫn về đầu | 63 |
| 069 | Lăn qua ánh nước | Thở | P13 + P01 — một lòng tròn rộng, một khối cản thấp quen thuộc, tuyến ngắn | Luyện, màn nghỉ: cảm nhận chuyển động nước với ít quyết định; không thêm luật, dòng hoặc nhiều bi | Mọi vùng lòng hộp đều nối về lỗ; va chạm chỉ làm chậm hoặc đổi hướng | 56 |
| **070 · BOSS** | **Thủy cung ánh trăng** | **BOSS** | P13 + P11 + P15 — một bi, ba sân nước nối bằng địa hình cố định và đường quanh phù điêu | Tổng hợp điều hướng, chọn nhánh và đổi mặt đỡ đã luyện dưới nước; không thêm cửa động hoặc luồng nước | Mỗi vùng có sân hứng riêng; ánh sáng và VFX giữ bi rõ; không phải chờ bi trôi một quãng dài để thử lại | **68** |

## Chương 8 — Đọc mặt cao trong thủy ngân

71 là đối chứng NƯỚC; 72 giữ đúng hình học để giới thiệu THỦY NGÂN. Từ 72–80, mỗi màn có một bi và môi trường thủy ngân đồng nhất. Mặt quan sát rõ là yêu cầu bắt buộc; hình ảnh nhìn xuyên là hỗ trợ đọc, không phải thuộc tính quang học thật.

| Màn | Tên | Vai trò | Nguồn và biến thể cần dựng | Kỹ năng mới hoặc luyện | Hồi phục vật lý cần có | D mục tiêu |
| --- | --- | --- | --- | --- | --- | ---: |
| 071 | Lại một hộp nước | Đối chứng | P13 — bản nước đúng kích thước, spawn, khối cản và đích sẽ dùng ở 72 | Đối chứng: nhắc thép chìm và mặt đỡ phía thấp trong nước; chưa có thủy ngân | Một sân rộng và đường vòng khối cản dễ; thời gian ngắn để giữ tác dụng đối chiếu | 45 |
| 072 | Bi tìm lên phía trên | Giới thiệu | P14 — giữ nguyên hình học của 71, đổi toàn hộp sang thủy ngân với thông số cố định | Giới thiệu duy nhất: bi thép nổi về mặt cao; xoay để dẫn tới lỗ; hỗ trợ thoát cuối vẫn giống các màn khác | Nắp trong rộng trở thành mặt đỡ; không có bẫy ở phần trên hoặc yêu cầu xoay nhanh để thoát | 47 |
| 073 | Theo mặt ở phía cao | Luyện | P14 + P04 — đường L dưới mặt nắp, sân chờ rộng ở góc | Luyện: nhận biết mặt cao sau khi xoay rồi đổi hướng nổi; không đảo điều khiển bằng code | Góc có sân để ổn định lại; bi đi sai về mặt cao gần nhất, không vào khoang bịt kín | 51 |
| 074 | Một vầng bạc yên | Thở | P14 + P06 — cung vòng rộng dưới nắp, tuyến ngắn và đích nhìn thấy rõ | Luyện, màn nghỉ: một cung nổi đã quen; không thay mật độ hoặc thêm hành động | Vòng cho phép đi quá rồi nghiêng lại; không có khe trên trần chỉ vừa sát bi | 45 |
| 075 | Vòng qua vật cản phía trên | Luyện | P14 + P02 — vật cản gắn tới mặt nắp, hai lối đi vòng cùng về sân đích | Luyện: dùng mặt đỡ phía cao để đọc vật cản và chọn đường, giữ nguyên lực nổi đã học | Cả hai lối đều thông; góc giữa vật cản và nắp không giữ bi trong khe không thể rút | 55 |
| 076 | Lên sân kế tiếp | Luyện | P14 + P11 — hai mặt đỡ phía cao nối qua một khoảng chuyển lớn, cùng môi trường thủy ngân | Luyện đổi cao độ bằng hành vi nổi; chỉ một khoảng chuyển, không thêm cửa hoặc dòng chất lỏng | Có hốc nhận rộng ở mặt cao kế; quay ngược đưa bi về mặt trước mà không cần reset | 59 |
| 077 | Qua cổ bạc | Kết hợp | P14 + P07 — hai khoang có mặt cao lệch nhau, cổ nối đủ rộng và có sân căn hai đầu | Kết hợp căn lối và đổi mặt đỡ phía cao; không yêu cầu vận tốc tối thiểu hay cú phóng | Hai đầu cổ có túi ổn định; lạc hướng vẫn ở khoang hiện tại, dễ căn lại | 63 |
| 078 | Mê cung hướng lên | Kết hợp | P14 + P11 — hai đoạn mê cung ngắn nối theo chiều nổi, đường tầng kế hiện rõ khi tiếp cận | Luyện: chọn hướng sau mỗi lần lên mặt đỡ khác; không thêm nhánh mù hoặc đổi chất lỏng | Mỗi chỗ chuyển có sân nhận, cho phép trở lại chặng gần; tránh buộc nổi xuyên toàn bộ hộp để thử lại | 67 |
| 079 | Tĩnh trên mặt bạc | Thở | P14 + P01 — một khoang tròn, lỗ tiếp cận rộng và đường từ spawn ngắn | Luyện, màn nghỉ: đọc mặt cao và đưa bi tới vùng hỗ trợ thoát; không thêm hành động cần học | Không khoang phụ hoặc vật cản động; mọi vị trí hợp lệ đều dẫn được trở lại đích | 60 |
| **080 · BOSS** | **Mặt trăng bạc** | **BOSS** | P14 + P07 + P11 + P15 — các khoang phía cao nối quanh một phù điêu mặt trăng, một bi | Tổng hợp nổi, căn cổ nối, vòng vật cản và đổi mặt đỡ; cả màn là thủy ngân, không chuyển môi trường | Sân trên mỗi khoang giữ tiến độ cục bộ; đường nổi sai ngắn và quay lại được; mặt quan sát phải thể hiện rõ dù vật liệu bạc | **72** |

## Chương 9 — Không gian và hình dạng khác thường

Toàn chương KHÔ; 81 làm lượt đối chiếu ngắn sau chất lỏng. Dùng vật thể lạ, liên kết đường vòng, cao độ và trục lồng đã biết để tạo quyết định khác. Không thay topology giữa lúc chơi bằng phép biến hình, không thêm lực hoặc điều khiển.

| Màn | Tên | Vai trò | Nguồn và biến thể cần dựng | Kỹ năng mới hoặc luyện | Hồi phục vật lý cần có | D mục tiêu |
| --- | --- | --- | --- | --- | --- | ---: |
| 081 | Trở lại trọng lượng quen | Đối chứng | P02 trong một vỏ phù điêu P15 — môi trường khô, một vật cản và tuyến rất ngắn | Đối chứng trở lại khô: bi lại nằm về mặt thấp; không giới thiệu vật lý mới sau hai chương chất lỏng | Sân mở, lỗ dễ tiếp cận; bố cục không dùng quán tính bất ngờ để phạt thói quen vừa học | 50 |
| 082 | Chữ U qua góc hộp | Luyện | P05 + P11 — hai nhánh U đặt trên hai mặt vuông góc, nối bằng một sân góc rộng | Luyện: nhận ra đường chữ U dù nó đổi mặt đỡ; dùng cùng thao tác xoay đã học | Sân góc nhận bi từ cả hai nhánh; mọi đoạn có thể đi ngược về sân này | 54 |
| 083 | Vòng có lối xuyên tâm | Luyện | P06 + P11 — một vòng ngoài và đường xuyên tâm ở cao độ khác, nối tại hai sân | Luyện: chọn tuyến vòng hoặc tuyến chuyển mặt theo khả năng quan sát và kiểm soát; hai đường cùng đích | Các lối nối đều có sân nhận; chọn nhánh không khóa đường kia hoặc gây vòng lặp bắt buộc | 57 |
| 084 | Ghé qua chiếc mặt nạ | Thở | P15 + P04 — mặt nạ nhỏ có một đường L rộng quanh phù điêu | Luyện, màn nghỉ: một vật thể lạ về hình ảnh nhưng đường đi quen thuộc, không thêm hành động | Hai sân lớn trước và sau góc; không hốc bờm sâu hoặc nhánh cụt để ghi nhớ | 51 |
| 085 | Hai khoang lệch mặt | Kết hợp | P07 + P11 — hai khoang hướng vuông góc, cổ nối có sân chuyển đỡ ở giữa | Kết hợp căn cổ và đổi mặt đỡ; tuyến yêu cầu đổi hướng tại sân giữa, không chỉ là bản đổi màu | Sân giữa giữ bi khi đổi hướng; trượt khỏi cổ trở về khoang gần, không rơi qua nhiều chặng | 61 |
| 086 | Qua hai chiếc lồng | Kết hợp | P19 + P11 — hai lồng có trục treo khác hướng nhưng hoạt động nối tiếp, sân trung gian rộng | Luyện: áp dụng cùng giới hạn bản lề theo hai hướng quan sát; chỉ thao tác một lồng tại mỗi chặng | Sân giữa tách khỏi cả hai vùng quét; lỗi ở lồng sau không đưa bi về lồng đầu | 64 |
| 087 | Chọn đường trong cầu | Luyện | P12 — một mạng nhỏ có hai tuyến: ít góc nhưng cần căn hơn, hoặc nhiều góc rộng hơn | Luyện: chọn đánh đổi thao tác và độ dài theo kỹ năng cá nhân; không có tuyến giả hay cửa bí mật | Hai nhánh có điểm đổi ý và quay về nút chung; không nhánh nào trở thành bẫy sau khi đã chọn | 67 |
| 088 | Mê cung có đường quay lại | Kết hợp | P11 + P12 — ba vùng không gian ngắn có một đường nối vòng, mỗi vùng đọc được từ sân vào | Luyện: hiểu liên kết không gian và chọn lối quay về sân gần nhất sau khi đi quá | Đường vòng được nhìn thấy và có collider thật; không ép chạy lại toàn mạng khi sai một ngã | 70 |
| 089 | Nghỉ ở một ngôi sao | Thở | P08 + P19 — một lồng mở ra sân sao nông với lỗ gần tâm | Luyện, màn nghỉ: một lần rời lồng rồi một đoạn ngắn trên sân; không thêm nhánh hoặc giới hạn mới | Cánh sao nông cho phép rút bi dễ; sân hứng bao quanh miệng lồng | 63 |
| **090 · BOSS** | **Mê cung vạn hoa** | **BOSS** | P04/P06/P08/P11/P12/P15/P19 — ba vùng hình học rõ dấu hướng: vòng, góc chuyển đỡ và lồng trong vỏ đối xứng | Tổng hợp đọc topology, chọn tuyến và đổi mặt đỡ; đối xứng chỉ là bố cục, không có phép biến hình hay lực mới | Mỗi vùng có sân nghỉ và đường quay lại ngắn; dấu hình và màu cố định tránh gây lạc chỉ vì hiệu ứng vạn hoa | **75** |

## Chương 10 — Tất cả cùng vận hành

Toàn chương KHÔ. Hai bi phối hợp với cam, đòn bẩy, lồng và cú bay đã học. Tăng yêu cầu chọn thứ tự và giữ trạng thái; sân giữa các chặng tách các nhiệm vụ. Boss 100 là đỉnh của tuyến chính, không phải một bài kiểm tra luật mới.

| Màn | Tên | Vai trò | Nguồn và biến thể cần dựng | Kỹ năng mới hoặc luyện | Hồi phục vật lý cần có | D mục tiêu |
| --- | --- | --- | --- | --- | --- | ---: |
| 091 | Hai bi, một ký ức | Kết hợp | P16 + P22 — hai bi ở sân riêng; cam một nấc mở một đường chung, không cần giữ tải liên tục | Luyện kết hợp đã học: một bi tác động để tạo đường cho cả hai; không quy định cứng bi màu nào phải làm việc | Cam giữ trạng thái khi bi rời đi; bi đầu thoát không khiến bi sau bị khóa; mỗi sân có túi giữ rộng | 57 |
| 092 | Nâng đường tới rack | Kết hợp | P18 + P22 — đòn bẩy đưa một bi tới sân rack, cam mở đường rút bi còn lại | Luyện: giải phụ thuộc nâng → giữ → tác động cam → lấy lại bi, với từng cơ cấu đã quen | Chốt đòn bẩy giữ phần nâng; sân rack đủ giữ bi khi phục vụ phía kia; mọi trạng thái cam cho phép rút lại | 60 |
| 093 | Nhớ đường trước khi qua lồng | Kết hợp | P19 + P22 — một cam đổi nối giữa hai sân; lồng nằm sau sân đã mở, hai bi đi lần lượt | Luyện: hoàn tất trạng thái cơ khí trước khi chuyển bi qua mặt đỡ động; không vận hành rack trong lúc bắt bi | Cam giữ đường mở, sân trước lồng chứa được hai bi; lỗi ở lồng có đường thu hồi về sân này | 63 |
| 094 | Cùng đi qua sân rộng | Thở | P16 + P22 — hai bi, một nấc cam và sân chung ngắn dẫn thẳng tới lỗ | Luyện, màn nghỉ: một hành động tạo đường rồi đưa hai bi ra; không thêm cơ cấu hoặc hành động mới | Túi giữ hai phía rack tránh va chạm cản nhau; bi nào ra trước cũng không thay đường của bi còn lại | 57 |
| 095 | Hai lối cùng mở | Kết hợp | P16 + P22 — hai sân với hai cam dễ thao tác; người chơi chọn thứ tự mở các lối dẫn về sân chung | Luyện: chọn thứ tự công việc theo vị trí hai bi, dùng giữ trạng thái thay vì phải điều khiển đồng thời chính xác | Mỗi cam chỉ mở thêm lối an toàn; không có trạng thái mở một cửa rồi khóa vĩnh viễn cửa kia | 66 |
| 096 | Một bi đợi, một bi bay | Kết hợp | P16 + P21 — hai túi chuẩn bị, một đường phóng dùng lần lượt, sân nhận rộng đủ giữ bi đã đáp | Luyện: đỗ một bi bằng thành thật rồi thực hiện cú đón với bi kia; vai có thể đổi | Hụt cú đón về túi chuẩn bị gần; sân nhận giữ bi đã qua khỏi các độ nghiêng cần dùng cho lượt tiếp theo | 69 |
| 097 | Đổi vai ở chiếc lồng | Kết hợp | P18 + P19 + P23 — A nâng, B giữ chốt, cả hai tới sân lồng; thứ tự ra lồng do vị trí thực quyết định | Luyện: giải phóng bi giữ tải rồi chuyển từng bi qua lồng; không thêm nút điều khiển riêng hoặc khóa theo danh tính | Chốt giữ phần phối hợp, sân lồng đủ chỗ hai bi; một bi ra trước không lấy mất điểm tựa bắt buộc của bi sau | 72 |
| 098 | Ba sân chung một hộp | Kết hợp | P18/P19/P21/P22/P23 — cam mở một nhánh tới cầu, sân giữa cho chọn chuyển qua lồng hoặc đường bay đã học | Luyện: chọn một tuyến phù hợp sau chuỗi mở đường; không bắt buộc làm mọi cơ cấu hoặc xử lý tất cả cùng lúc | Hai tuyến đều dẫn về cùng sân cuối; chốt giữ tiến độ, lỗi chỉ quay về sân đầu tuyến đang chọn | 75 |
| 099 | Khoảnh khắc trước vũ trụ | Thở | P18 + P23 — hai bi trên đòn bẩy dễ giữ, chốt rõ, hai sân thoát ngắn, bỏ lồng và cú bay | Luyện, màn nghỉ: hoàn thành một phối hợp đẹp và dễ quan sát; không thêm hành động hoặc độ chính xác mới | Cửa ra rộng, sân chung lớn; lấy lại bi giữ tải bằng một độ nghiêng quen thuộc | 68 |
| **100 · BOSS** | **Vũ trụ trong lòng bàn tay** | **BOSS** | P18/P19/P21/P22/P23 — hai bi, ba chặng khô: phối hợp đòn bẩy, lưu đường bằng cam, chuyển lồng rồi đón về sân cuối | Tổng hợp hoàn toàn mechanic đã học: chọn thứ tự hai bi, giữ tiến độ và chuyển mặt đỡ; không thêm lực, chất lỏng hoặc điều khiển mới | Từng chặng có sân chứa hai bi và đường thu hồi riêng; chốt giữ phần đã xong; cú đón cuối không xóa chuỗi cơ khí; cả hai phải thoát thật | **80** |

## Cách dùng thư viện prototype

| Nội dung hiện có | Vai trò trong bản 100 màn |
| --- | --- |
| P01–P08, P15 | Nền hình học và cảm giác khô ở chương 1, tiếp tục tạo topology và vật thể khác thường ở chương 9. Hình vỏ khác chỉ có ý nghĩa gameplay khi đường đi hoặc quyết định thực sự khác. |
| P09/P10/P17 | Nền thanh trượt, hốc giữ, cầu và chuỗi sân của chương 2. P10 nguyên bản không được mặc định là boss chỉ vì ID 10. |
| P11/P12/P19 | Đổi mặt đỡ và lồng ở chương 3; phát triển đường vòng, liên kết ba chiều ở chương 9. P12 nguyên bản có 25 đoạn, 21 khúc đổi hướng và hai nhánh cụt, nên giữ riêng trong Lab/Challenge, không ép toàn bộ tuyến đó vào màn giới thiệu hoặc boss 30. |
| P16/P18/P23 | Hai bi, mô men đòn bẩy và chốt phối hợp ở chương 4; phối hợp cùng mechanic đã học ở chương 10. |
| P22 | Nền chương 5: giữ nấc, hồi rack, đường trung gian và thứ tự thao tác. Không chỉ sao chép cùng một cơ cấu rồi tăng số lần đẩy. |
| P20/P21 | Nền chương 6, sau đó kết hợp có chọn lọc ở chương 10. P21 hiện cần đường cứu dài qua nắp; variant campaign chỉ được chấp nhận khi có hồi phục cục bộ mới đã kiểm chứng. |
| P13/P14 | Hai chương riêng 7/8, có cặp đối chứng cùng hình học tại 61/62 và 71/72. Không trộn nước với thủy ngân hoặc đổi môi trường giữa chừng một màn. |

## Điều kiện trước khi chốt thứ tự và số điểm

1. Dựng bản đơn giản nhất của mỗi bước giới thiệu, rồi cho người chưa xem lời giải thử. Nếu hiểu sai nguyên nhân, sửa cách nhìn, hình học hoặc thứ tự học trước khi tăng khó.
2. Tách thời gian suy luận, thao tác, mất dấu bi và hồi phục. Hai màn cùng thời gian hoàn thành có thể tạo cảm giác khác nhau; chờ lâu, khó thấy bi hoặc phải chạy lại đường dài không phải thành công về puzzle.
3. Kiểm chứng cả đường giải và đường hồi phục từ những lỗi dễ xảy ra. Với hai bi, thử thứ tự thoát khác nhau và mọi trạng thái giữ/chốt liên quan; không khóa bi còn lại sau khi viên đầu đã ra.
4. Màn thở x4/x9 không ra mắt hành động mới. Nếu một biến thể hình học vẫn khiến người chơi phải học lại điều khiển hoặc mất phương hướng, coi đó là nội dung mới cần dời khỏi vị trí nghỉ.
5. Fluid chỉ thay đổi giữa màn, được báo trước ở phần chọn màn và nhắc bằng biểu tượng môi trường. 61/62 và 71/72 phải giữ cùng kích thước, spawn, vật cản và đích để phép đối chiếu có ý nghĩa; không đồng thời thay hình học và lực. Hộp chứa đầy một môi trường; chất lỏng được giữ lại bằng luật level, không chảy ra lỗ, không mô phỏng chuyển chất lỏng giữa khoang.
6. Các hộp nước/thủy ngân khác hình chữ nhật cần được kiểm tra mô hình thể tích, bề mặt va chạm và lực môi trường riêng khi authoring; prototype hiện tại không tự chứng minh chúng đã chính xác. VFX chất lỏng hiện chủ yếu phục vụ một bi, nên chương 7/8 giữ một bi và không thêm cơ cấu động chưa kiểm chứng trong chất lỏng.
7. Đánh giá boss bằng hiểu mục tiêu, đọc rõ cơ cấu, hồi phục và cảm giác hoàn thành, không chỉ số lần reset. Hình ảnh bất ngờ không được che đường, đổi va chạm hoặc buộc một cách giải ẩn. Test tự động giải được chỉ chứng minh tính khả thi, chưa chứng minh độ khó hoặc viral.

Lưu ý cơ khí: P16 hiện dùng cửa có motor với nguồn năng lượng hỗ trợ, còn bản lề/chốt của P17/P18/P23 dùng cơ cấu thụ động hoặc ràng buộc chốt lý tưởng. Khi authoring biến thể dùng P16, phải chọn và thể hiện rõ mô hình: giữ motor thì cho thấy liên kết và nguồn năng lượng; thay bằng cơ cấu thụ động thì dựng và kiểm chứng cơ cấu mới. Không gọi mọi biến thể là thụ động khi thiết kế chưa xác định.

## Dữ liệu dùng cho công cụ

[CAMPAIGN_100_LEVELS.json](CAMPAIGN_100_LEVELS.json) chứa đúng 100 bản ghi tương ứng ma trận, gồm `index`, `title`, `chapter`, `boss`, `role`, `source_prototypes`, `target_d`, `skill`, `recovery`. `role` dùng các giá trị `intro`, `practice`, `combine`, `rest`, `calibration`, `boss`; `skill` kèm mô tả bố trí để việc tái dùng một prototype không mất thông tin khác biệt của biến thể. Đây là dữ liệu kế hoạch để dựng biểu đồ/biên tập, chưa phải catalog runtime của Unity.
