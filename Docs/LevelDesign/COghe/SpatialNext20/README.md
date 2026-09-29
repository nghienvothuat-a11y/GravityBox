# Spatial 11–30 — kế hoạch 20 màn tiếp

> **Cập nhật 29/09/2026 — đã dựng đủ 20 màn** (scene, definition, catalog 01–30, art Glass C) và giải trọn bằng chạm thật trong Unity PlayMode và native Mac ([ảnh, kết quả](../../../Verification/COgheSpatialNext20/README.md)). Bố cục thực tế, khác biệt với hồ sơ, số đo và phần chưa kiểm (OPPO, người chơi mới): **[AS_BUILT_2026_09_29](AS_BUILT_2026_09_29.md)**. Đoạn dưới giữ nguyên nội dung bàn giao thiết kế.

29/09/2026 · **Bàn giao kế hoạch trên `NewGraphic`, chưa triển khai Unity**. Phong cách Glass C / mạch điện mảnh / ray dịu và ràng buộc gameplay được chốt trong bộ bàn giao. Bố cục từng màn vẫn là thiết kế cần chứng minh trong prototype, giữ trạng thái `design-only`. Nối sau Spatial Pilot 01–10, không thay thế catalog V2 cũ. Máy Q chia đôi khối lượng phần vào, lặp lại nếu quay vào; gần nhau tự tụ. Tối đa bốn phần trong lời giải, chưa đặt hard cap cho toàn game.

- **[Bắt đầu trên máy khác: phong cách, ràng buộc, source, thứ tự dựng và kiểm chứng](IMPLEMENTATION_HANDOFF.md)**
- [Mở trang review 20 màn](review.html)
- [Kế hoạch, tiến trình khó, kiến trúc, test và performance](PLAN.md)
- [Hợp đồng cơ quan: Q, khối, dây, ròng rọc, ống](MECHANICS.md)
- [Kiểm tra minh hoạ và giới hạn](ILLUSTRATION_QA.md)
- [Kiểm tra gói source và tài liệu trước bàn giao](HANDOFF_CHECK.md)
- Dữ liệu: `levels.json`; prompt: `Prompts/`; hình gốc được copy vào `Illustrations/`; sơ đồ mốc: `Routes/`; nguồn sinh ảnh: `generation-manifest.json`.

| Hồ sơ | Minh hoạ | Vai trò | Số phần hữu ích |
| --- | --- | --- | --- |
| [11 · Kê một bậc](Level11/README.md) | [Ảnh](Illustrations/11.png) | Nghỉ nhịp / ôn đẩy kéo | 1 |
| [12 · Khối lớn đi trước](Level12/README.md) | [Ảnh](Illustrations/12.png) | Luyện xếp hình | 1 |
| [13 · Thùng đi thang](Level13/README.md) | [Ảnh](Illustrations/13.png) | Kết hợp đã biết | 1 |
| [14 · Luồn một vòng](Level14/README.md) | [Ảnh](Illustrations/14.png) | Giới thiệu ống | 1 |
| [15 · Gặp nhau ở ngã ba](Level15/README.md) | [Ảnh](Illustrations/15.png) | Luyện chọn đường ống | 1 |
| [16 · Một thành hai](Level16/README.md) | [Ảnh](Illustrations/16.png) | Giới thiệu máy lượng tử | 2 |
| [17 · Bạn giữ, mình luồn](Level17/README.md) | [Ảnh](Illustrations/17.png) | Luyện hai phần | 2 |
| [18 · Bám dây sang bờ](Level18/README.md) | [Ảnh](Illustrations/18.png) | Giới thiệu đu dây | 1 |
| [19 · Đưa bến lại gần](Level19/README.md) | [Ảnh](Illustrations/19.png) | Luyện đu + đẩy | 1 |
| [20 · BOSS · Hai nửa một máy](Level20/README.md) | [Ảnh](Illustrations/20.png) | Boss chương hai | 2 |
| [21 · Kéo đối trọng](Level21/README.md) | [Ảnh](Illustrations/21.png) | Nghỉ nhịp / ôn lực | 1 |
| [22 · Ba mảnh thành đường](Level22/README.md) | [Ảnh](Illustrations/22.png) | Luyện lắp ghép | 1 |
| [23 · Đổi tuyến trên vách](Level23/README.md) | [Ảnh](Illustrations/23.png) | Kết hợp ống + cơ quan cao | 1 |
| [24 · Hai rồi bốn](Level24/README.md) | [Ảnh](Illustrations/24.png) | Giới thiệu tách lặp | 4 |
| [25 · Giữ lại phần lớn](Level25/README.md) | [Ảnh](Illustrations/25.png) | Luyện khối lượng / ba vai | 3 |
| [26 · Đu và luồn](Level26/README.md) | [Ảnh](Illustrations/26.png) | Kết hợp hai tuyến | 2 |
| [27 · Bốn trạm tiếp sức](Level27/README.md) | [Ảnh](Illustrations/27.png) | Luyện bốn vai | 4 |
| [28 · Đường ống ba chiều](Level28/README.md) | [Ảnh](Illustrations/28.png) | Vận dụng không gian | 2 |
| [29 · Xưởng lắp cầu](Level29/README.md) | [Ảnh](Illustrations/29.png) | Chuẩn bị Boss / kết hợp | 3 |
| [30 · BOSS · Hộp cộng hưởng](Level30/README.md) | [Ảnh](Illustrations/30.png) | Boss chương ba | 4 |

Các hồ sơ theo 10 mục của LEVEL_TEMPLATE. Thời gian và độ khó là giả thuyết; kiểm tra file/khối lượng trong tài liệu không chứng minh scene chơi được. Không có build mới ở lượt thiết kế này. Q và đu dây là hai rủi ro phải prototype trước khi xây cả bộ.
