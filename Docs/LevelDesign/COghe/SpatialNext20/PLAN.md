# COghe — 20 màn tiếp theo: từ sắp xếp đến phối hợp

29/09/2026 · **Đề xuất 11–30 nối tiếp 10 màn Spatial Pilot**. Đây là kế hoạch và minh hoạ, chưa dựng scene hoặc đo khả giải trong Unity. Các số 11–30 ở đây không ghi đè bộ V2 30 màn cũ.

[Xem toàn bộ minh hoạ](review.html) · [20 hồ sơ](README.md) · [Hợp đồng cơ quan](MECHANICS.md) · [Luật thiết kế chung](../../../COGHE_LEVEL_DESIGN_RULES.md).

**Bàn giao ngày 29/09/2026, nhánh `NewGraphic`:** [Hướng dẫn triển khai trên máy khác và ràng buộc đã chốt](IMPLEMENTATION_HANDOFF.md). Bản này lưu cùng source/asset của 10 Spatial; các ước lượng và trạng thái chưa kiểm chứng của 20 màn bên dưới vẫn giữ nguyên.

## 1. Hướng phát triển

Tăng độ khó bằng **thứ tự thao tác, hiểu đường nối và phân công**, giữ thao tác dễ. Xen sàn, bậc cao, vách và đường ống; không biến mọi màn thành ba tầng đặc cơ quan. Màn 11 và 21 nghỉ nhịp sau Boss. Các đoạn giới thiệu cơ chế mới cũng giảm số phụ thuộc để người chơi có thời gian hiểu. Vì vậy độ khó tăng theo chương, không ép mọi màn sau luôn khó hơn màn ngay trước.

Đẩy/kéo khối làm đường xuất hiện nhiều, nhưng thay đổi câu hỏi: kê ở đâu → đưa mảnh nào trước → vận chuyển lên cao → dùng làm đối trọng → lắp cả một cấu trúc có thể nâng. Ròng rọc có dây chịu lực nhìn thấy; mạch điện chỉ biểu diễn liên kết điều khiển. Đu dây là một cơ chế riêng, không giả bằng thang chạy ngang.

Hộp cố định theo bộ Spatial đã có; kéo để nhìn quanh, không nghiêng trọng lực. Đây là lựa chọn thiết kế của bản đề xuất để tránh lặp lại khó khăn căn trượt/rơi đã được người chơi phản ánh. Nếu sau này có bài nghiêng hộp, phải là bài riêng có dạy lại điều khiển.

## 2. Tiến trình 20 màn

| Màn | Tên | Câu đố chính | Phần cơ thể hữu ích tối đa | Ước lượng lần đầu* |
| --- | --- | --- | --- | --- |
| 11 | Kê một bậc | Một thùng trở thành bậc nối | 1 | 30–60 giây |
| 12 | Khối lớn đi trước | Đổi thứ tự hai khối, giữ ô chờ | 1 | 45–90 giây |
| 13 | Thùng đi thang | Chở thùng lên rồi làm bậc | 1 | 60–100 giây |
| 14 | Luồn một vòng | Kéo mở ống, nhìn cơ thể chảy qua | 1 | 60–100 giây |
| 15 | Gặp nhau ở ngã ba | Đi nhánh cơ quan trước nhánh thoát | 1 | 75–120 giây |
| 16 | Một thành hai | Học Q, hai phần giữ hai nút | 2 | 75–120 giây |
| 17 | Bạn giữ, mình luồn | Một giữ cửa, một mở chốt đường về | 2 | 90–150 giây |
| 18 | Bám dây sang bờ | Học bám dây, chọn bến rộng | 1 | 60–100 giây |
| 19 | Đưa bến lại gần | Đưa khay vào cung đu rồi mới đi | 1 | 90–150 giây |
| **20** | **BOSS · Hai nửa một máy** | **Tách → giữ → ống → giải phóng → nhập → kê → ròng rọc** | **2** | **3–5 phút** |
| 21 | Kéo đối trọng | Tải thùng làm cầu về gối | 1 | 60–100 giây |
| 22 | Ba mảnh thành đường | Ô chờ, ráp nhịp xa trước | 1 | 90–150 giây |
| 23 | Đổi tuyến trên vách | Đi ống tới trạm đổi đường trên cao | 1 | 90–150 giây |
| 24 | Hai rồi bốn | Học tách lặp, bốn nút chốt đường | 4 | 2–3 phút |
| 25 | Giữ lại phần lớn | 50% đẩy, hai phần 25% giữ khóa | 3 | 2–3 phút |
| 26 | Đu và luồn | Bạn đi ống mở bến cho bạn đu | 2 | 2–3 phút |
| 27 | Bốn trạm tiếp sức | Hai giữ, một nâng, một mở chốt giải phóng | 4 | 3–4 phút |
| 28 | Đường ống ba chiều | Phối hợp trong/ngoài mạng ống trên vách | 2 | 3–4 phút |
| 29 | Xưởng lắp cầu | Giữ khóa, ráp khung, nhập rồi nâng cầu | 3 | 3–5 phút |
| **30** | **BOSS · Hộp cộng hưởng** | **Ba khoang; mở độc lập hai nhánh, ghép khối rồi nâng đường cuối** | **4** | **5–8 phút** |

