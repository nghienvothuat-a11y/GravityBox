"""The five crate levels (Mrk, 06/10/2026): level 1 by hand, 2–5 found by search (generate2/climb, seeds below) and all
checked by BFS. Writes one plate per level and an overview."""
import json
from PIL import Image, ImageDraw
from puzzle import Crate, Puzzle, bar, col
from render import plate, draw_plan, draw_iso, Camera, font, BG, INK, MUTED

HOLE = (3, 2)


def load(path, seed, key):
    r = next(r for r in json.load(open(path)) if r['seed'] == seed)
    return [Crate(c['name'], [tuple(t) for t in c['shape']], c['axis'], c['stops'], tuple(c['anchor']), c['red'], False, c['tall']) for c in r['crates']]


def kind(c):
    cells = len(c.shape)
    xs = {dx for dx, _ in c.shape}; zs = {dz for _, dz in c.shape}
    if c.red:
        return 'thùng đỏ'
    if cells == 4:
        return 'thùng vuông lớn'
    if cells == 3 and len(xs) > 1 and len(zs) > 1:
        return 'thùng chữ L'
    return f'thùng dài {cells} ô'


def where(c):
    xs = sorted({x for x, _ in c.cells(0)}); zs = sorted({z for _, z in c.cells(0)})
    col_name = 'trái' if xs[-1] <= 1 else 'phải' if xs[0] >= 4 else 'giữa'
    row_name = 'trước' if zs[-1] <= 1 else 'sau' if zs[0] >= 3 else 'giữa'
    return f'{row_name}–{col_name}' if row_name != col_name else 'giữa'


def direction(c, frm, to):
    d = c.stops[to] - c.stops[frm]
    if c.axis == 'x':
        return ('sang phải' if d > 0 else 'sang trái') + f' {abs(d)} ô'
    return ('vào trong' if d > 0 else 'ra phía trước') + f' {abs(d)} ô'


def steps_text(p, path):
    state = [0] * len(p.crates)
    seen = {}
    out = []
    for n, i in enumerate(path, 1):
        c = p.crates[i]
        frm = state[i]; to = (frm + 1) % len(c.stops)
        again = ' (lần 2: trả về)' if i in seen else ''
        what = kind(c) if c.red else f'{kind(c)} ({where(c)})'
        out.append(f'{n}. Kéo {what} {direction(c, frm, to)}{again}')
        seen[i] = n
        state[i] = to
    return out


LEVELS = [
    dict(key='T1', title='Thùng 1 · Dọn đường', idea='Thùng đỏ che lỗ thoát; một thùng chặn đường nó.', insert='sau màn 4 (Kéo là mở)',
         crates=[Crate('R', bar(2), 'x', [0, 1], (3, 2), red=True), Crate('1', col(2), 'z', [0, 2], (5, 1)), Crate('2', bar(2), 'x', [0, 2], (0, 0))],
         extra='Thùng phía trước–trái không liên quan: không phải thùng nào cũng cần kéo.'),
    dict(key='T2', title='Thùng 2 · Gỡ từ ngoài vào', idea='Ba thùng chặn nhau thành chuỗi: gỡ thùng ngoài cùng trước.', insert='sau màn 12 (Khối lớn đi trước)',
         crates=load('cand2_P2.json', 400, 'P2'), extra='Một thùng không cần kéo; kéo nhầm thì kéo lại được.'),
    dict(key='T3', title='Thùng 3 · Thùng vuông', idea='Thùng vuông lớn; một thùng phải kéo đi rồi kéo về.', insert='sau màn 22 (chương 3)',
         crates=load('cand_P3.json', 106, 'P3')),
    dict(key='T4', title='Thùng 4 · Đi rồi trả lại', idea='Hai thùng phải kéo đi rồi kéo về; có thùng chữ L.', insert='sau màn 32 (chương 4)',
         crates=load('cand2_P4.json', 402, 'P4')),
    dict(key='T5', title='Thùng 5 · Kho chật', idea='Tám thùng, có thùng vuông và chữ L; ba thùng phải kéo hai lần.', insert='trước Boss 50 (chương 5)',
         crates=load('cand2_P5.json', 401, 'P5')),
]


def main():
    import os
    os.makedirs('out', exist_ok=True)
    summary = []
    for lv in LEVELS:
        p = Puzzle(lv['key'], lv['title'], HOLE, lv['crates'])
        path, states = p.solve()
        good, bad = p.first_moves()
        reps = len(path) - len(set(path))
        used = len(set(path))
        steps = steps_text(p, path)
        sub = f"{len(path)} lần kéo ít nhất · {len(p.crates)} thùng · {reps} thùng kéo hai lần · chèn {lv['insert']}"
        notes = [lv['idea']]
        if lv.get('extra'):
            notes.append(lv['extra'])
        notes.append(f'Bộ giải BFS: {states} thế bày có thể tới; lời giải ngắn nhất {len(path)} lần kéo.')
        taken = p.occupied(tuple(0 for _ in p.crates))
        cogh = next(((x, z) for z in range(5) for x in range(6) if (x, z) not in taken), None)
        plate(p, path, lv['title'], sub, steps, notes, f"out/{lv['key']}.png", cogh=cogh)
        summary.append(dict(key=lv['key'], title=lv['title'], pulls=len(path), crates=len(p.crates), repeats=reps, used=used, states=states, steps=steps,
                            idea=lv['idea'], insert=lv['insert'],
                            crates_def=[dict(kind=kind(c), axis=c.axis, stops=c.stops, anchor=c.anchor, shape=c.shape, red=c.red) for c in p.crates]))
    json.dump(summary, open('out/summary.json', 'w'), ensure_ascii=False, indent=1)
    # Overview: the five plans side by side, easy to hard.
    sheet = Image.new('RGB', (5 * 640 + 40, 760), BG)
    d = ImageDraw.Draw(sheet)
    d.text((40, 24), 'Năm màn thùng, dễ → khó (số = lần kéo thứ mấy; vòng xanh bạc hà = lỗ thoát dưới thùng đỏ)', font=font(34), fill=INK)
    for k, lv in enumerate(LEVELS):
        p = Puzzle(lv['key'], lv['title'], HOLE, lv['crates'])
        path, _ = p.solve()
        tile = Image.new('RGB', (620, 660), BG)
        draw_plan(tile, p, path, 10, 120, 100)
        td = ImageDraw.Draw(tile)
        td.text((10, 10), lv['title'], font=font(30), fill=INK)
        td.text((10, 56), f"{len(path)} lần kéo · {len(p.crates)} thùng", font=font(24), fill=MUTED)
        sheet.paste(tile, (20 + k * 640, 90))
    sheet.save('out/overview.png')
    for s in summary:
        print(s['key'], s['pulls'], s['crates'], s['repeats'], s['states'])
        for t in s['steps']:
            print('   ', t)


if __name__ == '__main__':
    main()
