# COghe — 22: Đáp rồi chui

**Prototype Unity v3, 18/09/2026.** ID ổn định `venom.origin.22`, vị trí chơi **11**,
sau Boss mới và trước màn cũ 08. Đã kiểm tra load/reset/idle, ống tự nhận tiếp xúc và
ngân sách mô phỏng trên macOS; chưa playtest tay mobile. Đây là nhịp nghỉ có
cú rơi ngắn, không tăng tốc độ phản xạ. [Tiến trình](../../../COGHE_CAMPAIGN_30_DESIGN.md) ·
[Quy chuẩn](../../../COGHE_LEVEL_DESIGN_RULES.md) · [Day Lab](../../../ArtDirection/COghe/STYLE_RULES.md).

## 1. Mục tiêu và phác thảo

![Phác thảo 22 · Đáp rồi chui](mockup-v1.png)

[Xem cả 10 bản phác thảo](../Sketches21-30/review.html). Bản v1 để duyệt bố cục và thao tác;
chi tiết tỷ lệ, vách kín, ray/chốt và tiếp xúc cơ khí được giản lược, cần kiểm chứng khi dựng.

Cho thấy cơ thể mất bám, được vùng đón thật giữ lại, rồi chảy thành dòng qua ống.
Người chơi chọn một vùng đích rộng trước cú rơi; không phải điều khiển khi đang rơi.

```text
NHÌN CẠNH — đã bỏ mặt kính gần camera, chỉ trong sơ đồ
 ┌──── hộp A ─────┐                     ┌──── hộp B ────┐
 │ bệ leo S ── P  │                     │              │
 │          ╲ ↓ ╱│  P = mép trơn rộng  │              │
 │           (C) ╞════ ống ngắn ═══════╡ R            O│
 │      sàn an toàn│ C = vành bám thật   │ bệ hồi hình   │
 └────────────────┘                     └──────────────┘
  đường leo trở lại P                  O = lỗ cuối riêng
```

- Bệ tiếp cận có mặt bám, mép P trơn. Hai má dẫn cố định thật tạo hành lang rơi rộng
  về C; không đóng thành ống kín che mất cú rơi. Thử khoảng rơi 4–8 cm từ mép tới vùng
  bắt, chưa coi là kích thước chuẩn. Giữ một đoạn không tiếp xúc để nhìn thấy rơi.
- C có lòng ống đủ nhỏ để nhìn thấy thân biến dạng nhưng đủ cho nguyên bản thể đi
  qua bằng mô phỏng. Vành và má dẫn không được chặn chính miệng ống.
- Sàn hộp A có bám và đường leo về P thấy rõ. Không có cơ quan cần giữ hoặc timer.
- Xoay khóa; camera nghiêng bên hông theo trục ống, thấy P/C/ống và vùng ra R. Có
  nút xem hộp A/B nếu toàn cảnh khiến điểm chạm nhỏ; các nút không ra lệnh di chuyển.

## 2. Lời giải và tín hiệu

1. Chỉ lên bệ rồi vùng P. Sinh vật bò tới trước khi mất bám.
2. Cơ thể rơi ngắn, má dẫn giới hạn lệch bằng contact. Khi bám C thật, bộ thực thi
   căn miệng và chảy qua ống; không hút từ phía ngoài hoặc teleport.
3. Mô tập hợp ở R trong hộp B. Chỉ lỗ O để thoát nguyên bản thể.

Chạm O ngay đầu không tự giải chuỗi rơi thay người chơi. Điểm chạm gần P phải quy
về đúng mặt nhìn thấy; không chọn xuyên vỏ trước tới mặt sau không chủ ý.

## 3. Trạng thái, phục hồi và đường thay thế

`Approach → Falling → RealRimContact → EnterTube → BodyInB → FinalExit`.
Falling hụt C → sàn → leo lại P. Nếu đã bám ngoài vùng tự vào, phải có đường bò thật
tới miệng; không giữ một lệnh ở trạng thái chờ vô hạn. Sai lệnh trước rơi được đổi.

- Vành auto-enter hiện có điều kiện contact/tâm/tốc độ; cần đo với bố cục mới.
  Không chỉ tăng bán kính bám mà bỏ qua các điều kiện vào ống.
- Rơi hụt không thua. Khi biết đáp án, mục tiêu là thử lại bằng một chuỗi chạm rõ,
  không phải reset. Hủy/pause/reset trong ống phải giữ vật chất hoặc reset toàn màn đúng.
- Nếu người chơi bò trực tiếp tới C được bằng mặt thật thì cho qua; màn này ưu tiên
  cảm giác chảy vui, không khóa lỗ bằng cờ “đã rơi”. Phiên bản khó hơn là màn cũ 08.
- Ống A–B là Transfer, không tính thoát. Thắng chỉ tại O với toàn bộ cơ thể đã tụ.

## 4. Dùng lại và hình ảnh

