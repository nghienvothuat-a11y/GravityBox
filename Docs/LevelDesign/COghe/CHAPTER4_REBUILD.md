# Chương 4 dựng lại: chia đúng cỡ (06/10/2026)

Theo `PLANS/COGHE_LEVEL_HOOK_PLAN.md` mục 5.4 và lời Mrk (06/10/2026): "Sửa tiếp Chương 4 và 5 sau đó thêm các level của
51-60 xen kẽ trong các chương. Xếp levels theo độ khó tăng dần". Đây là phần 1 của chương 4. Chương 5 và việc xen màn thùng
làm sau.

## Thứ tự chương 4

| Vị trí | Nội dung (key) | Tên | Ghi chú |
| --- | --- | --- | --- |
| 31 | `N31` | Cân ở cửa | **Mới.** Dạy chia hai lần (25%) và đồng hồ cân |
| 32 | `N32` | Nặng đi trước | **Mới.** Thay `23` (bản sao màn 16) |
| 33 | `N33` | Hai phần tư thành một nửa | **Mới.** Thay `E09` |
| 34 | `N34` | Bập bênh nâng bạn | **Mới.** Thay `24` (bản sao màn 17) |
| 35 | `N35` | Ba phần tư | **Mới.** Thay `25` |
| 36 | `E10` | Đu rồi luồn | Màn nghỉ, giữ |
| 37 | `E11` | Giữ thang cho bạn | Giữ; "Thang chở thùng nặng" làm sau |
| 38 | `26` | Đu và luồn | Giữ; sửa độ rõ làm sau |
| 39 | `29` | Xưởng lắp cầu | Đưa từ 45 lên làm màn chuẩn bị Boss |
| 40 | `N40` | Cân ba phần tư (Boss) | **Mới.** Thay `30` (Hộp cộng hưởng) |

Bỏ khỏi thứ tự chơi: `23`, `E09`, `24`, `25`, `27` (Bốn trạm), `30`. Cảnh và test của chúng vẫn còn.

Bánh răng dồn sang chương 5. Tạm thời `E08` (Bánh răng đầu tiên) mở chương 5 ở vị trí 41, các màn 42–50 lùi một bậc.
Chương 5 sẽ dựng lại theo mục 5.5.

## Cơ quan mới

- **Đồng hồ cân trên nút** (`COgheTissueSensor.GaugeTiles`):
  - Mỗi ô là một phần tư thân. Nút cần nửa thân có 2 ô, nút cần một phần tư có 1 ô.
  - Ô sáng khi tải trên nút đạt phần của nó. Một phần tư trên nút 2 ô chỉ sáng 1 ô.
  - Ô trống mang màu mạch nhạt, ô sáng mang màu mạch đậm.
- **Cân đúng ba phần tư** (`COgheTissueSensor.Column`, `MaxLoad`, `Overload`, `Beam`):
  - Đĩa cân chỉ nhận một khoảng tải, ở đây đúng 75%.
  - Cả thân lên thì quá nặng: ô đỏ sáng, đòn cân nghiêng về phía đĩa.
  - Nửa thân thì quá nhẹ: đòn nghiêng về phía quả đối trọng.
  - Đúng 75% thì đòn nằm ngang.
  - Chốt trục cân chỉ rút khi có một phần đứng nút B ở phòng bên.
- **Cân nguyên phần:** đĩa cân và khay thang cân bằng tính cả phần thân đang chạm vào chúng, không chỉ phần mô nằm trên đĩa.
  - Lý do: lúc đầu chỉ đếm mô trên đĩa. Khi đó 75% đứng lệch thì cân thiếu, còn cả thân đứng lệch một nửa lại lọt đúng 75%.
