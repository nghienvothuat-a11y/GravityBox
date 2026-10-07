"""Difficulty of the 60 play positions (Mrk, 06/10/2026: "vẽ biểu đồ độ khó hiện tại của 60 màn và đưa ra những đề xuất sắp
xếp lại levels"). Reads Artifacts/Metrics/levels.jsonl, written by the Explicit PlayMode test MeasureLevelMetrics, which plays
each level's reference solution in the game."""
import json

# Player-facing devices (the code's mechanism classes; rails, decks and sensors are parts of these).
CONCEPTS = {'COgheTapRail': 'tay nắm', 'COgheQuantumSplitter': 'máy Q (chia)', 'COgheTubeNetwork': 'ống', 'COghePassengerLift': 'thang',
            'COgheSwingTransfer': 'dây đu', 'COgheSeesawBridge': 'bập bênh', 'COgheGearTrain': 'bánh răng', 'COgheLoadLatch': 'khoá cân',
            'COgheTurntable': 'bàn xoay', 'COghePulleyDrive': 'ròng rọc', 'COghePropSocket': 'thùng rời'}


def load(path='../../PLANS/difficulty-60/levels.jsonl'):
    rows = {}
    for line in open(path):
        r = json.loads(line); rows[r['pos']] = r          # a later line for a position replaces an earlier one
    out = []
    for pos in sorted(rows):
        r = rows[pos]
        ops = r['mechanismTaps'] + r['partSelections']
        walks = max(0, r['taps'] - ops)
        concepts = sorted({CONCEPTS[k] for k in r['mechanisms'] if k in CONCEPTS})
        parts = r['maxParts'] - 1
        score = ops + .5 * walks + parts + .5 * len(concepts)
        out.append(dict(pos=pos, key=r['key'], title=r['title'].split(' · ', 1)[1], ops=ops, walks=walks, parts=r['maxParts'],
                        concepts=concepts, seconds=r['seconds'], turn=r['turnDegrees'] > 0, score=round(score, 1), solved=r['solved']))
    return out


if __name__ == '__main__':
    for l in load():
        print(f"{l['pos']:2} {l['key']:4} {l['title'][:26]:26} ops {l['ops']:2} walks {l['walks']:2} parts {l['parts']} conc {len(l['concepts'])} t {l['seconds']:5} D {l['score']:5}  {', '.join(l['concepts'])}")
