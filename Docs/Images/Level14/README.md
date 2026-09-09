# Bàn 14 — hình ảnh thủy ngân

Ba ảnh từ prefab thật, render lúc **03:10:54 UTC ngày 09/09/2026**. Bắt đầu ở spawn và mô phỏng liên tục; chỉ xoay hộp, không ghi lại pose/vận tốc bi sau spawn. Đã xem cả ba ảnh và kiểm tra shader.

- [Đang nổi lên](Mercury14Rising.png): sau 0,1 s, tâm bi y=−0,004419 m, từ spawn y=−0,024 m.
- [Áp vào nắp](Mercury14Ceiling.png): sau 1 s, tâm bi y=0,027000 m, cách mặt trong nắp đúng bán kính bi.
- [Xoay hộp](Mercury14Turned.png): sau khi đổi góc, bi tiếp tục đi về mặt cao theo world gravity; tracer đọc được chuyển động.
- [Metadata](Mercury14RenderFixtures.json): vị trí bi và số wake 4/2/19 ở các thời điểm.

Đây là chế độ nhìn xuyên hỗ trợ quan sát chất lỏng đục, không phải mô phỏng thủy ngân trong suốt. Không có caustic của nước. Hình ảnh không thay thế test lực hoặc lượt chơi tay.

Lệnh tạo lại: Unity batchmode **có graphics**, `-executeMethod GravityBox.Editor.WaterPreviewCapture.CaptureMercury`.
