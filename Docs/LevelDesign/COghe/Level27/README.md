# COghe — venom.origin.27 — Một bánh, hai việc

## 1. Định danh, phạm vi và trạng thái

- ID ổn định `venom.origin.27`; vị trí hiển thị **21**, trước màn cũ 12 ở vị trí 22; màn cũ 17 ở vị trí 24.
- Prototype Unity v3, 18/09/2026; scene, definition, builder và driver hai trạm đã tồn tại.
- Origin hiện hành, 32 hạt/120 Hz; thay bản v0.1 hai bánh trung gian để không lặp đúng bài màn cũ 17.
- Người dùng thích đẩy/kéo bánh răng; yêu cầu thử thách vừa sức thay cho thao tác tilt chính xác.
- Trạm A/B, cửa thật, reset và ngân sách mô phỏng đã qua test macOS; chưa playtest tay mobile.
Nguồn: [quy tắc level](../../../COGHE_LEVEL_DESIGN_RULES.md), [template](../LEVEL_TEMPLATE.md), [Day Lab](../../../ArtDirection/COghe/STYLE_RULES.md), [kiến trúc](../../../COGHE_EXPANSION_11_20.md).

**Sửa theo phản hồi bánh răng không khớp, 18/09:** cả năm bánh có cùng mặt phẳng
trục, bán kính vòng chia 45 mm, 18 răng. G khớp giữa hai bánh cố định cách G 90 mm
tại mỗi trạm. Ray A↔B dài 180 mm; nắp A nâng 225 mm để hở cả đỉnh răng khi G đi
qua. Răng được dựng thành mesh involute có khe hở nhỏ, thay các khối chữ nhật
chồng nhau. Truyền lực kiểm tra khoảng cách tâm, mặt phẳng và hướng trục thật;
hai bánh cố định quay ngược G, pha răng được căn cả khi G đi vào khớp. Góc quay
theo hành trình tải đo được; rút G thì phanh giữ tải. Reset trả pha ban đầu.
[Bằng chứng kiểm tra và giới hạn](../../../Verification/COgheCampaign30/gears-21.md).

## 2. Mục tiêu trải nghiệm và tiến trình

- Nhận ra có thể dùng lại **cùng một bánh** để làm hai việc, vì cơ cấu đầu đã gài chốt giữ kết quả.
- Đã biết một bánh trung gian ở màn mới 23; tổ hợp mới là chọn trạm cấp lực, không thêm thao tác điều khiển.
- Nguồn quay/nhánh tải nhìn thấy từ đầu; ở A, nắp cản ray nâng lên; ở B, cửa cuối nâng lên.
- Màn luyện tập vừa sức; chỉ nhắc cách chọn/buông tay nắm, không chỉ dẫn thứ tự lời giải bằng mũi tên.
- Một cơ thể; không nâng chỉ số, nút giữ đồng thời, timer hoặc quyền mở Nhà mới.

## 3. Phác thảo, hình học, camera và thao tác

![Phác thảo 27 · Một bánh, hai việc](mockup-v1.png)

[Xem cả 10 bản phác thảo](../Sketches21-30/review.html). Bản v1 để duyệt bố cục và thao tác;
chi tiết tỷ lệ, vách kín, ray/chốt và tiếp xúc cơ khí được giản lược, cần kiểm chứng khi dựng.

