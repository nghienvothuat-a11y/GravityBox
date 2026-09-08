#!/usr/bin/env python3
"""Read-only voxel topology audit of the generated SphereMaze prefab.
No Unity execution. Uses serialized OBB planks also used to bake collision meshes.
Over-approximate mode conservatively expands centre-space by half a voxel diagonal.
A no-leak result in that mode bounds any continuous path in the true OBB geometry.
Under-approximate mode keeps only voxels wholly clear of the inflated OBBs.
This audits topology, not whether bounded hand rotation can execute the route.

Requires Python 3 and NumPy. From the repository root:
    python3 Tools/audit-sphere-topology.py --mode over --step .003 --cuts
    python3 Tools/audit-sphere-topology.py --mode under --step .003
"""
from pathlib import Path
from datetime import datetime, timezone
import argparse, array, hashlib, json, math, re, time
import numpy as np

ROOT=Path(__file__).resolve().parents[1]
PREFAB=ROOT/'Assets/_Game/PhysicsLab/Prefabs/SphereMaze box.prefab'


def vec(text):
    values={k:float(v) for k,v in re.findall(r'([xyzw]):\s*([-+0-9.eE]+)',text)}
    return np.array([values[k] for k in ('xyzw' if 'w' in values else 'xyz')],dtype=float)


def field_vector(text,name):
    return vec(re.search(r'^  '+re.escape(name)+r': (\{[^\n]+\})$',text,re.M).group(1))


def rot(q):
    x,y,z,w=q/np.linalg.norm(q)
    return np.array([[1-2*(y*y+z*z),2*(x*y-z*w),2*(x*z+y*w)],
                     [2*(x*y+z*w),1-2*(x*x+z*z),2*(y*z-x*w)],
                     [2*(x*z-y*w),2*(y*z+x*w),1-2*(x*x+y*y)]])


def read_prefab():
    source=PREFAB.read_bytes()
    source_sha256=hashlib.sha256(source).hexdigest()
    text=source.decode('utf-8')
    chunks={int(m.group(2)):(int(m.group(1)),m.group(3)) for m in re.finditer(r'^--- !u!(\d+) &(\d+)\n(.*?)(?=^--- !u!|\Z)',text,re.M|re.S)}
    guid=re.search(r'^guid: (\w+)',(ROOT/'Assets/_Game/Scripts/Gameplay/SpatialMaze.cs.meta').read_text(),re.M).group(1)
    meta=next(body for typ,body in chunks.values() if typ==114 and 'guid: '+guid in body)
    radius=float(re.search(r'^  InnerRadius: (.+)$',meta,re.M).group(1))
    nodes=[vec(m.group(1)) for m in re.finditer(r'^  - (\{[^\n]+\})$',re.search(r'^  NodesLocal:\n(.*?)(?=^  \w)',meta,re.M|re.S).group(1),re.M)]
    mainstr=re.search(r'^  MainPath: (.*)$',meta,re.M).group(1)
    if mainstr.strip():
        main=list(np.frombuffer(bytes.fromhex(mainstr.strip()),dtype='<i4').astype(int))
    else:
        main=[int(x) for x in re.findall(r'^  - (\d+)$',re.search(r'^  MainPath:\n(.*?)(?=^  \w)',meta,re.M|re.S).group(1),re.M)]
    edges=[(int(m.group(1)),int(m.group(2))) for m in re.finditer(r'^  - A: (\d+)\n    B: (\d+)',meta,re.M)]
    planks=[]
    for m in re.finditer(r'^  - CentreLocal: (\{[^\n]+\})\n    Size: (\{[^\n]+\})\n    RotationLocal: (\{[^\n]+\})\n    SectionCollider: \{fileID: (\d+)\}',meta,re.M):
        planks.append((vec(m.group(1)),vec(m.group(2)),rot(vec(m.group(3))),int(m.group(4))))
    if not planks or not nodes or not main or not edges:
        raise RuntimeError('Connected maze metadata missing or could not be parsed.')
    runtime=next(body for typ,body in chunks.values() if typ==114 and '\n  BallSpawn:' in body)
    spawn_id=int(re.search(r'^  BallSpawn: \{fileID: (\d+)\}',runtime,re.M).group(1))
    def world_matrix(tid):
        body=chunks[tid][1]
        local=np.eye(4); local[:3,:3]=rot(field_vector(body,'m_LocalRotation'))@np.diag(field_vector(body,'m_LocalScale'))
        local[:3,3]=field_vector(body,'m_LocalPosition')
        parent=int(re.search(r'^  m_Father: \{fileID: (\d+)\}',body,re.M).group(1))
        return world_matrix(parent)@local if parent else local
    spawn=world_matrix(spawn_id)[:3,3]
    return radius,np.array(nodes),main,edges,planks,spawn,source_sha256


