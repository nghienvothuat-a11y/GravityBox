using System.Collections;
using System.Linq;
using GravityBox.Venom;
using GravityBox.Venom.ChapterProof;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace GravityBox.Tests
{
 // COghe sound: boots once, keeps one music loop, and follows real events of a real solve without spamming. The
 // harness calls COgheAudio.Observe after every physics tick, so each transition is seen.
 public sealed partial class COgheSpatialCampaignTests
 {
  static readonly string[] AudioClips={"music_lab_loop","creature_crawl_loop","mech_motor_loop","block_slide_loop","mech_arrive","ui_tap",
   "creature_ack_1","creature_ack_2","creature_ack_3","creature_happy","creature_curious_1","creature_curious_2","creature_hm",
   "creature_land","creature_grab","creature_split","creature_merge","creature_merge_full","tube_in","tube_out","creature_swing",
   "creature_exit","mech_latch","mech_gear_mesh","mech_pad_on","mech_pad_off","mech_lift_ding","game_win","game_fail","game_retry",
   "game_level_start","lab_far_beep","lab_far_clink","lab_far_thud"};
  private int Heard(string name)=>COgheAudio.Instance.Played.TryGetValue(name,out int n)?n:0;
  private float loudestMotor,loudestSlide;
  private IEnumerator SolveHearing(string key)
  {
   yield return LoadPlus(key);yield return null;
   var audio=COgheAudio.Instance;Assert.IsNotNull(audio);Assert.AreSame(game,audio.Game,"Audio follows the loaded level");
   audio.Played.Clear();loudestMotor=loudestSlide=0;
   trace=()=>{loudestMotor=Mathf.Max(loudestMotor,audio.MotorLevel);loudestSlide=Mathf.Max(loudestSlide,audio.SlideLevel);};
   try{yield return new COgheSpatialScenario(game,Tap,Until).Solve();}finally{trace=null;}
   Assert.AreEqual(1,Heard("game_win"),"One win stinger");Assert.AreEqual(1,Heard("creature_exit"),"One exit");
   Assert.GreaterOrEqual(Heard("creature_ack_1")+Heard("creature_ack_2")+Heard("creature_ack_3"),2,"The creature answers commands");
   Debug.Log($"COGHE_AUDIO {key} motor≤{loudestMotor:F2} slide≤{loudestSlide:F2}: "+string.Join(", ",audio.Played.OrderByDescending(p=>p.Value).Select(p=>$"{p.Key}×{p.Value}")));
   foreach(var pair in audio.Played)Assert.Less(pair.Value,80,$"{pair.Key} does not spam ({pair.Value} times)");
  }
  [UnityTest] public IEnumerator AudioBootsOnceWithOneMusicLoop()
  {
   foreach(var name in AudioClips)Assert.IsNotNull(Resources.Load<AudioClip>("COgheAudio/"+name),"Clip "+name);
   yield return Load(1);for(int i=0;i<30;i++)yield return null;
   var audio=COgheAudio.Instance;Assert.IsNotNull(audio,"Audio boots at play start");
   Assert.AreEqual(1,Object.FindObjectsByType<COgheAudio>(FindObjectsSortMode.None).Length,"One audio object");
   Assert.AreEqual("music_lab_loop",audio.Music.clip.name);Assert.IsTrue(audio.Music.loop);
   if(COgheAudio.MusicOn)Assert.Greater(audio.Music.volume,0,"Music fades in on a Spatial level");
   var music=audio.Music;yield return Load(2);for(int i=0;i<5;i++)yield return null;
   Assert.AreSame(audio,COgheAudio.Instance);Assert.AreSame(music,COgheAudio.Instance.Music,"The same music source carries across levels");
  }
  [UnityTest] public IEnumerator AudioHearsPadsGearsAndLatches()
  {
   yield return SolveHearing("E14");
   Assert.GreaterOrEqual(Heard("mech_pad_on"),2,"Pad P, twice");Assert.GreaterOrEqual(Heard("mech_gear_mesh"),2,"Both layers mesh");
   Assert.GreaterOrEqual(Heard("mech_arrive"),3,"Gate, carriages and bridge arrive");Assert.GreaterOrEqual(Heard("creature_grab"),2,"Grabs A and B");
   Assert.Greater(loudestMotor,.5f,"The gate and the bridge run the motor");Assert.Greater(loudestSlide,.3f,"Carriages slide on the table");
  }
  [UnityTest] public IEnumerator AudioHearsBlocksSlidingOnTheFloor()
  {
   yield return SolveHearing("E04");
   Assert.Greater(loudestSlide,.5f,"Cart A and crate B slide");Assert.GreaterOrEqual(Heard("mech_arrive"),2,"Both arrive");
   Assert.Less(loudestMotor,.3f,"Nothing lifts in 21: no motor");
  }
  [UnityTest] public IEnumerator AudioHearsALooseCratePushed()
  {
   yield return SolveHearing("E01");
   Assert.Greater(loudestSlide,.3f,"The loose crate slides off the tray");Assert.GreaterOrEqual(Heard("mech_lift_ding"),1,"The lift arrives");
  }
  [UnityTest] public IEnumerator AudioHearsSplitTubesAndMerge()
  {
   yield return SolveHearing("E13");
   Assert.AreEqual(1,Heard("creature_split"),"One split in Q");Assert.GreaterOrEqual(Heard("tube_in"),2);Assert.GreaterOrEqual(Heard("tube_out"),2);
   Assert.AreEqual(1,Heard("creature_merge_full"),"Whole again, once");
  }
  [UnityTest] public IEnumerator AudioHearsRopeSwings()
  {
   yield return SolveHearing("E05");
   Assert.AreEqual(2,Heard("creature_swing"),"Two ropes");Assert.GreaterOrEqual(Heard("creature_land"),2,"Two landings");
  }
  [UnityTest] public IEnumerator AudioRetryIsOneRewindNotAMerge()
  {
   yield return LoadPlus("E03");yield return null;var audio=COgheAudio.Instance;
   var q=game.Owner.Apparatus.GetComponentInChildren<COgheQuantumSplitter>();var s=new COgheSpatialNextScenario(game,Tap,Until);
   yield return s.Split(q,game.Motion.Selected);yield return Wait(1);
   audio.Played.Clear();game.ResetLevel();yield return Wait(1);
   Assert.AreEqual(1,Heard("game_retry"),"Retry rewinds");Assert.AreEqual(0,Heard("creature_merge")+Heard("creature_merge_full"),"Retry is not a merge");
  }
  [UnityTest] public IEnumerator AudioEffectsSwitchOff()
  {
   bool music=COgheAudio.MusicOn,effects=COgheAudio.EffectsOn;
   try
   {
    yield return Load(1);yield return null;COgheAudio.EffectsOn=false;COgheAudio.MusicOn=false;
    for(int i=0;i<90;i++)yield return null;
    var audio=COgheAudio.Instance;Assert.AreEqual(0,audio.Music.volume,.001f,"Music off");
    Assert.AreEqual(0,audio.Motor.volume,.001f,"Motor off");Assert.AreEqual(0,audio.Slide.volume,.001f,"Slide off");
    audio.Play("mech_latch",1);Assert.IsFalse(Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None).Any(a=>a.isPlaying&&a.clip!=null&&a.clip.name=="mech_latch"),"Effects off: nothing plays");
   }
   finally{COgheAudio.MusicOn=music;COgheAudio.EffectsOn=effects;}
  }
 }
}
