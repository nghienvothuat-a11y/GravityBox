using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>Opening-room guidance reads actual progress; it never issues commands or unlocks anything.</summary>
    public sealed class COgheTapLesson : MonoBehaviour
    {
        public enum LessonKind { OneGear, Reverse, Chain, Split, Cooperate, Retrieve, Selector, SharedSelector, Reconnect, Boss }
        public LessonKind Lesson;
        private VenomCampaign game;
        private COgheTapRail a, b;
        private COgheTapPad pad;
        private COgheGuillotine knife;
        private void Awake()
        {
            game=GetComponent<VenomCampaign>();
            foreach(var rail in GetComponentsInChildren<COgheTapRail>())
            {if(rail.Label=="A")a=rail;else if(rail.Label=="B")b=rail;}
            pad=GetComponentInChildren<COgheTapPad>();knife=GetComponentInChildren<COgheGuillotine>();
        }
        public string StopStatus => a!=null&&a.HasStops ? (a.CurrentStop>=0 ? $"A · Chốt {a.CurrentStop+1} → {a.NextStop+1}" : $"A · Đích: chốt {a.NextStop+1}") : string.Empty;
        public string Hint
        {
            get
            {
                if(game==null||game.Owner==null||game.Home||game.Definition.Boss||game.Owner.Completed||game.Progress.Completed.Contains(game.Definition.Id))return string.Empty;
                if(knife!=null&&(knife.Phase==VenomCampaign.BladePhase.Warning||knife.Phase==VenomCampaign.BladePhase.Falling))
                    return "Dao đang tách COghe. Sau đó chọn từng phần để đi.";
                if(a!=null&&a.Busy||b!=null&&b.Busy)return "COghe đang thao tác. Cơ quan sẽ tự dừng ở chốt.";
                const string exit="Chạm lỗ sáng để đưa COghe ra ngoài.";
                const string lifting="Bộ truyền đang nâng cửa. Quan sát thanh răng.";
                switch(Lesson)
                {
                    case LessonKind.OneGear:
                        return a.AtEnd?(game.FinalExitAvailable?exit:lifting):"Chạm bánh răng A. COghe sẽ đưa nó vào ổ, rồi tự dừng.";
                    case LessonKind.Reverse:
                        if(a.AtEnd)return b.AtEnd?"Chạm A lần nữa để đưa bánh răng về chỗ cũ.":"Chốt B đã được nhả. Chạm B để bật nguồn.";
                        return b.AtEnd?(game.FinalExitAvailable?exit:lifting):"Chạm A để kéo ra. Quan sát chốt nối tới B.";
                    case LessonKind.Chain:
                        return a.AtEnd&&b.AtEnd?(game.FinalExitAvailable?exit:lifting):"Quan sát chốt và đường truyền. Tìm cách nối hai bánh răng.";
                    case LessonKind.Split:
                        if(game.Matter.CutCount==0)return "Chạm dao để tách COghe thành hai phần.";
                        return game.Matter.TotalFragmentCount>1?"Chọn từng phần, chạm sàn để đi. Đưa lại gần nhau để nhập.":exit;
                    case LessonKind.Cooperate:
                        if(game.FinalExitAvailable)
                            return game.Matter.TotalFragmentCount==1?exit:pad.Sensor.Active?"Đã chốt cửa. Chọn phần ở A, chạm A lần nữa để rời.":"Đưa hai phần lại gần nhau, hợp thể rồi thoát.";
                        if(game.Matter.TotalFragmentCount==1)return "Chạm dao để chia: cần hai phần cùng vận hành.";
                        if(!pad.Sensor.Active)return "Chọn một phần rồi chạm A để đứng giữ bàn đạp.";
                        return b.AtEnd?lifting:"A đang được giữ. Chọn phần khác rồi chạm bánh răng B.";
                    case LessonKind.Selector:
                        if(game.FinalExitAvailable)return exit;
                        return "Mỗi chạm đi tới một số. Chốt 2 nhả khóa B; chốt 4 nối bộ truyền.";
                    case LessonKind.Retrieve:
                    case LessonKind.SharedSelector:
                        if(game.FinalExitAvailable)return game.Matter.TotalFragmentCount==1?exit:"Cửa đã chốt. Đưa phần giữ bàn đạp trở về và hợp thể.";
                        return game.Definition.Lesson;
                    case LessonKind.Reconnect:
                        return game.FinalExitAvailable?exit:game.Definition.Lesson;
                    default:return game.Definition.Lesson;
                }
            }
        }
    }
}
