"""Plates for the crate puzzles: a 3D view in the game's camera (pitch 36°, yaw 20°) of the glass box, and a plan with
the pull order, each crate's slide and the exit under the red crate."""
import math
from PIL import Image, ImageDraw, ImageFont
from puzzle import W, H

FONT = '/System/Library/Fonts/Supplemental/Arial Unicode.ttf'
BG = (244, 241, 232)
INK = (43, 58, 64)
MUTED = (110, 124, 128)
IVORY_TOP, IVORY_FRONT, IVORY_SIDE = (236, 231, 211), (214, 206, 182), (198, 190, 166)
TALL_TOP, TALL_FRONT, TALL_SIDE = (226, 220, 196), (200, 192, 166), (184, 176, 150)
RED_TOP, RED_FRONT, RED_SIDE = (222, 112, 92), (196, 92, 74), (178, 80, 64)
FLOOR = (205, 216, 220)
GRID = (186, 198, 203)
MINT = (36, 168, 128)
HANDLE = (57, 127, 173)
CELL = .12  # metres
BOX = (.80, .60)


def font(size):
    return ImageFont.truetype(FONT, size)


class Camera:
    def __init__(self, cx, cy, scale, pitch=36, yaw=20):
        p, y = math.radians(pitch), math.radians(yaw)
        self.right = (math.cos(y), 0, -math.sin(y))
        self.up = (math.sin(y) * math.sin(p), math.cos(p), math.cos(y) * math.sin(p))
        self.fwd = (math.sin(y) * math.cos(p), -math.sin(p), math.cos(y) * math.cos(p))
        self.cx, self.cy, self.s = cx, cy, scale

    def pt(self, v):
        x = sum(a * b for a, b in zip(v, self.right)); y = sum(a * b for a, b in zip(v, self.up))
        return (self.cx + x * self.s, self.cy - y * self.s)

    def depth(self, v):
        return sum(a * b for a, b in zip(v, self.fwd))


def cell_origin(x, z):
    """World position (centre of the cell's floor) of grid cell (x, z); the grid is centred in the box."""
    gx, gz = W * CELL, H * CELL
    return (-gx / 2 + (x + .5) * CELL, 0, -gz / 2 + (z + .5) * CELL)


def cuboid_faces(x0, x1, z0, z1, h):
    top = [(x0, h, z0), (x1, h, z0), (x1, h, z1), (x0, h, z1)]
    front = [(x0, 0, z0), (x1, 0, z0), (x1, h, z0), (x0, h, z0)]
    left = [(x0, 0, z0), (x0, 0, z1), (x0, h, z1), (x0, h, z0)]
    return top, front, left


def crate_rects(cells):
    """Split a crate's cells into the unit cubes we draw (merged visually by matching colours and no inner lines)."""
    return sorted(cells)


