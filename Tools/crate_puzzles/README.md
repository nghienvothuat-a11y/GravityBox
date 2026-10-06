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
