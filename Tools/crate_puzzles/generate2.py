"""Deep crate puzzles: for a random set of two-stop crates, BFS the whole state graph backwards from every solved
state and take the start (red crate on the exit) farthest from any solution. Pulls are reversible, so the graph is
undirected and the farthest start is the hardest layout this set of crates allows."""
import itertools, random, sys, json
from collections import deque
from puzzle import Crate, Puzzle, W, H, bar, col
from generate import SHAPES, fits


def crate_set(rng, n, hole, kinds):
    axis = rng.choice('xz')
    shape = bar(2) if axis == 'x' else col(2)
    off = rng.choice([0, 1])
    anchor = (hole[0] - off, hole[1]) if axis == 'x' else (hole[0], hole[1] - off)
    if not fits(shape, anchor, axis, 0):
        return None
    opts = [d for d in range(-4, 5) if d and fits(shape, anchor, axis, d) and hole not in Crate('r', shape, axis, [0, d], anchor).cells(1)]
    if not opts:
        return None
    crates = [Crate('R', shape, axis, [0, rng.choice(opts)], anchor, red=True)]
    taken = set(crates[0].cells(0))
    tries = 0
    while len(crates) < n + 1 and tries < 800:
        tries += 1
        kind = rng.choice(kinds)
        shape = SHAPES[kind]
        axis = 'x' if kind in ('bar2', 'bar3') else 'z' if kind in ('col2', 'col3') else rng.choice('xz')
        anchor = (rng.randrange(W), rng.randrange(H))
        if not fits(shape, anchor, axis, 0):
            continue
        c0 = Crate('t', shape, axis, [0], anchor)
        if c0.cells(0) & taken:
            continue
        opts = [d for d in range(-4, 5) if d and fits(shape, anchor, axis, d)]
        if not opts:
            continue
        c = Crate(str(len(crates)), shape, axis, [0, rng.choice(opts)], anchor, tall=kind == 'square')
        crates.append(c)
        taken |= c.cells(0)
    return crates if len(crates) == n + 1 else None


def deepest(crates, hole):
    n = len(crates)
    valid = []
    for st in itertools.product((0, 1), repeat=n):
        cells = set()
        ok = True
        for i, c in enumerate(crates):
            cs = c.cells(st[i])
            if cs & cells:
                ok = False; break
            cells |= cs
        if ok:
            valid.append(st)
    vs = set(valid)
    p = Puzzle('g', 'g', hole, crates)
    dist = {}
    q = deque()
    for st in valid:
        if p.solved(st):
            dist[st] = 0; q.append(st)
    while q:
        s = q.popleft()
        for i in range(n):
            t = list(s); t[i] ^= 1; t = tuple(t)
            if t not in vs or t in dist:
                continue
            if crates[i].swept(s[i], t[i]) & p.occupied(s, skip=i):
                continue
            dist[t] = dist[s] + 1; q.append(t)
    starts = [(d, s) for s, d in dist.items() if s[0] == 0]
    return max(starts) if starts else None


def normalised(crates, start):
    """Re-anchor so every crate starts at stops[0]."""
    out = []
    for c, s in zip(crates, start):
        if s == 0:
            out.append(c)
        else:
            d = c.stops[1]
            ax, az = c.anchor
            anchor = (ax + d, az) if c.axis == 'x' else (ax, az + d)
            out.append(Crate(c.name, c.shape, c.axis, [0, -d], anchor, c.red, c.heavy, c.tall))
    return out


def search(depth_lo, depth_hi, n, seed, hole, kinds, budget, need_repeat):
    rng = random.Random(seed)
    found = []
    for _ in range(budget):
        cs = crate_set(rng, n, hole, kinds)
        if cs is None:
            continue
        r = deepest(cs, hole)
        if r is None:
            continue
        d, start = r
        if not depth_lo <= d <= depth_hi:
            continue
        cs = normalised(cs, start)
        p = Puzzle('g', 'g', hole, cs)
        path, _ = p.solve()
        if path is None or len(path) != d:
            continue
        used = len(set(path))
        repeats = len(path) - used
        if repeats < need_repeat or used < len(cs) - 2:
            continue
        good, bad = p.first_moves()
        found.append(dict(depth=d, used=used, crates=len(cs), repeats=repeats, first_good=len(good), first_all=len(good) + len(bad), path=path,
                          crates_def=[(c.name, c.shape, c.axis, c.stops, c.anchor, c.red, c.tall) for c in cs]))
        if len(found) >= 8:
            break
    return found


if __name__ == '__main__':
    lo, hi, n, seed, rep = map(int, sys.argv[1:6])
    kinds = sys.argv[6].split(',') if len(sys.argv) > 6 else ['bar2', 'bar3', 'col2', 'col3', 'square', 'L', 'J']
    for f in search(lo, hi, n, seed, (3, 2), kinds, 30000, rep):
        print(json.dumps({k: v for k, v in f.items() if k != 'crates_def'}))
        for c in f['crates_def']:
            print('   ', c)
