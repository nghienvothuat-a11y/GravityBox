# Hỗ trợ thoát ở mọi màn

Theo yêu cầu mới ngày 09/09/2026, cả 14 màn dùng lực hỗ trợ khi bi đã đến gần lỗ. Đây là quy tắc gameplay có chủ ý để giảm độ khó ở bước cuối, không phải lực từ của vật liệu hay tính chất mới của nước/thủy ngân.

Tâm bi vào vùng cầu bán kính **40 mm** quanh tâm miệng lỗ sẽ kích hoạt. Bi được căn vào trục lỗ rồi đẩy ra ngoài qua collider thật. Các giá trị mặc định nằm trên `ExitSocket`: tốc độ căn 0,24 m/s, tốc độ đẩy 0,65 m/s, giới hạn gia tốc hỗ trợ 45 m/s². Sau khi tâm đã vượt mặt ngoài hơn nửa bán kính bi, bổ sung hướng rời ngang 0,35 m/s, hướng ra xa tâm hộp (trục tiếp tuyến dự phòng cho lỗ ở tâm). Nhờ vậy lỗ hướng lên không thả bi rơi ngay trở lại đúng trục lỗ. Đây là vận tốc mục tiêu của bộ điều khiển lực, không phải ghi trực tiếp `Rigidbody.linearVelocity`.

Hai sphere sweep kiểm tra đường căn và lòng lỗ trước khi kích hoạt. Không hút từ ngoài hộp vào, từ xa, xuyên vách mê cung hoặc qua một shutter đang che lỗ. `RequiredChannel`, trạng thái session, collider thật và điều kiện full-sphere clearance vẫn có hiệu lực. Khi đã kích hoạt, giới hạn giữ là 100 mm để tránh lôi bi từ xa nếu người chơi xoay làm nó văng khỏi vùng. Nếu bị vật cản mới chặn trong lúc hút, PhysX tiếp tục giải va chạm; lực không thể bỏ qua collider.

`ExitSocket.Assist.cs` là phần gameplay của `ExitSocket`, triển khai `IForceProvider/IForceStepProvider`. `LevelRuntime.Initialize` đăng ký nó một lần với `EnvironmentForceSystem`, sau môi trường chất lỏng. Mọi màn dùng cùng đường thực thi, không có nhánh riêng cho thủy ngân. Trọng lực, buoyancy, cản và va chạm vẫn chạy; gia tốc hỗ trợ được cộng vào trong fixed step 120 Hz. Không parent bi vào hộp, không teleport, không thay mass hay đánh dấu thắng sớm.

**Chỉ khi toàn bộ bi vượt mặt ngoài lỗ mới thắng.** Sau đó ngừng lực hỗ trợ, bi tiếp tục quỹ đạo thật dưới trọng lực. Reset/disable/khóa cửa/hết session xóa trạng thái và gia tốc hỗ trợ; đổi màn xóa provider cùng các provider cũ. HUD hiện “EXIT ASSIST · DRAWING THE BALL OUT” ngay khi bắt đầu hút, rồi trở lại hướng dẫn của màn khi reset.

Bàn 13/14 đồng thời làm mờ sàn xuống opacity 0,12 khi mặt ngoài quay về camera. Sàn trở về opacity 1 ở hướng nhìn từ trong hộp; chỉ đổi hình ảnh, giữ nguyên va chạm. [Kiểm chứng](Verification/ExitAssist/README.md), [ảnh màn 14](Images/Level14/ExitAssist/README.md).
