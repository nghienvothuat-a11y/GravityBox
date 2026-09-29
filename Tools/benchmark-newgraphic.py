#!/usr/bin/env python3
"""Run the existing V2 touch author replay on an authorized Android device.
The APK must be built with COGHE_BENCHMARK=1. No installation, save deletion or
permission bypass is performed. A/B uses the same APK and reloads each scene.
"""
import argparse, re, subprocess, time
from pathlib import Path
p=argparse.ArgumentParser()
p.add_argument('--adb',default=str(Path.home()/'Library/Android/sdk/platform-tools/adb'))
p.add_argument('--serial',required=True)
p.add_argument('--profiles',default='current,blender,current,blender,current,blender')
p.add_argument('--output',default='Artifacts/COgheNewGraphic/Android')
a=p.parse_args();out=Path(a.output);out.mkdir(parents=True,exist_ok=True)
base=[a.adb,'-s',a.serial];package='com.gravityboxlab.venom'
def adb(*args):
 return subprocess.run(base+list(args),check=True,capture_output=True,text=True,timeout=30).stdout
if adb('get-state').strip()!='device':raise SystemExit('An authorized device is required.')
for item,args in {'device':['shell','getprop'],'display':['shell','wm','size'],'installed':['shell','dumpsys','package',package]}.items():
 (out/(item+'.txt')).write_text(adb(*args))
for index,profile in enumerate(a.profiles.split(','),1):
 if profile not in ('current','blender'):raise SystemExit('Unknown art profile')
 run=out/f'{index:02}-{profile}';run.mkdir(exist_ok=True)
 (run/'battery-before.txt').write_text(adb('shell','dumpsys','battery'))
 adb('shell','am','force-stop',package)
 with (run/'logcat.txt').open('w') as log:
  logger=subprocess.Popen(base+['logcat','-T','1','-v','threadtime','Unity:I','AndroidRuntime:E','*:S'],stdout=log,stderr=log)
  try:
   launch=adb('shell','am','start','-n',package+'/com.unity3d.player.UnityPlayerGameActivity','--es','coghe_view_proof','newgraphic','--ei','coghe_view_first','1','--ei','coghe_view_last','10','--es','coghe_graphic',profile)
   (run/'launch.txt').write_text(launch);print(f'START {index} {profile}',flush=True)
   start=time.monotonic();last_pss=0;seen=set();done=False
   while time.monotonic()-start<600:
    time.sleep(5)
    content=(run/'logcat.txt').read_text(errors='replace')
    for line in content.splitlines():
     if 'COGHE_VIEW_LEVEL ' in line and line not in seen:print(line,flush=True);seen.add(line)
    if 'COGHE_VIEW_DONE ' in content and 'COGHE_VIEW_LEVEL 10 ' in content:done=True;break
    if time.monotonic()-last_pss>=30:
     (run/f'pss-{int(time.monotonic()-start):03}.txt').write_text(adb('shell','dumpsys','meminfo',package));last_pss=time.monotonic()
   if not done:raise RuntimeError('Replay timed out; inspect logcat and screen before retrying.')
  finally:
   logger.terminate();logger.wait(timeout=10)
 (run/'battery-after.txt').write_text(adb('shell','dumpsys','battery'))
 print(f'DONE {index} {profile} {time.monotonic()-start:.0f}s',flush=True)
 # Preserve complete native proof reports, including any failed route and screenshots.
 native=re.findall(r'COGHE_VIEW_DONE .*directory=(\S+)',content)[-1]
 adb('pull',native,str(run/'proof'))
print('ALL REPLAYS COMPLETE',flush=True)
