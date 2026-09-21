using System;
using System.Collections.Generic;
using System.IO;
using GravityBox.Venom;
using UnityEditor;
using UnityEngine;

namespace GravityBox.Editor
{
    public static partial class VenomCampaignBuilder
    {
        public static string[] Campaign40ScenePaths()
        {
            var paths=new string[40];
            for(int i=0;i<paths.Length;i++)paths[i]=$"{Campaign30Folder}/{Campaign30ScenePrefix}{i+1:00}.unity";
            return paths;
        }

        [MenuItem("Gravity Box/COghe/Generate Approved Chapter 31–40")]
        public static void GenerateCampaign40()
        {
            foreach(string path in Campaign30ScenePaths())
                if(!File.Exists(path))throw new FileNotFoundException("Existing campaign must be preserved",path);
            PrepareCampaign30Assets();
            for(int n=31;n<=40;n++)BuildCampaign30Content(n,n);
            var scenes=new List<EditorBuildSettingsScene>();
            foreach(string path in Campaign40ScenePaths())scenes.Add(new EditorBuildSettingsScene(path,true));
            foreach(var scene in EditorBuildSettings.scenes)
                if(!scenes.Exists(s=>s.path==scene.path))scenes.Add(scene);
            EditorBuildSettings.scenes=scenes.ToArray();
            AssetDatabase.SaveAssets();
            Debug.Log("COGHE CHAPTER 31–40 GENERATED; earlier scenes preserved.");
        }

        private static void ChapterSolid(Transform parent,string name,Vector3 centre,Vector3 size)
        {
            var part=MechanismVisual(parent,name,centre,size,metal);
            var collider=part.gameObject.AddComponent<BoxCollider>();collider.sharedMaterial=contact;collider.contactOffset=.0005f;
        }

        private static void ApplyChapterWorkingSurfaces(ExpansionContext c)
        {
            var porcelain=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Venom/Art/DayLab/Warm porcelain.mat");
            var amber=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Game/Venom/Art/DayLab/Amber resin.mat");
            foreach(var face in c.Surfaces)
            {
                if(face.GetComponentInParent<VenomMovableProp>()!=null)continue;
                string label=face.name.ToLowerInvariant();
                if(label.Contains("bank")||label.Contains("pedestal"))face.GetComponent<Renderer>().sharedMaterial=porcelain;
                if(label.Contains("docked deck")||label=="b restored deck")face.GetComponent<Renderer>().sharedMaterial=amber;
            }
        }

        private static COgheRailSlider ChapterExit(ExpansionContext c)
        {
            var gate=ExpansionRail(c,"E final shutter",c.Exit+Vector3.left*.012f+Vector3.down*.025f,
                Vector3.up,.18f,0,new Vector3(.016f,.15f,.15f),.04f,.014f,false,false);
            gate.LatchAtEnd=true;AddRackTeeth(gate);return gate;
        }

        private static COgheGearTrain ChapterGear(ExpansionContext c,COgheRailSlider carriage,COgheRailSlider gate,Vector3 localWheel)
        {
            var train=new GameObject("G measured transmission",typeof(COgheGearTrain)).GetComponent<COgheGearTrain>();
            train.transform.SetParent(c.Root,false);
            Vector3 end=carriage.Start+carriage.Axis*carriage.Travel+localWheel;
            train.Wheels=new[]{MeshingStationWheel(c.Root,"Motor",end+Vector3.up*.08f,.04f,18,0),
                MeshingStationWheel(carriage.transform,"G wheel",localWheel,.04f,18,10),
                MeshingStationWheel(c.Root,"Output",end+Vector3.down*.08f,.04f,18,0)};
            train.PitchRadii=new[]{.04f,.04f,.04f};train.ToothCounts=new[]{18,18,18};
            train.Rack=gate;train.MeshTolerance=.004f;return train;
        }

