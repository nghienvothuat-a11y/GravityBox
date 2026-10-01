using System.Collections;
using System.Linq;
using GravityBox.Venom;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object=UnityEngine.Object;

namespace GravityBox.Tests
{
    // Style (Mrk 01/10, screen by Codex): inks held into COghe, a hat and things inside, saved and worn everywhere.
    public partial class COgheProductUITests
    {
        private IEnumerator EnterStyle(COgheStyle look,int level=50)
        {
            COgheStyle.ResetForTests(look);COgheStyle.Current.ApplyTo(game);
            game.Progress.HomeUnlocked=true;COgheHomeRoom.UnlockedLevelOverride=level;
            yield return Click("Home");Assert.AreEqual(COgheProductPage.Home,ui.Page);yield return new WaitForSecondsRealtime(.3f);
            yield return Click("Style");Assert.AreEqual(COgheProductPage.Style,ui.Page);yield return new WaitForSecondsRealtime(1.2f);
        }
        private void LeaveStyleTest(){COgheStyle.ResetForTests();COgheHomeRoom.UnlockedLevelOverride=null;}
        /// <summary>Middle of COghe's body on screen.</summary>
        private Vector2 OnCOghe()
        {
            var drawn=game.Matter.GetComponent<VenomSurface>().DrawnParticles;var c=Vector3.zero;foreach(var d in drawn)c+=d;
            return game.Owner.View.WorldToScreenPoint(c/drawn.Length);
        }
        private IEnumerator Hold(Vector2 p,float seconds)
        {
            InputSystem.QueueStateEvent(mouse,new MouseState{position=p});yield return null;
            InputSystem.QueueStateEvent(mouse,new MouseState{position=p,buttons=1});yield return null;
            float end=Time.realtimeSinceStartup+seconds;
            while(Time.realtimeSinceStartup<end){InputSystem.QueueStateEvent(mouse,new MouseState{position=p,buttons=1});yield return null;}
            InputSystem.QueueStateEvent(mouse,new MouseState{position=p});yield return null;yield return null;
        }
        /// <summary>Scroll the catalog so a card shows, then tap it.</summary>
        private IEnumerator Card(string id)
        {
            Canvas.ForceUpdateCanvases();
            var card=(RectTransform)ui.GetComponentsInChildren<Button>().Single(b=>b.name=="Card "+id).transform;
            var scroll=card.GetComponentInParent<ScrollRect>();float room=scroll.content.rect.height-scroll.viewport.rect.height;
            scroll.content.anchoredPosition=new Vector2(0,Mathf.Clamp(-card.anchoredPosition.y-4,0,Mathf.Max(0,room)));
            yield return null;yield return Click("Card "+id);
        }
        private COgheInking Inking=>game.Matter.GetComponent<COgheInking>();
        private static float Total(Vector4[] amounts){float t=0;foreach(var a in amounts)t+=a.x+a.y+a.z+a.w;return t;}

        [UnityTest] public IEnumerator StyleHoldInjectsTheChosenInkAndUndoTakesItBack()
        {
            try
            {
                yield return EnterStyle(new COgheStyle());
                Assert.IsTrue(ui.BlockWorldInput,"The room and the creature get no gestures");
                if(game.Personality!=null)Assert.IsTrue(game.Personality.OnStage,"COghe stands still on the stage");
                Assert.IsFalse(ui.GetComponentsInChildren<CanvasGroup>().Single(g=>g.name=="Undo action").interactable,"Nothing to undo yet");
                // a press off the body adds nothing
                var stage=(RectTransform)ui.GetComponentsInChildren<Image>().Single(i=>i.name=="Stage").transform;
                Vector2 corner=RectTransformUtility.WorldToScreenPoint(null,stage.TransformPoint(stage.rect.min+new Vector2(12,12)));
                yield return Hold(corner,.4f);
                Assert.IsTrue(Inking==null||Total(Inking.Amount)<.001f,"Only COghe's skin takes ink");Assert.AreEqual(0,ui.StyleUndoCount);
                // a hold on the body: Ocean goes in where it was held
                int commands=game.Feedback.CommandCount;var start=game.Motion.Centre(0);
                yield return Card("INK_CORAL");Assert.AreEqual("INK_CORAL",ui.StyleInk);Assert.IsTrue(Inking==null,"Choosing a syringe changes nothing");
                yield return Card("INK_OCEAN");
                yield return Hold(OnCOghe(),1.3f);
                Assert.IsFalse(ui.StyleHolding);Assert.IsNotNull(Inking);Assert.AreEqual("INK_OCEAN",Inking.Inks[0]);
                Assert.Greater(Inking.Amount.Max(a=>a.x),.3f,"Ink where it was held");
                Assert.Less(Inking.Amount.Min(a=>a.x),Inking.Amount.Max(a=>a.x)*.6f,"and less far from it");
                Assert.AreEqual(1,ui.StyleUndoCount,"One whole hold, one undo step");
                Assert.AreEqual(commands,game.Feedback.CommandCount,"No world command");
                Assert.Less((game.Motion.Centre(0)-start).magnitude,.02f,"COghe was not steered");
                Assert.IsTrue(ui.GetComponentsInChildren<Text>().Any(t=>t.name=="Name"&&t.text=="Ocean"&&t.transform.parent.name=="Slot 1"),"Your mix shows Ocean");
                // it settles, then saves exactly what is on COghe
                yield return new WaitForSecondsRealtime(2.2f);
                Assert.AreEqual("INK_OCEAN",COgheStyle.Current.Inks[0]);
                for(int i=0;i<32;i++)Assert.AreEqual(Inking.Amount[i],COgheStyle.Current.Amount[i]);
                Assert.IsTrue(ui.GetComponentsInChildren<Text>().Any(t=>t.name=="Status"&&t.text=="Saved"));
                // Undo: black again, saved too
                yield return Click("Undo");yield return null;
                Assert.IsFalse(COgheStyle.Current.HasInk);Assert.AreEqual(0,ui.StyleUndoCount);
                Assert.IsTrue(Inking==null||Total(Inking.Amount)<.001f);
                // Done: back to the room, COghe free again
                yield return Click("Done");Assert.AreEqual(COgheProductPage.Home,ui.Page);
                if(game.Personality!=null)Assert.IsFalse(game.Personality.OnStage);
                Assert.IsFalse(game.transform.GetComponentsInChildren<COgheSyringe>(true).Any(),"The syringe is put away");
            }
            finally{LeaveStyleTest();}
        }

