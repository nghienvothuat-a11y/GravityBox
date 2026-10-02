# COghe — kiếm tiền: Giọt, cửa hàng, quảng cáo, Plus

02/10/2026 · Theo quyết định của Mrk. Kế hoạch đầy đủ ở workspace Buzz: `PLANS/COGHE_MONETIZATION_PLAN.md`.
Đã làm phần trong game. **Quảng cáo thật và thanh toán thật chờ SDK** (Mrk tích hợp), cắm vào các chỗ ở mục "Tích hợp SDK".

Ảnh (proof native trên Mac): [`shop-screens.jpg`](shop-screens.jpg).

## Trong game

- **Giọt (Drops):**
  - Giọt nước bóng cùng chất liệu với COghe (`Resources/COgheUI/Drop.png`, render từ Unity bằng `RenderDropIcon`).
  - Bộ đếm có ở Menu, Home, Style, Victory. Bấm vào bộ đếm thì hiện cách kiếm thêm.
  - Nguồn:
    - +10 cho mỗi màn thắng lần đầu (chơi lại không nhận lại);
    - ×3 (+20) nếu xem quảng cáo ở Victory;
    - quà mỗi ngày ở Home (+15, ×2 bằng quảng cáo);
    - xem quảng cáo +15 (tối đa 3 lần / ngày).
- **Tặng:** quả bóng (mở Home, màn 10), Ocean, Beanie, Star bits (món đầu mỗi nhóm Style).
- **Đồ Home:**
  - Tới màn thì mở bán; mua xong mới xuất hiện trong nhà và "bật ra", COghe chạy tới chơi.
  - Trong menu Items: giá kèm giọt; chọn món chưa mua thì hiện hình mờ của nó trong phòng và hộp mua.
- **Style (cửa hàng):**
  - Thẻ hiện giá. Món đã tới màn **mặc thử được** trên COghe (tiêm mực, đội mũ, thả vật trôi) nhưng **không lưu** khi chưa mua.
  - Có nút Mua ngay cạnh tên món đang thử.
  - Rời Style khi còn món thử: hỏi **Mua (tổng) / Bỏ ra / Thử tiếp**.
  - Rời bằng cách khác (Pause → Menu, tắt app): tự trả về diện mạo đã sở hữu.
- **Sau mỗi màn thắng:**
  - +Giọt; nút ×3 khoảng 3,5 s;
  - popup **"New for COghe!"** với các món vừa mở (quà ghi "Gift ✓", món bán có nút giá, màn 10 có nút "Visit Home");
  - quảng cáo xen kẽ nếu đến lượt;
  - sang màn.
  - Popup dừng tự chuyển màn tới khi bấm Continue.
- **Giá:**
  - Home và mực 40–120, mũ 50–110, vật trôi 60–120, tăng theo level.
  - Món ≤ 60 có thể lấy bằng một quảng cáo thưởng.
  - Mọi số nằm trong `COgheEconomy`, đổi từ Remote Config bằng `COgheEconomy.Set(key, value)` (tên key ở kế hoạch §9).
- **Người chơi bản cũ:** lần đầu chạy bản mới, mọi món đã tới màn được tặng (`COgheShop.Migrate`).
- **Banner:**
  - Chỉ ở Main Menu, Home, Victory.
  - Ba màn này tự chừa chỗ theo chiều cao banner SDK báo về (`COgheAds.BannerHeight`), nút và camera lùi lên.
  - Không có banner trong lúc chơi, Style, intro.
- **Quảng cáo xen kẽ:**
  - Chỉ sau Victory.
  - Từ lần thắng thứ 6; cách ≥ 2 màn và ≥ 90 s.
  - Tối đa 4 lần / phiên, 10 lần / ngày; không ngay sau quảng cáo thưởng.
  - Chưa tải được thì bỏ qua.
- **COghe Plus ($5.99) / No Ads ($2.99):**
  - Nút Plus ở góc Menu; popup quyền lợi, có "Restore purchases".
  - Plus: không quảng cáo, thưởng nhận ngay (không cần xem), Giọt ×2, nút gọi màn quái vật ở Home.
  - No Ads: chỉ bỏ quảng cáo.
  - Bộ đồ độc quyền của Plus chưa thiết kế.

