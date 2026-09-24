# COghe — kiểm chứng màn 56–60, 24/09/2026

Đã thêm năm màn xoay vật thể rỗng, toàn bộ mặt trong trơn, theo màn 7 đang hiển thị trong campaign (legacy content 06). Màn 56 là bình cổ hẹp, 57 mặt nạ, 58 ấm nghiêng, 59 đầu lâu với lỗ hàm, 60 Boss vỏ ốc. Cả năm dùng collider theo hình thật và một lỗ thoát; trang trí không tạo vùng bám hay lực mới. [Thiết kế chương](COGHE_CAMPAIGN_56_60.md).

## Bản được kiểm chứng

- Worktree: `/Users/tommynguyen/.buzz/REPOS/GravityBox-macos-preview`, nhánh `codex/venom-macos-preview`.
- Base HEAD: `0f4a48e45095e4067f2cf87b624284914d729b90`; thay đổi của lượt này **chưa commit/push**. Kết quả dưới đây thuộc source đang sửa, không phải riêng commit base.
- Unity **6000.3.19f1**, URP và Input System theo lockfile hiện có; không đổi package.
- Map SHA-256 của **7.474 file** trong `Assets/`, `Packages/`, `ProjectSettings/`, `Tools/`: [final-source.json](../Artifacts/COgheCampaign60/final-source.json). Hash canonical JSON (sort keys, separators `(',', ':')`): `3de846889a35b1f9b59eccd954b5aca6ea51527de142a90ea4add34075e7ff45`.
- Source của PlayMode05, EditMode01, build02 và bản bàn giao khớp nhau sau khi bỏ hai file `InitTestScene…` tạm do test runner sinh. Các file tạm đã chuyển vào artifact, không đưa vào source.
- [Release manifest](../Artifacts/COgheCampaign60/RELEASE_MANIFEST.json) ghi hash và đường dẫn bằng chứng. Artifact là dữ liệu local, không được Git theo dõi; ảnh minh họa được lưu trực tiếp cùng năm hồ sơ level.

## Kết quả cuối

| Kiểm tra | Kết quả | Bằng chứng |
| --- | --- | --- |
| Toàn bộ PlayMode | **403/403**, 0 fail/skip; 335,877 giây | [XML](../Artifacts/COgheCampaign60/playmode-05/TestResults.xml), [log](../Artifacts/COgheCampaign60/playmode-05/Editor.log) |
| Toàn bộ EditMode | **8/8**, 0 fail/skip | [XML](../Artifacts/COgheCampaign60/editmode-01/TestResults.xml) |
| Build Mac development | Unity exit 0; `Build Finished, Result: Success.` và marker campaign 60 | [log](../Artifacts/COgheCampaign60/build-02/Editor.log) |
| Mac player cuối | **5/5**; mỗi màn 32 hạt thoát, một cơ thể; 0 error/exception được ghi nhận; process exit 0 | [run.json](../Artifacts/COgheCampaign60/player-02/20260924T035249115Z/run.json), [log](../Artifacts/COgheCampaign60/player-02.log) |
| Asset cũ | **6.880/6.880** file asset không phải C# giữ nguyên bytes; riêng 55 scene + 55 definition + meta là **220/220** | [audit](../Artifacts/COgheCampaign60/ORIGINAL_ASSETS_CHECK.json) |
| Asset mới | 42 file asset có meta tương ứng; không trùng GUID trong `Assets/**/*.meta` | Kiểm tra source cuối ngày 24/09/2026 |

PlayMode chạy có graphics vì suite có RenderTexture. MCP transport hết thời gian chờ sau 300 giây, nhưng runner do tool sở hữu tiếp tục và hoàn tất: XML cuối ghi 403/403, `run.json` ghi exit 0 và không timeout. Cờ `success:false` của wrapper đến từ chuỗi lỗi handshake/access token của Unity licensing trong log; không dùng cờ đó thay cho XML/build result.

## Phạm vi kiểm tra

Mười ca mới trong [COgheCampaign60Tests.cs](../Assets/_Game/Tests/PlayMode/COgheCampaign60Tests.cs) kiểm tra lời giải của cả năm màn qua lệnh xoay và mô phỏng vật lý; giữ mô trong vỏ khi idle/xoay sai; toàn bộ mặt trơn; pause/retry; picking theo mesh; framing ở 720×1280 và 720×1612; ẩn trang trí khi Follow và khôi phục overview; truy vấn BVH đối chiếu brute force; topology/raycast của miệng mở; không thêm lực/torque hoặc sửa vị trí/vận tốc hạt khi chạm; catalog/chuyển màn và biên cuối 60. 393 ca cũ chạy cùng suite, gồm lưu tiến trình và các chương trước.

Kiểm tra không có lực bám dùng cùng trạng thái vật lý, qua năm vật thể và ba tư thế. Đây không phải khẳng định hai lần reload PhysX riêng biệt sẽ có quỹ đạo giống từng tọa độ.

Mac player dùng vòng `FixedUpdate` bình thường và mouse drag được đưa vào Input System qua [COgheVesselProofPlayer.cs](../Assets/_Game/Venom/Runtime/ChapterProof/COgheVesselProofPlayer.cs). Không teleport sinh vật hoặc ép trạng thái thắng. Scene navigation, pause và reset dùng API công khai. Chế độ proof chỉ được bật bằng cờ dòng lệnh trong development player, không ghi đè tiến trình người chơi.

