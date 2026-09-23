# COghe — coghe.tap.v1.07 — Bốn điểm dừng

Nguồn: [luật level](../../COGHE_LEVEL_DESIGN_RULES.md), [Day Lab](../../ArtDirection/COghe/STYLE_RULES.md), [chapter](../../COGHE_TAP_CAMPAIGN.md). Hồ sơ theo các mục của LEVEL_TEMPLATE. Ngày 23/09/2026; Mrk yêu cầu thêm 06–10 để user thử chung, event `185e1e5d67f04a4c4d00919d9960accc7afb7cc4b3ef749ef9cb45155dfcf8d0`.

## 1. Định danh và trạng thái

ID `coghe.tap.v1.07`, thứ tự 7, scene `Assets/_Game/Venom/TapCampaign/COgheTap07.unity`, definition `TapCampaign/Definitions/Tap07.asset`. Builder `COgheTapCampaignBuilder.cs`. Phiên bản 1. Trạng thái: **Prototype — gameplay/input đã qua kiểm chứng Mac**; xem báo cáo chapter để biết kết quả thực. Chưa có novice/mobile evidence. Không thay ID hoặc completion của Origin/01–05.

## 2. Mục tiêu và tiến trình

Giới thiệu selector. Cam ở chốt 2 nhả khóa B. B bật nguồn. Chốt 4 mới ăn khớp đường truyền cửa.
Lời nhắc đọc trạng thái thực; không ra lệnh hoặc mở khóa hộ. Không cần mua đồ/nâng cấp.

## 3. Phác thảo, camera và thao tác

Nhìn từ trên, x trái→phải; z trước→sau, đơn vị mét:

```text
sau z=+.30
  sàn đi được    motor x=-.15,z=.13 -- A/B x=-.04 -- đầu ra -- nắp thoát x=.25,z=.18
  không dao    A: z=-.11 → -.03 → .05 → .13
  cần nguồn B: x=.19 → .27,z=-.06
spawn (.20,-.275,-.20); vùng tụ (.22,-.30,-.15)
trước z=-.30
```

Hộp .8×.6 m, sàn y=-.30; lỗ thật bán kính .041, nắp nâng .14. Góc đầu 55°/25°, camera overview/follow sẵn có, khóa xoay. Nóc không chọn; sàn vẫn nhận đích. Các mốc portrait: 720×1280, 720×1612. Nhãn số nằm bên ray, chấm mint chỉ chốt kế; không chỉ thứ tự giải. Chạm cơ quan giao một hành trình; chạm phần để chọn; chạm sàn giao di chuyển. Chạm dồn không xếp hàng, pause/retry giữ nguyên cơ chế chung.

## 4. Lời giải và phục hồi

Lời giải cho tác giả/test (runtime không đọc): **A: 1→2 → bật B → A: 2→3→4 → cửa mở → thoát.**

Nếu bỏ qua B ở chốt 2: đi hết vòng trở lại chốt 2 rồi bật B. Mỗi bước dừng theo vị trí thật; bước 4→1 đi ngược toàn ray, không teleport. Mở rồi bỏ khớp ở 07 làm cơ cấu hồi đóng cửa; chạm tới 4 lại mở.

Không yêu cầu chia mô. Cửa hồi khi bộ truyền ngắt; nguồn/cơ quan không có lịch sử bí mật để ép thứ tự.

Thắng khi một cơ thể đã nhập trong hộp rồi đủ 32 hạt ra lỗ cuối. Một phần thoát sớm: thua `bạn phải hợp thể trước khi chui ra`. Không có cổng transfer. Vật bị cản không xuyên vật/hoàn thành giả; lệnh báo kẹt, sửa trạng thái hoặc retry. Retry xóa lực/tác vụ, trả cơ quan và mô về cấu hình đầu.

## 5. Cơ quan và kiến trúc

- `COgheTapRail` + `COgheRailSlider`: lực hữu hạn, điểm đứng tách tay nắm; Stops=[0,.08,.16,.24], cam RequiredPosition=.08.
- `COgheGearTrain`: tiếp xúc vòng chia thật, nguồn và ly hợp; thanh răng nâng cửa. ReturnWhenDisconnected=true: lò xo kéo đóng khi mất kết nối.
- Không có dao/pad ở màn này.
- `COgheTapGatePresentation` và `COgheTapStopPresentation` chỉ đọc trạng thái; không thêm collider hay điều kiện thắng.

Hình học động và lỗ dùng cơ chế invalidation hiện có; input và sensor không dựa vào số màn. ID hạt/khối lượng vẫn do solver sở hữu; hành trình kết thúc theo vị trí và tốc độ thật.

## 6. Hình ảnh và phản hồi

Bộ Day Lab hiện có: gear/cần amber, kim loại nhạt, pad có cap hạ theo tải, đường truyền và chốt nhìn thấy; exit mint phẳng. Xúc tu đọc lực thao tác. Chốt kế được chỉ bằng marker, số 1–4 vẫn đọc được; ảnh thật ở evidence chapter. Lời nhắc ngắn tách trạng thái đang thao tác, cửa mở và cần nhập.

## 7. Độ khó và chi phí

Một vai; suy luận thứ tự và vị trí cơ quan. Bốn trạng thái nhìn thấy; có đường sửa bỏ lỡ chốt. Sai chạm hoặc không nhìn được không được coi là độ khó mong muốn. Thời gian giải, chạm sai, retry: chưa đo với người mới. Giữ 32 hạt/120 Hz, ánh sáng/material chung, không tăng physics solver hoặc hạt. Chưa định lượng ngân sách GC/GPU/mobile; test cần ghi khung chạm và khi nhiều cơ quan hoạt động.

## 8. Chơi thử và hồi quy

Ca: giải trọn bằng screen picking/normal player input; pause/retry; chốt/khóa; đảo hành trình, phục hồi sai thứ tự; UI portrait; full PlayMode và EditMode. Bằng chứng ghi trong `Docs/COGHE_TAP_CHAPTER_VERIFICATION_2026_09_23.md`, không lấy kiểm chứng A/B cũ làm kết quả màn mới.

## 9. Hiệu năng thiết bị

Chưa đo điện thoại, nhiệt hoặc lượt 15–20 phút. Mac replay kiểm tra integration/input, không chứng nhận FPS điện thoại. Chưa có novice-playtest; ghi thời gian chờ cơ quan tách khỏi thời gian suy nghĩ trong phiếu.

## 10. Tích hợp và nghiệm thu

Danh sách TapCampaign gồm 01–10, mỗi ID riêng, không đổi save schema `venom.origin.v2`. Nút tiếp theo đi đúng Tap08. Origin được giữ để hồi quy. Bản build riêng `Builds/COgheTapChapter`. Kết luận: lời giải/screen input và hồi quy đã qua; Mac 10/10, full PlayMode 382/382, EditMode 8/8. Novice/device là bước tiếp theo của người dùng.

Bằng chứng nghiệm thu kỹ thuật: [báo cáo chapter](../../COGHE_TAP_CHAPTER_VERIFICATION_2026_09_23.md), build `91afe91551bf45c9aea0ed7ab35c14ef`. Không chứng nhận mobile performance hoặc novice comprehension.
