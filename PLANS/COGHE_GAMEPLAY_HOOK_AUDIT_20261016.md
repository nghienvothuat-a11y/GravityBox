# Rà soát gameplay và kế hoạch giữ chân người chơi COghe

16/10/2026 · Phạm vi: **65 màn trong working tree NewGraphic hiện tại**, kể cả công việc chưa commit. [Bảng từng màn](level-hook-audit-20261016/LEVELS_01_65.md) · [CSV](level-hook-audit-20261016/levels.csv) · [Dữ liệu nguồn và hash scene](level-hook-audit-20261016/audit.json).

**Kết luận:** COghe đã có đủ cơ chế để tạo nhiều câu đố khác nhau. Việc cần ưu tiên là tổ chức lại *kiến thức mà người chơi khám phá*: làm rõ quan hệ nhân quả, luyện từng quy tắc trước khi kết hợp, tạo những lần nhận ra cách dùng khác của vật quen. Tăng số cơ quan hoặc kéo dài chuỗi thao tác không tự tạo ra chiều sâu. Retention tốt hơn là giả thuyết cần kiểm chứng trên người mới, chưa có số D1/D7/D30 thực tế trong lượt rà soát này.

Đề xuất dưới đây chưa sửa gameplay, thứ tự hay scene. Giữ **Boss 10 nguyên thiết kế** và **màn 6: dời khối đỏ để lộ lỗ sàn, thoát xuống, không công tắc**. Luật kính, Q chia 50/50, tự hợp thể khi gần nhau, hợp thể trước lỗ cuối, tiêm màu sau 8 và Hiểu COghe bắt buộc vẫn là ràng buộc hiện hành.

## 1. Bằng chứng và giới hạn

Đối chiếu `SpatialOrder`, 65 saved scenes, builder, lời giải tác giả, curriculum, hint/flow/analytics. Script [audit_catalog.py](level-hook-audit-20261016/audit_catalog.py) đọc lại các scene và ghi số màn hiện tại, content key, hash và component. Đếm component có serialize, kể cả inactive; đó không phải số quyết định hoặc bằng chứng mọi component đều được dùng trong lời giải.

Biên bản [Difficulty65](../Docs/Verification/COgheDifficulty65/README.md) có 65/65 author solve trước đó. Màn 6 dùng [bằng chứng sửa mới hơn](../Docs/Verification/COgheCrateReveal6/README.md): 9/9 tests, native Mac qua bằng 3 chạm, 32/32 hạt. Không chạy lại 65 màn trong lượt audit này. Replay tác giả chứng minh một đường giải, không chứng minh người mới hiểu đường đó hay muốn quay lại.

`PLANS/COGHE_LEVEL_HOOK_PLAN.md` là kế hoạch 50 màn ngày 05/10. Một phần đã được triển khai; các số màn, lỗi và đề nghị reset save trong đó không được coi là hiện trạng. Bảng mới theo đúng 65 vị trí, không tự reset tiến độ. Một số tài liệu cũ còn ghi Q/đu dây là “planned”; code và scene hiện tại đã có các component đó.

## 2. Những gì game đang làm tốt

- Một thao tác chạm có thể dùng cho nhiều cơ quan; toàn thân COghe có cá tính và phản ứng. Đây là nền để câu đố phức tạp hơn mà đầu vào vẫn đơn giản.
- Sàn, tường, trần có thể trở thành đường đi; đổi góc nhìn đem lại thông tin mới. Đây là lợi thế riêng so với game kéo khối 2D.
- Khối lượng được bảo toàn khi tách/nhập, tạo tài nguyên hữu hạn cho puzzle. Hai phần làm hai việc, cả thân làm việc nặng: quy tắc dễ suy luận nếu hình ảnh rõ.
- Đã có các ý tưởng đáng phát triển: lỗ giấu dưới khối ở 6; chuẩn bị hàng trước ở 14; khối lò xo ở 17; bánh đệm đổi chiều ở 28; mượn bánh sau khi chốt ở 39; khối đi rồi trả lại ở 59.
- Nhịp mới đã đặt bài thang 8–9 trước Boss 10, ống 22–23 trước phối hợp 29, đu 33–34 trước đu/ống 35. Không cần làm lại toàn bộ campaign.

## 3. Những vấn đề cụ thể

### Bản sắc sinh vật đến muộn

