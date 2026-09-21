"""Draw a NEW deterministic plan diagram, not an edit of generated artwork."""
from pathlib import Path
from PIL import Image,ImageDraw,ImageFont
root=Path(__file__).resolve().parent
im=Image.new('RGB',(1200,1800),'white');d=ImageDraw.Draw(im)
font='/System/Library/Fonts/Supplemental/Arial.ttf'
bold='/System/Library/Fonts/Supplemental/Arial Bold.ttf'
def f(n,b=False):return ImageFont.truetype(bold if b else font,n)
ink='#173fa7'; pale='#a6bfdc'; muted='#546e90'
for y in range(40,1800,45):d.line((0,y,1200,y),fill='#e1effa',width=1)
d.line((48,0,48,1800),fill='#f4bac9',width=2)
d.text((78,48),'34 — Nhường đúng chỗ',font=f(52,True),fill=ink)
d.text((80,119),'Sơ đồ mặt bằng · Khóa xoay · Chạm để chỉ dẫn',font=f(26),fill=ink)
d.text((80,161),'Mượn chỗ cho X, rồi trả lại để cầu B nối tới cửa.',font=f(27),fill=ink)
states=[('Ban đầu',0,0,0),('1. Thu cầu B',1,0,0),('2. Cất X vào hốc',1,1,0),('3. Đẩy G qua',1,1,1),('4. Trả X về',1,0,1),('5. Trả cầu B',0,0,1)]
for idx,(title,ret,park,dock) in enumerate(states):
    ox=77+(idx%2)*561;oy=230+(idx//2)*437
    d.rounded_rectangle((ox,oy,ox+530,oy+410),12,fill='white',outline=pale,width=2)
    d.text((ox+20,oy+16),title,font=f(28,True),fill=ink)
    # Same positions and swept spaces in every panel.
    def line(coords,color=ink,width=3):d.line(tuple(v+(ox if i%2==0 else oy) for i,v in enumerate(coords)),fill=color,width=width)
    def rect(box,fill='white',outline=ink,width=3):d.rectangle(tuple(v+(ox if i%2==0 else oy) for i,v in enumerate(box)),fill=fill,outline=outline,width=width)
    def text(x,y,s,size=21,b=False):d.text((ox+x,oy+y),s,font=f(size,b),fill=ink)
    line((35,128,490,128),pale,7);line((230,100,230,270),pale,7)
    # Hốc X: B occupies it when extended. Retraction clears it.
    for a in range(206,260,12):line((a,223,a+6,223),pale,2);line((a,280,a+6,280),pale,2)
    for a in range(223,279,12):line((206,a,206,a+6),pale,2);line((260,a,260,a+6),pale,2)
    by=322 if ret else 238
    rect((202,by,465,by+34),'#edf4ff');text(354,by+3,'B',22,True)
    line((389,230,389,356),pale,3);line((398,230,398,356),pale,3)
    gx=436 if dock else 78
    rect((gx-27,103,gx+27,153),'#eef2ff');text(gx-9,109,'G',25,True)
    xy=250 if park else 128
    rect((205,xy-25,255,xy+25),'#e4ecff');text(222,xy-17,'X',25,True)
    # Spawn/body is on left safe floor, away from all rail sweeps.
    d.ellipse((ox+30,oy+210,ox+65,oy+235),fill=ink)
    d.ellipse((ox+41,oy+194,ox+60,oy+224),fill=ink)
    text(29,244,'S',20,True)
    text(292,189,'Hốc tạm X',19)
    line((330,214,265,245),pale,2)
    rect((470,224,507,283),'white')
    if dock:
        d.ellipse((ox+479,oy+244,ox+499,oy+265),outline=ink,width=3)
        rect((470,215,507,231),'#dce9ff')
    else:
        rect((470,224,507,283),'#dce9ff')
    text(477,288,'E',20,True)
    text(20,373,'E đã chốt mở' if dock else 'E đang đóng',21)
    text(330,72,'Ổ truyền',19)
    line((388,94,436,99),pale,2)
d.text((80,1570),'G: giá bánh răng  ·  X: vật chắn  ·  B: cầu  ·  E: cửa cuối',font=f(25,True),fill=ink)
d.text((80,1616),'1 → 2 → 3 → 4 → 5: thu cầu, cất X, đẩy G, trả X, trả cầu.',font=f(25),fill=ink)
d.text((80,1662),'Sai thứ tự: va chặn thật; kéo ngược để sửa. Cửa E giữ kết quả.',font=f(25),fill=ink)
d.text((80,1715),'Sơ đồ quan hệ vị trí; chưa phải bản kích thước hoặc gameplay Unity.',font=f(22),fill=muted)
out=root.parent/'Level34'/'mockup-v1.png';im.save(out)
print(out)
