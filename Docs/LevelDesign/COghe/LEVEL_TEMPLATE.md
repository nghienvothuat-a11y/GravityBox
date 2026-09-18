# COghe — Hồ sơ level `<LevelId>` — `<Tên hiển thị>`

Sao chép mẫu này vào hồ sơ của level; điền `<…>`, dùng `Không áp dụng — <lý do>` khi cần. Ô chưa có bằng chứng giữ là **chưa kiểm chứng**. Hồ sơ thiết kế không tự chứng nhận scene hiện có đã đạt.

Đích dùng mẫu: `Docs/LevelDesign/COghe/LevelNN/README.md`. Khi sao chép vào thư mục con này, tăng một cấp `../` cho từng liên kết nguồn ở dòng dưới (ví dụ `../../COGHE_LEVEL_DESIGN_RULES.md` thành `../../../COGHE_LEVEL_DESIGN_RULES.md`, `../../../README.md` thành `../../../../README.md`), rồi kiểm tra lại các liên kết. Không ghi đè hồ sơ đã có.

Nguồn chuẩn: [quy trình và quy tắc level](../../COGHE_LEVEL_DESIGN_RULES.md), [luật hiện hành trong README](../../../README.md), [kiến trúc](../../VENOM_LEVEL_ARCHITECTURE.md), [Day Lab](../../ArtDirection/COghe/STYLE_RULES.md), [hiệu năng mobile](../../COGHE_MOBILE_PERFORMANCE.md).

## 1. Định danh, phạm vi và trạng thái

- LevelId ổn định / tên hiển thị / vị trí trong catalog: `<…>` / `<…>` / `<…>`.
- Phiên bản hồ sơ / nội dung / RuleSet hoặc profile dùng chung: `<…>` / `<…>` / `<…>`.
- Scene, definition, builder/prefab và cơ quan liên quan: `<đường dẫn hoặc GUID>`.
- Người thực hiện / ngày cập nhật / commit hoặc mã artifact: `<…>` / `<…>` / `<…>`.
- Phác thảo, quyết định và phạm vi đã được người dùng chốt: `<link + nội dung cụ thể>`.
- Trạng thái: `<Nháp | Thiết kế đã chốt | Prototype | Đang kiểm chứng | Đạt>`; ghi riêng mức kiểm chứng Editor/Mac/thiết bị thật.
- Giới hạn còn mở / việc chưa kiểm chứng / sai khác so với bản đã chốt: `<…>`.

| Mốc | Điều kiện chuyển bước | Trạng thái và bằng chứng |
| --- | --- | --- |
| Thiết kế | Mục tiêu, phác thảo, luật và đường giải đủ rõ để dựng | `<…>` |
| Prototype | Chơi trọn bằng điều khiển thật; có cách phục hồi các sai lầm đã liệt kê | `<…>` |
| Hoàn thiện | Cơ quan dùng chung, Day Lab, camera và phản hồi đọc được | `<…>` |
| Nghiệm thu | Gameplay, hồi quy, thiết bị mục tiêu và lưu/chuyển màn có bằng chứng | `<…>` |

Các mốc trên là điều kiện chất lượng; tiếp tục công việc trong phạm vi đã được cho phép, không xin chốt lại quyết định còn hiệu lực. Nếu mới có bằng chứng prototype/Editor, ghi đúng mức đó.

## 2. Mục tiêu trải nghiệm và vị trí trong tiến trình

- Một ý tưởng chính người chơi cần nhận ra: `<…>`.
- Kỹ năng đã biết / kỹ năng mới hoặc tổ hợp mới: `<…>` / `<…>`.
- Khoảnh khắc khám phá và tín hiệu khiến người chơi hiểu: `<…>`.
- Vai trò: `<giới thiệu | luyện tập | kết hợp | Boss | nghỉ nhịp>`; quan hệ với màn trước/sau: `<…>`.
- Chính sách hướng dẫn: `<…>`; Boss tự khám phá, không tiết lộ lời giải. Boss 10 giữ `Hints=None`.
- Kiến thức ghi nhận từ hành động thật / quyền mở khi thắng / chỉ cấp lần đầu: `<…>`.
- Xác nhận giải đố thuần: không cần nâng chỉ số, mua đồ, buff nhà hoặc chăm sóc để giải/mở màn: `<…>`.

## 3. Phác thảo, hình học, camera và thao tác

