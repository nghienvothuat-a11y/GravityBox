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
            var p=bridge.GetComponent<COgheAssemblyBridgePresentation>()??bridge.gameObject.AddComponent<COgheAssemblyBridgePresentation>();p.Bridge=bridge;p.Lamps=new Renderer[3];
            for(int i=0;i<3;i++)
            {
                var rail=bridge.Rails[i];string key=((char)('A'+i)).ToString();float x=rail.Start.x;
                // A visible end socket matches the real rail stop. The line is
                // a reference for deck alignment, not an extra walkable surface.
                foreach(float side in new[]{-.058f,.058f})
                    Box(art,"Socket "+key,new Vector3(x+side,-.112f,.054f),new Vector3(.014f,.017f,.014f),.003f,ivory);
                Label(art,key,new Vector3(x,-.098f,.054f),Quaternion.Euler(90,0,0),.018f,ink);
                Box(indicators,"Seat indicator "+key,new Vector3(x,-.10f,.054f),new Vector3(.032f,.004f,.007f),.001f,mint);
                p.Lamps[i]=indicators.Find("Seat indicator "+key).GetComponent<Renderer>();
                Remove(rail.transform,"Day Lab bridge trim");var detail=Child(rail.transform,"Day Lab bridge trim");
                for(int line=0;line<4;line++)
                    Box(detail,"Deck grip rib",new Vector3(0,.101f,-.03f+line*.02f),new Vector3(.124f,.0018f,.0018f),.0005f,ivory);
                // A slim structural frame reveals the transparent supporting block.
                foreach(float side in new[]{-.064f,.064f})
                    Box(detail,"Block upright",new Vector3(side,0,.037f),new Vector3(.004f,.19f,.004f),.001f,alloy);
                Label(detail,key,new Vector3(0,-.059f,-.086f),Quaternion.identity,.014f,ink);
                CombineByMaterial(detail);
            }
        }
    }
}
