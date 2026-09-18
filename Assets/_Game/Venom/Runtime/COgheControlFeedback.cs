using UnityEngine;
using UnityEngine.Rendering;

namespace GravityBox.Venom
{
    /// <summary>Reads accepted commands and simulation state. Never picks a new
    /// destination, changes a path, or adds a collision surface.</summary>
    [DefaultExecutionOrder(120)]
    public sealed class COgheControlFeedback : MonoBehaviour
    {
        public Material MarkerMaterial;
        public bool HasCommand {get;private set;}
        public Vector3 CommandPoint=>anchor!=null?anchor.TransformPoint(localPoint):localPoint;
        public Vector3 CommandNormal=>anchor!=null?anchor.TransformDirection(localNormal).normalized:localNormal;
        public VenomSurfacePatch CommandSurface {get;private set;}
        public Vector3 RoofHintPoint=>game.Root.TransformPoint(new Vector3(reachedRoof?-.13f:-.49f,.23f,0));
        public bool SelectedArrowVisible=>selection!=null&&selection.enabled;
        private VenomCampaign game;
        private Transform visuals,anchor;
        private Vector3 localPoint,localNormal;
        private float commandAt;
        private int commandGroup;
        private bool pushedOnce,reachedRoof;
        private LineRenderer target,ripple,face,stem,selection,hint,roofRing,roofGlyph;
        private Mesh faceMesh;
        private MeshRenderer faceFill;
        private readonly Vector3[] faceVertices=new Vector3[4];
        private readonly Color[] faceColors=new Color[4];
        private readonly LineRenderer[] lines=new LineRenderer[8];
        private static readonly Color Mint=new Color(.10f,.57f,.45f);
        private static readonly Color Amber=new Color(.87f,.49f,.12f);
        private Texture2D rotateIcon,lockedIcon;
        private GUIStyle iconLabel;
        private Material fallback;

