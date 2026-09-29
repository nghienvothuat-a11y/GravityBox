using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;

namespace GravityBox.Venom
{
    /// <summary>Level-one visual experiment. Reads live tissue; never changes its physics.</summary>
    [DefaultExecutionOrder(-400)]
    public sealed class COgheGlassDepthStudy:MonoBehaviour
    {
        public static int Selection=3;
        public bool ShowComparison;
        public Renderer[] Surfaces,Frame;
        public Material[] RefinedFrame;
        public Material ClearLit,FloorLit,ClearReflected,FloorReflected,Creature;
        public Renderer Bench;
        public Material LabBackground;
        public Light Key,Fill;
        private VenomCampaign game;
        private VenomProfile skinProfile;
        private VenomSurfacePatch[] patches;
        private MaterialPropertyBlock block;
        private GUIStyle button;
        private static readonly string[] Labels={"Gốc","A · Ánh sáng","B · Chất liệu","C · Phòng lab"};
        private static readonly int Contact=Shader.PropertyToID("_Contact"),Alpha=Shader.PropertyToID("_ContactAlpha");
        public int Active {get;private set;}
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Launch()
        {
            Selection=3;var args=Environment.GetCommandLineArgs();
            for(int i=0;i<args.Length-1;i++)if(args[i]=="-coghe-depth")Selection=Parse(args[i+1]);
#if UNITY_ANDROID && !UNITY_EDITOR
            using(var player=new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using(var activity=player.GetStatic<AndroidJavaObject>("currentActivity"))
            using(var intent=activity.Call<AndroidJavaObject>("getIntent"))
                if(intent.Call<bool>("hasExtra","coghe_depth"))Selection=Parse(intent.Call<string>("getStringExtra","coghe_depth"));
#endif
        }
        private static int Parse(string value)=>value=="a"?1:value=="b"?2:value=="c"?3:0;
        private void Awake()
        {
            game=GetComponent<VenomCampaign>();Active=Selection;
            patches=new VenomSurfacePatch[Surfaces.Length];block=new MaterialPropertyBlock();
            for(int i=0;i<patches.Length;i++)patches[i]=Surfaces[i].GetComponent<VenomSurfacePatch>();
            if(Active==0)return;
            Key.transform.rotation=Quaternion.Euler(48,-38,0);Key.intensity=1.28f;Key.shadowStrength=.46f;
            Key.shadowBias=.01f;Key.shadowNormalBias=.015f;Key.GetUniversalAdditionalLightData().usePipelineSettings=false;
            Fill.intensity=.30f;
            RenderSettings.ambientSkyColor=new Color(.63f,.72f,.78f);RenderSettings.ambientEquatorColor=new Color(.41f,.47f,.49f);
            for(int i=0;i<Surfaces.Length;i++)Surfaces[i].sharedMaterial=patches[i].Normal.y>.98f?(Active>=2?FloorReflected:FloorLit):(Active>=2?ClearReflected:ClearLit);
            if(Active>=2)
            {
                for(int i=0;i<Frame.Length;i++)Frame[i].sharedMaterial=RefinedFrame[i];
                var owner=GetComponent<VenomLevelController>();skinProfile=Instantiate(owner.MatterProfile);
                skinProfile.Skin=Creature;owner.MatterProfile=skinProfile;
            }
            if(Active==3)Bench.sharedMaterial=LabBackground;
        }
        private void LateUpdate()
        {
            if(Active==0||game.Matter==null||game.Motion==null)return;
            // At most six planes x 32 particles. No physics query, allocation or scene search.
            for(int s=0;s<patches.Length;s++)
            {
                var p=patches[s];Vector2 min=Vector2.one*999,max=-min;float closest=.06f;int count=0;
                if(!game.Owner.Completed&&!game.Home)
                for(int i=0;i<game.Matter.Bodies.Length;i++)
                {
                    if(game.Matter.Escaped[i])continue;
                    Vector3 q=p.transform.InverseTransformPoint(game.Matter.Bodies[i].position);
                    if(q.z<0||q.z>.06f||!p.Contains(new Vector3(q.x,q.y,0)))continue;
                    Vector2 point=new Vector2(q.x,q.y);min=Vector2.Min(min,point);max=Vector2.Max(max,point);closest=Mathf.Min(closest,q.z);count++;
                }
                Surfaces[s].GetPropertyBlock(block);
                float a=count>0?Mathf.Clamp01(count/12f)*Mathf.Lerp(.52f,0,closest/.06f):0;
                Vector2 centre=(min+max)*.5f,radius=(max-min)*.5f+Vector2.one*.018f;
                block.SetVector(Contact,count>0?new Vector4(centre.x,centre.y,radius.x,radius.y):Vector4.zero);
                block.SetFloat(Alpha,a);Surfaces[s].SetPropertyBlock(block);
            }
        }
        private void OnGUI()
        {
            if(!ShowComparison||game==null||game.Owner==null||game.Owner.Completed||game.Home)return;
            if(button==null)button=new GUIStyle(GUI.skin.button){fontSize=12,alignment=TextAnchor.MiddleCenter};
            var area=Screen.safeArea;float scale=Mathf.Min(area.width/540f,area.height/960f),h=area.height/scale;
            var matrix=GUI.matrix;var color=GUI.backgroundColor;bool enabled=GUI.enabled;
            GUI.matrix=Matrix4x4.TRS(new Vector3(area.x+(area.width-540*scale)*.5f,Screen.height-area.yMax,0),Quaternion.identity,Vector3.one*scale);
#if (DEVELOPMENT_BUILD || COGHE_MOBILE_BENCHMARK) && !UNITY_EDITOR
            GUI.enabled=!ChapterProof.COgheViewProofPlayer.IsRunning;
#endif
            for(int i=0;i<4;i++)
            {
                GUI.backgroundColor=i==Active?new Color(.40f,.68f,.67f):new Color(.88f,.90f,.86f);
                if(GUI.Button(new Rect(26+i*124,h-119,118,29),Labels[i],button)&&i!=Active)
                {Selection=i;SceneManager.LoadScene(SceneManager.GetActiveScene().path);}
            }
            GUI.matrix=matrix;GUI.backgroundColor=color;GUI.enabled=enabled;
        }
        private void OnDestroy(){if(skinProfile!=null)Destroy(skinProfile);}
    }
}
