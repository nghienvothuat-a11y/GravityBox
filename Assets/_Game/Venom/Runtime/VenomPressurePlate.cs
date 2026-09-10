using UnityEngine;
namespace GravityBox.Venom
{
    public sealed class VenomPressurePlate : MonoBehaviour
    {
        public Rigidbody Body; public Vector3 RestLocal; public Renderer Light;
        public float Load { get; private set; }
        public int Group { get; private set; } = -1;
        public bool Pressed => Load >= .009f;
        private readonly float[] contacts = new float[CohesiveOrganism.ParticleCount];
        private readonly float[] groupLoads = new float[CohesiveOrganism.ParticleCount];
        private MaterialPropertyBlock block;
        public void Touch(int index, float time) => contacts[index] = time;
        public void ResetPlate(Transform box)
        {
            if (block == null) block = new MaterialPropertyBlock();
            Body.position = box.TransformPoint(RestLocal); Body.rotation = box.rotation;
            Body.linearVelocity = Body.angularVelocity = Vector3.zero;
            for (int i = 0; i < contacts.Length; i++) contacts[i] = -100;
            Load = 0; Group = -1;
        }
        public void Step(CohesiveOrganism organism, Transform box)
        {
            Load = 0; Group = -1;
            System.Array.Clear(groupLoads, 0, groupLoads.Length);
            for (int i = 0; i < contacts.Length; i++) if (!organism.Escaped[i] && organism.SimulationTime - contacts[i] < .065f)
                groupLoads[organism.Groups[i]] += organism.Profile.ParticleMass;
            // One fragment must supply the threshold on its own. A tiny second
            // fragment cannot relabel a plate mainly occupied by the first one.
            for (int group = 0; group < groupLoads.Length; group++)
                if (groupLoads[group] > Load) { Load = groupLoads[group]; Group = group; }
            Vector3 axis = box.up;
            float displacement = Vector3.Dot(Body.position - box.TransformPoint(RestLocal), axis);
            float relative = Vector3.Dot(Body.linearVelocity - box.GetComponent<Rigidbody>().GetPointVelocity(Body.position), axis);
            Body.AddForce(Vector3.down * 9.81f, ForceMode.Acceleration);
            Body.AddForce(axis * (-displacement * 75 - relative * .28f));
            Color color = Pressed ? new Color(.26f, 1, .65f) : new Color(.1f,.35f,.4f);
            block.SetColor("_BaseColor", color); block.SetColor("_EmissionColor", color * (Pressed ? 1.5f : .3f)); Light.SetPropertyBlock(block);
        }
    }
}