| Họ cơ quan trong saved scenes | Xuất hiện đầu | Số scene có component |
|---|---:|---:|
| Thang chở COghe | 8 | 6 |
| Q phân tách | 16 | 21 |
| Ống biến dạng | 22 | 8 |
| Truyền động bánh răng | 27 | 10 |
| Đu dây | 33 | 4 |
| Khay tải/đối trọng kiểu SeesawBridge | 36 | 3 |
| Thang cân hai tải BalanceLift | 60 | 1 |

Có `COgheTapRail` trong **51/65 scene** và **10 màn thùng** ở 6, 12, 24, 40, 44, 46, 50, 59, 61, 63. Việc dùng chung đầu vào là tốt, không phải chứng cứ 51 màn trùng nhau. Nguy cơ là người chơi chỉ thấy “chạm tay nắm rồi chờ”, nếu kết quả kéo luôn giống nhau.

**Giả thuyết cần thử:** một phần người chơi rời game trước khi thấy Q/biến dạng. Không nên dồn tất cả cơ chế lên đầu. Ưu tiên một cú khám phá nhỏ trong phiên đầu và một lời hứa cụ thể về cơ quan sắp tới. Không gắn cứng “D1 = màn 10” vì tốc độ và lịch chơi mỗi người khác nhau.

### Nhóm cơ chế đã học không đồng nghĩa quy tắc đã được học

- **Màn 16:** lần đầu Q đồng thời đòi hỏi chia đôi, chọn phần, giao hai đích, hiểu hai nút cùng lúc và tụ lại trước thoát. Bố cục đối xứng giúp đọc, nhưng lượng kiến thức mới vẫn lớn.
- **Boss 32:** giữ P → qua trần/B → D tiếp quản → phần cũ rời P để kéo C/A. Giữ rồi chốt vĩnh viễn ở 21/29 khác với *bàn giao một điều kiện đang phải duy trì*. Màn 30 vẫn một cơ thể, màn 31 xếp mảnh đường. Chưa có bài bàn giao đơn giản ngay trước Boss. Các bài 42/64 cho reference nhưng nằm sau nó.
- **Boss 54:** đây là **scene duy nhất trong 65 scene có `COgheTissueSensor.MaxLoad > 0`**. Cân nhận khoảng `.060–.084 kg`, tương ứng trạng thái 75% của thân `.096 kg`; cả thân quá nặng. Bài 48/49 dạy tải tối thiểu, bài 52 dạy lực 75%, không dạy “nặng hơn lại sai”. Boss đang giới thiệu quy tắc giới hạn trên.
- **Màn 55 và 60:** cùng một input không có nghĩa không có kiến thức mới. Bàn xoay đổi quan hệ vị trí; cân hai khay thụ động khác mô tơ chở tải. Cần một lần thử an toàn để hiểu trước khi gắn thêm phân vai.

`check-coghe-curriculum.py` hiện kiểm tra có *ít nhất một bài luyện của họ cơ chế* đứng trước bài kết hợp. Nó không kiểm tra từng quy tắc như bàn giao, tải tối đa, chốt nhớ hay sử dụng lại vật. Vì vậy cả 10 learning chains có thể đạt mà vẫn thiếu bài luyện ở trên.

### Màn luyện, màn nghỉ và màn lặp chưa được phân biệt đủ rõ

14/N22 và 15/N29 dùng cùng kiểu interlock “B chỉ di chuyển khi A chưa cập bến”. Khác biệt theo chiều cao có giá trị nếu người chơi cần đổi cách chọn điểm đứng; nếu không, chúng chỉ là hai lần làm B rồi A. Giữ cặp này để test chuyển kiến thức, không mặc định xóa một màn.

Các màn thùng 40/44/50 được bố trí như nhịp nghỉ, nhưng gỡ nhiều khối có thể tốn trí nhớ hơn một cơ quan mới đơn giản. Sau 40 còn thùng ở 44/46/50 và 59/61/63. Cần phân biệt *ít loại cơ quan* với *nhẹ đầu*.

### Số thao tác cao là tín hiệu kiểm tra, không phải điểm khó

