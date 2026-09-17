using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>Day Lab campaign presentation. All commands delegate to the
    /// existing campaign; this component never writes simulation state directly.</summary>
    public sealed class COgheDayLabPresentation : MonoBehaviour
    {
        public Renderer[] FocusOccluders;
        public Transform Step,StepShadow;
        public Renderer[] FadingFloors;
        public VenomSurfacePatch[] GlassSurfaces;
        public Material WorldTextMaterial;
        public Font WorldTextFont;
        private VenomCampaign game;
        private COgheCooperationPresentation cooperation;
        private GUIStyle brand, caption, title, body, chip, selected, action, status, footer;
        private Texture2D tile, chosen, pressed;
        private MaterialPropertyBlock block;
        private readonly bool[] seenGroups=new bool[32];
        private static readonly Color Ink=new Color(.19f,.29f,.34f);
        private void Awake()
        {
            game=GetComponent<VenomCampaign>();cooperation=GetComponent<COgheCooperationPresentation>();block=new MaterialPropertyBlock();
            Font.textureRebuilt+=RefreshFontAtlas;RefreshFontAtlas(WorldTextFont);
            if(GlassSurfaces==null)return;
            foreach(var p in GlassSurfaces)
            {
                if(p==null)continue;var r=p.GetComponent<Renderer>();r.GetPropertyBlock(block);
                block.SetFloat("_HasGrip",p.RingGrip?1:0);block.SetVector("_GripCentre",new Vector4(p.HoleCentre.x,p.HoleCentre.y,p.GripRadius,0));
                block.SetFloat("_RegionOnly",p.HasSlipRegion?1:0);
                block.SetVector("_SlipRect",new Vector4(p.SlipRegion.xMin,p.SlipRegion.yMin,p.SlipRegion.xMax,p.SlipRegion.yMax));
                bool internalProp=p.GetComponentInParent<VenomMovableProp>()!=null||p.name.StartsWith("Low wall");
                block.SetFloat("_NearFade",internalProp?0:1);block.SetFloat("_Spherical",p.SphereRadius>0?1:0);r.SetPropertyBlock(block);
            }
        }
        private void RefreshFontAtlas(Font font)
        {
            if(font!=null&&font==WorldTextFont&&WorldTextMaterial!=null)
                WorldTextMaterial.mainTexture=font.material.mainTexture;
        }
        private void LateUpdate()
        {
            if(game==null||FocusOccluders==null)return;
            // Leave forceRenderingOff to the victory camera. Only the near
            // decorative corner disappears while following the creature.
            // Compartment views retain the frame as a spatial reference.
            foreach(var r in FocusOccluders)if(r!=null&&r.enabled==game.Zoom)r.enabled=!game.Zoom;
            // This authored room is locked upright. The contact patch follows
            // the existing moving crate on its flat floor; it never adds force.
            if(Step!=null&&StepShadow!=null)StepShadow.position=new Vector3(Step.position.x,-.2997f,Step.position.z);
            if(FadingFloors!=null&&game.Owner!=null)
                foreach(var r in FadingFloors)
                {
                    if(r==null)continue;
                    float dot=Vector3.Dot(r.transform.forward,(game.Owner.View.transform.position-r.bounds.center).normalized);
                    float alpha=Mathf.Lerp(.045f,.93f,Mathf.SmoothStep(0,1,Mathf.InverseLerp(-.12f,.2f,dot)));
                    r.GetPropertyBlock(block);block.SetColor("_BaseColor",new Color(.77f,.83f,.84f,alpha));r.SetPropertyBlock(block);
                }
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
            if(game.CameraRig.Inspecting)GUI.Box(new Rect(17,8,506,game.CameraRig.ShowZones?237:191),GUIContent.none,chip);
            GUI.Label(new Rect(26,13,240,48),"COghe",brand);
            GUI.Label(new Rect(300,20,214,30),game.Home?"A  P L A C E  T O  B E L O N G":(game.Definition.Boss?"B O S S   /   ":"D A Y   L A B   /   ")+game.Definition.Order.ToString("00"),caption);
            if(GUI.Button(new Rect(26,61,98,25),"01–10",game.LevelPage==0?selected:chip))game.LevelPage=0;
            if(GUI.Button(new Rect(130,61,98,25),"11–20",game.LevelPage==1?selected:chip))game.LevelPage=1;
            for(int slot=0;slot<10;slot++)
            {
                int i=game.LevelPage*10+slot+1;
                if(GUI.Button(new Rect(26+slot*49,91,43,27),i%10==0?"B"+i:i.ToString("00"),i==game.Definition.Order?selected:chip))game.Load(i);
            }
            if(!game.Owner.Completed)
            {
                GUI.Label(new Rect(24,125,492,33),game.Home?"Nhà của COghe":game.Definition.Order==7?"Cùng nhau dịch chuyển":game.Definition.Title,title);
                if(!game.Definition.Boss&&!game.Home)
                    GUI.Label(new Rect(34,158,472,35),cooperation!=null?cooperation.Hint:game.Definition.Order==7?"Chạm thùng, rồi chạm nơi muốn đẩy hoặc kéo tới.":game.Definition.Lesson,body);
            }
            if(game.CameraRig.ShowZones)
            {
                float cell=488f/(game.CameraRig.ZoneCount+1);
                for(int i=-1;i<game.CameraRig.ZoneCount;i++)
                    if(GUI.Button(new Rect(26+(i+1)*cell,201,cell-6,37),i<0?"Toàn cảnh":game.Definition.CameraZones[i].Label,
                        !game.Zoom&&game.CameraRig.Zone==i?selected:chip))game.CameraRig.SelectZone(i);
            }
            if(game.Owner.Lost)
            {
                GUI.Box(new Rect(25,h*.40f,490,140),GUIContent.none,chip);
                GUI.Label(new Rect(43,h*.40f+12,454,70),game.Failure,body);
                if(GUI.Button(new Rect(155,h*.40f+88,230,38),"Thử lại",action))game.ResetLevel();
            }
            else if(game.Owner.Completed)
            {
                GUI.Label(new Rect(25,h-195,490,45),"Chúng mình làm được rồi!",title);
                if(game.Definition.Boss&&game.Owner.Celebration.ReadyForNext)
                {
                    if(GUI.Button(new Rect(26,h-137,235,40),"Nhà của COghe",action))game.EnterHome();
                    if(game.Definition.Order<VenomCampaign.LevelCount&&GUI.Button(new Rect(276,h-137,235,40),"Tiếp tục",action))game.Load(game.Definition.Order+1);
                }
            }
            else
            {
                string activity=game.Owner.Paused?"Đang nghỉ một chút":game.Attached?(game.IsPulling?"COghe đang kéo vật":"COghe đang giữ vật"):(game.Activity=="Idle"?"COghe đang chờ được chỉ đường":game.Activity);
                GUI.Label(new Rect(26,h-154,game.Attached?316:488,30),activity,status);
                if(game.Attached&&GUI.Button(new Rect(350,h-154,164,29),"Buông vật",chip))game.ReleaseProp();
                if(game.Matter.TotalFragmentCount>1)
                {
                    System.Array.Clear(seenGroups,0,seenGroups.Length);int slot=0;
                    int groupCount=Mathf.Max(1,game.Matter.TotalFragmentCount);int columns=Mathf.Min(6,groupCount);
                    float cell=488f/columns;
                    for(int i=0;i<32;i++)
                    {
                        int group=game.Matter.Groups[i];if(seenGroups[group]||game.Matter.Escaped[i])continue;seenGroups[group]=true;
                        if(GUI.Button(new Rect(26+(slot%columns)*cell,h-119-(slot/columns)*32,cell-6,29),cooperation!=null?cooperation.FragmentLabel(i,slot+1):"Phần "+(slot+1),game.Matter.Groups[game.Motion.Selected]==group?selected:chip))game.SelectFragment(i);
                        slot++;
                    }
                }
            }
            bool homeAvailable=game.Progress.HomeUnlocked;float width=homeAvailable?112:152,gap=homeAvailable?125:168;
            if(GUI.Button(new Rect(26,h-77,width,42),"Làm lại",action))game.ResetLevel();
            if(GUI.Button(new Rect(26+gap,h-77,width,42),game.Owner.Paused?"Tiếp tục":"Tạm dừng",action))game.Owner.TogglePause();
            if(GUI.Button(new Rect(26+gap*2,h-77,width,42),game.Zoom?"Toàn cảnh":"Theo COghe",action))game.CameraRig.ToggleFollow();
            if(homeAvailable&&GUI.Button(new Rect(26+gap*3,h-77,width,42),game.Home?"Chào bạn":"Nhà",action)){if(game.Home)game.GreetHome();else game.EnterHome();}
            if(game.Home)
            {
                if(GUI.Button(new Rect(100,h-119,160,29),"Cho ăn",chip))game.FeedHome();
                if(GUI.Button(new Rect(280,h-119,160,29),"Chơi cùng",chip))game.GreetHome();
            }
            GUI.Label(new Rect(26,h-29,488,20),"COghe  /  PHÒNG NGHIÊN CỨU",footer);
            GUI.matrix=old;GUI.color=oldColor;GUI.backgroundColor=oldBackground;GUI.contentColor=oldContent;
        }
        private void OnDestroy()
        {Font.textureRebuilt-=RefreshFontAtlas;if(tile!=null)Destroy(tile);if(chosen!=null)Destroy(chosen);if(pressed!=null)Destroy(pressed);}
    }
}
