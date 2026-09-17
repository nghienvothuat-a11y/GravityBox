"""Abstract reachability audit for COghe Boss 20; NOT Unity/physics validation.
Mass = six tokens. An actor is (mass, room 0..2, holding_pad).
Pipe routes are reversible. Spatial merge proximity/clearance is assumed.
Cutters exist in rooms 0 and 1. The selected actor may act; other pads stay held.
G needs >=4 tokens and A held. Pads need >=2. C needs >=1.
H needs all six tokens in this coarse model (runtime force is continuous).
"""
import json
from collections import deque
from pathlib import Path

def state(parts, gear=False, core=False, cap=False):
    return (tuple(sorted(parts)), gear, core, cap)

START = state([(6, 0, False)])

def successors(s, max_parts=6):
    parts, gear, core, cap = s
    a = any(r == 0 and hold for m, r, hold in parts)
    b = any(r == 1 and hold for m, r, hold in parts)
    for i, (mass, room, hold) in enumerate(parts):
        rest = list(parts[:i] + parts[i + 1:])
        if hold:
            yield state(rest + [(mass, room, False)], gear, core, cap), f"Release pad {'A' if room == 0 else 'B'}"
            continue
        for other_room in (room - 1, room + 1):
            if 0 <= other_room <= 2:
                yield state(rest + [(mass, other_room, False)], gear, core, cap), f"Move {mass}/6: {room+1} -> {other_room+1}"
        if room in (0, 1) and mass >= 2:
            yield state(rest + [(mass, room, True)], gear, core, cap), f"Hold {'A' if room == 0 else 'B'} with {mass}/6"
        if room in (0, 1) and len(parts) < max_parts:
            for small in range(1, mass // 2 + 1):
                yield state(rest + [(small, room, False), (mass-small, room, False)], gear, core, cap), f"Cut {mass}/6 -> {small}/6 + {mass-small}/6 in {room+1}"
        if room == 1 and mass >= 4 and a and not gear:
            yield state(parts, True, core, cap), f"Raise G with {mass}/6 while A held"
        if room == 2 and a and b and gear and not core:
            yield state(parts, gear, True, cap), f"Pull C with {mass}/6 while A+B held: latch reunion doors and unlock H"
        if room == 2 and mass >= 6 and core and not cap:
            yield state(parts, gear, core, True), "Pull H with full body"
        for j in range(i+1, len(parts)):
            other_mass, other_room, other_hold = parts[j]
            if not other_hold and room == other_room:
                merged = [p for k,p in enumerate(parts) if k not in (i,j)]
                yield state(merged + [(mass+other_mass, room, False)], gear, core, cap), f"Merge {mass}/6 + {other_mass}/6 in {room+1}"

def goal(s):
    parts, gear, core, cap = s
    return cap and parts == ((6, 2, False),)

def explore(max_parts=6):
    q = deque([START])
    seen = {START}
    parent = {}
    reverse = {START: []}
    goals = []
    core_count = 0
    while q:
        s = q.popleft()
        if goal(s):
            goals.append(s)
        if s[2]:
            core_count += 1
        for nxt, action in successors(s, max_parts):
            reverse.setdefault(nxt, []).append(s)
            if nxt not in seen:
                seen.add(nxt)
                parent[nxt] = (s, action)
                q.append(nxt)
    recoverable = set(goals)
    q = deque(goals)
    while q:
        for prev in reverse[q.popleft()]:
            if prev not in recoverable:
                recoverable.add(prev)
                q.append(prev)
    trace = []
    if goals:
        s = goals[0]
        while s != START:
            prev, action = parent[s]
            trace.append(action)
            s = prev
        trace.reverse()
    return {
        "reachable_states": len(seen),
        "states_with_main_mechanism_open": core_count,
        "goal_states": len(goals),
        "states_with_a_path_to_goal": len(recoverable),
        "states_without_a_path_to_goal": len(seen - recoverable),
        "shortest_abstract_solution": trace,
    }

if __name__ == "__main__":
    report = {
        "scope": "Finite abstract resource/location/lock model only. No Unity, geometry, force, animation, touch or difficulty validation.",
        "full_model": explore(),
        "at_most_two_fragments": explore(2),
        "limits": [
            "Six-token mass quantization is illustrative, not a prescribed cut ratio.",
            "Routes and merges assume physical clearance and reachability.",
            "Door latches, safe partial-stroke recovery, pin actuation and pipe navigation are assumed, not proven.",
            "No gravity, rotation, timing, continuous contact or intentional premature-escape loss states.",
            "H uses six tokens for the intended solution; actual force thresholds must remain continuous and the merged-body win rule is separate.",
            "Shortest abstract action count is not player completion time or measured difficulty.",
        ],
    }
    output = Path(__file__).with_name("logic-audit.json")
    output.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n")
    print(json.dumps(report, ensure_ascii=False, indent=2))
