using GravityBox.Venom;
using UnityEngine;

namespace GravityBox.Editor
{
    public static partial class COgheDayLabBuilder
    {
        private static void BuildCooperationArt(VenomCampaign game,Transform art)
        {
            var owner=game.GetComponent<VenomLevelController>();var root=owner.Rotation.transform;
            var winch=root.GetComponentInChildren<COgheCooperativeWinch>();
            var knife=root.GetComponentInChildren<COgheGuillotine>();
            var clear=Glass("Cooperation divider glass",.075f,0,false);
            foreach(var patch in game.Surfaces)
                if(patch.name.Contains("divider")||patch.name.Contains("passage")||patch.name.Contains("door header"))
                    patch.GetComponent<Renderer>().sharedMaterial=clear;
            // A has a fixed socket and a separate depressed cap, driven by actual tissue load.
            var pad=winch.Input;Remove(pad.transform,"Pressure cap art");
            foreach(var r in pad.GetComponentsInChildren<MeshRenderer>())r.enabled=false;
            Disk(art,"A porcelain socket",pad.transform.localPosition+Vector3.up*.002f,Vector3.up,.049f,.006f,ivory);
            var cap=Child(pad.transform,"Pressure cap art");pad.Cap=cap;
            Disk(cap,"A amber pressure plate",Vector3.up*.008f,Vector3.up,.037f,.006f,amber);
            Label(cap,"A",Vector3.up*.012f,Quaternion.Euler(90,0,0),.027f,ink);
            Label(art,"GIU A",new Vector3(-.32f,-.290f,.235f),Quaternion.Euler(90,0,0),.018f,ink);
            // Low bands identify both legitimate passages without hiding the body in the glass divider.
            Box(art,"Divider floor seam",new Vector3(0,-.292f,0),new Vector3(.014f,.004f,.16f),.001f,alloy);
            foreach(float z in new[]{-.268f,-.112f})
                Box(art,"Knife upright",new Vector3(-.03f,-.17f,z),new Vector3(.014f,.25f,.014f),.002f,alloy);
            Box(art,"Knife crosshead",new Vector3(-.03f,-.041f,-.19f),new Vector3(.028f,.026f,.176f),.004f,ivory);
            foreach(float x in new[]{-.054f,-.006f})
                Box(art,"Cut floor warning",new Vector3(x,-.289f,-.19f),new Vector3(.008f,.002f,.12f),.001f,amber);
            Label(art,"CAT",new Vector3(-.105f,-.289f,-.19f),Quaternion.Euler(90,0,0),.018f,ink);
            // B's carriage follows the real rigidbody and remains a physical push/pull interaction.
            var handle=winch.Handle.transform;Remove(handle,"B readable grip");
            foreach(var r in handle.GetComponentsInChildren<MeshRenderer>())r.enabled=false;
            var grip=Child(handle,"B readable grip");
            Box(grip,"B amber carriage",Vector3.zero,new Vector3(.045f,.052f,.045f),.005f,amber);
            Box(grip,"B metal grip",new Vector3(0,.018f,-.025f),new Vector3(.038f,.009f,.009f),.002f,alloy);
            Label(grip,"B",new Vector3(0,-.006f,-.024f),Quaternion.identity,.023f,ink);
            Box(art,"B anchored rail",new Vector3(.312f,-.293f,-.16f),new Vector3(.13f,.008f,.074f),.003f,ivory);
            Label(art,"KEO B",new Vector3(.29f,-.289f,-.235f),Quaternion.Euler(90,0,0),.019f,ink);
            // This arrow is an ordinary floor target beyond the carriage end stop.
            Box(art,"Pull arrow shaft",new Vector3(.415f,-.288f,-.16f),new Vector3(.056f,.003f,.005f),.001f,amber);
            foreach(float sign in new[]{-1f,1f})
            {
                var t=Child(art,"Pull arrow wing");t.localPosition=new Vector3(.436f,-.288f,-.16f+sign*.010f);t.localRotation=Quaternion.Euler(0,sign*45,0);
                Box(t,"Wing",Vector3.zero,new Vector3(.027f,.003f,.005f),.001f,amber);
            }
            for(int i=0;i<winch.Doors.Length;i++)
            {
                var door=winch.Doors[i];Remove(door.transform,"Readable shutter");
                foreach(var r in door.GetComponentsInChildren<MeshRenderer>())if(r.name!="Gate lift cable")r.enabled=false;
                var skin=Child(door.transform,"Readable shutter");
                Vector3 size=i==0?new Vector3(.020f,.15f,.18f):new Vector3(.012f,.12f,.12f);
                Box(skin,"Porcelain sliding gate",Vector3.zero,size,.003f,ivory);
                Box(skin,"Alloy lifting band",new Vector3(-size.x*.5f-.001f,-.03f,0),new Vector3(.003f,.012f,size.z*.72f),.001f,alloy);
                Label(skin,i==0?"1":"2",new Vector3(-size.x*.5f-.003f,.012f,0),Quaternion.Euler(0,90,0),.034f,ink);
                foreach(float z in new[]{door.Start.z-size.z*.5f-.010f,door.Start.z+size.z*.5f+.010f})
                    Box(art,"Gate lift track",new Vector3(door.Start.x,door.Start.y+door.Travel*.5f,z),new Vector3(.025f,size.y+door.Travel+.025f,.010f),.002f,alloy);
                CombineByMaterial(skin);
            }
            Label(art,"GAP LAI",new Vector3(-.125f,-.289f,.17f),Quaternion.Euler(90,0,0),.015f,ink);
            // Paired lamps and a floor conduit expose the A -> B interlock.
            var cable=Child(art,"A to B interlock conduit").gameObject.AddComponent<LineRenderer>();
            cable.useWorldSpace=false;cable.positionCount=6;
            cable.SetPositions(new[]{new Vector3(-.37f,-.287f,.16f),new Vector3(-.43f,-.287f,.16f),new Vector3(-.43f,-.287f,-.278f),new Vector3(.29f,-.287f,-.278f),new Vector3(.29f,-.287f,-.21f),new Vector3(.29f,-.287f,-.197f)});
            cable.startWidth=cable.endWidth=.003f;cable.sharedMaterial=amber;cable.numCornerVertices=3;
            Remove(root,"Cooperation state lamps");
            var lights=Child(root,"Cooperation state lamps");
            Renderer Lamp(string name,Vector3 p)
            {Disk(lights,name,p,Vector3.up,.009f,.003f,amber);return lights.Find(name).GetComponent<Renderer>();}
            var feedback=owner.GetComponent<COgheCooperationPresentation>()??owner.gameObject.AddComponent<COgheCooperationPresentation>();feedback.Winch=winch;
            feedback.UnlockLamps=new[]{Lamp("A load",new Vector3(-.365f,-.283f,.16f)),Lamp("B unlocked",new Vector3(.29f,-.284f,-.199f))};
            feedback.DoorLamps=new[]{Lamp("Gate 1 open",new Vector3(-.052f,-.287f,.24f)),Lamp("Gate 2 open",new Vector3(.444f,-.287f,.24f))};
            pad.Indicator=feedback.UnlockLamps[0];
            CombineByMaterial(grip);
        }
    }
}
