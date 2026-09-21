# Màn 32 — sửa điểm chạm cửa thoát

Mrk báo không bấm được lỗ cuối sau khi đưa cầu từ A trở về B.
Kiểm chứng trên Unity **6000.3.19f1**, worktree `codex/venom-macos-preview`,
commit nền `b8594b2b4d70a39b401c6420c425af7fa4e64d6a` cộng diff sửa lỗi này.

## Nguyên nhân và thay đổi

Đã tái hiện bằng vật lý thật: đưa cầu tới A, qua cầu, kéo L để mở E,
trở về bệ giữa, trả cầu về B, rồi chạm tâm cửa thoát. Với camera cũ
`(24,12,0)`, tia chạm chọn `A B sealed partition`, không tạo lệnh thoát.
Màn khóa xoay và không có camera khoang để đổi hướng nhìn.

Đổi camera riêng màn 32 thành `(24,-24,0)` để thấy lỗ trên vách bên phải.
`BuildCampaign32` và `Slot32.asset` dùng cùng cấu hình; Editor method
`VenomCampaignBuilder.RefreshCampaign32Camera` cập nhật definition bằng
`SerializedObject` và `AssetDatabase`, không dựng lại hình học.

Không đổi cơ quan, collider, luật vật lý, thắng/thua hay quy tắc chạm xuyên vách.
Khi còn giữ tay cầu, bấm **Buông vật** rồi chạm lỗ. Đây là thao tác điều khiển
hiện có, giữ nguyên sau bản sửa.

## Kiểm thử bắt lỗi

Kiểm thử cũ đi hết màn bằng lệnh world-space; kiểm thử input chỉ xét tay nắm
ở trạng thái đầu. Vì vậy kết quả cũ không chứng minh cú chạm cửa sau đổi cầu.
Ca tái hiện mới chạy trên bản camera cũ đã thất bại đúng tại bước này;
**342/343** ca PlayMode còn lại đạt. Log ghi:

```text
LEVEL32_EXIT_TAP (480, 800) selected=False
face=A B sealed partition
hits=Laboratory front@1.3679; A B sealed partition@2.0707
```

Sau sửa, `COgheCampaign40RouteTests` chạy toàn bộ lời giải màn 32 qua
`TouchPoint`: chạm tay, hướng kéo, điểm đi trên bệ, tay L, kéo cầu về B,
rồi chạm thẳng cửa từ bệ giữa. Chọn phần và buông dùng cùng API của HUD.
Không đặt vị trí mô, không ép trạng thái cửa hoặc thắng. Cả ba tỷ lệ
**480×800, 720×1280, 720×1612** đã thoát đủ **32/32** hạt và reset sau thắng.
Kiểm thử tay nắm ở trạng thái đầu cũng đạt với góc camera mới.

Các phép thử tự động này không thay cho chơi tay trên điện thoại hoặc đo
hiệu năng mobile. Kết quả hồi quy đầy đủ và bản Mac được ghi ở phần nghiệm thu.

## Bằng chứng

Đường dẫn tương đối từ root repo:

- `Artifacts/level32-exit-20260921/baseline/`: ảnh lỗi và patch ca tái hiện.
- `Artifacts/level32-exit-20260921/red-playmode/`: 343 ca, lỗi duy nhất là chạm cửa màn 32.
- `Artifacts/level32-exit-20260921/camera-update/`: log cập nhật definition qua Unity.
- `Artifacts/level32-exit-20260921/green-playmode-01/`: toàn bộ hồi quy sau sửa.
- `Artifacts/level32-exit-20260921/test-source-sha256.json`: mã nguồn/runtime/scene được đối chiếu.

## Nghiệm thu — 21/09/2026

| Kiểm tra | Kết quả | Bằng chứng trong `Artifacts/level32-exit-20260921/` |
| --- | --- | --- |
| Toàn bộ PlayMode | **345/345**, 0 lỗi, 0 bỏ qua | `green-playmode-01/TestResults.xml` |
| Toàn bộ EditMode | **8/8**, 0 lỗi | `editmode/TestResults.xml` |
| Mac Development | **Build Success**, arm64 + x86_64 | `build-macos/Editor.log` |
| Chạy trong Mac player | **Thắng**, 32/32 hạt, 1 phần, không thua | `player/20260921T154548798Z/run.json` |
| Lỗi runtime trong replay | `runtimeErrors: []` | Cùng run manifest |
| Ảnh framebuffer thật | Trước thoát và thắng, **480×800** | `player/20260921T154548798Z/level-32-open.png`, `level-32-won.png` |

Lượt player chạy trên Mac16,10 / Apple M4. Replay sinh trực tiếp từ test đã đạt,
có manifest SHA256 và build guard chống dùng kịch bản cũ. Đây là lệnh chạm màn
hình tự động với vật lý 120Hz, chưa phải thao tác tay trên điện thoại. Save bị
cô lập trong toàn bộ replay; app kiểm chứng tự đóng khi xong.

Đối chiếu **128** hash nguồn kiểm thử trước build; toàn bộ **6.020** file trong
`Assets`, `Packages`, `ProjectSettings` giữ đúng hash manifest build sau khi dựng
và chạy app. Các scene 01–40 không đổi. Ba material PhysicsLab bị Unity import
lại ngoài phạm vi đã được lưu bản chênh lệch và phục hồi trước build.
Runner có cảnh báo licensing trong phần tổng hợp, nhưng Unity trả mã 0;
XML và log `Build Finished, Result: Success` là bằng chứng kết quả thực tế.

Bản app: `Builds/Venom/macOS/Venom.app`.
Bundle chia sẻ tại `/Users/tommynguyen/.buzz/OUTBOX/COGHE_LEVEL32_EXIT_FIX_2026_09_21/`.
Code và app đã cập nhật local trong worktree; lần sửa này chưa commit/push.
