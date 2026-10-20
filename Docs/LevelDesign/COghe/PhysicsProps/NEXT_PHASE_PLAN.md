# Màn vật lý — kế hoạch giai đoạn sau

Mrk 20/10/2026 (thread Venom f65d362c): giữ nguyên 65 màn, ba màn vật lý là ba màn mới 66, 67, 68; "lưu kế hoạch làm
những bài vật lý vào để triển khai cho giai đoạn sau". Tài liệu này là điểm bắt đầu khi quay lại phần vật lý.
Màn đã dựng và cách dựng: [README.md](README.md).

## 1. Đã có (20/10/2026)

- **Màn:** 66 F1 Bi giữ cửa (dễ), 67 F2 Bánh răng thả bi (vừa), 68 F3 Kính vỡ, đối trọng (khó). Chuỗi học `physics`
  trong `Difficulty65/curriculum.json` (giới thiệu F1, luyện F2, kết hợp F3).
- **Cơ quan mới:** bi sắt + rãnh + chốt (`SteelBall`, `BallChute` trong `COghePhysicsPropLevels.cs`), nút cân được đồ
  vật (`COgheTissueSensor.Weights`), cột đổ (`COgheTopplePillar`), kính mỏng vỡ (`COgheBreakableGlass`).
- **Có từ trước, dùng chung được:** dây đu (`COgheSwingTransfer`), ròng rọc (`COghePulleyDrive`), bập bênh nhận một vật
  (`COgheSeesawBridge.RequiredLoad`), so cân (`COgheBalanceLift`), thùng.

## 2. Ý tưởng chưa làm

Từ 12 ý gửi Mrk 19/10/2026 (đã làm: 1 bi giữ nút, 7 cột đổ, 8 kính vỡ).

| Ý | Cách chơi | Phần engine phải thêm |
| --- | --- | --- |
| 1b. Bi một phần tư | Bi nhỏ nặng bằng một phần tư thân, ghép với nút phần tư (họ N31/N33). | Chỉ đổi khối lượng, cỡ bi và rãnh. |
| 2. Đường lăn trên kính | Rãnh bi chạy trên kính, có ghi rẽ nhánh; COghe gạt ghi để bi tới đúng nút. Ghép với đường kính. | Ghi rẽ trong `BallChute`, tay gạt trên kính. |
| 3. Bi to | Bi to chỉ đẩy được xuống dốc, lăn một chiều; đặt sai chỗ thì Retry. | Cho COghe đẩy vật rời hình cầu. |
| 4. Cầu treo | Cầu võng theo sức nặng: nửa thân qua được, cả thân võng chạm đáy (hoặc ngược lại). | Cầu nhiều đoạn. Nên tính võng theo mức tải cố định, không dùng khớp vật lý thật, để lần nào cũng như nhau. |
| 5. Gàu ròng rọc | Thả bi hoặc thùng vào gàu làm đối trọng kéo bục lên. | `COghePulleyDrive` + gàu cân bằng `Weights`. |
| 6. Dây đu đẩy cột, mang bi | Đu dây va đổ cột, hoặc dây mang bi sang bờ kia. | Cho `COgheTopplePillar` nhận cú va từ dây đu (hiện chỉ đổ khi bi chạm). |
| 7b. Cột đổ thành cầu | Cột ngã nằm ngang qua khe thay vì thành bậc. | Có sẵn, chỉ đổi cách đặt. |
| 9. Domino | Một cú đổ kéo theo chuỗi cột, cột cuối mở đường. | Cột đổ đẩy cột kế tiếp (tính như thanh cứng quay quanh chân, như cột hiện tại). |
| 10. Cửa sập dưới thùng | Tay nắm mở cửa sập, thùng rơi xuống hố thành bậc. Không dùng rút chốt (Mrk đã bác tháp chốt). | Cửa sập trên bản lề. |
| 11. Bi lăn chắn lối | Bi tự do nằm chắn lối hẹp, phải đẩy hoặc nhử đi chỗ khác. | Không cần mới; phải chứng minh không kẹt vĩnh viễn. |
| 12. Chuông, lục lạc | Chạm hay bò qua thì kêu, chỉ để vui, không có vai trò giải đố. | Âm thanh và vật trang trí. |

