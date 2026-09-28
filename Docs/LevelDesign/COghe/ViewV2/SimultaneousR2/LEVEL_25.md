# COghe V2 R2 — 25 · Giữ rồi sang

## 1. Định danh, phạm vi và trạng thái

ID giữ nguyên `coghe.view.v2.25`; scene `Assets/_Game/Venom/ViewCampaign/COgheView25.unity`. Mrk giao dựng qua Buzz event `6e0465c806f74f6bb00658db11fd75fcfc8138f1dff6a95c489967d236738474`, 28/09/2026. Bản R1 và bằng chứng candidate22 được giữ tại [hồ sơ trước](../Level25.md); chúng không chứng nhận R2. Trạng thái R2: **Đã dựng và kiểm chứng trên Mac**. Unity6000.3.19f1; Mac development player. Chưa nghiệm thu điện thoại hoặc người mới.

## 2. Mục tiêu trải nghiệm và tiến trình

Phối hợp từ hai bờ và đường về. Đã biết cắt thật, chọn phần, tự tụ và chạm giao việc. Không yêu cầu thao tác hai ngón đồng thời, thời hạn, nâng chỉ số hoặc vật mua. Gợi ý ngắn mô tả mối liên hệ cơ quan; không tự thực hiện lệnh.

## 3. Hình học, camera và thao tác

Ống hai chiều đưa B sang bờ xa; A ở bờ gần, tay B ở bờ xa. Khung1,12×0,80m, sàn y=−0,30m; cửa trên tường z=+0,40m có lỗ thật và shutter va chạm. Hộp khóa xoay vật lý; yaw/pinch chỉ đổi camera. Cơ quan giữ độc lập với vách fade. Bệ tụ nằm trong phòng, cách cửa cuối. Dải bám phải nối tới điểm đứng; vách còn lại trơn có vật liệu satin. Kiểm tra portrait720×1280 và720×1612, góc khuất và vùng HUD.

## 4. Lời giải và phục hồi

Cắt → A giữ → B đi ống → B kéo cầu → B trở về tụ với A → cả thân qua cầu tới cửa. Mỗi lệnh đi mới hủy tác vụ của đúng phần; chọn phần khác không hủy. Thiếu một đầu vào: cửa/cầu nâng hồi an toàn; phanh/cầu dừng tại chỗ, B chờ và tiếp tục khi A giữ lại. Chốt chỉ gài ở cuối hành trình thật. Không lưu nửa lực từ thao tác A rồi B của một thân. Hợp đủ32 hạt bên trong trước hạt thoát đầu tiên; thoát sớm vẫn thua đúng luật. Cắt lệch bảo toàn mô, phần quá nhỏ có thể không đủ lực và phải tụ/cắt lại. Không cooldown hay cấm tụ theo tác vụ. Retry xóa cả chốt/lệnh/lực; pause dừng mô phỏng.

## 5. Cơ quan và kiến trúc

`COgheTapRail.HoldAtEnd`: tay hồi lò xo, chân bám đo trên surface thật, lực hữu hạn và phản lực lên mô. `COgheCooperativeDrive`: TwinPull/BrakeBridge/PairedValves; hai đầu vào liên tục, rail đầu ra, vùng quét chống kẹp và chốt. Bộ chọn dùng rail thật, đầu ra đã chốt được giữ. Builder `COgheSimultaneousBuilder.cs` cấu hình scene qua Unity API; runtime không đọc số màn/lời giải. ID/save/catalog30 giữ nguyên.

## 6. Hình ảnh và phản hồi

Bộ V2/DayLab: sứ kem, vách xanh, tay amber, đường truyền nhỏ, mint theo trạng thái thực. Hai nhánh nối chung một đầu ra; đèn nhánh thiếu tắt, tay hồi, thanh cân bằng lệch hoặc guốc phanh kẹp. Chốt cuối có pin dịch và tiếng cạch. Xúc tu đọc thao tác thực. Art không thêm collider hoặc lực. Ảnh Unity và trạng thái bắt buộc được xem trước bàn giao.

## 7. Độ khó và chi phí

Tối đa hai vai trò đồng thời. Khó qua phân công/đường đi, không qua nhịp bấm. Vật lý32 hạt/120Hz, collider/joint đơn giản; dây/puly trình bày không có chuỗi Rigidbody. Vật liệu dùng chung, một đèn có bóng. Không suy ra FPS điện thoại từ số object. Ngân sách mobile giữ theo `Docs/COGHE_VIEW_V2.md`; chưa có đo thiết bị.

## 8. Chơi thử và hồi quy

Cần: lời giải qua chạm thật, làm một thân A→B/B→A không qua, đảo thứ tự hai phần vẫn qua, bỏ dở và phục hồi, đổi chọn giữ việc, đổi lệnh buông, pause/retry, cắt lệch/tụ, chống kẹp không chốt, real wall exit, đủ32 hạt. Full PlayMode/EditMode và native replay30. Kết quả cuối: toàn bộ 521/521 PlayMode và 8/8 EditMode đạt; 30/30 màn replay native tăng tốc, 8/8 màn 23–30 native tốc độ thực đạt. Mỗi lượt thoát đủ 32 hạt trong một cơ thể. [XML, JSON và ảnh thực tế](../../../../Verification/COgheSimultaneous/README.md).

## 9. Hiệu năng thiết bị

Mac author replay là bằng chứng đường giải, không là người mới hoặc mobile. Điện thoại, nhiệt, touch thật và 15–20 phút sustained: chưa thực hiện. Manifest source/build và JSON gắn với bản Mac GUID `de4a91fe0f5e417a9e1cd71ff3617a6e`. Lượt tốc độ thực chạy đồng thời với Editor regression nên không dùng frame samples làm benchmark hiệu năng riêng.

## 10. Tích hợp và nghiệm thu

Giữ scene/definition GUID, content ID và progress key cũ. Không thêm màn31 hoặc mở thưởng mới. Các màn01–22 và catalog lịch sử giữ thiết kế. Đã đạt phạm vi kiểm chứng Mac nói trên; [báo cáo bàn giao](../../../../Verification/COgheSimultaneous/README.md) giữ XML/JSON, manifest, ảnh cả tám màn và lịch sử sửa prototype. Chưa nghiệm thu mobile hoặc khả năng tự hiểu của người chơi mới.
