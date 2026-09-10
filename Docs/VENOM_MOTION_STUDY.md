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

`CohesiveOrganism` vẫn quyết định chuyển động 32 hạt, khối lượng 96 g, cắt–hàn liên kết, đè nút và thoát. `VenomLifeAnimation` chỉ đọc vận tốc/tiếp xúc để dựng hình. Các sợi “bám” là lớp diễn xuất. Từ thí nghiệm 02/03, `VenomLocomotion` tạo lực bò ở tiếp xúc thật; animation đọc ý định điều khiển nhưng không tự tác động lực. Thể tích của mesh chưa được bảo toàn chính xác và nếp khối không phải mô phỏng cơ sinh học.

Hướng phát triển tiếp nếu cảm giác này phù hợp: thêm màng nối có độ dày thay đổi. Màn 01 vẫn giữ quyền điều khiển bằng nghiêng hộp; màn 02/03 dùng hệ lực bò riêng và có cơ chế luồn khe.

## Kiểm chứng

- 12/12 `VenomPrototypeTests` qua trên Unity 6000.3.19f1, có graphics.
- Kiểm tra vị trí/vận tốc hạt không đổi qua render ở cả lúc yên và chuyển động; đầu bám nằm trên collider và giữ tọa độ cục bộ; nhả khi rời mặt đỡ.
- Pause đóng băng cả các đỉnh mesh. Dịch chuyển camera trong lúc pause không đổi tư thế. Reset xóa trạng thái animation.
- Đường giải PhysX vẫn cắt, mở cửa bằng hai phần, nhập và thoát đủ 32 hạt.
- Chụp 100 frame yên và 34 frame trượt/dừng, 10 fps từ camera cận cảnh. Đây là thời gian mô phỏng, không phải benchmark tốc độ render thiết bị.
- Build macOS cập nhật tại `Builds/Venom/macOS/Venom.app`. Chưa build Android cho lần này.

[Ảnh và animation capture của prototype](Images/VenomLife/README.md). Feeling cần người chơi thử trực tiếp: kiểm tra cơ chế thành công không đồng nghĩa đã đạt chất lượng diễn xuất điện ảnh.

### Tinh chỉnh sau phản hồi chơi

Nhịp diễn xuất hiện là 1,5×; phồng thân 3,6 mm, đỉnh tò mò tối đa 41 mm, tầm vươn 34 mm. Mesh theo từng khung hình thay vì giữ tư thế thế giới ở 30 Hz. Skin được giới hạn ở sàn đặc, tôn trọng lỗ thật và các nguồn vật chất đã thoát. Chặn hành trình dưới của cửa/lưỡi chém được sửa cùng lúc để không ép khối vào sàn. [Chi tiết va chạm và kiểm chứng](VENOM_PROTOTYPE_01.md#sửa-tiếp-xúc-sàn-và-tăng-sức-sống--10092026).

### Xúc tu trên cao và động tác nhảy múa — 10/09/2026

Theo yêu cầu diễn xuất mới, cơ thể có hai nhóm xúc tu: tối đa năm sợi bám sàn như trước và bốn sợi tự do mọc phía trên (hai sợi với phần nhỏ). Nhóm trên vươn lên, cuộn đầu rồi thu về theo chu kỳ lệch nhau; gốc, thời lượng và pha quẫy thay đổi giữa các lượt. Khi bò, các sợi trên ngắn lại còn khoảng 58% để vẫn đọc rõ hướng tiến.

Khi đứng yên đủ lâu, sinh vật dồn vai, vươn thành một đỉnh mềm cao hơn rồi uốn thân sang hai bên. Phần vai và ngọn lệch pha; xúc tu giơ lên mạnh hơn trong đợt này, tạo cảm giác nhảy múa mà đáy vẫn tựa trên sàn. Đợt đầu thường xuất hiện sau khoảng bốn giây nghỉ; sau đó khoảng 6–8 giây mỗi lượt với tốc độ animation mặc định 1,5×, mỗi đợt khoảng 2,4–2,9 giây.

Thông số có thể chỉnh trong `Living matter.asset`: `RaisedTendrilReach` = 52 mm, `DanceLift` = 64 mm, `DanceInterval` = 10 giây trên đồng hồ animation (chịu hệ số `AnimationSpeed`). Nếp khối khi nghỉ chạy nhanh hơn trước.

- Bắt đầu điều khiển thì thu động tác vươn thân/nhảy múa. Đang luồn khe, biên độ nếp khối giảm 85%, các xúc tu trên thu gọn theo mức nén; ở mức luồn hoàn toàn thì không giơ xúc tu.
- Đường cong và độ dày xúc tu được kiểm tra với collider xung quanh; gần kính, lưỡi hoặc trần thì rút ngắn cử động. Các ray có đoạn dài gần bằng không được bỏ qua.
- Tất cả dùng thời gian mô phỏng, không phụ thuộc camera hoặc đồng hồ thực. Pause giữ nguyên mesh; reset và thay đổi nhóm xóa các đợt diễn xuất cũ.
- Chỉ thay mesh/field: không dịch hạt, thêm lực, sửa collider, khối lượng, liên kết hoặc điều kiện puzzle. Đây là tạo hình diễn xuất, không mô phỏng cơ sinh học/bảo toàn thể tích của phần vươn thêm.

Kiểm chứng: **14/14** PlayMode có graphics qua, gồm 12 kiểm tra màn 01, kiểm tra thu diễn xuất khi điều khiển/luồn khe, và đường giải cắt–nhập–luồn–thoát đủ 32/32 hạt. Log/XML tại `Artifacts/VenomDance/final.*`; sau khi thêm chặn đoạn ray cực ngắn, chạy lại 4/4 kiểm tra animation liên quan qua tại `Artifacts/VenomDance/guard.*`. Có kiểm tra vị trí/vận tốc hạt không đổi khi dựng hình, pause/reset, sợi bám thật và đỉnh sợi nằm trong hộp khi đứng sát kính. Capture gồm 100 frame yên và 34 frame chuyển động, mỗi frame cách nhau 0,1 giây mô phỏng.

[GIF và ảnh cận cảnh](Images/VenomLife/README.md). Các capture phục vụ xem diễn xuất, không phải số đo FPS trên thiết bị.
