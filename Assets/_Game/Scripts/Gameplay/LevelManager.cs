using System;
using System.Collections.Generic;
using GravityBox.Foundation;
using GravityBox.Simulation;
using UnityEngine;

namespace GravityBox.Gameplay
{
    public sealed class LevelManager : MonoBehaviour
    {
        private LevelCatalog catalog;
        private EnvironmentForceSystem forces;
        private LevelCatalog primaryCatalog, alternateCatalog;
        private CampaignProgress campaignProgress;
        private LevelDefinition loadedDefinition;
        private ICampaignProgressStorage progressStorage;
        private float transitionAt;
        private float startTime;
        public readonly GameSession Session = new GameSession();
        public LevelRuntime Current { get; private set; }
        private readonly List<BallController> balls = new List<BallController>();
        public IReadOnlyList<BallController> Balls => balls;
        // Primary-ball compatibility for single-ball diagnostics and legacy presentation.
        public BallController Ball => balls.Count == 0 ? null : balls[0];
        public int EscapedCount => Current != null ? Current.Exit.EscapedCount : 0;
        public BallController ActiveBall
        {
            get { foreach (BallController b in balls) if (!Current.Exit.HasBallExited(b)) return b; return Ball; }
        }
        public LevelDefinition Definition => loadedDefinition != null ? loadedDefinition : catalog.Levels[Index];
        public LevelCatalog Catalog => catalog;
        public LevelCatalog AlternateCatalog => catalog == primaryCatalog ? alternateCatalog : primaryCatalog;
        public CampaignProgress Progress => campaignProgress;
        public int Index { get; private set; }
        public int ResetCount { get; private set; }
        public int DragCount { get; private set; }
        public float DragDistance { get; private set; }
        public float Elapsed => Mathf.Max(0, Time.time - startTime);
        public event Action<LevelDefinition> Loaded;
        public event Action<string> GameplayEvent;

        public void Initialize(LevelCatalog config, EnvironmentForceSystem forceSystem, LevelCatalog alternate = null, bool resume = false,
            ICampaignProgressStorage storage = null)
        {
            if (config == null || config.Levels == null || config.Levels.Length == 0)
                throw new InvalidOperationException("Gravity Box requires a non-empty LevelCatalog.");
            catalog = config;
            primaryCatalog = config;
            alternateCatalog = alternate;
            progressStorage = storage;
            campaignProgress = config.IsCampaign ? new CampaignProgress(config.Id, progressStorage) : null;
            forces = forceSystem;
            Session.Changed -= OnStateChanged;
            Session.Changed += OnStateChanged;
            Load(resume && campaignProgress != null ? campaignProgress.ResumeIndex(config) : 0);
        }

        public void SwitchCatalog()
        {
            LevelCatalog next = AlternateCatalog;
            if (next == null) return;
            catalog = next;
            campaignProgress = catalog.IsCampaign ? new CampaignProgress(catalog.Id, progressStorage) : null;
            Load(campaignProgress != null ? campaignProgress.ResumeIndex(catalog) : 0);
        }

        public void Load(int index)
        {
            if (Current != null) GameplayEvent?.Invoke(Session.State == SessionState.Completing ? "level_leave_complete" : "level_abandon");
            Session.BeginLoading();
            Time.timeScale = 1;
            transitionAt = float.PositiveInfinity;
            CleanupLevel();
            Index = Mathf.Clamp(index, 0, catalog.Levels.Length - 1);
            LevelDefinition level = catalog.Levels[Index];
            loadedDefinition = level;
            Current = Instantiate(level.Prefab, Vector3.zero, Quaternion.identity);
            Current.name = "LevelRoot • " + level.DisplayName;
            // Every dynamic ball lives in world space, independent of the rotating root.
            for (int i = 0; i < Current.BallCount; i++)
            {
                Transform spawn = Current.GetBallSpawn(i);
                BallController ball = Instantiate(catalog.BallPrefab, spawn.position, spawn.rotation);
                ball.name = "Ball " + (i + 1) + " (world space)";
                ball.Configure(catalog.BallProfile, Current.transform.TransformDirection(level.InitialLocalVelocity), level.Environment.IsZeroGravity);
                balls.Add(ball);
                if (i == 0) forces.Configure(ball, level.Environment); else forces.Register(ball);
                if (Current.BallCount > 1)
                {
                    var tint = new MaterialPropertyBlock();
                    tint.SetColor("_BaseColor", i % 2 == 0 ? new Color(.42f,.8f,.88f) : new Color(.95f,.67f,.3f));
                    foreach (Renderer renderer in ball.GetComponentsInChildren<Renderer>()) renderer.SetPropertyBlock(tint);
                }
            }
            Current.Initialize(balls, catalog.Rotation, level.RotationMode, forces);
            Current.Exit.BallExited += OnBallExited;
            Current.Exit.Exited += Complete;
            foreach (KillVolume hazard in Current.Hazards) hazard.Hit += Fail;
            ResetCount = 0;
            DragCount = 0;
            DragDistance = 0;
            startTime = Time.time;
            UnityEngine.Physics.SyncTransforms();
            Session.Activate();
            campaignProgress?.Visit(level.Id);
            Loaded?.Invoke(level);
            GameplayEvent?.Invoke("level_start");
        }