| Vị trí / content | Chạm trong replay đã lưu | Điều cần xác minh |
|---|---:|---|
| 18 / N18 | 26 | Có thử nửa thân thiếu lực và chọn lại |
| 47 / N26 | 30 | Có thử nửa thân không làm ván nghiêng, đi vòng Q |
| 49 / N33 | 29 | Thử phần tư trước khi nhập đủ tải |
| 52 / N35 | 44 | Gồng thất bại, chia/nhập và retry; không phải 44 quyết định |
| 54 / N40 | 33 | Cố tình thử cả thân và nửa thân trên cân |
| 56 / N45 | 33 | Demo 50/50 không lên hết, sau đó thử 75/25 |
| 65 / N50 | 40 | Nhiều bước phối hợp và tái cấu hình |

Nguồn: [levels.jsonl](../Docs/Verification/COgheDifficulty65/levels.jsonl), [replay code](../Assets/_Game/Venom/Runtime/ChapterProof/COgheSpatialPlusScenario.cs). Thời gian của phần lớn lượt này là thời gian simulation; không so với native Mac màn 6, không suy ra thời lượng phiên người mới. `partSelections=0` cũng không nghĩa không đổi phần: replay có chỗ gọi API chọn phần trực tiếp.

Nên tách **replay lời giải gọn**, **test thử sai/phục hồi**, **playtest người mới**. Mỗi loại trả lời một câu hỏi khác nhau.

### Chưa đủ dữ liệu để biết bỏ vì chán hay vì khó

[COgheLevelAttempt](../Assets/_Game/Venom/Runtime/Shop/COgheLevelAttempt.cs) đã có start/end/complete/fail/abandon/retry, ID ổn định, thứ tự, attempt_id, duration và replay. Có ink-guide, bonus, item và ad events. Không nên làm lại nền analytics này.

Chưa thấy event gameplay cho thao tác bị từ chối, chuyển trạng thái cơ quan, thời gian tới tiến triển đầu, lúc hiển thị/xem lại hint và lúc thực sự bắt đầu màn sau. `hint_unlock` không bằng `hint_shown`. App bị hệ điều hành kill không bảo đảm chạy `OnApplicationQuit`, nên không thể lấy số `level_abandon` làm toàn bộ số bỏ cuộc. Retry hiện đi cùng abandon/retry, phải nhóm theo attempt_id để không cộng đôi.

Có hai chuyển cảnh bắt buộc cần đo riêng: tiêm màu sau 8 và Hiểu COghe sau Boss. Giữ quyết định sản phẩm hiện tại; nếu người chơi rời tại đó thì không đổ hết lỗi cho puzzle 8/10.

## 4. Đơn vị thiết kế mới là một phát hiện

Mỗi màn phải trả lời được: **“Sau màn này, người chơi hiểu thêm điều gì và vừa tự làm được điều gì?”**

Một chu kỳ đề xuất, có thể trải trên 4–6 màn, không phải lịch cứng:

1. **Gặp:** một luật mới, một đầu ra, một lần tác động thấy ngay kết quả.
2. **Khám phá:** cho thử một giả thuyết sai hợp lý; thất bại đưa thông tin, sửa được ngay.
3. **Thấu hiểu:** cùng luật trong bố cục khác, không mũi tên chỉ lời giải; người chơi dự đoán được kết quả.
4. **Kết hợp:** một luật mới đã học với một luật cũ; tăng quan hệ, không tăng độ chính xác ngón tay.
5. **Bất ngờ:** cùng luật đúng nhưng đổi vai trò vật, dùng lại nguồn lực hoặc quay về trạng thái cũ.
6. **Tổng hợp và nghỉ:** Boss tổ chức khác, rồi một màn ngắn để lấy lại nhịp và hé lộ điều tiếp theo.

Bất ngờ công bằng là “lẽ ra mình có thể suy ra”, không phải công tắc vô hình, màu gây hiểu nhầm hoặc vật lý đổi luật. Một khối làm bậc rồi trở thành đối trọng có chiều sâu hơn thêm ba cửa phải mở theo cùng chuỗi.

### Theo dõi độ khó bằng nhiều trục

Chấm tác giả mức thấp/vừa/cao cho từng trục; chỉ hiệu chỉnh trọng số sau playtest, không tạo một điểm chính xác giả:

| Trục | Cách đọc khi thiết kế |
|---|---|
| Kiến thức mới | Bao nhiêu quy tắc người chơi chưa từng vận dụng thành công? |
| Phụ thuộc | Phải chuẩn bị trước bao nhiêu điều; có quay về dùng lại vật không? |
| Trạng thái đồng thời | Bao nhiêu điều kiện đang sống phải giữ; có đổi người giữ không? |
| Quan sát | Bao nhiêu mặt/khoang phải ghi nhớ; kết quả có nhìn thấy từ control không? |
| Sửa sai | Sai một bước mất bao nhiêu thao tác để thử lại, có đường phục hồi không? |
| Điều khiển/chờ | Chạm trượt, chọn nhầm phần, đi vòng, đợi hoạt cảnh có lấn át suy nghĩ không? |

