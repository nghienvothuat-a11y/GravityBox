# COghe 27 — Bốn trạm tiếp sức

Theo [LEVEL_TEMPLATE](../../LEVEL_TEMPLATE.md). Nguồn: [quy tắc](../../../../COGHE_LEVEL_DESIGN_RULES.md), [style](../../../../ArtDirection/COghe/STYLE_RULES.md), [kế hoạch](../PLAN.md), [hợp đồng cơ quan](../MECHANICS.md).

## 1. Định danh, phạm vi và trạng thái

- ID: `coghe.spatial.next.27` · vị trí đề xuất 27 sau Spatial Pilot 01–10 · hồ sơ v0.1, 29/09/2026.
- Trạng thái **Đã dựng — greybox + art Glass C** (29/09/2026): scene `Assets/_Game/Venom/SpatialCampaign/COgheSpatial27.unity`, definition `Definitions/Spatial27.asset`, ID giữ nguyên. Giải trọn bằng chạm thật trong Unity PlayMode (bộ hồi quy 76/76) và native Mac (20/20 màn 11–30); chưa đo OPPO, chưa chơi thử người mới. Bố cục thực tế và khác biệt ở mục 10 và [AS_BUILT](../AS_BUILT_2026_09_29.md).
- [Minh hoạ](../Illustrations/27.png) · [sơ đồ phụ thuộc](../Routes/27.svg). Ảnh là concept, không phải screenshot hoặc bản kích thước thi công.
- Được yêu cầu: thiết kế tiếp 20 màn, đẩy/kéo/xếp khối, ròng rọc, đu dây, ống, Q tách 50/50. Bố trí riêng màn này là đề xuất để người dùng xem.
- Cổng chất lượng: đủ hồ sơ → greybox chơi trọn → art/animation → hồi quy và thiết bị thật. Hiện mới ở hồ sơ, không tự chứng nhận mốc sau.

## 2. Mục tiêu trải nghiệm và vị trí trong tiến trình

**Giữ hai khóa trong khi hai bạn mở chốt cuối.** Vai trò: Luyện bốn vai. Không gian hai tầng thoáng.

Khoảnh khắc khám phá: Một phần chạm đỉnh và giải phóng công việc của cả nhóm. Tham khảo: Origin 19 / Spatial 08. Kỹ năng/tổ hợp xem bảng tiến trình trong PLAN; chỉ ghi học được khi hành động thật thành công. Pure puzzle, không nâng chỉ số/mua đồ/buff. Hints: **Gợi ý thao tác mới, sau đó giảm dần**. Boss không giới thiệu cơ chế mới; mũi tên trong hình chỉ dành cho người dựng.

## 3. Phác thảo, hình học, camera và thao tác

Bốn phần 25%; hai nút A ở hai góc sàn mở đường lên thang. Phần thứ ba đưa khay nhẹ vào bến thang và giữ cần B để nâng phần thứ tư. Trên cao có tay chốt hồi lưu; kéo nó thì cả thang lẫn đường tụ được giữ mở, giải phóng ba người còn lại.

**Ràng buộc hình học:** Hai nút A ở sàn, tay B cạnh bến dưới, khay một hành trình thẳng. Tải nhẹ đủ cho 25% kéo. Chốt C là móc trung tính trên bệ cao, không thêm màu circuit thứ ba. C mở đường gọi thang cho cả nhóm.

Hộp đứng yên, kéo đổi camera, pinch zoom. Camera đầu 3/4 thấy điểm xuất phát và cơ quan đầu; xem tổng quan luôn có. Màn nhiều cao độ dùng điểm xem sàn/bệ/vách; thay camera không đổi vật lý. Hộp ngang khoảng 0.8 m là mốc greybox của bộ trước, không cố nhét cơ quan vào nếu mục tiêu chạm quá nhỏ. Dùng kích thước theo cơ thể và vùng chạm tại PLAN, phải đo lại trên màn dọc 720×1280 và OPPO.

Chạm sàn/bệ → tới điểm; chạm tay → tiếp cận/bám; chạm hốc/đích ray → tác động; chạm phần hoặc ô phần → đổi chọn. Mũi tên chọn + % ở bài tách. Hủy bằng chọn đích khác trong pha an toàn. Nóc/trơn vẫn chọn được; trơn không phát lực bám. Khi ở ống chỉ nhận nhánh/đầu ống, không kéo theo chạm kính ngoài. Không chọn xuyên phần kính gần tới cơ quan sau bị che.

## 4. Lời giải, kết thúc và phục hồi

