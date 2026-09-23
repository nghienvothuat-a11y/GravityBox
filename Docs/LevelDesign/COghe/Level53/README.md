# COghe — Hồ sơ level `venom.origin.53` — 53 · Kéo về đúng chỗ

Nguồn chuẩn: [quy tắc](../../../COGHE_LEVEL_DESIGN_RULES.md), [Day Lab](../../../ArtDirection/COghe/STYLE_RULES.md), [hồ sơ nguồn](../../../COGHE_EXPANSION_11_20.md). Theo mẫu LEVEL_TEMPLATE; revision 1, Codex, 23/09/2026.

## 1. Định danh, phạm vi và trạng thái

ID `venom.origin.53`, vị trí 53. Scene `Assets/_Game/Venom/Campaign30/COgheOrigin53.unity`; definition `Definitions/Slot53.asset` cùng thư mục. Builder `Assets/_Game/Editor/COgheCampaign55Builder.cs`. Mrk giao qua event `f9df450879e1d3467a4081de6a1185331dee050738c4b648c63d4fdf13dba34d`. Trạng thái **Prototype — đã kiểm chứng gameplay và input replay trên Mac**; không gán đạt mobile/người mới từ test Mac. Bằng chứng tập trung tại `Artifacts/COgheCampaign55`.

## 2. Mục tiêu trải nghiệm và vị trí trong tiến trình

Kéo ba khối lùi về đường ngang. Màn thường, hướng dẫn ngắn theo cơ quan. Không thêm chỉ số, vật phẩm bắt buộc hoặc phần thưởng vĩnh viễn. Luyện cơ chế đẩy/kéo của màn hiển thị 19 (legacy content 16), không học thêm động từ mới.

## 3. Phác thảo, hình học, camera và thao tác

Hộp 0,60 × 0,60 m; spawn (-.255,-.27,-.255), lỗ cuối (.30,-.076,0), bán kính .041 m. Bờ trái leo được; bờ phải chỉ bám mặt trên. 3 khối chia khoảng x=-.21..+.21, mặt trên y=-.10, thân cao .20, sâu .09 m. Sàn trơn ở x=-.21..+.21, z=-.085..+.085; vách trơn. Camera (52,-12,0), có overview/follow, không đổi trọng lực. Khóa xoay hộp. Cần kiểm tra portrait 720×1280 và 720×1612, HUD sáu trang, tay nắm không bị che. Camera chỉ đổi quan sát. Nóc/vách trơn vẫn theo luật chọn mặt hiện hành; không cung cấp lực leo.

Sơ đồ nhìn trên: Spawn/hành lang khô phía trước → bờ trái leo lên → A — B — C → bờ phải → lỗ cuối. Ổ đích là vị trí thanh ray + trục × hành trình; đèn chỉ đọc chốt vật lý.

## 4. Lời giải, kết thúc và phục hồi

Chạm tay nắm từng khối → tiếp cận bằng bò thật → chạm hướng đích trên mặt phẳng thao tác → lực hữu hạn đưa khối tới chốt. Buông, đi theo hành lang khô về bờ trái, leo lên rồi chạm lỗ để qua các mặt cầu. Các khối độc lập; thứ tự ngược cũng hợp lệ. Kéo ngược nhả chốt; có thể thao tác lại. Rơi về sàn thì trở lại bờ trái; Retry khôi phục tất cả khối. Thắng khi đủ 32 hạt của một cơ thể đã nhập bên trong đi qua FinalExit. Giữ luật thua khi thoát trước khi nhập. Không có dao, transfer hoặc cửa logic ẩn; bài đố là hình học của mặt cầu. Pause dừng mô phỏng; Retry hủy lệnh và lực cũ, khôi phục vị trí đầu. Không teleport hoặc ép thắng trong kiểm chứng.

## 5. Cơ quan, trạng thái và kiến trúc dùng lại

3 COgheRailSlider + VenomMovableProp; lực và reaction qua VenomCampaign.StepProp. Ray đẩy .16 m, ray kéo .14 m; chốt cuối giữ thật. COgheAssemblyBridge chỉ đếm chốt, không điều khiển cửa. Presentation đọc Rails để đổi đèn. Route invalidate theo geometry động hiện hành. Runtime không đọc lời giải hoặc đổi luật theo số màn. Giữ khối lượng và 32 hạt ở 120 Hz.

## 6. Hình ảnh, animation và phản hồi

Day Lab: mặt cầu amber, sườn trơn lavender trong, tay nắm kim loại, ổ porcelain và đèn mint khi chốt thật. Tay/skin đọc tiếp xúc; hình trang trí không thêm đường đi. Nhận lệnh → bò tới → bám/đẩy/kéo → chốt hoặc buông. Ảnh thật và video nằm cùng bằng chứng, chưa dùng concept để chứng nhận input.

## 7. Độ khó và ngân sách runtime

Kéo ba khối lùi về đường ngang. Dự kiến một thao tác cơ quan mỗi lần, không yêu cầu canh thời gian. 3 ray động, một cơ thể; so với ba ray ở màn 19. Khó quan sát/tap nhầm không phải mục tiêu thiết kế. FPS mobile mục tiêu 60; p95/GC/GPU/nhiệt và ngân sách bộ nhớ chưa đo/chốt. Không suy ra hiệu năng từ số khối.

## 8. Chơi thử và hồi quy

Dùng full PlayMode + EditMode Unity 6000.3.19f1. COgheCampaign55Tests phủ giải bằng screen tap và lệnh bò, thứ tự ngược, không lách đường khi chưa ghép, chạm portrait, kéo ngược/pause/retry. Kiểm tra Mac player dùng input thật và FixedUpdate bình thường riêng. Kết quả: full 393/393 PlayMode, 8/8 EditMode và Mac replay 15/15. Người mới và điện thoại chưa kiểm chứng.

## 9. Bằng chứng hiệu năng trên thiết bị

Chưa đo điện thoại, thermal 15–20 phút, frame-time/GC/GPU hoặc retention. Replay Mac chỉ chứng minh workflow kỹ thuật. Build/source/report cuối phải trùng hash trong manifest. Không tuyên bố đã đạt ngân sách mobile.

## 10. Tích hợp, tương thích và nghiệm thu

Build mặc định có đủ 55 scene, bộ chọn 6 trang; không dùng --tap cho bản integrated. ID mới không đụng các content cũ. Save key/schema giữ nguyên. Scenes 01–40 và Tap gốc giữ bytes/GUID; rollback trạng thái trước qua `BEFORE_CAMPAIGN55.tar.gz`. Kết luận: prototype đạt kiểm chứng kỹ thuật Editor/Mac; chưa nghiệm thu mobile/người mới. Xem [báo cáo](../../../COGHE_CAMPAIGN_41_55_VERIFICATION_2026_09_23.md).
