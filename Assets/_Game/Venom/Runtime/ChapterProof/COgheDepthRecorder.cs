#if (DEVELOPMENT_BUILD || COGHE_MOBILE_BENCHMARK) && !UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using UnityEngine;
namespace GravityBox.Venom.ChapterProof
{
    /// <summary>Explicit opt-in visual evidence. Recording runs must not be used for FPS comparisons.</summary>
    public sealed class COgheDepthRecorder:MonoBehaviour
    {
        private string directory;private int frame;private StreamWriter times;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Begin()
        {
            var args=Environment.GetCommandLineArgs();
            for(int i=0;i<args.Length-1;i++)if(args[i]=="-coghe-depth-record")
            {var go=new GameObject("Depth study optional recording");DontDestroyOnLoad(go);go.AddComponent<COgheDepthRecorder>().directory=Path.GetFullPath(args[i+1]);return;}
        }
        private IEnumerator Start()
        {
            Directory.CreateDirectory(directory);times=new StreamWriter(Path.Combine(directory,"times.csv"));times.WriteLine("frame,seconds");
            while(true)
            {
                yield return new WaitForEndOfFrame();
                double t=Time.realtimeSinceStartupAsDouble;var image=ScreenCapture.CaptureScreenshotAsTexture();
                try{File.WriteAllBytes(Path.Combine(directory,$"{frame:00000}.png"),image.EncodeToPNG());}
                finally{Destroy(image);}
                times.WriteLine((frame++).ToString()+","+t.ToString("F6",System.Globalization.CultureInfo.InvariantCulture));times.Flush();
                yield return new WaitForSecondsRealtime(.125f);
            }
        }
        private void OnDestroy(){times?.Dispose();}
    }
}
#endif
