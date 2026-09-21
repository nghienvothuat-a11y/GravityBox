"""Create the review booklet after all selected illustrations are copied."""
from pathlib import Path
from xml.sax.saxutils import escape
import json
from reportlab.pdfgen import canvas
from reportlab.pdfbase import pdfmetrics
from reportlab.pdfbase.ttfonts import TTFont
from reportlab.platypus import Paragraph
from reportlab.lib.styles import ParagraphStyle
from reportlab.lib.colors import HexColor

ROOT=Path(__file__).resolve().parent
DATA=json.loads((ROOT/'designs.json').read_text())
OUT=ROOT/'COGHE_LEVELS_31_40_REVIEW_V1.pdf'
FONT='/System/Library/Fonts/Supplemental/'
pdfmetrics.registerFont(TTFont('Body', FONT+'Arial.ttf'))
pdfmetrics.registerFont(TTFont('Bold', FONT+'Arial Bold.ttf'))
W,H=1120,790
c=canvas.Canvas(str(OUT), pagesize=(W,H))
c.setTitle('COghe - Thiết kế minh họa màn 31-40 - v1')
c.setAuthor('Codex')
INK='#203B57'; BLUE='#1746A4'; MUTED='#677583'
def label(s,x,y,size=12,color=INK,bold=False):
    c.setFillColor(HexColor(color));c.setFont('Bold' if bold else 'Body',size);c.drawString(x,y,s)
def para(s,x,y,width,size=13,leading=19,color=INK,bold=False):
    st=ParagraphStyle('p',fontName='Bold' if bold else 'Body',fontSize=size,leading=leading,textColor=HexColor(color))
    p=Paragraph(s,st);_,height=p.wrap(width,H);p.drawOn(c,x,y-height);return y-height
def base(page):
    c.setFillColor(HexColor('#F7F6F0'));c.rect(0,0,W,H,fill=1,stroke=0)
    label('COghe / VENOM    ·    LEVEL DESIGN    ·    21.09.2026',40,H-30,10,BLUE,True)
    c.setStrokeColor(HexColor('#D3DCE4'));c.line(40,42,W-40,42)
    label('Nháp thiết kế v1 · Chưa dựng / playtest Unity',40,25,10,MUTED)
    c.setFont('Body',10);c.drawRightString(W-40,25,str(page))
base(1)
label('Màn chơi 31-40',45,690,48,INK,True)
label('Dùng lại cơ quan. Đổi thứ tự. Đổi vai.',45,642,25,BLUE)
y=para('Mười thiết kế nối tiếp campaign 30 màn hiện hành. Giữ kiểu phác bút bi xanh và cơ quan dễ đọc; tăng suy luận bằng trình tự, quan hệ phụ thuộc và phân công cơ thể.',45,600,990,17,26)
y=para('<b>Thao tác chung:</b> khóa xoay, chạm để giao đích, tay nắm lớn và ray có chặn. Có thể dừng suy nghĩ; không yêu cầu bấm đúng thời điểm hoặc điều khiển nhiều ngón.',45,y-22,990,15,23)
y-=42
label('MÀN',45,y,11,BLUE,True);label('TÊN',115,y,11,BLUE,True);label('TRỌNG TÂM',420,y,11,BLUE,True)
shorts={31:'Lùi để lộ tay mở khóa',32:'Một cầu dùng cho hai bến',33:'Đi nhánh phụ rồi trở về đổi tuyến',34:'Mượn chỗ đỗ rồi trả lại cho cầu',35:'Chốt cửa để người giữ được rời nút',36:'Hai phần đổi vai cho nhau',37:'Giữ nguồn, dùng lại cùng bánh răng',38:'Chia - tụ - chia lại theo việc',39:'Ba vị trí, hai đầu ra có thứ tự',40:'Tổng hợp chuẩn bị, phân vai, đoàn tụ'}
for l in DATA['levels']:
    y-=29;label(str(l['id']),45,y,13,BLUE,True);label(l['title'],115,y,13,INK,True);label(shorts[l['id']],420,y,13)