## 3. Hướng triển khai (chọn khi tới giai đoạn này)

- **A. Làm trọn chương 7 vật lý.** Game đếm 11 màn một chương, nên 66–68 là ba màn đầu chương 7. Làm tiếp 69–75 và Boss
  76 bằng các ý ở mục 2, theo nhịp giới thiệu → luyện → kết hợp. Ví dụ: bi một phần tư, gàu ròng rọc, cầu treo (giới
  thiệu rồi luyện), đường lăn trên kính, domino, dây đu đẩy cột; Boss 76 kết hợp bi, kính vỡ và cầu treo. Trước khi làm
  Boss 76 phải kiểm phần thưởng sau Boss chương 7: Hiểu ra chương 7 (câu hỏi sinh tự động trong `COgheBonus`), thẻ năng
  lực, đồ Home và mốc mở khoá (hiện cúp ở 65).
- **B. Thay màn lặp trong 65 màn.** Mrk đã hoãn ngày 20/10/2026 ("giữ nguyên level hiện tại"); chỉ làm khi Mrk hỏi lại.
  Bảng các màn lặp và cách đo: `RESEARCH/COGHE_REPEATED_PLAY_2026_10_20.md` trong workspace của agent (bản tóm tắt dưới).
  - Lặp rõ: 17 N13 với 20 "12" (cùng phòng); 40 K04 với 46 K06 (cùng luật chuỗi thùng); 13 G2 với 11 G7 (cùng 3 bước);
    51 "26" với 29 "17" và Boss 43 "20" (cùng 6 bước mở đầu); 64 N19 với 18 N18 (phòng lật trái phải).
  - Lặp nhẹ: 36 "21" và 37 N25 (cố ý: dạy rồi luyện); 58 N32 (phòng của 20); 48 N31 và 57 N49; đoạn cuối ba màn bánh
    răng 28, 39, 45.
  - Mrk muốn giữ 20, 31, 46. Bản nháp đã gửi: F1 → 17 (dựng lại phòng F1 cho khác N18), F2 → 40, F3 → 51; hoặc
    F2 → 51, F3 → 58 và giữ K04. F1 dùng phòng N18 và F3 dùng phòng N25, nên không đặt gần 18 và 36–37.
- **C. Rắc đồ nghịch.** Thêm chuông, lục lạc (ý 12) vào màn có sẵn, không đổi lời giải. Vẫn chạy lại kiểm tra đường tắt
  và bẫy kẹt cho màn được thêm.

## 4. Luật giữ nguyên

- Không bắt phản xạ hay canh thời gian.
- Vật COghe đứng lên được là thêm đường: chạy lại kiểm tra đường tắt.
- Không có trạng thái không cứu được: Retry phải đặt lại mọi vật (bi, kính, cột…).
- Bi chỉ lăn trong rãnh hoặc vùng giới hạn để kết quả lặp lại; ít vật động cùng lúc.
- Mỗi màn mới phải đòi một ý khác (luật 6.1.4 trong `Docs/COGHE_LEVEL_DESIGN_RULES.md`); đừng dựng lại phòng của màn
  nằm gần nó.
- Bài học khi dựng 66–68 (vách rãnh quay mặt vào trong, chốt nhấc 10 cm, kính cân nguyên thân, khe hẹp giữa rãnh và
  bục) ở mục "Bài học khi dựng" của [README.md](README.md).

## 5. Các bước cho mỗi màn vật lý mới

1. Dựng trong `COghePhysicsPropLevels.cs` (khoá F4, F5…), thêm vào `PlusLevels` và `SpatialOrder`. Nếu chưa chắc giữ
   thì đặt sau dòng `// Prototypes` để kiểm tra chuỗi học bỏ qua.
2. Test: Solve, Wander, giải ở 60 và 30 khung/giây, test âm (cơ quan không lách được), kiểm tra đường tắt và bẫy kẹt.
3. Cập nhật `curriculum.json`, số màn trong `Tools/check-coghe-curriculum.py` và các assert số màn trong test
   (`COgheProductUITests`, `COgheSpatialRecoveryTests`).
4. Quay clip có phụ đề từng bước, gửi Mrk, chờ duyệt.
5. Chạy đủ PlayMode suite, commit, push `NewGraphic`.