\* Là giả thuyết để playtest, không phải thời gian đã đo hoặc lời hứa về độ khó. Theo dõi riêng số quyết định, chiều sâu phụ thuộc, số vai đồng thời, độ chính xác, che khuất và khả năng sửa sai; không quy về một “điểm khó” giả chính xác.

Đường học: xếp khối 11→12→13→22→29; ống 14→15→23→28; Q 16→17→20→24→25→27→30; đu dây 18→19→26→30. Boss chỉ phối hợp kỹ năng đã gặp. Màn 21 là biến thể trực quan của ròng rọc đã học, không thêm hệ cân chính xác bằng số.

## 3. Máy lượng tử: đổi ngoại hình lẫn hợp đồng tách

Thay cưa ở các màn mới bằng máy Q nhỏ, vỏ ngà, cửa kính, quét trắng dịu, một cửa vào và **hai** khay ra có vách phân luồng. Tự chạy sau khi toàn bộ phần đi vào nằm trong vùng nhận và lối ra an toàn.

- 100% vào → 50% + 50%.
- Một nửa 50% quay lại → 25% + 25%; nửa còn lại ngoài máy vẫn 50%.
- Cho nửa 50% còn lại vào → bốn phần 25%.
- Máy không chia tiếp liên tục khi đứng trong vùng. Phải ra và vào lại cho một lượt mới.
- Gần nhau, không vách ngăn thì nhập ngay; không thêm thời gian cấm nhập hoặc điều kiện đang làm nhiệm vụ.
- Chỉ thắng khi toàn bộ đã nhập ở bên trong rồi thoát. Ống chuyển không phải lỗ thắng.

Bản đề xuất dùng **tối đa 4 phần trong lời giải** để dễ chọn trên điện thoại. Đây không phải quyết định cấm máy chia nhỏ hơn trên toàn game. Khối lượng nhỏ nhất / độ phân giải solver vẫn cần thử; trước triển khai phải nêu rõ, không âm thầm sai tỷ lệ. Xem [MECHANICS](MECHANICS.md) cho trạng thái, giữ ID, bảo toàn mô và rủi ro.

Không đổi dao của các catalog cũ trong đợt tài liệu này. Khi chuyển các màn cũ, cần thay geometry, đường giải và chạy hồi quy; không chỉ đổi model cưa thành model máy rồi giữ cách cắt theo vị trí.

## 4. Hình học giúp dễ điều khiển mà vẫn là câu đố

Lấy đường kính mô khi nghỉ `D` và tầm với cực đại `R` của runtime làm đơn vị kiểm tra. Mốc greybox đề xuất: mặt đường ≥2D, khay đón/nhập ≥3D, chỗ quay đầu đủ toàn thân, vùng thao tác có chân đứng trước tay cầm. Đây là tỷ lệ bắt đầu dựng, phải điều chỉnh qua test — không phải kích thước chuẩn đã đo. Vùng chạm biểu kiến hướng tới ≥44–48 logical px trên mobile; không tăng hitbox tới mức chọn nhầm mặt đối diện.

Cơ thể leo được vách, nên **cao hơn không tự tạo câu đố**. Mỗi hồ sơ có ràng buộc chặn đường tắt: dải không bám nối kín tới kính/nóc, khoảng hở hoặc bề mặt đứng không bám; khối/ống/dây tạo mặt đường thực sự vượt vùng đó. Tránh dùng một miếng trơn phẳng nằm ngang như tường vô hình: sinh vật vẫn có thể trôi qua nó theo quán tính. Dải trơn vẫn chạm được và thể hiện thất bại bám một cách nhất quán.

