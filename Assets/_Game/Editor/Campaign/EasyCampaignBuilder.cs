using System;
using GravityBox.Gameplay;
using GravityBox.Simulation;
using UnityEngine;
using static GravityBox.Editor.EasyCampaignGeometry;

namespace GravityBox.Editor
{
    /// <summary>
    /// Physical authoring for the 55 low-load campaign beats. Every handled index changes a route,
    /// dependency, supporting face, clearance, or recovery court described by the campaign matrix.
    /// </summary>
    internal static class EasyCampaignBuilder
    {
        internal static bool Handles(int index)
        {
            switch (index)
            {
                case 1: case 2: case 3: case 4: case 5: case 6: case 7: case 8: case 9:
                case 11: case 12: case 13: case 14: case 19: case 21: case 22: case 24: case 26: case 29:
                case 31: case 32: case 34: case 39: case 41: case 42: case 43: case 44: case 49:
                case 51: case 52: case 53: case 54: case 55: case 59:
                case 61: case 62: case 63: case 64: case 65: case 66: case 69:
                case 71: case 72: case 73: case 74: case 75: case 76: case 79:
                case 81: case 82: case 83: case 84: case 89: case 94: case 99:
                    return true;
                default: return false;
            }
        }

        internal static LevelRuntime Build(CampaignLevelSpec spec, CampaignBuildContext c)
        {
            switch (spec.index)
            {
                case 1: return C01(c);
                case 2: return C02(c);
                case 3: return C03(c);
                case 4: return C04(c);
                case 5: return C05(c);
                case 6: return C06(c);
                case 7: return C07(c);
                case 8: return C08(c);
                case 9: return C09(c);
                case 11: return GateLesson(c, 11);
                case 12: return GateLesson(c, 12);
                case 13: return GateLesson(c, 13);
                case 14: return GateCorner(c);
                case 19: return GateRing(c);
                case 21: return LayerLesson(c, 21);
                case 22: return LayerLesson(c, 22);
                case 24: return CageRest(c);
                case 26: return ShortSpatial(c);
                case 29: return CageToSpatial(c);
                case 31: return TwoBallOpen(c);
                case 32: return TwoBallLanes(c);
                case 34: return BalanceRest(c);
                case 39: return HeartRest(c);
                case 41: return MemoryLesson(c, 41);
                case 42: return MemoryLesson(c, 42);
                case 43: return MemoryCorner(c);
                case 44: return MemoryLesson(c, 44);
                case 49: return MemoryRing(c);
                case 51: return PendulumLesson(c, 51);
                case 52: return PendulumLesson(c, 52);
                case 53: return PendulumCorner(c);
                case 54: return PendulumLesson(c, 54);
                case 55: return FlightLesson(c, 55);
                case 59: return FlightLesson(c, 59);
                case 61: return LiquidCalibration(c, false, 61);
                case 62: return LiquidCalibration(c, true, 62);
                case 63: return WaterL(c, false, 63);
                case 64: return LiquidArc(c, false, 64);
                case 65: return LiquidChoice(c, false, 65);
                case 66: return LiquidLayers(c, false, 66);
                case 69: return LiquidBasin(c, false, 69);
                case 71: return LiquidCalibration(c, true, 71);
                case 72: return LiquidCalibration(c, false, 72);
                case 73: return WaterL(c, true, 73);
                case 74: return LiquidArc(c, true, 74);
                case 75: return LiquidChoice(c, true, 75);
                case 76: return LiquidLayers(c, true, 76);
                case 79: return LiquidBasin(c, true, 79);
                case 81: return DryRelief(c);
                case 82: return DryTopology(c, 82);
                case 83: return DryTopology(c, 83);
                case 84: return DryReliefL(c);
                case 89: return CageStar(c);
                case 94: return MemoryTwoBall(c);
                case 99: return BalanceFinale(c);
                default: throw new ArgumentOutOfRangeException(nameof(spec.index), spec.index, "Easy campaign builder does not own this index.");
            }
        }

        private static LevelRuntime C01(CampaignBuildContext c)
            => c.Planar("C01 Gentle roll", Circle(.17f), V(-.055f,.020f), V(.095f,-.085f));

