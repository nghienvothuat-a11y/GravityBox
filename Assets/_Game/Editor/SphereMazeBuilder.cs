using System;
using System.Collections.Generic;
using GravityBox.Gameplay;
using GravityBox.Simulation;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace GravityBox.Editor
{
    public static class SphereMazeBuilder
    {
        public const float InnerRadius = .36f, Thickness = .006f, ExitRadius = .023f;
        private const int Seed = 12, PlankCount = 32;

        private sealed class PlankPose
        {
            public Vector3 Centre, Size;
            public Quaternion Rotation;
            public float BoundingRadius => Size.magnitude * .5f;
        }

        // Return an unsaved root so the shared catalog builder owns prefab persistence.
        public static LevelRuntime Build(Material glass, Material frame, Material rim, PhysicsMaterial contact)
        {
            if (glass == null || frame == null || rim == null || contact == null)
                throw new ArgumentNullException("A sphere maze requires its glass, edge, exit and contact materials.");
            var root = new GameObject("SphereMaze box", typeof(Rigidbody), typeof(BoxRotationController), typeof(LevelRuntime), typeof(SpatialMaze));
            Rigidbody body = root.GetComponent<Rigidbody>(); body.isKinematic = true; body.useGravity = false;
            LevelRuntime level = root.GetComponent<LevelRuntime>();
            level.Rotation = root.GetComponent<BoxRotationController>();
            level.BoundsHalfExtent = .43f;
            level.InteriorDepth = 2 * (InnerRadius + Thickness);
            level.Footprint = new Vector2[96];
            for (int i = 0; i < level.Footprint.Length; i++)
            {
                float angle = i * Mathf.PI * 2 / level.Footprint.Length;
                level.Footprint[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * InnerRadius;
            }
            SpatialMaze maze = root.GetComponent<SpatialMaze>();
            maze.InnerRadius = InnerRadius; maze.ShellThickness = Thickness;
            maze.AuthoringSeed = Seed; maze.SpawnPlankIndex = 0; maze.CatchPlankIndex = 1;
            maze.Planks = new BoxCollider[PlankCount];
            Material shellGlass = Transparent("SphereMaze clear spherical glass", glass, new Color(.66f, .83f, .88f, .022f), true);
            Material plankGlass = Transparent("SphereMaze separate glass planks", glass, new Color(.48f, .76f, .83f, .18f), false);
            Material edgeInlay = Transparent("SphereMaze quiet plank edges", glass, new Color(.52f, .79f, .86f, .30f), false);
            Material exitInlay = Transparent("SphereMaze faint exit inlay", glass, new Color(.45f, .72f, .49f, .55f), false);
            exitInlay.EnableKeyword("_EMISSION"); exitInlay.SetColor("_EmissionColor", new Color(.018f, .035f, .02f));
            EditorUtility.SetDirty(exitInlay);
            maze.ShellCollider = MeshObject("Continuous glass sphere with round cut", root.transform,
                SphereMazeGeometry.Shell(InnerRadius, Thickness, ExitRadius), shellGlass, contact).GetComponent<MeshCollider>();

            PlankPose[] layout = Layout();
            var poses = new List<Matrix4x4>(); var sizes = new List<Vector3>();
            var field = new GameObject("Separate glass planks"); field.transform.SetParent(root.transform, false);
            for (int i = 0; i < PlankCount; i++)
            {
                PlankPose pose = layout[i];
                var plank = GameObject.CreatePrimitive(PrimitiveType.Cube);
                plank.name = i == 0 ? "Starting plank" : i == 1 ? "First catch plank"
                    : i < 26 ? "Free plank " + i.ToString("00") : i < 30 ? "Radial deflector " + (i - 25) : "Exit baffle " + (i - 29);
                plank.transform.SetParent(field.transform, false);
                plank.transform.localPosition = pose.Centre; plank.transform.localRotation = pose.Rotation;
                plank.transform.localScale = pose.Size;
                Renderer renderer = plank.GetComponent<Renderer>(); renderer.sharedMaterial = plankGlass;
                renderer.shadowCastingMode = ShadowCastingMode.Off; renderer.receiveShadows = false;
                BoxCollider collider = plank.GetComponent<BoxCollider>();
                collider.sharedMaterial = contact; collider.contactOffset = .0005f;
                maze.Planks[i] = collider;
                poses.Add(Matrix4x4.TRS(pose.Centre, pose.Rotation, Vector3.one)); sizes.Add(pose.Size);
            }
            MeshObject("Subtle plank edge inlays", root.transform,
                SphereMazeGeometry.PlankEdges("SphereMaze separate plank edge inlays", poses, sizes), edgeInlay, null);

            var spawn = new GameObject("BallSpawn"); spawn.transform.SetParent(root.transform, false);
            spawn.transform.localPosition = layout[0].Centre + Vector3.up * (layout[0].Size.y * .5f + .015f + .003f);
            level.BallSpawn = spawn.transform;
            float innerCut = Mathf.Sqrt(InnerRadius * InnerRadius - ExitRadius * ExitRadius);
            float outerRadius = InnerRadius + Thickness;
            float outerCut = Mathf.Sqrt(outerRadius * outerRadius - ExitRadius * ExitRadius);
            var exit = new GameObject("Flush round exit", typeof(ExitSocket)); exit.transform.SetParent(root.transform, false);
            exit.transform.localPosition = Vector3.down * ((innerCut + outerCut) * .5f);
            exit.transform.localRotation = Quaternion.LookRotation(Vector3.down, Vector3.forward);
            level.Exit = exit.GetComponent<ExitSocket>();
            level.Exit.ApertureRadius = ExitRadius; level.Exit.WallHalfDepth = (outerCut - innerCut) * .5f;
            Mesh ring = SphereMazeGeometry.Ring("SphereMaze flush round exit inlay", ExitRadius, .0007f, level.Exit.WallHalfDepth);
            GameObject exitVisual = MeshObject("Faint flush exit rim", exit.transform, ring, exitInlay, null);
            exitVisual.transform.localRotation = Quaternion.Euler(90, 0, 0);
            return level;
        }

        private static PlankPose[] Layout()
        {
            var layout = new PlankPose[PlankCount];
            layout[0] = Pose(new Vector3(0, .215f, 0), new Vector3(.090f, .004f, .026f), Quaternion.identity);
            layout[1] = Pose(new Vector3(0, .070f, .045f), new Vector3(.100f, .004f, .040f), Quaternion.identity);
            Vector3[] radialDirections =
            {
                new Vector3(.85f, .18f, .50f), new Vector3(-.62f, .55f, .56f),
                new Vector3(.15f, -.45f, -.88f), new Vector3(-.72f, -.55f, -.42f)
            };
            for (int i = 0; i < radialDirections.Length; i++)
            {
                Vector3 direction = radialDirections[i].normalized;
                layout[26 + i] = Pose(direction * .305f, new Vector3(.085f, .0035f, .022f),
                    Quaternion.FromToRotation(Vector3.right, direction) * Quaternion.AngleAxis(i * 37, Vector3.right));
            }
            layout[30] = Pose(new Vector3(0, -.240f, 0), new Vector3(.110f, .004f, .024f), Quaternion.identity);
            layout[31] = Pose(new Vector3(.020f, -.294f, 0), new Vector3(.095f, .004f, .022f), Quaternion.Euler(0, 90, 15));
            var random = new System.Random(Seed);
            // Preserve an open first drop from the starting plank to the catch.
            var firstDrop = new Bounds(new Vector3(0, .145f, .045f), new Vector3(.090f, .200f, .170f));
            for (int index = 2; index < 26; index++)
            {
                for (int attempt = 0; attempt < 20000; attempt++)
                {
                    Vector3 centre = new Vector3(Range(-.29f, .29f), Range(-.29f, .29f), Range(-.29f, .29f));
                    if (centre.magnitude > .29f) continue;
                    Vector3 size = new Vector3(Range(.060f, .100f), Range(.003f, .004f), Range(.018f, .028f));
                    var candidate = Pose(centre, size, Quaternion.Euler(Range(-80, 80), Range(0, 360), Range(-80, 80)));
                    if ((firstDrop.ClosestPoint(centre) - centre).sqrMagnitude < candidate.BoundingRadius * candidate.BoundingRadius) continue;
                    // Keep the last free approach around the one physical exit open.
                    if (centre.y - candidate.BoundingRadius < -.305f && new Vector2(centre.x, centre.z).magnitude < candidate.BoundingRadius + .035f) continue;
                    bool separate = true;
                    foreach (PlankPose other in layout)
                    {
                        if (other == null) continue;
                        float required = candidate.BoundingRadius + other.BoundingRadius + .020f;
                        if ((candidate.Centre - other.Centre).sqrMagnitude < required * required) { separate = false; break; }
                    }
                    if (!separate) continue;
                    layout[index] = candidate;
                    break;
                }
                if (layout[index] == null) throw new InvalidOperationException("Could not place a separate spherical maze plank for seed " + Seed + ".");
            }
            foreach (PlankPose plank in layout)
                foreach (int x in new[] { -1, 1 })
                    foreach (int y in new[] { -1, 1 })
                        foreach (int z in new[] { -1, 1 })
                        {
                            Vector3 corner = plank.Centre + plank.Rotation * Vector3.Scale(plank.Size * .5f, new Vector3(x, y, z));
                            if (corner.magnitude > InnerRadius - .004f)
                                throw new InvalidOperationException("A free plank must remain clear of the spherical shell.");
                        }
            return layout;

            float Range(float minimum, float maximum) => minimum + (float)random.NextDouble() * (maximum - minimum);
        }

        private static PlankPose Pose(Vector3 centre, Vector3 size, Quaternion rotation)
            => new PlankPose { Centre = centre, Size = size, Rotation = rotation };

        private static GameObject MeshObject(string name, Transform parent, Mesh mesh, Material material, PhysicsMaterial contact)
        {
            var go = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer)); go.transform.SetParent(parent, false);
            go.GetComponent<MeshFilter>().sharedMesh = mesh;
            Renderer renderer = go.GetComponent<Renderer>(); renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off; renderer.receiveShadows = false;
            if (contact != null)
            {
                MeshCollider collider = go.AddComponent<MeshCollider>(); collider.sharedMesh = mesh;
                collider.sharedMaterial = contact; collider.contactOffset = .0005f;
            }
            return go;
        }

        private static Material Transparent(string name, Material source, Color color, bool reflection)
        {
            string path = PhysicsLabBuilder.Folder + "/Materials/" + name + ".mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null) { material = new Material(source) { name = name }; AssetDatabase.CreateAsset(material, path); }
            else material.CopyPropertiesFromMaterial(source);
            material.SetColor("_BaseColor", color); material.SetFloat("_Metallic", 0);
            material.SetFloat("_Smoothness", reflection ? .85f : .30f);
            material.SetFloat("_Surface", 1); material.SetFloat("_Blend", 0);
            material.SetFloat("_BlendModePreserveSpecular", 0);
            material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_SrcBlendAlpha", (float)BlendMode.One);
            material.SetFloat("_DstBlendAlpha", (float)BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_ZWrite", 0); material.SetFloat("_Cull", 0);
            material.SetFloat("_SpecularHighlights", reflection ? 1 : 0);
            material.SetFloat("_EnvironmentReflections", reflection ? 1 : 0);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT"); material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            if (reflection) { material.DisableKeyword("_SPECULARHIGHLIGHTS_OFF"); material.DisableKeyword("_ENVIRONMENTREFLECTIONS_OFF"); }
            else { material.EnableKeyword("_SPECULARHIGHLIGHTS_OFF"); material.EnableKeyword("_ENVIRONMENTREFLECTIONS_OFF"); }
            material.DisableKeyword("_EMISSION"); material.SetShaderPassEnabled("ShadowCaster", false);
            material.renderQueue = (int)RenderQueue.Transparent; EditorUtility.SetDirty(material);
            return material;
        }
    }
}
