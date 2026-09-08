using GravityBox.Foundation;
using GravityBox.Gameplay;
using UnityEngine;

namespace GravityBox.Presentation
{
    public sealed class GameFeedback : MonoBehaviour
    {
        public AudioClip ImpactClip, SuccessClip, FailClip, MechanismClip, RollClip;
        private AudioSource effects, rolling;
        private LevelManager levels;
        private float lastImpact;
        private Camera view;

        public void Initialize(LevelManager manager, Camera camera)
        {
            levels = manager;
            view = camera;
            effects = gameObject.AddComponent<AudioSource>();
            effects.playOnAwake = false;
            rolling = gameObject.AddComponent<AudioSource>();
            rolling.playOnAwake = false;
            rolling.loop = true;
            rolling.clip = RollClip;
            rolling.volume = 0;
            if (RollClip != null) rolling.Play();
            levels.Loaded += OnLoaded;
            levels.GameplayEvent += OnEvent;
            OnLoaded(levels.Definition);
        }

        private void OnLoaded(LevelDefinition definition)
        {
            view.backgroundColor = definition.Environment.Background;
            levels.Ball.Impact += OnImpact;
            levels.Current.Rotation.Snapped += Mechanism;
            foreach (PressurePlate plate in levels.Current.Plates) plate.Activated += Mechanism;
            foreach (ImpulsePad pad in levels.Current.Pads) pad.Fired += Mechanism;
        }

        private void OnImpact(float impulse)
        {
            if (Time.time - lastImpact < 0.09f || impulse < 0.12f) return;
            lastImpact = Time.time;
            Play(ImpactClip, Mathf.Clamp(impulse * 0.055f, 0.025f, 0.32f));
        }

        private void Mechanism() => Play(MechanismClip, 0.25f);
        private void OnEvent(string name)
        {
            if (name == "level_complete") Play(SuccessClip, 0.45f);
            if (name == "level_fail") Play(FailClip, 0.22f);
        }
        private void Play(AudioClip clip, float volume) { if (clip != null) effects.PlayOneShot(clip, volume); }

        private void Update()
        {
            if (levels == null || levels.Ball == null) return;
            float speed = levels.Ball.Body.linearVelocity.magnitude;
            float volume = levels.Ball.HasContact && levels.Session.State == SessionState.Active ? Mathf.Clamp01(speed / 8) * 0.1f : 0;
            rolling.volume = Mathf.MoveTowards(rolling.volume, volume, Time.unscaledDeltaTime);
            rolling.pitch = 0.65f + Mathf.Clamp(speed / 8, 0, 1);
        }

        private void OnDestroy()
        {
            if (levels == null) return;
            levels.Loaded -= OnLoaded;
            levels.GameplayEvent -= OnEvent;
        }
    }
}
