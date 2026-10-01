# COghe — khoảnh khắc: pháo giấy khi thắng, mở màn Boss, chọn màn để test

01/10/2026 · Theo yêu cầu của Mrk. Xem: [`win-confetti.mp4`](win-confetti.mp4) (5 s, bản Mac, có tiếng),
[`boss-intro.mp4`](boss-intro.mp4) (10 s, có tiếng; bản nhanh hơn),
[`screens.jpg`](screens.jpg) (bản Mac: màn thắng có pháo giấy, Pause có nút test, lưới chọn màn, cảnh báo Boss).

## Pháo giấy khi thắng

- Nổ ngay trên đầu COghe (Mrk chỉnh 01/10): hai tiếng "pốc" nhỏ sát nhau ngay trên đỉnh đầu, đúng lúc camera vừa tới cận
  cảnh COghe (0,55 s sau khi thắng), giấy bung lên một chút rồi rơi xuống quanh và trước COghe. Vị trí tính theo khung
  hình cận cảnh chiến thắng (`VenomCelebration.SettledViewport`), nên đúng trên mọi tỉ lệ màn hình.
- Khoảng 50 mảnh giấy màu theo bảng màu game (san hô, hổ phách, teal, xanh nhạt, ngà), lật và đung đưa khi rơi, mờ dần,
  hết sau ~4 s. Kèm tiếng "pốc" nhẹ và giấy sột soạt.
- Tinh tế: mảnh nhỏ (5–8 × 3–5 điểm), không phủ màn hình, không che chữ "Well done!".
- Code: `Assets/_Game/Venom/Runtime/Product/COgheConfetti.cs` (ảnh UI, chạy bằng code, không ParticleSystem). Gọi khi
  màn hình thắng hiện lên (`COgheProductUI.Update`).

## Mở màn Boss

Mỗi lần vào một màn Boss (không lặp lại khi Restart). Phần đi vòng và lùi ra nhanh hơn ~25% so với bản đầu (Mrk chỉnh
01/10): khoảng 7,5 s thay vì 10 s, cảnh báo giữ 2,7 s.

1. **Lặn vào hộp** (1,1 s): từ đúng góc nhìn của game, camera tiến vào trong hộp kính (chuyển từ orthographic sang phối
   cảnh bằng dolly-zoom nên không giật).
2. **Đi một vòng** (~1,35 s mỗi chặng): lướt sát tối đa 4 cơ quan nổi bật nhất (chọn tự động theo kích thước, đi theo thứ tự
   gần nhất), rồi tới lối ra. Đường đi là spline mượt; camera luôn ở trong vách kính.
3. **Lùi ra** (1,15 s): về đúng góc nhìn chuẩn của game.
4. **Cảnh báo** (2,7 s): dải sọc vàng–đen quét ngang màn hình, chữ đỏ "WARNING" và "VERY HARD LEVEL" chạy ngang rồi dừng
   giữa, viền đỏ nhịp đập, tiếng còi cảnh báo hai tông. Sau đó màn chơi bắt đầu.

- Màn chơi tạm dừng trong lúc này; chạm màn hình để bỏ qua phần đi vòng (vẫn xem cảnh báo).
- Code: `Assets/_Game/Venom/Runtime/Boss/COgheBossIntro.cs`; gọi từ `COgheProductUI` khi vào màn có `Definition.Boss`.
  Trong lúc chạy, `VenomCampaign` không điều khiển camera (`COgheBossIntro.OwnsCamera`).

## Chuyển cảnh intro → màn 1

- Lỗi (Mrk báo 01/10): hết intro, COghe hiện ở giữa hộp (chỗ đứng ở menu) rồi giật về chỗ xuất phát của màn 1.
- Nguyên nhân: `ResetLevel` đặt lại `Rigidbody.position`, nhưng transform (thứ lớp da đọc để vẽ) chỉ cập nhật ở bước vật lý
  kế tiếp. Intro tạm dừng màn chơi (`timeScale = 0`) nên suốt 20 s không có bước vật lý nào: màn chơi lộ ra sau intro vẫn
  vẽ COghe ở chỗ cũ, đến khi chạy lại mới nhảy về.
- Sửa: `ResetLevel` chép luôn vị trí đã đặt lại sang transform của các hạt và đồ vật, sau khi các cơ cấu đã đặt lại
  (ray trượt như lưỡi dao tự đặt lại vị trí riêng). Cũng sửa trường hợp vào màn Boss từ menu (phần đi vòng cũng chạy khi
  màn đang tạm dừng).
- Test: `FirstRunIntroDrawsCOgheAtTheLevelStart`, `ResetLevelKeepsRailCarriagesAtTheirResetPose`.

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
