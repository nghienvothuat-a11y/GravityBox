# Bàn giao Spatial 11–30 — NewGraphic

Ngày **29/09/2026**. Mục tiêu: tiếp tục dựng trên máy khác từ cùng source, cùng
phong cách và cùng luật. **Chốt phong cách / ràng buộc; bàn giao kế hoạch chi tiết.**
20 màn mới vẫn ở trạng thái `design-only`: chưa có scene, chưa chứng minh khả giải
bằng mô phỏng hoặc đo trên OPPO. Không đổi trạng thái thành Đạt vì đã push tài liệu.

## 1. Điểm bắt đầu và thứ tự tham chiếu

Nhánh chính xác: **`NewGraphic`** tại `nghienvothuat-a11y/GravityBox`.
Không tạo nhánh khác chỉ khác hoa/thường. Mở repo bằng **Unity 6000.3.19f1**
(theo `ProjectSettings/ProjectVersion.txt`), dùng package lock đã commit.
Không nâng Unity/package trong cùng đợt dựng level. Mac cần module macOS build;
Android cần Android Build Support cùng SDK/NDK/OpenJDK của bản Unity này.

Đọc theo thứ tự:

1. [Luật thiết kế và quy trình chín bước](../../../COGHE_LEVEL_DESIGN_RULES.md).
2. [STYLE_RULES v2.0 — Glass C / Spatial](../../../ArtDirection/COghe/STYLE_RULES.md).
3. [PLAN — tiến trình, kiến trúc, giai đoạn và test](PLAN.md).
4. [MECHANICS — hợp đồng trạng thái, khối lượng, lực và phục hồi](MECHANICS.md).
5. [Danh mục 20 hồ sơ](README.md), hồ sơ màn đang dựng và `levels.json`.
6. [Gallery](review.html), `Illustrations/NN.png` và `Routes/NN.svg`.

Yêu cầu mới nhất của người dùng đứng trước tài liệu. Với Spatial, quy tắc phạm vi
hiện hành ở đầu STYLE_RULES và hợp đồng Q đứng trước đề xuất Blender/V2 opaque/dao
lịch sử. Nếu phối cảnh vẽ sai số khay, che đường hoặc trông có đường tắt, dùng hồ sơ
và hợp đồng làm chuẩn rồi ghi sửa minh hoạ; không tự thay luật để khớp một chi tiết ảnh.
[ILLUSTRATION_QA](ILLUSTRATION_QA.md) ghi giới hạn của các ảnh.

## 2. Trong Git đã có gì

| Thành phần | Đường dẫn tính từ gốc repo | Trạng thái |
| --- | --- | --- |
| Nền 10 màn chạy được | `Assets/_Game/Venom/SpatialCampaign/` | Scene, definitions, mesh, material, `.meta` đã lưu |
| Builder nền | `Assets/_Game/Editor/COgheSpatialCampaignBuilder.cs` | Generate 10 levels / Build Mac test |
| Art chung | `Assets/_Game/Editor/COgheSpatialArtBuilder.cs` | Mạch in, ray satin, vỏ ngà; giữ physics |
| Kế hoạch mới | `Docs/LevelDesign/COghe/SpatialNext20/` | 20 hồ sơ, 20 ảnh PNG, 20 sơ đồ SVG, dữ liệu/prompt/kiểm tra |
| Bằng chứng 10 màn | `Docs/Verification/COgheSpatialCircuits/` | 41/41 PlayMode, 10/10 native Mac của lượt trước; chưa đo OPPO |
| Bằng chứng trước art pass | `Docs/Verification/COgheSpatialPilot/` | Giữ để so sánh, không thay số đo mới |

Gallery dùng đường dẫn tương đối và dữ liệu có sẵn trong HTML, không cần dịch vụ
web, tài khoản ảnh hay Downloads của tác giả. `generation-manifest.json` giữ đường
dẫn nguồn máy tác giả để truy vết; ảnh bàn giao thật nằm ở `Illustrations/`, có SHA-256.
Builds, Library, Temp và log cục bộ không thuộc gói source; máy mới tự import/build.

Mở `Assets/_Game/Venom/SpatialCampaign/COgheSpatial01.unity` và Play để kiểm tra nền.
**Gravity Box → COghe → Spatial pilot → Build Mac test** xuất
`Builds/SpatialLab/macOS/COghe.app`. Có thể chơi các scene đã lưu ngay, không cần
Generate lại. Generate sẽ dựng lại 10 scene: lưu chỉnh sửa tay trước khi dùng.
Art pass riêng là **Spatial pilot → Refine circuits and verify physics**.

Menu Android/Mac V2 thông thường và script `build-venom…` chưa phải entry point
build Spatial 30 màn. Khi triển khai phải mở rộng catalog/selector/build Spatial
và kiểm tra APK thực mở đúng scene; không chỉ thêm scene vào EditorBuildSettings.

