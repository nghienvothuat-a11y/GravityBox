using System.Collections.Generic;
using GravityBox.Gameplay;
using GravityBox.Presentation;
using GravityBox.Simulation;
using UnityEditor;
using UnityEngine;

namespace GravityBox.Editor
{
    internal static class BossStaticBuilders
    {
        internal static LevelRuntime Lion(CampaignLevelSpec spec,CampaignBuildContext c)
        {
            // Broad convex mane facets replace the prototype's many narrow pockets.
            var outline=new[]{new Vector2(0,.34f),new Vector2(-.12f,.31f),new Vector2(-.22f,.32f),new Vector2(-.29f,.25f),new Vector2(-.31f,.13f),new Vector2(-.33f,0),new Vector2(-.29f,-.15f),new Vector2(-.19f,-.27f),new Vector2(0,-.33f),new Vector2(.19f,-.27f),new Vector2(.29f,-.15f),new Vector2(.33f,0),new Vector2(.31f,.13f),new Vector2(.29f,.25f),new Vector2(.22f,.32f),new Vector2(.12f,.31f)};
            var l=c.Planar("Awakening lion",outline,new Vector2(0,.25f),new Vector2(0,-.25f));
            var a=BossGeometry.Author(l,c);
            // Full-height muzzle island: either cheek is a legal route.
            var nose=new GameObject("Muzzle island");nose.transform.SetParent(a.Root,false);
            for(int i=0;i<18;i++)
            {
                float t=i*20*Mathf.Deg2Rad;
                var b=a.Block("Rounded muzzle facet "+i,new Vector3(Mathf.Cos(t)*.09f,0,-.055f+Mathf.Sin(t)*.075f),new Vector3(.042f,.086f,.022f),c.Frame,nose.transform);
                b.transform.localRotation=Quaternion.Euler(0,-i*20,0);
            }
            // Filled island prevents a ball from landing in an enclosed hollow after inversion.
            a.Block("Muzzle solid core",new Vector3(0,0,-.055f),new Vector3(.16f,.086f,.13f),c.Frame);
            var brow=BossGeometry.Line(a,"Brow constellation",new[]{new Vector3(-.22f,-.041f,.19f),new Vector3(-.11f,-.041f,.14f),new Vector3(0,-.041f,.21f),new Vector3(.11f,-.041f,.14f),new Vector3(.22f,-.041f,.19f)},c.Rim);
            var whiskerL=BossGeometry.Line(a,"Left whiskers",new[]{new Vector3(-.27f,-.041f,-.02f),new Vector3(-.16f,-.041f,-.11f),new Vector3(-.24f,-.041f,-.18f)},c.Rim);
            var whiskerR=BossGeometry.Line(a,"Right whiskers",new[]{new Vector3(.27f,-.041f,-.02f),new Vector3(.16f,-.041f,-.11f),new Vector3(.24f,-.041f,-.18f)},c.Rim);
            var mouth=BossGeometry.Arc(a,"Lion smile",new Vector3(0,-.041f,-.19f),.11f,.09f,c.Rim,195,150);
            var p=BossGeometry.Present(a,BossGeometry.Region("Forehead",new Vector3(0,0,.19f),new Vector3(.5f,.10f,.2f),brow),
                BossGeometry.Region("Left cheek",new Vector3(-.22f,0,-.05f),new Vector3(.15f,.1f,.32f),whiskerL),
                BossGeometry.Region("Right cheek",new Vector3(.22f,0,-.05f),new Vector3(.15f,.1f,.32f),whiskerR));
            p.Finale=new[]{brow,whiskerL,whiskerR,mouth};return l;
        }

