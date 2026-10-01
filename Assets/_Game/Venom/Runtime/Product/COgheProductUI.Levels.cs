using UnityEngine;
using UnityEngine.UI;

namespace GravityBox.Venom
{
    /// <summary>Test builds only (<see cref="COgheTestTools"/>): Pause → "Levels (test)" opens every level of the catalog.
    /// Boss levels are marked; finished ones show a check. Loading a level does not change progress.</summary>
    public sealed partial class COgheProductUI
    {
        private void LevelsPopup(RectTransform panel, float w, float h)
        {
            PopupTitle(panel, w, "Levels (test)"); ClosePopup(panel, w, () => ShowPopup(COgheProductPopup.Pause));
            const int columns = 5;
            float cell = (w - 28) / columns, cellHeight = 50;
            int count = Catalog.Levels.Length, rows = (count + columns - 1) / columns;
            var viewport = art.Rect(panel, "Viewport", new Rect(14, 74, w - 28, h - 88)); viewport.gameObject.AddComponent<RectMask2D>();
            var content = art.Rect(viewport, "Content", new Rect(0, 0, w - 28, rows * cellHeight + 8));
            var scroll = panel.gameObject.AddComponent<ScrollRect>(); scroll.viewport = viewport; scroll.content = content;
            scroll.horizontal = false; scroll.vertical = true; scroll.movementType = ScrollRect.MovementType.Clamped;
            for (int i = 0; i < count; i++)
            {
                var level = Catalog.Levels[i]; if (level == null) continue;
                int n = i + 1; bool current = n == Game.Definition.Order, boss = level.Boss, done = Game.Progress.Completed.Contains(level.Id);
                var box = art.Box(content, "Level " + n, new Rect((i % columns) * cell + 3, (i / columns) * cellHeight + 3, cell - 6, cellHeight - 6),
                    current ? COgheUIArt.Mint : boss ? new Color(.99f, .93f, .86f) : new Color(.995f, .995f, .972f), true);
                box.pixelsPerUnitMultiplier = 2f;
                var b = box.gameObject.AddComponent<Button>(); b.targetGraphic = box;
                b.onClick.AddListener(() => { COgheAudio.UiTap(); LoadForTest(n); });
                art.Label(box.transform, "Number", n.ToString("00"), new Rect(0, 4, cell - 6, 24), 15, boss ? new Color(.74f, .35f, .25f) : COgheUIArt.Ink);
                art.Label(box.transform, "Tag", boss ? "Boss" : done ? "Done" : "", new Rect(0, 25, cell - 6, 14), 9, done ? COgheUIArt.Teal : COgheUIArt.Muted);
            }
            content.anchoredPosition = new Vector2(0, Mathf.Clamp((Game.Definition.Order - 1) / columns * cellHeight - 2 * cellHeight, 0, Mathf.Max(0, rows * cellHeight - (h - 88))));
        }
    }
}
