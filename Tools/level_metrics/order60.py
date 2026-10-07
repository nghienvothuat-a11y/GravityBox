import json, sys
import matplotlib
matplotlib.use("Agg")
import matplotlib.pyplot as plt
from matplotlib import font_manager
from matplotlib.patches import Patch
from PIL import Image

ORDER = [["01","02","03","04","05","K01","06","07","08","K02","09","10"],
         ["16","12","N13","14","N15","K03","15","K04","17","N18","N19","20"],
         ["18","N22","N23","21","N25","N26","K05","22","13","K06","N29","B1"],
         ["N31","N32","N33","N34","N35","K07","E10","E11","26","K08","29","N40"],
         ["N41","N42","N43","N44","N45","K09","N46","N47","N48","N49","K10","N50"]]
BOSS = {"10","20","B1","N40","N50"}
# Decisions in the reference solution: chapter 1–2 per plan 5.1/5.6; chapters 3–5 counted on the rebuilt levels; crate levels =
# pulls in the shortest solution. E11, 26, 29 are not rebuilt yet (estimated from their current solutions).
DEC = {"01":0,"02":1,"03":1,"04":1,"05":2,"06":2,"07":2,"08":3,"09":3,"10":6,
       "16":4,"12":4,"N13":5,"14":3,"N15":4,"15":5,"17":6,"N18":7,"N19":9,"20":12,
       "18":2,"N22":3,"N23":3,"21":2,"N25":4,"N26":5,"22":4,"13":3,"N29":4,"B1":7,
       "N31":5,"N32":7,"N33":8,"N34":8,"N35":7,"E10":3,"E11":6,"26":6,"29":8,"N40":9,
       "N41":3,"N42":5,"N43":7,"N44":4,"N45":8,"N46":4,"N47":5,"N48":4,"N49":7,"N50":12,
       "K01":2,"K02":3,"K03":4,"K04":5,"K05":6,"K06":6,"K07":7,"K08":8,"K09":9,"K10":10}
CONCEPTS = {'COgheTapRail','COgheQuantumSplitter','COgheTubeNetwork','COghePassengerLift','COgheSwingTransfer','COgheSeesawBridge',
            'COgheGearTrain','COgheLoadLatch','COgheTurntable','COghePulleyDrive','COghePropSocket'}
idx = {}
for line in open(sys.argv[1]):
    r = json.loads(line); ops = r['mechanismTaps'] + r['partSelections']; walks = max(0, r['taps'] - ops)
    idx[r['key']] = ops + .5 * walks + (r['maxParts'] - 1) + .5 * len([k for k in r['mechanisms'] if k in CONCEPTS])

seq = [k for ch in ORDER for k in ch]
assert len(seq) == 60 and set(seq) == set(DEC), set(DEC) ^ set(seq)
font = "/System/Library/Fonts/Supplemental/Arial Unicode.ttf"; font_manager.fontManager.addfont(font)
plt.rcParams["font.family"] = font_manager.FontProperties(fname=font).get_name()
ink, ink2, grid, surface, band = "#2b2b29", "#6b6a63", "#e6e5df", "#fcfcfb", "#f1f0ea"
blue, orange = "#2a78d6", "#eb6834"
xs = list(range(1, 61)); cols = [orange if k.startswith('K') else blue for k in seq]
fig, axes = plt.subplots(2, 1, figsize=(18, 9.5), dpi=120, facecolor=surface, sharex=True)
for ax, vals, ylab, top in [(axes[0], [DEC[k] for k in seq], "Số quyết định trong lời giải", 14), (axes[1], [idx[k] for k in seq], "Chỉ số đo trong game", 46)]:
    ax.set_facecolor(surface)
    for c in range(5):
        if c % 2 == 0: ax.axvspan(c * 12 + .5, c * 12 + 12.5, color=band, zorder=0)
        ax.text(c * 12 + 6.5, top * .97, f"Chương {c+1}", ha="center", va="top", fontsize=11, color=ink2)
    ax.bar(xs, vals, color=cols, width=.72, zorder=2)
    for x, k, v in zip(xs, seq, vals):
        if k in BOSS: ax.text(x, v + top * .015, "★", ha="center", va="bottom", fontsize=11, color=ink)
    ax.set_ylim(0, top); ax.set_ylabel(ylab, color=ink2, fontsize=10.5)
    ax.grid(axis="y", color=grid, lw=1, zorder=1); ax.set_axisbelow(True)
    for side in ("top", "right", "left"): ax.spines[side].set_visible(False)
    ax.spines["bottom"].set_color(grid); ax.tick_params(colors=ink2, labelsize=9); ax.tick_params(axis="x", length=0)
axes[1].set_xticks(xs); axes[1].set_xticklabels([f"{x}\n{k}" for x, k in zip(xs, seq)], fontsize=7.2, color=ink2)
axes[0].set_title("60 màn theo thứ tự mới: 5 chương × 12 màn, mỗi chương 2 màn thùng", loc="left", fontsize=14, color=ink, pad=30)
axes[0].legend(handles=[Patch(color=blue, label="màn thường"), Patch(color=orange, label="màn thùng (K01–K10)"), Patch(color=surface, label="★ màn trùm")],
               loc="upper left", bbox_to_anchor=(0, 1.13), ncol=3, frameon=False, fontsize=10, labelcolor=ink)
fig.text(.01, .008, "Chỉ số đo = thao tác cơ quan + đổi phần + 0,5 × chạm để đi + (số phần tối đa − 1) + 0,5 × số loại cơ quan, chạy lời giải mẫu trong game (06/10/2026). "
         "Lời giải mẫu của N26, N33, N35, N45, N50 cố ý làm sai trước để kiểm tra, nên chỉ số các màn đó cao hơn cảm nhận.", fontsize=9, color=ink2)
fig.tight_layout(rect=(0, .025, 1, 1))
fig.savefig(sys.argv[2] + ".raw.png", facecolor=surface)
im = Image.open(sys.argv[2] + ".raw.png").convert("RGB"); clean = Image.new("RGB", im.size); clean.putdata(list(im.getdata())); clean.save(sys.argv[2])
