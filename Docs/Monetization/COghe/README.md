# COghe — kiếm tiền: Giọt, cửa hàng, quảng cáo, Plus

02/10/2026 · Theo quyết định của Mrk. Kế hoạch đầy đủ trong repo: `PLANS/COGHE_MONETIZATION_PLAN.md`.
Cập nhật 04/10: Android đã nối Google Mobile Ads + UMP, Firebase Analytics và Remote Config. [Cấu hình, tracking, kiểm chứng](GOOGLE_SERVICES.md). IAP thật chờ Mrk tạo sản phẩm trên Play Console; bản store chưa mở bán. Runtime bám theo `PLANS/COGHE_MONETIZATION_PLAN.md` và mục cập nhật 04/10 trong đó.

Ảnh (proof native trên Mac): [`shop-screens.jpg`](shop-screens.jpg).

## Trong game

- **Giọt (Drops):**
  - Giọt nước bóng cùng chất liệu với COghe (`Resources/COgheUI/Drop.png`, render từ Unity bằng `RenderDropIcon`).
  - Bộ đếm có ở Menu, Home, Style, Victory. Bấm vào bộ đếm thì hiện cách kiếm thêm.
  - Nguồn:
    - +10 cho mỗi màn thắng lần đầu (chơi lại không nhận lại);
    - bonus +20 nếu xem quảng cáo ở Victory;
    - quà mỗi ngày ở Home (+15, ×2 bằng quảng cáo);
    - xem quảng cáo +15 (tối đa 3 lần / ngày).
- **Tặng:** quả bóng (mở Home, màn 12: Boss đầu), Ocean, Beanie, Star bits (món đầu mỗi nhóm Style).
- **Đồ Home:**
  - Tới màn thì mở bán; mua xong mới xuất hiện trong nhà và "bật ra", COghe chạy tới chơi.
  - Trong menu Items: giá kèm giọt; chọn món chưa mua thì hiện hình mờ của nó trong phòng và hộp mua.
- **Style (cửa hàng):**
  - Thẻ hiện giá. Món đã tới màn **mặc thử được** trên COghe (tiêm mực, đội mũ, thả vật trôi) nhưng **không lưu** khi chưa mua.
  - Có nút Mua ngay cạnh tên món đang thử.
  - Rời Style khi còn món thử: hỏi **Mua (tổng) / Bỏ ra / Thử tiếp**.
  - Rời bằng cách khác (Pause → Menu, tắt app): tự trả về diện mạo đã sở hữu.
- **Sau mỗi màn thắng:**
  - +Giọt; nút +20 khoảng 3,5 s;
  - popup **"New for COghe!"** với các món vừa mở (quà ghi "Gift ✓", món bán có nút giá, màn 12 có nút "Visit Home");
  - quảng cáo xen kẽ nếu đến lượt;
  - sang màn.
  - Màn thắng giữ nguyên tới khi bấm Next Level; sau đó popup mở khóa chờ Continue.
- **Giá:**
  - Home và mực 40–120, mũ 50–110, vật trôi 60–120, tăng theo level.
  - Món ≤ 60 có thể lấy bằng một quảng cáo thưởng.
  - Mọi số nằm trong `COgheEconomy`, đổi qua Firebase Remote Config, áp dụng ở lần mở app tiếp theo (tên key ở kế hoạch §9).
- **Người chơi bản cũ:** lần đầu chạy bản mới, mọi món đã tới màn được tặng (`COgheShop.Migrate`).
- **Banner:**
  - Chỉ ở Main Menu, Home, Victory.
  - Ba màn này tự chừa chỗ theo chiều cao banner SDK báo về (`COgheAds.BannerHeight`), nút và camera lùi lên.
  - Không có banner trong lúc chơi, Style, intro.
- **Quảng cáo xen kẽ:**
  - Chỉ khi user bấm Next Level tại Victory; không tự hiện trong lúc chờ hay khi đóng quảng cáo thưởng.
  - Từ lần thắng thứ 6; cách ≥ 2 màn và ≥ 90 s.
  - Tối đa 4 lần / phiên, 10 lần / ngày; không ngay sau quảng cáo thưởng.
  - Chưa tải được thì bỏ qua.
- **COghe Plus ($5.99) / No Ads ($2.99):**
  - Nút Plus ở góc Menu; popup quyền lợi, có "Restore purchases".
  - Plus: không quảng cáo, thưởng nhận ngay (không cần xem), Giọt ×2, nút gọi màn quái vật ở Home.
  - No Ads: bỏ banner/xen kẽ; vẫn xem rewarded nếu muốn.
  - Bộ đồ độc quyền của Plus chưa thiết kế.

