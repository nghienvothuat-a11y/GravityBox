using System;
using System.Collections;
using GravityBox.Venom;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace GravityBox.Tests
{
 // Crate taps (Mrk 07/10/2026): "Khi click vào mặt khối, COghe phải đẩy hoặc kéo (trừ khi bị kịch đường). Ví dụ lần thứ
 // nhất đẩy thì lần thứ hai kéo"; 08/10/2026: any face, a long side too. Played through real taps on the crates, as a
 // player does.
 public sealed partial class COgheSpatialCampaignTests
 {
  private COgheTapRail CrateTask(string name)=>Array.Find(game.Owner.Apparatus.GetComponentsInChildren<COgheTapRail>(),t=>t.Rail.name==name);
  /// <summary>A point on a crate the camera sees first (the glass box lets the tap through), or null.</summary>
  private Vector3? CrateSpot(COgheTapRail t,params Vector3[] candidates)
  {
   var cam=game.Owner.View;
   foreach(var p in candidates)
   {
    var ray=cam.ScreenPointToRay(cam.WorldToScreenPoint(p));
    var hits=Physics.RaycastAll(ray,10);Array.Sort(hits,(a,b)=>a.distance.CompareTo(b.distance));
    foreach(var h in hits)
    {
     var pane=h.collider.GetComponent<VenomSurfacePatch>();
     if(h.collider.isTrigger||h.collider.GetComponent<VenomContact>()!=null||pane!=null&&pane.ExteriorGlass)continue;
     if(h.rigidbody==t.Rail.Body)return p;
     break;
    }
   }
   return null;
  }
  private Vector3 EndSpot(COgheTapRail t,int side)
  {
   Vector3 along=t.Rail.WorldAxis,local=t.Rail.Frame.InverseTransformDirection(along),up=game.Root.up;
   float half=Mathf.Abs(local.x)*t.CrateSize.x*.5f+Mathf.Abs(local.z)*t.CrateSize.z*.5f;
   var spot=CrateSpot(t,t.Rail.Body.position+along*side*half*.78f+up*t.CrateSize.y*.5f,t.Rail.Body.position+along*side*half*.62f+up*t.CrateSize.y*.5f);
   Assert.IsTrue(spot.HasValue,"The camera sees that end of "+t.Rail.name);return spot.Value;
  }

  [UnityTest] public IEnumerator TheSameEndOfACrateTappedTwicePushesThenPulls()
  {
   yield return LoadPlus("K01");yield return Wait(.5f);
   // the same end tapped twice: away from it a push, then toward it a pull (wherever COghe has room at that end; else
   // it works the crate from the other end). Find a crate and an end that allow both, and play it.
   bool alternated=false;
   foreach(var t in game.Owner.Apparatus.GetComponentsInChildren<COgheTapRail>())
   {
    if(alternated||!t.CrateFaces)continue;
    foreach(int s in new[]{1,-1})
    {
     if(alternated||!t.CanWorkFrom(s))continue;
     bool pushFirst=s!=t.NextMoveDirection;int done=t.CompletedJourneys;
     yield return Tap(EndSpot(t,s));Assert.IsTrue(t.Busy,"The tapped end: "+t.LastFailure);
     Assert.AreEqual(s,t.WorkingSide,"COghe works from the end that was tapped");Assert.AreEqual(!pushFirst,t.Pulling);
     yield return Until(20,()=>t.CompletedJourneys==done+1,"It slides");
     if(!t.CanWorkFrom(s))continue;   // no room at that end now: not the case to show
     yield return Tap(EndSpot(t,s));Assert.IsTrue(t.Busy,"The same end again: "+t.LastFailure);
     Assert.AreEqual(s,t.WorkingSide);Assert.AreEqual(pushFirst,t.Pulling,"The other way now: push then pull (or pull then push)");
     yield return Until(20,()=>t.CompletedJourneys==done+2,"It slides back");
     alternated=true;
    }
   }
   Assert.IsTrue(alternated,"Some crate shows the same end pushing then pulling");
  }

  // Mrk 08/10/2026: "lúc đẩy và kéo thùng, phải cho COghe mặt nào cũng đẩy bám được, cho thật hơn". A tap near a crate's
  // long edge takes it by that side: COghe grips the side and walks along with the crate, and ends beside it.
  [UnityTest] public IEnumerator ACrateTakenByALongSideSlidesWithCOgheBesideIt()
  {
   yield return LoadPlus("K01");yield return Wait(.5f);
   bool shown=false;
   foreach(var t in game.Owner.Apparatus.GetComponentsInChildren<COgheTapRail>())
   {
    if(shown||!t.CrateFaces)continue;
    Vector3 up=game.Root.up,along=t.Rail.WorldAxis,across=Vector3.Cross(up,along).normalized;
    var l=t.Rail.Frame.InverseTransformDirection(across);float half=Mathf.Abs(l.x)*t.CrateSize.x*.5f+Mathf.Abs(l.z)*t.CrateSize.z*.5f;
    foreach(int s in new[]{1,-1})
    {
     if(shown)continue;
     var spot=CrateSpot(t,t.Rail.Body.position+across*s*half*.78f+up*t.CrateSize.y*.5f,t.Rail.Body.position+across*s*half*.66f+up*t.CrateSize.y*.5f);
     if(!spot.HasValue)continue;
     // the side the tap names, if that side has room (TapRail's across is up × axis in its frame, as here)
     if(!t.CanWorkAcross(s))continue;
     int done=t.CompletedJourneys;float before=t.Rail.Position;
     yield return Tap(spot.Value);
     Assert.IsTrue(t.Busy,"A tap near the long edge takes the crate: "+t.LastFailure);
     Assert.AreEqual(s,t.WorkingAcross,"COghe holds the side that was tapped");
     yield return Until(25,()=>t.CompletedJourneys==done+1,"It slides with COghe walking beside it");
     Assert.Greater(Mathf.Abs(t.Rail.Position-before),t.Rail.Travel*.9f);
     float beside=Vector3.Dot(game.Motion.Centre(0)-t.Rail.Body.position,across)*s;
     Assert.Greater(beside,half,"COghe is beside the crate, on that side");
     shown=true;
    }
   }
   Assert.IsTrue(shown,"Some crate in level 6 can be taken by a long side");
  }

  [UnityTest] public IEnumerator ABlockedCrateIsRefusedAtOnce()
  {
   yield return LoadPlus("K01");yield return Wait(.5f);
   // the red crate is held in by the others at the start: a tap is refused there and then, and nothing moves
   var red=CrateTask("Red crate");float before=red.Rail.Position;var me=game.Motion.Centre(0);
   foreach(int s in new[]{1,-1})
   {
    var spot=CrateSpot(red,red.Rail.Body.position+red.Rail.WorldAxis*s*.03f+game.Root.up*red.CrateSize.y*.5f);if(!spot.HasValue)continue;
    yield return Tap(spot.Value);
    Assert.IsFalse(red.Busy,"Blocked: COghe does not set off");Assert.IsFalse(string.IsNullOrEmpty(red.LastFailure),"It says why");
   }
   yield return Wait(1);
   Assert.AreEqual(before,red.Rail.Position,.001f,"The red crate has not moved");
   Assert.Less(Vector3.Distance(me,game.Motion.Centre(0)),.02f,"COghe did not walk onto the crate");
  }
 }
}
