# COghe — Hồ sơ V02 — Ghép đường lên cao

Theo [LEVEL_TEMPLATE](../../LEVEL_TEMPLATE.md), [quy tắc level](../../../../COGHE_LEVEL_DESIGN_RULES.md), [art C](../../../../ArtDirection/COghe/STYLE_RULES.md), [kế hoạch](../../../../ArtDirection/COghe/VerticalMechanisms/PLAN.md).

## 1. Định danh, phạm vi và trạng thái

- `study.vertical.assemble-route`, v0.1, Codex, 29/09/2026. **Nháp** độc lập, không phải màn 02 hoặc một màn thay thế đã chốt.
- [Concept](../../../../ArtDirection/COghe/VerticalMechanisms/03-build-route.png). Chưa có scene, prefab mới, build hoặc kiểm chứng. Dùng V2 camera-only và luật Origin hiện hành.
- Thiết kế trên giấy hoàn thành ở mức quan hệ không gian; prototype/hoàn thiện/nghiệm thu còn mở. Concept không phải bản vẽ kích thước.

## 2. Mục tiêu và tiến trình

Sau khi biết leo, trơn, chạm tay kéo và liên kết màu: người chơi ghép **đường bám liên tục** qua một dải không bám. Hai cơ quan A/B thay vị trí hai tấm bám; không dùng cửa che cuối. Khoảnh khắc khám phá là đường từ chân hộp lên cao hình thành sau khi dịch tấm thứ hai.

Màn kết hợp nhẹ–vừa, chưa là Boss. Không nâng chỉ số/đồ mua, không cắt, không phải hold hai nơi đồng thời. Không yêu cầu căn nhảy/rơi, không điều khiển chính xác từng hạt. Chưa gắn vào vị trí campaign hoặc thêm thưởng mới.

## 3. Phác thảo, hình học, camera, thao tác

Tọa độ dưới đây là **tỷ lệ bố cục** nhìn từ mặt trước vào vách sau, không phải kích thước thi công. X từ trái→phải, Y từ sàn→nóc.

| Thành phần | Vị trí/quan hệ đề xuất |
|---|---|
| Spawn | Sàn dưới trái, X khoảng .15 |
| Vành trơn | Y .30–.75, chạy kín quanh cả bốn vách, liên tục qua bốn góc |
| Bệ giữa | X khoảng .48, Y .52; đảo bám độc lập trong vùng trơn |
| Tấm A | Một tấm nghiêng rộng, khi đỗ nối vùng thường dưới Y .30 đến bệ Y .52; chạy ray từ vị trí thu xuống dưới lên vị trí nối |
| Tay A | Trên vách thường dưới dải trơn, tiếp cận từ sàn |
| Tấm B | Tấm bám đứng cao từ Y .50 đến .80, chạy ray ngang. Đỗ đúng thì chân tấm chồng vùng bệ giữa và đầu tấm vượt mép trên .75; sai thì nằm lệch ngang không nối bệ |
| Tay B | Trên bệ giữa, có chỗ chân cố định; không nằm trên tấm B đang chạy |
| Lỗ cuối | Vùng kính thường phía trên dải trơn, trên phải; không có shutter |

B có ba vị trí đỗ rõ, trong đó một vị trí nối liên tục. Chọn sai rồi đảo/đổi vị trí được. Vị trí đúng không dùng hiệu ứng giấu kín; người chơi nhìn mép tấm và mép bệ để suy ra.

Cần đo D và độ với tối đa của cơ thể thật trước khi chốt: tấm/bệ đủ cho toàn bộ mô và quay đầu; khe sai lớn hơn độ với chủ động; khe đúng không tạo mép mắc. Vành trơn phải đủ cao ngăn bò xuyên bằng việc thân dưới còn bám; ray, khung gá và cạnh kính không được tạo đường vòng ngoài chủ đích. Ray rộng có thể bám phải được bọc vật liệu trơn thấy được; chi tiết chỉ trang trí phải đủ nhỏ không hứa hẹn đường đi.

Chỉ xoay camera. Góc đầu 3/4 nhìn rõ A, bệ B và đích; ảnh mô tả có mặt đứng phụ để tác giả đọc đường nối. Người chơi dùng chạm tay A/B; COghe tự tới điểm làm việc rồi vận hành bằng lực hữu hạn. Chạm mặt còn lại là lệnh bò, vùng trơn vẫn nhận chạm và không cho lực bám. Không tự nhảy tới tấm hoặc tự làm B khi người chơi chưa ra lệnh.

## 4. Lời giải, kết thúc và phục hồi

| Bước | Lệnh | Thay đổi thật và tín hiệu | Sửa sai |
|---|---|---|---|
| 1 | Chạm tay A | Leo vách dưới, kéo A nâng tấm nghiêng tới chốt; hai đầu nối chồng mặt bám | Kéo lại nếu chưa xong; hủy không thả tấm rơi tự do |
| 2 | Chạm bệ giữa/tay B | Bò theo tấm A đã đỗ đến đảo bám trong vành trơn | Rơi thì về sàn, A vẫn giữ chốt, leo lại |
| 3 | Chạm tay B để chọn vị trí ray | Tấm đứng B dịch ngang; ở đúng vị trí chân chạm bệ và đầu chạm vùng trên | Sai vị trí không phạt; chuyển tới vị trí tiếp theo hoặc đảo hành trình |
| 4 | Chạm vùng thường trên cao gần lỗ | Bò bệ→tấm đứng B→kính trên→lỗ | Nếu chưa nối, không teleport qua khe; cho tiếp tục điều chỉnh B |