Các pad phải xa nhau quá sải của một phần và có khoảng đứng rõ; cần kiểm tra nguyên khối có vô tình phủ hai pad hay không. Không chặn cheat bằng đếm `GroupId` nếu cơ chế công bố là tải vật lý. Khối di động trên ray không thể tùy ý mang tới mọi pad; nếu người chơi tạo một lời giải khác hợp luật thì chấp nhận hoặc sửa hình học công khai.

Mỗi nhiệm vụ giữ có một bước **chốt rồi giải phóng**, để không giam người giữ khi cần hợp thể. Mỗi khay có gọi về; mỗi tube có bến chờ và lối lui; mỗi khối có tay kéo còn tiếp cận được khi đặt sai; dây có tời trả về và sàn cứu hộ. Không countdown, không điểm nhả dây theo frame, không vùng nhận bé xíu.

## 5. Đồ hoạ và animation

Giữ style đã chốt, không quay về bộ Blender: kính trong C, nền lab dịu, khung nhôm mảnh, nhựa ngà, bảng số dán kính. Cơ quan chiếm ít màu; xanh A/san hô B chỉ trên tay, nhãn, đường mạch mảnh và dải nhận ở đầu ra. Cùng chức năng nhiều nút dùng A1/A2… cùng màu; không sinh thêm một màu cho từng nút. Glyph tròn/thoi và ký tự hỗ trợ người khó phân biệt màu. Tím = không bám; mint = lỗ cuối; cyan = vật liệu vỏ ống.

Ray/cột đỡ satin xám lùi về nền. Dây cơ học vẫn nhìn được đường truyền lực qua bánh; mạch điều khiển đi trên bề mặt, tránh giằng chéo chắn COghe. Q có hình dáng khác hẳn cửa/van và nhìn thấy hai khay equal-mass. Không dùng tím hoặc mint làm ánh quét Q.

Animation gồm: thăm dò tay cầm → ghì đẩy/kéo theo lực; ôm vòng bằng xúc tu → thân kéo dài theo cung đu; dồn thân → dòng đen trong ống → gom ở khay; tò mò nhìn Q → hai thùy tách → hai phần bật nhịp nhẹ. Hành động xác nhận bởi physics/state, không bởi hết clip. Giữ idle khi chờ; không để tất cả các phần giãy làm khó chọn.

Boss20/30 có hiệu ứng mở đường theo từng chốt thật, âm cơ khí êm và ăn mừng focus COghe. Không thêm realtime light, tia sáng hoặc rung camera che thao tác. Minh hoạ có mũi tên lời giải cho người dựng; game Boss **Hints=None**.

## 6. Kiến trúc triển khai

| Phần | Dùng lại / mở rộng | Tách trách nhiệm |
| --- | --- | --- |
| Catalog/authoring | Builder Spatial, stable IDs 11–30; definitions gồm nodes, surface tags, links | ID không đổi khi sắp thứ tự; runtime không `if level==...` |
| Đẩy/kéo/khối | `COgheTapRail`, `COgheRailSlider`, `VenomMovableProp` | Hốc/chốt dùng pose thật, không snap xa; thân art riêng collider |
| Ròng rọc | `COghePulleyDrive` | Force/constraint và chốt tải; line/circuit chỉ hiển thị |
| Thang | `COghePassengerLift`, mở rộng tải prop | Bến, occupancy, gọi về, lực motor hữu hạn; không parent mô |
| Ống | `COgheTubeNetwork`, van tuyến có interlock | Topology/occupancy quyết định tuyến; preview không mở đường |
| Q mới | Đề xuất `COgheQuantumSplitter` | Transaction một nhóm, chia đúng khối lượng, ID, hai khay, chống reentry giả |
| Đu mới | Đề xuất `COgheSwingTransfer` | Một constraint dây + tời + bến; bám/nhả chỉ khi tiếp xúc thật |
| Logic phối hợp | Cảm biến tải, AND, Hold, Latch dùng chung | Phân biệt cửa tạm và chốt vĩnh viễn bằng hình + state |
| Navigation | Surface/route hiện có + liên kết động theo trạng thái | Dirty vùng thay đổi, không rebuild cả hộp mỗi frame |
| Feedback | Binder từ state sang cap, mạch, animation, audio | Không dùng VFX để hoàn tất objective |

Giữ baseline 32 hạt tổng, 120 Hz và lực/khối lượng hiện có. Nhiều phần không tự nhân 32 hạt mỗi phần. Reuse registry/scratch buffers; không tìm toàn scene hoặc cấp phát đường đi mỗi frame; pooling VFX, share materials, gộp geometry tĩnh nhưng giữ collider cơ quan riêng. Chỉ tối ưu sau profiler, không giảm độ chính xác/mass theo điện thoại.

