using GravityBox.Gameplay;
using GravityBox.Simulation;
using UnityEngine;

namespace GravityBox.Editor
{
    internal static class HardMemoryLevels
    {
        internal static LevelRuntime Build(CampaignLevelSpec s,CampaignBuildContext c)
        {
            switch(s.index)
            {
                case 45: case 46: case 48:return Serial(s,c);
                case 47:return BridgeLoop(s,c);
                case 91:return SharedCam(s,c);
                case 95:return Parallel(s,c);
                case 92: case 93: case 97:return Stacked(s,c);
                case 98:return Choice(s,c);
                default:throw new System.ArgumentOutOfRangeException(nameof(s.index));
            }
        }

        private static LevelRuntime Serial(CampaignLevelSpec s,CampaignBuildContext c)
        {
            bool corner=s.index==46,coupled=s.index!=48;
            Vector2[] outline=corner
                ?new[]{new Vector2(-.36f,-.66f),new Vector2(.36f,-.66f),new Vector2(.36f,-.06f),new Vector2(.76f,-.06f),new Vector2(.76f,.66f),new Vector2(-.36f,.66f)}
                :HardCampaignBuilder.Rectangle(.36f,.66f);
            var l=c.Planar(HardCampaignBuilder.Name(s),outline,new Vector2(-.26f,-.57f),corner?new Vector2(.66f,.30f):new Vector2(.25f,.56f),.12f);
            var a=HardCampaignBuilder.Author(l,c);
            var first=HardCampaignModules.Cam(a,new Vector3(0,0,-.28f),Quaternion.identity,1,coupled?1:0);
            var second=HardCampaignModules.Cam(a,corner?new Vector3(.40f,0,.30f):new Vector3(0,0,.28f),corner?Quaternion.Euler(0,90,0):Quaternion.identity,2);
            if(coupled)
            {
                first.MaximumTeeth=2;
                // A native angular coupling makes the two shutter rotors one
                // retained mechanical state. Translation remains free in this
                // coupling because each rotor has its own fixed axle bearing.
                // Contact on either reachable rack supplies the work.
                var coupling=second.Cam.gameObject.AddComponent<ConfigurableJoint>();
                coupling.connectedBody=first.Cam.Body;coupling.autoConfigureConnectedAnchor=true;
                coupling.xMotion=coupling.yMotion=coupling.zMotion=ConfigurableJointMotion.Free;
                coupling.angularXMotion=coupling.angularYMotion=coupling.angularZMotion=ConfigurableJointMotion.Locked;
                coupling.enableCollision=false;
                var belt=new GameObject("Visible shared cam timing belt",typeof(LineRenderer));belt.transform.SetParent(a.Root,false);
                var line=belt.GetComponent<LineRenderer>();line.useWorldSpace=false;line.sharedMaterial=c.Frame;line.startWidth=line.endWidth=.005f;
                line.positionCount=3;line.SetPositions(new[]{first.Cam.transform.localPosition+new Vector3(.15f,.057f,0),new Vector3(corner?.40f:.15f,.057f,0),second.Cam.transform.localPosition+new Vector3(corner?0:.15f,.057f,corner?.15f:0)});
            }
            // The second actuator is physically north of gate one. A player can
            // return to the previous court at both retained positions: the first
            // rotor has a swept slot, rather than closing again at tooth two.
            HardCampaignModules.Pocket(a,new Vector3(-.25f,-.056f,-.005f),Quaternion.identity,.10f,.12f,.08f);
            if(corner)
                a.Block("Corner court braking island",new Vector3(-.15f,0,.36f),new Vector3(.14f,.12f,.12f),c.Glass);
            else if(s.index==48)
                a.Block("Independent second cam approach fork",new Vector3(.17f,0,.07f),new Vector3(.010f,.12f,.17f),c.Glass);
            return l;
        }

        private static LevelRuntime SharedCam(CampaignLevelSpec s,CampaignBuildContext c)
        {
            var l=c.Planar(HardCampaignBuilder.Name(s),HardCampaignBuilder.Rectangle(.36f,.32f),new Vector2(-.27f,-.24f),new Vector2(.25f,.24f),.12f);
            var a=HardCampaignBuilder.Author(l,c);a.AddBall(new Vector3(.245f,-.040f,-.24f));
            HardCampaignModules.Cam(a,Vector3.zero,Quaternion.identity,1);
            HardCampaignModules.Pocket(a,new Vector3(-.27f,-.056f,-.25f),Quaternion.Euler(0,180,0),.10f,.10f,.08f);
            HardCampaignModules.Pocket(a,new Vector3(.25f,-.056f,-.25f),Quaternion.Euler(0,180,0),.10f,.10f,.08f);
            a.Block("Two start courts central nose",new Vector3(0,0,-.27f),new Vector3(.010f,.12f,.10f),c.Glass);
            return l;
        }

