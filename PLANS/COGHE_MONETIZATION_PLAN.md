---
title: "COghe — kế hoạch kiếm tiền chi tiết (đã chốt, chờ tích hợp SDK)"
tags: [coghe, monetization, ads, iap, economy, plan]
status: active
created: 2026-10-02
---

# COghe — kế hoạch kiếm tiền chi tiết

**Trạng thái (02/10/2026, tối):** Mrk chốt thêm: tặng bóng, tặng món đầu mỗi nhóm Style, cho mặc thử món chưa mua.
**Đã code phần trong game** trên nhánh `claude/coghe-monetization`: Giọt, cửa hàng Home / Style, thử đồ, Victory, nhịp
quảng cáo, Plus / No Ads, chỗ banner, sự kiện. Kèm quảng cáo thử và mua thử cho bản test. Hướng dẫn tích hợp trong repo:
`Docs/Monetization/COghe/README.md`. **Còn chờ SDK:** quảng cáo thật (mediation), thanh toán thật (Unity IAP + sản phẩm
trên store), tracking, Remote Config. Còn chưa làm: bộ đồ độc quyền của Plus, gợi ý bằng quảng cáo thưởng. Bản phân tích ban đầu: `PLANS/COGHE_MONETIZATION_ANALYSIS_2026_10_02.md`. Kế hoạch của Codex:
`PLANS/COGHE_MONETIZATION_CODEX_2026_10_02.md`; một số ý kỹ thuật của Codex đã đưa vào đây, có ghi rõ.

## 0. Quyết định của Mrk (02/10/2026)

| # | Quyết định |
| --- | --- |
| 1 | Banner **chỉ ở Main Menu, Home, Victory**. Không có trong gameplay, Style, intro. |
| 2 | **Một loại tiền:** giọt nước có chất liệu giống COghe (chất lỏng bóng). Tên hiển thị: **Drops / "Giọt"**. |
| 3 | **Style thành cửa hàng.** **Đồ Home tới level thì mở quyền mua; phải trả tiền mới hiện trong nhà.** Sau mỗi màn thắng có popup báo tính năng / món vừa mở. |
| 4 | **COghe Plus $5.99**, **No Ads $2.99** (mua một lần). |
| 5 | Đối tượng độ tuổi: **tính sau**. Trước khi phát hành phải chốt, vì nó quyết định cấu hình quảng cáo (Apple Kids Category, Google Families). |

## 1. Cần có trước khi bắt đầu (Mrk tích hợp)

- Mediation quảng cáo (tên SDK, phiên bản) và các ad unit: banner, interstitial, rewarded, cho iOS và Android, kèm ID thử.
- SDK tracking event (Firebase / AppsFlyer / Adjust…): API gửi event và nhận doanh thu theo lượt hiển thị.
- Sản phẩm IAP đã tạo trên hai store: `coghe_plus` ($5.99), `coghe_no_ads` ($2.99), non-consumable. Tài khoản sandbox / tester.
- Remote Config (nếu SDK tracking có) để chỉnh tần suất và giá mà không phải ra bản mới.

## 2. Kiến trúc trong code

Tách phần kiếm tiền khỏi physics, puzzle và JSON diện mạo (Codex nhấn mạnh điểm này, mình đồng ý):

| Thành phần | Việc | Ghi chú |
| --- | --- | --- |
| `COgheAds` (interface + 1 bản cho SDK của Mrk + 1 bản giả cho test/editor) | `ShowInterstitial(placement, done)`, `ShowRewarded(placement, onReward, onFail)`, `Banner(bool show)`, sự kiện `BannerHeightChanged` | Chưa tải được quảng cáo thì bỏ qua, không bắt chờ. Mọi chỗ gọi đi qua đây. |
| `COgheEntitlements` | Quyền Plus / No Ads từ store; khôi phục giao dịch | Không lấy một bool PlayerPrefs làm nguồn sự thật duy nhất; xác minh qua Unity IAP receipt. |
| `COgheWallet` | Số Giọt, lịch sử cộng/trừ (transaction id chống cộng trùng), quà ngày, đếm quảng cáo | Lưu cục bộ (PlayerPrefs `coghe.wallet.v1`), chạy offline. |
| `COgheInventory` | Món đã **sở hữu** (Home + Style) | Tách khỏi `COgheStyle` (JSON chỉ là diện mạo đang mặc). |
| `COgheEconomy` | Giá, thưởng, giới hạn (đọc Remote Config, có mặc định trong code) | Mục 9 liệt kê các key. |
| `COgheAnalytics` | Bọc SDK tracking | Mục 8. |

**Chỗ cắm vào code hiện tại:**

