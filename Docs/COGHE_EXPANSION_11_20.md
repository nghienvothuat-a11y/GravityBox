# COghe — chương 11–20

Nội dung dựa trên các phác thảo trong `Docs/LevelDesign/COghe/Level11` đến `Level20`.
Các scene nằm trong `Assets/_Game/Venom/Campaign/VenomOrigin11.unity`…`VenomOrigin20.unity`.
Giữ nguyên chương 01–10, save `venom.origin.v2`, khối lượng mô và 32 hạt vật lý.

## Màn chơi và điều khiển

| Màn | Cơ quan chính | Xoay hộp |
| --- | --- | --- |
| 11 · Kê cao lên | Đẩy/kéo thùng cao làm bậc vượt dải trơn | Khóa |
| 12 · Trượt rồi bay | Leo bệ, trượt máng, bay và bám vùng đón | Khóa |
| 13 · Mở đường | Cần gạt → cửa phòng → nút bên trong → nắp ống uốn | Khóa |
| 14 · Đổi chiều | Cầu bản lề và cửa trượt theo hai hướng trọng lực | Có |
| 15 · Tìm lối ra | Mê cung ống có vòng nối, ngã ba và đầu cụt | Có |
| 16 · Ghép cầu | Đẩy/kéo ba mảnh cầu trên ray, leo qua dải trơn tới lỗ | Khóa |
| 17 · Nối bánh răng | Dịch hai ổ trượt để nối bộ truyền và nâng cửa | Khóa |
| 18 · Ghép đường | Ba thanh cầu trượt theo ba trục, tự khớp dưới trọng lực | Có |
| 19 · Cùng nhau | Tách cơ thể, giữ A, kéo B, mở lối hợp thể giữa hai khoang | Khóa |
| 20 · BOSS · TAM HỢP | Ba khoang, hai dao, hai ống, bánh răng và tời liên động | Khóa |

Menu chọn chương **01–10 / 11–20** ở trên cùng. Phím **1…9, 0** chọn chương đầu;
**Shift + 1…9, 0** chọn màn 11…20. Chạm phần cơ thể hoặc nút Phần để đổi chủ thể.
Chạm vật có thể thao tác rồi chạm đích để đẩy/kéo; sau ba giây không ra lệnh sẽ buông.
Trong ống, chạm nhánh nối tại ngã ba; chạm lại nhánh vừa đi để quay khỏi đầu cụt.

Boss không hiển thị lời giải. Các phần không được tự đi theo nhau: phần đang giữ
nút tiếp tục giữ khi người chơi đổi phần điều khiển. Cơ thể chỉ tách qua dao và tự
tụ khi đủ gần và không có vách ngăn ở giữa. Phải hợp thể trước khi bất kỳ phần nào thoát
qua lỗ cuối. Các ống nối khoang không tính là lỗ thắng.

## Kiến trúc triển khai

- `VenomCampaignExpansion`: khám phá, reset và cập nhật các `COgheMechanism` của
  scene; tổng hợp điều kiện cửa, chọn phần, trạng thái chảy và cản hợp thể qua vách.
- `COgheRailSlider`: Rigidbody/ConfigurableJoint, hành trình thật, trọng lực thế
  giới, lực cản và chốt đầu hành trình có thể kéo ngược để nhả.
- `COgheTissueSensor`: đo tải mô trên vùng tiếp xúc; mặt nút và đèn phản ánh tải thật.
- `COgheGuillotine`: cảm biến mô → cảnh báo một giây → rơi → cắt liên kết thực tế
  theo vị trí; xung lực sau chém chỉ tác động nhóm vừa bị cắt.
- `COgheGearTrain` và `COgheCooperativeWinch`: kiểm tra vị trí ăn khớp, truyền chuyển
  động, motor có lực hữu hạn, khóa tải và chốt cửa khi đã mở đủ hành trình.
- `COgheTubeNetwork`: đồ thị ống rõ nhánh nối; điều khiển mô bằng lực dọc lòng ống,
  dừng để chọn nhánh, quay lại đầu bịt, ống hai chiều và tâm tuyến cao su biến dạng.
  Trạng thái chảy thuộc từng nhóm mô, không vô hiệu hóa phần đang giữ nút ở nơi khác.
  Lỗ nối khoét theo đường cong thật; toàn bộ mô phải vào ngã ba trước khi đổi nhánh.
  `AllowsExitAssist` chặn lực hút lỗ cuối khi đường kéo thẳng còn xuyên thành ống;
  mỗi hạt chỉ chuyển sang lực hỗ trợ thoát khi đoạn cuối đã thông thoáng.