def build_grid(radius,planks,h,mode):
    pad=math.sqrt(3)*h/2
    clearance=.015-pad if mode=='over' else .015+pad if mode=='under' else .015
    extent=math.ceil((radius+.006)/h)*h
    xyz=np.arange(-extent,extent+h*.5,h,dtype=np.float64)
    free=((xyz[:,None,None]**2+xyz[None,:,None]**2+xyz[None,None,:]**2) <= (radius-.015+(pad if mode=='over' else -pad if mode=='under' else 0))**2)
    for centre,size,axes,_ in planks:
        half=size/2
        aabb=np.abs(axes)@half+clearance
        lo=np.maximum(0,np.ceil((centre-aabb-xyz[0])/h).astype(int)-1)
        hi=np.minimum(len(xyz),np.floor((centre+aabb-xyz[0])/h).astype(int)+2)
        if np.any(hi<=lo):continue
        xs=xyz[lo[0]:hi[0],None,None]-centre[0]
        ys=xyz[None,lo[1]:hi[1],None]-centre[1]
        zs=xyz[None,None,lo[2]:hi[2]]-centre[2]
        d2=np.zeros(tuple(hi-lo),dtype=float)
        for axis in range(3):
            delta=np.maximum(np.abs(xs*axes[0,axis]+ys*axes[1,axis]+zs*axes[2,axis])-half[axis],0)
            d2+=delta*delta
        free[lo[0]:hi[0],lo[1]:hi[1],lo[2]:hi[2]] &= (d2>=clearance**2)
    return xyz,free,pad


def closest_distance(points,nodes,edges):
    best=np.full(len(points),np.inf)
    for a,b in edges:
        delta=nodes[b]-nodes[a]
        t=np.clip(((points-nodes[a])@delta)/(delta@delta),0,1)
        best=np.minimum(best,np.linalg.norm(points-(nodes[a]+t[:,None]*delta),axis=1))
    return best


def coord_index(point,xyz,h):
    ijk=np.rint((point-xyz[0])/h).astype(int)
    return int(np.ravel_multi_index(ijk,(len(xyz),)*3))


def flood(free,start,goal=None,forbidden=None,parent=False):
    n=free.shape[0]; flat=memoryview(free.ravel()).cast('B')
    if not flat[start]: raise RuntimeError('Spawn/seed voxel is blocked.')
    seen=bytearray(free.size); seen[start]=1
    q=array.array('I',[start]); previous={start:-1} if parent else None
    offsets=[dx*n*n+dy*n+dz for dx in (-1,0,1) for dy in (-1,0,1) for dz in (-1,0,1) if dx or dy or dz]
    cursor=0; found=None
    while cursor<len(q):
        here=q[cursor];cursor+=1
        if (goal is not None and here==goal) or (forbidden is not None and forbidden[here]):found=here;break
        for offset in offsets:
            there=here+offset
            if flat[there] and not seen[there]:
                seen[there]=1;q.append(there)
                if parent:previous[there]=here
    path=[]
    if found is not None and parent:
        x=found
        while x!=-1:path.append(x);x=previous[x]
        path.reverse()
    return seen,q,found,path


