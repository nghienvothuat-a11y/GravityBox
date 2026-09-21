#if DEVELOPMENT_BUILD && !UNITY_EDITOR
namespace GravityBox.Venom.ChapterProof
{
    // A request yields control before the scenario can issue its next command
    // or reset the scene. The driver captures the real end-of-frame framebuffer.
    internal sealed class COgheChapterProofCapture
    {
        internal readonly VenomCampaign Game;
        internal readonly string Label;
        private COgheChapterProofCapture(VenomCampaign game, string label) { Game = game; Label = label; }
        internal static COgheChapterProofCapture Request(VenomCampaign game, string label) => new COgheChapterProofCapture(game, label);
    }
}
#endif
