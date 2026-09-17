# Kiểm tra bộ concept v1

Ngày 11/09/2026. Đã xem trực tiếp cả bốn hình và hai lần chỉnh bằng imagegen tích hợp. Đây là kiểm tra concept, không phải nghiệm thu gameplay hay benchmark trên điện thoại.

| File cuối | Kích thước | Nội dung đã kiểm tra |
| --- | --- | --- |
| `Concepts/01-journey05.png` | 941 × 1672 | Khung dọc gần 9:16; hai phần, A 30/24 g, B 66/60 g; OUT 0/96 g; dao nâng; lỗ trần mở; nắp B và nắp trần là hai vật thể riêng |
| `Concepts/02-living-matter.png` | 1536 × 1024 | Sáu dáng, không mắt/mặt; bám dưới trần và võng xuống; tách 30 + 66 = 96 g; mô liên tục qua khe; bảng vật liệu |
| `Concepts/03-first-encounters.png` | 1672 × 941 | Ba bài đầu; 01 lỗ sàn; 02 lỗ trần và bám tường phía trong; 03 nắp đóng và vòng giữ 1,8 giây; không có B/dao ở bài 03 |
| `Concepts/04-glass-flower.png` | 1536 × 1024 | Nhãn FUTURE LEVEL CONCEPT; hình chính có sáu mặt nối nhau; A/tay kéo, C 60 g, phần nhỏ 30 g, D; cửa ra còn đóng |

## Những chỉnh sửa đã thực hiện

Tấm màn 05 ban đầu có viền lỗ quá cao, hai chức năng nắp dễ bị hiểu thành một, lưỡi dao dựng cạnh dọc và tư thế phần lớn giống ăn mừng. Đã dùng [prompt chỉnh màn 05](Prompts/hero-edit-v2.txt) để giảm viền về mặt trần, tách hai nắp, đưa cạnh cắt về phương ngang trên khe tiếp cận, hạ dáng giữ và giảm chênh lệch tỷ lệ hai phần.

Boss ban đầu vẽ dư một mặt đứng và đáy nhiều cạnh. Đã dùng [prompt chỉnh boss](Prompts/boss-edit-v2.txt) để bỏ mặt dư và đưa về sơ đồ sáu mặt: một đáy vuông, bốn vách nối bốn cạnh, một mái nối cạnh ngoài của vách sau.

Bản cuối được sao chép nguyên PNG vào project; không dùng script chỉnh sửa hình. Bản thử ban đầu không được đưa vào danh sách concept duyệt.

## Những phần cần hoàn thiện khi dựng asset

- Tranh không phải bản vẽ kích thước. Tỷ lệ kích thước cơ thể mang tính minh họa; lượng mô phải đọc từ simulation khi chạy.
- Ở màn 05, ray nắp B trong tranh còn được giản lược và kết thúc phía trên mặt sàn. Model sản xuất cần có điểm neo/đường dẫn rõ, nằm ngoài vùng bò, đúng hành trình của cơ cấu hiện tại.
- Các khung bo, sensor pad và vòng chọn được làm dày để đọc trong concept. Khi dựng, giữ viền thoát phẳng, không để gờ trang trí làm người chơi hiểu sai collider hoặc chặn đường.
- Tấm ba màn đầu dùng camera trực diện hơn tấm màn 05. Cần thống nhất framing trên cùng camera runtime và kiểm tra sau khi xoay.
- Các tư thế trên sheet mô tả ngôn ngữ cơ thể; chưa phải bộ animation hoặc tỷ lệ giải phẫu cố định.
- Boss mới đạt sơ đồ hình ảnh sáu mặt. Khe bản lề, đường nối, vùng chờ, gate D và vị trí chốt cần blockout + kiểm chứng vật lý; tranh không chứng minh lời giải hoặc đường nền.
- Chữ trong tranh đã kiểm tra bằng mắt ở độ phân giải file. Chưa chạy thử text layout/localization, độ tương phản đo được, thao tác chạm hoặc grayscale trên thiết bị.

Chỉ thêm thư mục `Docs/ArtDirection/Venom`. Không sửa code, scene, material Unity, build hoặc save; không chạy lại tests Unity.