- Phác thảo có chú thích và phiên bản: `<link>`; tỷ lệ/kích thước quan trọng: `<…>`.
- Đánh dấu spawn, lỗ cuối, đường giải, khoang, lối nối, mặt bám/trơn, dao, nút và vật động: `<…>`.
- Khoảng hở theo cơ thể, mép chuyển mặt, vùng quay đầu và vùng tụ an toàn: `<…>`.
- Quyền xoay hộp: `<cho phép | khóa>`; lý do, biểu tượng và hành vi kéo khi khóa: `<…>`.
- Camera đầu / toàn cảnh / theo phần đang chọn / nút xem khoang: `<…>`.
- Tỷ lệ màn hình, safe area, HUD và mục tiêu chạm nhỏ/che khuất cần thử: `<…>`.
- Bảng điểm chạm → ý định → bộ thực thi; cách chọn phần/đổi lệnh/hủy: `<…>`.
- Kiểm tra nóc và vùng trơn nhận chạm khi là đích hợp lệ trong chế độ hiện tại; vùng trơn không phát lực bám. Trong chế độ chỉ đi trong ống, điểm ngoài ống không thay lệnh. Camera không đổi trạng thái puzzle: `<…>`.

## 4. Lời giải, kết thúc và phục hồi

| Bước | Lệnh người chơi | Trạng thái trước → sau | Tín hiệu nhìn thấy | Nếu bỏ dở/làm ngược |
| --- | --- | --- | --- | --- |
| 1 | `<…>` | `<…>` | `<…>` | `<…>` |
| 2 | `<…>` | `<…>` | `<…>` | `<…>` |
| 3 | `<…>` | `<…>` | `<…>` | `<…>` |

- Điều kiện thắng: toàn bộ vật chất hợp thành một cơ thể **trong hộp trước lần ra đầu tiên**, rồi toàn bộ đi qua đúng lỗ cuối; bằng chứng: `<…>`.
- Điều kiện thua riêng: `<…>`; ra trước khi nhập hết hiện đúng “bạn phải hợp thể trước khi chui ra”; nhập ngoài không cứu thua.
- Phân biệt cổng `Transfer` và `FinalExit`, cơ quan chốt/mở tạm, lần chốt Won/Lost: `<…>`.
- Đường giải thay thế hợp lệ về vật lý và cách kiểm chứng: `<…>`.
- Tình huống phục hồi: vật sát kính/góc, rơi hụt, cắt lệch, tụ quá sớm, cửa đóng, phần bị bỏ lại: `<đường về hoặc thất bại có chủ đích>`.
- Softlock cần loại bỏ và đường thử lại tại chỗ / retry: `<…>`.
- Khi đổi lệnh, pause, retry hoặc tải lại: lực/joint/lệnh/callback nào phải hủy; dữ liệu nào được giữ: `<…>`.
- Tách chỉ qua dao; đủ gần và không vật cản thì tự tụ, kể cả khác đích/giữ nút; không cooldown sau cắt. Cách bố trí đáp ứng luật: `<…>`.

## 5. Cơ quan, trạng thái và kiến trúc dùng lại

| MechanismId / loại dùng chung | Điểm tác động, lực/hành trình | Trạng thái và sự kiện | Phụ thuộc → tác động | Reset |
| --- | --- | --- | --- | --- |
| `<…>` | `<…>` | `<…>` | `<…>` | `<…>` |

- Sơ đồ liên kết cơ quan, kể cả chu kỳ hoặc điều kiện cần nhiều phần: `<link hoặc sơ đồ>`.
- Phần mở rộng dùng lại được / dữ liệu authoring mới / lý do chưa thể tái sử dụng: `<…>`.
- Geometry động nào làm đổi route; vùng cần cập nhật và sự kiện kích hoạt: `<…>`.
- Cách giữ ID vật chất, khối lượng, lực hữu hạn và nhiệm vụ khi cắt/tụ: `<…>`.
- Kiểm tra không thêm nhánh theo số màn để đổi lực/luật thắng; runtime không đọc lời giải mẫu: `<…>`.

## 6. Hình ảnh, animation và phản hồi

- Tham chiếu Day Lab Unity màn 07, vật liệu và biến thể đã dùng: `<…>`.
- Mỗi hành động: nhận ý định → tiếp cận → tác động → kết quả thật; tín hiệu khi không thể thực hiện: `<…>`.
- Animation/VFX đọc tiếp xúc và trạng thái thật; chỗ có thể che cơ quan hoặc sai lệch hình học: `<…>`.
- Dấu chọn phần, trạng thái nút/cửa, quan hệ nguyên nhân–kết quả và âm thanh: `<…>`.
- Khoảnh khắc thắng/Boss và luồng sau thắng: `<…>`; ảnh/video trên khung hình điện thoại: `<link>`.

