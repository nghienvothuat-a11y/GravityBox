# Nhịp độ 65 màn — 15/10/2026, thay 6 màn 19/10/2026

Theo yêu cầu tăng mượt độ khó, giảm bước nhảy 9 → 10 và tăng Boss 32 so với Boss 21.
Giữ đủ 65 content ID, luật kính, sáu Boss ở 10/21/32/43/54/65 và save đã chơi.
Không reset tiến độ; đổi thứ tự catalog, SceneSequence và số plaque đồng bộ.
Màn đã giải vẫn được nhận dạng bằng ID cũ, không nhận lại thưởng lần đầu.

## Thay 6 màn bằng màn đường kính (19/10/2026)

Mrk chọn: 66 → 8, 67 → 11, 68 → 25, 69 → 27, 70 → 59, 71 → 63 (08, G1, 07, N41, K08, K10 rút khỏi thứ tự; scene, ID và save giữ nguyên).
Cột "Vị trí cũ" của sáu màn này là số thứ tự bản thử 66–71. Hai bài học không còn màn giới thiệu riêng:
thang máy (08 ở 8; 9 và Boss 10 dùng thang) và ghép bánh răng (N41 ở 27; 28 là bài luyện có bánh đệm).
Mrk được báo và giữ danh sách (19/10, "giữ nguyên danh sách của tao"): thang gặp lần đầu ở 9 cùng cầu, bánh răng ở 28 cùng bánh đệm.
`curriculum.json` (revision 2026-10-19) ghi hai chuỗi này kèm ghi chú; `Tools/check-coghe-curriculum.py` qua.
Mốc mở khoá theo thứ tự trước 19/10 được giữ bằng `COgheProgressionMilestones` (revision 2).

## Ba màn vật lý 66–68 (20/10/2026)

Mrk chốt (20/10, sau khi xem clip các màn lặp 17, 40): giữ nguyên 65 màn, ba màn vật lý là ba màn mới 66, 67, 68
sau Boss 65 (F1 Bi giữ cửa, F2 Bánh răng thả bi, F3 Kính vỡ, đối trọng). Không thay màn nào, không đổi thứ tự,
Boss và mốc mở khoá. `curriculum.json` (revision 2026-10-20) thêm chuỗi `physics` (giới thiệu F1, luyện F2, kết hợp F3).
Kế hoạch làm tiếp các màn vật lý ở giai đoạn sau: [PhysicsProps/NEXT_PHASE_PLAN.md](../PhysicsProps/NEXT_PHASE_PLAN.md).

## Nhịp học và khó

Mỗi họ cơ chế có bài giới thiệu, bài luyện trước khi kết hợp. Biến thể mới trong cùng
họ vẫn có thể xuất hiện ở bài luyện; không coi mọi hình dạng khối là một kỹ năng mới.
Sau Boss có bài nhẹ hơn để nghỉ và học tiếp. Không ép điểm khó tăng ở từng màn;
độ dài phụ thuộc, vai trò đồng thời, quan sát và chi phí sửa sai được xét riêng.
Không tăng yêu cầu căn chạm/căn thời điểm, không thay lực hay tốc độ dùng chung.

- **8–10:** 8 là màn đường kính G6 (từ 19/10); thang gặp lần đầu ở 9 cùng cầu. Boss 10 giữ nguyên thiết kế và cơ chế đã chốt theo yêu cầu mới nhất; không sửa layout hoặc chuỗi giải.
- **Trước 21:** luyện xếp hàng/khối ở 14–15, tách/giữ ở 16–17, nhập để kéo nặng
  ở 18; chở hàng ở 19, thứ tự khối ở 20. Boss không là lần đầu thấy lực cả thân.
- **22–32:** ống riêng → luyện ống → giữ/chui ống; đường trần → chuỗi kệ/trần.
  Boss 32 có hai pha phân vai và bàn giao người giữ. Boss 21 chỉ cần một pha
  giữ đồng thời rồi chốt vĩnh viễn. Boss 32 đòi hỏi giữ nhiệm vụ sống lâu hơn,
  quay về nhánh ban đầu và đổi vai; không dùng hạt nhỏ hơn hay bến hẹp hơn.
