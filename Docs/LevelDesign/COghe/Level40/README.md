# COghe — Màn 40 — BOSS · Cỗ máy đoàn tụ

## 1. Định danh, phạm vi và trạng thái

ID đề xuất `venom.origin.40` · vị trí chơi **40** · BOSS · Cỗ máy đoàn tụ. Người thiết kế: Codex, 21/09/2026. **Nháp v1** theo yêu cầu Mrk; không tự ghi là đã duyệt. Hồ sơ và phác thảo mới, không thay scene cũ. Chưa có scene/definition/builder mới; chưa kiểm chứng Editor/Mac/mobile.

Thiết kế: đủ brief/lời giải/phục hồi để review. Prototype, hoàn thiện và nghiệm thu: chưa thực hiện. [Hợp đồng chương và nguồn](../Sketches31-40/README.md).

## 2. Mục tiêu trải nghiệm và vị trí trong tiến trình

Chuẩn bị máy khi còn đủ lực, chia việc theo ba khoang, hoàn tất hai đầu ra rồi thu hồi cả cơ thể.

Dùng kỹ năng bò/leo/luồn/đẩy/kéo đã có, cùng cắt/tụ và giao việc cho phần cơ thể. Vai trò: Boss tổng hợp, không hướng dẫn lời giải. Kết quả chỉ ghi hoàn tất màn, không cấp buff hoặc mở lại Nhà. Điểm mới là quan hệ cơ quan trong bố cục này, không phải kỹ năng có thể mua.

## 3. Phác thảo, hình học, camera và thao tác

![Màn 40 — BOSS · Cỗ máy đoàn tụ](mockup-v1.png)

Ba khoang kiểu màn 39, ống chuyển hai chiều trên cao, D1/D2 đoàn tụ sát sàn. Khoang trái chứa spawn, giá truyền Q nặng, chốt P, dao 1 và A. Q ban đầu che hốc P và bị P chặn trước ổ ăn khớp; kéo lùi Q lộ P, rút P rồi đẩy Q vào ổ. Giữa có dao 2 và B. Phải có C chọn I/II: I mở/chốt D1/D2; chỉ khi cả hai chốt mở thật mới nhả chặn II; II rút/chốt khóa nắp H cuối. H vẫn cần lực kéo trên ray ngang để lộ lỗ. A/B cần được giữ trong cả hai nhịp C. Chỗ tụ đủ rộng cạnh H, cách lỗ để mô chưa tụ không vô tình ra ngoài.

Khóa xoay; tổng quan 3/4 thấy spawn, cơ quan mục tiêu và cửa cuối. Nhìn gần/theo phần chọn chỉ đổi camera. Màn nhiều khoang có nút xem khoang; không che trạng thái phần đang giữ ở khoang khác. Các bản vẽ không theo tỷ lệ. Kích thước khởi điểm để blockout: bệ tụ khoảng 0,18 × 0,18 m; đường chân bám dọc ray khoảng 0,12 m; tay nắm 0,05–0,06 m. Đây là giả thuyết, phải kiểm tra theo kích thước/tầm giãn mô thật; chưa chốt clearance ống. Không dùng ngưỡng khối lượng vô hình tại ống.

Chạm sàn/đích → bò/leo; chạm tay → tới bám; chạm đầu hành trình → đẩy/kéo; đổi lệnh → hủy lực cũ. Nóc/trơn vẫn nhận đích hợp lệ; mặt trơn không sinh lực bám. Retry/pause/zoom/chọn phần nằm ngoài hộp và safe area.

## 4. Lời giải, kết thúc và phục hồi

1. Khi còn nguyên khối, kéo Q lùi để lộ P.
2. Kéo P rút chốt; đẩy Q tiến vào ổ truyền và chốt giữ.
3. Dùng hai dao, đưa các phần tới ba khoang bằng ống; giữ A và B.
4. Phần thứ ba kéo C về I, nâng/chốt D1 và D2.
5. Giữ A/B, đổi C sang II để rút và chốt khóa H.
6. Buông cơ quan; gọi phần ở A/B tới vùng tụ qua hai cửa sàn.
7. Tụ lại, kéo H sang hốc chứa để lộ lỗ cuối.
8. Dẫn toàn bộ bản thể qua lỗ; dùng ăn mừng hiện hành.