Tấm A/B đỗ có chốt cơ khí. Không yêu cầu đứng trên tấm đang chạy; nếu người chơi có thể bám và đi cùng tấm bằng vật lý thật thì coi là lời giải thay thế hợp lệ, cần kiểm tra không xuyên/kẹp. Sàn an toàn hứng rơi, không mất mô, không reset cả màn vì sai đỗ. Khe sai không được nhỏ đến mức hạt tự xuyên/bridge bằng biến dạng quá mức.

Win khi toàn bộ 32 hạt của bản thể hợp nhất ra FinalExit. Không có Transfer/dao trong bài này; luật thoát trước tụ vẫn giữ chung. Không thêm cờ “đã dùng A và B” để ép thắng: nếu tìm đường tắt hợp vật lý, sửa hình học hoặc chấp nhận đường đó theo mục tiêu. Không lấy tranh làm bằng chứng vành đã kín.

Retry xóa tác vụ/lực, đưa hai tấm về đầu đỗ, reset thắng/thua; pause dừng mô phỏng. Tấm chạy vào mô cần dừng/giới hạn lực, không nghiền/chia mô giả. Chạm B sau rơi phải được từ chối tiếp cận hoặc tìm đường thật qua A, không điều khiển từ xa bằng ý định đơn thuần.

## 5. Cơ quan, trạng thái và kiến trúc

| ID | Chức năng/khả năng dùng lại | Phụ thuộc, reset |
|---|---|---|
| A.handle / A.panel | TapRail, input rail và output rail; tấm nghiêng theo ray nâng có chốt | Không phụ thuộc cờ B. Reset thu xuống |
| B.handle / B.panel | TapRail với Stops + rail ngang; điểm chân là bệ giữa cố định | Tiếp cận bằng hình học thật, không cờ thứ tự. Reset lệch phải |
| slippery.belt | Các mặt trơn hiện có trên cả bốn vách và góc | Luôn là vật liệu trơn, không đổi theo đèn |
| exit | FinalExit | Không cần khóa logic theo A/B |

Đường nối cập nhật theo vị trí thật của tấm. Không chỉ đánh dấu route mở khi người chơi bấm. Navigation động phải kiểm tra va chạm và tiếp xúc ở trạng thái trung gian, không cache một đường giả qua khe đang mở. Tách dữ liệu circuit màu khỏi trạng thái gameplay; không runtime đọc lời giải mẫu. Nhu cầu mở rộng: kiểm chứng mặt bám chuyển động và các liên kết graph giữa cụm động/tĩnh.

## 6. Hình ảnh và phản hồi

A xanh vòng tròn, B coral hình thoi; tấm ngà, vùng trơn lavender có biên/vân, lỗ mint. Bệ giữa nhỏ nhưng chân sinh vật dễ nhìn; không thêm sàn kín ở giữa. Quai thật lớn, vị trí đỗ đánh dấu cùng mã, chốt vào mới có âm/đèn. COghe co kéo/bò bám theo contact; không phát animation đi tới đích khi còn khe. Nét đứt vị trí trong concept là chú thích nghiên cứu; trong game dùng dấu chân đỗ vừa phải, không mũi tên lời giải.

## 7. Độ khó và ngân sách

Hai lần suy luận: nâng A để tiếp cận B, rồi chọn vị trí B nối cả hai đầu. Một bản thể; hai thao tác nối tiếp; không yêu cầu timing. Thử nghiệm so với V01 về hiểu màu, hình học và khả năng sửa sai, chưa ấn định thời gian giải.

Chi phí dự kiến hai cụm ray, các mặt bám động và một vành vật liệu chia vùng. Giảm trang trí, không giảm physics. Không biết trần số surface/renderer trước đo; ưu tiên đo graph lúc tấm chạy và người chơi chạm liên tiếp. Cả bốn dải trơn có thể dùng shader/material chung, tránh nhiều lớp kính phủ chồng.

## 8. Chơi thử và hồi quy

**Chưa thực chạy**. Ca bắt buộc: toàn bộ thứ tự đỗ A/B; thử lỗ trước, bấm B từ sàn; leo quanh bốn góc, ray, mặt dưới bệ, trần; bám tấm đang chạy; rơi và quay lại; thân bắc qua khe sai; hủy/đổi lệnh/pause/retry; spam input và orbit; 32 hạt ra đúng lỗ. Chơi tay portrait + người chưa biết giải, không dùng dịch chuyển mô để chứng minh đường đi. Hồi quy TapRail, navigation, moving surfaces, slippery và thoát.

## 9. Hiệu năng thiết bị

Chưa có APK/build/SoC/số đo. Ghi baseline và phiên bản cùng OPPO; đo spike khi đổi route, hai tấm chuyển, contact tăng, orbit/pinch và chơi kéo dài. Lưu raw frame time, CPU/GPU/GC, BuildGraph. Mục tiêu 60 FPS là mục tiêu thử, không kết luận đạt. Ảnh concept không có thông tin hiệu năng.

## 10. Tích hợp và nghiệm thu

Đề xuất thử nghiệm riêng, chưa thay scene hoặc save. Sau khi chứng minh khả giải và chi phí mới chọn vị trí campaign. Không cần migration hiện tại. Trạng thái: **nháp có mô tả đường giải; kích thước, sức bám, ray truyền động, camera và khả giải chưa kiểm chứng**. Bước tiếp theo là greybox đo cơ thể và dựng vành trơn kín trước, rồi hai tấm và hình ảnh.
