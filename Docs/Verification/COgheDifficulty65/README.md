# COghe — Kiểm chứng nhịp học 65 màn

Revision 15/10/2026, từ `627298b7`, nhánh NewGraphic.
[Thiết kế và bảng thứ tự](../../LevelDesign/COghe/Difficulty65/README.md).

## Phạm vi

- Giữ đủ 65 stable content ID và sáu Boss ở 10/21/32/43/54/65.
- Đặt bài luyện thang/cầu ở 8–9; luyện ống, đu, truyền lực và phần tư trước bài kết hợp.
- **Boss 10 giữ nguyên theo yêu cầu mới nhất.** Scene, builder, replay, tests và
  các mesh mà scene tham chiếu giống bản trước chỉnh sửa. Definition chỉ cập nhật
  `SceneSequence` dùng chung; không đổi hình học, cơ quan hay chuỗi giải.
  [Đối chiếu hash](boss10-preserved.json). Đợt cải thiện thao tác kế tiếp chỉ thêm
  `TouchControlBody` cho bốn công tắc của Boss 10, không đổi bố trí/chuỗi giải.
- Ngoài Boss 32, 27 scene được sửa chỉ đổi `m_Name`/`m_Text` của số plaque.
  Không có sửa vật lý ở những scene này.
- Boss 32 thêm chuyển người giữ P → D, khóa C cần B và D, rồi C mở A. A giữ cầu
  và mở lỗ cuối. Có thể bỏ nhầm D rồi quay lại để tiếp tục; chưa hợp thể vẫn không
  được thoát. Lỗ được dời khỏi đường về của cần A để tránh hút nhầm nửa thân;
  nắp trượt vào trong, không chạm vách. Xoay camera để nhìn dưới kệ phải trước
  khi chỉ vào lỗ. [Hồ sơ Boss 32](../../LevelDesign/COghe/Difficulty65/BOSS32.md).

## Save cũ

Màn đã qua và thưởng lần đầu vẫn dựa trên ID ổn định. Một lần migration ghi lại
những mốc mở đồ đạt được theo thứ tự cũ, nên người đã mở tiêm màu ở màn 8 cũ không
bị khóa lại khi cập nhật. Không tự giải màn mới ở vị trí 8, không cộng lại Drops,
không reset màu, bước học hoặc Nhà. Mốc mở đồ mới vẫn theo thứ tự mới.

## Bằng chứng native

Boss 32 đã giải trong player Mac bằng touch events và drag camera, vòng Update
thật: 32/32 hạt thoát, hợp thể thành một phần, không lỗi.
[Kết quả](native-boss32.json), [ảnh đầu](boss32-start.png),
[đổi vai](boss32-relay.png), [thắng](boss32-won.png).
Đây là author replay qua bộ điều khiển prototype; ảnh có bộ chọn debug, không
phải UI Product. Không dịch chuyển mô hoặc ép trạng thái thắng.

Lượt cuối chạy riêng trong player Mac sau khi dời lỗ, không chạy song song với
Unity Editor tests. Build có công cụ kiểm chứng; không dùng để kết luận FPS mobile.
Độ khó cảm nhận cần playtest người chưa biết lời giải, nhất là 8–10 và 29–32.
Số chạm/thời gian của lời giải tác giả không thay cho bằng chứng đó.

## Kiểm tra tự động

- **65/65 lời giải hoàn tất**, đủ 32 hạt thoát. [Dữ liệu replay](levels.jsonl).
  64 màn giữ kết quả lượt toàn bộ; hàng Boss 32 dùng lượt cuối sau sửa vị trí lỗ.
- **46/46 Product UI tests**, gồm save cũ, bài tiêm màu, thưởng, popup, layout.
- Boss 10: solve, đi tắt/đảo cầu, wander và Retry đều đạt; bản thiết kế giữ nguyên.
- Boss 32: **7/7 lượt cuối đạt** — solve, 30 FPS, 60 FPS, sai thứ tự/bỏ D rồi
  quay lại, đi tắt kính, wander và replay metrics. Đây là các nhịp Update mô phỏng
  trong Editor; không phải phép đo FPS điện thoại.
- [Lịch sử và kết quả mới nhất của 58 test case](tests.json). Lượt đầu có hai lỗi
  30/60 FPS do đường về đi sát lỗ cũ. Đã sửa vị trí lỗ, hướng trượt nắp và góc
  quan sát lỗ dưới kệ; lượt cuối kiểm tra lại toàn bộ các ca Boss 32 nói trên.

`Tools/check-coghe-curriculum.py` kiểm tra đủ ID, Boss cố định và 10 chuỗi học;
đây là kiểm tra dữ liệu, không thay kiểm tra physics.

Audit mạch in sau build: **65 scene, 326 renderers, 65.868 điểm lấy mẫu,
0 điểm không có mặt đỡ, 0 lỗi**.

## Chạy lại

`Gravity Box → COghe → Spatial → Apply 65 level pacing` dựng lại G5, thứ tự,
plaque và catalog; không dựng lại Boss 10. Bộ test `COgheDifficulty65Tests`
kiểm tra khóa sai thứ tự và phục hồi sau bỏ người giữ; `COgheLevelMetrics`
chạy mọi lời giải và fail nếu có một màn không hoàn tất.