        private static LevelRuntime C02(CampaignBuildContext c)
        {
            LevelRuntime level = c.Planar("C02 Brake court", Circle(.19f), V(-.115f,.035f), V(.105f,-.085f));
            var a = Author(level,c);
            // The crescent catches an overrun but leaves a 96 mm open return around either end.
            Wall(a,"Wide braking crescent",V(-.020f,-.160f),V(.100f,-.020f),.09f,.009f);
            return level;
        }

        private static LevelRuntime C03(CampaignBuildContext c)
        {
            LevelRuntime level=c.Planar("C03 Two ways around",Rectangle(.18f,.17f),V(-.125f,.085f),V(.125f,-.105f));
            var a=Author(level,c);
            a.Block("Single impact cube",new Vector3(0,0,0),new Vector3(.064f,.084f,.064f),c.Frame);
            return level;
        }

        private static LevelRuntime C04(CampaignBuildContext c)
        {
            Vector2[] triangle={V(-.19f,-.11f),V(.19f,-.11f),V(0,.219f)};
            LevelRuntime level=c.Planar("C04 Quiet triangle",triangle,V(-.060f,.020f),V(.082f,-.068f));
            var a=Author(level,c);
            Wall(a,"Friendly angled guide",V(-.118f,.055f),V(.030f,.142f),.09f,.008f);
            return level;
        }

        private static LevelRuntime C05(CampaignBuildContext c)
        {
            Vector2[] l={V(-.25f,-.24f),V(.25f,-.24f),V(.25f,-.045f),V(-.045f,-.045f),V(-.045f,.24f),V(-.25f,.24f)};
            LevelRuntime level=c.Planar("C05 One broad turn",l,V(-.155f,.155f),V(.170f,-.145f));
            var a=Author(level,c);
            Wall(a,"Overrun return cheek",V(-.025f,-.158f),V(.095f,-.158f),.09f,.008f);
            return level;
        }

        private static LevelRuntime C06(CampaignBuildContext c)
            => c.Planar("C06 Open ring",Circle(.25f,96),V(-.183f,0),V(.183f,0),.09f,new[]{Circle(.105f,64,true)});

        private static LevelRuntime C07(CampaignBuildContext c)
        {
            Vector2[] u={V(-.25f,-.24f),V(.25f,-.24f),V(.25f,.24f),V(.075f,.24f),V(.075f,-.045f),V(-.075f,-.045f),V(-.075f,.24f),V(-.25f,.24f)};
            LevelRuntime level=c.Planar("C07 Wide U court",u,V(-.165f,.150f),V(.165f,.150f));
            var a=Author(level,c);
            Wall(a,"Bottom speed softener",V(-.145f,-.145f),V(.045f,-.205f),.09f,.008f);
            return level;
        }

        private static LevelRuntime C08(CampaignBuildContext c)
        {
            Vector2[] dumbbell={V(-.34f,-.08f),V(-.28f,-.15f),V(-.13f,-.15f),V(-.060f,-.058f),V(.060f,-.058f),V(.13f,-.15f),V(.28f,-.15f),V(.34f,-.08f),V(.34f,.08f),V(.28f,.15f),V(.13f,.15f),V(.060f,.058f),V(-.060f,.058f),V(-.13f,.15f),V(-.28f,.15f),V(-.34f,.08f)};
            LevelRuntime level=c.Planar("C08 Friendly neck",dumbbell,V(-.230f,0),V(.235f,0));
            var a=Author(level,c);
            Wall(a,"West alignment cheek",V(-.160f,-.118f),V(-.075f,-.050f),.09f,.007f);
            Wall(a,"East alignment cheek",V(.075f,.050f),V(.160f,.118f),.09f,.007f);
            return level;
        }

        private static LevelRuntime C09(CampaignBuildContext c)
        {
            return c.Planar("C09 Shallow star",Star(.245f,.158f,5),V(-.105f,-.105f),V(0,.190f));
        }

