# COghe V2 — 20 màn tiếp theo (11–30)

Bản thiết kế và minh họa trước khi dựng scene, theo yêu cầu Mrk ngày 28/09/2026. **Trạng thái: Nháp thiết kế; chưa phải 20 màn chơi được.** Tiếp nối V2 đã giao tại `0ab0931b6d9e2ddf967dd35eade6d606bf25e427`, không dùng số màn 11–30 của campaign cũ.

Hướng trải nghiệm: người chơi khám phá một cỗ máy nhỏ, hiểu quan hệ rồi tự chọn thứ tự. Giữ chạm giao việc, kéo ngang quan sát, pinch và Toàn cảnh. Hộp/trọng lực đứng yên. Độ khó tăng bằng trạng thái, đường đi và tối đa hai vai trò; không bấm nhanh, nghiêng máy, timer hoặc thêm sức mạnh.

**Nhịp học:** 11–13 đường lộ ra và đi vòng; 14–16 truyền động/chốt/thứ tự; 17–19 ống và đảo trạng thái; 20 Boss một cơ thể; 21 nghỉ nhịp; 22–25 tách/chọn/giữ/gọi về; 26–29 phối hợp; 30 Boss đoàn tụ. Mỗi màn giới thiệu chỉ thêm một ý mới. Boss 20/30 không có lời giải hoặc ngón tay chỉ bước tiếp trong game.

Các hình AI mô tả ý đồ hình ảnh/cơ quan, không phải bản vẽ kỹ thuật và không chứng minh khả giải. Hồ sơ từng màn là nguồn luật, hình cần sửa theo greybox nếu đường đi/che khuất không đúng. Chưa thay code, scene, build hoặc trạng thái nghiệm thu. Phản hồi điều khiển tốt của Mrk cho phép đi tiếp phần thiết kế; chưa thay thế số đo điện thoại hoặc thử người mới.

| Màn | Tên | Điểm khám phá | Vai trò |
| --- | --- | --- | --- |
| [11](LEVEL_11.md) | Bến bí mật | Cầu dời đi làm lộ lối cũ; quan sát hậu quả ngoài đích đang nhìn. | Nghỉ nhịp sau Boss |
| [12](LEVEL_12.md) | Dưới chiếc nắp | Một mặt tưởng là sàn kín thực ra che đường xuống rộng. | Giới thiệu cách quan sát đường dưới nắp |
| [13](LEVEL_13.md) | Đi vòng trở lại | Lối ra gần nơi xuất phát nhưng chỉ tới được khi đi vòng sau vách. | Kết hợp nắp và cầu |
| [14](LEVEL_14.md) | Răng truyền lực | Một thao tác gần làm cửa xa chuyển động qua bộ truyền rõ ràng. | Giới thiệu truyền động nhìn thấy |
| [15](LEVEL_15.md) | Chuyền bộ truyền | Dùng lại cùng bộ truyền cho cửa sau mà cửa trước vẫn giữ kết quả nhờ chốt thật. | Kết hợp trạng thái và chốt |
| [16](LEVEL_16.md) | Hai nhịp cầu | Dời đoạn thứ nhất vừa tạo đường vừa nhường ray cho đoạn thứ hai. | Bài thứ tự hình học |
| [17](LEVEL_17.md) | Ống vòng lưng | Đi vào phía trước, xuất hiện phía sau vách ở cao độ khác; nhìn thấy đường đi xuyên ống. | Giới thiệu transfer tube |
| [18](LEVEL_18.md) | Rẽ nhánh | Phải thăm nhánh phụ trước để mở đường chính, rồi quay lại dùng chính ngã rẽ. | Kết hợp chọn nhánh và đi-về |
| [19](LEVEL_19.md) | Mở rồi đóng | Đóng lại lối vừa dùng mới mở được lối ra, trong khi kết quả chốt vẫn giữ. | Ôn đảo trạng thái trước Boss |
| [20](LEVEL_20.md) | Boss: Cỗ máy vòng | Tự nhận ra chuỗi đường mở bằng bộ cơ quan đã học, vòng cuối trở về gần khởi đầu. | Boss một cơ thể |
| [21](LEVEL_21.md) | Lối tắt quen | Một chuyển động tạo hai kết quả có liên kết nhìn thấy: nối cầu và mở đường vòng. | Nghỉ nhịp sau Boss |
| [22](LEVEL_22.md) | Tách rồi gặp | Hai phần vẫn là một cơ thể; đến gần và không có vật cản thì tự hợp. | Giới thiệu cắt/hợp thể an toàn |
| [23](LEVEL_23.md) | Hai việc riêng | Chọn phần khác không hủy việc phần cũ; hai làn có thể xử lý theo thứ tự bất kỳ. | Luyện chọn phần và lệnh độc lập |
| [24](LEVEL_24.md) | Bạn đứng giữ | Một phần đứng giữ để phần kia đi qua; nhiệm vụ giữ vẫn chạy khi đổi chọn. | Giới thiệu áp lực duy trì |
| [25](LEVEL_25.md) | Gọi bạn về | Gài chốt ở bên kia để người giữ cũng sang được. | Luyện giải phóng vai trò giữ |
| [26](LEVEL_26.md) | Giữ để vận hành | Bạn giữ để rút khóa, tôi thực hiện một hành trình; cầu tự đứng vững khi đã khớp. | Kết hợp duy trì và hữu hạn |
| [27](LEVEL_27.md) | Hai khoang | Ở xa nhau nhưng tác động cùng một cơ quan qua liên kết thấy được. | Vận dụng phối hợp khác vị trí |
| [28](LEVEL_28.md) | Đổi ý vẫn được | Sai nhánh không mất cửa đã mở; đưa bộ truyền về và thử nhánh còn lại. | Bài phục hồi có chủ đích |
| [29](LEVEL_29.md) | Nhường đường | Người đi trước xây đường quay về cho người giữ. | Luyện tổ hợp trước Boss |
| [30](LEVEL_30.md) | Boss: Cùng trở về | Cỗ máy chỉ hoàn tất khi cả hai phần đều có đường về; kết thúc bằng một cơ thể. | Boss hai vai trò |

