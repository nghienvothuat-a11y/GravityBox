using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>Reads rack travel; never supplies forces, progress or completion.</summary>
    public sealed class COgheTapGatePresentation : MonoBehaviour
    {
        public COgheRailSlider Rack;
        public Transform Pinion;
        public float PinionRadius = .05f;
        public Transform[] SpringCoils;
        private Quaternion restRotation;
        private Vector3[] coilRest;
        private void Awake()
        {
            restRotation = Pinion.localRotation;
            coilRest = new Vector3[SpringCoils.Length];
            for (int i = 0; i < coilRest.Length; i++) coilRest[i] = SpringCoils[i].localPosition;
        }
        private void LateUpdate()
        {
            if (Rack == null || coilRest == null) return;
            Pinion.localRotation = restRotation * Quaternion.AngleAxis(Rack.Position / PinionRadius * Mathf.Rad2Deg, Vector3.forward);
            for (int i = 0; i < coilRest.Length; i++)
                SpringCoils[i].localPosition = coilRest[i] + Vector3.up * Rack.Position * (1f - i / (float)coilRest.Length) * .55f;
        }
    }
}
