# Bàn 16 — hai bi phối hợp

Sáu ảnh 796 × 1494 từ một lượt mô phỏng liên tục trong Unity Play Mode, bắt đầu **04:19:55 UTC ngày 09/09/2026**. Chỉ gửi lệnh xoay hộp sau khi đặt hai bi tại spawn. Play Mode được dùng để nút nhận callback va chạm thật; không ép trạng thái nút/cửa.

| Ảnh | Thời gian mô phỏng | Trạng thái |
| --- | ---: | --- |
| [Xuất phát](Coop16Start.png) | 1,000 s | Hai bi trong hai khoang, cửa đóng |
| [A đang giữ](Coop16Hold.png) | 1,833 s | A nén nút; cửa bắt đầu đáp ứng, chưa thông hoàn toàn |
| [B qua cửa](Coop16Cross.png) | 3,433 s | A vẫn tì nút, cửa cyan thông và B đi qua |
| [B đã chốt](Coop16Retained.png) | 8,083 s | Hai cửa giữ mở, A không cần tiếp tục nhấn |
| [Một bi ra ngoài](Coop16OneOut.png) | 19,508 s | A vừa thoát, B còn trong hộp; chưa thắng |
| [Cả hai đã thoát](Coop16BothOut.png) | 27,833 s | B vừa thoát; A đã rơi khỏi khung hình từ trước |

[Metadata](Coop16Evidence.json) ghi vị trí hai bi, tư thế hộp, hành trình nút, độ thông cửa và bộ đếm ở từng ảnh. Đây là fixture điều khiển tự động, không phải lượt chơi tay hoặc thời gian giải của người chơi. Không suy diễn độ khó từ tốc độ của policy test.

Tạo lại: Unity `-batchmode -projectPath /Users/mrk/GravityBox -executeMethod GravityBox.Editor.CooperativePreviewCapture.Capture -logFile /Users/mrk/GravityBox/Artifacts/coop16-render.log`, có graphics, không truyền `-quit` vì utility tự thoát sau Play Mode.

[Ảnh macOS](MacOSLevel16.png) chụp qua F12 lúc **04:29:04 UTC**, từ bản build lúc 04:27:05 UTC. Đã chọn màn 16 và reset về hai spawn, quan sát title, hai bi, gờ/cửa và bộ đếm `OUT 0/2`. Ảnh native xác nhận hiển thị bản mới, không phải bằng chứng giải màn bằng tay.
