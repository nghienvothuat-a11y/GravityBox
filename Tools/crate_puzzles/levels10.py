"""Ten crate levels with standing room for COghe (Mrk, 06/10/2026: "10 màn kéo hộp ... độ khó tăng dần"; design
before any Unity work). Picks from cand10.json (search10.py), checks each with BFS, writes plates and an overview."""
import json, os
from PIL import Image, ImageDraw
from puzzle import Crate, Puzzle
from render import plate, draw_plan, font, BG, INK, MUTED
from levels import kind, where, direction

HOLE = (3, 2)
PICKS = [(1, 1), (2, 1), (3, 3), (4, 3), (5, 3), (6, 4), (7, 5), (8, 3), (9, 33), (10, 215)]
NAMES = {}


def load(level, seed):
    r = next(r for r in json.load(open('cand10.json')) if r['level'] == level and r['seed'] == seed)
    cs = [Crate(c['name'], [tuple(t) for t in c['shape']], c['axis'], c['stops'], tuple(c['anchor']), c['red'], False, c['tall']) for c in r['crates']]
    return Puzzle(f'K{level:02}', '', HOLE, cs, access=True)


def steps_text(p, path):
    state = [0] * len(p.crates); times = {}; out = []
    for n, i in enumerate(path, 1):
        c = p.crates[i]; frm = state[i]; to = (frm + 1) % len(c.stops)
        times[i] = times.get(i, 0) + 1
        k = times[i]
        again = '' if k == 1 else f' (lần {k}: trả về)' if k % 2 == 0 else f' (lần {k})'
        what = kind(c) if c.red else f'{kind(c)} ({where(c)})'
        out.append(f'{n}. {what[0].upper() + what[1:]} {direction(c, frm, to)}{again}')
        state[i] = to
    return out


def reach_and_dead(p):
    start = tuple(0 for _ in p.crates)
    seen = {start}; q = [start]
    while q:
        s = q.pop()
        for _, t in p.moves(s):
            if t not in seen: seen.add(t); q.append(t)
    return len(seen)


def main(names):
    os.makedirs('out10', exist_ok=True)
    summary = []; tiles = []
    for (level, seed), (title, idea) in zip(PICKS, names):
        p = load(level, seed)
        path, _ = p.solve()
        reach = reach_and_dead(p)
        reps = len(path) - len(set(path))
        shapes = sorted({kind(c) for c in p.crates if not c.red})
        sub = f'{len(path)} lần kéo ít nhất · {len(p.crates)} thùng · {reps} lần kéo lại thùng đã kéo · {reach} thế bày'
        notes = [idea, 'COghe đẩy từ ô trống sau thùng, hoặc kéo khi ô sau chỗ dừng còn trống.',
                 'Không có thế bày nào bị kẹt: kéo sai luôn kéo lại được.']
        plate(p, path, f'Màn {level} · {title}', sub, steps_text(p, path), notes, f'out10/K{level:02}.png',
              cogh=next(((x, z) for z in range(5) for x in range(6) if (x, z) not in p.occupied(tuple(0 for _ in p.crates))), None))
        summary.append(dict(level=level, title=title, idea=idea, pulls=len(path), crates=len(p.crates), returns=reps, reach=reach,
                            shapes=shapes, path=path, steps=steps_text(p, path),
                            crates_def=[dict(kind=kind(c), axis=c.axis, stops=c.stops, anchor=c.anchor, shape=c.shape, red=c.red) for c in p.crates]))
        tiles.append((p, path, level, title, len(path), len(p.crates)))
    json.dump(summary, open('out10/summary10.json', 'w'), ensure_ascii=False, indent=1)
    sheet = Image.new('RGB', (5 * 560 + 40, 2 * 600 + 90), BG); d = ImageDraw.Draw(sheet)
    d.text((40, 24), 'Mười màn kéo hộp, dễ → khó (số = lần kéo thứ mấy; vòng xanh bạc hà = lỗ thoát dưới thùng đỏ)', font=font(34), fill=INK)
    for k, (p, path, level, title, pulls, n) in enumerate(tiles):
        tile = Image.new('RGB', (540, 580), BG); td = ImageDraw.Draw(tile)
        draw_plan(tile, p, path, 10, 110, 86)
        td.text((10, 8), f'{level} · {title}', font=font(28), fill=INK)
        td.text((10, 50), f'{pulls} lần kéo · {n} thùng', font=font(22), fill=MUTED)
        sheet.paste(tile, (20 + (k % 5) * 560, 90 + (k // 5) * 600))
    sheet.save('out10/overview10.png')
    for s in summary:
        print(s['level'], s['title'], s['pulls'], s['crates'], s['returns'], s['reach'], s['shapes'])


if __name__ == '__main__':
    import sys
    names = json.load(open('names10.json')) if os.path.exists('names10.json') else [(f'Màn {i}', '') for i in range(1, 11)]
    main(names)
