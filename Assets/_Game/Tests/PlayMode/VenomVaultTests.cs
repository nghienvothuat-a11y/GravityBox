#if UNITY_EDITOR
using System;
using System.Collections;
using System.Linq;
using System.IO;
using UnityEngine.Rendering;
using GravityBox.Venom;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;

namespace GravityBox.Tests
{
    public sealed class VenomVaultTests
    {
        private VenomLevelController level;
        private SimulationMode mode;
        private const float Dt=1f/120;
        private Vector3 Centre=>level.Rotation.transform.InverseTransformPoint(level.Locomotion.Selected.Centre);
        [UnitySetUp] public IEnumerator Setup()
        {
            mode=Physics.simulationMode;Physics.simulationMode=SimulationMode.Script;
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/_Game/Venom/Venom05.unity",new LoadSceneParameters(LoadSceneMode.Single));
            level=Object.FindFirstObjectByType<VenomLevelController>();
            level.enabled=false;level.Rotation.enabled=false;level.GetComponent<VenomInput>().enabled=false;level.ResetExperiment();
        }
        [UnityTearDown] public IEnumerator Teardown()
        {
            foreach(var root in SceneManager.GetActiveScene().GetRootGameObjects())Object.Destroy(root);
            yield return null;Physics.simulationMode=mode;Time.timeScale=1;
        }
        private void Steps(int n){for(int i=0;i<n;i++){level.Step(Dt);level.Rotation.Step(Dt);Physics.Simulate(Dt);}}
        private void Go(float x,float z,int ticks=1400)
        {
            for(int i=0;i<ticks;i++)
            {
                Vector3 delta=new Vector3(x-Centre.x,0,z-Centre.z);
                if(delta.magnitude<.01f)break;
                level.Locomotion.SetInput(level.Rotation.transform.TransformDirection(Vector3.ClampMagnitude(delta/.025f,1)));Steps(1);
            }
            Debug.Log($"VAULT THROAT amount={level.Locomotion.Selected.Squeeze.Amount:F2} axis={level.Locomotion.Selected.Squeeze.Axis:F2} at={level.Locomotion.Selected.Squeeze.Throat:F3}");
            level.Locomotion.SetInput(Vector3.zero);Steps(60);
            Debug.Log($"VAULT GO {x:F3},{z:F3}: {Centre:F3} parts={level.Organism.FragmentCount} cut={level.Organism.CutCount} released={level.SplitVault.FollowersReleased}");
            Assert.That(Vector2.Distance(new Vector2(Centre.x,Centre.z),new Vector2(x,z)),Is.LessThan(.04f));
            Assert.That(Centre.y,Is.GreaterThan(.19f),"Ceiling route cannot be replaced by falling under the vault.");
        }
        private void CutFromSpawn()
        {
            Steps(120);level.Locomotion.SetInput(Vector3.right);
            for(int i=0;i<2400 && !(Centre.y>.215f && Centre.x<.19f);i++)Steps(1);
            level.Locomotion.SetInput(Vector3.zero);Steps(90);
            Assert.That(Centre.y,Is.GreaterThan(.20f));
            Go(.183f,.174f);Go(.128f,.174f);Go(.128f,.115f);
            Debug.Log("VAULT CUT "+string.Join("; ",level.Locomotion.Fragments.Select(f=>$"{f.Anchor}:{f.Count}@{f.Centre:F3}")));
            Assert.That(level.Organism.CutCount,Is.GreaterThan(0));Assert.That(level.Organism.FragmentCount,Is.GreaterThanOrEqualTo(2));
            Assert.That(level.Locomotion.Selected.Count,Is.EqualTo(level.Locomotion.Fragments.Max(f=>f.Count)));
        }
        [Test] public void KnifeLeaderEntryThenFollowersSolveWithoutSwitching()
        {
            Capture("05-overview");CutFromSpawn();Capture("05-cut");Go(0,.138f);
            var follower=level.Locomotion.Fragments.First(f=>!f.Selected);int followerId=follower.Anchor;
            Vector3 parked=follower.Centre;
            Capture("05-waiting");Steps(720);Assert.That(level.SplitVault.FollowersReleased,Is.False);Assert.That(level.Locomotion.PathSearches,Is.Zero);
            follower=level.Locomotion.Fragments.First(f=>f.Group==level.Organism.Groups[followerId]);
            Assert.That(Vector3.Distance(follower.Centre,parked),Is.LessThan(.04f));
            Assert.That(level.Locomotion.Select(followerId),Is.False);
            for(int i=0;i<2400&&!level.SplitVault.FollowersReleased;i++)
            {
                level.Locomotion.SetInput(level.Rotation.transform.TransformDirection(Vector3.ClampMagnitude(new Vector3(-Centre.x,0,.031f-Centre.z)/.025f,1)));Steps(1);
            }
            Capture("05-leader-entered");Debug.Log($"VAULT ENTRY released={level.SplitVault.FollowersReleased} entered={level.SplitVault.EntryCount} out={level.Organism.EscapedCount}");
            Assert.That(level.SplitVault.FollowersReleased,Is.True,"Follower starts only when every node of the leader has entered the vault.");
            level.Locomotion.SetInput(Vector3.zero);
            bool auto=false;
            for(int i=0;i<3600&&!level.Completed&&!level.Lost;i++)
            {
                if(level.Locomotion.Selected!=null && !level.SplitVault.AutoFollow(level.Locomotion.Selected))
                    level.Locomotion.SetInput(Vector3.ClampMagnitude(Vector3.ProjectOnPlane(level.Outlet.position-level.Locomotion.Selected.Centre,level.Rotation.transform.up)/.025f,1));
                Steps(1);auto|=level.Locomotion.Fragments.Any(f=>f.Following);
            }
            Debug.Log($"VAULT EXIT {level.Organism.EscapedCount}/32 parts={level.Organism.FragmentCount} searches={level.Locomotion.PathSearches} lost={level.Lost}");
            if(!level.Completed)Debug.Log(string.Join("; ",level.Organism.Bodies.Select((b,i)=>$"{i}:out={level.Organism.Escaped[i]} p={b.position:F4}")));
            Assert.That(auto,Is.True);Assert.That(level.Completed,Is.True);Assert.That(level.Organism.EscapedCount,Is.EqualTo(32));
            Assert.That(level.Organism.TotalMass,Is.EqualTo(.096f).Within(.000001f));
            Steps(180);Capture("05-all-escaped");
            level.ResetExperiment();Assert.That(level.SplitVault.FollowersReleased,Is.False);
            Assert.That(level.SplitVault.Captain,Is.EqualTo(-1));Assert.That(level.Organism.CutCount,Is.Zero);
            Assert.That(level.Locomotion.Selected.Count,Is.EqualTo(32));
        }
        [Test] public void VaultHasOnlyNarrowEntryAndResetClearsRelease()
        {
            Steps(120);var vault=level.SplitVault;var nav=level.Locomotion.Navigator;
            Vector3 outside=new Vector3(0,.227f,.16f),inside=new Vector3(0,.227f,.049f);
            Assert.That(nav.Clear(outside,inside,.029f),Is.False);Assert.That(nav.Clear(outside,inside,.011f),Is.True);
            Assert.That(nav.Clear(new Vector3(.15f,.227f,0),Vector3.zero,.011f,true),Is.False);
            Assert.That(level.SegmentBlocked(new Vector3(0,.10f,0),new Vector3(0,.22f,0)),Is.True,"The underside is physically closed.");
            level.ResetExperiment();Assert.That(vault.FollowersReleased,Is.False);Assert.That(vault.Captain,Is.EqualTo(-1));
            Assert.That(level.Organism.CutCount,Is.Zero);Assert.That(level.Locomotion.PathSearches,Is.Zero);
        }
        [Test] public void IntactCreatureCannotThreadTheNarrowDoor()
        {
            Steps(120);Vector3 delta=new Vector3(0,.226f,.137f)-Centre;
            foreach(var body in level.Organism.Bodies){body.position+=delta;body.linearVelocity=Vector3.zero;}
            Physics.SyncTransforms();Steps(90);level.Locomotion.SetInput(Vector3.back);Steps(1200);
            Assert.That(level.Organism.CutCount,Is.Zero);Assert.That(level.Completed,Is.False);
            Assert.That(level.Organism.Bodies.All(b=>level.SplitVault.IsInside(b.position)),Is.False);
            Assert.That(level.SplitVault.FollowersReleased,Is.False);
            Assert.That(level.Locomotion.Selected.Squeeze.Amount,Is.Zero);
        }
        [TestCase(.128f)] [TestCase(.154f)] public void ActualKnifePositionDeterminesLargerPiece(float x)
        {
            // Different initial approach offsets, then forces move the body onto
            // the authored knife. No artificial group IDs or fixed split ratios.
            Steps(120);Vector3 delta=new Vector3(x,.225f,.177f)-Centre;
            foreach(var b in level.Organism.Bodies){b.position+=delta;b.linearVelocity=Vector3.zero;}
            Physics.SyncTransforms();Steps(90);level.Locomotion.SetInput(Vector3.back);
            for(int i=0;i<400 && level.Organism.CutCount==0;i++)Steps(1);
            level.Locomotion.SetInput(Vector3.zero);Steps(1);
            Debug.Log($"KNIFE OFFSET {x:F3}: "+string.Join(",",level.Locomotion.Fragments.Select(f=>f.Count)));
            Assert.That(level.Organism.FragmentCount,Is.GreaterThanOrEqualTo(2));
            Assert.That(level.Locomotion.Selected.Count,Is.EqualTo(level.Locomotion.Fragments.Max(f=>f.Count)));
            Assert.That(level.SplitVault.FollowersReleased,Is.False);
        }
        private void Capture(string name)
        {
            string folder=Environment.GetEnvironmentVariable("VENOM_CAPTURE_DIR");
            if(string.IsNullOrEmpty(folder)||SystemInfo.graphicsDeviceType==GraphicsDeviceType.Null)return;
            Directory.CreateDirectory(folder);level.Organism.GetComponent<VenomSurface>().Rebuild(false);
            level.GetComponent<VenomGlassVisibility>().Refresh();
            level.GetComponent<VenomHud>().RefreshSelection();level.View.aspect=720f/1280;
            level.GetComponent<VenomHud>().FrameChamber(720,1280);
            var target=new RenderTexture(720,1280,24){antiAliasing=4};target.Create();var old=RenderTexture.active;
            var picture=new Texture2D(720,1280,TextureFormat.RGBA32,false);
            try
            {
                var request=new RenderPipeline.StandardRequest{destination=target};
                RenderPipeline.SubmitRenderRequest(level.View,request);RenderPipeline.SubmitRenderRequest(level.View,request);
                RenderTexture.active=target;picture.ReadPixels(new Rect(0,0,720,1280),0,0);picture.Apply();
                File.WriteAllBytes(Path.Combine(folder,name+".png"),picture.EncodeToPNG());
            }
            finally{RenderTexture.active=old;target.Release();Object.DestroyImmediate(target);Object.DestroyImmediate(picture);}
        }
    }
}
#endif
