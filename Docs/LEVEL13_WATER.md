# Bàn 13 — Steel under water

Hộp vuông chứa đầy nước, cùng kích thước, spawn, lỗ tròn và khối lập phương cố định như bàn 02. Đây là thí nghiệm so sánh chuyển động của cùng một bi thép trong không khí và nước; không tăng độ khó bằng một mê cung mới.

## Hình học và luật nước

- Lòng hộp 320 × 320 mm; khoảng giữa hai mặt trong sàn/nắp 84 mm. Khối lập phương cạnh 64 mm ở giữa, giống bàn 02.
- Bi thép đặc đường kính 30 mm, mật độ 7.850 kg/m³, khối lượng vật chất 0,11097676 kg. Giữ mô-men quán tính thép, vật liệu tiếp xúc, trọng lực thế giới và mô phỏng 120 Hz. Từ bản hiệu chỉnh, Rigidbody dùng quán tính tịnh tiến gồm thép và added mass của nước; xem công thức dưới đây.
- Nước phủ kín vùng trong hộp. Không có khoảng khí, mặt nước hở, bọt nổi lớn hoặc cơ chế đổ nước.
- Nước được giữ trong hộp theo yêu cầu thiết kế, kể cả tại lỗ thoát. Đây là quy tắc giữ thể tích nước của bàn thử, không phải mô phỏng một bể mở có thể chảy qua lỗ. Không thêm collider bịt lỗ: bi vẫn đi qua lỗ tròn R=23 mm và phải ra hoàn toàn mới thắng.

## Mô hình lực

`WaterProfile` giữ mật độ 998,2 kg/m³ và độ nhớt động lực 0,001002 Pa·s, gần nước 20°C. Không tăng độ nhớt để tạo cảm giác đặc. `WaterHydrodynamics` chứa các công thức thuần; `WaterVolume` lấy trạng thái bi, hình học và dòng nước rồi áp lực qua `EnvironmentForceSystem`.

