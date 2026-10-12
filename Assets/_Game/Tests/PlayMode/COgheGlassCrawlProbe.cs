using System;
using System.Collections;
using System.IO;
using NUnit.Framework;
using GravityBox.Venom;
using UnityEngine;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;

namespace GravityBox.Tests
{
 // Can COghe crawl the glass in 3D (Mrk 12/10/2026: five levels where COghe may crawl on the glass)? Explicit probe:
 // level 1 with every pane grippy; taps on a wall high up, the roof's middle, the far wall, then the floor; after each,
 // where the body is and how many particles hold the glass. Frames in Artifacts/GlassCrawl.
 public sealed partial class COgheSpatialCampaignTests
 {
  [UnityTest,Explicit,Timeout(900000)] public IEnumerator GlassCrawlProbe()
  {
   Directory.CreateDirectory("Artifacts/GlassCrawl");var log=new System.Text.StringBuilder();
   yield return LoadScene("COgheSpatial01");yield return Wait(1);
   // the roof takes taps from above: needs VenomCampaign.CanPick to honour InterceptExterior in ViewOnly levels (the one-line
   // change in Docs/LevelDesign/COghe/GlassCrawl5/README.md); without it the roof stops are not reached
   foreach(var f in game.Surfaces)if(f!=null&&f.ExteriorGlass){f.Slippery=false;if(f.Normal.y<-.9f)f.InterceptExterior=true;}
   game.Motion.BuildGraph(true);
   var cam=Object.Instantiate(game.Owner.View);cam.enabled=false;cam.orthographic=true;cam.orthographicSize=.36f;cam.aspect=1.5f;
   cam.transform.rotation=game.Root.rotation*Quaternion.Euler(22,-30,0);cam.transform.position=game.Root.TransformPoint(new Vector3(0,-.10f,0))-cam.transform.forward*3f;
   var stops=new (string name,Vector3 local)[]{("left wall high",new Vector3(-.40f,0f,0)),("roof middle",new Vector3(0,.10f,0)),("roof far corner",new Vector3(.30f,.10f,.22f)),
                                             ("right wall low",new Vector3(.40f,-.18f,-.10f)),("back wall high",new Vector3(.10f,.02f,.30f)),("floor",new Vector3(-.10f,-.30f,-.10f))};
   int k=0;
   foreach(var (name,local) in stops)
   {
    var target=game.Root.TransformPoint(local);
    yield return Tap(target);var o=game.Motion.Get(0);int path=o!=null?o.Path.Count:-1;
    yield return WaitFor(25,()=>Vector3.Distance(game.Motion.Centre(0),target)<.06f||game.Motion.Get(0)==null);yield return Wait(1.5f);
    var c=game.Root.InverseTransformPoint(game.Motion.Centre(0));int onGlass=0,onRoof=0;float top=-1;
    for(int i=0;i<32;i++)
    {
     var p=game.Root.InverseTransformPoint(game.Matter.Bodies[i].position);top=Mathf.Max(top,p.y);
     if(p.y>.07f)onRoof++;if(Mathf.Abs(p.x)>.37f||Mathf.Abs(p.z)>.27f||p.y>.07f)onGlass++;
    }
    log.AppendLine($"{name}: path={path} centre={c:F3} dist={Vector3.Distance(c,local):F3} topY={top:F3} onGlass={onGlass} onRoof={onRoof} parts={game.Matter.TotalFragmentCount} activity={game.Activity}");
    SoftShot(cam,$"Artifacts/GlassCrawl/{k++:00}-{name.Replace(' ','-')}.png",1200,800);
   }
   File.WriteAllText("Artifacts/GlassCrawl/probe.txt",log.ToString());Debug.Log("GLASS CRAWL\n"+log);
   Object.Destroy(cam.gameObject);
  }
 }
}
