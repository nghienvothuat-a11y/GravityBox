# Nghiên cứu chuyển động symbiote — 10/09/2026

Mục tiêu: thay cảm giác “viên thạch có chân” bằng một khối vật chất sống có hướng thăm dò, độ dồn và độ trễ. Tham chiếu chính là **symbiote chưa nhập vật chủ** trong Venom (2018), không lấy cách đi của Venom hình người làm dáng bò trong hộp.

## Những gì xác nhận được từ người làm phim

Paul Franklin, VFX supervisor của phim, mô tả rig có một thân trung tâm liên tục đổi hình, di chuyển theo mọi hướng. Sau animation chủ đạo, nhóm FX thêm mô cuộn, các sợi nối hình thành–tan đi và lớp chuyển động nhớt. Họ tham khảo slime mould, sứa và amoeba. [Phỏng vấn Franklin — The Art of VFX, 18/10/2018](https://www.artofvfx.com/venom-paul-franklin-overall-vfx-supervisor-dneg/).

Trong một phỏng vấn khác, Franklin giải thích các lớp màng gập lên nhau tạo chuyển động cuộn phức tạp, nhưng vẫn giữ ý định do animator đặt ra. Đây là căn cứ để ưu tiên chuyển động của khối lớn trước chi tiết bề mặt. [Phỏng vấn Franklin — Cartoon Brew, 08/10/2018](https://www.cartoonbrew.com/feature-film/venom-what-happens-when-the-creature-you-have-to-create-is-so-many-different-things-165024.html).

Franklin cũng xác nhận chuyển động nhớt là lớp FX bổ sung lên animation; nhóm tham khảo sinh vật biển và chất lỏng phi Newton. Điều này không đồng nghĩa phim dùng một solver chất lỏng tự chạy để quyết định toàn bộ diễn xuất. [Phỏng vấn DNEG — Motion Picture Association, 05/10/2018](https://www.motionpictures.org/2018/10/vfx-artists-explain-how-they-made-venom-scary-as-hell/).

Đã đối chiếu thêm hình ảnh ở khoảng 00:05–00:10 trong [Extended Preview chính thức của Sony](https://www.youtube.com/watch?v=Worz_qCsYvU&t=5s): khối symbiote trong khu thí nghiệm có các nếp mô dày và mép loang mỏng, không có bộ chân chia đều. Đoạn này chỉ được dùng để tham chiếu hình khối; không coi một vài khung hình là dữ liệu đo tốc độ hoặc chu kỳ chuyển động. Không sao chép hay đưa footage phim vào game.

## Chẩn đoán bản trước

Các nhận xét dưới đây là đánh giá từ code và hình render của prototype, không phải thông số do DNEG công bố.

- Thân khá tròn, phần lớn hoạt động nằm ở phụ kiện: đầu nổi lên và các chân. Silhouette chính thay đổi ít.
- Sáu xúc tua chia đều quanh thân, dùng cùng cấu trúc chu kỳ. Lệch pha vẫn tạo cảm giác một bộ chân có cơ cấu cố định.
- Đầu nhìn về camera, liếc bằng sóng sin và có hai mắt sáng cố định. Nó giống một con vật nhỏ có cổ hơn là một phần chất sống tạm thời dựng lên.
- Chuyển động chưa có độ trễ rõ giữa vùng tiếp xúc và mô phía trên; tăng tốc, dừng và đổi hướng khó đọc qua thân.

## Áp dụng trong lần chỉnh này

| Tình huống | Cách thể hiện |
| --- | --- |
| Yên | Nếp khối rộng di chuyển chậm qua thân trên; biên độ và vị trí thay đổi mềm, phần đáy bám mặt đỡ. Không phồng đều cả quả cầu. |
| Tò mò | Dồn một vai lệch lên thành một đỉnh mềm; giữ ngắn, nghiêng nhẹ, cuộn thu lại. Chọn hướng có khoảng trống/cơ cấu để thăm dò; không phụ thuộc camera. Bỏ hai mắt sáng. |
| Bắt đầu trượt | Hình khối kéo dài theo vận tốc tương đối với mặt đỡ và hướng dốc. Phần trên trễ hơn phần đáy. |
| Đang trượt | Các sợi mọc độc lập, phần lớn hướng theo dòng chảy, một số giữ phía sau. Góc, độ dài, thời gian giữ và độ dày đổi giữa mỗi lần mọc. |
| Căng/nhả | Đầu sợi giữ điểm trên collider; khi khối đi xa, sợi mảnh dần, cong thu về. Dừng, quá căng hoặc bị vật cản thì nhả. |
| Đổi hướng/dừng | Hướng thân và độ trễ được làm mượt; các điểm bám cũ nhả trước khi các sợi mới chiếm ưu thế. |
| Chia/nhập, rời sàn, thoát | Trạng thái theo nhóm hạt; bỏ điểm bám hết hiệu lực, không neo trang trí vào lỗ hoặc không khí. |

Những thời gian, biên độ và quy tắc này là lựa chọn cho prototype, không phải số đo từ phim. Thân dùng biến dạng field có giới hạn; xúc tua có gốc bè và đầu mảnh. Chưa triển khai màng lưới nhiều lớp hay hàng trăm sợi như shot điện ảnh.

## Giữ quyền điều khiển và vật lý

`CohesiveOrganism` vẫn quyết định chuyển động 32 hạt, khối lượng 96 g, cắt–hàn liên kết, đè nút và thoát. `VenomLifeAnimation` chỉ đọc vận tốc/tiếp xúc để dựng hình. Động tác “bám” là diễn xuất, chưa tạo lực kéo chủ động. Thể tích của mesh chưa được bảo toàn chính xác và nếp khối không phải mô phỏng cơ sinh học.

Hướng phát triển tiếp nếu cảm giác này phù hợp: thêm màng nối có độ dày thay đổi, rồi mới thử lực bám có giới hạn năng lượng và kiểm tra lại thiết kế puzzle. Không dùng chuyển động tự bò mạnh đến mức triệt tiêu thao tác nghiêng của người chơi.

## Kiểm chứng

- 9/9 `VenomPrototypeTests` qua trên Unity 6000.3.19f1, có graphics.
- Kiểm tra vị trí/vận tốc hạt không đổi qua render ở cả lúc yên và chuyển động; đầu bám nằm trên collider và giữ tọa độ cục bộ; nhả khi rời mặt đỡ.
- Pause đóng băng cả các đỉnh mesh. Dịch chuyển camera trong lúc pause không đổi tư thế. Reset xóa trạng thái animation.
- Đường giải PhysX vẫn cắt, mở cửa bằng hai phần, nhập và thoát đủ 32 hạt.
- Chụp 100 frame yên và 34 frame trượt/dừng, 10 fps từ camera cận cảnh. Đây là thời gian mô phỏng, không phải benchmark tốc độ render thiết bị.
- Build macOS cập nhật tại `Builds/Venom/macOS/Venom.app`. Chưa build Android cho lần này.

[Ảnh và animation capture của prototype](Images/VenomLife/README.md). Feeling cần người chơi thử trực tiếp: kiểm tra cơ chế thành công không đồng nghĩa đã đạt chất lượng diễn xuất điện ảnh.
