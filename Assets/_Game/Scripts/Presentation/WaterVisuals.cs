using GravityBox.Foundation;
using GravityBox.Simulation;
using UnityEngine;
using UnityEngine.Rendering;

namespace GravityBox.Presentation
{
    /// <summary>Bounded optical tracers and a motion-driven wake. Never applies forces.</summary>
    public sealed class WaterVisuals : MonoBehaviour, IResettable
    {
        public Renderer VolumeRenderer, FloorRenderer;
        public Material TracerMaterial;
        [Tooltip("Silver diagnostic visualization of opaque mercury; has no effect on physics.")]
        public bool MercuryCutaway;
        private const int Motes = 100, WakeParticles = 120, Count = Motes + WakeParticles;
        private struct Particle { public Vector3 Position, Velocity; public float Age, Life, Size; }
        private readonly Particle[] particles = new Particle[Count];
        private readonly Vector3[] vertices = new Vector3[Count * 4];
        private readonly Color[] colors = new Color[Count * 4];
        private WaterVolume water;
        private Mesh mesh;
        private GameObject tracerObject;
        private Renderer tracerRenderer;
        private MaterialPropertyBlock block;
        private System.Random random;
        private float clock, emission;
        private int nextWake;
        public int LiveWakeCount { get; private set; }
        public int TracerCount => Count;
        public float VisualClock => clock;
        public float FloorOpacity { get; private set; } = 1;

        public void Initialize(WaterVolume volume)
        {
            water = volume;
            if (mesh == null)
            {
                mesh = new Mesh { name = "Water optical tracers (visual only)" }; mesh.MarkDynamic();
                var uv = new Vector2[Count * 4]; var triangles = new int[Count * 6];
                for (int i = 0; i < Count; i++)
                {
                    int v = i * 4, t = i * 6;
                    uv[v] = Vector2.zero; uv[v + 1] = Vector2.right; uv[v + 2] = Vector2.one; uv[v + 3] = Vector2.up;
                    triangles[t] = v; triangles[t + 1] = v + 2; triangles[t + 2] = v + 1;
                    triangles[t + 3] = v; triangles[t + 4] = v + 3; triangles[t + 5] = v + 2;
                }
                mesh.vertices = vertices; mesh.uv = uv; mesh.triangles = triangles;
                tracerObject = new GameObject("Suspended tracers and steel wake", typeof(MeshFilter), typeof(MeshRenderer));
                tracerObject.transform.SetParent(transform, false);
                tracerObject.GetComponent<MeshFilter>().sharedMesh = mesh;
                var renderer = tracerObject.GetComponent<MeshRenderer>(); renderer.sharedMaterial = TracerMaterial;
                tracerRenderer = renderer;
                renderer.shadowCastingMode = ShadowCastingMode.Off; renderer.receiveShadows = false;
                block = new MaterialPropertyBlock();
            }
            ResetState();
        }

        private void LateUpdate()
        {
            if (water == null) return;
            Advance(Mathf.Min(Time.deltaTime, .05f));
            Refresh(Camera.main);
        }

        public void Advance(float dt)
        {
            if (water == null || dt <= 0) return;
            clock += dt;
            Vector3 ballLocal = water.Ball != null ? transform.InverseTransformPoint(water.Ball.Body.position) : Vector3.one * 100;
            Vector3 relative = water.Ball != null ? transform.InverseTransformDirection(water.RelativeVelocity) : Vector3.zero;
            float speed = relative.magnitude;
            if (water.SubmergedFraction > .1f && speed > .035f)
            {
                emission += dt * Mathf.Min(180, speed * 260);
                while (emission >= 1)
                {
                    emission--;
                    Vector3 direction = relative / speed;
                    Vector3 side = Vector3.Cross(direction, Mathf.Abs(direction.y) < .85f ? Vector3.up : Vector3.right).normalized;
                    Vector3 up = Vector3.Cross(direction, side);
                    float phase = clock * 32 + nextWake * 2.39996f;
                    Vector3 offset = (side * Mathf.Cos(phase) + up * Mathf.Sin(phase)) * .009f;
                    Vector3 position = ballLocal - direction * .020f + offset;
                    particles[Motes + nextWake] = new Particle
                    {
                        Position = position, Velocity = relative * .12f + Vector3.Cross(direction, offset) * 3,
                        Life = Range(.45f, .9f), Age = 0, Size = Range(.0009f, .0019f)
                    };
                    nextWake = (nextWake + 1) % WakeParticles;
                }
            }
            else emission = 0;
            LiveWakeCount = 0;
            for (int i = 0; i < Count; i++)
            {
                Particle p = particles[i];
                if (i >= Motes && p.Age >= p.Life) continue;
                p.Age += dt;
                Vector3 flow = water.LocalFlowAt(p.Position);
                p.Velocity *= Mathf.Exp(-dt * 4);
                p.Position += (flow + p.Velocity) * dt;
                Vector3 fromBall = p.Position - ballLocal;
                if (fromBall.sqrMagnitude < .018f * .018f)
                    p.Position = ballLocal + (fromBall.sqrMagnitude > .000001f ? fromBall.normalized : Vector3.right) * .018f;
                if (!water.ContainsVisual(p.Position, p.Size * 2))
                {
                    if (i < Motes) p.Position = RandomInside();
                    else p.Age = p.Life;
                }
                if (i >= Motes && p.Age < p.Life) LiveWakeCount++;
                particles[i] = p;
            }
        }

