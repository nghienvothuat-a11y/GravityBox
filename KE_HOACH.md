# COghe iOS: ra mắt và test Google Ads 30 ngày ở Mỹ

Yêu cầu của Mrk (08/10/2026, 07:23 UTC): đưa COghe lên iOS, chạy test khoảng 1 tháng bằng Google Ads ở thị trường Mỹ, ngân
sách tăng dần 20 → 30 → 50 → 100 $/ngày. Cần kế hoạch quảng cáo, campaign, clip quảng cáo, bộ icon và screenshot ASO, cách
theo dõi và đánh giá qua doanh thu và Firebase console.

Nguồn đã tra:
- `RESEARCH/COGHE_GOOGLE_ADS_IOS_2026_10_08.md`: Google Ads cho app iOS, giá thầu, ngân sách, quy cách nội dung quảng cáo, số liệu tham khảo.
- `RESEARCH/COGHE_APP_STORE_ASO_2026_10_08.md`: quy cách App Store, ASO, quyền riêng tư.
- Code trong repo: `Docs/Monetization/COghe/GOOGLE_SERVICES.md` (repo GravityBox, nhánh NewGraphic) và `PLANS/COGHE_MONETIZATION_PLAN.md` (có trong nhánh này).

## 0. Tóm tắt

1. **Bản iOS chưa sẵn sàng để chạy quảng cáo.** Firebase, AdMob và Remote Config mới chỉ chạy trên Android. Mua trong app
   (Plus, No Ads) chưa có thật trên nền tảng nào. Không có Firebase trên iOS thì Google Ads không biết ai đã cài, ai chơi
   tiếp. Mục 2 liệt kê việc của Mrk trên các trang quản trị và việc tao làm trong code.
2. **Một campaign, tối ưu theo lượt cài.** Loại "App installs", chỉ iOS, chỉ Mỹ, tiếng Anh. Giá thầu "Maximize
   conversions", không đặt CPI mục tiêu. Với 20–100 $/ngày, mỗi ngày chỉ có khoảng 4 tới 60 lượt cài, tuỳ CPI. Tối ưu theo hành động
   trong game cần ít nhất 10 người/ngày làm hành động đó, nên tháng này chưa dùng được. Các sự kiện trong game vẫn được đo
   để mình tự đánh giá.
3. **Ngân sách tăng từng bước nhỏ, đi qua các mốc 20, ~30, 50, 100.** Google khuyên mỗi lần đổi không quá 20%.
   Lịch: 20 → 24 → 29 → 35 → 42 → 50 → 60 → 72 → 86 → 100 $/ngày, tổng khoảng 1.370 $ cho 30 ngày. Mốc 30 là 29 $, để
   mỗi bước giữ quanh 20%. Ở các ngày 7, 14, 21 xét số liệu theo mục 8: đạt thì tuần sau chạy tiếp theo lịch, không đạt
   thì giữ nguyên mức đang chạy.
4. **Mục tiêu của tháng test là học, không phải lãi ngay.** Cuối tháng phải trả lời được bốn câu:
   - một lượt cài ở Mỹ trên iOS tốn bao nhiêu;
   - người chơi có quay lại không (ngày 1, ngày 7), và đi tới đâu trong game;
   - mỗi lượt cài mang về bao nhiêu tiền trong 7 ngày đầu;
   - clip nào kéo được lượt cài rẻ nhất.
5. **Trang App Store quan trọng gấp đôi.** Tài liệu của Google chỉ nói quảng cáo iOS dẫn về trang App Store mặc định.
   Tao không tìm thấy cách nào để dẫn tới trang riêng cho từng quảng cáo; nên hỏi đại diện Google Ads cho chắc. Google còn
   tự lấy screenshot, icon và video của trang đó để làm quảng cáo.

## 1. Đã giao kèm kế hoạch

