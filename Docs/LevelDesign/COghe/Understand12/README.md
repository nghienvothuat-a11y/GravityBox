# Hiểu ra — 12 bài trong 65 màn

Cập nhật 19/10/2026 theo yêu cầu tăng nhịp gặp COghe và tăng độ khó dễ chịu.
Đây là các **bài tương tác ở giữa màn**, không phải 12 scene puzzle mới.
65 màn, Boss 10 và các lời giải giữ nguyên.

Nguồn: [quy tắc level](../../../COGHE_LEVEL_DESIGN_RULES.md),
[mẫu hồ sơ](../LEVEL_TEMPLATE.md), [ability cards](../../../COGHE_ABILITY_CARDS.md),
[kiểm chứng](../../../Verification/COgheUnderstandPacing/README.md).

## Nhịp tiến trình

- Bài luyện ngắn sau **6, 16, 27, 38, 49, 60**: một câu, một mục tiêu học.
- Bài đầy đủ sau **10, 21, 32, 43, 54, 65**: ba lượt, gồm ôn tập và đồ chơi ở cuối (từ Boss 21).
- Mỗi bài hoàn thành cho chọn **một ability card, +1**, tổng 12 lựa chọn cho người chơi mới qua đủ 65 màn.
- Khoảng cách 4–6 màn; không đổi Boss hoặc buộc mỗi chương dài đúng 6 màn.
- Bài đầu mượn phòng tương tác trước khi mở Nhà. Nhà/Collection vẫn mở sau Boss 10,
  bài tiêm màu vẫn sau màn 8; không mở sớm cửa hàng hay tự bật wardrobe.
- Luyện → ôn/kết hợp → thử đồ chơi; không đồng thời tăng độ dài, đếm và đồ vật mới.
  Bài sau Boss có thể bắt đầu nhẹ hơn bài luyện cuối chương để người chơi lấy nhịp.

| Bài / hồ sơ | Sau màn | Nội dung chính |
| --- | --- | --- |
| [P01 — Một cái ôm](P01.md) | 6 | Một biểu tượng → một hành động. Không cần đọc chữ, chọn đồ hay đếm. |
| [B01 — Ba điều quen thuộc](B01.md) | 10 | Ba câu độc lập, mỗi câu chỉ một hành động; giữ độ dễ của bài cũ. |
| [P02 — Hai mong muốn](P02.md) | 16 | Hai yêu cầu, chưa bắt nhớ thứ tự hay số lượng. |
| [B02 — Chơi cùng nhau](B02.md) | 21 | Ôn hai yêu cầu rồi khám phá một đồ chơi. |
| [P03 — Trước và sau](P03.md) | 27 | Đúng thứ tự với hai hành động quen, chưa thêm đếm. |
| [B03 — Nhớ lời COghe](B03.md) | 32 | Chuỗi hai hành động, sau đó đồ chơi mô phỏng giai điệu. |
| [P04 — Hai miếng nhé](P04.md) | 38 | Số hai được giới thiệu riêng, không ghép chuỗi. |
| [B04 — Đếm cùng nhau](B04.md) | 43 | Luyện số hai bằng hai đồ vật; đồ chơi có các lượt riêng. |
| [P05 — Đếm rồi ôm](P05.md) | 49 | Kết hợp đếm với thứ tự, chỉ hai yêu cầu. |
| [B05 — Một nhịp phối hợp](B05.md) | 54 | Hai yêu cầu, một yêu cầu đếm; giới thiệu con quay riêng. |
| [P06 — Ba điều nhỏ](P06.md) | 60 | Chuỗi ba yêu cầu, chỉ một yêu cầu đếm đến hai. |
| [B06 — Chúng mình hiểu nhau](B06.md) | 65 | Ôn chuỗi hai rồi ba yêu cầu; không đếm nhiều từ hoặc đếm đến ba trong câu. |

## Luật nhẹ nhàng

Câu hỏi không có giới hạn thời gian. Người chơi có thể quan sát COghe lặp lại hình thể;
sai không trừ Drops, mất mạng hay reset cả bài. Các yêu cầu đã làm đúng được giữ trong
câu; nếu đang đếm mà sai thì chỉ đếm lại yêu cầu đó. Sau hai lần sai, UI nói rõ hành
động tiếp theo. Con quay nhập môn cho 5,5 giây để cứu, thêm 1,25 giây mỗi lần sai (tối đa 8 giây): đây là hành vi đồ chơi riêng,
không phải đồng hồ giới hạn cả bài, và rơi không xóa các lần cứu đã đúng.

