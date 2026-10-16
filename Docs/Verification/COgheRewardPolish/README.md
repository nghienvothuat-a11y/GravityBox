# COghe — Hướng dẫn tiêm màu và điểm nhấn thưởng

Revision 15/10/2026. Dùng Glass C / UI ivory–teal hiện hành.

- Mũi tên amber viền xanh đậm và vòng đích dùng chung với hướng dẫn gameplay.
  Chọn Ocean → nhấn giữ lên COghe → nhả tay → Done để tiếp tục màn kế tiếp.
  Mũi tên theo đúng icon/cơ thể/nút sau resize; không chặn chạm. Dưới popup thì ẩn.
  Bước đã học và màu thật được lưu; thoát ứng dụng/Back rồi quay lại vẫn tiếp tục.
  Bài đầu giữ danh mục tại Ocean, tránh vô tình cuộn mất đích hướng dẫn.
- Mở khóa đồ: hai chùm pháo hoa nhỏ mint/amber/coral quanh panel. Không che icon,
  không đổi luồng mua/nhận đồ. Chỉ phát lần đầu hiện thông báo của một lượt thắng.
- Giọt thưởng: glint ấm ngắn, nhấn nhẹ bộ đếm, tối đa bốn glint trong một đợt giọt bay.
  Đổi layout không phát lại hiệu ứng đã kết thúc. Không lóe toàn màn hay dùng bloom.
- VFX chỉ hiển thị, không cộng tiền, không mở khóa, không tự chuyển màn.
  Giữ màn thắng đến khi người chơi chạm dòng level kế tiếp.

## Chi phí

`COgheRewardFx` là một UI mesh nhỏ cho mỗi hiệu ứng, material UI dùng chung,
không tạo texture/light/ParticleSystem, không cấp phát trong Update/OnPopulateMesh.
Pháo hoa tối đa 336 vertices, tồn tại 2.6s; glint tối đa 24 vertices, 1.35s.
Hiệu ứng tính thời gian UI unscaled nên không treo mãi trong popup tạm dừng.
Không có kết luận FPS iPhone/OPPO cho revision này khi chưa đo trên thiết bị.

## Kiểm tra

- `difficulty65-full.xml`: 46/46 Product UI tests, gồm migration mốc mở đồ cũ,
  pointer đúng vị trí thật, input không
  bị VFX chặn, tô mô thật, save/resume, Done/level kế tiếp, bố cục 720×1280,
  1080×2340 và 1536×2048; VFX không trả thưởng trùng, không lặp khi relayout,
  tự hủy cả khi timeScale=0.
- Review native Mac: 7 ảnh, chọn màu/nhấn giữ/nhả tay/Done và trở lại màn 9 đều
  hoàn tất. Chạy `-coghe-reward-review`; fixture UI minh họa, không phải
  bằng chứng đã giải tám puzzle. Fixture tắt ghi save người chơi.