def draw_iso(img, puzzle, state, cam, cogh=None, show_hole=True):
    d = ImageDraw.Draw(img, 'RGBA')
    bx, bz = BOX[0] / 2, BOX[1] / 2
    hgt = .30
    # Floor and the two far walls (glass, faint).
    d.polygon([cam.pt(v) for v in [(-bx, 0, -bz), (bx, 0, -bz), (bx, 0, bz), (-bx, 0, bz)]], fill=FLOOR)
    d.polygon([cam.pt(v) for v in [(-bx, 0, bz), (bx, 0, bz), (bx, hgt, bz), (-bx, hgt, bz)]], fill=(214, 226, 230, 160))
    d.polygon([cam.pt(v) for v in [(bx, 0, -bz), (bx, 0, bz), (bx, hgt, bz), (bx, hgt, -bz)]], fill=(206, 220, 225, 150))
    # Grid lines (printed faintly on the floor, like the rails' tracks).
    gx, gz = W * CELL / 2, H * CELL / 2
    for i in range(W + 1):
        x = -gx + i * CELL
        d.line([cam.pt((x, 0, -gz)), cam.pt((x, 0, gz))], fill=GRID, width=1)
    for j in range(H + 1):
        z = -gz + j * CELL
        d.line([cam.pt((-gx, 0, z)), cam.pt((gx, 0, z))], fill=GRID, width=1)
    # The exit: a mint ring in the floor under the red crate (drawn first; the crate covers most of it).
    hx, _, hz = cell_origin(*puzzle.hole)
    if show_hole:
        ring = [cam.pt((hx + .046 * math.cos(a), .001, hz + .046 * math.sin(a))) for a in [i * math.pi / 24 for i in range(49)]]
        d.line(ring, fill=MINT, width=4)
    # Crates: unit cubes, far to near.
    cubes = []
    for i, c in enumerate(puzzle.crates):
        h = .06 if c.tall else .045
        cols = (RED_TOP, RED_FRONT, RED_SIDE) if c.red else (TALL_TOP, TALL_FRONT, TALL_SIDE) if c.tall else (IVORY_TOP, IVORY_FRONT, IVORY_SIDE)
        cells = c.cells(state[i])
        for (x, z) in cells:
            ox, _, oz = cell_origin(x, z)
            x0, x1, z0, z1 = ox - CELL / 2 + .004, ox + CELL / 2 - .004, oz - CELL / 2 + .004, oz + CELL / 2 - .004
            # Join neighbouring cells of the same crate: no gap on shared sides.
            if (x - 1, z) in cells: x0 -= .008
            if (x + 1, z) in cells: x1 += .008
            if (x, z - 1) in cells: z0 -= .008
            if (x, z + 1) in cells: z1 += .008
            cubes.append((cam.depth((ox, h / 2, oz)), x0, x1, z0, z1, h, cols, c, (x, z), cells))
    cubes.sort(key=lambda t: -t[0])
    for _, x0, x1, z0, z1, h, (ct, cf, cs), c, (x, z), cells in cubes:
        top, front, left = cuboid_faces(x0, x1, z0, z1, h)
        edge = (150, 62, 48) if c.red else (150, 140, 112)
        if (x - 1, z) not in cells:
            d.polygon([cam.pt(v) for v in left], fill=cs)
        if (x, z - 1) not in cells:
            d.polygon([cam.pt(v) for v in front], fill=cf)
        d.polygon([cam.pt(v) for v in top], fill=ct)
        # Outline each crate (its outer edges only), so neighbouring ivory crates read as separate pieces.
        tl, tr, br, bl = top[0], top[1], top[2], top[3]   # front-left, front-right, back-right, back-left
        if (x, z - 1) not in cells: d.line([cam.pt(tl), cam.pt(tr)], fill=edge, width=2)
        if (x + 1, z) not in cells: d.line([cam.pt(tr), cam.pt(br)], fill=edge, width=2)
        if (x, z + 1) not in cells: d.line([cam.pt(br), cam.pt(bl)], fill=edge, width=2)
        if (x - 1, z) not in cells: d.line([cam.pt(bl), cam.pt(tl)], fill=edge, width=2)
        if (x - 1, z) not in cells and (x, z - 1) not in cells:
            d.line([cam.pt((x0, 0, z0)), cam.pt(tl)], fill=edge, width=2)
    # Handles: a short coloured bar on the face the crate is pulled from (its rail's first end).
    for i, c in enumerate(puzzle.crates):
        cells = c.cells(state[i])
        h = .06 if c.tall else .045
        sign = 1 if c.stops[1 if state[i] == 0 else 0] > c.stops[state[i]] else -1
        if c.axis == 'x':
            edge = max(cells) if sign > 0 else min(cells)
            ex = [cl for cl in cells if cl[0] == edge[0]]
            zc = sum(cl[1] for cl in ex) / len(ex)
            ox, _, oz = cell_origin(edge[0], zc)
            px = ox + sign * (CELL / 2 + .004)
            seg = [(px, h * .6, oz - .025), (px, h * .6, oz + .025)]
        else:
            edge = max(cells, key=lambda t: t[1]) if sign > 0 else min(cells, key=lambda t: t[1])
            ex = [cl for cl in cells if cl[1] == edge[1]]
            xc = sum(cl[0] for cl in ex) / len(ex)
            ox, _, oz = cell_origin(xc, edge[1])
            pz = oz + sign * (CELL / 2 + .004)
            seg = [(ox - .025, h * .6, pz), (ox + .025, h * .6, pz)]
        d.line([cam.pt(v) for v in seg], fill=(160, 70, 56) if c.red else HANDLE, width=5)
    # The exit seen through the red crate: a dashed mint ring on its top, so the plate shows what it covers.
    if show_hole:
        red = puzzle.crates[0]
        if puzzle.hole in red.cells(state[0]):
            top = (.06 if red.tall else .045) + .001
            for k in range(0, 48, 2):
                a0, a1 = k * math.pi / 24, (k + 1) * math.pi / 24
                d.line([cam.pt((hx + .046 * math.cos(a0), top, hz + .046 * math.sin(a0))), cam.pt((hx + .046 * math.cos(a1), top, hz + .046 * math.sin(a1)))], fill=(255, 255, 255, 230), width=4)
    if cogh is not None:
        ox, _, oz = cell_origin(*cogh)
        cxp, cyp = cam.pt((ox, .02, oz))
        r = cam.s * .035
        d.ellipse([cxp - r * 1.25, cyp - r * .8, cxp + r * 1.25, cyp + r * .8], fill=(30, 32, 38))
        d.ellipse([cxp - r * .5, cyp - r * .55, cxp - r * .05, cyp - r * .25], fill=(120, 126, 140))
    # Glass frame edges.
    corners = [(-bx, 0, -bz), (bx, 0, -bz), (bx, 0, bz), (-bx, 0, bz)]
    tops = [(x, hgt, z) for x, _, z in corners]
    for a, b in zip(corners, corners[1:] + corners[:1]):
        d.line([cam.pt(a), cam.pt(b)], fill=(150, 170, 176), width=3)
    for a, b in zip(tops, tops[1:] + tops[:1]):
        d.line([cam.pt(a), cam.pt(b)], fill=(170, 186, 190), width=2)
    for a, b in zip(corners, tops):
        d.line([cam.pt(a), cam.pt(b)], fill=(170, 186, 190), width=2)