def positions(indices,xyz):
    n=len(xyz); ids=np.asarray(indices,dtype=np.int64)
    return np.column_stack((xyz[ids//(n*n)],xyz[(ids//n)%n],xyz[ids%n]))


def main():
    parser=argparse.ArgumentParser();parser.add_argument('--step',type=float,default=.003)
    parser.add_argument('--mode',choices=['over','under','exact'],default='over');parser.add_argument('--cuts',action='store_true')
    args=parser.parse_args()
    if not (0 < args.step <= .005):
        parser.error('--step must be greater than zero and no more than 0.005 metres.')
    started_utc=datetime.now(timezone.utc).isoformat(timespec='seconds')
    t0=time.time()
    radius,nodes,route,edges,planks,spawn,prefab_sha256=read_prefab()
    xyz,free,pad=build_grid(radius,planks,args.step,args.mode)
    start=coord_index(spawn,xyz,args.step)
    goal_point=np.array([0,-radius+.022,0]);goal=coord_index(goal_point,xyz,args.step)
    # Outer empty volume clearly outside all authored corridor/hub envelopes.
    candidates=np.flatnonzero(free.ravel())
    outer=bytearray(free.size)
    for lo in range(0,len(candidates),200000):
        ids=candidates[lo:lo+200000];points=positions(ids,xyz)
        mask=(np.linalg.norm(points,axis=1)>.295)&(closest_distance(points,nodes,edges)>.060)
        for idx in ids[mask]:outer[int(idx)]=1
    seen,component,leak,path=flood(free,start,forbidden=outer,parent=True)
    report={'prefab':str(PREFAB),'prefab_sha256':prefab_sha256,'audit_started_utc':started_utc,
            'step_m':args.step,'mode':args.mode,'half_voxel_diagonal_m':pad,
            'effective_ball_radius_m':.015-pad if args.mode=='over' else .015+pad if args.mode=='under' else .015,
            'planks':len(planks),'sections':len(set(x[3] for x in planks)), 'graph_nodes':len(nodes),'edges':len(edges),
            'main_nodes':list(map(int,route)),'spawn':spawn.tolist(),'goal':goal_point.tolist(),
            'spawn_component_voxels_examined':len(component),'outer_leak':leak is not None,
            'goal_reachable':bool(seen[goal]),'cuts_requested':args.cuts,
            'expected_main_path_legs':len(route)-1,'cuts':[]}
    if leak is not None:
        coords=positions(path,xyz); d=closest_distance(coords,nodes,edges)
        first=np.flatnonzero(d>.047)
        report['first_outside_framework']=coords[first[0]].tolist() if len(first) else None
        report['leak_endpoint']=coords[-1].tolist()
        report['leak_path']=coords[::max(1,len(coords)//160)].tolist()+[coords[-1].tolist()]
    else:
        report['component_max_radius_m']=float(np.max(np.linalg.norm(positions(component,xyz),axis=1)))
        if args.cuts:
            for a,b in zip(route[:-1],route[1:]):
                mid=(nodes[a]+nodes[b])/2;axis=int(np.argmax(np.abs(nodes[b]-nodes[a])))
                half=np.ones(3)*.043;half[axis]=args.step*1.6
                lo=np.maximum(0,np.ceil((mid-half-xyz[0])/args.step).astype(int))
                hi=np.minimum(len(xyz),np.floor((mid+half-xyz[0])/args.step).astype(int)+1)
                if np.any(hi<=lo):continue
                slices=tuple(slice(x,y) for x,y in zip(lo,hi));saved=free[slices].copy();free[slices]=False
                _,q,found,_=flood(free,start,goal=goal)
                report['cuts'].append({'edge':[int(a),int(b)],'exit_still_reachable':found is not None,'voxels_examined':len(q)})
                free[slices]=saved
    report['elapsed_seconds']=time.time()-t0
    report['audit_finished_utc']=datetime.now(timezone.utc).isoformat(timespec='seconds')
    report['scope']='Topology of the generated prefab\'s serialized OBB collision geometry to an interior exit-approach point. This does not simulate dynamic motion or final passage through the spherical bore, and does not establish hand-controller solvability or human difficulty.'
    failures=[]
    if report['outer_leak']:
        failures.append('The spawn component reaches the outer free volume.')
    if not report['goal_reachable']:
        failures.append('The interior exit approach is unreachable from spawn.')
    if args.cuts:
        if len(report['cuts']) != report['expected_main_path_legs']:
            failures.append('The number of evaluated cuts differs from the main path leg count.')
        if any(cut['exit_still_reachable'] for cut in report['cuts']):
            failures.append('At least one blocked main passage has an alternate route to the exit approach.')
    report['checks_failed']=failures
    report['passed']=not failures
    dest=ROOT/'Artifacts'/f'connected-sphere-topology-{args.mode}-{int(args.step*1000)}mm.json'
    dest.parent.mkdir(parents=True,exist_ok=True)
    dest.write_text(json.dumps(report,indent=2))
    print(json.dumps({k:v for k,v in report.items() if k not in ('leak_path','main_nodes','cuts')},indent=2))
    print('Mandatory cuts:',sum(not x['exit_still_reachable'] for x in report['cuts']),'/',len(report['cuts']))
    print('Report:',dest)
    return 1 if failures else 0

if __name__=='__main__':raise SystemExit(main())
