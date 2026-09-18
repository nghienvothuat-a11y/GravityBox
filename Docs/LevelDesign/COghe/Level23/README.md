# COghe — 23: Khớp rồi!

**Prototype Unity v2, 17/09/2026.** ID ổn định `venom.origin.23`, vị trí chơi **9**,
sau màn cũ 07 và trước Boss đầu mới. Scene, chọn cơ quan qua camera, bộ truyền–cửa,
reset và ngân sách mô phỏng đã qua test macOS; chưa playtest tay mobile.
[Tiến trình](../../../COGHE_CAMPAIGN_30_DESIGN.md) · [Quy chuẩn](../../../COGHE_LEVEL_DESIGN_RULES.md) ·
[Day Lab](../../../ArtDirection/COghe/STYLE_RULES.md).

## 1. Ý tưởng và bố trí

![Phác thảo 23 · Khớp rồi!](mockup-v1.png)

[Xem cả 10 bản phác thảo](../Sketches21-30/review.html). Bản v1 để duyệt bố cục và thao tác;
chi tiết tỷ lệ, vách kín, ray/chốt và tiếp xúc cơ khí được giản lược, cần kiểm chứng khi dựng.

Sau khi biết bám vật và chỉ đích, người chơi dùng cùng thao tác để nối một bộ truyền.
Chỉ một bánh trung gian cần đặt. Nhìn chuyển động truyền tới cửa là phần thưởng.

```text
MẶT CƠ QUAN — nhìn từ trong hộp; không phải gợi ý trong HUD
    bánh nguồn M        vị trí cuối A       bánh ra R ┃ thanh răng/cửa
         ⚙ ───────────────── ⚙ ─────────────── ⚙      ┃ [ O bị che ]
                              ↑                      ┃
                      ray có chặn cuối               ┃
                             [A]                     ┃
                         tay nắm cố định             ┃
    S / sàn bám ───── vùng đứng kéo rộng ───── tới cửa đã mở
```

- M/R cố định; A nằm trên giá trượt ngắn vuông góc hàng truyền động. Cuối ray là
  vị trí đúng khoảng cách tâm và cùng mặt phẳng; người chơi không phải căn micromet.
- Ray có chốt cuối, kéo ngược nhả. Tay nắm gắn giá **không quay**, dễ chạm phía trước.
- Động cơ nhỏ hiện rõ ở M, mô-men hữu hạn; R truyền lực sang cửa thanh răng, chốt
  giữ mở đủ. Răng có che chắn không cắt mô và không tạo đường đứng kẹt nguy hiểm.
- Lỗ O nằm trên vỏ với cửa kín thật che hoàn toàn lúc đầu. Leo quanh bộ truyền
  không làm thoát qua tấm che; cửa không có tay nắm thao tác trực tiếp ở màn này.
- Hộp khóa xoay; camera 3/4 gần đối diện mặt máy, thấy giá/tay nắm/cửa cùng lúc.

## 2. Lời giải và sửa sai

1. Chạm tay nắm A; sinh vật tới bám. Chạm vùng phía cuối ray để đẩy A tới chốt.
2. A ăn khớp thật với M/R; chuyển động chạy qua ba bánh, cửa nâng theo tải tới chốt.
3. Buông prop sau 3 giây hoặc đổi lệnh hợp lệ, rồi chỉ O và thoát đủ bản thể.

Một vùng đích rộng trên mặt thao tác phải dẫn tới cùng hành trình. Không thêm
thao tác kéo răng bằng ngón, không tự snap A khi còn cách xa vị trí ăn khớp.
Nếu đẩy sai, chặn ray dừng vật và có phản hồi đang tì; kéo ngược sửa được. Nếu buông
giữa đường, trạng thái dở dang ổn định đủ để bám lại; không trượt mất toàn bộ tiến bộ.

## 3. Trạng thái cơ quan

| Trạng thái | Kết quả thật | Hủy/đổi hướng |
| --- | --- | --- |
| A chưa ăn khớp | M quay, R/cửa đứng | A kéo/đẩy tự do trong ray |
| A tiếp xúc vòng chia | Bộ truyền có tải, cửa đi lên | Mất khớp thì ngừng truyền, cửa có cơ cấu giữ tải |
| Cửa tới cuối | Chốt giữ, nguồn dừng hoặc ly hợp giới hạn tải | Rời A không làm cửa đóng lại |
| Reset | A/cửa về pose đầu, lệnh/nguồn/catch đặt lại | Không giữ lực từ lần chơi trước |

