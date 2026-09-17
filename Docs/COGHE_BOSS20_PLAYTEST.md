# Boss 20 — cách giải và đo hiệu năng macOS

17/09/2026. Màn này đã được giải thắng trên bản macOS chạy thật, bằng lệnh di chuyển và thao tác cơ quan, không dịch chuyển vật thể, gán nhóm hoặc mở cửa bằng code. Lượt trình diễn dùng kịch bản điều khiển phát lại theo thời gian thực; vật lý vẫn quyết định cắt, bám, tải nút, sức kéo, hợp thể và thoát.

## Cách giải

1. **Cắt lệch ở máy chém khoang 1.** Nhắm khoảng 1/3 cơ thể ở phía trái và 2/3 ở phía phải. Phần giữ A cần đủ tải, phần đi tiếp cần đủ sức nâng G; cắt đôi bằng nhau có thể khiến G quá nặng.
2. **Phần nhỏ đứng lên A.** Giữ nó ở đó. Dẫn phần lớn leo vào ống trên vách ngăn để sang khoang giữa, rồi bò xuống bệ phía bên kia.
3. **Nâng bánh răng G.** Dẫn phần lớn tới chân cụm bánh răng, chạm tay nắm G để bám rồi chỉ vị trí cao hơn để kéo lên. Kéo tới khi bánh răng giữa khớp hai bánh răng bên cạnh và chốt cuối hành trình giữ lại.
4. **Đi qua máy chém thứ hai.** A vẫn được giữ. Sau nhát cắt, lấy phần ở bên phải đi sang khoang 3 qua ống trước. Không đổi chéo hai phần: gần nhau là tự hợp thể.
5. **Phần bên trái vòng sau cụm bánh răng để giữ B.** Đi vòng phía trái giá G, tới vùng sàn phía sau giá, rồi mới rẽ tới B. Đi thẳng xuyên chân giá sẽ mắc vật cản. Phần C đã ở khoang 3 nên không va nhập với phần B.
6. **Kéo C trong khi A và B đều được giữ.** Chạm tay nắm C, kéo sang phải; giữ lệnh cho tới khi cả hai cửa mở hết và chốt lại. Ba vai trò này phải tồn tại đồng thời.
7. **Cho cả ba phần gặp nhau ở khoang 3.** Lúc cửa đã chốt mở, các phần A/B được rời nút và đi qua cửa. Tới gần nhau chúng tự tụ lại.
8. **Hợp thể kéo nắp H rồi chui ra lỗ.** Cơ thể đầy đủ mới đủ sức kéo nắp cuối. Chỉ ra ngoài sau khi đã tụ; cả 32 phần tử vật chất phải thoát qua miệng lỗ.

Nút **Toàn cảnh** giúp xem quan hệ A–G–B–C–H. Dùng **Khoang 1/2/3** để chọn cơ quan rõ hơn, và hàng **Phần 1/2/3** để đổi phần điều khiển. Số phần trên HUD là thứ tự hiện tại, không phải tên vai trò A/B/C cố định.

## Điều đã sửa trong đường thử

Đường thử cũ chọn B/C theo kích thước rồi cho chúng đổi chéo vị trí ngay sau chém; chúng chạm nhau và tự tụ theo đúng luật. Đường mới giữ thứ tự trái/phải, đưa C sang khoang 3 trước và vòng B ra sau giá G. Không sửa sức bám, khối lượng, dao, ngưỡng tải hay khoảng tụ để làm test qua.

`Level20FullSolutionUsesMassOrderThreeRolesAndMergedEscape` đã đạt. Cả lượt chạy macOS trước và sau tối ưu phải ghi `COGHE_DEMO_WIN escaped=32` mới được coi là giải xong.

## Phương pháp đo

Máy Apple M5, Mac17,2; bản macOS Development, cùng kích thước render 960×1600, giới hạn 60 FPS, không Deep Profiling. Khởi động scene, chờ ổn định 3 giây, rồi chạy cùng chuỗi lệnh giải toàn màn. Không chạy Unity Editor/build/test đồng thời trong thời gian đo. Dùng thời gian khung hình thực tế, ghi riêng dựng mô, gameplay, dựng graph và tìm đường. Hai vùng CPU có thể lồng nhau; không cộng các cột để suy ra tổng CPU.

Đây là lượt chơi mẫu tự động qua API điều khiển của game, không phải robot touch từng pixel. Thao tác chạm thực đã có trong bộ hồi quy, và lượt đầu cũng được kiểm tra trực tiếp trên cửa sổ Mac. Không sửa trạng thái thắng/cửa hoặc đặt lại vị trí sinh vật trong kịch bản.

| Chỉ số toàn lượt giải | Trước | Bản bàn giao |
| --- | ---: | ---: |
| FPS trung bình | 46,57 | 57,33 |
| p95 thời gian khung hình | 16,99 ms | 17,03 ms |
| p99 thời gian khung hình | 249,82 ms | 49,33 ms |
| Khung hình chậm nhất | 950,38 ms | 84,25 ms |
| Số khung hình trên 50,5 ms | 61 / 3.639 | 15 / 3.842 |
| CPU dựng graph lớn nhất trong một khung hình | 913,52 ms | 55,47 ms |
| CPU tìm đường lớn nhất trong một khung hình | 11,76 ms | 2,43 ms |

