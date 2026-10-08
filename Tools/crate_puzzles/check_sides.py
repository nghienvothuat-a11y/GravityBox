"""Mrk 08/10/2026: COghe grips a crate by any face. Does that change how hard the ten crate levels are? For each, the fewest
pulls with end grips only (as built) and with side grips too (LOGIC_SIDES=1), and whether the rules still hold.
Run: python3 check_sides.py  (it runs both modes itself)."""
import json, os, subprocess, sys

if os.environ.get('LOGIC_SIDES_CHILD') == '1':
    from puzzle import Crate
    from logic import Logic, shortest
    from levels_logic import PICKS, CANDIDATES
    cands = json.load(open(CANDIDATES)); out = []
    for k, (level, seed) in enumerate(PICKS, 1):
        r = next(r for r in cands if r['level'] == level and r['seed'] == seed)
        cs = [Crate(c['name'], [tuple(t) for t in c['shape']], c['axis'], c['stops'], tuple(c['anchor']), c['red'], False, c['tall']) for c in r['crates']]
        lg = Logic(cs); s0 = tuple(0 for _ in cs); node = (s0, tuple(r['spawn']))
        path = shortest(lg, node); reach = lg.reach(node)
        out.append(dict(key=f'K{k:02}', pulls=len(path), path=path, fair=all(lg.fair[b[0]] for b in reach), alive=all(b in lg.dist for b in reach),
                        redlast=path[-1] == 0, layouts=len(reach)))
    print(json.dumps(out)); sys.exit(0)

def run(sides):
    env = dict(os.environ, LOGIC_SIDES_CHILD='1', LOGIC_SIDES='1' if sides else '0')
    res = subprocess.run([sys.executable, __file__], env=env, capture_output=True, text=True, cwd=os.path.dirname(os.path.abspath(__file__)))
    return json.loads(res.stdout.strip().splitlines()[-1])

ends, sides = run(False), run(True)
for a, b in zip(ends, sides):
    flag = '' if a['pulls'] == b['pulls'] else '  <-- easier'
    print(f"{a['key']}: ends only {a['pulls']} pulls ({a['layouts']} layouts) | any face {b['pulls']} pulls ({b['layouts']} layouts) fair {b['fair']} no-dead {b['alive']} red-last {b['redlast']}{flag}")