Màn thường tăng một trục chính; Boss tăng phối hợp các trục đã học. Giữ trục điều khiển thấp ở mọi độ khó. Không yêu cầu cân thời điểm trên vật trơn để làm khó hơn.

## 5. Thay đổi đề xuất theo chương

| Chương hiện tại | Trọng tâm sửa | Kết quả mong người chơi nhận ra |
|---|---|---|
| 1–10 | Giữ Boss10 và màn6; làm rõ vì sao cầu9 chặn thang; phát hiện sàn/tường/đồ vật có vai trò khác nhau; đo ink8 riêng | Tôi hướng dẫn một sinh vật có khả năng, không chỉ bấm nút tuần tự |
| 11–21 | Thử bản 11 khám phá Q an toàn, 12 luyện chọn/nhập; 16 chuyển thành vận dụng hai nút. Chỉnh trong ngân sách 65 sau khi playtest; không bê nguyên bài16 lên11 | Tôi có thể chia công việc rồi tụ lại để mạnh hơn |
| 22–32 | Giữ ống22–23 và bánh27–28; làm một bài bàn giao đơn giản ở30 hoặc31 trước Boss32 | Người đang giữ đường có thể được bạn thay ca |
| 33–43 | Giữ đu33–35; khai thác mượn bánh39; test xem Boss43 có tạo suy luận khác32 hay chỉ thêm phòng | Dùng xong không có nghĩa phải để vật ở đó mãi |
| 44–54 | Giảm đi lại/chọn phần ở47–52; một bài cân có tải tối đa trước 54; 54 tổng hợp, không giấu luật mới | Chọn đúng lượng cơ thể, không phải luôn lấy phần to nhất |
| 55–65 | Dạy tải người chạy/người đi thật rõ; thử thang cân60 trong bố cục gọn; 65 có 2–3 pha hoàn tất nhìn thấy được | Tôi phân bổ, thu hồi và tái sử dụng các nguồn lực |

Bản 11–12/Q là **phương án thử**, chưa đổi catalog hay bỏ G1/K02. Nếu thử không tăng tò mò hoặc gây ngộp, giữ thứ tự và dùng preview Q ngắn sau Boss. Không coi số màn bắt buộc phải cố định là điều quan trọng hơn bằng chứng chơi thử.

### Bốn nguyên mẫu có thể triển khai bằng cơ chế sẵn có

**Bàn giao đường — chuẩn bị cho32:** hai bờ thấp, một cầu, hai pad thay nhau duy trì cầu. A giữ pad gần, B sang pad xa, cầu vẫn mở; A rời chỗ và cùng về. Chỉ sau khi người chơi hiểu thay ca mới thêm khóa B/C/A và tuyến trần. Dùng sensor/latch/rail hiện có; kiểm tra tự nhập không phá nhiệm vụ khi pad gần nhau.

**Cân vừa đủ — chuẩn bị cho54:** một cân ba vạch, cả thân làm kim vượt vùng hợp lệ, 75% nằm trong vùng, một cửa. Không thêm nhiều khoang và tay gạt khóa trục ở bài giới thiệu. Ngưỡng tối đa là luật vật lý hiển thị bằng kim, không phải “màu đúng” hoặc điều kiện ẩn.

**Mượn một bánh — phát triển39:** máy A làm cầu mở và lẫy giữ lại, sau đó cùng bánh ấy chuyển sang máy B. Không thêm bánh chỉ để tăng số vật. Người chơi thấy rõ cầu vẫn được lẫy giữ dù máy A ngừng quay.

**Vật cản có việc thứ hai — phát triển14/59:** dời khối để lộ đường; vị trí cuối của nó lại chặn lối về nên phải đưa vào bãi tạm, sau đó dùng làm bậc/đối trọng. Hai vai trò phải hợp vật liệu và khả năng leo: khối trơn không bỗng trở thành mặt bám không có lớp hiển thị.

