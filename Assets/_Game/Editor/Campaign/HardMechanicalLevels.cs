using System;
using GravityBox.Gameplay;
using GravityBox.Simulation;
using UnityEngine;

namespace GravityBox.Editor
{
    internal static class HardMechanicalLevels
    {
        internal static LevelRuntime Bridge(CampaignLevelSpec s,CampaignBuildContext c)
        {
            bool gate=s.index==18;float x=gate?.10f:0;
            // C17 finishes down the east side of the return bend. Its north
            // court is the recovery-ramp toe, so the real bore must be south
            // of that support. Planar cuts the panel at this same coordinate.
            Vector2 exit=new Vector2(gate?.41f:.31f,s.index==17?-.11f:0);
            var l=c.Planar(HardCampaignBuilder.Name(s),HardCampaignBuilder.Rectangle(gate?.65f:.37f,.17f),
                new Vector2(gate?-.55f:-.22f,s.index==16?.09f:-.085f),exit,.30f,spawnY:.018f);
            var a=HardCampaignBuilder.Author(l,c);
            HardCampaignModules.Bridge(a,new Vector3(x,0,0),Quaternion.identity,s.index!=16);
            if(gate)
            {
                a.Block("Gate entrance raised court",new Vector3(-.4135f,-.056f,0),new Vector3(.473f,.106f,.34f),c.Frame);
                HardCampaignModules.Slider(a,new Vector3(-.39f,0,0),Quaternion.identity,.17f,.15f);
                HardCampaignModules.Pocket(a,new Vector3(-.26f,-.003f,.085f),Quaternion.identity,.10f,.095f);
            }
            if(s.index==16)
            {
                // Change the usable rest direction: the first approach is north,
                // the safe bridge-fold pocket opens from the south.
                a.Block("North approach turning cheek",new Vector3(-.08f,.04f,.10f),new Vector3(.012f,.08f,.14f),c.Glass);
            }
            if(s.index==17)
            {
                // A physical return bend after the landing makes the bridge-to-
                // court transfer a separate braking decision.
                HardCampaignBuilder.Wall(a,"Landing return bend",.27f,.28f,-.17f,.03f,-.15f,.15f);
                HardCampaignModules.Pocket(a,new Vector3(.325f,-.146f,.11f),Quaternion.identity,.065f,.075f,.060f);
            }
            return l;
        }

