# COghe — venom.origin.29 — Bạn giữ, tớ kéo

## 1. Định danh, phạm vi và trạng thái

- ID ổn định `venom.origin.29`; vị trí hiển thị **26**, sau Boss cũ 10 ở vị trí 20.
- Prototype Unity v2, 17/09/2026; scene, definition, builder, dao, nút A và tời B đã tồn tại.
- Origin hiện hành, 32 hạt/120 Hz; chuẩn bị phối hợp ở màn cũ 19, không dạy thêm kiểu điều khiển.
- Yêu cầu đã có: độ khó vừa, chạm rộng, phân chia không phải đúng tỷ lệ; chưa chốt số lực/kích thước bằng chạy thật.
- Hợp đồng cơ quan, reset/idle và ngân sách mô phỏng đã qua test macOS; chưa playtest tay mobile.
Nguồn: [quy tắc level](../../../COGHE_LEVEL_DESIGN_RULES.md), [template](../LEVEL_TEMPLATE.md), [Day Lab](../../../ArtDirection/COghe/STYLE_RULES.md), [kiến trúc](../../../COGHE_EXPANSION_11_20.md).

## 2. Mục tiêu trải nghiệm và tiến trình

- Khám phá một phần duy trì ly hợp, phần còn lại kéo; khi cửa gài chốt thì cả hai được tự do hợp thể.
- Đã biết dao, chọn phần, giữ nút và hợp thể từ Boss cũ 10; mới là gắn giữ nút với kéo ngắn thay cho hai nút.
- Chỉ hai vai trò đồng thời, không bánh răng/ống/chia ba hoặc đổi góc hộp.
- Hướng dẫn cơ bản “Chạm phần để chọn; phần khác tiếp tục việc đang làm”; không bảo vệ mảnh giữ nút khỏi hợp thể.
- Không nâng lực/buff Nhà; thành công chỉ ghi hoàn tất màn, không mở khóa vĩnh viễn mới.

## 3. Phác thảo, hình học, camera và thao tác

![Phác thảo 29 · Bạn giữ, tớ kéo](mockup-v1.png)

[Xem cả 10 bản phác thảo](../Sketches21-30/review.html). Bản v1 để duyệt bố cục và thao tác;
chi tiết tỷ lệ, vách kín, ray/chốt và tiếp xúc cơ khí được giản lược, cần kiểm chứng khi dựng.

- Sơ đồ v0.1: `Spawn + dao rộng → A giữ ─ly hợp/cáp→ B kéo → cửa có chốt → vùng tụ → Exit`.
- Một buồng thao tác rộng khoảng 0,75 m; spawn trước-giữa, dao phía trước; A bên trái, B bên phải, cửa phía sau-phải.
- A và chỗ bám tay B cách khoảng 0,38–0,45 m bằng đường bám rộng; khoảng cách cần kiểm chứng với giới hạn kéo giãn và tether thật.
- Khoang thoát có vách kín từ sàn tới nóc; cửa đặc là lối duy nhất, không bò vòng trên ray/cáp hoặc qua đầu vách.
- Dao nằm trong vùng đứng rộng, cảnh báo 1 giây như runtime; điểm đặt đích lớn, không yêu cầu chia đều hay căn đúng khoảnh khắc.
- A rộng khoảng 0,10×0,10 m; B có tay 60 mm; vùng tụ trước cửa khoảng 0,18×0,18 m, cách A đủ để giữ hai phần chưa chạm nhau.
- Khóa xoay; camera tĩnh khoảng 40°/12°, toàn cảnh thấy dao/A/B/cửa; fit bounds tĩnh, follow không đổi cơ quan.
- Chọn phần qua mô hoặc nút; chạm A tạo lệnh giữ, đổi chọn không bỏ A. Chạm B rồi chỉ đích tạo lực kéo thực.
- Buông vật/3 giây không lệnh chỉ buông B; **không hủy lệnh giữ A**. Nóc/trơn vẫn nhận đích hợp lệ, không có lực bám giả.
- Kiểm tra 720×1280/720×1612, safe area và mọi đích không bị dao/khung che; không dùng nhãn nhìn xuyên nắp.

