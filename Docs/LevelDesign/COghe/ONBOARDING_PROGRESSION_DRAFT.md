# COghe — dự thảo đường học 60 màn, 24/09/2026

**Trạng thái: giả thuyết thiết kế; chưa áp vào campaign production.** Pilot riêng chỉ gồm mười hàng đầu. Bản đồ dưới đây giữ đủ 60 ID, không lặp/mất màn; sáu Boss giữ đúng mốc10. Không coi bảng này là kết quả xếp hạng người mới.

Tiên quyết là **vị trí mới** trong bảng. Chuỗi / vai trò là số cụm quyết định ước lượng và số cơ thể đồng thời trong lời giải mẫu; không phải số chạm tối thiểu. Thao tác, quan sát và chi phí làm lại được ghi riêng, không cộng thành điểm. Mức chi phí thấp/vừa/cao chỉ là giả thuyết tương đối để chọn ca quan sát. Tất cả tỷ lệ thành công, thời gian và precision trên điện thoại: **chưa đo**.

Nguồn: từng definition liên kết bên dưới; [thiết kế30](../../COGHE_CAMPAIGN_30_DESIGN.md), [mở rộng41–55](../../COGHE_CAMPAIGN_41_55.md), [vỏ56–60](../../COGHE_CAMPAIGN_56_60.md); các hồ sơ [31](Level31/README.md)–[40](Level40/README.md) có ước lượng chuỗi/vai trò của tác giả. Source hiện tại và test lời giải được ưu tiên khi hồ sơ lịch sử khác runtime.