        private static LevelRuntime Parallel(CampaignLevelSpec s,CampaignBuildContext c)
        {
            var l=c.Planar(HardCampaignBuilder.Name(s),HardCampaignBuilder.Rectangle(.74f,.34f),new Vector2(-.64f,-.24f),new Vector2(0,.26f),.12f);
            var a=HardCampaignBuilder.Author(l,c);a.AddBall(new Vector3(.48f,-.040f,-.24f));
            HardCampaignModules.Cam(a,new Vector3(-.37f,0,0),Quaternion.identity,1,0,.37f);
            HardCampaignModules.Cam(a,new Vector3(.37f,0,0),Quaternion.identity,1,0,.37f);
            HardCampaignBuilder.Wall(a,"Independent workshop divider",-.005f,.005f,-.34f,.13f,-.06f,.06f);
            HardCampaignModules.Pocket(a,new Vector3(-.63f,-.056f,-.24f),Quaternion.Euler(0,180,0),.10f,.10f,.08f);
            HardCampaignModules.Pocket(a,new Vector3(.48f,-.056f,-.24f),Quaternion.Euler(0,180,0),.10f,.10f,.08f);
            return l;
        }

        private static LevelRuntime BridgeLoop(CampaignLevelSpec s,CampaignBuildContext c)
        {
            var l=c.Planar(HardCampaignBuilder.Name(s),HardCampaignBuilder.Rectangle(.80f,.36f),new Vector2(-.66f,-.13f),new Vector2(-.66f,.30f),.30f,spawnY:-.038f);
            var a=HardCampaignBuilder.Author(l,c);
            HardCampaignModules.Cam(a,new Vector3(-.40f,0,0),Quaternion.Euler(0,90,0),1,0,.24f);
            // Seal the entire under/over-cam space while retaining the original
            // 120 mm working height. No path can skirt its rotor in the deep box.
            a.Block("Cam west lower support",new Vector3(-.425f,-.105f,-.06f),new Vector3(.75f,.09f,.60f),c.Frame);
            a.Block("Cam west upper cover",new Vector3(-.425f,.105f,-.06f),new Vector3(.75f,.09f,.60f),c.Glass);
            HardCampaignBuilder.Wall(a,"Cam south bypass closure",-.405f,-.395f,-.36f,-.24f,-.06f,.06f);
            HardCampaignModules.Ramp(a,new Vector3(-.09f,-.056f,0),new Vector3(.025f,-.003f,0),.12f,c.Frame);
            HardCampaignModules.Bridge(a,new Vector3(.30f,0,0),Quaternion.identity,true);
            HardCampaignBuilder.Wall(a,"Bridge lower outer partition",.295f,.305f,-.36f,-.17f,-.15f,.15f);
            HardCampaignBuilder.Wall(a,"Bridge upper outer partition",.295f,.305f,.17f,.25f,-.15f,.15f);
            // The final return lane is physically isolated from the start and
            // runs back behind the cam after the bridge's receiving court.
            HardCampaignBuilder.Wall(a,"Retained mechanism return-loop wall",-.80f,.57f,.246f,.254f,-.15f,.15f);
            return l;
        }

