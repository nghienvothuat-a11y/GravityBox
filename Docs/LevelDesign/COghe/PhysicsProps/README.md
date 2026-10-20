# Đồ vật vật lý — màn 66–68

Mrk 19/10/2026: thêm yếu tố vật lý vào màn chơi (bi sắt lăn giữ nút, sức nặng, rơi, vỡ, đổ), để vừa giải đố vừa nghịch và
cản đường COghe. Yếu tố vật lý phải kết hợp được với cơ quan đã có. Lấy 3 màn có sẵn, biến đổi, làm 66, 67, 68 để chơi thử.
Code: `Assets/_Game/Editor/COghePhysicsPropLevels.cs` (khoá F1–F3, nằm sau 65 màn).

Mrk 20/10/2026: giữ nguyên 65 màn, F1–F3 là ba màn mới 66, 67, 68 (không thay màn nào). Chuỗi học `physics` trong
`Difficulty65/curriculum.json`. Kế hoạch giai đoạn sau: [NEXT_PHASE_PLAN.md](NEXT_PHASE_PLAN.md).

## Cơ quan mới

- **Bi sắt** (`SteelBall`): đồ rời hình cầu nặng 0,05 (gần nửa thân COghe), có trọng lực và Retry như mọi đồ rời. Không
  phải mặt đứng. Chỉ lăn trong rãnh trơn (`BallChute`), nên lần nào cũng tới cùng một chỗ. Chốt trong rãnh là cửa trơn,
  nhấc lên 10 cm, do tay nắm (`ViewLink`) hoặc bánh răng nhấc.
- **Nút cân được đồ vật**: `COgheTissueSensor.Weights` cộng khối lượng đồ rời đang nằm trên nút.
- **Cột đổ** (`COgheTopplePillar`): cột trên bản lề ở chân. Chỉ ngã khi bi chạm vào; COghe đẩy thì không. Cú ngã được
  tính như một thanh cứng quay quanh chân, nên lần nào cũng nằm đúng chỗ. Mặt trên trơn khi cột đứng (không thành thang),
  bám được khi cột đã nằm.
- **Kính mỏng vỡ** (`COgheBreakableGlass`): cân toàn bộ phần thân đứng trên kính (`Column`) cộng đồ vật trên đó. Qua
  ngưỡng nứt thì hiện vết nứt; tới ngưỡng vỡ thì tấm kính mất, mảnh vỡ bắn lên rồi rơi (chỉ hình). Retry lắp lại kính.

## Ba màn

| Màn | Khoá | Từ màn | Tên | Lời giải |
| --- | --- | --- | --- | --- |
| 66 | F1 | N18 (màn 18) | Bi giữ cửa (dễ) | Bỏ máy tách Q. Kéo D: bi lăn xuống rãnh dọc kính trái, nằm lên nút A ở góc trước. Bi giữ cửa lò xo. Cả thân đi qua, đẩy thùng 100% (cần nguyên thân) sát kệ, leo lên, ra. COghe tự đứng A thì cửa mở nhưng bước ra là cửa sập. |
| 67 | F2 | N41 (bài bánh răng đầu tiên) | Bánh răng thả bi (vừa) | Đầu ra của dàn bánh răng là chốt giữ bi. Kéo A (bánh G vào chỗ trống), đứng P: chốt nhấc lên, bi lăn xuống đập đổ cột. Cột nằm thành bậc 3 cm trước bệ lỗ thoát cao 6 cm (gờ 3 cm leo được, 6 cm thì không). |
| 68 | F3 | N25 (màn 37, "Chưa đủ nặng") | Kính vỡ, đối trọng (khó, bản dài 19/10) | Bỏ thùng. Miệng hố có tấm kính mỏng, ngay dưới là khay đối trọng. COghe bắt đầu dưới sàn cạnh máy Q. Chốt giữ bi do hai nút A dưới sàn, cách nhau 43 cm, hai bên Q cùng mở: tách đôi, mỗi nửa đứng một nút thì chốt nhấc, bi lăn khỏi máng trên kính sau, rơi xuống kính; kính chịu được (0,05 < 0,12). Nửa thân đứng cùng bi chỉ làm nứt (0,098). Hai nửa nhập lại dưới sàn, cả thân leo cầu thang lên bục. Cả thân và bi thì vỡ (0,146): cả hai rơi xuống khay, đối trọng kéo cầu ván nằm ngang. Leo ra khỏi hố, qua cầu, ra. |

## Kiểm tra

- `SpatialPlusF1–F3Solve`, `SpatialPlusF1–F3Wander`, `PhysicsF1–F3SolveAtFrameRate` (60 khung/giây).
- `PhysicsF1DoorNeedsTheBall`: COghe đứng A thì cửa mở, bước ra thì cửa sập trước khi qua được.
- `PhysicsF2PillarIgnoresCOghe`: COghe đẩy cột từ hai phía, cột không đổ, không lên được bệ.
- `PhysicsF3OnePadKeepsTheBall`: cả thân đứng từng nút A một thì chốt không nhấc; COghe một mình trên kính chỉ làm nứt; Retry lắp lại kính.
- `PhysicsF3HalfAndBallOnlyCrack`: tách đôi, giữ hai nút, bi nằm trên kính; một nửa bước lên cạnh bi thì kính chỉ nứt, không vỡ.
- `PhysicsF3SolveAt30Fps`: giải 68 ở 30 khung/giây.
- Phim: Explicit `GlassRoadFilm` với `GLASSROAD_KEYS=F1,F2,F3`.

## Bài học khi dựng

- Mặt `Panel` có phần đặc 8 mm nằm sau mặt hiển thị. Vách rãnh quay mặt ra ngoài thì phần đặc lấn vào trong, rãnh còn
  4,4 cm và kẹt viên bi 5 cm cùng cái chốt. Vách rãnh phải quay mặt vào trong.
- Rãnh dốc thì đỉnh viên bi ở phía trên chốt cao hơn ở ngay chốt; chốt nhấc 6 cm vẫn vướng ở dốc 32°, nên nhấc 10 cm.
- Nút cân theo lớp tiếp xúc chỉ đọc được một phần thân. Kính cần cân nguyên phần thân (`Column`).
- Tay nắm được thao tác từ chỗ đứng cách 5 cm phía trước. Đặt sát kính trước thì chỗ đứng nằm ngoài hộp.
- Dải sàn hẹp giữa rãnh, tay nắm và mép bục dễ thành khe 1–9 cm làm kẹt COghe. Ở 68 máng bi treo trên kính sau, cao
  hơn tấm kính 11 cm.
- 68 bản dài: máy Q rộng gần 40 cm (kể cả hai khay), không vừa trên bục 25 cm. Q đặt dưới sàn trước, cầu thang lên bục dời ra sau
  khay trái của Q (z −0,02) để không kẹp Q vào giữa.