**Luật chung cho cả 20 màn**

- Tay nắm amber, ray hai nấc hoặc bộ chọn có nấc rõ; mỗi chạm chỉ giao một tác vụ. COghe tự tới, bám và tác động hữu hạn. Không di chuyển tức thời cơ quan/mô hoặc mở theo animation.
- Mọi kết quả chốt đọc vị trí/vận tốc thực. Chốt nhìn thấy và có trạng thái phân biệt với bàn đạp phải giữ; mất lực trước chốt có đường phục hồi. Không khóa bằng lịch sử đã chạm/đã cắt.
- Chỉ dao mới cắt. Không ép chia đều, tạo thêm mô, bảo vệ nhiệm vụ khỏi nhập hoặc cooldown hợp thể. Hai phần đủ gần, không vật cản thì tự tụ. Bố cục tạo khoảng cách/vách thật khi cần phối hợp.
- Dùng một cơ thể ở 11–21, giới thiệu tách ở 22; bài tác giả từ 22–30 dùng hai vai trò. Cắt ngoài ý đồ vẫn phải bảo toàn mô và có hồi phục; không âm thầm xóa phần thứ ba.
- Màn 22/23 cho phép lời giải một cơ thể nếu vật lý cho phép; bài học ghi theo hành động thật. Từ 24/26 kiểm tra nhu cầu phối hợp do hình học/tải, không thêm biến bắt buộc từng cắt.
- Hợp thể trong hộp trước lần mô đầu tiên đi qua FinalExit, rồi đủ 32 hạt mới thắng. Transfer không tính thoát. Ra sớm hiện đúng thông báo cũ. Nhà đã mở/tiến trình cũ được giữ; Boss mới không cấp nâng chỉ số.
- Tay nắm phải có vùng đứng cố định, đường tới/nhả; không đặt trên một vật đang buộc phải kéo khi không có chỗ bám. Ống hai chiều và các nhánh cụt phải quay đầu được.
- Kính ngoài phía camera là cutaway; vách/nắp trong vẫn che/chặn chọn. Mặt bám vật lý của vỏ không bị xóa. Chọn chip phần khi cần, không chạm xuyên tường để thao tác.

**Độ khó và kiểm tra rủi ro**

Số quyết định trong từng hồ sơ là giả thuyết thiết kế, không phải số lần chạm hay kết quả playtest. Dự kiến 11/21 là nhịp nghỉ; màn thường khoảng 1–2 phút, Boss khoảng 2–4 phút để quan sát, không có đồng hồ giới hạn. Không tăng nhiều trục cùng lúc. Cắt lệch phải có biên lực/tải đủ rộng và đổi vai, không đòi 16/16. Không hứa mọi mảnh cực nhỏ vận hành được mọi vật.

