using System;
using System.Collections.Generic;
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
        private sealed class BallVoice
        {
            public BallController Ball;
            public AudioSource Rolling;
            public Action<float> Impact;
        }
        private readonly List<BallVoice> ballVoices = new List<BallVoice>();
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
            ClearBallVoices();
            foreach (BallController ball in levels.Balls)
            {
                var voice = new BallVoice { Ball = ball, Rolling = CreateSource() };
                voice.Impact = impulse => OnImpact(ball, impulse);
                ball.Impact += voice.Impact;
                voice.Rolling.loop = true; voice.Rolling.clip = rolling.clip; voice.Rolling.volume = 0;
                if (voice.Rolling.clip != null) voice.Rolling.Play();
                ballVoices.Add(voice);
            }
            view.backgroundColor = definition.Environment.Background;
            lastImpact = float.NegativeInfinity;
            rolling.volume = 0;
            foreach (AudioSource source in impactVoices) source.Stop();
            levels.Current.Rotation.Snapped += Mechanism;
            foreach (PressurePlate plate in levels.Current.Plates) plate.Activated += Mechanism;
            foreach (ImpulsePad pad in levels.Current.Pads) pad.Fired += Mechanism;
        }

        private void OnImpact(BallController observedBall, float impulse)
        {
            if (observedBall == null || contactImpact == null) return;
            float mass = observedBall.Body.mass;
            // Impulse is measured in N·s. Threshold and loudness use the equivalent
            // velocity change so a 111 g bearing is audible at real-world scale.
            if (impulse < mass * 0.025f || Time.unscaledTime - lastImpact < 0.028f) return;
            lastImpact = Time.unscaledTime;
            float strength = Mathf.Clamp01(impulse / (mass * 1.8f));
            AudioSource voice = impactVoices[nextVoice++ % impactVoices.Length];
            voice.panStereo = BallPan(observedBall);
            voice.PlayOneShot(contactImpact, Mathf.Sqrt(strength) * 0.62f);
        }

        private float BallPan(BallController observedBall)
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
            if (levels == null) return;
            foreach (BallVoice voice in ballVoices) UpdateRolling(voice);
        }
        private void UpdateRolling(BallVoice voice)
        {
            BallController observedBall = voice.Ball;
            AudioSource rolling = voice.Rolling;
            if (observedBall == null) return;
            bool audible = observedBall.HasContact && !observedBall.IsCaptured && !levels.Current.Exit.HasBallExited(observedBall) &&
                levels.Session.State != SessionState.Paused && Time.timeScale > 0;
            float speed = observedBall.ContactSpeed;
            float gravity = levels.Definition.Environment.Acceleration.magnitude;
            float weight = observedBall.Profile.Mass * Mathf.Max(gravity, 0.01f);
            float load = Mathf.Sqrt(Mathf.Clamp01(observedBall.ContactLoad / (weight * 1.5f)));
            float rollingReference = Mathf.Sqrt(Mathf.Max(0.001f, gravity * observedBall.Profile.Radius * 2));
            float motion = Mathf.Sqrt(Mathf.Clamp01(speed / rollingReference));
            float volume = audible ? motion * load * 0.16f : 0;
            rolling.volume = Mathf.MoveTowards(rolling.volume, volume, Time.unscaledDeltaTime * 2);
            float turnsPerSecond = speed / (2 * Mathf.PI * observedBall.Profile.Radius);
            rolling.pitch = 0.65f + Mathf.Clamp01(turnsPerSecond / 10) * 0.85f;
            rolling.panStereo = BallPan(observedBall);
        }

        private void ClearBallVoices()
        {
            foreach (BallVoice voice in ballVoices)
            {
                if (voice.Ball != null) voice.Ball.Impact -= voice.Impact;
                if (voice.Rolling != null) { voice.Rolling.Stop(); Destroy(voice.Rolling); }
            }
            ballVoices.Clear();
        }

        private void OnDestroy()
        {
            if (levels != null)
            {
                levels.Loaded -= OnLoaded;
                levels.GameplayEvent -= OnEvent;
            }
            ClearBallVoices();
            if (generatedImpact != null) Destroy(generatedImpact);
            if (generatedRoll != null) Destroy(generatedRoll);
        }
    }
}
