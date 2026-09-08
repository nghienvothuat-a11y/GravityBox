using GravityBox.Foundation;
using GravityBox.Gameplay;
using GravityBox.Simulation;
using UnityEngine;

namespace GravityBox.Presentation
{
    public sealed class GameFeedback : MonoBehaviour
    {
        public AudioClip ImpactClip, SuccessClip, FailClip, MechanismClip, RollClip;
        public bool SynthesizeContactAudio = true;
        private readonly AudioSource[] impactVoices = new AudioSource[6];
        private AudioSource effects, rolling;
        private AudioClip generatedImpact, generatedRoll, contactImpact;
        private LevelManager levels;
        private BallController observedBall;
        private float lastImpact = float.NegativeInfinity;
        private int nextVoice;
        private Camera view;

        public void Initialize(LevelManager manager, Camera camera)
        {
            levels = manager;
            view = camera;
            effects = CreateSource();
            for (int i = 0; i < impactVoices.Length; i++) impactVoices[i] = CreateSource();
            rolling = CreateSource();
            rolling.loop = true;
            if (SynthesizeContactAudio)
            {
                generatedImpact = ProceduralContactAudio.CreateImpact();
                generatedRoll = ProceduralContactAudio.CreateRolling();
            }
            contactImpact = generatedImpact != null ? generatedImpact : ImpactClip;
            rolling.clip = generatedRoll != null ? generatedRoll : RollClip;
            rolling.volume = 0;
            if (rolling.clip != null) rolling.Play();
            levels.Loaded += OnLoaded;
            levels.GameplayEvent += OnEvent;
            OnLoaded(levels.Definition);
        }

        private AudioSource CreateSource()
        {
            var source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            // The camera changes distance to frame the apparatus. Its framing must
            // not change the perceived strength of the same physical contact.
            source.spatialBlend = 0;
            return source;
        }

        private void OnLoaded(LevelDefinition definition)
        {
            if (observedBall != null) observedBall.Impact -= OnImpact;
            observedBall = levels.Ball;
            observedBall.Impact += OnImpact;
            view.backgroundColor = definition.Environment.Background;
            lastImpact = float.NegativeInfinity;
            rolling.volume = 0;
            foreach (AudioSource source in impactVoices) source.Stop();
            levels.Current.Rotation.Snapped += Mechanism;
            foreach (PressurePlate plate in levels.Current.Plates) plate.Activated += Mechanism;
            foreach (ImpulsePad pad in levels.Current.Pads) pad.Fired += Mechanism;
        }

        private void OnImpact(float impulse)
        {
            if (observedBall == null || contactImpact == null) return;
            float mass = observedBall.Body.mass;
            // Impulse is measured in N·s. Threshold and loudness use the equivalent
            // velocity change so a 111 g bearing is audible at real-world scale.
            if (impulse < mass * 0.025f || Time.unscaledTime - lastImpact < 0.028f) return;
            lastImpact = Time.unscaledTime;
            float strength = Mathf.Clamp01(impulse / (mass * 1.8f));
            AudioSource voice = impactVoices[nextVoice++ % impactVoices.Length];
            voice.panStereo = BallPan();
            voice.PlayOneShot(contactImpact, Mathf.Sqrt(strength) * 0.62f);
        }

        private float BallPan()
        {
            float x = view.WorldToViewportPoint(observedBall.Body.position).x;
            return Mathf.Clamp(x * 2 - 1, -1, 1) * 0.35f;
        }

        private void Mechanism() => Play(MechanismClip, 0.15f);
        private void OnEvent(string name)
        {
            if (name == "level_complete") Play(SuccessClip, 0.12f);
            if (name == "level_fail") Play(FailClip, 0.08f);
        }
        private void Play(AudioClip clip, float volume) { if (clip != null) effects.PlayOneShot(clip, volume); }

        private void Update()
        {
            if (levels == null || observedBall == null) return;
            bool audible = observedBall.HasContact && !observedBall.IsCaptured &&
                levels.Session.State != SessionState.Paused && Time.timeScale > 0;
            float speed = observedBall.ContactSpeed;
            float gravity = levels.Definition.Environment.Acceleration.magnitude;
            float weight = observedBall.Body.mass * Mathf.Max(gravity, 0.01f);
            float load = Mathf.Sqrt(Mathf.Clamp01(observedBall.ContactLoad / (weight * 1.5f)));
            float rollingReference = Mathf.Sqrt(Mathf.Max(0.001f, gravity * observedBall.Profile.Radius * 2));
            float motion = Mathf.Sqrt(Mathf.Clamp01(speed / rollingReference));
            float volume = audible ? motion * load * 0.16f : 0;
            rolling.volume = Mathf.MoveTowards(rolling.volume, volume, Time.unscaledDeltaTime * 2);
            float turnsPerSecond = speed / (2 * Mathf.PI * observedBall.Profile.Radius);
            rolling.pitch = 0.65f + Mathf.Clamp01(turnsPerSecond / 10) * 0.85f;
            rolling.panStereo = BallPan();
        }

        private void OnDestroy()
        {
            if (levels != null)
            {
                levels.Loaded -= OnLoaded;
                levels.GameplayEvent -= OnEvent;
            }
            if (observedBall != null) observedBall.Impact -= OnImpact;
            if (generatedImpact != null) Destroy(generatedImpact);
            if (generatedRoll != null) Destroy(generatedRoll);
        }
    }
}
