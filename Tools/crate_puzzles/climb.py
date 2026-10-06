"""Hill-climb crate sets toward a target depth (shortest solution length): mutate one crate at a time (new place, new
stop, new shape) and keep the change when the deepest start gets deeper. Prints puzzles that hit the target."""
import random, sys, json
from puzzle import Crate, Puzzle, W, H
from generate import SHAPES, fits
from generate2 import crate_set, deepest, normalised


def mutate(rng, crates, kinds):
    cs = list(crates)
    i = rng.randrange(1, len(cs))
    for _ in range(200):
        kind = rng.choice(kinds)
        shape = SHAPES[kind]
        axis = 'x' if kind in ('bar2', 'bar3') else 'z' if kind in ('col2', 'col3') else rng.choice('xz')
        anchor = (rng.randrange(W), rng.randrange(H))
        if not fits(shape, anchor, axis, 0):
            continue
        opts = [d for d in range(-4, 5) if d and fits(shape, anchor, axis, d)]
        if not opts:
            continue
        cs[i] = Crate(cs[i].name, shape, axis, [0, rng.choice(opts)], anchor, tall=kind == 'square')
        return cs
    return cs


def evaluate(cs, hole):
    r = deepest(cs, hole)
    if r is None:
        return -1, None
    return r


def reach(cs, start, hole):
    """How many layouts a player can reach from the start (pulls are reversible): a measure of choice."""
    p = Puzzle('g', 'g', hole, normalised(cs, start))
    s0 = tuple(0 for _ in cs)
    seen = {s0}; q = [s0]
    while q:
        s = q.pop()
        for _, n in p.moves(s):
            if n not in seen:
                seen.add(n); q.append(n)
    return len(seen)


def climb2(seed, n, target, hole, kinds, steps=3000):
    """Depth at least the target, then as many reachable layouts as possible."""
    rng = random.Random(seed)
    cs = None
    while cs is None or evaluate(cs, hole)[0] < 1:
        cs = crate_set(rng, n, hole, kinds)
    d, start = evaluate(cs, hole)
    def value(d, st, c):
        return (min(d, target), reach(c, st, hole) if st is not None else 0)
    best = value(d, start, cs)
    for _ in range(steps):
        cand = mutate(rng, cs, kinds)
        d2, st2 = evaluate(cand, hole)
        if d2 < 1:
            continue
        v = value(d2, st2, cand)
        if v >= best:
            cs, start, best, d = cand, st2, v, d2
    return d, normalised(cs, start), best[1]


def climb(seed, n, target, hole, kinds, steps=4000):
    rng = random.Random(seed)
    cs = None
    while cs is None or evaluate(cs, hole)[0] < 1:
        cs = crate_set(rng, n, hole, kinds)
    best, start = evaluate(cs, hole)
    for _ in range(steps):
        cand = mutate(rng, cs, kinds)
        d, st = evaluate(cand, hole)
        if d >= best:
            cs, best, start = cand, d, st
        if best >= target:
            break
    return best, normalised(cs, start), cs


if __name__ == '__main__':
    seed, n, target = map(int, sys.argv[1:4])
    hole = tuple(map(int, sys.argv[4].split(','))) if len(sys.argv) > 4 else (3, 2)
    kinds = sys.argv[5].split(',') if len(sys.argv) > 5 else ['bar2', 'bar3', 'col2', 'col3', 'square', 'L', 'J']
    best, cs, _ = climb(seed, n, target, hole, kinds)
    p = Puzzle('g', 'g', hole, cs)
    path, _ = p.solve()
    good, bad = p.first_moves()
    print(json.dumps(dict(depth=best, path=path, used=len(set(path)), repeats=len(path) - len(set(path)), first_good=good, first_bad=bad)))
    for c in cs:
        print('   ', (c.name, c.shape, c.axis, c.stops, c.anchor, c.red))