- **Thang cân bằng** (`COgheBalanceLift`):
  - Hai khay treo chung một đòn: bên nặng hơn đi xuống, bên nhẹ hơn đi lên.
  - Nặng ngang nhau (lệch dưới 8 g) thì đứng yên, ví dụ 25 với 25 hay 50 với 50.
  - Cả hai khay trống thì khay về chỗ cũ. Nhờ vậy khay lỡ lên rỗng sẽ xuống lại, không thành ngõ cụt.
  - Lúc khay khởi hành, phần đứng trên khay bỏ lệnh đi, giống thang chở người.

Tải đo được trên nút thường (chỉ đếm lớp mô sát nút): một phần tư 0,021–0,024; nửa thân 0,033–0,042. Vì vậy nút cần nửa
thân đặt ngưỡng 0,028 (`HalfLoad`), nằm giữa hai khoảng đó. Đĩa cân tính nguyên phần nên ra đúng: 0,024 · 0,048 · 0,072 · 0,096.

## Màn mới

| Màn | Lời giải | Điểm học |
| --- | --- | --- |
| 31 · Cân ở cửa | Tách đôi. Một nửa đứng nút 2 ô ở cửa. Nửa kia tách tiếp, mỗi phần tư đứng một nút 1 ô ở phía sau. Ba nút cùng có tải thì cửa mở luôn. Nhập lại rồi đi | Một phần tư chỉ đầy nửa đồng hồ |
| 32 · Nặng đi trước | Cả thân đẩy khối thấp B (100%) ra khỏi giao lộ. Tách: một nửa đứng nút A (rút chốt khối cao A), nửa kia đẩy A vào chỗ đỗ. Nhập lại, cả thân đẩy B về giao lộ làm bậc. Leo B, A, lên đảo | Việc nặng làm khi còn nguyên thân. Tách trước thì nửa thân gồng B không nổi, nhập lại thì sửa được |
| 33 · Hai phần tư thành một nửa | Một nửa giữ nút A, cửa mở. Nửa kia tách: hai phần tư đứng B1, B2 cùng lúc thì nắp kính trên nút C nhấc lên luôn. Một phần tư lên C chỉ sáng 1/2. Hai phần tư nhập lại trên C thì chốt trên khung cửa trượt vào, cửa đứng luôn. Người giữ A được thả, tất cả nhập lại | Nhập hai phần nhỏ để đủ cân |
| 34 · Bập bênh nâng bạn | Nửa với nửa trên hai khay thì cân, không lên. Tách nửa trên khay thang thành hai phần tư. Một phần tư lên khay, nửa thân lên khay đối trọng, thế là phần tư được nâng lên bờ cao. Nó kéo B, bậc thoát trượt ra. Nó quay lại khay, đối trọng bước xuống thì khay hạ. Nhập lại, leo ra | Bên nặng hơn mới kéo được bên kia lên |
| 35 · Ba phần tư | Một nửa đứng A1 thì rút được chốt khối B (75%). Nửa kia đẩy B thì gồng rồi buông. Người giữ quay về máy Q tách tiếp: một phần tư đứng A1, phần tư kia nhập với nửa thành 75%, đẩy B vào ổ. Nhập lại, leo B | Máy Q chỉ chia đôi; 75% = 50 + 25 |
| 40 · Boss · Cân ba phần tư | Cả thân lên đĩa thì quá nặng. Tách: một nửa giữ nút A, cửa mở. Nửa kia tách: một phần tư sang phòng bên đứng nút B (rút chốt trục cân). Nửa thân lên đĩa thì quá nhẹ. Phần tư còn lại nhập vào thành 75%: đòn nằm ngang, bậc thoát ở phòng bên trượt ra, cửa đứng luôn. Tất cả sang phòng bên, nhập lại, leo ra | Cả chương "nặng hơn là tốt", Boss đòi đúng cân |

Số quyết định trong lời giải (đếm tay: tách, đứng nút, nhập, đẩy/kéo, bước leo có ý nghĩa; tính cả lần thử sai mà màn
cố ý cho thấy) so với kế hoạch 5.4:

| Màn | 31 | 32 | 33 | 34 | 35 | 40 |
| --- | --- | --- | --- | --- | --- | --- |
| Lời giải mẫu | 5 | 7 | 8 | 8 | 7 | 9 |
| Kế hoạch | 4 | 6–7 | 7 | 6 | 7 | 10–11 |

Màn 34 cao hơn kế hoạch vì phần tư còn phải đi xuống lại. Boss 40 thấp hơn kế hoạch một–hai quyết định.

## Đặt nút tránh đường đi

Hai phần thân chạm nhau là tự nhập lại. Vì vậy nút phải giữ lâu không được nằm trên đường các phần khác đi qua.

Bản đầu của Boss 40 đặt nút A ngay trước cửa. Phần tư đi sang phòng bên chạm vào nửa thân đang giữ A, hai phần nhập thành
75%, kẹt ở phòng bên. Bản sau dùng bố cục phòng của E09: nút A ở góc trái phía trước, đường từ máy Q tới cửa không đi qua đó.

## Lỗi gặp khi dựng và cách sửa

- **Đi vào máy Q từ phía sau:** lối vào chạy lên làn khay ra và kẹt ở cổng làn, nên phải vòng ra trước miệng Q. Lời giải mẫu
  của 35 và 40 có thêm bước vòng ra trước. Người chơi chạm Q từ phía sau cũng sẽ gặp; cần sửa đường vào Q (việc riêng).
- **Lỗ thoát cắt theo lối thoát lúc dựng vỏ:** `PlusTwoRooms` nhận lối thoát làm tham số. Đổi `c.Exit` sau khi dựng vỏ thì
  tường không có lỗ.
- **Thang cân bằng dừng giữa chừng:** lệnh đi tới chỗ cũ trên khay giữ phần tư lại. Đã sửa bằng cách bỏ lệnh lúc khởi hành.
  Khay đối trọng 12 cm cũng cân thiếu khi nửa thân đứng lệch, nên mở rộng thành 14 cm và chuyển sang cân nguyên phần.
- **Phần tư không leo được mép khay 2 cm trơn:** khay thang còn 1,2 cm, mép màu ngà.
- **Cần B nối thẳng vào bậc thoát:** phần tư (0,17 N) không đủ sức kéo cả hai. Giờ B chỉ kéo tay nắm của nó, một lẫy đẩy bậc ra.

## Kiểm tra

- Test giải và test đi lang thang của `N31`, `N32`, `N33`, `N34`, `N35`, `N40`: đạt.
- Lời giải mẫu kiểm thêm:
  - 33: một phần tư trên C không làm chốt trượt.
  - 34: nửa với nửa không làm khay lên.
  - 35: nửa thân không đẩy được khối 75%.
  - 40: cả thân thì quá nặng, nửa thân thì không đủ.
- Clip: `Artifacts/Clips/ch4/chuong4.mp4`.

Code:
- Dựng cảnh: `Assets/_Game/Editor/COgheSpatialPlusLevels.cs` (`PlusN31`…`PlusN35`, `PlusN40`, `PadGauge`, `PlusLatch`,
  `PlusTwoRooms`).
- Cơ quan: `COgheTissueSensor` (đồng hồ, cân), `COgheBalanceLift` (mới).
- Lời giải mẫu: `COgheSpatialPlusScenario`.
- Lệnh dựng: `GenerateChapterTwoLevels -coghe-plus-levels N31,N32,N33,N34,N35,N40`.

## Còn lại của chương 4

- 37 "Thang chở thùng nặng" (E11 sửa): cả thân đẩy thùng lên khay trước, rồi tách. Nửa lên cùng thùng gồng không nổi, nên
  phải chốt điện để nửa dưới lên cùng, nhập lại rồi đẩy thùng vào ổ.
- 38: sửa độ rõ (khay gấp lộ mép, chốt trên nắp ống, ống đậm hơn).
