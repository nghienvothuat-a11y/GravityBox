"""Small paper-design checks. These deliberately do not claim Unity solvability."""
from collections import deque, defaultdict
from pathlib import Path
import json

def explore(start, successors, goal):
    q=deque([start]);parent={start:None};edge={};reverse=defaultdict(set);goals=[]
    while q:
        state=q.popleft()
        if goal(state):goals.append(state)
        for action,target in successors(state):
            reverse[target].add(state)
            if target not in parent:
                parent[target]=state;edge[target]=action;q.append(target)
    recoverable=set(goals);q=deque(goals)
    while q:
        for source in reverse[q.popleft()]:
            if source not in recoverable:recoverable.add(source);q.append(source)
    path=[]
    if goals:
        state=goals[0]
        while parent[state] is not None:path.append(edge[state]);state=parent[state]
        path.reverse()
    return {'reachable_states':len(parent),'goal_states':len(goals),'states_without_path_to_goal':len(parent.keys()-recoverable),'shortest_abstract_actions':path}

def level34(s):
    bridge,block,gear,door=s
    if bridge=='out':yield 'Thu B',('in',block,gear,door)
    elif block=='cross':yield 'Trả B',('out',block,gear,door)
    if block=='cross' and bridge=='in':yield 'Cất X', (bridge,'pocket',gear,door)
    if block=='pocket':yield 'Trả X', (bridge,'cross',gear,door)
    if block=='pocket':
        if gear=='start':yield 'Đẩy G qua giao điểm, E chốt',(bridge,block,'dock',True)
        else:yield 'Rút G',(bridge,block,'start',door)

def stations(n):
    def successors(s):
        places,stage=s
        for i in range(n):
            for target in 'ABCR':
                if places[i]!=target:
                    p=list(places);p[i]=target
                    yield f'Phần {i+1} tới {target}',(tuple(p),stage)
        if all(x in places for x in 'ABC'):
            if stage==0:yield 'C vận hành I; D1/D2 chốt',(places,1)
            if stage==1:yield 'C vận hành II; E chốt',(places,2)
    return explore((tuple('R'*n),0),successors,lambda s:s[1]==2 and all(p=='R' for p in s[0]))

r={
 'scope':'Abstract dependencies only; no geometry, force, timing, cutting, body stretch or input model. No reset edge used. R is a safe reunion location. Station bodies may move independently via assumed two-way transfer paths.',
 'level34':explore(('out','cross','start',False),level34,lambda s:s[0]=='out' and s[1]=='cross' and s[2]=='dock' and s[3]),
 'level39_three_roles':stations(3),
 'level39_two_roles':stations(2),
 'limits':'Level 39 model checks need for three distinct simultaneous work sites under the explicit assumption one body can occupy only one site. It does not establish that real soft tissue cannot span sites. Level 40 reuses this dependency but is not exhaustively modeled. No claim about all ten levels being solved in Unity.'
}
assert r['level34']['states_without_path_to_goal']==0
assert len(r['level34']['shortest_abstract_actions'])==5
assert r['level39_three_roles']['states_without_path_to_goal']==0
assert r['level39_two_roles']['goal_states']==0
(Path(__file__).parent/'logic-audit.json').write_text(json.dumps(r,ensure_ascii=False,indent=2)+'\n')
print(json.dumps(r,ensure_ascii=False,indent=2))