| Thư mục trong nhánh này | Nội dung |
| --- | --- |
| `ADS/` | Clip quảng cáo quay từ game thật, mỗi clip 3 khổ: dọc 9:16, vuông 1:1, ngang 16:9 |
| `ADS_IMAGES/` | Ảnh quảng cáo cho Google: 1200×1200, 1200×628, 1200×1500 |
| `APP_STORE/SCREENSHOTS/` | 8 screenshot theo thứ tự upload, 3 bộ kích thước: iPhone 6.9" 1320×2868, iPhone 6.3" 1206×2622, iPad 13" 2064×2752 |
| `APP_STORE/PREVIEW/` | Video xem trước trên App Store (App Preview) |
| `APP_STORE/ICON/` | Icon đã duyệt và 3 bản đổi màu nền để test sau |
| `APP_STORE/METADATA.md` | Tên, phụ đề, từ khoá, mô tả, nội dung quảng cáo; đã đếm ký tự |
| `WEEKLY_TRACKER.csv` | Bảng theo dõi từng tuần, mở bằng Google Sheets (mục 7.4) |
| `SOURCE/` | Script và file cấu hình để dựng lại mọi thứ ở trên khi đổi chữ hay đổi cảnh |
| `KE_HOACH.md` | Bản này, không có phần đầu YAML |

## 2. Việc phải xong trước ngày chạy đầu tiên

### 2.1 Mrk làm trên các trang quản trị

| # | Việc | Ghi chú |
| --- | --- | --- |
| 1 | **App Store Connect:** tạo app COghe | Bundle ID `com.gravityboxlab.venom` (bản build iOS đang dùng ID này). Ngôn ngữ chính English (U.S.). Danh mục Games, nhóm Puzzle, nhóm phụ Casual. Giá: Free. |
| 2 | **Hợp đồng Paid Apps**, thuế, tài khoản ngân hàng | Bắt buộc để bán Plus và No Ads. |
| 3 | **Hai sản phẩm mua trong app** | `coghe_plus` 5,99 $ và `coghe_no_ads` 2,99 $, loại Non-Consumable. Mỗi sản phẩm cần ảnh chụp và ghi chú cho người duyệt. |
| 4 | **Privacy Policy URL** và **Support URL** | Còn thiếu từ 04/10. App Store bắt buộc có. |
| 5 | **App Privacy** (khai báo dữ liệu thu thập) và **phân loại độ tuổi** | Xem mục 6.6. **Không** chọn Kids Category: Kids Category gần như cấm quảng cáo và analytics của bên thứ ba. |
| 6 | **Firebase:** thêm app iOS vào project `coghe-57f21` | Gửi tao file `GoogleService-Info.plist`. Khi đã có App Store ID thì điền vào phần cài đặt app iOS trong Firebase, vì Google Ads cần ID này cho sự kiện iOS. |
| 7 | **AdMob:** thêm app iOS, tạo 3 ad unit (banner, interstitial, rewarded) | Gửi tao app ID và 3 unit ID. Vào Privacy & messaging, tạo thông báo giải thích trước hộp hỏi theo dõi của Apple (IDFA explainer) và thông báo cho các bang của Mỹ. |
| 8 | **Google Ads:** tài khoản, thanh toán | Liên kết Firebase hoặc GA4 (Admin → Product links → Google Ads), bật auto-tagging, import conversion (mục 3.3). |
| 9 | **Kênh YouTube** | Google Ads chỉ nhận video đăng trên YouTube. Đăng các clip ở chế độ Unlisted. |
| 10 | **Máy build iOS** | Trong Unity Hub, thêm module **iOS Build Support** cho Unity 6000.3.19f1. Cần Xcode mới và đăng nhập Apple ID của team vào Xcode để ký bản build. |

### 2.2 Tao làm trong code

| # | Việc | Chi tiết |
| --- | --- | --- |
| 1 | Firebase, AdMob, Remote Config chạy trên iOS | Bỏ giới hạn chỉ Android trong `COgheGoogleServices`, đưa `GoogleService-Info.plist` vào build, điền app ID và unit ID iOS. Builder đã có sẵn chốt chặn: bản store thiếu AdMob iOS thì không build. |
| 2 | Hỏi quyền theo dõi (App Tracking Transparency, ATT) | Thêm câu `NSUserTrackingUsageDescription`. Hỏi sau khi người chơi thắng màn đầu, không hỏi lúc vừa mở app. Trước đó hiện thông báo giải thích của AdMob (UMP), có trả lời rồi mới tải quảng cáo. Apple cấm thưởng hay khoá tính năng theo câu trả lời. |
| 3 | `SKAdNetworkItems` và privacy manifest | Danh sách mạng quảng cáo của Google/AdMob trong Info.plist. Kiểm tra file `PrivacyInfo.xcprivacy` của các SDK và của app. |
| 4 | Mua trong app thật | Unity IAP (StoreKit) cho Plus và No Ads, kèm nút Khôi phục. Gửi doanh thu sang Firebase. Phải kiểm trong DebugView để không đếm một giao dịch hai lần. |
| 5 | Ba sự kiện mốc | `level_5_complete`, `home_unlocked` (thắng Boss màn 12, mở Nhà), `level_24_complete`. Dùng để đo phễu; sau này đủ người thì import vào Google Ads. |
| 6 | Mục "Report an ad" | Apple yêu cầu app có quảng cáo phải cho người dùng báo quảng cáo không phù hợp. Thêm vào màn Pause, gửi email về địa chỉ hỗ trợ. |
| 7 | Build iOS, TestFlight, kiểm trên iPhone thật | Hỏi theo dõi, quảng cáo test, DebugView nhận sự kiện, mua thử bằng tài khoản sandbox, Khôi phục, chơi offline. |

