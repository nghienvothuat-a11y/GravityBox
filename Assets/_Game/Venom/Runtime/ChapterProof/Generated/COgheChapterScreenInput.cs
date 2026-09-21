#if DEVELOPMENT_BUILD && !UNITY_EDITOR
// Generated from Assets/_Game/Tests/PlayMode/COgheChapterScreenInput.cs
// SHA256 474dfbf04bc16e980d8fb3a9bc16805ae28928a4cc44856e50ba9f5286460732
// Do not edit; regenerate after test changes.
using GravityBox.Venom;

using UnityEngine;
using Object = UnityEngine.Object;

namespace GravityBox.Venom.ChapterProof
{
    // Screen rays and authored HUD camera choices only. Never changes puzzle state.
    public sealed class COgheChapterScreenInput
    {
        public int Width = 480, Height = 800;
        private int zone = -1;
        public void Tap(VenomCampaign game, Vector3 world, bool chooseView = true)
        {
            if (chooseView)
            {
                zone = -1;
                Vector3 local = game.Root.InverseTransformPoint(world);
                float best = float.PositiveInfinity;
                for (int i = 0; i < game.CameraRig.ZoneCount; i++)
                {
                    Bounds bounds = game.Definition.CameraZones[i].LocalBounds;
                    float distance = (bounds.ClosestPoint(local) - local).sqrMagnitude;
                    if (distance < best) { best = distance; zone = i; }
                }
            }
            var camera = game.Owner.View;
            var previous = camera.targetTexture;
            var target = new RenderTexture(Width, Height, 24);
            camera.targetTexture = target;
            try
            {
                game.CameraRig.SelectZone(zone);
                game.CameraRig.Frame(Width, Height, 0, true);
                Vector3 screen = camera.WorldToScreenPoint(world);
                // A long rail may cross compartment framing. The overview is a
                // real HUD action and keeps the drag endpoint on the display.
                if(!chooseView && !game.CameraRig.UsableRect(Width,Height,new Rect(0,0,Width,Height)).Contains(screen))
                {
                    zone=-1;game.CameraRig.SelectZone(-1);game.CameraRig.Frame(Width,Height,0,true);
                    screen=camera.WorldToScreenPoint(world);
                }
                Assert.Greater(screen.z, 0, "Target is in front of camera");
                Assert.IsTrue(game.CameraRig.UsableRect(Width, Height, new Rect(0, 0, Width, Height)).Contains(screen),
                    $"Target {game.Root.InverseTransformPoint(world):F3} in view {zone} must remain outside HUD; pixel={screen:F1}");
                Assert.IsTrue(game.CameraRig.AllowsPointer(screen, Width, Height), "HUD must not intercept target");
                game.TouchPoint(screen);
                Debug.Log($"AUDIT_TAP level={game.Definition.Order} size={Width}x{Height} view={zone} world={game.Root.InverseTransformPoint(world):F3} picked={game.Feedback.CommandSurface?.name} target={game.Feedback.CommandPoint:F3} exit={game.Motion.Get(game.Motion.Selected)?.Exit}");
            }
            finally { camera.targetTexture = previous; Object.DestroyImmediate(target); }
        }
    }
}

#endif
