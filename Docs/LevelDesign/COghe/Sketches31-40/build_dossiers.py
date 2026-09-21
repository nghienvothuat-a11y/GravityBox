"""Generate new v1 design dossiers from the reviewed data. No runtime changes."""
from pathlib import Path
import json
import html

ROOT = Path(__file__).resolve().parent
DATA = json.loads((ROOT / 'designs.json').read_text())

COMMON = '''
## Hợp đồng chung cho chương

- **Đây là thiết kế minh họa v1, chưa dựng scene hoặc playtest Unity.** Các mốc phút, dung sai và độ khó là mục tiêu khảo sát.
- Màn chơi/ID mới đề xuất **31–40 nối sau campaign hiện tại**; giữ nguyên toàn bộ ID 01–30. Không nhầm content 30 cũ với Boss ở vị trí chơi 30.
- Tất cả khóa xoay, camera tổng quan 3/4 và nhìn gần theo phần chọn. Chạm cơ quan để tới bám, chạm vùng đích để đẩy/kéo; không kéo đồ vật từ xa. Chọn một phần tại một thời điểm, phần khác giữ việc đã giao.
- Cần lớn, một trục có chặn rõ. Mục tiêu vùng chạm ít nhất 48 điểm logic trên điện thoại; phải đo lại trên hai tỷ lệ 9:16 và màn dọc dài, safe area thật. Đường bám nằm cạnh ray, không treo cơ thể bằng lực vô hạn.
- Mỗi nhịp kéo mục tiêu hoàn tất trước timeout prop 3 giây (thử khoảng 2–2,5 giây gồm tăng tốc). Nếu chưa đạt, sửa tải/tỉ số/hành trình; không buộc spam chạm. Nút giữ không chịu timeout prop.
- Cơ quan chia hai loại đọc được bằng hình: **lò xo hồi** cần giữ liên tục; **ngàm chốt** giữ kết quả. Phanh giữ tải giữa hành trình được ghi riêng, không coi là đã hoàn tất. Mất nguồn phải ngừng truyền công.
- Dao báo 1 giây rồi cắt mô thật, không ép chia đều. Tự tụ khi đủ gần và không bị ngăn, kể cả giữ nút/khác đích; không cooldown. Không kiểm tra số phần chính xác để mở máy.
- Lối chuyển khoang là Transfer, đi hai chiều; lỗ cuối là FinalExit. **Hợp thể trong hộp trước lần ra đầu tiên; toàn bộ 32 hạt ra mới thắng.** Ra sớm khi còn phần khác: “bạn phải hợp thể trước khi chui ra”.
- Vách kín từ sàn tới trần và hai đầu, cửa/ống có lỗ thật. Nóc vẫn nhận đích chạm hợp lệ. Không cấm leo bằng luật ẩn; nếu tìm được đường tắt vật lý hợp lệ thì chấp nhận hoặc sửa hình học công khai.
- Rơi tới sàn an toàn có đường leo lại; không yêu cầu chạm giữa lúc rơi, căn răng, chia đúng tỷ lệ hoặc làm nhiều ngón đồng thời. Không Copy, buff Nhà, chỉ số nâng cấp, đồng hồ đếm ngược.
- Hình bút bi xanh là tài liệu tác giả; không phải ảnh gameplay. Các insets là thời điểm khác, không thêm bản thể. Nét đứt là vị trí sau/đường bị che, không phải lối đi xuyên vật.
- Khi dựng art dùng Day Lab: kính sạch, sứ ấm, cơ quan amber, ống cyan, vùng trơn lavender có vân, exit mint sát mặt; sinh vật đen bóng không mắt/miệng. Nhãn và chất liệu cùng diễn tả chức năng, không chỉ màu.
- Màn 40 không có lời giải, số bước hay mũi tên đáp án trong HUD. Phản hồi thao tác, nguồn/ly hợp/chốt và icon khóa xoay vẫn có. Không thêm phần thưởng vĩnh viễn chưa được yêu cầu.

## Căn cứ đã đọc

[Luật hiện hành](../../../COGHE_LEVEL_DESIGN_RULES.md) · [Campaign 30](../../../COGHE_CAMPAIGN_30_DESIGN.md) · [Kỹ năng](../../../VENOM_CREATURE_SKILLS.md) · [Giải đố thuần](../../../VENOM_PURE_PUZZLE_AND_HOME.md) · [Template](../LEVEL_TEMPLATE.md) · [Day Lab](../../../ArtDirection/COghe/STYLE_RULES.md) · [Bộ phác cũ](../Sketches21-30/README.md).

Luật hiện hành ưu tiên hơn các đoạn Journey cũ về cooldown/chặn tụ. GDD gốc `Docs/GDD_REFERENCE.md` là Steel Ball Lab, không áp rotation-only vào COghe. Nguồn đọc tại commit `5fb2609f0407e0871e5d973aa41a7b65af7f3b05`.
'''