**Sửa sai:** Chia sớm khi Q chưa đặt: về qua ống, tụ lại kéo Q rồi cắt tiếp. Bỏ một nút thì tời dừng/phanh, không hủy chốt. Kéo H sớm gặp chốt thật. Mọi ống hai chiều, không nhốt mảnh. Tụ sớm được phép, dao vẫn tiếp cận được.

**Điều kiện hoàn tất:** tụ trong hộp trước khi ra FinalExit, rồi đủ toàn bộ 32 hạt. Cửa/ống nội bộ chỉ chuyển khoang. Chấp nhận cách giải khác đúng vật lý, không khóa theo lịch sử từng bước. Retry trả vị trí/vận tốc, chốt, tải, ly hợp, lệnh và mô về trạng thái đầu; pause dừng mô phỏng, không để callback cũ mở máy. Mất phần/cửa kẹp/không quay lại tay nắm được là lỗi prototype cần sửa, không coi Retry là lời giải thường lệ.

## 5. Cơ quan, trạng thái và kiến trúc dùng lại

**Quan hệ:** `Q lùi → P rút → Q khớp; Q AND A AND B AND C=I → D1/D2 chốt; D1/D2 chốt AND Q AND A AND B AND C=II → khóa H rút/chốt; kéo H bằng lực thật → hợp nhất/thoát đủ mô`.

Kết hợp chốt lộ tay của 31, tải tập trung của 38 và tời hai đầu ra của 39. Không thêm kỹ năng mới tại Boss; cần driver cấu hình dùng lại, reset đầy đủ.

Dùng `VenomMovableProp`, `COgheRailSlider`, `COgheGearTrain`, `COgheTissueSensor`, `COgheGuillotine` và cooperative winch khi hợp đồng phù hợp. Đây là hướng tái dùng, không khẳng định driver mới đã tồn tại. Motor mô-men hữu hạn, điểm bám/phản lực thật, chốt chỉ bắt ở cuối hành trình thật. Mọi cơ quan reset cùng scene; ngắt nguồn không được animation tiếp tục truyền lực. Pose cơ quan/cửa đổi phải cập nhật route và aperture. Không rải nhánh theo số màn hoặc đọc lời giải mẫu trong runtime.

## 6. Hình ảnh, animation và phản hồi

Chuỗi chuyển động Q–nguồn–ly hợp–tời nhìn từ trái sang phải. Từng chốt có tư thế đóng/mở đọc được, C bị chặn khi chưa xong I. Khóa H rút khác rõ nắp H đã mở.

Chuỗi phản hồi: nhận chạm → đi tới → bám → truyền lực → vật chuyển/va chặn → chốt thật. Đèn chỉ báo trạng thái đo được; âm khớp/catch ngắn, không thay tín hiệu hình. Sinh vật không tự giải bước tiếp theo. Phác thảo tạo bằng imagegen tích hợp; prompt và manifest ở thư mục review. Khi dựng cần ảnh Unity thật theo Day Lab 07. Ăn mừng/chuyển màn dùng luồng hiện có; Boss chờ lựa chọn tiếp tục.

## 7. Độ khó và ngân sách runtime

Dự kiến **8 cụm quyết định**, tối đa **3 vai trò cùng lúc** trong lời giải mẫu; đây không phải số cú chạm tối thiểu. Mục tiêu lần đầu 8–12 phút, **chưa đo**. Độ chính xác/thời điểm giữ thấp; cho phép dừng quan sát giữa các bước.

Rủi ro riêng: Cổng H không mở chỉ bằng việc tới khoang phải; có chốt thật. C không tự chạy sau buông. Q/H dùng lực thật, chấp nhận cách chia lệch/đẩy chung hợp lệ; không kiểm tra đúng ba phần hoặc đã tụ để cho vật chạy. Boss không có tutorial, số bước hay mũi tên lời giải trong game.

Giữ baseline 32 hạt/120 Hz, Day Lab một key light có bóng. Dự kiến điểm nặng: pose cơ quan → cập nhật đường đi, tiếp xúc ray/catch và tách/tụ/đổi phần. Chưa chốt ngân sách collider/joint/GC/GPU; số cơ quan không chứng minh FPS. So cùng thiết bị với màn cơ khí/tam hợp hiện có, không giảm luật vật lý để đạt chỉ tiêu.

## 8. Chơi thử và hồi quy

Chưa chạy gameplay tests cho các màn mới. Checklist khi prototype:

