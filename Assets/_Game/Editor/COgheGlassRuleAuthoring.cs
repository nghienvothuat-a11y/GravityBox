using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GravityBox.Venom;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GravityBox.Editor
{
    /// <summary>
    /// Spatial glass contract, 14/10/2026: bare glass grips; only the visible coating releases it.
    /// This is an authoring pass, not a runtime rule keyed to a level number. Builders and migration use the same data.
    /// Opening layouts have bare walls. Later layouts isolate the old access points with real coating regions;
    /// G1–G5 already author their own regions. Rendering reads the resulting patches without changing physics.
    /// </summary>
    public static class COgheGlassRuleAuthoring
    {
        const float Band = .09f;
        static Vector3[] Corners(VenomSurfacePatch p) => new[] {
            p.transform.TransformPoint(new Vector3(-p.Size.x/2,-p.Size.y/2,0)),
            p.transform.TransformPoint(new Vector3(p.Size.x/2,-p.Size.y/2,0)),
            p.transform.TransformPoint(new Vector3(p.Size.x/2,p.Size.y/2,0)),
            p.transform.TransformPoint(new Vector3(-p.Size.x/2,p.Size.y/2,0))};

        public static void Configure(string key, VenomCampaign game)
        {
            if(!VenomCampaignBuilder.SpatialOrder.Contains(key))return; // archived experiments keep their own rules
            var root = game.GetComponent<VenomLevelController>().Rotation.transform;
            var patches = game.Surfaces.Where(p => p != null && p.gameObject.activeInHierarchy && !p.IsCurved).ToArray();
            bool opening = VenomCampaignBuilder.SpatialPosition(key) <= 6;
            foreach (var pane in patches.Where(p => p.ExteriorGlass))
            {
                pane.Selectable = true;
                if (key.StartsWith("G", StringComparison.Ordinal)) continue; // approved explicit coatings
                if (opening) { pane.Slippery = false; pane.HasSlipRegion = false; pane.SlipRegions = Array.Empty<Rect>(); continue; }
                var rects = new List<Rect>();
                var normal = root.InverseTransformDirection(pane.Normal);
                if (key == "06") PulleyCoatings(pane, root, rects);
                else if (Mathf.Abs(normal.y) < .5f)
                {
                    // Protect each actual access edge, including an elevated ledge; leave the rest of the pane bare.
                    foreach (var support in patches.Where(p => !p.ExteriorGlass && p.MotionFrame == null && root.InverseTransformDirection(p.Normal).y > .9f))
                    {
                        var q = Corners(support).Select(pane.transform.InverseTransformPoint).ToArray();
                        for (int i = 0; i < 4; i++)
                        {
                            var a = q[i]; var b = q[(i + 1) % 4];
                            if (Mathf.Abs(a.z) > .04f || Mathf.Abs(b.z) > .04f || Mathf.Abs(a.y - b.y) > .003f) continue;
                            Add(pane, rects, Rect.MinMaxRect(Mathf.Min(a.x,b.x), (a.y+b.y)*.5f, Mathf.Max(a.x,b.x), (a.y+b.y)*.5f+Band));
                        }
                    }
                    // A partition meeting glass must not gain a bare route around its end when glass becomes climbable.
                    foreach (var wall in patches.Where(p => !p.ExteriorGlass && p.MotionFrame == null && Mathf.Abs(root.InverseTransformDirection(p.Normal).y) < .1f && Mathf.Abs(Vector3.Dot(p.Normal,pane.Normal)) < .1f))
                    {
                        var q = Corners(wall).Select(pane.transform.InverseTransformPoint).ToArray();
                        if(q.Max(v=>v.y)-q.Min(v=>v.y)<.13f)continue;
                        var touching=q.Where(v=>Mathf.Abs(v.z)<.006f).ToArray();
                        if(touching.Length<2)continue;
                        float x=touching.Average(v=>v.x);
                        Add(pane,rects,Rect.MinMaxRect(x-Band*.5f,-pane.Size.y*.5f,x+Band*.5f,pane.Size.y*.5f));
                    }
                }
                else if (normal.y < -.9f)
                {
                    // Edge coating prevents a free wall -> roof -> far-side shortcut; the middle remains bare glass.
                    float x=pane.Size.x*.5f,y=pane.Size.y*.5f;
                    Add(pane,rects,Rect.MinMaxRect(-x,-y,x,-y+Band));Add(pane,rects,Rect.MinMaxRect(-x,y-Band,x,y));
                    Add(pane,rects,Rect.MinMaxRect(-x,-y+Band,-x+Band,y-Band));Add(pane,rects,Rect.MinMaxRect(x-Band,-y+Band,x,y-Band));
                }
                if(pane.HasSlipRegion)Add(pane,rects,pane.SlipRegion);
                // A swing teaches crossing the gap: a continuous stripe closes the bare-glass route between banks.
                if(key=="18") DividerStripe(pane,root,rects,.10f);
                pane.Slippery=false;pane.HasSlipRegion=false;pane.SlipRegions=rects.Distinct().ToArray();
            }
        }

        static void PulleyCoatings(VenomSurfacePatch pane,Transform root,List<Rect> rects)
        {
            var normal=root.InverseTransformDirection(pane.Normal);
            // Left glass carries C directly. The centre stripe continues across the roof and front.
            DividerStripe(pane,root,rects,.10f);
            // The right platform cannot be reached from the floor along the glass. Once the bridge is raised its
            // top is above this band and the last short climb to the outlet is bare.
            if(normal.x<-.9f || Mathf.Abs(normal.z)>.9f)
            {
                float lo=-pane.Size.x*.5f,hi=pane.Size.x*.5f;
                if(Mathf.Abs(normal.z)>.9f)
                {
                    float at=pane.transform.InverseTransformPoint(root.TransformPoint(new Vector3(.10f,0,0))).x;
                    float right=pane.transform.InverseTransformVector(root.right).x;
                    if(right>0)lo=at;else hi=at;
                }
                float floor=pane.transform.InverseTransformPoint(root.TransformPoint(new Vector3(0,-.30f,0))).y;
                Add(pane,rects,Rect.MinMaxRect(lo,floor,hi,floor+.16f));
            }
        }

        static void DividerStripe(VenomSurfacePatch pane,Transform root,List<Rect> rects,float halfWidth)
        {
            var normal=root.InverseTransformDirection(pane.Normal);
            if(Mathf.Abs(normal.z)<.9f&&normal.y>-.9f)return;
            var a=pane.transform.InverseTransformPoint(root.TransformPoint(new Vector3(-halfWidth,-.30f,-.30f)));
            var b=pane.transform.InverseTransformPoint(root.TransformPoint(new Vector3(halfWidth,.30f,.30f)));
            Add(pane,rects,Rect.MinMaxRect(Mathf.Min(a.x,b.x),-pane.Size.y*.5f,Mathf.Max(a.x,b.x),pane.Size.y*.5f));
        }

        // Clip to the actual pane and leave the complete exit collar bare. The same rectangles feed grip and artwork.
        static void Add(VenomSurfacePatch pane,List<Rect> dest,Rect r)
        {
            r=Rect.MinMaxRect(Mathf.Max(r.xMin,-pane.Size.x*.5f),Mathf.Max(r.yMin,-pane.Size.y*.5f),Mathf.Min(r.xMax,pane.Size.x*.5f),Mathf.Min(r.yMax,pane.Size.y*.5f));
            if(r.width<.005f||r.height<.005f)return;
            if(!pane.Hole){UnionAdd(dest,r);return;}
            float radius=pane.HoleRadius+.008f;
            var hole=new Rect(pane.HoleCentre-Vector2.one*radius,Vector2.one*radius*2);
            if(!r.Overlaps(hole)){UnionAdd(dest,r);return;}
            void Keep(float x0,float y0,float x1,float y1){if(x1-x0>.005f&&y1-y0>.005f)UnionAdd(dest,Rect.MinMaxRect(x0,y0,x1,y1));}
            Keep(r.xMin,r.yMin,r.xMax,Mathf.Min(r.yMax,hole.yMin));Keep(r.xMin,Mathf.Max(r.yMin,hole.yMax),r.xMax,r.yMax);
            Keep(r.xMin,Mathf.Max(r.yMin,hole.yMin),Mathf.Min(r.xMax,hole.xMin),Mathf.Min(r.yMax,hole.yMax));
            Keep(Mathf.Max(r.xMin,hole.xMax),Mathf.Max(r.yMin,hole.yMin),r.xMax,Mathf.Min(r.yMax,hole.yMax));
        }

        // Disjoint rectangles avoid drawing two transparent films at the same depth where bands intersect.
        static void UnionAdd(List<Rect> regions,Rect add)
        {
            var pending=new List<Rect>{add};
            foreach(var existing in regions)
            {
                var next=new List<Rect>();
                foreach(var r in pending)
                {
                    if(!r.Overlaps(existing)){next.Add(r);continue;}
                    float x0=Mathf.Max(r.xMin,existing.xMin),x1=Mathf.Min(r.xMax,existing.xMax),y0=Mathf.Max(r.yMin,existing.yMin),y1=Mathf.Min(r.yMax,existing.yMax);
                    void Keep(float a,float b,float c,float d){if(c-a>.00001f&&d-b>.00001f)next.Add(Rect.MinMaxRect(a,b,c,d));}
                    Keep(r.xMin,r.yMin,r.xMax,y0);Keep(r.xMin,y1,r.xMax,r.yMax);Keep(r.xMin,y0,x0,y1);Keep(x1,y0,r.xMax,y1);
                }
                pending=next;if(pending.Count==0)break;
            }
            regions.AddRange(pending);
        }

        public static void MigrateSavedScenes()
        {
            var rows=new List<string>{"display,content,glassPanes,coatedRegions"};
            var args=Environment.GetCommandLineArgs();int at=Array.IndexOf(args,"-coghe-glass-levels");
            var only=at>=0&&at+1<args.Length?args[at+1].Split(','):Array.Empty<string>();
            foreach(var key in VenomCampaignBuilder.SpatialOrder)
            {
                if(only.Length>0&&!only.Contains(key))continue;
                var scene=EditorSceneManager.OpenScene(VenomCampaignBuilder.SpatialContentPath(key));
                var game=Object.FindFirstObjectByType<VenomCampaign>();Configure(key,game);
                COgheSoftEdges.RefreshTape(char.IsDigit(key[0])?key:"Plus"+key,game);
                EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
                rows.Add($"{game.Definition.Order},{key},{game.Surfaces.Count(p=>p.ExteriorGlass)},{game.Surfaces.Sum(p=>p.SlipRegions?.Length??0)}");
            }
            Directory.CreateDirectory("Artifacts/GlassRule20261014");File.WriteAllLines("Artifacts/GlassRule20261014/coatings.csv",rows);
            AssetDatabase.SaveAssets();
        }
    }
}
