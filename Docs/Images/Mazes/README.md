# Ảnh kiểm tra bàn 10–11

Ngày 08/09/2026, Unity 6000.3.19f1.

- [Bộ chọn 11 bàn](Selector.png): ảnh native macOS của player đã build. Bàn 10 còn được quan sát trực tiếp trong cửa sổ game với hai cửa đóng và trạng thái A mở/B đóng; không lưu được ảnh riêng của hai trạng thái này.
- [Tầng trên](Layer11Top.png), [tầng giữa](Layer11Middle.png), [tầng dưới](Layer11Bottom.png): ảnh render prefab bàn 11 với `MazeLayerView` thật, cho thấy tầng đang chứa bi và các tầng khác được làm mờ.
- [Xem tổng thể](Layer11Overview.png): cả ba tầng cùng hiển thị; các bề mặt kính chồng nhau sáng hơn chế độ theo bi.
- [Metadata ảnh](Layer11RenderFixtures.json): độ phân giải, camera, thời điểm và vị trí bi của từng fixture.

Ảnh bàn 11 được render trong một Unity Editor riêng, không can thiệp lượt chơi native đang diễn ra. Bi được đặt tại đầu đường của từng tầng để kiểm tra hình ảnh; những ảnh này không phải bằng chứng giải màn. Fixture dùng camera và ánh sáng của scene Gameplay, không dựng HUD hoặc chạy vật lý. Bằng chứng đường giải liên tục từ spawn đến thoát nằm trong [PlayMode tests](../../Verification/README.md).

Có thể tạo lại bằng menu `Gravity Box > Capture Layered Maze Render Fixtures` hoặc execute method `GravityBox.Editor.MazePreviewCapture.Capture` trong batch mode có graphics. Ảnh xuất vào `Artifacts/`; fixture không lưu thay đổi vào scene. Unity có thể chuẩn hóa vật liệu URP/settings khi khởi động Editor; các thay đổi tự sinh trong lần chụp này đã được hoàn nguyên về cấu hình đã build. Ảnh dùng để đánh giá bố cục và phân biệt tầng, không so khớp pixel với player.
