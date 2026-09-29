using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;

namespace GravityBox.Venom
{
    /// <summary>Experimental A/B presentation only. Switching reloads the same puzzle.</summary>
    [DefaultExecutionOrder(-500)]
    public sealed class COgheGraphicProfile : MonoBehaviour
    {
        public static bool UseNew = false;
        public bool ShowComparison;
        public Renderer[] Replaced;
        public GameObject[] NewRoots;
        public Transform[] NewPanes;
        public Material[] FadeSources, FadeVariants;
        public Material CreatureMaterial,StudioMaterial;
        public float FramingPadding = .13f;
        private VenomProfile temporaryProfile;
        public bool ActiveNew { get; private set; }
        public string Meter { get; private set; } = "Đang đo";
        private float elapsed;
        private int frames;
        private void Update()
        {
            elapsed+=Time.unscaledDeltaTime;frames++;
            if(elapsed<.75f)return;
            Meter=(ActiveNew?"BLENDER":"BẢN CŨ")+"  ·  "+(frames/elapsed).ToString("F0")+" FPS";
            elapsed=0;frames=0;
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ReadLaunchProfile()
        {
            UseNew = false;
            var args = Environment.GetCommandLineArgs();
            for (int i=0;i<args.Length-1;i++) if(args[i]=="-coghe-graphic") UseNew=args[i+1]!="current";
#if UNITY_ANDROID && !UNITY_EDITOR
            using(var player=new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using(var activity=player.GetStatic<AndroidJavaObject>("currentActivity"))
            using(var intent=activity.Call<AndroidJavaObject>("getIntent"))
                if(intent.Call<bool>("hasExtra","coghe_graphic")) UseNew=intent.Call<string>("getStringExtra","coghe_graphic")!="current";
#endif
        }
        private void Awake()
        {
            ActiveNew = UseNew;
            foreach(var root in NewRoots) if(root!=null) root.SetActive(ActiveNew);
            if(!ActiveNew) return;
            foreach(var r in Replaced) if(r!=null) r.enabled=false;
            var view=GetComponent<COgheViewPresentation>();
            view.PaneVisuals=NewPanes;view.FadeSources=FadeSources;view.FadeVariants=FadeVariants;
            var owner=GetComponent<VenomLevelController>();
            temporaryProfile=Instantiate(owner.MatterProfile);temporaryProfile.name="NewGraphic visual skin only";
            temporaryProfile.Skin=CreatureMaterial;owner.MatterProfile=temporaryProfile;
            var studio=transform.Find("Day Lab studio");
            if(studio!=null&&StudioMaterial!=null)
                foreach(var renderer in studio.GetComponentsInChildren<MeshRenderer>())
                    if(renderer.bounds.size.x>8&&renderer.bounds.size.z>8)renderer.sharedMaterial=StudioMaterial;
            if(studio!=null)foreach(var light in studio.GetComponentsInChildren<Light>())
            {
                if(light.shadows!=LightShadows.None)
                {light.GetUniversalAdditionalLightData().usePipelineSettings=false;light.intensity=1.18f;light.color=new Color(1,.96f,.88f);light.shadowStrength=.60f;light.shadowBias=.08f;light.shadowNormalBias=.06f;}
                else {light.intensity=.25f;light.color=new Color(.79f,.89f,1);}
            }
            RenderSettings.ambientSkyColor=new Color(.76f,.81f,.85f);
            RenderSettings.ambientEquatorColor=new Color(.56f,.58f,.57f);
            RenderSettings.ambientGroundColor=new Color(.34f,.31f,.26f);
            RenderSettings.reflectionIntensity=.90f;
        }
        private void Start()
        {
            if(!ActiveNew)return;
            // Day Lab configures the old outline in Awake; the Blender annulus owns it here.
            foreach(var line in GetComponent<VenomLevelController>().Outlet.GetComponentsInChildren<LineRenderer>(true))line.enabled=false;
        }
        public void Toggle()
        {
            UseNew=!ActiveNew;
            SceneManager.LoadScene(SceneManager.GetActiveScene().path);
        }
        private void OnDestroy(){if(temporaryProfile!=null)Destroy(temporaryProfile);}
    }
}
