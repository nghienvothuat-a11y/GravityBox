# COghe — 24: Kéo ra mới qua

> **Rà soát v3 — 18/09/2026, màn hiển thị 14:** đã sửa hình học và kiểm chứng lại
> đường giải. [Báo cáo và ảnh Unity](../../../Verification/COgheCampaign30/review-14-16-18.md)
> ghi trạng thái hiện hành; các ước lượng/Nháp bên dưới là lịch sử phác thảo.
> Chưa kiểm thử lại trên OPPO ở lượt này.

**Prototype Unity v2, 17/09/2026.** ID ổn định `venom.origin.24`, vị trí chơi **14**,
sau nắp rơi cũ 09 và trước kê thùng cũ 11. Ray nắp, chốt mở lỗ, reset, camera chạm
và ngân sách mô phỏng đã qua test macOS; chưa playtest tay mobile.
[Tiến trình](../../../COGHE_CAMPAIGN_30_DESIGN.md) · [Quy chuẩn](../../../COGHE_LEVEL_DESIGN_RULES.md) ·
[Day Lab](../../../ArtDirection/COghe/STYLE_RULES.md).

## 1. Mục tiêu và phác thảo

![Phác thảo 24 · Kéo ra mới qua](mockup-v1.png)

[Xem cả 10 bản phác thảo](../Sketches21-30/review.html). Bản v1 để duyệt bố cục và thao tác;
chi tiết tỷ lệ, vách kín, ray/chốt và tiếp xúc cơ khí được giản lược, cần kiểm chứng khi dựng.

Một màn nghỉ tay xoay: hiểu rằng cùng một tay nắm có thể kéo ngược để mở lối.
Mặt nắp che lỗ nối với ray ngang, có hốc chứa rõ ở bên trái; phía phải là chặn thật.

```text
NHÌN MẶT CƠ QUAN TỪ TRONG HỘP
 ┌─────────────────────────────────────────────┐
 │ [hốc chứa trống] ← ray ── [nắp che O]│chặn   │
 │                          └ tay nắm ─┘        │
 │        vùng lùi để kéo                       │
 │  S / sàn có bám ──────────────────────────── │
 └─────────────────────────────────────────────┘
```

- Tay nắm ở mặt trước nắp, không nằm giữa nắp và kính. Nắp trượt song song mặt vỏ,
  không nhô một cạnh ra để mô bị kẹp. Độ hở ray không tạo lối thoát phụ xuyên vỏ.
- Hốc chứa chừa khe thao tác thật để tay nắm còn lộ ở mọi vị trí, kể cả nắp vào hết.
  Không cho nắp giấu mất tay khiến kéo ngược chỉ còn thực hiện được bằng lệnh từ xa.
- Nắp kín thật che O; có vùng trống/hốc nhận cùng chiều ray làm tín hiệu. Cửa có
  chốt ở vị trí mở, đủ không gian để toàn cơ thể buông tay rồi bò tới O.
- Không có khóa màu, nút đếm thao tác hoặc yêu cầu đẩy sai trước. Người hiểu có thể
  kéo đúng ngay. Hộp khóa xoay; góc camera thấy cả hốc, tay nắm và lỗ khi lộ ra.

## 2. Lời giải và phục hồi

1. Chạm tay nắm; COghe bám. Chạm vùng sàn bên trái để kéo nắp về hốc.
2. Nắp tới chốt, lỗ tròn lộ hoàn toàn. Thả tác vụ/buông rồi chạm O để thoát.

Chạm bên phải khi đang bám khiến nắp gặp chặn; thân thể hiện tì và vật không xuyên.
Chạm lại bên trái đổi hướng kéo mà không cần Retry. Buông giữa chừng thì bám lại;
không cho nắp tự trôi đóng vào mô. Hướng kéo xét điểm đích trong hệ mặt ray, không
phụ thuộc nhầm trục camera. Đích rộng, không kéo trực tiếp nắp bằng gesture từ xa.

## 3. Trạng thái và an toàn đường giải

`Covered → PartlyOpen → LatchedOpen → FullBodyExit`.
Pull ngược có thể nhả chốt nếu đúng luật ray; việc đóng lại phải có contact với mô,
không cắt hoặc nghiền mất hạt. Reset hủy lệnh, đặt nắp/COghe đầu màn, mở phanh đúng.

Đường vòng trên tường vẫn tới mặt cửa nhưng tấm nắp kín ngăn thoát trước khi kéo.
Lỗ đủ rộng cho bản thể sau mở; không để lối phụ qua phía sau hốc. Vật liệu trơn không
cần cho câu đố này. Không dùng lực nặng để ép thao tác lâu hoặc cần chia cơ thể.
Luật hợp thể và toàn bộ vật chất qua cửa cuối vẫn áp dụng; không có Transfer/dao.

## 4. Cơ quan, art và animation

Dùng `VenomMovableProp` tay nắm riêng, `COgheRailSlider` horizontal + chốt cuối,
surface patches trên phần cần bám. Lực cơ thể thật di chuyển nắp; không có động cơ
hoặc liên kết cửa từ xa. Cần cấu hình ma sát/khối lượng giữ đứng khi buông, không
giả định collider tĩnh sẽ làm được một vật động.

Nắp sứ có cạnh hổ phách để thấy là một bộ phận rời, ray thép mảnh. COghe vươn xúc tu,
thân kéo căng và lùi theo tải; khi nắp mở lộ viền lỗ mint. Không vẽ mũi tên lời giải
liên tục; hình hốc và ray đủ đọc, chạm có vòng đích và phản hồi chọn tay nắm.

## 5. Độ khó và kiểm chứng

Một cơ quan động, một cơ thể, một suy luận hướng kéo, không canh thời gian. Mục tiêu
“đẩy nhầm cũng sửa ngay được”, không kiểm tra độ khéo tại bước cuối.

Test từ vị trí nắp giữa ray, đẩy vào chặn, kéo ra, đổi lệnh, timeout 3 giây, pause,
reset, camera/tap vùng rộng, nắp đóng khi COghe gần mép, không có lối lọt trước mở.
Đo thời gian một hành trình và số lần cần bám lại ở người đã biết lời giải.
Profile vẫn 32 hạt/120 Hz; đo trên OPPO p95/p99/max và skin/graph khi cửa đổi chỗ,
không xem ít object là bảo đảm FPS. Chưa có test pass hoặc số đo cho màn này.

## 6. Tích hợp và trạng thái

Scene/definition/builder đã tạo với ID 24 độc lập display 14. Tuyến kéo nắp vào hốc
rồi thoát đã qua full-solution test và reset/integration test; cảm giác kéo và mobile
performance còn cần người chơi kiểm tra trên thiết bị.