        private static LevelRuntime GateLesson(CampaignBuildContext c,int index)
        {
            LevelRuntime level=c.ClonePrototype(9,$"C{index:00} gravity gate lesson");
            var a=Author(level,c);
            if(index==11)
            {
                RemoveContaining(level,"Holding recess");
                level.BallSpawn.localPosition=new Vector3(-.205f,-.024f,-.105f);
                Wall(a,"Visible gate approach guide",V(-.245f,-.020f),V(-.080f,-.020f));
            }
            else if(index==12)
            {
                RemoveContaining(level,"Holding recess");
                Pocket(a,"Shallow waiting pocket",V(-.190f,.075f),V(-.095f,.020f),.09f,.050f,.045f);
                level.BallSpawn.localPosition=new Vector3(-.195f,-.024f,.070f);
            }
            else
            {
                RemoveContaining(level,"Holding recess");
                Pocket(a,"Practice holding pocket",V(-.205f,.080f),V(-.105f,.010f),.09f,.045f,.052f);
                Wall(a,"Post gate waiting rail",V(.060f,.060f),V(.235f,.060f));
                level.BallSpawn.localPosition=new Vector3(-.205f,-.024f,.075f);
            }
            return level;
        }

        private static LevelRuntime GateCorner(CampaignBuildContext c)
        {
            LevelRuntime level=c.ClonePrototype(10,"C14 gate then broad corner");
            // The rest level keeps one familiar gate and the authored corner below. Remove every
            // prototype crosswall so the approach does not retain the longer alternating maze.
            RemoveContaining(level,"Maze gate B","Gate B ","West maze lower","West maze upper",
                "East maze lower","East maze middle","East maze upper");
            var a=Author(level,c);
            Wall(a,"Single post gate corner",V(.015f,-.105f),V(.215f,-.105f));
            Wall(a,"Single post gate guide",V(.215f,-.105f),V(.215f,.155f));
            level.BallSpawn.localPosition=new Vector3(-.245f,-.024f,-.190f);
            return level;
        }

        private static LevelRuntime GateRing(CampaignBuildContext c)
        {
            LevelRuntime level=c.ClonePrototype(9,"C19 gate and short arc");
            RemoveContaining(level,"Holding recess");
            var a=Author(level,c);
            // A three-sided island creates an open, reversible arc after the familiar gate.
            FixedBaffle(a,"Arc island west",V(.055f,-.115f),V(.055f,.105f));
            FixedBaffle(a,"Arc island north",V(.055f,.105f),V(.165f,.105f));
            FixedBaffle(a,"Arc island east",V(.165f,.105f),V(.165f,-.020f));
            return level;
        }

        private static LevelRuntime LayerLesson(CampaignBuildContext c,int index)
        {
            LevelRuntime level=c.ClonePrototype(11,$"C{index:00} two deck lesson");
            // Removing the middle deck turns P11 into two short, fully physical supporting faces.
            RemoveContaining(level,"Middle maze");
            UnityEngine.Object.DestroyImmediate(level.GetComponent<LayeredMaze>());
            var a=Author(level,c);
            RemoveContainingUnder(level,"Upper maze",index==21?"Row wall":"Column wall");
            if(index==21)
                Wall(a,"Upper one turn guide",V(-.270f,-.045f),V(.095f,-.045f),.09f,.010f);
            else
            {
                Wall(a,"Upper height marker wall",V(-.080f,-.225f),V(-.080f,.075f),.09f,.010f);
                Wall(a,"Lower reverse direction wall",V(.070f,-.075f),V(.070f,.220f),.09f,.010f);
            }
            return level;
        }

        private static LevelRuntime CageRest(CampaignBuildContext c)
        {
            LevelRuntime level=c.ClonePrototype(19,"C24 cage near stop");
            var a=Author(level,c);
            PhysicalHinge hinge=level.GetComponentInChildren<PhysicalHinge>();
            if(hinge==null) throw new InvalidOperationException("C24 prototype is missing its physical cage hinge.");
            // Keep the authored joint frame and connected anchor intact. Moving the near stop to -12 degrees
            // makes the neutral physical pose start close to the familiar limit without presetting a Rigidbody.
            hinge.Joint.limits=new JointLimits{min=-12,max=32,bounciness=0,contactDistance=.05f};
            level.BallSpawn.localPosition=new Vector3(0,-.004f,-.020f);
            a.Block("Broad mouth landing",new Vector3(0,-.174f,-.145f),new Vector3(.190f,.006f,.125f),c.Glass);
            return level;
        }