- `COgheSlideLaunchMonitor`: chỉ quan sát trượt, bay và bám; không mở cửa bằng một
  điều kiện lịch sử vô hình. Đường bay do hình máng, tiếp xúc và trọng lực tạo ra.
- `COgheLatchedAccessSequence`, `COgheGravityBridgeAssembly`: cơ quan cửa có chốt
  của màn 13 và phép đo cầu/cửa chuyển động thụ động của màn 14.
- `COgheExpansionArtBuilder`: áp Day Lab, dùng chung vật liệu và cubemap, mesh riêng
  từng scene; trang trí không tạo va chạm hoặc quyết định kết quả giải đố.

Mô phỏng này vẫn là prototype mô mềm 32 hạt, không phải solver chất lỏng bảo toàn
thể tích. Bộ truyền dùng ràng buộc bánh răng lý tưởng; ống mềm dùng tâm tuyến có
ràng buộc, không phải mô phỏng đầy đủ vật liệu cao su. Cần đo hiệu năng trên điện
thoại trước khi đưa ra cam kết FPS.

## Animation và phản hồi

Dùng cùng bộ animation procedural của COghe: xúc tu bám theo tiếp xúc thật khi
leo, thân kéo dài khi kéo vật, dồn thân khi đẩy, mất bám và chùng xuống theo trọng
lực khi trượt/rơi. Máng trượt 12 không dùng animation để phóng cơ thể. Trong ống,
mô chảy theo lòng ống và co gọn; nhóm đang ở ngoài vẫn giữ animation tiếp xúc riêng.
Ngã ba dừng để nhận lệnh chọn nhánh. Nút, cửa, bánh răng và đèn đọc trạng thái cơ
quan thực tế. Sau thắng giữ ba điệu nhảy ngẫu nhiên và camera cận cảnh hiện có.

## Checklist chơi thử

- Thử chỉ sai đích, đổi hướng, bỏ vật sau ba giây và reset giữa lúc cơ quan chạy.
- Màn 12: trượt khỏi mép máng, có khoảng bay thật rồi bám được vùng đón.
- Màn 14/18: xoay hộp làm cơ quan chuyển động do trọng lực; đứng sai vị trí có thể
  rơi và phải tìm đường quay lại. Không dùng một góc xoay như mã mở cửa vô hình.
- Màn 15: thử cả ba ngõ cụt và quay về ngã ba, đi hai chiều vòng nối; chỉ ống cuối
  dẫn tới lỗ thắng. Chạm đường ống tại ngã ba phải chọn được nhánh đang thấy.
- Màn 16: bám tay nắm thấp để đẩy/kéo ba mảnh cầu, sau đó leo từ bệ trái qua
  đường đã ghép tới lỗ bên phải. [Thiết kế thay thế](LevelDesign/COghe/Level16/README.md).

Boss 20 không hiện checklist hay lời giải trong game. Bộ test giải màn đi qua các
lệnh điều khiển có sẵn và chờ mô phỏng; người chơi vẫn phải khám phá thứ tự dùng
khối lượng, chia cơ thể và phối hợp giữa ba khoang.

## Dựng lại và kiểm chứng

Unity **6000.3.19f1**. Menu **Gravity Box → COghe → Generate Expansion 11–20** chỉ
dựng chương mới và đăng ký đủ 20 scene. Không ghi đè scene 01–10. Các định nghĩa
camera, hình học, trạng thái cơ quan và quyền xoay được tác giả đặt trong các partial
builder; runtime không đọc lời giải từ tests.

Các suite `COgheEarlyExpansionTests`, `COghePipeExpansionTests`,
`COgheMechanismExpansionTests` kiểm tra đường giải bằng lệnh điều khiển và mô phỏng
vật lý. `COgheExpansionIntegrationTests` kiểm tra ID, trạng thái ban đầu, reset và
chụp ảnh cả chương. Các fixture lực/cơ quan riêng không thay thế full solution tests.

Chạy `bash Tools/verify-coghe-expansion.sh` để kiểm tra cả chương mới và hồi quy
01–10. Thêm `--generate` nếu cần dựng lại scene 11–20 trước khi kiểm tra.