Lượt trước dài 78,13 giây, lượt bản bàn giao 67,02 giây; cùng thứ tự thao tác, chờ cơ quan bằng trạng thái vật lý thay vì ép đồng hồ. Cả hai thắng với `escaped=32`. FPS = số khung hình / tổng thời gian; p95/p99 thấp hơn là tốt hơn. p95 gần như không đổi vì trước tối ưu đa số frame vốn đã nhanh, nhưng có ít frame rất chậm; p99 và frame lớn nhất phản ánh phần khựng được xử lý.

Một lượt kiểm tra trước đó của cùng phương án tối ưu đạt 57,90 FPS, max 67,48 ms. Bảng dùng lượt xác nhận build cuối, không lấy số đẹp nhất. Vẫn có khựng ngắn lúc dựng graph cho cơ quan chuyển động; chưa đạt 60 FPS ổn định tuyệt đối. Số liệu Mac không thay thế phép đo lại OPPO.

Dữ liệu: [so sánh tổng hợp](Verification/COgheMac/comparison.json), [khung hình trước](Verification/COgheMac/baseline.csv), [khung hình sau](Verification/COgheMac/optimized.csv), [từng pha trước](Verification/COgheMac/baseline.jsonl), [từng pha sau](Verification/COgheMac/optimized.jsonl), [kiểm chứng và dấu vân tay source](Verification/COgheMac/validation.json). CSV trước đã chuẩn hóa dấu nháy quanh tên pha có dấu phẩy; giữ nguyên mọi giá trị đo. Bản gốc nằm tại `Artifacts/COgheMac/baseline-demo.csv`. Trường `graphBuilds` trong JSONL đếm số frame có dựng graph, không đếm số lần gọi trong cùng frame. CPU và delta time có thể lệch một frame do thời điểm lấy mẫu; so sánh tổng hợp thay vì cộng từng hàng.

## Tối ưu

- Mạng đường đi chỉ dựng lại khi mặt kính, vật di động hoặc trạng thái lỗ thay đổi. Lệnh chọn vật, đi tới vật và buông vật không còn dựng trùng một graph khi hình học giống nhau. Reset vẫn dựng mới bắt buộc.
- Bộ kiểm tra đường dùng cây bounding box để loại các bề mặt ở xa. Sau lọc, vẫn kiểm tra mặt phẳng, miệng lỗ và collider thật như trước; các yêu cầu clearance lớn dùng đường kiểm tra đầy đủ.
- Dijkstra dùng hàng đợi ưu tiên và bộ đệm dùng lại, thay cho quét toàn bộ nút mỗi bước và cấp phát mảng mỗi lệnh. Giữ nguyên quy tắc xử lý đường đồng chi phí theo chỉ số nút.
- Giữ 32 phần tử mô, 120 Hz vật lý, số đỉnh mesh, animation, camera và chất lượng dựng hình. Không hạ độ phân giải để cải thiện số đo.

Ngưỡng tái sử dụng pose của graph: 0,05 mm và 0,01 độ, nhỏ hơn khoảng hở tìm đường 6 mm. Thay đổi trạng thái lỗ/kích thước mặt/bật tắt mặt được phát hiện trực tiếp. Khi thêm/xóa collider hoặc thay mesh collider trong runtime, gọi `Motion.BuildGraph(true)` để vô hiệu cache ngay.

## Hồi quy

- So truy vấn hình học trước/sau tối ưu ở tất cả màn 11–20, cả tư thế xoay và bề mặt cơ quan.
- So đường Dijkstra dùng heap với bản quét cũ, gồm thứ tự đường đồng chi phí và waypoint tại góc kính, ở các màn 08/12/13/14/16/19/20.
- Kiểm tra lệnh lặp dùng lại graph; đổi lỗ hoặc di chuyển cửa làm graph hết hiệu lực.
- So toàn bộ nút/cạnh của graph dùng cache với graph dựng mới bắt buộc tại bốn pose cơ quan và trạng thái lỗ khác nhau ở màn 13/14/19/20.
- Bộ PlayMode: **80/81 đạt**. `Level20FullSolutionUsesMassOrderThreeRolesAndMergedEscape` đạt. Ca chưa đạt là `FourthLessonPeelsAndFallsWhenClimbingIntoSlipperyBand`: đường thử sau trượt chưa thoát được màn 04, đã tồn tại trước đợt này. Không ghi toàn campaign đạt.

## Chạy lại trên Mac

Đóng phiên game cũ trước khi chạy. Build: `bash Tools/build-venom.sh`.

```sh
Builds/Venom/macOS/Venom.app/Contents/MacOS/Venom \
  -screen-fullscreen 0 -screen-width 960 -screen-height 1600 \
  -coghe-profile boss20-rerun -coghe-profile-mode demo
```

Đo chơi tay: đổi `demo` thành `manual`, bấm F8 để kết thúc thu số liệu. CSV/JSONL ghi trong `Application.persistentDataPath`, đường dẫn được in ra Player log bằng `COGHE_MAC_PROFILE_DONE`. Kịch bản chỉ tồn tại trong Development/Editor và chỉ kích hoạt khi truyền tham số; mở app bình thường không tự chơi và không bật overlay. Phiên đo không lưu tiến trình.