        private static LevelRuntime ShortSpatial(CampaignBuildContext c)
        {
            LevelRuntime level=c.Planar("C26 three face network",Rectangle(.30f,.25f),V(-.230f,-.180f),V(.220f,.205f),.27f,null,.066f);
            var a=Author(level,c);
            TwoDeckTransfer(a,"Three-face",true);
            Wall(a,"Upper first turn",V(-.245f,-.140f),V(-.050f,-.140f),.27f,.010f);
            Wall(a,"Lower final turn",V(.050f,.130f),V(.250f,.130f),.27f,.010f);
            return level;
        }

        private static LevelRuntime CageToSpatial(CampaignBuildContext c)
        {
            LevelRuntime level=c.ClonePrototype(19,"C29 cage to short network");
            var a=Author(level,c);
            a.Block("Outer transfer deck",new Vector3(.105f,-.095f,-.075f),new Vector3(.300f,.006f,.180f),c.Glass);
            Wall(a,"Outer deck turn",V(-.035f,-.150f),V(.255f,-.150f),.18f,.010f);
            return level;
        }

        private static LevelRuntime TwoBallOpen(CampaignBuildContext c)
        {
            LevelRuntime level=c.Planar("C31 two balls open",Rectangle(.25f,.22f),V(-.145f,.105f),V(.150f,-.125f));
            AddSecondBall(level,new Vector3(.050f,-.024f,.115f));
            var a=Author(level,c);
            FixedBaffle(a,"Open court divider",V(-.020f,.165f),V(-.020f,-.035f),.09f,.008f);
            return level;
        }

        private static LevelRuntime TwoBallLanes(CampaignBuildContext c)
        {
            LevelRuntime level=c.Planar("C32 two resting lanes",Rectangle(.29f,.24f),V(-.155f,.155f),V(0,-.175f));
            AddSecondBall(level,new Vector3(.155f,-.024f,.155f));
            var a=Author(level,c);
            FixedBaffle(a,"Lane separator",V(0,.205f),V(0,-.095f));
            Pocket(a,"Left resting pocket",V(-.155f,-.045f),V(-.085f,-.105f),.09f,.050f,.045f);
            Pocket(a,"Right resting pocket",V(.155f,-.045f),V(.085f,-.105f),.09f,.050f,.045f);
            return level;
        }

        private static LevelRuntime BalanceRest(CampaignBuildContext c)
        {
            LevelRuntime level=c.ClonePrototype(18,"C34 short balance release");
            Scale(level,"A long arm floor",new Vector3(.235f,.006f,.082f));
            Scale(level,"B short arm floor",new Vector3(.095f,.006f,.082f));
            Scale(level,"A landing",new Vector3(.090f,.008f,.250f));
            Scale(level,"B landing",new Vector3(.200f,.008f,.250f));
            RemoveContaining(level,"clear roof");
            return level;
        }

        private static LevelRuntime HeartRest(CampaignBuildContext c)
        {
            LevelRuntime level=c.ClonePrototype(23,"C39 broad heart rehearsal");
            Scale(level,"A holding pocket recessed floor",new Vector3(.062f,.006f,.085f));
            Scale(level,"Heart cage floor",new Vector3(.220f,.006f,.260f));
            Scale(level,"Receiver left cheek",new Vector3(.004f,.050f,.380f));
            Scale(level,"Receiver right cheek",new Vector3(.004f,.050f,.380f));
            return level;
        }

        private static LevelRuntime MemoryLesson(CampaignBuildContext c,int index)
        {
            LevelRuntime level=c.ClonePrototype(22,$"C{index:00} memory cam lesson");
            var a=Author(level,c);
            MemoryRatchetAssembly memory=level.GetComponent<MemoryRatchetAssembly>();
            if(index==41)
            {
                memory.MaximumTeeth=1; memory.PassageOpenDegrees=26;
                RemoveContaining(level,"Memory index 1","Memory index 2","Memory index 3");
                RemoveContaining(level,"Fixed cam sleeve");
                Wall(a,"One-notch direct lane",V(-.285f,-.175f),V(.250f,-.175f),.09f,.008f);
            }
            else if(index==42)
            {
                memory.MaximumTeeth=2; memory.PassageOpenDegrees=56;
                RemoveContaining(level,"Memory index 2","Memory index 3");
                RemoveContaining(level,"Fixed cam sleeve");
                Pocket(a,"Rack return court",V(-.245f,-.175f),V(-.145f,-.115f),.09f,.055f,.060f);
            }
            else
            {
                memory.MaximumTeeth=1; memory.PassageOpenDegrees=26;
                RemoveContaining(level,"Memory index 1","Memory index 2","Memory index 3");
                RemoveContaining(level,"Fixed cam sleeve");
            }
            return level;
        }

