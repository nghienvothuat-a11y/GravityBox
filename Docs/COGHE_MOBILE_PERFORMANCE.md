# COghe — hiệu năng OPPO, 17/09/2026

**Cập nhật sau phép đo OPPO:** đã giải thắng Boss 20 trên Mac và tối ưu thêm cache đồ thị, truy vấn cạnh và hàng đợi tìm đường. Bộ hồi quy mới đạt **80/81**; chỉ còn ca màn 04 dưới đây chưa đạt. [Báo cáo Mac và lời giải Boss 20](COGHE_BOSS20_PLAYTEST.md). Các số đo/APK OPPO trong báo cáo này là mốc trước thay đổi camera và tối ưu bổ sung; chưa dùng chúng để kết luận hiệu năng Android của bản mới.

Phạm vi: các màn Origin 11–20 trên OPPO CPH2591, Android 15, 720×1612, ARM64. Đối chiếu APK từ commit `a80110259c1b0935c645e4f03e7085dd0a4e8c2a` với bản tối ưu; cả hai là IL2CPP Release và dùng cùng kịch bản đo. Bản trước chỉ bổ sung instrumentation để thu số liệu.

## Kết quả thiết bị thật

| Màn | FPS trước: đứng/bò | FPS sau: đứng/bò | FPS xoay: trước → sau | p95 tệ nhất: trước → sau (ms) |
| --- | ---: | ---: | ---: | ---: |
| 11 | 21.0 / 21.1 | 50.3 / 49.9 | Khóa xoay | 49.8 → 33.2 |
| 12 | 9.7 / 9.1 | 49.9 / 49.6 | Khóa xoay | 116.1 → 33.2 |
| 13 | 7.9 / 8.2 | 46.3 / 45.9 | Khóa xoay | 149.2 → 33.2 |
| 14 | 12.4 / 12.3 | 50.2 / 49.7 | 7.9 → 43.2 | 281.7 → 33.2 |
| 15 | 27.6 / 18.2 | 47.7 / 47.7 | 26.0 → 46.1 | 49.8 → 33.2 |
| 16 | 8.9 / 8.1 | 46.2 / 45.6 | Khóa xoay | 132.6 → 33.2 |
| 17 | 10.4 / 10.4 | 47.5 / 47.8 | Khóa xoay | 116.0 → 33.2 |
| 18 | 11.3 / 11.1 | 49.5 / 49.5 | 10.0 → 47.4 | 265.1 → 33.2 |
| 19 | 8.0 / 7.4 | 46.6 / 46.1 | Khóa xoay | 165.7 → 33.2 |
| 20 | 4.6 / 4.5 | 45.5 / 44.9 | Khóa xoay | 265.2 → 33.2 |

CPU dựng mô/animation khi đứng yên: màn 13 **95,3 → 4,6 ms**, Boss 20 **180,2 → 5,1 ms**. Trong kịch bản đo không phát hiện mô thất lạc hoặc trạng thái thua bất thường.

**Giới hạn còn thấy trên máy:** Boss 20 có một khung hình **198,9 ms** ở pha ra lệnh/di chuyển, dù p95 là 33,2 ms; màn 13 và 16 có đỉnh khoảng 82,9 ms. Hiệu năng trung bình đã cải thiện rõ, nhưng chưa hết mọi lần khựng và chưa đạt 60 FPS ổn định.

FPS là số khung hình thực tế chia thời gian đo. `p95` là thời gian mà 95% khung hình không vượt quá; thấp hơn là tốt hơn. Một lượt gồm tải scene, 2 giây ổn định, 5 giây đứng yên, một lệnh di chuyển rồi đo 8 giây; các màn cho xoay có thêm 10 giây đổi hướng nghiêng. Các hàng xoay chỉ xuất hiện ở màn cho phép. Không dùng Deep Profiling.

