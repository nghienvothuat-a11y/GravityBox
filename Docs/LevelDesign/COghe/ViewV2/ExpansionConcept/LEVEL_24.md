# COghe V2 — 24 · Bạn đứng giữ

## 1. Định danh, phạm vi và trạng thái

ID đề xuất `coghe.view.v2.24`; vị trí V2 24; dossier v1 ngày 28/09/2026, Codex. Bản nền source `0ab0931b6d9e2ddf967dd35eade6d606bf25e427`. **Nháp thiết kế**, chưa scene/definition/Unity test/build mới. Theo yêu cầu làm 20 màn tiếp và gửi minh họa trước; chi tiết này là đề xuất. [Nguồn, hợp đồng và kế hoạch chung](DESIGN.md). Prototype, hoàn thiện và nghiệm thu đều **chưa kiểm chứng**.

## 2. Mục tiêu trải nghiệm và vị trí

Một phần đứng giữ để phần kia đi qua; nhiệm vụ giữ vẫn chạy khi đổi chọn. Vai trò: **Giới thiệu áp lực duy trì**. 3 quyết định chính, hai vai trò. Kỹ năng theo tuyến ở DESIGN; hướng dẫn bài mới đọc sự kiện thật, bài luyện giảm chỉ dẫn. Boss không hướng dẫn lời giải; trạng thái khóa/chạy/khớp vẫn rõ. Không chỉ số mua, buff Nhà hoặc phần thưởng tăng lực.

## 3. Phác thảo, hình học, camera và thao tác

[Bảng hình 04_LEVELS_23_26](GALLERY.md). S = spawn; E = FinalExit, A/B/C/P/G = điểm thao tác, không phải thứ tự bắt buộc ngoài điều kiện vật lý.

Pad A ở hốc trái S; cửa tạm chặn tới rail B. B nhả chốt cửa E ở phía S, rồi phần đi quay lại qua cửa tạm đang được giữ. Vách ngắn ngăn tụ ngay cạnh pad.

Kích thước/clearance sẽ chốt bằng greybox theo bán kính hạt/thân, chưa lấy tỷ lệ ảnh AI làm số đo. Mặt sàn/dốc/đường về có bám; mặt phủ trơn nếu cần phân cách phải có viền/chất liệu satin và collider thực, không cấm route vô hình. Hộp khóa xoay vật lý; camera yaw quanh Y, pitch cố định theo V2, pinch/Toàn cảnh. Kiểm tra portrait 720×1280 và 720×1612, safe area, tất cả tay nắm nhìn và tới được. Chạm mặt → đi; tay nắm → một hành trình; pad → giữ/nhả; chip phần → chọn, không tự zoom.

## 4. Lời giải, kết thúc và phục hồi

1. Tách và giao phần đủ tải giữ A.
2. Chọn phần còn lại qua cửa tới B, thao tác B tới chốt.
3. Đưa phần đi về sân cạnh S; gọi phần giữ ra hoặc đến gần đúng phía để nhập; E.

Tín hiệu trước/sau dựa vị trí/lực thật: khóa còn cài → mở khóa, rail chuyển → khớp, pad có tải → không tải. Đèn không bật theo lần chạm. Đây là lời giải tác giả ngoài runtime.

Phục hồi: Rời A sớm: cửa về vị trí đóng có chống kẹp, phần ở xa còn lối lùi; giữ A lại để mở. Cắt lệch thì đổi vai, cắt lại khi cần.

Thắng khi đã nhập trong hộp trước lần ra đầu tiên và đủ 32 hạt qua E; ống chuyển không tính thắng. Ra sớm khi chưa nhập: “bạn phải hợp thể trước khi chui ra”. Không cooldown/tấm chắn nhiệm vụ khỏi tụ. Cho lời giải khác hợp vật lý; phải rà đường leo vách, luồn khe và truyền tải bằng một thân, không gán thắng theo số hành động. Retry hủy lực/lệnh/ownership/callback và reset toàn puzzle/camera; pause/resume không sinh tap hay lực cũ. Save không bị xóa.

## 5. Cơ quan, trạng thái và kiến trúc