## Điểm tích hợp SDK

| Phần | Cắm vào | Ghi chú |
| --- | --- | --- |
| Quảng cáo (mediation) | Viết một lớp `ICOgheAdProvider`, gán `COgheAds.Provider = …` khi khởi động | `ShowInterstitial`, `ShowRewarded(done(bool))`, `SetBanner(bool)`, `BannerHeight` (pixel màn hình, banner adaptive); gọi `COgheAds.NotifyBannerChanged()` khi banner đổi cỡ |
| Thanh toán (Unity IAP) | Lớp `ICOgheStore`, gán `COgheEntitlements.Store = …` | Sản phẩm non-consumable `coghe_plus`, `coghe_no_ads`. Khi mua hoặc khôi phục thành công thì gọi `COgheEntitlements.Grant(id)`. Bản store chưa có IAP thì nút Plus ẩn. |
| Tracking | `COgheAnalytics.Sink = (name, values) => …` | Sự kiện đã có: `level_start`, `level_complete`, `level_fail`, `level_retry`, `feature_unlock`, `item_unlock`, `item_preview`, `item_buy`, `item_grant`, `drops_earn`, `drops_spend`, `daily_gift`, `style_inject`, `style_rinse`, `ad_show`, `ad_complete`, `ad_closed`, `ad_fail`, `ad_rewarded_instant`, `iap_view`, `iap_purchase`, `iap_restore`, `iap_entitlement` |
| Remote Config | `COgheRemoteConfig.Fetch` + `COgheEconomy.StageRemote/LoadCached` | Giá, thưởng, nhịp quảng cáo đã kiểm tra giới hạn; cache offline, áp dụng phiên tiếp theo |

## Bản test / dev

- **Android có SDK:** dùng quảng cáo test của Google qua mạng; không dùng ad unit doanh thu để QA. UMP vẫn kiểm tra trạng thái consent.
- **Editor/Mac chưa nối SDK:** thẻ xám "Test ad", chạy 2 s rồi đóng; banner xám "Test banner 320×50". Mặc định bật trên bản dev/test.
- **Mua thử:** thành công ngay, không mất tiền.
- **Pause → Levels (test):**
  - "Ads on/off": tắt bật quảng cáo thử;
  - "+500": cộng Giọt;
  - "Reset": xóa ví, đồ đã mua và Plus.
- Bản store (`COGHE_STORE=1`) không có công cụ thử; Android dùng ba ad unit thật, chịu điều kiện consent và xét duyệt của AdMob. Chưa nối IAP thật nên không bán Plus trong store build.

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
- thắng màn trả Giọt một lần, hiện đúng món mở (màn 12: Ball, Ocean, Mint);
- nhịp quảng cáo xen kẽ và giới hạn mỗi phiên;
- Plus bỏ quảng cáo, nhận thưởng ngay, ×2;
- banner không đè nút ở Menu / Home và không có trong Style / lúc chơi;
- người chơi cũ giữ đồ; quà mỗi ngày một lần.

Ngoài ra màn 1 thắng thật trả 10 Giọt (`GuidingThroughExit…`). Proof native: ảnh 38–46.

43/43 PlayMode tests đạt; 9/9 test liên quan và 2/2 test banner/Plus chạy lại sau chỉnh sửa cuối. Firebase đã publish bản cấu hình 1 gồm 57 tham số, đối chiếu với template. Xem bằng chứng và giới hạn kiểm chứng trong [GOOGLE_SERVICES.md](GOOGLE_SERVICES.md).

## Quyền lợi và độ bền phần thưởng (04/10)

- No Ads giữ rewarded; Plus nhận tức thì. Thắng Plus 20 + 20 = 40; quà ngày Plus 30 + 15 = 45. Nút hiển thị đúng số Giọt, không hứa ×3 sai.
- `COgheReward` chụp giá trị offer trước khi xem; `COgheShop.ApplyReward` lưu số dư, ownership, cap và khóa giao dịch cùng lúc tại callback earned. Callback đóng quảng cáo chỉ cập nhật UI.
- Ad lỗi không ghi `ad_complete` hoặc tiêu cap interstitial; chỉ callback mở thực sự mới tính lượt. Popup che màn hình ẩn banner.
- Plus chỉ được giới thiệu sau khi vào Home/Style. Store chưa tích hợp: không có nút mua thật hoặc Restore giả trong bản store.
- Xuất template Firebase: Unity → Gravity Box → COghe → Export monetization Remote Config. File `remote-config.defaults.json` sinh từ cùng các giá trị mặc định runtime.
