using UnityEngine;

namespace GravityBox.Venom
{
    public sealed class VenomJourneyHud
    {
        private readonly VenomLevelController level;
        private GUIStyle title,text,small,button,badge;
        public VenomJourneyHud(VenomLevelController owner){level=owner;}
        public void Draw()
        {
            var journey=level.Journey;
            float scale=VenomCameraFraming.UiScale(level,Screen.width,Screen.height),h=Screen.height/scale;
            float ox=(Screen.width-540*scale)*.5f;
            GUI.matrix=Matrix4x4.TRS(new Vector3(ox,0,0),Quaternion.identity,Vector3.one*scale);
            if(title==null)
            {
                small=new GUIStyle(GUI.skin.label){fontSize=12,wordWrap=true,normal={textColor=new Color(.51f,.73f,.7f)}};
                title=new GUIStyle(small){fontSize=25,fontStyle=FontStyle.Bold,normal={textColor=new Color(.9f,.96f,.9f)}};
                text=new GUIStyle(small){fontSize=15,normal={textColor=new Color(.78f,.88f,.85f)}};
                button=new GUIStyle(GUI.skin.button){fontSize=13,fontStyle=FontStyle.Bold,padding=new RectOffset(4,4,4,4)};
                badge=new GUIStyle(small){alignment=TextAnchor.MiddleCenter,fontStyle=FontStyle.Bold};
            }
            GUI.Label(new Rect(28,22,490,24),"V E N O M     /     M Ộ T   N G Ư Ờ I   B Ạ N",small);
            for(int i=1;i<=5;i++)
            {
                GUI.backgroundColor=i==journey.Chapter?new Color(.28f,.8f,.59f):new Color(.24f,.35f,.33f);
                string mark=(journey.Progress.Completed&(1<<(i-1)))!=0?" ✓":"";
                if(GUI.Button(new Rect(28+(i-1)*99,57,89,34),$"{i:00}"+mark,button)&&journey.Chapter!=i)journey.Load(i);
            }
            GUI.backgroundColor=Color.white;
            GUI.Label(new Rect(28,105,490,36),journey.Title,title);
            GUI.Label(new Rect(28,148,490,48),journey.Hint,text);
            int n=0,select=-1;
            foreach(var f in level.Locomotion.Fragments)
            {
                var task=journey.TaskFor(f.Anchor);
                Vector3 p=level.View.WorldToScreenPoint(f.Centre);
                if(p.z>0&&!level.FollowView.Zoomed)
                    GUI.Label(new Rect((p.x-ox)/scale-80,(Screen.height-p.y)/scale-53,160,36),task?.Feedback??"CHỜ BẠN CHỈ",badge);
                if(n<4)
                {
                    GUI.backgroundColor=f.Selected?new Color(.3f,.8f,.62f):new Color(.27f,.38f,.37f);
                    GUI.enabled=level.CanControl&&!journey.Cutting;
                    string label=$"{(char)('A'+n)} · {f.Count*3} g"+(f.Selected?" ●":"");
                    if(GUI.Button(new Rect(28+n*124,h-278,114,34),label,button))select=f.Anchor;
                }
                n++;
            }
            if(select>=0)journey.Select(select);
            GUI.enabled=true;GUI.backgroundColor=Color.white;
            string state=level.Completed?"ĐÃ THOÁT TRỌN VẸN · CHÚNG MÌNH LÀM ĐƯỢC RỒI!":level.Lost?"BẠN NHỎ BỊ KẸT · THỬ LẠI":level.Paused?"TẠM DỪNG":journey.Status;
            GUI.Label(new Rect(28,h-235,488,36),state,text);
            string stats=$"THOÁT {level.Organism.EscapedCount*100/32}%";
            if(journey.PadA!=null)stats+=$"    A: {journey.MassA*1000:0}/{journey.RequiredA*1000:0} g";
            if(journey.PadB!=null)stats+=$"    B: {journey.MassB*1000:0}/{journey.RequiredB*1000:0} g";
            if(journey.Chapter==3&&!journey.Solved)stats+=$"    GIỮ {Mathf.Clamp01(journey.HoldProgress/1.8f)*100:0}%";
            GUI.Label(new Rect(28,h-193,490,21),stats,small);
            GUI.Label(new Rect(28,h-170,490,30),journey.Learning,small);
            GUI.backgroundColor=new Color(.27f,.52f,.45f);
            if(GUI.Button(new Rect(28,h-130,152,34),level.FollowView.Zoomed?"THU XA / Z":"PHÓNG GẦN / Z",button))level.ToggleZoom();
            GUI.enabled=level.CanControl&&!journey.Cutting;
            if(journey.Chapter>=4&&GUI.Button(new Rect(197,h-130,152,34),"GỌI VỀ HỢP THỂ",button))journey.CallTogether();
            if(level.GateLatched)
            {if(GUI.Button(new Rect(366,h-130,152,34),"CÙNG RA NGOÀI",button))journey.GuideAllOut();}
            else if(journey.Chapter==5)
            {if(GUI.Button(new Rect(366,h-130,152,34),journey.CutBias>0?"DAO: CẮT LỆCH":"DAO: CẮT GIỮA",button))journey.SetCutBias(journey.CutBias>0?0:.018f);}
            GUI.enabled=true;
            if(GUI.Button(new Rect(28,h-85,235,40),"THỬ LẠI / R",button))level.ResetExperiment();
            if(GUI.Button(new Rect(282,h-85,236,40),level.Paused?"TIẾP TỤC":"TẠM DỪNG / P",button))level.TogglePause();
            GUI.backgroundColor=Color.white;
            GUI.Label(new Rect(28,h-34,490,25),"CHẠM: GIAO VIỆC · KÉO: XOAY HỘP · 1–5: CHỌN MÀN",small);
        }
    }
}
