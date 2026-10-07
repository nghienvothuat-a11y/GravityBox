# Chương 5 dựng lại: bánh răng (06/10/2026)

Theo `PLANS/COGHE_LEVEL_HOOK_PLAN.md` mục 5.5. Mrk (06/10/2026, 14:06): "Làm tiếp Chương 5 theo kế hoạch". Đây là phần 1.

## Thứ tự hiện tại (sau phần 3: chương 5 xong theo kế hoạch)

| Vị trí | Nội dung (key) | Tên | Ghi chú |
| --- | --- | --- | --- |
| 41 | `N41` | Bánh răng đầu tiên | **Mới** (phần 2): gộp E08 và E12 |
| 42 | `N42` | Hai xe chéo nhau | **Mới** (phần 2) |
| 43 | `N43` | Ống theo hộp số | **Mới** (phần 3) |
| 44 | `N44` | Bánh đệm đổi chiều | **Mới** (phần 1) |
| 45 | `N45` | Ai chạy máy | **Mới** (phần 1, E15 sửa) |
| 46 | `N46` | Bàn xoay chở hàng | **Mới** (phần 3) |
| 47 | `N47` | Mượn bánh | **Mới** (phần 2) |
| 48 | `N48` | Hai tầng trục | **Mới** (phần 3) |
| 49 | `N49` | Hai động cơ một cửa | **Mới** (phần 2) |
| 50 | `N50` | Hộp số (Boss) | **Mới** (phần 2) |

Bỏ khỏi thứ tự chơi:
- `E13` (Hai ống, hai nửa: nhiều chạm, gần như không có quyết định), `E15` (thay bằng N45).
- Phần 2: `E08` và `E12` (gộp thành N41), `E14` (Hai tầng răng), `E18` (Ba lớp răng), `B2` (Boss cũ Tháp bánh răng).
- Phần 3: `28` (Đường ống ba chiều: ba tầng ống, van bằng tay gạt), `E16` (Bàn xoay), `E17` (Hai máy nối nhau).

## Cơ quan mới (`COgheGearTrain`, bật theo cờ, màn cũ không đổi)

- **Thanh răng hai chiều** (`Reversible`, `ForwardSign`):
  - Thanh răng chạy theo chiều quay của bánh cuối.
  - Ba bánh nối nhau và bốn bánh nối nhau thì bánh cuối quay ngược nhau. Vì vậy thêm một bánh đệm là đổi chiều.
  - Nhiều đoàn bánh có thể dùng chung một thanh răng; chỉ đoàn đang khớp mới đẩy nó. Hai đoàn cùng khớp thì đẩy ngược nhau:
    kẹt.
- **Lực máy theo cân nặng** (`ForcePerLoad`, `RackSpring`):
  - Lực kéo của máy bằng hệ số × cân nặng người đứng nút. Nút cân nguyên phần; nhiều nút thì cộng lực, dùng cho "Hai động
    cơ một cửa" sau này.
  - Một lò xo kéo thanh răng về, nên người lái nhẹ chỉ nâng được nửa chừng.
  - Lúc kẹt, máy kéo hết lực. Hệ số cũ (8 × sai lệch tốc độ) giới hạn lực quanh 1 N, nên với cờ này hệ số là 40.
- **Đồng hồ trên nút máy** (`COgheTissueSensor.GaugeFull`): nút máy cần bất kỳ phần nào để chạy, nhưng đồng hồ 4 ô cho thấy
  người lái nặng bao nhiêu phần tư.

## Màn mới

| Màn | Lời giải | Điểm học |
| --- | --- | --- |
| 44 · Bánh đệm đổi chiều | Bánh A (thẳng, đã lắp sẵn): ba bánh, đứng nút P thì bậc thoát chạy ngược vào trong. Rút A ra, đẩy cặp bánh đệm B vào (hai bánh xếp zigzag): bốn bánh, đứng P thì bậc chạy ra. Leo ra. Lắp cả A lẫn B thì kẹt | Thêm một bánh là đổi chiều |
| 45 · Ai chạy máy | Tách. Nửa lái, nửa đi thang: thang dừng giữa chừng. Người lái bước xuống thì thang hạ. Người đi thang tách tiếp: 1/4 đi thang, 1/4 nhập vào người lái thành 75%: thang lên tới đỉnh. 1/4 kéo C nâng bậc cầu thang, người lái leo lên | Người nặng chạy máy, người nhẹ đi thang |

