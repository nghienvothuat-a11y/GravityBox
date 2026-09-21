# COghe — camera gần và xem từng khoang

## Sửa mép nền lộ khi xoay màn 03 — 17/09/2026

Camera tự vừa hộp khi xoay nhưng bàn nền cũ chỉ rộng 12 m, far clip 10 m và camera luôn cách tâm nhìn 2 m. Ở góc nhìn thấp, khung hình dọc có thể nhìn ra ngoài mép bàn hoặc cắt xuống dưới mặt bàn, làm lộ màu clear xám thành dải/đường chéo thay đổi khi xoay. Test tái hiện trước sửa thất bại: tia góc chạm bàn tại z = 6,274 m, ngoài mép z = 6 m.

`COgheStudioBackdrop` giữ nguyên cao độ bàn và vùng bóng đổ, chỉ mở rộng mesh nền không có collider để phủ bốn góc viewport. Camera lùi dọc hướng nhìn khi cần để mọi tia bắt đầu phía trên bàn, và far clip đủ tới nền. Camera trực giao nên thao tác này giữ kích thước/vị trí hộp trên màn hình. Phạm vi ray chọn vật theo khoảng nhìn thay vì cố định 5 m. Không thêm renderer, shader, texture, collider hoặc tính toán tìm đường; chỉ đọc mesh/bounds một lần và tính bốn góc mỗi frame.

Kiểm tra nền bao phủ cả 20 màn, ở 720×1280 và 720×1612, bốn tư thế xoay và chế độ toàn cảnh/theo sinh vật. Bộ camera **3/3 đạt**; toàn bộ hồi quy **81/82 đạt**, chỉ còn lỗi cũ ở đường thử hồi phục sau trượt màn 04. Không sửa trọng lực, vị trí vật thể vật lý, mặt bám hay luật puzzle. [Ảnh trước](Verification/COgheBackground/03-before.png), [ảnh sau](Verification/COgheBackground/03-after.png), [render tỷ lệ OPPO](Verification/COgheBackground/03-oppo-aspect.png), [kết quả](Verification/COgheBackground/validation.json). Log đầy đủ nằm trong `Artifacts/COgheBackground/`; ảnh từng tư thế ở `Artifacts/COgheCamera/03-background-*.png`.

Cập nhật sau đợt camera: đường giải Boss 20 đã đạt trong PlayMode và bản Mac chạy thật; lỗi hai phần B/C tụ sớm được xử lý bằng thứ tự di chuyển đúng, giữ nguyên luật hợp thể. [Lời giải và đo hiệu năng bản camera mới](COGHE_BOSS20_PLAYTEST.md). Kết quả 76/78 bên dưới là mốc kiểm chứng trước đợt này.

Ngày 17/09/2026. Áp dụng cho Origin 01–20; giữ góc nhìn đã chọn riêng cho từng màn.

## Cách dùng

- **Toàn cảnh:** nhìn đủ hộp và các liên kết để định hướng. Khung hình tự khớp kích thước hộp, tỉ lệ màn hình và vùng trống giữa HUD; chừa viền, nóc và biểu tượng xoay.
- **Theo COghe:** camera tiến gần và theo phần sinh vật đang chọn. Chọn phần khác để quan sát phần đó. Bấm lại nút **Toàn cảnh** để trở về.
- **Hộp/Khoang:** màn 08 có hai hộp, màn 19 có hai khoang, Boss 20 có ba khoang. Các nút dưới tên màn chuyển mượt tới khu vực tương ứng, cho phép xem cơ quan xa sinh vật. Nút toàn cảnh luôn nằm cùng hàng.
- Khi chọn khoang, phần hộp lân cận có thể nằm ngoài màn hình. Màn 19/20 ưu tiên vùng có cơ quan phía dưới; toàn cảnh vẫn cho thấy đủ nóc. Không tự chuyển khoang hay đánh dấu thứ tự lời giải.
- Làm lại trở về toàn cảnh. Camera chiến thắng và Nhà vẫn giữ hành vi riêng. Cấm xoay hộp vẫn có hiệu lực trong mọi chế độ nhìn.

## Kiến trúc và chi phí

`VenomCampaignCamera` chỉ điều khiển camera, không ghi trạng thái mô phỏng. Khi vào scene, lấy bounds từ mặt kính tĩnh trong tọa độ hộp và cộng khoảng hở cho viền kính. Mỗi frame chỉ chiếu tám góc, hoặc dùng bán kính cho khối cầu; không quét renderer, collider hoặc tìm đường. Vật di động không làm bounds thay đổi.

Góc quay camera vẫn lấy `CameraEuler` trong definition. `CameraZones` chứa nhãn và bounds từng vùng, không phụ thuộc số màn trong runtime; có thể thêm khu vực cho level tương lai qua Inspector. Builder màn 08/19/20 giữ cấu hình này khi tạo lại scene. `ViewRadius` chỉ còn là fallback khi scene không có mặt kính và dùng để đối chiếu camera cũ.

