using UnityEngine;

namespace GravityBox.Venom
{
    public sealed class COgheAssemblyBridgePresentation : MonoBehaviour
    {
        public COgheAssemblyBridge Bridge;
        public Renderer[] Lamps;
        MaterialPropertyBlock block;
        void LateUpdate()
        {
            if(Bridge==null||Lamps==null)return;
            if(block==null)block=new MaterialPropertyBlock();
            for(int i=0;i<Lamps.Length;i++)
            {
                if(Lamps[i]==null)continue;
                bool seated=Bridge.Rails[i].AtEnd&&Bridge.Rails[i].Latched;
                var colour=seated?new Color(.32f,.70f,.59f):new Color(.84f,.61f,.32f);
                block.SetColor("_BaseColor",colour);block.SetColor("_EmissionColor",colour*(seated?.12f:0));
                Lamps[i].SetPropertyBlock(block);
            }
        }
    }
}
