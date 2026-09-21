"""New deterministic engineering diagrams for 38/39; no raster artwork is edited."""
from pathlib import Path
from PIL import Image,ImageDraw,ImageFont
root=Path(__file__).resolve().parent
F='/System/Library/Fonts/Supplemental/Arial.ttf';FB='/System/Library/Fonts/Supplemental/Arial Bold.ttf'
INK='#163f9e';BLUE='#6485b9';PALE='#dae6f4'
def font(s,b=False):return ImageFont.truetype(FB if b else F,s)
def start(title,subtitle):
    im=Image.new('RGB',(1200,1800),'white');d=ImageDraw.Draw(im)
    for y in range(40,1800,45):d.line((0,y,1200,y),fill='#e5f0fa')
    d.line((48,0,48,1800),fill='#edc2d2',width=2)
    d.text((78,40),title,font=font(48,True),fill=INK)
    d.text((80,113),subtitle,font=font(25),fill=INK)
    return im,d
def txt(d,x,y,s,size=25,b=False,color=INK):d.text((x,y),s,font=font(size,b),fill=color)
def blob(d,x,y,size=1):
    # Smooth asymmetric liquid silhouette with no face or limbs.
    for box in [(-32,-11,32,18),(-17,-38,14,9),(-36,2,-4,21),(10,-5,39,16)]:
        d.ellipse(tuple((x if i%2==0 else y)+v*size for i,v in enumerate(box)),fill=INK)
def pad(d,x,y,name,occupied):
    d.ellipse((x-40,y-14,x+40,y+23),fill='#e6effa',outline=INK,width=3)
    if occupied:blob(d,x,y)
    txt(d,x-10,y+29,name,26,True)
def lever(d,x,y,name,occupied):
    d.rectangle((x-15,y-12,x+18,y+15),fill='white',outline=INK,width=3)
    d.line((x,y,x+22,y-50),fill=INK,width=5);d.ellipse((x+13,y-59,x+31,y-41),fill='white',outline=INK,width=3)
    if occupied:blob(d,x-58,y);d.line((x-31,y-7,x+18,y-45),fill=INK,width=5)
    txt(d,x+5,y+29,name,25,True)
def wall(d,x,y,name,open_=True,height=260):
    d.line((x,y,x,y+height),fill=BLUE,width=5)
    d.rectangle((x-13,y+height-120,x+13,y+height-40),fill='white',outline=INK,width=3)
    if not open_:d.rectangle((x-13,y+height-120,x+13,y+height-40),fill='#d9e5f5',outline=INK,width=3)
    else:d.rectangle((x-17,y+height-135,x+17,y+height-120),fill='#d9e5f5',outline=INK,width=3)
    txt(d,x-20,y+height+8,name,23,True)
def cutter(d,x,y,name):
    d.rectangle((x-22,y-23,x+22,y+17),outline=BLUE,width=3)
    d.polygon([(x-17,y-19),(x+17,y-19),(x+17,y-8),(x-17,y)],fill=PALE,outline=INK)
    txt(d,x-25,y+23,name,18)
def exit_(d,x,y,open_):
    d.ellipse((x-23,y-30,x+23,y+30),outline=INK,width=4)
    if not open_:d.rectangle((x-29,y-37,x+29,y+37),fill='#dce8f7',outline=INK,width=3)
    else:d.rectangle((x-29,y-58,x+29,y-38),fill='#dce8f7',outline=INK,width=3)
    txt(d,x-10,y+42,'E',24,True)
def qcar(d,x,y,docked):
    d.line((625,y+12,880,y+12),fill=BLUE,width=6)
    d.rectangle((x-45,y-18,x+45,y+42),fill='#e5edf8',outline=INK,width=3)
    txt(d,x-12,y-12,'Q',29,True)
    d.rectangle((888,y-19,934,y+41),outline=INK,width=3);txt(d,895,y-11,'M',25,True)
    txt(d,630,y+57,'Ray Q → ổ truyền M',20)

