# Boss 32 — Hai nửa trên trần, relay

Content `G5`, stable ID `coghe.spatial.plus.g5`, revision 15/10/2026.
Giữ Glass C, máy Q 50/50 và tự tụ khi gần nhau. Không hướng dẫn đáp án ở Boss.

## Trải nghiệm và chuỗi giải

Hai phần cần **bàn giao việc giữ đường**, sau đó nhập lại bên trong để thoát.
1. Q chia đôi. Phần thứ nhất giữ P dưới sàn, nâng cầu trần.
2. Phần thứ hai leo góc kính trái sau, qua cầu trần đến kệ phải, kéo B.
3. Phần thứ hai đứng lên D trên kệ phải. D tiếp quản cầu; B **và** D cùng mở chốt C.
4. Phần thứ nhất bỏ P, kéo C dưới sàn, phía trước giữa hộp. C mở chốt A trên kính sau phía trái.
5. Phần thứ nhất leo kính kéo A. A giữ cầu mở ổn định và mở lỗ cuối.
6. Hai phần xuống sàn, tụ lại. Xoay camera nhìn từ sau để thấy lỗ dưới kệ phải,
   rồi chỉ vào lỗ thoát.

P/D là cảm biến tải thực. Cần giữ D trong lúc người kia làm việc; B riêng không đủ.
Nếu buông D sớm, C khóa lại và cầu thu về. Đứng lại lên D để tiếp tục; không reset cả màn.
A/B/C có thể đảo chiều. Nếu rút A trước khi rời cầu, quay lại D/P để dựng đường.
Không thêm tốc độ, lực bám giả, teleport hay điều kiện theo số màn.
Lỗ chỉ mở sau A; chưa hợp thể mà thoát vẫn thua theo luật chung.

## So với Boss 21

| Trục | Boss 21 (B1) | Boss 32 (G5 mới) |
|---|---|---|
| Cơ thể | Hai phần, rồi cả thân kéo tháp | Hai phần và bàn giao người giữ |
| Giữ đồng thời | Một pha, chốt xong được thả | P hỗ trợ vượt; D tiếp quản; C/A giải phóng |
| Quy hoạch | Xếp tầng trên trước, nhập và kéo dưới | Qua trần, kích B, đổi vai, quay nhánh kính trái, nhập |
| Sửa sai | Đảo khối/Retry | Đứng lại P/D, đảo cần/Retry |
| Độ chính xác | Bến/khối có khoảng dung sai | Giữ bề rộng cũ, pad 95mm; không tăng căn chạm |

Mục tiêu tác giả: tăng suy luận và phối hợp so với B1. Cần playtest người mới để
xác nhận độ khó thực tế; số bước replay không phải thời gian giải của người chơi.

## Bố trí và ngân sách

Hộp cũ 0.8 × 0.6m; cầu trần cũ, kệ phải và tay gạt trên kính trái. Lỗ ở chân kính sau
bên phải, cách đường đi xuống từ A: mở A không được hút nhầm nửa thân trên đường
quay về hợp thể. Kiểm tra nhịp 30/60 FPS đã phát hiện nguy cơ này ở vị trí lỗ cũ.
Nắp lỗ trượt sang trái để có đủ hành trình bên trong hộp, không đâm vào kính phải.
Kệ phải dài 0.50m tách B khỏi D để không nhập nhiệm vụ/đè nút ngoài ý muốn.
C ở giữa sàn, A trên kính sau phía trái; không có kệ chắn lối bò trên kính trái. Mạch in bám mặt thật, nút/cần/đầu ra đồng màu.
Thêm một pad, hai tay nắm, hai chốt; kéo dài kệ phải để không va vào hành trình B. Không thay mô 32 hạt/timestep,
không thêm light/shader hay tìm đường mỗi frame. Kiểm tra mạch và hình portrait.

Author replay: `COgheSpatialPlusScenario.G5`. Kiểm tra sai thứ tự, bỏ người giữ,
quay lại D, Retry, solve 30/60fps theo nhịp Update, wander và đường tắt kính.
[Biên bản](../../../Verification/COgheDifficulty65/README.md).