Không cần cơ quan hoàn toàn mới ở đợt đầu. Nếu các chu kỳ trên đã hoạt động tốt, có thể prototype **đệm đàn hồi nén/nhả**: tải nén, lẫy giữ năng lượng, chọn bến rộng rồi chạm nhả; không đòi canh thời điểm. Đây là tính năng mới chưa có bằng chứng runtime, phải thiết kế dossier, mô phỏng và kiểm tra mobile trước khi thêm vào campaign. Nó chỉ đáng làm khi tạo quan hệ mới về tích trữ/giải phóng năng lượng, không chỉ một animation phóng thân.

## 6. Cách bố trí để câu đố được đọc ngay

- Control, dây/mạch, vật chuyển động cùng một quan hệ màu đã duyệt; bổ sung vị trí chốt, hình dạng và chuyển động để không phụ thuộc màu đơn thuần. Không thêm màu mới cho mỗi vật.
- Một góc mở màn cho thấy vấn đề, đích và cơ quan chính. Cần orbit vì nó giúp khám phá quan hệ không gian, không vì tay nắm nhỏ bị che ngẫu nhiên.
- Mỗi thay đổi trạng thái có dấu vật lý: pin rút, lẫy bắt, dây căng, bánh ngừng/quay, kim cân lệch. Chỉ “đèn xanh” chưa giải thích nguyên nhân.
- Tín hiệu khi bị khóa, thiếu tải, quá tải, mất bám phải khác nhau. COghe thử/gồng/phản ứng giúp người chơi hiểu, không tự giải hộ.
- Một mục tiêu phụ vừa hoàn thành nên để lại thay đổi ổn định nhìn thấy được, giảm trí nhớ ngắn hạn. Điều kiện cần tiếp tục giữ phải nhìn khác điều kiện đã chốt.
- Giữ bến rộng, chạm cả thân control, lỗ/miệng ống rõ. Kính luôn bám; chỉ lớp trơn nhìn thấy mới bỏ bám.

## 7. Kế hoạch kiểm chứng retention

### Định nghĩa kết quả

Một giả thuyết làm việc: D1 phản ánh người chơi hiểu và muốn thử tiếp; D7 cần thấy nhiều cách dùng mới; D30 cần cả chiều sâu lẫn nội dung quay lại. Đây không phải quan hệ nhân quả đã đo. Home và cảm xúc hỗ trợ gắn bó nhưng không chữa được vòng chơi chính gây chán.

