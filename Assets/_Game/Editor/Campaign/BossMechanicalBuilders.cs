using GravityBox.Gameplay;
using GravityBox.Presentation;
using UnityEngine;

namespace GravityBox.Editor
{
    internal static class BossMechanicalBuilders
    {
        internal static LevelRuntime Garden(CampaignLevelSpec spec,CampaignBuildContext c)
        {
            var l=c.Planar("Mechanical garden",BossGeometry.Rectangle(.82f,.32f),new Vector2(-.67f,-.18f),new Vector2(.73f,-.19f),.30f,spawnY:-.022f);
            var a=BossGeometry.Author(l,c);
            // Three wide garden courts. Their floors are raised to the same door datum.
            a.Block("Garden west court",new Vector3(-.55f,-.094f,0),new Vector3(.53f,.108f,.628f),c.Glass);
            a.Block("Garden middle court",new Vector3(-.01f,-.094f,0),new Vector3(.54f,.108f,.628f),c.Glass);
            a.Block("Garden third approach",new Vector3(.355f,-.094f,0),new Vector3(.18f,.108f,.628f),c.Glass);
            HardCampaignModules.Slider(a,new Vector3(-.28f,0,0),Quaternion.identity,.32f,.15f);
            HardCampaignModules.Slider(a,new Vector3(.26f,0,0),Quaternion.Euler(0,180,0),.32f,.15f);
            var bridge=HardCampaignModules.Bridge(a,new Vector3(.45f,-.037f,0),Quaternion.identity,recoveryFloor:-.110f);
            a.Block("Garden bridge upper closure",new Vector3(.45f,.1295f,0),new Vector3(.010f,.041f,.64f),c.Glass);
            foreach(int sign in new[]{-1,1})a.Block("Garden bridge outer partition "+sign,new Vector3(.45f,0,sign*.245f),new Vector3(.010f,.30f,.15f),c.Glass);
            // Court three receiver slopes down to the real aperture; missed crossing stays on its own side.
            HardCampaignModules.Ramp(a,new Vector3(.70f,-.04f,0),new Vector3(.73f,-.144f,-.135f),.11f,c.Frame);
            HardCampaignModules.Pocket(a,new Vector3(-.58f,-.038f,.19f),Quaternion.identity,.11f,.12f);
            HardCampaignModules.Pocket(a,new Vector3(-.04f,-.038f,-.18f),Quaternion.Euler(0,180,0),.11f,.12f);
            var leaf1=Leaf(a,new Vector3(-.52f,-.038f,0),c);var leaf2=Leaf(a,new Vector3(0,-.038f,0),c);var leaf3=Leaf(a,new Vector3(.66f,-.143f,.20f),c);
            var p=BossGeometry.Present(a,BossGeometry.Region("First garden court",new Vector3(-.05f,0,0),new Vector3(.38f,.3f,.6f),leaf1),
                BossGeometry.Region("Second opposite door",new Vector3(.35f,0,0),new Vector3(.14f,.3f,.6f),leaf2),
                new BossPresentation.Milestone{Name="Bridge resting on its pawl",Seat=bridge.GetComponent<ContactSeatLatch>(),Inlays=new[]{leaf3}});p.Finale=new[]{leaf1,leaf2,leaf3};return l;
        }
        internal static LevelRuntime Lotus(CampaignLevelSpec spec,CampaignBuildContext c)
        {
            var shape=new Vector2[48];for(int i=0;i<shape.Length;i++){float t=i*Mathf.PI*2/shape.Length;float r=1+.045f*Mathf.Cos(8*t);shape[i]=new Vector2(Mathf.Cos(t)*.86f*r,Mathf.Sin(t)*.69f*r);}
            var l=c.Planar("Lotus of remembered motion",shape,new Vector2(-.59f,-.27f),Vector2.zero,.32f,spawnY:-.038f);
            var a=BossGeometry.Author(l,c);
            // A continuous raised, perforated deck closes all underside bypasses.
            BossGeometry.Mesh(a,"Lotus raised load deck",PhysicsLabGeometry.Panel("Lotus raised deck",shape,-.105f,.045f,Vector2.zero,.023f,new[]{new[]{new Vector2(-.052f,.324f),new Vector2(.32f,.324f),new Vector2(.32f,.53f),new Vector2(-.052f,.53f)}}),c.Glass);
            BossGeometry.Mesh(a,"Lotus low inspection roof",PhysicsLabGeometry.Panel("Lotus mechanism roof",shape,.06f,.003f,Vector2.zero,0,new[]{new[]{new Vector2(-.36f,.21f),new Vector2(.23f,.21f),new Vector2(.23f,.56f),new Vector2(-.36f,.56f)}}),c.Glass);
            var first=HardCampaignModules.Cam(a,new Vector3(-.42f,0,0),Quaternion.identity,3);
            var last=HardCampaignModules.Cam(a,new Vector3(.40f,0,0),Quaternion.Euler(0,-90,0),3);
            // Modules' sleeves connect to physical partitions, leaving one route around the three courts.
            a.Block("Lotus west outside cam closure",new Vector3(-.835f,0,0),new Vector3(.11f,.32f,.012f),c.Glass);
            a.Block("Lotus left to centre partition",new Vector3(-.06f,0,-.27f),new Vector3(.012f,.32f,.96f),c.Glass);
            a.Block("Lotus centre north wall",new Vector3(.17f,0,.208f),new Vector3(.46f,.32f,.012f),c.Glass);
            a.Block("Lotus centre south wall",new Vector3(.17f,0,-.208f),new Vector3(.46f,.32f,.012f),c.Glass);
            a.Block("Lotus last cam south closure",new Vector3(.40f,0,-.53f),new Vector3(.012f,.32f,.34f),c.Glass);
            // Folding bridge sits in the northern court; raised leaf housing closes its upper bypass.
            var bridge=HardCampaignModules.Bridge(a,new Vector3(-.06f,-.057f,.38f),Quaternion.identity,recoveryFloor:-.100f);
            a.Block("Lotus upper landing connector",new Vector3(.25f,-.061f,.38f),new Vector3(.16f,.008f,.11f),c.Frame);
            HardCampaignModules.Ramp(a,new Vector3(.19f,-.151f,.48f),new Vector3(.32f,-.060f,.48f),.085f,c.Frame);
            a.Block("Lotus bridge overhead leaf",new Vector3(-.06f,.127f,.38f),new Vector3(.014f,.066f,.34f),c.Glass);
            a.Block("Lotus bridge outer closure",new Vector3(-.06f,0,.64f),new Vector3(.014f,.32f,.20f),c.Glass);
            HardCampaignModules.Pocket(a,new Vector3(-.63f,-.058f,-.36f),Quaternion.Euler(0,180,0),.10f,.12f);
            HardCampaignModules.Pocket(a,new Vector3(.65f,-.058f,.33f),Quaternion.identity,.10f,.12f);
            var petal1=Leaf(a,new Vector3(-.50f,-.058f,-.32f),c);var petal2=Leaf(a,new Vector3(0,-.058f,.51f),c);var petal3=Leaf(a,new Vector3(.65f,-.058f,-.20f),c);
            var p=BossGeometry.Present(a,new BossPresentation.Milestone{Name="First retained cam",Memory=first,Inlays=new[]{petal1}},new BossPresentation.Milestone{Name="Bridge seat",Seat=bridge.GetComponent<ContactSeatLatch>(),Inlays=new[]{petal2}},new BossPresentation.Milestone{Name="Return to lotus centre",Memory=last,Inlays=new[]{petal3}});p.Finale=new[]{petal1,petal2,petal3};return l;
        }
        internal static LevelRuntime Orbit(CampaignLevelSpec spec,CampaignBuildContext c)
        {
            var l=c.Planar("Orbital dance",BossGeometry.Rectangle(.69f,.43f),new Vector2(-.50f,.29f),new Vector2(.56f,-.31f),.90f,spawnY:.015f);
            var a=BossGeometry.Author(l,c);
            // The pendulum passage is at the launch court's height. Its northern
            // waiting pocket is a distinct stopping place before the launch ramp.
            var pendulum=HardCampaignModules.Pendulum(a,new Vector3(-.38f,.13f,.15f),Quaternion.identity,.30f);
            a.Block("Orbit first court raised deck",new Vector3(-.38f,-.03f,.29f),new Vector3(.60f,.046f,.27f),c.Glass);
            a.Block("Orbit west separation",new Vector3(-.075f,0,.275f),new Vector3(.014f,.90f,.31f),c.Glass);
            a.Block("Orbit pendulum overhead closure",new Vector3(-.38f,.36f,.15f),new Vector3(.60f,.18f,.014f),c.Glass);
            HardCampaignModules.Pocket(a,new Vector3(-.38f,-.004f,.32f),Quaternion.identity,.11f,.10f);
            a.Block("Orbit pendulum below-floor closure",new Vector3(-.38f,-.229f,.15f),new Vector3(.60f,.442f,.014f),c.Glass);
            var flightOrigin=new Vector3(-.18f,-.12f,-.10f);
            HardCampaignModules.Flight(a,flightOrigin,Quaternion.identity,.02f);
            // A waiting platform connects the pendulum exit to the launch pocket without an immediate jump.
            HardCampaignModules.Ramp(a,new Vector3(-.48f,.00f,.09f),new Vector3(-.48f,.00f,-.10f),.10f,c.Frame);
            BossGeometry.Mesh(a,"Orbit local recovery basin",PhysicsLabGeometry.Panel("Orbit basin",new[]{new Vector2(-.69f,-.43f),new Vector2(.17f,-.43f),new Vector2(.17f,.43f),new Vector2(-.69f,.43f)},-.34f,.004f,new Vector2(.08f,-.08f),.04f),c.Glass);
            BossCooperationBuilder.Tube(a,"Orbit receiver down-transfer",new Vector3(.08f,-.25f,-.08f),.17f,.082f);
            HardCampaignModules.Ramp(a,new Vector3(.08f,-.376f,-.08f),new Vector3(.225f,-.335f,-.10f),.105f,c.Frame);
            var cage=HardCampaignModules.Cage(a,new Vector3(.37f,-.32f,-.10f),Quaternion.identity);
            // Upper/lower sides confine the receiver transfer but allow the cage's whole sweep.
            foreach(int side in new[]{-1,1})a.Block("Orbit cage transfer wall "+side,new Vector3(.38f,-.28f,-.10f+side*.165f),new Vector3(.59f,.33f,.010f),c.Glass);
            a.Block("Orbit cage upper housing",new Vector3(.38f,-.105f,-.10f),new Vector3(.59f,.010f,.34f),c.Glass);
            // End receiver is generous; the ball can rest before descending to the exit.
            HardCampaignModules.Ramp(a,new Vector3(.61f,-.32f,-.10f),new Vector3(.56f,-.442f,-.265f),.11f,c.Frame);
            var ring1=BossGeometry.Arc(a,"Pendulum orbit",new Vector3(-.38f,.277f,.15f),.16f,.10f,c.Rim);
            var ring2=BossGeometry.Arc(a,"Catch orbit",new Vector3(.035f,-.157f,-.08f),.095f,.078f,c.Rim);
            var ring3=BossGeometry.Arc(a,"Cage orbit",new Vector3(.37f,-.096f,-.10f),.16f,.15f,c.Rim);
            var p=BossGeometry.Present(a,BossGeometry.Region("Pendulum passed",new Vector3(-.38f,.02f,.08f),new Vector3(.55f,.18f,.10f),ring1),BossGeometry.Region("Physical catch",new Vector3(.035f,-.14f,-.08f),new Vector3(.22f,.07f,.20f),ring2),BossGeometry.Region("After the cage",new Vector3(.59f,-.31f,-.10f),new Vector3(.12f,.18f,.22f),ring3));p.Finale=new[]{ring1,ring2,ring3};return l;
        }
        private static Renderer Leaf(MechanicalAuthoring a,Vector3 p,CampaignBuildContext c)
        {
            return BossGeometry.Line(a,"Engraved leaf",new[]{p+new Vector3(-.08f,0,0),p+new Vector3(-.02f,0,.06f),p+new Vector3(.08f,0,0),p+new Vector3(-.02f,0,-.06f),p+new Vector3(-.08f,0,0)},c.Rim);
        }
    }
}