        public void ResetLevel()
        {
            if (Current == null) return;
            GameplayEvent?.Invoke("level_reset");
            Time.timeScale = 1;
            transitionAt = float.PositiveInfinity;
            ResetCount++;
            Current.ResetAll();
            Session.Activate();
        }

        public void RecordDrag(float distance, bool began)
        {
            if (Session.State != SessionState.Active) return;
            if (began) DragCount++;
            DragDistance += distance;
        }

        private void OnBallExited(BallController ball) => GameplayEvent?.Invoke("ball_exit_" + (balls.IndexOf(ball) + 1));

        private void Complete()
        {
            if (!Session.TryComplete()) return;
            campaignProgress?.Complete(Definition);
            GameplayEvent?.Invoke("level_complete");
            transitionAt = float.PositiveInfinity;
        }

        public void Fail()
        {
            if (!Session.TryFail()) return;
            foreach (BallController ball in balls) if (!Current.Exit.HasBallExited(ball)) ball.Capture(ball.Body.position);
            GameplayEvent?.Invoke("level_fail");
            transitionAt = Time.unscaledTime + catalog.FailureDelay;
        }

        public void Next()
        {
            if (catalog.IsCampaign && Index == catalog.Levels.Length - 1) return;
            Load((Index + 1) % catalog.Levels.Length);
        }

        public void TogglePause()
        {
            Session.TogglePause();
            GameplayEvent?.Invoke(Session.State == SessionState.Paused ? "level_pause" : "level_resume");
        }

        public void RecordEvent(string eventName) => GameplayEvent?.Invoke(eventName);

        private void Update()
        {
            if (Current == null) return;
            if (Session.State == SessionState.Active)
            {
                Current.Exit.EvaluateTraversal();
                foreach (BallController ball in balls)
                    if (!Current.Exit.HasBallExited(ball) && Current.IsOutside(ball.Body.position)) { Fail(); break; }
            }
            if (Time.unscaledTime < transitionAt) return;
            transitionAt = float.PositiveInfinity;
            if (Session.State == SessionState.Completing) Next();
            else if (Session.State == SessionState.Failed) ResetLevel();
        }

        private void OnStateChanged(SessionState state)
        {
            if (Current != null)
            {
                Current.Rotation.InputEnabled = state == SessionState.Active;
                Current.Exit.Accepting = state == SessionState.Active;
            }
            Time.timeScale = state == SessionState.Paused ? 0 : 1;
        }

        private void CleanupLevel()
        {
            forces.Clear();
            if (Current != null)
            {
                Current.Exit.Exited -= Complete;
                Current.Exit.BallExited -= OnBallExited;
                foreach (KillVolume hazard in Current.Hazards) hazard.Hit -= Fail;
                Current.ReleaseProps();
                Current.gameObject.SetActive(false);
                Destroy(Current.gameObject);
                Current = null;
            }
            foreach (BallController ball in balls)
            {
                if (ball == null) continue;
                ball.gameObject.SetActive(false);
                Destroy(ball.gameObject);
            }
            balls.Clear();
        }

        private void OnDestroy()
        {
            Session.Changed -= OnStateChanged;
            if (forces != null) CleanupLevel();
            Time.timeScale = 1;
        }
    }
}