- **Mở khóa và sở hữu:**
  - `COgheHomeRoom.Unlocked(item)` hiện là "tới level". Thêm `Owned(item)`: món chỉ **hiện trong phòng** khi đã sở hữu.
  - Hiệu ứng "đồ bật ra" (`Reveal`, `Newly`) chuyển sang lúc **mua**.
- **Style:**
  - `COgheProductUI.Earned(level)` tách thành **Unlocked** (tới level) và **Owned**.
  - Thẻ catalog có 3 trạng thái: khóa (Level N), mở bán (giá + nút mua), đã có.
- **Victory:** điều kiện tự chuyển màn trong `COgheProductUI.Update` (`Page==Victory && Game.AutoAdvance && ReadyForNext`) thêm bước popup mở khóa, thưởng ×3, rồi quảng cáo xen kẽ.
- **Banner:**
  - `Layout()` đọc chiều cao **adaptive banner từ SDK**, không cố định 50/58 (Codex).
  - Áp làm "vùng nội dung" cho UI, khung camera (`FrameShowcase`, `FrameHome`) và vùng chạm, chỉ trên 3 màn có banner.

## 3. Tiền tệ "Giọt" (Drops)

- **Hình:**
  - Một giọt chất lỏng 3D, cùng vật liệu da COghe (đen bóng, phản sáng), render từ Unity thành icon UI (64 / 128 px).
  - Khi người chơi đã tiêm mực, có thể cho giọt mang màu mực chính của họ (để sau).
  - Bộ đếm ở góc trên Menu / Home / Style / Victory; giọt bay vào bộ đếm khi nhận thưởng; tiếng "bloop" chất lỏng.
- **Nguồn (mặc định, chỉnh được từ xa):**

| Nguồn | Giọt | Ghi chú |
| --- | ---: | --- |
| Thắng màn **lần đầu** | +10 | Không nhận lại khi chơi lại |
| Thắng không Retry | +5 | (Codex đề xuất bỏ để người chơi yếu không thiệt: cho vào A/B) |
| Victory "×3" bằng quảng cáo thưởng | +20 thêm | Tự bấm |
| Quà mỗi ngày ở Home | +15 (×2 bằng quảng cáo) | |
| Shop "Xem quảng cáo +Giọt" | +15 | Tối đa 3 lần / ngày |
| COghe Plus | ×2 mọi nguồn trừ quảng cáo; thưởng quảng cáo nhận ngay | |

- **Chi (giá khởi điểm):**

| Nhóm | Số món | Giá | Tổng |
| --- | ---: | --- | ---: |
| Đồ Home | 14 (Ball tặng*) | 13 món × 40–120 (tăng theo level) | ~950 |
| Mực | 12 (Ocean tặng) | 11 × 40–120 | ~800 |
| Mũ | 8 (Beanie tặng) | 7 × 50–110 | ~550 |
| Vật trôi | 6 (Star bits tặng) | 5 × 60–120 | ~450 |
| **Tổng** | 40 | | **~2.750** |

  \* **Đề xuất cần Mrk xác nhận:** tặng **Ball** khi mở Home (màn 10), để lần đầu vào nhà không trống và trò chơi bóng
  hoạt động ngay. Mỗi nhóm Style tặng món đầu tiên để người chơi thử được Style ngay.

- **Cân bằng mục tiêu tại màn 50** (sẽ chỉnh theo số liệu thật):

| Người chơi | Giọt kiếm được | Mua được |
| --- | ---: | --- |
| Không xem quảng cáo | ~500 (màn) + ~200 (quà ngày) ≈ 700 | ~25% |
| Xem quảng cáo thưởng đều (×3 một nửa số màn, quà ×2, shop 1–2 lần/ngày) | ~1.600–1.900 | ~60–70% |
| COghe Plus | ×2 + nhận thưởng ngay ≈ 2.800+ | ~100% |

  Nếu tỉ lệ 25% làm người không xem quảng cáo bỏ game (đo D7) thì tăng thưởng màn hoặc hạ giá. Mọi con số nằm trong
  Remote Config.

- **Kim tiêm / mực:** mua **một lần, tiêm thoải mái** (Mrk và cả hai bản phân tích cùng hướng). Undo / Rinse không tốn
  và không hoàn Giọt.

## 4. Sở hữu đồ và người chơi cũ

- **Trạng thái mỗi món:** `Khóa (chưa tới level)` → `Mở bán (tới level)` → `Đã có (đã mua / được tặng / Plus)`.
- **Home:** món chưa mua **không hiện** trong phòng. Trong menu Items: thẻ "Mở bán · 60 Giọt". Bấm mua thì món "bật ra"
  trong nhà như hiện nay. Món khóa vẫn xem trước được bằng hình mờ.