HEADINGS = [
 '1. Định danh, phạm vi và trạng thái',
 '2. Mục tiêu trải nghiệm và vị trí trong tiến trình',
 '3. Phác thảo, hình học, camera và thao tác',
 '4. Lời giải, kết thúc và phục hồi',
 '5. Cơ quan, trạng thái và kiến trúc dùng lại',
 '6. Hình ảnh, animation và phản hồi',
 '7. Độ khó và ngân sách runtime',
 '8. Chơi thử và hồi quy',
 '9. Bằng chứng hiệu năng trên thiết bị',
 '10. Tích hợp, tương thích và nghiệm thu'
]

def write_new(path, text):
    if path.exists():
        raise FileExistsError(path)
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(text)

for l in DATA['levels']:
    n=l['id']; previous=n-1
    sections = [
      f"ID đề xuất `venom.origin.{n}` · vị trí chơi **{n}** · {l['title']}. Người thiết kế: Codex, 21/09/2026. **Nháp v1** theo yêu cầu Mrk; không tự ghi là đã duyệt. Hồ sơ và phác thảo mới, không thay scene cũ. Chưa có scene/definition/builder mới; chưa kiểm chứng Editor/Mac/mobile.\n\nThiết kế: đủ brief/lời giải/phục hồi để review. Prototype, hoàn thiện và nghiệm thu: chưa thực hiện. [Hợp đồng chương và nguồn](../Sketches31-40/README.md).",
      l['idea']+f"\n\nDùng kỹ năng bò/leo/luồn/đẩy/kéo đã có"+(", cùng cắt/tụ và giao việc cho phần cơ thể." if l['roles']>1 else "; không thêm kiểu input.")+f" Vai trò: {'Boss tổng hợp, không hướng dẫn lời giải' if n==40 else 'nghỉ nhịp sau Boss 30, giới thiệu lùi để mở quyền tiếp cận' if n==31 else 'luyện/kết hợp thêm quan hệ so với màn '+str(previous)}. Kết quả chỉ ghi hoàn tất màn, không cấp buff hoặc mở lại Nhà. Điểm mới là quan hệ cơ quan trong bố cục này, không phải kỹ năng có thể mua.",
      f"![Màn {n} — {l['title']}](mockup-v1.png)\n\n"+l['layout']+"\n\nKhóa xoay; tổng quan 3/4 thấy spawn, cơ quan mục tiêu và cửa cuối. Nhìn gần/theo phần chọn chỉ đổi camera. Màn nhiều khoang có nút xem khoang; không che trạng thái phần đang giữ ở khoang khác. Các bản vẽ không theo tỷ lệ. Kích thước khởi điểm để blockout: bệ tụ khoảng 0,18 × 0,18 m; đường chân bám dọc ray khoảng 0,12 m; tay nắm 0,05–0,06 m. Đây là giả thuyết, phải kiểm tra theo kích thước/tầm giãn mô thật; chưa chốt clearance ống. Không dùng ngưỡng khối lượng vô hình tại ống.\n\nChạm sàn/đích → bò/leo; chạm tay → tới bám; chạm đầu hành trình → đẩy/kéo; đổi lệnh → hủy lực cũ. Nóc/trơn vẫn nhận đích hợp lệ; mặt trơn không sinh lực bám. Retry/pause/zoom/chọn phần nằm ngoài hộp và safe area.",
      '\n'.join(f"{i+1}. {s}" for i,s in enumerate(l['steps']))+"\n\n**Sửa sai:** "+l['recovery']+"\n\n**Điều kiện hoàn tất:** tụ trong hộp trước khi ra FinalExit, rồi đủ toàn bộ 32 hạt. Cửa/ống nội bộ chỉ chuyển khoang. Chấp nhận cách giải khác đúng vật lý, không khóa theo lịch sử từng bước. Retry trả vị trí/vận tốc, chốt, tải, ly hợp, lệnh và mô về trạng thái đầu; pause dừng mô phỏng, không để callback cũ mở máy. Mất phần/cửa kẹp/không quay lại tay nắm được là lỗi prototype cần sửa, không coi Retry là lời giải thường lệ.",
      "**Quan hệ:** `"+l['logic']+"`.\n\n"+l['extension']+"\n\nDùng `VenomMovableProp`, `COgheRailSlider`, `COgheGearTrain`, `COgheTissueSensor`, `COgheGuillotine` và cooperative winch khi hợp đồng phù hợp. Đây là hướng tái dùng, không khẳng định driver mới đã tồn tại. Motor mô-men hữu hạn, điểm bám/phản lực thật, chốt chỉ bắt ở cuối hành trình thật. Mọi cơ quan reset cùng scene; ngắt nguồn không được animation tiếp tục truyền lực. Pose cơ quan/cửa đổi phải cập nhật route và aperture. Không rải nhánh theo số màn hoặc đọc lời giải mẫu trong runtime.",
      l['feedback']+"\n\nChuỗi phản hồi: nhận chạm → đi tới → bám → truyền lực → vật chuyển/va chặn → chốt thật. Đèn chỉ báo trạng thái đo được; âm khớp/catch ngắn, không thay tín hiệu hình. Sinh vật không tự giải bước tiếp theo. Phác thảo tạo bằng imagegen tích hợp; prompt và manifest ở thư mục review. Khi dựng cần ảnh Unity thật theo Day Lab 07. Ăn mừng/chuyển màn dùng luồng hiện có"+("; Boss chờ lựa chọn tiếp tục." if n==40 else "."),
      f"Dự kiến **{l['decisions']} cụm quyết định**, tối đa **{l['roles']} vai trò cùng lúc** trong lời giải mẫu; đây không phải số cú chạm tối thiểu. Mục tiêu lần đầu {l['minutes']} phút, **chưa đo**. Độ chính xác/thời điểm giữ thấp; cho phép dừng quan sát giữa các bước.\n\nRủi ro riêng: "+l['risk']+"\n\nGiữ baseline 32 hạt/120 Hz, Day Lab một key light có bóng. Dự kiến điểm nặng: pose cơ quan → cập nhật đường đi, tiếp xúc ray/catch"+(" và tách/tụ/đổi phần." if l['roles']>1 else ".")+" Chưa chốt ngân sách collider/joint/GC/GPU; số cơ quan không chứng minh FPS. So cùng thiết bị với màn cơ khí/tam hợp hiện có, không giảm luật vật lý để đạt chỉ tiêu.",
      "Chưa chạy gameplay tests cho các màn mới. Checklist khi prototype:\n\n- Giải trọn bằng chạm/API điều khiển thật, không teleport mô/gán cửa mở; thử cả camera tổng quan và gần.\n- Làm ngược thứ tự, kéo dở, buông sau timeout, đổi phần, mất tải giữa hành trình, pause/reset ở mọi chốt.\n- "+l['risk']+"\n- Với nhiều phần: cắt lệch, tụ sớm, nhiều hơn số vai trò mẫu, mọi phần có đường về; đủ mô, không nhập xuyên kính, không thoát sớm. Với một thân: thử leo vòng vách/tay nắm và tiếp cận từ nóc.\n- Với ray/ống/cầu: đo nhiều đích chạm lệch; không kẹp, không mắc tại chặn, không bám lại liên tục để giải.\n- Playtest người chưa biết lời giải và replay người đã biết: ghi số người/lượt, thời gian suy luận riêng thao tác, điểm chạm sai, lần thử lại và điểm không hiểu.\n- Khi sửa shared runtime chạy toàn bộ EditMode/PlayMode của package và hồi quy các scene dùng chung.",
      "**Chưa có build, thiết bị, mẫu frame-time hoặc FPS cho màn này.** Chưa có chứng nhận performance. Khi dựng: đo cùng máy/profile với baseline 30 màn, ghi commit+diff+build hash, resolution/safe area, warm-up, thời lượng thực; tách CPU/GPU, p50/p95/p99/max, GC/memory, spike lúc ra lệnh/cắt/tụ/cửa đổi. Mục tiêu chung 60 FPS cần đo trên mobile; thử phiên 15–20 phút để quan sát nhiệt. Không dùng hình minh họa hoặc test logic thay số đo.",
      f"Vị trí {n}/ID {n} là đề xuất mới, chưa đăng ký catalog, scene, save hoặc build list; giữ nguyên các ID cũ. Khi tích hợp kiểm tra mở tiếp/replay/save idempotent, mất điện giữa lưu và ăn mừng; không cấp Nhà lại. Màn 40 kết thúc chương mới, chưa tự đặt màn 41.\n\nKết luận: **bàn giao thiết kế minh họa**, chưa nghiệm thu playable. Bước tiếp theo nếu triển khai là blockout và kiểm chứng rủi ro nêu trên trước art; không cần build game để review tài liệu này. Lịch sử v1: tạo theo yêu cầu 21/09/2026"+("; đã sửa bước trả X về trước khi trả cầu B để tránh va chạm." if n==34 else ".")
    ]
    dossier=f"# COghe — Màn {n} — {l['title']}\n\n"+'\n\n'.join('## '+h+'\n\n'+s for h,s in zip(HEADINGS,sections))+'\n'
    write_new(ROOT.parent / f'Level{n}' / 'README.md',dossier)