Câu dài nhất trong 12 bài là **ba yêu cầu / bốn thao tác**, chỉ một yêu cầu đếm đến hai.
Các trò chơi trống, kèn, bóng, con quay và hộp nhạc giữ cấp nhập môn `Level=0`, có mẫu
được diễn trước. Số tiếng trống hoặc hơi thổi trong đồ chơi không phải số từ trong câu.
Trống và bóng: 1 → 2 → 2 lần; kèn: 2 → 2 → 3 nốt. Lượt giữa luyện lại trước khi tăng, giữ những lượt trước đã xong. Hộp nhạc giữ 3 lần chạm mỗi đoạn, không yêu cầu đúng nhịp. Không yêu cầu mua đồ.

Mục tiêu thời lượng bài ngắn: người chơi mới khoảng 20–40 giây; đây là **mục tiêu cần
chơi thử với trẻ**, không phải số đo comprehension. Script biết đáp án có thể nhanh hơn.
Bài đầy đủ không bị kéo dài bằng chờ chủ động hay điều kiện chính xác từng frame.

## Dữ liệu và tương thích

- Boss giữ ID `1..6`, khóa `bonus:1..6` và khóa từng câu cũ, nên thẻ/toys/Drops đã có không bị mất.
- Bài luyện dùng ID `1001..1006`, khóa `bonus:practice:1..6`; không renumber boss.
- `COgheBonus.Milestones` lấy boss thật trong catalog và đặt bài luyện ở màn thứ sáu
  từ boss trước; không đặt lên boss nếu chương quá ngắn. Chỉ sáu bài luyện đã author
  được bật. Các chương sinh tự động từ 7 trở đi chưa có bài luyện mới trong đợt này.
- `COgheShop.MigrateUnderstand` chạy một lần trước chơi. Bài luyện mới nằm ở mốc đã
  thắng của save cũ được đánh dấu **skipped**, không bắt chơi bù, không cấp thẻ hồi tố.
  Skipped khác Done. Bài boss còn dở và các câu đã xong vẫn chờ đúng như trước.
- Rời game ở màn chọn thẻ: Play mở lại thẻ đã kiếm được trước khi sang puzzle,
  kể cả khi Nhà chưa mở. Không hỏi lại bài đã xong, không cộng phần thưởng lần hai.
- Một bài luyện: thưởng câu đầu + thưởng hoàn tất bằng hai lần thưởng câu cơ sở
  (mặc định 10 + 20 Drops). Bài boss giữ toàn bộ phần thưởng cũ.
  Thẻ đều +1; hiệu lực/max của Playful, Speedy, Clever không đổi.
- Analytics `bonus_offer/start/round/complete/leave` và `ability_pick` giữ trường `chapter`
  là ID interlude ổn định: 1–6 là bài boss, 1001–1006 là bài luyện. Khi phân tích
  completion/misses phải tách hai loại, không coi 1001 là chương thứ một nghìn.
- Đồ chơi mới vẫn mở sau Boss 21/32/43/54/65. Card IDs lưu trong `CardsPicked`.

## Ranh giới và kiểm chứng

Không thêm mesh, shader, đèn, physics, mạng, scene hoặc cơ quan mới. Dùng phòng, hình
thể, đồ chơi và UI Glass C hiện hành. Chỉ thay dữ liệu bài, mốc mở và luồng tiến trình.
Dossier áp dụng các mục của LEVEL_TEMPLATE cho bài tương tác; hình học hộp, trơn,
phân tách/tụ và lỗ thoát không áp dụng.

Kiểm tra bắt buộc: cả 12 bài bằng các đường input thật; lựa chọn/nhận thẻ; trả lời sai;
resume checkpoint; chơi bài đầu lúc Nhà khóa; migration và idempotency.
Cần quan sát thêm với nhóm 4–7, 8–11, 12–15 tuổi: số lần diễn lại, chạm nhầm,
thời gian hoàn thành và tỷ lệ rời bài. Chưa tuyên bố cải thiện retention D1/D7/D30.