| Màn | Số drag trong replay | Kết quả |
| --- | --- | --- |
| 56 — Bình cổ hẹp | 5 | 32 hạt / 1 cơ thể |
| 57 — Mặt nạ thủy tinh | 5 | 32 hạt / 1 cơ thể |
| 58 — Ấm nghiêng | 5 | 32 hạt / 1 cơ thể |
| 59 — Đầu lâu pha lê | 8 | 32 hạt / 1 cơ thể |
| 60 — BOSS · Vỏ ốc | 12 | 32 hạt / 1 cơ thể |

Thiết bị: Mac16,10 / Apple M4. Build GUID `abc7b14ca4894c4aad84a8ffc0476640`. Player chạy 03:52:49–03:53:25 UTC ngày 24/09/2026. Số drag là dữ liệu replay tác giả, không phải độ khó hay số bước bắt buộc cho người chơi.

## Hình ảnh và video

Đã rà hình cả năm vật thể, các đoạn xoay, hình thắng, Follow/overview. Mask/teapot dùng tiết diện song song để tránh vỏ tự gập. Skull bắt đầu nghiêng 115°, lỗ nằm ở hàm. Shell có khoảng hở rõ giữa các vòng. Trang trí được đưa vào danh sách occluder của presentation để Follow không bị tay cầm/răng/viền che.

Ảnh lưu cùng hồ sơ: [56](LevelDesign/COghe/Level56/README.md), [57](LevelDesign/COghe/Level57/README.md), [58](LevelDesign/COghe/Level58/README.md), [59](LevelDesign/COghe/Level59/README.md), [60](LevelDesign/COghe/Level60/README.md).

[Video bản cuối](../Artifacts/COgheCampaign60/COGHE_LEVELS_56_60_PLAYTHROUGH_FINAL.mp4): 147 khung hình thật, 540×900, H.264, 36,119 giây, không audio, 1.907.261 byte. Mã hóa từ ffconcat giữ thời gian capture; đã giải mã ảnh preview và kiểm tra MP4 fast start (`moov` trước `mdat`). SHA-256: `e907227238b47f332210ef1760a7eaf8b6f7533fd427626ba105032db46b1f2f`. [Container check](../Artifacts/COgheCampaign60/VIDEO_FINAL_CONTAINER_CHECK.json).

Video là replay tự động của tác giả. Chưa có người mới chơi; chưa kiểm chứng click nút IMGUI từ input native của hệ điều hành, chạm trên điện thoại, FPS/nhiệt/GC mobile hoặc APK trong lượt này. Hai tỷ lệ dọc được render trong Editor, không đại diện hai thiết bị thật.

## Các lỗi tìm được trong quá trình làm

Lượt đầu `-nographics` làm một ca render cũ crash, không có XML hoàn chỉnh; đã đổi sang chạy có graphics. PlayMode02 đạt 400/402: hai ca mới phát hiện điểm chạm đặt vào vùng trống giữa vòng ốc và spawn/đoạn cong đầu quá chật; đã sửa hình/spawn và chọn điểm trên mesh thật. PlayMode03 đạt 402/403 do assertion so quỹ đạo giữa hai lần reload PhysX; thay bằng kiểm tra trực tiếp lực/torque/trạng thái trong cùng frame như trên. PlayMode04 đạt 403/403. Sau review Follow, bổ sung đăng ký occluder và assertion tương ứng; PlayMode05 chạy lại toàn bộ đạt 403/403.

Generator làm đổi keyword của `Quiet mint light.mat`, còn suite cũ sửa ba `* maze glass.mat`; cả bốn được phục hồi chính xác từ baseline, và final source đã được đối chiếu với input trước test/build. Không sửa asset màn 01–55. Hai project settings untracked có sẵn trước lượt này được giữ nguyên.

## Build và thử tiếp

App có sẵn: `Builds/COgheCampaign60/macOS/COghe.app`. Mở app, chọn nhóm `51–60`, rồi chọn `56` đến `B60`; kéo trong vùng chơi để xoay vật thể, đưa sinh vật về lỗ mint. Dùng `Làm lại`, `Tạm dừng`, `Theo COghe` để thử phục hồi/camera.

Build APK đủ 60 màn, không thêm `--tap`:

```bash
cd /Users/tommynguyen/.buzz/REPOS/GravityBox-macos-preview
bash Tools/build-venom-android.sh
```

Output theo wrapper: `Builds/Venom/Android/COghe.apk`. Cờ `--tap` vẫn là bản 10 màn riêng. Chưa chạy build APK trong lượt này.

Tái tạo riêng năm màn: `GravityBox.Editor.VenomCampaignBuilder.GenerateCampaign60`; build Mac: `GravityBox.Editor.VenomCampaignBuilder.BuildCampaign60Mac`. Hai entrypoint nằm trong [builder](../Assets/_Game/Editor/COgheCampaign60Builder.cs), chạy bằng Unity đúng phiên bản trên. Test và build cần chạy tuần tự, khi project không có Editor khác đang mở.
