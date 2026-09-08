# Bàn 12 — Lost in glass

Bàn thử đặt 32 thanh kính nhỏ, cố định và tách rời trong cùng một khối cầu thủy tinh. Bi được lăn trên thanh, rơi tự do qua khoảng không, chạm thanh khác hoặc thành cầu rồi tiếp tục chuyển động. Không có tầng, phòng kín, đường ống giữ bi hoặc lỗ chuyển tiếp giữa các thanh. Lỗ tròn duy nhất là cửa thoát trên vỏ cầu.

## Bố trí

| Thành phần | Kích thước / vai trò |
| --- | --- |
| Mặt trong vỏ cầu | Bán kính 0,360 m |
| Vỏ thủy tinh | Dày 0,006 m, đường kính ngoài 0,732 m |
| Các thanh | 32 tấm kính nhỏ, chủ yếu dày 0,004 m, nhiều hướng và độ cao |
| Thanh xuất phát | Tâm (0; +0,215; 0) m, kích thước 0,090 × 0,004 × 0,026 m |
| Thanh đón đầu tiên | Tâm (0; +0,070; +0,045) m, kích thước 0,100 × 0,004 × 0,040 m |
| Các thanh khác | 24 thanh trong lòng cầu, 4 thanh ngắn gần vỏ, 2 thanh lệch nhau gần lỗ thoát |
| Lỗ thoát trên vỏ | Bán kính 0,023 m, ở cực −Y local của cầu |

24 thanh trong lòng cầu được đặt bằng seed authoring cố định rồi lưu vào prefab. Chúng phân bố theo vị trí và hướng riêng, có khoảng trống giữa nhau. Vùng giữa thanh xuất phát và thanh đón được để trống để quan sát một cú rơi rõ ràng. Những thanh gần vỏ tác động lên bi đang lăn dọc thành cong; hai thanh gần cửa thoát làm thay đổi cách tiếp cận vùng cuối.

Các thanh cùng gắn vào root kinematic và không rơi khỏi cầu. Bi thép là body tự do độc lập, đường kính 30 mm, khối lượng khoảng 111 g. Độ trong suốt chỉ ảnh hưởng hình ảnh. Mỗi thanh có một BoxCollider hữu hạn đúng kích thước nhìn thấy; phần không gian bên ngoài thanh thật sự trống.

## Chuyển động và điều kiện thắng

Người chơi xoay cầu để thay đổi mặt đỡ và hướng rơi tương đối của bi. Rời mép thanh không có thao tác chuyển vị trí: bi giữ vận tốc đang có và tăng tốc theo trọng lực thế giới. Va chạm tiếp theo với thanh hoặc vỏ quyết định phản xạ, spin và năng lượng còn lại. Có thể bỏ qua một số thanh hoặc tìm đường khác nếu hình học và động lượng cho phép.

Trọng lực vẫn là `(0, −9.81, 0)` trong world space, clock 120 Hz. Input chỉ điều khiển góc xoay root qua giới hạn tốc độ/gia tốc hiện có; không có lực hút về thanh, hỗ trợ đáp xuống, khóa vào đường đi hoặc tăng tốc riêng cho bàn này.

Lỗ thoát là khoảng cắt thật xuyên hai mặt cong của vỏ. ExitSocket đặt giữa hai đường biên mặt cắt, với chiều dày lấy từ mesh. Viền sáng mảnh không có collider hoặc gờ. Chỉ khi toàn bộ bi đi từ trong qua lỗ và vượt mặt ngoài mới thắng; mô phỏng tiếp tục sau đó.

## Metadata và kiểm chứng

`SphereMazeBuilder` tạo các thanh và `SphereMazeGeometry` tạo vỏ tại Editor. `SpatialMaze` lưu bán kính, vỏ, mảng `Planks`, chỉ số thanh xuất phát/thanh đón và seed authoring. Không có đồ thị khoang, danh sách cửa bắt buộc hoặc lời giải chạy trong player. Không dùng chế độ xem tầng của bàn 11.

Bốn nhóm test riêng cho bàn 12 kiểm tra:

- Các thanh nhỏ, trong suốt, cố định, không có collider hành lang ẩn; phần lớn các mẫu trong lòng cầu đủ trống cho cả viên bi và có khoảng rơi giữa hai thanh đầu.
- Bi rời thanh xuất phát bằng cách nghiêng cầu, có một khoảng không tiếp xúc đo được, tăng tốc theo 9,81 m/s² rồi chạm thanh đón. Đây là rơi tự do và đáp bằng va chạm thật.
- Một route từ spawn tới thoát chỉ gửi rotation intent, có contact với thanh và có khoảng bay tự do; theo dõi liên tục vị trí, gravity và body dynamic. Test không buộc bi đi qua mọi thanh hoặc coi các đường khác là sai.
- Reset khôi phục pose vật lý và vận tốc, giữ các thanh cố định, không áp lại rotation cũ ở bước tiếp theo; unload thu hồi cả root và bi, giữ đúng một force target.

Generic tests vẫn kiểm tra vỏ từ hơn 350 hướng radial bằng toàn bộ bán kính bi, silhouette cầu, lỗ cắt thật, spawn, nghỉ, containment, reset, điều kiện thoát và bộ chọn đủ 12 bàn. Fixture rơi/đường đi không thay gravity hoặc viết pose/vận tốc của bi giữa hành trình; các fixture hình học và reset dùng điều kiện đo riêng của chúng.

Bố trí này phục vụ thử nghiệm chuyển động giữa các vật cản rời trong không gian. Số thanh không tự chứng minh mức “siêu khó”; cần chơi trực tiếp để đánh giá khả năng đọc chiều sâu, căn điểm rơi, hồi phục sau khi đi chệch và các đường tắt vật lý. Kết quả test/build và ảnh của đúng phiên bản hiện tại được ghi trong [bằng chứng](Verification/README.md) và [nhật ký](DEVELOPMENT_LOG.md).
