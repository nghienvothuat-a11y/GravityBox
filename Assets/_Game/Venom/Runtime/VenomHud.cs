using UnityEngine;
namespace GravityBox.Venom
{
    public sealed class VenomHud : MonoBehaviour
    {
        private VenomLevelController level;
        private GUIStyle small, title, body, button;
        public void Initialize(VenomLevelController controller) => level = controller;
        private void OnGUI()
        {
            if (level == null || level.Organism == null) return;
            float scale = Screen.width / 540f, h = Screen.height / scale;
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scale,scale,1));
            if (small == null)
            {
                small = new GUIStyle(GUI.skin.label) { fontSize = 12, normal = { textColor = new Color(.45f,.68f,.67f) } };
                title = new GUIStyle(small) { fontSize = 30, fontStyle = FontStyle.Bold, normal = { textColor = new Color(.88f,.95f,.89f) } };
                body = new GUIStyle(small) { fontSize = 16, wordWrap = true, normal = { textColor = new Color(.76f,.83f,.81f) } };
                button = new GUIStyle(GUI.skin.button) { fontSize = 14, fontStyle = FontStyle.Bold, padding = new RectOffset(6,6,6,6) };
            }
            GUI.Label(new Rect(30,32,480,24), "V E N O M     /     L I V I N G   M A T T E R", small);
            GUI.Label(new Rect(30,66,490,46), "01   Một cơ thể, hai ý chí", title);
            GUI.Label(new Rect(30,118,480,48), "Nghiêng hộp. Để sinh vật chảy qua lưỡi chém.\nHai phần giữ hai nút, rồi tìm về với nhau.", body);
            var matter = level.Organism;
            string state = level.Lost ? "VẬT CHẤT RA SAI ĐƯỜNG — THỬ LẠI" : level.Completed ? "TOÀN BỘ SINH VẬT ĐÃ THOÁT" : level.Paused ? "TẠM DỪNG" :
                level.GateLatched ? (matter.FragmentCount == 1 ? "ĐÃ NHẬP LẠI · ĐƯA ĐẾN LỖ TRÒN" : "ĐƯỜNG ĐÃ MỞ · ĐƯA CÁC PHẦN LẠI GẦN") :
                matter.CutCount > 0 ? "HAI PHẦN · CÙNG GIỮ HAI NÚT SÁNG" : "QUA LƯỠI CHÉM ĐỂ TÁCH CƠ THỂ";
            GUI.Label(new Rect(30,h-200,480,44),state, body);
            GUI.Label(new Rect(30,h-150,480,24), $"{matter.FragmentCount:00} PHẦN     ·     THOÁT {Mathf.RoundToInt(matter.EscapedCount*100f/CohesiveOrganism.ParticleCount)}%     ·     {(level.GateLatched ? "CỬA ĐÃ GIỮ MỞ" : "NÚT " + (level.LeftPad.Pressed ? "●" : "○") + " / " + (level.RightPad.Pressed ? "●" : "○"))}", small);
            GUI.backgroundColor = new Color(.36f,.65f,.56f);
            if (GUI.Button(new Rect(30,h-104,230,50), "THỬ LẠI  /  R", button)) level.ResetExperiment();
            if (GUI.Button(new Rect(280,h-104,230,50), level.Paused ? "TIẾP TỤC" : "TẠM DỪNG  /  P", button)) level.TogglePause();
            GUI.backgroundColor = Color.white;
            GUI.Label(new Rect(30,h-40,480,24), "KÉO ĐỂ NGHIÊNG     ·     THẢ TAY ĐỂ QUAN SÁT", small);
        }
    }
}