Kết quả chạy và ảnh thật nằm trong `Artifacts/COgheExpansion`; những tệp có tên
`first`, `second`, `third` là bằng chứng các lượt tìm lỗi, không phải chứng nhận đạt.
Build macOS bằng `bash Tools/build-venom.sh`, đầu ra `Builds/Venom/macOS/Venom.app`.
APK/iOS cũ không được coi là đã cập nhật chỉ vì có scene mới trong repository.

## Kết quả kiểm chứng — 17/09/2026

Lượt trước thay đổi tốc độ +30% và cập nhật màn 08/12: **68/68 đạt, 0 lỗi**
(Unity PlayMode, 260,4 giây). Bảng dưới là kết quả lịch sử; kết quả mới nhất nằm cuối tài liệu. Màn 16 trong bảng này đã bị thay bằng thiết kế Ghép cầu; các kiểm tra ống cũ không chứng nhận màn 16 mới.

| Nhóm | Đạt | Phạm vi |
| --- | ---: | --- |
| Origin 01–10 và đầu vào | 45/45 | Hồi quy di chuyển, trơn, ống, dao, hợp thể, thắng/thua, Collection; chém lần hai không hất phần giữ nút khác; kéo nhanh, cấm xoay, chạm, pause/reset/disable |
| Màn 11–14 | 4/4 | Đường giải hoàn chỉnh bằng điều khiển và tiếp xúc vật lý, đủ mô qua lỗ cuối |
| Màn 15–16 | 7/7 | Ba ngõ cụt và quay lại, vòng nối, chạm chọn nhánh, sáu ống mềm, thoát đủ 32 hạt |
| Cơ quan 17–20 | 11/11 | Tải, truyền động, ghép cầu, phối hợp, chạm cơ quan và đường giải hoàn chỉnh tới thắng |
| Tích hợp | 1/1 | Đủ 20 scene/ID, không thiếu script; 10 màn mới đứng yên, reset, quyền xoay và render portrait |

Ảnh Unity thật của lượt kiểm tra nằm ở `Artifacts/COgheExpansion/Frames` và `Pipes`.
Các phép thử giải màn không đặt lại vị trí cơ thể hoặc mở cửa thay cho người chơi.
Fixture riêng cho dao/tải có sắp đặt trạng thái để kiểm tra từng luật và được ghi
rõ trong test. Kết quả này xác nhận các đường giải đã kiểm tra, không thay cho
playtest độ khó với người chơi mới hoặc phép đo hiệu năng trên điện thoại.

Kiểm tra trực tiếp trên bản Mac: đổi tab 11–20, chọn màn 11, chạm thùng và zoom;
chọn nhánh ống màn 15 tới nút A; kéo nhanh màn 16 làm quả cầu đổi hướng;
màn 20 hiện ba khoang, khóa xoay và không hiện lời giải. Kiểm tra chọn nóc
01–03 dùng fixture xoay vỏ hộp và chạm tọa độ màn hình; không phải phép thử
hoàn thành màn bằng cách leo nóc.

Hồi quy màn 07 phát hiện thùng xoay chéo đã chạm kính nhưng điều kiện trèo còn
đo bằng tâm thùng và nửa chiều rộng cố định. Điều kiện nay đo góc xa nhất của mặt trên theo hướng tới kính
với khoảng với tới 15 mm như trước; vẫn yêu cầu căn ngang và thùng gần đứng yên.
Hai đường thử giải màn kiểm tra thùng ban đầu thẳng và lệch ±12°; không dịch
chuyển vật thể thay cho người chơi trong quá trình giải.

Mọi nóc hộp hiện nhận chạm từ ngoài và trong, bao gồm các màn khóa xoay.
Vật liệu trơn không khóa đầu vào: animation cố bò được dùng trên cả mặt phẳng
và mặt cầu khi cơ thể không có đủ lực bám. Kiểm tra đối chiếu có/không ra lệnh
trên mặt trơn xác nhận không thêm lực, không đổi quỹ đạo vật lý của 32 hạt.

## Màn 12 — thao tác và góc nhìn, 17/09/2026

- Camera cố định nhìn từ phía đối diện: pitch 16°, yaw −24°, ViewRadius 0,43.
  Thấy mặt bám của lối leo, bệ đầu máng, đoạn tung người và vùng đón trên vách.