- **33–43:** đu riêng → luyện vị trí bến → đu/ống, rồi đối trọng và phối hợp thang.
- **44–54:** nửa thân → phần tư → luyện nhập phần tư → ba phần tư → Boss cân.
- **55–65:** chở hàng xoay, tải động cơ, hai động cơ; cuối chương tăng phụ thuộc,
  nhu cầu trả khối/đổi vai. Mỗi mốc Boss vẫn không chỉ đáp án.

Đây là mục tiêu của tác giả, chưa phải bằng chứng người mới thấy Boss 32 khó hơn 21.
Cần đo trên người chơi: số lần thử, thời gian suy nghĩ, chạm sai, lạm dụng gợi ý,
và tỉ lệ bỏ cuộc tại 8–10 / 18–21 / 29–32. Replay tự động chỉ chứng minh khả giải.

## Dữ liệu từng màn

| Màn | Content | Vị trí cũ | Vai trò |
|---:|---|---:|---|
| 1 | 01 | 1 | Chạm đích, bò đến lỗ trên kính. |
| 2 | 02 | 2 | Luyện bò và chuyển từ sàn lên tường. |
| 3 | 03 | 3 | Học orbit để thấy đích bị che. |
| 4 | 04 | 4 | Học kéo một tay nắm, một cửa. |
| 5 | 05 | 5 | Luyện kéo; học mở nắp trước khi kéo. |
| 6 | K01 | 6 | Dời thùng cản → kéo khối đỏ để lộ lỗ dưới sàn → thoát xuống. Không công tắc/nút. [Revision 15/10](../CrateReveal6/README.md). |
| 7 | 06 | 7 | Luyện nắp và leo; giới thiệu cầu ròng rọc. |
| 8 | G6 | 66 | Đường trên kính (dễ): một đường kính, A mở chân đường. Thay 08 (bài thang riêng) theo Mrk 19/10. |
| 9 | 09 | 17 | Luyện thang với cầu/chốt đã học; không thêm kỹ năng. |
| 10 | 10 | 10 | Boss 10 giữ nguyên thiết kế đã chốt; màn 8–9 chuẩn bị thang và cầu. |
| 11 | G7 | 67 | Sau tấm chắn (vừa): đường khuất sau tấm chắn, B treo trên kính. Thay G1 theo Mrk 19/10. |
| 12 | K02 | 13 | Luyện ba khối trượt trên sàn. |
| 13 | G2 | 14 | Luyện đường leo khi có lớp phủ trơn. |
| 14 | N22 | 11 | Học chuẩn bị hàng trước khi xe cập bến. |
| 15 | N29 | 26 | Luyện thứ tự khối trên/khối dưới, chuẩn bị tháp Boss. |
| 16 | 16 | 18 | Học Q chia đôi, chọn phần, giữ hai nút, nhập lại. |
| 17 | N13 | 20 | Luyện một phần giữ, phần kia làm việc; buông sai có thể làm lại. |
| 18 | N18 | 57 | Luyện hợp thể: nửa thân thử khối nặng rồi cả thân kéo được. |
| 19 | 13 | 16 | Kết hợp thang đã học với chở thùng; không thêm thao tác. |
| 20 | 12 | 27 | Luyện thứ tự khối cao/thấp trước tháp Boss. |
| 21 | B1 | 21 | Boss tháp khối: đồng thời chốt, xếp trên trước, nhập lại kéo cả tháp. |
| 22 | 14 | 15 | Học một đường ống và nắp, không phân tách. |
| 23 | N15 | 19 | Luyện ống: chuẩn bị bậc trước khi sang phòng. |
| 24 | K03 | 23 | Bài khối dài, luyện không gian sàn. |
| 25 | G8 | 68 | Bốn mặt kính (khó): đường qua bốn vách, có đường cụt. Thay 07 theo Mrk 19/10. |
| 26 | G3 | 25 | Học đường qua trần bằng tấm cầu trần. |
| 27 | G9 | 69 | Hai nửa, bốn vách (cực khó): tách đôi, đổi người giữ cửa. Thay N41 (học bánh răng) theo Mrk 19/10. |
| 28 | N44 | 28 | Luyện bánh răng, khám phá bánh đệm đổi chiều. |
| 29 | 17 | 29 | Kết hợp giữ nút và chui ống sau hai bài luyện ống. |
| 30 | G4 | 30 | Luyện chuỗi kệ/trần và nút chốt trước Boss. |
| 31 | 22 | 31 | Luyện bãi chờ, xếp ba mảnh đường. |
| 32 | G5 | 32 | Boss relay: giữ P → qua trần/B → giữ D → đổi phần/C → leo/A → nhập/thoát. |
| 33 | 18 | 33 | Học bám dây và chọn bến rộng. |
| 34 | N23 | 35 | Luyện căn vị trí bến trước khi đu. |
| 35 | E10 | 34 | Kết hợp đu với ống đã quen. |
| 36 | 21 | 36 | Học đối trọng qua khay tải. |
| 37 | N25 | 37 | Luyện thêm tải để khay đủ nặng. |
| 38 | N48 | 39 | Luyện truyền lực hai tầng trục. |
| 39 | N47 | 38 | Kết hợp hai máy dùng chung một bánh. |
| 40 | K04 | 40 | Nhịp nghỉ: tháo khối theo lớp. |
| 41 | 15 | 41 | Ống ngã ba: chọn nhánh, quay lại được. |
| 42 | E11 | 42 | Luyện giữ thang/đổi người trước Boss hợp tác. |
| 43 | 20 | 43 | Boss phối hợp nhiều khoang; kỹ năng đều đã xuất hiện. |
| 44 | K05 | 44 | Luyện khối vuông sau Boss. |
| 45 | N42 | 45 | Hai xe bánh răng chia sẻ ngã tư. |
| 46 | K06 | 46 | Khối dây chuyền, quan sát thứ tự. |
| 47 | N26 | 47 | Học tải nửa thân trên ván nghiêng. |
| 48 | N31 | 48 | Học Q lần hai và đồng hồ phần tư. |
| 49 | N33 | 51 | Luyện nhập hai phần tư thành nửa ngay sau bài phần tư. |
| 50 | K07 | 50 | Bài khối xen kẽ để giảm tải trí nhớ. |
| 51 | 26 | 49 | Vận dụng đu/ống với hai vai trò. |
| 52 | N35 | 52 | Luyện ghép ba phần tư và giữ phần còn lại. |
| 53 | N43 | 53 | Kết hợp hộp số với chọn nhánh ống. |
| 54 | N40 | 54 | Boss cân ba phần tư sau hai bài luyện ghép khối lượng. |
| 55 | N46 | 24 | Đổi ngữ cảnh: bàn xoay mang hàng, không thêm kỹ năng điều khiển. |
| 56 | N45 | 55 | Học tải của người chạy máy ảnh hưởng lực. |
| 57 | N49 | 56 | Luyện hai động cơ cộng lực. |
| 58 | N32 | 58 | Vận dụng khối nặng trước, tách sau. |
| 59 | G10 | 70 | Thang máy giữa hai đường (vừa–khó). Thay K08 theo Mrk 19/10. |
| 60 | N34 | 60 | Kết hợp khối lượng với bập bênh chở bạn. |
| 61 | K09 | 61 | Kho chật, thử sai có thể đảo chiều. |
| 62 | 29 | 62 | Ráp nhiều phần vào khung trước khi nâng. |
| 63 | G11 | 71 | Bánh răng và dây đu (khó): bánh răng trên kính, đu dây. Thay K10 theo Mrk 19/10. |
| 64 | N19 | 64 | Luyện đổi vai người giữ, chuẩn bị Boss cuối. |
| 65 | N50 | 65 | Boss hộp số, phối hợp các hệ đã luyện. |
| 66 | F1 | 66 | Bi giữ cửa (dễ): kéo D thả bi xuống nút A, bi giữ cửa thay mình; cả thân đẩy thùng 100%. |
| 67 | F2 | 67 | Bánh răng thả bi (vừa): bánh răng nhấc chốt, bi lăn đổ cột thành bậc 3 cm lên bục lỗ thoát. |
| 68 | F3 | 68 | Kính vỡ, đối trọng (khó): hai nửa giữ hai nút thả bi, nhập lại; cả thân cùng bi mới đủ nặng làm vỡ kính. |

## Kiểm chứng

[Biên bản test](../../../Verification/COgheDifficulty65/README.md).
`curriculum.json` là bảng tác giả; `Tools/check-coghe-curriculum.py` kiểm tra thứ tự và
bài luyện trước kết hợp, không chứng nhận physics. Author replay dùng input/thang/Q thật.
