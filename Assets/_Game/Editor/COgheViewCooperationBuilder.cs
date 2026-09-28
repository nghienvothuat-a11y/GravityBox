using UnityEngine;

namespace GravityBox.Editor
{
    public static partial class VenomCampaignBuilder
    {
        private static void ChapterCooperation(ExpansionContext c)
        {
            // Level 22 introduces the physical cut and reunion; 23–30 use the simultaneous builder.
            ChapterShell(c); ChapterKnife(c);
            ViewBlock(c, "Separation island", new Vector3(-.32f, -.255f, -.045f), new Vector3(.025f, .09f, .11f));
        }
    }
}