        private static void BuildCampaign31(ExpansionContext c)
        {
            ConfigureNew(c,"Lùi để tiến","Quan sát chốt và khoảng trống trước khi đưa bánh G vào bộ truyền.",false,new Vector3(32,24,0),.55f);
            c.Spawn=new Vector3(-.34f,-.265f,-.19f);c.Exit=new Vector3(.42f,-.18f,.17f);c.Outward=Vector3.right;
            ExpansionShell(c,.42f);
            var g=ExpansionRail(c,"G access carriage",new Vector3(-.30f,-.213f,-.08f),Vector3.right,.50f,.20f,
                new Vector3(.19f,.154f,.018f),.09f,.035f,false,true);
            g.LatchAtEnd=true;
            g.GetComponent<VenomMovableProp>().ManipulationGrip.localPosition+=Vector3.down*.042f;
            var p=ExpansionRail(c,"P retracting pin",new Vector3(-.095f,-.268f,.025f),Vector3.back,.18f,0,
                new Vector3(.045f,.040f,.035f),.06f,.030f,false,true);p.LatchAtEnd=true;
            ChapterSolid(p.transform,"P real rail stop",new Vector3(.17f,.055f,-.105f),new Vector3(.025f,.080f,.028f));
            MechanismVisual(p.transform,"P connecting bar",new Vector3(.085f,.035f,-.105f),new Vector3(.17f,.012f,.012f),metal);
            Panel(c.Root,"P alcove back",new Vector3(-.095f,-.225f,.09f),Vector3.back,new Vector2(.17f,.15f),stone,false,Vector2.zero,0,c.Surfaces);
            foreach(float side in new[]{-1f,1f})
                Panel(c.Root,"P alcove side",new Vector3(-.095f+side*.085f,-.225f,.02f),Vector3.left*side,new Vector2(.14f,.15f),stone,false,Vector2.zero,0,c.Surfaces);
            Panel(c.Root,"P alcove roof",new Vector3(-.095f,-.15f,.02f),Vector3.down,new Vector2(.17f,.14f),stone,false,Vector2.zero,0,c.Surfaces).InterceptExterior=true;
            ChapterGear(c,g,ChapterExit(c),new Vector3(0,.10f,.14f));
        }

        private static void ChapterWideShell(ExpansionContext c,float halfX,float halfZ)
        {
            ExpansionShell(c,halfX);
            foreach(var face in c.Surfaces)
            {
                Vector3 normal=face.Normal;
                if(Mathf.Abs(normal.z)>.9f)face.transform.localPosition=-normal*halfZ;
                if(Mathf.Abs(normal.y)>.9f)face.Size=new Vector2(halfX*2,halfZ*2);
                if(Mathf.Abs(normal.x)>.9f)face.Size=new Vector2(halfZ*2,.60f);
                var mesh=Save(PanelMesh(face.Size,face.Hole,face.HoleCentre,face.HoleRadius));
                ((MeshCollider)face.Shape).sharedMesh=mesh;
                face.GetComponent<MeshFilter>().sharedMesh=Save(PanelMesh(face.Size,face.Hole,face.HoleCentre,face.HoleRadius,false));
            }
        }

        private static void ChapterBridgeGrip(ExpansionContext c,COgheRailSlider bridge,float x,float z=0)
        {
            var prop=bridge.GetComponent<VenomMovableProp>();var grip=prop.ManipulationGrip;
            grip.localPosition=new Vector3(x,.035f,z);grip.localRotation=Quaternion.LookRotation(Vector3.left,Vector3.up);
            grip.localScale=new Vector3(.054f,.022f,.024f);grip.gameObject.AddComponent<BoxCollider>();
            prop.ManipulationHandleOnly=true;
            MechanismVisual(bridge.transform,"Bridge handle connecting arm",new Vector3(x*.5f,.022f,z),new Vector3(Mathf.Abs(x),.008f,.008f),metal);
            var aim=new GameObject("Bridge command plane").transform;aim.SetParent(c.Root,false);aim.localPosition=new Vector3(0,-.16f,0);prop.ManipulationPlane=aim;
        }

