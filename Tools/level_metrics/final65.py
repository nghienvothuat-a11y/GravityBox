"""The 65-level order as built (Mrk, 12/10/2026: "Chốt thứ tự 65 màn mới này"), with every level measured in game, G1–G5
included. The order is read from VenomCampaignBuilder.SpatialOrder; difficulty D is reorder65.py's (decisions in the
reference solution + 0.25 × the in-game index, capped at 20, or 12 for the five solutions that make deliberate mistakes).
Usage: python3 final65.py <levels.jsonl> <COgheSpatialCampaignBuilder.cs> <out.png>"""
import json, re, sys
import matplotlib
matplotlib.use("Agg")
import matplotlib.pyplot as plt
import numpy as np
from matplotlib import font_manager
from matplotlib.patches import Patch
from PIL import Image

# decisions in each reference solution (reorder65.py)
DEC = {"01":0,"02":1,"03":1,"04":1,"05":2,"06":2,"07":3,"08":3,"09":3,"10":6,
       "16":4,"12":4,"N13":5,"14":3,"N15":4,"15":5,"17":6,"N18":7,"N19":9,"20":12,
       "18":2,"N22":3,"N23":3,"21":2,"N25":4,"N26":5,"22":4,"13":3,"N29":4,"B1":7,
       "N31":5,"N32":7,"N33":8,"N34":8,"N35":7,"E10":3,"E11":6,"26":6,"29":8,"N40":9,
       "N41":3,"N42":5,"N43":7,"N44":4,"N45":8,"N46":4,"N47":5,"N48":4,"N49":7,"N50":12,
       "K01":2,"K02":3,"K03":4,"K04":5,"K05":6,"K06":6,"K07":7,"K08":8,"K09":9,"K10":10,
       "G1":2,"G2":3,"G3":3,"G4":5,"G5":7}
G_ESTIMATE = {"G1":2.5,"G2":4.0,"G3":5.0,"G4":5.5,"G5":10.5}   # the design estimates the proposal used
NOISY = {"N26","N33","N35","N45","N50"}
CONCEPTS = {'COgheTapRail','COgheQuantumSplitter','COgheTubeNetwork','COghePassengerLift','COgheSwingTransfer','COgheSeesawBridge',
            'COgheGearTrain','COgheLoadLatch','COgheTurntable','COghePulleyDrive','COghePropSocket'}

src = open(sys.argv[2], encoding="utf-8").read()
body = re.search(r"SpatialOrder=\{(.*?)\};", src, re.S).group(1)
order = re.findall(r'"([A-Z]?\d+)"', re.sub(r"//[^\n]*", "", body))
assert len(order) == 65 and set(order) == set(DEC), order
idx, title, solved = {}, {}, {}
for line in open(sys.argv[1], encoding="utf-8"):
    r = json.loads(line); ops = r['mechanismTaps'] + r['partSelections']; walks = max(0, r['taps'] - ops)
    idx[r['key']] = ops + .5 * walks + (r['maxParts'] - 1) + .5 * len([k for k in r['mechanisms'] if k in CONCEPTS])
    title[r['key']] = r['title'].split(' · ', 1)[1]; solved[r['key']] = r['solved']
missing = [k for k in order if k not in idx]; assert not missing, f"not measured: {missing}"
D = {k: DEC[k] + .25 * min(idx[k], 12 if k in NOISY else 20) for k in order}
bosses = [k for k in order if k in {"10","20","B1","N40","N50","G5"}]
slots = [order.index(k) + 1 for k in bosses]; starts = [1] + [b + 1 for b in slots[:-1]]

