using GravityBox.Foundation;
using GravityBox.Gameplay;
using GravityBox.Presentation;
using GravityBox.Simulation;
using UnityEngine;

namespace GravityBox.App
{
    [DefaultExecutionOrder(-100)]
    public sealed class GameBootstrap : MonoBehaviour
    {
        public LevelCatalog Catalog;
        public LevelCatalog PhysicsLab;
        public bool ResumeCampaign = true;
        public Camera GameplayCamera;
        public GameFeedback Feedback;
        public bool RecordPlaytest = true;
        public LevelManager Levels { get; private set; }

        private void Awake()
        {
            Application.targetFrameRate = 60;
            PhysicsTiming.Apply();
            var forces = new GameObject("Environment Force System").AddComponent<EnvironmentForceSystem>();
            forces.transform.SetParent(transform, false);
            Levels = new GameObject("Level Manager").AddComponent<LevelManager>();
            Levels.transform.SetParent(transform, false);
            Levels.Initialize(Catalog, forces, PhysicsLab, ResumeCampaign && Catalog.IsCampaign);
            var hud = new GameObject("HUD").AddComponent<GameHud>();
            hud.transform.SetParent(transform, false);
            hud.Initialize(Levels);
            GameplayCamera.GetComponent<CameraRig>()?.Initialize(Levels);
            var replay = gameObject.AddComponent<BossReplay>();
            replay.Initialize(Levels, GameplayCamera.GetComponent<CameraRig>());
            hud.AttachReplay(replay);
            hud.AttachInspection(GameplayCamera.GetComponent<CameraRig>());
            var input = new GameObject("Rotation Input").AddComponent<RotationInputController>();
            input.transform.SetParent(transform, false);
            input.Initialize(Levels, GameplayCamera, hud);
            if (Feedback != null) Feedback.Initialize(Levels, GameplayCamera);
            if (RecordPlaytest) gameObject.AddComponent<LocalPlaytestRecorder>().Initialize(Levels);
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused && Levels != null && Levels.Session.State == SessionState.Active) Levels.TogglePause();
        }
    }
}