def draw_plan(img, puzzle, path, ox, oy, cs):
    """Top view: front row at the bottom. Each crate shows its slide (arrow to a dashed ghost) and when it is pulled."""
    d = ImageDraw.Draw(img, 'RGBA')
    def rect(x, z):
        return (ox + x * cs, oy + (H - 1 - z) * cs, ox + (x + 1) * cs, oy + (H - z) * cs)
    d.rectangle([ox - 6, oy - 6, ox + W * cs + 6, oy + H * cs + 6], fill=(220, 228, 231))
    for x in range(W):
        for z in range(H):
            d.rectangle(rect(x, z), outline=GRID, width=1, fill=(232, 238, 240))
    order = {}
    for step, i in enumerate(path, 1):
        order.setdefault(i, []).append(step)
    for i, c in enumerate(puzzle.crates):
        # Ghost where the pull takes it.
        for (x, z) in c.cells(1):
            gx0, gy0, gx1, gy1 = rect(x, z)
            d.rectangle([gx0 + 6, gy0 + 6, gx1 - 6, gy1 - 6], outline=(178, 80, 64, 150) if c.red else (120, 130, 132, 140), width=2)
    for i, c in enumerate(puzzle.crates):
        fill = RED_TOP if c.red else TALL_TOP if c.tall else IVORY_TOP
        line = RED_SIDE if c.red else (160, 152, 128)
        cells = c.cells(0)
        for (x, z) in cells:
            gx0, gy0, gx1, gy1 = rect(x, z)
            l = 0 if (x - 1, z) in cells else 4; r = 0 if (x + 1, z) in cells else 4
            t = 0 if (x, z + 1) in cells else 4; b = 0 if (x, z - 1) in cells else 4
            d.rectangle([gx0 + l, gy0 + t, gx1 - r, gy1 - b], fill=fill)
        # Outline only on the crate's outer edges.
        for (x, z) in cells:
            gx0, gy0, gx1, gy1 = rect(x, z)
            if (x - 1, z) not in cells: d.line([gx0 + 4, gy0 + 4, gx0 + 4, gy1 - 4], fill=line, width=3)
            if (x + 1, z) not in cells: d.line([gx1 - 4, gy0 + 4, gx1 - 4, gy1 - 4], fill=line, width=3)
            if (x, z + 1) not in cells: d.line([gx0 + 4, gy0 + 4, gx1 - 4, gy0 + 4], fill=line, width=3)
            if (x, z - 1) not in cells: d.line([gx0 + 4, gy1 - 4, gx1 - 4, gy1 - 4], fill=line, width=3)
        # Slide arrow from the crate's centre toward its other stop.
        cx = sum((rect(x, z)[0] + rect(x, z)[2]) / 2 for x, z in cells) / len(cells)
        cy = sum((rect(x, z)[1] + rect(x, z)[3]) / 2 for x, z in cells) / len(cells)
        dd = c.stops[1] - c.stops[0]
        vx, vy = (dd * cs, 0) if c.axis == 'x' else (0, -dd * cs)
        length = math.hypot(vx, vy); ux, uy = vx / length, vy / length
        sx, sy = cx + ux * cs * .18, cy + uy * cs * .18
        ex, ey = cx + vx * .62, cy + vy * .62
        col = (160, 70, 56) if c.red else HANDLE
        d.line([sx, sy, ex, ey], fill=col, width=5)
        px, py = -uy, ux
        d.polygon([(ex + ux * 14, ey + uy * 14), (ex + px * 10, ey + py * 10), (ex - px * 10, ey - py * 10)], fill=col)
        # When it is pulled. The red crate's number sits on its cell away from the exit ring.
        label = ' '.join(str(n) for n in order.get(i, [])) or '–'
        f = font(int(cs * .26))
        tw = d.textlength(label, font=f)
        bxp, byp = cx - ux * cs * .22, cy - uy * cs * .22
        if c.red:
            rest = [cl for cl in cells if cl != puzzle.hole]
            bxp = sum((rect(x, z)[0] + rect(x, z)[2]) / 2 for x, z in rest) / len(rest)
            byp = sum((rect(x, z)[1] + rect(x, z)[3]) / 2 for x, z in rest) / len(rest)
        r = max(tw / 2 + 10, cs * .19)
        d.rounded_rectangle([bxp - r, byp - cs * .19, bxp + r, byp + cs * .19], radius=int(cs * .19), fill=(255, 255, 255, 235), outline=col, width=3)
        d.text((bxp - tw / 2, byp - cs * .17), label, font=f, fill=INK)
    # The exit, drawn over the red crate that hides it.
    x0, y0, x1, y1 = rect(*puzzle.hole)
    for k in range(0, 36, 2):
        a0, a1 = k * math.pi / 18, (k + 1) * math.pi / 18
        rr = cs * .32; mx, my = (x0 + x1) / 2, (y0 + y1) / 2
        d.line([mx + rr * math.cos(a0), my + rr * math.sin(a0), mx + rr * math.cos(a1), my + rr * math.sin(a1)], fill=MINT, width=5)


