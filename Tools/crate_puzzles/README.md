# Crate puzzles (thùng che lỗ thoát)

Solver, generator and plate renderer for the crate levels proposed in `PLANS/COGHE_CRATE_PUZZLES.md`.

- `puzzle.py`: the model. A 6 × 5 grid of 12 cm cells; each crate slides on its own rail between two stops (one pull
  moves it to the other stop); crates block each other; the goal is the exit cell free. BFS gives the shortest
  solution and the number of layouts a player can reach.
- `generate2.py`, `climb.py`, `batch2.py`: search. For a set of crates, BFS the whole state graph backwards from the
  solved layouts and start from the farthest one; hill-climb the crate set for depth, then for choice (reachable
  layouts). `python3 batch2.py P5 400 416` writes `cand2_P5.json`.
- `levels.py`: the five chosen levels (seeds inside) → `out/T1.png` … `out/T5.png`, `out/overview.png`,
  `out/summary.json` (exact crate definitions: shape cells, axis, stops, anchor).

Run from this folder: `uv run --with pillow python3 levels.py` (Pillow is only needed for the plates).

## Ten levels with standing room (06/10/2026)

`Puzzle(access=True)` adds COghe's standing room: a pull needs a free cell inside the box right behind the crate (push)
or right past its stop (pull). `search10.py <levels> <seed from> <seed to> [append]` searches the directed state graph
(backward BFS from the solved layouts over reversed pulls), requires the shapes each level introduces, and climbs for
depth, then no dead layouts, then choice → `cand10.json`. `levels10.py` renders the chosen ten (`PICKS`, names in
`names10.json`) into `out10/`.

## Ten levels redone so they read true (06/10/2026)

Mrk, on level 52: the red crate's slide looked clear but the order said otherwise. The hidden rule was the standing room.
`logic.py` searches layouts in which, in every layout the player can reach, a crate with a clear slide always has room for
COghe on one side (both cells of a two-cell face), COghe walks on the floor only (its piece of floor is part of the state),
the red crate starts blocked by a crate and its pull is the last one, and there are no dead layouts.
`python3 logic.py <levels> <seed from> <seed to> [append]` with `LOGIC_OUT=<file>`. Experiments switch rules off with
`LOGIC_FAIR=0`, `LOGIC_REDLAST=0`, `LOGIC_REDBLOCKED=0`, and try other floors with `CRATE_GRID=7x6 CRATE_HOLE=3,3`.
`levels_logic.py` renders the chosen ten (kept in `cand_logic.json`) into `out_logic/`: plates, the overview and
`summary_logic.json` (spawn, and each pull's crate, push or pull, and stand cell). `export_unity.py` turns that summary into
the C# layouts and routes of levels 51–60.
