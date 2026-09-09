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
    public static class MechanicalPreviewCapture
    {
        public static void Capture()
        {
            if(SystemInfo.graphicsDeviceType==GraphicsDeviceType.Null)throw new InvalidOperationException("Capture needs graphics.");
            var scene=EditorSceneManager.OpenScene(PrototypeBuilder.ScenePath);
            Camera camera=null;
            foreach(GameObject go in scene.GetRootGameObjects())
            {
                var bootstrap=go.GetComponent<GameBootstrap>();
                if(bootstrap!=null){bootstrap.enabled=false;camera=bootstrap.GameplayCamera;}
            }
            if(camera==null)throw new InvalidOperationException("Gameplay camera missing.");
            camera.GetComponent<CameraRig>().enabled=false;camera.enabled=false;camera.clearFlags=CameraClearFlags.SolidColor;
            var catalog=AssetDatabase.LoadAssetAtPath<LevelCatalog>(PrototypeBuilder.CatalogPath);
            string output=Path.Combine(Directory.GetParent(Application.dataPath).FullName,"Artifacts/Mechanical17-23");
            Directory.CreateDirectory(output);
            var texture=new RenderTexture(900,1100,24){antiAliasing=4};texture.Create();camera.targetTexture=texture;camera.aspect=900f/1100;
            try
            {
                for(int index=16;index<catalog.Levels.Length;index++)
                {
                    var definition=catalog.Levels[index];var level=UnityEngine.Object.Instantiate(definition.Prefab);
                    var balls=new List<BallController>();
                    try
                    {
                        for(int i=0;i<level.BallCount;i++)
                        {
                            var ball=UnityEngine.Object.Instantiate(catalog.BallPrefab);ball.transform.position=level.GetBallSpawn(i).position;
                            ball.Body.position=ball.transform.position;ball.Body.isKinematic=true;balls.Add(ball);
                            if(level.BallCount>1)
                            {
                                var tint=new MaterialPropertyBlock();tint.SetColor("_BaseColor",i==0?new Color(.42f,.8f,.88f):new Color(.95f,.67f,.3f));
                                foreach(Renderer renderer in ball.GetComponentsInChildren<Renderer>())renderer.SetPropertyBlock(tint);
                            }
                        }
                        camera.backgroundColor=definition.Environment.Background;
                        float framing=level.BoundsHalfExtent*.95f;camera.fieldOfView=40;camera.nearClipPlane=.005f;camera.farClipPlane=20;
                        float angle=Mathf.Min(20*Mathf.Deg2Rad,Mathf.Atan(Mathf.Tan(20*Mathf.Deg2Rad)*camera.aspect));
                        float distance=framing/Mathf.Sin(angle);
                        camera.transform.position=new Vector3(6,11,-12).normalized*distance;camera.transform.LookAt(Vector3.zero);
                        MazePreviewCapture.Render(camera,texture,Path.Combine(output,$"Level{index+1:00}.png"));
                        camera.transform.position=new Vector3(0,16,-5).normalized*distance;camera.transform.LookAt(Vector3.zero);
                        MazePreviewCapture.Render(camera,texture,Path.Combine(output,$"Level{index+1:00}-top.png"));
                    }
                    finally
                    {
                        foreach(var ball in balls)UnityEngine.Object.DestroyImmediate(ball.gameObject);
                        UnityEngine.Object.DestroyImmediate(level.gameObject);
                    }
                }
                File.WriteAllText(Path.Combine(output,"README.txt"),"Authored geometry render fixtures at original spawns. No simulated or human solution is claimed by these images. See PlayMode results for physical route verification.\nCaptured UTC: "+DateTime.UtcNow.ToString("O"));
            }
            finally { camera.targetTexture=null;texture.Release();UnityEngine.Object.DestroyImmediate(texture); }
        }
    }
}