Ước lượng: phần code mất 2–3 ngày làm việc sau khi tao có file và ID từ Mrk. Apple duyệt thường mất 1–3 ngày. **Chỉ bật
quảng cáo sau khi bản trên App Store gửi được sự kiện `first_open` về Firebase**, kiểm bằng một lần cài thật từ App Store.

## 3. Campaign Google Ads

### 3.1 Cài đặt

| Mục | Chọn | Lý do |
| --- | --- | --- |
| Loại | App promotion → **App installs** | Tháng đầu chỉ đủ dữ liệu để tối ưu lượt cài. |
| Nền tảng | iOS, tìm app bằng App Store ID | |
| Tên | `COghe iOS US Installs 2026-10` | |
| Vị trí | United States, "Presence: people in or regularly in" | |
| Ngôn ngữ | English | Toàn bộ game và quảng cáo là tiếng Anh. |
| Giá thầu | **Maximize conversions**, tối ưu theo Installs, **không đặt CPI mục tiêu** | Google gợi ý cách này cho iOS. Đặt CPI mục tiêu thấp trên ngân sách nhỏ thường làm campaign không tiêu hết tiền và học chậm. |
| Ngân sách | Theo lịch mục 4 | |
| Số campaign | **1** | Ngân sách nhỏ chia cho nhiều campaign thì không campaign nào học được. Khi chưa có đo lường trên máy (on-device measurement), Google khuyên tối đa 8 campaign cho mỗi app iOS. |
| Nhóm quảng cáo | 1 nhóm "Evergreen" | Mục 5. |

### 3.2 Nội dung trong nhóm quảng cáo

Tiêu đề (tối đa 5, ≤ 30 ký tự) và mô tả (tối đa 5, ≤ 90 ký tự) nằm trong `APP_STORE/METADATA.md`, đã đếm ký tự. Video: tất
cả clip trong `ADS/`, đủ 3 khổ, từ 10 đến 30 giây. Ảnh: 3 khổ trong `ADS_IMAGES/`.

### 3.3 Conversion

| Sự kiện (từ Firebase) | Vai trò |
| --- | --- |
| `first_open` (iOS) | **Chính**, "Include in conversions: Yes". Campaign tối ưu theo sự kiện này. |
| `level_5_complete`, `home_unlocked`, `level_24_complete` | Phụ, "Include in conversions: No": chỉ quan sát. |
| Doanh thu mua trong app (`in_app_purchase` hoặc `purchase`) | Phụ, quan sát. |

- Đánh dấu các sự kiện trên là **Key event** trong GA4 trước khi import.
- Cửa sổ ghi nhận conversion để mặc định 30 ngày.
- Số liệu SKAdNetwork của Apple: đặt bảng giá trị conversion trong GA4 (Admin → Data streams → iOS → SKAdNetwork
  conversion value schema) theo các mốc chơi ở trên, để Firebase tự gửi. Ở lượng cài này, Apple sẽ ẩn phần lớn giá trị
  chi tiết, nên đây chỉ là số tham khảo.
- Google Ads trên iOS báo lượt cài trễ tới 5 ngày. Không đánh giá CPI theo từng ngày; luôn xem cửa sổ 7 ngày đã qua ít
  nhất 5 ngày.

### 3.4 Không làm trong tháng này

- Tối ưu theo hành động trong game (target CPA): hành động phải có ít nhất 10 người làm mỗi ngày; checklist của Google
  khuyên 30 lần/ngày.