        private static LevelRuntime MemoryCorner(CampaignBuildContext c)
        {
            LevelRuntime level=c.ClonePrototype(22,"C43 rack on returnable L branch");
            var a=Author(level,c);
            FixedBaffle(a,"Rack branch divider",V(-.055f,-.285f),V(-.055f,-.055f));
            FixedBaffle(a,"Exit branch divider",V(-.055f,.145f),V(.265f,.145f));
            Pocket(a,"State reading bay",V(.205f,-.095f),V(.105f,-.025f),.09f,.052f,.050f);
            return level;
        }

        private static LevelRuntime MemoryRing(CampaignBuildContext c)
        {
            LevelRuntime level=c.ClonePrototype(22,"C49 one notch then arc");
            var a=Author(level,c);
            MemoryRatchetAssembly memory=level.GetComponent<MemoryRatchetAssembly>();
            memory.MaximumTeeth=1; memory.PassageOpenDegrees=26;
            RemoveContaining(level,"Memory index 1","Memory index 2","Memory index 3");
            RemoveContaining(level,"Fixed cam sleeve");
            FixedBaffle(a,"Arc centre west",V(.030f,-.205f),V(.030f,.200f));
            FixedBaffle(a,"Arc centre north",V(.030f,.200f),V(.185f,.200f));
            FixedBaffle(a,"Arc centre east",V(.185f,.200f),V(.185f,.045f));
            return level;
        }

        private static LevelRuntime PendulumLesson(CampaignBuildContext c,int index)
        {
            LevelRuntime level=c.ClonePrototype(20,$"C{index:00} pendulum lesson");
            var a=Author(level,c);
            if(index==51)
            {
                Scale(level,"Pendulum blocking bob",new Vector3(.082f,.066f,.014f));
                Scale(level,"Waiting pocket left guide",new Vector3(.008f,.068f,.235f));
                Scale(level,"Waiting pocket right guide",new Vector3(.008f,.068f,.235f));
            }
            else if(index==52)
            {
                Move(level,"Waiting pocket left guide",new Vector3(-.080f,-.103f,.137f));
                Move(level,"Waiting pocket right guide",new Vector3(-.008f,-.103f,.137f));
                level.BallSpawn.localPosition=new Vector3(-.044f,-.119f,.230f);
                Wall(a,"Diagonal crossing guide",V(-.045f,.095f),V(.180f,-.110f),.09f,.008f);
            }
            else
            {
                Scale(level,"Pendulum blocking bob",new Vector3(.075f,.060f,.014f));
                level.BallSpawn.localPosition=new Vector3(0,-.119f,.160f);
            }
            return level;
        }

        private static LevelRuntime PendulumCorner(CampaignBuildContext c)
        {
            LevelRuntime level=c.ClonePrototype(20,"C53 L approach pendulum");
            var a=Author(level,c);
            level.BallSpawn.localPosition=new Vector3(-.230f,-.119f,.215f);
            Wall(a,"L approach inner vertical",V(-.100f,.270f),V(-.100f,.075f),.09f,.010f);
            Wall(a,"L approach inner horizontal",V(-.100f,.075f),V(-.035f,.075f),.09f,.010f);
            Pocket(a,"Pendulum observation pocket",V(-.085f,.035f),V(-.020f,.005f),.09f,.040f,.042f);
            return level;
        }