Đây là phép đo một chuỗi thao tác cố định, không phải chơi giải trọn từng màn trên điện thoại. Nhiệt độ máy, sạc USB, hệ điều hành và hoạt động nền có thể làm kết quả dao động. Không xem FPS trung bình là cam kết 60 FPS hoặc bằng chứng không còn khựng ở mọi tình huống. Thời gian CPU trong dữ liệu là từng khối code đo bằng Stopwatch; `gameStepMs` không bao gồm toàn bộ bộ giải PhysX, `graphMs` nằm trong các lời gọi liên quan và không được cộng lại để suy ra tổng CPU.

Dữ liệu gốc: [trước tối ưu](Verification/COgheMobile/baseline.jsonl), [sau tối ưu](Verification/COgheMobile/optimized.jsonl). [Kết quả kiểm thử và SHA-256 của APK](Verification/COgheMobile/validation.json). APK thử đo và log đầy đủ lưu local trong `Artifacts/COgheAndroid/`.

## Nguyên nhân và thay đổi

- **Dựng mô và xúc tu:** trước đây mỗi đỉnh mô và mỗi tia dò của xúc tu đều kiểm tra nhiều bề mặt/collider ở xa. Nay chụp trạng thái hình học một lần cho mỗi lượt dựng, lọc theo vùng từng phần cơ thể, rồi gọi kiểm tra chính xác với các vật thể gần. Raycast thật vẫn quyết định va chạm và điểm bám.
- **Dựng mesh:** tái sử dụng bộ đệm, bỏ tính góc/pháp tuyến cho ô hoàn toàn trong hoặc ngoài mô; dùng lại giao điểm cạnh chung giữa các tứ diện. Giữ nguyên độ mịn, thứ tự tam giác và animation, dựng lại mỗi khung hình.
- **Đường đi:** tìm cặp nút gần nhau bằng lưới không gian thay cho so mọi cặp; dùng lại danh sách và dữ liệu collider. Lọc theo bounding box trước phép cắt mặt/raycast chính xác. Giữ thứ tự cạnh để đường đi đồng chi phí không đổi tùy thứ tự duyệt hash.
- **Mô và cơ quan:** lưu vị trí/vận tốc hạt một lần trong lượt cộng lực; chỉ gọi truy vấn vận chuyển/cắt mô trên các cơ quan có khả năng đó. Không giảm 32 hạt, khối lượng, tốc độ di chuyển, gia tốc, bước vật lý 120 Hz hoặc độ chi tiết Day Lab.
- **Màn 14:** cầu và cửa ray dùng Continuous Speculative CCD để bao gồm chuyển động quay; tăng số vòng giải khớp, đặt khoảng tiếp xúc giới hạn ray 6 mm, projection nhỏ để khôi phục sai lệch khớp. Cầu/cửa vẫn chạy bằng trọng lực và khớp thật, không đặt lại pose để giả chuyển động.

## Kiểm chứng và lỗi còn lại

Bộ PlayMode cuối: **74/76 đạt** (`Artifacts/COgheExpansion/perf-final5.xml`). Nhóm màn 11–14 7/7; ống 5/5; ghép cầu 3/3; tích hợp 1/1; hồi quy hiệu năng/hình học 2/2; cơ quan 11/12; Origin 45/46.

Đối chiếu truy vấn trước/sau tại tất cả màn 11–20, gồm pose xoay, miệng lỗ và bề mặt cơ quan: vị trí cắt mô, pháp tuyến, đường trống/bị chặn, điểm bị chiếm, tia xúc tu và collider trúng phải tương đương. Bounding box chỉ loại ứng viên xa, không trở thành collider mới. Giữ phép tính chính xác tại các mép đồng phẳng; nới vùng loại thô 1 cm để không bỏ sót collider mỏng/collider ghép.

