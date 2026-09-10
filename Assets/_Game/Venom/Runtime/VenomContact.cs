using UnityEngine;
namespace GravityBox.Venom
{
    public sealed class VenomContact : MonoBehaviour
    {
        private CohesiveOrganism organism; private int index;
        public void Initialize(CohesiveOrganism owner, int particle) { organism = owner; index = particle; }
        private void OnCollisionStay(Collision hit) { for (int i = 0; i < hit.contactCount; i++) organism.Contact(index, hit.collider, hit.GetContact(i).normal); }
    }
}
