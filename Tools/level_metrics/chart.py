"""Charts: the difficulty of the current 60 positions, and the proposed order. Run: uv run --with matplotlib python3 chart.py"""
import json, os
import matplotlib
matplotlib.use('Agg')
import matplotlib.pyplot as plt
from matplotlib import font_manager
from metrics import load

font_manager.fontManager.addfont('/System/Library/Fonts/Supplemental/Arial Unicode.ttf')
plt.rcParams['font.family'] = 'Arial Unicode MS'
CH = ['#7fb3d5', '#82c9a3', '#f2c27b', '#c9a3d6', '#e8a3a8']   # chapters 1–5
CRATE = '#d9644f'
OUT = 'out'

levels = load('../../PLANS/difficulty-60/levels.jsonl')
by_key = {l['key']: l for l in levels}

# Proposed order: inside each chapter the level that brings a device in stays before the levels using it (and N18 before
# N19, 33 → 34 → 35); the rest rise by difficulty; two crate levels per chapter where their difficulty fits; finales last.
PROPOSED = [
    ['01', '02', '03', '04', '05', 'K01', '07', '06', '08', 'K02', '09', '10'],
    ['14', 'N15', '12', '16', '15', 'K03', 'N13', '17', 'K04', '20', 'N18', 'N19'],
    ['18', '21', 'E06', 'E04', 'E07', 'E05', '19', '13', '22', 'K05', 'K06', 'B1'],
    ['E08', 'E10', '23', 'E09', '26', 'K07', 'E11', '24', 'K08', '25', '27', '30'],
    ['E12', 'E16', 'E17', 'E14', 'E18', 'K09', '29', '28', 'K10', 'E15', 'E13', 'B2'],
]
flat = [k for ch in PROPOSED for k in ch]
assert sorted(flat) == sorted(by_key), (set(by_key) - set(flat), set(flat) - set(by_key))


def chapter_of_current(pos): return min((pos - 1) // 10, 5)


def draw(ax, seq, colours, title, chapters):
    xs = list(range(1, len(seq) + 1))
    ys = [l['score'] for l in seq]
    ax.bar(xs, ys, color=colours, edgecolor='white', linewidth=.6)
    # Five-level running mean: the curve a player feels.
    run = [sum(ys[max(0, i - 2):i + 3]) / len(ys[max(0, i - 2):i + 3]) for i in range(len(ys))]
    ax.plot(xs, run, color='#2b3a44', linewidth=2, label='trung bình trượt 5 màn')
    for x, l in zip(xs, seq):
        if l['key'] in ('10', 'B1', 'B2', '30', '20'):
            ax.text(x, l['score'] + .6, '★', ha='center', fontsize=12, color='#2b3a44')
    for b in chapters[:-1]:
        ax.axvline(b + .5, color='#9aa7ab', linestyle='--', linewidth=1)
    ax.set_xlim(.3, len(seq) + .7); ax.set_ylim(0, 30)
    ax.set_xticks(xs); ax.set_xticklabels([f"{x}\n{l['key']}" for x, l in zip(xs, seq)], fontsize=6.5)
    ax.set_ylabel('chỉ số độ khó'); ax.set_title(title, fontsize=15, loc='left')
    ax.grid(axis='y', color='#e3e7e8'); ax.set_axisbelow(True)


os.makedirs(OUT, exist_ok=True)
fig, axes = plt.subplots(2, 1, figsize=(20, 11.5))
cur = levels
draw(axes[0], cur, [CRATE if l['key'].startswith('K') else CH[chapter_of_current(l['pos'])] for l in cur],
     'Hiện tại: 60 màn theo thứ tự chơi (màu = chương; đỏ = 10 màn thùng 51–60; ★ = màn trùm / cuối chương)', [10, 20, 30, 40, 50, 60])
prop = [by_key[k] for k in flat]
colours = []
for c, ch in enumerate(PROPOSED):
    for k in ch: colours.append(CRATE if k.startswith('K') else CH[c])
draw(axes[1], prop, colours, 'Đề xuất: 5 chương × 12 màn, mỗi chương 2 màn thùng, trong chương tăng dần', [12, 24, 36, 48, 60])
axes[0].legend(loc='upper left'); axes[1].legend(loc='upper left')
fig.text(.01, .005, 'Chỉ số = thao tác cơ quan + đổi phần + 0,5 × chạm để đi + (số phần tối đa − 1) + 0,5 × số loại cơ quan. Đo bằng lời giải mẫu chạy trong game (MeasureLevelMetrics), 06/10/2026.',
         fontsize=10, color='#6e7c80')
fig.tight_layout(rect=(0, .02, 1, 1))
fig.savefig(f'{OUT}/difficulty_60.png', dpi=110)

# Table of the proposal for the doc.
rows = []
for c, ch in enumerate(PROPOSED):
    for i, k in enumerate(ch):
        l = by_key[k]; rows.append((c * 12 + i + 1, l['pos'], k, l['title'], l['score']))
json.dump(rows, open(f'{OUT}/proposal.json', 'w'), ensure_ascii=False)
for r in rows: print(r)
