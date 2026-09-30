#!/usr/bin/env python3
"""COghe audio, first version: one background track, a lab room tone and a small set of sound effects.

Everything is synthesized here so it can be tuned and regenerated; any file can be replaced by a recorded or
composed one with the same name (see Docs/Audio/COghe/README.md). Mood: chill, deep, a little lonely in a quiet lab;
the creature is small, curious, playful and friendly.

Usage: python3 Tools/audio/synth_coghe_audio.py [out_dir] [--music] [--intro]   (needs numpy, scipy, soundfile)
--music also rebuilds the background loop, --intro the 20 s intro score, --personality the sounds of COghe's acts
(otherwise the committed ones are kept).
Default out_dir: Assets/_Game/Venom/Resources/COgheAudio
"""
import os, sys
import numpy as np
import soundfile as sf
from scipy import signal

SR = 44100
RNG = np.random.default_rng(20260930)
ARGS = [a for a in sys.argv[1:] if not a.startswith("--")]
OUT = ARGS[0] if ARGS else os.path.join(os.path.dirname(__file__), "../../Assets/_Game/Venom/Resources/COgheAudio")
os.makedirs(OUT, exist_ok=True)


# ---------------------------------------------------------------------------------------------------------------------
# Building blocks
def t_axis(seconds):
    return np.arange(int(round(seconds * SR))) / SR

def midi(n):
    return 440.0 * 2 ** ((n - 69) / 12)

def env(n, attack, release, hold=None, curve=3.0):
    """Smooth attack / exponential-ish release envelope over n samples (seconds for attack/release)."""
    e = np.ones(n)
    a = max(1, int(attack * SR)); r = max(1, int(release * SR))
    a = min(a, n); e[:a] = (np.linspace(0, 1, a)) ** 1.6
    if hold is not None:
        start = min(n, int(hold * SR)); r = n - start
    else:
        start = max(a, n - r); r = n - start
    if r > 0:
        x = np.linspace(0, 1, r)
        e[start:] *= np.exp(-curve * x) * (1 - x)
    return e

def decay(n, seconds):
    return np.exp(-np.arange(n) / (seconds * SR))

def glide(n, f0, f1, shape=1.0):
    x = np.linspace(0, 1, n) ** shape
    return f0 * (f1 / f0) ** x

def osc(freq):
    """Sine oscillator following a frequency array (or scalar with n samples via np.full)."""
    return np.sin(2 * np.pi * np.cumsum(freq) / SR)

def lowpass(x, cutoff, order=2):
    sos = signal.butter(order, min(cutoff, SR * 0.45), "low", fs=SR, output="sos"); return signal.sosfilt(sos, x, axis=0)

def highpass(x, cutoff, order=2):
    sos = signal.butter(order, cutoff, "high", fs=SR, output="sos"); return signal.sosfilt(sos, x, axis=0)

def bandpass(x, lo, hi, order=2):
    sos = signal.butter(order, [lo, min(hi, SR * 0.45)], "band", fs=SR, output="sos"); return signal.sosfilt(sos, x, axis=0)