- Lối leo và bệ màu hổ phách; lòng máng tím satin; hai thành máng cyan trong.
  Mesh hiển thị máng lấy đúng hình cong của collider liền mạch, tránh các tấm
  trang trí chồng lên nhau che cơ thể. Vật lý máng, trọng lực và lực bám giữ nguyên.
- Sửa lỗi máng có collider liền mạch nhưng các patch điều hướng không có collider
  hoạt động, khiến chạm máng bị bỏ qua. `COgheSurfacePickProxy` ánh xạ tam giác trúng
  vào đoạn máng tương ứng; không bật các collider rời gây gờ va chạm.
- Chạm thành máng được hiểu là đi vào lòng máng tại đoạn đó, căn ngang về giữa
  lòng máng để không ra lệnh bò qua thành. Đây là điểm đích cho bộ vận động hiện có;
  không dịch chuyển cơ thể hoặc thêm lực phóng. Sinh vật vẫn trượt và bay do trọng lực.
- Cách thử: chạm bệ hổ phách trên cao để leo, chạm đầu máng tím để trượt,
  sau khi bám vùng đón thì chạm lỗ thoát. Bấm Làm lại để thử tiếp.
- Có thể dựng lại riêng màn này bằng menu `Gravity Box/COghe/Rebuild Slide Level 12`
  hoặc execute method `GravityBox.Editor.VenomCampaignBuilder.GenerateSlideLevel12`.

### Kết quả mới nhất sau sửa màn 12

Lượt `Artifacts/COgheExpansion/verification.xml`, kết thúc 17/09/2026 05:53:24Z:
**66/69 đạt, 3 chưa đạt**, 191 giây. Màn 12 qua đủ ba điểm chạm ngang: giữa,
−25 mm và +25 mm; kiểm tra qua toàn bộ cơ thể, bám thực vào vách, ra đủ vật chất,
chạm lỗ cuối và khung hình dọc. So sánh dữ liệu trước/sau: collider đang hoạt động,
Rigidbody và pose vật lý không đổi; chỉ thêm ánh xạ nhận chạm cho collider máng.

Các ca cần xử lý tiếp, chưa được sửa trong cập nhật màn 12:
- `FourthLessonPeelsAndFallsWhenClimbingIntoSlipperyBand`: sau khi tuột, đường vòng
  không hoàn thành; sinh vật dừng ở góc sàn.
- `Level14_OppositeGravityDirectionsSeatTheBridgeThenOpenTheRailGate`: không leo
  tới đỉnh dải bám trong đường giải kiểm thử.
- `Level20FullSolutionUsesMassOrderThreeRolesAndMergedEscape`: chưa bám được cơ
  quan G để bắt đầu thao tác trong thời hạn kiểm thử.

Không coi bộ kiểm thử hiện tại là đạt hoàn toàn. Ba lỗi ngoài màn 12 cần tái hiện
và sửa riêng; chưa kết luận nguyên nhân chỉ từ kết quả này.

Bản macOS cập nhật màn 12 build thành công. Đã mở binary mới, kiểm tra khung hình
và chạm bệ hổ phách: sinh vật leo lên được, đầu máng nhận lệnh chạm,
sau đó trượt và bám vào vùng đón trên vách (HUD “Bám được rồi”).
Sau khi chạm lỗ cuối, bản Mac hoàn thành màn và tự chuyển sang 13. Đã chọn lại
màn 12 ở trạng thái ban đầu để người dùng test.

## Màn 13 — làm rõ chuỗi cơ quan, 17/09/2026

Phác thảo được giữ nguyên: tới kéo cần → cửa hộp nhỏ mở → vào trong nhấn nút →
nắp ống mở → chảy qua ống uốn đến lỗ ngoài. Cần A chuyển ra phía trước hộp nhỏ,
sinh vật bắt đầu ở góc xa, cách cần khoảng 44 cm. Không có cơ quan tự mở khi vừa vào màn.

- Bản cũ có bản lề quá mềm: cần tự đổ bởi trọng lực và vô tình chốt cửa mở.
  Spring 0,065 và damper 0,004 giữ cần ở vị trí nghỉ; vẫn cần lực tiếp xúc thật của
  sinh vật để quay qua ngưỡng 31°. Cửa có chốt giữ sau thao tác, không cần đứng giữ cần.
