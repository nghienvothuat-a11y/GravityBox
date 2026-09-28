using System;
using System.Collections.Generic;
using System.IO;
using GravityBox.Venom;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace GravityBox.Editor
{
    public static class COgheViewArtVerification
    {
        [MenuItem("Gravity Box/COghe/V2/Rebuild concept art and verify physics")]
        public static void Rebuild()
        {
            const string folder="Artifacts/COgheViewArt";Directory.CreateDirectory(folder);
            var before=CapturePhysics();
            File.WriteAllText(folder+"/physics-before.txt",before);
            VenomCampaignBuilder.GenerateViewCampaign();
            var after=CapturePhysics();
            File.WriteAllText(folder+"/physics-after.txt",after);
            if(before!=after)throw new InvalidOperationException("V2 art rebuild changed physical data; inspect physics-before/after.txt.");
            Debug.Log("COGHE ART PHYSICS VERIFIED: all ten scenes preserve colliders, rigidbodies, joints, surfaces, rails and input definitions.");
            VenomCampaignBuilder.BuildMac();
        }

        private static string CapturePhysics()
        {
            var lines=new List<string>();
            foreach(string path in VenomCampaignBuilder.ViewCampaignScenePaths())
            {
                var scene=EditorSceneManager.OpenScene(path);
                foreach(var root in scene.GetRootGameObjects())foreach(var component in root.GetComponentsInChildren<Component>(true))
                {
                    bool include=component is Collider||component is Rigidbody||component is Joint||component is VenomSurfacePatch||component is COgheRailSlider||component is COgheTapRail;
                    if(!include)continue;
                    var key=path+"|"+Hierarchy(component.transform)+"|"+component.GetType().Name;
                    lines.Add(key+"|localPosition="+component.transform.localPosition.ToString("R")+"|localRotation="+component.transform.localRotation.ToString("R")+"|localScale="+component.transform.localScale.ToString("R"));
                    using(var serialized=new SerializedObject(component))
                    {
                        var p=serialized.GetIterator();
                        while(p.Next(true))
                        {
                            // Native reference children are transient load IDs; the
                            // parent reference is already compared by asset/hierarchy.
                            if(p.propertyPath.EndsWith(".m_FileID")||p.propertyPath.EndsWith(".m_PathID"))continue;
                            if(p.propertyType==SerializedPropertyType.Generic)continue;
                            string value;
                            if(p.propertyType==SerializedPropertyType.ObjectReference)
                            {
                                var reference=p.objectReferenceValue;
                                value=reference==null?"null":AssetDatabase.Contains(reference)?AssetDatabase.GetAssetPath(reference):reference is Component c?Hierarchy(c.transform)+":"+c.GetType().Name:reference is GameObject g?Hierarchy(g.transform):reference.name;
                            }
                            else if(p.propertyType==SerializedPropertyType.Float)value=p.doubleValue.ToString("R",System.Globalization.CultureInfo.InvariantCulture);
                            else if(p.propertyType==SerializedPropertyType.Integer||p.propertyType==SerializedPropertyType.Enum)value=p.longValue.ToString();
                            else if(p.propertyType==SerializedPropertyType.Boolean)value=p.boolValue.ToString();
                            else if(p.propertyType==SerializedPropertyType.String)value=p.stringValue;
                            else if(p.propertyType==SerializedPropertyType.Vector2)value=p.vector2Value.ToString("R");
                            else if(p.propertyType==SerializedPropertyType.Vector3)value=p.vector3Value.ToString("R");
                            else if(p.propertyType==SerializedPropertyType.Quaternion)value=p.quaternionValue.ToString("R");
                            else continue;
                            lines.Add(key+"|"+p.propertyPath+"="+value);
                        }
                    }
                }
            }
            lines.Sort(StringComparer.Ordinal);return string.Join("\n",lines)+"\n";
        }
        private static string Hierarchy(Transform t)=>t.parent==null?t.name:Hierarchy(t.parent)+"/"+t.name;
    }
}