def swept_bandpass(noise, centers, q=4.0, block=256):
    """Band-pass noise whose centre follows `centers` (per sample), processed in short blocks."""
    out = np.zeros_like(noise); zi = None
    for s in range(0, len(noise), block):
        c = float(np.clip(centers[min(s + block // 2, len(centers) - 1)], 60, SR * 0.4))
        b, a = signal.iirpeak(c, q, fs=SR)
        if zi is None: zi = signal.lfilter_zi(b, a) * 0
        out[s:s + block], zi = signal.lfilter(b, a, noise[s:s + block], zi=zi)
    return out

def swept_lowpass(x, cutoffs, block=512):
    """Low-pass whose cutoff follows `cutoffs` (per sample); filter state carries across blocks."""
    out = np.zeros_like(x); zi = np.zeros((1, 2))
    for s in range(0, len(x), block):
        c = float(np.clip(cutoffs[min(s + block // 2, len(cutoffs) - 1)], 40, SR * 0.45))
        sos = signal.butter(2, c, "low", fs=SR, output="sos")
        out[s:s + block], zi = signal.sosfilt(sos, x[s:s + block], zi=zi)
    return out

def circular(fn, x):
    """Apply a filter to a loop as if it repeated forever: filter three copies and keep the middle one."""
    n = len(x); y = fn(np.concatenate([x, x, x], axis=0)); return y[n:2 * n]

def resonator(x, freq, q):
    b, a = signal.iirpeak(freq, q, fs=SR); return signal.lfilter(b, a, x)

def noise(n):
    return RNG.standard_normal(n)

def brown(n):
    b = np.cumsum(RNG.standard_normal(n)); b -= np.linspace(b[0], b[-1], n); return b / (np.max(np.abs(b)) + 1e-9)

def impulse_response(seconds, stereo=True, damp=4000.0, pre=0.012):
    """Synthetic room: decaying noise, darker as it decays, a short pre-delay."""
    n = int(seconds * SR); t = np.arange(n) / SR
    chans = []
    for c in range(2 if stereo else 1):
        bright = noise(n) * np.exp(-6.9 * t / (seconds * 0.55))
        dark = lowpass(noise(n), 1400) * np.exp(-6.9 * t / seconds)
        ir = lowpass(bright, damp) * 0.6 + dark * 1.4
        ir[: int(pre * SR)] = 0
        chans.append(ir / np.sqrt(np.sum(ir ** 2)))
    return np.stack(chans, axis=1) if stereo else chans[0]

def reverb(x, seconds, wet, damp=4000.0):
    """x mono or stereo; returns stereo (len + tail)."""
    if x.ndim == 1: x = np.stack([x, x], axis=1)
    ir = impulse_response(seconds, damp=damp)
    tail = len(ir)
    out = np.zeros((len(x) + tail, 2))
    out[: len(x)] += x * (1 - wet)
    for c in range(2):
        conv = signal.fftconvolve(x[:, c], ir[:, c]); out[: len(conv), c] += conv * wet * 1.2
    return out

def mono_reverb(x, seconds, wet, damp=4500.0):
    return reverb(x, seconds, wet, damp).mean(axis=1)

def soft_clip(x, drive=1.0):
    return np.tanh(x * drive) / np.tanh(drive)

def fade_edges(x, fin=0.002, fout=0.01):
    n = len(x); a = min(n, int(fin * SR)); b = min(n, int(fout * SR))
    w = np.ones(n); w[:a] = np.linspace(0, 1, a); w[n - b:] = np.linspace(1, 0, b)
    return x * (w if x.ndim == 1 else w[:, None])

def trim_silence(x, threshold=1e-4):
    mag = np.abs(x) if x.ndim == 1 else np.abs(x).max(axis=1)
    idx = np.where(mag > threshold)[0]
    return x[: idx[-1] + 1] if len(idx) else x

def normalize(x, peak_db=-3.0):
    p = np.max(np.abs(x)) + 1e-12; return x * (10 ** (peak_db / 20) / p)

def place(buf, x, at):
    """Mix x into buf starting at sample `at` (wrapping around the end: buffers here are loops)."""
    n = len(buf)
    for i in range(0, len(x), n):
        chunk = x[i:i + n]; s = (at + i) % n; e = s + len(chunk)
        if e <= n: buf[s:e] += chunk
        else: k = n - s; buf[s:] += chunk[:k]; buf[: e - n] += chunk[k:]

def write_ogg(name, x):
    """Vorbis via libsndfile, written in blocks (one large write crashes libsndfile's Vorbis encoder)."""
    x = np.ascontiguousarray(x.astype(np.float32))
    with sf.SoundFile(os.path.join(OUT, name + ".ogg"), "w", SR, 1 if x.ndim == 1 else x.shape[1], format="OGG", subtype="VORBIS") as f:
        for s in range(0, len(x), 16384): f.write(x[s:s + 16384])

def save(name, x, peak_db=-3.0):
    x = normalize(fade_edges(trim_silence(x)), peak_db)
    write_ogg(name, x)
    return x

def save_loop(name, x, peak_db=-3.0):
    """Loops: no edge fades (the tail is already wrapped), normalized only."""
    x = normalize(x, peak_db)
    write_ogg(name, x)
    return x


# ---------------------------------------------------------------------------------------------------------------------
# Instruments
def felt_piano(freq, seconds, vel=0.6):
    n = int(seconds * SR); t = np.arange(n) / SR
    out = np.zeros(n)
    for k, (amp, dec) in enumerate([(1.0, 2.4), (0.42, 1.3), (0.2, 0.8), (0.1, 0.5), (0.05, 0.35)], start=1):
        f = freq * k * (1 + 0.0004 * k * k)
        out += amp * np.sin(2 * np.pi * f * t + RNG.uniform(0, 6.28)) * np.exp(-t / dec)
    out *= (1 - np.exp(-t / 0.006))                      # soft hammer
    thump = lowpass(noise(n), 700) * np.exp(-t / 0.018) * 0.08
    out = lowpass(out + thump, 1800 + 1400 * vel)
    return out * vel * env(n, 0.004, 0.25)

def celesta(freq, seconds, vel=0.5):
    n = int(seconds * SR); t = np.arange(n) / SR
    mod = np.sin(2 * np.pi * freq * 3.5 * t) * 1.2 * np.exp(-t / 0.18)
    tone = np.sin(2 * np.pi * freq * t + mod) * np.exp(-t / 1.6)
    tone += 0.25 * np.sin(2 * np.pi * freq * 2 * t) * np.exp(-t / 0.6)
    tone *= (1 - np.exp(-t / 0.002))
    return lowpass(tone, 5200) * vel

def glass(freq, seconds, vel=0.3):
    n = int(seconds * SR); t = np.arange(n) / SR
    out = sum(a * np.sin(2 * np.pi * freq * r * t) * np.exp(-t / d) for r, a, d in [(1, 1, 1.2), (2.76, 0.35, 0.6), (5.4, 0.12, 0.25)])
    return out * (1 - np.exp(-t / 0.003)) * vel

def pad_note(freq, seconds, vel=0.3, attack=2.2, release=2.5, bright=750):
    n = int(seconds * SR); t = np.arange(n) / SR
    out = np.zeros(n)
    for detune in (-0.0045, 0.0, 0.005):
        f = freq * (1 + detune); kmax = max(1, int(3200 / f))
        ph = RNG.uniform(0, 6.28)
        for k in range(1, kmax + 1):
            out += np.sin(2 * np.pi * f * k * t + ph * k) / k
    lfo = 0.5 + 0.5 * np.sin(2 * np.pi * 0.07 * t + RNG.uniform(0, 6.28))
    out = swept_lowpass(out, bright * (0.8 + 0.4 * lfo))
    return out * vel * env(n, attack, release) / 3

def voice(f0_curve, seconds, formants, breath=0.03, vib=0.018, nasal=False):
    """The creature: a small wordless voice. f0_curve: array of pitch per sample; formants: list of (freq, q, gain)
    pairs or a callable giving them over time (start->end interpolation)."""
    n = int(seconds * SR); t = np.arange(n) / SR
    f0 = np.interp(np.linspace(0, 1, n), np.linspace(0, 1, len(f0_curve)), f0_curve)
    f0 = f0 * (1 + vib * np.sin(2 * np.pi * 6.5 * t) * np.clip(t / 0.08, 0, 1))
    phase = 2 * np.pi * np.cumsum(f0) / SR
    src = sum(np.sin(k * phase) / (k ** 1.8) for k in range(1, 7))   # round, not buzzy
    src += breath * lowpass(noise(n), 5000)
    start, end = formants
    out = np.zeros(n); block = 441
    for s in range(0, n, block):
        u = s / n
        seg = np.zeros(min(block, n - s))
        for (fa, qa, ga), (fb, qb, gb) in zip(start, end):
            b, a = signal.iirpeak(fa + (fb - fa) * u, qa + (qb - qa) * u, fs=SR)
            seg += (ga + (gb - ga) * u) * signal.lfilter(b, a, src[s:s + block])
        out[s:s + block] = seg
    if nasal: out = lowpass(out, 1100) + 0.25 * out
    return lowpass(out, 3800) * env(n, 0.018, 0.07)

U = [(380, 4, 1.0), (950, 6, 0.5)]            # "u"
I = [(330, 4, 0.8), (2400, 8, 0.6)]           # "i"
A = [(800, 4, 1.0), (1300, 6, 0.6)]           # "a"
M = [(280, 3, 1.0), (1000, 8, 0.15)]          # closed mouth "m"
O = [(520, 4, 1.0), (900, 6, 0.5)]            # "o"


# ---------------------------------------------------------------------------------------------------------------------
# Background music: one ambient loop (72 BPM, D major / B minor with a Lydian colour)
def music():
    global RNG
    RNG = np.random.default_rng(7272)   # its own stream: changing the effects never changes the music
    bpm = 72; beat = 60 / bpm; chord_len = 8 * beat
    chords = [  # (bass, upper voices) MIDI
        (38, [50, 57, 61, 64, 66]),   # Dmaj9
        (35, [54, 57, 61, 62, 66]),   # Bm9
        (31, [50, 54, 59, 61, 66]),   # Gmaj7#11
        (40, [52, 59, 62, 66, 67]),   # Em9
        (42, [50, 57, 61, 64, 66]),   # Dmaj9/F#
        (35, [52, 57, 62, 64, 66]),   # Bm11
        (31, [50, 54, 57, 59, 62]),   # Gmaj9
        (33, [52, 57, 59, 64, 66]),   # A6sus2
    ]
    cycles = 3
    total = cycles * len(chords) * chord_len
    n = int(total * SR); tail = int(6 * SR)
    L = np.zeros((n + tail, 2))

    def add(x, at_s, pan=0.0, gain=1.0):
        s = int(at_s * SR); x = x * gain
        if s < 0: x = x[-s:]; s = 0
        l = x * np.cos((pan + 1) * np.pi / 4); r = x * np.sin((pan + 1) * np.pi / 4)
        e = min(len(L), s + len(x)); L[s:e, 0] += l[: e - s]; L[s:e, 1] += r[: e - s]

    motif = [74, 78, 81, 80]   # D5 F#5 A5 G#5: the creature's curious question
    for cyc in range(cycles):
        for ci, (bass, upper) in enumerate(chords):
            t0 = (cyc * len(chords) + ci) * chord_len
            # pad: whole chord, long overlap
            for j, m in enumerate(upper):
                add(pad_note(midi(m), chord_len + 2.6, vel=0.17, attack=2.0, release=2.6), t0, pan=-0.5 + j * 0.25, gain=1.0)
            # sub bass: very soft sine
            sub = np.sin(2 * np.pi * midi(bass) * t_axis(chord_len + 1.5)) * env(int((chord_len + 1.5) * SR), 1.2, 1.5) * 0.12
            add(lowpass(sub, 160), t0, 0.0)
            # felt piano: sparse broken chord; the last cycle is thinner (the lonely part)
            pattern = [0, 1.5, 3, 4.5, 6] if cyc < 2 else [0, 3, 6]
            if cyc == 0 and ci < 2: pattern = [0, 3, 6]
            tones = [bass + 12] + upper
            for k, b in enumerate(pattern):
                m = tones[(k * 2 + ci) % len(tones)] + (12 if k == len(pattern) - 1 and ci % 2 else 0)
                jitter = RNG.normal(0, 0.012)
                add(felt_piano(midi(m), 3.2, vel=RNG.uniform(0.35, 0.6)), t0 + b * beat + jitter, pan=RNG.uniform(-0.35, 0.35), gain=0.55)
            # motif on celesta in the middle cycle, an echo of its first two notes in the last
            if cyc == 1 and ci % 2 == 0:
                for k, m in enumerate(motif):
                    add(celesta(midi(m), 2.4, vel=0.33), t0 + (2 + k) * beat, pan=0.3, gain=1.0)
            if cyc == 2 and ci in (1, 5):
                for k, m in enumerate(motif[:2]):
                    add(celesta(midi(m + 12), 2.4, vel=0.22), t0 + (4 + k * 1.5) * beat, pan=-0.3)
            # glass shimmer, rare
            if (cyc, ci) in [(0, 6), (1, 3), (2, 0), (2, 6)]:
                add(glass(midi(upper[-1] + 24), 3.0, vel=0.08), t0 + 5 * beat, pan=0.6)
    # air: a faint breath under everything
    air = lowpass(highpass(noise(n + tail), 300), 3500) * 0.004
    L[:, 0] += air; L[:, 1] += np.roll(air, 1234)
    wet = lowpass(reverb(L, 3.2, 0.32, damp=3500), 9000)
    # loop: fold everything past the end back onto the start (filters already applied, so the seam stays continuous)
    out = wet[:n].copy(); rest = wet[n:]
    for c in range(2): place(out[:, c], rest[:, c], 0)
    out = soft_clip(out * 1.1, 1.2)
    rms = np.sqrt(np.mean(out ** 2)); out *= 10 ** (-20 / 20) / rms    # ~-20 dBFS RMS
    peak = np.max(np.abs(out))
    if peak > 10 ** (-1 / 20): out *= 10 ** (-1 / 20) / peak
    write_ogg("music_lab_loop", out)
    return out


# ---------------------------------------------------------------------------------------------------------------------
# Sound effects (mono unless stated)
def small_room(x, wet=0.14):
    return mono_reverb(x, 0.7, wet)

def sfx():
    out = {}
    # UI: a soft glass tick
    n = int(0.12 * SR); t = np.arange(n) / SR
    x = (np.sin(2 * np.pi * 1760 * t) + 0.35 * np.sin(2 * np.pi * 2640 * t)) * np.exp(-t / 0.028)
    x += lowpass(noise(n), 5000) * np.exp(-t / 0.002) * 0.15
    out["ui_tap"] = save("ui_tap", lowpass(x, 6000), -8)

    # Creature voice: acknowledgement "u?", three variants
    for i, (curve, dur, v) in enumerate([([560, 600, 790], 0.19, (U, I)), ([620, 700, 690, 840], 0.21, (O, I)), ([520, 560, 760], 0.17, (U, A))], start=1):
        out[f"creature_ack_{i}"] = save(f"creature_ack_{i}", small_room(voice(np.array(curve), dur, v)), -6)
    out["creature_happy"] = save("creature_happy", small_room(np.concatenate([
        voice(np.array([600, 880, 900]), 0.13, (A, I)), np.zeros(int(0.035 * SR)), voice(np.array([760, 1100, 1180]), 0.19, (O, I))]), 0.18), -5)
    out["creature_curious_1"] = save("creature_curious_1", small_room(voice(np.array([430, 400, 390, 470, 580]), 0.42, (M, U), nasal=True), 0.2), -7)
    out["creature_curious_2"] = save("creature_curious_2", small_room(np.concatenate([
        voice(np.array([700, 760]), 0.09, (U, I)), np.zeros(int(0.06 * SR)), voice(np.array([760, 820]), 0.08, (U, I))]), 0.2), -8)
    out["creature_hm"] = save("creature_hm", small_room(voice(np.array([470, 420, 400, 450]), 0.32, (M, M), nasal=True), 0.15), -8)

    # Crawl: squishy steps, 2 s loop. Not used since 30/09/2026 (Mrk: no crawling sound); still computed so the random
    # stream, and with it every effect generated after it, stays exactly as approved.
    n = 2 * SR; buf = np.zeros(n)
    for k in range(14):
        m = int(0.11 * SR); tt = np.arange(m) / SR
        c = RNG.uniform(500, 1050)       # phone speakers start around 250 Hz: keep the squish above it
        squish = swept_bandpass(noise(m), glide(m, c * 1.5, c * 0.6), q=5.0) * np.exp(-tt / 0.04) * (1 - np.exp(-tt / 0.008))
        blub = np.sin(2 * np.pi * np.cumsum(glide(m, 440, 260)) / SR) * np.exp(-tt / 0.03) * (1 - np.exp(-tt / 0.006)) * 0.5
        place(buf, (squish * 0.8 + blub) * RNG.uniform(0.6, 1.0), int((k / 14 + RNG.normal(0, 0.008)) * n))
    circular(lambda v: highpass(lowpass(v, 3000), 200), buf)

    # Land: a soft plop
    n = int(0.3 * SR); t = np.arange(n) / SR
    thump = np.sin(2 * np.pi * np.cumsum(glide(n, 420, 190, 0.5)) / SR) * np.exp(-t / 0.06)
    splash = bandpass(noise(n), 400, 1600) * np.exp(-t / 0.035) * 0.45
    blub = np.sin(2 * np.pi * np.cumsum(glide(n, 560, 320)) / SR) * np.exp(-t / 0.04) * 0.3
    out["creature_land"] = save("creature_land", small_room(thump + splash + blub), -5)

    # Grab: a small squeeze and a closed-mouth "mm"
    n = int(0.16 * SR); t = np.arange(n) / SR
    sq = bandpass(noise(n), 500, 1300) * (np.exp(-((t - 0.03) / 0.018) ** 2) + 0.7 * np.exp(-((t - 0.09) / 0.02) ** 2))
    mm = voice(np.array([330, 360, 340]), 0.16, (M, M), nasal=True)
    out["creature_grab"] = save("creature_grab", small_room(sq * 0.5 + mm * 0.8), -7)

    # Split: an elastic stretch, then a pop into two
    n = int(0.28 * SR); t = np.arange(n) / SR
    f = glide(n, 260, 720, 1.3)
    stretch = np.sin(2 * np.pi * np.cumsum(f) / SR + 1.4 * np.exp(-t / 0.12) * np.sin(2 * np.pi * np.cumsum(f * 2) / SR)) * env(n, 0.03, 0.03)
    m = int(0.05 * SR); pop = bandpass(noise(m), 900, 2600) * np.exp(-np.arange(m) / SR / 0.006)
    x = np.concatenate([stretch * 0.7, pop * 0.9, np.zeros(int(0.1 * SR))])
    out["creature_split"] = save("creature_split", small_room(x, 0.2), -5)

    # Merge: two bubbles; a whole body again: a warm three-note chime
    def bubble(f0, f1, dur):
        m = int(dur * SR); tt = np.arange(m) / SR
        return np.sin(2 * np.pi * np.cumsum(glide(m, f0, f1, 0.6)) / SR) * np.exp(-tt / (dur * 0.45)) * (1 - np.exp(-tt / 0.003))
    out["creature_merge"] = save("creature_merge", small_room(np.concatenate([bubble(900, 380, 0.11), np.zeros(int(0.02 * SR)), bubble(680, 420, 0.09) * 0.7, np.zeros(int(0.12 * SR))]), 0.2), -6)
    x = np.zeros(int(1.8 * SR))
    for k, m in enumerate([86, 90, 93]):
        c = celesta(midi(m), 1.6, 0.5); s = int(k * 0.06 * SR); x[s:s + len(c)] += c[: len(x) - s]
    out["creature_merge_full"] = save("creature_merge_full", reverb(x, 1.4, 0.3), -6)

    # Tube: in (a slurp up through glass), out (a pop and a bubble)
    n = int(0.45 * SR); t = np.arange(n) / SR
    slurp = swept_bandpass(noise(n), glide(n, 380, 2300, 1.4), q=2.5) * env(n, 0.06, 0.12)
    ring = glass(1318, 0.9, 0.15)[:n] * 0.6
    out["tube_in"] = save("tube_in", mono_reverb(slurp + ring, 0.9, 0.22), -6)
    m = int(0.02 * SR); pop = bandpass(noise(m), 800, 3000) * np.exp(-np.arange(m) / SR / 0.004)
    x = np.concatenate([pop, bubble(300, 700, 0.09) * 0.8, np.zeros(int(0.15 * SR))]); x[: len(glass(1568, 0.5, 0.1))] += glass(1568, 0.5, 0.1)[: len(x)]
    out["tube_out"] = save("tube_out", small_room(x, 0.2), -6)

    # Swing: a whoosh and a tiny "wee"
    n = int(0.65 * SR)
    wh = swept_bandpass(noise(n), np.concatenate([glide(n // 2, 300, 1400), glide(n - n // 2, 1400, 500)]), q=1.6) * env(n, 0.12, 0.25)
    wee = np.zeros(n); v = voice(np.array([700, 950, 1050]), 0.22, (I, I)); wee[int(0.08 * SR): int(0.08 * SR) + len(v)] += v * 0.45
    out["creature_swing"] = save("creature_swing", small_room(wh * 0.8 + wee, 0.15), -6)

    # Exit: "wheee" and the suction of the outlet
    n = int(0.8 * SR)
    suck = swept_bandpass(noise(n), glide(n, 250, 1800, 1.5), q=2.0) * env(n, 0.2, 0.3)
    whee = np.zeros(n); v = voice(np.array([620, 800, 1150, 1250]), 0.5, (A, I)); whee[: len(v)] += v
    out["creature_exit"] = save("creature_exit", small_room(suck * 0.6 + whee, 0.18), -5)

    # Mechanisms
    n = int(0.12 * SR); t = np.arange(n) / SR
    tick = bandpass(noise(n), 2200, 5000) * np.exp(-t / 0.0025)
    body = np.sin(2 * np.pi * 460 * t) * np.exp(-t / 0.02) * 0.5
    metal = (np.sin(2 * np.pi * 2960 * t) + 0.5 * np.sin(2 * np.pi * 4440 * t)) * np.exp(-t / 0.035) * 0.12
    out["mech_latch"] = save("mech_latch", small_room(tick + body + metal, 0.1), -6)
    n = int(0.16 * SR); t = np.arange(n) / SR
    click = lambda at: np.concatenate([np.zeros(int(at * SR)), bandpass(noise(int(0.006 * SR)), 1800, 4500)])
    x = np.zeros(n); c1 = click(0); c2 = click(0.035) * 0.8; x[: len(c1)] += c1; x[: len(c2)] += c2
    x += (np.sin(2 * np.pi * 1800 * t) + 0.6 * np.sin(2 * np.pi * 2700 * t)) * np.exp(-t / 0.05) * 0.1 + np.sin(2 * np.pi * 390 * t) * np.exp(-t / 0.02) * 0.35
    out["mech_gear_mesh"] = save("mech_gear_mesh", small_room(x, 0.1), -6)
    n = int(0.45 * SR); t = np.arange(n) / SR
    x = (np.sin(2 * np.pi * 523.25 * t) + 0.3 * np.sin(2 * np.pi * 1046.5 * t)) * np.exp(-t / 0.12) * (1 - np.exp(-t / 0.004))
    x += (np.sin(2 * np.pi * 100 * t) + 0.4 * np.sin(2 * np.pi * 200 * t)) * env(n, 0.03, 0.2) * 0.12
    out["mech_pad_on"] = save("mech_pad_on", small_room(x, 0.12), -7)
    n = int(0.3 * SR); t = np.arange(n) / SR
    out["mech_pad_off"] = save("mech_pad_off", small_room(np.sin(2 * np.pi * 392 * t) * np.exp(-t / 0.08) * (1 - np.exp(-t / 0.004)), 0.12), -10)
    # Motor: a small gear motor, in the band a phone speaker plays (150 Hz – 2 kHz; the first machine loop sat at
    # 98 Hz and was inaudible on phones). 2 s, whole cycles of every component, so it loops cleanly.
    n = 2 * SR; t = np.arange(n) / SR
    buzz = sum(np.sin(2 * np.pi * 150 * k * t) / k for k in range(1, 15)) * (1 + .12 * np.sin(2 * np.pi * 4 * t))
    whine = (np.sin(2 * np.pi * 900 * t) + .4 * np.sin(2 * np.pi * 1350 * t)) * (0.8 + .2 * np.sin(2 * np.pi * .5 * t))
    ticks = np.zeros(n)
    for k in range(40):
        m = int(0.005 * SR); place(ticks, bandpass(noise(m), 1500, 3500) * np.hanning(m), int(k * n / 40))
    grit = circular(lambda v: bandpass(v, 600, 1600), noise(n))
    motor = circular(lambda v: highpass(lowpass(v, 1900), 140), buzz * .30 + whine * .10 + ticks * .35 + grit * .10)
    out["mech_motor_loop"] = save_loop("mech_motor_loop", motor, -8)
    # Slide: a block dragged over the lab bench — soft friction with a fine grain. 2 s loop.
    n = 2 * SR; grains = np.zeros(n)
    for k in range(70):
        m = int(RNG.uniform(.012, .03) * SR); place(grains, np.hanning(m) * RNG.uniform(.4, 1.0), int(RNG.uniform(0, n)))
    body = circular(lambda v: bandpass(v, 220, 1000), noise(n)) * (0.55 + 0.45 * grains / (grains.max() + 1e-9))
    hiss = circular(lambda v: bandpass(v, 1500, 3200), noise(n)) * .08
    out["block_slide_loop"] = save_loop("block_slide_loop", body + hiss, -8)
    # Arrived: a soft thunk and a small "tink" — a mechanism reached the end of its travel.
    n = int(0.45 * SR); t = np.arange(n) / SR
    thunk = np.sin(2 * np.pi * np.cumsum(glide(n, 260, 140, 0.5)) / SR) * np.exp(-t / 0.05)
    click = bandpass(noise(n), 1000, 3000) * np.exp(-t / 0.004) * 0.5
    tink = glass(1318.5, 0.45, 0.35)[:n]; tink = np.concatenate([np.zeros(int(0.03 * SR)), tink])[:n]
    out["mech_arrive"] = save("mech_arrive", small_room(thunk + click + tink, 0.12), -5)
    x = glass(1046.5, 1.2, 0.8)
    out["mech_lift_ding"] = save("mech_lift_ding", reverb(x, 1.0, 0.25), -8)

    # Game flow
    x = np.zeros(int(3.6 * SR))
    for k, m in enumerate([74, 78, 81, 86]):
        c = celesta(midi(m), 2.6, 0.55); s = int(k * 0.14 * SR); x[s:s + len(c)] += c[: len(x) - s]
    for j, m in enumerate([50, 57, 61, 64, 66]):
        p = pad_note(midi(m), 3.4, 0.22, attack=0.35, release=2.4, bright=1500); x[: len(p)] += p[: len(x)]
    out["game_win"] = save("game_win", reverb(x, 2.2, 0.3), -4)
    x = np.zeros(int(1.6 * SR)); a = felt_piano(midi(69), 1.2, 0.5); b = felt_piano(midi(66), 1.2, 0.45)
    x[: len(a)] += a; s = int(0.38 * SR); x[s:s + len(b)] += b[: len(x) - s]
    out["game_fail"] = save("game_fail", reverb(x, 1.8, 0.3), -8)
    n = int(0.35 * SR)
    rew = swept_bandpass(noise(n), glide(n, 2200, 300, 0.7), q=2.0) * env(n, 0.05, 0.1)
    out["game_retry"] = save("game_retry", small_room(rew, 0.15), -9)
    x = np.zeros(int(2.0 * SR)); p = felt_piano(midi(74), 1.8, 0.5); c = celesta(midi(81), 1.8, 0.2)
    x[: len(p)] += p; x[int(0.05 * SR): int(0.05 * SR) + len(c)] += c[: len(x) - int(0.05 * SR)]
    out["game_level_start"] = save("game_level_start", reverb(x, 1.8, 0.3), -9)

    # Far away in the lab (played rarely, very quietly)
    n = int(0.5 * SR); t = np.arange(n) / SR
    beep = np.sin(2 * np.pi * 1244 * t) * (((t > 0) & (t < 0.1)) | ((t > 0.2) & (t < 0.3)))
    out["lab_far_beep"] = save("lab_far_beep", reverb(lowpass(fade_edges(beep, 0.005, 0.005), 2000), 2.0, 0.6), -12)
    out["lab_far_clink"] = save("lab_far_clink", reverb(lowpass(glass(2637, 0.8, 0.8), 3500), 2.0, 0.6), -12)
    n = int(0.4 * SR); t = np.arange(n) / SR
    thud = bandpass(noise(n), 280, 900) * np.exp(-t / 0.05) + np.sin(2 * np.pi * 300 * t) * np.exp(-t / 0.06) * 0.6
    out["lab_far_thud"] = save("lab_far_thud", reverb(thud, 2.2, 0.55), -12)
    return out


# ---------------------------------------------------------------------------------------------------------------------
# Intro score: 20 s under the opening comic, hit points synced to COgheIntro's shots (seconds):
# 0 fall (whistle, rising air) · 1.8 impact · 3.0 arrival (rotor, drum pulse) · 5.6 the sphere (heartbeat) · 7.0 it
# opens (steam, chime) · 7.62 COghe's first "u?" · 7.8 soldiers flinch · 9.6 the warm turn (D major) · 10.6 it lands on
# her palm · 11.4 close-up (the creature motif, glints 12.0/12.55/13.2) · 14.6 the lab · 16.3 it hops into the box ·
# 18.0-19.6 the dissolve resolves on Dmaj9, the chord the game's music starts on.
def intro_score():
    global RNG
    RNG = np.random.default_rng(3131)   # its own stream, like the music
    total = 20.0; n = int(total * SR)
    L = np.zeros((n + int(4 * SR), 2))

    def add(x, at_s, pan=0.0, gain=1.0):
        s = int(at_s * SR); x = x * gain
        if s < 0: x = x[-s:]; s = 0
        l = x * np.cos((pan + 1) * np.pi / 4); r = x * np.sin((pan + 1) * np.pi / 4)
        e = min(len(L), s + len(x)); L[s:e, 0] += l[: e - s]; L[s:e, 1] += r[: e - s]

    # 0-1.8 the fall: a dark pad swelling, air rising, the object's whistle bending down (doppler)
    for m in (38, 45, 50, 53):
        add(pad_note(midi(m), 2.6, vel=0.2, attack=1.4, release=0.6, bright=900), 0, pan=RNG.uniform(-.4, .4))
    m = int(1.8 * SR); tt = np.arange(m) / SR
    air = swept_bandpass(noise(m), glide(m, 400, 3200, 2.0), q=1.6) * (tt / 1.8) ** 2 * 0.9
    add(air, 0, 0.2)
    whistle = np.sin(2 * np.pi * np.cumsum(glide(m, 2100, 700, 0.7)) / SR) * np.clip((tt - 0.4) / 0.6, 0, 1) * 0.12
    add(lowpass(whistle, 3000), 0, 0.5)

    # 1.8 impact: a deep boom that phones can still hear (a mid crunch on top), then debris
    m = int(2.6 * SR); tt = np.arange(m) / SR
    boom = np.sin(2 * np.pi * np.cumsum(glide(m, 95, 38, 0.4)) / SR) * np.exp(-tt / 0.55)
    crunch = bandpass(noise(m), 250, 2400) * np.exp(-tt / 0.16) * 0.9
    body = bandpass(brown(m), 150, 700) * np.exp(-tt / 0.7) * 2.5
    hit = soft_clip((boom * 1.2 + crunch + body) * 1.4, 1.6)
    add(reverb(hit, 2.4, 0.35)[:, 0], 1.8, -0.1); add(reverb(hit, 2.4, 0.35)[:, 1], 1.8, 0.1)
    for k in range(18):
        at = 2.0 + RNG.uniform(0, 0.9); q = int(0.05 * SR)
        add(bandpass(noise(q), 800, 3500) * np.exp(-np.arange(q) / SR / 0.01) * RNG.uniform(0.05, 0.15), at, RNG.uniform(-.7, .7))

    # 3.0-5.6 arrival: rotor chop, a low drum pulse and a pulsing string bed (heroic, tense)
    m = int(2.8 * SR); tt = np.arange(m) / SR
    chop = bandpass(noise(m), 180, 1200) * (0.5 + 0.5 * np.sign(np.sin(2 * np.pi * 17 * tt))) * env(m, 0.6, 0.6) * 0.22
    add(lowpass(chop, 1400), 3.0, 0.6)
    beat = 60 / 104
    for k in range(6):
        q = int(0.5 * SR); tq = np.arange(q) / SR
        drum = np.sin(2 * np.pi * np.cumsum(glide(q, 180, 70, 0.5)) / SR) * np.exp(-tq / 0.16) + bandpass(noise(q), 200, 900) * np.exp(-tq / 0.03) * 0.5
        add(drum, 3.0 + k * beat, 0, 0.55 + 0.08 * k)
    for k in range(12):
        f = midi(50 if k % 4 != 3 else 53); q = int(beat / 2 * SR); tq = np.arange(q) / SR
        saw = sum(np.sin(2 * np.pi * f * h * tq) / h for h in range(1, 9))
        add(lowpass(saw, 1400) * env(q, 0.01, 0.12) * 0.09, 3.0 + k * beat / 2, -0.3)

    # 5.6-7.0 the sphere: heartbeat and a thin rising glass tone
    for k, at in enumerate([5.7, 5.98, 6.5, 6.78]):
        q = int(0.3 * SR); tq = np.arange(q) / SR
        add(np.sin(2 * np.pi * np.cumsum(glide(q, 150, 90)) / SR) * np.exp(-tq / 0.09) + bandpass(noise(q), 250, 700) * np.exp(-tq / 0.02) * 0.3, at, 0, 0.5 if k % 2 == 0 else 0.35)
    m = int(1.5 * SR); tt = np.arange(m) / SR
    add(np.sin(2 * np.pi * np.cumsum(glide(m, 880, 1320, 1.5)) / SR) * env(m, 1.0, 0.2) * 0.05, 5.6, 0.3)
    for mm in (38, 44, 51):
        add(pad_note(midi(mm), 1.8, vel=0.12, attack=0.8, release=0.4), 5.4, RNG.uniform(-.4, .4))

    # 7.0 it opens: steam and a bright chime; 7.62 COghe's first sound; 7.8 the soldiers flinch
    m = int(1.2 * SR); tt = np.arange(m) / SR
    add(highpass(noise(m), 2500) * np.exp(-tt / 0.35) * 0.18, 7.0, -0.2)
    for f, d in ((midi(86), 0), (midi(93), 0.04)):
        add(glass(f, 1.6, 0.18), 7.0 + d, 0.3)
    add(small_room(voice(np.array([520, 560, 780]), 0.22, (U, I)), 0.2), 7.62, 0.0, 1.4)
    for mm in (50, 52, 57, 59, 64):   # held breath while it looks around: a suspended, curious chord
        add(pad_note(midi(mm), 3.0, vel=0.13, attack=0.5, release=0.8, bright=1100), 7.0, RNG.uniform(-.5, .5))
    q = int(0.7 * SR); tq = np.arange(q) / SR
    stab = sum(np.sin(2 * np.pi * midi(mm) * h * tq) / h for mm in (38, 45, 51) for h in range(1, 6))
    add(lowpass(stab, 1500) * np.exp(-tq / 0.2) * (1 - np.exp(-tq / 0.005)) * 0.08, 7.8, 0)

    # 9.6 the warm turn: D major pad, felt piano; 10.6 the creature lands on her palm, happy
    for mm in (50, 57, 61, 64, 66):
        add(pad_note(midi(mm), 8.5, vel=0.15, attack=1.2, release=2.4), 9.6, -0.5 + (mm - 50) / 16)
    add(np.sin(2 * np.pi * midi(38) * t_axis(8.0)) * env(int(8.0 * SR), 1.0, 2.0) * 0.1, 9.6, 0)
    for at, mm in ((9.65, 62), (10.1, 66), (11.4, 69), (12.4, 66), (13.4, 64)):
        add(felt_piano(midi(mm), 3.0, vel=0.5), at, RNG.uniform(-.3, .3), 0.6)
    q = int(0.3 * SR); tq = np.arange(q) / SR
    add(np.sin(2 * np.pi * np.cumsum(glide(q, 420, 200, 0.5)) / SR) * np.exp(-tq / 0.06) * 0.5, 10.6, 0.2)
    add(small_room(np.concatenate([voice(np.array([600, 880, 900]), 0.13, (A, I)), np.zeros(int(0.035 * SR)),
        voice(np.array([760, 1100, 1180]), 0.19, (O, I))]), 0.18), 10.75, 0.2, 0.8)

    # 11.4 close-up: the creature motif on celesta (as in the game's music), glints
    for k, mm in enumerate([74, 78, 81, 80]):
        add(celesta(midi(mm), 2.4, vel=0.3), 11.9 + k * 0.42, 0.25)
    for at, mm in ((12.0, 98), (12.55, 102), (13.2, 105)):
        add(glass(midi(mm), 0.9, 0.05), at, RNG.uniform(-.5, .5))

    # 14.6 the lab: quieter, a single piano phrase; 16.3 it hops into the box; 16.7 a small "hm?"
    for mm in (50, 54, 57, 61):
        add(pad_note(midi(mm + 12), 5.8, vel=0.08, attack=0.6, release=1.6, bright=1400), 14.6, RNG.uniform(-.5, .5))
    for at, mm in ((14.7, 69), (15.4, 66), (16.1, 62)):
        add(felt_piano(midi(mm), 2.6, vel=0.4), at, 0, 0.55)
    q = int(0.3 * SR); tq = np.arange(q) / SR
    add(np.sin(2 * np.pi * np.cumsum(glide(q, 460, 210, 0.5)) / SR) * np.exp(-tq / 0.05) * 0.45 + bandpass(noise(q), 400, 1600) * np.exp(-tq / 0.03) * 0.2, 16.46, -0.3)
    add(small_room(voice(np.array([430, 400, 390, 470, 580]), 0.42, (M, U), nasal=True), 0.2), 16.8, -0.3, 0.7)

    # 18.0-19.6 the dissolve: a glass shimmer rising into Dmaj9, fading to leave the game's music its first chord
    m = int(1.8 * SR); tt = np.arange(m) / SR
    add(swept_bandpass(noise(m), glide(m, 1500, 6000, 1.2), q=6.0) * env(m, 1.2, 0.5) * 0.25, 17.9, 0.3)
    for k, mm in enumerate([74, 78, 81, 85, 88]):
        add(celesta(midi(mm), 2.6, vel=0.22), 18.0 + k * 0.12, -0.4 + k * 0.2)
    for mm in (38, 50, 57, 61, 64, 66):
        add(pad_note(midi(mm), 3.2, vel=0.14 if mm > 40 else 0.2, attack=0.5, release=1.6), 18.0, RNG.uniform(-.4, .4))

    wet = lowpass(reverb(L, 2.6, 0.28, damp=4200), 11000)[:n]
    fade = np.ones(n); f = int(0.6 * SR); fade[-f:] = np.linspace(1, 0, f) ** 2
    out = soft_clip(wet * fade[:, None] * 1.1, 1.2)
    rms = np.sqrt(np.mean(out ** 2)); out *= 10 ** (-17 / 20) / rms
    peak = np.max(np.abs(out))
    if peak > 10 ** (-1 / 20): out *= 10 ** (-1 / 20) / peak
    write_ogg("intro_score", out)
    return out


# ---------------------------------------------------------------------------------------------------------------------
# COghe's character (COghePersonality acts): its own random stream, so the approved effects above never change.
def personality_sfx():
    global RNG
    RNG = np.random.default_rng(5151)
    out = {}
    # tantrum: grumble, whoosh, splat on the glass, a squeaky slide, "hmph"
    n = int(.55 * SR); t = np.arange(n) / SR
    growl = voice(np.array([330, 300, 290, 310, 270]), .55, (M, U), breath=.06, vib=.06, nasal=True)
    growl *= 1 + .35 * np.sin(2 * np.pi * 24 * t[:len(growl)])
    out["creature_grumble"] = save("creature_grumble", small_room(growl, .15), -6)
    n = int(.32 * SR)
    out["creature_whoosh"] = save("creature_whoosh", swept_bandpass(noise(n), glide(n, 700, 2600, 1.4), q=2.2) * env(n, .08, .14), -9)
    n = int(.4 * SR); t = np.arange(n) / SR
    slap = bandpass(noise(n), 500, 2800) * np.exp(-t / .025)
    thump = np.sin(2 * np.pi * np.cumsum(glide(n, 520, 280, .5)) / SR) * np.exp(-t / .07) * .8
    squelch = swept_bandpass(noise(n), glide(n, 1800, 600), q=6) * np.exp(-((t - .09) / .05) ** 2) * .6
    out["creature_splat"] = save("creature_splat", small_room(slap + thump + squelch, .12), -4)
    n = int(.7 * SR); t = np.arange(n) / SR
    squeak = np.sin(2 * np.pi * np.cumsum(glide(n, 980, 720) * (1 + .03 * np.sin(2 * np.pi * 9 * t))) / SR) * env(n, .05, .2) * .5
    squeak += bandpass(noise(n), 1500, 4000) * env(n, .05, .2) * .12
    out["creature_slide"] = save("creature_slide", squeak, -12)
    hmph = voice(np.array([440, 400, 330]), .22, (M, M), breath=.12, nasal=True)
    puff = bandpass(noise(int(.12 * SR)), 900, 3000) * env(int(.12 * SR), .01, .08) * .5
    out["creature_hmph"] = save("creature_hmph", small_room(np.concatenate([hmph, puff]), .12), -7)
    # wave: a cheerful "hi-i!"
    out["creature_hi"] = save("creature_hi", small_room(np.concatenate([
        voice(np.array([620, 760]), .12, (A, I)), np.zeros(int(.03 * SR)), voice(np.array([760, 980, 1040]), .2, (I, I))]), .18), -6)
    # a shape appears: two sparkly notes and a pleased "o!"
    x = np.zeros(int(1.0 * SR)); a = celesta(midi(86), .9, .45); b = celesta(midi(93), .9, .45)
    x[:len(a)] += a; s0 = int(.09 * SR); x[s0:s0 + len(b)] += b[:len(x) - s0]
    o = voice(np.array([700, 900, 880]), .16, (O, O)); s1 = int(.05 * SR); x[s1:s1 + len(o)] += o * .6
    out["creature_tada"] = save("creature_tada", reverb(x, 1.2, .25)[:, 0], -7)
    # melt: a sinking "uuuh" with a gloop; the pop back up
    melt = voice(np.array([520, 480, 400, 330, 300]), .75, (U, U), breath=.05)
    m = int(.2 * SR); tt = np.arange(m) / SR
    gloop = np.sin(2 * np.pi * np.cumsum(glide(m, 600, 300)) / SR) * np.exp(-tt / .06) * .5
    mx = np.zeros(len(melt) + m); mx[:len(melt)] += melt; mx[int(.5 * SR):int(.5 * SR) + m] += gloop
    out["creature_melt"] = save("creature_melt", small_room(mx, .18), -7)
    m = int(.25 * SR); tt = np.arange(m) / SR
    pop = np.sin(2 * np.pi * np.cumsum(glide(m, 380, 900, .6)) / SR) * np.exp(-tt / .06) + bandpass(noise(m), 1200, 3500) * np.exp(-tt / .006) * .4
    out["creature_pop"] = save("creature_pop", small_room(pop, .12), -6)
    # a knock on the glass pane
    m = int(.14 * SR); tt = np.arange(m) / SR
    tok = sum(a_ * np.sin(2 * np.pi * f * tt) * np.exp(-tt / d) for f, a_, d in [(620, .8, .03), (2350, .5, .02), (3900, .25, .012)])
    tok += bandpass(noise(m), 1500, 5000) * np.exp(-tt / .003) * .4
    out["glass_tok"] = save("glass_tok", reverb(tok, .8, .18)[:, 0], -8)
    # a soft snore: a breath in, a small hum out
    m = int(1.3 * SR); tt = np.arange(m) / SR
    inhale = bandpass(noise(m), 600, 2500) * np.exp(-((tt - .3) / .18) ** 2) * .35
    hum = voice(np.linspace(300, 270, 8), .55, (M, M), breath=.1, nasal=True)
    sn = inhale.copy(); s2 = int(.62 * SR); sn[s2:s2 + len(hum)] += hum[:len(sn) - s2] * .8
    out["creature_snore"] = save("creature_snore", small_room(sn, .15), -10)
    return out


if __name__ == "__main__":
    sfx()
    # The music is only rebuilt on request: the committed loop is the one that was approved by ear.
    if "--music" in sys.argv: music()
    if "--intro" in sys.argv: intro_score()
    if "--personality" in sys.argv: personality_sfx()
    print("written to", os.path.abspath(OUT))
    for f in sorted(os.listdir(OUT)):
        if f.endswith(".ogg"):
            info = sf.info(os.path.join(OUT, f)); print(f"  {f:34s} {info.duration:6.2f} s  {info.channels} ch")
