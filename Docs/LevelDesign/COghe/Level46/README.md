# COghe — Hồ sơ level `coghe.tap.v1.06` — 46 · Đón bạn trở về

Nguồn chuẩn: [quy tắc](../../../COGHE_LEVEL_DESIGN_RULES.md), [Day Lab](../../../ArtDirection/COghe/STYLE_RULES.md), [hồ sơ nguồn](../../TapCampaign/Level06.md). Theo mẫu LEVEL_TEMPLATE; revision 1, Codex, 23/09/2026.

## 1. Định danh, phạm vi và trạng thái

ID `coghe.tap.v1.06`, vị trí 46. Scene `Assets/_Game/Venom/Campaign30/COgheOrigin46.unity`; definition `Definitions/Slot46.asset` cùng thư mục. Builder `Assets/_Game/Editor/COgheCampaign55Builder.cs`. Mrk giao qua event `f9df450879e1d3467a4081de6a1185331dee050738c4b648c63d4fdf13dba34d`. Trạng thái **Prototype — đã kiểm chứng gameplay và input replay trên Mac**; không gán đạt mobile/người mới từ test Mac. Bằng chứng tập trung tại `Artifacts/COgheCampaign55`.

## 2. Mục tiêu trải nghiệm và vị trí trong tiến trình

Giữ tải, nối hai bánh răng rồi thu hồi phần giữ. Màn thường, hướng dẫn ngắn theo cơ quan. Không thêm chỉ số, vật phẩm bắt buộc hoặc phần thưởng vĩnh viễn. Giữ nhận diện nội dung Tap đã chơi.

## 3. Phác thảo, hình học, camera và thao tác

Giữ nguyên hình học, spawn, lỗ cuối, camera và cơ quan của màn Tap gốc; xem hồ sơ nguồn. Khóa xoay hộp. Cần kiểm tra portrait 720×1280 và 720×1612, HUD sáu trang, tay nắm không bị che. Camera chỉ đổi quan sát. Nóc/vách trơn vẫn theo luật chọn mặt hiện hành; không cung cấp lực leo.

Bố cục và sơ đồ nguồn giữ nguyên.

## 4. Lời giải, kết thúc và phục hồi

Lời giải, chốt, cắt/giữ tải và phục hồi giữ nguyên theo hồ sơ Tap nguồn. Chỉ số hiển thị thay đổi. Thắng khi đủ 32 hạt của một cơ thể đã nhập bên trong đi qua FinalExit. Giữ luật thua khi thoát trước khi nhập. Có dao cắt theo hồ sơ gốc; không thêm cooldown hay bảo vệ nhiệm vụ. Pause dừng mô phỏng; Retry hủy lệnh và lực cũ, khôi phục vị trí đầu. Không teleport hoặc ép thắng trong kiểm chứng.

## 5. Cơ quan, trạng thái và kiến trúc dùng lại

COgheTapRail, COgheTapPad, COgheGearTrain và COgheGuillotine giữ nguyên dữ liệu nguồn. Runtime không đọc lời giải hoặc đổi luật theo số màn. Giữ khối lượng và 32 hạt ở 120 Hz.

## 6. Hình ảnh, animation và phản hồi

Day Lab: mặt cầu amber, sườn trơn lavender trong, tay nắm kim loại, ổ porcelain và đèn mint khi chốt thật. Tay/skin đọc tiếp xúc; hình trang trí không thêm đường đi. Nhận lệnh → bò tới → bám/đẩy/kéo → chốt hoặc buông. Ảnh thật và video nằm cùng bằng chứng, chưa dùng concept để chứng nhận input.

## 7. Độ khó và ngân sách runtime

Giữ tải, nối hai bánh răng rồi thu hồi phần giữ. Dự kiến một thao tác cơ quan mỗi lần, không yêu cầu canh thời gian. Số vai trò/cơ quan theo hồ sơ nguồn. Khó quan sát/tap nhầm không phải mục tiêu thiết kế. FPS mobile mục tiêu 60; p95/GC/GPU/nhiệt và ngân sách bộ nhớ chưa đo/chốt. Không suy ra hiệu năng từ số khối.

## 8. Chơi thử và hồi quy

Dùng full PlayMode + EditMode Unity 6000.3.19f1. Integrated41Through50RetainAllTenPlayableSolutions giải đủ mười màn sau khi đổi vị trí. Kiểm tra Mac player dùng input thật và FixedUpdate bình thường riêng. Kết quả: full 393/393 PlayMode, 8/8 EditMode và Mac replay 15/15. Người mới và điện thoại chưa kiểm chứng.

## 9. Bằng chứng hiệu năng trên thiết bị

Chưa đo điện thoại, thermal 15–20 phút, frame-time/GC/GPU hoặc retention. Replay Mac chỉ chứng minh workflow kỹ thuật. Build/source/report cuối phải trùng hash trong manifest. Không tuyên bố đã đạt ngân sách mobile.

## 10. Tích hợp, tương thích và nghiệm thu

Build mặc định có đủ 55 scene, bộ chọn 6 trang; không dùng --tap cho bản integrated. Giữ ID của Tap gốc, không tạo lại tiến trình hoặc cấp trùng completion. Save key/schema giữ nguyên. Scenes 01–40 và Tap gốc giữ bytes/GUID; rollback trạng thái trước qua `BEFORE_CAMPAIGN55.tar.gz`. Kết luận: prototype đạt kiểm chứng kỹ thuật Editor/Mac; chưa nghiệm thu mobile/người mới. Xem [báo cáo](../../../COGHE_CAMPAIGN_41_55_VERIFICATION_2026_09_23.md).
