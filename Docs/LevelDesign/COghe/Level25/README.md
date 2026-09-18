# COghe — venom.origin.25 — Hai nhịp một cửa

> **Rà soát v3 — 18/09/2026, màn hiển thị 16:** đã sửa hình học và kiểm chứng lại
> đường giải. [Báo cáo và ảnh Unity](../../../Verification/COgheCampaign30/review-14-16-18.md)
> ghi trạng thái hiện hành; các ước lượng/Nháp bên dưới là lịch sử phác thảo.
> Chưa kiểm thử lại trên OPPO ở lượt này.

## 1. Định danh, phạm vi và trạng thái

- ID ổn định `venom.origin.25`; vị trí hiển thị **16**, trước màn cũ 13; không đổi ID màn cũ.
- Prototype Unity v2, 17/09/2026; scene, definition và builder đã tồn tại.
- Origin hiện hành, 32 hạt/120 Hz; chuỗi nắp A → tay B → cửa cuối và reset đã qua test macOS.
- Người dùng yêu cầu thêm màn vừa sức, dễ điều khiển mobile; kích thước dưới đây chưa chốt bằng prototype.
- Chưa playtest tay trên mobile; kích thước và độ khó vẫn cần hiệu chỉnh từ người chơi thật.
Nguồn: [quy tắc level](../../../COGHE_LEVEL_DESIGN_RULES.md), [template](../LEVEL_TEMPLATE.md), [Day Lab](../../../ArtDirection/COghe/STYLE_RULES.md), [kiến trúc](../../../COGHE_EXPANSION_11_20.md).

## 2. Mục tiêu trải nghiệm và tiến trình

- Nhận ra một hành động có chốt tạo điều kiện cho hành động kế; không phải giữ hai nút bằng một cơ thể.
- Đã biết chạm vật rồi chỉ đích đẩy/kéo; tổ hợp mới là hai thao tác ngắn nối tiếp, chuẩn bị màn cũ 13.
- Khoảnh khắc nhận ra: cửa nhỏ A ở lại vị trí mở, tay nắm B lộ ra phía sau.
- Màn luyện tập; hướng dẫn cơ bản “Chạm tay nắm, rồi chạm phía muốn kéo”; không vẽ đường giải lên HUD.
- Không kỹ năng trả phí, nâng lực, buff Nhà hay phần thưởng vĩnh viễn mới.

## 3. Phác thảo, hình học, camera và thao tác

![Phác thảo 25 · Hai nhịp một cửa](mockup-v1.png)

[Xem cả 10 bản phác thảo](../Sketches21-30/review.html). Bản v1 để duyệt bố cục và thao tác;
chi tiết tỷ lệ, vách kín, ray/chốt và tiếp xúc cơ khí được giản lược, cần kiểm chứng khi dựng.

- Sơ đồ v0.1, chưa theo tỷ lệ: `Spawn → A [khoang tay nắm B] → B ─cáp→ cửa cuối → Exit`.
- Hộp khoảng 0,60 m; spawn trước-trái trên sàn bám; lỗ cuối ở vách phải, thấp và nhìn thấy.
- Một vách liền từ sàn tới nóc bao khoang thoát; lỗ duy nhất bị tấm cửa B đặc che kín lúc đầu.
- A là nắp trượt ngang của hốc nông trên sàn; hốc có đáy, hai bên và mái thật để không bò vòng lấy B.
- A và B có tay nắm thấp 50–60 mm, vùng đứng bám khoảng 0,12 m; khoảng hở/cạnh chuyển mặt phải đo với mô.
- Khóa xoay; camera tĩnh 3/4 dự kiến pitch 35°/yaw 15°, thấy cả hai tay nắm khi chúng thực sự được lộ.
- Fit bounds tĩnh; follow chỉ đổi góc nhìn. Thử 720×1280, 720×1612 và safe area, không chọn xuyên nắp A.
- Chạm A/B → tiếp cận và bám → chạm đích trên sàn → lực kéo; chạm sàn khi đã buông → bò.
- “Buông vật” hoặc 3 giây không lệnh chỉ hủy thao tác prop; không reset chốt. Nóc/trơn hợp lệ vẫn nhận đích, không tạo bám.

## 4. Lời giải, kết thúc và phục hồi

| Bước | Lệnh | Trạng thái / tín hiệu | Bỏ dở hoặc làm ngược |
| --- | --- | --- | --- |
| 1 | Kéo A tới đầu ray | Hốc đóng → mở, chốt A gài thấy được | Buông sớm dừng tại chỗ; chạm lại để tiếp tục |
| 2 | Buông A, tới B và kéo | B kéo cáp nâng cửa, chốt cửa bắt ở cuối | Phanh tải giữ phần hành trình; kéo tiếp, không phải chạy đua |
| 3 | Buông B, chỉ vào lỗ | Toàn thân đi qua cửa thật | Có thể quay về trước khi bắt đầu thoát |