Số đo trong game, màn 45, vị trí thang (đỉnh 0,168):
- Nửa lái nửa: 0,073.
- 75% lái 25%: 0,166.
- Nửa lái 1/4 (tính toán): khoảng 0,11, không tới đỉnh.

## Lỗi gặp khi dựng và cách sửa

- **Bàn bánh răng cao 3 cm, trơn:** đường đi tính nó là một bậc leo được, nên COghe đi thẳng qua bàn và bị bánh răng vướng.
  Màn 44 nâng bàn lên 5 cm, đường đi vòng.
- **Tay nắm đặt sau bàn (B, màn 44):** chỗ đứng mặc định là phía trước tay nắm, tức là trên bàn. B được đẩy từ phía sau khối.
- **Biển số che hàng bánh răng (44):** dời sang phải (`MoveChapterThreePlaques`).
- **1/4 thân không leo được mép khay thang 3 cm** (như màn 34): khay thang màn 45 mỏng 1,2 cm, mép màu ngà. `PlusE15` nhận độ
  dày khay; màn 15 cũ giữ 3 cm.

## Kiểm tra

- Test giải và test đi lang thang của N44, N45: đạt.
- Lời giải mẫu màn 45 kiểm luôn rằng nửa lái nửa thì thang không tới đỉnh.
- Các màn bánh răng cũ (E08, E12, E14–E18, B2), Product UI và Spatial recovery: đạt sau thay đổi `COgheGearTrain`.
- Clip: `Artifacts/Clips/ch5/chuong5a.mp4`.

## Phần 2 (06/10/2026): 41, 42, 47, 49, Boss 50

### Cơ quan thêm (`COgheGearTrain`, bật theo trường, màn cũ không đổi)

- **Đèn khớp** (`MeshLamp`, `MeshLampOn/Off`): đèn tối cho tới khi mọi cặp bánh của đoàn khớp nhau.
- **Lò xo có lực nén sẵn** (`RackPreload`): cộng thêm một lực kéo về cố định vào `RackSpring`. Người lái nhẹ không nhấc nổi
  tải; người lái vừa thì tải dừng giữa chừng.
- **Nhiều đoàn chung bánh** (`SharedWheels`): mỗi đoàn có bản sao ẩn của bánh dùng chung; bánh nhìn thấy quay theo đoàn đang
  khớp và đang chạy. Dùng ở Boss: hai đầu ra × (bánh thẳng hoặc bánh đệm) = bốn đoàn chung một mô tơ.
- Vòng khe trống (`PlusSlotRing`) và đèn khớp (`PlusMeshLamp`) là phần hình trong builder.

### Màn mới