| Mới | Cũ/source | ID ổn định | Nội dung / kỹ năng luyện | Tiên quyết | Vai trò bài | Chuỗi / cơ thể | Thao tác; quan sát; chi phí sửa sai |
| ---: | ---: | --- | --- | --- | --- | --- | --- |
| 01 | [01](../../../Assets/_Game/Venom/Campaign30/Definitions/Slot01.asset) | `venom.origin.01` | Bò đi | — | Dạy | 1 / 1 | chạm; mặt/lỗ thấy rõ; thấp |
| 02 | [02](../../../Assets/_Game/Venom/Campaign30/Definitions/Slot02.asset) | `venom.origin.02` | Leo đi | 01 | Dạy | 1–2 / 1 | chạm; mặt/lỗ thấy rõ; thấp |
| 03 | [41](../../../Assets/_Game/Venom/Campaign30/Definitions/Slot41.asset) | `coghe.tap.v1.01` | Khớp nối | 01 | Dạy | 1 / 1 | chạm; kiểm tra chốt/che khuất; thấp |
| 04 | [08](../../../Assets/_Game/Venom/Campaign30/Definitions/Slot08.asset) | `venom.origin.07` | Đẩy | 02 | Dạy | 2–3 / 1 | chạm tay nắm / đích; kiểm tra chốt/che khuất; thấp |
| 05 | [14](../../../Assets/_Game/Venom/Campaign30/Definitions/Slot14.asset) | `venom.origin.24` | Kéo ra mới qua | 04 | Dạy | 2 / 1 | chạm tay nắm / đích; kiểm tra chốt/che khuất; thấp |
| 06 | [18](../../../Assets/_Game/Venom/Campaign30/Definitions/Slot18.asset) | `venom.origin.26` | Bắc một nhịp | 05 | Dạy | 2–3 / 1 | chạm tay nắm / đích; kiểm tra chốt/che khuất; thấp |
| 07 | [09](../../../Assets/_Game/Venom/Campaign30/Definitions/Slot09.asset) | `venom.origin.23` | Khớp rồi! | 05 | Luyện | 2 / 1 | chạm tay nắm / đích; kiểm tra chốt/che khuất; thấp |
| 08 | [16](../../../Assets/_Game/Venom/Campaign30/Definitions/Slot16.asset) | `venom.origin.25` | Hai nhịp một cửa | 05 | Luyện | 3 / 1 | chạm tay nắm / đích; kiểm tra chốt/che khuất; thấp |
| 09 | [42](../../../Assets/_Game/Venom/Campaign30/Definitions/Slot42.asset) | `coghe.tap.v1.02` | Đưa trở về | 03 | Luyện | 3 / 1 | chạm; kiểm tra chốt/che khuất; thấp |
| 10 | [10](../../../Assets/_Game/Venom/Campaign30/Definitions/Slot10.asset) | `venom.origin.30` | BOSS · Nhà máy tí hon | 07, 08 | Boss | 3 / 1 | chạm tay nắm / đích; kiểm tra chốt/che khuất; thấp |
| 11 | [03](../../../Assets/_Game/Venom/Campaign30/Definitions/Slot03.asset) | `venom.origin.03` | Xoay đi | 02 | Dạy | 1–2 / 1 | xoay / định hướng; mặt/lỗ thấy rõ; thấp |
| 12 | [04](../../../Assets/_Game/Venom/Campaign30/Definitions/Slot04.asset) | `venom.origin.04` | Trơn đấy | 11 | Dạy | 2 / 1 | xoay / định hướng; mặt/lỗ thấy rõ; thấp |
| 13 | [05](../../../Assets/_Game/Venom/Campaign30/Definitions/Slot05.asset) | `venom.origin.21` | Nghiêng là tới | 11, 12 | Luyện | 2 / 1 | xoay / định hướng; mặt/lỗ thấy rõ; thấp |
| 14 | [06](../../../Assets/_Game/Venom/Campaign30/Definitions/Slot06.asset) | `venom.origin.05` | Trượt đi | 13 | Luyện | 2 / 1 | xoay / định hướng; mặt/lỗ thấy rõ; thấp |
| 15 | [07](../../../Assets/_Game/Venom/Campaign30/Definitions/Slot07.asset) | `venom.origin.06` | Xoay tròn | 13 | Luyện | 1–2 / 1 | xoay / định hướng; mặt/lỗ thấy rõ; thấp |
| 16 | [56](../../../Assets/_Game/Venom/Campaign30/Definitions/Slot56.asset) | `venom.origin.56` | Bình cổ hẹp | 15 | Luyện | 2 / 1 | xoay / định hướng; đọc miệng / lòng vỏ; thấp |
| 17 | [57](../../../Assets/_Game/Venom/Campaign30/Definitions/Slot57.asset) | `venom.origin.57` | Mặt nạ thủy tinh | 16 | Luyện | 2 / 1 | xoay / định hướng; đọc miệng / lòng vỏ; thấp |
| 18 | [59](../../../Assets/_Game/Venom/Campaign30/Definitions/Slot59.asset) | `venom.origin.59` | Đầu lâu pha lê | 17 | Luyện | 2 / 1 | xoay / định hướng; đọc miệng / lòng vỏ; thấp |
| 19 | [58](../../../Assets/_Game/Venom/Campaign30/Definitions/Slot58.asset) | `venom.origin.58` | Ấm nghiêng | 16 | Luyện | 2–3 / 1 | xoay / định hướng; đọc miệng / lòng vỏ; thấp |
| 20 | [60](../../../Assets/_Game/Venom/Campaign30/Definitions/Slot60.asset) | `venom.origin.60` | BOSS · Vỏ ốc | 15, 19, 18 | Boss | 3+ / 1 | xoay / định hướng; đọc miệng / lòng vỏ; vừa |
| 21 | [11](../../../Assets/_Game/Venom/Campaign30/Definitions/Slot11.asset) | `venom.origin.22` | Đáp rồi chui | 13 | Luyện | 2 / 1 | chọn nhánh; kiểm tra khoang/góc; thấp |
| 22 | [12](../../../Assets/_Game/Venom/Campaign30/Definitions/Slot12.asset) | `venom.origin.08` | Chui qua lỗ | 21 | Luyện | 2–3 / 1 | chọn nhánh; kiểm tra khoang/góc; thấp |
| 23 | [13](../../../Assets/_Game/Venom/Campaign30/Definitions/Slot13.asset) | `venom.origin.09` | Lật đi | 11 | Luyện | 2 / 1 | xoay / định hướng; kiểm tra chốt/che khuất; thấp |
| 24 | [15](../../../Assets/_Game/Venom/Campaign30/Definitions/Slot15.asset) | `venom.origin.11` | Kê cao lên! | 04 | Luyện | 3 / 1 | chạm tay nắm / đích; kiểm tra chốt/che khuất; thấp |
| 25 | [51](../../../Assets/_Game/Venom/Campaign30/Definitions/Slot51.asset) | `venom.origin.51` | Nối hai nhịp | 06 | Luyện | 3 / 1 | chạm tay nắm / đích; kiểm tra khoang/góc; thấp |
| 26 | [19](../../../Assets/_Game/Venom/Campaign30/Definitions/Slot19.asset) | `venom.origin.16` | Ghép cầu | 25 | Kết hợp | 4 / 1 | chạm tay nắm / đích; kiểm tra khoang/góc; vừa |
| 27 | [44](../../../Assets/_Game/Venom/Campaign30/Definitions/Slot44.asset) | `coghe.tap.v1.04` | Tách và gặp lại | 02 | Dạy | 3 / 2 | chọn phần + giữ; kiểm tra chốt/che khuất; vừa |
| 28 | [45](../../../Assets/_Game/Venom/Campaign30/Definitions/Slot45.asset) | `coghe.tap.v1.05` | Cùng vận hành | 27, 03 | Kết hợp | 4 / 2 | chọn phần + giữ; kiểm tra chốt/che khuất; vừa |
| 29 | [26](../../../Assets/_Game/Venom/Campaign30/Definitions/Slot26.asset) | `venom.origin.29` | Bạn giữ, tớ kéo | 28, 05 | Kết hợp | 4 / 2 | chọn phần + giữ; kiểm tra chốt/che khuất; vừa |
| 30 | [20](../../../Assets/_Game/Venom/Campaign30/Definitions/Slot20.asset) | `venom.origin.10` | BOSS · Chờ nhau | 27, 28, 29 | Boss | 4 / 2 | chọn phần + giữ; kiểm tra chốt/che khuất; vừa |
| 31 | [43](../../../Assets/_Game/Venom/Campaign30/Definitions/Slot43.asset) | `coghe.tap.v1.03` | Mở từng bước | 09 | Luyện | 3 / 1 | chạm; kiểm tra chốt/che khuất; thấp |
| 32 | [47](../../../Assets/_Game/Venom/Campaign30/Definitions/Slot47.asset) | `coghe.tap.v1.07` | Bốn điểm dừng | 31 | Dạy | 3–4 / 1 | chạm; kiểm tra chốt/che khuất; thấp |
| 33 | [49](../../../Assets/_Game/Venom/Campaign30/Definitions/Slot49.asset) | `coghe.tap.v1.09` | Rút rồi nối | 32 | Kết hợp | 4 / 1 | chạm; kiểm tra chốt/che khuất; thấp |
| 34 | [21](../../../Assets/_Game/Venom/Campaign30/Definitions/Slot21.asset) | `venom.origin.27` | Một bánh hai việc | 07, 09 | Luyện | 3 / 1 | chạm tay nắm / đích; kiểm tra chốt/che khuất; thấp |
| 35 | [23](../../../Assets/_Game/Venom/Campaign30/Definitions/Slot23.asset) | `venom.origin.28` | Nhường đường | 04, 07 | Luyện | 3 / 1 | chạm tay nắm / đích; kiểm tra chốt/che khuất; thấp |
| 36 | [24](../../../Assets/_Game/Venom/Campaign30/Definitions/Slot24.asset) | `venom.origin.17` | Nối bánh răng | 07, 08 | Luyện | 3 / 1 | chạm tay nắm / đích; kiểm tra chốt/che khuất; thấp |
| 37 | [46](../../../Assets/_Game/Venom/Campaign30/Definitions/Slot46.asset) | `coghe.tap.v1.06` | Đón bạn trở về | 28 | Kết hợp | 4 / 2 | chọn phần + giữ; kiểm tra chốt/che khuất; vừa |
| 38 | [48](../../../Assets/_Game/Venom/Campaign30/Definitions/Slot48.asset) | `coghe.tap.v1.08` | Chia việc đổi khớp | 37, 32 | Kết hợp | 4–5 / 2 | chọn phần + giữ; kiểm tra chốt/che khuất; vừa |
| 39 | [29](../../../Assets/_Game/Venom/Campaign30/Definitions/Slot29.asset) | `venom.origin.19` | Cùng nhau | 29, 22 | Kết hợp | 4 / 2 | chọn phần + giữ; kiểm tra khoang/góc; vừa |
| 40 | [50](../../../Assets/_Game/Venom/Campaign30/Definitions/Slot50.asset) | `coghe.tap.v1.10` | Cỗ máy chung | 38, 33 | Boss | 5+ / 2 | chọn phần + giữ; kiểm tra chốt/che khuất; vừa |
| 41 | [22](../../../Assets/_Game/Venom/Campaign30/Definitions/Slot22.asset) | `venom.origin.12` | Trượt rồi bay! | 21 | Luyện | 3 / 1 | chọn nhánh; kiểm tra khoang/góc; thấp |
| 42 | [17](../../../Assets/_Game/Venom/Campaign30/Definitions/Slot17.asset) | `venom.origin.13` | Mở đường! | 08, 22 | Kết hợp | 4 / 1 | chọn nhánh; kiểm tra khoang/góc; thấp |
| 43 | [28](../../../Assets/_Game/Venom/Campaign30/Definitions/Slot28.asset) | `venom.origin.15` | Tìm lối ra! | 22 | Luyện | 3+ / 1 | chọn nhánh; kiểm tra khoang/góc; vừa |
| 44 | [25](../../../Assets/_Game/Venom/Campaign30/Definitions/Slot25.asset) | `venom.origin.14` | Đổi chiều! | 13, 26 | Luyện | 3+ / 1 | xoay / định hướng; kiểm tra khoang/góc; thấp |
| 45 | [27](../../../Assets/_Game/Venom/Campaign30/Definitions/Slot27.asset) | `venom.origin.18` | Ghép đường | 11, 26 | Luyện | 3+ / 1 | xoay / định hướng; kiểm tra chốt/che khuất; thấp |
| 46 | [35](../../../Assets/_Game/Venom/Campaign30/Definitions/Slot35.asset) | `venom.origin.35` | Giữ rồi trả tự do | 29, 37 | Kết hợp | 4 / 2 | chọn phần + giữ; kiểm tra khoang/góc; vừa |
| 47 | [36](../../../Assets/_Game/Venom/Campaign30/Definitions/Slot36.asset) | `venom.origin.36` | Đổi ca | 46 | Kết hợp | 5 / 2 | chọn phần + giữ; kiểm tra khoang/góc; vừa |
| 48 | [37](../../../Assets/_Game/Venom/Campaign30/Definitions/Slot37.asset) | `venom.origin.37` | Chung một bánh | 47, 34 | Kết hợp | 5 / 2 | chọn phần + giữ; kiểm tra khoang/góc; vừa |
| 49 | [39](../../../Assets/_Game/Venom/Campaign30/Definitions/Slot39.asset) | `venom.origin.39` | Ba nơi, hai nhịp | 47, 48 | Kết hợp | 6 / 3 | chọn phần + giữ; kiểm tra khoang/góc; cao |
| 50 | [30](../../../Assets/_Game/Venom/Campaign30/Definitions/Slot30.asset) | `venom.origin.20` | TAM HỢP | 49 | Boss | 6+ / 3 | chọn phần + giữ; kiểm tra khoang/góc; cao |
| 51 | [52](../../../Assets/_Game/Venom/Campaign30/Definitions/Slot52.asset) | `venom.origin.52` | Đường lệch | 26, 25 | Kết hợp | 4 / 1 | chạm tay nắm / đích; kiểm tra khoang/góc; thấp |
| 52 | [53](../../../Assets/_Game/Venom/Campaign30/Definitions/Slot53.asset) | `venom.origin.53` | Kéo về đúng chỗ | 51, 05 | Kết hợp | 4 / 1 | chạm tay nắm / đích; kiểm tra khoang/góc; thấp |
| 53 | [54](../../../Assets/_Game/Venom/Campaign30/Definitions/Slot54.asset) | `venom.origin.54` | Đẩy và kéo | 52 | Kết hợp | 4 / 1 | chạm tay nắm / đích; kiểm tra khoang/góc; thấp |
| 54 | [55](../../../Assets/_Game/Venom/Campaign30/Definitions/Slot55.asset) | `venom.origin.55` | Xếp đường dài | 53 | Kết hợp | 5 / 1 | chạm tay nắm / đích; kiểm tra khoang/góc; cao |
| 55 | [31](../../../Assets/_Game/Venom/Campaign30/Definitions/Slot31.asset) | `venom.origin.31` | Lùi để tiến | 33, 34 | Luyện | 3 / 1 | chạm tay nắm / đích; kiểm tra chốt/che khuất; thấp |
| 56 | [32](../../../Assets/_Game/Venom/Campaign30/Definitions/Slot32.asset) | `venom.origin.32` | Một cầu, hai bến | 26, 09 | Luyện | 3 / 1 | chạm tay nắm / đích; kiểm tra khoang/góc; vừa |
| 57 | [33](../../../Assets/_Game/Venom/Campaign30/Definitions/Slot33.asset) | `venom.origin.33` | Đổi đường ống | 42, 56 | Kết hợp | 4 / 1 | chọn nhánh; kiểm tra khoang/góc; vừa |
| 58 | [34](../../../Assets/_Game/Venom/Campaign30/Definitions/Slot34.asset) | `venom.origin.34` | Nhường đúng chỗ | 35, 55 | Kết hợp | 5 / 1 | chạm tay nắm / đích; kiểm tra chốt/che khuất; vừa |
| 59 | [38](../../../Assets/_Game/Venom/Campaign30/Definitions/Slot38.asset) | `venom.origin.38` | Tụ để đổi việc | 46, 47 | Kết hợp | 6 / 2 | chọn phần + giữ; kiểm tra khoang/góc; cao |
| 60 | [40](../../../Assets/_Game/Venom/Campaign30/Definitions/Slot40.asset) | `venom.origin.40` | CỖ MÁY ĐOÀN TỤ | 50, 58, 59 | Boss | 7+ / 3 | chọn phần + giữ; kiểm tra khoang/góc; cao |

