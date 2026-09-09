# Bàn 13 — Steel under water

Hộp vuông chứa đầy nước, cùng kích thước, spawn, lỗ tròn và khối lập phương cố định như bàn 02. Đây là thí nghiệm so sánh chuyển động của cùng một bi thép trong không khí và nước; không tăng độ khó bằng một mê cung mới.

## Hình học và luật nước

- Lòng hộp 320 × 320 mm; khoảng giữa hai mặt trong sàn/nắp 84 mm. Khối lập phương cạnh 64 mm ở giữa, giống bàn 02.
- Bi thép đặc đường kính 30 mm, mật độ 7.850 kg/m³, khối lượng 0,11097676 kg. Không đổi mass, inertia, vật liệu tiếp xúc, gravity hoặc tốc độ mô phỏng 120 Hz.
- Nước phủ kín vùng trong hộp. Không có khoảng khí, mặt nước hở, bọt nổi lớn hoặc cơ chế đổ nước.
- Nước được giữ trong hộp theo yêu cầu thiết kế, kể cả tại lỗ thoát. Đây là quy tắc giữ thể tích nước của bàn thử, không phải mô phỏng một bể mở có thể chảy qua lỗ. Không thêm collider bịt lỗ: bi vẫn đi qua lỗ tròn R=23 mm và phải ra hoàn toàn mới thắng.

## Mô hình lực

`WaterProfile` chứa mật độ nước 998,2 kg/m³, độ nhớt động lực 0,001002 Pa·s, tương ứng gần nhiệt độ phòng. `WaterVolume` là force provider riêng của bàn 13. `EnvironmentForceSystem` chuẩn bị provider theo mỗi bước vật lý trước khi cộng gia tốc; provider bị thu hồi cùng level, không lan lực cản sang các bàn khô.

Lực nổi `Fb = −rho_water × V_submerged × g_world`. Bi vẫn chìm vì thép đặc nặng hơn nước nhiều. Khi chìm hoàn toàn, lực nổi khoảng 0,138436 N, giảm tải đỡ xuống khoảng 0,95025 N; gia tốc chìm ban đầu 8,56257 m/s² khi chưa có tốc độ. Cơ sở: [Archimedes — OpenStax](https://openstax.org/books/university-physics-volume-1/pages/14-4-archimedes-principle-and-buoyancy).

Lực cản tác dụng ngược vận tốc tương đối với nước: `Fd = −0,5 rho Cd A |v_rel| v_rel`, `A = pi r²`. Dùng tương quan Schiller–Naumann dưới Re=1.000, Cd=0,44 ở vùng Newton; biểu thức dưới Re nhỏ tiến về lực Stokes. Tích phân lực bằng Rigidbody, không ghi đè vận tốc hoặc dùng damping toàn cục. Impulse tiêu tán được giới hạn để không đảo hướng vận tốc tương đối trong một tick. Tham khảo [phương trình lực cản — NASA](https://www1.grc.nasa.gov/beginners-guide-to-aeronautics/drag-equation/) và [tương quan trong tài liệu CFD COMSOL](https://doc.comsol.com/5.6/doc/com.comsol.help.cfd/CFDModuleUsersGuide.pdf).

Lực nổi/lực cản giảm liên tục theo thể tích chỏm cầu còn nằm trong nước khi bi qua mặt cửa; hết hoàn toàn khi bi rời nước. Phép tính chỏm cầu chính xác tại mặt cửa phẳng tách biệt. Ở góc ngoài của hộp, phép gần đúng mặt gần nhất không phải phép giao thể tích cầu–hộp đầy đủ, nhưng các vùng đó bị vỏ vật lý chặn.

Nước có vận tốc khối xấp xỉ được kéo theo chuyển động xoay của hộp, với thời gian đáp ứng 0,4 s. Vận tốc tương đối theo pháp tuyến bằng không tại mỗi mặt hộp. Đây là nội suy dòng khối có giới hạn, chưa giải áp suất hay bảo toàn không nén được của CFD. Mô-men nhớt quay dùng giới hạn Stokes `−8 pi mu r³ omega_rel`.

Giới hạn: chưa mô phỏng SPH/Navier–Stokes, added mass, history force, lubrication sát thành, biến dạng mặt nước, hoặc dòng xoáy phản hồi hai chiều từ bi vào nước. Lực cản cầu cô lập và mô-men nhớt thấp-Re là xấp xỉ; tiếp xúc bi–đáy vẫn dùng material chung, chưa có màng bôi trơn nước. Những giới hạn này cần giữ rõ khi đánh giá cảm giác, không tự tăng lực cản để biến nước thành chất lỏng đặc.

## Hình ảnh

`WaterVisuals` tách biệt khỏi simulation: độ nhuộm xanh theo độ dày nhìn xuyên, kính bao trong nhẹ, ánh sáng caustic động trên đáy, hạt quang học lơ lửng và vệt xoáy nhỏ phát ra theo tốc độ tương đối của bi. Hạt là dấu hiệu để đọc chuyển động, không phải các hạt chất lỏng mang khối lượng hay bọt khí sinh ra trong hộp đầy nước.

Tối đa 220 quad dùng một mesh động, không tạo GameObject theo từng hạt; shader clip hạt ở biên thể tích. Caustic được dựng thủ tục để gợi ánh sáng trong nước, chưa ray-trace khúc xạ thật. Không có nước hoặc particle bắn ra ngoài khi bi thoát. Pause dừng đồng hồ VFX, reset xóa wake/dòng xoay tích lũy và trả về điều kiện ban đầu.

## Kiểm chứng và thử tay

Sáu test PlayMode riêng kiểm tra: cùng hộp/bi và lực nổi; lực cản theo nghiệm giảm tốc tham chiếu; tải đỡ ổn định; chuyển qua biên nước và trở lại rơi tự do; đường từ spawn vòng qua cube ra lỗ bằng xoay; dòng nước/VFX/reset/unload. Ở bài hiệu chuẩn bỏ gravity và biên bể, tốc độ 1 m/s sau một giây còn 0,41510 m/s, gần nghiệm liên tục 0,41688 m/s. Đây là bể hiệu chuẩn riêng, không phải thay hình học trong player.

So sánh với bàn 02 bằng cùng góc nghiêng: quan sát lúc bắt đầu lăn, khi lao vào cube, khi đổi chiều hộp và lúc bi chìm qua chiều dày hộp. Ở tốc độ nhỏ và quãng đường ngắn, khác biệt của bi thép trong nước có thể vừa phải; nước không làm nó lơ lửng hoặc chuyển động như slow motion.

Xem [ảnh kiểm tra](Images/Level13/README.md) và [kết quả kiểm chứng](Verification/Water13/README.md).
