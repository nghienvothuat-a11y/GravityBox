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
            selectionMaterial = new Material(level.LeftPad.Light.sharedMaterial);
            selectionMaterial.SetColor("_BaseColor",new Color(.25f,.88f,.67f)); selection.sharedMaterial = selectionMaterial;
            RefreshSelection();
        }
        private void LateUpdate()
        {
            if (level != null && level.DirectControl) FrameChamber(Screen.width,Screen.height);
            RefreshSelection();
        }
        public void FrameChamber(int width, int height)
        {
            if (!level.DirectControl) return;
            float scale = width/540f;
            float bottom = 282*scale, top = height-205*scale;
            float available = Mathf.Max(80*scale,top-bottom);
            Vector3 right = level.View.transform.right, up = level.View.transform.up;
            float minX = float.PositiveInfinity, minY = minX, maxX = float.NegativeInfinity, maxY = maxX;
            for (int x=-1;x<=1;x+=2) for (int y=0;y<=1;y++) for (int z=-1;z<=1;z+=2)
            {
                Vector3 p = level.Rotation.transform.TransformVector(new Vector3(x*.268f,y==0?-.076f:.181f,z*.33f));
                float px = Vector3.Dot(p,right), py = Vector3.Dot(p,up);
                minX = Mathf.Min(minX,px); maxX = Mathf.Max(maxX,px); minY = Mathf.Min(minY,py); maxY = Mathf.Max(maxY,py);
            }
            float size = Mathf.Max((maxY-minY)*.5f*height/available,(maxX-minX)*.5f*height/(width*.9f));
            Vector3 centre = level.Rotation.transform.position+right*((minX+maxX)*.5f)+up*((minY+maxY)*.5f);
            level.View.orthographicSize = size;
            level.View.transform.position = centre-level.View.transform.forward*1.3f-up*(((bottom+top)/height-1f)*size);
        }
        public void RefreshSelection()
        {
            if (selection == null) return;
            var chosen = level.Locomotion.Selected;
            selection.enabled = chosen != null && !level.Completed;
            if (!selection.enabled) return;
            float radius = .024f;
            for (int i = 0; i < 32; i++) if (!level.Organism.Escaped[i] && level.Organism.Groups[i] == chosen.Group)
                radius = Mathf.Max(radius,Vector3.ProjectOnPlane(level.Organism.Bodies[i].transform.position-chosen.Centre,Vector3.up).magnitude+.016f);
            Vector3 centre = chosen.Centre; centre.y = level.FloorBoundary.Top+.001f;
            for (int i = 0; i < 64; i++) selection.SetPosition(i,centre+new Vector3(Mathf.Cos(i*Mathf.PI/32)*radius,0,Mathf.Sin(i*Mathf.PI/32)*radius));
        }
        private void OnGUI()
        {
            if (level == null || level.Organism == null) return;
            float scale = Screen.width / 540f, h = Screen.height / scale;
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scale,scale,1));
            if (small == null)
            {
                small = new GUIStyle(GUI.skin.label) { fontSize = 12, normal = { textColor = new Color(.45f,.68f,.67f) } };
                title = new GUIStyle(small) { fontSize = 27, fontStyle = FontStyle.Bold, normal = { textColor = new Color(.88f,.95f,.89f) } };
                body = new GUIStyle(small) { fontSize = 15, wordWrap = true, normal = { textColor = new Color(.76f,.83f,.81f) } };
                button = new GUIStyle(GUI.skin.button) { fontSize = 13, fontStyle = FontStyle.Bold, padding = new RectOffset(4,4,4,4) };
                badge = new GUIStyle(small) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, normal = { textColor = new Color(.64f,1,.82f) } };
            }
            GUI.Label(new Rect(30,24,480,24), "V E N O M     /     L I V I N G   M A T T E R", small);
            string[] modes = { "01 · NGHIÊNG HỘP", "02 · CHỌN PHẦN", "03 · ĐI THEO" };
            for (int i = 1; i <= 3; i++)
            {
                GUI.backgroundColor = level.LevelNumber == i ? new Color(.3f,.85f,.62f) : new Color(.25f,.35f,.33f);
                if (GUI.Button(new Rect(30+(i-1)*164,58,152,34),modes[i-1],button) && level.LevelNumber != i) level.LoadExperiment(i);
            }
            GUI.backgroundColor = Color.white;
            string name = level.LevelNumber == 2 ? "02   Hai phần, một kế hoạch" : level.LevelNumber == 3 ? "03   Tìm về chủ thể" : "01   Một cơ thể, hai ý chí";
            string hint = level.LevelNumber == 2 ? "Chạm chọn một phần, giữ–kéo để bò.\nĐặt hai phần lên hai nút sáng để mở cửa." : level.LevelNumber == 3 ?
                "Điều khiển phần lớn nhất bằng giữ–kéo.\nPhần nhỏ chờ 3 giây rồi tìm đường về với bạn." : "Nghiêng hộp. Để sinh vật chảy qua lưỡi chém.\nHai phần giữ hai nút, rồi tìm về với nhau.";
            GUI.Label(new Rect(30,104,490,44),name,title);
            GUI.Label(new Rect(30,150,480,48),hint,body);
            var matter = level.Organism;
            string state = level.Lost ? "VẬT CHẤT RA SAI ĐƯỜNG — THỬ LẠI" : level.Completed ? "TOÀN BỘ SINH VẬT ĐÃ THOÁT" : level.Paused ? "TẠM DỪNG" :
                level.GateLatched ? "CỬA ĐÃ MỞ · ĐƯA TẤT CẢ TỚI LỖ TRÒN" :
                matter.CutCount == 0 ? "QUA LƯỠI CHÉM ĐỂ TÁCH CƠ THỂ" : level.LevelNumber == 3 ? "DẪN PHẦN NHỎ VỀ · NHẬP LẠI ĐỂ MỞ CỬA" : "HAI PHẦN · CÙNG GIỮ HAI NÚT SÁNG";
            if (level.DirectControl)
            {
                int index = 0, choose = -1;
                foreach (var fragment in level.Locomotion.Fragments)
                {
                    Vector3 p = level.View.WorldToScreenPoint(fragment.Centre);
                    string label = fragment.Selected ? (level.LevelNumber == 3 ? "CHỦ THỂ" : "ĐANG CHỌN") :
                        level.LevelNumber == 2 ? "ĐANG BÁM" : fragment.WaitRemaining > 0 ? $"CHỜ {fragment.WaitRemaining:0.0}s" : fragment.Blocked ? "CHỜ LỐI MỞ" : "ĐANG TÌM VỀ";
                    if (p.z > 0) GUI.Label(new Rect(p.x/scale-70,(Screen.height-p.y)/scale-55,140,24),label,badge);
                    if (index < 4)
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
                    Vector2 a = input.StickOrigin/scale, b = input.StickPosition/scale;
                    GUI.Label(new Rect(a.x-18,h-a.y-18,36,36),"○",title);
                    GUI.Label(new Rect(b.x-12,h-b.y-12,24,24),"●",body);
                }
            }
            GUI.Label(new Rect(30,h-213,480,42),state,body);
            string puzzle = level.LevelNumber == 3 ? (level.GateLatched ? "ĐÃ HỢP THỂ" : "NHẬP LẠI ĐỂ MỞ CỬA") :
                level.GateLatched ? "CỬA ĐÃ GIỮ MỞ" : "NÚT " + (level.LeftPad.Pressed ? "●" : "○") + " / " + (level.RightPad.Pressed ? "●" : "○");
            GUI.Label(new Rect(30,h-158,480,24),$"{matter.FragmentCount:00} PHẦN  ·  THOÁT {matter.EscapedCount*100/32}%  ·  {puzzle}",small);
            GUI.backgroundColor = new Color(.36f,.65f,.56f);
            if (GUI.Button(new Rect(30,h-108,230,48),"THỬ LẠI  /  R",button)) level.ResetExperiment();
            if (GUI.Button(new Rect(280,h-108,230,48),level.Paused ? "TIẾP TỤC" : "TẠM DỪNG  /  P",button)) level.TogglePause();
            GUI.backgroundColor = Color.white;
            GUI.Label(new Rect(30,h-44,480,24),level.DirectControl ? "GIỮ–KÉO / WASD ĐỂ BÒ  ·  1–3 CHỌN MÀN" : "KÉO ĐỂ NGHIÊNG  ·  1–3 CHỌN MÀN",small);
        }
        private void OnDestroy() { if (selectionMaterial != null) Destroy(selectionMaterial); }
    }
}