table='| Màn | Thiết kế | Cụm quyết định / vai trò | Điểm tăng suy luận |\n| --- | --- | --- | --- |\n'
for l in DATA['levels']:
    table+=f"| {l['id']} | [{l['title']}](../Level{l['id']}/README.md) | {l['decisions']} / {l['roles']} | {l['idea']} |\n"
intro='''# COghe — Thiết kế minh họa màn chơi 31–40

21/09/2026 · Nháp v1 · Codex · Theo yêu cầu Mrk.

[Mở gallery ảnh và mô tả](review.html). Mỗi màn có một PNG gốc và hồ sơ theo template của repo. Đây là bộ thiết kế nối tiếp vị trí chơi 30 “Tam hợp”, không phải sắp lại các màn đã có.

Chủ đề chương: **dùng lại cơ quan, đổi thứ tự và đổi vai**. Màn 31 nghỉ nhịp sau Boss 30; từ đó tăng dần phạm vi phải dự tính. 31–34 một cơ thể, 35–37 hai vai trò, 38 đổi phân bổ mô, 39–40 ba vai trò. Số bước không phải một thang khó duy nhất: 35 ít bước hơn 34 nhưng thêm việc duy trì hai nhiệm vụ ở hai nơi. Mức khó/phút chỉ là dự đoán cần kiểm tra với người chơi.

'''
summaries=''
for l in DATA['levels']:
    summaries+=f"\n## {l['id']} — {l['title']}\n\n![Phác thảo {l['id']}](../Level{l['id']}/mockup-v1.png)\n\n{l['idea']}\n\n"+'\n'.join(f"{i+1}. {s}" for i,s in enumerate(l['steps']))+f"\n\n**Nếu làm sai:** {l['recovery']}\n\n[Hồ sơ đầy đủ](../Level{l['id']}/README.md).\n"