        [UnityTest] public IEnumerator AFifthInkAsksWhichColorItReplacesAndUndoRestoresThePattern()
        {
            var look=new COgheStyle{Inks=new[]{"INK_OCEAN","INK_MINT","INK_CORAL","INK_GOLD"},Seed=3};
            for(int i=0;i<32;i++)look.Amount[i]=new Vector4(i<8?.8f:0,i>=8&&i<16?.8f:0,i>=16&&i<24?.8f:0,i>=24?.8f:0);
            try
            {
                yield return EnterStyle(look);
                var before=(Vector4[])Inking.Amount.Clone();
                yield return Card("INK_LAVA");Assert.AreEqual(COgheProductPopup.StyleReplaceInk,ui.Popup,"Four colors: which one goes?");
                yield return Click("Cancel");Assert.AreEqual(COgheProductPopup.None,ui.Popup);CollectionAssert.AreEqual(look.Inks,Inking.Inks,"Cancel changes nothing");
                yield return Hold(OnCOghe(),.5f);Assert.AreEqual(COgheProductPopup.StyleReplaceInk,ui.Popup,"Holding asks again, adds nothing");
                for(int i=0;i<32;i++)Assert.AreEqual(before[i],Inking.Amount[i]);
                yield return Click("Replace INK_MINT");Assert.AreEqual(1,ui.StyleReplaceSlot);Assert.AreEqual("INK_MINT",Inking.Inks[1],"Choosing only arms the change");
                yield return Hold(OnCOghe(),1f);
                Assert.AreEqual("INK_LAVA",Inking.Inks[1]);Assert.AreEqual(-1,ui.StyleReplaceSlot);
                Assert.Greater(Inking.Amount.Max(a=>a.y),.2f,"Lava went in");
                yield return new WaitForSecondsRealtime(.3f);yield return Click("Undo");yield return null;
                CollectionAssert.AreEqual(look.Inks,Inking.Inks,"Mint is back");
                for(int i=0;i<32;i++)Assert.AreEqual(before[i],Inking.Amount[i],"the same pattern, not an average");
            }
            finally{LeaveStyleTest();}
        }

        [UnityTest] public IEnumerator RinseAsksFirstKeepsTheWardrobeAndCanBeUndone()
        {
            var look=new COgheStyle{Inks=new[]{"INK_OCEAN",null,null,null},Hat="HAT_CAP",Seed=5};
            for(int i=0;i<32;i++)look.Amount[i]=new Vector4(.7f,0,0,0);
            try
            {
                yield return EnterStyle(look);
                yield return Click("Rinse");Assert.AreEqual(COgheProductPopup.StyleRinse,ui.Popup);
                Assert.IsTrue(ui.GetComponentsInChildren<Text>().Any(t=>t.text.StartsWith("COghe returns to its original black.")));
                yield return Click("Cancel");Assert.IsTrue(COgheStyle.Current.HasInk);
                yield return Click("Rinse");yield return Click("Rinse colors");yield return new WaitForSecondsRealtime(1.5f);
                Assert.IsFalse(COgheStyle.Current.HasInk,"Black again (and saved)");Assert.AreEqual("HAT_CAP",COgheStyle.Current.Hat,"The hat stays on");
                Assert.IsTrue(ui.GetComponentsInChildren<Text>().Any(t=>t.name=="Mix count"&&t.text=="0 / 4 colors"),"Your mix empties too");
                yield return Click("Undo");yield return null;
                Assert.AreEqual("INK_OCEAN",Inking.Inks[0]);Assert.AreEqual(.7f,Inking.Amount[5].x,1e-4f,"Undo brings the mix back");
            }
            finally{LeaveStyleTest();}
        }

