# Kiểm tra gói bàn giao — 29/09/2026

Phạm vi: source Spatial 01–10, quy chuẩn Glass C và thiết kế Spatial 11–30 trên
nhánh `NewGraphic`. Đây là kiểm tra file/tài liệu trước commit, không phải lượt
chạy Unity, build APK hay đo FPS mới.

- `validate_design.py`: **20 hồ sơ, 20 ảnh, không lỗi**. Kiểm tra ID, 10 mục hồ sơ,
  Boss 20/30, các bước khối lượng 50/50 và liên kết; chi tiết ở [design-audit.json](design-audit.json).
- JavaScript inline trong `review.html`: `node --check` đạt.
- Các đường dẫn cục bộ trong bốn bộ SpatialNext20, SpatialPilot, SpatialCircuits và
  bằng chứng SpatialPilot đã được kiểm tra tồn tại và có trong Git index. README,
  AGENTS, STYLE_RULES và hướng dẫn bàn giao không có liên kết file bị thiếu.
- Asset Unity mới/sửa: có `.meta` cho asset và thư mục, không trùng GUID mới, không
  thiếu GUID tham chiếu khi đối chiếu asset trong Git và package của project.
- 20 PNG đều là file trong repo, không chỉ là link tới cache ảnh trên máy tác giả.
  File lớn nhất trong gói dưới 2 MB; không cần đưa Library/Builds hoặc dùng Git LFS
  cho các ảnh bàn giao này.
- Đã đối chiếu XML kiểm thử lưu ở SpatialCircuits: **41 passed / 0 failed**;
  hai snapshot physics trước/sau art pass bằng nhau. Đây là bằng chứng đã lưu của
  lượt chạy 10 màn trước, không phải test mới cho 11–30.

Chưa có kiểm chứng clean import Unity trên máy thứ hai, chơi người mới, Q/đu dây
trong runtime hoặc mobile performance của 20 màn mới. Thực hiện theo
[IMPLEMENTATION_HANDOFF](IMPLEMENTATION_HANDOFF.md) khi bắt đầu dựng.

Các chỉnh sửa local không thuộc Spatial (ba material kính PhysicsLab, một thay đổi
keyword của Quiet mint light và file crash Mono) được giữ ngoài commit bàn giao.
