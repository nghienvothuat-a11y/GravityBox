using UnityEngine;
namespace GravityBox.Venom
{
    public sealed class VenomContact : MonoBehaviour
    {
        private CohesiveOrganism organism; private int index;
        public void Initialize(CohesiveOrganism owner, int particle) { organism = owner; index = particle; }
        private void OnCollisionEnter(Collision hit) => Observe(hit, false);
        private void OnCollisionStay(Collision hit) => Observe(hit, true);
        private void Observe(Collision hit, bool reportLoad)
        {
            for (int i = 0; i < hit.contactCount; i++)
            {
                ContactPoint contact = hit.GetContact(i);
                organism.Contact(index, hit.collider, contact.normal, contact.point, reportLoad);
            }
        }
    }
}
