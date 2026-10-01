using UnityEngine;
using UnityEngine.Rendering;

namespace GravityBox.Venom
{
    /// <summary>The ink syringe (built in code, about 7 cm): needle, hub, a clear barrel with the ink column, plunger. Posed by
    /// its tip on COghe's skin; the column drains and the plunger follows as the ink goes in.</summary>
    public sealed class COgheSyringe : MonoBehaviour
    {
        private const float Barrel = .03f, Start = .018f;
        private Transform liquid, plunger;
        private Material ink, lit, glass;
        private COgheHomeMaterials materials;

        public static COgheSyringe Create(Transform parent, Material template)
        {
            var s = new GameObject("Ink syringe").AddComponent<COgheSyringe>(); s.transform.SetParent(parent, false);
            s.Build(template); return s;
        }

        private void Build(Material template)
        {
            lit = new Material(template) { name = "Syringe lit" }; lit.SetFloat("_Metallic", 0); lit.SetFloat("_Smoothness", .55f);
            glass = Fx(new Color(.82f, .92f, .97f, .14f), 1.2f, .5f, (int)RenderQueue.Transparent + 30);
            materials = new COgheHomeMaterials(lit, glass);
            var k = new COgheLowPoly();
            k.Paint("#9aa3a8").Rod(Vector3.zero, new Vector3(0, .015f, 0), .0007f, 6, .0005f);           // needle
            k.Paint("#e8e0cf").At(new Vector3(0, .014f, 0)).Cylinder(.0024f, .0024f, 0, .004f, 10);       // hub
            k.At(new Vector3(0, Start + Barrel, 0)).Box(new Vector3(0, .0012f, 0), new Vector3(.017f, .0024f, .007f), .0008f);   // finger flange
            k.At(Vector3.zero); k.Bake("Syringe body", transform, materials);
            var g = new COgheLowPoly(); g.Paint(new Color(.85f, .93f, .97f, .26f)).At(new Vector3(0, Start, 0)).Cylinder(.0047f, .0047f, 0, Barrel, 14);
            g.Bake("Syringe barrel", transform, materials, true);
            liquid = new GameObject("Ink").transform; liquid.SetParent(transform, false); liquid.localPosition = new Vector3(0, Start, 0);
            var l = new COgheLowPoly(); l.Paint(Color.white).Cylinder(.0039f, .0039f, 0, 1, 12); var lgo = l.Bake("Ink column", liquid, materials);
            ink = Fx(Color.white, .3f, 0, (int)RenderQueue.Transparent + 25);
            foreach (var r in lgo.GetComponentsInChildren<Renderer>()) r.sharedMaterial = ink;
            plunger = new GameObject("Plunger").transform; plunger.SetParent(transform, false);
            var p = new COgheLowPoly();
            p.Paint("#304e56").At(new Vector3(0, Start, 0)).Cylinder(.0041f, .0041f, 0, .003f, 12);     // rubber stopper
            p.Paint("#e8e0cf").At(Vector3.zero).Rod(new Vector3(0, Start + .003f, 0), new Vector3(0, Start + Barrel + .02f, 0), .0013f, 6);
            p.At(new Vector3(0, Start + Barrel + .02f, 0)).Cylinder(.0052f, .0052f, 0, .0022f, 14);       // thumb rest
            p.At(Vector3.zero); p.Bake("Plunger", plunger, materials);
            foreach (var r in GetComponentsInChildren<Renderer>()) r.shadowCastingMode = ShadowCastingMode.Off;
        }

        public void SetInk(COgheInk i)
        {
            var c = i.Color; c.a = .92f; ink.SetColor("_Color", c);
        }

        // the barrel and the ink are drawn with the small FX shader (always in builds), after COghe's skin
        private static Material Fx(Color color, float rim, float rimAlpha, int queue)
        {
            var m = new Material(Resources.Load<Shader>("COgheInk/AccessoryFx")) { name = "Syringe FX" };
            m.SetColor("_Color", color); m.SetFloat("_Rim", rim); m.SetFloat("_RimAlpha", rimAlpha);
            m.SetFloat("_SrcBlend", (float)BlendMode.One); m.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha); m.renderQueue = queue;
            return m;
        }

        /// <summary>Tip on <paramref name="tip"/>, body along <paramref name="outward"/>; <paramref name="away"/> metres back
        /// from the skin (negative: the needle is in), <paramref name="fill"/> of the ink left.</summary>
        public void Pose(Vector3 tip, Vector3 outward, float away, float fill)
        {
            var dir = outward.normalized;
            transform.SetPositionAndRotation(tip + dir * away, Quaternion.FromToRotation(Vector3.up, dir));
            fill = Mathf.Clamp01(fill);
            liquid.localScale = new Vector3(1, Mathf.Max(.0001f, Barrel * fill), 1);
            plunger.localPosition = new Vector3(0, -Barrel * (1 - fill), 0);
        }

        private void OnDestroy() { materials?.Dispose(); Destroy(lit); Destroy(glass); Destroy(ink); }
    }
}
