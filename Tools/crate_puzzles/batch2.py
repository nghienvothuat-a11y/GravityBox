import json, sys
from multiprocessing import Pool
from puzzle import Puzzle
from climb import climb, climb2

SPECS = {
    'P2': dict(n=4, target=4, kinds=['bar2', 'bar3', 'col2', 'col3']),
    'P3': dict(n=4, target=6, kinds=['bar2', 'bar3', 'col2', 'col3', 'square']),
    'P4': dict(n=5, target=8, kinds=['bar2', 'bar3', 'col2', 'col3', 'square', 'L', 'J']),
    'P5': dict(n=7, target=10, kinds=['bar2', 'bar3', 'col2', 'col3', 'square', 'L', 'J']),
}


def run(args):
    key, seed = args
    spec = SPECS[key]
    best, cs, reach = climb2(seed, spec['n'], spec['target'], (3, 2), spec['kinds'], steps=2500)
    p = Puzzle(key, key, (3, 2), cs)
    path, _ = p.solve()
    if path is None:
        return None
    good, bad = p.first_moves()
    return dict(key=key, seed=seed, reach=reach, depth=len(path), path=path, used=len(set(path)), repeats=len(path) - len(set(path)),
                first_good=good, first_bad=bad,
                crates=[dict(name=c.name, shape=c.shape, axis=c.axis, stops=c.stops, anchor=c.anchor, red=c.red, tall=c.tall) for c in cs])


if __name__ == '__main__':
    key = sys.argv[1]
    seeds = range(int(sys.argv[2]), int(sys.argv[3]))
    with Pool(8) as pool:
        out = [r for r in pool.map(run, [(key, s) for s in seeds]) if r]
    json.dump(out, open(f'cand2_{key}.json', 'w'))
    for r in sorted(out, key=lambda r: (-r['depth'], -r['reach'])):
        print(r['seed'], 'reach', r['reach'], 'depth', r['depth'], 'used', r['used'], 'rep', r['repeats'], 'first', r['first_good'], r['first_bad'])
