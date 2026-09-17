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
    /// <summary>Exports the existing ten Origin scenes for a physical iPhone.
    /// Signing credentials stay in the local environment, outside the repository.</summary>
    public static class COgheIOSBuilder
    {
        public const string Output="Builds/Venom/iOS";
        public const string BundleId="com.gravityboxlab.venom";

        [MenuItem("Gravity Box/COghe/Build iPhone Xcode project")]
        public static void Build()
        {
            if(!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.iOS,BuildTarget.iOS))
                throw new InvalidOperationException("Install iOS Build Support for this Unity version first.");
            var scenes=VenomCampaignBuilder.CampaignScenePaths();

            var platform=NamedBuildTarget.iOS;
            string product=PlayerSettings.productName,identifier=PlayerSettings.GetApplicationIdentifier(platform);
            var sdk=PlayerSettings.iOS.sdkVersion;
            var backend=PlayerSettings.GetScriptingBackend(platform);
            var configuration=PlayerSettings.GetIl2CppCompilerConfiguration(platform);
            var orientation=PlayerSettings.defaultInterfaceOrientation;
            try
            {
                PlayerSettings.productName="COghe";
                PlayerSettings.SetApplicationIdentifier(platform,BundleId);
                PlayerSettings.iOS.sdkVersion=iOSSdkVersion.DeviceSDK;
                PlayerSettings.SetScriptingBackend(platform,ScriptingImplementation.IL2CPP);
                PlayerSettings.SetIl2CppCompilerConfiguration(platform,Il2CppCompilerConfiguration.Release);
                PlayerSettings.defaultInterfaceOrientation=UIOrientation.Portrait;
                Directory.CreateDirectory(Output);
                var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{
                    scenes=scenes,target=BuildTarget.iOS,locationPathName=Output,options=BuildOptions.None});
                if(report.summary.result!=BuildResult.Succeeded)
                    throw new Exception("COghe iOS export failed: "+report.summary.result);
#if UNITY_IOS
                ConfigureSigning();
#endif
                Debug.Log("COGHE IOS EXPORT SUCCESS: "+Path.GetFullPath(Output));
            }
            finally
            {
                PlayerSettings.productName=product;
                PlayerSettings.SetApplicationIdentifier(platform,identifier);
                PlayerSettings.iOS.sdkVersion=sdk;
                PlayerSettings.SetScriptingBackend(platform,backend);
                PlayerSettings.SetIl2CppCompilerConfiguration(platform,configuration);
                PlayerSettings.defaultInterfaceOrientation=orientation;
            }
        }
#if UNITY_IOS
        private static void ConfigureSigning()
        {
            string team=Environment.GetEnvironmentVariable("COGHE_IOS_TEAM");
            string profile=Environment.GetEnvironmentVariable("COGHE_IOS_PROFILE");
            if(string.IsNullOrEmpty(team))return;
            bool manual=!string.IsNullOrEmpty(profile);
            string path=PBXProject.GetPBXProjectPath(Output);
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