| Bước | Lệnh / ý định | Trạng thái trước → sau | Tín hiệu thật | Bỏ dở/làm ngược |
| --- | --- | --- | --- | --- |
| 1 | Tách bốn phần; hai phần giữ hai nút A | Hoàn tất mốc 0 → mốc 1; tiếp xúc/cơ quan thật xác nhận | Tư thế, cap/chốt hoặc đường bám tương ứng đổi thật | Đổi đích hủy tiếp cận; lực/chốt đang tải chuyển an toàn theo hợp đồng |
| 2 | Phần thứ ba đưa khay về bến, giữ cần B | Hoàn tất mốc 1 → mốc 2; tiếp xúc/cơ quan thật xác nhận | Tư thế, cap/chốt hoặc đường bám tương ứng đổi thật | Đổi đích hủy tiếp cận; lực/chốt đang tải chuyển an toàn theo hợp đồng |
| 3 | Phần thứ tư lên khay và đi tới tay chốt trên cao | Hoàn tất mốc 2 → mốc 3; tiếp xúc/cơ quan thật xác nhận | Tư thế, cap/chốt hoặc đường bám tương ứng đổi thật | Đổi đích hủy tiếp cận; lực/chốt đang tải chuyển an toàn theo hợp đồng |
| 4 | Kéo chốt: đường tụ và thang vào detent giữ an toàn | Hoàn tất mốc 3 → mốc 4; tiếp xúc/cơ quan thật xác nhận | Tư thế, cap/chốt hoặc đường bám tương ứng đổi thật | Đổi đích hủy tiếp cận; lực/chốt đang tải chuyển an toàn theo hợp đồng |
| 5 | Cho các phần lên sàn hội tụ rồi hợp thể và thoát | Hoàn tất mốc 4 → mốc 5; tiếp xúc/cơ quan thật xác nhận | Tư thế, cap/chốt hoặc đường bám tương ứng đổi thật | Đổi đích hủy tiếp cận; lực/chốt đang tải chuyển an toàn theo hợp đồng |

- **Phục hồi riêng:** Thả cần B sớm thì khay hạ về bến có giảm chấn, không kẹp mô; thang gọi về được. Chốt cuối không reset khi rời nút.
- Dòng khối lượng dự kiến: `100% → 50% + 50% → 50% + 25% + 25% → 25% + 25% + 25% + 25% → 50% + 50% → 100%`. Tách duy nhất tại Q; cùng gần nhau không vật cản thì tự tụ, kể cả khác nhiệm vụ, không cooldown. Với bài giữ nút, mở chốt đường về trước khi gọi các phần về tụ.
- Thắng: một cơ thể chứa toàn bộ mô đã nhập **bên trong**, rồi toàn bộ qua `FinalExit`. Phần ra trước: Lost một lần, đúng câu “bạn phải hợp thể trước khi chui ra”. `Transfer` không kết thúc màn.
- Đường thay thế hợp vật lý được chấp nhận; không ép lời giải bằng số màn. Boss 30 cho đổi thứ tự hai nhánh và nhập cặp hoặc nhập đủ sớm nếu hình học cho phép.
- Retry hủy command, force lease, callback, trạng thái tải/van/dây; trả đủ mô và mọi prop về trạng thái đầu. Pause đóng băng sim, không tiếp tục split hoặc timer riêng. Tải lại không giữ tác vụ tạm.

## 5. Cơ quan, trạng thái và kiến trúc dùng lại

| MechanismId | Loại | Dữ liệu / tác động | Reset |
| --- | --- | --- | --- |
| L27.quantum | COgheQuantumSplitter (đề xuất mới, chưa có runtime) | Xem hợp đồng; dữ liệu hành trình, tải và liên kết của màn | Reset pose, lực, chốt, người giữ và tác vụ |
| L27.pads | cảm biến tải + logic Hold/AND/Latch dùng chung | Xem hợp đồng; dữ liệu hành trình, tải và liên kết của màn | Reset pose, lực, chốt, người giữ và tác vụ |
| L27.lift | COghePassengerLift (mở rộng chở prop) | Xem hợp đồng; dữ liệu hành trình, tải và liên kết của màn | Reset pose, lực, chốt, người giữ và tác vụ |
| L27.blocks | COgheTapRail + COgheRailSlider / VenomMovableProp | Xem hợp đồng; dữ liệu hành trình, tải và liên kết của màn | Reset pose, lực, chốt, người giữ và tác vụ |

**Phụ thuộc:** A1 & A2 giữ → B giữ nâng bạn thứ tư → C latch đường về → rời A/B → hợp thể.

