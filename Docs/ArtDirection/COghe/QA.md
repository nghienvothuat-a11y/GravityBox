# COghe — Kiểm tra concept

Ngày 15/09/2026. Đã xem trực tiếp cả ba ảnh cuối.

## Quy trình và nguồn

Dùng imagegen tích hợp, tạo ba concept độc lập từ screenshot runtime; chỉnh một lượt cho Boss. Không dùng script chỉnh sửa ảnh. Các PNG cuối được sao chép nguyên bản vào `Concepts/`.

Tham chiếu sinh vật:
- `Artifacts/Venom01/OriginFrames/home.png`: khối đen có nếp mô, đỉnh thân tạm thời và xúc tu nhỏ, không mắt/mặt.
- `Artifacts/Venom01/OriginFrames/07-climbing-step.png`: bám thùng và quan hệ thùng–vách–lỗ ở góc camera mới.
- `Artifacts/Venom01/OriginFrames/10-cooperation.png`: hai phần, A/B, dao và nắp trong Boss hiện hành.

| PNG cuối | Output gốc của imagegen |
| --- | --- |
| `01-day-lab.png` | `exec-6d08d15a-2a11-4426-a70b-15c07f522e04.png` |
| `02-night-trial.png` | `exec-764ea129-ba1a-4d2e-becb-72ba8b66ca10.png` |
| `03-a-place-to-belong.png` | `exec-da83d887-6ab5-4b04-8d5d-43374a32450f.png` |

Các file gốc nằm dưới `/Users/mrk/.codex/generated_images/01a07f0d-8d91-7e21-89c0-7806517c9eb8/`; tài liệu project dùng bản sao trong repo.

## Kiểm tra bằng mắt

| Nội dung | Kết quả |
| --- | --- |
| Tên game | “COghe” giữ đúng cách viết trên thẻ/title của ba concept |
| Danh tính | Đều là khối mô đen bất đối xứng, không mắt, miệng, trang phục hoặc mặt cố định |
| Màn 7 | Thân nằm trên mặt thùng hướng về camera; đọc được nóc thùng, vùng trơn và miệng lỗ trên vách |
| Boss | Hai phần đều ở trong hộp, A/B đọc được; chưa có mô thoát ở lỗ cuối |
| Phản chiếu Boss | Bản đầu có một bóng sinh vật trên kính bên trái, dễ nhầm phần thứ ba. Bản v2 đã bỏ bóng đó |
| Dao Boss | Bản đầu có ray sàn dễ gợi chuyển động ngang. Bản v2 dùng thanh dẫn đứng và giá đỡ nhỏ; cơ khí chi tiết vẫn cần dựng theo simulation |
| Nhà | Một cơ thể liền, một cử chỉ vươn mềm về đầu ngón tay qua kính; bát ăn và bóng đồ chơi ở bên trong |
| Tính nhất quán | Dùng chung kính trong, gá sáng, mặt thiết bị màu kem và thân đen; Night thay ánh sáng, Home thêm đồ vật chăm sóc |

## Giới hạn cần giữ khi triển khai

- Concept là tranh diễn giải, chưa phải asset 3D, shader, animation hay screenshot build.
- Cơ thể có dáng tĩnh minh hoạ. Khi dựng, dùng mesh/mô hiện tại, không lấy phần nhô lên làm đầu cố định hoặc thêm giải phẫu mới.
- Mức độ bóng, ánh sáng nền và độ mờ hậu cảnh trong tranh không phải cam kết chất lượng realtime trên mobile. Dùng cubemap và nền chuẩn bị sẵn để tiến gần hướng hình ảnh với chi phí được đo.
- Khung gá, viền lỗ và thùng trong tranh có độ dày minh hoạ. Vùng bò, khe và miệng lỗ phải giữ kích thước vật lý; viền exit cần phẳng và ánh sáng nhỏ, yếu.
- Vành bám trong Day Lab hiện khá nhám/nhạt: khi dựng cần kiểm tra để người chơi phân biệt rõ “bám được” với lớp trơn xanh, kể cả grayscale.
- Cửa và nắp Boss cần giá đỡ/hành trình khớp bộ cơ quan hiện có. Không thêm một vách cố định hoặc ray trang trí chắn đường chỉ vì có trong tranh.
- Hình Nhà diễn đạt tiếp xúc qua kính; không suy ra thêm cơ chế chạm trực tiếp qua lỗ hoặc tự đổi điều khiển gameplay.
- Tương phản sau resize xuống màn điện thoại, thứ tự lớp kính khi quay và performance cần được kiểm tra trong Unity/thiết bị; chưa có kết quả đo cho bộ art mới.
- Không sửa scene, script runtime, physics, build hoặc save trong đợt này. Không chạy test Unity vì chỉ thêm tài liệu và ảnh.

Prompt gốc và bản chỉnh được lưu tại `Prompts/`. Mọi thay đổi cơ chế trong tài liệu Venom cũ không phải yêu cầu của bộ concept COghe này.