def plate(puzzle, path, title, subtitle, steps, notes, out, cogh=(0, 0)):
    size = 21 if len(steps) <= 8 else 19 if len(steps) <= 11 else 18
    height = max(1100, 660 + 42 + len(steps) * (size + 9) + 8 + len(notes) * 26 + 40)
    img = Image.new('RGB', (1800, height), BG)
    d = ImageDraw.Draw(img)
    d.text((60, 40), title, font=font(48), fill=INK)
    d.text((60, 104), subtitle, font=font(28), fill=MUTED)
    cam = Camera(560, 700, 940)
    start = tuple(0 for _ in puzzle.crates)
    draw_iso(img, puzzle, start, cam, cogh=cogh)
    d.text((70, height - 90), 'Trong game: thùng ngà, tay nắm xanh ở đầu kéo; thùng đỏ che lỗ thoát (vòng xanh bạc hà).', font=font(22), fill=MUTED)
    draw_plan(img, puzzle, path, 1150, 200, 86)
    d.text((1150, 160), 'Nhìn từ trên (hàng dưới = phía trước). Số = lần kéo thứ mấy.', font=font(19), fill=MUTED)
    y = 660
    d.text((1150, y), 'Lời giải ngắn nhất', font=font(28), fill=INK); y += 42
    for s in steps:
        d.text((1150, y), s, font=font(size), fill=INK); y += size + 9
    y += 8
    for n in notes:
        d.text((1150, y), n, font=font(18), fill=MUTED); y += 26
    img.save(out)
