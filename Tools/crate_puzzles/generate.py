"""Search for crate puzzles with a given depth: random layouts, BFS, keep the ones whose shortest solution has the
wanted number of pulls, uses most crates, and (for the harder ones) has to pull some crate twice."""
import random, sys, json
from puzzle import Crate, Puzzle, W, H, bar, col, SQUARE

SHAPES = {
    'bar2': bar(2), 'bar3': bar(3), 'col2': col(2), 'col3': col(3), 'square': SQUARE,
    'L': [(0, 0), (1, 0), (0, 1)], 'J': [(0, 0), (1, 0), (1, 1)],
}


def fits(shape, anchor, axis, d):
    ax, az = anchor
    ox, oz = (d, 0) if axis == 'x' else (0, d)
    return all(0 <= ax + ox + dx < W and 0 <= az + oz + dz < H for dx, dz in shape)


def random_crate(name, kinds, rng, three_stops):
    kind = rng.choice(kinds)
    shape = SHAPES[kind]
    # Long crates slide along their length (a rail along the crate), squares and L either way.
    axis = 'x' if kind in ('bar2', 'bar3') else 'z' if kind in ('col2', 'col3') else rng.choice('xz')
    anchor = (rng.randrange(W), rng.randrange(H))
    if not fits(shape, anchor, axis, 0):
        return None
    options = [d for d in range(-5, 6) if d and fits(shape, anchor, axis, d)]
    if not options:
        return None
    if three_stops and len(options) >= 2 and rng.random() < .35:
        a, b = rng.sample(options, 2)
        stops = [0, a, b]
    else:
        stops = [0, rng.choice(options)]
    return Crate(name, shape, axis, stops, anchor, tall=kind == 'square')


def make(rng, n, three_stops, hole):
    # The red crate: 1 x 2 over the hole, sliding off it along its length.
    axis = rng.choice('xz')
    shape = bar(2) if axis == 'x' else col(2)
    off = rng.choice([0, 1])
    anchor = (hole[0] - off, hole[1]) if axis == 'x' else (hole[0], hole[1] - off)
    if not fits(shape, anchor, axis, 0):
        return None
    options = [d for d in range(-4, 5) if d and fits(shape, anchor, axis, d)
               and hole not in Crate('r', shape, axis, [0, d], anchor).cells(1)]
    if not options:
        return None
    red = Crate('R', shape, axis, [0, rng.choice(options)], anchor, red=True)
    crates = [red]
    taken = set(red.cells(0))
    kinds = ['bar2', 'bar3', 'col2', 'col3', 'square', 'L', 'J']
    tries = 0
    while len(crates) < n + 1 and tries < 400:
        tries += 1
        c = random_crate(str(len(crates)), kinds, rng, three_stops)
        if c is None or c.cells(0) & taken:
            continue
        crates.append(c)
        taken |= c.cells(0)
    if len(crates) < n + 1:
        return None
    return Puzzle('gen', 'gen', hole, crates)


def score(p):
    path, explored = p.solve()
    if path is None:
        return None
    used = set(path)
    repeats = len(path) - len(used)
    good, bad = p.first_moves()
    return dict(moves=len(path), used=len(used), crates=len(p.crates), repeats=repeats, first_good=len(good),
                first_all=len(good) + len(bad), states=explored, path=path)


def search(target, n, three_stops, repeats_min, seed, hole=(3, 2), budget=40000):
    rng = random.Random(seed)
    found = []
    for _ in range(budget):
        p = make(rng, n, three_stops, hole)
        if p is None:
            continue
        s = score(p)
        if s is None or s['moves'] < target[0] or s['moves'] > target[1]:
            continue
        if s['repeats'] < repeats_min or s['used'] < s['crates'] - 1:
            continue
        found.append((s, p))
        if len(found) >= 12:
            break
    return found


if __name__ == '__main__':
    lo, hi, n, three, rep, seed = map(int, sys.argv[1:7])
    for s, p in search((lo, hi), n, bool(three), rep, seed):
        print(json.dumps({k: v for k, v in s.items() if k != 'path'}), 'path', s['path'])
        for c in p.crates:
            print('  ', c.name, c.shape, c.axis, c.stops, c.anchor, 'RED' if c.red else '')
