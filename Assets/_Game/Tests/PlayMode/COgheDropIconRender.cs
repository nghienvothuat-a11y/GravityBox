using System.Collections;
using System.IO;
using GravityBox.Venom;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TestTools;
namespace GravityBox.Tests
{
 // The Drops icon (Mrk 02/10: a drop of the same liquid as COghe): a glossy teardrop in COghe's own skin material, lit by a
 // level's lights, rendered on transparency into Resources/COgheUI/Drop.png.
 public sealed partial class COgheSpatialCampaignTests
 {
  [Explicit("Writes Assets/_Game/Venom/Resources/COgheUI/Drop.png")]
  [UnityTest] public IEnumerator RenderDropIcon()
  {
   yield return Load(1);yield return Frames(10);
   const int layer=31,size=256;
   var drop=new GameObject("Drop",typeof(MeshFilter),typeof(MeshRenderer));drop.layer=layer;drop.transform.position=new Vector3(100,100,100);
   var mesh=new Mesh();const int rings=48,sides=48;var v=new System.Collections.Generic.List<Vector3>();var t=new System.Collections.Generic.List<int>();
   for(int r=0;r<=rings;r++)
   {
    // a round bottom (unit sphere) rising into a soft point at the top
    float u=r/(float)rings,y,radius;
    if(u<.62f){float a=Mathf.Lerp(-90,32,u/.62f)*Mathf.Deg2Rad;y=1+Mathf.Sin(a);radius=Mathf.Cos(a);}
    else{float k=(u-.62f)/.38f;y=Mathf.Lerp(1+Mathf.Sin(32*Mathf.Deg2Rad),2.75f,k);radius=Mathf.Cos(32*Mathf.Deg2Rad)*Mathf.Pow(1-k,1.5f);}
    for(int s=0;s<=sides;s++){float a=s*Mathf.PI*2/sides;v.Add(new Vector3(Mathf.Cos(a)*radius,y,Mathf.Sin(a)*radius));}
   }
   for(int r=0;r<rings;r++)for(int s=0;s<sides;s++){int a=r*(sides+1)+s,b=a+1,c=a+sides+1,d=c+1;t.Add(a);t.Add(c);t.Add(b);t.Add(b);t.Add(c);t.Add(d);}
   mesh.SetVertices(v);mesh.SetTriangles(t,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
   drop.GetComponent<MeshFilter>().sharedMesh=mesh;
   var skin=new Material(game.Matter.Profile.Skin);if(skin.HasProperty("_Smoothness"))skin.SetFloat("_Smoothness",.9f);
   var r_=drop.GetComponent<MeshRenderer>();r_.sharedMaterial=skin;r_.shadowCastingMode=ShadowCastingMode.Off;
   drop.transform.localScale=Vector3.one*.1f;drop.transform.rotation=Quaternion.Euler(0,0,-8);
   var cam=new GameObject("Drop camera").AddComponent<Camera>();cam.enabled=false;cam.cullingMask=1<<layer;
   cam.orthographic=true;cam.orthographicSize=.17f;cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(0,0,0,0);
   cam.transform.SetPositionAndRotation(drop.transform.position+new Vector3(.012f,.142f,-1),Quaternion.identity);cam.nearClipPlane=.01f;cam.farClipPlane=5;
   // a soft key light from the upper left: the wet highlight of COghe's skin
   var key=new GameObject("Drop key light").AddComponent<Light>();key.type=LightType.Point;key.cullingMask=1<<layer;key.range=1.2f;key.intensity=2.5f;
   key.transform.position=drop.transform.position+new Vector3(-.16f,.36f,-.28f);
   var rt=RenderTexture.GetTemporary(size*2,size*2,24,RenderTextureFormat.ARGB32);var tex=new Texture2D(size*2,size*2,TextureFormat.RGBA32,false);
   try
   {
    cam.targetTexture=rt;cam.Render();cam.targetTexture=null;
    RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,size*2,size*2),0,0);tex.Apply();RenderTexture.active=null;
    // half size (smooth edges), straight alpha
    var outTex=new Texture2D(size,size,TextureFormat.RGBA32,false);
    for(int y=0;y<size;y++)for(int x=0;x<size;x++)
    {
     Color sum=Color.clear;float alpha=0;
     for(int j=0;j<2;j++)for(int i=0;i<2;i++){var c=tex.GetPixel(x*2+i,y*2+j);sum+=c*c.a;alpha+=c.a;}
     outTex.SetPixel(x,y,alpha>0?new Color(sum.r/alpha,sum.g/alpha,sum.b/alpha,alpha/4):Color.clear);
    }
    outTex.Apply();
    Assert.Greater(outTex.GetPixel(size/2,size/3).a,.9f,"The drop is there");Assert.AreEqual(0,outTex.GetPixel(4,size-4).a,.01f,"on transparency");
    File.WriteAllBytes("Assets/_Game/Venom/Resources/COgheUI/Drop.png",outTex.EncodeToPNG());Object.Destroy(outTex);
   }
   finally{RenderTexture.ReleaseTemporary(rt);Object.Destroy(tex);Object.Destroy(cam.gameObject);Object.Destroy(key.gameObject);Object.Destroy(drop);Object.Destroy(skin);Object.Destroy(mesh);}
  }
 }
}
