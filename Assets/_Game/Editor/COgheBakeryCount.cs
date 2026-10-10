using System.Linq;
using GravityBox.Venom;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace GravityBox.Editor
{
    /// <summary>Triangles drawn by a level scene (enabled, active mesh renderers), for the bakery cost comparison.</summary>
    public static class COgheBakeryCount
    {
        public static void CountN41()
        {
            EditorSceneManager.OpenScene(VenomCampaignBuilder.SpatialContentPath("N41"));long total=0,dress=0;int renderers=0;
            foreach(var r in Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))
            {
                if(!r.enabled||!r.gameObject.activeInHierarchy)continue;var mf=r.GetComponent<MeshFilter>();if(mf==null||mf.sharedMesh==null)continue;
                long t=0;for(int s=0;s<mf.sharedMesh.subMeshCount;s++)t+=mf.sharedMesh.GetIndexCount(s)/3;
                if(r.bounds.size.x>5)continue;   // the studio table
                total+=t;renderers++;if(r.transform.GetComponentsInParent<Transform>().Any(p=>p.name=="Bakery dress"||p.name.StartsWith("Bakery")||p.name.StartsWith("Tripo")))dress+=t;
            }
            Debug.Log($"BAKERY COUNT N41: {total} triangles in {renderers} renderers ({dress} in bakery pieces)");
        }
    }
}
