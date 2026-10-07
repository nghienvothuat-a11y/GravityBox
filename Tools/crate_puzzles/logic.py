"""Crate levels that read true on screen (Mrk, 06/10/2026, on level 52: "chỉ cần kéo miếng màu đỏ xuống phía dưới là xong.
Tại sao phải đẩy xong mới đẩy miếng đó ... sửa lại những màn kiểu này cho có tính logic").

The ten levels of 06/10 used a hidden rule: COghe needs a free cell right behind a crate to push it (or past its stop to
pull it). A crate whose slide looked clear could still be stuck, so the order of pulls looked arbitrary. Here a layout
counts only if, in every layout the player can reach:

- every crate whose slide is clear can be pulled: there is room for COghe on one side (all the cells behind its face to
  push, or all the cells just past its stop to pull; a face two cells wide needs both); the only thing that ever holds
  a crate back is another crate in its way;
- the free floor is one piece, so COghe walks to any standing place on the floor and never has to climb over a crate.

Search as in search10.py: hill-climb crate sets; for each, BFS the state graph backwards from the solved layouts and
start from the farthest layout (red crate on the exit) whose whole reachable set obeys the two rules and has no dead
ends. `python3 logic.py <levels> <seed from> <seed to> [append]` → `cand_logic.json`."""
import itertools, json, random, sys
from collections import deque, defaultdict
from multiprocessing import Pool
from puzzle import Crate, Puzzle, W, H
from generate import SHAPES, fits
from generate2 import crate_set, normalised

import os
HOLE = tuple(map(int, os.environ.get('CRATE_HOLE', '3,2').split(',')))
FAIR = os.environ.get('LOGIC_FAIR', '1') == '1'          # experiments: switch a rule off
CONNECT = os.environ.get('LOGIC_CONNECT', '1') == '1'
REDLAST = os.environ.get('LOGIC_REDLAST', '1') == '1'
REDBLOCKED = os.environ.get('LOGIC_REDBLOCKED', '1') == '1'
ALL = ['bar2', 'col2', 'bar3', 'col3', 'square']
SPECS = {  # level: (other crates, target pulls, shapes, required: (crates 3 long, squares))
    1: (2, 2, ['bar2', 'col2'], (0, 0)), 2: (3, 3, ['bar2', 'col2'], (0, 0)), 3: (3, 4, ALL[:4], (1, 0)),
    4: (4, 5, ALL[:4], (2, 0)), 5: (4, 6, ALL, (1, 1)), 6: (5, 7, ALL, (1, 1)), 7: (5, 8, ALL, (2, 1)),
    8: (6, 10, ALL, (2, 1)), 9: (6, 12, ALL, (2, 1)), 10: (7, 15, ALL, (1, 1)),
    # Five or six crates go deeper than seven on a 6 x 5 floor that COghe must still walk: the hard end of the ramp.
    11: (5, 12, ALL, (2, 1)), 12: (5, 15, ALL, (1, 1)), 13: (6, 15, ALL, (1, 1)),
}


def inside(cell):
    return 0 <= cell[0] < W and 0 <= cell[1] < H


def direction(c, frm, to):
    s = 1 if c.stops[to] - c.stops[frm] > 0 else -1
    return (s, 0) if c.axis == 'x' else (0, s)


def stand_cells(c, frm, to, others):
    """Where COghe can stand for this pull: behind the face (push) or just past the stop (pull). Every cell along the
    face must be free floor: COghe stands at the middle of the face, across both cells of a two-cell face."""
    dx, dz = direction(c, frm, to)
    cur, dst = c.cells(frm), c.cells(to)
    behind = {(x - dx, z - dz) for x, z in cur} - cur
    beyond = {(x + dx, z + dz) for x, z in dst} - dst
    free = lambda cells: all(inside(t) and t not in others for t in cells)
    return [side for side, cells in (('push', behind), ('pull', beyond)) if free(cells)]


def components(free):
    """Each free cell -> the smallest cell of its piece of floor."""
    comp = {}
    for cell in sorted(free):
        if cell in comp: continue
        piece = {cell}; q = [cell]
        while q:
            x, z = q.pop()
            for n in ((x + 1, z), (x - 1, z), (x, z + 1), (x, z - 1)):
                if n in free and n not in piece: piece.add(n); q.append(n)
        for t in piece: comp[t] = cell
    return comp


