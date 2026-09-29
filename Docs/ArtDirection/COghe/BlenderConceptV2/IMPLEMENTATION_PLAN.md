# NewGraphic — Kế hoạch thử Blender trên V2 01–10

Ngày 28/09/2026. Nhánh local `NewGraphic`, tạo từ `Venom` tại `800e7a0`.
Concept: [ba bảng và hướng dẫn dựng hình](README.md).

## Mục tiêu và phạm vi

Dựng lại phần nhìn của **COgheView01–COgheView10** bằng asset Blender để đánh giá độ đẹp, độ rõ thao tác và chi phí trên OPPO. Đây là 10 màn V2 hiện hành, không phải 10 màn Origin cũ. V2 kéo để đổi góc nhìn, không xoay vật lý chiếc hộp. Scene 11–30 vẫn được giữ nhưng không reskin trong đợt này.

Giữ ID/save, bố cục, lỗ, điểm bám, collider, raycast, joint, lực, navigation, đường giải và chiến thắng. Không thay thiết kế màn bằng bố cục minh hoạ LAB 07. Hồ sơ từng màn hiện tại là nguồn gameplay; thêm kết quả art vào hồ sơ sau mỗi lần thực hiện, không tạo 10 puzzle khác.

Cập nhật triển khai 29/09/2026: đã tạo kit Blender, áp art cho V2 01–10, thêm A/B trong game và build thử. Kết quả thực tế, phạm vi kiểm thử và phần chưa đo ghi tại [báo cáo](../../../Verification/COgheNewGraphic/README.md). Các mục tiêu đo dưới đây vẫn là tiêu chí, không tự coi là đã đạt.

## Kiến trúc dự kiến

Tận dụng `COgheViewArtBuilder`, `COgheViewStudioBuilder`, `COgheViewPresentation` và camera V2. Không viết lại navigation hay creature solver để thay hình ảnh.

- Nguồn `.blend`: `ArtSource/COghe/NewGraphic/`, nằm ngoài Assets để Unity không tự gọi Blender khi import. Mỗi nhóm module có file nguồn, export settings và phiên bản Blender ghi kèm.
- Mesh xuất: `Assets/_Game/Venom/Art/NewGraphic/Models/`; material, texture và prefab nằm trong các thư mục riêng cùng gốc. FBX xuất rõ ràng, không phụ thuộc Blender cài trên máy build.
- `COgheGraphicProfile` **đã thêm**: liên kết prefab, shared material, LOD và thông số trình bày. Hai profile `Current` và `BlenderMobile` áp cùng scene/gameplay; không tạo hai hệ cơ quan.
- Builder art **đã thêm** gắn visual vào transform thật của cơ quan, thay renderer cũ có chọn lọc. Không xóa cả GameObject đang mang collider/input; không gọi lại generator gameplay chỉ để đổi visual. Chạy lại builder phải không nhân đôi object/material.
- Nút, nắp, cầu, tay nắm có mesh riêng theo bộ phận động. Animation/đèn đọc trạng thái thật; animation không mở cửa hoặc kéo cơ quan thay physics.
- Vỏ buồng chia từng mặt để cutaway/fade theo camera; không gộp vách vào khối không thể ẩn. Collider và mặt chọn được giữ nguyên. Vách nội bộ/nắp vẫn che thao tác đúng luật.
- COghe giữ mesh và mô phỏng hiện tại trong vòng A/B đầu. Chỉ tinh chỉnh vật liệu/ánh sáng; thử rig hoặc biến dạng mới phải là vòng riêng để biết chi phí tăng từ đâu.

Các class và đường dẫn đã tồn tại. Pipeline thực tế dùng Blender evaluated geometry → meshpack → Unity Mesh; FBX là bản thư viện bàn giao. LOD riêng chưa cần cho kit nhỏ này và chưa được thêm.

## Trình tự thực hiện và điều kiện chuyển bước

| Giai đoạn | Công việc | Đầu ra / điều kiện |
| --- | --- | --- |
| 0 — Baseline | Chụp V2 01–10; ghi hash code/APK, profile, máy; chạy lời giải và đo bản hiện tại | Có số liệu thật làm đối chứng, tách lỗi cũ khỏi lỗi art |
| 1 — Kit Blender | Dựng đế/vách, aperture, bậc, tay nắm/ray, cửa/nắp, cầu; palette chung | File nguồn, FBX, ảnh preview, kiểm kích thước/pivot/normals |
| 2 — Pilot 01, 05, 10 | Cảnh đơn giản, một cơ quan, chuỗi cơ quan; tích hợp profile art mới | Gameplay không đổi, ảnh Unity gần concept, đo OPPO trước khi nhân rộng |
| 3 — Hoàn thiện 02–04, 06–09 | Tái dùng kit; xử lý góc khuất, fade, zoom và liên kết trạng thái | Toàn bộ 10 màn chơi được; mỗi màn có ảnh và checklist riêng |
| 4 — Tối ưu và nghiệm thu | A/B cùng route, chỉnh mesh/shadow/transparent; kiểm tra nóng máy | Build Mac để review và APK thử, báo cáo hình ảnh + frame time + memory |

