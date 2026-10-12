"""65 levels with bosses from level 10 and difficulty rising evenly (Mrk, 12/10/2026: "sắp xếp lại các màn Boss cho xuất hiện
sớm hơn, từ level 10. Và tạo ra độ khó tăng dần trải đều từ level 1 - 65").

Difficulty D = decisions in the reference solution + 0.25 × the in-game index (capped at 20: five reference solutions make
deliberate mistakes, which inflates their index). Six bosses (the five chapter bosses and G5, designed as a boss) sit at
10, 21, 32, 43, 54, 65 in rising D, so each closes a chapter of 10 or 11. Every other slot takes the easiest remaining level
whose prerequisites (the levels that teach what it uses) are already placed; a level that teaches a new device waits at
least four slots after the previous one. A boss's prerequisites are due early enough to leave practice levels before it.
Usage: python3 reorder65.py <levels.jsonl> <out.png> [<order.json>]"""
import json, sys
import matplotlib
matplotlib.use("Agg")
import matplotlib.pyplot as plt
from matplotlib import font_manager
from matplotlib.patches import Patch
from PIL import Image

OLD = ["01","02","03","04","05","K01","06","07","08","K02","09","10",
       "16","12","N13","14","N15","K03","15","K04","17","N18","N19","20",
       "18","N22","N23","21","N25","N26","K05","22","13","K06","N29","B1",
       "N31","N32","N33","N34","N35","K07","E10","E11","26","K08","29","N40",
       "N41","N42","N43","N44","N45","K09","N46","N47","N48","N49","K10","N50"]
DEC = {"01":0,"02":1,"03":1,"04":1,"05":2,"06":2,"07":3,"08":3,"09":3,"10":6,
       "16":4,"12":4,"N13":5,"14":3,"N15":4,"15":5,"17":6,"N18":7,"N19":9,"20":12,
       "18":2,"N22":3,"N23":3,"21":2,"N25":4,"N26":5,"22":4,"13":3,"N29":4,"B1":7,
       "N31":5,"N32":7,"N33":8,"N34":8,"N35":7,"E10":3,"E11":6,"26":6,"29":8,"N40":9,
       "N41":3,"N42":5,"N43":7,"N44":4,"N45":8,"N46":4,"N47":5,"N48":4,"N49":7,"N50":12,
       "K01":2,"K02":3,"K03":4,"K04":5,"K05":6,"K06":6,"K07":7,"K08":8,"K09":9,"K10":10,
       "G1":2,"G2":3,"G3":3,"G4":5,"G5":7}
G_INDEX = {"G1":2.5,"G2":4.0,"G3":5.0,"G4":5.5,"G5":10.5}   # estimated from the designs (order65.py)
TITLE = {"G1":"Leo kính lấy chìa","G2":"Kệ cao","G3":"Qua trần nhà","G4":"Ba kệ","G5":"Hai nửa trên trần"}
BOSSES = ["10","20","B1","N40","N50","G5"]
BOSS_SLOTS = [10,21,32,43,54,65]
# what each level needs taught first (its direct prerequisites)
NEEDS = {"02":["01"],"03":["01"],"04":["01"],"05":["04"],"K01":["04"],"06":["04"],"07":["04"],"08":["04"],"K02":["K01"],
         "09":["08"],"10":["08","05","07"],"G1":["06","02"],"G2":["G1"],"G3":["G1","08"],"G4":["G3","16"],"G5":["G3","17"],
         "16":["04"],"12":["04"],"N13":["16"],"14":["04"],"N15":["14"],"K03":["K02"],"15":["14"],"K04":["K03"],"17":["16","14"],
         "N18":["17"],"N19":["N18"],"20":["17","06"],"18":["01"],"N22":["04"],"N23":["18","N22"],"21":["01"],"N25":["21"],
         "N26":["N25","16"],"K05":["K04"],"22":["04"],"13":["08"],"K06":["K05"],"N29":["N22"],"B1":["16"],"N31":["17"],
         "N32":["N18"],"N33":["N31"],"N34":["N31","21"],"N35":["N33"],"K07":["K06"],"E10":["18","14"],"E11":["17","08"],
         "26":["17","18"],"K08":["K07"],"29":["N31","21"],"N40":["N35"],"N41":["04"],"N42":["N41"],"N43":["N42","14"],
         "N44":["N41"],"N45":["N41","N31"],"K09":["K08"],"N46":["N41"],"N47":["N44"],"N48":["N47"],"N49":["N45"],"K10":["K09"],
         "N50":["N49","N44"]}
