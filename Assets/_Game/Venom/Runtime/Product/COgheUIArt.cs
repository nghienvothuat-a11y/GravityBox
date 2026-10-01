using System;
using UnityEngine;
using UnityEngine.UI;

namespace GravityBox.Venom
{
    public enum COgheIcon { Play, Pause, Restart, Home, Menu, Help, Music, Sound, Muted,
        Overview, Close, Hand, Back, Forward, Skip, Check, Lock, Film, Food, Heart,
        Tap, Drag, Pinch, Minus, Circle, Left, Right }

    /// <summary>One icon atlas and a nine-slice panel, shared by all retained UI.</summary>
    public sealed class COgheUIArt : IDisposable
    {
        public static readonly Color Ink = new Color(.188f,.306f,.337f);
        public static readonly Color Muted = new Color(.385f,.467f,.482f);
        public static readonly Color Paper = new Color(.98f,.977f,.933f);
        public static readonly Color Teal = new Color(.208f,.427f,.427f);
        public static readonly Color Mint = new Color(.847f,.906f,.867f);
        public readonly Font Font,BoldFont;
        public readonly Sprite Panel;
        private readonly Sprite[] icons;
        public COgheUIArt()
        {
            Font = Resources.Load<Font>("COgheUI/Manrope") ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            BoldFont=Resources.Load<Font>("COgheUI/ManropeBold")??Font;
            var panel = Resources.Load<Texture2D>("COgheUI/Panel");
            Panel = Sprite.Create(panel,new Rect(0,0,128,128),Vector2.one*.5f,100,0,SpriteMeshType.FullRect,new Vector4(32,32,32,32));
            var atlas = Resources.Load<Texture2D>("COgheUI/Icons");
            icons = new Sprite[Enum.GetValues(typeof(COgheIcon)).Length];
            for(int i=0;i<icons.Length;i++) icons[i]=Sprite.Create(atlas,new Rect(i%8*64,256-(i/8+1)*64,64,64),Vector2.one*.5f,100);
        }
        public RectTransform Rect(Transform parent,string name,Rect bounds)
        {
            var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);
            r.anchorMin=r.anchorMax=new Vector2(0,1);r.pivot=new Vector2(0,1);
            r.anchoredPosition=new Vector2(bounds.x,-bounds.y);r.sizeDelta=bounds.size;return r;
        }
        public Image Box(Transform parent,string name,Rect bounds,Color color,bool block=false)
        {
            var im=Rect(parent,name,bounds).gameObject.AddComponent<Image>();im.sprite=Panel;im.type=Image.Type.Sliced;
            im.color=color;im.raycastTarget=block;return im;
        }
        public Text Label(Transform parent,string name,string value,Rect bounds,int size,Color? color=null,TextAnchor align=TextAnchor.MiddleCenter)
        {
            var t=Rect(parent,name,bounds).gameObject.AddComponent<Text>();t.font=size>=20?BoldFont:Font;t.text=value;t.fontSize=size;
            t.color=color??Ink;t.alignment=align;t.raycastTarget=false;t.supportRichText=true;
            t.horizontalOverflow=HorizontalWrapMode.Wrap;t.verticalOverflow=VerticalWrapMode.Overflow;return t;
        }
        public Image Icon(Transform parent,COgheIcon icon,Rect bounds,Color? color=null)
        {
            var im=Rect(parent,icon.ToString(),bounds).gameObject.AddComponent<Image>();im.sprite=icons[(int)icon];im.color=color??Ink;
            im.raycastTarget=false;return im;
        }
        public Button Button(Transform parent,string name,Rect bounds,COgheIcon icon,Action click,string label=null,bool primary=false,bool selected=false)
        {
            Box(parent,name+" shadow",new Rect(bounds.x,bounds.y+3,bounds.width,bounds.height),primary?new Color(.16f,.33f,.33f):new Color(.78f,.83f,.79f)).pixelsPerUnitMultiplier=primary?1.15f:2f;
            var im=Box(parent,name,bounds,primary?Teal:selected?Mint:new Color(.995f,.995f,.972f),true);
            im.pixelsPerUnitMultiplier=primary?1.15f:2f;
            var b=im.gameObject.AddComponent<Button>();b.targetGraphic=im;
            var colors=b.colors;colors.highlightedColor=new Color(.94f,.98f,.94f);colors.pressedColor=new Color(.79f,.87f,.82f);colors.selectedColor=Color.white;b.colors=colors;
            b.onClick.AddListener(()=>{COgheAudio.UiTap();click();});
            Color fg=primary?Paper:Ink;
            float ix=label==null?(bounds.width-30)*.5f:bounds.width*.5f-51;
            Icon(im.transform,icon,new Rect(ix,(bounds.height-30)*.5f,30,30),fg);
            if(label!=null)Label(im.transform,"Label",label,new Rect(bounds.width*.5f-16,0,bounds.width*.5f, bounds.height),16,fg,TextAnchor.MiddleLeft).font=BoldFont;
            return b;
        }
        public bool GuiButton(Rect bounds,COgheIcon icon)
        {
            bool pressed=GUI.Button(bounds,GUIContent.none,GUIStyle.none);
            if(Event.current.type==EventType.Repaint)
            {
                var previous=GUI.color;GUI.color=Paper;GUI.DrawTexture(bounds,Panel.texture);
                var glyph=icons[(int)icon];var uv=glyph.rect;var tex=glyph.texture;
                GUI.color=Ink;float inset=bounds.width*.22f;
                GUI.DrawTextureWithTexCoords(new Rect(bounds.x+inset,bounds.y+inset,bounds.width-2*inset,bounds.height-2*inset),tex,new Rect(uv.x/tex.width,uv.y/tex.height,uv.width/tex.width,uv.height/tex.height));
                GUI.color=previous;
            }
            return pressed;
        }
        private GUIStyle pill,pillText;
        /// <summary>A labelled pill button for IMGUI overlays (the intro), drawn like the retained UI's buttons.</summary>
        public bool GuiPill(Rect bounds,COgheIcon icon,string label,float unit)
        {
            bool pressed=GUI.Button(bounds,GUIContent.none,GUIStyle.none);
            if(Event.current.type==EventType.Repaint)
            {
                if(pill==null)pill=new GUIStyle{normal={background=Panel.texture},border=new RectOffset(32,32,32,32)};
                if(pillText==null)pillText=new GUIStyle{font=BoldFont,alignment=TextAnchor.MiddleLeft,normal={textColor=Ink}};
                pillText.fontSize=Mathf.RoundToInt(16*unit);
                var previous=GUI.color;GUI.color=Paper;pill.Draw(bounds,false,false,false,false);
                var glyph=icons[(int)icon];var uv=glyph.rect;var tex=glyph.texture;float size=bounds.height*.5f;
                GUI.color=Ink;GUI.DrawTextureWithTexCoords(new Rect(bounds.x+bounds.height*.42f,bounds.y+(bounds.height-size)*.5f,size,size),tex,new Rect(uv.x/tex.width,uv.y/tex.height,uv.width/tex.width,uv.height/tex.height));
                GUI.color=Color.white;pillText.Draw(new Rect(bounds.x+bounds.height*.42f+size+8*unit,bounds.y,bounds.width,bounds.height),label,false,false,false,false);
                GUI.color=previous;
            }
            return pressed;
        }
        public void Dispose(){UnityEngine.Object.Destroy(Panel);foreach(var s in icons)UnityEngine.Object.Destroy(s);}
    }
}