font = "/System/Library/Fonts/Supplemental/Arial Unicode.ttf"; font_manager.fontManager.addfont(font)
plt.rcParams["font.family"] = font_manager.FontProperties(fname=font).get_name()
ink, ink2, grid, surface, band = "#2b2b29", "#6b6a63", "#e6e5df", "#fcfcfb", "#f1f0ea"
blue, orange, green, red = "#2a78d6", "#eb6834", "#1f9e6b", "#c0392b"
def colour(k): return green if k.startswith('G') else orange if k.startswith('K') else blue
fig, (ax0, ax1) = plt.subplots(2, 1, figsize=(19, 10), dpi=110, facecolor=surface, gridspec_kw=dict(hspace=.42))
xs = list(range(1, 66))
for ax, vals, ylab in [(ax0, [D[k] for k in order], "Độ khó (quyết định + 0,25 × chỉ số đo)"), (ax1, [idx[k] for k in order], "Chỉ số đo trong game")]:
    top = max(vals) * 1.12; ax.set_facecolor(surface)
    for c, (a, b) in enumerate(zip(starts, slots)):
        if c % 2 == 0: ax.axvspan(a - .5, b + .5, color=band, zorder=0)
        ax.text((a + b) / 2, top * .985, f"Chương {c+1}", ha="center", va="top", fontsize=11, color=ink2)
    ax.bar(xs, vals, color=[colour(k) for k in order], width=.72, zorder=2)
    for x, k, v in zip(xs, order, vals):
        if k in bosses: ax.text(x, v + top * .012, "★", ha="center", va="bottom", fontsize=12, color=ink)
        if k.startswith('G'): ax.text(x, v + top * (.06 if k in bosses else .012), k, ha="center", va="bottom", fontsize=8.5, color=green, fontweight="bold")
    ax.set_ylim(0, top); ax.set_xlim(.3, 65.7); ax.set_ylabel(ylab, color=ink2, fontsize=10.5)
    ax.grid(axis="y", color=grid, lw=1, zorder=1); ax.set_axisbelow(True)
    for side in ("top", "right", "left"): ax.spines[side].set_visible(False)
    ax.spines["bottom"].set_color(grid); ax.tick_params(colors=ink2, labelsize=9); ax.tick_params(axis="x", length=0)
    ax.set_xticks(xs); ax.set_xticklabels([f"{x}\n{k}" for x, k in zip(xs, order)], fontsize=6.6, color=ink2)
nb = [(x, D[k]) for x, k in zip(xs, order) if k not in bosses]
fx, fy = np.array([a for a, _ in nb]), np.array([b for _, b in nb]); m, c0 = np.polyfit(fx, fy, 1)
ax0.plot(xs, [m * x + c0 for x in xs], color=red, lw=1.6, ls="--", zorder=3)
ax0.set_title("Thứ tự đã chốt: 65 màn, boss ở " + " · ".join(map(str, slots)) + " (đo trong game, G1–G5 đã dựng)", loc="left", fontsize=15, color=ink, pad=30)
ax0.legend(handles=[Patch(color=blue, label="màn thường"), Patch(color=orange, label="màn thùng"), Patch(color=green, label="màn bò kính"),
                    Patch(color=surface, label="★ boss"), plt.Line2D([], [], color=red, ls="--", label="xu hướng (màn thường)")],
           loc="upper left", bbox_to_anchor=(0, 1.16), ncol=5, frameon=False, fontsize=10, labelcolor=ink)
fig.text(.01, .004, "Độ khó = số quyết định trong lời giải + 0,25 × chỉ số đo (giới hạn 20; 12 cho N26, N33, N35, N45, N50 vì lời giải mẫu cố ý làm sai). "
         "Chỉ số đo = thao tác cơ quan + đổi phần + 0,5 × chạm để đi + (số phần tối đa − 1) + 0,5 × số loại cơ quan; chạy lời giải mẫu trong game.", fontsize=9, color=ink2)
fig.savefig(sys.argv[3] + ".raw.png", facecolor=surface, bbox_inches="tight")
im = Image.open(sys.argv[3] + ".raw.png").convert("RGB"); clean = Image.new("RGB", im.size); clean.putdata(list(im.getdata())); clean.save(sys.argv[3])
import os; os.remove(sys.argv[3] + ".raw.png")
for i, k in enumerate(order, 1):
    est = f"  (ước tính {G_ESTIMATE[k]})" if k in G_ESTIMATE else ""
    print(f"{i:2} {k:4} {'★' if k in bosses else ' '} D {D[k]:5.2f} idx {idx[k]:5.2f}{est} {'' if solved[k] else 'UNSOLVED'} {title.get(k,'')}")
res = fy - (m * fx + c0); jumps = [D[b] - D[a] for a, b in zip(order, order[1:]) if a not in bosses and b not in bosses]
print(f"SMOOTH rms {np.sqrt((res**2).mean()):.2f} maxjump {max(jumps):.2f} bosses {[(k, round(D[k], 2)) for k in bosses]}")
