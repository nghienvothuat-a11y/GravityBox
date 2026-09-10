using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace GravityBox.Venom
{
    // Compact-support metaballs, polygonised per connected fragment. This is only
    // the skin; PhysX particles and cohesive forces own all motion and contacts.
    public sealed class VenomSurface : MonoBehaviour
    {
        private CohesiveOrganism organism;
        private Mesh mesh; private MeshRenderer skin;
        private readonly List<Vector3> vertices = new List<Vector3>(20000), normals = new List<Vector3>(20000);
        private readonly List<int> triangles = new List<int>(30000);
        private float[] field = Array.Empty<float>(); private Vector3[] gradient = Array.Empty<Vector3>();
        private readonly Vector3[] corners = new Vector3[8], cornerNormals = new Vector3[8];
        private readonly float[] values = new float[8];
        private readonly int[] inside = new int[4], outside = new int[4];
        private readonly int[] seen = new int[CohesiveOrganism.ParticleCount];
        private readonly Vector3[] points = new Vector3[CohesiveOrganism.ParticleCount+6];
        private readonly float[] supports = new float[CohesiveOrganism.ParticleCount+6], weights = new float[CohesiveOrganism.ParticleCount+6];
        private readonly int[] particleIds = new int[CohesiveOrganism.ParticleCount];
        private readonly int[,] tetrahedra = { {0,5,1,6},{0,1,2,6},{0,2,3,6},{0,3,7,6},{0,7,4,6},{0,4,5,6} };
        private readonly Vector3Int[] offsets = { new Vector3Int(0,0,0),new Vector3Int(1,0,0),new Vector3Int(1,1,0),new Vector3Int(0,1,0),new Vector3Int(0,0,1),new Vector3Int(1,0,1),new Vector3Int(1,1,1),new Vector3Int(0,1,1) };
        private MaterialPropertyBlock block;
        private float nextRefresh;
        private VenomLifeAnimation life;
        public int VertexCount => mesh != null ? mesh.vertexCount : 0;

        public void Initialize(CohesiveOrganism source, VenomLevelController owner)
        {
            organism = source; block = new MaterialPropertyBlock();
            var go = new GameObject("Continuous wet skin",typeof(MeshFilter),typeof(MeshRenderer)); go.transform.SetParent(transform,false);
            mesh = new Mesh { name = "Living isosurface", indexFormat = IndexFormat.UInt32 }; mesh.MarkDynamic();
            go.GetComponent<MeshFilter>().sharedMesh = mesh; skin = go.GetComponent<MeshRenderer>(); skin.sharedMaterial = source.Profile.Skin;
            skin.shadowCastingMode = ShadowCastingMode.On;
            life = gameObject.AddComponent<VenomLifeAnimation>(); life.Initialize(source,owner);
            Rebuild();
        }
        private void LateUpdate()
        {
            if (organism == null || Time.unscaledTime < nextRefresh) return;
            nextRefresh = Time.unscaledTime + 1f/30; Rebuild();
            block.SetColor("_EmissionColor", new Color(.02f,.22f,.15f) * organism.FusionGlow * .2f);
            skin.SetPropertyBlock(block);
        }
        public void Rebuild(bool interpolate = true)
        {
            vertices.Clear(); normals.Clear(); triangles.Clear(); int seenCount = 0;
            life.BeginFrame();
            for (int i = 0; i < CohesiveOrganism.ParticleCount; i++)
            {
                int group = organism.Groups[i]; if (Array.IndexOf(seen,group,0,seenCount)>=0) continue;
                seen[seenCount++] = group; int count=0;
                for (int j=0;j<CohesiveOrganism.ParticleCount;j++)
                    if (organism.Groups[j]==group && organism.Bodies[j].position.sqrMagnitude < 9)
                    {
                        particleIds[count]=j;supports[count]=organism.Profile.SkinSupport;weights[count]=1;
                        points[count++]=transform.InverseTransformPoint(interpolate ? organism.Bodies[j].transform.position : organism.Bodies[j].position);
                    }
                if(count>0) BuildFragment(life.Decorate(points,supports,weights,particleIds,count));
            }
            mesh.Clear(); mesh.SetVertices(vertices); mesh.SetNormals(normals); mesh.SetTriangles(triangles,0); mesh.RecalculateBounds();
            life.EndFrame();
        }
        private void BuildFragment(int count)
        {
            float threshold=organism.Profile.SkinThreshold;
            Vector3 min=points[0]-Vector3.one*supports[0], max=points[0]+Vector3.one*supports[0];
            for(int i=1;i<count;i++){min=Vector3.Min(min,points[i]-Vector3.one*supports[i]);max=Vector3.Max(max,points[i]+Vector3.one*supports[i]);}
            Vector3 size=max-min; float cell=Mathf.Max(organism.Profile.MeshCell,Mathf.Max(size.x,size.y,size.z)/64);
            int nx=Mathf.CeilToInt(size.x/cell)+1, ny=Mathf.CeilToInt(size.y/cell)+1,nz=Mathf.CeilToInt(size.z/cell)+1;
            int length=nx*ny*nz;
            if(field.Length<length){field=new float[length];gradient=new Vector3[length];}
            Array.Clear(field,0,length);Array.Clear(gradient,0,length);
            for(int p=0;p<count;p++)
            {
                float support=supports[p],h2=support*support,weight=weights[p];
                Vector3 q=(points[p]-min)/cell;
                int reach=Mathf.CeilToInt(support/cell);
                for(int z=Mathf.Max(0,(int)q.z-reach);z<=Mathf.Min(nz-1,(int)q.z+reach);z++)
                for(int y=Mathf.Max(0,(int)q.y-reach);y<=Mathf.Min(ny-1,(int)q.y+reach);y++)
                for(int x=Mathf.Max(0,(int)q.x-reach);x<=Mathf.Min(nx-1,(int)q.x+reach);x++)
                {
                    Vector3 d=min+new Vector3(x,y,z)*cell-points[p];float s=1-d.sqrMagnitude/h2;if(s<=0)continue;
                    int index=x+nx*(y+ny*z);field[index]+=s*s*s*weight;gradient[index]+=d*(6*s*s*weight/h2);
                }
            }
            for(int z=0;z<nz-1;z++)for(int y=0;y<ny-1;y++)for(int x=0;x<nx-1;x++)
            {
                int above=0;
                for(int c=0;c<8;c++)
                {
                    Vector3Int o=offsets[c];int index=x+o.x+nx*(y+o.y+ny*(z+o.z));
                    values[c]=field[index];if(values[c]>=threshold)above++;
                    corners[c]=min+new Vector3(x+o.x,y+o.y,z+o.z)*cell;cornerNormals[c]=gradient[index];
                }
                if(above==0||above==8)continue;
                for(int t=0;t<6;t++)
                {
                    int ni=0,no=0;
                    for(int c=0;c<4;c++){int id=tetrahedra[t,c];if(values[id]>=threshold)inside[ni++]=id;else outside[no++]=id;}
                    if(ni==0||ni==4)continue;
                    if(ni==1){Edge(inside[0],outside[0],out var a,out var an);Edge(inside[0],outside[1],out var b,out var bn);Edge(inside[0],outside[2],out var c,out var cn);Triangle(a,b,c,an,bn,cn);}
                    else if(ni==3){Edge(outside[0],inside[0],out var a,out var an);Edge(outside[0],inside[1],out var b,out var bn);Edge(outside[0],inside[2],out var c,out var cn);Triangle(a,b,c,an,bn,cn);}
                    else
                    {
                        Edge(inside[0],outside[0],out var a,out var an);Edge(inside[0],outside[1],out var b,out var bn);
                        Edge(inside[1],outside[0],out var c,out var cn);Edge(inside[1],outside[1],out var d,out var dn);
                        Triangle(a,b,c,an,bn,cn);Triangle(b,d,c,bn,dn,cn);
                    }
                }
            }
        }
        private void Edge(int a,int b,out Vector3 p,out Vector3 n)
        {
            float t=(organism.Profile.SkinThreshold-values[a])/(values[b]-values[a]);p=Vector3.Lerp(corners[a],corners[b],t);n=Vector3.Lerp(cornerNormals[a],cornerNormals[b],t).normalized;
        }
        private void Triangle(Vector3 a,Vector3 b,Vector3 c,Vector3 an,Vector3 bn,Vector3 cn)
        {
            if(Vector3.Dot(Vector3.Cross(b-a,c-a),an+bn+cn)<0){(b,c)=(c,b);(bn,cn)=(cn,bn);}
            int i=vertices.Count;vertices.Add(a);vertices.Add(b);vertices.Add(c);normals.Add(an);normals.Add(bn);normals.Add(cn);triangles.Add(i);triangles.Add(i+1);triangles.Add(i+2);
        }
        private void OnDestroy(){if(mesh!=null)Destroy(mesh);}
    }
}