## 3. Ràng buộc đã chốt để triển khai

| Phạm vi | Phải giữ | Bằng chứng cần có |
| --- | --- | --- |
| Sản phẩm | Giải đố thuần; không tăng chỉ số, không đồ Nhà tăng khả năng qua màn | Mọi lời giải dùng kỹ năng đã học và khối lượng trong màn |
| Điều khiển | Hộp đứng yên; kéo đổi camera, pinch zoom; chạm để giao việc | Chạm được mọi đích từ góc cho phép; camera không thay trọng lực |
| Tiến trình | Xen sàn, bậc, vách, khoảng không; giới thiệu → luyện → kết hợp | Người mới hiểu cơ quan trước khi phối hợp; 11/21 nghỉ nhịp sau Boss |
| Độ khó | Khó ở thứ tự, đường nối và phân công, không ở chạm nhỏ/căn nhả dây | Bến và tay cầm rộng, kéo ngược được, thử sai có đường phục hồi |
| Cơ quan | Lực/khớp/tải/trạng thái thật; phân biệt Hold, Latch và AND | Thử buông, đảo hướng, đổi lệnh, tải lệch, pause, Retry |
| Không gian | Sinh vật bò tường/nóc được; chiều cao đơn thuần không chặn nó | Rà đường tắt qua sáu mặt; không tường vô hình, không khóa theo số màn |
| Mặt trơn | Vẫn nhận chạm; không có lực bò/bám, còn trọng lực/quán tính | Trượt/rơi nhất quán; có thể cứu và đi lại |
| Q | Một phần vào đủ → hai phần bằng 50% khối lượng phần đó; ra rồi vào lại mới tách tiếp | Bảo toàn ID/mô/khối lượng; hai khay có vách thật, không tách mỗi frame |
| Hợp thể | Đủ gần, không vách ngăn thì nhập cả khi giữ nút/khác đích | Không cooldown, không khóa tụ theo nhiệm vụ; chốt giải phóng người giữ trước khi nhập |
| Thoát | Tất cả hợp thành một cơ thể trong hộp trước lần thoát đầu tiên | Ra sớm thua: `bạn phải hợp thể trước khi chui ra`; Transfer không phải FinalExit |
| Boss | 20/30 kết hợp kỹ năng đã biết, không hướng dẫn lời giải trong game | Vẫn rõ tín hiệu input/trạng thái; tự giải được bằng cơ quan thật |
| Hình ảnh | Glass C, biển nhựa dán kính, circuit xanh A/coral B, ray dịu | So ảnh portrait thực với bộ SpatialCircuits; màu đi cùng chữ/glyph |
| Performance | Baseline 32 hạt tổng, 120 Hz; chia phần không nhân bản 32 hạt/phần | Đo tải thật, không giảm physics theo máy để đổi khả giải |

Số phần hữu ích của lời giải lên tới **4** ở các màn 24/27/30; đây là phạm vi của
20 thiết kế, **không phải hard cap toàn game đã được người dùng duyệt**. Q phải
chia đúng 50/50 cả khi một nhóm đã nhập lại; không làm tròn sai khối lượng vì thiếu
độ phân giải. Ngưỡng nhỏ nhất, cách báo không đủ độ phân giải, lực chấp nhận tải,
dung sai lắp ghép và kích thước đích chạm còn phải chốt bằng prototype. Tỷ lệ ≥2D
cho đường và ≥3D cho khay trong PLAN chỉ là điểm bắt đầu, không phải thông số đã đo.

## 4. Kiến trúc và thứ tự xây

Giữ bốn phần độc lập: **simulation cơ thể → cơ quan → navigation/input → presentation**.
Art đọc trạng thái, không sở hữu collider, điều kiện thắng hay đường giải. Nét mạch
chỉ là tín hiệu; dây ròng rọc thực vẫn truyền lực. Đồ động đi theo rigidbody thật.
Vùng ảnh hưởng navigation phải cập nhật khi cửa/van/khối đổi hình học, không rebuild
toàn scene mỗi frame. Không cài logic `if level == ...` để ép lực hoặc thắng.

| Phần | Dùng lại | Công việc còn thiếu |
| --- | --- | --- |
| Đẩy/kéo, ray | `COgheTapRail`, `COgheRailSlider`, `VenomMovableProp` | Hốc/ô chờ/chốt lắp khối, threshold lực đã đo |
| Ròng rọc, thang | `COghePulleyDrive`, `COghePassengerLift` | Tải props, đối trọng, gọi về hai bến theo từng hồ sơ |
| Ống | `COgheTubeNetwork` | Van tuyến, interlock chống kẹp, bến chờ/quay lại |
| Máy Q | Solver tách/tụ và định danh mô hiện có | `COgheQuantumSplitter` là đề xuất mới, chưa có component |
| Đu dây | Tiếp xúc, lực bám, constraint hữu hạn | `COgheSwingTransfer` là đề xuất mới; phải chứng minh cung đu và chuyển bám thật |
| Catalog | Builder/definition/selector Spatial | ID mới `coghe.spatial.next.11`…`.30`, scene và build list riêng |