## 7. Thứ tự làm và cổng kiểm chứng

1. **Thử kỹ thuật Q và dây trong sandbox**: chứng minh exact50/50, nhập, reentry, khay bị chặn; cung đu, đổi ý và hồi về bằng input thật. Rủi ro lớn nhất trước khi dựng hàng loạt.
2. **Greybox 11–15**: lắp khối, tải prop và pipe; kiểm tra đường tắt qua kính/nóc, tay bị che, lùi sửa sai. Khóa kích thước chỉ sau khi chơi trên màn dọc.
3. **Greybox 16–20**: Q hai phần, giữ/chốt/giải phóng, dây và Boss đầu. Hồi quy bộ01–10 sau sửa component chung.
4. **Greybox 21–25**: đối trọng, thứ tự ba mảnh, tuyến trên vách, bốn phần và phân bổ 50/25/25. Chỉ thêm bốn phần sau khi chọn phần thật dễ.
5. **Greybox 26–30**: kết hợp, hồi lưu và Boss ba khoang. Thử đảo thứ tự, tụ sớm, tải nửa hành trình, thoát sớm.
6. **Art/animation + native build**: áp cùng prefab/material; so minh hoạ cạnh runtime từng màn, ghi sai khác có lý do. Playtest người mới và profile OPPO trước nghiệm thu.

Không chốt ngày công giả trước hai prototype mới. Mỗi cụm chỉ đi tiếp khi đường giải, thao tác và phục hồi đủ bằng chứng; user đã cho phép phạm vi triển khai thì không xin lại các quyết định cũ.

## 8. Kiểm chứng gameplay, khó và mobile

- Author replay dùng input chơi thật + assertions trên mô/chốt, không set pose sinh vật để qua bài. Thử mọi mốc Hold trước/sau latch, Retry, pause, tắt app, lùi ống, thang gọi về; đường thay thế hợp lý.
- Q: ≥100 lượt vòng split/merge, 100→50/50→50/25/25→25×4, tách lại sau nhập; một lượt vào chỉ một split; nhóm ngoài không mất command; đầu ra bị chiếm phải chờ an toàn. Mỗi pha tổng mass/particle ID giữ đúng.
- Dây: bến thiếu tầm, đổi target giữa cung, rơi xuống sàn, recall, nhận bến có mô/prop; không teleport hoặc nhả giữa không khí vì timer.
- Ống: nhánh quay lại, điểm giao không nối, van đổi khi có mô, toàn bộ về một nhóm; Transfer khác FinalExit; thoát chưa nhập báo đúng một lần.
- Playtest tối thiểu một nhóm nhỏ 5–8 người chưa biết lời giải cho mỗi chặng. Ghi thời gian, chạm nhầm, hint/retry, nơi không tiến triển, người hiểu lời giải nhưng thi hành thất bại. Nhóm này định hướng chỉnh, chưa đủ khẳng định retention toàn thị trường.
- Mục tiêu nghiệm thu đề xuất trên OPPO: hướng tới 60 FPS; frame time p95≤20 ms và p99≤33.3 ms ở kịch bản chuẩn sau warm-up; ghi toàn bộ spike và nguyên nhân, không chỉ FPS trung bình. Đây là **mục tiêu chưa đo** và cần đối chiếu refresh rate/cấu hình máy thực.
- Ba lượt mỗi màn, gồm đoạn nặng nhất (Boss bốn nhóm, ống/van, rope/khay) và 20 phút lặp để thấy thermal throttling; đo cùng baseline01–10, quality/resolution/cap cố định. Lưu CPU/GPU, allocations/GC, memory, raw frame times, hashAPK, thiết bị và diff.
- Giảm overdraw/mạch/VFX/mesh trước; không thay số mô, fixed step, lực bám hoặc luật chia theo cấu hình thấp. Nếu chưa đạt, ghi bottleneck cụ thể và kiểm chứng lại sau sửa.

## 9. Phạm vi bàn giao hiện tại

20 concept, 20 hồ sơ theo mẫu, sơ đồ phụ thuộc, hợp đồng cơ quan, kế hoạch xây dựng và trang review. Kiểm tra cấu trúc tài liệu không chứng minh Unity chơi được. Q, rope, kích thước va chạm, độ khó lần đầu và performance là những việc còn phải kiểm chứng khi triển khai.
