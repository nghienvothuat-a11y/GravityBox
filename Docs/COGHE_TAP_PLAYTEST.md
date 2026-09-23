# COghe — thử chapter mười màn với người mới

Mục đích: kiểm tra người chơi tự hiểu và thực hiện được ý định, đặc biệt việc
điều khiển hai phần. Đây là kế hoạch thu thập bằng chứng, **chưa có kết quả người mới**.

## Chuẩn bị

- Dùng app `Builds/COgheTapChapter/macOS/COghe.app` cho vòng quan sát ban đầu trên Mac.
  Bản này dùng chuột thay một ngón tay; không coi kết quả là kiểm chứng touch/FPS trên điện thoại.
- Người chơi chưa thấy lời giải hoặc video replay. Nếu so sánh bản cũ/mới, dùng
  hai nhóm mới độc lập; đề xuất 5–8 người/bản. Ghi số người thực tế.
- Mở bình thường, không dùng cờ `-coghe-tap-proof-*`. Bắt đầu màn 01.
- Người quan sát không chỉ thứ tự cơ quan. Cho người chơi đọc hướng dẫn trong game,
  thử sai và làm lại. Nếu phải giúp, ghi nguyên văn câu giúp và thời điểm.

## Quan sát

| Màn | Điều cần quan sát |
| --- | --- |
| 01 | Có nhận ra bánh răng cần chạm? Có thấy liên kết làm cửa mở? Có biết chạm lỗ để thoát? |
| 02 | Có hiểu lần chạm sau đưa cơ quan trở về, và vì sao quay về lại có ích? |
| 03 | Có đọc được chốt/kết nối để chọn thứ tự, hay chỉ bấm ngẫu nhiên tất cả? |
| 04 | Có tự thử dao, chọn từng phần và nhập lại? Có bỏ qua bài luyện bằng cách đi thẳng ra? |
| 05 | Có hiểu vì sao cần hai phần, đổi chọn mà vẫn giữ A, nhả A rồi nhập lại? |
| 06 | Có nhớ thu hồi phần giữ bàn đạp khi cửa đã chốt? |
| 07 | Có đọc được chốt hiện tại/kế tiếp và vai trò chốt 2/4? Có sửa được khi bỏ lỡ bật nguồn? |
| 08 | Có phân vai và đổi cơ quan mà không làm phần giữ bỏ việc? |
| 09 | Có hiểu rút A rồi trả về, thời lượng có tạo nhịp nghỉ? |
| 10 | Có tự kết hợp luật mà không được chỉ lời giải? Có nhập rồi thoát và vào Nhà? |

Màn 04 cho phép thoát nguyên khối; không có điều kiện lịch sử ẩn buộc phải cắt.
Nếu bỏ qua rồi mắc ở 05, ghi đó là vấn đề onboarding cần sửa.

## Phiếu một lượt

- Mã người chơi (không cần tên), đã chơi bản nào trước đây, máy/OS, build/hash:
- Màn tự qua không trợ giúp:
- Màn cần giúp; nội dung trợ giúp:
- Mỗi màn: thời gian suy nghĩ, thời gian chờ thao tác, số lần bấm nhầm/làm lại:
- Ví dụ ý định đúng nhưng game không thực hiện được (thao tác và trạng thái cụ thể):
- Sau màn 05, hỏi: “Vì sao cần tách thành hai phần?” và “Cái gì làm cửa mở?”
  Ghi câu trả lời nguyên văn.
- Có tự chọn chơi tiếp nếu có thêm màn? Lý do bằng lời của người chơi:

## Mục tiêu đánh giá bản 10 màn

Mục tiêu đề xuất: ít nhất 80% tự qua 01–03, đồng thời đa số giải thích được vai trò
của hai phần ở 05. Báo cả tử số/mẫu số, không chỉ phần trăm. Tách lỗi hiểu câu đố
khỏi lỗi input và đường đi. Mrk đã yêu cầu dựng đủ 10 màn để thử chung; nếu thao tác cơ bản còn gây mắc, ưu tiên sửa nó trước khi mở rộng tiếp.

Vòng điện thoại tiếp theo cần ghi model/OS/build, kích thước màn hình/safe area,
chạm bằng một ngón, thao tác sát UI, pause/background/resume và frame-time/nhiệt.
Kết quả replay tự động trên Mac không thay thế vòng này.
