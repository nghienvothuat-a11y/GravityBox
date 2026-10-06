"""Ten crate levels with COghe's standing room (Mrk, 06/10/2026: "10 màn kéo hộp ... độ khó tăng dần").
Directed state graph (a pull may need room on one side only): backward BFS from the solved layouts over reversed
pulls, start from the farthest layout with the red crate on the exit, hill-climb for depth then choice."""
import itertools, json, random, sys
from collections import deque, defaultdict
from multiprocessing import Pool
from puzzle import Crate, Puzzle, W, H
from generate import SHAPES, fits
from generate2 import crate_set, normalised

HOLE = (3, 2)
ALL = ['bar2', 'col2', 'bar3', 'col3', 'square']
SPECS = {  # level: (other crates, target pulls, shapes, required: (crates 3 long, squares))
    1: (2, 2, ['bar2', 'col2'], (0, 0)), 2: (3, 3, ['bar2', 'col2'], (0, 0)), 3: (3, 4, ALL[:4], (1, 0)),
    4: (4, 5, ALL[:4], (2, 0)), 5: (4, 6, ALL, (1, 1)), 6: (5, 7, ALL, (1, 1)), 7: (5, 8, ALL, (2, 1)),
    8: (6, 10, ALL, (2, 1)), 9: (6, 12, ALL, (2, 1)), 10: (7, 15, ALL, (1, 1)),
}


def counts(cs):
    long3 = sum(1 for c in cs[1:] if len(c.shape) == 3)
    sq = sum(1 for c in cs[1:] if len(c.shape) == 4)
    return long3, sq


def meets(cs, need):
    l, q = counts(cs)
    return l >= need[0] and q >= need[1]


def graph(cs):
    p = Puzzle('g', 'g', HOLE, cs, access=True)
    n = len(cs)
    valid = []
    for st in itertools.product((0, 1), repeat=n):
        cells = set(); ok = True
        for i, c in enumerate(cs):
            cc = c.cells(st[i])
            if cc & cells: ok = False; break
            cells |= cc
        if ok: valid.append(st)
    rev = defaultdict(list); fwd = {}
    for s in valid:
        fwd[s] = [t for _, t in p.moves(s)]
        for t in fwd[s]: rev[t].append(s)
    dist = {}; q = deque()
    for s in valid:
        if p.solved(s): dist[s] = 0; q.append(s)
    while q:
        s = q.popleft()
        for t in rev[s]:
            if t not in dist: dist[t] = dist[s] + 1; q.append(t)
    return p, fwd, dist


def assess(cs):
    p, fwd, dist = graph(cs)
    starts = [(d, s) for s, d in dist.items() if s[0] == 0]
    if not starts: return None
    d, start = max(starts)
    seen = {start}; q = [start]; dead = 0
    while q:
        s = q.pop()
        if s not in dist: dead += 1
        for t in fwd[s]:
            if t not in seen: seen.add(t); q.append(t)
    return d, start, len(seen), dead


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
    for _ in range(400):
        cs = crate_set(rng, n, HOLE, kinds)
        if cs is None: continue
        for _ in range(60):   # bring in the shapes this level introduces
            if meets(cs, need): break
            l, q = counts(cs)
            cs = mutate(rng, cs, kinds, force=rng.choice(['bar3', 'col3']) if l < need[0] else 'square')
        if not meets(cs, need): continue
        a = assess(cs)
        if a and a[0] >= 1: break
        a = None
    if not a: return None
    value = lambda a: (min(a[0], target), -a[3], a[2])   # depth up to the target, then no dead ends, then choice
    best = value(a)
    for _ in range(2500):
        cand = mutate(rng, cs, kinds)
        if not meets(cand, need): continue
        b = assess(cand)
        if not b or b[0] < 1: continue
        if value(b) >= best: cs, a, best = cand, b, value(b)
    d, start, reach, dead = a
    cs = normalised(cs, start)
    p = Puzzle('g', 'g', HOLE, cs, access=True)
    path, _ = p.solve()
    if path is None or len(path) != d: return None
    return dict(level=level, seed=seed, depth=d, reach=reach, dead=dead, repeats=len(path) - len(set(path)), path=path,
                crates=[dict(name=c.name, shape=c.shape, axis=c.axis, stops=c.stops, anchor=c.anchor, red=c.red, tall=c.tall) for c in cs])


if __name__ == '__main__':
    levels = [int(x) for x in sys.argv[1].split(',')]; seeds = range(int(sys.argv[2]), int(sys.argv[3]))
    with Pool(9) as pool:
        out = [r for r in pool.map(run, [(l, s) for l in levels for s in seeds]) if r]
    old = json.load(open('cand10.json')) if len(sys.argv) > 4 else []
    json.dump(old + out, open('cand10.json', 'w'))
    for l in levels:
        rs = sorted([r for r in out if r['level'] == l], key=lambda r: (-min(r['depth'], SPECS[l][1]), r['dead'], -r['reach']))
        for r in rs[:3]:
            print(l, 'seed', r['seed'], 'depth', r['depth'], 'reach', r['reach'], 'dead', r['dead'], 'rep', r['repeats'])