TEACH = {"04","K01","08","G1","16","14","17","N18","18","21","G3","N31","N41"}   # each brings a new device or skill

CONCEPTS = {'COgheTapRail','COgheQuantumSplitter','COgheTubeNetwork','COghePassengerLift','COgheSwingTransfer','COgheSeesawBridge',
            'COgheGearTrain','COgheLoadLatch','COgheTurntable','COghePulleyDrive','COghePropSocket'}
idx, title = dict(G_INDEX), dict(TITLE)
for line in open(sys.argv[1]):
    r = json.loads(line); ops = r['mechanismTaps'] + r['partSelections']; walks = max(0, r['taps'] - ops)
    idx[r['key']] = ops + .5 * walks + (r['maxParts'] - 1) + .5 * len([k for k in r['mechanisms'] if k in CONCEPTS])
    title[r['key']] = r['title'].split(' · ', 1)[1]
# the reference solutions of these five make deliberate mistakes first (to test recovery): their index counts them
NOISY = {"N26","N33","N35","N45","N50"}
D = {k: DEC[k] + .25 * min(idx[k], 12 if k in NOISY else 20) for k in DEC}
# Boss order, chosen by the smoothness of the rest (see the SMOOTH/ANNEAL lines): B1 (needs only Q) before G5, which needs
# Q, holding, tubes and the roof (as the second boss G5 crammed four lessons into the five levels before it); "20" before
# N40, whose quarter-gauge chain otherwise piles up in chapter 4; the gearbox (N50) stays the finale it was designed as.
# COGHE_BOSSES overrides, e.g. "10,B1,G5,N40,20,N50".
import os
bosses = os.environ.get("COGHE_BOSSES", "10,B1,G5,20,N40,N50").split(",")
order = [None] * 65
for slot, k in zip(BOSS_SLOTS, bosses): order[slot - 1] = k
# chapter 1 keeps its approved opening (onboarding, a handle, the boxed handle, the first crate, high handles, the pin,
# the lift): it already rises steadily to the first boss
PINNED = ["01","02","03","04","05","K01","06","07","08"]
for i, k in enumerate(PINNED): order[i] = k
left = sorted([k for k in DEC if k not in BOSSES and k not in PINNED], key=lambda k: (D[k], k))
placed, last_teach = set(), -99
CH_START = [1] + [b + 1 for b in BOSS_SLOTS[:-1]]
def chapter(p): return max(i for i, a in enumerate(CH_START) if a - 1 <= p)
lessons = [0] * len(BOSS_SLOTS)
reserved = 0
def lesson_ok(p, gap=3, needed=False):
    # at most three new devices a chapter (counting those the coming boss still needs), in its first 70%, three slots
    # apart: the rest of the chapter practises them
    c = chapter(p); a, b = CH_START[c] - 1, BOSS_SLOTS[c] - 1
    return lessons[c] + (0 if needed else reserved) < 3 and (p - a) <= .7 * (b - a) and p - last_teach >= gap