- **Style:** catalog hiện giá; bấm món mở bán thì hiện hộp mua (Giọt / xem quảng cáo nếu món rẻ / Hủy). Món chưa sở hữu
  vẫn **xem thử** được trên COghe (thử mực một lần rồi Undo, đội thử mũ), nhưng không lưu nếu chưa mua. Đây là cách bán
  hiệu quả; cần Mrk xác nhận.
- **Người chơi đã có bản trước (migration):** món đã mở theo level trước bản cập nhật được tặng luôn, không ai mất đồ.
  Diện mạo đang mặc được giữ.

## 5. Sau mỗi màn thắng (Victory)

Thứ tự (màn hình Victory **có banner**):

1. Hiệu ứng thắng như hiện nay. Bộ đếm Giọt bay +10 (+5).
2. Nút **"×3 Giọt (xem quảng cáo)"** hiện khoảng 3 s. Trong lúc đó tạm dừng tự chuyển màn; bấm thì xem, không bấm thì tiếp.
3. **Popup mở khóa** (nếu màn này mở món hoặc tính năng):
   - "Mới: Cầu trượt / Mực Gold / Home…" có hình món.
   - Nút **Mua (N Giọt)**, **Để sau**; với món rẻ thêm **Xem quảng cáo để nhận**.
   - Mở Home (màn 10) và Style là popup **tính năng** có nút "Vào xem".
   - Popup **dừng tự chuyển màn** tới khi đóng.
4. **Quảng cáo xen kẽ** (nếu đủ điều kiện ở mục 6). Chưa tải được thì bỏ qua.
5. Sang màn sau.

## 6. Quảng cáo

**Banner (Main Menu, Home, Victory):**

- Adaptive, neo đáy, đọc chiều cao từ SDK. Vùng nội dung của 3 màn này chừa chỗ banner + khoảng đệm.
- Ẩn khi mở popup che toàn màn, khi chiếu intro, khi đang chơi, ở Style. Ẩn khi có No Ads / Plus.
- Cần dời trên 3 màn:
  - Menu: Play, Home, Intro;
  - Home: 4 nút + Zoom; camera nhà (`FrameHome` lấy giữa vùng nội dung);
  - Victory: các nhãn, nút Menu, popup mở khóa.

**Quảng cáo xen kẽ (mặc định, chỉnh từ xa):**

- Chỉ ở bước 4 của Victory. Từ khi đã thắng ≥ 6 màn.
- Cách nhau ≥ 2 màn thắng **và** ≥ 90 s.
- Tối đa 4 lần / phiên, 10 lần / ngày (giới hạn theo phiên / ngày là ý của Codex).
- Không chạy: khi mở app, sau thua / Retry, ngay sau quảng cáo thưởng (90 s), khi có No Ads / Plus.
- Biến thể A/B cho bản thử: bản chặt hơn của Codex (từ màn 11, ≥ 3 màn và ≥ 180 s).

**Quảng cáo thưởng (luôn do người chơi bấm, nói rõ phần thưởng, cấp đúng một lần):**

| Vị trí | Thưởng |
| --- | --- |
| Victory ×3 | +20 Giọt |
| Popup mở khóa (món rẻ) | Nhận món |
| Quà ngày ×2 | +15 |
| Shop "+Giọt" | +15, tối đa 3 / ngày |
| (Sau) Gợi ý khi kẹt | Mũi tên chỉ bước tiếp |

Plus: các thưởng trên **nhận ngay** không cần xem. No Ads: vẫn xem được quảng cáo thưởng nếu muốn.

## 7. Mua trong app

| | No Ads $2.99 | COghe Plus $5.99 |
| --- | --- | --- |
| Bỏ banner + xen kẽ | ✓ | ✓ |
| Thưởng không cần xem quảng cáo | | ✓ |
| Giọt ×2 | | ✓ |
| Bộ độc quyền (3 mực, 2 mũ, 1 chủ đề phòng) | | ✓ |
| Nút gọi màn quái vật ở Home | | ✓ |

- Có nút **Khôi phục giao dịch** (bắt buộc với Apple). Người đã mua No Ads vẫn mua Plus giá $5.99, vì store không có giá
  "nâng cấp" cho non-consumable. Ghi rõ trong mô tả.
- **Nơi mời mua:** Main Menu (nút nhỏ), Shop Style / Home, popup mở khóa (dòng "Plus: ×2 Giọt"). **Không** chặn lúc mở
  game; lần mời đầu sau khi người chơi đã vào Home / Style (Codex).
- Bộ đồ độc quyền cần thiết kế (icon theo phong cách Codex) trước khi bán Plus.

## 8. Sự kiện tracking