        internal static LevelRuntime Cooperation(CampaignLevelSpec s,CampaignBuildContext c)
        {
            var l=c.ClonePrototype(18,HardCampaignBuilder.Name(s));
            var a=HardCampaignBuilder.Author(l,c);
            foreach(var label in l.GetComponentsInChildren<TextMesh>())UnityEngine.Object.DestroyImmediate(label.gameObject);
            var latch=l.GetComponentInChildren<ContactSeatLatch>();
            Transform carrier=latch.Hinge.transform.Find("Balanced carrier frame");
            if(s.index==33)
            {
                // A genuinely shorter long arm with its corresponding exit wall
                // and receiver brought inward, not a scale of the rigidbody.
                foreach(string n in new[]{"A long arm floor","A long arm near rail","A long arm far rail","A long arm clear roof"})
                {
                    Transform t=carrier.Find(n);Vector3 p=t.localPosition;p.x+=.03f;t.localPosition=p;
                    Vector3 size=t.localScale;size.x-=.06f;t.localScale=size;
                }
                foreach(Transform child in l.transform)
                    if(child.name.StartsWith("A fixed")||child.name.StartsWith("A left window")||child.name.StartsWith("A right window"))child.localPosition+=Vector3.right*.06f;
                latch.Receiver.transform.localPosition+=Vector3.right*.06f;
                l.BallSpawn.localPosition=latch.Hinge.transform.localPosition+Quaternion.Euler(0,0,-25)*new Vector3(-.105f,.02f,-.055f);
                // Fill the old west floor gap to make the shortened arm's exit
                // land in an actual fixed court.
                a.Block("Short arm broad receiving extension",new Vector3(-.22f,-.002f,0),new Vector3(.09f,.008f,.22f),c.Frame);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(carrier.Find("A long arm floor").gameObject);
                a.Block("Wide A contact pocket recessed floor",new Vector3(-.20f,-.008f,-.055f),new Vector3(.064f,.006f,.070f),c.Frame,carrier);
                a.Block("A contact pocket outer lip",new Vector3(-.236f,0,-.055f),new Vector3(.008f,.006f,.070f),c.Frame,carrier);
                a.Block("A contact pocket approach",new Vector3(-.061f,0,-.055f),new Vector3(.214f,.006f,.070f),c.Frame,carrier);
            }
            if(s.index==35)
            {
                // Force a choice of the useful far loading region from an open
                // central starting section; the receiving court now turns north.
                l.BallSpawn.localPosition=latch.Hinge.transform.localPosition+Quaternion.Euler(0,0,-25)*new Vector3(-.045f,.020f,-.055f);
                a.Block("B receiver braking cheek",new Vector3(.268f,.040f,.010f),new Vector3(.008f,.080f,.082f),c.Glass);
            }
            if(s.index==36||s.index==38)
            {
                // P23's learned contact latch, without its narrow housing/cage.
                // B reaches a real spring plunger only after the beam raises it.
                var heart=l.gameObject.AddComponent<MechanicalHeart>();heart.BridgeCatch=latch;latch.Armed=false;
                Vector3 at=new Vector3(.234f,.026f,.082f);
                heart.ReleasePlunger=AddReleasePlunger(a,at);
                a.Block("Latch pocket east cheek",new Vector3(.284f,.027f,.054f),new Vector3(.008f,.05f,.09f),c.Glass);
                if(s.index==38)
                {
                    a.Block("Latch pocket west cheek",new Vector3(.19f,.027f,.080f),new Vector3(.008f,.050f,.064f),c.Glass);
                    HardCampaignBuilder.Wall(a,"Release recovery S bend",.025f,.035f,-.23f,-.155f,-.16f,-.09f);
                    HardCampaignBuilder.Wall(a,"Release second S bend",-.11f,-.10f,-.18f,-.115f,-.16f,-.09f);
                }
                else
                {
                    // The only new relationship is the retaining contact: A is
                    // already in the broad load recess, ready to hold the beam.
                    l.BallSpawn.localPosition=latch.Hinge.transform.localPosition+Quaternion.Euler(0,0,-25)*new Vector3(-.202f,.014f,-.055f);
                }
            }
            if(s.index==37)
            {
                // Separate safe side courts join through a south U route. The
                // receiver remains latched regardless of which ball leaves first.
                HardCampaignModules.Pocket(a,new Vector3(-.28f,.002f,.05f),Quaternion.identity,.060f,.10f,.070f);
                HardCampaignModules.Pocket(a,new Vector3(.235f,.002f,.05f),Quaternion.identity,.095f,.10f,.070f);
                HardCampaignBuilder.Wall(a,"Two courts common exit splitter",-.050f,.050f,-.153f,-.142f,-.16f,-.03f);
            }
            return l;
        }