Làm Q và đu dây thành hai thử nghiệm cơ quan trước khi nhân ra 20 scene. Q phải
chứng minh 100→50/50, 50→25/25, tụ lại, đầu ra bị chặn, hủy/reset. Đu dây phải chứng
minh chọn bến rộng, không nhả theo frame, hụt thì quay về/sàn cứu hộ, không teleport.
Sau đó dựng theo nhóm trong PLAN: 11–15, 16–20, 21–25, 26–30; mỗi nhóm chơi trọn bằng
input thật rồi mới hoàn thiện art. Boss dành lượt review đường giải và phục hồi riêng.

Giữ ID/save của Spatial Pilot và V2/Origin. Khi nối 01–30, mở rộng scene sequence
cùng selector, build và đường Next; không đánh số lại ID cũ. `levels.json` là dữ liệu
authoring và lời giải kiểm thử, **không phải script để runtime tự giải thay người chơi**.

## 5. Kiểm tra trước khi bàn giao bản chơi

Từ gốc repo, kiểm tra tài liệu:

```sh
python3 Docs/LevelDesign/COghe/SpatialNext20/validate_design.py
```

Lệnh xác minh 20 ID, ảnh và liên kết, các mục hồ sơ, Boss và chuỗi khối lượng/mốc
trong tài liệu. Nó không mô phỏng va chạm, sức bám, softlock hay FPS.

Với nền Spatial hiện có, Unity Test Runner → PlayMode: chạy
`COgheSpatialCampaignTests`, `COgheSpatialRecoveryTests`, `COgheViewCampaignTests`.
Ví dụ chạy từ shell trên máy đã có lệnh Unity đúng bản, khi project không mở Editor:

```sh
Unity -batchmode -projectPath . -runTests -testPlatform PlayMode -testFilter 'COgheSpatialCampaignTests;COgheSpatialRecoveryTests;COgheViewCampaignTests' -testResults spatial-tests.xml -logFile spatial-tests.log
```

Các con số 41/41 đã lưu thuộc đúng filter trong báo cáo cũ; không dùng chúng để
tuyên bố mọi test mới hoặc 20 màn mới đã chạy. Bổ sung test Q/dây/van/tải theo hợp
đồng rồi chạy hồi quy tất cả màn dùng chung component vừa đổi.

Với từng màn: đường giải chính và thay thế, làm sai rồi phục hồi, tách/tụ sớm,
đi ngược, hủy lệnh/pause/retry, tất cả hạt thoát đúng lỗ, chọn target từ các góc,
ảnh so concept và chạy người mới. Không gán cửa mở, teleport mô hoặc ép Won để
chứng minh qua màn. Ghi commit/build/hash và artifact vào hồ sơ theo
[LEVEL_TEMPLATE](../LEVEL_TEMPLATE.md).

Đo OPPO cùng cấu hình với baseline; ít nhất 3 lượt có warm-up và chơi dài khoảng
20 phút, ghi thời gian thực, p50/p95/p99/max, spike, CPU/GPU, GC và memory. Mục tiêu
ban đầu 60 FPS, p95≤20 ms/p99≤33.3 ms trong PLAN là **mục tiêu cần đo**, không phải
cam kết đã đạt. Ưu tiên xử lý query/graph, contact và overdraw; không đổi luật vật lý
để lấy số đẹp. Mac ngắn không thay cho thử nhiệt hoặc khả năng đọc/chạm trên OPPO.

## 6. Hoàn tất một màn

- Hồ sơ và minh hoạ phản ánh bố cục cuối, ghi khác biệt và lý do.
- Lời giải, sửa sai, input, art và performance có trạng thái/bằng chứng riêng.
- Scene/definition/mesh/material/prefab cần thiết cùng `.meta` đã vào Git, không
  tham chiếu asset ngoài repo hoặc chỉ tồn tại trong Library.
- Mở được từ selector, Next, replay và clean checkout; save cũ không mất.
- README/catalog/ảnh kiểm chứng cùng phiên bản build; chưa đạt mục nào ghi rõ mục đó.

Đợt bàn giao này lưu source 10 Spatial và kế hoạch/ảnh 11–30. Không kèm APK hoặc
binary Mac mới, không tuyên bố 30 Spatial đã xây xong.