class Logic:
    """State = (stops, COghe's piece of floor). COghe walks on the floor only; it pulls a crate from the side it can reach."""
    def __init__(self, cs):
        self.cs = cs
        self.p = Puzzle('g', 'g', HOLE, cs)
        everything = {(x, z) for x in range(W) for z in range(H)}
        self.valid = []
        for st in itertools.product((0, 1), repeat=len(cs)):
            cells = set(); ok = True
            for i, c in enumerate(cs):
                cc = c.cells(st[i])
                if cc & cells: ok = False; break
                cells |= cc
            if ok: self.valid.append(st)
        self.comp, self.fair = {}, {}
        for s in self.valid:
            self.comp[s] = components(everything - self.p.occupied(s))
            fair = True
            for i, c in enumerate(cs):
                t = 1 - s[i]; others = self.p.occupied(s, skip=i)
                if not c.swept(s[i], t) & others and not stand_cells(c, s[i], t, others): fair = False   # clear slide, no room
            self.fair[s] = fair or not FAIR
        self.fwd = {}; self.move = {}
        for s in self.valid:
            for r in set(self.comp[s].values()):
                node = (s, r); self.fwd[node] = []
                if self.solved(node): continue
                for i, c in enumerate(cs):
                    t = 1 - s[i]; others = self.p.occupied(s, skip=i)
                    if c.swept(s[i], t) & others: continue
                    dx, dz = direction(c, s[i], t); dist = abs(c.stops[t] - c.stops[s[i]])
                    cur, dst = c.cells(s[i]), c.cells(t)
                    for side in stand_cells(c, s[i], t, others):
                        if side == 'push':
                            stand = sorted({(x - dx, z - dz) for x, z in cur} - cur)
                            end = [(x + dx * dist, z + dz * dist) for x, z in stand]
                        else:
                            stand = sorted({(x + dx, z + dz) for x, z in cur} - cur)       # at the face, inside the slide
                            end = sorted({(x + dx, z + dz) for x, z in dst} - dst)         # backed off past the stop
                        if self.comp[s].get(stand[0]) != r: continue
                        n = list(s); n[i] = t; n = tuple(n)
                        nxt = (n, self.comp[n][end[0]])
                        self.fwd[node].append((i, nxt)); self.move[(node, i, nxt)] = (side, stand)
        rev = defaultdict(list)
        for a, ms in self.fwd.items():
            for _, b in ms: rev[b].append(a)
        self.dist = {}; q = deque()
        for a in self.fwd:
            if self.solved(a): self.dist[a] = 0; q.append(a)
        while q:
            a = q.popleft()
            for b in rev[a]:
                if b not in self.dist: self.dist[b] = self.dist[a] + 1; q.append(b)

    def solved(self, node):
        s, r = node
        return self.comp[s].get(HOLE) == r      # the exit is open and COghe can walk to it

    def rush(self, stops):
        """Pulls needed if COghe could walk anywhere: how much of the order comes from crates in each other's way."""
        prev = {stops}; q = deque([(stops, 0)])
        while q:
            s, d = q.popleft()
            if HOLE not in self.p.occupied(s): return d
            for i, c in enumerate(self.cs):
                t = 1 - s[i]; others = self.p.occupied(s, skip=i)
                if c.swept(s[i], t) & others or not stand_cells(c, s[i], t, others): continue
                n = list(s); n[i] = t; n = tuple(n)
                if n not in prev: prev.add(n); q.append((n, d + 1))
        return 0

    def reach(self, start):
        seen = {start}; q = [start]
        while q:
            a = q.pop()
            for _, b in self.fwd[a]:
                if b not in seen: seen.add(b); q.append(b)
        return seen

    def best_start(self):
        """Farthest start (red crate on the exit) whose reachable layouts are all fair and all still solvable."""
        for d, a in sorted(((d, a) for a, d in self.dist.items() if a[0][0] == 0 and d > 0), reverse=True):
            r = self.reach(a)
            if all(self.fair[b[0]] and b in self.dist for b in r):
                # The pull that clears the exit is the last one: opening the exit and still being cut off from it reads
                # like a trick.
                if REDLAST and shortest(self, a)[-1] != 0: continue
                # Mrk's idea: the red crate is held by crates in its way, not only by COghe being shut in.
                if REDBLOCKED and not self.cs[0].swept(0, 1) & self.p.occupied(a[0], skip=0): continue
                return d, a, len(r), self.rush(a[0])
        return None


