"""Chapter 3 rebuilt (06/10/2026): decisions per level (the reference solution's moves, counted by hand: a pull, a push,
a split, a pad, a merge, a climb that matters) against the plan's curve (PLANS/COGHE_LEVEL_HOOK_PLAN.md 5.6).
Run: uv run --with matplotlib python3 chart_ch3.py"""
import matplotlib
matplotlib.use('Agg')
import matplotlib.pyplot as plt
from matplotlib import font_manager

font_manager.fontManager.addfont('/System/Library/Fonts/Supplemental/Arial Unicode.ttf')
plt.rcParams['font.family'] = 'Arial Unicode MS'
LEVELS = [  # position, name, decisions now, plan, done this round
    (21, 'Bám dây sang bờ', 2, 2, True), (22, 'Chất hàng trước', 3, 3, True), (23, 'Đưa bến lại gần', 3, 3.5, False),
    (24, 'Kéo đối trọng', 2, 2, True), (25, 'Chưa đủ nặng', 4, 4, True), (26, 'Nhẹ quá không nghiêng', 5, 5, True),
    (27, 'Ba mảnh thành đường', 4, 6, False), (28, 'Thùng đi thang', 3, 6, False), (29, 'Xếp tầng trên trước', 4, 4.5, True),
    (30, 'BOSS Tháp khối', 7, 11, False)]
fig, ax = plt.subplots(figsize=(13, 5.4))
xs = [l[0] for l in LEVELS]
ax.bar(xs, [l[2] for l in LEVELS], color=['#f2c27b' if l[4] else '#e3e6e8' for l in LEVELS],
       edgecolor=['#c9963f' if l[4] else '#9aa7ab' for l in LEVELS], hatch=None)
for l in LEVELS:
    if not l[4]: ax.bar([l[0]], [l[2]], color='none', edgecolor='#9aa7ab', hatch='//')
ax.plot(xs, [l[3] for l in LEVELS], 'o-', color='#2b3a44', linewidth=2, label='kế hoạch')
ax.set_xticks(xs); ax.set_xticklabels([f'{l[0]}\n{l[1]}' for l in LEVELS], fontsize=8)
ax.set_ylim(0, 12); ax.set_ylabel('số quyết định trong lời giải')
from matplotlib.patches import Patch
ax.legend(handles=[Patch(color='#f2c27b', label='đã làm theo kế hoạch'), Patch(facecolor='#e3e6e8', edgecolor='#9aa7ab', hatch='//', label='chưa làm sâu (phần sau)'),
                   plt.Line2D([], [], color='#2b3a44', marker='o', label='đường kế hoạch')], loc='upper left', fontsize=9)
ax.set_title('Chương 3 làm lại: số quyết định từng màn so với kế hoạch', fontsize=14, loc='left')
ax.grid(axis='y', color='#e3e7e8'); ax.set_axisbelow(True)
fig.tight_layout(); fig.savefig('out/chapter3.png', dpi=110)
