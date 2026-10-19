# Hiểu ra — kiểm chứng nhịp 12 bài

19/10/2026 · `NewGraphic`, nền `8b9c783cad196e68f188b1f8ffe2952596d6b385` + working tree của đợt này.
Thiết kế và 12 dossier: [Understand12](../../LevelDesign/COghe/Understand12/README.md).

## Phạm vi

- Thêm sáu bài luyện ngắn, giữ sáu bài sau Boss, một thẻ/bài.
- Giảm số yêu cầu ghép/đếm của các bài sau; làm nhẹ lượt nhập môn của đồ chơi.
- Mở phòng tương tác trước Home mà không mở Collection/shop; tiếp tục từ thẻ đang chọn.
- Migration bỏ qua các bài luyện mới ở mốc đã thắng của save cũ; không cấp hồi tố/trùng.
- Không thay 65 scene, geometry, vật liệu, solver, cơ quan, camera, Boss 10 hoặc sức mạnh mỗi cấp ability.

## Môi trường và cách chạy

Unity **6000.3.19f1**, PlayMode có graphics, macOS **26.5.1 (25F80)**.
Fixture khởi tạo tiến trình/checkpoint trong bộ nhớ, tắt ghi PlayerPrefs. Chỉ sắp đặt
mốc bắt đầu; lời đáp và đồ chơi chạy qua TouchPoint, Feed và item input thật.
UI card chọn bằng pointer qua Canvas. Không gán trạng thái hoàn tất để thay lời đáp
trong bài kiểm tra end-to-end.

- Dữ liệu/migration: `GravityBox.Tests.COgheUnderstandPacingTests`.
- Tất cả 12 bài + đồ chơi + thưởng + thẻ:
  `GravityBox.Tests.COgheProductUITests.AllTwelveUnderstandLessonsPlayThroughWithTheirCards` (Explicit).
- Thắng màn 6 → bài đầu khi Home khóa → sai/hint/pause → hoàn tất → thoát/reload màn
  chọn thẻ → chọn Clever → tải màn 7:
  `FirstPracticeBeforeHomeUnlockRecoversFromMissesAndResumesItsCard`.
- Recovery trong câu dài: `PracticeKeepsCorrectWordsWhenACountOrLaterAnswerIsWrong`.
- Hồi quy: BonusTests, BonusChapterTests, ToyTests (không render fixture), AbilityUI,
  ChapterProgression, quy tắc giới hạn/card-idempotency.

Lệnh tái hiện:

```sh
/Applications/Unity/Hub/Editor/6000.3.19f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -projectPath "$PWD" -runTests -testPlatform PlayMode \
  -testFilter "$(cat Docs/Verification/COgheUnderstandPacing/filter.txt)" \
  -testResults Artifacts/COgheUnderstandPacing/final.xml \
  -logFile Artifacts/COgheUnderstandPacing/final.log
```

`Tools/verify-coghe-expansion.sh` không lọc fixture nên tự chứa các test mới thông
thường; test end-to-end 12 bài là Explicit, chạy riêng bằng filter trên.

## Kết quả

**40/40 ca phân biệt đã đạt qua hai lượt.**

- [Lượt đầy đủ](full-run.xml): 39/40 đạt, khoảng 603 giây. Bao gồm chơi trọn cả 12
  bài, đồ chơi, nhận thưởng và chọn thẻ; từng bài dùng input thật, không ép hoàn tất.
- Một ca `TopGameKeepsTheTopSpinning` đo 7 giây từ lúc con quay đã chao >20% cửa sổ,
  nên vượt cả 8 giây cho phép. Đây là lỗi mốc thời gian của fixture; sửa đo từ
  `ToyWaitingForPlayer` ngay khi cửa sổ phản ứng bắt đầu. Không tăng thời gian runtime
  để che lỗi test.
- [Lượt kiểm tra lại](recheck.xml): 2/2 đạt, khoảng 42 giây; con quay giữ lượt cứu đã
  xong sau hai lần rơi và vẫn nhận thao tác sau 7 giây, cùng luồng thắng màn 6 →
  bài đầu → hint/pause → thẻ/reload → màn 7.
- Các ca đã qua còn gồm: thứ tự 12 mốc; 12 thẻ khác nhau; không trả thưởng/thẻ trùng;
  migrate save cũ không phát sinh backlog; round checkpoint với thưởng 0 Drops;
  không mất những từ đúng khi sai ở giữa/cuối câu; bài tiêm màu ở 8 và Nhà ở 10;
  layout thẻ và hướng dẫn trên phone/tablet; các đồ chơi và chương sinh sau 6.
- Sau lượt chính chỉ sửa mốc đo của fixture con quay, chú thích trống và câu gợi ý
  chung của chương 6 thành “One wish at a time.” (vì câu đầu có hai yêu cầu, câu sau
  ba). Lượt kiểm tra lại biên dịch toàn bộ runtime cuối cùng.

Thời gian bài luyện trong lượt input tự động, **đã biết đáp án**, từ bắt đầu bài đến
popup hoàn tất, chưa gồm thời gian chọn thẻ:

| Sau màn | Giây |
| --- | --- |
| 6 | 8.2 |
| 16 | 10.7 |
| 27 | 11.9 |
| 38 | 14.3 |
| 49 | 15.2 |
| 60 | 26.2 |

Các bài boss vẫn ba lượt; `AllTwelveUnderstandLessonsPlayThroughWithTheirCards`
hoàn thành toàn bộ 12 bài + thẻ và kiểm tra đồ chơi trong khoảng 354 giây.
Không diễn giải các số này thành thời gian chơi hoặc mức hiểu của trẻ mới chơi.

Không có scene/definition, chất liệu hoặc shader thay đổi trong working tree.
Chuẩn hóa importer tự phát của Unity đã được trả về đúng dữ liệu đầu task.


## Giới hạn

Không chạy lại 65 lời giải puzzle vì không đổi cơ quan, vật lý hoặc scene. Chưa build
bản này lên iPad/OPPO, chưa có benchmark điện thoại và chưa quan sát trẻ 4–15 tuổi.
Thời gian script đã biết đáp án không đo khả năng hiểu của người mới, cũng không
chứng minh retention. Cần test người thật theo nhóm tuổi và xem tỷ lệ hoàn tất/rời bài,
số lần sai/gợi ý, thời gian chơi chủ động trước khi điều chỉnh tiếp.