- Tối ưu theo doanh thu (target ROAS): cần ít nhất 10 conversion/ngày, hoặc 300 trong 30 ngày.
- Thí nghiệm A/B của Google Ads: tính năng so sánh video và đo uplift hiện chưa hỗ trợ iOS.
- Trang App Store riêng cho từng quảng cáo (Custom Product Page): tài liệu Google không có cách dẫn traffic iOS tới đó.

## 4. Lịch ngân sách 30 ngày

Ngày 1 là ngày đầu tiên campaign chạy, sau khi `first_open` từ bản App Store đã về Firebase.

| Ngày | $/ngày | Việc | Điều kiện để tăng sau giai đoạn này |
| --- | ---: | --- | --- |
| 1–7 | 20 | Giai đoạn học. Không đổi gì, trừ khi có lỗi tracking. Mỗi ngày xem đã tiêu tiền chưa và `first_open` có về không. | Ngày 7: kiểm tracking và CPI sơ bộ. Chưa đủ lượt cài để đánh giá tỉ lệ quay lại |
| 8–9 | 24 | | |
| 10–14 | 29 (mốc 30) | Ngày 14: lần đầu xem hiệu quả từng clip (asset label), thêm clip mới. | Ngày 14: lần xét đầu tiên đủ số liệu (mục 8) |
| 15–16 | 35 | | |
| 17–18 | 42 | | |
| 19–21 | 50 (mốc 50) | Ngày 21: thêm hoặc thay clip lần hai. | Ngày 21: xét lần hai |
| 22–23 | 60 | | |
| 24–25 | 72 | | |
| 26–27 | 86 | | |
| 28–30 | 100 (mốc 100) | | Họp tổng kết ngày 30, chốt lại số ngày 37 khi nhóm cài cuối đủ 7 ngày |

- **Tổng khoảng 1.370 $.** Nếu muốn đúng 1 tháng ở 100 $/ngày cho tuần cuối như kế hoạch ban đầu thì khoảng 1.600 $.
- **Mỗi lần tăng chỉ khoảng 20%.** Trong một tuần, ngân sách đi theo lịch. Lần xét ở ngày 7, 14, 21 quyết định cả tuần sau có đi tiếp không. Không đạt thì giữ mức đang chạy.
- **CPI cao:** giữ ngân sách và thay clip trước; không hạ ngân sách đột ngột.
- **Tỉ lệ quay lại ngày 1 thấp:** đây là vấn đề của game, không phải của quảng cáo. Dừng tăng, xem phễu ở mục 7.3 để biết
  người chơi bỏ ở màn nào, rồi sửa game.
- **Lỗi tracking** (mất `first_open`, sự kiện không về): tạm dừng campaign tới khi sửa xong.

## 5. Clip và ảnh quảng cáo

### 5.1 Bộ giao lần này

Tất cả quay từ bản game hiện tại (commit 254a1366: có mắt, mũ đứng yên, thùng cao, bám mọi mặt). Không có cảnh dựng giả.
Chữ trên clip là tiếng Anh. Nhạc và tiếng động là bản gốc làm cho quảng cáo 03/10.

| Clip | Dài | Ý chính |
| --- | --- | --- |
| AD1 Puzzle | 15 s | Hỏi thử thách "Can you get COghe out?": đu dây, kéo tay nắm, tách đôi, bánh răng, đẩy thùng, nhảy mừng |
| AD2 Pet | 17 s | "A pet made of liquid": vuốt, ôm (mắt trái tim), chọc nhiều thì giận, chơi bóng, ăn, đổi màu, đội mũ |
| AD3 Showcase | 30 s | Cả game: 5 loại câu đố, "60 puzzles to solve", rồi nuôi, đổi màu, đội mũ |

Mỗi clip có 3 khổ, tổng 9 video. Chi tiết ở `ADS/README.md`. Ảnh quảng cáo: 3 cảnh (mắt trái tim, tách đôi, COghe màu
xanh lá) × 3 khổ.

### 5.2 Cách thay clip

- Ngày 1: đăng tất cả clip, đủ 3 khổ, cùng một nhóm quảng cáo.
- Ngày 14 và 21: mở báo cáo Assets. Nhóm mới dùng 9 trên 20 chỗ cho video, nên Google khuyên **thêm** clip trước, chưa xoá
  clip "Low". Clip "Best" thì làm thêm biến thể cùng ý (đổi 2 giây đầu, đổi chữ). Chỉ khi đủ 20 video mới thay dần từng
  clip "Low".
- Clip còn ở trạng thái "Learning" thì chưa kết luận gì. Google không công bố cần bao nhiêu lượt hiển thị.