write_new(ROOT/'README.md',intro+table+COMMON+summaries)

cards=[]
for l in DATA['levels']:
    e=html.escape
    cards.append(f'''<article id="level-{l['id']}"><a class="visual" href="../Level{l['id']}/mockup-v1.png"><img loading="lazy" src="../Level{l['id']}/mockup-v1.png" alt="Màn {l['id']}: {e(l['title'])}"></a><div class="copy"><p class="eyebrow">MÀN {l['id']} / {'BOSS' if l['id']==40 else 'CHƯƠNG 04'}</p><h2>{e(l['title'])}</h2><p class="idea">{e(l['idea'])}</p><p class="meta">{l['decisions']} cụm quyết định · {l['roles']} vai trò · Khóa xoay</p><h3>Lời giải dự kiến</h3><ol>{''.join('<li>'+e(s)+'</li>' for s in l['steps'])}</ol><h3>Sai vẫn sửa được</h3><p>{e(l['recovery'])}</p><details><summary>Bố cục và cơ quan</summary><p>{e(l['layout'])}</p><p><strong>Cần kiểm chứng:</strong> {e(l['risk'])}</p></details><p><a href="../Level{l['id']}/README.md">Hồ sơ đầy đủ ↗</a> · <a href="../Level{l['id']}/mockup-v1.png" download>Tải ảnh gốc</a></p></div></article>''')
