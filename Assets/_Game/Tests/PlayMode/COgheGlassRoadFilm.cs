using System;
using System.Collections;
using System.IO;
using GravityBox.Venom;
using GravityBox.Venom.ChapterProof;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace GravityBox.Tests
{
 // Glass roads 66–68 on film (Explicit; Mrk 18/10/2026): a turn of the camera round the box, then the reference solution as a
 // player plays it. Frames (15 a second, JPG) and the step each frame belongs to go to GLASSROAD_FILM/<key>/.
 public sealed partial class COgheSpatialCampaignTests
 {
  [UnityTest,Explicit,Timeout(3600000)] public IEnumerator GlassRoadFilm()
  {
   string root=Environment.GetEnvironmentVariable("GLASSROAD_FILM")??"Artifacts/GlassRoadFilm";
   foreach(var key in (Environment.GetEnvironmentVariable("GLASSROAD_KEYS")??"G6,G7,G8,G9").Split(','))
   {
    yield return LoadPlus(key);yield return Wait(.5f);
    string dir=$"{root}/{key}";Directory.CreateDirectory(dir);int frame=0;var marks=new System.Text.StringBuilder();string last="";
    void Mark(string what){if(what!=last){marks.AppendLine($"{frame}\t{what}");last=what;}}
    // a slow turn round the box: the roads on every wall
    Mark("orbit");
    for(int i=0;i<90;i++){game.CameraRig.Orbit(-720f*4f/240f,720);Tick();FilmFrame($"{dir}/f{frame++:00000}.jpg");if(i%8==0)yield return null;}
    for(int i=0;i<15;i++){Tick();FilmFrame($"{dir}/f{frame++:00000}.jpg");}
    IEnumerator Film(float seconds,Func<bool> done,string reason)
    {
     Mark(reason);
     for(int i=0;i<seconds/Dt&&!done()&&!game.Owner.Lost;i++){Tick();if(i%8==7){FilmFrame($"{dir}/f{frame++:00000}.jpg");yield return null;}}
     Assert.IsTrue(done(),reason);
    }
    yield return new COgheSpatialScenario(game,Tap,Film).Solve();
    Mark("won");for(int i=0;i<30;i++){for(int k=0;k<8;k++)Tick();FilmFrame($"{dir}/f{frame++:00000}.jpg");yield return null;}
    File.WriteAllText($"{dir}/marks.tsv",marks.ToString());
    Assert.AreEqual(32,game.Matter.EscapedCount,key);
   }
  }
 }
}