        internal static PressurePlunger AddReleasePlunger(MechanicalAuthoring a,Vector3 at)
        {
            var go=new GameObject("Campaign retaining contact plunger",typeof(Rigidbody),typeof(PhysicalProp),typeof(ConfigurableJoint));
            go.transform.SetParent(a.Root,false);go.transform.localPosition=at;
            var body=go.GetComponent<Rigidbody>();body.mass=.020f;body.useGravity=false;body.solverIterations=32;body.solverVelocityIterations=12;
            a.Block("Retaining contact face",Vector3.zero,new Vector3(.082f,.044f,.008f),a.Rim,go.transform);
            var joint=go.GetComponent<ConfigurableJoint>();joint.autoConfigureConnectedAnchor=false;joint.connectedBody=a.Root.GetComponent<Rigidbody>();
            joint.connectedAnchor=at+Vector3.forward*.004f;joint.axis=Vector3.forward;joint.secondaryAxis=Vector3.up;
            joint.xMotion=ConfigurableJointMotion.Limited;joint.yMotion=joint.zMotion=ConfigurableJointMotion.Locked;
            joint.angularXMotion=joint.angularYMotion=joint.angularZMotion=ConfigurableJointMotion.Locked;
            joint.linearLimit=new SoftJointLimit{limit=.004f,contactDistance=.0002f};joint.enableCollision=false;
            var guide=go.AddComponent<GravitySliderGuide>();guide.Configure(joint,at,Vector3.forward,.008f,.003f);
            var pressure=go.AddComponent<PressurePlunger>();pressure.Guide=guide;return pressure;
        }

        internal static LevelRuntime Flight(CampaignLevelSpec s,CampaignBuildContext c)
        {
            bool pendulum=s.index==58,two=s.index==96,turn=s.index==57;
            var l=c.Planar(HardCampaignBuilder.Name(s),HardCampaignBuilder.Rectangle(.38f,pendulum?.45f:turn?.40f:.27f),
                new Vector2(-.27f,pendulum?.33f:0),new Vector2(.26f,turn?.32f:.13f),.40f,spawnY:pendulum?-.115f:.143f);
            var a=HardCampaignBuilder.Author(l,c);
            // The mouth starts 20 mm beyond the straight launch centreline. A
            // 30 mm sphere must move laterally about 35 mm to land wholly inside.
            HardCampaignModules.Flight(a,Vector3.zero,Quaternion.identity,.13f);
            if(two)
            {
                a.AddBall(new Vector3(-.27f,.144f,.12f));
                HardCampaignModules.Pocket(a,new Vector3(.27f,-.037f,.205f),Quaternion.identity,.08f,.06f,.075f);
            }
            if(pendulum)
            {
                // A high waiting court leads through the stable pendulum to the
                // launch side pocket; the two operations are separated by a floor.
                HardCampaignModules.Pendulum(a,new Vector3(-.27f,0,.22f),Quaternion.identity,.11f);
                a.Block("Pendulum waiting deck",new Vector3(-.27f,-.137f,.33f),new Vector3(.22f,.008f,.23f),c.Frame);
                a.Block("Post pendulum preparation landing",new Vector3(-.27f,-.137f,.165f),new Vector3(.22f,.008f,.11f),c.Frame);
                HardCampaignModules.Ramp(a,new Vector3(-.27f,-.133f,.165f),new Vector3(-.27f,.120f,.07f),.095f,c.Frame);
            }
            if(turn)
            {
                // Extend the receiving tower's lower court into a 90-degree
                // corridor, with the final bore after the bend.
                var side=l.transform.Find("Flight receiver side 1");
                side.localPosition=new Vector3(.215f,-.014f,.243f);side.localScale=new Vector3(.25f,.144f,.006f);
                a.Block("L receiver north continuation back",new Vector3(.34f,-.14f,.31f),new Vector3(.008f,.12f,.134f),c.Glass);
                a.Block("L receiver north continuation west",new Vector3(.09f,-.14f,.31f),new Vector3(.008f,.12f,.134f),c.Glass);
                a.Block("L receiver north continuation cap",new Vector3(.215f,-.14f,.378f),new Vector3(.25f,.12f,.008f),c.Glass);
                // Below the catch deck, a baffle prevents a direct diagonal roll.
                a.Block("L lower-court turn baffle",new Vector3(.26f,-.165f,.22f),new Vector3(.15f,.070f,.008f),c.Glass);
            }
            return l;
        }

        internal static LevelRuntime Memory(CampaignLevelSpec s,CampaignBuildContext c)
        {
            return HardMemoryLevels.Build(s,c);
        }
    }
}