Pad A → cửa tạm; B → chốt cửa E. Output B giữ sau khi buông pad.

Nền cần dùng/thêm: COgheTapPad + sensor/temporary gate; chống kẹp và split V2 cần test. Scene và component mới chưa được tạo. Reset trả về trạng thái đầu chưa chốt; trạng thái thực quyết định graph và aperture. Mọi thay đổi hình học phải invalidate route trước nhận lệnh mới; không đi xuyên cửa trong thời gian chờ. Giữ ID mô/khối lượng/lực hữu hạn; owner task phải được xử lý khi đổi/cắt/tụ. Không thêm nhánh theo số màn hoặc đọc lời giải tác giả để runtime tự giải.

## 6. Hình ảnh, animation và phản hồi

Giữ kit V2 hiện tại: sứ kem, vách xanh dày, amber cho tay nắm/vật động, mint cho kết quả thật; COghe đen ướt bất đối xứng, không mặt/mắt. Nhận lệnh → tới/bám → tác động → hoàn tất hoặc báo chưa có đường/khóa/kẹt. Bộ truyền có thanh/rack nhìn được; chốt và pad dùng hình dáng khác nhau ngoài màu. Mesh trang trí không collider/route; vách trong vẫn che chọn. Hình concept không chứng minh art runtime; ảnh/video Unity chưa có. Boss thắng dùng ăn mừng hiện có, không tự cấp phần thưởng mới.

## 7. Độ khó và ngân sách runtime

3 quyết định chính, hai vai trò. Không căn thời gian/chạm pixel nhỏ. Thời gian/chạm nhầm/retry chưa đo. Rủi ro chính: **Không để một thân trải dài vừa phủ A vừa chạm B. Khoảng cách/vách phải chứng minh phối hợp cần thiết; tải/force từng phần cần đo.**

Giữ 32 hạt/120 Hz, một key light có bóng và vật liệu dùng chung. Chưa đặt trần khớp/renderer/GC/memory vì chưa đo. Profile cả khi giao lệnh/đổi cơ quan, không chỉ đứng yên; không giảm hạt/tick để đổi FPS. Chi phí nhiều phần và ống/cơ quan động cần đo riêng.

## 8. Chơi thử và hồi quy

Chưa chạy test. Cần giải trọn bằng touch/public commands, không teleport mô/ép cửa/ép thắng; thử thứ tự ngược, bỏ dở, quay lại, nhiều yaw/zoom, pause/retry/chuyển scene. Riêng màn này ưu tiên rủi ro nêu ở mục 7 và các đường phục hồi mục 4. Nếu có tách: cắt lệch, đổi vai, tụ sớm, task còn chạy, vật cản ngăn tụ, toàn bộ mô được giữ. Chạy full EditMode/PlayMode cả runtime dùng chung; ghi XML, commit+diff, build hash. Chơi người chưa biết lời giải có số người/lượt thật, không lấy tác giả thay thế.

## 9. Hiệu năng trên thiết bị

**Chưa kiểm chứng**: chưa có device/SoC/OS/build hash cho màn mới, chưa có số FPS/nhiệt. Mục tiêu kế thừa V2: 60 FPS; p95 ≤20 ms, p99 ≤33,3 ms; ghi spike >50 ms, CPU/GPU nếu có, GC/memory, input latency và phiên 15–20 phút. Ghi độ phân giải/quality/backend/warm-up/sạc/nhiệt. Desktop không chứng nhận Android.

## 10. Tích hợp, tương thích và nghiệm thu

Catalog/scene/build cho màn 24 **chưa tích hợp**. Khi dựng, nối sau V2 23, giữ ID 01–10 và progress/Home; không kế thừa thắng bằng số màn legacy. Kiểm tra Next/selector/load/save/replay và tắt app lúc thắng. Không đổi code ngoài phạm vi để phù hợp concept. Kết luận: hoàn thành hồ sơ nháp, chưa đạt gameplay/input/art/mobile. Việc tiếp theo: greybox cơ quan/bố cục rủi ro trước art. Mọi thay đổi ý đồ ghi trong phiên bản kế tiếp, không ghi đè lời giải để che lỗi.
