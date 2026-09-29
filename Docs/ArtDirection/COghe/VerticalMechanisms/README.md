# COghe — Nghiên cứu cơ quan và chiều cao

29/09/2026 · Đề xuất để xem và thảo luận, **chưa sửa gameplay/campaign**.

- [Trang xem minh họa và diễn giải tiếng Việt](review.html).
- [Kế hoạch đầy đủ, palette, kiến trúc, đối chiếu code và kiểm chứng](PLAN.md).
- [Hồ sơ V01 — Trạm trên vách](../../../LevelDesign/COghe/VerticalStudy/V01/README.md).
- [Hồ sơ V02 — Ghép đường lên cao](../../../LevelDesign/COghe/VerticalStudy/V02/README.md).

## Minh họa

1. [Màu và hình cơ quan](01-color-grammar.png): xanh vòng tròn A, coral hình thoi B; hình dạng nêu thao tác, màu nêu liên kết. Nút chịu tải trong game cần mô thực đứng lên mặt cap; pose xúc tu trong tranh chỉ minh họa tương tác, không xác nhận sensor có cơ chế nhấn chủ động.
2. [Cơ quan trên vách](02-wall-station.png): leo → kéo A → cửa A mở → bò lên đích. Minh họa ngoại hình và sự phân bố chiều cao.
3. [Ghép đường bám](03-build-route.png): A đưa tới đảo bám giữa vành trơn; B dịch tấm đứng nối đảo với vùng trên. Bản vẽ tỷ lệ logic trong trang review/hồ sơ là nguồn cho greybox; không đo kích thước bằng pixel concept.

Giữ glass C và bảng số dán kính. Bề mặt trơn không bám chủ động; không chỉ giảm tốc. V2 xoay camera, không xoay trọng lực. Hình có thể nhấn mạnh phản xạ/độ mờ nền hơn renderer thật; khi dựng phải dùng kit C và chụp Unity để so, không bổ sung post-process đắt chỉ để chạy theo tranh.

## Nguồn, prompt và kiểm tra

Tạo bằng **imagegen tích hợp**, không dùng CLI/API riêng. Tham chiếu: [ảnh Unity glass C hiện hành](../../../Verification/COgheSpecimenPlates/01.png), [concept Day Lab gốc](../Concepts/01-day-lab.png) chỉ hỗ trợ chất liệu. Prompt nguyên văn nằm trong [Prompts](Prompts/).

| File cuối | Lần tạo / chỉnh | Kiểm tra hình ảnh |
|---|---|---|
| 01-color-grammar.png | 01-color-grammar.txt + 01-color-grammar-correction.txt | Đã sửa chú thích trơn từ giảm tốc thành NO ADHESION; màu/glyph, đầu vào–đầu ra và trạng thái nhìn rõ |
| 02-wall-station.png | 02-wall-station.txt | Một cụm xanh, điểm bám/tay kéo trên vách, cửa trên cao; giữ cơ thể không mắt/mặt |
| 03-build-route.png | 03-build-route.txt + 03-build-route-correction.txt | Đã sửa B thành tấm đứng trượt ngang, đầu vượt vành trơn và điểm đỗ về bệ giữa; hình thoi B. Vành trong tranh phối cảnh vẫn mang tính minh họa; kích thước theo hồ sơ |

Original outputs được giữ trong thư mục imagegen của Codex; bản cuối đã sao chép vào repository. Chưa chạy Unity, chưa đo OPPO, chưa xác nhận puzzle khả giải. Đã đọc code cảm biến tải, rail, truyền động, graph revision và camera để ghi rõ khả năng tái dùng/mở rộng; đã kiểm tra liên kết tài liệu và đường dẫn ảnh local. Không thay STYLE_RULES vì palette/cơ quan mới đang là đề xuất.

Làm rõ tiếp theo từ người dùng: thư viện gồm ròng rọc, thang nâng, khối xếp và các cơ cấu khác; giới thiệu dần, xen kẽ sàn/chiều cao. Xem mục 9 của PLAN và phần “Thư viện & nhịp học” trên trang review. Hai minh họa không giới hạn hướng phát triển vào bánh răng/cửa.