| Màn | Lời giải | Điểm học |
| --- | --- | --- |
| 41 · Bánh răng đầu tiên | Đứng P: chỉ bánh mô tơ quay, đèn tối, khe là một vòng trống. Kéo A: bánh G vào vòng, đèn sáng. Đứng P: bậc thoát chạy ra | Bánh phải khớp liền nhau mới truyền lực |
| 42 · Hai xe chéo nhau | Bánh nhỏ của xe A phải đi qua khe của xe B để tới khe mình. B đang nằm trong khe nên A từ chối. Rút B ra, đẩy A vào, đẩy B vào lại, đứng P | Thứ tự kiểu màn 12 bằng xe bánh răng |
| 47 · Mượn bánh | Chỉ có một bánh G, đang ở máy 1. Một thanh chắn ngang đường xe G. Máy 2 không có bánh (vòng trống). Đứng P1: máy 1 nâng thanh chắn, thanh chắn chốt lại. Đẩy G sang máy 2, đứng P2: bậc ra | Chốt đầu ra trước rồi mới mượn bánh |
| 49 · Hai động cơ một cửa | Hai mô tơ cùng kéo cửa ra, mỗi cái kéo bằng 2 × cân nặng người đứng nút, lực cộng lại. Nút C (chốt bánh ra) cần một phần đứng. Tách 50/50, tách tiếp một nửa: 50 + 25 trên hai mô tơ, 25 trên C | Lực hai máy cộng lại; chọn cỡ chia |
| 50 · Boss Hộp số | A đang lắp: đứng P thì cổng cầu thang (đang hé 3 cm) hạ xuống. Rút A, lắp cặp bánh đệm B: cổng nâng, chốt. Cửa ra trên sàn giữa chỉ mở theo chiều cũ: rút B, lắp lại A. Cửa nặng và có chốt C trên sàn giữa: nửa lên C, nửa lái thì cửa nhích 4,9 cm rồi tụt. Nửa trên C xuống Q tách: 1/4 lên lại C, 1/4 nhập vào người lái thành 75%: cửa mở | Lắp vào rồi phải tháo ra; phần nặng nhất chạy máy cuối |

Lực cửa (49, 50): 2 × 9,81 × cân nặng trên nút mô tơ, chống lại 0,65 N + 6 N/m × vị trí (cửa đi 10 cm).
- 25%: 0,47 N, không nhấc nổi.
- 50%: 0,94 N, đo được dừng ở 0,049.
- 75%: 1,41 N, lên tới 0,096 (đỉnh).

Bỏ bộ chọn của kế hoạch ở Boss: cầu thang và cửa đã đi theo hai chiều ngược nhau, nên chính bánh đệm là bộ chọn. Thêm một bộ
chọn cần thêm ba bánh và không vừa sàn.

### Lỗi gặp khi dựng và cách sửa

- **Cửa nhẹ quá thì mô tơ chỉ kéo được nửa sức.** Cửa 22 g: bộ điều tốc của mô tơ bật tắt xen kẽ từng bước mô phỏng, trung
  bình chỉ được nửa lực, nên cửa kẹt ở 1 cm dù có 75% lái. Cửa ở 49 và 50 nặng 0,25 kg.
- **Phần vừa tách đứng trong làn khay của Q.** Đường đi vòng Q từ đó bị kẹt (lỗi đã biết, như 40 và 45). Lời giải mẫu dẫn
  phần đó ra cạnh trái của Q trước.
- **Chỗ đứng kéo tay nắm bị kẹp** (Boss): bàn bánh răng chạy sát kính trước, nên chỗ đứng trước xe A và khe giữa xe B với nút P
  quá hẹp. A kéo từ phía sau xe, B kéo từ phía trước.
- **Biển số che bàn bánh răng** (41, 42, 47): dời sang phải (`MoveChapterThreePlaques`).
- **Chạm ngay sau lưng COghe** (47): từ khung hình này, chạm vào bệ thoát ngay sau thân thì tia chạm đi qua thân, thành chọn
  thân. Lời giải mẫu chạm lệch sang trái.

### Kiểm tra (phần 2)

- Test giải và test đi lang thang của N41, N42, N47, N49, N50: đạt.
- `SpatialPlusN49HalfStalls`: nửa thân trên C, mỗi mô tơ 1/4: cửa dừng ở 0,049.
- Lời giải mẫu kiểm luôn:
  - 41: đứng P khi khe trống thì không chạy.
  - 42: A bị từ chối khi B còn trong khe.
  - 47: G bị từ chối khi thanh chắn còn hạ; thanh chắn vẫn chốt khi G rời máy 1.
  - 50: nửa thân lái thì cửa dừng giữa chừng.

## Phần 3 (06–07/10/2026): 43, 46, 48

