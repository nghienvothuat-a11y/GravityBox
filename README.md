# COghe iOS: kế hoạch và tài nguyên cho test Google Ads 30 ngày ở Mỹ

Nhánh riêng cho kế hoạch và tài nguyên quảng cáo, không chứa code game. Soạn ngày 08/10/2026 theo yêu cầu của Mrk.

## Lấy về máy khác

```sh
git clone -b marketing/coghe-ios-ua-2026-10 --single-branch https://github.com/nghienvothuat-a11y/GravityBox.git coghe-ios-ua
```

Chỉ tải nhánh này (khoảng 120 MB), không tải code game.

## Đọc gì trước

1. **`KE_HOACH.md`**: kế hoạch đầy đủ.
   - Mục 2: việc phải xong trước ngày chạy đầu tiên, gồm phần Mrk làm trên các trang quản trị và phần làm trong code.
   - Mục 3: cài đặt campaign.
   - Mục 4: lịch ngân sách.
   - Mục 7–8: cách theo dõi và ngưỡng đánh giá.
2. **`APP_STORE/METADATA.md`**: toàn bộ chữ để dán vào App Store Connect và Google Ads (đã đếm ký tự), thứ tự screenshot,
   bảng khai báo quyền riêng tư.
3. **`WEEKLY_TRACKER.csv`**: mở bằng Google Sheets, mỗi tuần điền một dòng.

## Tài nguyên

| Thư mục | Dùng ở đâu | Nội dung |
| --- | --- | --- |
| `ADS/` | YouTube (Unlisted), rồi dán link vào nhóm quảng cáo Google Ads | 3 clip × 3 khổ (9:16, 1:1, 16:9) |
| `ADS_IMAGES/` | Google Ads, mục Images | 3 cảnh × 1200×1200, 1200×1500, 1200×628 |
| `APP_STORE/SCREENSHOTS/` | App Store Connect, mục Screenshots | 8 ảnh × 3 bộ: iPhone 6.9", iPhone 6.3", iPad 13" |
| `APP_STORE/PREVIEW/` | App Store Connect, mục App Previews (iPhone) | 29,5 s, 886×1920 |
| `APP_STORE/ICON/` | Unity hoặc Xcode (icon 1024) | Icon đã duyệt và 3 bản đổi màu nền để thử nghiệm sau |
| `OVERVIEW_*.png` | Xem nhanh | Ảnh tổng hợp screenshot, icon, ảnh quảng cáo |
| `RESEARCH/` | Tham khảo | Nghiên cứu Google Ads cho iOS và ASO trên App Store, có nguồn |
| `PLANS/COGHE_MONETIZATION_PLAN.md` | Tham khảo | Kế hoạch kiếm tiền: quảng cáo, Plus, No Ads, sự kiện |
| `SOURCE/` | Chỉ khi cần dựng lại | Script và file cấu hình để đổi chữ hay đổi cảnh rồi dựng lại |

Script trong `SOURCE/` dùng các cảnh quay gốc trên máy làm việc (`Artifacts/` của repo game, không vào git). Muốn dựng lại
thì nhờ Claude trên máy đó. Triển khai campaign không cần các script này.
