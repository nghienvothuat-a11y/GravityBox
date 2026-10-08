name = "COghe: Physics Puzzle Pet"
subtitle = "Stretch, Grab & Solve Levels"
keywords = "virtual,brain,teaser,logic,slime,blob,goo,jelly,squishy,kawaii,cute,gravity,rope,lever,gears,offline"
promo = "Meet COghe, a living liquid with big curious eyes. Guide it through 60 glass-box physics puzzles, then feed it, hug it and dress it up at Home."
desc = open('desc.txt').read().strip()
heads = ["A pet made of liquid", "60 physics puzzles", "Can you get COghe out?", "Cute blob, clever puzzles", "Tap. Stretch. Solve.",
         "Split it. Merge it. Escape.", "Feed it, hug it, dress it up", "Play free, offline"]
descs = ["Guide COghe, a living liquid, through 60 glass-box puzzles of levers, ropes and gears.",
         "Tap to move. COghe crawls, stretches and grips to pull handles and open the way out.",
         "Split COghe in two so each half does a job, then merge back. Brainy, cute and calm.",
         "Between puzzles, COghe lives at Home: feed it, play ball, hug it and earn Drops.",
         "Color it with inks, add hats and little floating friends. Make COghe one of a kind.",
         "Push and pull tall crates to clear a path. No timers, no rush: think it through."]
for k, v, lim in [("name", name, 30), ("subtitle", subtitle, 30), ("promo", promo, 170), ("description", desc, 4000)]:
    print(f"{k}: {len(v)}/{lim} {'OK' if len(v) <= lim else 'TOO LONG'}")
print(f"keywords: {len(keywords.encode())}/100 bytes {'OK' if len(keywords.encode()) <= 100 else 'TOO LONG'}")
used = set(w.lower().strip(':&,') for w in (name + ' ' + subtitle).split())
print('keyword repeats:', [k for k in keywords.split(',') if k in used or k.rstrip('s') in used])
for h in heads: print(len(h), 'OK' if len(h) <= 30 else 'LONG', h)
for d in descs: print(len(d), 'OK' if len(d) <= 90 else 'LONG', d)