## Tích hợp SDK (việc của Mrk, rồi Claude nối)

| Phần | Cắm vào | Ghi chú |
| --- | --- | --- |
| Quảng cáo (mediation) | Viết một lớp `ICOgheAdProvider`, gán `COgheAds.Provider = …` khi khởi động | `ShowInterstitial`, `ShowRewarded(done(bool))`, `SetBanner(bool)`, `BannerHeight` (pixel màn hình, banner adaptive); gọi `COgheAds.NotifyBannerChanged()` khi banner đổi cỡ |
| Thanh toán (Unity IAP) | Lớp `ICOgheStore`, gán `COgheEntitlements.Store = …` | Sản phẩm non-consumable `coghe_plus`, `coghe_no_ads`. Khi mua hoặc khôi phục thành công thì gọi `COgheEntitlements.Grant(id)`. Bản store chưa có IAP thì nút Plus ẩn. |
| Tracking | `COgheAnalytics.Sink = (name, values) => …` | Sự kiện đã có: `level_start`, `level_complete`, `level_fail`, `level_retry`, `feature_unlock`, `item_unlock`, `item_preview`, `item_buy`, `item_grant`, `drops_earn`, `drops_spend`, `daily_gift`, `style_inject`, `style_rinse`, `ad_show`, `ad_complete`, `ad_closed`, `ad_fail`, `ad_rewarded_instant`, `iap_view`, `iap_purchase`, `iap_restore`, `iap_entitlement` |
| Remote Config | `COgheEconomy.Set(key, value)` lúc khởi động | Giá `price_<ID>`, thưởng, nhịp quảng cáo |

## Bản test / dev

- **Quảng cáo thử:** thẻ xám "Test ad", chạy 2 s rồi đóng; banner xám "Test banner 320×50". Mặc định bật trên bản dev/test.
- **Mua thử:** thành công ngay, không mất tiền.
- **Pause → Levels (test):**
  - "Ads on/off": tắt bật quảng cáo thử;
  - "+500": cộng Giọt;
  - "Reset": xóa ví, đồ đã mua và Plus.
- Bản store (`COGHE_STORE=1`) không có những thứ này. Khi chưa có SDK thì bản store không có quảng cáo, không có nút Plus.

## File

| File | Nội dung |
| --- | --- |
| `Runtime/Shop/COgheEconomy.cs` | Danh mục 40 món, giá, thưởng, nhịp quảng cáo |
| `Runtime/Shop/COgheShop.cs` | Ví Giọt, đồ sở hữu, thưởng chỉ trả một lần theo khóa, quà ngày, migration (PlayerPrefs `coghe.shop.v1`) |
| `Runtime/Shop/COgheEntitlements.cs` | Plus / No Ads, cửa hàng thử |
| `Runtime/Shop/COgheAds.cs` | Lớp quảng cáo chung, nhịp quảng cáo xen kẽ, quảng cáo thử |
| `Runtime/Shop/COgheAnalytics.cs` | Sự kiện |
| `Runtime/Product/COgheProductUI.Shop.cs` | Bộ đếm, hộp mua, popup mở khóa, quà ngày, Plus, banner, luồng Victory |
| `Home/COgheHomeRoom.cs` | `Present` (đã có), `Available` (tới màn), `ForSale` |
| `Product/COgheProductUI.Style.cs` | Thử đồ, chỉ lưu diện mạo đã sở hữu, Mua, hỏi khi rời |

## Kiểm tra

`COgheShopTests`:
- đồ Home chỉ hiện khi đã mua (mua qua menu Items, món bật ra);
- Style mặc thử không giữ nếu bỏ, mua thì giữ;
- thắng màn trả Giọt một lần, hiện đúng món mở (màn 10: Ball, Ocean, Mint);
- nhịp quảng cáo xen kẽ và giới hạn mỗi phiên;
- Plus bỏ quảng cáo, nhận thưởng ngay, ×2;
- banner không đè nút ở Menu / Home và không có trong Style / lúc chơi;
- người chơi cũ giữ đồ; quà mỗi ngày một lần.

Ngoài ra màn 1 thắng thật trả 10 Giọt (`GuidingThroughExit…`). Proof native: ảnh 38–46.