Greybox ưu tiên **15, 18, 25, 30**: lần lượt chứng minh chuyển nguồn giữ chốt, đổi nhánh ống an toàn, giải phóng người giữ và chuỗi hai vai trò có đường về. Nếu một họ cơ quan không chạy được bằng touch V2 và mô phỏng thật, chỉnh bố cục/thiết kế trước khi nhân ra nhiều màn; ghi thay đổi, không đổi test để hợp thức hóa.

Giữ 32 hạt/120 Hz và kit art V2: vách xanh dày bo nhẹ, sàn sứ kem có ron, tay nắm amber, một đèn có bóng, phản chiếu dựng sẵn, mesh trang trí gộp theo vật liệu. Ngân sách renderer/khớp/memory chưa đo; không tự đưa trần không có cơ sở. Tránh nhiều lớp kính/ống chồng nhau. Android 60 FPS vẫn là mục tiêu; kiểm tra phiên 15–20 phút trên máy mục tiêu trước kết luận.

**Phần kỹ thuật đã có và phần phải làm**

| Nền trong source hiện tại | Giới hạn cần xác nhận |
| --- | --- |
| `COgheTapRail`, `COgheViewMechanism`, `COgheDockedBridgeDeck` | Đã dùng trong 10 V2; bố cục mới cần lời giải/input/clearance riêng. |
| `COgheGearTrain` có `LatchOutput`, `PowerRail`, `InputClutch` | Có trong legacy, chưa xác nhận toàn bộ xe chuyển hai output dùng V2. Bộ chọn/chốt có thể cần component mới. |
| `COgheTapPad` có giữ/nhả bằng chạm; sensor đo mô | Kiểm tra chọn phần, ownership khi cắt/tụ, nguồn tạm và chống kẹp trong V2. |
| `COgheTubeNetwork` và cơ quan cắt/tụ legacy | Kiểm tra transfer trong hộp đứng yên, V2 picking và bộ chuyển nhánh mới; không lấy test cũ làm chứng nhận mới. |

**Trình tự dựng tiếp**

1. Gửi đủ năm bảng hình và hai mươi hồ sơ này trước khi dựng scene theo yêu cầu.
2. Greybox bốn rủi ro trên bằng API điều khiển thật và chạm màn hình; sửa thiết kế có ghi lý do nếu cần.
3. Dựng 11–20 rồi 21–30, dùng ID đề xuất `coghe.view.v2.11`…`.30`; bổ sung catalog/save/build, giữ nguyên ID 01–10 và lịch sử cũ. Mã ID mới chỉ thành chính thức khi tích hợp.
4. Full EditMode + PlayMode, lời giải 30 màn qua touch, negative/failure/retry/cắt lệch/đổi phần/ống/che khuất; ảnh Unity thực portrait và replay thời gian thực gắn commit/hash. Build Mac; Android theo phạm vi build đã được yêu cầu khi triển khai.
5. Chơi tay trên điện thoại, thử người chưa biết lời giải, đo nhiệt/FPS; ghi riêng phần chưa có bằng chứng. Không lấy ảnh AI hoặc replay tác giả thay cho thử người mới.

Các bước này không đặt thêm vòng xin phép cho việc đã được giao. Đây là đợt bàn giao hình và thiết kế trước triển khai.

**Nguồn và tệp**

- Yêu cầu Buzz: event `51283fcc8040cf69d93691b6f87597306cfa7bdf9927ea6f1588a62bc5b8d46a`, channel `1c3a4604-73ef-4fe7-b2fd-ac9f87e89f30`.
- [Quy tắc level](../../../../COGHE_LEVEL_DESIGN_RULES.md), [mẫu hồ sơ dùng cho 20 màn](../../../../LevelDesign/COghe/LEVEL_TEMPLATE.md), [art V2](../../../../ArtDirection/COghe/STYLE_RULES.md), [hợp đồng V2](../../../../COGHE_VIEW_V2.md).
- Tuyến đề xuất 28/09, mục 6 (workspace `PLANS/COGHE_VIEW_ONLY_REDESIGN_2026_09_28.md`). Chi tiết bất ngờ và bố cục ở tài liệu này là đề xuất mới theo yêu cầu hiện tại, chưa được coi là đã duyệt riêng.
- Bộ hình đầu làm tham chiếu (workspace `OUTBOX/COGHE_FIRST_10_LEVEL_CONCEPTS_2026_09_28/GALLERY.md`); [prompt đầy đủ](PROMPTS.json), tạo bằng built-in `image_gen.imagegen`.
- [Gallery](GALLERY.md); [dữ liệu 20 màn](LEVELS.json). Mỗi hồ sơ có đủ mười phần của LEVEL_TEMPLATE, các mục test/hiệu năng chưa làm được ghi đúng trạng thái.