        private static LevelRuntime FlightLesson(CampaignBuildContext c,int index)
        {
            LevelRuntime level=c.ClonePrototype(21,$"C{index:00} broad flight landing");
            if(index==55)
            {
                Scale(level,"Receiver front wall",new Vector3(.310f,.205f,.008f));
                Scale(level,"Receiver back wall",new Vector3(.310f,.205f,.008f));
                Scale(level,"Receiver left wall",new Vector3(.008f,.205f,.280f));
                Scale(level,"Receiver right wall",new Vector3(.008f,.205f,.280f));
            }
            else
            {
                Scale(level,"Receiver front wall",new Vector3(.280f,.225f,.008f));
                Scale(level,"Receiver back wall",new Vector3(.280f,.225f,.008f));
                var a=Author(level,c);
                a.Block("Short recovery return shelf",new Vector3(-.090f,-.270f,.145f),new Vector3(.250f,.008f,.150f),c.Glass);
            }
            return level;
        }

        private static LevelRuntime LiquidCalibration(CampaignBuildContext c,bool water,int index)
        {
            string medium=index==61?"dry":index==62?"water":index==71?"water calibration":"mercury";
            LevelRuntime level=c.Planar($"C{index:00} {medium} comparison",Rectangle(.16f,.16f),V(-.10f,0),V(.105f,-.105f));
            var a=Author(level,c);
            a.Block("Comparison cube",Vector3.zero,new Vector3(.064f,.084f,.064f),c.Frame);
            if(index==62 || index==71) AddLiquid(level,false,new Vector3(.16f,.042f,.16f));
            if(index==72) AddLiquid(level,true,new Vector3(.16f,.042f,.16f));
            return level;
        }

        private static LevelRuntime WaterL(CampaignBuildContext c,bool mercury,int index)
        {
            LevelRuntime level=c.Planar($"C{index:00} liquid L",Rectangle(.18f,.18f),V(-.125f,.120f),V(.120f,-.120f));
            var a=Author(level,c);
            FixedBaffle(a,"L baffle north",V(-.035f,.180f),V(-.035f,-.030f));
            FixedBaffle(a,"L baffle east",V(-.035f,-.030f),V(.115f,-.030f));
            // The shell and the inside of the L already bound a broad observation court. Extra
            // pocket cheeks here left less than one ball diameter beside the west shell.
            AddLiquid(level,mercury,new Vector3(.18f,.042f,.18f));
            return level;
        }

        private static LevelRuntime LiquidArc(CampaignBuildContext c,bool mercury,int index)
        {
            LevelRuntime level=c.Planar($"C{index:00} liquid arc",Circle(.19f),V(-.135f,0),V(.135f,0),.09f,new[]{Circle(.060f,48,true)});
            AddLiquid(level,mercury,new Vector3(.19f,.042f,.19f));
            return level;
        }

        private static LevelRuntime LiquidChoice(CampaignBuildContext c,bool mercury,int index)
        {
            LevelRuntime level=c.Planar($"C{index:00} liquid route choice",Rectangle(.20f,.18f),V(-.155f,.105f),V(.155f,-.105f));
            var a=Author(level,c);
            a.Block("Choice obstacle north",new Vector3(-.035f,0,.040f),new Vector3(.060f,.084f,.075f),c.Frame);
            a.Block("Choice obstacle south",new Vector3(.065f,0,-.055f),new Vector3(.070f,.084f,.065f),c.Frame);
            AddLiquid(level,mercury,new Vector3(.20f,.042f,.18f));
            return level;
        }

        private static LevelRuntime LiquidLayers(CampaignBuildContext c,bool mercury,int index)
        {
            LevelRuntime level=c.Planar($"C{index:00} liquid two heights",Rectangle(.30f,.25f),V(-.220f,-.175f),V(.220f,.205f),.18f,null,mercury?.055f:.060f);
            var a=Author(level,c);
            TwoDeckTransfer(a,"Liquid",index==66);
            AddLiquid(level,mercury,new Vector3(.30f,.087f,.25f));
            return level;
        }

        private static LevelRuntime LiquidBasin(CampaignBuildContext c,bool mercury,int index)
        {
            LevelRuntime level=c.Planar($"C{index:00} liquid basin",Circle(.19f),V(-.080f,.060f),V(.105f,-.095f));
            var a=Author(level,c);
            a.Block("Low familiar island",new Vector3(0,-.020f,0),new Vector3(.055f,.044f,.055f),c.Frame);
            AddLiquid(level,mercury,new Vector3(.19f,.042f,.19f));
            return level;
        }