        internal static LevelRuntime Liquid(CampaignLevelSpec spec,CampaignBuildContext c,bool mercury)
        {
            // Exact same retained rectangular fluid domain as the validated liquid prototype.
            var l=c.Planar(mercury?"Silver moon":"Moonlit aquarium",BossGeometry.Rectangle(.16f,.16f),
                new Vector2(-.108f,.106f),new Vector2(.11f,-.112f),.09f,spawnY:mercury?.024f:-.023f);
            var a=BossGeometry.Author(l,c);
            // Axis-aligned stair facets preserve the current wall-resistance model.
            // Every passage is >= 66 mm; both water and Hg expose the same connected courts.
            a.Block("Moon upper shoulder",new Vector3(.005f,0,.063f),new Vector3(.092f,.079f,.015f),c.Glass);
            a.Block("Moon vertical spine",new Vector3(-.034f,0,.018f),new Vector3(.014f,.079f,.105f),c.Glass);
            a.Block("Moon lower crescent",new Vector3(.02f,0,-.061f),new Vector3(.12f,.079f,.014f),c.Glass);
            var water=l.gameObject.AddComponent<WaterVolume>();
            water.Profile=AssetDatabase.LoadAssetAtPath<WaterProfile>(PhysicsLabBuilder.Folder+"/Profiles/Room temperature "+(mercury?"mercury":"water")+".asset");
            if(water.Profile==null)throw new System.InvalidOperationException("Build the physics lab liquid profiles before campaign.");
            water.HalfSize=new Vector3(.16f,.042f,.16f);water.Obstacle=new Bounds(new Vector3(5,5,5),Vector3.zero);
            var visuals=l.gameObject.AddComponent<WaterVisuals>();visuals.MercuryCutaway=mercury;
            visuals.FloorRenderer=l.transform.Find("Floor with circular cut").GetComponent<Renderer>();
            var floor=AssetDatabase.LoadAssetAtPath<Material>(PhysicsLabBuilder.Folder+"/Materials/"+(mercury?"Mercury cutaway floor":"Water illuminated floor")+".mat");
            if(floor!=null)visuals.FloorRenderer.sharedMaterial=floor;
            var volume=a.Block(mercury?"Optical mercury cutaway — visualization only":"Retained water optical volume",Vector3.zero,water.HalfSize*2,c.Glass,null,false);
            visuals.VolumeRenderer=volume.GetComponent<Renderer>();
            var fluid=AssetDatabase.LoadAssetAtPath<Material>(PhysicsLabBuilder.Folder+"/Materials/"+(mercury?"Mercury see-through volume":"Full water optical volume")+".mat");
            if(fluid!=null)visuals.VolumeRenderer.sharedMaterial=fluid;
            visuals.TracerMaterial=AssetDatabase.LoadAssetAtPath<Material>(PhysicsLabBuilder.Folder+"/Materials/Water suspended optical tracers.mat");
            var top=BossGeometry.Arc(a,"Northern moon",new Vector3(.018f,mercury?.039f:-.038f,.06f),.075f,.04f,c.Rim,0,180);
            var lower=BossGeometry.Arc(a,"Southern moon",new Vector3(.012f,mercury?.039f:-.038f,-.055f),.075f,.044f,c.Rim,180,170);
            var p=BossGeometry.Present(a,BossGeometry.Region("First crescent",new Vector3(-.10f,0,0),new Vector3(.10f,.084f,.20f),top),
                BossGeometry.Region("Return crescent",new Vector3(.08f,0,-.10f),new Vector3(.14f,.084f,.10f),lower));p.Finale=new[]{top,lower};
            if(mercury)a.Label("OPTICAL CUTAWAY",new Vector3(.015f,.039f,.132f),.003f);
            return l;
        }