page='''<!doctype html><html lang="vi"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>COghe — Thiết kế 31–40</title><style>
:root{color-scheme:light;--ink:#203b57;--muted:#64717d;--accent:#143eae}*{box-sizing:border-box}body{margin:0;background:#f5f3ed;color:var(--ink);font:17px/1.65 system-ui,sans-serif}header,main,footer{max-width:1300px;margin:auto;padding:32px}header{padding-top:64px}h1{font-size:clamp(32px,5vw,64px);line-height:1.12;margin:8px 0 20px}h2{font-size:32px;line-height:1.2;margin:4px 0 18px}h3{font-size:18px;margin:24px 0 8px}.eyebrow{font-size:12px;font-weight:750;letter-spacing:.15em;color:var(--accent)}.intro{max-width:840px;font-size:20px}.note{max-width:900px;border-left:3px solid #d7a749;padding-left:16px;color:var(--muted)}nav{display:flex;flex-wrap:wrap;gap:10px;margin-top:28px}a{color:var(--accent)}nav a{padding:6px 15px;border:1px solid #cbd2da;border-radius:30px;text-decoration:none}article{display:grid;grid-template-columns:1fr 1fr;gap:36px;margin:20px 0 56px;background:#fff;border-radius:14px;padding:26px;box-shadow:0 6px 24px #183c5710;scroll-margin-top:15px}.visual img{display:block;width:100%;height:auto;border:1px solid #e2e6ee}.copy{padding:12px}.idea{font-size:20px}.meta{font-size:14px;color:var(--muted)}li{padding-left:4px;margin-bottom:10px}ol{padding-left:25px}summary{cursor:pointer;font-weight:600}footer{color:var(--muted);font-size:14px}@media(max-width:820px){header,main,footer{padding:20px}header{padding-top:35px}article{grid-template-columns:1fr;padding:12px;gap:8px}.copy{padding:14px}h2{font-size:28px}}@media print{nav{display:none}article{break-before:page;box-shadow:none;border:0;margin:0}.copy{font-size:12px}h2{font-size:23px}}
</style><header><p class="eyebrow">COGHE / VENOM · LEVEL DESIGN · 21.09.2026</p><h1>Mười chiếc hộp.<br>Mười cách nghĩ tiếp.</h1><p class="intro">Thiết kế minh họa màn chơi <strong>31–40</strong>: dùng lại cơ quan, đổi thứ tự, đổi vai — thao tác bằng chạm, có thời gian suy nghĩ.</p><p class="note">Nháp v1 để review. Đây là hình thiết kế, chưa phải gameplay Unity. Độ khó, thời gian và khả giải vật lý cần prototype/playtest. Màn 31 nghỉ nhịp sau Boss 30; từ đó tăng dần số quan hệ cần dự tính. Mũi tên lời giải không đưa vào HUD Boss.</p><nav>'''+''.join(f'<a href="#level-{n}">{n}</a>' for n in range(31,41))+'''</nav></header><main>'''+''.join(cards)+'''</main><footer>Luật giữ nguyên: chỉ tách qua dao; gần nhau tự tụ; hợp thể trong hộp trước khi thoát; đủ toàn bộ 32 hạt mới thắng. Không nâng chỉ số, không Copy, không thử thách bấm đúng nhịp.<br><a href="README.md">Hợp đồng chương và căn cứ thiết kế</a> · <a href="generation-prompts.json">Prompt ảnh</a></footer></html>'''
write_new(ROOT/'review.html',page)
print('Created 10 dossiers, chapter README and responsive review gallery.')