Bản chỉnh màn hiển thị 30 ngày 18/09 thêm `InitialCameraZone` (mặc định −1:
toàn cảnh). Boss 30 đặt 0 để bắt đầu gần khoang có sinh vật và dao; Retry về
góc khởi đầu này. Ba nút **A · Dao / G · B / C · Lỗ** và **Toàn cảnh** vẫn do
người chơi chọn. Camera không tự đổi khoang theo bước giải hoặc thay vật lý;
các màn khác giữ mặc định toàn cảnh. Xem [bản chỉnh Boss 30](Verification/COgheCampaign30/boss-30.md).

Vị trí và độ phóng đại chuyển mượt. Khi hộp xoay làm silhouette rộng hơn, toàn cảnh nới ngay đủ chứa hộp rồi thu lại từ từ. Vùng chơi có xét `Screen.safeArea`. Nhấn nút đổi vùng không phát lệnh di chuyển/xoay. Chi tiết viền cản tầm nhìn được ẩn khi theo sinh vật; xem từng khoang vẫn giữ khung kính để định hướng. Không ẩn collider hay cơ quan.

Không thêm camera phụ, RenderTexture, shader, hiệu ứng hậu kỳ hoặc thay đổi chất lượng mô mềm ở runtime. Cần đo lại trên thiết bị nếu muốn kết luận FPS của bản camera này; số đo tối ưu trước đó không phải số đo của lần sửa camera.

## Kiểm chứng

- 78 PlayMode tests: **76 đạt, 2 lỗi gameplay đã tồn tại** — màn 04 đi tiếp sau khi trượt chưa hoàn tất đường giải; Boss 20 các phần B/C tụ trước khi giữ đủ ba vai trò. Không ghi nhận bài kiểm tra mới bị lỗi. Xem [kết quả](Verification/COgheCamera/validation.json).
- Kiểm tra đủ 20 toàn cảnh ở 720×1280 và 720×1612, kể cả safe area; các màn cho xoay được thử thêm ba tư thế. Khối cầu dùng biên cầu thực, không lấy đường chéo của hộp bao.
- Kiểm tra các vùng 08/19/20: đủ vùng cơ quan, chuyển mượt, không đổi vị trí Rigidbody, nút không phát lệnh di chuyển, theo sinh vật và reset về toàn cảnh. Bộ hồi quy bao gồm screen tap thật, chảy qua ống, cắt/hợp thể, thắng/thua và Collection.
- [Gallery 20 màn](Verification/COgheCamera/overview-contact-sheet.jpg), [số đo độ phóng đại](Verification/COgheCamera/framing.csv). Ảnh là render URP thật, chưa gồm IMGUI; ảnh giao diện bản chạy được kiểm tra riêng. Toàn cảnh lớn hơn khoảng 9–29% ở đa số màn trên 9:16; cầu 06 +70%, mê cung 15 +46%. Màn 08 toàn cảnh chỉ tăng khoảng 1% vì phải giữ đủ hai hộp; dùng nút Hộp để xem gần.
- Các ảnh trước/sau dùng cùng scene và trạng thái, chỉ thay công thức camera; không phải ảnh concept.

| Chế độ | Ảnh Unity |
| --- | --- |
| Boss 20 trước | [Ảnh](Verification/COgheCamera/20-before.png) |
| Boss 20 toàn cảnh mới | [Ảnh](Verification/COgheCamera/20-after.png) |
| Boss 20 khoang giữa | [Ảnh](Verification/COgheCamera/20-zone-2.png) |
| Màn 08 hộp đầu | [Ảnh](Verification/COgheCamera/08-zone-1.png) |

Đã build và mở bản macOS mới: kiểm tra trực tiếp HUD dọc, chuyển Hộp 1 ở màn 08, Khoang 2 ở Boss 20, Theo COghe và Nhà. Bản cuối giữ khung kính khi xem khoang; kiểm tra lại Boss 20 sau build xác nhận nút và khung đều hoạt động. Không thay collider, joint, trọng lượng, hình học scene hoặc luật puzzle trong đợt camera này.

Bản macOS: `Builds/Venom/macOS/Venom.app`. APK trên OPPO vẫn là bản tối ưu trước đợt camera này; chưa build lại Android trong lượt này.

## Góc nhìn riêng theo khoang — rà 32–40, 21/09/2026

`VenomCameraZone` có `OverrideCameraEuler` và `CameraEuler` tùy chọn. Dữ liệu
cũ mặc định không override; Toàn cảnh và Follow dùng heading của level. Camera
32–35 khai báo góc nhìn khoang khi vách/ống/nắp che đường chạm. Việc chọn khoang
không xoay Root hoặc thay trạng thái cơ quan. Kiểm thử portrait/safe area và
chuyển về toàn cảnh nằm trong `ChapterInspectionAnglesFrameBothPortraitsWithoutMovingPuzzle`.
