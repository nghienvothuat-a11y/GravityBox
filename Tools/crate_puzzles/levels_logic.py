"""The ten crate levels redone so they read true on screen (Mrk, 06/10/2026, on level 52: "sửa lại những màn kiểu này cho
có tính logic"). Picks from cand_logic.json (logic.py), checks each again, writes plates and an overview into out_logic/.
Run: uv run --with pillow python3 levels_logic.py
Any-face grips (Mrk 08/10/2026): COghe also takes a crate by a long side and walks along with it. Run with LOGIC_SIDES=1. K01–K08
keep their layouts (their fewest pulls do not change); K09 and K10 were found again under the rule (cand_sides.json) so they
stay at 9 and 10 pulls."""
import json, os
from PIL import Image, ImageDraw
from puzzle import Crate, Puzzle
import render
from render import plate, draw_plan, font, BG, INK, MUTED

# Crates stand taller than COghe: they read as walls it walks around, not steps it climbs.
render.LOW_H, render.TALL_H = .06, .07
from levels10 import steps_text
from logic import Logic, shortest, HOLE, SIDES

CANDIDATES = 'cand_logic.json'   # the chosen ten (logic.py writes every candidate; trimmed to these)
# (search level, seed): easy to hard by the number of pulls.
PICKS = [(1, 0), (2, 5), (3, 19), (4, 124), (5, 292), (6, 135), (6, 48), (6, 182), (31, 5, 'cand_sides.json'), (33, 49, 'cand_sides.json')]
NAMES = [   # (title, idea)
    ('Một thùng chắn', 'Thùng đỏ trượt được, nhưng một thùng đứng ngay trên đường trượt.'),
    ('Chuỗi ba thùng', 'Thùng chắn thùng đỏ lại bị thùng khác chắn: gỡ từ ngoài vào.'),
    ('Thùng dài', 'Thùng dài 3 ô cần một khoảng trống dài mới trượt hết.'),
    ('Gỡ từng lớp', 'Năm thùng chắn nhau thành chuỗi: gỡ đúng thứ tự.'),
    ('Thùng vuông', 'Thùng vuông lớn; một thùng phải dời đi rồi trả về.'),
    ('Dây chuyền', 'Sáu thùng chắn nhau thành một chuỗi; không lần kéo nào thừa.'),
    ('Mở lối cho COghe', 'COghe bị thùng vây trong góc: mở lối cho nó trước.'),
    ('Đi rồi trả lại', 'Hai thùng phải dời tạm rồi trả về chỗ cũ.'),
    ('Kho chật', 'Chín lần kéo; một thùng phải dời qua lại bốn lần.'),
    ('Mê cung thùng', 'Mười lần kéo, bài cuối của bộ.'),
]


def load(level, seed, candidates=CANDIDATES):
    r = next(r for r in json.load(open(candidates)) if r['level'] == level and r['seed'] == seed)
    cs = [Crate(c['name'], [tuple(t) for t in c['shape']], c['axis'], c['stops'], tuple(c['anchor']), c['red'], False, c['tall']) for c in r['crates']]
    return cs, tuple(r['spawn'])


def spawn_cell(lg, piece):
    """COghe starts on the front-most, then left-most cell of its piece of floor."""
    s0 = tuple(0 for _ in lg.cs)
    cells = [c for c, r in lg.comp[s0].items() if r == piece]
    return min(cells, key=lambda c: (c[1], c[0]))


def main():
    os.makedirs('out_logic', exist_ok=True)
    summary = []; tiles = []
    assert SIDES, 'run with LOGIC_SIDES=1 (COghe grips any face)'
    for k, (pick, (title, idea)) in enumerate(zip(PICKS, NAMES), 1):
        level, seed = pick[:2]
        cs, piece = load(level, seed, *pick[2:])
        lg = Logic(cs); s0 = tuple(0 for _ in cs); node = (s0, piece)
        path = shortest(lg, node)
        reach = lg.reach(node)
        assert path[-1] == 0 and all(lg.fair[b[0]] and b in lg.dist for b in reach)
        p = Puzzle(f'K{k:02}', title, HOLE, cs)
        cogh = spawn_cell(lg, piece)
        reps = len(path) - len(set(path))
        sub = f'{len(path)} lần kéo ít nhất · {len(cs)} thùng · {reps} lần kéo lại thùng đã kéo · {len(reach)} thế bày'
        notes = [idea,
                 'Thùng nào thấy đường trượt trống thì kéo được ngay;',
                 'chỉ thùng chắn đường mới giữ nó lại.',
                 'COghe đi trên sàn, không trèo qua thùng (thùng cao gấp đôi COghe);',
                 'nó bám được mọi mặt thùng: đẩy, kéo, hoặc bám mặt bên đi dọc theo.',
                 'Kéo sai luôn kéo lại được: không có thế bày nào bị kẹt.']
        plate(p, path, f'Màn {50 + k} · {title}', sub, steps_text(p, path), notes, f'out_logic/K{k:02}.png', cogh=cogh, plan_cogh=True)
        moves = []
        a = node
        for i in path:
            b = next(b for j, b in lg.fwd[a] if j == i and b in lg.dist and lg.dist[b] == lg.dist[a] - 1)
            side, stand = lg.move[(a, i, b)]
            moves.append(dict(crate=i, side=side, stand=stand[len(stand) // 2]))
            a = b
        summary.append(dict(key=f'K{k:02}', level=50 + k, title=title, idea=idea, pulls=len(path), crates=len(cs), returns=reps,
                            reach=len(reach), spawn=cogh, path=path, moves=moves, steps=steps_text(p, path), source=dict(level=level, seed=seed),
                            crates_def=[dict(shape=c.shape, axis=c.axis, stops=c.stops, anchor=c.anchor, red=c.red) for c in cs]))
        tiles.append((p, path, k, title, len(path), len(cs), cogh))
    json.dump(summary, open('out_logic/summary_logic.json', 'w'), ensure_ascii=False, indent=1)
    sheet = Image.new('RGB', (5 * 560 + 40, 2 * 600 + 90), BG); d = ImageDraw.Draw(sheet)
    d.text((40, 24), 'Mười màn thùng làm lại, dễ → khó (số = lần kéo thứ mấy; chấm đen = COghe; vòng xanh bạc hà = lỗ thoát)', font=font(34), fill=INK)
    for k, (p, path, n, title, pulls, crates, cogh) in enumerate(tiles):
        tile = Image.new('RGB', (540, 580), BG); td = ImageDraw.Draw(tile)
        draw_plan(tile, p, path, 10, 110, 86, cogh=cogh)
        td.text((10, 8), f'{50 + n} · {title}', font=font(28), fill=INK)
        td.text((10, 50), f'{pulls} lần kéo · {crates} thùng', font=font(22), fill=MUTED)
        sheet.paste(tile, (20 + (k % 5) * 560, 90 + (k // 5) * 600))
    sheet.save('out_logic/overview_logic.png')
    for s in summary:
        print(s['key'], s['title'], s['pulls'], s['crates'], s['returns'], s['reach'], s['spawn'], s['moves'])


if __name__ == '__main__':
    main()
