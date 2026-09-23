# COghe — hồ sơ coghe.tap.v1.01 — Khớp nối

Nguồn: [hợp đồng prototype](../../COGHE_TAP_CAMPAIGN.md), [luật Origin](../../COGHE_LEVEL_DESIGN_RULES.md), [Day Lab](../../ArtDirection/COghe/STYLE_RULES.md). Theo cấu trúc [LEVEL_TEMPLATE](../COghe/LEVEL_TEMPLATE.md).

## 1. Định danh, phạm vi và trạng thái

ID `coghe.tap.v1.01`, vị trí 1/5. Scene `Assets/_Game/Venom/TapCampaign/COgheTap01.unity`, definition `Tap01.asset`. Builder `COgheTapCampaignBuilder.cs`. Ngày 23/09/2026, phiên bản thiết kế 1. User chốt hướng qua Buzz event `553837bdccdd4729a2e978fd25fb4d5bd28d5a1cab799562e4e445c54da3bed4`. Trạng thái **Prototype — đã qua kiểm chứng kỹ thuật trên Mac**. Các cổng nghiệm thu input/gameplay/art/mobile chỉ đạt khi có bằng chứng riêng.

## 2. Mục tiêu trải nghiệm và tiến trình

Một chạm tự hoàn tất và cơ khí truyền tới cửa. Chưa yêu cầu kỹ năng trước. Vai trò: giới thiệu/luyện thao tác. Lời hướng dẫn ngắn được phép ở năm màn mở đầu; đây không phải Boss. Giữ giải đố thuần, không nâng chỉ số hoặc đồ mua để giải.

## 3. Bố cục, hình học và điều khiển

Bố cục: spawn trái trước; A giữa; cửa bên phải sau. Phòng kích thước .8 × .6 m, sàn y=-.30, nóc y=.02. Camera 55°/25°, điện thoại dọc; nóc vẫn va chạm nhưng không chặn lựa chọn từ trên. Khóa xoay toàn màn. Vật lý: 32 hạt, trọng lực thế giới. Chạm cơ quan giao một hành trình; chạm mặt sàn giao đường đi; chạm phần khác đổi chọn. Vị trí đo cụ thể và điểm đứng/tay nắm do builder khai báo. Bản vẽ bằng quan hệ dưới đây; ảnh/render thật trong artifact kiểm chứng sẽ bổ sung.

```text
Chạm A -> ăn khớp -> cửa nâng -> chạm cửa. Chạm A lần hai rút gear và cửa hồi đóng.
```

## 4. Lời giải, kết thúc và phục hồi

Chạm A → ăn khớp → cửa nâng → chạm cửa. Chạm A lần hai rút gear và cửa hồi đóng.

Thắng khi toàn bộ mô nhập bên trong rồi đủ 32 hạt qua đúng lỗ cuối. Nếu một phần thoát trước nhập: thua với `bạn phải hợp thể trước khi chui ra`. Dao không ép tỷ lệ 50/50. Tụ quá sớm có thể quay về dao; phần quá nhỏ thì nhập và cắt lại. Cơ quan hai trạng thái cho phép đảo lại. Chạm dồn không tích lệnh. Pause đóng băng thời gian; Retry trả mọi cơ quan/mô/lệnh về đầu. Phải kiểm tra đường gọi phần bị bỏ lại về, không chỉ mở được cửa.

## 5. Cơ quan và kiến trúc dùng lại

Một rail có hai chốt, bộ truyền 3 bánh và cửa hồi. Runtime mới: `COgheTapRail`, `COgheTapPad`, mở rộng tùy chọn `COgheGearTrain`; dùng lại rail, sensor, guillotine và mô hiện có. Dữ liệu cảnh quyết định liên kết; runtime không đọc lời giải hoặc số màn để phát lực. Điểm dừng đo từ Rigidbody; cập nhật route khi hình học đổi. Tác vụ thuộc anchor hạt/lệnh, không thuộc phần đang chọn trên UI. Tụ vẫn theo luật lệnh mới nhất; không bảo vệ tác vụ để cấm nhập.

## 6. Hình ảnh và phản hồi

Giữ Day Lab: sinh vật đen mềm, cơ quan hổ phách, khung nhôm và cửa mint. Nguồn, gear, trục và rack phải liên tục; pin khóa hiện trạng thái liên động. Cửa hồi và cửa có chốt phân biệt bằng cơ cấu. Skin tiếp cận/bám/đẩy/kéo đọc hành vi thật. Không mở cửa chỉ vì hết animation. Kiểm tra toàn cảnh, close view và lúc hai phần cùng hoạt động. Chưa nghiệm thu chỉ từ concept.

## 7. Độ khó và chi phí

Độ khó mục tiêu: một chạm tự hoàn tất và cơ khí truyền tới cửa. Độ chính xác chạm/thời điểm thấp; mở đầu không căn xoay. Thời gian suy luận và thao tác đo riêng; chưa có số liệu người mới. Dùng cùng solver/120 Hz/32 hạt, không đổi luật theo chất lượng máy. Ngân sách CPU/GPU/GC cần đo trên thiết bị mục tiêu trước cam kết.

## 8. Chơi thử và hồi quy

`COgheTapCampaignTests` chạy cùng toàn bộ package PlayMode. Kiểm tra screen command, chiều thuận/ngược, blocked path, pause/retry, idle, tap spam, chọn phần, cut/merge, đường thoát. Mô di chuyển bằng lực/điều khiển thật; không đặt thẳng vào cơ quan. Kết quả từng ca ở `Artifacts/COgheTap/playmode-*`; kết quả chưa đọc XML không ghi đạt. Màn 04 cho phép đi thẳng ra nếu người chơi bỏ qua bài luyện; theo dõi hành vi đó trong novice playtest.

## 9. Thiết bị và bằng chứng hiệu năng

Chưa kiểm chứng điện thoại. Bản Mac kiểm tra chức năng/render không thay FPS trên mobile. Khi có phép đo, ghi SoC/OS/build hash, profile, sample window, frame p50/p95/p99/max, allocation và nhiệt. Artifact phải gắn exact HEAD+diff và thời gian chạy.

## 10. Tích hợp và nghiệm thu

Scene thuộc danh sách riêng năm màn, giữ ID ổn định; next không nhảy sang Origin06. Completion mới ghi cùng save bằng ID riêng, không xóa progress/Home cũ. Không cấp quyền Boss từ prototype. Generator sở hữu scene/asset mới, không ghi đè hồ sơ Origin. Nghiệm thu kỹ thuật: xem [báo cáo cuối](../../COGHE_TAP_VERIFICATION_2026_09_23.md). Người mới/điện thoại vẫn cần bằng chứng riêng.
