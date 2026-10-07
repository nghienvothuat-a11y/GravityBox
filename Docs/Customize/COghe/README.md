# COghe — Style: tiêm màu và phụ kiện

01–02/10/2026 · Theo yêu cầu của Mrk: trong Home có phần tuỳ biến nhân vật — tiêm màu vào thân (giữ để màu lan),
đội mũ và thả đồ vật nhỏ trôi bên trong thân, mang theo vào mọi màn. Kế hoạch và quyết định: `PLANS/COGHE_CUSTOMIZE_PLAN.md`
(workspace Buzz). Thiết kế màn hình và 26 icon do Codex làm (`OUTBOX/COGHE_CUSTOMIZE_UI_2026_10_01`); shader, tiêm màu,
phụ kiện, lưu và giao diện Unity do Claude làm. Mrk đồng ý đổi quy tắc mỹ thuật (thân mặc định vẫn tối; xem
`Docs/ArtDirection/COghe/STYLE_RULES.md` §1).

Ảnh chụp bản Mac (proof native): [`style-screens.jpg`](style-screens.jpg) — màn Style, đang tiêm Ocean, mix 2 màu, mũ,
đồ bên trong, món còn khoá, hỏi thay màu thứ năm, trở về Home.

## Cách chơi

- Home → **Style** (nút thứ tư; Zoom chuyển thành nút tròn nổi bên phải). COghe đứng yên ở chỗ trống giữa phòng,
  đồ đạc ẩn đi cho sân khấu gọn, camera lại gần. Rời Style (Done, nút Back, phím back Android) mọi thứ trở lại như cũ.
- **Colors:** chọn ống tiêm (không đổi gì trên thân), **giữ ngón tay trên da COghe** để tiêm; giữ lâu thì màu lan rộng hơn.
  Nhả tay là ngừng, chất lỏng lắng lại rồi tự lưu ("Saving…" → "Saved"). Tối đa 4 màu; màu thứ năm hỏi "Replace which
  color?" — chọn xong chỉ đánh dấu, lần giữ tiếp theo mới thay (không bao giờ tự bỏ màu). Trên điện thoại điểm tiêm nằm
  ngay trên đầu ngón tay để không bị che.
- **Rinse:** hỏi trước, đưa COghe về màu đen gốc; mũ và đồ bên trong giữ nguyên. **Undo:** lùi đúng một thao tác
  (một lần giữ, một lần rinse, một lần đổi đồ), tối đa 20 bước trong một lần vào Style; trả lại đúng vân màu, không phải màu trung bình.
- **Accessories:** Hats — một mũ, ô **None** để bỏ mũ. Inside — tối đa 2 món; chạm món đang đeo để bỏ; món thứ ba hỏi
  thay món nào. Thân tối thì đồ bên trong không thấy: báo "Needs clear ink" (Keep this look / Colors), không tự đổi màu.
- Món chưa mở: xem trước, "Complete level N to unlock …", không đeo được.

## Danh mục (mở theo màn đã hoàn thành; mốc ×1,2 cho 60 màn, Mrk 06/10/2026)

| Màu | Màn | | Mũ | Màn | | Bên trong | Màn |
| --- | --- | --- | --- | --- | --- | --- | --- |
| Ocean (trong) | 12 | | Beanie | 13 | | Star bits | 30 |
| Mint | 12 | | Party hat | 18 | | Little fish | 32 |
| Coral | 16 | | Flower crown | 20 | | Bubbles | 37 |
| Firefly (phát sáng, hơi trong) | 19 | | Cap | 25 | | Baby jellyfish | 47 |
| Gold (kim loại) | 23 | | Straw hat | 29 | | Tiny pearls | 48 |
| Sakura (ánh nhũ) | 26 | | Propeller cap | 35 | | Tiny planet | 56 |
| Stardust (sao lấp lánh) | 30 | | Crown | 42 | | | |
| Aurora (óng ánh, hơi trong) | 34 | | Space helmet | 54 | | | |
| Lava (vân phát sáng) | 38 | | | | | | |
| Pearl (xà cừ) | 43 | | | | | | |
| Galaxy (tinh vân, trong) | 50 | | | | | | |
| Prism (cầu vồng, hơi trong) | 60 | | | | | | |