        private static LevelRuntime Stacked(CampaignLevelSpec s,CampaignBuildContext c)
        {
            bool balance=s.index==92||s.index==97,cam=s.index==92||s.index==93,cage=s.index==93||s.index==97;
            var l=c.Planar(HardCampaignBuilder.Name(s),HardCampaignBuilder.Rectangle(.43f,.34f),new Vector2(-.26f,-.24f),new Vector2(0,.18f),balance?1.10f:.72f);
            var a=HardCampaignBuilder.Author(l,c);
            MechanicalHeart heart=null;MemoryRatchetAssembly memory=null;
            if(balance)heart=BossCooperationBuilder.AddBalance(a,new Vector3(0,.36f,0));
            if(cam)
            {
                float y=balance?.03f:.20f;
                memory=HardCampaignModules.Cam(a,new Vector3(0,y,0),Quaternion.identity,s.index==93?2:1);
                CamCourt(a,y,balance,new Vector2(0,s.index==93?.17f:.22f));
                if(balance)
                {
                    BossCooperationBuilder.Tube(a,"Balance to accessible rack court",new Vector3(0,.145f,-.19f),.116f,.082f);
                    // The rack contact physically arms the already loaded beam's
                    // receiver. Its own spring is supplied once by the cam.
                    UnityEngine.Object.DestroyImmediate(heart.ReleasePlunger.gameObject);
                    var contact=memory.Rack.gameObject.AddComponent<PressurePlunger>();contact.Guide=memory.Rack;contact.Spring=contact.Damper=0;contact.ActivationTravel=.045f;
                    heart.ReleasePlunger=contact;
                }
                else
                {
                    l.BallSpawn.localPosition=new Vector3(-.26f,y-.040f,-.24f);
                    a.AddBall(new Vector3(.25f,y-.040f,-.24f));
                    HardCampaignModules.Pocket(a,new Vector3(.25f,y-.056f,-.23f),Quaternion.Euler(0,180,0),.10f,.10f,.08f);
                }
            }
            if(cage)
            {
                Vector3 center=new Vector3(0,s.index==93?-.04f:0,s.index==93?.17f:-.19f);
                var moving=BossCooperationBuilder.TransferCage(a,center,Quaternion.identity);
                if(heart!=null)heart.Cage=moving;
                float upper=s.index==93?.14f:.20f;
                BossCooperationBuilder.Tube(a,"Retained stage to cage inlet",new Vector3(center.x,(upper+center.y+.085f)*.5f,center.z),upper-center.y-.085f,.082f);
                // A local receiving pan under the moving mouth, with one genuine
                // bore. Misses remain in this pan; the prior latch remains set.
                Vector2[] outline={new Vector2(-.25f,-.32f),new Vector2(.25f,-.32f),new Vector2(.25f,.32f),new Vector2(-.25f,.32f)};
                BossGeometry.Mesh(a,"Cage receiving pan",PhysicsLabGeometry.Panel("Campaign local catch",outline,-.17f,.004f,new Vector2(0,.18f),.04f),c.Glass);
                BossGeometry.Mesh(a,"Cage receiving pan rim",PhysicsLabGeometry.Border("Campaign local catch rim",outline,.006f,-.17f,-.09f),c.Glass);
                BossCooperationBuilder.Tube(a,"Catch to real exit enclosure",new Vector3(0,(-.17f-l.InteriorDepth*.5f)*.5f,.18f),l.InteriorDepth*.5f-.17f,.082f);
            }
            else
            {
                // Cam output drains to a sealed lower court containing the final
                // exit. Both partners retain the route after the first leaves.
                BossCooperationBuilder.Tube(a,"Cam to final physical bore",new Vector3(0,(-.03f-.55f)*.5f,.22f),.52f,.082f);
                // Move the real aperture and matching shell together by replacing
                // the bottom panel, never merely moving an exit assist marker.
                RecutExit(l,c,new Vector2(0,.22f));
            }
            return l;
        }

        private static void CamCourt(MechanicalAuthoring a,float y,bool inlet,Vector2 outlet)
        {
            var outline=HardCampaignBuilder.Rectangle(.36f,.30f);
            BossGeometry.Mesh(a,"Sealed cam stage floor",PhysicsLabGeometry.Panel("Campaign cam floor",outline,y-.06f,.003f,outlet,.04f),a.Glass);
            BossGeometry.Mesh(a,"Sealed cam stage roof",PhysicsLabGeometry.Panel("Campaign cam roof",outline,y+.06f,.003f,new Vector2(0,-.19f),inlet?.04f:0),a.Glass);
            BossGeometry.Mesh(a,"Sealed cam stage perimeter",PhysicsLabGeometry.Border("Campaign cam perimeter",outline,.006f,y-.06f,y+.06f),a.Glass);
        }
        private static void RecutExit(LevelRuntime l,CampaignBuildContext c,Vector2 at)
        {
            var floor=l.transform.Find("Floor with circular cut").gameObject;
            Mesh mesh=PhysicsLabGeometry.Panel(l.name+" physical exit floor",l.Footprint,-l.InteriorDepth*.5f,.003f,at,.023f);
            floor.GetComponent<MeshFilter>().sharedMesh=mesh;floor.GetComponent<MeshCollider>().sharedMesh=mesh;
            l.Exit.transform.localPosition=new Vector3(at.x,-l.InteriorDepth*.5f,at.y);
        }

