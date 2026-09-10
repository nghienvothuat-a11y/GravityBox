using UnityEngine;
namespace GravityBox.Venom
{
    public sealed class VenomHud : MonoBehaviour
    {
        private VenomLevelController level;
        private GUIStyle small, title, body, button, badge;
        private LineRenderer selection;
        private Material selectionMaterial;
        public void Initialize(VenomLevelController controller)
        {
            level = controller;
            if (!level.DirectControl) return;
            selection = new GameObject("Selected fragment contact ring",typeof(LineRenderer)).GetComponent<LineRenderer>();
            selection.transform.SetParent(transform,false); selection.useWorldSpace = true;
            selection.loop = true; selection.positionCount = 64; selection.widthMultiplier = .0012f;
            // Reuse a referenced material so player shader stripping cannot
            // remove the selection shader (Shader.Find alone is insufficient).
            selectionMaterial = new Material(level.IndicatorMaterial != null ? level.IndicatorMaterial : level.LeftPad.Light.sharedMaterial);
            selectionMaterial.SetColor("_BaseColor",new Color(.25f,.88f,.67f)); selection.sharedMaterial = selectionMaterial;
            RefreshSelection();
        }
        private void LateUpdate()
        {
            if (level != null && level.DirectControl) FrameChamber(Screen.width,Screen.height);
            RefreshSelection();
        }
        public void FrameChamber(int width, int height) => VenomCameraFraming.Frame(level,width,height);
        public void RefreshSelection()
        {
            if (selection == null) return;
            var chosen = level.Locomotion.Selected;
            selection.enabled = chosen != null && !level.Completed;
            if (!selection.enabled) return;
            Vector3 normal=level.WallCrawl?level.Climbing.Normal:Vector3.up;
            Vector3 centre=chosen.Centre;
            if(level.WallCrawl)
            {
                Vector3 sum=Vector3.zero;int count=0;
                for(int i=0;i<32;i++)if(level.Organism.TryGetSupport(i,out _,out var p,out var n) && Vector3.Dot(n,normal)>.9f){sum+=p;count++;}
                if(count==0){selection.enabled=false;return;}
                centre-=normal*Vector3.Dot(centre-sum/count,normal);centre+=normal*.001f;
            }
            else centre.y=level.FloorBoundary.Top+.001f;
            float radius=.024f;
            for(int i=0;i<32;i++)if(!level.Organism.Escaped[i] && level.Organism.Groups[i]==chosen.Group)
                radius=Mathf.Max(radius,Vector3.ProjectOnPlane(level.Organism.Bodies[i].position-chosen.Centre,normal).magnitude+.016f);
            Vector3 axis=Vector3.Cross(normal,level.View.transform.forward).normalized;
            if(axis.sqrMagnitude<.1f)axis=Vector3.Cross(normal,level.View.transform.up).normalized;
            Vector3 other=Vector3.Cross(normal,axis);
            for(int i=0;i<64;i++)selection.SetPosition(i,centre+(axis*Mathf.Cos(i*Mathf.PI/32)+other*Mathf.Sin(i*Mathf.PI/32))*radius);
        }
        private void OnGUI()
        {
            if (level == null || level.Organism == null) return;
            float scale = VenomCameraFraming.UiScale(level,Screen.width,Screen.height), h = Screen.height / scale;
            float offsetX=(Screen.width-540*scale)*.5f;
            GUI.matrix = Matrix4x4.TRS(new Vector3(offsetX,0,0), Quaternion.identity, new Vector3(scale,scale,1));
            if (small == null)
            {
                small = new GUIStyle(GUI.skin.label) { fontSize = 12, normal = { textColor = new Color(.45f,.68f,.67f) } };
                title = new GUIStyle(small) { fontSize = 27, fontStyle = FontStyle.Bold, normal = { textColor = new Color(.88f,.95f,.89f) } };
                body = new GUIStyle(small) { fontSize = 15, wordWrap = true, normal = { textColor = new Color(.76f,.83f,.81f) } };
                button = new GUIStyle(GUI.skin.button) { fontSize = 13, fontStyle = FontStyle.Bold, padding = new RectOffset(4,4,4,4) };
                badge = new GUIStyle(small) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, normal = { textColor = new Color(.64f,1,.82f) } };
            }
            GUI.Label(new Rect(30,24,480,24), "V E N O M     /     L I V I N G   M A T T E R", small);
            string[] modes = { "01 · NGHIÊNG", "02 · CHỌN", "03 · ĐI THEO", "04 · BÒ TƯỜNG" };
            for (int i = 1; i <= 4; i++)
            {
                GUI.backgroundColor = level.LevelNumber == i ? new Color(.3f,.85f,.62f) : new Color(.25f,.35f,.33f);
                if (GUI.Button(new Rect(30+(i-1)*122,58,112,34),modes[i-1],button) && level.LevelNumber != i) level.LoadExperiment(i);
            }
            GUI.backgroundColor = Color.white;
            string name = level.WallCrawl ? "04   Bò khắp sáu mặt" : level.LevelNumber == 2 ? "02   Hai phần, một kế hoạch" : level.LevelNumber == 3 ? "03   Tìm về chủ thể" : "01   Một cơ thể, hai ý chí";
            string hint = level.WallCrawl ? "Giữ–kéo để bò. Hai ngón / chuột phải xoay hộp.\nBám sàn, tường và trần. Tìm về lỗ tròn." : level.LevelNumber == 2 ? "Chạm chọn một phần, giữ–kéo để bò.\nĐặt hai phần lên hai nút sáng để mở cửa." : level.LevelNumber == 3 ?
                "Điều khiển phần lớn nhất bằng giữ–kéo.\nPhần nhỏ chờ 3 giây rồi tìm đường về với bạn." : "Nghiêng hộp. Để sinh vật chảy qua lưỡi chém.\nHai phần giữ hai nút, rồi tìm về với nhau.";
            GUI.Label(new Rect(30,104,490,44),name,title);
            GUI.Label(new Rect(30,150,480,48),hint,body);
            var matter = level.Organism;
            string state = level.Lost ? "VẬT CHẤT RA SAI ĐƯỜNG — THỬ LẠI" : level.Completed ? "TOÀN BỘ SINH VẬT ĐÃ THOÁT" : level.Paused ? "TẠM DỪNG" :
                level.WallCrawl ? $"ĐANG BÁM {level.Climbing.SurfaceName} · {level.Climbing.VisitedCount}/6 MẶT ĐÃ KHÁM PHÁ" :
                level.GateLatched ? "CỬA ĐÃ MỞ · ĐƯA TẤT CẢ TỚI LỖ TRÒN" :
                matter.CutCount == 0 ? "QUA LƯỠI CHÉM ĐỂ TÁCH CƠ THỂ" : level.LevelNumber == 3 ? "DẪN PHẦN NHỎ VỀ · NHẬP LẠI ĐỂ MỞ CỬA" : "HAI PHẦN · CÙNG GIỮ HAI NÚT SÁNG";
            if (level.DirectControl)
            {
                int index = 0, choose = -1;
                foreach (var fragment in level.Locomotion.Fragments)
                {
                    Vector3 p = level.View.WorldToScreenPoint(fragment.Centre);
                    string label = level.WallCrawl ? "BÁM · " + level.Climbing.SurfaceName : fragment.Selected ? (level.LevelNumber == 3 ? "CHỦ THỂ" : "ĐANG CHỌN") :
                        level.LevelNumber == 2 ? "ĐANG BÁM" : fragment.WaitRemaining > 0 ? $"CHỜ {fragment.WaitRemaining:0.0}s" : fragment.Blocked ? "CHỜ LỐI MỞ" : "ĐANG TÌM VỀ";
                    if (p.z > 0) GUI.Label(new Rect((p.x-offsetX)/scale-70,(Screen.height-p.y)/scale-55,140,24),label,badge);
                    if (index < 4 && !level.WallCrawl)
                    {
                        GUI.backgroundColor = fragment.Selected ? new Color(.3f,.85f,.62f) : new Color(.27f,.35f,.33f);
                        GUI.enabled = level.CanControl && level.LevelNumber == 2;
                        if (GUI.Button(new Rect(30+index*122,h-266,112,34),$"{(char)('A'+index)} · {Mathf.RoundToInt(fragment.Count*100f/32)}%",button)) choose = fragment.Anchor;
                        GUI.enabled = true;
                    }
                    index++;
                }
                // Selection refreshes the connectivity snapshot; apply it after
                // enumerating the buttons so this GUI event cannot invalidate it.
                if (choose >= 0) level.Locomotion.Select(choose);
                GUI.backgroundColor = Color.white;
                var input = level.GetComponent<VenomInput>();
                if (input.Holding)
                {
                    Vector2 offset=new Vector2(offsetX,0);
                    Vector2 a = (input.StickOrigin-offset)/scale, b = (input.StickPosition-offset)/scale;
                    GUI.Label(new Rect(a.x-18,h-a.y-18,36,36),"○",title);
                    GUI.Label(new Rect(b.x-12,h-b.y-12,24,24),"●",body);
                }
            }
            GUI.Label(new Rect(30,h-213,480,42),state,body);
            string puzzle = level.WallCrawl ? "BÒ TỚI LỖ TRÒN ĐỂ THOÁT" : level.LevelNumber == 3 ? (level.GateLatched ? "ĐÃ HỢP THỂ" : "NHẬP LẠI ĐỂ MỞ CỬA") :
                level.GateLatched ? "CỬA ĐÃ GIỮ MỞ" : "NÚT " + (level.LeftPad.Pressed ? "●" : "○") + " / " + (level.RightPad.Pressed ? "●" : "○");
            GUI.Label(new Rect(30,h-158,480,24),$"{matter.FragmentCount:00} PHẦN  ·  THOÁT {matter.EscapedCount*100/32}%  ·  {puzzle}",small);
            GUI.backgroundColor = new Color(.36f,.65f,.56f);
            if (GUI.Button(new Rect(30,h-108,230,48),"THỬ LẠI  /  R",button)) level.ResetExperiment();
            if (GUI.Button(new Rect(280,h-108,230,48),level.Paused ? "TIẾP TỤC" : "TẠM DỪNG  /  P",button)) level.TogglePause();
            GUI.backgroundColor = Color.white;
            GUI.Label(new Rect(30,h-44,480,24),level.WallCrawl ? "CHUỘT TRÁI / WASD: BÒ · CHUỘT PHẢI: XOAY · 1–4" : level.DirectControl ? "GIỮ–KÉO / WASD ĐỂ BÒ  ·  1–4 CHỌN MÀN" : "KÉO ĐỂ NGHIÊNG  ·  1–4 CHỌN MÀN",small);
        }
        private void OnDestroy() { if (selectionMaterial != null) Destroy(selectionMaterial); }
    }
}