## Kỹ thuật

- **Màu do hạt mang:** mỗi hạt trong 32 hạt giữ phần của tối đa 4 mực (`COgheInking.Amount`); da cộng mực của hạt lân cận
  cùng công thức với hình dạng da (`VenomSurface`, UV2) nên màu đi theo chất lỏng, xoáy khi di chuyển, chia khi tách.
  Shader `COgheInk/InkSkin` (URP PBR: kim loại, độ bóng, phát sáng, độ trong, tinh vân, óng ánh, vân, sao). Không có lực:
  chỉ là phần nhìn.
- **Phụ kiện** (`COgheAccessories`): đọc vị trí da sau mỗi lần dựng lại; mũ trên đỉnh khối lớn nhất (rơi khỏi mảnh nhỏ,
  ống, lối ra), đồ bên trong vẽ dưới da (màu trong thì thấy). Không collider, không rigidbody.
- **Mũ không giật** (Mrk 07/10: "cái mũ khi đội nó cũng bị giật giật sang bên này rồi sang bên kia giống con mắt hồi
  đầu"): trước đây mũ đặt vào đỉnh da cao nhất, mà da dựng lại mỗi khung và lúc đứng yên COghe đẩy một bướu thăm dò lúc
  bên này lúc bên kia, nên mũ nhảy theo (rung 1,4 mm mỗi khung, cứ 20 khung có cú trên 10 mm). Giờ vị trí ngang theo đỉnh
  mềm của các hạt; độ cao theo đỉnh mềm của da (`VenomSurface.SkinCrown`: da quanh đỉnh, tính theo diện tích và độ gần
  đỉnh, nên không phụ thuộc cách da chia tam giác mỗi khung); rồi lọc one-euro theo thân (`COgheSteady`, chung với mắt)
  và theo đồng hồ mô phỏng. Còn 0,3 mm, cú lớn nhất ~1,6 mm, ít hơn chính đỉnh thân (`TheHatSitsStillOnARestingCOghe`).
  Đội mũ thì mắt hạ xuống dưới vành. Clip trước/sau: `OUTBOX/COGHE_HAT_STEADY_2026_10_07` (workspace Buzz).
- **Lưu:** `COgheStyle` (PlayerPrefs `coghe.style.v1`, JSON) — 4 mực, vân của từng hạt, seed, mũ, đồ bên trong. Áp vào
  COghe ở mọi màn khi scene mở (`COgheProductUI.Start`). Không ghi khi test (`VenomCampaignSave.PersistenceEnabled`).
- **Build:** material tạo lúc chạy chỉ dùng shader trong `Resources` (`InkSkin`, `AccessoryFx`); biến thể trong suốt khai báo
  bằng `multi_compile_local_fragment`. Icon: `Resources/COgheStyle/Icons` (256 px, ASTC 6×6 trên điện thoại, như icon đồ Home).
- **Giao diện:** `COgheProductUI.Style.cs`; vùng sân khấu `COgheStyleStage` giữ con trỏ từ lúc chạm đến lúc nhả, ngón thứ
  hai bị bỏ qua, mất focus/tạm dừng app là kết thúc giữ. Không gửi lệnh nào xuống phòng hay COghe.

## Kiểm chứng

- PlayMode (`COgheStyleTests`): tiêm đúng chỗ giữ và lưu đúng vân, bấm ngoài thân không tiêm, Undo; màu thứ năm hỏi/huỷ/thay
  và Undo trả đúng vân; Rinse hỏi trước, giữ mũ, Undo; mũ/None/khoá, đồ bên trong + hỏi thay + "Needs clear ink"; đồ và màu
  mang sang màn sau. Cùng các test cũ của Home, mực, phụ kiện.
- Proof native trên Mac (`-coghe-product-proof`): ảnh 30–37.
- Chưa kiểm: cài trên OPPO, cảm giác chạm thật, khung hình trên điện thoại.