- Thắng chỉ khi một cơ thể hợp nhất bên trong rồi toàn bộ 32 hạt qua `FinalExit`; không có `Transfer`.
- Không dao/nút giữ ở màn này; nếu fixture tạo phần rời, vẫn dùng lỗi “bạn phải hợp thể trước khi chui ra”.
- Kéo B trước: nắp A chặn tiếp cận thật. Không dựa riêng vào cờ “đã làm bước 1”.
- Kéo A ngược được nhưng COghe/vật chắn làm nắp dừng vật lý; không ép nắp xuyên người hay khóa người trong hốc.
- Không có góc nhét vật: hai tay nắm trên ray có chặn; lùi A/B hoặc kéo tiếp đều sửa được, Retry luôn sẵn.
- Pause ngừng mô phỏng; retry/tải lại trả cả ray, cáp, chốt, vận tốc và lệnh về gốc; giữ save chiến thắng đã có.
- Tách chỉ qua dao; gần và không cản thì tự tụ, không bảo vệ tác vụ hoặc cooldown nếu thử nhiều phần.

## 5. Cơ quan và kiến trúc dùng lại

| Cơ quan | Lực / hành trình ước lượng | Trạng thái, phụ thuộc và reset |
| --- | --- | --- |
| A / `COgheRailSlider` | Ngang 70 mm, lực cản 0,03–0,05 N | Cuối ray chốt; kéo ngược nhả; reset đóng |
| B / tay kéo + cửa trên ray | Tay 60–80 mm, cửa nâng khoảng 0,12 m | Cáp/ròng rọc thật về bố trí, lực hữu hạn từ mô; reset hạ cửa |
| Phanh/chốt tải | Truyền lực qua tỉ số ròng rọc cố định | Buông giữ tải; kéo ngược có kiểm soát, chốt cuối giữ cửa |

- Dùng rail/catch có sẵn; truyền tay kéo–cáp cần component dùng lại hoặc cấu hình tời, **chưa tuyên bố đã hỗ trợ trực tiếp**.
- Không motor vô hình: COghe cấp công; hành trình/tỉ số kéo phải đủ nâng tải trong vài lần chạm rộng.
- Cập nhật route khi A/cửa B chuyển tương đối; không đổi lực, thắng hoặc nhập theo số màn.

## 6. Hình ảnh, animation và phản hồi

- Day Lab màn 07: nắp hổ phách, ray bạc, sàn bám xanh nhạt; không dùng trang trí thay collider kín.
- Vòng chạm → xúc tu bám → thân kéo → chốt bật/đèn đọc pose thật; bị chắn thì cơ cấu dừng, không giả báo thành công.
- Đường cáp và phanh nhìn thấy; nắp mở không che B. Thắng dùng camera/ba hoạt ảnh hiện có; chưa có ảnh Unity.

## 7. Độ khó và ngân sách runtime

- Dự kiến 2 quyết định tuần tự, 1 cơ thể/1 thao tác chủ động; không timing hay căn góc, mục tiêu thử 1–3 phút.
- Hai ray và truyền tải ngắn; query/graph tăng khi cửa động; kính chỉ lớp cần thiết, một đèn có bóng theo Day Lab.
- Ngân sách collider/GC/GPU chưa đo; không suy ra FPS từ số vật. Ưu tiên đo frame nhận lệnh và nâng cửa.

## 8. Chơi thử và hồi quy

- Tuyến chuẩn A→B→lỗ đã qua full-solution test bằng chọn tay và kéo thật. Các ca B trước A, buông giữa hành trình và playtest mobile vẫn là checklist hiệu chỉnh.
- Bắt buộc thử bò tường/nóc/lách mép khi cửa đóng; nếu qua được thì sửa collider, không thêm khóa thắng vô hình.
- Hồi quy rail/catch/tời, màn cũ 13/19/20 nếu dùng chung thay đổi; không teleport hay đặt cửa mở trong full solution.
- Playtest người mới: ghi số người/lượt, chạm nhầm, thời gian và có hiểu chốt tồn tại sau buông không; hiện chưa có dữ liệu.

## 9. Bằng chứng hiệu năng trên thiết bị

- Chưa có device/build/SHA/log; chưa đo p50/p95/p99/max, spike, CPU/GPU/GC/memory hoặc nhiệt.
- Đo OPPO mục tiêu sau khi xác nhận model/OS, cùng profile 32 hạt/120 Hz; 60 FPS là mục tiêu, chưa đạt được chứng minh.
- Ghi cấu hình/baseline và thao tác A→B→thoát; thêm phiên 15–20 phút, chốt ngân sách p95/p99 trước nghiệm thu.

## 10. Tích hợp, tương thích và nghiệm thu

- Chỉ đề xuất slot 16; scene/build/catalog và điều hướng theo thứ tự mới chưa sửa. ID `venom.origin.25` không thay ID 13.
- Save cũ giữ nguyên; mapping vị trí mới cần kiểm tra tải/replay/next và lưu idempotent; màn này không mở Nhà.
- Kết luận: hồ sơ Nháp v0.1; còn phải dựng, kiểm chứng truyền cáp/phanh, chạm màn hình, phục hồi và hiệu năng.
