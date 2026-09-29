# COghe 12 — Khối lớn đi trước

Theo [LEVEL_TEMPLATE](../../LEVEL_TEMPLATE.md). Nguồn: [quy tắc](../../../../COGHE_LEVEL_DESIGN_RULES.md), [style](../../../../ArtDirection/COghe/STYLE_RULES.md), [kế hoạch](../PLAN.md), [hợp đồng cơ quan](../MECHANICS.md).

## 1. Định danh, phạm vi và trạng thái

- ID: `coghe.spatial.next.12` · vị trí đề xuất 12 sau Spatial Pilot 01–10 · hồ sơ v0.1, 29/09/2026.
- Trạng thái **Đã dựng — greybox + art Glass C** (29/09/2026): scene `Assets/_Game/Venom/SpatialCampaign/COgheSpatial12.unity`, definition `Definitions/Spatial12.asset`, ID giữ nguyên. Giải trọn bằng chạm thật trong Unity PlayMode (bộ hồi quy 76/76) và native Mac (20/20 màn 11–30); chưa đo OPPO, chưa chơi thử người mới. Bố cục thực tế và khác biệt ở mục 10 và [AS_BUILT](../AS_BUILT_2026_09_29.md).
- [Minh hoạ](../Illustrations/12.png) · [sơ đồ phụ thuộc](../Routes/12.svg). Ảnh là concept, không phải screenshot hoặc bản kích thước thi công.
- Được yêu cầu: thiết kế tiếp 20 màn, đẩy/kéo/xếp khối, ròng rọc, đu dây, ống, Q tách 50/50. Bố trí riêng màn này là đề xuất để người dùng xem.
- Cổng chất lượng: đủ hồ sơ → greybox chơi trọn → art/animation → hồi quy và thiết bị thật. Hiện mới ở hồ sơ, không tự chứng nhận mốc sau.

## 2. Mục tiêu trải nghiệm và vị trí trong tiến trình

**Phải sắp đúng thứ tự để không chắn tay nắm.** Vai trò: Luyện xếp hình. Không gian sàn, hai cao độ.

Khoảnh khắc khám phá: Hai khối ghép thành một đường đi gọn. Tham khảo: Origin 16 · Ghép cầu. Kỹ năng/tổ hợp xem bảng tiến trình trong PLAN; chỉ ghi học được khi hành động thật thành công. Pure puzzle, không nâng chỉ số/mua đồ/buff. Hints: **Gợi ý thao tác mới, sau đó giảm dần**. Boss không giới thiệu cơ chế mới; mũi tên trong hình chỉ dành cho người dựng.

## 3. Phác thảo, hình học, camera và thao tác

Hai khối chữ nhật thấp và cao nằm trên hai rãnh vuông góc ở sàn. Đẩy khối cao vào hốc phía sau trước, sau đó khối thấp vào phía trước tạo cầu thang hai bậc đến đảo thoát. Hốc đỗ vẽ viền nhạt cùng dấu khối, không đánh số lời giải.

**Ràng buộc hình học:** Hai rãnh chỉ giao ở một vùng đủ cho một khối; có ô chờ bên ngoài. Hai khối đứng cạnh nhau thành bậc, không xếp xuyên hoặc treo lơ lửng. Cả đảo thoát và kính lân cận được ngăn bằng dải không bám.

Hộp đứng yên, kéo đổi camera, pinch zoom. Camera đầu 3/4 thấy điểm xuất phát và cơ quan đầu; xem tổng quan luôn có. Màn nhiều cao độ dùng điểm xem sàn/bệ/vách; thay camera không đổi vật lý. Hộp ngang khoảng 0.8 m là mốc greybox của bộ trước, không cố nhét cơ quan vào nếu mục tiêu chạm quá nhỏ. Dùng kích thước theo cơ thể và vùng chạm tại PLAN, phải đo lại trên màn dọc 720×1280 và OPPO.

Chạm sàn/bệ → tới điểm; chạm tay → tiếp cận/bám; chạm hốc/đích ray → tác động; chạm phần hoặc ô phần → đổi chọn. Mũi tên chọn + % ở bài tách. Hủy bằng chọn đích khác trong pha an toàn. Nóc/trơn vẫn chọn được; trơn không phát lực bám. Khi ở ống chỉ nhận nhánh/đầu ống, không kéo theo chạm kính ngoài. Không chọn xuyên phần kính gần tới cơ quan sau bị che.

## 4. Lời giải, kết thúc và phục hồi