Mỗi vòng chỉ thay một nhóm chi phí khi đang tìm nguyên nhân: mesh/material, rồi transparency, rồi lighting. Không hạ chất lượng đồng loạt khiến không biết phần nào gây lag.

## Ma trận 10 màn

| V2 | Nội dung hiện tại | Art phải dựng | Ca kiểm tra quan trọng |
| --- | --- | --- | --- |
| [01](../../../LevelDesign/COghe/ViewV2/Level01.md) — Chạm để đi | Chạm lỗ và bò tới | Chamber/base, exit mint phẳng, palette COghe | Silhouette ở xa, chạm đúng lỗ, toàn bộ mô thoát |
| [02](../../../LevelDesign/COghe/ViewV2/Level02.md) — Chạm mặt khác | Leo mặt chuyển nghiêng/bậc | Bậc bo ngoài phần bám, mép nối rõ | Không tạo bậc/collider mới; leo và quay đầu không mắc |
| [03](../../../LevelDesign/COghe/ViewV2/Level03.md) — Kéo để nhìn | Orbit để thấy lỗ sau vách L | Vách L, panel tách mặt và cutaway | Vách thật vẫn che; đảo chiều orbit không bật/tắt giật |
| [04](../../../LevelDesign/COghe/ViewV2/Level04.md) — Nhìn gần hơn | Đi qua hai khe trong hành lang, pinch quan sát | Vách I/II, coating trơn và ký hiệu đọc được khi zoom | Không thu hẹp khe; pinch không ra lệnh bò; Overview reset |
| [05](../../../LevelDesign/COghe/ViewV2/Level05.md) — Chạm cơ quan | A đẩy con trượt mở cửa | Tay nắm amber, carriage, ray, cửa và liên kết nhìn thấy | Chạm→bám→đẩy liên tục, cửa/đèn theo trạng thái thật |
| [06](../../../LevelDesign/COghe/ViewV2/Level06.md) — Tự làm lại | Cơ quan A ở bố cục đối xứng | Tái sử dụng kit 05 theo vị trí hiện hành | Hướng tay nắm/pivot đúng, không dùng scale âm gây lỗi mặt |
| [07](../../../LevelDesign/COghe/ViewV2/Level07.md) — Đưa trở về | A mở cửa vào, trả A để mở cửa ra | Hai cửa, ray/tay nắm đọc được cả hai phía | Tiếp cận hai phía, đóng/mở không kẹp mô; không gợi hai cửa cùng mở |
| [08](../../../LevelDesign/COghe/ViewV2/Level08.md) — Bắc một nhịp | Dời cầu đến nấc, đi qua bờ phải | Cầu/deck, ray, chốt và bến ghép | Visual trùng mặt thật khi chốt; nối hai bờ không hở/khấc |
| [09](../../../LevelDesign/COghe/ViewV2/Level09.md) — Hai bước nhìn thấy | A dời nắp, B mở cửa | Nắp dày có tay nắm, cơ quan B và tín hiệu A/B | B chỉ lộ/chọn được khi nắp thực sự rời; không thấy chữ xuyên nắp |
| [10](../../../LevelDesign/COghe/ViewV2/Level10.md) — Cỗ máy nhỏ | A dời nắp, B nối cầu, C mở cửa | Bộ cơ quan đồng nhất, bố cục nhiều thành phần rõ | Chuỗi trọn màn, tải render cao, không thêm hướng dẫn lời giải Boss |

Ống, bánh răng và máng trượt trong concept là kit mở rộng. Không tự nhét các cơ quan đó vào 10 màn V2 nếu màn gốc không có; làm sau khi phần 01–10 đã đo đạt.

## Hợp đồng asset Blender → Unity

- Mét, kích thước và trục chức năng lấy từ scene; không đo collider từ tranh. Kiểm tra import bằng mẫu 1 m và hướng trục để tránh lỗi tỷ lệ 100 lần.
- Bevel 2–3 segments làm khởi điểm; tăng theo silhouette thật ở zoom gần. Model trang trí không trở thành collider.
- Giữ source modifier trong .blend, kiểm transform/normals/UV ở mesh xuất. Dựng vật liệu URP riêng; không kỳ vọng node Blender chuyển nguyên vẹn.
- 6–8 shared materials cho kit ban đầu, một atlas 1K khi cần; thông số phải chốt bằng cảnh pilot. Props nhỏ 300–1.500 triangles, cơ quan phức tạp 1.500–4.000 là ngân sách thử, không định mức bắt buộc.
- Tái dùng mesh giữa các màn. Gộp phần tĩnh theo material khi có lợi và đo lại draw calls; giữ riêng phần cần chuyển động/fade. Không gộp thành mesh quá lớn làm mất culling.
- Một shadow key, fill nhẹ, cubemap có sẵn; nền lab đơn giản. Chưa thêm SSAO, bloom, refraction, fluid simulation hoặc reflection capture realtime.
- AO chỉ nằm ở khe/lắp ghép cục bộ. Không bake bóng thế giới lên vật quay/chuyển động. Lỗ thoát giữ aperture thật và viền mint phẳng.