- Camera 34°/22°, ViewRadius 0,46. Hộp nhỏ có khung riêng, kính trong, cửa sứ trên
  ray nâng; cần có chân đế, trục kim loại và tay nắm hổ phách. Nút B có đế sứ,
  mặt nhấn đi theo Rigidbody thật. Nắp ống B có viền và ký hiệu riêng.
- Dây nối và cặp đèn A nối cần với cửa; cặp B nối nút với nắp ống. Đèn chỉ đổi màu
  khi cơ cấu chốt thật, do `COgheAccessSequencePresentation` đọc trạng thái.
- Sửa chiều cao ba vách hộp nhỏ cho khớp sàn −0,30 m và mái −0,055 m; trước đó
  chiều cao bị tính sai dấu, vách nhô lên khỏi mái. Không thêm collider vào trang trí.
- Cách thử: chạm tay nắm A, đợi sinh vật bám, chạm sàn bên trái để kéo; sau cửa nâng,
  chỉ sinh vật tới trước cửa rồi nhấn nút B bên trong; chạm miệng ống đã mở để đi vào.
  Chưa đảm bảo mọi lệnh tắt từ ngoài vách thẳng tới nút B đều tìm được đường; đường
  kiểm thử đi qua cửa bằng một điểm chỉ trung gian.
- Dựng riêng: `GravityBox.Editor.VenomCampaignBuilder.GenerateAccessLevel13`
  hoặc menu `Gravity Box/COghe/Rebuild Access Level 13`.

Kiểm thử: **2/2 đạt** tại `Artifacts/COgheExpansion/level13-after.xml`, kết thúc
17/09/2026 06:16:35Z. Kiểm tra không lệnh trong 15 giây, khoảng cách xuất phát,
đến bám không tự mở, một lệnh kéo đủ trong 2,9 giây, cửa di chuyển thật, nút nhận tải,
chạm vào ống dẫn đủ cơ thể ra ngoài, rồi reset đóng lại các chốt. Đường giải dùng
`TouchPoint` cho cần, hướng kéo, cửa, nút và ống; không teleport hoặc mở khóa hộ.
Hai test này không thay thế kết quả toàn campaign đã ghi phía trên.

Kiểm tra thao tác trên binary macOS: sinh vật đi từ góc xa tới cần, kéo về trái
làm cửa A nâng; qua cửa rồi nhấn B làm nắp ống trượt sang bên. Cặp đèn A/B
đổi theo chốt thật. Nút B dùng màu hổ phách khi nghỉ và mint khi đang chịu tải.
Chạm miệng ống đã mở trên bản Mac đưa sinh vật qua ống, hoàn thành màn 13 và tự
chuyển sang màn 14. Bản giao cuối giữ nguyên vật lý đã kiểm tra này, bổ sung màu
nút B và dùng từ “vật” thay cho “thùng” trong HUD khi đang thao tác với cần.


### Level 13 — quay ra khỏi nắp B (17/09/2026)

Lỗi được tái hiện bằng cách chạm nắp B ngay khi bắt đầu, trước khi kéo A:
sinh vật đi về phía sau hộp nhỏ rồi không quay về cần được. Ba vấn đề liên quan:

- Vách hộp nhỏ có collider dày 8 mm nhưng chỉ đăng ký mặt bám phía trong.
  Bổ sung metadata bám/tìm đường ở mặt ngoài đúng vị trí −8 mm, dùng lại collider
  hiện có; không tạo collider trùng hoặc tăng lực bám toàn game.
- Đường bò vẫn coi lỗ trên vách là thông khi nắp đóng. `NavigationHoleBlocked`
  phản ánh `IsEntryOpen` thực tế, chỉ dùng cho kiểm tra đường đi/vòng mép vách;
  không lấp collider, không tự mở nắp, không tạo lực hút.
- Collider ống chỉ quay vào trong. Phần cơ thể đi ở phía ngoài có thể xuyên vào
  lòng ống và mắc lại. Màn 13 thêm vỏ va chạm ngoài bán kính 39 mm, lòng 35 mm,
  hai vành đầu để thành ống kín. Luồng vào gom phần đuôi trước miệng rồi mới đưa
  qua tiết diện thật, thay vì kéo xiên xuyên thành ống. Cấu hình `SolidExterior`
  hiện chỉ bật cho ống màn 13; các ống khác giữ cấu hình hiện có.