        private static LevelRuntime Choice(CampaignLevelSpec s,CampaignBuildContext c)
        {
            // Four non-overlapping work zones: cam, bridge, choice, final court.
            // The north branch is a cage; the south branch has a true flight gap.
            var l=c.Planar(HardCampaignBuilder.Name(s),HardCampaignBuilder.Rectangle(1.13f,.43f),new Vector2(-.96f,-.22f),new Vector2(1.05f,.23f),.40f,spawnY:-.177f);
            var a=HardCampaignBuilder.Author(l,c);a.AddBall(new Vector3(-.96f,-.177f,.22f));
            // The world is shifted 230 mm left relative to the individual module
            // sketches so the complete route fits a centred enclosure.
            const float shift=.23f;
            Vector3 CamAt=new Vector3(-.95f+shift,-.14f,0);
            HardCampaignModules.Cam(a,CamAt,Quaternion.Euler(0,90,0),1);
            a.Block("Cam-only upper depth closure",new Vector3(-.745f,.061f,0),new Vector3(.59f,.278f,.86f),c.Glass);
            HardCampaignBuilder.Wall(a,"Cam south perimeter extension",-.955f+shift,-.945f+shift,-.43f,-.36f,-.20f,-.08f);
            HardCampaignBuilder.Wall(a,"Cam north perimeter extension",-.955f+shift,-.945f+shift,.36f,.43f,-.20f,-.08f);
            HardCampaignModules.Ramp(a,new Vector3(-.65f+shift,-.196f,0),new Vector3(-.477f+shift,-.003f,0),.14f,c.Frame);
            HardCampaignModules.Bridge(a,new Vector3(-.20f+shift,0,0),Quaternion.identity,true,-.036f);
            HardCampaignBuilder.Wall(a,"Bridge south whole-depth partition",-.205f+shift,-.195f+shift,-.43f,-.17f,-.20f,.20f);
            HardCampaignBuilder.Wall(a,"Bridge north whole-depth partition",-.205f+shift,-.195f+shift,.17f,.43f,-.20f,.20f);
            a.Block("Bridge under-sill closure",new Vector3(-.20f+shift,-.16f,0),new Vector3(.010f,.080f,.34f),c.Glass);
            a.Block("Bridge over-tip closure",new Vector3(-.20f+shift,.177f,0),new Vector3(.010f,.046f,.34f),c.Glass);
            // Broad intermediate deck and an off-axis refuge separate the acts.
            a.Block("Shared choice court",new Vector3(.125f+shift,-.118f,0),new Vector3(.31f,.164f,.82f),c.Frame);
            HardCampaignModules.Pocket(a,new Vector3(.14f+shift,-.032f,.33f),Quaternion.identity,.10f,.10f,.075f);
            Vector3 cageAt=new Vector3(.48f+shift,-.02f,.23f);
            HardCampaignModules.Cage(a,cageAt,Quaternion.identity,true);
            HardSpatialLevels.CageSleeve(a,cageAt,Quaternion.identity);
            HardCampaignModules.Ramp(a,new Vector3(.25f+shift,-.032f,.23f),new Vector3(.19f+shift,-.12f,.23f),.11f,c.Frame);
            // One flight lane, used serially by either ball.
            Vector3 launchAt=new Vector3(.40f+shift,0,-.24f);
            HardCampaignModules.Flight(a,launchAt,Quaternion.identity,.025f);
            HardCampaignModules.Ramp(a,new Vector3(.14f+shift,-.032f,-.24f),new Vector3(.13f+shift,.120f,-.24f),.10f,c.Frame);
            // The final court is accessible only at the cage's receiving height
            // or from inside the enclosed flight receiver below its bore.
            const float boundary=.969f;
            HardCampaignBuilder.Wall(a,"Final boundary south jamb",boundary-.004f,boundary+.004f,-.43f,-.328f,-.20f,.20f);
            HardCampaignBuilder.Wall(a,"Final boundary middle jamb",boundary-.004f,boundary+.004f,-.102f,.10f,-.20f,.20f);
            HardCampaignBuilder.Wall(a,"Final boundary north jamb",boundary-.004f,boundary+.004f,.36f,.43f,-.20f,.20f);
            HardCampaignBuilder.Wall(a,"Flight final outlet lintel",boundary-.004f,boundary+.004f,-.328f,-.102f,-.13f,.20f);
            HardCampaignBuilder.Wall(a,"Cage final outlet sill",boundary-.004f,boundary+.004f,.10f,.36f,-.20f,-.145f);
            HardCampaignBuilder.Wall(a,"Cage final outlet lintel",boundary-.004f,boundary+.004f,.10f,.36f,.13f,.20f);
            // Relocate the receiving back to the final boundary, then open only
            // its lower transfer face. All other tower faces remain floor-tight.
            Transform back=l.transform.Find("Flight braking back");
            back.localPosition=new Vector3(boundary,-.036f,-.215f);back.localScale=new Vector3(.008f,.188f,.22f);
            // Seal the extra depth beneath the receiver's original 188 mm skirt.
            a.Block("Flight tower bottom front extension",new Vector3(launchAt.x+.086f,-.208f,-.215f),new Vector3(.008f,.025f,.22f),c.Glass);
            foreach(int sign in new[]{-1,1})
                a.Block("Flight tower bottom side extension "+sign,new Vector3(launchAt.x+.215f,-.208f,-.215f+sign*.113f),new Vector3(.25f,.025f,.006f),c.Glass);
            return l;
        }
    }
}