        private static void BuildCampaign32(ExpansionContext c)
        {
            ConfigureNew(c,"Một cầu, hai bến","Một cây cầu có thể đưa bạn tới hai bến.",false,new Vector3(24,12,0),.66f);
            c.Owner.ApertureRadius=.05f;c.Spawn=new Vector3(-.30f,-.11f,.16f);c.Exit=new Vector3(.46f,-.06f,.16f);c.Outward=Vector3.right;
            ChapterWideShell(c,.46f,.62f);
            RaisedBank(c,"S central bank",new Vector3(-.31f,-.23f,0),new Vector3(.30f,.14f,.98f));
            foreach(float z in new[]{-.16f,.16f})
            {
                RaisedBank(c,z<0?"A bank":"B bank",new Vector3(.31f,-.23f,z),new Vector3(.30f,.14f,.24f));
                var face=Panel(c.Root,z<0?"A portal":"B portal",new Vector3(.15f,0,z<0?-.31f:.31f),Vector3.left,
                    new Vector2(.62f,.60f),glass,true,new Vector2(z-(z<0?-.31f:.31f),-.08f),.087f,c.Surfaces);
            }
            Panel(c.Root,"A B sealed partition",new Vector3(.305f,0,0),Vector3.back,new Vector2(.31f,.60f),glass,false,Vector2.zero,0,c.Surfaces);
            var bridge=ExpansionRail(c,"B reusable bridge",new Vector3(-.003f,-.173f,.16f),Vector3.back,.32f,0,
                new Vector3(.30f,.026f,.15f),.12f,.035f,false,true);bridge.LatchAtEnd=bridge.LatchAtStart=true;
            ChapterBridgeGrip(c,bridge,-.195f,.11f);
            AddMovingAperturePlate(c,bridge,new Vector3(.132f,.103f,0),.42f,.86f,.145f,.20f);
            var deck=bridge.gameObject.AddComponent<COgheTwoDockDeck>();deck.Rail=bridge;
            foreach(var face in bridge.GetComponentsInChildren<VenomSurfacePatch>())if(Vector3.Dot(face.Normal,c.Root.up)>.9f)deck.MovingTop=face;
            deck.StartTop=Panel(c.Root,"B docked deck",new Vector3(-.003f,-.16f,.16f),Vector3.up,new Vector2(.30f,.15f),stone,false,Vector2.zero,0,c.Surfaces);
            deck.EndTop=Panel(c.Root,"A docked deck",new Vector3(-.003f,-.16f,-.16f),Vector3.up,new Vector2(.30f,.15f),stone,false,Vector2.zero,0,c.Surfaces);
            deck.StartTop.gameObject.SetActive(false);deck.EndTop.gameObject.SetActive(false);
            var lever=ExpansionRail(c,"L remote winch",new Vector3(.30f,-.12f,-.16f),Vector3.right,.065f,0,new Vector3(.045f,.05f,.045f),.06f,.03f,false,true);
            var motor=new GameObject("A cable to B",typeof(COgheSequentialWinch)).GetComponent<COgheSequentialWinch>();motor.transform.SetParent(c.Root,false);
            motor.Handle=lever;motor.Door=ChapterExit(c);motor.DoorSpeed=.09f;motor.HandleForce=.04f;
            AddWinchVisuals(c,motor,new Vector3(.33f,.13f,-.15f));
        }

