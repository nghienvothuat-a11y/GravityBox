# COghe — coghe.tap.v1.06 — Đón bạn trở về

Nguồn: [luật level](../../COGHE_LEVEL_DESIGN_RULES.md), [Day Lab](../../ArtDirection/COghe/STYLE_RULES.md), [chapter](../../COGHE_TAP_CAMPAIGN.md). Hồ sơ theo các mục của LEVEL_TEMPLATE. Ngày 23/09/2026; Mrk yêu cầu thêm 06–10 để user thử chung, event `185e1e5d67f04a4c4d00919d9960accc7afb7cc4b3ef749ef9cb45155dfcf8d0`.

## 1. Định danh và trạng thái

ID `coghe.tap.v1.06`, thứ tự 6, scene `Assets/_Game/Venom/TapCampaign/COgheTap06.unity`, definition `TapCampaign/Definitions/Tap06.asset`. Builder `COgheTapCampaignBuilder.cs`. Phiên bản 1. Trạng thái: **Prototype — gameplay/input đã qua kiểm chứng Mac**; xem báo cáo chapter để biết kết quả thực. Chưa có novice/mobile evidence. Không thay ID hoặc completion của Origin/01–05.

## 2. Mục tiêu và tiến trình

Luyện phối hợp. B cần tải A; C cần B tới cuối; hai bánh răng nối tiếp mới truyền đến cửa.
Lời nhắc đọc trạng thái thực; không ra lệnh hoặc mở khóa hộ. Không cần mua đồ/nâng cấp.

## 3. Phác thảo, camera và thao tác

Nhìn từ trên, x trái→phải; z trước→sau, đơn vị mét:

```text
sau z=+.30
  pad x=-.28,z=.08    motor x=-.15,z=.13 -- A/B x=-.04 -- C/B x=.07 -- đầu ra -- nắp thoát x=.25,z=.18
  dao x=-.177,z=-.12    ray hai đầu, hành trình .15
  vùng trước dành cho tiếp cận và tụ
spawn (.20,-.275,-.20); vùng tụ (.22,-.30,-.15)
trước z=-.30
```

Hộp .8×.6 m, sàn y=-.30; lỗ thật bán kính .041, nắp nâng .14. Góc đầu 55°/25°, camera overview/follow sẵn có, khóa xoay. Nóc không chọn; sàn vẫn nhận đích. Các mốc portrait: 720×1280, 720×1612. Nhãn số nằm bên ray, chấm mint chỉ chốt kế; không chỉ thứ tự giải. Chạm cơ quan giao một hành trình; chạm phần để chọn; chạm sàn giao di chuyển. Chạm dồn không xếp hàng, pause/retry giữ nguyên cơ chế chung.

## 4. Lời giải và phục hồi

Lời giải cho tác giả/test (runtime không đọc): **Giữ bàn đạp A → lắp B → lắp C → cửa chốt → nhả A → nhập → thoát.**

Làm sai thứ tự: khóa cơ khí từ chối, không thay trạng thái. Có thể chạm đảo cơ quan hoặc retry.

Nếu tụ sớm, về dao chia lại. Cắt lệch giữ nguyên khối lượng thật; phần quá nhỏ chưa đủ lực có thể về nhập rồi cắt lại. Chọn phần khác không nhả bàn đạp. Khi cửa đã chốt, chọn phần giữ và chạm lại pad để ra điểm an toàn; đưa cả hai về sàn trước. Nhả tải sớm khóa nguồn/khóa tác vụ và phải giữ lại để tiếp tục.

Thắng khi một cơ thể đã nhập trong hộp rồi đủ 32 hạt ra lỗ cuối. Một phần thoát sớm: thua `bạn phải hợp thể trước khi chui ra`. Không có cổng transfer. Vật bị cản không xuyên vật/hoàn thành giả; lệnh báo kẹt, sửa trạng thái hoặc retry. Retry xóa lực/tác vụ, trả cơ quan và mô về cấu hình đầu.

## 5. Cơ quan và kiến trúc

- `COgheTapRail` + `COgheRailSlider`: lực hữu hạn, điểm đứng tách tay nắm; hai chốt đầu/cuối.
- `COgheGearTrain`: tiếp xúc vòng chia thật, nguồn và ly hợp; thanh răng nâng cửa. LatchOutput=true: chốt thật khi tới cuối cho phép thu hồi phần giữ.
- `COgheGuillotine`, `COgheTapPad`, `COgheTissueSensor`: dao tách bond thật; sensor đo tải; không khóa nhập bằng nhiệm vụ/cooldown.
- `COgheTapGatePresentation` và nhãn A/B chỉ đọc trạng thái; không thêm collider hay điều kiện thắng.

Hình học động và lỗ dùng cơ chế invalidation hiện có; input và sensor không dựa vào số màn. ID hạt/khối lượng vẫn do solver sở hữu; hành trình kết thúc theo vị trí và tốc độ thật.

## 6. Hình ảnh và phản hồi

Bộ Day Lab hiện có: gear/cần amber, kim loại nhạt, pad có cap hạ theo tải, đường truyền và chốt nhìn thấy; exit mint phẳng. Xúc tu đọc lực thao tác. Chốt kế được chỉ bằng marker, số 1–4 vẫn đọc được; ảnh thật ở evidence chapter. Lời nhắc ngắn tách trạng thái đang thao tác, cửa mở và cần nhập.

## 7. Độ khó và chi phí

Hai vai đồng thời; không yêu cầu bấm đồng thời hoặc căn thời gian. Chỉ hai trạng thái mỗi cơ quan. Sai chạm hoặc không nhìn được không được coi là độ khó mong muốn. Thời gian giải, chạm sai, retry: chưa đo với người mới. Giữ 32 hạt/120 Hz, ánh sáng/material chung, không tăng physics solver hoặc hạt. Chưa định lượng ngân sách GC/GPU/mobile; test cần ghi khung chạm và khi nhiều cơ quan hoạt động.

## 8. Chơi thử và hồi quy

Ca: giải trọn bằng screen picking/normal player input; pause/retry; chốt/khóa; ngăn lách với một phần, nhả tải, gọi về và nhập; UI portrait; full PlayMode và EditMode. Bằng chứng ghi trong `Docs/COGHE_TAP_CHAPTER_VERIFICATION_2026_09_23.md`, không lấy kiểm chứng A/B cũ làm kết quả màn mới.

## 9. Hiệu năng thiết bị

Chưa đo điện thoại, nhiệt hoặc lượt 15–20 phút. Mac replay kiểm tra integration/input, không chứng nhận FPS điện thoại. Chưa có novice-playtest; ghi thời gian chờ cơ quan tách khỏi thời gian suy nghĩ trong phiếu.

## 10. Tích hợp và nghiệm thu

Danh sách TapCampaign gồm 01–10, mỗi ID riêng, không đổi save schema `venom.origin.v2`. Nút tiếp theo đi đúng Tap07. Origin được giữ để hồi quy. Bản build riêng `Builds/COgheTapChapter`. Kết luận: lời giải/screen input và hồi quy đã qua; Mac 10/10, full PlayMode 382/382, EditMode 8/8. Novice/device là bước tiếp theo của người dùng.

Bằng chứng nghiệm thu kỹ thuật: [báo cáo chapter](../../COGHE_TAP_CHAPTER_VERIFICATION_2026_09_23.md), build `91afe91551bf45c9aea0ed7ab35c14ef`. Không chứng nhận mobile performance hoặc novice comprehension.
