"""Difficulty with the five glass-crawl levels interleaved (Mrk, 12/10/2026: "nếu vậy độ khó mới sẽ như nào? vẽ biểu đồ").
The 60 built levels use the in-game index measured by MeasureLevelMetrics (Artifacts/Metrics/levels.jsonl) and the decision
counts of order60.py. G1–G5 are not built: their decisions and index are estimated from their designed reference solutions
(Docs/LevelDesign/COghe/GlassCrawl5) with the same formula, and drawn hatched.
Usage: python3 order65.py <levels.jsonl> <out.png>"""
import json, sys
import matplotlib
matplotlib.use("Agg")
import matplotlib.pyplot as plt
from matplotlib import font_manager
from matplotlib.patches import Patch
from PIL import Image

OLD = [["01","02","03","04","05","K01","06","07","08","K02","09","10"],
       ["16","12","N13","14","N15","K03","15","K04","17","N18","N19","20"],
       ["18","N22","N23","21","N25","N26","K05","22","13","K06","N29","B1"],
       ["N31","N32","N33","N34","N35","K07","E10","E11","26","K08","29","N40"],
       ["N41","N42","N43","N44","N45","K09","N46","N47","N48","N49","K10","N50"]]
NEW = [["01","02","03","04","05","K01","06","G1","07","08","K02","G2","09","10"],
       ["16","12","G3","N13","14","N15","K03","G4","15","K04","17","G5","N18","N19","20"]] + OLD[2:]
BOSS = {"10","20","B1","N40","N50"}
DEC = {"01":0,"02":1,"03":1,"04":1,"05":2,"06":2,"07":2,"08":3,"09":3,"10":6,
       "16":4,"12":4,"N13":5,"14":3,"N15":4,"15":5,"17":6,"N18":7,"N19":9,"20":12,
       "18":2,"N22":3,"N23":3,"21":2,"N25":4,"N26":5,"22":4,"13":3,"N29":4,"B1":7,
       "N31":5,"N32":7,"N33":8,"N34":8,"N35":7,"E10":3,"E11":6,"26":6,"29":8,"N40":9,
       "N41":3,"N42":5,"N43":7,"N44":4,"N45":8,"N46":4,"N47":5,"N48":4,"N49":7,"N50":12,
       "K01":2,"K02":3,"K03":4,"K04":5,"K05":6,"K06":6,"K07":7,"K08":8,"K09":9,"K10":10}
# G1–G5, from the designed solutions: decisions, and (mechanism taps + part selections, walk taps, max parts, device kinds)
G = {"G1": (2, 1, 2, 1, 1),   # climb to the shelf; pull A; exit
     "G2": (3, 2, 3, 1, 1),   # A; climb round to the shelf from above; B (chapter 1: a handle, not a pad); exit
     "G3": (3, 2, 4, 1, 2),   # B; lift; up the wall, over the roof; down; exit
     "G4": (5, 2, 5, 1, 2),   # A; climb to the left shelf, stand on P (latch); over the roof to the right shelf; B; exit
     "G5": (7, 5, 6, 2, 3)}   # Q; select halves (3); one half to P, the other up the lane, over the roof, B; back; merge; exit
CONCEPTS = {'COgheTapRail','COgheQuantumSplitter','COgheTubeNetwork','COghePassengerLift','COgheSwingTransfer','COgheSeesawBridge',
            'COgheGearTrain','COgheLoadLatch','COgheTurntable','COghePulleyDrive','COghePropSocket'}
idx = {}
for line in open(sys.argv[1]):
    r = json.loads(line); ops = r['mechanismTaps'] + r['partSelections']; walks = max(0, r['taps'] - ops)
    idx[r['key']] = ops + .5 * walks + (r['maxParts'] - 1) + .5 * len([k for k in r['mechanisms'] if k in CONCEPTS])
for k, (d, ops, walks, parts, kinds) in G.items():
    DEC[k] = d; idx[k] = ops + .5 * walks + (parts - 1) + .5 * kinds

old = [k for ch in OLD for k in ch]; new = [k for ch in NEW for k in ch]
assert len(new) == 65 and set(new) == set(old) | set(G)
font = "/System/Library/Fonts/Supplemental/Arial Unicode.ttf"; font_manager.fontManager.addfont(font)
plt.rcParams["font.family"] = font_manager.FontProperties(fname=font).get_name()
ink, ink2, grid, surface, band = "#2b2b29", "#6b6a63", "#e6e5df", "#fcfcfb", "#f1f0ea"
blue, orange, green = "#2a78d6", "#eb6834", "#1f9e6b"
def colour(k): return green if k in G else orange if k.startswith('K') else blue