        private static void BuildCampaign33(ExpansionContext c)
        {
            ConfigureNew(c,"Đổi đường ống","Quan sát tuyến đang thông và cửa ở hai khoang.",false,new Vector3(26,22,0),.61f);
            c.Owner.ApertureRadius=.05f;c.Spawn=new Vector3(-.30f,-.265f,-.20f);c.Exit=new Vector3(.50f,.115f,.14f);c.Outward=Vector3.right;
            ChapterWideShell(c,.50f,.40f);
            c.Definition.CameraZones=new[]{new VenomCameraZone("S",new Vector3(0,-.14f,0),new Vector3(1,.32f,.60f)),
                new VenomCameraZone("A",new Vector3(-.25f,.16f,0),new Vector3(.50f,.30f,.60f)),
                new VenomCameraZone("B",new Vector3(.25f,.16f,0),new Vector3(.50f,.30f,.60f))};
            foreach(float side in new[]{-1f,1f})
            {
                Vector3 centre=new Vector3(side*.25f,.025f,0);
                var rotation=Quaternion.LookRotation(Vector3.up,Vector3.forward);
                Vector3 hole=Quaternion.Inverse(rotation)*(new Vector3(side*.10f,.025f,-.12f)-centre);
                Panel(c.Root,side<0?"A perforated floor":"B perforated floor",centre,Vector3.up,new Vector2(.50f,.80f),glass,true,new Vector2(hole.x,hole.y),.048f,c.Surfaces);
                // Back faces seal surface navigation in both directions.
                var down=Quaternion.LookRotation(Vector3.down,Vector3.forward);
                hole=Quaternion.Inverse(down)*(new Vector3(side*.10f,.025f,-.12f)-centre);
                Panel(c.Root,"S perforated roof",centre,Vector3.down,new Vector2(.50f,.80f),glass,true,new Vector2(hole.x,hole.y),.048f,c.Surfaces);
            }
            foreach(float side in new[]{-1f,1f})Panel(c.Root,"A B upper partition",new Vector3(0,.163f,0),Vector3.right*side,new Vector2(.80f,.275f),glass,false,Vector2.zero,0,c.Surfaces);
            var selector=ExpansionRail(c,"T route selector",new Vector3(-.10f,-.254f,-.23f),Vector3.right,.20f,.20f,new Vector3(.045f,.045f,.045f),.08f,.035f,false,true);
            selector.LatchAtStart=selector.LatchAtEnd=true;
            foreach(float side in new[]{-1f,1f})
            {
                // Two solid shutters are on the same captive carriage. At either
                // stop one bore is clear and the opposite bore is physically closed.
                var cap=MechanismVisual(selector.transform,side<0?"A selector shutter":"B selector shutter",new Vector3(side*.20f,.249f,.11f),new Vector3(.125f,.014f,.125f),plastic);
                var shape=cap.gameObject.AddComponent<BoxCollider>();shape.sharedMaterial=contact;shape.contactOffset=.0005f;
                MechanismVisual(selector.transform,"T shutter linkage",new Vector3(side*.10f,.249f,.11f),new Vector3(.20f,.012f,.014f),metal);
                float x=side*.10f;
                var nodes=new[]{new COgheTubeNetwork.Node("S",new Vector3(x,-.050f,-.12f),COgheTubeNetwork.TerminalKind.Entry,Vector3.down),
                    new COgheTubeNetwork.Node(side<0?"A":"B",new Vector3(side*.30f,.13f,-.12f),COgheTubeNetwork.TerminalKind.Entry,Vector3.right*side)};
                var route=new[]{nodes[0].LocalPosition,new Vector3(x,-.025f,-.12f),new Vector3(x,.025f,-.12f),new Vector3(x,.065f,-.12f),
                    new Vector3(side*.165f,.13f,-.12f),nodes[1].LocalPosition};
                var net=TubeNetwork(c.Root,side<0?"A bidirectional pipe":"B bidirectional pipe",nodes,new[]{new COgheTubeNetwork.Edge("Transfer",0,1,route)},.044f,glass);
                // A sliding valve requires a transverse sleeve slot. Keep the
                // fixed transport centreline, but leave 16 mm between sleeves
                // for the 14 mm shutter (smaller than an 18 mm tissue particle).
                var lower=new Mesh();var upper=new Mesh();
                var lowerPath=new List<Vector3>();var upperPath=new List<Vector3>();
                var sampled=COgheTubeNetwork.SampleCurve(route,6);
                for(int i=0;i<sampled.Length;i++)
                {
                    Vector3 point=sampled[i];
                    if(point.y<=-.013f)lowerPath.Add(point);
                    if(i>0&&sampled[i-1].y<-.013f&&point.y>-.013f)
                        lowerPath.Add(Vector3.Lerp(sampled[i-1],point,(-.013f-sampled[i-1].y)/(point.y-sampled[i-1].y)));
                    if(i>0&&sampled[i-1].y<.003f&&point.y>.003f)
                        upperPath.Add(Vector3.Lerp(sampled[i-1],point,(.003f-sampled[i-1].y)/(point.y-sampled[i-1].y)));
                    if(point.y>=.003f)upperPath.Add(point);
                }
                COgheTubeNetwork.BuildSweptMesh(lowerPath.ToArray(),.044f,false,lower);
                COgheTubeNetwork.BuildSweptMesh(upperPath.ToArray(),.044f,false,upper);
                var sleeve=new Mesh{name="Slotted valve sleeve"};
                sleeve.CombineMeshes(new[]{new CombineInstance{mesh=lower,transform=Matrix4x4.identity},
                    new CombineInstance{mesh=upper,transform=Matrix4x4.identity}},true,true);
                sleeve=Save(sleeve);net.Edges[0].Filter.sharedMesh=sleeve;net.Edges[0].Collider.sharedMesh=sleeve;
                UnityEngine.Object.DestroyImmediate(lower);UnityEngine.Object.DestroyImmediate(upper);
                net.EntryBlocker=shape;
                // Leave the full receiving lead plus a particle radius below
                // the downward mouth, so the trailing tissue can clear it.
                RaisedBank(c,"S mouth pedestal",new Vector3(x,-.2125f,-.12f),new Vector3(.11f,.175f,.15f));
                // The horizontal mouth delivers onto a bank away from the
                // floor bore, so releasing flow cannot drop tissue back to S.
                RaisedBank(c,"Upper pipe landing bank",new Vector3(side*.40f,.0625f,-.12f),new Vector3(.18f,.075f,.19f));
            }
            var lever=ExpansionRail(c,"L remote winch",new Vector3(-.33f,.066f,.13f),Vector3.right,.065f,0,new Vector3(.045f,.05f,.045f),.06f,.03f,false,true);
            var motor=new GameObject("A cable to E",typeof(COgheSequentialWinch)).GetComponent<COgheSequentialWinch>();motor.transform.SetParent(c.Root,false);
            // This upper room needs a side pocket: an upward shutter would
            // start inside the floor and exhaust its clearance at the ceiling.
            var exit=ExpansionRail(c,"E final shutter",c.Exit+Vector3.left*.012f,Vector3.forward,.18f,0,
                new Vector3(.016f,.14f,.14f),.04f,.014f,false,false);exit.LatchAtEnd=true;
            MechanismVisual(exit.transform,"E side rack",new Vector3(-.022f,0,0),new Vector3(.009f,.012f,.12f),metal);
            for(int i=0;i<8;i++)MechanismVisual(exit.transform,"E side rack tooth",new Vector3(-.028f,0,-.049f+i*.014f),new Vector3(.014f,.014f,.007f),metal);
            motor.Handle=lever;motor.Door=exit;motor.DoorSpeed=.09f;motor.HandleForce=.04f;
            AddWinchVisuals(c,motor,new Vector3(-.30f,.23f,.18f),.16f);
        }

