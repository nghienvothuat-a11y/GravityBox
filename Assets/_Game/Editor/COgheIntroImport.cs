using UnityEditor;
using UnityEngine;

namespace GravityBox.Editor
{
    /// <summary>
    /// Import settings for the intro art (Resources/COgheIntro, written by Tools/intro/prepare_intro_layers.py at their
    /// final size). ASTC 8×8 on phones keeps the whole comic to a few MB. No mipmaps: the layers are drawn near 1:1, and on
    /// Android a non-power-of-two texture with mipmaps is stored uncompressed (the lab plate went from 0.35 to 5.5 MB).
    /// Replacement files with the same names get the same settings.
    /// </summary>
    public sealed class COgheIntroImport : AssetPostprocessor
    {
        public override uint GetVersion() => 2;   // bump when the settings change: the intro art reimports

        private void OnPreprocessTexture()
        {
            if (!assetPath.Contains("/Resources/COgheIntro/")) return;
            var importer = (TextureImporter)assetImporter;
            string name = System.IO.Path.GetFileNameWithoutExtension(assetPath);
            importer.textureType = TextureImporterType.Default;
            importer.sRGBTexture = true;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.alphaIsTransparency = true;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.isReadable = false;
            importer.maxTextureSize = 2048;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            foreach (var platform in new[] { "Android", "iPhone" })
            {
                var settings = importer.GetPlatformTextureSettings(platform);
                settings.overridden = true; settings.maxTextureSize = 2048; settings.format = TextureImporterFormat.ASTC_8x8;
                importer.SetPlatformTextureSettings(settings);
            }
        }
    }
}
