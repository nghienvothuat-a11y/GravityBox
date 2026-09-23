using GravityBox.Venom;
using UnityEngine;

namespace GravityBox.Editor
{
    public static partial class COgheDayLabBuilder
    {
        private static void BuildAssemblyBridgeArt(VenomCampaign game,Transform art)
        {
            var bridge=game.GetComponent<VenomLevelController>().Apparatus.GetComponentInChildren<COgheAssemblyBridge>();
            var clearSide=Glass("Bridge slippery sidewalls",.055f,.3f,false);
            clearSide.SetColor("_BaseColor",new Color(.43f,.37f,.76f,.055f));
            foreach(var patch in game.Surfaces)
            {
                if(patch.name.StartsWith("Climbable start bank")||patch.name.StartsWith("Exit bank"))
                    patch.GetComponent<Renderer>().sharedMaterial=patch.Slippery?clearSide:amber;
                if(patch.GetComponentInParent<VenomMovableProp>()!=null&&patch.Slippery)
                    patch.GetComponent<Renderer>().sharedMaterial=clearSide;
            }
            Remove(bridge.transform,"Bridge indicators");var indicators=Child(bridge.transform,"Bridge indicators");
            var p=bridge.GetComponent<COgheAssemblyBridgePresentation>()??bridge.gameObject.AddComponent<COgheAssemblyBridgePresentation>();p.Bridge=bridge;p.Lamps=new Renderer[bridge.Rails.Length];
            for(int i=0;i<bridge.Rails.Length;i++)
            {
                var rail=bridge.Rails[i];string key=((char)('A'+i)).ToString();float x=rail.Start.x;
                float z=(rail.Start+rail.Axis*rail.Travel).z,halfWidth=.21f/bridge.Rails.Length;
                // A visible end socket matches the real rail stop. The line is
                // a reference for deck alignment, not an extra walkable surface.
                foreach(float side in new[]{-halfWidth+.012f,halfWidth-.012f})
                    Box(art,"Socket "+key,new Vector3(x+side,-.112f,z+.054f),new Vector3(.014f,.017f,.014f),.003f,ivory);
                Label(art,key,new Vector3(x,-.098f,z+.054f),Quaternion.Euler(90,0,0),.018f,ink);
                Box(indicators,"Seat indicator "+key,new Vector3(x,-.10f,z+.054f),new Vector3(.032f,.004f,.007f),.001f,mint);
                p.Lamps[i]=indicators.Find("Seat indicator "+key).GetComponent<Renderer>();
                Remove(rail.transform,"Day Lab bridge trim");var detail=Child(rail.transform,"Day Lab bridge trim");
                for(int line=0;line<4;line++)
                    Box(detail,"Deck grip rib",new Vector3(0,.101f,-.03f+line*.02f),new Vector3(halfWidth*2-.016f,.0018f,.0018f),.0005f,ivory);
                // A slim structural frame reveals the transparent supporting block.
                foreach(float side in new[]{-halfWidth+.006f,halfWidth-.006f})
                    Box(detail,"Block upright",new Vector3(side,0,.037f),new Vector3(.004f,.19f,.004f),.001f,alloy);
                var grip=rail.GetComponent<VenomMovableProp>().ManipulationGrip;
                Label(detail,key,grip.localPosition+new Vector3(0,.005f,-.011f),Quaternion.identity,.014f,ink);
                CombineByMaterial(detail);
            }
        }
    }
}