        internal static LevelRuntime Constellation(CampaignLevelSpec spec,CampaignBuildContext c,bool kaleidoscope)
        {
            var l=BossGeometry.Sphere(kaleidoscope?"Kaleidoscope junctions":"Glass constellation",.64f,new Vector3(-.20f,.188f,.12f),c);
            var a=BossGeometry.Author(l,c);
            // Closed, independently suspended first room, with a broad 84 mm mouth.
            var cage=a.Hinge("Observatory suspended cage",new Vector3(-.20f,.22f,.12f),Vector3.forward,.18f,-22,22,.00010f);
            a.Block("Observatory floor",new Vector3(0,-.05f,0),new Vector3(.22f,.006f,.14f),c.Glass,cage.transform);
            a.Block("Observatory roof",new Vector3(0,.05f,0),new Vector3(.22f,.006f,.14f),c.Glass,cage.transform);
            a.Block("Observatory back",new Vector3(-.108f,0,0),new Vector3(.006f,.10f,.14f),c.Glass,cage.transform);
            foreach(int side in new[]{-1,1})a.Block("Observatory side "+side,new Vector3(0,0,side*.068f),new Vector3(.22f,.10f,.006f),c.Glass,cage.transform);
            a.Block("Observatory keel",new Vector3(-.01f,-.06f,0),new Vector3(.09f,.014f,.07f),c.Frame,cage.transform);
            cage.Body.centerOfMass=new Vector3(0,-.043f,0);
            // Stationary receiver sleeves overlap the swept mouth, containing any spill.
            a.Block("Cage spill catch",new Vector3(-.15f,.105f,.12f),new Vector3(.35f,.006f,.28f),c.Glass);
            foreach(int side in new[]{-1,1})a.Block("Cage containment side "+side,new Vector3(-.15f,.235f,.12f+side*.14f),new Vector3(.35f,.266f,.006f),c.Glass);
            a.Block("Cage containment rear",new Vector3(-.324f,.235f,.12f),new Vector3(.006f,.266f,.28f),c.Glass);
            a.Block("Cage containment roof",new Vector3(-.15f,.367f,.12f),new Vector3(.35f,.006f,.28f),c.Glass);
            // Right wall has only the 116 mm transfer window aligned with first node.
            a.Block("Observatory outlet high jamb",new Vector3(-.058f,.305f,.12f),new Vector3(.006f,.124f,.28f),c.Glass);
            foreach(int side in new[]{-1,1})a.Block("Observatory outlet side jamb "+side,new Vector3(-.058f,.174f,.12f+side*.10f),new Vector3(.006f,.136f,.08f),c.Glass);
            var nodes=new List<Vector3Int>{new Vector3Int(0,0,0),new Vector3Int(1,0,0),new Vector3Int(1,0,1),new Vector3Int(1,-1,1),new Vector3Int(2,-1,1),new Vector3Int(2,-1,0),new Vector3Int(2,-2,0),new Vector3Int(1,-2,0),new Vector3Int(1,-2,-1),new Vector3Int(0,-2,-1),new Vector3Int(0,-3,-1),new Vector3Int(0,-4,-1),new Vector3Int(0,-5,-1),new Vector3Int(0,-6,-1)};
            if(kaleidoscope)
            {
                // Two short true loops return to the same junction; there are no fake reflected paths.
                nodes.AddRange(new[]{new Vector3Int(0,0,1),new Vector3Int(0,-1,1),new Vector3Int(-1,-2,-1),new Vector3Int(-1,-3,-1)});
            }
            BossGeometry.Rooms(a,nodes,.12f,new Vector3(0,.18f,.12f),nodes[0],new Vector3Int(0,-6,-1),Vector3Int.left,Vector3Int.down);
            var n1=BossGeometry.Arc(a,"First orbital landmark",new Vector3(0,.245f,.12f),.08f,.08f,c.Rim);
            var n2=BossGeometry.Line(a,"Diamond landmark",new[]{new Vector3(.24f,.125f,.14f),new Vector3(.30f,.125f,.24f),new Vector3(.24f,.125f,.32f),new Vector3(.18f,.125f,.24f),new Vector3(.24f,.125f,.14f)},c.Rim);
            var n3=BossGeometry.Line(a,"Final fork landmark",new[]{new Vector3(-.055f,-.115f,-.04f),new Vector3(0,-.115f,0),new Vector3(.055f,-.115f,-.04f)},c.Rim);
            var p=BossGeometry.Present(a,BossGeometry.Region("Observatory",new Vector3(.04f,.18f,.12f),new Vector3(.2f,.12f,.12f),n1),BossGeometry.Region("Diamond court",new Vector3(.24f,.06f,.18f),new Vector3(.12f,.12f,.24f),n2),BossGeometry.Region("Polar return",new Vector3(0,-.12f,0),new Vector3(.13f,.24f,.13f),n3));p.Finale=new[]{n1,n2,n3};return l;
        }
    }
}