### 5.3 Ý tưởng cho đợt clip sau

- **"Fail rồi thắng":** COghe làm sai một bước, quay lại, rồi giải được. Kiểu mở đầu bằng thất bại thường giữ người xem.
- **Mắt COghe nhìn thẳng người xem** trong 1 giây đầu, chữ "Hi." rồi cắt nhanh vào màn chơi.
- **"Chỉ một nước đi":** đóng băng màn chơi, hỏi "Which handle first?", rồi đáp án.
- **Cận cảnh tách đôi và nhập lại**, tua chậm.
- **Nuôi COghe:** cho ăn, ôm, đội mũ; nhắm người thích nuôi thú ảo.

## 6. App Store (ASO)

Nội dung đầy đủ để dán vào App Store Connect nằm trong `APP_STORE/METADATA.md`. Tóm tắt các quyết định:

### 6.1 Tên, phụ đề, từ khoá

- **Tên:** `COghe: Physics Puzzle Pet`. Các game vật lý dẫn đầu (Cut the Rope, Royal Smash) đều có "Physics Puzzle"
  trong tên. Thêm "Pet" cho phần Nhà. Trên App Store Mỹ chưa có app nào tên "coghe".
- **Phụ đề:** `Stretch, Grab & Solve Levels`.
- **Từ khoá:** 16 từ, đúng 100 byte, không lặp chữ của tên và phụ đề.
- Không có số liệu lượt tìm thật (cần công cụ trả phí). App Store Connect chỉ cho biết bao nhiêu lượt cài đến từ tìm kiếm,
  không tách theo từ khoá. Muốn đo từng từ phải dùng công cụ ASO trả phí hoặc Apple Ads, ngoài phạm vi tháng test.
- Chữ "slime" kéo cả người tìm game đồ chơi ASMR; chữ "blob" kéo cả người tìm game chạy hyper-casual và game .io. Vẫn
  giữ vì hợp hình dạng COghe.

### 6.2 Screenshot

8 ảnh, mỗi ảnh có chữ lớn ở dải màu phía trên và cảnh quay thật bên dưới (cách Cut the Rope và Royal Smash đang làm).
Ảnh màn chơi có thêm một ô tròn phóng to COghe để thấy mắt. Thứ tự: đu dây, tách đôi, ôm (mắt trái tim), bánh răng, chơi
bóng, thùng, thay đồ, 60 màn. Khi chưa có video xem trước, ba ảnh đầu hiện ngay trong kết quả tìm kiếm, nên có cả hai mặt: giải đố và thú cưng. Khi có video, video hiện ở chỗ đó. Chi tiết
từng ảnh ở `APP_STORE/METADATA.md`.

### 6.3 Video xem trước (App Preview)

- 29,5 giây, 886×1920, đúng quy cách Apple: chỉ cảnh quay trong game, có chữ mô tả, không giá, không nút "Play free".
- Video tự chạy không tiếng và đứng trước screenshot, nên 2 giây đầu là COghe nhìn thẳng ra.
- Ảnh bìa đặt ở giây 6 (COghe đang đu dây). Mặc định của Apple là giây 5, rơi đúng chỗ chuyển cảnh.

### 6.4 Icon

- Dùng icon đã duyệt 07/10 (nền xanh bạc hà) cho cả tháng test, để số liệu so sánh được với nhau.
- Ba bản đổi màu nền (cam san hô, tím, vàng) dùng cho thử nghiệm icon của App Store (Product Page Optimization) sau tháng
  test. Icon thay thế phải nằm sẵn trong bản build. Với vài trăm lượt cài mỗi tháng, thử nghiệm sẽ không đủ người để kết luận.

### 6.5 iPhone và iPad

Bản build hiện để "iPhone + iPad", nên App Store bắt buộc có screenshot iPad 13". Tao đã làm đủ bộ iPad. **Đề xuất cho
tháng test: build chỉ cho iPhone.** Lý do:
- Bớt một loại màn hình phải kiểm (giao diện iPad tỉ lệ 4:3 chưa ai thử).
- Người dùng iPad vẫn cài được bản iPhone.
- Quảng cáo nhắm vào điện thoại là chính.

Mày quyết. Nếu giữ iPad thì dùng bộ `IPAD_13_2064x2752`.

### 6.6 Quyền riêng tư và độ tuổi

