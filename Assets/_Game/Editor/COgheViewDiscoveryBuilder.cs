using GravityBox.Venom;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GravityBox.Editor
{
    public static partial class VenomCampaignBuilder
    {
        private static void ChapterBanks(ExpansionContext c)
        {
            ChapterFloor(c, "Departure bank", new Vector3(-.35f, -.30f, 0), new Vector2(.42f, .80f));
            ChapterFloor(c, "Receiving bank", new Vector3(.35f, -.30f, .25f), new Vector2(.42f, .30f), true);
            var basin = ChapterFloor(c, "Recovery basin", new Vector3(0, -.42f, 0), new Vector2(1.12f, .80f)); basin.Slippery = true;
            Panel(c.Root, "Recovery climb", new Vector3(-.14f, -.36f, 0), Vector3.right, new Vector2(.80f, .12f), stone, false, Vector2.zero, 0, c.Surfaces);
            var cliff = Panel(c.Root, "Receiving cliff", new Vector3(.14f, -.36f, .25f), Vector3.left, new Vector2(.30f, .12f), stone, false, Vector2.zero, 0, c.Surfaces); cliff.Slippery = true;
            ChapterShell(c, false);
            foreach (var s in c.Surfaces) if (s.ExteriorGlass) s.Slippery = true;
        }
        private static COgheTapRail ChapterBridge(ExpansionContext c, string label = "B", float x = 0, float span = .27f, float startZ = -.20f, float endZ = .20f, VenomSurfacePatch work = null, float handX = -.30f)
        {
            var rail = ExpansionRail(c, label + " bridge", new Vector3(x, -.313f, startZ), Vector3.forward, endZ - startZ, 0,
                new Vector3(span, .026f, .14f), .035f, .008f, false, true);
            rail.LatchAtStart = rail.LatchAtEnd = true; rail.GetComponent<VenomMovableProp>().Manipulable = false;
            var task = rail.gameObject.AddComponent<COgheTapRail>(); task.Rail = rail; task.Label = label;
            task.Handle = rail.GetComponent<VenomMovableProp>().ManipulationGrip;
            task.Handle.localPosition = new Vector3(handX, .025f, 0);
            task.WorkingSurface = work ?? c.Surfaces[0]; task.StandOffset = new Vector3(-.07f, 0, 0);
            task.PickHandleOnly = task.TrackStandPoint = true; task.TouchSize = new Vector3(.080f, .060f, .070f);
            float armY = c.Number == 11 || c.Number == 13 || c.Number == 21 ? .075f : .025f;
            var armStart = new Vector3(-span * .4f, armY, 0); var armEnd = new Vector3(handX, armY, 0);
            TapLink(rail.transform, "Handle arm", armStart, armEnd);
            if(armY>.025f) { TapLink(rail.transform,"Deck arm riser",armStart,new Vector3(armStart.x,0,0)); TapLink(rail.transform,"Grip arm riser",armEnd,task.Handle.localPosition); }
            var dock = ChapterFloor(c, "Docked bridge top", new Vector3(x, -.30f, endZ), new Vector2(span + .020f, .14f));
            dock.gameObject.SetActive(false);
            var deck = rail.gameObject.AddComponent<COgheDockedBridgeDeck>(); deck.Rail = rail;
            deck.MovingSurfaces = rail.GetComponentsInChildren<VenomSurfacePatch>(); deck.DockedTop = dock;
            Object.DestroyImmediate(dock.Shape); var shape = dock.gameObject.AddComponent<BoxCollider>();
            shape.center = Vector3.back * .013f; shape.size = new Vector3(span + .020f, .14f, .026f);
            shape.sharedMaterial = contact; shape.contactOffset = .0003f; dock.Shape = shape;
            return task;
        }
        private static COgheRailSlider ChapterCover(ExpansionContext c, COgheTapRail opener, COgheTapRail target, bool slideBack = false)
        {
            Vector3 p = target.Handle.position;
            var cover = ViewGate(c, "Opaque handle cover", new Vector3(p.x, -.235f, p.z), slideBack ? Vector3.back : Vector3.forward, .17f, new Vector3(.12f, .018f, .13f));
            ViewLink(c, opener.Rail, cover, false, null); target.RequiredRail = cover; target.RequiredEnd = true;
            return cover;
        }
        private static void ChapterDiscovery(ExpansionContext c)
        {
            int n = c.Number;
            if (n == 12) { ChapterUnderLid(c); return; }
            if (n == 14)
            {
                ChapterShell(c); var a = ChapterTask(c, "A", -.22f, -.06f, .14f);
                ChapterDrive(c, a.Rail, null, ChapterFinal(c)).RetainOutputs = false; return;
            }
            if (n == 17) { ChapterTransferLesson(c); return; }
            if (n == 19) { ChapterReturnRoom(c); return; }
            if (n == 20) { ChapterMachineLoop(c); return; }
            if (n == 16) { ChapterTwoBridges(c); return; }
            ChapterSecretReturn(c);
        }
        private static void ChapterSecretReturn(ExpansionContext c)
        {
            // The old parking space covers a real lower exit pocket. Only the far bank has a dry return ramp.
            c.Exit = new Vector3(0, -.42f, -.20f);
            ChapterFloor(c, "Departure bank", new Vector3(-.35f, -.30f, 0), new Vector2(.42f, .80f));
            ChapterFloor(c, "Receiving bank", new Vector3(.35f, -.30f, .25f), new Vector2(.42f, .30f));
            var basin = ChapterFloor(c, "Recovery basin", new Vector3(0, -.44f, 0), new Vector2(1.12f, .80f)); basin.Slippery = true;
            Panel(c.Root, "Recovery climb", new Vector3(-.14f, -.37f, 0), Vector3.right, new Vector2(.80f, .14f), stone, false, Vector2.zero, 0, c.Surfaces);
            ChapterFloor(c, "Secret exit pocket", new Vector3(.10f, -.42f, -.20f), new Vector2(.34f, .18f), true);
            ChapterFloor(c, "Lower return corridor", new Vector3(.35f, -.42f, -.21f), new Vector2(.22f, .20f));
            Panel(c.Root, "Return ramp", new Vector3(.35f, -.36f, -.01f), new Vector3(0,1,-.6f).normalized, new Vector2(.22f, .233238f), stone, false, Vector2.zero, 0, c.Surfaces);
            ViewBlock(c, "Pocket separation wall", new Vector3(-.145f, -.35f, -.15f), new Vector3(.006f, .18f, .50f));
            foreach(var patch in c.Surfaces) if(patch.name == "Pocket separation wall") patch.Slippery = true;
            ChapterShell(c, false); foreach(var patch in c.Surfaces)if(patch.ExteriorGlass)patch.Slippery=true;
            var bridge = ChapterBridge(c, c.Number == 13 ? "B" : "A");
            if(c.Number == 13) { var a=ChapterTask(c,"A",-.48f,.28f,.10f); ChapterCover(c,a,bridge); }
            if(c.Number == 21) ViewLink(c,bridge.Rail,ChapterFinal(c),true,ViewExitSurface(c));
            else
            {
                // This plate is part of the same Rigidbody and moves with the bridge, exposing its parking pocket.
                DynamicFace(bridge.transform,"Bridge underside pocket cover",new Vector3(0,-.089f,0),Vector3.up,new Vector2(.13f,.13f),plastic,c.Surfaces);
                var rule=bridge.gameObject.AddComponent<COgheExitRailLock>();rule.Rail=bridge.Rail;
            }
        }
        private static void ChapterUnderLid(ExpansionContext c)
        {
            c.Exit = new Vector3(.36f, -.40f, .27f);
            var main = ChapterFloor(c, "Upper floor", new Vector3(-.18f, -.30f, 0), new Vector2(.76f, .80f));
            ChapterFloor(c, "Lower passage", new Vector3(.38f, -.40f, .12f), new Vector2(.36f, .56f), true);
            Panel(c.Root, "Wide downward ramp", new Vector3(.25f, -.35f, -.23f), new Vector3(0, 1, 1).normalized, new Vector2(.22f, .14142136f), stone, false, Vector2.zero, 0, c.Surfaces);
            ChapterShell(c, false);
            var a = ChapterTask(c, "A", -.20f, -.13f, .14f, main);
            var lid = ViewGate(c, "Passage lid", new Vector3(.27f, -.280f, -.23f), Vector3.forward, .26f, new Vector3(.25f, .026f, .22f));
            ViewLink(c, a.Rail, lid, false, null);
            // A broad side descent remains physically connected to the main floor at the ramp mouth.
            ChapterFloor(c, "Ramp approach", new Vector3(.25f, -.30f, -.345f), new Vector2(.22f, .11f));
            a.TwoSided = true;
        }
        private static void ChapterTwoBridges(ExpansionContext c)
        {
            // Two real decks meet an island. Their parked footprints intersect, making order physical.
            ChapterFloor(c, "Departure bank", new Vector3(-.43f, -.30f, 0), new Vector2(.26f, .80f));
            ChapterFloor(c, "Centre island", new Vector3(0, -.30f, .20f), new Vector2(.12f, .22f));
            ChapterFloor(c, "Receiving bank", new Vector3(.43f, -.30f, .25f), new Vector2(.26f, .30f), true);
            var basin = ChapterFloor(c, "Recovery basin", new Vector3(0, -.42f, 0), new Vector2(1.12f, .80f)); basin.Slippery = true;
            Panel(c.Root, "Recovery climb", new Vector3(-.30f, -.36f, 0), Vector3.right, new Vector2(.80f, .12f), stone, false, Vector2.zero, 0, c.Surfaces);
            ChapterShell(c, false); foreach (var s in c.Surfaces) if (s.ExteriorGlass) s.Slippery = true;
            var a = ChapterBridge(c, "A", -.18f, .23f, -.20f, .20f, c.Surfaces[0], -.23f);
            // B's handle extends to the fixed near bank; its rail crosses A's initial parked lane.
            var b = ChapterBridge(c, "B", .18f, .23f, -.20f, .20f, c.Surfaces[0], -.66f);
            ChapterSolid(a.transform,"A sequencing dog",new Vector3(.25f,-.013f,.090f),new Vector3(.46f,.016f,.025f));
            var gate = ChapterFinal(c); ViewLink(c, b.Rail, gate, true, ViewExitSurface(c));
        }
        private static COgheRailSlider ChapterMovingDeck(ExpansionContext c,string name,Vector3 start,Vector3 axis,float travel,Vector2 size)
        {
            var rail=ViewGate(c,name,start,axis,travel,new Vector3(size.x,.026f,size.y));rail.LatchAtEnd=true;
            var dock=ChapterFloor(c,"Docked bridge top",start+axis*travel+Vector3.up*.013f,size+new Vector2(.020f,.020f));dock.gameObject.SetActive(false);
            var deck=rail.gameObject.AddComponent<COgheDockedBridgeDeck>();deck.Rail=rail;deck.MovingSurfaces=rail.GetComponentsInChildren<VenomSurfacePatch>();deck.DockedTop=dock;
            Object.DestroyImmediate(dock.Shape);var shape=dock.gameObject.AddComponent<BoxCollider>();shape.center=Vector3.back*.013f;
            shape.size=new Vector3(size.x+.020f,size.y+.020f,.026f);shape.sharedMaterial=contact;shape.contactOffset=.0003f;dock.Shape=shape;
            return rail;
        }
        private static void ChapterReturnRoom(ExpansionContext c)
        {
            ChapterFloor(c,"Departure bank",new Vector3(0,-.30f,-.20f),new Vector2(1.12f,.40f));
            ChapterFloor(c,"Side chamber",new Vector3(-.37f,-.30f,.20f),new Vector2(.38f,.40f));
            ChapterFloor(c,"Receiving bank",new Vector3(.32f,-.30f,.27f),new Vector2(.48f,.26f),true);
            var basin=ChapterFloor(c,"Recovery basin",new Vector3(0,-.42f,0),new Vector2(1.12f,.80f));basin.Slippery=true;
            ChapterShell(c,false);foreach(var patch in c.Surfaces)if(patch.ExteriorGlass)patch.Slippery=true;
            // Two real openings in a sealed partition; one selector opens one and closes the other.
            foreach(float side in new[]{-1f,1f})foreach(float x in new[]{-.48f,0f,.48f})
                Panel(c.Root,"Alternating door partition",new Vector3(x,-.10f,.006f*side),Vector3.forward*side,new Vector2(x==0?.40f:.16f,.40f),stone,false,Vector2.zero,0,c.Surfaces);
            foreach(float x in new[]{-.30f,.30f})foreach(float side in new[]{-1f,1f})
                Panel(c.Root,"Alternating header",new Vector3(x,0,.006f*side),Vector3.forward*side,new Vector2(.20f,.20f),stone,false,Vector2.zero,0,c.Surfaces);
            var left=ViewGate(c,"Side access shutter",new Vector3(-.30f,-.20f,-.02f),Vector3.up,.19f,new Vector3(.202f,.20f,.018f));
            var right=ViewGate(c,"Main access shutter",new Vector3(.30f,-.20f,-.02f),Vector3.up,.19f,new Vector3(.202f,.20f,.018f));
            right.InitialTravel=right.Travel;right.Body.position=c.Root.TransformPoint(right.Start+right.Axis*right.Travel);
            var a=ChapterTask(c,"A",-.12f,-.16f,.10f);ViewLink(c,a.Rail,left,false,null);
            var reverse=new GameObject("Alternating door linkage",typeof(COgheViewMechanism)).GetComponent<COgheViewMechanism>();reverse.transform.SetParent(c.Root,false);reverse.Input=a.Rail;reverse.Output=right;reverse.Reverse=true;
            var b=ChapterTask(c,"B",-.48f,.27f,.10f,c.Surfaces[1]);
            var deck=ChapterMovingDeck(c,"Latched return bridge",new Vector3(-.02f,-.313f,.07f),Vector3.right,.32f,new Vector2(.18f,.120f));
            ChapterDrive(c,b.Rail,null,deck,null,false);
        }
        private static COgheTubeNetwork ChapterTube(ExpansionContext c, string name, params Vector3[] path)
        {
            var nodes = new[] { new COgheTubeNetwork.Node("Vào", path[0], COgheTubeNetwork.TerminalKind.Entry),
                new COgheTubeNetwork.Node("Ra", path[path.Length - 1], COgheTubeNetwork.TerminalKind.Entry) };
            var network = TubeNetwork(c.Root, name, nodes, new[] { new COgheTubeNetwork.Edge("Transfer", 0, 1, path) }, .038f, glass);
            network.CaptureSurfaceCommandsWhileInside = true; return network;
        }
        private static void ChapterTransferLesson(ExpansionContext c)
        {
            c.Exit=new Vector3(.39f,-.16f,.29f);ChapterShell(c);
            ViewBlock(c,"Pipe return wall",new Vector3(.04f,-.15f,.02f),new Vector3(.44f,.30f,.02f));
            ChapterFloor(c,"Raised receiving platform",new Vector3(.28f,-.16f,.26f),new Vector2(.56f,.28f),true);
            Panel(c.Root,"Return ramp",new Vector3(.46f,-.23f,.03f),new Vector3(0,1,-.777778f).normalized,new Vector2(.18f,.228035f),stone,false,Vector2.zero,0,c.Surfaces);
            ChapterTube(c,"U transfer tube",new Vector3(-.28f,-.255f,-.16f),new Vector3(-.28f,-.20f,-.04f),
                new Vector3(-.32f,-.08f,.16f),new Vector3(-.10f,-.08f,.26f),new Vector3(.10f,-.115f,.24f),new Vector3(.20f,-.115f,.24f));
        }
        private static void ChapterMachineLoop(ExpansionContext c)
        {
            c.Exit = new Vector3(.39f, -.30f, -.29f);
            ChapterBanks(c);
            var oldBank = c.Surfaces.Find(p=>p.name=="Receiving bank"); c.Surfaces.Remove(oldBank); Object.DestroyImmediate(oldBank.gameObject);
            ChapterFloor(c,"Receiving bank",new Vector3(.35f,-.30f,0),new Vector2(.42f,.80f),true);
            var oldCliff = c.Surfaces.Find(p=>p.name=="Receiving cliff"); c.Surfaces.Remove(oldCliff); Object.DestroyImmediate(oldCliff.gameObject);
            var cliff = Panel(c.Root,"Receiving cliff",new Vector3(.14f,-.36f,0),Vector3.left,new Vector2(.80f,.12f),stone,false,Vector2.zero,0,c.Surfaces); cliff.Slippery=true;
            var b=ChapterTask(c,"B",-.47f,-.07f,.10f);
            var a=ChapterTask(c,"A",-.47f,-.29f,.10f);ChapterCover(c,a,b,true);
            foreach(float side in new[]{-1f,1f})
            {
                var normal=Vector3.forward*side;var centre=new Vector3(-.35f,-.10f,.045f+side*.006f);
                var local=Quaternion.Inverse(Quaternion.LookRotation(normal,Vector3.up))*(new Vector3(-.29f,-.19f,centre.z)-centre);
                Panel(c.Root,"Sealed C bay",centre,normal,new Vector2(.42f,.40f),stone,true,new Vector2(local.x,local.y),.055f,c.Surfaces);
            }
            var inlet=ViewGate(c,"Tube inlet shutter",new Vector3(-.29f,-.259f,-.175f),Vector3.up,.14f,new Vector3(.09f,.074f,.014f));
            ChapterDrive(c,b.Rail,null,inlet,null,false);
            var tube=ChapterTube(c,"Loop transfer",new Vector3(-.29f,-.255f,-.14f),new Vector3(-.29f,-.19f,-.02f),new Vector3(-.29f,-.19f,.09f),new Vector3(-.29f,-.255f,.16f));
            tube.EntryBlocker=inlet.GetComponent<VenomMovableProp>().CollisionShapes[0];
            var control=ChapterTask(c,"C",-.48f,.32f,.10f);
            var bridge=ChapterMovingDeck(c,"C powered bridge",new Vector3(0,-.313f,-.20f),Vector3.forward,.40f,new Vector2(.27f,.14f));
            ChapterDrive(c,control.Rail,null,bridge,null,false);
            var right=c.Surfaces.Find(p=>p.name=="Receiving bank");
            var d=ChapterTask(c,"D",.34f,.08f,.10f,right);ViewLink(c,d.Rail,ChapterFinal(c),true,ViewExitSurface(c));
        }
    }
}
