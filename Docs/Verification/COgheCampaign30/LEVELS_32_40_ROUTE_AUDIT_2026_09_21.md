# Rà soát màn 32–40 — 21/09/2026

Đã sửa lỗi chạm và phục hồi được tái hiện. **20/20 kịch bản giải/phục hồi** bằng
chạm màn hình đạt; **9/9 màn32–40 thắng đủ32/32** trong app Mac mới.

Nguồn: worktree `codex/venom-macos-preview`, HEAD
`b8594b2b4d70a39b401c6420c425af7fa4e64d6a` cộng diff của đợt sửa. Unity6000.3.19f1.
Bản sửa ở local, chưa commit/push. Bản sửa camera32 từ lượt trước được giữ lại.

| Màn | Nhánh được kiểm tra ngoài lời giải chính | Điều chỉnh |
|---|---|---|
|32|Vào B trước, quay về; kéo cầu nửa chừng rồi đảo chiều; A→L→B→thoát|Góc nhìn riêng Cầu B / A / B; bệ B và lỗ cuối nhận chạm|
|33|Vào B trước khi mở E rồi quay lại S; đổi T giữa chừng; A→S→B|Góc S thấy hai miệng ống; góc A/B tách sàn khỏi ống/nóc; mặt nhận hướng kéo L theo ray|
|34|Đẩy G khi X chưa đỗ, kéo G về; kéo X nửa chừng rồi trả lại; hoàn tất và thoát|Chốt đầu ray giữ G sau khi lùi; góc riêng cho bệ/lỗ thoát|
|35|Bỏ A khi phần kia đã qua D, giữ lại rồi chốt L, hợp thể và kéo E|Góc khoang2 tránh nắp E che cú chạm lỗ|
|36|Bỏ/giữ lại A, kéo L, đổi ca B/C, hợp thể và thoát|Cơ quan hiện tại qua kiểm thử|
|37|Đưa G tới II rồi quay I, trở lại II, kéo C và hợp thể|Cơ quan hiện tại qua kiểm thử; Toàn cảnh dùng cho hành trình kéo dài|
|38|Thử Q với phần nhỏ, buông, tụ đủ khối lượng, kéo Q và chia lại B/C|Cơ quan hiện tại qua kiểm thử|
|39|Thử II trước I; buông C; bỏ A sau I, giữ lại rồi vận hành II|Phục hồi và hợp thể qua cửa thật, thoát đủ32 hạt|
|40|Q/P, chém thành ba vai trò; thử II sớm; bỏ A rồi giữ lại; hợp thể, kéo H|Phục hồi và thắng; Boss giữ luật không gợi ý lời giải|

Mỗi đường kiểm tra thắng thật và Retry sau thắng. Điều khiển thế giới đi qua
`TouchPoint`; chọn phần, đổi góc và Buông vật dùng API của HUD. Không đặt vị trí mô,
không gán cửa mở hoặc ép thắng. Vật lý120Hz,32 hạt và luật tự hợp thể được giữ.
Các màn dùng480×800 và720×1612; màn32 còn có720×1280. Kiểm thử input trạng thái đầu
của chương dùng720×1280/720×1612; góc mới còn kiểm tra safe area và bất biến trạng thái.

| Kiểm chứng | Kết quả | Bằng chứng trong `Artifacts/audit32-40-20260921/` |
|---|---|---|
|Toàn bộ PlayMode|355/355,0 lỗi,0 bỏ qua|`screen-recovery-04/TestResults.xml`|
|Toàn bộ EditMode|8/8,0 lỗi|`editmode/TestResults.xml`|
|Build Mac Development|Success,arm64+x86_64|`build-macos/Editor.log`|
|App Mac thực tế|10/10 gồm màn31 hồi quy; 32–40 đều thắng32/32|`player/20260921T162952493Z/run.json`|
|Lỗi runtime trong replay|Không có|`runtimeErrors: []` trong run manifest|
|Ảnh thực tế|480×800;18 ảnh32–40 đã xem đủ trước thoát và sau thắng|Cùng thư mục player|

Player chạy2026-09-21T16:29:52Z–16:30:18Z trên Mac16,10/Apple M4. Đây là replay
lệnh chạm tự động trong app, không phải chơi tay trên điện thoại. Chưa kết luận
về mọi tổ hợp vật lý, độ dễ hiểu với người chơi mới hoặc hiệu năng mobile.

Các lần lỗi được giữ trong `screen-baseline`, `screen-recovery-01`–`03`.
Scene33 chỉ thêm Transform/mối liên kết mặt điều khiển L; scene34 thêm chốt đầu G.
Unity tự chuẩn hóa shadow near-plane0.05→0.10 trong hai scene lúc lưu. Collider,
khối lượng và hình học joint không đổi; scene01–32,35–40 không đổi.
Ba material PhysicsLab tự đổi blend khi import đã được lưu bản chênh lệch và phục hồi;
hai file ProjectSettings chưa được theo dõi từ trước được giữ nguyên.

`final-test-source-sha256.json` xác nhận6022 tệp khớp sau bộ test. Sau đó chỉ sinh
lại replay Development và phục hồi ba material nói trên. `build-source-sha256.json`
và `app-sha256.json` định danh nguồn/binary của app kiểm chứng. Build guard kiểm tra
hash nguồn test trước khi dựng replay. Các cảnh báo licensing trong runner không
phải kết quả test: XML, mã thoát0 và `Build Finished, Result: Success` xác nhận kết quả.

App: `/Users/tommynguyen/.buzz/REPOS/GravityBox-macos-preview/Builds/Venom/macOS/Venom.app`.
Bundle: `/Users/tommynguyen/.buzz/OUTBOX/COGHE_LEVELS_32_40_AUDIT_2026_09_21/`.
