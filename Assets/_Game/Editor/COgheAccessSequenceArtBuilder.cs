using GravityBox.Venom;
using UnityEngine;

namespace GravityBox.Editor
{
    public static partial class COgheDayLabBuilder
    {
        private static void BuildAccessSequenceArt(VenomCampaign game,Transform art)
        {
            var owner=game.GetComponent<VenomLevelController>();var root=owner.Rotation.transform;
            var sequence=root.GetComponent<COgheLatchedAccessSequence>();
            if(sequence==null)return;
            // The room frame traces its real bounds; the clear front opening
            // and retracted door remain physically and visually unobstructed.
            const float left=-.045f,right=.235f,front=-.115f,back=.175f,top=-.055f,bottom=-.3f;
            foreach(float x in new[]{left,right})foreach(float z in new[]{front,back})
                Box(art,"Inner chamber upright",new Vector3(x,(top+bottom)*.5f,z),new Vector3(.005f,top-bottom,.005f),.001f,alloy);
            foreach(float y in new[]{top,bottom})
            {
                foreach(float z in new[]{front,back})Box(art,"Inner chamber rail",new Vector3((left+right)*.5f,y,z),new Vector3(right-left,.005f,.005f),.001f,ivory);
                foreach(float x in new[]{left,right})Box(art,"Inner chamber rail",new Vector3(x,y,(front+back)*.5f),new Vector3(.005f,.005f,back-front),.001f,ivory);
            }
            var roomGlass=Glass("Access chamber glass",.10f,0,false);
            foreach(var patch in game.Surfaces)
                if(patch.name.StartsWith("Inner room")||patch.name.StartsWith("Door frame"))
                    patch.GetComponent<Renderer>().sharedMaterial=roomGlass;

            void HideOriginal(Transform parent)
            {foreach(var renderer in parent.GetComponentsInChildren<MeshRenderer>())renderer.enabled=false;}
            // Lever art follows its real hinge; a visible foot makes the pull
            // direction comprehensible instead of reading as a loose bar.
            var lever=sequence.Lever.transform;Remove(lever,"Access lever shell");HideOriginal(lever);
            var leverArt=Child(lever,"Access lever shell");
            Box(leverArt,"Metal lever shaft",new Vector3(0,-.010f,0),new Vector3(.014f,.12f,.014f),.003f,alloy);
            Box(leverArt,"Amber lever grip",new Vector3(0,.054f,0),new Vector3(.030f,.039f,.030f),.006f,amber);
            Vector3 pivot=root.InverseTransformPoint(lever.TransformPoint(sequence.LeverJoint.anchor));
            Disk(art,"Lever anchored foot",pivot,Vector3.up,.035f,.010f,ivory);
            Disk(art,"Lever bearing",pivot+Vector3.back*.018f,Vector3.forward,.012f,.010f,graphite);
            Label(leverArt,"A",new Vector3(0,.054f,-.016f),Quaternion.identity,.018f,ink);
            foreach(var old in root.GetComponentsInChildren<Transform>())
                if(old.name=="A"||old.name=="B")
                {var renderer=old.GetComponent<MeshRenderer>();if(renderer!=null)renderer.enabled=false;}

            var door=sequence.RoomDoor.transform;Remove(door,"Access door shell");HideOriginal(door);
            var doorArt=Child(door,"Access door shell");
            Box(doorArt,"Porcelain shutter",Vector3.zero,new Vector3(.115f,.18f,.014f),.004f,ivory);
            Box(doorArt,"Shutter handle band",new Vector3(0,-.036f,-.008f),new Vector3(.072f,.010f,.003f),.001f,alloy);
            Label(doorArt,"A",new Vector3(0,.032f,-.008f),Quaternion.identity,.033f,ink);
            foreach(float x in new[]{.018f,.146f})
                Box(art,"Door lift track",new Vector3(x,-.128f,front-.020f),new Vector3(.008f,.324f,.020f),.002f,alloy);
            Box(art,"Door actuator",new Vector3(.082f,.037f,front-.020f),new Vector3(.15f,.021f,.027f),.004f,ivory);

            var button=sequence.Button;Remove(button.transform,"Moving pressure cap");HideOriginal(button.transform);
            Vector3 pad=button.RestLocal;
            Disk(art,"Button porcelain socket",new Vector3(pad.x,-.295f,pad.z),Vector3.up,.047f,.008f,ivory);
            var capArt=Child(button.transform,"Moving pressure cap");
            Disk(capArt,"Amber B cap",Vector3.up*.006f,Vector3.up,.038f,.006f,amber);
            button.Light=capArt.Find("Amber B cap").GetComponent<Renderer>();
            Label(capArt,"B",Vector3.up*.010f,Quaternion.Euler(90,0,0),.030f,ink);

            var lid=sequence.TubeLid.transform;Remove(lid,"Access inlet cover shell");HideOriginal(lid);
            var lidArt=Child(lid,"Access inlet cover shell");
            Box(lidArt,"Porcelain inlet cover",Vector3.zero,new Vector3(.082f,.082f,.014f),.004f,ivory);
            Box(lidArt,"Slippery cover inset",new Vector3(0,0,-.008f),new Vector3(.069f,.069f,.001f),.001f,Glass("Slide satin coating",.24f,1,false));
            Label(lidArt,"B",new Vector3(0,0,-.010f),Quaternion.identity,.028f,ink);
            foreach(var network in root.GetComponentsInChildren<COgheTubeNetwork>())
            {
                Vector3 inlet=root.TransformPoint(network.Nodes[0].LocalPosition);
                Ring(art,"Tube entry collar",inlet,root.forward,.046f,.004f,alloy);
                Ring(art,"Tube exit collar",owner.Outlet.position,owner.Outlet.forward,.046f,.004f,alloy);
            }
            void Cable(string name,Vector3[] points)
            {
                var line=Child(art,name).gameObject.AddComponent<LineRenderer>();
                line.useWorldSpace=false;line.positionCount=points.Length;line.SetPositions(points);
                line.startWidth=line.endWidth=.0025f;line.numCornerVertices=3;line.sharedMaterial=alloy;
            }
            Cable("A lever to door",new[]{pivot+Vector3.up*.007f,new Vector3(-.19f,-.292f,-.274f),new Vector3(.082f,-.292f,-.274f),new Vector3(.082f,-.292f,front-.020f)});
            Cable("B button to tube cover",new[]{new Vector3(pad.x,-.291f,pad.z+.047f),new Vector3(.202f,-.291f,pad.z+.047f),new Vector3(.202f,-.291f,back-.004f),new Vector3(.202f,-.165f,back-.004f)});
            Renderer Lamp(Transform parent,string name,Vector3 position,Vector3 normal)
            {
                // Keep outside the static mesh batch so the lamp can reflect its latch.
                Disk(parent,name,position,normal,.007f,.002f,amber);
                return parent.Find(name).GetComponent<Renderer>();
            }
            Remove(root,"Access sequence lamps");
            var lights=Child(root,"Access sequence lamps");
            var feedback=owner.gameObject.GetComponent<COgheAccessSequencePresentation>()??owner.gameObject.AddComponent<COgheAccessSequencePresentation>();
            feedback.Sequence=sequence;
            feedback.DoorLamps=new[]{Lamp(lights,"Lever A state",pivot+new Vector3(.023f,.009f,0),Vector3.up),Lamp(lights,"Door A state",new Vector3(.125f,.037f,front-.035f),Vector3.back)};
            feedback.TubeLamps=new[]{Lamp(lights,"Button B state",new Vector3(pad.x+.04f,-.288f,pad.z),Vector3.up),Lamp(lights,"Inlet B state",new Vector3(.19f,-.165f,back-.01f),Vector3.back)};
            CombineByMaterial(leverArt);CombineByMaterial(doorArt);CombineByMaterial(lidArt);
        }
    }
}
