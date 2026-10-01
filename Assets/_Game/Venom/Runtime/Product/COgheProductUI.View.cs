using UnityEngine;
using UnityEngine.UI;

namespace GravityBox.Venom
{
    public sealed partial class COgheProductUI
    {
        private readonly bool[] shownGroups=new bool[32];
        private void Layout()
        {
            var area=Screen.safeArea;if(area.width<1||area.height<1)area=new Rect(0,0,Screen.width,Screen.height);
            float scale=Mathf.Min(area.width/360f,area.height/640f);
            canvas.GetComponent<CanvasScaler>().scaleFactor=scale;canvas.scaleFactor=scale;
            width=area.width/scale;height=area.height/scale;
            safe.anchoredPosition=new Vector2(area.x/scale,-(Screen.height-area.yMax)/scale);safe.sizeDelta=new Vector2(width,height);
            lastSafe=Screen.safeArea;lastWidth=Screen.width;lastHeight=Screen.height;
        }
        private void Clear(ref RectTransform r){if(r!=null){r.gameObject.SetActive(false);Destroy(r.gameObject);}r=null;}
        private void Rebuild()
        {
            if(!started)return;
            Clear(ref popupRoot);Clear(ref pageRoot);dynamicRoot=null;
            if(toast!=null)Destroy(toast.transform.parent.gameObject);toast=null;
            pageRoot=art.Rect(safe,"Page "+Page,new Rect(0,0,width,height));
            switch(Page)
            {
                case COgheProductPage.MainMenu:MainMenuView();break;
                case COgheProductPage.Home:HomeView();break;
                case COgheProductPage.Game:GameView();break;
                case COgheProductPage.Victory:VictoryView();break;
            }
            if(Popup!=COgheProductPopup.None)PopupView();
        }
        private void MainMenuView()
        {
            art.Label(pageRoot,"COghe logo","C<color=#356D6D>O</color>ghe",new Rect(0,height*.12f,width,83),65);
            art.Box(pageRoot,"Brand underline",new Rect(width*.5f-12,height*.12f+92,24,2),new Color(.58f,.71f,.66f));
            float y=height-265,w=Mathf.Min(280,width-80),x=(width-w)*.5f;
            art.Button(pageRoot,"Play",new Rect(x,y,w,64),COgheIcon.Play,Play,"Play",true);
            int next=ResumeLevel;art.Label(pageRoot,"Progress",next>0?"Level "+next:"All puzzles complete",new Rect(x,y+75,w,20),12,COgheUIArt.Muted);
            MenuEntry(width*.5f-88,y+118,"Home",Game.Progress.HomeUnlocked?COgheIcon.Home:COgheIcon.Lock,OpenHome);
            MenuEntry(width*.5f+32,y+118,"Intro",COgheIcon.Film,ReplayIntro);
            // Real 3D creature receives a greeting; this transparent target does not capture Play/Home.
            var touch=art.Box(pageRoot,"Greet COghe",new Rect(width*.16f,height*.29f,width*.68f,Mathf.Max(48,y-18-height*.29f)),Color.clear,true);
            touch.gameObject.AddComponent<Button>().onClick.AddListener(()=>{Game.GreetHome();COgheAudio.Happy();});
        }
        private void MenuEntry(float x,float y,string label,COgheIcon icon,System.Action action)
        {
            art.Button(pageRoot,label,new Rect(x,y,56,52),icon,action);
            art.Label(pageRoot,label+" label",label,new Rect(x-10,y+61,76,24),12);
        }
        private void Top(bool home=false)
        {
            if(home){art.Button(pageRoot,"Back",new Rect(24,25,52,52),COgheIcon.Back,ShowMenu);art.Label(pageRoot,"Home title","Home",new Rect(85,25,width-170,52),19);}
            else
            {
                art.Label(pageRoot,"Level number",Game.Definition.Order.ToString("00"),new Rect(24,25,48,52),26,null,TextAnchor.MiddleLeft);
                art.Label(pageRoot,"Day Lab","D A Y\nL A B",new Rect(78,33,55,34),9,COgheUIArt.Muted,TextAnchor.MiddleLeft);
            }
            art.Button(pageRoot,"Pause",new Rect(width-76,25,52,52),COgheIcon.Pause,()=>ShowPopup(COgheProductPopup.Pause));
        }
        private void GameView()
        {
            Top();art.Button(pageRoot,"Overview",new Rect(width-72,height-82,48,48),COgheIcon.Overview,()=>Game.CameraRig.Overview());
            if(Game.CameraRig.ShowZones)
            {
                float x=24;int count=Game.CameraRig.ZoneCount;
                for(int i=0;i<count;i++){int zone=i;art.Button(pageRoot,"Inspect zone "+(i+1),new Rect(x+i*56,88,48,48),COgheIcon.Circle,()=>Game.CameraRig.SelectZone(zone));art.Label(pageRoot,"Zone "+(i+1),(i+1).ToString(),new Rect(x+i*56,88,48,48),12);}
            }
            BuildContextControls();
        }
        private void BuildContextControls()
        {
            if(Page!=COgheProductPage.Game)return;
            Clear(ref dynamicRoot);dynamicRoot=art.Rect(pageRoot,"Context controls",new Rect(0,0,width,height));
            if(Game.Matter.TotalFragmentCount>1)
            {
                float trayWidth=width-120;
                var tray=art.Box(dynamicRoot,"Fragments",new Rect(24,height-85,trayWidth,60),COgheUIArt.Paper,true).rectTransform;
                var viewport=art.Rect(tray,"Viewport",new Rect(4,2,trayWidth-8,56));viewport.gameObject.AddComponent<RectMask2D>();
                var content=art.Rect(viewport,"Content",new Rect(0,0,Game.Matter.TotalFragmentCount*80,56));
                var scroll=tray.gameObject.AddComponent<ScrollRect>();scroll.viewport=viewport;scroll.content=content;scroll.horizontal=true;scroll.vertical=false;scroll.movementType=ScrollRect.MovementType.Clamped;
                System.Array.Clear(shownGroups,0,shownGroups.Length);int slot=0,selected=0;
                for(int i=0;i<32;i++)
                {
                    int group=Game.Matter.Groups[i];if(shownGroups[group]||Game.Matter.Escaped[i])continue;shownGroups[group]=true;
                    int count=0;for(int j=0;j<32;j++)if(Game.Matter.Groups[j]==group)count++;
                    int anchor=i;bool chosen=group==Game.Matter.Groups[Game.Motion.Selected];if(chosen)selected=slot;
                    var b=art.Button(content,"Fragment "+(slot+1),new Rect(slot*80,0,74,52),COgheIcon.Circle,()=>{Game.SelectFragment(anchor);BuildContextControls();},null,false,chosen);
                    // Mass, not ordinal, is meaningful when the quantum machine divides again.
                    foreach(var image in b.GetComponentsInChildren<Image>())if(image.gameObject!=b.gameObject)image.enabled=false;
                    art.Label(b.transform,"Mass",Mathf.RoundToInt(count*100f/32)+"%",new Rect(0,0,74,52),13,chosen?COgheUIArt.Teal:COgheUIArt.Ink);
                    slot++;
                }
                content.sizeDelta=new Vector2(slot*80,56);float overflow=Mathf.Max(0,content.sizeDelta.x-viewport.sizeDelta.x);
                content.anchoredPosition=new Vector2(-Mathf.Clamp(selected*80-40,0,overflow),0);
            }
            if(Holding())
            {
                var b=art.Button(dynamicRoot,"Release object",new Rect(width-72,height-147,48,48),COgheIcon.Hand,Release);
                art.Icon(b.transform,COgheIcon.Close,new Rect(29,29,14,14));
            }
        }
        private void HomeView()
        {
            Top(true);float x=width*.5f-130,y=height-152;
            MenuEntry(x,y,"Feed",COgheIcon.Food,()=>{Game.FeedHome();COgheAudio.Happy();});
            MenuEntry(x+104,y,"Play",COgheIcon.Heart,()=>{Game.GreetHome();COgheAudio.Happy();});
            MenuEntry(x+208,y,"Items",COgheIcon.Menu,()=>ShowPopup(COgheProductPopup.Collection));
        }
        private void VictoryView()
        {
            bool final=Game.Definition.Order==Game.PlayableLevelCount;
            art.Icon(pageRoot,COgheIcon.Check,new Rect(width*.5f-22,height*.13f,44,44),COgheUIArt.Teal);
            art.Label(pageRoot,"Victory",final?"All done!":"Well done!",new Rect(20,height*.13f+54,width-40,44),28);
            if(final)
            {
                art.Label(pageRoot,"Campaign complete","Every puzzle, together.",new Rect(20,height-225,width-40,32),14,COgheUIArt.Muted);
                art.Button(pageRoot,"Back to menu",new Rect(width*.5f-130,height-140,260,58),COgheIcon.Menu,ShowMenu,"Menu",true);
            }
            else art.Label(pageRoot,"Next level","→  Level "+(Game.Definition.Order+1),new Rect(24,height-105,width-48,32),14,COgheUIArt.Muted);
            if(Game.Progress.RevealHome)
            {art.Label(pageRoot,"Home unlocked","Home unlocked",new Rect(20,height-208,width-40,30),17);art.Button(pageRoot,"Visit Home",new Rect(width*.5f-70,height-170,140,52),COgheIcon.Home,EnterHome,"Home");}
        }
        private void PopupView()
        {
            popupRoot=art.Rect(safe,"Popup "+Popup,new Rect(0,0,width,height));
            var veil=art.Box(popupRoot,"Input shield",new Rect(-width,-height,width*3,height*3),new Color(.13f,.23f,.21f,.36f),true);veil.sprite=null;
            float w=Mathf.Min(312,width-40),h=Popup==COgheProductPopup.Pause?(COgheTestTools.LevelSelect&&Page!=COgheProductPage.Home?482:430):Popup==COgheProductPopup.Levels?Mathf.Min(560,height-90):Popup==COgheProductPopup.Help?450:Popup==COgheProductPopup.Collection&&Game.HomeRoom!=null?Mathf.Min(560,height-90):260;
            var panel=art.Box(popupRoot,"Panel",new Rect((width-w)*.5f,(height-h)*.5f,w,h),COgheUIArt.Paper,true).rectTransform;
            if(Popup==COgheProductPopup.Pause)
            {
                PopupTitle(panel,w,"Paused");art.Label(panel,"Context",Page==COgheProductPage.Home?"Home":"Level "+Game.Definition.Order,new Rect(20,65,w-40,22),12,COgheUIArt.Muted);
                bool home=Page==COgheProductPage.Home;float cell=(w-36)/3;
                Tile(panel,18,108,cell,COgheIcon.Restart,"Restart",()=>ShowPopup(COgheProductPopup.Restart),!home);
                Tile(panel,18+cell,108,cell,Game.Progress.HomeUnlocked?COgheIcon.Home:COgheIcon.Lock,"Home",OpenHome,!home);
                Tile(panel,18+2*cell,108,cell,COgheIcon.Menu,"Menu",RequestMenu);
                Tile(panel,18,214,cell,COgheIcon.Help,"How to play",()=>ShowPopup(COgheProductPopup.Help),!home);
                Tile(panel,18+cell,214,cell,COgheIcon.Music,"Music",ToggleMusic,true,COgheAudio.MusicOn);
                Tile(panel,18+2*cell,214,cell,COgheAudio.EffectsOn?COgheIcon.Sound:COgheIcon.Muted,"Sound",ToggleSound,true,COgheAudio.EffectsOn);
                art.Button(panel,"Resume",new Rect(24,h-82,w-48,58),COgheIcon.Play,Resume,"Resume",true);ClosePopup(panel,w,Resume);
                if(COgheTestTools.LevelSelect&&!home)art.Button(panel,"Test levels",new Rect(24,h-138,w-48,48),COgheIcon.Overview,()=>ShowPopup(COgheProductPopup.Levels),"Levels (test)");
            }
            else if(Popup==COgheProductPopup.Levels)LevelsPopup(panel,w,h);
            else if(Popup==COgheProductPopup.Collection&&Game.HomeRoom!=null)ItemsPopup(panel,w,h);
            else if(Popup==COgheProductPopup.Help)
            {
                PopupTitle(panel,w,"How to play");ClosePopup(panel,w,()=>ShowPopup(COgheProductPopup.Pause));
                HelpRow(panel,w,99,COgheIcon.Tap,"Tap to guide","Choose a surface or handle.");
                HelpRow(panel,w,184,COgheIcon.Drag,"Drag to look","See another side of the box.");
                HelpRow(panel,w,269,COgheIcon.Pinch,"Pinch to zoom","Bring the details closer.");
                art.Button(panel,"Got it",new Rect(24,h-80,w-48,56),COgheIcon.Check,()=>ShowPopup(COgheProductPopup.Pause),"Got it",true);
            }
            else
            {
                string title="Leave this puzzle?",message="Your level is saved.\nThis attempt will restart.";
                switch(Popup)
                {
                    case COgheProductPopup.Restart:title="Restart this puzzle?";message="Start this level again.";break;
                    case COgheProductPopup.Failure:title="Let's try again";message=Game.Failure==VenomCampaign.MergeFailure?"Merge every part before\ngoing through the exit.":"Guide COghe safely to the exit.";break;
                    case COgheProductPopup.Locked:title="A home for COghe";message="Complete level 10\nto unlock Home.";break;
                    case COgheProductPopup.Collection:title="Collection";message="Your first home is ready.\nMore furnishings are coming.";break;
                }
                PopupTitle(panel,w,title);art.Label(panel,"Message",message,new Rect(24,93,w-48,64),14,COgheUIArt.Muted);
                if(Popup==COgheProductPopup.Locked||Popup==COgheProductPopup.Collection)
                    art.Button(panel,"Got it",new Rect(24,h-80,w-48,56),COgheIcon.Check,Resume,"Got it",true);
                else
                {
                    art.Button(panel,"Cancel",new Rect(24,h-80,(w-60)*.5f,56),COgheIcon.Close,()=>{if(Popup==COgheProductPopup.Failure)ShowMenu();else ShowPopup(COgheProductPopup.Pause);});
                    art.Button(panel,"Confirm",new Rect(w*.5f+6,h-80,(w-60)*.5f,56),COgheIcon.Check,Confirm,null,true);
                }
            }
        }
        private void PopupTitle(Transform p,float w,string s)=>art.Label(p,"Title",s,new Rect(24,27,w-48,42),23);
        private void ClosePopup(Transform p,float w,System.Action action)
        {var b=art.Button(p,"Close",new Rect(w-51,4,48,48),COgheIcon.Close,action);b.image.color=Color.clear;var shadow=p.Find("Close shadow");if(shadow!=null)shadow.gameObject.SetActive(false);}
        private void Tile(Transform p,float x,float y,float w,COgheIcon icon,string label,System.Action action,bool visible=true,bool? on=null)
        {
            if(!visible)return;
            var b=art.Button(p,label,new Rect(x+(w-58)*.5f,y,58,55),icon,action,null,false,on??false);
            if(on.HasValue)art.Icon(b.transform,on.Value?COgheIcon.Check:COgheIcon.Close,new Rect(39,37,15,15),COgheUIArt.Teal);
            if(label=="Music"&&on==false){var line=art.Box(b.transform,"Muted slash",new Rect(13,26,34,2),COgheUIArt.Muted);line.rectTransform.localRotation=Quaternion.Euler(0,0,45);}
            art.Label(p,label+" caption",label,new Rect(x,y+64,w,24),11);
        }
        private void HelpRow(Transform p,float w,float y,COgheIcon icon,string title,string hint)
        {
            art.Box(p,"Gesture tile",new Rect(24,y,57,57),COgheUIArt.Mint);art.Icon(p,icon,new Rect(35,y+11,35,35),COgheUIArt.Teal);
            art.Label(p,"Gesture",title,new Rect(100,y-2,w-120,27),14,null,TextAnchor.MiddleLeft);
            art.Label(p,"Hint",hint,new Rect(100,y+26,w-120,38),11,COgheUIArt.Muted,TextAnchor.UpperLeft);
        }
    }
}