fig = plt.figure(figsize=(19, 14), dpi=110, facecolor=surface)
gs = fig.add_gridspec(3, 1, height_ratios=[1, 1, 1.05], hspace=.42)
ax1, ax2, ax3 = fig.add_subplot(gs[0]), fig.add_subplot(gs[1]), fig.add_subplot(gs[2])
xs = list(range(1, 66)); bounds = [0]
for ch in NEW: bounds.append(bounds[-1] + len(ch))
for ax, vals, ylab, top in [(ax1, [DEC[k] for k in new], "Số quyết định trong lời giải", 14), (ax2, [idx[k] for k in new], "Chỉ số đo trong game", max(46, max(idx.values()) + 4))]:
    ax.set_facecolor(surface)
    for c in range(5):
        if c % 2 == 0: ax.axvspan(bounds[c] + .5, bounds[c + 1] + .5, color=band, zorder=0)
        ax.text((bounds[c] + bounds[c + 1]) / 2 + .5, top * .97, f"Chương {c+1} ({len(NEW[c])} màn)", ha="center", va="top", fontsize=11, color=ink2)
    bars = ax.bar(xs, vals, color=[colour(k) for k in new], width=.72, zorder=2)
    for b, k in zip(bars, new):
        if k in G: b.set_hatch("///"); b.set_edgecolor("white"); b.set_linewidth(0)
    for x, k, v in zip(xs, new, vals):
        if k in BOSS: ax.text(x, v + top * .015, "★", ha="center", va="bottom", fontsize=11, color=ink)
        if k in G: ax.text(x, v + top * .015, k, ha="center", va="bottom", fontsize=9, color=green, fontweight="bold")
    ax.set_ylim(0, top); ax.set_ylabel(ylab, color=ink2, fontsize=10.5); ax.set_xlim(.3, 65.7)
    ax.grid(axis="y", color=grid, lw=1, zorder=1); ax.set_axisbelow(True)
    for side in ("top", "right", "left"): ax.spines[side].set_visible(False)
    ax.spines["bottom"].set_color(grid); ax.tick_params(colors=ink2, labelsize=9); ax.tick_params(axis="x", length=0)
    ax.set_xticks(xs); ax.set_xticklabels([f"{x}\n{k}" for x, k in zip(xs, new)], fontsize=6.6, color=ink2)
ax1.set_title("Thứ tự mới: 65 màn, 5 màn bò kính (G1–G5) chen vào chương 1–2", loc="left", fontsize=15, color=ink, pad=30)
ax1.legend(handles=[Patch(color=blue, label="màn thường"), Patch(color=orange, label="màn thùng"), Patch(facecolor=green, hatch="///", edgecolor="white", label="màn bò kính mới (ước tính, chưa dựng)"),
                    Patch(color=surface, label="★ màn trùm")], loc="upper left", bbox_to_anchor=(0, 1.16), ncol=4, frameon=False, fontsize=10, labelcolor=ink)

# chapters 1–2, before and after: the in-game index by position within the chapter's run
ax3.set_facecolor(surface)
o12 = OLD[0] + OLD[1]; n12 = NEW[0] + NEW[1]
ax3.plot(range(1, len(o12) + 1), [idx[k] for k in o12], color="#9a998f", lw=2, marker="o", ms=5, label="trước: 24 màn, theo vị trí cũ", zorder=2)
ax3.plot(range(1, len(n12) + 1), [idx[k] for k in n12], color=blue, lw=2.4, marker="o", ms=5, label="sau: 29 màn (tên màn dưới trục là thứ tự mới)", zorder=3)
for i, k in enumerate(n12, 1):
    if k in G: ax3.plot(i, idx[k], marker="o", ms=11, color=green, zorder=4); ax3.annotate(k, (i, idx[k]), textcoords="offset points", xytext=(0, 10), ha="center", fontsize=10, color=green, fontweight="bold")
    if k in BOSS: ax3.annotate("★", (i, idx[k]), textcoords="offset points", xytext=(0, 9), ha="center", fontsize=12, color=ink)
ax3.axvline(len(NEW[0]) + .5, color=grid, lw=1.5); ax3.text(len(NEW[0]) / 2 + .5, ax3.get_ylim()[1] if False else max(idx[k] for k in n12) + 4, "Chương 1", ha="center", color=ink2, fontsize=11)
ax3.text(len(NEW[0]) + len(NEW[1]) / 2 + .5, max(idx[k] for k in n12) + 4, "Chương 2", ha="center", color=ink2, fontsize=11)
ax3.set_xticks(range(1, len(n12) + 1)); ax3.set_xticklabels([f"{i}\n{k}" for i, k in enumerate(n12, 1)], fontsize=7.5, color=ink2)
ax3.set_ylim(0, max(idx[k] for k in n12) + 7); ax3.set_ylabel("Chỉ số đo trong game", color=ink2, fontsize=10.5)
ax3.grid(axis="y", color=grid, lw=1); ax3.set_axisbelow(True)
for side in ("top", "right"): ax3.spines[side].set_visible(False)
ax3.tick_params(colors=ink2, labelsize=9); ax3.tick_params(axis="x", length=0)
ax3.set_title("Phóng to chương 1–2: trước và sau khi chen 5 màn", loc="left", fontsize=13, color=ink, pad=12)
ax3.legend(loc="upper left", frameon=False, fontsize=10, labelcolor=ink)
fig.text(.01, .006, "Chỉ số đo = thao tác cơ quan + đổi phần + 0,5 × chạm để đi + (số phần tối đa − 1) + 0,5 × số loại cơ quan, chạy lời giải mẫu trong game (12/10/2026). "
         "G1–G5 chưa dựng: ước tính từ lời giải thiết kế bằng cùng công thức; sẽ đo lại sau khi dựng.", fontsize=9, color=ink2)
fig.savefig(sys.argv[2] + ".raw.png", facecolor=surface, bbox_inches="tight")
im = Image.open(sys.argv[2] + ".raw.png").convert("RGB"); clean = Image.new("RGB", im.size); clean.putdata(list(im.getdata())); clean.save(sys.argv[2])
for k in G: print(k, "decisions", DEC[k], "index", idx[k])