**Quán tính và lực nổi.** Đặt `m` là khối lượng thép, `mf = rho × V_submerged`, `ma = 0,5 mf`. Hệ số added mass 0,5 là nghiệm cầu trong dòng thế không bị giới hạn bởi thành, dùng làm gần đúng cơ sở. [Brennen, Caltech — Added mass](https://brennen.caltech.edu/fluidbook/basicfluiddynamics/unsteadyflows/addedmass/introduction.pdf).

Phương trình đang tích phân: `(m + ma) a = m g − mf g + Fdrag + (mf + ma) Du/Dt + Fcontact`. Khi ngập hoàn toàn: `mf = 14,11172 g`, `ma = 7,05586 g`; quán tính tịnh tiến là 118,03262 g, trọng lượng vẫn chỉ `m g`. Rigidbody.mass chứa `m+ma` để cả lực và impulse tiếp xúc cùng dùng quán tính này. Provider trừ phần `ma g` mà gravity provider chung đã cộng. Mô-men quán tính giữ `2/5 m r²`; HUD hiển thị **111 g thép**. Không ghi đè vận tốc.

Lực nổi vẫn 0,138436 N và tải nghỉ 0,95025 N. Gia tốc chìm ban đầu ở nước đứng yên đổi từ 8,56257 thành **8,05071 m/s²**, vì cần gia tốc cả lượng nước tương đương. [Archimedes — OpenStax](https://openstax.org/books/university-physics-volume-1/pages/14-4-archimedes-principle-and-buoyancy).

**Cản khi bay và khi lăn.** Xa thành, giữ `Fd = −0,5 rho Cd A |v_rel| v_rel`, với Schiller–Naumann dưới Re=1.000 và Cd=0,44 ở vùng Newton trước drag crisis. [NASA — Drag equation](https://www1.grc.nasa.gov/beginners-guide-to-aeronautics/drag-equation/), [COMSOL — CFD guide](https://doc.comsol.com/5.6/doc/com.comsol.help.cfd/CFDModuleUsersGuide.pdf).

Sát mặt phẳng, dùng cản hiệu dụng của cầu lăn không trượt: `Cd_gap = (−44,2 log10(G/D) + 34)/Re`; cộng `Cd_wake = 1,70 − 0,136 log10(Re) − 0,0716 log10(Re)²` trong phạm vi 5–300. Báo cáo thực nghiệm cho vùng Re khoảng 1.000–10.000 có Cd xấp xỉ 1, cao hơn cầu tự do. [Nanayakkara et al., JFM 2024, §2.1 và §4.3](https://doi.org/10.1017/jfm.2024.146).

Lựa chọn triển khai: giữ đa thức wake trong miền công bố, nội suy tới plateau 1 từ Re 300–1.000; không kéo dài đa thức tới tốc độ lớn. Giả định độ nhám hiệu dụng 3 µm (`G/D=10⁻⁴` cho bi 30 mm), chưa đo trên một hộp thật. Khe vật lý lớn hơn độ nhám sẽ thay thế G. Tắt dần hiệu chỉnh trong khoảng cách r/4 khỏi mặt; giảm theo độ trượt và dùng mặt lăn có trọng số lớn nhất. Các phép nội suy này là lựa chọn mô hình, không phải tương quan thực nghiệm đã kiểm chứng cho cả hộp.

Sáu tia trong hệ tọa độ hộp tìm mặt collider thật của sàn, nắp, thành và cube. Tia qua lỗ không gặp sàn nên không thêm lực thành tại cửa. Cản lăn hiệu dụng đã bao gồm đóng góp mô-men/r: áp dưới dạng lực tại tâm, giảm mô-men nhớt tự do theo cùng trọng số để tránh tính hai lần. Giữ ma sát và cản lăn vật liệu do biến dạng như bản khô. Khi trượt mạnh, quay về mô hình cầu tự do; chưa có tương quan riêng cho cầu trượt sát thành.

Ở 0,2 m/s, cản lăn tham chiếu là **0,014609 N**, trong khi cầu cô lập là **0,006209 N**. Force thực tế tại 120 Hz sau bước tích phân ổn định là 0,014534 N. Đây là kết quả của mô hình hiệu chỉnh, không phải đo lực trên thiết bị thật.

**Tích phân và dòng nước.** Cản được tích phân với hệ số ngầm theo từng thành phần vận tốc, đảm bảo impulse cản không đảo chiều thành phần tương đối; solver tiếp xúc vẫn xử lý chuyển động cuối bước. Giữ trường vận tốc khối có thời gian đáp ứng 0,4 s. Tính `Du/Dt` bằng truy ngược một phần tử nước qua hai trạng thái dòng liên tiếp; việc bi đổi vị trí trong nước tĩnh không sinh lực áp suất giả. Đây là lực gia tốc từ trường dòng xấp xỉ, chưa giải phương trình áp suất.

Phần ngập theo chỏm cầu làm lực và added mass giảm liên tục tại cửa; ra hết nước thì trả Rigidbody.mass về mass thép và rơi với 9,81 m/s². Reset/disable/unload xóa lịch sử dòng và quán tính nước. Thay added mass theo phần ngập giữ vận tốc liên tục; chưa mô phỏng năng lượng mặt thoáng, nước bám hoặc impulse nước khi ra/vào.

**Giới hạn cần giữ rõ.** Mô hình tốt hơn cho cản lăn và quán tính, chưa phải nước chính xác hoàn toàn: không CFD, history force, lift/wake dao động, squeeze-film theo pháp tuyến khi va chạm, ảnh hưởng nhiều thành lên tensor added mass, hoặc phản hồi bi–nước hai chiều. Plateau trên Re=10.000 là ngoại suy; hệ số gap và cản hiệu dụng dùng cho lăn không trượt gần mặt phẳng, không chứng minh chính xác cho mọi va chạm/cạnh/lỗ. Trường dòng chưa bảo toàn không nén được và chưa giải dòng quanh cube. Hệ số ma sát/restitution tiếp xúc vẫn là bản khô; chỉ chuyển động trước/sau va chạm chịu lực nước. Muốn hiệu chuẩn tiếp cần đo video cùng bi 30 mm và hộp thật, đặc biệt vận tốc, nhám và độ nảy.

## Hình ảnh

`WaterVisuals` tách biệt khỏi simulation: độ nhuộm xanh theo độ dày nhìn xuyên, kính bao trong nhẹ, ánh sáng caustic động trên đáy, hạt quang học lơ lửng và vệt xoáy nhỏ phát ra theo tốc độ tương đối của bi. Hạt là dấu hiệu để đọc chuyển động, không phải các hạt chất lỏng mang khối lượng hay bọt khí sinh ra trong hộp đầy nước.

Tối đa 220 quad dùng một mesh động, không tạo GameObject theo từng hạt; shader clip hạt ở biên thể tích. Caustic được dựng thủ tục để gợi ánh sáng trong nước, chưa ray-trace khúc xạ thật. Không có nước hoặc particle bắn ra ngoài khi bi thoát. Pause dừng đồng hồ VFX, reset xóa wake/dòng xoay tích lũy và trả về điều kiện ban đầu.

## Kiểm chứng và thử tay

Các test nước kiểm tra lực nổi/added mass, giảm tốc giữa nước, cản lăn so với nghiệm giải tích tại 60/120/240 Hz, hệ số trong các miền Reynolds, năng lượng tiêu tán, mặt cube/sàn/lỗ thật, gia tốc dòng, tải nghỉ, thoát hoàn toàn và lifecycle. Trên mặt phẳng hiệu chuẩn dài, bỏ riêng cản biến dạng khô để cô lập thủy động lực: từ 0,3 m/s, sau 1 giây đạt 0,179540 m/s tại 120 Hz; nghiệm liên tục là 0,179418 m/s. Sai số tích phân khoảng 0,068%, không phải độ chính xác so với nước thật.

So sánh với bàn 02 bằng cùng góc nghiêng: quan sát lúc bắt đầu lăn, khi lao vào cube, khi đổi chiều hộp và lúc bi chìm qua chiều dày hộp. Ở tốc độ nhỏ và quãng đường ngắn, khác biệt của bi thép trong nước có thể vừa phải; nước không làm nó lơ lửng hoặc chuyển động như slow motion.

Xem [ảnh kiểm tra](Images/Level13/README.md) và [kết quả kiểm chứng](Verification/Water13/README.md).