def assess(cs):
    return Logic(cs).best_start()


def counts(cs):
    return sum(1 for c in cs[1:] if len(c.shape) == 3), sum(1 for c in cs[1:] if len(c.shape) == 4)


def meets(cs, need):
    l, q = counts(cs)
    return l >= need[0] and q >= need[1]


def mutate(rng, cs, kinds, force=None):
    cs = list(cs); i = rng.randrange(1, len(cs))
    for _ in range(200):
        kind = force or rng.choice(kinds); shape = SHAPES[kind]
        axis = 'x' if kind in ('bar2', 'bar3') else 'z' if kind in ('col2', 'col3') else rng.choice('xz')
        anchor = (rng.randrange(W), rng.randrange(H))
        if not fits(shape, anchor, axis, 0): continue
        opts = [d for d in range(-4, 5) if d and fits(shape, anchor, axis, d)]
        if not opts: continue
        cs[i] = Crate(cs[i].name, shape, axis, [0, rng.choice(opts)], anchor, tall=kind == 'square'); return cs
    return cs


def run(args):
    level, seed = args
    n, target, kinds, need = SPECS[level]
    rng = random.Random(seed * 100 + level)
    cs, a = None, None
    for _ in range(600):
        cs = crate_set(rng, n, HOLE, kinds)
        if cs is None: continue
        for _ in range(60):
            if meets(cs, need): break
            l, q = counts(cs)
            cs = mutate(rng, cs, kinds, force=rng.choice(['bar3', 'col3']) if l < need[0] else 'square')
        if not meets(cs, need): continue
        a = assess(cs)
        if a: break
    if not a: return None
    # Depth up to the target, then how much of it comes from crates blocking each other, then choice.
    value = lambda a: (min(a[0], target), min(a[3], target), a[2])
    best = value(a)
    for _ in range(2500):
        cand = mutate(rng, cs, kinds)
        if not meets(cand, need): continue
        b = assess(cand)
        if not b: continue
        if value(b) >= best: cs, a, best = cand, b, value(b)
    d, (stops, piece), reach, rush = a
    cs = normalised(cs, stops)
    lg = Logic(cs)
    s0 = tuple(0 for _ in cs)
    node = (s0, piece)
    assert lg.dist[node] == d and all(lg.fair[b[0]] for b in lg.reach(node))
    path = shortest(lg, node)
    return dict(level=level, seed=seed, depth=d, rush=rush, reach=reach, repeats=len(path) - len(set(path)), path=path, spawn=piece,
                crates=[dict(name=c.name, shape=c.shape, axis=c.axis, stops=c.stops, anchor=c.anchor, red=c.red, tall=c.tall) for c in cs])


def shortest(lg, s0):
    prev = {s0: None}; q = deque([s0])
    while q:
        s = q.popleft()
        if lg.solved(s):
            path = []
            while prev[s] is not None: s, i = prev[s]; path.append(i)
            return list(reversed(path))
        for i, t in lg.fwd[s]:
            if t not in prev: prev[t] = (s, i); q.append(t)


if __name__ == '__main__':
    levels = [int(x) for x in sys.argv[1].split(',')]; seeds = range(int(sys.argv[2]), int(sys.argv[3]))
    with Pool(9) as pool:
        out = [r for r in pool.map(run, [(l, s) for l in levels for s in seeds]) if r]
    old = json.load(open(os.environ.get('LOGIC_OUT', 'cand_logic.json'))) if len(sys.argv) > 4 else []
    json.dump(old + out, open(os.environ.get('LOGIC_OUT', 'cand_logic.json'), 'w'))
    for l in levels:
        rs = sorted([r for r in out if r['level'] == l], key=lambda r: (-min(r['depth'], SPECS[l][1]), -r.get('rush', 0), -r['reach']))
        for r in rs[:3]:
            print(l, 'seed', r['seed'], 'depth', r['depth'], 'rush', r.get('rush'), 'reach', r['reach'], 'rep', r['repeats'])