## 4. Lời giải, kết thúc và phục hồi

| Bước | Lệnh | Trạng thái / tín hiệu | Bỏ dở hoặc làm ngược |
| --- | --- | --- | --- |
| 1 | Đi vào vùng dao, tách | Một thân → các phần thực, xung tách cân bằng | Né dao là hợp lệ; quay lại sau khi vùng cảm biến trống |
| 2 | Giao một phần giữ A | Tải A đóng ly hợp, đèn A sáng và tay B nhả phanh | Rời A/tụ sớm thì ly hợp nhả, phanh giữ cửa |
| 3 | Chọn phần còn lại, kéo B | Tời có nguồn chạy, cửa tới chốt | Buông B giữa đường dừng tải; bám lại tiếp tục |
| 4 | Buông B, đưa hai phần về vùng tụ rồi Exit | Tụ tự nhiên; cửa chốt vẫn mở | Phần còn giữ A không tự bỏ việc, người chơi gọi về |

- Thắng chỉ khi hợp nhất bên trong rồi đủ 32 hạt qua `FinalExit`; không transfer. Ra trước tụ: “bạn phải hợp thể trước khi chui ra”.
- Ngưỡng thử A khoảng 12–18 g, lực yêu cầu B khoảng 0,06–0,09 N: mục tiêu cả chia 1/4–3/4 lẫn đảo vai đều dùng được; chưa chứng minh.
- Không cho một thân bỏ A rồi chạy B: ly hợp nhả ngay khi mất tải, không delay quyền kéo. Phanh giữ vị trí cửa, không tích lũy mở khi thiếu tải A.
- **Ca chặn nghiệm thu:** một cơ thể kéo dài vừa đè A vừa kéo B. Phải chứng minh không đủ tầm/lực với hình học thật; không thêm kiểm tra group-ID làm khóa giả.
- Nếu một thân làm được, tăng khoảng cách/đổi hành lang và đo lại; hồ sơ chưa tuyên bố đã ngăn bypass chỉ bằng khoảng cách nêu trên.
- Cắt lệch tạo mảnh quá nhỏ: hợp lại tại vùng rộng rồi cắt lại; cắt thêm không hất mảnh đang giữ A. Gần/không cản luôn tự tụ, không cooldown.
- Reset trả dao/cửa/chốt/pin/tải/lệnh/vận tốc và mô về gốc; pause dừng mô phỏng; cửa không sập khi người chơi đổi chọn.

## 5. Cơ quan và kiến trúc dùng lại

| Cơ quan | Lực / hành trình ước lượng | Trạng thái, phụ thuộc và reset |
| --- | --- | --- |
| Dao / `COgheGuillotine` | Cảnh báo 1 giây, cắt liên kết qua lưỡi thực | Ready→Warning→Falling→Returning→AwaitClear; reset cao |
| A / `COgheTissueSensor` | Tải 12–18 g, vùng rộng | Ly hợp chỉ đóng khi có tải; reset 0 tải, không chốt |
| B + `COgheCooperativeWinch` | Tay 40–60 mm, cửa khoảng 0,10 m; motor/tời hiện rõ | A có tải + lực B mới nâng; phanh dừng tải, cửa cuối chốt |