Giữ chuỗi A → cửa → nút B → nắp ống → thoát. Không teleport, cắt bớt cơ thể,
reset hộ người chơi hoặc bỏ collider để né kẹt.

Bổ sung: với ống có vỏ ngoài, hỗ trợ hút tại lỗ thoát chỉ tiếp quản khi phần cơ thể
đã tới đoạn cổ ống cuối. Phần còn trong khúc cong tiếp tục theo trục ống, tránh
lực hút đi tắt qua thành ống. Khẩu độ lòng ống giữ nguyên 35 mm.

Kiểm thử bản sửa: **4/4 đạt**, `Artifacts/COgheExpansion/level13-lid-after.xml`,
kết thúc 2026-09-17 06:45:47Z. Gồm chạm nắp đóng rồi quay lại cần A; tự bò tới
mặt ngoài dưới nắp rồi quay lại cần A; đứng yên không tự mở; đường giải hoàn chỉnh
bằng chạm màn hình và reset. Test tái hiện trước sửa đã thất bại tại
`Artifacts/COgheExpansion/level13-lid-before.xml`. Không chạy lại toàn campaign
trong lượt sửa riêng màn 13 này.

Bản macOS đã build thành công. Kiểm tra thao tác trên binary: chạm nắp B khi
còn đóng, sau đó chỉ sang cần/sàn; sinh vật quay ra được. Đã reset và mở sẵn
màn 13 để test tiếp. Player.log không có exception hoặc assertion.


### Level 15 — chỉ chọn đường ống khi đã vào mê cung (17/09/2026)

`COgheTubeNetwork.CaptureSurfaceCommandsWhileInside` bật riêng ở scene 15 và
builder `BuildPipeMaze`. Khi phần sinh vật được chọn đang ở trong mạng ống,
`TryTouch` nhận toàn bộ chạm trong vùng chơi: nhánh nối hợp lệ vẫn được chọn,
chạm mặt hộp hoặc vị trí không hợp lệ không rơi xuống lệnh bò/thoát ngoài ống
và không tạo dấu đích trên kính. Áp dụng cả lúc đang chảy và lúc chờ ngã rẽ/đầu
cụt. Trước khi vào ống, hoặc sau reset, vẫn chỉ đường trên bề mặt bình thường.
Kéo xoay hộp, camera và các nút HUD không thay đổi.

Kiểm thử: **7/7 bài kiểm tra hệ thống ống đạt** tại
`Artifacts/COgheExpansion/level15-input.xml`, kết thúc 17/09/2026 07:00:49Z.
Bao gồm chạm sàn/nóc bị bỏ qua trong ống, không đổi marker hoặc nhánh đang chọn,
chạm thực tế để đi A–C, reset mở lại điều khiển ngoài ống, quay về từ đầu cụt,
đường giải đủ cơ thể ra ngoài, và hồi quy hệ thống ống của màn 16.

### Level 15 — phân biệt đường ống với kính hộp (17/09/2026)

Thân ống dùng kính xanh ngọc, bốn gân đồng mảnh dọc từng nhánh và vòng nối màu
sứ. Các ngã rẽ giữ độ trong để thấy sinh vật. `COghePipeMazeArtBuilder` dựng
chi tiết theo mesh lòng ống thực tế, gộp mesh theo vật liệu; không thêm collider.
Menu dựng riêng: **Gravity Box → COghe → Rebuild Day Lab · Pipe Maze 15**.

Audit trước/sau: 22 thành phần vật lý và 25 transform tổ tiên không thay đổi;
Level15 definition giống nguyên bản. **7/7 kiểm thử ống đạt** tại
`Artifacts/COgheExpansion/PipeArt/tests.xml` (07:08:24Z). Bao gồm chọn nhánh,
chặn lệnh mặt hộp khi trong ống, đầu cụt, đường giải và hồi quy màn 16.
Ảnh trước/sau portrait lưu tại `Artifacts/COgheExpansion/PipeArt/`.

Bản macOS build thành công; đã kiểm tra trực tiếp tổng quan, chạm vào miệng ống,
COghe tới ngã rẽ và chế độ nhìn gần, rồi reset màn 15 để test. Thao tác kéo bằng
công cụ UI không xác nhận được xoay hộp trong lượt này. Chưa đo hiệu năng mobile.

