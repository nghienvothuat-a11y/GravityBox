# Kiểm tra minh hoạ — Spatial 11–30

Ngày 29/09/2026. Ảnh concept tạo bằng **built-in image_gen**, dùng screenshot Unity `Docs/Verification/COgheSpatialCircuits/04-start.png` làm tham chiếu style. Không dùng Blender/renderer Unity để tạo các ảnh này. Prompt và lịch sử sửa lưu tại `Prompts/` và `generation-manifest.json`; file gốc sinh ảnh được giữ ở thư mục công cụ, bản bàn giao copy vào `Illustrations/`.

## Cách đọc

- Ảnh biểu diễn bố trí và cảm giác đồ hoạ; tỷ lệ/cung chuyển động/va chạm **chưa phải bản thi công**. Các inset biểu diễn nhiều thời điểm, không cộng sinh vật giữa các inset thành một trạng thái.
- Dữ liệu `levels.json`, lời giải và sơ đồ `Routes/` làm rõ phụ thuộc. Mỗi hồ sơ có riêng ràng buộc chống đi tắt, bến về, vùng tụ và vị trí lỗ cuối. Nếu hình chưa biểu diễn đủ chi tiết này, phải dựng theo hồ sơ và kiểm chứng greybox, không lấy hình làm chứng minh khả giải.
- Một số concept tối giản dải trơn/vách/khớp để nhìn rõ cơ quan. Không được bỏ chúng trong scene nếu cần chặn leo tắt; không thêm collider vô hình để cứu bản vẽ.
- Bảng số, vòng thoát và bề dày ray trong concept cần đối chiếu kích thước runtime style C. Lỗ cuối phải cắt trên kính ngoài, cùng cao độ bến nhận; mint không biến miệng Transfer thành lỗ thắng.

## Các sửa hình đã thực hiện

Đã xem từng ảnh sinh ra. Những bản đầu có lỗi nội dung được sửa trước bàn giao: hốc nuốt thùng ở11, nước xanh sai trong14, dư cơ thể ở17/20/22/29, hình Q chia ba đầu ra, dây đu vẽ thành cáp trượt, tải đối trọng không nối với cầu. 26 được vẽ lại một cảnh duy nhất; các bài cuối dùng hình chính rộng để tránh nhầm các trạng thái từ inset.

| Màn | Trọng tâm kiểm tra / điều còn phải chốt khi dựng |
| --- | --- |
| 11 | Khối đỗ ngoài bệ; độ cao bậc và khe bám sau khi kê phải thử, không cho nhảy theo mũi tên |
| 12 | Hai khối và ô chờ; receiving supports không tạo sẵn toàn bộ đường leo |
| 13 | Một khay động, hai bến cố định; kiểm tra chở prop và người cùng lúc |
| 14 | Ống trống cyan, mô đen; dải không bám nối kín tới kính, không đi vòng vách |
| 15 | Junction thật và cửa chốt; ngã ba phải chứa được toàn thân để đổi ý |
| 16 | Hai phần bằng nhau; hai nút A/B mở hai khóa của cầu, đủ cả hai mới latch |
| 17 | Chỉ hai phần; A là pad đứng; B chốt giải phóng người giữ và mở lối tụ |
| 18 | Dây đu từ một điểm neo; cung minh hoạ không là nghiệm vật lý, cần thử tời/bến |
| 19 | Bến dịch trên ray, dây pendulum; cần khớp cung thật và tránh leo thẳng cột đích |
| 20 | Hai phần, ống, B đường về, thùng, tời; phải tách rõ đường hồi lưu và nhịp cuối |
| 21 | Dây từ khay tải qua bánh tới đuôi cầu; góc khởi đầu, hố hành trình tải và gối chốt cần greybox |
| 22 | Ba mảnh có gối đỡ; ô đỗ/giá nhận không được tự tạo đường tắt khi chưa ghép |
| 23 | Ống giao nhưng không nối, cửa sổ junction thật; Transfer và lỗ ngoài là hai vị trí khác nhau |
| 24 | Bốn phần qua các lượt tách; A1/A2 và B1/B2, bốn tải chốt rồi được rời |
| 25 | Tổng50+25+25; phép tách50→25+25; nhập100 trước thoát |
| 26 | Hai phần, A giữ ống, B mở bến, dây một điểm neo; tời/đường leo cần chi tiết authoring |
| 27 | Hai giữ A, một giữ B dưới sàn, một lên C; C cho cả nhóm đường về |
| 28 | Ba junction và vòng quay lại; các bậc ngoài phải nối được A→B→vùng tụ |
| 29 | Ba phần hữu ích; hai mảnh ghép vào cùng khung rồi nâng bằng B |
| 30 | Bốn phần ở ba khoang; hai nhánh độc lập, vùng tụ và bến cầu cuối rộng |

## Kiểm tra tài liệu

`python3 Docs/LevelDesign/COghe/SpatialNext20/validate_design.py` kiểm tra20ID,20ảnh,20hồ sơ đủ10mục, links cục bộ, Boss20/30, các chuyển khối lượng đúng chia đôi/nhập cộng. Kết quả ở `design-audit.json`. Đây là kiểm tra **tính nhất quán tài liệu**, không phải test Unity hoặc chứng minh không softlock.

JavaScript trang review được kiểm tra cú pháp bằng `node --check`. Chưa kiểm chứng tương tác trang qua browser automation: công cụ browser từ chối mở URL `file://`; không dùng đường vòng để vượt giới hạn. Có thể mở `review.html` thủ công để xem. Các ảnh được xem trực tiếp qua công cụ ảnh trong quá trình làm.

## Chưa được xác nhận

Chưa có scene, mesh thi công, native build, proof replay, playtest người mới hay FPS OPPO cho20màn này. MáyQ, dây, tải đối trọng và bốn nhóm mô cần prototype trước. Chưa có cơ sở nói tất cả đã qua màn hoặc đạt60FPS.