        public void Initialize(VenomCampaign owner)
        {
            game=owner;
            visuals=new GameObject("Control feedback · presentation only").transform;
            visuals.SetParent(transform,false);
            if(MarkerMaterial==null)MarkerMaterial=fallback=new Material(Shader.Find("COghe/Guidance"));
            target=Line(0,"Accepted destination",49,.0022f,Mint);
            ripple=Line(1,"Touch ripple",49,.0022f,Mint);
            face=Line(2,"Touched face boundary",5,.002f,Mint);
            stem=Line(3,"Destination normal",2,.0017f,Mint);
            selection=Line(4,"Selected fragment arrow",5,.003f,Mint);
            hint=Line(5,"Lesson arrow",5,.003f,Amber);
            roofRing=Line(6,"Roof departure marker",49,.0016f,Amber);
            roofGlyph=Line(7,"Roof departure chevron",3,.002f,Amber);
            var fill=new GameObject("Touched glass wash");fill.transform.SetParent(visuals,false);
            faceMesh=new Mesh{name="Temporary face feedback"};faceMesh.MarkDynamic();
            faceMesh.vertices=faceVertices;faceMesh.colors=faceColors;faceMesh.triangles=new[]{0,1,2,0,2,3};
            fill.AddComponent<MeshFilter>().sharedMesh=faceMesh;
            faceFill=fill.AddComponent<MeshRenderer>();faceFill.sharedMaterial=MarkerMaterial;
            faceFill.shadowCastingMode=ShadowCastingMode.Off;faceFill.receiveShadows=false;faceFill.enabled=false;
        }
        private LineRenderer Line(int slot,string name,int points,float width,Color color)
        {
            var go=new GameObject(name);go.transform.SetParent(visuals,false);
            var line=go.AddComponent<LineRenderer>();line.sharedMaterial=MarkerMaterial;
            line.useWorldSpace=true;line.positionCount=points;line.startWidth=line.endWidth=width;
            line.startColor=line.endColor=color;line.numCapVertices=3;line.numCornerVertices=3;
            line.shadowCastingMode=ShadowCastingMode.Off;line.receiveShadows=false;
            line.enabled=false;lines[slot]=line;return line;
        }
        public void ResetFeedback()
        {HasCommand=false;anchor=null;CommandSurface=null;pushedOnce=reachedRoof=false;foreach(var line in lines)if(line!=null)line.enabled=false;if(faceFill!=null)faceFill.enabled=false;}
        public void ShowCommand(Vector3 point,Vector3 normal,Transform follows,VenomSurfacePatch surface=null)
        {
            anchor=follows;localPoint=anchor!=null?anchor.InverseTransformPoint(point):point;
            localNormal=anchor!=null?anchor.InverseTransformDirection(normal):normal;
            CommandSurface=surface;commandAt=Time.unscaledTime;
            commandGroup=game.Matter.Groups[game.Motion.Selected];HasCommand=true;
            Refresh();
        }
        private static void ColorAlpha(LineRenderer line,Color color,float alpha)
        {color.a=alpha;line.startColor=line.endColor=color;}
        private static void Circle(LineRenderer line,Vector3 point,Vector3 normal,float radius)
        {
            Vector3 x=Vector3.Cross(normal,Vector3.up).normalized;
            if(x.sqrMagnitude<.1f)x=Vector3.Cross(normal,Vector3.right).normalized;
            Vector3 y=Vector3.Cross(normal,x);
            for(int i=0;i<49;i++){float a=i*Mathf.PI*2/48;line.SetPosition(i,point+(x*Mathf.Cos(a)+y*Mathf.Sin(a))*radius);}
        }
        private void Arrow(LineRenderer line,Vector3 tip,float size)
        {
            var up=game.Owner.View.transform.up;var right=game.Owner.View.transform.right;
            line.SetPosition(0,tip+up*size);
            line.SetPosition(1,tip);
            line.SetPosition(2,tip+up*size*.44f-right*size*.38f);
            line.SetPosition(3,tip);
            line.SetPosition(4,tip+up*size*.44f+right*size*.38f);
        }
        private void LateUpdate()=>Refresh();
        public void Refresh()
        {
            if(game==null||game.Owner==null||game.Motion==null)return;
            foreach(var line in lines)line.enabled=false;
            faceFill.enabled=false;
            if(game.Home||game.Owner.Completed||game.Owner.Lost)return;
            float now=Time.unscaledTime;
            if(HasCommand&&commandGroup!=game.Matter.Groups[game.Motion.Selected])HasCommand=false;
            if(HasCommand)
            {
                float age=now-commandAt;
                if(age>1.1f&&game.Motion.Get(game.Motion.Selected)==null&&!game.Attached)HasCommand=false;
                if(HasCommand)
                {
                    Vector3 n=CommandNormal,p=CommandPoint+n*.003f;
                    target.enabled=true;Circle(target,p,n,.013f+.0015f*Mathf.Sin(now*3));
                    stem.enabled=true;stem.SetPosition(0,p);stem.SetPosition(1,p+n*.013f);
                    float pulse=Mathf.Clamp01(age/.85f);
                    if(pulse<1){ripple.enabled=true;Circle(ripple,p,n,Mathf.Lerp(.014f,.041f,pulse));ColorAlpha(ripple,Mint,1-pulse);}
                    if(age<1.15f&&CommandSurface!=null&&CommandSurface.SphereRadius<=0)
                    {
                        face.enabled=true;var patch=CommandSurface;
                        // Keep the flash inside the pane; the opaque frame would
                        // otherwise hide an outline exactly along the glass edge.
                        float inset=Mathf.Min(.014f,Mathf.Min(patch.Size.x,patch.Size.y)*.12f);
                        var half=patch.Size*.5f-Vector2.one*inset;
                        float fade=Mathf.Clamp01(1-age/1.15f);
                        for(int i=0;i<5;i++)
                        {
                            int k=i%4;Vector3 world=patch.transform.TransformPoint(new Vector3(k==1||k==2?half.x:-half.x,k>=2?half.y:-half.y,.0025f));
                            face.SetPosition(i,world);
                            if(i<4){faceVertices[i]=visuals.InverseTransformPoint(world);var color=Mint;color.a=.075f*fade;faceColors[i]=color;}
                        }
                        ColorAlpha(face,Mint,.85f*fade);
                        // Never paint over a real hole, even briefly.
                        if(!patch.Hole){faceMesh.vertices=faceVertices;faceMesh.colors=faceColors;faceMesh.RecalculateBounds();faceFill.enabled=true;}
                    }
                }
            }
            if(game.Matter.TotalFragmentCount>1)
            {
                int group=game.Matter.Groups[game.Motion.Selected];Vector3 centre=game.Motion.Centre(game.Motion.Selected);
                Vector3 up=game.Owner.View.transform.up;float height=0;
                for(int i=0;i<32;i++)if(game.Matter.Groups[i]==group&&!game.Matter.Escaped[i])
                    height=Mathf.Max(height,Vector3.Dot(game.Matter.Bodies[i].position-centre,up));
                selection.enabled=true;
                Arrow(selection,centre+up*(height+.026f+.003f*Mathf.Sin(now*3)),.027f);
            }
            if(game.Tube!=null&&game.Tube.DepartureHint!=null&&!game.InTube&&
                Vector3.Dot(game.Motion.Centre(game.Motion.Selected)-game.Tube.transform.position,game.Tube.transform.forward)<0)
            {
                hint.enabled=true;
                Arrow(hint,game.Tube.DepartureHint.position+game.Owner.View.transform.up*(.008f+.003f*Mathf.Sin(now*2.5f)),.034f);
            }
            int lesson=game.Definition.Order;
            if(lesson==1||lesson==2)
            {
                hint.enabled=true;Vector3 p=game.Owner.Outlet.position;
                Arrow(hint,p+game.Owner.View.transform.up*(.025f+.005f*Mathf.Sin(now*2.5f)),.034f);
            }
            if(lesson==7)
            {
                pushedOnce|=game.Attached;
                if(!pushedOnce&&game.Props.Length>0)
                {
                    hint.enabled=true;var prop=game.Props[0];
                    Arrow(hint,prop.Body.position+game.Owner.View.transform.up*(.13f+.004f*Mathf.Sin(now*2.5f)),.03f);
                }
            }
            if(lesson==8&&!game.InTube&&game.Root.InverseTransformPoint(game.Motion.Centre(game.Motion.Selected)).x<-.11f)
            {
                // First invite a climb onto the safe roof. Only then move the
                // cue to the slippery departure; a direct command from below
                // would send the creature up the ungrippable inlet wall.
                Vector3 centre=game.Root.InverseTransformPoint(game.Motion.Centre(game.Motion.Selected));
                reachedRoof|=centre.y>.18f&&centre.x<-.20f;
                Vector3 p=RoofHintPoint+game.Root.up*.003f;
                roofRing.enabled=roofGlyph.enabled=true;
                Circle(roofRing,p,game.Root.up,.019f+.0015f*Mathf.Sin(now*2.5f));
                roofGlyph.SetPosition(0,p-game.Root.right*.006f-game.Root.forward*.007f);
                roofGlyph.SetPosition(1,p+game.Root.right*.004f);
                roofGlyph.SetPosition(2,p-game.Root.right*.006f+game.Root.forward*.007f);
            }
        }
        private static Texture2D RotationIcon(bool locked)
        {
            const int size=96;var texture=new Texture2D(size,size,TextureFormat.RGBA32,false){hideFlags=HideFlags.HideAndDontSave,filterMode=FilterMode.Bilinear};
            var pixels=new Color[size*size];Color color=locked?new Color(.57f,.34f,.28f):new Color(.19f,.43f,.44f);
            for(int y=0;y<size;y++)for(int x=0;x<size;x++)
            {
                Vector2 p=new Vector2(x-47.5f,y-47.5f);float a=Mathf.Atan2(p.y,p.x);
                bool arc=Mathf.Abs(p.magnitude-29)<2.3f&&Mathf.Abs(Mathf.Sin(a))>.23f;
                bool arrow=(p.x>20&&p.x<38&&Mathf.Abs(p.y-8)<(38-p.x)*.62f)||(p.x< -20&&p.x> -38&&Mathf.Abs(p.y+8)<(38+p.x)*.62f);
                bool slash=locked&&Mathf.Abs(p.x+p.y)<3&&Mathf.Abs(p.x-p.y)<70;
                pixels[y*size+x]=arc||arrow||slash?color:Color.clear;
            }
            texture.SetPixels(pixels);texture.Apply();return texture;
        }
        private void OnGUI()
        {
            if(game==null||game.Owner==null||game.Home||game.Owner.Completed||game.Owner.Lost)return;
            if(game.Definition.Order<3&&game.Definition.CanRotate)return;
            if(rotateIcon==null){rotateIcon=RotationIcon(false);lockedIcon=RotationIcon(true);}
            if(iconLabel==null)iconLabel=new GUIStyle(GUI.skin.label){fontSize=12,alignment=TextAnchor.MiddleCenter,normal={textColor=new Color(.19f,.29f,.34f)}};
            var old=GUI.matrix;var color=GUI.color;
            float scale=Mathf.Min(Screen.width/540f,Screen.height/960f),h=Screen.height/scale;
            var canvas=Matrix4x4.TRS(new Vector3((Screen.width-540*scale)*.5f,0,0),Quaternion.identity,Vector3.one*scale);
            GUI.matrix=canvas;GUI.color=Color.white;
            bool locked=!game.Definition.CanRotate;float y=h-219;
            GUI.DrawTexture(new Rect(249,y,42,42),locked?lockedIcon:rotateIcon);
            GUI.Label(new Rect(155,y+38,230,22),locked?"Không thể xoay":"Kéo để xoay hộp",iconLabel);
            GUI.matrix=old;GUI.color=color;
        }
        private void OnDestroy()
        {if(fallback!=null)Destroy(fallback);if(faceMesh!=null)Destroy(faceMesh);if(rotateIcon!=null)Destroy(rotateIcon);if(lockedIcon!=null)Destroy(lockedIcon);}
    }
}