## Màn 16 — thay thiết kế bằng Ghép cầu, 17/09/2026

Thiết kế khối cầu/sáu ống và phác thảo cũ được bỏ khỏi nội dung đang dùng.
Màn thay thế có ba mô-đun cầu chủ động A/B/C trên ray, bệ leo bên trái,
dải trơn và lỗ thoát bên phải. Hộp khóa xoay.
[Thiết kế / phác thảo mới](LevelDesign/COghe/Level16/README.md).

Dùng lại lực đẩy/kéo phụ thuộc mô, joint tuyến tính và chốt có thể nhả bằng
kéo ngược. `ManipulationGrip` cho phép tác giả đặt tay nắm ở vị trí với tới
được. `ManipulationHandleOnly` phân biệt chạm tay nắm (thao tác vật) và chạm
mặt cầu (bò lên). `ManipulationPlane` đặt mặt phẳng ngắm đích ở cao độ cầu,
tránh mặt trước của vật nuốt lệnh chỉ vào vị trí ghép phía sau. Các trường
này tùy chọn, mặc định rỗng/tắt ở các màn cũ.

`COgheAssemblyBridge` chỉ đo chốt; `COgheAssemblyBridgePresentation` đọc trạng
thái thật để đổi đèn. Thắng vẫn do toàn bộ cơ thể đi qua lỗ cuối, không yêu
cầu một cờ lịch sử ghép cầu hoặc tự mở đường bằng script. Reset trả các vật,
chốt, cơ thể và trạng thái quan sát về ban đầu.

Builder: **Gravity Box → COghe → Rebuild Assembly Bridge Level 16**, hoặc
`GravityBox.Editor.VenomCampaignBuilder.GenerateAssemblyBridgeLevel16`.
Bộ test ống bỏ hai ca riêng cho màn 16 cũ; kiểm tra chuột xoay chuyển sang
màn 18 có hỗ trợ xoay. Bộ `COgheAssemblyBridgeTests` kiểm tra từ scene mới,
bao gồm lời giải bằng lực, chạm tay nắm và chỉ đích, đi cầu và đủ 32 hạt thoát,
kéo ngược chốt, tự buông, reset và không thắng khi chỉ thẳng vào lỗ lúc chưa ghép.

Kiểm chứng lần thay màn: `Artifacts/COgheExpansion/bridge16-regression.xml`,
**27/28 đạt**, trong đó **3/3 màn 16 đạt**. Ca còn lỗi là
`Level20FullSolutionUsesMassOrderThreeRolesAndMergedEscape`, kẹt khi tiếp cận
G heavy gear carriage — cùng điểm lỗi đã ghi nhận trước thay màn 16. Không
coi campaign là hoàn toàn đạt. Hồi quy còn lại gồm đẩy/kéo màn 07, chuột xoay
ở màn 18, đầu vào khi khóa xoay/pause/reset, mê cung ống 15, cơ quan dùng chung
và load/idle/reset/roof-picking/render đủ 20 scene.

Lần kiểm tra cuối sau chỉnh nhãn/loại tay nắm trang trí dư: **3/3 màn 16 đạt**,
`Artifacts/COgheExpansion/bridge16-final.xml`, kết thúc 17/09/2026 07:36:02Z.
Build macOS thành công; thử trực tiếp chạm tay nắm A, chỉ đích vào chốt, quan
sát vật dịch chuyển, nhìn gần rồi reset. Đã để sẵn màn 16 ở trạng thái ban đầu.
Player.log không có exception/assertion. Chưa build APK hoặc đo hiệu năng mobile.

### Màn 19 — rõ cơ quan và thao tác (17/09/2026)

Camera 40°/18°, ViewRadius 0.57; nút A có đế/nắp lún, tay nắm B và mũi tên kéo, hai cửa đánh số với đèn trạng thái. HUD hướng dẫn theo cơ quan và nút chọn phần cho biết trái/phải/giữ A. Tốc độ nâng riêng màn 19 tăng lên 0.10 m/s để một lệnh kéo đủ mở hai cửa trước khi tự buông sau 3 giây. Kiểm chứng màn 19 và liên động: **4/4 đạt**, `cooperation19-final.xml`; có đường giải bằng chạm màn hình đến thoát đủ cơ thể. [Cách chơi và kiểm chứng](LevelDesign/COghe/Level19/README.md).
