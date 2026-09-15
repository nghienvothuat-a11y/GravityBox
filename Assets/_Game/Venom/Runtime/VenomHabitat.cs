using UnityEngine;

namespace GravityBox.Venom
{
    // Collection's first playable room. Presentation and food never alter puzzle mass.
    public sealed class VenomHabitat
    {
        private readonly VenomCampaign game;
        private VenomSurfacePatch[] surfaces;
        private VenomMovableProp[] props;
        private GameObject room,food;
        private Material furnishing;
        private float greeting;
        public float Greeting=>Mathf.Clamp01(greeting-game.Matter.SimulationTime);
        public VenomHabitat(VenomCampaign owner){game=owner;}
        public void Enter()
        {
            surfaces=game.Surfaces;props=game.Props;
            game.Owner.Apparatus.gameObject.SetActive(false);
            room=new GameObject("Creature's home");room.transform.SetParent(game.transform,false);
            furnishing=new Material(game.Matter.Profile.Skin);furnishing.name="Habitat ceramic";
            furnishing.SetColor("_BaseColor",new Color(.32f,.45f,.44f));furnishing.SetFloat("_Metallic",0);furnishing.SetFloat("_Smoothness",.35f);
            var floor=Surface("Home floor",new Vector3(0,-.3f,0),Vector3.up,new Vector2(.7f,.7f));
            var back=Surface("Home back wall",new Vector3(0,0,.35f),Vector3.back,new Vector2(.7f,.6f));
            game.Surfaces=new[]{floor,back};game.Props=new VenomMovableProp[0];
            Decor(PrimitiveType.Cylinder,"Soft resting mat",new Vector3(-.1f,-.294f,-.1f),new Vector3(.20f,.004f,.15f),game.Owner.IndicatorMaterial);
            Decor(PrimitiveType.Sphere,"Round lamp",new Vector3(.19f,-.25f,.17f),Vector3.one*.065f,game.Owner.IndicatorMaterial);
            Decor(PrimitiveType.Cylinder,"Lamp stand",new Vector3(.19f,-.28f,.17f),new Vector3(.025f,.022f,.025f),furnishing);
            Vector3 shift=new Vector3(-.1f,-.274f,-.1f)-game.Motion.Centre(0);
            foreach(var b in game.Matter.Bodies){b.position+=shift;b.linearVelocity=b.angularVelocity=Vector3.zero;}
            Physics.SyncTransforms();game.Motion.Reset();Greet();
        }
        private VenomSurfacePatch Surface(string name,Vector3 position,Vector3 normal,Vector2 size)
        {
            var go=new GameObject(name,typeof(BoxCollider),typeof(VenomSurfacePatch));go.transform.SetParent(room.transform,false);
            go.transform.localPosition=position;go.transform.localRotation=Quaternion.LookRotation(normal,Mathf.Abs(normal.y)>.9f?Vector3.forward:Vector3.up);
            var c=go.GetComponent<BoxCollider>();c.center=Vector3.back*.006f;c.size=new Vector3(size.x,size.y,.012f);c.sharedMaterial=game.Matter.Profile.Contact;
            var p=go.GetComponent<VenomSurfacePatch>();p.Shape=c;p.Size=size;
            var visual=GameObject.CreatePrimitive(PrimitiveType.Cube);visual.transform.SetParent(go.transform,false);visual.transform.localPosition=c.center;visual.transform.localScale=c.size;visual.GetComponent<Renderer>().sharedMaterial=furnishing;Object.Destroy(visual.GetComponent<Collider>());
            return p;
        }
        private GameObject Decor(PrimitiveType type,string name,Vector3 p,Vector3 scale,Material material)
        {
            var o=GameObject.CreatePrimitive(type);o.name=name;o.transform.SetParent(room.transform,false);o.transform.localPosition=p;o.transform.localScale=scale;o.GetComponent<Renderer>().sharedMaterial=material;Object.Destroy(o.GetComponent<Collider>());return o;
        }
        public void Greet(){greeting=game.Matter.SimulationTime+3;}
        public void Feed()
        {
            if(food!=null)Object.Destroy(food);
            food=Decor(PrimitiveType.Sphere,"Snack",new Vector3(-.04f,-.279f,-.12f),Vector3.one*.024f,game.Owner.IndicatorMaterial);
            game.Motion.Move(0,food.transform.position);
        }
        public void Step(float dt)
        {
            if(food==null||Vector3.Distance(game.Motion.Centre(0),food.transform.position)>.045f)return;
            food.transform.localScale*=Mathf.Exp(-dt*4);Greet();if(food.transform.localScale.x<.002f)Object.Destroy(food);
        }
        public void Leave()
        {
            if(surfaces==null)return;
            game.Surfaces=surfaces;game.Props=props;surfaces=null;
            game.Owner.Apparatus.gameObject.SetActive(true);Object.Destroy(room);Object.Destroy(furnishing);room=food=null;
        }
        public void Dispose(){if(furnishing!=null)Object.Destroy(furnishing);}
    }
}
