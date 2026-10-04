using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using UnityEditor;
using UnityEditor.Android;
using UnityEditor.Build;
using UnityEngine;

namespace GravityBox.Editor
{
    /// <summary>The builder temporarily sets the Android package name; Firebase's editor watcher can
    /// miss that change in batch mode. Generate native resources from the matching client at build time.</summary>
    public sealed class COgheFirebaseAndroidConfig : IPostGenerateGradleAndroidProject
    {
        public int callbackOrder => 1000;
        public void OnPostGenerateGradleAndroidProject(string path)
        {
            if (PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.Android) != COgheAndroidBuilder.BundleId) return;
            string source = Path.Combine(Application.dataPath, "google-services.json");
            if (!File.Exists(source)) throw new BuildFailedException("COghe: missing Assets/google-services.json.");
            var config = JsonUtility.FromJson<Config>(File.ReadAllText(source));
            var client = config.client?.FirstOrDefault(c => c.client_info?.android_client_info?.package_name == COgheAndroidBuilder.BundleId);
            if (client == null || config.project_info?.project_id != "coghe-57f21" ||
                string.IsNullOrEmpty(client.client_info.mobilesdk_app_id) || client.api_key == null || client.api_key.Length == 0)
                throw new BuildFailedException("COghe: Firebase config must match the COghe Android app and project.");
            var resources = new XElement("resources");
            void Add(string name, string value)
            {
                if (!string.IsNullOrEmpty(value)) resources.Add(new XElement("string",
                    new XAttribute("name", name), new XAttribute("translatable", "false"), value));
            }
            Add("google_app_id", client.client_info.mobilesdk_app_id);
            Add("gcm_defaultSenderId", config.project_info.project_number);
            Add("project_id", config.project_info.project_id);
            Add("google_storage_bucket", config.project_info.storage_bucket);
            Add("google_api_key", client.api_key[0].current_key);
            Add("google_crash_reporting_api_key", client.api_key[0].current_key);
            string folder = Path.Combine(path, "src/main/res/values"); Directory.CreateDirectory(folder);
            string destination = Path.Combine(folder, "coghe_google_services.xml");
            // A future SDK may restore automatic generation; avoid duplicate resources in this module.
            foreach (string file in Directory.GetFiles(folder, "*.xml"))
                if (file != destination)
                {
                    var appId = XDocument.Load(file).Descendants("string").FirstOrDefault(x => (string)x.Attribute("name") == "google_app_id");
                    if (appId == null) continue;
                    if (appId.Value != client.client_info.mobilesdk_app_id)
                        throw new BuildFailedException("COghe: generated Firebase Android resources refer to a different app.");
                    if (File.Exists(destination)) File.Delete(destination);
                    return;
                }
            new XDocument(resources).Save(destination);
            Debug.Log("COghe: Firebase Android resources verified for coghe-57f21.");
        }
        [Serializable] private sealed class Config { public Project project_info; public Client[] client; }
        [Serializable] private sealed class Project { public string project_id, project_number, storage_bucket; }
        [Serializable] private sealed class Client { public ClientInfo client_info; public Key[] api_key; }
        [Serializable] private sealed class ClientInfo { public string mobilesdk_app_id; public AndroidInfo android_client_info; }
        [Serializable] private sealed class AndroidInfo { public string package_name; }
        [Serializable] private sealed class Key { public string current_key; }
    }
}