Dùng `VenomTransferTube`/bắt vành hiện có, `VenomSurfacePatch`, camera vùng và dẫn
đường qua mặt. Cần kiểm chứng cấu hình, không giả định đổi tọa độ là auto-enter đạt.
Ống ngắn thẳng để đọc biến dạng trước khi gặp các ống uốn ở màn sau.

P tím, C có vùng bám hổ phách và viền cyan; ống cyan trong, cổ sứ. COghe căng mỏng,
chảy liên tục rồi gom lại ở R. Không dùng một tia VFX thay cho mô thực đi qua ống.
Niềm vui là thấy sự biến đổi, không thêm mini-game chạm đúng lúc.

## 5. Chi phí và kiểm chứng

Một cơ thể, hai hộp nhỏ, một ống, không khớp động. Cẩn thận nhiều mặt kính trùng
trong khung; batch chi tiết cố định, giới hạn vòng trang trí không có collider.
Mục tiêu độ khó nhẹ: một lựa chọn điểm tới và quan sát kết quả, không thao tác giữa rơi.

Test đích chạm giữa/lệch hai phía, contact C, bám ngoài vùng tự vào, rơi hụt quay lại,
đi ngược trong ống nếu luật hỗ trợ, retry khi nửa thân ở mỗi hộp. Đo tỷ lệ thành công
với đích lệch sau khi người chơi hiểu, số rơi hụt và thời gian hồi phục. Đo frame time
trên OPPO lúc mô ở giữa ống; chưa có FPS hoặc bằng chứng khả giải cho cấu hình mới.

Ghi riêng thắng bằng rơi bắt vành và thắng bằng bò trực tiếp tới vành. Tuyến thứ hai
không được dùng để chứng nhận cú rơi dễ điều khiển; đo dung sai tuyến rơi độc lập.

## 6. Tích hợp và trạng thái

Scene/builder/catalog đã tích hợp, ID không trùng `venom.origin.08`. Tuyến rơi bắt
vành–chảy toàn thân qua ống–thoát đã qua full-solution test. Mobile FPS và độ dễ căn
cú rơi vẫn cần playtest thiết bị; bằng chứng batch không thay cho kiểm tra cảm giác.


## 7. Sửa theo phác thảo tại vị trí chơi 11 — 18/09/2026

Người chơi báo không hiểu và không qua được màn. Đối chiếu bản v2 phát hiện cửa
ống đặt vào giữa hộp A, hai vỏ dùng chung một khung trang trí lớn; các bệ và vùng
rơi gần như trong suốt. Test cũ ra lệnh thẳng tới cửa ống, nên không xác nhận được
chuỗi chạm mép tím → rơi → bắt vành mà phác thảo yêu cầu.

Bản v3 giữ nguyên mục tiêu và luật xoay khóa, dựng lại bố cục:

- Hai hộp riêng, cạnh 44 cm, tâm tại x = ±36 cm; khe nối 28 cm. Ống xuyên đúng hai
  mặt kính tại x = ±14 cm, y = −4,5 cm, z = −8 cm. Miệng ống bán kính 4,2 cm.
- Bệ trái màu hổ phách ở y = 14 cm, có khối leo nối tới sàn. Mép tím ngắn dốc xuống
  y = 10,5 cm, cách đỉnh vành bám 4,5 cm. Hai má dẫn thật giới hạn lệch ngang.
- Vành bám màu hổ phách, bán kính ngoài 10,5 cm, đồng phẳng với kính. Auto-enter
  vẫn cần ít nhất hai tiếp xúc bám thật; không hút sinh vật từ bệ hoặc từ xa.
- Ống cyan và hai cổ sứ nhìn rõ dòng mô đi qua. Bệ mint ở hộp B đỡ cơ thể sau khi
  qua ống và nối tới lỗ cuối viền xanh. Ống chuyển hộp không tính thắng.
- Camera 26°/22°, có nút Hộp 1/Hộp 2 để xem gần mà không đổi trọng lực. Mũi tên
  nhẹ chỉ mép tím là dữ liệu `DepartureHint`, chỉ trình bày, không ra lệnh thay user.

Lời giải người chơi: **chạm mép tím có mũi tên → quan sát rơi/bám/chảy sang hộp 2
→ chạm lỗ viền xanh ở tường bên phải**. Không phải chạm thêm trong lúc đang rơi.
Nếu xuống sàn hộp 1, chọn lại bệ hổ phách để leo lên rồi thử lại.

Builder tái tạo riêng: `Gravity Box → COghe → Rebuild Campaign 11 · Catch and Flow`.
Kiểm chứng chi tiết và ảnh Unity xem [biên bản](../../../Verification/COgheCampaign30/README.md).

Kết quả bản v3: **20/20 test liên quan đạt**, gồm ba vị trí chạm mép, phục hồi từ
sàn và reset giữa ống. Đã build và chơi thắng trên Mac bằng hai lần chạm thật
(mép tím, rồi lỗ cuối); game tự chuyển sang màn 12. Chưa test lại trên OPPO.
