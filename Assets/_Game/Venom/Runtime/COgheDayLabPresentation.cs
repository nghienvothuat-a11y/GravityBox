using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>Level 07's light laboratory HUD. All commands delegate to the
    /// existing campaign; this component never writes simulation state directly.</summary>
    public sealed class COgheDayLabPresentation : MonoBehaviour
    {
        public Renderer[] FocusOccluders;
        public Transform Step,StepShadow;
        private VenomCampaign game;
        private GUIStyle brand, caption, title, body, chip, selected, action, status, footer;
        private Texture2D tile, chosen, pressed;
        private static readonly Color Ink=new Color(.19f,.29f,.34f);
        private void Awake()=>game=GetComponent<VenomCampaign>();
        private void LateUpdate()
        {
            if(game==null||FocusOccluders==null)return;
            // Leave forceRenderingOff to the victory camera. Only the near
            // decorative corner disappears during close inspection.
            foreach(var r in FocusOccluders)if(r!=null&&r.enabled==game.Zoom)r.enabled=!game.Zoom;
            // This authored room is locked upright. The contact patch follows
            // the existing moving crate on its flat floor; it never adds force.
            if(Step!=null&&StepShadow!=null)StepShadow.position=new Vector3(Step.position.x,-.2997f,Step.position.z);
        }
        private Texture2D Tile(Color color)
        {
            const int size=32;const float radius=7;
            var texture=new Texture2D(size,size,TextureFormat.RGBA32,false){hideFlags=HideFlags.HideAndDontSave,filterMode=FilterMode.Bilinear};
            var pixels=new Color[size*size];
            for(int y=0;y<size;y++)for(int x=0;x<size;x++)
            {
                float dx=Mathf.Max(Mathf.Abs(x-(size-1)*.5f)-(size*.5f-radius),0);
                float dy=Mathf.Max(Mathf.Abs(y-(size-1)*.5f)-(size*.5f-radius),0);
                var c=color;c.a*=Mathf.Clamp01(radius-Mathf.Sqrt(dx*dx+dy*dy));pixels[y*size+x]=c;
            }
            texture.SetPixels(pixels);texture.Apply();return texture;
        }
        private void Styles()
        {
            tile=Tile(new Color(.98f,.98f,.94f,.89f));chosen=Tile(new Color(.21f,.43f,.44f));pressed=Tile(new Color(.75f,.84f,.81f));
            brand=new GUIStyle(GUI.skin.label){fontSize=33,fontStyle=FontStyle.Bold,normal={textColor=Ink}};
            caption=new GUIStyle(brand){fontSize=10,fontStyle=FontStyle.Normal,alignment=TextAnchor.MiddleRight};
            footer=new GUIStyle(caption){alignment=TextAnchor.MiddleCenter,fontSize=9};
            title=new GUIStyle(brand){fontSize=23,alignment=TextAnchor.MiddleCenter};
            body=new GUIStyle(title){fontSize=14,fontStyle=FontStyle.Normal,wordWrap=true};
            status=new GUIStyle(body){fontSize=15};
            chip=new GUIStyle(GUI.skin.button){fontSize=13,alignment=TextAnchor.MiddleCenter,padding=new RectOffset(3,3,1,1),border=new RectOffset(8,8,8,8),normal={background=tile,textColor=Ink},hover={background=tile,textColor=Ink},active={background=pressed,textColor=Ink},focused={background=tile,textColor=Ink}};
            selected=new GUIStyle(chip){normal={background=chosen,textColor=Color.white},hover={background=chosen,textColor=Color.white}};
            action=new GUIStyle(chip){fontSize=14};
        }
        private void OnGUI()
        {
            if(game==null||game.Owner==null)return;
            if(brand==null)Styles();
            var old=GUI.matrix;var oldColor=GUI.color;var oldBackground=GUI.backgroundColor;var oldContent=GUI.contentColor;
            float s=Mathf.Min(Screen.width/540f,Screen.height/960f),h=Screen.height/s;
            GUI.matrix=Matrix4x4.TRS(new Vector3((Screen.width-540*s)*.5f,0,0),Quaternion.identity,Vector3.one*s);
            GUI.color=GUI.backgroundColor=GUI.contentColor=Color.white;
            if(game.Zoom)GUI.Box(new Rect(17,8,506,167),GUIContent.none,chip);
            GUI.Label(new Rect(26,13,240,48),"COghe",brand);
            GUI.Label(new Rect(300,20,214,30),"D A Y   L A B   /   0 7",caption);
            for(int i=1;i<=10;i++)
                if(GUI.Button(new Rect(26+(i-1)*49,68,43,28),i==10?"BOSS":i.ToString("00"),i==7?selected:chip))game.Load(i);
            if(!game.Owner.Completed)
            {
                GUI.Label(new Rect(24,108,492,33),"Cùng nhau dịch chuyển",title);
                GUI.Label(new Rect(34,141,472,30),"Chạm thùng, rồi chạm nơi muốn đẩy hoặc kéo tới.",body);
            }
            if(game.Owner.Lost)
            {
                GUI.Box(new Rect(25,h*.40f,490,140),GUIContent.none,chip);
                GUI.Label(new Rect(43,h*.40f+12,454,70),game.Failure,body);
                if(GUI.Button(new Rect(155,h*.40f+88,230,38),"Thử lại",action))game.ResetLevel();
            }
            else if(game.Owner.Completed)
                GUI.Label(new Rect(25,h-195,490,45),"Chúng mình làm được rồi!",title);
            else
            {
                string activity=game.Owner.Paused?"Đang nghỉ một chút":game.Attached?(game.IsPulling?"COghe đang kéo thùng":"COghe đang giữ thùng"):(game.Activity=="Idle"?"COghe đang chờ được chỉ đường":game.Activity);
                GUI.Label(new Rect(26,h-154,488,30),activity,status);
                if(game.Attached&&GUI.Button(new Rect(183,h-119,174,29),"Buông thùng",chip))game.ReleaseProp();
            }
            if(GUI.Button(new Rect(26,h-77,152,42),"Làm lại",action))game.ResetLevel();
            if(GUI.Button(new Rect(194,h-77,152,42),game.Owner.Paused?"Tiếp tục":"Tạm dừng",action))game.Owner.TogglePause();
            if(GUI.Button(new Rect(362,h-77,152,42),game.Zoom?"Thu nhỏ":"Nhìn gần",action))game.Zoom=!game.Zoom;
            GUI.Label(new Rect(26,h-29,488,20),"COghe  /  PHÒNG NGHIÊN CỨU",footer);
            GUI.matrix=old;GUI.color=oldColor;GUI.backgroundColor=oldBackground;GUI.contentColor=oldContent;
        }
        private void OnDestroy()
        {if(tile!=null)Destroy(tile);if(chosen!=null)Destroy(chosen);if(pressed!=null)Destroy(pressed);}
    }
}
