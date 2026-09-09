# Kiểm chứng campaign 100

Unity 6000.3.19f1, macOS Apple Silicon. Campaign đã build cho macOS và Android ARM64; [bằng chứng APK hiện tại](Android/README.md). Tài liệu ghi bằng chứng kỹ thuật; không thay thế playtest độ khó/feeling/readability.

## Phạm vi

- `geometry-audit.json`: 100 spawn không chồng lấn đáng kể, bore tĩnh của 100 lỗ thoát thông thoáng. Không kiểm tra toàn đường giải.
- `CampaignLifecycleTests`: 100 ca load/physics/reset và 100 ca thoát đủ bi, cộng chuyển catalog/save ID và replay visual-only. Test aperture đặt từng bi sát miệng lỗ để cô lập hợp đồng thoát; không phải nghiệm từ spawn.
- `EasyCampaignRouteTests`: route bằng input nghiêng từ spawn ở các sân đầu, đối chứng nước và baffle đại diện; không đặt lại vị trí bi giữa đường.
- `HardCampaignMechanismTests`: cam một/hai/ba nấc, intermediate coupled cam, cầu tự gập, domain chất lỏng và clearance hai chiều ở mạng phòng; nhiều ca là fixture cơ cấu/hình học có giới hạn.
- `CampaignBossRouteTests`: hai nhánh sư tử từ spawn, hai cửa/cầu dưới lực thật, act cam đầu của hoa sen, clearance mạng 3D, refuge/reset hai bi, miền nước/thủy ngân. Chưa phải đường giải đầy đủ của cả 10 boss.
- `CampaignProgressTests`: save theo ID khi đổi thứ tự, completion idempotent, dữ liệu hỏng và schema tương lai.

Ảnh `Artifacts/Campaign100` là render tại spawn, không chứng minh gameplay. Bản Mac kiểm tra riêng UI, chọn chương, zoom, chuyển Lab. Các mức D và ngân sách recovery vẫn là giả thuyết thiết kế.

## Các lỗi đã tìm bằng kiểm tra

Lượt audit phát hiện spawn C014/C044/C055/C089 chồng collider; đã sửa layout hoặc dùng spawn có khoảng trống thật. Lượt mô phỏng đầu đạt 244/248 ca; phát hiện khe return cửa C020 cấn lintel, input return rack của C050 trượt khỏi sân, pocket C063/C073 bít lối nước và fixture tìm bridge sai hierarchy sau khi prop tách. Chỉnh clearance cửa, enclosure/route điều khiển thật, loại pocket thừa và truy vấn registry sở hữu props. Không sửa bi bằng teleport để làm test route qua.

Lượt tiếp theo tìm tiếp giao lan can cầu ở khoảng 80° và đường hồi phục chưa nối vào đầu hở của landing. Đã sửa khoảng hở thực, thêm 8 fixture recovery cho mọi vị trí dùng cầu và sửa cổ X của C085. Gen7 đạt 377/378; còn đường dốc C017 lấn bore cuối. C017 được cắt lại lỗ trong cùng lần author panel/socket/inlay. Gen8 audit đã sạch cả 100 màn. Kết quả runner/build cuối ghi ở mục dưới.


## Kết quả cuối Gen8

- **379/379 PlayMode passed**, không skip; gồm regression Lab và campaign. [XML](playmode-results.xml).
- **11/11 EditMode passed**, không skip. [XML](editmode-results.xml).
- **100/100 spawn và bore tĩnh passed**. [Audit](geometry-audit.json).
- Tóm tắt đọc bằng máy: [verification-summary.json](verification-summary.json).

`campaign-playmode.xml` là lượt campaign đầu 244/248, được giữ để truy vết lỗi; `bridge-focused.xml` là lượt xác nhận riêng cầu 2/2. Kết quả tổng hợp cuối là `playmode-results.xml` bên trên. Không diễn giải 379 test passed thành 100 đường giải đã được chơi từ đầu đến cuối.


Bản Mac đầu phát hiện lỗi native MaterialPropertyBlock được khởi tạo trong field initializer của ContainerInspectionView. Đã chuyển sang Initialize sau AddComponent và thêm CampaignHudLoadsChapterBoundariesAndLabWithoutRuntimeErrors. Lượt cuối sau sửa đạt **379 PlayMode + 11 EditMode = 390 ca**.


## Bản Mac

Build thành công, 351.508.261 byte: [build-summary.json](build-summary.json). Native smoke test đã kiểm tra chọn chương1/5/10, boss50/100, zoom, xoay/reset, chuyển23mànLab và khôi phục màn100 sau đóng/mở. Player log cuối không có error/exception. [Ảnh và phạm vi UI](Mac/README.md). Đã xử lý thêm vệt mặt sàn do hai mặt transparent chồng nhau, và chữ nền bị xuyên qua modal.

## Bản Android

Development APK Campaign 100 được build sau thay đổi tự chuyển màn/âm thanh. File 59.272.201 byte, ARM64, min API 26, target API 36; ZIP và chữ ký v2 hợp lệ. Build log xác nhận scene Campaign và dữ liệu đóng gói chứa catalog/ID C001–C100. Không có thiết bị ADB kết nối nên chưa smoke-test runtime trên Android thật. [Chi tiết và hướng dẫn cài](Android/README.md).
