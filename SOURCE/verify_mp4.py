# Check each mp4: streams, size, fps, duration, bitrate, full decode, faststart (moov before mdat), no metadata tags.
import sys, json, subprocess
FF, FP = '/opt/homebrew/bin/ffmpeg', '/opt/homebrew/bin/ffprobe'
for p in sys.argv[1:]:
    j = json.loads(subprocess.run([FP, '-v', 'error', '-show_format', '-show_streams', '-of', 'json', p], capture_output=True, text=True).stdout)
    v = [s for s in j['streams'] if s['codec_type'] == 'video'][0]; a = [s for s in j['streams'] if s['codec_type'] == 'audio']
    dec = subprocess.run([FF, '-v', 'error', '-i', p, '-f', 'null', '-'], capture_output=True, text=True).stderr.strip()
    head = open(p, 'rb').read(4096); fast = head.find(b'moov') != -1 and (head.find(b'mdat') == -1 or head.find(b'moov') < head.find(b'mdat'))
    tags = {k: v2 for k, v2 in j['format'].get('tags', {}).items() if k not in ('major_brand', 'minor_version', 'compatible_brands')}
    print(f"{p.split('/')[-1]}: {v['width']}x{v['height']} {v['codec_name']}/{v.get('profile')} {v['r_frame_rate']} "
          f"{float(j['format']['duration']):.2f}s {int(j['format']['bit_rate'])/1e6:.1f}Mbps audio={a[0]['codec_name'] + '/' + a[0]['sample_rate'] + '/' + str(a[0]['channels']) + 'ch/' + str(int(a[0].get('bit_rate', 0))//1000) + 'k' if a else 'none'} "
          f"decode={'ok' if not dec else dec[:80]} faststart={fast} tags={tags or 'none'}")
