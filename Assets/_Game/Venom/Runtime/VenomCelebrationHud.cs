using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>Leave the middle of the screen entirely to the creature.</summary>
    public sealed class VenomCelebrationHud
    {
        private readonly VenomLevelController level;
        private GUIStyle title, note, button;
        public VenomCelebrationHud(VenomLevelController owner) { level=owner; }
        public void Draw()
        {
            float scale=Mathf.Min(Screen.width/540f,Screen.height/960f);
            float height=Screen.height/scale,offset=(Screen.width-540*scale)*.5f;
            GUI.matrix=Matrix4x4.TRS(new Vector3(offset,0,0),Quaternion.identity,Vector3.one*scale);
            if(title==null)
            {
                title=new GUIStyle(GUI.skin.label){fontSize=27,fontStyle=FontStyle.Bold,alignment=TextAnchor.MiddleCenter,normal={textColor=new Color(.88f,.96f,.91f)}};
                note=new GUIStyle(title){fontSize=15,fontStyle=FontStyle.Normal,wordWrap=true,normal={textColor=new Color(.52f,.79f,.70f)}};
                button=new GUIStyle(GUI.skin.button){fontSize=14};
            }
            GUI.Label(new Rect(15,66,510,42),"CHÚNG MÌNH LÀM ĐƯỢC RỒI!",title);
            GUI.Label(new Rect(35,113,470,28),"Bạn nhỏ đã thoát trọn vẹn.",note);
            bool advance=level.Journey!=null&&level.Journey.AutoAdvance&&level.Journey.Chapter<5;
            GUI.Label(new Rect(35,height-165,470,50),level.Paused?"TẠM DỪNG":advance?"Cùng vui một chút, rồi khám phá hộp tiếp theo.":"Một điệu nhảy dành cho bạn!",note);
            GUI.backgroundColor=new Color(.28f,.50f,.43f);
            if(GUI.Button(new Rect(35,height-99,225,40),"THỬ LẠI / R",button))level.ResetExperiment();
            if(GUI.Button(new Rect(280,height-99,225,40),level.Paused?"TIẾP TỤC":"TẠM DỪNG / P",button))level.TogglePause();
            GUI.backgroundColor=Color.white;
        }
    }
}