- **App Privacy:** bảng khai báo ở `APP_STORE/METADATA.md`. Đã khai "dùng để theo dõi" thì bắt buộc phải có hộp hỏi theo
  dõi (ATT). Apple từ chối app khai mà không hỏi.
- **Độ tuổi:** dự kiến 4+. **Không** chọn "Made for Kids".
- **Báo cáo quảng cáo xấu:** quy định 2.5.18 của Apple yêu cầu app có quảng cáo phải cho người dùng báo quảng cáo không phù hợp. Tao
  thêm mục "Report an ad" trong Pause, gửi email về địa chỉ hỗ trợ.
- **Cần Mrk quyết, nên hỏi luật sư nếu có thể:**
  - COghe dễ thương, mắt to, có phần nuôi thú, nên luật bảo vệ trẻ em của Mỹ (COPPA, bản sửa áp dụng từ 22/04/2026) có thể
    coi game là "hướng tới trẻ em" dù không ở Kids Category.
  - Từ 2026, Texas, Utah và Louisiana yêu cầu app dùng API độ tuổi của Apple (Declared Age Range).
  - Hướng thường làm: phát hành cho mọi lứa tuổi; đặt mức nội dung quảng cáo của AdMob là G (mọi lứa tuổi); hỏi tuổi
    một lần lúc đầu; người dưới 13 tuổi thì tắt quảng cáo cá nhân hoá. Khoảng nửa ngày code nếu mày chọn hướng này.

## 7. Theo dõi và đánh giá

### 7.1 Xem gì, ở đâu

| Nguồn | Xem gì | Ở đâu |
| --- | --- | --- |
| **Google Ads** | Chi tiêu, lượt hiển thị, lượt cài, CPI, tỉ lệ cài; hiệu quả từng clip, ảnh, dòng chữ | Campaigns; Assets (nhãn Best / Good / Low) |
| **App Store Connect** | Lượt thấy app, lượt xem trang, tỉ lệ chuyển đổi, lượt tải lần đầu theo nguồn; doanh thu mua trong app | App Analytics (Sources, Benchmarks); Sales and Trends |
| **Firebase / GA4** | `first_open`, người chơi mỗi ngày, tỉ lệ quay lại theo nhóm cài, phễu màn chơi, doanh thu quảng cáo và mua trong app | Firebase Dashboard; GA4: Retention, Monetization, Explore; DebugView để kiểm |
| **AdMob** | Doanh thu, eCPM, lượt hiển thị theo loại quảng cáo, tỉ lệ có quảng cáo | Reports |

Thiết lập một lần, trước ngày 1:
1. **Liên kết AdMob với Firebase** (Firebase → Project settings → Integrations → AdMob). Doanh thu quảng cáo sẽ vào GA4
   qua sự kiện `ad_impression`. Không cộng thêm sự kiện `ad_revenue` của game, vì hai cái là cùng một khoản tiền.
2. **Đánh dấu Key event** trong GA4: `first_open`, `level_5_complete`, `home_unlocked`, `level_24_complete`, sự kiện mua.
3. **Ba báo cáo trong GA4 Explore:**
   - Cohort: tỉ lệ quay lại theo tuần cài, tách theo "First user Google Ads campaign" để so người từ quảng cáo với người tự tìm.
   - Funnel: `first_open` → thắng màn 1 → `level_5_complete` → `home_unlocked` → `level_24_complete`.
   - Bảng kết quả từng màn: đã có hướng dẫn ở `Docs/Monetization/COghe/GOOGLE_SERVICES.md` (nhánh NewGraphic).
4. **Bảng giá trị SKAdNetwork** trong GA4, theo các mốc ở trên.
5. **Bảng theo dõi tuần:** `WEEKLY_TRACKER.csv` trong thư mục giao, mở bằng Google Sheets. Mỗi tuần một dòng.

### 7.2 Công thức