D1/D7/D30 tính trên cohort ngày first_open: phần trăm user quay lại đúng ngày tương ứng, với cùng múi giờ/định nghĩa ở mọi biến thể. Không trộn exact-day với rolling retention, không dùng cohort chưa đủ ngày. [Google Analytics giải thích retention theo cohort](https://support.google.com/analytics/answer/11004084?hl=en).

Chỉ có 65 màn không bảo đảm đủ trải nghiệm cho D30. Sau khi nhịp chơi chính tốt, thử các thí nghiệm tái phối hợp được thiết kế tay, nhiệm vụ Home dựa trên kỹ năng đã học, và màn tùy chọn. Không tăng độ khó bằng thao tác khó, không dùng đồ trả tiền để vượt puzzle, không kéo dài campaign giả tạo để đủ 30 ngày.

### Event cần bổ sung vào nền có sẵn

Firebase có sẵn chuẩn `level_start`/`level_end`; giữ chuẩn đó cùng stable ID hiện có. [Firebase event reference](https://firebase.google.com/docs/reference/android/com/google/firebase/analytics/FirebaseAnalytics.Event). Dùng một nguồn outcome chuẩn cho funnel, không cộng `level_end` và `level_complete` thành hai win.

| Bổ sung đề xuất | Câu hỏi trả lời |
|---|---|
| `mechanism_action`: type, target_id, result, reason | Người chơi thử đúng vật nhưng bị từ chối, hay chưa biết chạm gì? |
| `puzzle_milestone`: rule_id, state_id, elapsed_active | Bao lâu tới tiến triển đầu; kẹt ở pha nào? |
| `hint_shown`: tier, source, level_id | Tự giải, nhận gợi ý thao tác hay xem đáp án? |
| `level_continue`: from/to stable ID | Thắng rồi có chủ động bắt đầu màn sau không? |
| `session_checkpoint`: attempt_id, state_id, outcome_pending | Đối chiếu phiên bị ngắt/kill mà không có abandon event |
| `design_version`, `order_version`, `experiment_arm` | Không trộn dữ liệu trước/sau đổi màn hoặc đảo thứ tự |

Gộp thống kê chạm vô hiệu/chọn phần vào cuối attempt, không gửi mỗi frame. Chỉ ghi milestone khi đổi trạng thái thật. Không ghi “aha” như một sự thật chỉ vì gate mở: cần quan sát và hỏi người chơi. Có thể họ đoán trúng hoặc làm theo hint.

Đo thêm active time tách khỏi thời gian chờ hoạt cảnh/đi lại; frame time của thiết bị; hint/retry; win→màn kế; return→tiếp tục puzzle; abandon sau lỗi điều khiển. Tách chuyển sang Home/tiêm màu khỏi bỏ game. Tách nguồn traffic, thiết bị, app version và lần chơi đầu/replay.

### Ba tầng test

1. **Tính đúng:** solve, wander, thứ tự sai, thả người giữ, nhập sớm, Retry, đi tắt tường/nóc, 30/60 Update và native device. Không làm lại runtime mỗi lần cần một câu đố: dữ liệu cơ quan và đồ thị điều kiện dùng lại, kiểm tra navigation khi trạng thái đổi, không tính mỗi frame.
2. **Khả năng học:** mỗi vòng 5–8 người mới, không hướng dẫn đáp án; hỏi “bạn định thử gì/vì sao?” ở vài mốc, tránh hỏi liên tục phá nhịp. Cho bài bố cục khác để kiểm tra chuyển kiến thức. Lượt mẫu nhỏ chỉ tìm lỗi, không chứng minh D1 tăng. Đặc biệt thử16, 30–32, 48–54 trên người đã học các bài prerequisite nhưng chưa biết lời giải.
3. **Tác động thật:** A/B ngẫu nhiên theo user từ first_open, giữ biến thể ổn định; đổi một giả thuyết mỗi thí nghiệm. Giữ quảng cáo/phần thưởng/traffic tương đương hoặc phân tầng. Đặt trước metric chính, mức cải thiện tối thiểu, sample size theo baseline và thời gian đủ quan sát D7/D30. Không kết luận sau vài người hoặc dừng sớm vì một ngày đẹp.

Ngưỡng nghiệm thu nội bộ đề xuất, cần hiệu chỉnh: bài giới thiệu có ít nhất 80% người thử hiểu thao tác không xem đáp án; sau học có thể dự đoán kết quả ở bố cục mới; không ai bị kẹt không phục hồi; chạm bị từ chối trên mục tiêu hợp lệ dưới 5%. Đây là mục tiêu tác giả, không benchmark ngành. Màn luyện phải làm người chơi hiểu hơn, không chỉ qua nhanh hơn.

## 8. Thứ tự thực hiện

**P0 — dữ liệu và hợp đồng học:** snapshot phiên bản, bổ sung rule_id cho từng kiến thức, sửa checker theo rule thay vì chỉ họ component. Làm sạch phép đo lời giải gọn/thử sai. Hoàn thiện event và funnel hiện có trước khi quy kết chỗ rơi retention.

**P1 — vòng thử nhỏ:** ưu tiên bàn giao trước 32 và tải tối đa trước 54 vì đã xác định khoảng trống bằng code. Song song prototype Q khám phá sớm sau Boss10 để thử sức hút; giữ 6/10. Một nhóm màn mỗi vòng, không đổi 65 màn cùng lúc.

**P2 — giảm lặp/ma sát:** playtest 14–15, các màn thùng được coi là nghỉ, 18/47/49/52/56. Đường dài chỉ giữ khi có ý nghĩa về trình tự/nguồn lực. Sửa chọn phần và đọc trạng thái trước khi tăng cơ quan.

**P3 — tăng tuổi thọ nội dung:** khi D1 và mức hoàn thành các bài học tốt hơn, triển khai các biến thể dùng lại vật, đổi vai, đổi chiều và bài tuần tự nhiều tầng; đo D7/D30. Sau đó mới quyết định đầu tư cơ quan mới hoàn toàn.

Mỗi bản sửa có dossier theo [LEVEL_TEMPLATE](../Docs/LevelDesign/COghe/LEVEL_TEMPLATE.md), bản vẽ có chuỗi giải/đường sửa sai, rule prerequisite, một câu “aha”, nguồn evidence và kế hoạch đo. Định nghĩa thành công: **người chơi tự hiểu, thao tác được ý mình, thấy thành quả rõ và chủ động muốn khám phá tiếp**.