## 7. Độ khó và ngân sách runtime

| Độ khó với người chơi | Dự kiến | Kết quả chơi thử |
| --- | --- | --- |
| Quyết định, thứ tự, số nhiệm vụ/phần đồng thời | `<…>` | `<…>` |
| Độ chính xác chạm/thời điểm, quan sát, mức dễ sửa sai | `<…>` | `<…>` |
| Thời gian suy luận/thao tác, retry, chạm nhầm, nơi không tiến triển | `<…>` | `<…>` |

| Chi phí thiết bị | Dự kiến/ngân sách và căn cứ | Thực đo |
| --- | --- | --- |
| Vật lý, contact, khớp, cơ quan động đồng thời | `<…>` | `<…>` |
| Route/raycast, skin/animation và cấp phát/GC | `<…>` | `<…>` |
| GPU, kính trong suốt, bóng/VFX và bộ nhớ | `<…>` | `<…>` |

Không tự điền trần số cơ quan/hạt hoặc tuyên bố đạt FPS. Ghi mục tiêu và cấu hình đã chốt; tối ưu trình bày không được đổi lực, luật hay khả giải theo thiết bị.

## 8. Chơi thử và hồi quy

| Ca kiểm chứng | Thiết bị/build | Kết quả + artifact | Vấn đề / hành động tiếp |
| --- | --- | --- | --- |
| Đường giải chính và thay thế bằng lệnh chơi thật | `<…>` | `<…>` | `<…>` |
| Người chưa biết lời giải; lần đầu và replay có kỹ năng | `<…>` | `<…>` | `<…>` |
| Đường phục hồi, hủy lệnh, pause, retry, tải luân phiên | `<…>` | `<…>` | `<…>` |
| Tách/tụ, bảo toàn mô, transfer, thoát chưa nhập/cùng tick | `<…>` | `<…>` | `<…>` |
| Chạm, camera, vùng trơn và cơ quan ở nhiều hướng tiếp cận | `<…>` | `<…>` | `<…>` |
| Các màn dùng chung cơ quan/runtime vừa sửa | `<…>` | `<…>` | `<…>` |

Ghi lệnh test và phạm vi thực chạy: `<…>`. Không dịch chuyển trực tiếp sinh vật để thay bằng chứng khả giải; test logic không thay chơi tay/đọc hình trên điện thoại.

## 9. Bằng chứng hiệu năng trên thiết bị

- Máy/SoC/OS, màn hình và refresh rate: `<…>`; build ID, commit, diff chưa commit (nếu có), SHA APK/binary, chế độ build/profiler: `<…>`.
- Cấu hình đồ họa, vật lý, số hạt, frame cap và instrumentation: `<…>`.
- Kịch bản đo, thời gian warm-up, thời lượng lấy mẫu, số lượt và artifact/raw log: `<…>`.
- Đoạn nặng nhất: `<…>`; chạy kéo dài để quan sát nhiệt/throttling: `<thời lượng + kết quả>`.
- Frame time p50/p95/p99/max (ms), số/tần suất spike vượt ngân sách, CPU/GPU và GC/memory: `<…>`.
- So sánh baseline cùng máy/cấu hình/kịch bản: `<artifact + chênh lệch + nguyên nhân>`.
- Giới hạn phép đo, phần chưa được phủ và kết luận đạt/chưa đạt từng mục tiêu: `<…>`.

## 10. Tích hợp, tương thích và nghiệm thu

- Catalog/build list, thứ tự mở, ID giữ ổn định khi đổi vị trí, đường tới màn kế có thật: `<…>`.
- Save schema/phiên bản nội dung, tương thích save cũ, migration lặp an toàn hoặc không cần đổi: `<…>`.
- Thắng/thua/replay/tắt app giữa lưu–ăn mừng–mở khóa; không cấp trùng phần thưởng: `<bằng chứng>`.
- Thay đổi ngoài level và hồi quy liên quan / cách quay về bản trước khi cần: `<…>`.
- Kết luận: `<prototype | đạt phần nào | nghiệm thu>`; bằng chứng quyết định: `<…>`.
- Vấn đề còn lại và việc tiếp theo: `<…>`; lịch sử thay đổi ảnh hưởng luật/đường giải/performance: `<…>`.
