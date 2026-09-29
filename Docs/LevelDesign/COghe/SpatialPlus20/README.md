# Spatial Plus — 18 màn dễ + 2 Boss, đề xuất bản đầu 50 màn

30/09/2026 · **Đề xuất thiết kế, chưa dựng màn chơi được.** Theo yêu cầu của Mrk (29/09/2026): thêm 18 màn dễ xen giữa 30 màn có sẵn để độ khó mượt hơn, thêm 2 Boss rất khó bằng khối / bánh răng xếp nhiều lớp phải giải từng bước, có hình mô tả từng màn và đề xuất thứ tự để bản đầu có 50 màn.

- **[Thứ tự 50 màn và lý do](PLACEMENT.md)** — bảng đầy đủ, đường cong độ khó trước/sau.
- **[Cơ quan mới và mở rộng](MECHANICS.md)** — phần nào đã có runtime, phần nào phải prototype.
- Dữ liệu: [`levels.json`](levels.json). Hình: `Illustrations/NN-KEY.png` (một tấm mỗi màn) và `Illustrations/placement-50.png`.

![Đề xuất 50 màn](Illustrations/placement-50.png)

## 20 màn mới

| Vị trí | Màn | Hình | Vai trò | Phần | Ý chính |
| --- | --- | --- | --- | --- | --- |
| 13 | [Thang chở hàng](LevelE01/README.md) | [Hình](Illustrations/13-E01.png) | Luyện tập (dễ) | 1 | Thang nâng chở cả thùng; lên cao rồi thùng thành bậc cuối. |
| 15 | [Hai ống, một đích](LevelE02/README.md) | [Hình](Illustrations/15-E02.png) | Luyện tập (dễ) | 1 | Nhìn đầu ra của ống trước khi chui. |
| 18 | [Giữ cửa cho bạn](LevelE03/README.md) | [Hình](Illustrations/18-E03.png) | Luyện tập (dễ) | 2 | Một nửa giữ cửa, nửa kia chốt cửa lại để cả hai cùng về. |
| 21 | [Cõng thùng](LevelE04/README.md) | [Hình](Illustrations/21-E04.png) | Nghỉ nhịp sau Boss (dễ) | 1 | Kéo xe đỡ thì thùng trên xe đi theo; thùng còn trượt được trên xe. |
| 23 | [Hai nhịp dây](LevelE05/README.md) | [Hình](Illustrations/23-E05.png) | Luyện tập (dễ) | 1 | Đu rồi đu tiếp: bến giữa đủ rộng để đứng và bám dây sau. |
| 26 | [Bập bênh](LevelE06/README.md) | [Hình](Illustrations/26-E06.png) | Giới thiệu cơ quan mới (dễ) | 1 | Đi qua trục thì ván nghiêng về phía mình và thả mình xuống phòng bên. |
| 29 | [Chồng hai tầng](LevelE07/README.md) | [Hình](Illustrations/29-E07.png) | Chuẩn bị Boss (dễ) | 1 | Khối dưới vào trước làm nền, khối trên trượt tới mép làm bậc thứ hai. |
| 30 | [BOSS · Tháp khối](LevelB1/README.md) | [Hình](Illustrations/30-B1.png) | Boss chương 3 (rất khó) | 2 | Tháp khối vừa là dụng cụ vừa là đích: dùng nó để lên, rồi sắp lại nó để thoát. |
| 31 | [Bánh răng đầu tiên](LevelE08/README.md) | [Hình](Illustrations/31-E08.png) | Nghỉ nhịp + giới thiệu (dễ) | 1 | Đứng lên nút máy thì bánh răng quay và làm việc nặng thay mình. |
| 33 | [Đủ nặng mới mở](LevelE09/README.md) | [Hình](Illustrations/33-E09.png) | Luyện tập (dễ–vừa) | 3 | Nút nặng cần nửa thân; phần 25% quá nhẹ, chỉ đè được nút nhẹ. |
| 36 | [Đu rồi luồn](LevelE10/README.md) | [Hình](Illustrations/36-E10.png) | Luyện tập (dễ) | 1 | Dây đưa sang đảo, ống trên đảo mới dẫn vào chuồng thoát. |
| 37 | [Giữ thang cho bạn](LevelE11/README.md) | [Hình](Illustrations/37-E11.png) | Luyện tập (dễ–vừa) | 2 | Thang chỉ có điện khi có người giữ cần; lên trên thì chốt điện lại cho bạn. |
| 41 | [Khớp một bánh](LevelE12/README.md) | [Hình](Illustrations/41-E12.png) | Nghỉ nhịp + luyện (dễ) | 1 | Thiếu một bánh thì máy quay suông; đưa bánh G vào khe rồi mới bật máy. |
| 42 | [Hai ống, hai nửa](LevelE13/README.md) | [Hình](Illustrations/42-E13.png) | Luyện tập (dễ) | 2 | Mỗi nửa một ống, hai nút cao cùng lúc mới mở đường. |
| 44 | [Hai tầng răng](LevelE14/README.md) | [Hình](Illustrations/44-E14.png) | Luyện tập (dễ–vừa) | 1 | Lực đi từ tầng dưới lên tầng trên qua trục; khớp từng tầng theo thứ tự. |
| 46 | [Người chạy máy](LevelE15/README.md) | [Hình](Illustrations/46-E15.png) | Luyện tập (dễ–vừa) | 2 | Máy chỉ chạy khi có người đứng nút; người trên cao phải chốt lại cho người chạy máy. |
| 47 | [Bàn xoay](LevelE16/README.md) | [Hình](Illustrations/47-E16.png) | Giới thiệu biến thể mới (dễ) | 1 | Bánh răng xoay bàn; mặt bàn quay ngang mới nối hai bờ. |
| 48 | [Hai máy nối nhau](LevelE17/README.md) | [Hình](Illustrations/48-E17.png) | Luyện tập (dễ) | 1 | Máy 1 không nâng gì cả — nó đưa bánh G vào máy 2. |
| 49 | [Ba lớp răng](LevelE18/README.md) | [Hình](Illustrations/49-E18.png) | Chuẩn bị Boss (vừa) | 2 | Khớp tầng nào thì đường lên tầng sau mới mở; người giữ máy không cần đi lại. |
| 50 | [BOSS · Tháp bánh răng](LevelB2/README.md) | [Hình](Illustrations/50-B2.png) | Boss cuối (rất khó) | 4 | Mỗi tầng mở đường lên tầng sau; cửa cuối cần cả hai động cơ cùng lúc nên bốn phần phải chia vai đúng. |