- Sơ đồ v0.2: `motor → đai phân nhánh nguồn A/B; [trạm A] ← G cùng một ray → [trạm B]`.
- Tải A kéo nắp chắn ray; tải B kéo cửa cuối. Hai đầu trạm, đai và hai tải đều nhìn được ở toàn cảnh.
- Hộp khoảng 0,70 m; spawn trước-trái sàn bám; Exit vách phải thấp, có hành lang đi rộng phía trước.
- G trượt ngang khoảng 0,16–0,18 m giữa hai đầu chặn A/B; ban đầu G ở nửa trái, chưa khớp A.
- Mỗi trạm có bánh nguồn trên và bánh tải dưới, G nằm giữa khi khớp; đường G chạy ngang không xuyên qua tâm bánh cố định.
- Nắp A là vật đặc chắn ngang hành lang của carriage tại giữa ray; khi đóng, G thực sự va nắp trước khi tới B.
- A nâng nắp khỏi toàn bộ tiết diện G/tay/giá đỡ, bắt chốt trên; không dùng cờ lịch sử hoặc khóa chỉ tồn tại ở renderer.
- Khoang thoát được bao vách kín từ sàn tới nóc; cửa B đặc phủ duy nhất aperture, không có đường bò vòng.
- Tay G rộng 50–60 mm phía trước; nền đứng khoảng 0,12 m; ray và nắp có khe lắp riêng, tránh túi kẹp mô.
- Khóa xoay; camera tĩnh khoảng 36°/12°, fit bounds tĩnh, thấy A/B/nắp/cửa; follow không đổi cơ quan.
- Chạm tay → bám → chạm phía đầu ray → kéo; buông vật/3 giây chỉ kết thúc giữ prop, không xóa catch đã gài.
- Nóc/trơn hợp lệ vẫn nhận đích nhưng không tạo grip. Thử 720×1280/720×1612, safe area và đích hai đầu không bị nguồn che.

## 4. Lời giải, kết thúc và phục hồi

| Bước | Lệnh | Trạng thái / tín hiệu | Bỏ dở hoặc làm ngược |
| --- | --- | --- | --- |
| 1 | Kéo G về trạm A | Nguồn A–G–tải A nối; nắp giữa ray nâng và chốt | Buông giữ G bằng catch; rút sớm phanh giữ tải |
| 2 | Kéo cùng G sang trạm B | Catch A nhả, nắp A giữ mở; nguồn B–G–tải B nâng cửa | Kéo ngược về A vẫn được, không xóa kết quả đã chốt |
| 3 | Buông G, đi tới Exit | Cửa B tới chốt, đường thông thật | Ngắt G sau khi chốt không đóng sập cửa |

- Thắng: đủ 32 hạt của cơ thể đã hợp nhất bên trong qua `FinalExit`; không có `Transfer`.
- Đi B trước bị nắp A chặn carriage bằng tiếp xúc thật. Không dùng kiểm tra “đã hoàn thành A” để từ chối lực kéo.
- Có đủ khoảng chờ nắp mở; không cần kéo đúng lúc. Nếu khe mở một phần đã đủ cho G qua thật, coi đó là trạng thái hợp lệ cần kiểm chứng.
- Rút G giữa lúc một tải chạy: phanh giữ tải, trở về trạm tương ứng để tiếp tục; không tự dịch G hoặc tự chọn trạm kế.
- G không rời ray; mọi vị trí còn chạm được tay, cả hai chặn là chỗ ổn định. Kẹt vào nắp thì kéo lùi được.
- Reset trả G/nắp/cửa/pha/catch/phanh/lệnh về gốc; pause dừng mô phỏng; save cũ không đổi.
- Không dao; nếu thử nhiều phần vẫn tự tụ khi gần/không cản, không cooldown; thoát chưa tụ dùng lỗi chuẩn.

## 5. Cơ quan và kiến trúc dùng lại

| Cơ quan | Lực / hành trình ước lượng | Trạng thái, phụ thuộc và reset |
| --- | --- | --- |
| G / `COgheRailSlider` | Ngang 0,16–0,18 m, cản 0,03–0,06 N | Chốt ở cả hai đầu; kéo ngược nhả; reset nửa trái |
| Motor/đai + bộ truyền hai nhánh | Motor hiện rõ, mô-men dự kiến 0,03–0,04 N·m | Trạm có pitch contact mới nhận công; G chỉ một pose/quay |
| Tải A/B / hai rack | A nâng khoảng 0,13 m, B khoảng 0,12 m | Phanh giữ giữa chừng, catch cuối; reset cả hai đóng |