Bài stress màn 14 chạy năm hướng nghiêng mạnh, mỗi hướng 600 bước ở 120 Hz. Sai lệch neo bản lề lớn nhất **0,473 mm**, sai lệch ray **0,110 mm**, lệch góc cửa **0°** trong lượt kiểm tra. Đường giải đầy đủ màn 14 đạt: leo đúng mặt cầu đang lộ ra, đặt cầu bằng trọng lực, băng qua và mở cửa. Test cũ ra lệnh vào mặt bệ nằm sau tấm cầu đã được sửa thành điểm có thể tiếp cận; không dịch chuyển sinh vật/cửa để vượt test.

Hai ca chưa đạt đã tồn tại trước đợt tối ưu:

- **Màn 04:** sau khi trượt, đường thử quay lại đường bám an toàn chưa dẫn tới thắng. Không sửa luật trơn trong đợt này.
- **Boss 20:** đường thử toàn màn còn làm hai phần B/C chạm và nhập lại trước khi giữ đủ ba vai trò. Bài thử đã chờ xung lực chém tách mô và kiểm tra rõ số phần; không vô hiệu luật tự hợp thể để ép test qua. Các kiểm tra riêng tải nút, truyền động, cửa, chém và điều kiện thắng đạt, nhưng chưa chứng nhận đường giải toàn màn này.

Không đánh dấu toàn bộ campaign xanh. Các lượt đo FPS không thay thế hai ca giải màn còn lỗi.

## Quy tắc khi thêm cơ quan

`COgheMechanism` khai báo `HasSkinConstraint`, `TransportsTissue`, `SeparatesTissue`, `ControlsExit` đúng với hàm nó triển khai. Bộ lọc được tạo sau `InitializeMechanism`; các khả năng này phải ổn định trong vòng đời scene. Bật/tắt `isActiveAndEnabled` vẫn được kiểm tra lúc sử dụng. Nếu thêm/bớt cơ quan hoặc collider lúc đang chơi, phải cập nhật danh sách tương ứng; không tái sử dụng cache từ scene cũ.

Snapshot hình học chỉ sống trong một lượt truy vấn: mỗi lần dựng mô, dò animation, xây đồ thị hoặc bước hỗ trợ bám đều đọc lại pose thích hợp. Không giữ bounds của cơ quan động qua nhiều khung hình. Khi thêm kiểu ràng buộc mô mới, `MayConstrainSkin` phải là phép loại thô bảo thủ: có thể nhận dư, không được bỏ sót điểm mà phép kiểm tra thật có thể sửa.

## Chạy lại

```sh
bash Tools/verify-coghe-expansion.sh
COGHE_BENCHMARK=1 bash Tools/build-venom-android.sh
adb -s "$COGHE_ANDROID_DEVICE" install -r Builds/Venom/Android/COghe.apk
adb -s "$COGHE_ANDROID_DEVICE" shell am force-stop com.gravityboxlab.venom
adb -s "$COGHE_ANDROID_DEVICE" shell am start -n com.gravityboxlab.venom/com.unity3d.player.UnityPlayerGameActivity --es coghe_benchmark rerun
adb -s "$COGHE_ANDROID_DEVICE" pull /sdcard/Android/data/com.gravityboxlab.venom/files/benchmark-rerun.jsonl
```

Đợi log `COGHE_BENCHMARK_DONE` trước khi lấy tệp cuối. Trong lúc đo không đổi màn hoặc thoát app. Harness không lưu tiến trình campaign.

Build để chơi: `bash Tools/build-venom-android.sh` (không đặt `COGHE_BENCHMARK`); macOS: `bash Tools/build-venom.sh`. Bản chơi không chứa thành phần tự chạy đo. Đầu ra: `Builds/Venom/Android/COghe.apk` và `Builds/Venom/macOS/Venom.app`.

Bản chơi cuối đã cài cập nhật lên OPPO bằng `adb install -r`, giữ dữ liệu app. SHA-256 trên máy khớp APK build; mở bằng Activity bình thường, không truyền lệnh benchmark. macOS cũng đã build thành công.