## Hình minh hoạ được làm thế nào

Khác bộ 11–30 (ảnh concept AI), mỗi tấm ở đây là **greybox dựng trong Unity** bằng chính các helper, kích thước và art Glass C của Spatial 11–30, rồi chụp:

- trái: góc camera trong game lúc bắt đầu (có sinh vật thật ở điểm xuất phát);
- giữa: mặt bằng nhìn thẳng từ trên, với đường đi và số bước chiếu từ toạ độ trong scene;
- phải/dưới: ý chính, cơ quan, số liệu dự kiến, lời giải và lý do đặt ở vị trí đó.

Greybox **không phải màn chơi được**: không vào build/catalog, cơ quan mới chưa có runtime chỉ được đặt đúng chỗ. Kích thước là điểm bắt đầu để prototype, phải đo lại khi dựng.

Tạo lại: `Tools/render-spatial-plus-designs.sh` (dựng greybox → chụp PlayMode → xoá asset tạm). Code: `Assets/_Game/Editor/COgheSpatialPlusDesignBuilder.cs`, `COgheSpatialPlusDesignLevels.cs`, test chụp `Assets/_Game/Tests/PlayMode/COgheSpatialPlusDesignRender.cs` (Explicit, không chạy trong suite).

## Chưa làm

Dựng scene chơi được, chạy lời giải bằng chạm thật, prototype ván bập bênh / bàn xoay / đầu ra hai động cơ, art Glass C cho bánh răng, đo OPPO, người chơi mới.
