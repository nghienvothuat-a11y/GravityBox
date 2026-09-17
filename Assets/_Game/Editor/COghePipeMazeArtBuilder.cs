using System.Collections.Generic;
using GravityBox.Venom;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using Object=UnityEngine.Object;

namespace GravityBox.Editor
{
    public static partial class COgheDayLabBuilder
    {
        [MenuItem("Gravity Box/COghe/Rebuild Day Lab · Pipe Maze 15")]
        public static void RebuildPipeMazePresentation()
        {
            var scene=EditorSceneManager.OpenScene(VenomCampaignBuilder.Folder+"/VenomOrigin15.unity");
            ApplyExpansionLevel(Object.FindFirstObjectByType<VenomCampaign>());
            EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            Debug.Log("COGHE PIPE ART SUCCESS: level 15 presentation rebuilt.");
        }

        private static void BuildPipeMazeArt(VenomCampaign game,Transform art)
        {
            var network=game.GetComponent<VenomLevelController>().Apparatus.GetComponentInChildren<COgheTubeNetwork>();
            if(network==null)return;
            var bore=Glass("Maze teal bore",.24f,0,false);
            bore.SetColor("_BaseColor",new Color(.035f,.38f,.34f,.24f));
            var chamber=Glass("Maze clear junction",.14f,0,false);
            chamber.SetColor("_BaseColor",new Color(.055f,.43f,.39f,.14f));
            var copper=Lit("Maze copper spine",new Color(.64f,.31f,.13f),.38f,.48f);
            foreach(var renderer in network.GetComponentsInChildren<MeshRenderer>())renderer.sharedMaterial=chamber;
            // Trace the actual trimmed bore mesh. Decorative ribs and collars
            // have no collider, graph nodes or input interception.
            foreach(var edge in network.Edges)
            {
                if(edge.Filter==null)continue;
                edge.Filter.GetComponent<MeshRenderer>().sharedMaterial=bore;
                var source=edge.Filter.sharedMesh.vertices;
                const int sides=16;int rings=source.Length/sides;
                if(rings<2)continue;
                var centres=new Vector3[rings];
                for(int r=0;r<rings;r++)for(int s=0;s<sides;s++)centres[r]+=source[r*sides+s]/sides;
                Vector3 Local(Vector3 point)=>art.InverseTransformPoint(edge.Filter.transform.TransformPoint(point));
                var vertices=new List<Vector3>();var triangles=new List<int>();
                for(int rib=0;rib<4;rib++)
                {
                    int start=vertices.Count;
                    for(int r=0;r<rings;r++)
                    {
                        Vector3 radial=(source[r*sides+rib*4]-centres[r]).normalized;
                        Vector3 tangent=(centres[Mathf.Min(r+1,rings-1)]-centres[Mathf.Max(0,r-1)]).normalized;
                        Vector3 across=Vector3.Cross(tangent,radial).normalized*.0012f;
                        Vector3 at=centres[r]+radial*(network.Radius+.0008f);
                        vertices.Add(Local(at-across));vertices.Add(Local(at+across));
                        if(r==0)continue;
                        int a=start+(r-1)*2,b=a+1,c=a+2,d=a+3;
                        triangles.AddRange(new[]{a,c,b,b,c,d});
                    }
                }
                PipeArtMesh(art,edge.Name+" copper ribs",vertices,triangles,copper);
                foreach(int r in new[]{0,rings-1})
                {
                    Vector3 axis=(centres[Mathf.Min(r+1,rings-1)]-centres[Mathf.Max(0,r-1)]).normalized;
                    Vector3 worldAxis=art.InverseTransformDirection(edge.Filter.transform.TransformDirection(axis));
                    PipeCollar(art,Local(centres[r]),worldAxis,network.Radius+.0018f,.0033f,ivory);
                    PipeCollar(art,Local(centres[r]+axis*(r==0?.005f:-.005f)),worldAxis,network.Radius+.001f,.0015f,copper);
                }
            }
            EditorUtility.SetDirty(bore);EditorUtility.SetDirty(chamber);
        }

        private static void PipeCollar(Transform parent,Vector3 centre,Vector3 axis,float radius,float width,Material material)
        {
            const int around=32,cross=6;
            Vector3 u=Vector3.Cross(axis,Mathf.Abs(Vector3.Dot(axis,Vector3.up))>.9f?Vector3.right:Vector3.up).normalized;
            Vector3 v=Vector3.Cross(axis,u).normalized;
            var vertices=new List<Vector3>();var triangles=new List<int>();
            for(int a=0;a<around;a++)for(int b=0;b<cross;b++)
            {
                float angle=a*Mathf.PI*2/around,section=b*Mathf.PI*2/cross;
                Vector3 radial=u*Mathf.Cos(angle)+v*Mathf.Sin(angle);
                vertices.Add(centre+radial*(radius+Mathf.Cos(section)*width)+axis*Mathf.Sin(section)*width);
                int i=a*cross+b,j=((a+1)%around)*cross+b,k=a*cross+(b+1)%cross,l=((a+1)%around)*cross+(b+1)%cross;
                triangles.AddRange(new[]{i,j,k,k,j,l});
            }
            PipeArtMesh(parent,"Porcelain pipe coupling",vertices,triangles,material);
        }

        private static void PipeArtMesh(Transform parent,string name,List<Vector3> vertices,List<int> triangles,Material material)
        {
            var mesh=new Mesh{name=name};mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
            var go=Child(parent,name);go.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;
            var renderer=go.gameObject.AddComponent<MeshRenderer>();renderer.sharedMaterial=material;renderer.shadowCastingMode=ShadowCastingMode.Off;
        }
    }
}