Các cơ quan dùng dữ liệu ID/link, `Hold`/`Latch` phân biệt rõ. Navigation cập nhật vùng bị thay đổi khi ray/van/khay đạt trạng thái và trong lúc vật thật chuyển động; không rebuild cả hộp mỗi frame. Art không cấp quyền đi xuyên. Kỹ thuật tách 50/50 và bám dây cần prototype, xem MECHANICS; không chia theo số hạt làm mất khối lượng. Giữ ID ổn định, nhiệm vụ phần không vào máy không bị hủy.

## 6. Hình ảnh, animation và phản hồi

Glass C / Day Lab Unity 07: kính trong, khung nhôm mảnh, nút góc ngà, bảng nhựa **27** dán kính. Vỏ cơ quan ngà, ray satin xám; A xanh tròn / B san hô hình thoi ở vị trí tương tác và đầu nhận. Mạch mảnh dán sàn/vách, cáp ròng rọc là dây chịu lực. Tím chỉ mặt trơn, mint chỉ lỗ cuối, cyan là thành ống trống, không nước.

Nhận ý định → nhìn/vươn xúc tu → tiếp cận thật → ghì kéo/ôm vòng/chảy ống → thả theo kết quả sim. Thiếu lực: thân căng, vật nhích có tải và biểu tượng sức kéo, không rung giật liên tục. Q quét trắng dịu, hai thùy equal-mass ra hai khay; không blade/máu/nhân bản thêm vật chất. Animation thắng chỉ sau toàn bộ mô ra; ẩn vật che và focus COghe theo luồng hiện có.

Điểm nhấn riêng: **Một phần chạm đỉnh và giải phóng công việc của cả nhóm.**. Hiệu ứng không che tay, điểm bám hoặc cửa sổ junction. Chưa có ảnh/video runtime màn này.

## 7. Độ khó và ngân sách runtime

| Trục | Mục tiêu thiết kế | Thực đo |
| --- | --- | --- |
| Quyết định có ý nghĩa / chuỗi phụ thuộc | Khoảng 7 / 6 mốc | Chưa chơi thử |
| Số phần hữu ích lớn nhất | 4; chia/tụ như dòng khối lượng | Chưa chơi thử |
| Chính xác và quan sát | Thấp: chọn đích rộng, không canh thời điểm; camera xem được liên kết | Chưa chơi thử |
| Thời gian | 3–4 phút, ước lượng định hướng | Chưa đo |
| Cụm chuyển động cùng lúc | Mục tiêu khoảng 4; không phải trần số rigidbody | Chưa đo |
| Vật lý / route / skin | Giữ baseline 32 hạt, 120 Hz; ít constraint dây; cache route, không GC/frame sau warm-up | Chưa profiler |
| GPU / memory | Shared materials, một bóng chính, giảm overdraw kính, không light riêng từng cơ quan | Chưa profiler |

Không dùng điểm tổng giả chính xác. Thời gian dài không tự đồng nghĩa hay; nếu nhiều chạm nhầm/kẹt góc thì sửa điều khiển/hình học trước khi giảm câu đố.

## 8. Chơi thử và hồi quy

**Tất cả đang chờ triển khai**, chưa chạy Unity/thiết bị cho màn này.

- Ca riêng: Nhả B trước khi bạn lên bệ; nhả A trong khi thang chạy; thang hạ an toàn và không kẹp hạt.
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

29/09/2026 — **bản dựng**: Khay thang 20×20 cm (đủ cho 75% đứng trọn). Bốn phần 25%: A1+A2 rút chốt cần B; phần ba giữ B (cần phải giữ) cấp lực cho thang; phần tư đi thang lên bệ cao, kéo C (móc trung tính) giữ lực thang cho cả nhóm; ba phần còn lại gọi thang, lên, tụ. Khác hồ sơ: Khay thang đã ở bến dưới (không có bước “đưa khay vào bến” riêng); B là cần giữ cấp lực, không kéo tải bằng sức 25%. Bằng chứng: `COgheSpatialCampaignTests.Spatial27Solve` (PlayMode 120 Hz, chạm thật, 32/32 hạt qua lỗ cuối, 1 cơ thể, Retry về đầu); replay native Mac cùng kịch bản; ảnh `Docs/Verification/COgheSpatialNext20/27-*.png`. Chưa kiểm: OPPO/FPS điện thoại, người chơi mới, từng ca phục hồi và đường thay thế ở mục 8 (mới có đường giải chính).
