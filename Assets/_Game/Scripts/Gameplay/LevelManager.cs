using System;
using GravityBox.Foundation;
using GravityBox.Simulation;
using UnityEngine;

namespace GravityBox.Gameplay
{
    public sealed class LevelManager : MonoBehaviour
    {
        private LevelCatalog catalog;
        private EnvironmentForceSystem forces;
        private float transitionAt;
        private float startTime;
        public readonly GameSession Session = new GameSession();
        public LevelRuntime Current { get; private set; }
        public BallController Ball { get; private set; }
        public LevelDefinition Definition => catalog.Levels[Index];
        public LevelCatalog Catalog => catalog;
        public int Index { get; private set; }
        public int ResetCount { get; private set; }
        public int DragCount { get; private set; }
        public float DragDistance { get; private set; }
        public float Elapsed => Mathf.Max(0, Time.time - startTime);
        public event Action<LevelDefinition> Loaded;
        public event Action<string> GameplayEvent;

        public void Initialize(LevelCatalog config, EnvironmentForceSystem forceSystem)
        {
            if (config == null || config.Levels == null || config.Levels.Length == 0)
                throw new InvalidOperationException("Gravity Box requires a non-empty LevelCatalog.");
            catalog = config;
            forces = forceSystem;
            Session.Changed += OnStateChanged;
            Load(0);
        }

        public void Load(int index)
        {
            Session.BeginLoading();
            Time.timeScale = 1;
            transitionAt = float.PositiveInfinity;
            CleanupLevel();
            Index = Mathf.Clamp(index, 0, catalog.Levels.Length - 1);
            LevelDefinition level = Definition;
            Current = Instantiate(level.Prefab, Vector3.zero, Quaternion.identity);
            Current.name = "LevelRoot • " + level.DisplayName;
            // A dynamic ball must never inherit LevelRoot transforms.
            Ball = Instantiate(catalog.BallPrefab, Current.BallSpawn.position, Current.BallSpawn.rotation);
            Ball.name = "Ball (world space)";
            Ball.Configure(catalog.BallProfile, Current.transform.TransformDirection(level.InitialLocalVelocity), level.Environment.IsZeroGravity);
            forces.Configure(Ball, level.Environment);
            Current.Initialize(Ball, catalog.Rotation, level.RotationMode);
            Current.Exit.Exited += Complete;
            foreach (KillVolume hazard in Current.Hazards) hazard.Hit += Fail;
            ResetCount = 0;
            DragCount = 0;
            DragDistance = 0;
            startTime = Time.time;
            UnityEngine.Physics.SyncTransforms();
            Session.Activate();
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

        private void Complete()
        {
            if (!Session.TryComplete()) return;
            GameplayEvent?.Invoke("level_complete");
            transitionAt = Time.unscaledTime + catalog.CompletionDelay;
        }

        public void Fail()
        {
            if (!Session.TryFail()) return;
            Ball.Capture(Ball.Body.position);
            GameplayEvent?.Invoke("level_fail");
            transitionAt = Time.unscaledTime + catalog.FailureDelay;
        }

        public void Next()
        {
            if (Index + 1 < catalog.Levels.Length) Load(Index + 1);
            else
            {
                transitionAt = float.PositiveInfinity;
                Ball.Capture(Ball.Body.position);
                Session.Finish();
                GameplayEvent?.Invoke("catalog_complete");
            }
        }

        public void TogglePause() => Session.TogglePause();

        private void Update()
        {
            if (Current == null) return;
            if (Session.State == SessionState.Active)
            {
                Current.Exit.EvaluateTraversal();
                if (!Current.Exit.HasExited && Current.IsOutside(Ball.Body.position)) Fail();
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
            Time.timeScale = state == SessionState.Paused ? 0 : state == SessionState.Completing ? 0.35f : 1;
        }

        private void CleanupLevel()
        {
            forces.Clear();
            if (Current != null)
            {
                Current.Exit.Exited -= Complete;
                foreach (KillVolume hazard in Current.Hazards) hazard.Hit -= Fail;
                Current.gameObject.SetActive(false);
                Destroy(Current.gameObject);
                Current = null;
            }
            if (Ball != null)
            {
                Ball.gameObject.SetActive(false);
                Destroy(Ball.gameObject);
                Ball = null;
            }
        }

        private void OnDestroy()
        {
            Session.Changed -= OnStateChanged;
            if (forces != null) CleanupLevel();
            Time.timeScale = 1;
        }
    }
}