| Màn | Lời giải | Điểm học |
| --- | --- | --- |
| 43 · Ống theo hộp số | Mạng ống chữ Y của màn 16 (content 15). Một khoá chuyển ở ngã ba Y chỉ mở một nhánh: trái lên ban công, phải sang bến thoát. Nút P chạy hai mô tơ; bánh G trên xe nằm ở hàng nào thì hàng đó quay khoá chuyển (vòng khe xanh: nhánh xanh, vòng khe san hô: nhánh san hô). Cửa thoát trên bến phải mở bằng tay nắm C trên ban công. G và khoá chuyển bắt đầu ở bên phải, nên chuyến đầu tới cửa đóng. Lời giải: G sang trái, P, ống lên ban công, C, xuống ống, G sang phải, P, ống sang bến, ra | Đặt tuyến trước khi chui |
| 46 · Bàn xoay chở hàng | Bàn xoay sát sàn (3 cm) chở thùng A trên ray riêng dọc mặt bàn. Bệ thoát cao 9 cm, mặt trơn, bằng bàn cộng thùng. Đẩy thùng ra đầu xa, bước xuống, đứng P: bàn quay 90°, đưa thùng sát bệ làm bậc. Xoay trước thì thùng nằm đầu gần, vẫn đẩy dọc bàn ra bệ được | Bàn xoay mang theo đồ trên nó (màn nghỉ) |
| 48 · Hai tầng trục | Bố cục màn 44 cũ (sàn giữa, cầu lên bệ thoát), không còn bắt đi ngược. Tầng dưới: mô tơ, khe A, bánh cột. Đứng P khi khe A trống thì chỉ mô tơ quay. Lắp A, đứng P: tầng dưới quay một trục vít, nâng bánh C lên khe của tầng trên; cột truyền chuyển động lên, qua C tới bánh cầu, cầu nâng. Leo cầu thang, qua sàn giữa và cầu | Máy dưới đẩy bánh tầng trên vào khớp |

43 dựng mới, không sửa 28. Ba tầng ống của 28 chiếm gần hết sàn, không còn chỗ cho hộp số. 28 bỏ khỏi thứ tự chơi.

### Lỗi gặp khi dựng và cách sửa (phần 3)

- **46: chỗ đứng đẩy thùng tính theo khung của bàn xoay** (bàn đã quay 90°), nên COghe bị đưa ra cạnh thùng. `StandOffset` ghi theo khung bàn: +x của bàn là "phía sau thùng".
- **46: bàn bánh răng cách đầu bàn xoay 7 cm.** COghe bước xuống bàn thì kẹt vào khe đó. Bàn bánh răng dời ra góc trước phải.
- **43: chân ống sát sàn.** Đi ngang qua chỗ ống bắt đầu leo thì COghe trèo lên ống hoặc chui vào gầm. Hộp số và nút P đặt cùng một phía ống; dưới đoạn ống thấp có khối đỡ trơn như E13.
- **43: miệng ống dưới sàn quay vào kính trước, chỉ cách 8 cm.** COghe chui ra thì bị kẹp. Miệng lùi vào, cách kính 14 cm. Các màn khác chỉ chui vào ở miệng sàn, chưa có màn nào chui ra.
- **43: tay nắm xe G nằm sau bệ bánh răng, camera không thấy.** Núm tay nắm dựng trên cột, cao hơn bánh.
- **43: đẩy xe G từ bên trái thì COghe bị kẹp giữa xe và bệ** (chỗ đứng cách tâm xe 6 cm, lối giữa hai bệ rộng 8 cm; test
  thường qua, lúc quay clip thì kẹt). Bánh mô tơ và bánh ra lớn hơn (4 cm) trên bệ nhỏ hơn: lối rộng 10,6 cm, chỗ đứng cách
  xe 7,5 cm.

### Kiểm tra (phần 3)

- Test giải và test đi lang thang của N43, N46, N48: đạt. Chạy lại N50 sau khi đổi `SharedMotor` thành `SharedWheels`: đạt.
- Lời giải mẫu kiểm luôn:
  - 43: lúc đầu chỉ nhánh phải mở; khoá chuyển đổi nhánh theo hàng có G.
  - 48: khe A trống thì trục vít không quay.
