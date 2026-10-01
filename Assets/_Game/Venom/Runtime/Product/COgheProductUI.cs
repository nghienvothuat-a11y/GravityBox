using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace GravityBox.Venom
{
    public enum COgheProductPage { MainMenu, Game, Home, Intro, Victory, Style }
    public enum COgheProductPopup { None, Pause, Help, Restart, LeaveMenu, LeaveHome, Collection, Locked, Failure, Levels, StyleReplaceInk, StyleRinse, StyleReplaceInside, StyleLocked, StyleNeedsClear }

    /// <summary>Test-only tools (the level picker in Pause): compiled into development builds and the team's test builds
    /// (COGHE_TEST_TOOLS, added by the build scripts unless COGHE_STORE=1); absent from a store build.</summary>
    public static class COgheTestTools
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD || COGHE_TEST_TOOLS
        public const bool LevelSelect = true;
#else
        public const bool LevelSelect = false;
#endif
    }

    [DefaultExecutionOrder(150)]
    public sealed partial class COgheProductUI : MonoBehaviour
    {
        public VenomCampaign Game {get;private set;}
        public COgheProductPage Page {get;private set;}
        public COgheProductPopup Popup {get;private set;}
        public COgheProductCatalog Catalog {get;private set;}
        public bool BlockWorldInput => Page!=COgheProductPage.Game && Page!=COgheProductPage.Home || Popup!=COgheProductPopup.None;
        public bool Showcase => Page==COgheProductPage.MainMenu || Page==COgheProductPage.Home || Page==COgheProductPage.Style;
        public RectTransform SafeRoot => safe;
        public int ResumeLevel => Catalog.NextIncomplete(Game.Progress);
        private COgheUIArt art;
        private Canvas canvas;
        private RectTransform safe, pageRoot, popupRoot, dynamicRoot;
        private EventSystem ownEvents;
        private readonly List<RaycastResult> uiHits=new List<RaycastResult>(16);
        private PointerEventData pointerQuery;
        private Rect lastSafe;
        private int lastWidth,lastHeight,lastFragmentSignature;
        private float width,height,nextRefresh;
        private bool started,leaving;
        private COgheTapRail[] releaseRails;
        private Text toast;
        private Renderer[] tissueRenderers;
        private UnityEngine.Rendering.ShadowCastingMode[] tissueShadows;
        private float toastUntil;
        /// <summary>Levels 1, 3 and 4: the arrow and the rotate hint (null elsewhere).</summary>
        public COgheGuide Guide {get;private set;}

        public void Initialize(VenomCampaign game)
        {
            Game=game;Catalog=Resources.Load<COgheProductCatalog>("COgheUI/Catalog");
            if(Catalog==null || Catalog.Levels.Length==0)throw new InvalidOperationException("Build COghe Product UI assets before playing.");
            art=new COgheUIArt();
            tissueRenderers=game.Matter.GetComponentsInChildren<Renderer>();tissueShadows=new UnityEngine.Rendering.ShadowCastingMode[tissueRenderers.Length];
            for(int i=0;i<tissueRenderers.Length;i++)tissueShadows[i]=tissueRenderers[i].shadowCastingMode;
            releaseRails=game.Owner.Apparatus.GetComponentsInChildren<COgheTapRail>();
            var go=new GameObject("COghe Product Canvas",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            go.transform.SetParent(transform,false);canvas=go.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=100;
            var scaler=go.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ConstantPixelSize;
            safe=art.Rect(go.transform,"Safe area",new Rect());
            EnsureInput();
            Page=COgheProductPage.Game;
        }
        private void EnsureInput()
        {
            if(EventSystem.current!=null)return;
            ownEvents=new GameObject("COghe UI input",typeof(EventSystem),typeof(InputSystemUIInputModule)).GetComponent<EventSystem>();
            ownEvents.transform.SetParent(transform,false);ownEvents.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
        }
        private void Start()
        {
            EnsureInput();started=true;Layout();Guide=COgheGuide.Create(this,Game,safe,art);
            COgheStyle.Current.ApplyTo(Game);   // COghe's own look, everywhere it appears
            if(COgheProductMode.ReplayIntroOnLoad){COgheProductMode.ReplayIntroOnLoad=false;PlayIntro(true);}
            else if(!COgheProductMode.SessionStarted){COgheProductMode.SessionStarted=true;ShowMenu();}
            else if(Game.Definition.Order==1&&!COgheIntro.Seen)PlayIntro(false);
            else if(Game.Definition.Boss)PlayBossIntro();
            else Rebuild();
        }
        private void Update()
        {
            if(!started||leaving)return;
            StyleTick();
            if(lastWidth!=Screen.width||lastHeight!=Screen.height||lastSafe!=Screen.safeArea){Layout();Rebuild();}
            if(Page==COgheProductPage.Intro)return;
            if(Game.Owner.Paused&&Popup==COgheProductPopup.None)ShowPopup(COgheProductPopup.Pause);
            if(Page==COgheProductPage.Game)
            {
                if(Game.Owner.Lost&&Popup==COgheProductPopup.None){ShowPopup(COgheProductPopup.Failure);return;}
                if(Game.Owner.Completed){Page=COgheProductPage.Victory;Rebuild();COgheConfetti.Burst(safe,height,ConfettiOrigin(),Game.Definition.Order);return;}
                if(Time.unscaledTime>=nextRefresh)
                {
                    nextRefresh=Time.unscaledTime+.1f;int signature=FragmentSignature();
                    if(signature!=lastFragmentSignature){lastFragmentSignature=signature;BuildContextControls();}
                }
            }
            if(Page==COgheProductPage.Victory && Popup==COgheProductPopup.None && Game.AutoAdvance && Game.Owner.Celebration.ReadyForNext)
            {
                if(Game.Progress.RevealHome){Game.Progress.RevealHome=false;Game.Progress.Write();}
                if(Game.Definition.Order<Game.PlayableLevelCount)Load(Game.Definition.Order+1);
            }
            if(toast!=null&&Time.unscaledTime>toastUntil){Destroy(toast.transform.parent.gameObject);toast=null;}
        }
        public bool HandleInput()
        {
            if(!started)return true;
            var k=Keyboard.current;
            if(Page==COgheProductPage.Intro)
            {
                if(k!=null&&k.escapeKey.wasPressedThisFrame){FindFirstObjectByType<COgheIntro>()?.Skip();var boss=COgheBossIntro.Current;if(boss!=null&&boss.Clock<boss.TourLength+COgheBossIntro.PullBack)boss.Seek(boss.TourLength+COgheBossIntro.PullBack);}
                return true;
            }
            if(k!=null&&(k.escapeKey.wasPressedThisFrame||k.pKey.wasPressedThisFrame))
            {
                if(Page==COgheProductPage.Style){if(Popup!=COgheProductPopup.None)Resume();else LeaveStyle();return true;}
                if(Popup==COgheProductPopup.Help)ShowPopup(COgheProductPopup.Pause);
                else if(Popup!=COgheProductPopup.None)Resume();
                else if(Page==COgheProductPage.MainMenu)return true;
                else ShowPopup(COgheProductPopup.Pause);
                return true;
            }
            return BlockWorldInput;
        }
        public bool AllowsWorldPointer(Vector2 point)
        {
            if(BlockWorldInput||!RectTransformUtility.RectangleContainsScreenPoint(safe,point))return false;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(safe,point,null,out var p);
            float y=-p.y;
            if(y<90||y>height-104)return false;
            // Test the actual retained controls, including zone and release buttons outside the strips.
            // Reuse the query and results: no allocation on every drag/pinch sample.
            if(EventSystem.current!=null)
            {
                if(pointerQuery==null)pointerQuery=new PointerEventData(EventSystem.current);
                pointerQuery.Reset();pointerQuery.position=point;uiHits.Clear();
                EventSystem.current.RaycastAll(pointerQuery,uiHits);
                foreach(var hit in uiHits)if(hit.module is GraphicRaycaster)return false;
            }
            return true;
        }
        private void SetPaused(bool value){if(Game.Owner.Paused!=value)Game.Owner.TogglePause();Game.CancelProductPointer();}
        public void ShowPopup(COgheProductPopup popup){Popup=popup;SetPaused(true);Rebuild();}
        public void Resume(){Popup=COgheProductPopup.None;SetPaused(false);Rebuild();}
        public void ShowMenu()
        {
            Popup=COgheProductPopup.None;Game.EnterShowcase();Page=COgheProductPage.MainMenu;
            Game.SetHabitatPresentation(true);MenuShadows(true);Rebuild();
        }
        public void Play()
        {
            int next=ResumeLevel;
            if(next==0){Notify("All puzzles complete. Visit Home!");return;}
            if(Game.Definition.Order!=next){Load(next);return;}
            MenuShadows(false);Game.ResetLevel();Page=COgheProductPage.Game;Popup=COgheProductPopup.None;
            if(next==1&&!COgheIntro.Seen)PlayIntro(false);else if(Game.Definition.Boss)PlayBossIntro();else Rebuild();
        }
        public void ReplayIntro()
        {
            if(Game.Definition.Order!=1){COgheProductMode.ReplayIntroOnLoad=true;Load(1);return;}
            PlayIntro(true);
        }
        private void PlayIntro(bool replay)
        {
            MenuShadows(false);Game.ResetLevel();Page=COgheProductPage.Intro;Popup=COgheProductPopup.None;canvas.gameObject.SetActive(false);
            COgheIntro.Play(Game,()=>{if(this==null)return;canvas.gameObject.SetActive(true);if(replay)ShowMenu();else{Page=COgheProductPage.Game;Rebuild();}},replay);
        }
        private void Load(int n){leaving=true;SetPaused(false);Game.Load(n);}
        /// <summary>Boss levels open with a tour of the box and a warning, the level paused underneath.</summary>
        private void PlayBossIntro()
        {
            MenuShadows(false);Page=COgheProductPage.Intro;Popup=COgheProductPopup.None;canvas.gameObject.SetActive(false);
            COgheBossIntro.Play(Game,()=>{if(this==null)return;canvas.gameObject.SetActive(true);Page=COgheProductPage.Game;Rebuild();});
        }
        /// <summary>Just over COghe's head once the victory shot settles, in safe-area units from its lower-left corner.</summary>
        private Vector2 ConfettiOrigin()
        {
            var v=VenomCelebration.SettledViewport(.07f,Screen.width,Screen.height);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(safe,new Vector2(v.x*Screen.width,v.y*Screen.height),null,out var local);
            var p=local-safe.rect.min;return new Vector2(Mathf.Clamp(p.x,0,width),Mathf.Clamp(p.y,height*.3f,height*.85f));
        }
        /// <summary>Test builds: jump to any level from Pause.</summary>
        public void LoadForTest(int n){Popup=COgheProductPopup.None;Load(n);}
        public void OpenHome()
        {
            if(!Game.Progress.HomeUnlocked){ShowPopup(COgheProductPopup.Locked);return;}
            if(Page==COgheProductPage.Game&&!Game.Owner.Completed){ShowPopup(COgheProductPopup.LeaveHome);return;}
            EnterHome();
        }
        private void EnterHome(){MenuShadows(false);Game.EnterHome();Game.SetHabitatPresentation(false);Page=COgheProductPage.Home;Popup=COgheProductPopup.None;homeSize=-1;homeYaw=0;homeZoom=false;Rebuild();}
        public void Confirm()
        {
            var action=Popup;Popup=COgheProductPopup.None;
            if(action==COgheProductPopup.Restart||action==COgheProductPopup.Failure){Game.ResetLevel();Page=COgheProductPage.Game;Rebuild();}
            else if(action==COgheProductPopup.LeaveHome)EnterHome();
            else ShowMenu();
        }
        public void RequestMenu()
        {
            if(Page==COgheProductPage.Game)ShowPopup(COgheProductPopup.LeaveMenu);else ShowMenu();
        }
        public void ToggleMusic(){COgheAudio.MusicOn=!COgheAudio.MusicOn;Rebuild();}
        public void ToggleSound(){COgheAudio.EffectsOn=!COgheAudio.EffectsOn;Rebuild();}
        public void Release()
        {
            foreach(var rail in releaseRails)if(rail.Owns(Game.Motion.Selected)&&rail.CanInterrupt)rail.CancelTask();
            if(Game.Attached)Game.ReleaseProp();BuildContextControls();
        }
        private bool Holding()
        {
            if(Game.Attached)return true;
            foreach(var rail in releaseRails)if(rail.Owns(Game.Motion.Selected)&&rail.CanInterrupt)return true;
            return false;
        }
        private int FragmentSignature()
        {
            unchecked{int hash=Game.Motion.Selected;for(int i=0;i<32;i++)hash=hash*31+Game.Matter.Groups[i];return hash*2+(Holding()?1:0);}
        }
        public void Notify(string message)
        {
            if(toast!=null)Destroy(toast.transform.parent.gameObject);
            var box=art.Box(safe,"Notice",new Rect(24,height-180,width-48,58),COgheUIArt.Ink);
            toast=art.Label(box.transform,"Message",message,new Rect(12,2,width-72,54),12,COgheUIArt.Paper);toastUntil=Time.unscaledTime+3;
        }
        private void MenuShadows(bool menu)
        {for(int i=0;i<tissueRenderers.Length;i++)if(tissueRenderers[i]!=null)tissueRenderers[i].shadowCastingMode=menu?UnityEngine.Rendering.ShadowCastingMode.Off:tissueShadows[i];}
        public void FrameShowcase()
        {
            var camera=Game.Owner.View;
            if(Page==COgheProductPage.Home&&FrameHome(camera))return;
            if(Page==COgheProductPage.Style&&FrameStyle(camera))return;
            camera.orthographic=true;camera.aspect=(float)Screen.width/Screen.height;
            bool menu=Page==COgheProductPage.MainMenu;
            camera.transform.rotation=Quaternion.Euler(menu?17:24,-12,0);
            var focus=Game.Motion.Centre(0)+Vector3.up*.025f;
            camera.orthographicSize=menu?Mathf.Max(.215f,.10f/camera.aspect):Mathf.Max(.20f,.15f/camera.aspect);
            // Reserve a live-character area between the logo and Play at both short and tall portrait ratios.
            float logicalY=menu?(height*.12f+122+height-290)*.5f:height*.49f;
            float pixelY=Screen.safeArea.yMax-logicalY*canvas.scaleFactor;
            float centre=pixelY/Screen.height;
            camera.transform.position=focus-camera.transform.forward*.8f-camera.transform.up*((centre-.5f)*2*camera.orthographicSize);
            camera.nearClipPlane=.01f;camera.farClipPlane=30;
        }
        private void OnDestroy(){if(styleSaving&&Game!=null&&Game.Matter!=null)SaveLook();art?.Dispose();DisposeItemIcons();if(ownEvents!=null)Destroy(ownEvents.gameObject);}
    }
}