| Chỉ số | Cách tính | Lấy số ở |
| --- | --- | --- |
| CPI | Chi tiêu ÷ lượt cài | Google Ads. Đối chiếu `first_open` từ Google Ads trong GA4 |
| IPM | Lượt cài ÷ lượt hiển thị × 1.000 | Google Ads |
| Tỉ lệ chuyển đổi trang store | Lượt tải lần đầu ÷ lượt xem trang | App Store Connect |
| Quay lại ngày 1 (D1), ngày 7 (D7) | % người cài ngày X còn mở game ngày X+1, X+7 | GA4 Cohort |
| ARPDAU | (Doanh thu quảng cáo + doanh thu mua thực nhận) trong ngày ÷ người chơi trong ngày | AdMob, App Store Connect, GA4 |
| ARPU D7 | (Doanh thu quảng cáo + doanh thu mua thực nhận) của nhóm cài trong 7 ngày đầu ÷ số lượt cài | GA4. Doanh thu mua: Apple giữ 15% nếu tham gia [Small Business Program](https://developer.apple.com/app-store/small-business-program/), nếu không thì 30% |
| ROAS D7 | ARPU D7 ÷ CPI | Tính tay |
| LTV ước tính 30 ngày | ARPDAU × số ngày chơi trung bình trong 30 ngày (cộng tỉ lệ quay lại của ngày 0 tới 30) | Tính tay |
| CPI hoà vốn | = LTV ước tính | So với CPI thật |

**Ví dụ để thấy khoảng cách, số là ước lượng của tao:**
- Quay lại: ngày 1 là 30%, ngày 7 là 8%, ngày 30 là 2%. Như vậy mỗi lượt cài chơi khoảng 3 ngày trong 30 ngày đầu.
- Mỗi ngày chơi mang về 0,02–0,05 $ quảng cáo. eCPM ở Mỹ trên iOS: interstitial khoảng 13 $, rewarded khoảng 17 $,
  banner khoảng 0,4 $. Mỗi ngày người chơi thấy khoảng 1–2 interstitial, vì game chỉ hiện từ màn thắng thứ 6.
- LTV 30 ngày khoảng 0,1–0,25 $, tính cả mua trong app.
- Trong khi CPI Mỹ trên iOS dự kiến 1,5–5 $.
- **Vậy rất có thể tháng test không hoà vốn.** Đây là điều bình thường với game mới chưa tối ưu. Việc của tháng này là đo
  đúng CPI, tỉ lệ quay lại và người chơi bỏ ở đâu, để biết phải sửa gì trước khi chi lớn.

### 7.3 Phễu trong game

Xem trong GA4 Funnel, tách người từ Google Ads và người tự tìm:

| Bước | Ý nghĩa nếu tụt mạnh |
| --- | --- |
| `first_open` → bắt đầu màn 1 | Intro hoặc lần mở đầu có vấn đề: tải lâu, hỏi quyền quá sớm |
| Màn 1 → thắng màn 1 | Người chơi không hiểu cách điều khiển |
| Màn 1 → `level_5_complete` | Độ khó các màn đầu |
| → `home_unlocked` (màn 12) | Chương 1 dài hoặc khó; người chơi chưa kịp thấy phần Nhà |
| → `level_24_complete` | Giữ chân trung hạn |

Màn nào có nhiều `level_retry` và `level_abandon` mà ít thắng thì xem bảng kết quả từng màn để sửa.

### 7.4 Nhịp kiểm tra

- **Mỗi ngày, 5 phút:**
  - Google Ads có tiêu tiền không;
  - `first_open` có về Firebase không;
  - có lỗi crash mới không;
  - doanh thu AdMob có số không.
  - Không sửa campaign vì số của một ngày.
- **Ngày 7, 14, 21, 30:** điền một dòng vào `WEEKLY_TRACKER.csv`, so với mục 8, quyết định tăng, giữ hay dừng. Tao làm phần
  đọc số và viết tóm tắt nếu mày cho quyền xem Firebase, Google Ads và AdMob, hoặc gửi ảnh chụp báo cáo.
- **Ngày 37:** chốt số D7 cho nhóm cài cuối cùng và viết báo cáo tổng kết.

## 8. Ngưỡng đánh giá

Số tham khảo cho game giải đố và casual. Nguồn ghi trong `RESEARCH/COGHE_GOOGLE_ADS_IOS_2026_10_08.md`:
- CPI casual iOS toàn cầu 1,41 $ (Liftoff, 2025); CPI mọi game ở Mỹ 1,71 $ (Adjust, 2025, cả iOS và Android). iOS ở Mỹ cao
  hơn, ước 1,5–5 $.
- Quay lại ngày 1: game giải đố có trung vị khoảng 20%, ngày 7 khoảng 4–5%. Nhóm 25% game tốt nhất, tính mọi thể loại:
  ngày 1 là 31–33% trên iOS, ngày 7 là 7–8% (GameAnalytics 2025; không có số "tốt nhất" riêng cho game giải đố).
- eCPM Mỹ trên iOS: rewarded khoảng 17 $, interstitial khoảng 13 $, banner khoảng 0,4 $ (Appodeal, quý 4/2024).
- Tỉ lệ chuyển đổi trang App Store trung bình mọi nhóm app ở Mỹ: 8,6% (AppTweak, 2025). Nên so với mức trung vị nhóm
  tương tự, xem trong App Store Connect → Benchmarks.

| Chỉ số (nhóm cài ≥ 50 lượt) | Tốt | Tạm | Xấu, thì làm gì |
| --- | --- | --- | --- |
| CPI (cửa sổ 7 ngày đã qua ít nhất 5 ngày) | ≤ 2,5 $ | 2,5–5 $ | > 5 $: thêm clip mới, xem lại 3 ảnh đầu và video xem trước |
| Quay lại ngày 1 | ≥ 30% | 20–30% | < 20%: dừng tăng tiền, sửa phần mở đầu và các màn đầu |
| Quay lại ngày 7 | ≥ 8% | 4–8% | < 4%: xem phễu; có thể đưa Nhà ra sớm hơn |
| Thắng màn 1 / lượt cài | ≥ 80% | 60–80% | < 60%: người chơi không hiểu cách chơi |
| Tới Nhà (màn 12) / lượt cài | ≥ 30% | 15–30% | < 15%: chương 1 quá dài hoặc khó |
| ROAS D7 | ≥ 15% | 5–15% | < 5%: chưa nên chi lớn; tăng giữ chân và quảng cáo trong game trước |

Hai dòng phễu và dòng ROAS là mục tiêu tao đặt cho COghe; ngành không công bố chuẩn cho các số này.

**Quy tắc tăng tiền:**
- **Ngày 7:** chỉ dừng lịch nếu tracking lỗi hoặc CPI sơ bộ quá 6 $. Lúc này chưa đủ 50 lượt cài và chưa đủ 5 ngày trễ để đánh giá.
- **Ngày 14 và 21:** **tăng** theo lịch tuần sau nếu CPI không "Xấu" **và** quay lại ngày 1 không "Xấu" **và** tracking chạy đúng.
- **Chưa đủ số liệu:** lượt cài trong cửa sổ đã qua 5 ngày dưới 50, hoặc CPI chưa ra được Tốt hay Xấu. Khi đó giữ mức đang
  chạy thêm 3–4 ngày rồi xét lại. Không tăng tiền khi chưa đọc được số.
- **Giữ** ngân sách nếu một trong hai chỉ số đầu "Xấu". Thêm clip mới nếu là CPI; sửa game nếu là quay lại ngày 1.
- **Dừng** quảng cáo trả tiền nếu tới ngày 21 vẫn CPI > 6 $ **và** quay lại ngày 1 < 20%: chi thêm cũng không có thêm thông tin.

## 9. Rủi ro

- **Số nhỏ, dao động lớn.** 1.370 $ ra khoảng 270–900 lượt cài, với CPI 1,5–5 $. Với 300 lượt cài, tỉ lệ quay lại ngày 7 sai lệch khoảng
  ±3 điểm phần trăm. Chỉ kết luận theo tuần, không theo ngày.
- **Google Ads trên iOS báo trễ và có phần ước lượng.** Lượt cài về muộn tới 5 ngày. Một phần conversion là số Google mô hình
  hoá vì người dùng từ chối theo dõi. Luôn đối chiếu với lượt tải trong App Store Connect.
- **Bị Apple từ chối.** Hay gặp ở các điểm:
  - khai App Privacy mà không có hộp hỏi theo dõi;
  - thiếu nút Khôi phục giao dịch;
  - thiếu mục báo cáo quảng cáo;
  - mô tả nói điều game chưa có.
  - Kế hoạch đã tính cả bốn điểm.
- **Trẻ em và luật độ tuổi ở Mỹ** (mục 6.6): cần mày quyết trước khi gửi duyệt.
- **Ngân sách thấp hơn mức Google khuyên.** Google khuyên ngân sách ngày gấp 50 lần CPI mục tiêu. Ở 20 $/ngày, campaign học
  chậm và có thể không tiêu hết tiền những ngày đầu. Đây là giới hạn của test nhỏ, không phải lỗi cài đặt.
- **Google tự tạo video.** Nếu nhóm không có video nào, Google tự ghép video từ ảnh của trang store. Mình đưa sẵn 9 video nên không cần.
