# COghe — khoảnh khắc: pháo giấy khi thắng, mở màn Boss, chọn màn để test

01/10/2026 · Theo yêu cầu của Mrk. Xem: [`boss-intro.mp4`](boss-intro.mp4) (13 s, có tiếng),
[`screens.jpg`](screens.jpg) (bản Mac: màn thắng có pháo giấy, Pause có nút test, lưới chọn màn, cảnh báo Boss).

## Pháo giấy khi thắng

- Hai ống pháo nhỏ ở hai góc dưới bắn lên (lệch nhau 0,12 s), khoảng 50 mảnh giấy màu theo bảng màu game (san hô, hổ
  phách, teal, xanh nhạt, ngà), lật và đung đưa khi rơi, mờ dần, hết sau ~3,6 s. Kèm tiếng "pốc" nhẹ và giấy sột soạt.
- Tinh tế: mảnh nhỏ (5–8 × 3–5 điểm), không phủ màn hình, không che chữ "Well done!".
- Code: `Assets/_Game/Venom/Runtime/Product/COgheConfetti.cs` (ảnh UI, chạy bằng code, không ParticleSystem). Gọi khi
  màn hình thắng hiện lên (`COgheProductUI.Update`).

## Mở màn Boss

Mỗi lần vào một màn Boss (không lặp lại khi Restart):

1. **Lặn vào hộp** (1,5 s): từ đúng góc nhìn của game, camera tiến vào trong hộp kính (chuyển từ orthographic sang phối
   cảnh bằng dolly-zoom nên không giật).
2. **Đi một vòng** (~1,8 s mỗi chặng): lướt sát tối đa 4 cơ quan nổi bật nhất (chọn tự động theo kích thước, đi theo thứ tự
   gần nhất), rồi tới lối ra. Đường đi là spline mượt; camera luôn ở trong vách kính.
3. **Lùi ra** (1,6 s): về đúng góc nhìn chuẩn của game.
4. **Cảnh báo** (2,7 s): dải sọc vàng–đen quét ngang màn hình, chữ đỏ "WARNING" và "VERY HARD LEVEL" chạy ngang rồi dừng
   giữa, viền đỏ nhịp đập, tiếng còi cảnh báo hai tông. Sau đó màn chơi bắt đầu.

- Màn chơi tạm dừng trong lúc này; chạm màn hình để bỏ qua phần đi vòng (vẫn xem cảnh báo).
- Code: `Assets/_Game/Venom/Runtime/Boss/COgheBossIntro.cs`; gọi từ `COgheProductUI` khi vào màn có `Definition.Boss`.
  Trong lúc chạy, `VenomCampaign` không điều khiển camera (`COgheBossIntro.OwnsCamera`).

## Chọn màn để test

- Tạm dừng → **Levels (test)** → lưới 01–50 (màn Boss tô cam, màn đã qua ghi "Done"). Chọn là vào màn đó ngay, không đổi
  tiến độ.
- Chỉ có trong bản test: Editor, bản Development, và bản build bằng các script hiện tại (thêm define `COGHE_TEST_TOOLS`).
- **Bản lên chợ:** build với biến môi trường `COGHE_STORE=1` (`COGHE_STORE=1 bash Tools/build-venom-android.sh --spatial`,
  tương tự iOS) → nút và màn hình chọn màn không được biên dịch vào game.

## Kiểm tra

- `COgheBossIntroTests`: màn Boss tạm dừng khi đi vòng, camera phối cảnh, tự kết thúc, trả đúng camera orthographic của
  game, không vật nào trong hộp bị dịch.
- Proof native trên Mac (`-coghe-product-proof`): thêm ảnh màn thắng có pháo giấy, Pause có nút test, lưới chọn màn, đi vòng
  Boss, cảnh báo, và màn Boss sau khi bàn giao.
- `RenderBossIntro` (explicit): khung hình 30 fps của toàn bộ phần mở màn Boss.
