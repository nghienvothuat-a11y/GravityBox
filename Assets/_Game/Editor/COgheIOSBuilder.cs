using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif

namespace GravityBox.Editor
{
    /// <summary>Exports an existing COghe catalog for a physical iPhone or iPad.
    /// Signing credentials stay in the local environment, outside the repository.</summary>
    public static class COgheIOSBuilder
    {
        public const string Output="Builds/Venom/iOS";
        public const string BundleId="com.gravityboxlab.venom";

        [MenuItem("Gravity Box/COghe/Build iPhone Xcode project")]
        public static void Build() => BuildScenes(VenomCampaignBuilder.CampaignScenePaths(), Output);

        [MenuItem("Gravity Box/COghe/Spatial pilot/Build iPhone-iPad Xcode project · 50 levels")]
        public static void BuildSpatial() => BuildScenes(VenomCampaignBuilder.SpatialScenePaths(), "Builds/SpatialLab/iOS");

        private static void BuildScenes(string[] scenes, string output)
        {
            if(!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.iOS,BuildTarget.iOS))
                throw new InvalidOperationException("Install iOS Build Support for this Unity version first.");
            foreach(var scene in scenes)if(!File.Exists(scene))throw new FileNotFoundException("Generate campaign before building",scene);

            var platform=NamedBuildTarget.iOS;
            string product=PlayerSettings.productName,identifier=PlayerSettings.GetApplicationIdentifier(platform);
            var sdk=PlayerSettings.iOS.sdkVersion;
            var backend=PlayerSettings.GetScriptingBackend(platform);
            var configuration=PlayerSettings.GetIl2CppCompilerConfiguration(platform);
            var orientation=PlayerSettings.defaultInterfaceOrientation;
            var device=PlayerSettings.iOS.targetDevice;
            var adsAsset=AssetDatabase.LoadMainAssetAtPath("Assets/GoogleMobileAds/Resources/GoogleMobileAdsSettings.asset");
            SerializedObject adsSettings=adsAsset!=null?new SerializedObject(adsAsset):null;
            var adsId=adsSettings?.FindProperty("adMobIOSAppId");
            string originalAdsId=adsId?.stringValue;
            try
            {
                PlayerSettings.productName="COghe";
                PlayerSettings.SetApplicationIdentifier(platform,BundleId);
                PlayerSettings.iOS.sdkVersion=iOSSdkVersion.DeviceSDK;
                PlayerSettings.SetScriptingBackend(platform,ScriptingImplementation.IL2CPP);
                PlayerSettings.SetIl2CppCompilerConfiguration(platform,Il2CppCompilerConfiguration.Release);
                PlayerSettings.defaultInterfaceOrientation=UIOrientation.Portrait;
                PlayerSettings.iOS.targetDevice=iOSTargetDevice.iPhoneAndiPad;
                if(adsId!=null&&string.IsNullOrWhiteSpace(originalAdsId))
                {
                    if(Environment.GetEnvironmentVariable("COGHE_STORE")=="1")
                        throw new InvalidOperationException("Configure an iOS AdMob app before exporting a store build.");
                    // Official Google sample app ID, used only by this local test export.
                    adsId.stringValue="ca-app-pub-3940256099942544~1458002511";
                    adsSettings.ApplyModifiedPropertiesWithoutUndo();AssetDatabase.SaveAssetIfDirty(adsAsset);
                }
                Directory.CreateDirectory(output);
                var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{
                    scenes=scenes,target=BuildTarget.iOS,locationPathName=output,options=BuildOptions.None,extraScriptingDefines=COgheAndroidBuilder.Defines()});
                if(report.summary.result!=BuildResult.Succeeded)
                    throw new Exception("COghe iOS export failed: "+report.summary.result);
#if UNITY_IOS
                ConfigureSigning(output);
#endif
                Debug.Log("COGHE IOS EXPORT SUCCESS: "+scenes.Length+" scenes, "+Path.GetFullPath(output));
            }
            finally
            {
                PlayerSettings.productName=product;
                PlayerSettings.SetApplicationIdentifier(platform,identifier);
                PlayerSettings.iOS.sdkVersion=sdk;
                PlayerSettings.SetScriptingBackend(platform,backend);
                PlayerSettings.SetIl2CppCompilerConfiguration(platform,configuration);
                PlayerSettings.defaultInterfaceOrientation=orientation;
                PlayerSettings.iOS.targetDevice=device;
                if(adsId!=null)
                {
                    adsSettings.Update();adsId=adsSettings.FindProperty("adMobIOSAppId");
                    adsId.stringValue=originalAdsId;adsSettings.ApplyModifiedPropertiesWithoutUndo();AssetDatabase.SaveAssetIfDirty(adsAsset);
                }
            }
        }
#if UNITY_IOS
        private static void ConfigureSigning(string output)
        {
            string team=Environment.GetEnvironmentVariable("COGHE_IOS_TEAM");
            string profile=Environment.GetEnvironmentVariable("COGHE_IOS_PROFILE");
            if(string.IsNullOrEmpty(team))return;
            bool manual=!string.IsNullOrEmpty(profile);
            string path=PBXProject.GetPBXProjectPath(output);
            var project=new PBXProject();project.ReadFromFile(path);
            string main=project.GetUnityMainTargetGuid(),framework=project.GetUnityFrameworkTargetGuid();
            foreach(string target in new[]{main,framework})
            {
                project.SetBuildProperty(target,"DEVELOPMENT_TEAM",team);
                project.SetBuildProperty(target,"CODE_SIGN_STYLE",manual?"Manual":"Automatic");
                project.SetBuildProperty(target,"CODE_SIGN_IDENTITY","Apple Development");
                project.SetBuildProperty(target,"PROVISIONING_PROFILE_SPECIFIER",manual&&target==main?profile:"");
            }
            project.WriteToFile(path);
        }
#endif
    }
}