- **Màn chơi:** `level_start`, `level_complete` (level, thời gian, số lần thử), `level_fail`, `level_retry`.
- **Tính năng:** `feature_unlock` (home, style), `item_unlock`, `item_buy` (id, giá, nguồn tiền), `item_preview`, `style_inject`, `style_rinse`.
- **Tiền:** `drops_earn` (nguồn), `drops_spend`; `daily_gift`.
- **Quảng cáo:** `ad_show` / `ad_complete` / `ad_fail` theo loại và placement. Doanh thu theo lượt hiển thị (từ mediation).
- **IAP:** `iap_view`, `iap_purchase`, `iap_restore`.
- **Chỉ số quyết định:** doanh thu thuần theo nhóm cài sau 30 ngày, D1 / D7 / D30 (Codex). Tỉ lệ bấm banner bất thường.
  Người không xem quảng cáo có bỏ game ở Home / Style không.

## 9. Remote Config (mặc định trong code)

```
drops_first_win=10  drops_clean_win=5  drops_triple_ad=20  drops_daily=15  drops_ad_shop=15  ad_shop_daily_cap=3
price_<ITEM_ID>=…   gift_items=BALL,INK_OCEAN,HAT_BEANIE,FLOAT_STARS
interstitial_from_win=6  interstitial_min_wins=2  interstitial_min_seconds=90  interstitial_session_cap=4  interstitial_daily_cap=10
banner_menu=1  banner_home=1  banner_victory=1  plus_offer_after=home_visit
```

## 10. Kiểm tra

- **PlayMode:**
  - cộng / trừ Giọt đúng một lần (chơi lại không cộng, tắt app giữa chừng không mất);
  - mua món Home thì món hiện và "bật ra";
  - chưa mua không hiện;
  - Style xem thử không lưu khi chưa mua;
  - migration người cũ;
  - popup Victory dừng tự chuyển;
  - quảng cáo xen kẽ đúng điều kiện (dùng bản quảng cáo giả);
  - No Ads / Plus tắt quảng cáo;
  - banner chỉ ở 3 màn và không che nút.
- **Ca lỗi (Codex):** đóng quảng cáo sớm, không có quảng cáo, mất mạng, xuống nền, callback trùng; mua treo / hủy /
  hoàn tiền / khôi phục sau cài lại.
- **Thiết bị thật:** OPPO (Android) và iPhone (ATT, sandbox store). Đo FPS, bộ nhớ, dung lượng sau khi thêm SDK.

## 11. Thứ tự làm (sau khi có SDK)

1. `COgheAds` + `COgheAnalytics` nối vào SDK của Mrk; bản giả cho editor / test.
2. Wallet + Inventory + migration; giá / thưởng trong `COgheEconomy`.
3. Home "mua mới hiện"; Style thành cửa hàng (thẻ giá, hộp mua, xem thử).
4. Victory: +Giọt, ×3, popup mở khóa, quảng cáo xen kẽ.
5. Banner 3 màn + vùng nội dung.
6. IAP No Ads / Plus + Khôi phục; bộ đồ Plus.
7. Test đầy đủ + build Android / iOS + video gửi Mrk.

## 12. Còn mở

- Độ tuổi đối tượng (Mrk: tính sau; phải chốt trước khi phát hành).
- Tặng Ball khi mở Home và tặng món đầu mỗi nhóm Style (đề xuất).
- Cho xem thử món chưa mua ở Style (đề xuất).
- Thiết kế bộ đồ độc quyền của Plus.

## Nguồn

- Google AdMob — [banner](https://support.google.com/admob/answer/6275345?hl=en), [interstitial](https://support.google.com/admob/answer/6201362?hl=en), [rewarded](https://support.google.com/admob/answer/7313578?hl=en), [adaptive banner Unity](https://developers.google.com/admob/unity/banner/anchored-adaptive)
- [Apple App Store Review Guidelines](https://developer.apple.com/app-store/review/guidelines/) (3.1.1 IAP / khôi phục, 1.3 Kids, 2.5.18 quảng cáo)
- [Google Play Families](https://support.google.com/googleplay/android-developer/answer/9893335?hl=en)
- eCPM Mỹ 2026: [MonetizeMore (theo MAF)](https://www.monetizemore.com/blog/how-much-ad-revenue-can-apps-generate/), [Choicely](https://www.choicely.com/blog/in-app-ad-revenue-how-to-forecast-and-maximize-your-apps-return)
- Tỉ lệ IAP: [Playio](https://blog.playio.co/hybrid-casual-games-monetization), [Game Growth Advisor](https://gamegrowthadvisor.com/blog/2026-06-02-hybrid-monetization-mobile-games-iap-ads-guide-2026/)