## Quy trình A/B performance

Thiết bị mục tiêu chính: OPPO đang dùng trong dự án (lịch sử CPH2591); xác nhận model/OS/refresh rate/resolution lúc đo, không lấy thông tin cũ làm số đo mới. Mac dùng review hình, không thay chứng nhận Android.

1. Bản thử triển khai một APK với hai profile A/B để bảo đảm cùng code gameplay/backend/quality/instrumentation. Chuyển profile bằng nút hoặc launch argument và reload màn. Giữ APK cũ làm bản lưu, nhưng không dùng nó làm phép đo A/B trực tiếp nếu instrumentation khác. Ghi SHA-256 APK và working-tree diff. Cả hai bộ asset cùng nằm trong APK: so sánh memory không thay được phép đo hai APK shipping tách asset.
2. Dùng instrumentation nhẹ trên cả hai bản cho phép so sánh; Development Build không Deep Profile dùng chẩn đoán CPU/GPU riêng, không so FPS trực tiếp với Release.
3. Mỗi màn chạy ba lượt có route: idle, bò, ra lệnh, vận hành cơ quan, orbit, pinch, thoát và retry. Tách loading/shader warmup khỏi steady state nhưng vẫn báo riêng chi phí cold start.
4. Xen kẽ A/B và để máy nguội tương đương. Ghi nguồn sạc, pin/nhiệt, ứng dụng nền và điều kiện đo. Chạy thêm 15–20 phút liên tục, ưu tiên 05/07/09/10.
5. Thu FPS thực, frame-time median/p95/p99/max, số frame >33,3/50ms, CPU main/render/GPU nếu thiết bị cung cấp, GC alloc, PSS/Unity memory, draw calls/triangles, thời gian tải. Nếu không lấy được GPU thì ghi thiếu, không suy từ FPS.
6. Benchmark Origin 11–20 cũ không được dùng để chứng nhận V2: cần route đúng `COgheView01–10`, tái dùng `COgheViewScenario` sau khi xác minh nó không can thiệp thắng/pose.

Mục tiêu thử: 60 FPS, p95≤20ms, p99≤33,3ms; báo mọi spike >50ms. Chưa có baseline nên đây không phải lời hứa đạt. Cảnh mới không tăng p95 >10% hoặc peak memory >15% so với A mà không có phân tích nguyên nhân. Nếu A vốn chậm vẫn phải báo cả hai chưa đạt, không gọi B đạt chỉ vì tương đương. Sau khi warmup, code presentation mới không cấp phát managed đều mỗi frame. Profile 30 FPS, nếu cần, phải báo riêng và giữ physics như cũ.

## Kiểm thử và bằng chứng

- Snapshot physics/navigation/input trước/sau theo `COgheViewArtVerification.CapturePhysics`; mở rộng kiểm tra reference/máy trạng thái còn thiếu. Hàm Rebuild hiện có gọi generator toàn campaign, nên không chạy trực tiếp để áp art 10 màn nếu nó ảnh hưởng 11–30; thêm phạm vi rõ cho thao tác mới.
- Chạy `COgheViewCampaignTests.View01Tap` đến `View10Boss`, feedback/input/orbit/pinch/cutaway/reset/save. Nếu sửa runtime dùng chung thì chạy cả suite V2 mở rộng và hồi quy phù hợp.
- Chơi tay màn 07/08/09/10; test qua API không thay kiểm tra chọn tay nắm bằng ngón tay.
- Chụp before/after cùng pose ở portrait 720×1280 và 720×1612, góc overview, zoom gần, orbit giữa hai mặt và chiến thắng. Kiểm tra grayscale/màn thu nhỏ để màu không là tín hiệu duy nhất.
- Kết quả dự kiến lưu `Docs/Verification/COgheNewGraphic/`, raw log trong `Artifacts/COgheNewGraphic/`; báo rõ pass/fail/chưa đo cho từng màn.
- Build output thử có tên và manifest riêng, không ghi đè APK đối chứng; APK A/B giữ catalog 30 màn để không phá lựa chọn/save; chỉ 01–10 có art mới, không xóa các scene khác hay reset save.

## Trạng thái hiện tại

- Nhánh `NewGraphic`: đã tạo tại `800e7a0`.
- Bộ concept: có đủ ba ảnh và prompt trong thư mục này.
- Blender: xem [ghi nhận cài đặt](SETUP.md).
- File .blend/.fbx, art adapter, 10 màn reskin và build Mac đã triển khai. Trạng thái APK/thiết bị và số đo được ghi trong báo cáo xác minh, không suy từ FPS trên Mac.
- Thay đổi material PhysicsLab và crash JSON có từ trước, không thuộc thử nghiệm này.