| Bước | Lệnh / ý định | Trạng thái trước → sau | Tín hiệu thật | Bỏ dở/làm ngược |
| --- | --- | --- | --- | --- |
| 1 | Kéo khối thấp ra ô chờ để mở đường ngang | Hoàn tất mốc 0 → mốc 1; tiếp xúc/cơ quan thật xác nhận | Tư thế, cap/chốt hoặc đường bám tương ứng đổi thật | Đổi đích hủy tiếp cận; lực/chốt đang tải chuyển an toàn theo hợp đồng |
| 2 | Đẩy khối cao vào hốc trong | Hoàn tất mốc 1 → mốc 2; tiếp xúc/cơ quan thật xác nhận | Tư thế, cap/chốt hoặc đường bám tương ứng đổi thật | Đổi đích hủy tiếp cận; lực/chốt đang tải chuyển an toàn theo hợp đồng |
| 3 | Đẩy khối thấp về chân khối cao | Hoàn tất mốc 2 → mốc 3; tiếp xúc/cơ quan thật xác nhận | Tư thế, cap/chốt hoặc đường bám tương ứng đổi thật | Đổi đích hủy tiếp cận; lực/chốt đang tải chuyển an toàn theo hợp đồng |
| 4 | Leo hai bậc tới đảo có lỗ thoát | Hoàn tất mốc 3 → mốc 4; tiếp xúc/cơ quan thật xác nhận | Tư thế, cap/chốt hoặc đường bám tương ứng đổi thật | Đổi đích hủy tiếp cận; lực/chốt đang tải chuyển an toàn theo hợp đồng |

- **Phục hồi riêng:** Ô chờ đủ chỗ quay đầu và giữ tay nắm lộ ra; đặt sai chỉ cần kéo lại, không reset cả màn.
- Dòng khối lượng dự kiến: `100%`. Tách duy nhất tại Q; cùng gần nhau không vật cản thì tự tụ, kể cả khác nhiệm vụ, không cooldown. Với bài giữ nút, mở chốt đường về trước khi gọi các phần về tụ.
- Thắng: một cơ thể chứa toàn bộ mô đã nhập **bên trong**, rồi toàn bộ qua `FinalExit`. Phần ra trước: Lost một lần, đúng câu “bạn phải hợp thể trước khi chui ra”. `Transfer` không kết thúc màn.
- Đường thay thế hợp vật lý được chấp nhận; không ép lời giải bằng số màn. Boss 30 cho đổi thứ tự hai nhánh và nhập cặp hoặc nhập đủ sớm nếu hình học cho phép.
- Retry hủy command, force lease, callback, trạng thái tải/van/dây; trả đủ mô và mọi prop về trạng thái đầu. Pause đóng băng sim, không tiếp tục split hoặc timer riêng. Tải lại không giữ tác vụ tạm.

## 5. Cơ quan, trạng thái và kiến trúc dùng lại

| MechanismId | Loại | Dữ liệu / tác động | Reset |
| --- | --- | --- | --- |
| L12.blocks | COgheTapRail + COgheRailSlider / VenomMovableProp | Xem hợp đồng; dữ liệu hành trình, tải và liên kết của màn | Reset pose, lực, chốt, người giữ và tác vụ |

**Phụ thuộc:** Ô chờ mở → khối cao vào trong → khối thấp vào ngoài → đường leo.

Các cơ quan dùng dữ liệu ID/link, `Hold`/`Latch` phân biệt rõ. Navigation cập nhật vùng bị thay đổi khi ray/van/khay đạt trạng thái và trong lúc vật thật chuyển động; không rebuild cả hộp mỗi frame. Art không cấp quyền đi xuyên. Kỹ thuật tách 50/50 và bám dây cần prototype, xem MECHANICS; không chia theo số hạt làm mất khối lượng. Giữ ID ổn định, nhiệm vụ phần không vào máy không bị hủy.

## 6. Hình ảnh, animation và phản hồi

Glass C / Day Lab Unity 07: kính trong, khung nhôm mảnh, nút góc ngà, bảng nhựa **12** dán kính. Vỏ cơ quan ngà, ray satin xám; A xanh tròn / B san hô hình thoi ở vị trí tương tác và đầu nhận. Mạch mảnh dán sàn/vách, cáp ròng rọc là dây chịu lực. Tím chỉ mặt trơn, mint chỉ lỗ cuối, cyan là thành ống trống, không nước.

Nhận ý định → nhìn/vươn xúc tu → tiếp cận thật → ghì kéo/ôm vòng/chảy ống → thả theo kết quả sim. Thiếu lực: thân căng, vật nhích có tải và biểu tượng sức kéo, không rung giật liên tục. Q quét trắng dịu, hai thùy equal-mass ra hai khay; không blade/máu/nhân bản thêm vật chất. Animation thắng chỉ sau toàn bộ mô ra; ẩn vật che và focus COghe theo luồng hiện có.

