using System;
using System.Collections.Generic;
using System.IO;
using GravityBox.Venom;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GravityBox.Editor
{
    // Authoring interchange: Blender receives exact local-space dimensions, never physics ownership.
    public static class COgheBlenderExport
    {
        public const string Source = "ArtSource/COghe/NewGraphic";
        [Serializable] public class Pack { public List<Level> levels = new List<Level>(); }
        [Serializable] public class Level { public int number; public List<Part> parts = new List<Part>(); }
        [Serializable] public class Part
        {
            public string id, parent, anchor, kind, material;
            public Vector3 centre, size;
            public Vector3[] vertices;
            public int[] triangles;
            public Vector2[] uv;
            public bool pane, hole;
        }
        public static string KeyOf(Transform t) => t.parent == null ? t.GetSiblingIndex().ToString() : KeyOf(t.parent) + "/" + t.GetSiblingIndex();
        public static string PathOf(Transform t) => t.parent == null ? t.name : PathOf(t.parent) + "/" + t.name;
        public static void Export()
        {
            Directory.CreateDirectory(Source); Directory.CreateDirectory("Artifacts/COgheNewGraphic");
            File.WriteAllText("Artifacts/COgheNewGraphic/physics-baseline.txt", COgheViewArtVerification.CapturePhysics());
            var pack = new Pack();
            for (int n = 1; n <= 10; n++)
            {
                EditorSceneManager.OpenScene($"Assets/_Game/Venom/ViewCampaign/COgheView{n:00}.unity");
                var game = Object.FindFirstObjectByType<VenomCampaign>();
                var level = new Level { number = n }; pack.levels.Add(level);
                foreach (var patch in game.Surfaces)
                {
                    if (patch.GetComponentInParent<VenomMovableProp>() != null) continue;
                    var mesh = patch.GetComponent<MeshFilter>()?.sharedMesh; if (mesh == null) continue;
                    var part = new Part { id = "surface_" + level.parts.Count, parent = PathOf(patch.transform), anchor = KeyOf(patch.transform),
                        kind = patch.ExteriorGlass ? "pane" : "surface", pane = patch.ExteriorGlass,
                        hole = patch.Hole, size = new Vector3(patch.Size.x, patch.Size.y, .032f),
                        material = patch.Slippery ? "Lavender" : Mathf.Abs(patch.Normal.y) > .7f ? "Floor" : "Blue",
                        vertices = mesh.vertices, triangles = mesh.triangles, uv = mesh.uv };
                    level.parts.Add(part);
                }
                foreach (var prop in game.Props)
                {
                    Bounds bounds = default; bool found = false;
                    foreach (var c in prop.CollisionShapes)
                    {
                        if (!(c is BoxCollider box) || c.GetComponent<VenomSurfacePatch>() == null || c.name == "Bridge underside pocket cover") continue;
                        for (int i = 0; i < 8; i++)
                        {
                            var p = prop.transform.InverseTransformPoint(box.transform.TransformPoint(box.center + Vector3.Scale(box.size * .5f, new Vector3((i&1)==0?-1:1,(i&2)==0?-1:1,(i&4)==0?-1:1))));
                            if (!found) { bounds = new Bounds(p,Vector3.zero); found = true; } else bounds.Encapsulate(p);
                        }
                    }
                    if (found) level.parts.Add(new Part { id = "prop_" + level.parts.Count, parent = PathOf(prop.transform), anchor = KeyOf(prop.transform), kind = "prop", material = "Amber", centre = bounds.center, size = bounds.size });
                }
                foreach (var t in game.GetComponentsInChildren<Transform>(true))
                {
                    if (t.name == "V2 rounded handle") level.parts.Add(new Part { id="handle_"+level.parts.Count,parent=PathOf(t),anchor=KeyOf(t),kind="handle",material="Amber" });
                }
                var owner = game.GetComponent<VenomLevelController>();
                foreach(var task in game.GetComponentsInChildren<COgheTapRail>(true))
                {
                    var rail=task.Rail;
                    var start=rail.Start+rail.Frame.InverseTransformVector(task.Handle.position-rail.transform.position)-Vector3.up*.012f;
                    foreach(float distance in new[]{0f,rail.Travel})
                        level.parts.Add(new Part{id="bearing_"+level.parts.Count,parent=PathOf(rail.Frame),anchor=KeyOf(rail.Frame),kind="bearing",material="Ivory",centre=start+rail.Axis*distance,size=Mathf.Abs(rail.Axis.x)>.5f?new Vector3(.022f,.029f,.072f):new Vector3(.072f,.029f,.022f)});
                }
                level.parts.Add(new Part {id="exit",parent=PathOf(owner.Outlet),anchor=KeyOf(owner.Outlet),kind="exit",material="Mint",size=new Vector3(owner.ApertureRadius,0,0)});
                bool bridge = game.GetComponentInChildren<COgheDockedBridgeDeck>(true) != null;
                bool open = bridge;
                foreach (var p in game.Surfaces) open |= p.Hole && Vector3.Dot(p.Normal,Vector3.up)>.9f;
                level.parts.Add(new Part {id="base",parent=PathOf(owner.Rotation.transform),anchor=KeyOf(owner.Rotation.transform),kind=open?"openbase":"base",material="Ivory",centre=new Vector3(0,bridge?-.408f:-.34f,0),size=new Vector3(.88f,.075f,.68f)});
            }
            File.WriteAllText(Source+"/layout.json",JsonUtility.ToJson(pack,true));
            Debug.Log("COGHE BLENDER: exported exact geometry for ten scenes.");
        }
    }
}
