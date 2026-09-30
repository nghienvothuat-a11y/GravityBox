#!/usr/bin/env python3
"""Generate fixed-weight mobile fonts from the approved OFL Manrope source.

Requires fonttools. Unity's legacy dynamic Font uses the variable font's default
weight (200), so bundle explicit medium and bold instances for readable UI.
"""
from pathlib import Path
from fontTools.ttLib import TTFont
from fontTools.varLib.instancer import instantiateVariableFont

root = Path(__file__).resolve().parents[1]
source = root / 'Docs/ArtDirection/COghe/ProductUI/assets/manrope-variable.ttf'
out = root / 'Assets/_Game/Venom/Resources/COgheUI'
font = TTFont(source)
for name, weight in [('Manrope', 500), ('ManropeBold', 750)]:
    instance = instantiateVariableFont(font, {'wght': weight}, inplace=False)
    instance.save(out / f'{name}.ttf')