Điểm nhấn riêng: **Hai khối ghép thành một đường đi gọn.**. Hiệu ứng không che tay, điểm bám hoặc cửa sổ junction. Chưa có ảnh/video runtime màn này.

## 7. Độ khó và ngân sách runtime

| Trục | Mục tiêu thiết kế | Thực đo |
| --- | --- | --- |
| Quyết định có ý nghĩa / chuỗi phụ thuộc | Khoảng 3 / 3 mốc | Chưa chơi thử |
| Số phần hữu ích lớn nhất | 1; chia/tụ như dòng khối lượng | Chưa chơi thử |
| Chính xác và quan sát | Thấp: chọn đích rộng, không canh thời điểm; camera xem được liên kết | Chưa chơi thử |
| Thời gian | 45–90 giây, ước lượng định hướng | Chưa đo |
| Cụm chuyển động cùng lúc | Mục tiêu khoảng 2; không phải trần số rigidbody | Chưa đo |
| Vật lý / route / skin | Giữ baseline 32 hạt, 120 Hz; ít constraint dây; cache route, không GC/frame sau warm-up | Chưa profiler |
| GPU / memory | Shared materials, một bóng chính, giảm overdraw kính, không light riêng từng cơ quan | Chưa profiler |

Không dùng điểm tổng giả chính xác. Thời gian dài không tự đồng nghĩa hay; nếu nhiều chạm nhầm/kẹt góc thì sửa điều khiển/hình học trước khi giảm câu đố.

## 8. Chơi thử và hồi quy

**Tất cả đang chờ triển khai**, chưa chạy Unity/thiết bị cho màn này.

- Ca riêng: Cố đẩy khối thấp vào trước rồi lấy ra; camera phải thấy tay khối cao ở trạng thái sai.
- Đường giải chính qua input thật; đường thay thế và ngẫu nhiên đổi thứ tự. Không set position sinh vật để chứng minh qua màn.
- Lùi, đổi đích, giữ nửa hành trình, pause, Retry, đổi màn nhiều lượt; tụ sớm/ra lỗ sớm; mỗi Transfer hai chiều.
- Tách đúng tỷ lệ và bảo toàn mô sau 100 lượt Q, split/merge cùng tick; không tự tách khi đứng yên trong khoang; máy bận/khay ra bị chiếm.
- Chạm gần khung, kính/nóc, vùng trơn và ở 3 góc cam; kiểm tra người mới không biết lời giải.
- Hồi quy Spatial 01–10 và catalog V2 với component chung; không thay luật archived blade cho tới khi chuyển catalog có chủ đích.

## 9. Bằng chứng hiệu năng trên thiết bị

Chưa có APK/binary hoặc log cho màn này. Khi dựng, ghi hash/build/commit/diff, OPPO model/SoC/OS, refresh rate, resolution, frame cap, quality, profiler mode và cấu hình sim. Đo warm-up, ba lượt đường giải, đoạn cực tải rồi 20 phút lặp; lưu p50/p95/p99/max, spikes, CPU/GPU, GC, memory, nhiệt. So baseline cùng máy/cấu hình; mục tiêu và điều kiện ở PLAN. Không lấy FPS của bộ trước gán cho màn mới.

## 10. Tích hợp, tương thích và nghiệm thu

Đề xuất nối sau Spatial Pilot 10 bằng ID ổn định, giữ catalog V2/Origin để đối chiếu. Không ghi đè ID hoặc save 01–10. Definition/scene/builder/prefab **chưa tạo**; quyết định đường dẫn sau greybox. Migration tiến độ thử lặp an toàn, thắng/replay không cấp trùng phần thưởng; tắt app giữa ăn mừng/lưu vẫn nhất quán.

29/09/2026: tạo hồ sơ và concept v0.1 theo yêu cầu 20 màn tiếp. Kết luận: **đề xuất thiết kế, chưa nghiệm thu khả giải hoặc performance**. Tiếp theo: prototype cơ quan mới, dựng greybox, kiểm chứng theo các mốc; không lấy hình làm bằng chứng physics.

29/09/2026 — **bản dựng**: Hai khối trên hai ray giao nhau; khối thấp phải chờ ngoài cho khối cao qua rồi về chân nó. Khác hồ sơ: Như hồ sơ (thứ tự thể hiện qua chỗ giao ray). Bằng chứng: `COgheSpatialCampaignTests.Spatial12Solve` (PlayMode 120 Hz, chạm thật, 32/32 hạt qua lỗ cuối, 1 cơ thể, Retry về đầu); replay native Mac cùng kịch bản; ảnh `Docs/Verification/COgheSpatialNext20/12-*.png`. Chưa kiểm: OPPO/FPS điện thoại, người chơi mới, từng ca phục hồi và đường thay thế ở mục 8 (mới có đường giải chính).