        [UnityTest] public IEnumerator HatsAndInsideThingsEquipReplaceAndExplainClearInk()
        {
            try
            {
                yield return EnterStyle(new COgheStyle(),35);
                yield return Click("Accessories");Assert.AreEqual(1,ui.StyleTab);
                yield return Card("HAT_CAP");Assert.AreEqual("HAT_CAP",COgheStyle.Current.Hat);
                var wear=game.Matter.GetComponent<COgheAccessories>();Assert.IsNotNull(wear);Assert.AreEqual("HAT_CAP",wear.Hat);
                yield return Card("HAT_STRAW");Assert.AreEqual("HAT_STRAW",COgheStyle.Current.Hat,"One hat: the new one replaces it");
                yield return Card("NONE");Assert.AreEqual("",COgheStyle.Current.Hat);
                // locked: a preview, nothing worn
                yield return Card("HAT_ASTRO");Assert.AreEqual(COgheProductPopup.StyleLocked,ui.Popup);
                Assert.IsTrue(ui.GetComponentsInChildren<Text>().Any(t=>t.text=="Complete level 45 to unlock Space helmet."));
                yield return Click("Close preview");Assert.AreEqual("",COgheStyle.Current.Hat);
                // inside: a black body hides them, so say so (and keep the choice)
                yield return Click("Inside");
                yield return Card("FLOAT_FISH");Assert.AreEqual(COgheProductPopup.StyleNeedsClear,ui.Popup);
                yield return Click("Keep this look");CollectionAssert.AreEqual(new[]{"FLOAT_FISH"},COgheStyle.Current.Inside);
                yield return Card("FLOAT_STARS");yield return Click("Keep this look");
                yield return Card("FLOAT_BUBBLES");Assert.AreEqual(COgheProductPopup.StyleReplaceInside,ui.Popup,"Two at most: which one goes?");
                yield return Click("Replace FLOAT_FISH");CollectionAssert.AreEqual(new[]{"FLOAT_BUBBLES","FLOAT_STARS"},COgheStyle.Current.Inside);
                Assert.AreEqual(COgheProductPopup.StyleNeedsClear,ui.Popup);yield return Click("Go to colors");Assert.AreEqual(0,ui.StyleTab,"Off to find a clear color");
                yield return Click("Accessories");yield return Card("FLOAT_STARS");CollectionAssert.AreEqual(new[]{"FLOAT_BUBBLES"},COgheStyle.Current.Inside,"Tap again to take it out");
                yield return new WaitForSecondsRealtime(.3f);Assert.AreEqual(1,game.Matter.GetComponent<COgheAccessories>().Inside.Count);
                int steps=ui.StyleUndoCount;yield return Click("Undo");Assert.AreEqual(steps-1,ui.StyleUndoCount);
                CollectionAssert.AreEqual(new[]{"FLOAT_BUBBLES","FLOAT_STARS"},COgheStyle.Current.Inside);
            }
            finally{LeaveStyleTest();}
        }

        [UnityTest] public IEnumerator TheSavedLookIsWornInTheNextLevel()
        {
            var look=new COgheStyle{Inks=new[]{"INK_GALAXY","INK_GOLD",null,null},Hat="HAT_BEANIE",Seed=9};
            look.Inside.Add("FLOAT_FISH");for(int i=0;i<32;i++)look.Amount[i]=new Vector4(i%2==0?.6f:.1f,i%2==1?.5f:0,0,0);
            var copy=JsonUtility.FromJson<COgheStyle>(JsonUtility.ToJson(look));
            CollectionAssert.AreEqual(look.Inks,copy.Inks.Select(s=>string.IsNullOrEmpty(s)?null:s).ToArray(),"Saved inks (an empty place is written as \"\")");CollectionAssert.AreEqual(look.Amount,copy.Amount);Assert.AreEqual(look.Hat,copy.Hat);CollectionAssert.AreEqual(look.Inside,copy.Inside);
            try
            {
                COgheStyle.ResetForTests(look);
                ui.LoadForTest(2);yield return null;yield return null;yield return new WaitForSecondsRealtime(.5f);
                game=Object.FindFirstObjectByType<VenomCampaign>();ui=game.ProductUI;
                Assert.AreEqual(2,game.Definition.Order);
                var inking=game.Matter.GetComponent<COgheInking>();Assert.IsNotNull(inking,"COghe's colors came along");
                CollectionAssert.AreEqual(look.Inks,inking.Inks);for(int i=0;i<32;i++)Assert.AreEqual(look.Amount[i],inking.Amount[i]);
                var wear=game.Matter.GetComponent<COgheAccessories>();Assert.IsNotNull(wear);
                Assert.AreEqual("HAT_BEANIE",wear.Hat);CollectionAssert.AreEqual(new[]{"FLOAT_FISH"},wear.Inside);
            }
            finally{LeaveStyleTest();}
        }
    }
}
