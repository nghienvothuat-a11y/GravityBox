using System.Globalization;
using System.IO;
using System.Text;
using GravityBox.Venom;
using UnityEditor;

namespace GravityBox.Editor
{
    public static class COgheMonetizationConfigExport
    {
        [MenuItem("Gravity Box/COghe/Export monetization Remote Config")]
        public static void Export()
        {
            var json = new StringBuilder("{\n  \"parameters\": {\n"); bool first = true;
            foreach (var pair in COgheEconomy.RemoteDefaults())
            {
                if (!first) json.Append(",\n"); first = false;
                var value = System.Convert.ToString(pair.Value, CultureInfo.InvariantCulture);
                json.Append("    \"").Append(pair.Key).Append("\": { \"defaultValue\": { \"value\": \"")
                    .Append(value).Append("\" }, \"valueType\": \"").Append(pair.Value is int ? "NUMBER" : "STRING").Append("\" }");
            }
            json.Append("\n  },\n  \"conditions\": []\n}\n");
            Directory.CreateDirectory("Docs/Monetization/COghe");
            File.WriteAllText("Docs/Monetization/COghe/remote-config.defaults.json", json.ToString());
            UnityEngine.Debug.Log("COghe Remote Config defaults exported.");
        }
    }
}