        public void Refresh(Camera camera)
        {
            if (water == null || mesh == null) return;
            Vector3 right = transform.InverseTransformDirection(camera != null ? camera.transform.right : Vector3.right);
            Vector3 up = transform.InverseTransformDirection(camera != null ? camera.transform.up : Vector3.up);
            for (int i = 0; i < Count; i++)
            {
                Particle p = particles[i];
                float alpha = i < Motes ? .13f + .055f * Mathf.Sin(clock + i) : Mathf.Clamp01(1 - p.Age / Mathf.Max(.001f, p.Life)) * .32f;
                float size = p.Size * (i < Motes ? 1 : 1 + p.Age * 1.3f);
                Vector3 a = right * size, b = up * size;
                int v = i * 4;
                vertices[v] = p.Position - a - b; vertices[v + 1] = p.Position + a - b;
                vertices[v + 2] = p.Position + a + b; vertices[v + 3] = p.Position - a + b;
                Color c = MercuryCutaway ? new Color(.82f, .88f, .95f, alpha * .65f) : new Color(.5f, .87f, 1, alpha);
                colors[v] = colors[v + 1] = colors[v + 2] = colors[v + 3] = c;
            }
            mesh.vertices = vertices; mesh.colors = colors; mesh.bounds = new Bounds(Vector3.zero, water.HalfSize * 2.1f);
            Vector3 localBall = water.Ball != null ? transform.InverseTransformPoint(water.Ball.Body.position) : Vector3.one * 100;
            block.SetVector("_BallLocal", localBall);
            block.SetVector("_HalfSize", water.HalfSize);
            block.SetFloat("_FluidClock", clock);
            block.SetFloat("_Motion", water.RelativeVelocity.magnitude * water.SubmergedFraction);
            // Fade the panel only when its outside face turns towards the viewer.
            // Keep the real floor/circular bore collider and the exit inlay untouched.
            Vector3 toCamera = camera != null && FloorRenderer != null
                ? camera.transform.position - FloorRenderer.bounds.center : transform.up;
            float facing = Vector3.Dot(-transform.up, toCamera.normalized);
            FloorOpacity = Mathf.Lerp(1, .12f, Mathf.SmoothStep(0, 1, Mathf.InverseLerp(-.2f, .45f, facing)));
            block.SetFloat("_FloorOpacity", FloorOpacity);
            block.SetMatrix("_WaterWorldToLocal", transform.worldToLocalMatrix);
            VolumeRenderer?.SetPropertyBlock(block); FloorRenderer?.SetPropertyBlock(block);
            tracerRenderer?.SetPropertyBlock(block);
        }

        public void CaptureInitialState() { }
        public void ResetState()
        {
            if (water == null) return;
            random = new System.Random(13013); clock = emission = 0; nextWake = LiveWakeCount = 0;
            for (int i = 0; i < Count; i++)
                particles[i] = new Particle { Position = RandomInside(), Size = Range(.00045f, .00085f), Age = 1, Life = i < Motes ? float.MaxValue : 0 };
            Refresh(Camera.main);
        }

        private Vector3 RandomInside()
        {
            for (int i = 0; i < 100; i++)
            {
                Vector3 p = new Vector3(Range(-1, 1) * water.HalfSize.x, Range(-1, 1) * water.HalfSize.y, Range(-1, 1) * water.HalfSize.z) * .94f;
                if (water.ContainsVisual(p, .002f)) return p;
            }
            return new Vector3(-.1f, 0, 0);
        }
        private float Range(float a, float b) => a + (float)random.NextDouble() * (b - a);
        private void OnDestroy()
        {
            if (mesh != null) { if (Application.isPlaying) Destroy(mesh); else DestroyImmediate(mesh); }
        }
    }
}
