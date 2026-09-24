using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace GravityBox.Editor
{
    /// <summary>Builds all current Origin scenes as a locally signed ARM64 APK.</summary>
    public static class COgheAndroidBuilder
    {
        public const string Output="Builds/Venom/Android/COghe.apk";
        public const string BundleId="com.gravityboxlab.venom";

        [MenuItem("Gravity Box/COghe/Build Android test APK")]
        public static void Build() => BuildScenes(VenomCampaignBuilder.CampaignScenePaths(), Output);

        [MenuItem("Gravity Box/COghe/Build Tap Campaign · Android test APK")]
        public static void BuildTap() => BuildScenes(VenomCampaignBuilder.TapCampaignScenePaths(), "Builds/COgheTapChapter/Android/COghe.apk");

        [MenuItem("Gravity Box/COghe/Onboarding/Build pilot Android test APK")]
        public static void BuildOnboarding() => BuildScenes(COgheOnboardingBuilder.ScenePaths(),
            "Builds/COgheOnboarding/Android/COghe-Learn.apk",BundleId+".onboarding","COghe Learn");

        private static void BuildScenes(string[] scenes, string output,string bundleId=BundleId,string displayName="COghe")
        {
            if(!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android,BuildTarget.Android))
                throw new InvalidOperationException("Install Android Build Support for this Unity version first.");
            foreach(var scene in scenes)if(!File.Exists(scene))throw new FileNotFoundException("Generate campaign before building",scene);

            var platform=NamedBuildTarget.Android;
            string product=PlayerSettings.productName,identifier=PlayerSettings.GetApplicationIdentifier(platform);
            var backend=PlayerSettings.GetScriptingBackend(platform);
            var configuration=PlayerSettings.GetIl2CppCompilerConfiguration(platform);
            var architectures=PlayerSettings.Android.targetArchitectures;
            var orientation=PlayerSettings.defaultInterfaceOrientation;
            bool bundle=EditorUserBuildSettings.buildAppBundle,export=EditorUserBuildSettings.exportAsGoogleAndroidProject;
            bool customKey=PlayerSettings.Android.useCustomKeystore;
            try
            {
                PlayerSettings.productName=displayName;
                PlayerSettings.SetApplicationIdentifier(platform,bundleId);
                PlayerSettings.SetScriptingBackend(platform,ScriptingImplementation.IL2CPP);
                PlayerSettings.SetIl2CppCompilerConfiguration(platform,Il2CppCompilerConfiguration.Release);
                PlayerSettings.Android.targetArchitectures=AndroidArchitecture.ARM64;
                PlayerSettings.defaultInterfaceOrientation=UIOrientation.Portrait;
                PlayerSettings.Android.useCustomKeystore=false;
                EditorUserBuildSettings.buildAppBundle=false;
                EditorUserBuildSettings.exportAsGoogleAndroidProject=false;
                Directory.CreateDirectory(Path.GetDirectoryName(output));
                var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{
                    scenes=scenes,target=BuildTarget.Android,locationPathName=output,options=BuildOptions.None,
                    extraScriptingDefines=Environment.GetEnvironmentVariable("COGHE_BENCHMARK")=="1"?
                        new[]{"COGHE_MOBILE_BENCHMARK"}:Array.Empty<string>()});
                if(report.summary.result!=BuildResult.Succeeded)
                    throw new Exception("COghe Android build failed: "+report.summary.result);
                Debug.Log("COGHE ANDROID BUILD SUCCESS: "+Path.GetFullPath(output));
            }
            finally
            {
                PlayerSettings.productName=product;
                PlayerSettings.SetApplicationIdentifier(platform,identifier);
                PlayerSettings.SetScriptingBackend(platform,backend);
                PlayerSettings.SetIl2CppCompilerConfiguration(platform,configuration);
                PlayerSettings.Android.targetArchitectures=architectures;
                PlayerSettings.defaultInterfaceOrientation=orientation;
                PlayerSettings.Android.useCustomKeystore=customKey;
                EditorUserBuildSettings.buildAppBundle=bundle;
                EditorUserBuildSettings.exportAsGoogleAndroidProject=export;
            }
        }
    }
}
