using System;
using System.Collections.Generic;
using System.IO;
using GravityBox.App;
using GravityBox.Gameplay;
using GravityBox.Presentation;
using GravityBox.Simulation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace GravityBox.Editor
{
    public static class CampaignPreviewCapture
    {
        public static void Capture()
        {
            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null) throw new InvalidOperationException("Render capture requires graphics.");
            var scene = EditorSceneManager.OpenScene(CampaignBuilder.ScenePath);
            var bootstrap = UnityEngine.Object.FindFirstObjectByType<GameBootstrap>(); bootstrap.enabled = false;
            Camera camera = bootstrap.GameplayCamera;
            camera.GetComponent<CameraRig>().enabled = false; camera.enabled = false;
            var catalog = AssetDatabase.LoadAssetAtPath<LevelCatalog>(CampaignBuilder.CatalogPath);
            const string output = "Artifacts/Campaign100"; Directory.CreateDirectory(output);
            var texture = new RenderTexture(720, 900, 24) { antiAliasing = 4 }; texture.Create();
            camera.targetTexture = texture; camera.aspect = 720f / 900;
            try
            {
                for (int index = 0; index < catalog.Levels.Length; index++)
                {
                    LevelDefinition definition = catalog.Levels[index];
                    LevelRuntime level = UnityEngine.Object.Instantiate(definition.Prefab);
                    var balls = new List<BallController>();
                    try
                    {
                        for (int b = 0; b < level.BallCount; b++)
                        {
                            var ball = UnityEngine.Object.Instantiate(catalog.BallPrefab);
                            ball.transform.position = level.GetBallSpawn(b).position; ball.Body.position = ball.transform.position;
                            ball.Body.isKinematic = true; balls.Add(ball);
                            if (level.BallCount > 1)
                            {
                                var tint = new MaterialPropertyBlock(); tint.SetColor("_BaseColor", b == 0 ? new Color(.42f,.8f,.88f) : new Color(.95f,.67f,.3f));
                                foreach (Renderer renderer in ball.GetComponentsInChildren<Renderer>()) renderer.SetPropertyBlock(tint);
                            }
                        }
                        var boss = level.GetComponent<BossPresentation>(); if (boss != null) boss.Bind(balls);
                        camera.backgroundColor = definition.Environment.Background;
                        float framing = level.BoundsHalfExtent * .98f; camera.fieldOfView = 40; camera.nearClipPlane = .002f; camera.farClipPlane = 30;
                        float angle = Mathf.Min(20 * Mathf.Deg2Rad, Mathf.Atan(Mathf.Tan(20 * Mathf.Deg2Rad) * camera.aspect));
                        float distance = framing / Mathf.Sin(angle);
                        camera.transform.position = new Vector3(6,11,-12).normalized * distance; camera.transform.LookAt(Vector3.zero);
                        MazePreviewCapture.Render(camera, texture, $"{output}/C{index + 1:000}.png");
                        if (definition.Design.IsBoss)
                        {
                            camera.transform.position = new Vector3(0,16,-5).normalized * distance; camera.transform.LookAt(Vector3.zero);
                            MazePreviewCapture.Render(camera, texture, $"{output}/C{index + 1:000}-top.png");
                        }
                    }
                    finally
                    {
                        foreach (var ball in balls) UnityEngine.Object.DestroyImmediate(ball.gameObject);
                        UnityEngine.Object.DestroyImmediate(level.gameObject);
                    }
                }
            }
            finally { camera.targetTexture = null; texture.Release(); UnityEngine.Object.DestroyImmediate(texture); }
            File.WriteAllText(output + "/README.txt", "100 authored-spawn geometry renders, plus boss top views. These are visual inspection fixtures, not gameplay solution proofs.\n");
            Debug.Log("CAMPAIGN CAPTURES COMPLETE");
        }
    }
}
