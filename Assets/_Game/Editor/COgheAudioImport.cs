using UnityEditor;
using UnityEngine;

namespace GravityBox.Editor
{
    /// <summary>
    /// Import settings for COghe sound (Resources/COgheAudio). Music streams (a 160 s loop decompressed would take
    /// about 28 MB of memory); the room tone stays compressed in memory; short effects decompress on load so they
    /// start instantly. Replacement files with the same names get the same settings.
    /// </summary>
    public sealed class COgheAudioImport : AssetPostprocessor
    {
        private void OnPreprocessAudio()
        {
            if (!assetPath.Contains("/Resources/COgheAudio/")) return;
            var importer = (AudioImporter)assetImporter;
            string name = System.IO.Path.GetFileNameWithoutExtension(assetPath);
            var settings = importer.defaultSampleSettings;
            settings.compressionFormat = AudioCompressionFormat.Vorbis;
            if (name.StartsWith("music_"))
            {
                settings.loadType = AudioClipLoadType.Streaming; settings.quality = .5f;
                importer.loadInBackground = true;
            }
            else if (name.StartsWith("ambience_"))
            {
                settings.loadType = AudioClipLoadType.CompressedInMemory; settings.quality = .4f;
                importer.loadInBackground = true;
            }
            else
            {
                settings.loadType = AudioClipLoadType.DecompressOnLoad; settings.quality = .6f;
                importer.loadInBackground = false;
            }
            settings.sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate;
            settings.preloadAudioData = !name.StartsWith("music_");
            importer.defaultSampleSettings = settings;
        }
    }
}