        private static LevelRuntime DryRelief(CampaignBuildContext c)
        {
            LevelRuntime level=c.Planar("C81 dry relief calibration",Star(.205f,.165f,6),V(-.105f,.080f),V(.105f,-.085f));
            var a=Author(level,c);
            a.Block("Dry calibration cube",Vector3.zero,new Vector3(.060f,.084f,.060f),c.Frame);
            return level;
        }

        private static LevelRuntime DryTopology(CampaignBuildContext c,int index)
        {
            LevelRuntime level=c.Planar($"C{index:00} dry topology",Rectangle(.30f,.25f),V(-.230f,-.175f),V(.220f,.200f),.22f,null,.056f);
            var a=Author(level,c);
            TwoDeckTransfer(a,index==82?"U faces":"Ring faces",index==82);
            if(index==82)
            {
                FixedBaffle(a,"Upper U left",V(-.245f,-.145f),V(-.245f,.115f),.22f);
                FixedBaffle(a,"Upper U base",V(-.245f,.115f),V(-.065f,.115f),.22f);
            }
            else
            {
                FixedBaffle(a,"Outer route split",V(-.025f,-.205f),V(-.025f,.205f),.22f);
                FixedBaffle(a,"Inner route return",V(.095f,-.125f),V(.095f,.125f),.22f);
            }
            return level;
        }

        private static LevelRuntime DryReliefL(CampaignBuildContext c)
        {
            LevelRuntime level=c.Planar("C84 relief L",Star(.275f,.225f,7),V(-.175f,.135f),V(.130f,-.130f));
            var a=Author(level,c);
            FixedBaffle(a,"Relief L first leg",V(-.065f,.205f),V(-.065f,-.045f));
            FixedBaffle(a,"Relief L second leg",V(-.065f,-.045f),V(.135f,-.045f));
            return level;
        }

        private static LevelRuntime CageStar(CampaignBuildContext c)
        {
            LevelRuntime level=c.ClonePrototype(19,"C89 cage into shallow star");
            var a=Author(level,c);
            // Five short radial cheeks form shallow, reversible star pockets around the cage mouth.
            for(int i=0;i<5;i++)
            {
                // Keep every ray clear of the prototype's authored spawn at (0, .08).
                float angle=(i*72)*Mathf.Deg2Rad;
                Vector2 from=V(Mathf.Cos(angle)*.145f,Mathf.Sin(angle)*.145f-.105f);
                Vector2 to=V(Mathf.Cos(angle)*.260f,Mathf.Sin(angle)*.260f-.105f);
                Wall(a,"Shallow star ray "+i,from,to,.18f,.008f);
            }
            return level;
        }

        private static LevelRuntime MemoryTwoBall(CampaignBuildContext c)
        {
            LevelRuntime level=c.ClonePrototype(22,"C94 one cam two balls");
            AddSecondBall(level,new Vector3(-.275f,-.038f,-.145f));
            var a=Author(level,c);
            MemoryRatchetAssembly memory=level.GetComponent<MemoryRatchetAssembly>();
            memory.MaximumTeeth=1; memory.PassageOpenDegrees=26;
            RemoveContaining(level,"Memory index 1","Memory index 2","Memory index 3");
            RemoveContaining(level,"Fixed cam sleeve");
            Pocket(a,"Ball A rack waiting pocket",V(-.235f,-.205f),V(-.145f,-.120f),.09f,.042f,.045f);
            Pocket(a,"Ball B clear waiting pocket",V(-.235f,-.075f),V(-.145f,-.105f),.09f,.042f,.045f);
            return level;
        }

        private static LevelRuntime BalanceFinale(CampaignBuildContext c)
        {
            LevelRuntime level=c.ClonePrototype(18,"C99 broad two-ball finale");
            Scale(level,"A long arm floor",new Vector3(.300f,.006f,.090f));
            Scale(level,"B short arm floor",new Vector3(.110f,.006f,.090f));
            Scale(level,"A landing",new Vector3(.105f,.008f,.270f));
            Scale(level,"B landing",new Vector3(.225f,.008f,.270f));
            RemoveContaining(level,"clear roof");
            return level;
        }
    }
}