y=para('<b>Nhịp chương:</b> 31 nghỉ sau Boss 30; 31-34 một thân, 35-37 hai vai trò, 38 đổi phân bổ mô, 39-40 ba vai trò. Mức khó là dự đoán thiết kế, cần hiệu chỉnh bằng playtest.',45,y-28,990,13,19)
assert y>58,y
c.showPage()
for p,l in enumerate(DATA['levels'],2):
    base(p)
    img=ROOT.parent/f"Level{l['id']}"/'mockup-v1.png'
    if not img.exists():raise FileNotFoundError(img)
    c.drawImage(str(img),35,55,width=450,height=675,preserveAspectRatio=True,anchor='c',mask='auto')
    x=515;width=560;y=722
    y=para(f"{l['id']} / {escape(l['title'])}",x,y,width,25,31,INK,True)
    y=para(escape(l['idea']),x,y-14,width,15,22)
    y=para(f"{l['decisions']} cụm quyết định · {l['roles']} vai trò · Khóa xoay",x,y-13,width,11,17,MUTED)
    y=para('LỜI GIẢI DỰ KIẾN',x,y-21,width,11,17,BLUE,True)
    for i,s in enumerate(l['steps'],1):
        y=para(f'<b>{i}.</b> '+escape(s),x,y-9,width,13,19)
    y=para('SAI VẪN SỬA ĐƯỢC',x,y-20,width,11,17,BLUE,True)
    y=para(escape(l['recovery']),x,y-8,width,12,18)
    y=para('CẦN KIỂM CHỨNG KHI DỰNG',x,y-18,width,10,16,BLUE,True)
    y=para(escape(l['risk']),x,y-7,width,11,16,MUTED)
    if l['id']==40:
        y=para('<b>Các bước trong hình chỉ dành cho tác giả. Boss trong game không có gợi ý lời giải.</b>',x,y-10,width,11,16)
    assert y>=57,(l['id'],y)
    c.showPage()
base(12)
label('Luật giữ nguyên / giới hạn bàn giao',45,704,32,INK,True)
y=684
blocks=[
('Luật cơ thể','Chỉ tách qua dao. Đủ gần và không có vật cản thì tự tụ, kể cả đang giữ nút; không cooldown. Hợp thể trong hộp trước khi đi ra FinalExit; toàn bộ 32 hạt phải thoát. Ống chuyển khoang không phải cửa thắng.'),
('Cơ quan phải nói đúng trạng thái','Ray, chốt, cáp và đường ống có hình học thật. Lò xo hồi khác chốt giữ. Đèn A/B chỉ theo tải; động cơ dừng khi mất nguồn. Không mở theo số phần, nhãn đỗ đúng ô, animation hoặc lịch sử làm đúng bước.'),
('Độ khó và điều khiển','Tăng quan hệ phụ thuộc, dùng lại cơ quan và phân vai. Mục tiêu tay nắm lớn, một lệnh tới đầu ray; nhịp kéo hoàn tất trước timeout prop, không spam chạm. Màn 31 nghỉ nhịp có chủ đích sau Boss 30. Các mức phút/độ khó trong hồ sơ chưa là số đo.'),
('Mức kiểm chứng của gói này','Đã đọc nguồn, viết lời giải/phục hồi và tự rà liên động trên giấy; đã rà các hình để thống nhất chú thích và trạng thái. Chưa dựng scene 31-40, chưa kiểm chứng lực/clearance, input điện thoại, khả giải Unity hoặc FPS. Bản vẽ giản lược đường truyền và tỷ lệ; dùng hồ sơ chi tiết để blockout.'),
('Nguồn chuẩn trong repository','AGENTS.md; Docs/COGHE_LEVEL_DESIGN_RULES.md; Docs/COGHE_CAMPAIGN_30_DESIGN.md; Docs/VENOM_CREATURE_SKILLS.md; Docs/VENOM_PURE_PUZZLE_AND_HOME.md; Docs/ArtDirection/COghe/STYLE_RULES.md; Docs/LevelDesign/COghe/LEVEL_TEMPLATE.md; các hồ sơ/mẫu 20, 27, 28, 29 và Sketches21-30.'),
('Phiên bản và nguồn ảnh','Nguồn đọc tại commit 5fb2609f0407e0871e5d973aa41a7b65af7f3b05. Bảy ảnh phác tạo/chỉnh bằng imagegen tích hợp; 34/38/39 dùng sơ đồ cơ quan và trạng thái dựng trực tiếp. Prompt và manifest nằm trong Sketches31-40. Hồ sơ theo template ở Level31 ... Level40. Không thay code, scene, catalog hoặc save của campaign 30 màn.')]
for title,body in blocks:
    y=para(escape(title),45,y-20,1010,14,21,BLUE,True)
    y=para(escape(body),45,y-5,1010,13,20)
assert y>58,y
c.save()
print(OUT)
