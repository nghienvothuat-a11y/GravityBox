using System;
using System.Collections.Generic;
using System.IO;
using GravityBox.Venom;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Object=UnityEngine.Object;

namespace GravityBox.Editor
{
    public static partial class VenomCampaignBuilder
    {
        public static string[] Campaign60ScenePaths()
        {
            var paths=new string[60];
            for(int i=0;i<paths.Length;i++)paths[i]=$"{Campaign30Folder}/{Campaign30ScenePrefix}{i+1:00}.unity";
            return paths;
        }
        [MenuItem("Gravity Box/COghe/Generate Slippery Vessels 56–60")]
        public static void GenerateCampaign60()
        {
            foreach(var path in Campaign55ScenePaths())if(!File.Exists(path))throw new FileNotFoundException("Preserve existing campaign 01–55",path);
            PrepareCampaign30Assets();string previous=authoredMeshFolder;
            try
            {
                authoredMeshFolder=Campaign30Folder+"/VesselMeshes";Directory.CreateDirectory(authoredMeshFolder);AssetDatabase.Refresh();
                for(int slot=56;slot<=60;slot++)BuildCampaign30Content(slot,slot);
            }
            finally{authoredMeshFolder=previous;}
            var scenes=new List<EditorBuildSettingsScene>();
            foreach(var path in Campaign60ScenePaths())scenes.Add(new EditorBuildSettingsScene(path,true));
            foreach(var scene in EditorBuildSettings.scenes)if(!scenes.Exists(s=>s.path==scene.path))scenes.Add(scene);
            EditorBuildSettings.scenes=scenes.ToArray();AssetDatabase.SaveAssets();
            Debug.Log("COGHE CAMPAIGN 60 GENERATED: 01–55 preserved, five physical slippery vessels appended.");
        }
        [MenuItem("Gravity Box/COghe/Build Campaign 60 · macOS")]
        public static void BuildCampaign60Mac()
        {
            const string output="Builds/COgheCampaign60/macOS/COghe.app";
            foreach(var path in Campaign60ScenePaths())if(!File.Exists(path))throw new FileNotFoundException("Generate campaign 60 first",path);
            Directory.CreateDirectory(Path.GetDirectoryName(output));string previous=PlayerSettings.productName;
            try
            {
                PlayerSettings.productName="Venom";
                var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=Campaign60ScenePaths(),target=BuildTarget.StandaloneOSX,locationPathName=output,options=BuildOptions.Development});
                if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Campaign60 build failed: "+report.summary.result);
                Debug.Log("COGHE CAMPAIGN 60 BUILD SUCCESS: "+output);
            }
            finally{PlayerSettings.productName=previous;}
        }
        private struct VesselRing
        {
            public Vector3 Centre,Tangent;public float Width,Depth;
            public VesselRing(float x,float y,float width,float depth){Centre=new Vector3(x,y,0);Width=width;Depth=depth;Tangent=Vector3.up;}
        }
        private static void BuildSlipperyVessel(ExpansionContext c)
        {
            int index=c.Number-56;
            string[] titles={"Bình cổ hẹp","Mặt nạ thủy tinh","Ấm nghiêng","Đầu lâu pha lê","BOSS · Vỏ ốc"};
            string[] lessons={"Kéo xoay chiếc bình. Mọi mặt đều trơn.","Xoay mặt nạ để đưa bạn nhỏ tới lỗ sáng.","Xoay chiếc ấm. Bạn nhỏ chỉ trượt nhờ trọng lực.","Xoay đầu lâu, tìm đường tới lỗ sáng.",string.Empty};
            ConfigureNew(c,titles[index],lessons[index],true,new Vector3(16,-14,0),.5f);
            c.Definition.Passive=true;c.Owner.ApertureRadius=.048f;
            var rings=VesselRings(index,c.Owner.ApertureRadius);
            if(index==3)for(int i=0;i<rings.Count;i++){var r=rings[i];r.Centre.y=-r.Centre.y;r.Tangent=Vector3.down;rings[i]=r;}
            var last=rings[rings.Count-1];c.Exit=last.Centre;c.Outward=last.Tangent;
            c.Spawn=index==4?rings[6].Centre:new Vector3(0,index==3?.16f:-.16f,0);
            var interior=VesselInterior(rings);
            var physics=VesselWall(interior,rings, .006f,out var visible);
            var go=new GameObject(titles[index]+" · continuous slippery cavity",typeof(MeshFilter),typeof(MeshRenderer),typeof(MeshCollider),typeof(VenomSurfacePatch),typeof(COgheCurvedSurface));
            go.transform.SetParent(c.Root,false);go.GetComponent<MeshFilter>().sharedMesh=Save(visible);go.GetComponent<Renderer>().sharedMaterial=slip;
            var collider=go.GetComponent<MeshCollider>();collider.sharedMesh=Save(physics);collider.sharedMaterial=slick;collider.contactOffset=.0003f;
            var curved=go.GetComponent<COgheCurvedSurface>();curved.Interior=Save(interior);
            var patch=go.GetComponent<VenomSurfacePatch>();patch.Shape=collider;patch.Curved=curved;patch.Size=Vector2.zero;
            patch.Slippery=true;patch.Selectable=true;patch.InterceptExterior=true;c.Surfaces.Add(patch);
        }
        private static List<VesselRing> VesselRings(int shape,float aperture)
        {
            var keys=new List<VesselRing>();
            if(shape==0)keys.AddRange(new[]{new VesselRing(0,-.29f,.09f,.09f),new VesselRing(0,-.25f,.16f,.16f),new VesselRing(0,-.13f,.235f,.235f),new VesselRing(0,.01f,.245f,.245f),new VesselRing(0,.13f,.19f,.19f),new VesselRing(0,.22f,.080f,.080f),new VesselRing(0,.30f,.065f,.065f),new VesselRing(0,.34f,aperture,aperture)});
            if(shape==1)keys.AddRange(new[]{new VesselRing(0,-.28f,.05f,.045f),new VesselRing(0,-.21f,.12f,.070f),new VesselRing(0,-.09f,.21f,.095f),new VesselRing(0,.08f,.25f,.10f),new VesselRing(0,.22f,.22f,.09f),new VesselRing(.065f,.29f,.11f,.068f),new VesselRing(.105f,.34f,aperture,aperture)});
            if(shape==2)keys.AddRange(new[]{new VesselRing(0,-.25f,.11f,.11f),new VesselRing(0,-.20f,.19f,.19f),new VesselRing(0,-.09f,.245f,.235f),new VesselRing(.01f,.04f,.22f,.21f),new VesselRing(.045f,.13f,.145f,.135f),new VesselRing(.135f,.19f,.083f,.080f),new VesselRing(.23f,.245f,.060f,.060f),new VesselRing(.31f,.31f,aperture,aperture)});
            if(shape==3)keys.AddRange(new[]{new VesselRing(0,-.26f,.10f,.11f),new VesselRing(0,-.21f,.19f,.17f),new VesselRing(0,-.09f,.25f,.21f),new VesselRing(0,.06f,.235f,.20f),new VesselRing(0,.17f,.16f,.13f),new VesselRing(0,.24f,.105f,.085f),new VesselRing(0,.29f,aperture,aperture)});
            if(shape==4)
            {
                // A single connected curved chamber, not overlapping concentric tubes.
                for(int i=0;i<=64;i++)
                {
                    float t=i/64f,angle=Mathf.Lerp(-150,230,t)*Mathf.Deg2Rad,r=Mathf.Lerp(.12f,.35f,t);
                    float width=Mathf.Lerp(.070f,aperture,t);
                    keys.Add(new VesselRing(Mathf.Cos(angle)*r,Mathf.Sin(angle)*r,width,width));
                }
            }
            var result=new List<VesselRing>();
            int subdivisions=shape==4?1:4;
            for(int i=0;i<keys.Count-1;i++)for(int j=0;j<subdivisions;j++)
            {
                float t=j/(float)subdivisions;
                var a=keys[Mathf.Max(0,i-1)];var b=keys[i];var c=keys[i+1];var d=keys[Mathf.Min(keys.Count-1,i+2)];
                float Interpolate(float p,float q,float r,float s)=>.5f*((2*q)+(-p+r)*t+(2*p-5*q+4*r-s)*t*t+(-p+3*q-3*r+s)*t*t*t);
                result.Add(new VesselRing(Interpolate(a.Centre.x,b.Centre.x,c.Centre.x,d.Centre.x),Interpolate(a.Centre.y,b.Centre.y,c.Centre.y,d.Centre.y),Interpolate(a.Width,b.Width,c.Width,d.Width),Interpolate(a.Depth,b.Depth,c.Depth,d.Depth)));
            }
            result.Add(keys[keys.Count-1]);
            for(int i=0;i<result.Count;i++)
            {var ring=result[i];ring.Tangent=shape==4?(result[Mathf.Min(i+1,result.Count-1)].Centre-result[Mathf.Max(0,i-1)].Centre).normalized:Vector3.up;result[i]=ring;}
            return result;
        }
        private const int VesselSegments=64;
        private static Mesh VesselInterior(List<VesselRing> rings)
        {
            var vertices=new List<Vector3>();var triangles=new List<int>();
            foreach(var ring in rings)
            {
                var right=Vector3.Cross(ring.Tangent,Vector3.forward).normalized;
                for(int a=0;a<VesselSegments;a++){float angle=a*Mathf.PI*2/VesselSegments;vertices.Add(ring.Centre+right*(Mathf.Cos(angle)*ring.Width)+Vector3.forward*(Mathf.Sin(angle)*ring.Depth));}
            }
            void Triangle(int a,int b,int c,Vector3 inward)
            {triangles.Add(a);if(Vector3.Dot(Vector3.Cross(vertices[b]-vertices[a],vertices[c]-vertices[a]),inward)>0){triangles.Add(b);triangles.Add(c);}else{triangles.Add(c);triangles.Add(b);}}
            for(int ring=0;ring<rings.Count-1;ring++)for(int a=0;a<VesselSegments;a++)
            {
                int b=(a+1)%VesselSegments,i=ring*VesselSegments+a,j=ring*VesselSegments+b,k=j+VesselSegments,l=i+VesselSegments;
                var middle=(rings[ring].Centre+rings[ring+1].Centre)*.5f;
                Triangle(i,j,k,middle-(vertices[i]+vertices[j]+vertices[k])/3);Triangle(i,k,l,middle-(vertices[i]+vertices[k]+vertices[l])/3);
            }
            int cap=vertices.Count;vertices.Add(rings[0].Centre);
            for(int a=0;a<VesselSegments;a++)Triangle(cap,a,(a+1)%VesselSegments,rings[0].Tangent);
            var mesh=new Mesh{name="True hollow interior"};mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;
        }
        private static Mesh VesselWall(Mesh interior,List<VesselRing> rings,float thickness,out Mesh visible)
        {
            var inner=interior.vertices;var normals=interior.normals;var indices=interior.triangles;
            var vertices=new Vector3[inner.Length*2];Array.Copy(inner,vertices,inner.Length);
            for(int i=0;i<inner.Length;i++)vertices[inner.Length+i]=inner[i]-normals[i]*thickness;
            var triangles=new List<int>(indices);var outer=new List<int>();
            for(int i=0;i<indices.Length;i+=3){outer.Add(indices[i]+inner.Length);outer.Add(indices[i+2]+inner.Length);outer.Add(indices[i+1]+inner.Length);}
            int rim=(rings.Count-1)*VesselSegments;
            for(int a=0;a<VesselSegments;a++)
            {
                int i=rim+a,j=rim+(a+1)%VesselSegments;
                outer.Add(i);outer.Add(j);outer.Add(j+inner.Length);outer.Add(i);outer.Add(j+inner.Length);outer.Add(i+inner.Length);
            }
            triangles.AddRange(outer);
            var physics=new Mesh{name="Six millimetre continuous hollow wall"};physics.vertices=vertices;physics.SetTriangles(triangles,0);physics.RecalculateNormals();physics.RecalculateBounds();
            visible=new Mesh{name="Vessel outer satin skin"};visible.vertices=vertices;visible.SetTriangles(outer,0);visible.RecalculateNormals();visible.RecalculateBounds();return physics;
        }
    }
}