Không dùng `AtEnd` của A thay toàn bộ kiểm tra ăn khớp. Đèn chỉ sáng khi bộ truyền
thật nhận tải. Chiều quay/thanh răng được tính theo bố trí, không hoạt ảnh cửa độc lập.
Luật thắng chuẩn; không có dao hoặc thêm điều kiện lịch sử đã đẩy bao nhiêu lần.

## 4. Runtime và trình bày

Tái dùng `COgheRailSlider`, `VenomMovableProp`, `COgheGearTrain` với M/A/R và Rack.
Kiểm tra phanh/giữ tải của Rack trong lúc mất khớp; mô hình hiện có dùng bộ truyền
lý tưởng, không phải va chạm từng răng. Cần author ray, bán kính và drive đúng chiều.
Builder/definition đã được tạo theo ID nội dung; runtime không branch theo display slot.

Giá hổ phách, răng đồng, cửa sứ/thanh răng thép. COghe bám–dồn thân đẩy, sau khớp
ngoái theo chuyển động lan tới cửa. Tiếng “cạch” đọc từ chốt thật, âm máy nhỏ và ngừng
khi xong; phản hồi thưởng ngắn không làm mất quyền điều khiển.

## 5. Kiểm chứng và ngân sách

Suy luận nhẹ–vừa, thao tác nhẹ: một quyết định vị trí, một phần cơ thể, không timer.
Cấu hình tối thiểu dự kiến: một giá động và cửa; ba bánh theo constraint hiện có.
Không thêm rigidbody từng răng hoặc lực vô hạn tại chặn. Mục tiêu giữ profile chung.

Test: A từ cả hai hướng, tap lệch vùng đích, buông/hủy giữa ray, kéo ngược khi vừa
ăn khớp, chặn tải cửa, đứng gần máy, leo vòng không xuyên cửa, đủ mô thoát, pause/reset.
Đường giải qua lệnh thật; chơi tay kiểm tra một lệnh đích có hoàn tất trong tác vụ
3 giây hay không. Đo OPPO p95/p99/max, skin/route lúc A chuyển; chưa có kết quả.

## 6. Tích hợp

Vị trí 9 là dữ liệu tiến trình; ID 23 dùng lưu trạng thái thắng riêng. Cần thêm vào
catalog/build/test/migration theo campaign 30 màn. Tuyến đưa G ăn khớp, nâng Rack và
thoát đã qua full-solution test; playtest thiết bị vẫn là mốc riêng.

## 7. Sửa bám tay nắm — 18/09/2026

Người dùng báo COghe bật ra và giật khi đẩy G ở vị trí chơi 09. Test một lệnh đẩy
tái hiện: mất tay nắm sau khoảng một giây, G dừng ở 61,8 mm/160 mm. Test lời giải
cũ tự bám lại nhiều lần nên chưa phát hiện chất lượng thao tác này.

Runtime chung `VenomPropManipulation` chọn chỗ đặt chân trên mặt cố định cạnh ray,
tách khỏi điểm nắm tay và kiểm tra khoảng trống cho thân. Chỉ bắt đầu đẩy khi có
tiếp xúc chân thật; thân tiếp tục theo mặt này khi tay nắm dịch chuyển. Lực theo
khối lượng, phản lực, ma sát, chốt và thời hạn buông 3 giây giữ nguyên.
Không thêm điều kiện theo số màn, không thay collider hay bố cục đã duyệt.

Test tập trung màn 09 đã tới chốt trong một lệnh, không mất bám và không tụt ngược
trong hành trình. Kết quả hồi quy/các màn dùng chung/build được ghi tại
[hồ sơ kiểm chứng](../../../Verification/COgheCampaign30/README.md).

Đã chơi tay qua màn 09 trên bản MacOS sau sửa: một lệnh đẩy đưa G tới chốt, cửa
mở, chỉ lỗ và thoát; game tự sang Boss 10. Cần lượt kiểm tra cảm giác trên điện thoại
riêng khi có bản cài mới.
