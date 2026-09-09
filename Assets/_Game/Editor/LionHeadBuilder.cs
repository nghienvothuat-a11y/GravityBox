using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GravityBox.Editor
{
    // A sculpted mask-shaped tray. The mane is the actual concave enclosure;
    // bevelled facial relief shares its baked mesh with its physical collider.
    internal static class LionHeadBuilder
    {
        public static Vector2[] Outline()
        {
            Vector2[] half = {
                V(0,.35f), V(.065f,.325f), V(.115f,.35f), V(.14f,.304f), V(.19f,.322f), V(.205f,.27f),
                V(.245f,.285f), V(.284f,.274f), V(.301f,.24f), V(.292f,.205f),
                V(.327f,.168f), V(.302f,.127f), V(.354f,.091f), V(.314f,.045f), V(.357f,-.012f),
                V(.31f,-.039f), V(.34f,-.095f), V(.291f,-.111f), V(.309f,-.174f), V(.254f,-.177f),
                V(.258f,-.245f), V(.198f,-.232f), V(.18f,-.30f), V(.126f,-.279f), V(.071f,-.348f), V(0,-.315f)
            };
            var points = new List<Vector2>(half);
            for (int i = half.Length - 2; i > 0; i--) points.Add(V(-half[i].x, half[i].y));
            points.Reverse(); // Counter-clockwise: constant-width walls grow outwards.
            return points.ToArray();
        }

        public static void Build(GameObject root, Vector2[] outline, Vector2 exit, float aperture, PhysicsMaterial contact)
        {
            Material bronze = Material("Lion bronze", new Color(.39f,.23f,.075f), .65f, .4f);
            Material face = Material("Lion face gold", new Color(.64f,.43f,.18f), .35f, .38f);
            Material pale = Material("Lion muzzle ivory", new Color(.78f,.62f,.34f), .18f, .42f);
            Material dark = Material("Lion onyx", new Color(.065f,.055f,.043f), .35f, .46f);
            Material edge = Material("Lion polished edges", new Color(.68f,.43f,.15f), .75f, .5f);
            string glassPath = PhysicsLabBuilder.Folder + "/Materials/Lion inspection glass.mat";
            Material glass = AssetDatabase.LoadAssetAtPath<Material>(glassPath);
            if (glass == null)
            {
                glass = new Material(Shader.Find("GravityBox/Inspection Glass"));
                AssetDatabase.CreateAsset(glass, glassPath);
            }
            root.transform.Find("Clear top cover").GetComponent<Renderer>().sharedMaterial = glass;
            root.transform.Find("Clear side walls").GetComponent<Renderer>().sharedMaterial = glass;
            root.transform.Find("Floor with circular cut").GetComponent<Renderer>().sharedMaterial = bronze;
            root.transform.Find("Lower machined edge").GetComponent<Renderer>().sharedMaterial = edge;
            root.transform.Find("Upper machined edge").GetComponent<Renderer>().sharedMaterial = edge;

            // Flat, collider-free inlays never hide the actual aperture.
            Vector2[] faceOutline = Mirrored(new[] { V(0,.24f), V(.10f,.215f), V(.17f,.145f), V(.165f,.062f),
                V(.143f,.008f), V(.16f,-.08f), V(.13f,-.172f), V(.07f,-.245f), V(0,-.257f) });
            MeshObject(root, "Lion face inlay", PhysicsLabGeometry.Panel("Lion face inlay", faceOutline, -.04175f, .0001f,
                exit, aperture + .0008f), face); // Expose the existing 0.7 mm light inlay below this decoration.
            Mane(root, outline);

            Vector2[] eye = { V(.047f,.103f), V(.114f,.128f), V(.15f,.094f), V(.11f,.071f), V(.066f,.079f) };
            Relief(root, "Right eye", eye, .032f, .82f, dark, contact);
            Relief(root, "Left eye", Mirror(eye), .032f, .82f, dark, contact);
            Relief(root, "Nose", new[] { V(-.056f,.012f), V(.056f,.012f), V(0,-.055f) }, .024f, .78f, dark, contact);
            Relief(root, "Right muzzle", Oval(V(.066f,-.095f), .064f, .047f, 12), .021f, .76f, pale, contact);
            Relief(root, "Left muzzle", Oval(V(-.066f,-.095f), .064f, .047f, 12), .021f, .76f, pale, contact);

            // Recessed ear colour sits on the real playable floor, not on an external prop.
            Vector2[] ear = { V(.225f,.254f), V(.261f,.258f), V(.28f,.237f), V(.265f,.209f), V(.239f,.216f) };
            MeshObject(root, "Right ear inset", PhysicsLabGeometry.Panel("Lion right ear inset", ear, -.0416f, .00008f, Vector2.zero), dark);
            MeshObject(root, "Left ear inset", PhysicsLabGeometry.Panel("Lion left ear inset", Mirror(ear), -.0416f, .00008f, Vector2.zero), dark);
            // Small warm highlights make the physical almond eyes readable through the lid.
            foreach (int sign in new[] { -1, 1 })
                MeshObject(root, sign < 0 ? "Left eye glint" : "Right eye glint",
                    PhysicsLabGeometry.Panel("Lion eye glint " + sign, Oval(V(sign*.102f,.101f), .014f, .004f, 8), .03215f, .00008f, Vector2.zero), edge);
        }

        private static void Mane(GameObject root, Vector2[] outline)
        {
            Material[] shades = {
                Material("Lion mane amber", new Color(.47f,.27f,.082f), .55f,.36f),
                Material("Lion mane shadow", new Color(.23f,.105f,.029f), .5f,.32f),
                Material("Lion mane light", new Color(.59f,.36f,.12f), .6f,.42f)
            };
            var vertices = new List<Vector3>(); var triangles = new[] { new List<int>(), new List<int>(), new List<int>() };
            for (int i = 0; i < outline.Length; i++)
            {
                Vector2 a = outline[i] * .975f, b = outline[(i+1)%outline.Length] * .975f;
                Vector2 innerA = Inner(a), innerB = Inner(b);
                int n = vertices.Count;
                vertices.Add(P(a,-.0418f)); vertices.Add(P(b,-.0418f)); vertices.Add(P(innerB,-.0418f)); vertices.Add(P(innerA,-.0418f));
                // Looking from +Y, reverse the XZ winding for front-facing triangles.
                triangles[i%3].AddRange(new[] { n,n+2,n+1,n,n+3,n+2 });
            }
            Mesh mesh = SaveMesh("Lion mane facets", vertices, triangles);
            GameObject go = MeshObject(root, "Mane floor facets", mesh, shades[0]);
            go.GetComponent<Renderer>().sharedMaterials = shades;
            Vector2 Inner(Vector2 p)
            {
                Vector2 q = p - V(0,-.015f);
                float length = Mathf.Sqrt(q.x*q.x/(.17f*.17f)+q.y*q.y/(.245f*.245f));
                return q/length + V(0,-.015f);
            }
        }

        private static void Relief(GameObject root, string name, Vector2[] contour, float top, float inset, Material material, PhysicsMaterial contact)
        {
            contour = PhysicsLabGeometry.OrientedContour(contour, true);
            Vector2 centre = Vector2.zero; foreach (Vector2 p in contour) centre += p; centre /= contour.Length;
            var vertices = new List<Vector3>(); var triangles = new List<int>();
            for (int i=0;i<contour.Length;i++)
            {
                Vector2 a=contour[i], b=contour[(i+1)%contour.Length];
                Vector2 ta=Vector2.Lerp(centre,a,inset), tb=Vector2.Lerp(centre,b,inset);
                Triangle(P(centre,top),P(tb,top),P(ta,top));
                Triangle(P(centre,-.042f),P(a,-.042f),P(b,-.042f));
                Triangle(P(a,-.042f),P(ta,top),P(tb,top));
                Triangle(P(a,-.042f),P(tb,top),P(b,-.042f));
            }
            GameObject go = MeshObject(root, name + " - fixed relief", SaveMesh("Lion " + name, vertices, new[] { triangles }), material);
            MeshCollider collider = go.AddComponent<MeshCollider>(); collider.sharedMesh = go.GetComponent<MeshFilter>().sharedMesh;
            collider.convex = true; collider.sharedMaterial = contact; collider.contactOffset = .0005f;
            void Triangle(Vector3 a,Vector3 b,Vector3 c)
            { int n=vertices.Count; vertices.Add(a);vertices.Add(b);vertices.Add(c);triangles.AddRange(new[]{n,n+1,n+2}); }
        }

        private static Mesh SaveMesh(string name, List<Vector3> vertices, List<int>[] triangles)
        {
            string path=PhysicsLabBuilder.Folder+"/Meshes/"+name+".asset";
            Mesh mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if(mesh==null){mesh=new Mesh{name=name};AssetDatabase.CreateAsset(mesh,path);}
            mesh.Clear();mesh.SetVertices(vertices);mesh.subMeshCount=triangles.Length;
            for(int i=0;i<triangles.Length;i++)mesh.SetTriangles(triangles[i],i);
            mesh.RecalculateNormals();mesh.RecalculateBounds();EditorUtility.SetDirty(mesh);return mesh;
        }
        private static GameObject MeshObject(GameObject root, string name, Mesh mesh, Material material)
        {
            var go=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(root.transform,false);
            go.GetComponent<MeshFilter>().sharedMesh=mesh;go.GetComponent<Renderer>().sharedMaterial=material;return go;
        }
        private static Material Material(string name, Color color, float metal, float smooth)
        {
            string path=PhysicsLabBuilder.Folder+"/Materials/"+name+".mat";
            Material material=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(material==null){material=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(material,path);}
            material.SetColor("_BaseColor",color);material.SetFloat("_Metallic",metal);material.SetFloat("_Smoothness",smooth);
            EditorUtility.SetDirty(material);return material;
        }
        private static Vector2[] Oval(Vector2 centre,float x,float z,int count)
        {var points=new Vector2[count];for(int i=0;i<count;i++){float a=i*Mathf.PI*2/count;points[i]=centre+V(Mathf.Cos(a)*x,Mathf.Sin(a)*z);}return points;}
        private static Vector2[] Mirror(Vector2[] points)
        {var result=new Vector2[points.Length];for(int i=0;i<points.Length;i++)result[i]=V(-points[i].x,points[i].y);return result;}
        private static Vector2[] Mirrored(Vector2[] half)
        {var points=new List<Vector2>(half);for(int i=half.Length-2;i>0;i--)points.Add(V(-half[i].x,half[i].y));return points.ToArray();}
        private static Vector3 P(Vector2 p,float y)=>new Vector3(p.x,y,p.y);
        private static Vector2 V(float x,float z)=>new Vector2(x,z);
    }
}