- Nguồn công là motor có thân/trục quay; A đóng ly hợp, B là cần vận hành có lực hữu hạn, không giả vờ một hành trình ngắn tự cấp vô hạn năng lượng.
- Dùng winch không Output/Transmission/GearCarriage, một Doors; clearance và nhịp chốt vẫn phải đo trong bố cục mới. `LockPin` của winch hiện chỉ phản ánh `Complete`, không dùng nó làm vật chặn tay B theo tải A; đèn/ly hợp đọc sensor và trạng thái power thật.
- Không dùng nguyên `DoorSpeed=.025` cho cửa 0,10 m: riêng thời gian chạy đã ít nhất 4 giây, vượt timeout bám 3 giây. Mục tiêu cấu hình là xong một lượt kéo trong khoảng 2,5 giây kể từ bám, có dư để tăng tốc. Thử `DoorSpeed=.05` với tải/lực hữu hạn và hành trình phù hợp; đây là giá trị bắt đầu cần đo, không tăng lực sinh vật hoặc bỏ timeout riêng cho màn. Chưa đạt thì sửa tải/tỉ số/hình học, không bắt người chơi spam lệnh để giữ B.
- Route theo cửa/dao; giữ ID/khối lượng và các lệnh nhóm chưa bị cắt. Không nhánh theo số màn, không khóa nhập theo tác vụ.

## 6. Hình ảnh, animation và phản hồi

- Day Lab 07: dao bạc sạch, A nắp hổ phách, cáp/ly hợp đọc được, không tăng trang trí thành lời giải tự chạy.
- Chạm → chọn mảnh → bám/giữ hoặc kéo → tải/cửa phản hồi thật; đèn A tắt ngay khi mất tải, cửa chốt khác rõ cửa đang được nâng.
- Hợp thể và ăn mừng hiện có; chưa có ảnh Unity/mobile. Giữ icon chọn phần rõ ở cả A và B.

## 7. Độ khó và ngân sách runtime

- Dự kiến 3 quyết định sau cắt, 2 vai trò, không timing; mục tiêu thử 3–5 phút, không cần chia tỷ lệ chính xác.
- Dao, tay kéo, cửa, sensor/tời và 2–3 nhóm mô; điểm nặng là split/fuse/graph/cửa và đổi phần, chưa đo.
- Chưa chốt ngân sách collider/skin/GC/GPU; 32 hạt/120 Hz giữ nguyên, không giảm vật lý theo thiết bị để đạt FPS.

## 8. Chơi thử và hồi quy

- Tuyến dao–giữ A–kéo B–tụ lại–thoát đã qua full-solution test. Các tỷ lệ cắt khác, đổi vai và playtest mobile vẫn là checklist hiệu chỉnh.
- Bắt buộc một thân cố kéo giãn/bỏ A rồi chạy B, bò vòng cửa, cắt lần hai, nhiều mảnh, nhỏ hơn ngưỡng, thoát chưa tụ và reset giữa cắt.
- Hồi quy cắt/tụ/chọn phần/tời màn cũ 10/19/20; input thật trên điện thoại, không teleport hay ép ngưỡng trong lời giải.
- Người mới ghi chạm sai phần, tưởng A tự bỏ sau 3 giây, thời gian/do dự/retry; chưa có số người/lượt hoặc dữ liệu.

## 9. Bằng chứng hiệu năng trên thiết bị

- Chưa có máy/OS/build/SHA/log, p50/p95/p99/max/spike/CPU/GPU/GC/memory/nhiệt; chưa chứng nhận khả giải hoặc FPS.
- Đo OPPO đã xác nhận cùng baseline/profile; 60 FPS là mục tiêu, ngân sách p95/p99 cần chốt trước nghiệm thu.
- Đo cắt→giữ→kéo→tụ→thoát và phiên 15–20 phút; ghi resolution/safe area/build type/commit/diff/warm-up/thời lượng thực.

## 10. Tích hợp, tương thích và nghiệm thu

- Slot 26 chỉ là đề xuất; giữ nguyên ID Boss cũ 10 và save cũ, đăng ký ID 29 riêng; scene/build/catalog/next chưa đổi.
- Không mở Nhà thêm. Kiểm tra tiến trình/replay/tải luân phiên và lưu idempotent; không đưa lời giải vào runtime.
- Kết luận Nháp v0.1; chặn nghiệm thu ưu tiên là một-thân-bypass và dung sai khối lượng, sau đó input, phục hồi và hiệu năng.
