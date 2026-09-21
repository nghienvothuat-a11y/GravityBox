"""Precise schematic supplement; distinct from the generated concept illustration."""
from pathlib import Path
from xml.sax.saxutils import escape
root=Path(__file__).resolve().parent
states=[('Ban đầu',False,False,False),('1 · Thu B',True,False,False),('2 · Cất X',True,True,False),('3 · Đẩy G',True,True,True),('4 · Trả X',True,False,True),('5 · Trả B',False,False,True)]
out=['<svg xmlns="http://www.w3.org/2000/svg" width="1500" height="1220" viewBox="0 0 1500 1220"><rect width="1500" height="1220" fill="#fff"/><style>text{font-family:Arial,sans-serif;fill:#173e95}.title{font-size:38px;font-weight:bold}.label{font-size:24px;font-weight:bold}.note{font-size:20px}.rail{stroke:#a9bdd5;stroke-width:7;fill:none}.border{stroke:#becddd;fill:none;stroke-width:2}</style><text x="40" y="52" class="title">34 · Sơ đồ mặt bằng: mượn chỗ rồi trả lại</text><text x="40" y="89" class="note">Hình kiểm tra vị trí cơ quan, không theo tỷ lệ. X phải rời hốc tạm trước khi B trở lại.</text>']
for idx,(title,retracted,parked,docked) in enumerate(states):
    ox=35+(idx%2)*745;oy=120+(idx//2)*350
    out.append(f'<g transform="translate({ox},{oy})"><rect x="0" y="0" width="710" height="326" rx="12" fill="#f7faff" stroke="#d1dce9"/><text x="20" y="34" class="label">{escape(title)}</text>')
    out.append('<path d="M60 116H650 M310 116V212" class="rail"/><rect x="277" y="180" width="65" height="64" fill="none" stroke="#6886ab" stroke-dasharray="5 5"/><text x="350" y="240" class="note">Hốc X / vị trí B</text>')
    by=270 if retracted else 205
    out.append(f'<path d="M490 195V289 M507 195V289" class="rail"/><rect x="275" y="{by}" width="375" height="29" rx="4" fill="#cfe4fe" stroke="#195cba" stroke-width="3"/><text x="535" y="{by+23}" class="label">B · cầu</text>')
    gy=116;gx=610 if docked else 100;xy=211 if parked else 116
    out.append(f'<rect x="{gx-36}" y="{gy-29}" width="72" height="58" rx="6" fill="#fff1d7" stroke="#9e6a16" stroke-width="3"/><text x="{gx-9}" y="{gy+8}" class="label">G</text><rect x="282" y="{xy-27}" width="56" height="54" fill="#e7eaff" stroke="#4a4aaf" stroke-width="3"/><text x="300" y="{xy+8}" class="label">X</text>')
    out.append(f'<text x="580" y="75" class="note">Ổ truyền</text><text x="20" y="303" class="note">E: {"đã chốt mở" if docked else "đang đóng"}</text></g>')
out.append('<text x="40" y="1200" class="note">Sơ đồ bổ sung xác định quan hệ va chạm B–hốc X và X–đường G; phối cảnh chỉ minh họa ý tưởng.</text></svg>')
(root/'layout34-states.svg').write_text(''.join(out))
print(root/'layout34-states.svg')
