using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>Reads the real load and winch state; never advances puzzle state.</summary>
    public sealed class COgheCooperationPresentation : MonoBehaviour
    {
        public COgheCooperativeWinch Winch;
        public Renderer[] UnlockLamps, DoorLamps;
        private VenomCampaign game;
        private MaterialPropertyBlock block;
        void Awake(){game=GetComponent<VenomCampaign>();block=new MaterialPropertyBlock();}
        public string Hint
        {
            get
            {
                if(game==null||Winch==null)return "";
                if(Winch.Complete)return game.Matter.TotalFragmentCount>1
                    ?"Hai cửa đã mở. Chỉ một phần đi qua chỗ phần kia để hợp thể, rồi mới thoát."
                    :"Đã hợp thể! Chạm lỗ thoát bên phải.";
                if(game.Matter.TotalFragmentCount==1)return "Chạm lưỡi máy chém để tách đôi. Hai phần sẽ giữ A và kéo B.";
                if(!Winch.Input.Active)return "Chọn phần bên trái → đứng trên nút A để mở khóa tay nắm B.";
                if(Winch.Engaged)return "Đang nâng hai cửa… Giữ phần bên trái trên nút A.";
                return game.Attached?"Chạm mũi tên cạnh B để kéo. Phần bên trái tiếp tục giữ A."
                    :"A đã mở khóa B. Chọn phần bên phải → chạm tay nắm B.";
            }
        }
        public string FragmentLabel(int anchor,int index)
        {
            var centre=game.Motion.Centre(anchor);
            if(Winch.Input.Active&&Vector3.Distance(centre,Winch.Input.transform.position)<.065f)
                return "Phần "+index+" · giữ A";
            return "Phần "+index+(game.Root.InverseTransformPoint(centre).x<0?" · trái":" · phải");
        }
        void LateUpdate()
        {
            if(Winch==null)return;
            Paint(UnlockLamps,Winch.Input.Active||Winch.Complete);
            Paint(DoorLamps,Winch.Complete);
        }
        void Paint(Renderer[] lamps,bool active)
        {
            if(lamps==null)return;
            var colour=active?new Color(.32f,.70f,.59f):new Color(.84f,.61f,.32f);
            block.SetColor("_BaseColor",colour);block.SetColor("_EmissionColor",colour*(active?.12f:0));
            foreach(var lamp in lamps)if(lamp!=null)lamp.SetPropertyBlock(block);
        }
    }
}