- Driver `COgheDualDockTransmission` dùng `COgheGearTrain.PitchContact`, một nơi duy nhất điều khiển góc G và phân tải. Lực kéo hữu hạn theo tải/ray, tối đa 0,8 N; chưa mô phỏng va chạm từng răng.
- Không gắn hai COgheGearTrain cùng ghi Transform G: chúng sẽ tranh quyền quay. Bộ truyền mới phải có test mất/đổi tiếp xúc và giới hạn mô-men chung.
- Đai nguồn cố định hiện rõ cấp hai nhánh; gear lý tưởng không đồng nghĩa nguồn lực vô hạn. Phanh/catch đọc trạng thái thật, không animation mở.
- Dung sai pitch dự kiến 2–3 mm; end catch và vùng chạm rộng giúp thao tác, không “ăn khớp từ xa”.
- Route đổi theo nắp/G/cửa; giữ mass/ID mô, không if-level tự mở tải hoặc thực hiện thứ tự thay người chơi.

## 6. Hình ảnh, animation và phản hồi

- Day Lab 07; G trên ổ hổ phách, hai nhánh bằng trục/đai bạc mảnh, nhãn A/B chỉ là tên trạm.
- Chạm → bám → dịch G → bánh tại trạm quay → tải tương ứng nâng; nắp A đã chốt vẫn ở trên khi G đi khỏi.
- Không vẽ hai bánh G hoặc vệt nối giả; toàn cảnh phải đọc được việc dùng lại một vật. Thắng dùng animation hiện có; ảnh Unity nằm trong báo cáo kiểm chứng v3.

## 7. Độ khó và ngân sách runtime

- Dự kiến 2 quyết định tuần tự, 1 cơ thể; mục tiêu thử 2–4 phút, không timing/tilt chính xác hay giữ đồng thời.
- Một carriage, hai rack và một driver chung; đo chi phí contact/route lúc đổi trạm, không tự thêm hàng chục collider răng.
- Ngân sách collider/skin/GC/GPU chưa có dữ liệu mới; dùng kit/đèn Day Lab, không suy FPS từ số vật.

## 8. Chơi thử và hồi quy

- Full solution A→B bằng cùng một bánh G đã qua bằng thao tác thật. B trước A, rút G giữa lúc nâng và playtest mobile vẫn là checklist hiệu chỉnh.
- Bắt buộc kiểm tra một wheel chỉ có một nguồn ghi pose, torque chia đúng, không khớp cả hai trạm cùng lúc, mất truyền vẫn phanh tải.
- Thử bò vòng aperture kín, tay G ở sát nắp/hai đầu, kéo lâu/buông; không teleport hoặc force contact trong full solution.
- Hồi quy rail/gear/17/20; playtest người mới ghi thời gian, có hiểu tái dùng G không, nhầm trạm/chạm và nhu cầu gợi ý.

## 9. Bằng chứng hiệu năng trên thiết bị

- Chưa có máy/OS/build/SHA/raw log, p50/p95/p99/max, spike, CPU/GPU/GC/memory hay phiên nhiệt.
- Đo OPPO được xác nhận cùng baseline/profile 32 hạt/120 Hz; 60 FPS là mục tiêu, ngân sách p95/p99 phải chốt trước nghiệm thu.
- Kịch bản đổi A↔B và ngắt truyền khi nâng, kèm frame chạm và phiên 15–20 phút; lưu commit/diff/cấu hình thực.

## 10. Tích hợp, tương thích và nghiệm thu

- Slot 21 đã tích hợp; ID 27 và ID cũ 17 độc lập. Catalog/build/next và driver phân nhánh đã triển khai.
- Không thưởng Nhà; kiểm tra save cũ, replay/lưu idempotent khi thêm scene. Lời giải tác giả không đưa vào runtime.
- Trạng thái v3: prototype đã triển khai và có kiểm thử cơ quan/đường giải. Xem báo cáo mới nhất để phân biệt kết quả gameplay, hình ảnh và hiệu năng; chưa chứng nhận FPS mobile cho lần sửa này.
