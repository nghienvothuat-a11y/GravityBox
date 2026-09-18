using GravityBox.Venom;
using UnityEngine;
using UnityEngine.Rendering;

namespace GravityBox.Editor
{
    public static partial class COgheDayLabBuilder
    {
        private static void BuildBossReadabilityArt(VenomCampaign game,Transform art)
        {
            var owner=game.Owner!=null?game.Owner:game.GetComponent<VenomLevelController>();
            var root=owner.Rotation.transform;
            var winch=root.GetComponentInChildren<COgheCooperativeWinch>();
            var clear=Glass("Cooperation divider glass",.075f,0,false);
            foreach(var patch in game.Surfaces)
            {
                if(patch.name.Contains("divider")||patch.name.Contains("door header")||patch.name.Contains("transfer aperture"))
                    patch.GetComponent<Renderer>().sharedMaterial=clear;
                if(patch.name=="High mouth approach shelf")patch.GetComponent<Renderer>().sharedMaterial=amber;
            }
            foreach(var pad in new[]{winch.Input,winch.Output})
            {
                string key=pad.name.Substring(0,1);
                foreach(var renderer in pad.GetComponentsInChildren<Renderer>())renderer.enabled=false;
                Disk(art,key+" porcelain socket",pad.transform.localPosition+Vector3.up*.001f,Vector3.up,.063f,.005f,ivory);
                var cap=Child(pad.transform,"Readable pressure cap");pad.Cap=cap;
                Disk(cap,key+" amber cap",Vector3.up*.008f,Vector3.up,.052f,.005f,amber);
                Label(cap,key,Vector3.up*.012f,Quaternion.Euler(90,0,0),.037f,ink);
                CombineByMaterial(cap);
            }
            // The two alternate passages read differently: cyan circular bores
            // above amber climbing steps; porcelain sliding doors at the rear.
            foreach(var pipe in root.GetComponentsInChildren<COgheTubeNetwork>())
                foreach(var node in pipe.Nodes)
                {
                    Ring(art,"Transfer collar",node.LocalPosition,node.LocalOutward,pipe.Radius+.004f,.006f,ivory);
                    Ring(art,"Transfer gasket",node.LocalPosition+node.LocalOutward*.002f,node.LocalOutward,pipe.Radius+.001f,.0015f,mint);
                }
            int doorIndex=0;
            foreach(var door in winch.Doors)
            {
                doorIndex++;
                var skin=Child(door.transform,"Boss gate casing");
                foreach(var renderer in door.GetComponentsInChildren<MeshRenderer>())
                    if(renderer.name!="Gate lift cable")renderer.enabled=false;
                Box(skin,"Porcelain reunion gate",Vector3.zero,new Vector3(.020f,.15f,.18f),.003f,ivory);
                foreach(float side in new[]{-1f,1f})
                    Label(skin,doorIndex.ToString(),new Vector3(side*.012f,.010f,0),Quaternion.Euler(0,-side*90,0),.036f,ink);
                foreach(float z in new[]{.07f,.27f})
                    Box(art,"Door guide",new Vector3(door.Start.x,-.14f,z),new Vector3(.026f,.33f,.008f),.002f,alloy);
                CombineByMaterial(skin);
            }
            foreach(var knife in root.GetComponentsInChildren<COgheGuillotine>())
            {
                Vector3 p=knife.Sensor.localPosition;
                foreach(float z in new[]{p.z-.080f,p.z+.080f})
                    Box(art,"Cutting station upright",new Vector3(p.x,-.17f,z),new Vector3(.012f,.25f,.012f),.002f,alloy);
                Box(art,"Cutting station head",new Vector3(p.x,-.037f,p.z),new Vector3(.029f,.018f,.182f),.003f,ivory);
                foreach(float x in new[]{p.x-.023f,p.x+.023f})
                    Box(art,"Blade floor boundary",new Vector3(x,-.289f,p.z),new Vector3(.005f,.002f,.13f),.001f,amber);
            }
            // Functional conduits expose A -> gear clutch and B -> winch clutch.
            // They are visuals only and do not collide or enter navigation.
            void Conduit(string name,Vector3[] points)
            {
                var line=Child(art,name).gameObject.AddComponent<LineRenderer>();
                line.useWorldSpace=false;line.positionCount=points.Length;line.SetPositions(points);
                line.startWidth=line.endWidth=.003f;line.sharedMaterial=alloy;line.numCornerVertices=2;
                line.shadowCastingMode=ShadowCastingMode.Off;
            }
            Conduit("A clutch conduit",new[]{new Vector3(-.46f,-.289f,.23f),new Vector3(-.46f,-.289f,.278f),new Vector3(-.10f,-.289f,.278f),new Vector3(-.10f,-.289f,.12f),new Vector3(-.10f,-.05f,.12f)});
            Conduit("B clutch conduit",new[]{new Vector3(.12f,-.289f,.23f),new Vector3(.12f,-.289f,.278f),new Vector3(.45f,-.289f,.278f),new Vector3(.45f,-.289f,-.16f)});
            foreach(float x in new[]{-.4f,0,.4f})
                Label(art,x<0?"I":x>0?"III":"II",new Vector3(x,-.288f,-.273f),Quaternion.Euler(90,0,0),.028f,ink);

            // Keep stateful renderers outside the static mesh batch.
            Remove(root,"Boss equipment indicators");var indicators=Child(root,"Boss equipment indicators");
            var feedback=owner.GetComponent<COgheCooperativeBossPresentation>()??owner.gameObject.AddComponent<COgheCooperativeBossPresentation>();
            feedback.Winch=winch;feedback.Lamps=new Renderer[5];
            Vector3[] points={new Vector3(-.39f,-.285f,.16f),new Vector3(-.055f,-.040f,.075f),new Vector3(.12f,-.285f,.242f),new Vector3(.435f,-.283f,-.16f),new Vector3(.576f,-.19f,.25f)};
            for(int i=0;i<points.Length;i++)
            {
                Disk(indicators,"Status "+i,points[i],i==4?Vector3.left:i==1?Vector3.back:Vector3.up,.010f,.003f,alloy);
                feedback.Lamps[i]=indicators.Find("Status "+i).GetComponent<Renderer>();
            }
            winch.Input.Indicator=feedback.Lamps[0];winch.Output.Indicator=feedback.Lamps[2];
            TextMesh State(string name,Vector3 point)
            {
                Label(indicators,name,point,Quaternion.Euler(90,0,0),.013f,ink);
                return indicators.Find("Label · "+name).GetComponent<TextMesh>();
            }
            feedback.GearState=State("G KHOA",new Vector3(-.075f,-.288f,.025f));
            feedback.WinchState=State("C KHOA",new Vector3(.39f,-.288f,-.24f));
            feedback.CoverState=State("H KHOA",new Vector3(.52f,-.288f,.24f));
        }
    }
}
