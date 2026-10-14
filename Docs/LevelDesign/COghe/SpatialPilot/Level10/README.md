# COghe Spatial 10 — Cỗ máy thân quen

## 1. Định danh và phạm vi

`coghe.spatial.pilot.10`, vị trí hiển thị 10 trong catalog Spatial 65 màn.
Scene `Assets/_Game/Venom/SpatialCampaign/COgheSpatial10.unity`.
Revision 14/10/2026 theo yêu cầu tăng độ khó Boss. Giữ ID, luật kính và Glass C;
không đổi V2/Origin. Bản minh họa/đo 29/09 là lịch sử, không mô tả revision này.

## 2. Trải nghiệm và kỹ năng

Boss đầu tiên kiểm tra phối hợp leo, thang nâng, mở khóa và cầu di động.
Không hướng dẫn đáp án. Cơ quan không dùng chữ A–D ở hình ảnh runtime; ký hiệu
trong tài liệu chỉ giúp mô tả: A blue, B coral, C amber, D green.
Không có đồng hồ, đu dây, phân tách hay thao tác mới chưa được học.

## 3. Hình học, camera và chạm

Hộp kính 0.8 × 0.6 m, tường/nóc kính trần bám được. Lớp phủ lavender nhìn thấy
được ở cạnh các tầng ngăn bò vòng qua cơ quan; mesh khớp SlipRegions.
Ban công ở y=0.04m chia hai đảo. Cầu giữa rộng 0.176m, sâu 0.136m, hành trình
0.152m, khe tiếp giáp 2mm; thang bên trái nâng 0.30m. Cầu hạ không đè vào cửa thoát.
Phần D ở ban công phải được che một phần bởi vách thấp; orbit giúp nhìn thấy.
Camera 3/4, pinch/orbit như cũ, hộp đứng yên. Không ép thao tác đúng lúc.

## 4. Chuỗi giải và hồi phục

1. Lên thang đến ban công trái; kéo C (amber) mở hộp che B dưới sàn.
2. Xuống thang; kéo B (coral) nâng cầu thật lên nối hai ban công.
3. Lên thang, đi qua cầu, đổi góc nhìn để tìm D; kéo D (green) mở hộp che A.
4. Qua cầu trở lại, xuống thang, kéo A (blue) mở cửa cùng màu, chạm lỗ thoát.

A/B từ đầu bị khóa và phản hồi từ chối; chạm D sớm không tự mở đường.
Có thể kéo B ngược để hạ cầu rồi nâng lại. Thang gọi về được nếu người chơi rơi
xuống sàn; không khóa vĩnh viễn. Retry reset cả cầu, nắp, tay kéo, thang và cửa.
Thắng khi hợp thể đủ 32 hạt trước khi thoát; không thay luật tách/tụ.

## 5. Cơ quan và trạng thái

`COgheTapRail` C → `COgheViewMechanism` → khóa nắp B.
B chỉ hoạt động khi nắp mở; B → cầu nâng vật lý.
D → khóa nắp A; A chỉ hoạt động khi nắp mở; A → cửa cuối và trạng thái lỗ.
Thang độc lập, dùng finite-force motor và tiếp xúc để chở sinh vật.
Không có teleport, lệnh thắng, bộ giải trong runtime hay state do material quyết định.
Author replay sử dụng chạm/di chuyển/thang thật.

## 6. Hình ảnh và phản hồi

Glass C, plaque trên kính, thân máy ivory, ray satin dịu, mạch điện mảnh cùng màu
với cơ quan. Nắp amber trên B, nắp green trên A. Cầu/cửa có màu liên kết.
COghe giữ animation tiếp xúc: lên khay, ấn nút, bò qua cầu, ghì kéo, vui khi thắng.

## 7. Độ khó và ngân sách

Tăng từ hai khóa độc lập trên cùng ban công thành chuỗi phụ thuộc bốn điều khiển
ở hai độ cao và hai đảo. Bốn chuyến thang và cầu tạo nhịp quan sát → suy luận →
kiểm chứng. Không tăng số hạt hoặc fixed timestep; thêm một rail vật lý cho cầu.
Độ khó người mới cần playtest, không suy ra từ solve tự động.

## 8–10. Kiểm chứng, hiệu năng và tích hợp

[Biên bản revision](../../../../Verification/COgheChapterProgression/README.md).
Chạy solve thật, wander, lệnh sai thứ tự, đảo chiều cầu, Retry và kiểm tra va chạm
khởi tạo. Kiểm tra UI ở màn hình ngắn, dài và iPad. Chỉ báo FPS khi có native run;
không dùng PlayMode tăng tốc làm bằng chứng FPS mobile.
Sau Boss mở Nhà, bắt buộc hoàn thành “Understand COghe” rồi tiếp chương 2.
Trang trí và bài tiêm màu đã mở sau màn hiển thị 8.
