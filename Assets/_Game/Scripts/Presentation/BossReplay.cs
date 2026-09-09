using System.Collections.Generic;
using GravityBox.Foundation;
using GravityBox.Gameplay;
using UnityEngine;
using UnityEngine.Rendering;

namespace GravityBox.Presentation
{
    // Records observed poses. Playback contains renderers only, never simulation bodies.
    public sealed class BossReplay : MonoBehaviour
    {
        private const int Capacity = 450;
        private const float Interval = 1f / 30f;
        private sealed class Frame
        {
            public Vector3[] Positions;
            public Quaternion[] Rotations;
            public Frame(int count) { Positions = new Vector3[count]; Rotations = new Quaternion[count]; }
        }
        private LevelManager levels;
        private CameraRig cameraRig;
        private Rigidbody[] bodies = System.Array.Empty<Rigidbody>();
        private Renderer[] originals = System.Array.Empty<Renderer>();
        private bool[] hiddenBeforeReplay;
        private readonly Frame[] frames = new Frame[Capacity];
        private int nextFrame, frameCount;
        private float accumulated, playbackTime, previousTimeScale;
        private bool recording;
        private GameObject ghostRoot;
        private Transform[] ghosts;
        public bool IsPlaying { get; private set; }
        public bool Available => frameCount > 1 && levels != null && levels.Definition.Design != null &&
            levels.Definition.Design.IsBoss && levels.Session.State == SessionState.Completing;

        public void Initialize(LevelManager manager, CameraRig rig)
        {
            levels = manager; cameraRig = rig;
            levels.Loaded += OnLoaded; levels.GameplayEvent += OnEvent;
            OnLoaded(levels.Definition);
        }
        private void OnLoaded(LevelDefinition definition)
        {
            Stop(); nextFrame = frameCount = 0; accumulated = 0;
            recording = definition.Design != null && definition.Design.IsBoss;
            if (!recording) { bodies = System.Array.Empty<Rigidbody>(); return; }
            var list = new List<Rigidbody> { levels.Current.GetComponent<Rigidbody>() };
            foreach (var prop in levels.Current.Props) list.Add(prop.Body);
            foreach (var ball in levels.Balls) list.Add(ball.Body);
            bodies = list.ToArray();
            for (int i = 0; i < Capacity; i++) frames[i] = new Frame(bodies.Length);
            var renderers = new List<Renderer>(levels.Current.GetComponentsInChildren<Renderer>());
            foreach (var prop in levels.Current.Props) renderers.AddRange(prop.GetComponentsInChildren<Renderer>());
            foreach (var ball in levels.Balls) renderers.AddRange(ball.GetComponentsInChildren<Renderer>());
            originals = renderers.ToArray();
            CaptureFrame();
        }
        private void OnEvent(string name)
        {
            if (name == "level_reset") { Stop(); nextFrame = frameCount = 0; accumulated = 0; recording = levels.Definition.Design != null && levels.Definition.Design.IsBoss; }
            else if (name == "level_complete") { if (recording) CaptureFrame(); recording = false; }
            else if (name == "level_abandon" || name == "level_leave_complete") Stop();
        }
        private void FixedUpdate()
        {
            if (!recording || IsPlaying || levels.Session.State != SessionState.Active) return;
            accumulated += Time.fixedDeltaTime;
            if (accumulated < Interval) return;
            accumulated -= Interval; CaptureFrame();
        }
        private void CaptureFrame()
        {
            if (bodies.Length == 0 || frames[nextFrame] == null) return;
            Frame frame = frames[nextFrame];
            for (int i = 0; i < bodies.Length; i++)
            {
                if (bodies[i] == null) return;
                frame.Positions[i] = bodies[i].position; frame.Rotations[i] = bodies[i].rotation;
            }
            nextFrame = (nextFrame + 1) % Capacity; frameCount = Mathf.Min(Capacity, frameCount + 1);
        }
        public void Toggle() { if (IsPlaying) Stop(); else Play(); }
        public void Play()
        {
            if (!Available || IsPlaying) return;
            ghostRoot = new GameObject("Observed boss replay — visuals only");
            ghosts = new Transform[bodies.Length];
            for (int i = 0; i < ghosts.Length; i++)
            {
                var go = new GameObject("Recorded body " + i); go.transform.SetParent(ghostRoot.transform, false);
                go.transform.SetPositionAndRotation(bodies[i].position, bodies[i].rotation); ghosts[i] = go.transform;
            }
            hiddenBeforeReplay = new bool[originals.Length];
            for (int i = 0; i < originals.Length; i++)
            {
                Renderer source = originals[i]; if (source == null) continue;
                hiddenBeforeReplay[i] = source.forceRenderingOff;
                var mesh = source.GetComponent<MeshFilter>();
                if (mesh == null || mesh.sharedMesh == null) { source.forceRenderingOff = true; continue; }
                Rigidbody owner = source.GetComponentInParent<Rigidbody>();
                int ownerIndex = System.Array.IndexOf(bodies, owner);
                if (ownerIndex < 0) continue;
                var visual = new GameObject(source.name, typeof(MeshFilter), typeof(MeshRenderer));
                visual.transform.SetParent(ghosts[ownerIndex], false);
                visual.transform.localPosition = bodies[ownerIndex].transform.InverseTransformPoint(source.transform.position);
                visual.transform.localRotation = Quaternion.Inverse(bodies[ownerIndex].rotation) * source.transform.rotation;
                visual.transform.localScale = source.transform.lossyScale;
                visual.GetComponent<MeshFilter>().sharedMesh = mesh.sharedMesh;
                var renderer = visual.GetComponent<MeshRenderer>(); renderer.sharedMaterials = source.sharedMaterials;
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                var properties = new MaterialPropertyBlock(); source.GetPropertyBlock(properties); renderer.SetPropertyBlock(properties);
                source.forceRenderingOff = true;
            }
            previousTimeScale = Time.timeScale; Time.timeScale = 0;
            IsPlaying = true; playbackTime = 0;
            if (cameraRig != null) cameraRig.ReplayView = true;
            levels.RecordEvent("boss_replay_start");
            Display(0);
        }
        private void LateUpdate()
        {
            if (!IsPlaying) return;
            playbackTime += Time.unscaledDeltaTime;
            float position = playbackTime / Interval;
            if (position >= frameCount - 1) { Stop(); return; }
            Display(position);
        }
        private void Display(float position)
        {
            int first = (nextFrame - frameCount + Capacity) % Capacity;
            int lower = Mathf.FloorToInt(position), upper = Mathf.Min(lower + 1, frameCount - 1);
            Frame a = frames[(first + lower) % Capacity], b = frames[(first + upper) % Capacity];
            float t = position - lower;
            for (int i = 0; i < ghosts.Length; i++)
                ghosts[i].SetPositionAndRotation(Vector3.Lerp(a.Positions[i], b.Positions[i], t), Quaternion.Slerp(a.Rotations[i], b.Rotations[i], t));
        }
        public void Stop()
        {
            if (!IsPlaying) return;
            IsPlaying = false;
            for (int i = 0; i < originals.Length; i++) if (originals[i] != null) originals[i].forceRenderingOff = hiddenBeforeReplay[i];
            if (ghostRoot != null) { ghostRoot.SetActive(false); Destroy(ghostRoot); }
            if (cameraRig != null) cameraRig.ReplayView = false;
            Time.timeScale = previousTimeScale;
            levels?.RecordEvent("boss_replay_end");
        }
        private void OnDestroy()
        {
            Stop();
            if (levels != null) { levels.Loaded -= OnLoaded; levels.GameplayEvent -= OnEvent; }
        }
    }
}
