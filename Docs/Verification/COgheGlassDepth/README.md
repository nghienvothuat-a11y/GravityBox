# COghe Glass Depth — thử màn 01

29/09/2026 · nhánh NewGraphic · Unity 6000.3.19f1. Thử nghiệm theo yêu cầu người dùng; chưa chốt style mới cho campaign.

[Trang so sánh ảnh và clip](review.html). Bản Mac chơi trực tiếp: `Builds/GlassDepth/macOS/COghe.app`.

## Các phương án

- **Gốc:** hộp kính của bản pilot trước.
- **A:** ánh sáng chính và bóng khung rõ hơn; sàn nhận bóng, thêm bóng tiếp xúc theo vị trí mô.
- **B:** A + phản xạ từ cubemap có sẵn, sứ nhám hơn và highlight cơ thể mềm hơn.
- **C:** B + nền chuyển sắc lạnh–ấm, silhouette thiết bị lab rất nhẹ và bóng trên bàn. Đây là mặc định của bản test.

Ở màn 01, bốn nút bên dưới đổi phương án và tải lại màn. Cùng layout, kích thước, camera, collider, điều khiển và luật thắng. Không chuyển các màn khác sang style này. Background C là hình procedural theo màn hình, không phải mô hình phòng lab hoặc hiệu ứng DoF.

## Kiểm chứng

- **26/26 PlayMode tests pass:** [XML](playmode.xml). Gồm đường giải 01–10, input/camera/cutaway, glass orbit và test cả bốn lựa chọn. Kiểm tra bóng tiếp xúc có khi sinh vật nghỉ và hết khi toàn bộ 32 hạt thoát ra.
- Sau sửa cách căn camera chụp ảnh: test riêng cả bốn phương án tiếp tục **1/1 pass**: [XML](visual.xml).
- Native Mac author replay cả Gốc/A/B/C **4/4 pass**, mỗi lượt 32 hạt thoát, một bản thể, không Lost hoặc runtime error. Đường giải dùng InputSystem touch thật qua camera. Đây không phải bằng chứng người mới chơi.
- Physics snapshot của **30 cảnh giống nhau** trước/sau builder. SHA-256: `0f57d8866c1e3cd37b62dcf06fdd61cc4c5cb821e0c758e1fb4bfa533a495356`. [Snapshot](physics-after.txt).
- Ảnh 0–3 là render camera Unity cùng góc, cùng bố cục. Clip A/B/C là screenshot bản Mac chạy thật, theo timestamp thực; lấy mẫu khoảng 6–8 ảnh/s. Các clip chỉ để xem ánh sáng và chuyển động, không dùng đánh giá FPS. HUD mờ vì replay khóa các nút.

## Đo hiệu năng — giới hạn rõ ràng

Mac17,2 / Apple M5 / Metal, 720×1280, target 60 FPS, normal player loop. Mỗi phương án một lượt ngắn ở màn 01; tách riêng khỏi quay video. Chưa đo GPU time, tải nhiệt hoặc phiên 15–20 phút. Không suy ra hiệu năng Android từ máy này.

| Phương án | Giây đường giải | Frame | FPS TB | p95 ms | p99 ms | Frame >33,3 ms |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| original | 4.11 | 242 | 59.99 | 16.67 | 16.71 | 0 |
| a | 4.11 | 241 | 59.75 | 16.94 | 17.57 | 0 |
| b | 4.12 | 241 | 59.74 | 16.99 | 17.57 | 0 |
| c | 4.12 | 241 | 59.75 | 17.02 | 17.57 | 0 |

Raw reports: [Gốc](native-original.json), [A](native-a.json), [B](native-b.json), [C](native-c.json).

ADB không có thiết bị trong lượt làm này; **chưa build/cài/đo phiên bản study này trên OPPO**. Chi phí mobile phải kiểm tra trên máy thật trước khi nhân rộng. Không dùng số đo của các bản Blender hoặc glass trước cho bản này.

## Kỹ thuật và tái tạo

- `COgheGlassDepthStudy`: chỉ đọc tối đa 6 mặt × 32 hạt để đặt bóng tiếp xúc trong toạ độ mặt; không thêm Physics query. PropertyBlock tái sử dụng, không allocation trong LateUpdate. Không sửa solver hoặc navigation. Chỉ clone skin profile rồi đổi material.
- Contact shading là xấp xỉ khoảng cách tới mặt, không phải SSAO. Mặt giữ đúng mesh có lỗ; hiệu ứng không bịt aperture. Trường hợp nhiều nhóm mô xa nhau chưa thuộc màn 01 này và chưa nghiệm thu để dùng chung.
- Hai shader và các material thử nghiệm ở `Assets/_Game/Venom/Art/GlassDepth`. Dùng cubemap studio tĩnh đã có; không thêm realtime reflection, bloom hoặc DoF. Khung giữ batching theo material.
- C dùng shader bàn có bóng, các hình nền ở screen-space và giữ sự yên tĩnh cho vùng chơi. Khi mở rộng phải kiểm tra cơ quan phức tạp/occlusion riêng.
- Unity menu: `Gravity Box → COghe → Glass preview → Build depth comparison Mac`. Builder giữ nguyên physics snapshot, tích hợp trong đường dựng art V2 của level 01.
- CLI bản Mac: `-coghe-depth original|a|b|c`. Lượt native dùng `-coghe-view-proof <dir> -coghe-view-first 1 -coghe-view-last 1 -coghe-proof-quit`.
- `-coghe-depth-record <dir>` là recorder opt-in dành cho evidence build; có ảnh hưởng frame time, report đánh dấu không dùng số FPS của lượt này.

Không commit/push trong lượt này. Blender và các thay đổi có sẵn được giữ lại; chỉ dùng study ở màn 01.