        private static void BuildCampaign34(ExpansionContext c)
        {
            ConfigureNew(c,"Nhường đúng chỗ","Quan sát hốc đỗ và giao điểm giữa các ray.",false,new Vector3(24,12,0),.60f);
            c.Owner.ApertureRadius=.05f;c.Spawn=new Vector3(-.33f,-.11f,-.16f);c.Exit=new Vector3(.46f,-.06f,.14f);c.Outward=Vector3.right;
            ChapterWideShell(c,.46f,.56f);
            // A 7 mm lateral gap keeps the bank's corner clear of the moving
            // deck; an 18 mm particle still spans the seam in either direction.
            RaisedBank(c,"Working bank",new Vector3(-.2385f,-.23f,0),new Vector3(.443f,.14f,.56f));
            RaisedBank(c,"Exit bank",new Vector3(.39f,-.23f,.14f),new Vector3(.14f,.14f,.23f));
            Panel(c.Root,"Exit island partition",new Vector3(.315f,0,0),Vector3.left,new Vector2(1.12f,.60f),glass,true,new Vector2(.14f,-.08f),.090f,c.Surfaces);
            var bridge=ExpansionRail(c,"B bay bridge",new Vector3(.15f,-.173f,-.17f),Vector3.forward,.26f,.26f,new Vector3(.32f,.026f,.24f),.12f,.04f,false,true);
            bridge.LatchAtEnd=true;bridge.LatchAtStart=true;ChapterBridgeGrip(c,bridge,-.35f);
            // A front-facing grip leaves its operator in the front aisle after
            // release, so the solid handle is not across the route to the deck.
            bridge.GetComponent<VenomMovableProp>().ManipulationGrip.localRotation=Quaternion.LookRotation(Vector3.back,Vector3.up);
            AddMovingAperturePlate(c,bridge,new Vector3(.141f,.103f,.05f),.42f,.76f,.15f,.20f);
            ChapterSolid(bridge.transform,"Bridge parking cheek",new Vector3(.02f,.073f,-.085f),new Vector3(.14f,.12f,.055f));
            var x=ExpansionRail(c,"X crossing block",new Vector3(-.10f,-.095f,.005f),Vector3.right,.27f,0,new Vector3(.12f,.12f,.095f),.14f,.04f,false,true);
            x.LatchAtEnd=x.LatchAtStart=true;
            var xProp=x.GetComponent<VenomMovableProp>();var xGrip=xProp.ManipulationGrip;
            xGrip.localPosition=new Vector3(-.20f,-.043f,-.09f);xGrip.localRotation=Quaternion.LookRotation(Vector3.left,Vector3.up);
            xGrip.localScale=new Vector3(.052f,.018f,.022f);xGrip.gameObject.AddComponent<BoxCollider>();xProp.ManipulationHandleOnly=true;
            MechanismVisual(x.transform,"X bank-side handle arm",new Vector3(-.10f,-.043f,-.09f),new Vector3(.20f,.008f,.010f),metal);
            var g=ExpansionRail(c,"G crossing carriage",new Vector3(-.10f,-.115f,-.18f),Vector3.forward,.40f,0,new Vector3(.065f,.07f,.07f),.07f,.035f,false,true);
            g.LatchAtEnd=true;
            var train=ChapterGear(c,g,ChapterExit(c),new Vector3(0,.10f,.0f));
            // The actual deck spans the island gap; the aperture plate never disappears.
            var deck=bridge.gameObject.AddComponent<COgheDockedBridgeDeck>();deck.Rail=bridge;
            foreach(var face in bridge.GetComponentsInChildren<VenomSurfacePatch>())if(Vector3.Dot(face.Normal,c.Root.up)>.9f)deck.MovingSurfaces=new[]{face};
            deck.DockedTop=Panel(c.Root,"B restored deck",new Vector3(.15f,-.16f,.09f),Vector3.up,new Vector2(.32f,.24f),stone,false,Vector2.zero,0,c.Surfaces);deck.DockedTop.gameObject.SetActive(false);
        }
    }
}
