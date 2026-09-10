# Bàn 14 — Steel in mercury

Cùng hộp vuông 320 × 320 mm, khoảng trong 84 mm, cube cố định 64 mm, spawn và lỗ tròn R=23 mm như bàn 02/13. Chỉ môi trường chất lỏng và cách hiển thị đổi. Bàn 13 vẫn là nước; bàn 14 dùng profile thủy ngân riêng.

## Điều người chơi quan sát

Bi thép 30 mm, 111 g **nổi lên**, áp vào mặt cao nhất của hộp theo phương thế giới. Xoay hộp sẽ đổi mặt đỡ phía trên; gravity vẫn hướng xuống.

Theo yêu cầu mới, khi bi đến vùng 40 mm quanh lỗ, [hỗ trợ thoát chung cho mọi màn](EXIT_ASSIST.md) sẽ căn và đẩy bi ra, kể cả trạng thái nổi đứng yên ở miệng lỗ hướng lên. Không cần thực hiện thêm một cú nghiêng chính xác. Lực này là hỗ trợ gameplay được bổ sung có chủ ý; không thay mật độ/độ nhớt để giả lập việc tự nổi hoàn toàn ra ngoài.

Toàn bộ bi vẫn phải vượt qua lỗ thật mới thắng; lực hỗ trợ ngừng sau hoàn thành và bi tiếp tục dynamic. Bản trước khi có hỗ trợ đã có đường giải từ spawn chỉ bằng xoay, nhưng thao tác cuối phụ thuộc hướng và tốc độ nghiêng. Bằng chứng lịch sử của bản đó được giữ riêng.

## Thông số và mô hình

Ở khoảng 20°C:

| Đại lượng | Nước bàn 13 | Thủy ngân bàn 14 |
|---|---:|---:|
| Mật độ | 998,2 kg/m³ | 13.546 kg/m³ |
| Độ nhớt động lực | 0,001002 Pa·s | 0,001567367 Pa·s |
| Nước/chất lỏng bị bi chiếm khi ngập hết | 14,11172 g | 191,50206 g |
| Added mass cơ sở | 7,05586 g | 95,75103 g |
| Lực nổi khi ngập hết | 0,138436 N | 1,878635 N |
| Gia tốc khởi đầu ở chất lỏng tĩnh | −8,05071 m/s² | +3,82122 m/s² |

Mật độ thủy ngân lấy từ [NIST — Composition of mercury](https://physics.nist.gov/cgi-bin/Star/compos.pl?matno=080&mode=text). Độ nhớt được tính từ `mu = 10^(-0,2561 + 132,29/293,15) × 10⁻³ Pa·s`, theo phương trình 2/bảng 5 của [Assael et al. (2012)](https://elib.dlr.de/76579/1/Metal-Pub.pdf). Thủy ngân có độ nhớt động lực chỉ khoảng 1,56 lần nước ở nhiệt độ này; không giả lập thành mật đặc.

Các số về lực/gia tốc trong bảng là tính toán của mô hình game. Dùng chung `WaterVolume/WaterHydrodynamics` với bàn 13: `(m+ma)a = (m−mf)g + Fdrag + (mf+ma)Du/Dt + Fcontact`, `ma=0,5mf`. Rigidbody.mass là quán tính tịnh tiến 206,72779 g khi ngập hết; khối lượng vật chất và mô-men quán tính quay vẫn thuộc bi thép. Lực dư đẩy lên 0,789953 N được nắp cân bằng khi bi nằm yên. Không đổi hướng gravity để tạo hiệu ứng nổi.

Tại cửa có thể tích ngập một phần, cân bằng thủy tĩnh ứng với tỷ lệ ngập `rho_steel/rho_mercury ≈ 0,5795`. Nếu tắt hỗ trợ gameplay, đây là lý do chỉ tới miệng lỗ chưa đủ để tự thoát. Khi ra hết chất lỏng, lực nổi/cản/added mass đều mất; không để lực thủy ngân lan sang các level sau.

## Hiển thị và giới hạn

Thủy ngân thật là kim loại lỏng màu bạc, không nhìn xuyên như nước. [RSC — Mercury](https://periodic-table.rsc.org/element/80/mercury). Bàn thử dùng **SEE-THROUGH VIEW** ghi rõ trên HUD: thể tích màu bạc với phản sáng, cube/bi nhìn được bên trong, tracer màu bạc để đọc chuyển động. Không dùng caustic nước xanh, mặt nước hở hay bọt lớn. Đây là hình ảnh hỗ trợ quan sát, không phải mô phỏng quang học thủy ngân. VFX không tạo lực/collider.

Sàn bàn 13/14 chuyển dần từ opacity 1 xuống 0,12 khi mặt ngoài quay về phía camera, nên lúc lật hộp vẫn nhìn được bi và cube. Xoay lại thì sàn trở về màu đặc. Tính theo hướng nhìn thực tế, không dùng góc Euler hay đổi màu đột ngột tại 180°. Dùng property block theo instance; collider, lỗ tròn R=23 mm, vòng sáng và profile vật lý không đổi.

Giữ luật chất lỏng đầy và không chảy ra lỗ như bàn 13. Không mô phỏng mặt thoáng, meniscus, sức căng bề mặt, thấm ướt hay chất lỏng bám bi. Cản lăn, added mass đẳng hướng và trường dòng có thời gian đáp ứng 0,4 s kế thừa mô hình rút gọn; chưa hiệu chuẩn riêng cho thủy ngân hoặc CFD. Các giới hạn về trượt, squeeze-film va chạm và vùng Reynolds cao vẫn áp dụng; tỷ lệ khối lượng chất lỏng/bi lớn khiến chúng đáng chú ý hơn. Xem [mô hình chung](LEVEL13_WATER.md).

Hộp kích thước này tương ứng khoảng **113 kg thủy ngân**, trừ cube/bi. Root vẫn được xoay kinematic theo input như các bàn trước; game chưa mô phỏng mô-men tay người phải chịu để xoay cả khối chất lỏng đó. Phép thử này tập trung vào chuyển động của bi trong môi trường, không phải sức nặng toàn hộp trong tay.

## Kiểm chứng

Bản thủy ngân đầu có sáu test riêng: thuộc tính/profile và gia tốc nổi; tải nắp ổn định; giảm tốc so với nghiệm cản; trạng thái nổi một phần không thắng và thoát thật mới thắng; VFX/reset/đổi lại nước; đường giải từ spawn chỉ bằng xoay. [Kết quả](Verification/Mercury14/README.md), [ảnh](Images/Level14/README.md). Chỉ build macOS theo yêu cầu đang có; APK cũ chưa chứa màn 14.

Kiểm chứng hiện tại có hút bi ở mọi màn: [ExitAssist](Verification/ExitAssist/README.md). Phép đo cân bằng nổi riêng tắt hỗ trợ để tiếp tục bảo vệ mô hình chất lỏng.