for p in range(65):
    if order[p] is not None:
        k = order[p]; missing = [n for n in NEEDS.get(k, []) if n not in placed]
        assert not missing, f"{k} at {p+1} needs {missing}"
        placed.add(k)
        if k in TEACH: last_teach = p; lessons[chapter(p)] += 1
        continue
    # the next boss's prerequisites must be in before its slot, each with room to practise: a prerequisite d steps
    # before the boss (in its chain) is due by boss − 1 − 3·d
    nb = next(b for b in BOSS_SLOTS if b - 1 > p); boss = order[nb - 1]
    def ready(k): return all(n in placed for n in NEEDS.get(k, [])) and (k not in TEACH or lesson_ok(p, needed=k in chain))
    def depth(k, memo={}):
        # how many links from k up to the boss, through levels not yet placed
        users = [u for u in DEC if k in NEEDS.get(u, []) and (u == boss or (u not in placed and u in chain))]
        return 1 + max((0 if u == boss else depth(u)) for u in users) if users else 1
    chain = set()
    def closure(k):
        for n in NEEDS.get(k, []):
            if n not in placed and n not in chain: chain.add(n); closure(n)
    closure(boss)
    reserved = sum(1 for k in chain if k in TEACH)
    # due slots counted back from the boss, two apart: the nearest prerequisites (and the harder of two) last
    due = {k: nb - 1 - 2 * i for i, k in enumerate(sorted(chain, key=lambda k: (depth(k), -D[k])))}
    late = sorted([k for k in chain if all(n in placed for n in NEEDS.get(k, [])) and p + 1 >= due[k]], key=lambda k: due[k])
    pool = [k for k in left if ready(k)]
    if not pool:   # stuck: relax the lesson rules, but never teach two devices back to back
        pool = [k for k in left if all(n in placed for n in NEEDS.get(k, [])) and (k not in TEACH or p - last_teach >= 2)] or \
               [k for k in left if all(n in placed for n in NEEDS.get(k, []))]
    if late: pool = late[:1]
    # variety: no two crate levels (K) or two glass levels (G) in a row while something close in difficulty can go instead
    fam = lambda k: k[0] if k[0] in "KG" else None
    prev = order[p - 1] if p else None
    if prev and fam(prev) and not late:
        others = [k for k in pool if fam(k) != fam(prev)]
        if not others:   # only that family is ready: anything else whose prerequisites are in, spacing aside
            others = [k for k in left if fam(k) != fam(prev) and all(n in placed for n in NEEDS.get(k, []))
                      and (k not in TEACH or lesson_ok(p, 2))]   # a lesson two slots on beats a third crate in a row
        if others: pool = others
    best = min(pool, key=lambda k: (D[k], k))
    # a lesson the coming boss needs goes first when its slot is open and it is not much harder than the alternative
    need = [k for k in pool if k in TEACH and k in chain]
    if need and not late:
        n = min(need, key=lambda k: (D[k], k))
        if best in TEACH or D[n] <= D[best] + 2.5: best = n   # always before a lesson the boss does not need
    k = best; order[p] = k; left.remove(k); placed.add(k)
    if k in TEACH: last_teach = p; lessons[chapter(p)] += 1
assert not left and len(set(order)) == 65

# ---- refinement: simulated annealing over the free slots (seeded: the same input gives the same order) ------------------
FAMILY = {}
for k in DEC:
    FAMILY[k] = ("crate" if k.startswith("K") else "glass" if k.startswith("G") else
                 "gear" if k in {"N41","N42","N43","N44","N45","N46","N47","N48","N49","N50"} else
                 "swing" if k in {"18","N23","E10","26"} else "seesaw" if k in {"21","N25","N26","N34","29"} else
                 "Q" if k in {"16","N13","17","N18","N19","20","B1","N31","N32","N33","N34","N35","N40","E11","G5"} else
                 "tube" if k in {"14","N15","15"} else "other")
BOSS_KEY = {"B1": "Q", "G5": "glass", "20": "Q", "N40": "Q", "N50": "gear", "10": "other"}   # what each boss tests
free = [i for i in range(65) if order[i] not in BOSSES and order[i] not in PINNED]
ideal = sorted(D[order[i]] for i in free)                  # a perfectly even ramp over the free slots
pos_of = {}
def cost(o):
    for i, k in enumerate(o): pos_of[k] = i
    c = 0.0
    for r, i in enumerate(free): c += (D[o[i]] - ideal[r]) ** 2
    for i, k in enumerate(o):                               # prerequisites first
        for n in NEEDS.get(k, []):
            if pos_of[n] > i: c += 100
    t = [i for i, k in enumerate(o) if k in TEACH]
    for a, b in zip(t, t[1:]):
        if b - a < 3: c += 8 * (3 - (b - a))
    for ci, (a, b) in enumerate(zip(CH_START, BOSS_SLOTS)):
        n = sum(1 for i in t if a - 1 <= i <= b - 1)
        if n > 3: c += 12 * (n - 3)
        for i in t:
            if a - 1 <= i <= b - 1 and (i - (a - 1)) > .75 * (b - a): c += 2   # a lesson in the chapter's last quarter
        # practice: a level of the boss's kind in the chapter, after its lesson, before the boss
        fam = BOSS_KEY.get(o[b - 1])
        if fam and fam != "other" and not any(FAMILY[o[i]] == fam and o[i] not in TEACH for i in range(a - 1, b - 1)): c += 15
    for i in range(1, 65):
        f1, f0 = FAMILY[o[i]], FAMILY[o[i - 1]]
        if f1 == f0 and f1 in ("crate", "glass"): c += 1.5
        if i > 1 and f1 == f0 == FAMILY[o[i - 2]] and f1 != "other": c += 3
    return c
