#if UNITY_EDITOR || DEVELOPMENT_BUILD || COGHE_MOBILE_BENCHMARK
using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace GravityBox.Venom
{
 public sealed class COgheRefinementProof:MonoBehaviour
 {
  string output;VenomCampaign game;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]static void Boot()
  {
   if(Array.IndexOf(Environment.GetCommandLineArgs(),"-coghe-refinement-review")<0)return;
   VenomCampaignSave.PersistenceEnabled=false;COgheProductMode.OverrideForTests=true;COgheIntro.SeenOverrideForTests=true;
   COgheProductMode.SessionStarted=true;
   var go=new GameObject("Refinement review");DontDestroyOnLoad(go);go.AddComponent<COgheRefinementProof>();
  }
  IEnumerator Load(string key)
  {
   yield return SceneManager.LoadSceneAsync(char.IsDigit(key[0])?"COgheSpatial"+key:"COgheSpatialPlus"+key);
   yield return null;yield return null;game=FindFirstObjectByType<VenomCampaign>();
   float timeout=Time.realtimeSinceStartup+10;
   while(game.ProductUI.Page!=COgheProductPage.Game&&Time.realtimeSinceStartup<timeout)yield return null;
   if(game.ProductUI.Page!=COgheProductPage.Game)throw new InvalidOperationException("Review scene did not enter gameplay: "+key);
   if(!SceneManager.GetActiveScene().name.EndsWith(key))throw new InvalidOperationException("Review scene changed unexpectedly: "+key);
   Debug.Log("REFINEMENT LOOK "+key+" => "+game.Definition.Id+" display "+game.Definition.Order);
   game.ProductUI.OnGameplayInteraction();
   yield return new WaitForSeconds(.35f);
  }
  void CloseView(float ratio)
  {
   game.CameraRig.PinchAt(ratio,game.Owner.View.WorldToScreenPoint(game.Motion.Centre(game.Motion.Selected)));
  }
  IEnumerator Shot(string name)
  {ScreenCapture.CaptureScreenshot(Path.Combine(output,name+".png"));yield return new WaitForEndOfFrame();yield return null;}
  IEnumerator Film(string name,float seconds)
  {
   Directory.CreateDirectory(Path.Combine(output,name));float end=Time.realtimeSinceStartup+seconds;int frame=0;
   while(Time.realtimeSinceStartup<end){yield return Shot(name+"/"+(frame++).ToString("D4"));yield return new WaitForSecondsRealtime(.08f);}
  }
  IEnumerator Start()
  {
   var args=Environment.GetCommandLineArgs();int at=Array.IndexOf(args,"-coghe-refinement-review");
   output=Path.GetFullPath(at+1<args.Length?args[at+1]:"Artifacts/COghePlaytestRefinement/Native");Directory.CreateDirectory(output);
   Application.runInBackground=true;Screen.SetResolution(720,960,FullScreenMode.Windowed);
   yield return new WaitForSecondsRealtime(2);
   COgheShop.ResetForTests(new COgheShop.State{Migrated=true,InkGiftRevision=1,Agility=1,LastAbility=1});
   yield return Load("01");CloseView(1.7f);
   yield return new WaitForSeconds(.4f);
   game.TouchPoint(game.Owner.View.WorldToScreenPoint(game.Root.TransformPoint(new Vector3(.12f,-.30f,-.07f))));
   yield return Film("speedy",4.5f);
   COgheShop.ResetForTests(new COgheShop.State{Migrated=true,InkGiftRevision=1,Intelligence=1,LastAbility=2});
   yield return Load("01");CloseView(2.7f);yield return Film("clever",7.5f);
   COgheShop.ResetForTests(new COgheShop.State{Migrated=true,InkGiftRevision=1});
   yield return Load("01");CloseView(2.7f);
   COgheAccessories.Attach(game).Dress("HAT_FLOWER",Array.Empty<string>());yield return new WaitForSeconds(1);yield return Shot("flower-crown");
   foreach(var key in new[]{"F1","N13","13","B1","22"})
   {
    yield return Load(key);yield return Shot("level-"+game.Definition.Order.ToString("D2")+"-start");
   }
   Debug.Log("REFINEMENT NATIVE LOOKS COMPLETE");Application.Quit(0);
  }
 }
}
#endif
