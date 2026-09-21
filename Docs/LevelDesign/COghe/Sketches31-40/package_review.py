from pathlib import Path
import json, shutil, zipfile, re

ROOT=Path(__file__).resolve().parent
DATA=json.loads((ROOT/'designs.json').read_text())
OUT=Path('/Users/tommynguyen/.buzz/OUTBOX/COGHE_LEVELS_31_40_V1')
OUT.mkdir(parents=True,exist_ok=True)
(OUT/'images').mkdir(exist_ok=True)
(OUT/'details').mkdir(exist_ok=True)
for l in DATA['levels']:
    n=l['id'];d=ROOT.parent/f'Level{n}'
    shutil.copy2(d/'mockup-v1.png',OUT/'images'/f'level-{n}.png')
    s=(d/'README.md').read_text().replace('(mockup-v1.png)',f'(../images/level-{n}.png)').replace('(../Sketches31-40/README.md)','(../README.md)').replace('(../Sketches31-40/layout34-states.svg)','(../layout34-states.svg)')
    (OUT/'details'/f'LEVEL_{n}.md').write_text(s)
gallery=(ROOT/'review.html').read_text()
for l in DATA['levels']:
    n=l['id'];gallery=gallery.replace(f'../Level{n}/mockup-v1.png',f'images/level-{n}.png').replace(f'../Level{n}/README.md',f'details/LEVEL_{n}.md')
gallery=gallery.replace('generation-prompts.json','PROMPTS.json')
(OUT/'START_HERE.html').write_text(gallery)
for name,target in [('COGHE_LEVELS_31_40_REVIEW_V1.pdf','COGHE_LEVELS_31_40_REVIEW_V1.pdf'),('generation-prompts.json','PROMPTS.json'),('generation-manifest.json','IMAGE_MANIFEST.json'),('designs.json','DESIGNS.json'),('logic-audit.json','LOGIC_AUDIT.json'),('layout34-states.svg','layout34-states.svg')]:
    shutil.copy2(ROOT/name,OUT/target)
readme='''# COghe — Thiết kế minh họa màn chơi 31–40

Nháp v1 · 21/09/2026 · Codex · Theo yêu cầu Mrk.

Mở **START_HERE.html** để xem gallery có mô tả; đọc **COGHE_LEVELS_31_40_REVIEW_V1.pdf** để xem cả bộ. Thư mục `images/` có 10 ảnh PNG, `details/` có hồ sơ từng màn. `layout34-states.svg` bổ sung mặt bằng cơ quan cho màn 34.

Đây là thiết kế minh họa, chưa phải scene/gameplay Unity. 31 là nhịp nghỉ sau Boss 30; 31–34 tăng lập kế hoạch một thân, 35–37 hai vai trò, 38 đổi phân bổ mô, 39–40 ba vai trò. Toàn chương khóa xoay, chạm chỉ dẫn; không yêu cầu căn thời điểm hoặc điều khiển nhiều ngón. Mức khó và thời gian là dự đoán cần playtest.

Hình giản lược cơ cấu, tỷ lệ và đường truyền, không thay bản lắp ráp. Nét đứt chỉ vị trí/chi tiết bị che, insets là thời điểm khác. Hồ sơ và sơ đồ mặt bằng quy định liên động dùng để dựng; không lấy chi tiết trang trí của ảnh AI làm collider.

Giữ luật: chỉ tách qua dao; đủ gần tự tụ nếu không bị ngăn; hợp thể trong hộp trước khi thoát; đủ 32 hạt qua lỗ cuối mới thắng. Boss không có gợi ý lời giải trong game; các bước ở đây dành cho tác giả/reviewer. Không Copy, buff Nhà hoặc nâng chỉ số.

## Mười màn

'''
for l in DATA['levels']:
    n=l['id'];readme+=f"### {n} — {l['title']}\n\n{l['idea']}\n\n"+'\n'.join(f"{i+1}. {s}" for i,s in enumerate(l['steps']))+f"\n\n[Ảnh](images/level-{n}.png) · [Hồ sơ](details/LEVEL_{n}.md)\n\n"
readme+='''## Căn cứ và kiểm chứng

Nguồn trong repo GravityBox nhánh Venom, commit `5fb2609f0407e0871e5d973aa41a7b65af7f3b05`: `AGENTS.md`, `Docs/COGHE_LEVEL_DESIGN_RULES.md`, `Docs/COGHE_CAMPAIGN_30_DESIGN.md`, `Docs/VENOM_CREATURE_SKILLS.md`, `Docs/VENOM_PURE_PUZZLE_AND_HOME.md`, `Docs/ArtDirection/COghe/STYLE_RULES.md`, template và mẫu level 20/27/28/29, `Sketches21-30`.

Mô hình trừu tượng màn 34 đã rà 9 trạng thái, có đường giải từ tất cả; mẫu ba vai trò màn 39 đã rà 192 trạng thái, có đường hoàn tất rồi tập hợp. Mẫu hai vai trò có 16 trạng thái, không hoàn tất theo giả định một phần chỉ làm một trạm. Xem LOGIC_AUDIT.json; không mô phỏng vật lý/mô mềm/input và không chứng minh khả giải Unity. Các màn khác tự rà lời giải/phục hồi trong hồ sơ, không tuyên bố đã vét cạn.

Bảy ảnh phác tạo/chỉnh bằng imagegen tích hợp; 34/38/39 dùng sơ đồ cơ quan và trạng thái dựng trực tiếp. PROMPTS.json và IMAGE_MANIFEST.json giữ nguồn và lịch sử lựa chọn. Không thay code/scene/catalog/save của 30 màn đang có.
'''
(OUT/'README.md').write_text(readme)
# Check all packaged local links, including image references.
missing=[]
for f in list(OUT.rglob('*.md'))+[OUT/'START_HERE.html']:
    text=f.read_text()
    dests=re.findall(r'\]\(([^)]+)\)',text) if f.suffix=='.md' else re.findall(r'(?:href|src)="([^"]+)"',text)
    for dest in dests:
        if dest.startswith('#') or ':' in dest:continue
        if not (f.parent/dest.split('#')[0]).exists():missing.append([str(f),dest])
assert not missing,missing
archive=OUT.with_suffix('.zip')
with zipfile.ZipFile(archive,'w',zipfile.ZIP_DEFLATED) as z:
    for f in OUT.rglob('*'):
        if f.is_file():z.write(f,Path(OUT.name)/f.relative_to(OUT))
print(json.dumps({'folder':str(OUT),'zip':str(archive),'images':len(list((OUT/'images').glob('*.png'))),'files':len(list(p for p in OUT.rglob('*') if p.is_file()))},ensure_ascii=False))
