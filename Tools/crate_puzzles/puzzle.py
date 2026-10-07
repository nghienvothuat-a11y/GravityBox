"""Crate puzzles for COghe (Mrk, 06/10/2026): a red crate covers the floor exit; every crate slides on its own
rail between fixed stops (one pull = one move from one stop to the next) and crates block each other.
Goal: the exit cell is free. BFS gives the minimum number of pulls, the forced order and the decoy moves."""
from collections import deque
from dataclasses import dataclass, field

import os
# Grid columns (x, left->right) and rows (z, front->back); one cell = 12 cm. CRATE_GRID=7x6 tries a larger floor.
W, H = map(int, os.environ.get('CRATE_GRID', '6x5').split('x'))


@dataclass
class Crate:
    name: str           # shown on plates only as a number; in the game crates carry no letters
    shape: list         # cells relative to the anchor, [(dx, dz), ...]
    axis: str           # 'x' or 'z'
    stops: list         # anchor offsets along the axis; stops[0] is where it starts
    anchor: tuple       # anchor cell at offset 0
    red: bool = False
    heavy: bool = False  # 100%: only the whole body moves it
    tall: bool = False

    def cells(self, stop):
        ax, az = self.anchor
        d = self.stops[stop]
        ox, oz = (d, 0) if self.axis == 'x' else (0, d)
        return {(ax + ox + dx, az + oz + dz) for dx, dz in self.shape}

    def swept(self, a, b):
        """Every cell the crate covers on its way from stop a to stop b (rails slide, they do not jump)."""
        lo, hi = sorted((self.stops[a], self.stops[b]))
        out = set()
        ax, az = self.anchor
        for d in range(lo, hi + 1):
            ox, oz = (d, 0) if self.axis == 'x' else (0, d)
            out |= {(ax + ox + dx, az + oz + dz) for dx, dz in self.shape}
        return out


@dataclass
class Puzzle:
    key: str
    title: str
    hole: tuple
    crates: list
    walls: set = field(default_factory=set)   # fixed blocks (cells nothing can slide into)
    access: bool = False   # COghe needs a free cell behind the crate (to push) or past its stop (to pull)

    def check(self):
        state = tuple(0 for _ in self.crates)
        taken = set(self.walls)
        for c in self.crates:
            for cell in c.cells(0):
                assert 0 <= cell[0] < W and 0 <= cell[1] < H, (self.key, c.name, cell)
                assert cell not in taken, (self.key, c.name, cell)
                taken.add(cell)
            for s in range(len(c.stops)):
                for cell in c.cells(s):
                    assert 0 <= cell[0] < W and 0 <= cell[1] < H, (self.key, c.name, s, cell)
        assert self.hole in self.crates[0].cells(0) and self.crates[0].red, "crate 0 is the red one on the exit"
        return state

    def occupied(self, state, skip=None):
        cells = set(self.walls)
        for i, c in enumerate(self.crates):
            if i != skip:
                cells |= c.cells(state[i])
        return cells

    def reachable_side(self, c, frm, to, others):
        """A free floor cell inside the box right behind the crate (push) or right past where it stops (pull)."""
        d = c.stops[to] - c.stops[frm]
        step = (1 if d > 0 else -1)
        dx, dz = (step, 0) if c.axis == 'x' else (0, step)
        cur, dst = c.cells(frm), c.cells(to)
        behind = {(x - dx, z - dz) for x, z in cur} - cur
        ahead = {(x + dx, z + dz) for x, z in dst} - dst
        ok = lambda cell: 0 <= cell[0] < W and 0 <= cell[1] < H and cell not in others
        return any(map(ok, behind)) or any(map(ok, ahead))

    def moves(self, state):
        for i, c in enumerate(self.crates):
            # A rail with two stops toggles; with more, a pull goes to the next stop and wraps (TapRail.Stops).
            nxt = (state[i] + 1) % len(c.stops)
            others = self.occupied(state, skip=i)
            if c.swept(state[i], nxt) & others:
                continue
            if self.access and not self.reachable_side(c, state[i], nxt, others):
                continue
            s = list(state); s[i] = nxt
            yield i, tuple(s)

    def solved(self, state):
        return self.hole not in self.occupied(state)

    def solve(self):
        start = self.check()
        prev = {start: None}
        q = deque([start])
        while q:
            s = q.popleft()
            if self.solved(s):
                path = []
                while prev[s] is not None:
                    s, i = prev[s]
                    path.append(i)
                return list(reversed(path)), len(prev)
            for i, n in self.moves(s):
                if n not in prev:
                    prev[n] = (s, i)
                    q.append(n)
        return None, len(prev)

    def first_moves(self):
        """Pulls possible at the start, and which of them begin a shortest solution."""
        start = self.check()
        best, _ = self.solve()
        good, bad = [], []
        for i, n in self.moves(start):
            sub = Puzzle(self.key, self.title, self.hole, self.crates, self.walls)
            p, _ = _solve_from(sub, n)
            (good if p is not None and len(p) + 1 == len(best) else bad).append(self.crates[i].name)
        return good, bad


def _solve_from(p, start):
    prev = {start: None}
    q = deque([start])
    while q:
        s = q.popleft()
        if p.solved(s):
            path = []
            while prev[s] is not None:
                s, i = prev[s]
                path.append(i)
            return list(reversed(path)), len(prev)
        for i, n in p.moves(s):
            if n not in prev:
                prev[n] = (s, i)
                q.append(n)
    return None, len(prev)


def bar(n):        # 1 x n along x
    return [(i, 0) for i in range(n)]


def col(n):        # 1 x n along z
    return [(0, i) for i in range(n)]


SQUARE = [(0, 0), (1, 0), (0, 1), (1, 1)]
