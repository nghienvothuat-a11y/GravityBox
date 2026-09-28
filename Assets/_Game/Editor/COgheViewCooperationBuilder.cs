using GravityBox.Venom;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GravityBox.Editor
{
    public static partial class VenomCampaignBuilder
    {
        private static COgheRailSlider ChapterCrossDoor(ExpansionContext c, bool tubeBore = false)
        {
            foreach (float face in new[] { -1f, 1f })
            {
                Vector3 normal = Vector3.right * face;
                Vector3 centre = new Vector3(face * .006f, -.10f, -.19f);
                Vector3 opening = new Vector3(centre.x, -.19f, -.20f);
                Vector3 hole = Quaternion.Inverse(Quaternion.LookRotation(normal, Vector3.up)) * (opening - centre);
                Panel(c.Root, "Cross partition front", centre, normal, new Vector2(.42f, .40f), stone, tubeBore, new Vector2(hole.x, hole.y), tubeBore ? .055f : 0, c.Surfaces);
                Panel(c.Root, "Cross partition rear", new Vector3(face * .006f, -.10f, .36f), normal, new Vector2(.08f, .40f), stone, false, Vector2.zero, 0, c.Surfaces);
                Panel(c.Root, "Cross partition header", new Vector3(face * .006f, 0, .17f), normal, new Vector2(.30f, .20f), stone, false, Vector2.zero, 0, c.Surfaces);
            }
            return ViewGate(c, "Return passage door", new Vector3(-.022f, -.20f, .17f), Vector3.up, .19f, new Vector3(.020f, .20f, .302f));
        }
        private static COgheSpringAccessDoor ChapterHeldDoor(ExpansionContext c, COgheTissueSensor pad, COgheRailSlider door, COgheTapRail latch)
        {
            var held = new GameObject("Measured load spring door", typeof(COgheSpringAccessDoor)).GetComponent<COgheSpringAccessDoor>();
            held.transform.SetParent(c.Root, false); held.Input = pad; held.Door = door; held.LatchHandle = latch == null ? null : latch.Rail;
            held.DoorSize = new Vector3(.020f, .20f, .302f);
            if (latch != null) { latch.RequiredRail = door; latch.RequiredEnd = true; }
            var pin = new GameObject("Terminal catch mount").transform; pin.SetParent(c.Root, false); pin.localPosition = new Vector3(0, .01f, .34f);
            held.CatchPin = MechanismVisual(pin, "Visible hold-open pin", Vector3.zero, new Vector3(.032f, .012f, .016f), metal);
            TapLink(c.Root, "Visible linkage housing", pad.transform.localPosition, door.Start);
            if (latch != null) TapLink(c.Root, "Visible linkage housing", latch.Rail.Start, pin.localPosition);
            return held;
        }
        private static COgheTubeNetwork ChapterCrossTube(ExpansionContext c)
            => ChapterTube(c, "Two-way compartment transfer", new Vector3(-.21f, -.255f, -.20f), new Vector3(-.10f, -.19f, -.20f),
                new Vector3(.10f, -.19f, -.20f), new Vector3(.21f, -.255f, -.20f));
        private static void ChapterCooperation(ExpansionContext c)
        {
            int n = c.Number;
            if (n == 26) { ChapterHeldBridge(c); return; }
            if (n == 29 || n == 30) { ChapterReunionMachine(c); return; }
            if (n == 24) c.Exit = new Vector3(-.20f, -.30f, .31f);
            ChapterShell(c); ChapterKnife(c);
            if (n == 22)
            {
                ViewBlock(c, "Separation island", new Vector3(-.32f, -.255f, -.045f), new Vector3(.025f, .09f, .11f)); return;
            }
            if (n == 23)
            {
                ViewBlock(c, "Independent lanes", new Vector3(-.12f, -.22f, .06f), new Vector3(.020f, .16f, .30f));
                var a = ChapterTask(c, "A", -.45f, .01f, .12f);
                var b = ChapterTask(c, "B", .09f, -.05f, .12f);
                // Two distinct real shutters guard successive parts of the exit approach.
                var first = ChapterPartition(c, "First exit catch", .13f, .36f, .24f);
                ChapterDrive(c, a.Rail, null, first, null, false);
                ChapterDrive(c, b.Rail, null, ChapterFinal(c)); return;
            }
            var pad = ChapterPad(c);
            if (n == 28)
            {
                var b = ChapterTask(c, "B", -.08f, -.03f, .20f); b.RequiredLoad = pad; TapLock(c, b, pad.transform.localPosition);
                var access = ViewGate(c, "Latched side alcove", new Vector3(.19f, -.20f, .20f), Vector3.up, .16f, new Vector3(.15f, .16f, .018f));
                var drive = ChapterDrive(c, b.Rail, b.Rail, access, ChapterFinal(c)); drive.Power = null; drive.InputLoad = pad;
                ViewBlock(c, "Wrong branch back wall", new Vector3(.19f, -.17f, .35f), new Vector3(.20f, .26f, .015f));
                foreach(float x in new[]{.09f,.29f}) ViewBlock(c,"Wrong branch side cheek",new Vector3(x,-.17f,.28f),new Vector3(.015f,.26f,.14f)); return;
            }
            var door = ChapterCrossDoor(c, n == 27);
            var far = ChapterTask(c, "B", .22f, .01f, .10f);
            if(n==24||n==25)
            {
                var grip=far.Handle.localPosition;grip.z=.035f;far.Handle.localPosition=grip;
                far.StandOffset=Vector3.forward*.05f;
            }
            if (n == 24)
            {
                ChapterHeldDoor(c, pad, door, null);
                ChapterDrive(c, far.Rail, null, ChapterFinal(c));
            }
            if (n == 25)
            {
                ChapterHeldDoor(c, pad, door, far);
                var final = ChapterFinal(c); ChapterDrive(c, far.Rail, null, final);
            }
            if (n == 27)
            {
                ChapterCrossTube(c); far.RequiredLoad = pad; TapLock(c, far, pad.transform.localPosition);
                ChapterDrive(c, far.Rail, null, door, null, false);
                ChapterDrive(c, far.Rail, null, ChapterFinal(c));
            }
        }
        private static void ChapterHeldBridge(ExpansionContext c)
        {
            ChapterBanks(c); ChapterKnife(c);
            var pad = ChapterPad(c, new Vector3(-.48f, -.297f, .32f));
            var tap = pad.GetComponent<COgheTapPad>();
            tap.ReleasePoint.localPosition = new Vector3(-.48f, -.28f, .10f);
            var b = ChapterBridge(c); b.RequiredLoad = pad; TapLock(c, b, pad.transform.localPosition);
            ViewLink(c, b.Rail, ChapterFinal(c), true, ViewExitSurface(c));
        }
        private static void ChapterReunionMachine(ExpansionContext c)
        {
            c.Exit=new Vector3(.46f,-.30f,c.Number==30?.32f:.29f);
            ChapterBanks(c);ChapterKnife(c);var pad=ChapterPad(c);
            // A continuous far-side service strip supports the operator throughout the deck's travel.
            var old=c.Surfaces.Find(p=>p.name=="Receiving bank");c.Surfaces.Remove(old);Object.DestroyImmediate(old.gameObject);
            float width=c.Number==30?.34f:.26f,inner=.56f-width;
            ChapterFloor(c,"Receiving lip",new Vector3((.14f+inner)*.5f,-.30f,.25f),new Vector2(inner-.14f,.30f));
            var right=ChapterFloor(c,"Receiving bank",new Vector3(.56f-width*.5f,-.30f,0),new Vector2(width,.80f),true);
            // Straight mouth leads stay above the continuous floor, away from the operator's service lane.
            if(c.Number==29)
            {
                ChapterTube(c,"Outbound transfer",new Vector3(-.21f,-.255f,-.20f),new Vector3(-.10f,-.19f,-.20f),
                    new Vector3(0,-.12f,-.10f),new Vector3(.35f,-.12f,-.10f),new Vector3(.35f,-.225f,-.16f),new Vector3(.35f,-.255f,-.21f),new Vector3(.35f,-.255f,-.26f));
            }
            else
                ChapterTube(c,"Outbound transfer",new Vector3(-.21f,-.255f,-.20f),new Vector3(-.10f,-.19f,-.20f),
                    new Vector3(0,-.12f,-.10f),new Vector3(.46f,-.12f,-.18f),new Vector3(.46f,-.255f,-.06f));
            COgheRailSlider bridge;
            if(c.Number==29)
            {
                var task=ChapterBridge(c,"B",0,.27f,-.20f,.20f,right,.43f);task.StandOffset=new Vector3(.08f,0,-.05f);task.RequiredLoad=pad;bridge=task.Rail;
            }
            else bridge=ChapterMovingDeck(c,"Powered reunion bridge",new Vector3(0,-.313f,-.20f),Vector3.forward,.40f,new Vector2(.27f,.14f));
            var latch=ChapterTask(c,"C",.23f,.34f,.08f,right);
            var door=ViewGate(c,"Return bridge door",new Vector3(.145f,-.20f,.20f),Vector3.up,.19f,new Vector3(.020f,.20f,.16f));
            var held=ChapterHeldDoor(c,pad,door,latch);held.DoorSize=new Vector3(.020f,.20f,.16f);
            ChapterDrive(c,latch.Rail,null,ChapterFinal(c));
            if(c.Number==30)
            {
                var power=ChapterTask(c,"P",.27f,-.24f,.10f,right);
                var selector=ChapterTask(c,"B",.24f,-.04f,.10f,right);selector.RequiredLoad=pad;TapLock(c,selector,pad.transform.localPosition);
                foreach(float side in new[]{-1f,1f})
                {
                    Panel(c.Root,"Boss access left jamb",new Vector3(.195f,-.10f,.10f+side*.006f),Vector3.forward*side,new Vector2(.11f,.40f),stone,false,Vector2.zero,0,c.Surfaces);
                    Panel(c.Root,"Boss access right jamb",new Vector3(.505f,-.10f,.10f+side*.006f),Vector3.forward*side,new Vector2(.11f,.40f),stone,false,Vector2.zero,0,c.Surfaces);
                    Panel(c.Root,"Boss access header",new Vector3(.35f,0,.10f+side*.006f),Vector3.forward*side,new Vector2(.20f,.20f),stone,false,Vector2.zero,0,c.Surfaces);
                }
                var access=ViewGate(c,"First caught access",new Vector3(.35f,-.20f,.08f),Vector3.up,.19f,new Vector3(.204f,.20f,.018f));
                ChapterDrive(c,power.Rail,selector.Rail,access,bridge,false);
                latch.RequiredRail=bridge;
            }
        }
    }
}