- Giải trọn bằng chạm/API điều khiển thật, không teleport mô/gán cửa mở; thử cả camera tổng quan và gần.
- Làm ngược thứ tự, kéo dở, buông sau timeout, đổi phần, mất tải giữa hành trình, pause/reset ở mọi chốt.
- Cổng H không mở chỉ bằng việc tới khoang phải; có chốt thật. C không tự chạy sau buông. Q/H dùng lực thật, chấp nhận cách chia lệch/đẩy chung hợp lệ; không kiểm tra đúng ba phần hoặc đã tụ để cho vật chạy. Boss không có tutorial, số bước hay mũi tên lời giải trong game.
- Với nhiều phần: cắt lệch, tụ sớm, nhiều hơn số vai trò mẫu, mọi phần có đường về; đủ mô, không nhập xuyên kính, không thoát sớm. Với một thân: thử leo vòng vách/tay nắm và tiếp cận từ nóc.
- Với ray/ống/cầu: đo nhiều đích chạm lệch; không kẹp, không mắc tại chặn, không bám lại liên tục để giải.
- Playtest người chưa biết lời giải và replay người đã biết: ghi số người/lượt, thời gian suy luận riêng thao tác, điểm chạm sai, lần thử lại và điểm không hiểu.
- Khi sửa shared runtime chạy toàn bộ EditMode/PlayMode của package và hồi quy các scene dùng chung.

## 9. Bằng chứng hiệu năng trên thiết bị

**Chưa có build, thiết bị, mẫu frame-time hoặc FPS cho màn này.** Chưa có chứng nhận performance. Khi dựng: đo cùng máy/profile với baseline 30 màn, ghi commit+diff+build hash, resolution/safe area, warm-up, thời lượng thực; tách CPU/GPU, p50/p95/p99/max, GC/memory, spike lúc ra lệnh/cắt/tụ/cửa đổi. Mục tiêu chung 60 FPS cần đo trên mobile; thử phiên 15–20 phút để quan sát nhiệt. Không dùng hình minh họa hoặc test logic thay số đo.

## 10. Tích hợp, tương thích và nghiệm thu

Vị trí 40/ID 40 là đề xuất mới, chưa đăng ký catalog, scene, save hoặc build list; giữ nguyên các ID cũ. Khi tích hợp kiểm tra mở tiếp/replay/save idempotent, mất điện giữa lưu và ăn mừng; không cấp Nhà lại. Màn 40 kết thúc chương mới, chưa tự đặt màn 41.

Kết luận: **bàn giao thiết kế minh họa**, chưa nghiệm thu playable. Bước tiếp theo nếu triển khai là blockout và kiểm chứng rủi ro nêu trên trước art; không cần build game để review tài liệu này. Lịch sử v1: tạo theo yêu cầu 21/09/2026.


## 11. Cập nhật triển khai và kiểm chứng — 21/09/2026

Phần trên lưu thiết kế minh họa v1. Màn 40 hiện đã được dựng thành `Assets/_Game/Venom/Campaign30/COgheOrigin40.unity`, đăng ký campaign40 màn với ID ổn định `venom.origin.40`. Các khoảng hở, tay nắm và góc nhìn được hiệu chỉnh qua mô phỏng vật lý; quan hệ cơ quan và thứ tự giải giữ theo concept.

Luồng đã chạy trong app macOS: **Nguyên khối xử lý Q/P, chia ba vận hành A/B/C qua I rồi II, hợp thể kéo H và thoát.** Hợp thể trước khi ra cửa; ảnh trước thoát ghi1 phần/0 hạt thoát, ảnh thắng ghi1 phần/32 hạt thoát; không thua. Kiểm tra reset sau thắng và chọn các điều khiển nhìn thấy trong trạng thái đầu màn đều đạt.

Toàn bộ suite: **341/341 PlayMode,8/8 EditMode**. Cả10 lời giải đạt trong bản macOS Development thực tế; chưa đo hiệu năng hay chơi chạm tay trên thiết bị mobile. [Hồ sơ triển khai chung](../CHAPTER_31_40_IMPLEMENTATION.md) ghi trạng thái nguồn, các điều chỉnh và giới hạn kiểm chứng. Bằng chứng màn này: `Artifacts/chapter40-player-proof/20260921T135858138Z/level-40.json`, `level-40-open.png` và `level-40-won.png`.
