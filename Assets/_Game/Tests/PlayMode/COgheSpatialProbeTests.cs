#if UNITY_EDITOR
using System.Collections;
using GravityBox.Venom;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace GravityBox.Tests
{
 // Engineering probe (not solution evidence): measures which slick riser heights the body surmounts from the floor.
 public sealed partial class COgheSpatialCampaignTests
 {
  [UnityTest] public IEnumerator SpatialStepProbe()
  {
   yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/_Game/Venom/SpatialCampaign/Probe/COgheSpatialProbe.unity",new LoadSceneParameters(LoadSceneMode.Single));
   game=Object.FindFirstObjectByType<VenomCampaign>();game.AutoAdvance=false;game.Owner.enabled=false;game.Owner.Rotation.enabled=false;yield return Wait(1);
   float[] heights={.02f,.035f,.05f,.065f,.08f};string report="";
   for(int i=0;i<heights.Length;i++)
   {
    game.ResetLevel();yield return Wait(.5f);
    var top=game.Root.TransformPoint(new Vector3(-.32f+i*.16f,-.30f+heights[i],.08f));
    game.Motion.Move(game.Motion.Selected,top+Vector3.up*.019f);
    float best=-1;for(int k=0;k<20*120;k++){Tick();if(k%240==0)yield return null;best=Mathf.Max(best,game.Motion.Centre(0).y);}
    bool reached=game.Motion.Centre(0).y>top.y+.012f&&Vector3.Distance(new Vector3(game.Motion.Centre(0).x,0,game.Motion.Centre(0).z),new Vector3(top.x,0,top.z))<.06f;
    report+=$"h={heights[i]:F3} reached={reached} centreY={game.Motion.Centre(0).y-top.y:F3}; ";
   }
   Debug.Log("SPATIAL STEP PROBE "+report);
  }
 }
}
#endif