import random
rng = random.Random(12)
best_o, cur = list(order), list(order)
cur_c = best_c = cost(cur)
T0, steps = 4.0, 400000
for step in range(steps):
    T = T0 * (1 - step / steps) + 1e-3
    i, j = rng.sample(free, 2)
    cur[i], cur[j] = cur[j], cur[i]
    c2 = cost(cur)
    if c2 <= cur_c or rng.random() < pow(2.718, (cur_c - c2) / T):
        cur_c = c2
        if c2 < best_c: best_c, best_o = c2, list(cur)
    else:
        cur[i], cur[j] = cur[j], cur[i]
greedy_cost = cost(order)
order = best_o
print(f"ANNEAL cost {greedy_cost:.1f} -> {best_c:.1f}")
for i, k in enumerate(order):
    bad = [n for n in NEEDS.get(k, []) if order.index(n) > i]
    assert not bad, f"{k} at {i+1} needs {bad}"
if len(sys.argv) > 3: json.dump({"order": order, "bosses": BOSS_SLOTS}, open(sys.argv[3], "w"), ensure_ascii=False, indent=1)

# ---- chart ------------------------------------------------------------------------------------------------------------
font = "/System/Library/Fonts/Supplemental/Arial Unicode.ttf"; font_manager.fontManager.addfont(font)
plt.rcParams["font.family"] = font_manager.FontProperties(fname=font).get_name()
ink, ink2, grid, surface, band = "#2b2b29", "#6b6a63", "#e6e5df", "#fcfcfb", "#f1f0ea"
blue, orange, green, red = "#2a78d6", "#eb6834", "#1f9e6b", "#c0392b"
def colour(k): return green if k.startswith('G') else orange if k.startswith('K') else blue
fig, (ax0, ax1, ax2) = plt.subplots(3, 1, figsize=(19, 14), dpi=110, facecolor=surface, gridspec_kw=dict(height_ratios=[1, 1, .9], hspace=.45))
xs = list(range(1, 66)); starts = [1] + [b + 1 for b in BOSS_SLOTS[:-1]]
for ax, vals, ylab in [(ax0, [D[k] for k in order], "Độ khó (quyết định + 0,25 × chỉ số đo)"), (ax1, [idx[k] for k in order], "Chỉ số đo trong game")]:
    top = max(vals) * 1.12; ax.set_facecolor(surface)
    for c, (a, b) in enumerate(zip(starts, BOSS_SLOTS)):
        if c % 2 == 0: ax.axvspan(a - .5, b + .5, color=band, zorder=0)
        ax.text((a + b) / 2, top * .985, f"Chương {c+1}", ha="center", va="top", fontsize=11, color=ink2)
    bars = ax.bar(xs, vals, color=[colour(k) for k in order], width=.72, zorder=2)
    for b, k in zip(bars, order):
        if k.startswith('G'): b.set_hatch("///"); b.set_edgecolor("white"); b.set_linewidth(0)
    for x, k, v in zip(xs, order, vals):
        if k in BOSSES: ax.text(x, v + top * .012, "★", ha="center", va="bottom", fontsize=12, color=ink)
    ax.set_ylim(0, top); ax.set_xlim(.3, 65.7); ax.set_ylabel(ylab, color=ink2, fontsize=10.5)
    ax.grid(axis="y", color=grid, lw=1, zorder=1); ax.set_axisbelow(True)
    for side in ("top", "right", "left"): ax.spines[side].set_visible(False)
    ax.spines["bottom"].set_color(grid); ax.tick_params(colors=ink2, labelsize=9); ax.tick_params(axis="x", length=0)
    ax.set_xticks(xs); ax.set_xticklabels([f"{x}\n{k}" for x, k in zip(xs, order)], fontsize=6.6, color=ink2)
