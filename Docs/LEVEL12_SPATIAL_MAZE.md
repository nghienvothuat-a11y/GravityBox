# Bàn 12 — Lost in glass

Các ván kính nhỏ được ghép thành một mê cung liên tục trong lòng cầu. Bi xuất phát bên trong khung, phải đi qua các khúc ngoặt theo cả ba trục rồi ra lỗ tròn trên vỏ. Những khe hở nhìn xuyên giữa các ván nhỏ hơn đường kính bi; vì vậy không thể rơi ra ngoài khung rồi lăn sát vỏ cầu để đi tắt.

Bố cục này thay thế bản 32 ván rải rác. Phiên native của bản cũ hoàn thành chỉ trong 16,305 giây; [bằng chứng cũ](Archive/Verification/SphereFreePlanks/README.md) được giữ riêng.

## Bố trí không gian

| Thành phần | Kích thước / vai trò |
| --- | --- |
| Vỏ cầu | Bán kính trong 0,360 m, dày 0,006 m |
| Lưới vị trí các nút | 3 × 3 × 3, khoảng cách 0,174 m |
| Tuyến chính | 25 đoạn, 21 khúc đổi hướng theo cả X/Y/Z |
| Nhánh cụt | Hai nhánh, phải đổi hướng để quay lại |
| Tiết diện lối đi | 50 mm, cho bi thép đường kính 30 mm |
| Các ván ghép | Dày 3 mm, rộng 10 mm; được chia thành mảnh ngắn |
| Khe nhìn giữa ván | 15 mm, không đủ cho bi lọt qua |
| Lỗ cuối | Bán kính 23 mm, khoét phẳng tại cực −Y local của vỏ |

Mỗi ngã rẽ được ghép từ những ván nhỏ và có mặt mở theo đúng các đoạn nối. Các hướng còn lại có ván chặn thật. Đây là mạng đường đi trong ba chiều, không phải nhiều mê cung phẳng xếp tầng; không có sàn lớn phủ toàn bộ cầu hoặc lỗ tròn chuyển tầng. Những đoạn chạy theo chiều sâu cũng trở thành đường rơi khi người chơi xoay cầu.

Các ván mỏng tạo khe nhìn để giữ tầm nhìn vào phía trong. Vật liệu alpha nhẹ và viền mảnh giúp đọc hình dáng từng đoạn. Khe quan sát có kích thước cố định; độ trong suốt không làm thay đổi va chạm.

## Cơ chế vật lý

Bi là Rigidbody tự do, 30 mm/~111 g, nhận gravity thế giới `(0, −9.81, 0)` ở 120 Hz. Toàn bộ khung và vỏ gắn vào root kinematic mà người chơi xoay. Khi đổi mặt đỡ, viên bi lăn, rơi và va chạm với mặt tiếp theo bằng cùng mô hình vật lý của những bàn khác.

Không có lực hút, ray vô hình, điều khiển bi theo waypoint hoặc chuyển vị trí giữa các nút. Mesh va chạm được ghép từ chính những hộp mỏng dùng để dựng hình các ván. Không có một khối collider kín thay thế cho những khe đang nhìn thấy. Các phép thử dùng toàn bộ bán kính bi để phân biệt khe nhìn và lối đi thật.

Đoạn cuối nối khung với cửa thoát trên vỏ. Viền sáng mảnh không có collider và không tạo gờ; ExitSocket chỉ phát thắng khi toàn bộ bi đi từ trong qua tiết diện tròn và vượt mặt ngoài. Bi tiếp tục được mô phỏng sau đó.

## Kiến trúc và kiểm chứng

`SphereMazeBuilder` tạo mạng nút/đoạn nối và các ván tại Editor. `SphereMazeGeometry` lưu mesh vỏ, các đoạn khung và nét viền. `SpatialMaze` lưu `NodesLocal`, `Edges`, `MainPath`, `JunctionColliders`, descriptor `Planks` và các kích thước. Geometry được lưu vào prefab; không sinh mê cung hoặc chạy lời giải trong player.

`ContentValidator` kiểm tra liên thông, route qua các cạnh thật, tham chiếu collider, kích thước lối đi/khe và giới hạn vỏ. PlayMode kiểm tra đường đi bằng collider thực, các phía bị chặn, route từ spawn bằng ý định xoay và reset/unload. Bằng chứng cuối cùng, phép kiểm tra đường tắt và giới hạn của chúng được ghi trong [Verification](Verification/README.md).

Nhiều khúc ngoặt và nhánh cụt tạo yêu cầu xoay sở rõ ràng hơn bản ván rải rác. Mức “siêu khó” vẫn phải được đánh giá bằng lượt chơi thật, thời gian, số lần thử và khả năng đọc chiều sâu; một policy kiểm thử thành công chỉ chứng minh có đường vật lý.
