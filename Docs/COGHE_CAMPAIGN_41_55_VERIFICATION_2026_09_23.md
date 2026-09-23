# COghe — bàn giao campaign 01–55 — 23/09/2026

**Đã hoàn thành:** mười màn Tap ở 41–50 và năm màn ghép hộp mới 51–55, dựa trên màn hiển thị 19. Full PlayMode **393/393**, EditMode **8/8**; Mac player qua **15/15 màn mới**, mỗi màn đủ 32 hạt trong một cơ thể, không error/exception.

Nguồn yêu cầu: Mrk event `f9df450879e1d3467a4081de6a1185331dee050738c4b648c63d4fdf13dba34d`. Worktree `/Users/tommynguyen/.buzz/REPOS/GravityBox-macos-preview`; branch `codex/venom-macos-preview`, base HEAD `8b7f92a871cd2d1facc83ccea114521056b5cda9`. **Source chưa commit/push**. Đây là prototype đã kiểm chứng kỹ thuật trên Mac; chưa nghiệm thu người mới hoặc điện thoại.

## Nội dung và tương thích

41–50 giữ gameplay của Tap 01–10, bao gồm Boss không gợi ý ở 50. Content ID `coghe.tap.v1.NN` giữ nguyên; chỉ đổi vị trí và nhãn hiển thị. 51 hai nhịp rộng; 52 ba mặt cầu lệch; 53 kéo ba hộp trở về; 54 đẩy–kéo xen kẽ; 55 bốn khối lệch kết hợp cả hai chiều. Không yêu cầu xoay hộp. Mọi lực, lỗ thoát, cắt/tụ và điều kiện thắng giữ luật runtime dùng chung.

[Hướng dẫn build và thử người chơi](COGHE_CAMPAIGN_41_55.md); hồ sơ 15 màn trong `Docs/LevelDesign/COghe/Level41`–`Level55`. Bộ chọn sáu trang vừa khung portrait. Bản default chọn đủ 55 scene; bản `--tap` cũ vẫn mười màn riêng.

Inventory đối chiếu hash **161 file cũ trong Campaign30 và 670 file Tap nguồn không đổi**, gồm scene/definition/meta. Giữ save key/schema; test completion cũ, completion Tap khi đổi vị trí và round-trip dữ liệu đều qua. Nguồn: `Artifacts/COgheCampaign55/ORIGINAL_ASSETS_CHECK.json`, `COgheCampaign55Tests`.

## Build để user test

```bash
cd /Users/tommynguyen/.buzz/REPOS/GravityBox-macos-preview
bash Tools/build-venom-android.sh
```

**Không thêm `--tap`**. APK khi bạn build: `Builds/Venom/Android/COghe.apk`. Lượt này chưa build APK hoặc cài máy Android.

App Mac có sẵn: `Builds/COgheCampaign55/macOS/COghe.app`; mở bằng `open -n` cùng đường dẫn tuyệt đối. ZIP: `Builds/COgheCampaign55/COGHE_55_LEVELS_MACOS_2026_09_23.zip` (113,770,803 bytes), SHA256 `efaf1e5418d37d843b899b2184b1edc1b1436314e4cb937c0558d6e76f28c345`. Editor method tái tạo đúng bản bàn giao: `GravityBox.Editor.VenomCampaignBuilder.BuildCampaign55Mac`. Wrapper Mac thường cũng chọn 55 scene nhưng xuất `Builds/Venom/macOS/Venom.app`.

## Kết quả trên source bàn giao

| Kiểm tra | Kết quả | Artifact trong `Artifacts/COgheCampaign55` |
| --- | --- | --- |
| Unity 6000.3.19f1 generate/import | Exit 0, marker đủ 55 | `generate-03/Editor.log` |
| Full PlayMode | **393/393**, 0 fail/skip, 321.965 s; exit 0 | `playmode-03/TestResults.xml`, `run.json` |
| Full EditMode | **8/8**, 0 fail/skip; exit 0 | `editmode-01/TestResults.xml`, `run.json` |
| Mac build | Exit 0, explicit success marker, 55 scene | `build-02/Editor.log` |
| Mac input replay 41–55 | **15/15**, 32 escaped / 1 fragment mỗi màn; exit 0; 0 runtime errors | `player-02/20260923T084326084Z/run.json` |
| Source identity | Maps trước build, PlayMode, EditMode và bytes cuối trùng nhau | `build-02-source.json`, `test-03-source.json`, `editmode-01-source.json` |
| Video | H.264 540×900, 268.314 s, không âm thanh, fast-start và decode preview đã kiểm tra | `COGHE_LEVELS_41_55_PLAYTHROUGH.mp4`, `VIDEO_CONTAINER_CHECK.json` |

PlayMode gồm **382 ca cũ + 11 ca mới**: giải đủ mười Tap sau đổi vị trí; năm lời giải hộp theo thứ tự ngược; thử đi thẳng khi chưa ghép; chạm mọi tay nắm ở 720×1280 và 720×1612; kéo ngược/pause/retry; ID/navigation 40→41, 50→51 và tới 55; bảo toàn completion. Các test không dịch chuyển mô, đặt cửa mở hoặc ép thắng.

Mac: Mac16,10 / Apple M4 / macOS 26.5.1, Development universal x86_64+arm64, Unity `6000.3.19f1`, build GUID `b6b9e4cbfe6a4b9e9224bdde2f7e63c6`. Chạy **08:43:26–08:47:54 UTC**. Gameplay dùng InputSystem mouse press/release và FixedUpdate bình thường, không test-step physics. Replay dùng public `Load` và `ReleaseProp` cho chuyển màn/buông; các lệnh này là đường thực thi production. HUD được chụp/render và navigation có test riêng, **chưa xác minh bấm nút HUD bằng sự kiện chuột cửa sổ native** vì Computer Use không có native pipe. Không gọi replay là người mới tự chơi hoặc đo FPS.

## Các lỗi đã sửa và giới hạn

Lượt đầu 390/392: sửa kỳ vọng catalog 40→55; camera màn 55 khiến C che tay nắm B khi chạm. Đổi camera 52°/-12° để thấy cả tay nắm lẫn lỗ thoát, bỏ nhãn thừa và cập nhật nhãn world 41–50. Lượt hai 393/393, lượt cuối sau chỉnh góc nhìn cũng 393/393. Player01 không hợp lệ vì synthetic InputSystem không kích hoạt nút IMGUI; sửa harness, không sửa trạng thái puzzle. Chi tiết `WORK_LOG.md`.

Unity runner có `success:false` do heuristic bắt licensing handshake ban đầu; mọi kết luận ở đây dựa trên exit code 0, XML và marker thực. Test dài vượt timeout MCP 300 s nhưng runner tiếp tục, kết quả được lấy từ artifact khi Unity thoát; không chạy hai Editor cùng project. Bốn material bị import thay đổi incidental đã được đối chiếu baseline, lưu diff và khôi phục trước source/build cuối.

Điện thoại thật, touch/safe area, người mới, độ khó, thời gian suy luận, FPS/nhiệt và retention **chưa đo**. User test theo phiếu trong hướng dẫn; không cho xem lời giải trước. Các màn hộp có thể sửa sai bằng thao tác ngược và Retry; chất lượng nhận biết tay nắm/đường leo cần đánh giá trên người chơi.