# the trend: non-boss levels, a straight fit, and the old order for comparison
nb = [(x, D[k]) for x, k in zip(xs, order) if k not in BOSSES]
import numpy as np
fx, fy = np.array([a for a, _ in nb]), np.array([b for _, b in nb]); m, c0 = np.polyfit(fx, fy, 1)
ax0.plot(xs, [m * x + c0 for x in xs], color=red, lw=1.6, ls="--", zorder=3)
ax0.set_title("Thứ tự đề xuất: 65 màn, boss ở 10 · 21 · 32 · 43 · 54 · 65, độ khó tăng đều", loc="left", fontsize=15, color=ink, pad=30)
ax0.legend(handles=[Patch(color=blue, label="màn thường"), Patch(color=orange, label="màn thùng"), Patch(facecolor=green, hatch="///", edgecolor="white", label="màn bò kính mới (ước tính)"),
                    Patch(color=surface, label="★ boss"), plt.Line2D([], [], color=red, ls="--", label="xu hướng (màn thường)")],
           loc="upper left", bbox_to_anchor=(0, 1.16), ncol=5, frameon=False, fontsize=10, labelcolor=ink)
ax2.set_facecolor(surface)
oldseq = [k for k in OLD]; newseq = order
ax2.plot(range(1, 61), [D[k] for k in oldseq], color="#9a998f", lw=1.6, marker="o", ms=3.5, label="trước: 60 màn (thứ tự hiện tại)")
ax2.plot(xs, [D[k] for k in newseq], color=blue, lw=2.2, marker="o", ms=4, label="sau: 65 màn (đề xuất)")
for x, k in zip(xs, newseq):
    if k in BOSSES: ax2.annotate("★", (x, D[k]), textcoords="offset points", xytext=(0, 6), ha="center", fontsize=11, color=ink)
ax2.set_xlim(.3, 65.7); ax2.set_ylabel("Độ khó", color=ink2, fontsize=10.5); ax2.grid(axis="y", color=grid); ax2.set_axisbelow(True)
for side in ("top", "right"): ax2.spines[side].set_visible(False)
ax2.tick_params(colors=ink2, labelsize=9); ax2.legend(loc="upper left", frameon=False, fontsize=10, labelcolor=ink)
ax2.set_title("So với thứ tự hiện tại (vị trí màn trên trục ngang)", loc="left", fontsize=13, color=ink, pad=10)
fig.text(.01, .004, "Độ khó = số quyết định trong lời giải + 0,25 × chỉ số đo trong game (giới hạn 20; 12 cho N26, N33, N35, N45, N50 vì lời giải mẫu cố ý làm sai). "
         "Chỉ số đo chạy lời giải mẫu 12/10/2026; G1–G5 ước tính từ thiết kế.\nLuật xếp: màn dạy luôn trước màn dùng; tối đa 3 cơ quan mới mỗi chương, cách nhau ≥3 màn, không dạy ở cuối chương; mỗi boss có màn luyện cùng loại trước nó; "
         "không 3 màn cùng loại liền nhau. Chương 1 giữ nguyên phần đầu đã duyệt.", fontsize=9, color=ink2)
fig.savefig(sys.argv[2] + ".raw.png", facecolor=surface, bbox_inches="tight")
im = Image.open(sys.argv[2] + ".raw.png").convert("RGB"); clean = Image.new("RGB", im.size); clean.putdata(list(im.getdata())); clean.save(sys.argv[2])
import os; os.remove(sys.argv[2] + ".raw.png")
for i, k in enumerate(order, 1):
    print(f"{i:2} {k:4} {'★' if k in BOSSES else ' '} D {D[k]:5.2f}  {title.get(k,'')}")
# smoothness: how far the non-boss levels stray from their trend, the largest jump up between neighbours (bosses aside), and
# the closest two lessons of new devices
res = fy - (m * fx + c0); jumps = [D[b] - D[a] for a, b in zip(order, order[1:]) if a not in BOSSES and b not in BOSSES]
teach = [i for i, k in enumerate(order) if k in TEACH]
per = [sum(1 for i in range(a - 1, b) if order[i] in TEACH) for a, b in zip(CH_START, BOSS_SLOTS)]
print(f"SMOOTH lessons/chapter {per} rms {np.sqrt((res**2).mean()):.2f} maxjump {max(jumps):.2f} closest-lessons {min(b-a for a,b in zip(teach,teach[1:]))} bosses {bosses}")