im,d=start('38 — Tụ để đổi việc','Ba giai đoạn · Cùng một bố cục · Khóa xoay, chạm chỉ dẫn')
for stage,y in enumerate([185,630,1075],1):
    titles={1:'1. CHIA — giữ A, kéo K mở và chốt D',2:'2. TỤ — dùng khối lượng lớn đẩy Q vào ổ',3:'3. CHIA LẠI — giữ B, kéo C mở và chốt E'}
    txt(d,80,y,titles[stage],28,True)
    top=y+75
    d.rectangle((90,top,1110,top+285),fill='white',outline=BLUE,width=3)
    wall(d,540,top,'D',True,260)
    pad(d,230,top+143,'A',stage==1);lever(d,436,top+145,'K',stage==1)
    cutter(d,136,top+47,'Dao 1');cutter(d,592,top+46,'Dao 2')
    qcar(d,720 if stage<3 else 829,top+72,stage==3)
    pad(d,687,top+217,'B',stage==3);lever(d,990,top+217,'C',stage==3)
    if stage==2:
        blob(d,649,top+87,1.02)
        d.line((659,top+71,686,top+71),fill=INK,width=5)
    exit_(d,1109,top+192,stage==3)
    txt(d,97,top+292,'D giữ mở sau khi chốt; các cơ quan không đổi chỗ giữa các hình.',20,color=BLUE)
txt(d,80,1538,'Sau nhịp 3: rời B/C → gặp nhau → tự tụ → thoát đủ cơ thể.',28,True)
blob(d,115,1654,1.2)
txt(d,190,1610,'Kéo Q dựa vào lực thật. Chia lệch mà phần lớn đủ lực vẫn hợp lệ.',23)
txt(d,190,1650,'Tụ giữa màn là lời giải mẫu; hợp thể trước khi thoát là bắt buộc.',23)
txt(d,80,1740,'Sơ đồ chức năng v1; chưa phải kích thước hoặc gameplay Unity.',23,color=BLUE)
im.save(root.parent/'Level38'/'mockup-v1.png')

im,d=start('39 — Ba nơi, hai nhịp','Giai đoạn phối hợp sau khi cắt · Ba phần, ba vị trí làm việc')
for phase,y in [(1,220),(2,870)]:
    txt(d,80,y,('I — Mở và chốt hai cửa đoàn tụ' if phase==1 else 'II — Tiếp tục giữ A/B, kéo C mở cửa E'),31,True)
    top=y+95
    d.rectangle((90,top,1110,top+380),fill='white',outline=BLUE,width=3)
    for x,name in [(430,'D1'),(770,'D2')]:
        wall(d,x,top,name,True,350)
        # Pipe crossing a partition; both directions are continuously available.
        d.rounded_rectangle((x-74,top+28,x+74,top+67),17,fill='white',outline=INK,width=4)
        txt(d,x-54,top-32,'Ống ↔',21)
    pad(d,247,top+202,'A',True);pad(d,588,top+202,'B',True)
    lever(d,992,top+202,'C',True)
    txt(d,876,top+276,'Chọn '+('I' if phase==1 else 'II'),23,True)
    cutter(d,148,top+303,'Dao 1');cutter(d,488,top+303,'Dao 2')
    exit_(d,1110,top+246,phase==2)
    txt(d,146,top+405,'A giữ nguồn',24,True);txt(d,492,top+405,'B giữ ly hợp',24,True);txt(d,863,top+405,'C vận hành tời',24,True)
    txt(d,80,top+461,('A có tải + B có tải + C kéo ở I → D1/D2 mở hết và chốt.' if phase==1 else 'Chỉ khi D1/D2 đã chốt, chặn II mới rút; A/B vẫn phải được giữ.'),24)
txt(d,80,1545,'Khi E đã chốt: rời A/B/C → tụ qua hai cửa sàn → thoát.',27,True)
txt(d,80,1615,'Mất tải giữa nhịp: tời dừng, phanh giữ tiến độ. Trở lại vị trí để tiếp tục.',23)
txt(d,80,1660,'Dao độc lập với A/B. Ống hai chiều giúp đi lại và sửa sai; không phải lỗ cuối.',23)
txt(d,80,1740,'Sơ đồ chức năng v1; chưa chứng minh lực, tầm giãn mô hoặc khả giải Unity.',23,color=BLUE)
im.save(root.parent/'Level39'/'mockup-v1.png')
print('Created precise new state diagrams for 38 and 39.')