## Việc phải giải quyết trước khi áp60

- Bài04 thùng cần đặt gần vách và leo; hai ý có thể quá nhiều dù input chỉ gồm chạm. So thời gian hiểu mục tiêu với thời gian thao tác riêng.
- Bài03/09 một chạm khác tay nắm–đích của04–08. Ký hiệu và câu ngắn trong pilot phân biệt rõ; nếu người mới vẫn dùng nhầm thì đổi vị trí hoặc làm thí nghiệm hợp đồng điều khiển riêng.
- Chương11–20 cần triển khai bài kéo mẫu + hướng rơi và kiểm tra vỏ ốc có phù hợp Boss20. Bản pilot10 hiện không chứa chương này.
- Bài27 (cũ44) dạy tách/chọn/hợp,28 dạy giữ. Kiểm chứng người chơi chủ động chọn từng phần và nhập trước khi thử Boss30.
- Bài49 (cũ39) là lần đầu yêu cầu ba vai trò. **Cần rút gọn thành bài luyện ba vai trò hoặc bổ sung bước đệm trước Boss50**, không dùng nguyên thử thách dài rồi gọi là bài nhập môn. Không coi việc dời số là đã giải quyết gate này.
- Nhóm51–54 nhiều cầu có thể nhẹ hơn chương trước; đây là nhịp hồi phục tạm, phải điều chỉnh bằng dữ liệu thực. Boss60 vẫn dùng kỹ năng đã dạy.
- Chỉ áp catalog60 và cập nhật chọn/next/build sau phiên người mới. Giữ completion theo ID, Home đã mở, key production; bộ test save pilot chứng minh cách ly namespace, chưa chứng minh một catalog60 mới đã ship.
