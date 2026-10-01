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
        /// <summary>The furnished room (Spatial catalog): the items COghe has earned, where it lives and plays.</summary>
        public COgheHomeRoom Room {get;private set;}
        /// <summary>The furnished room's food: steel balls thrown in that COghe eats (null in the plain habitat).</summary>
        public COgheFeedBalls FeedBalls {get;private set;}
        public bool Feeding=>food!=null||(FeedBalls!=null&&FeedBalls.Active);
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
            furnishing.SetColor("_BaseColor",(game.ProductUI!=null?new Color(.91f,.90f,.83f):new Color(.32f,.45f,.44f)));furnishing.SetFloat("_Metallic",0);furnishing.SetFloat("_Smoothness",.35f);
            bool furnished=game.Personality!=null;
            if(furnished)
            {
                // the furnished room: a bigger floor and back wall (their look comes from the room shell)
                var floor=Surface("Home floor",new Vector3(0,-.3f,0),Vector3.up,new Vector2(COgheHomeRoom.HalfWidth*2,COgheHomeRoom.HalfDepth*2),false);
                var back=Surface("Home back wall",new Vector3(0,-.3f+COgheHomeRoom.WallHeight*.5f,COgheHomeRoom.HalfDepth),Vector3.back,new Vector2(COgheHomeRoom.HalfWidth*2,COgheHomeRoom.WallHeight),false);
                game.Surfaces=new[]{floor,back};game.Props=new VenomMovableProp[0];
                Room=new COgheHomeRoom(game,room.transform,furnishing,GlassTemplate(),null);
                FeedBalls=new COgheFeedBalls(game,Room,furnishing);
            }
            else
            {
                var floor=Surface("Home floor",new Vector3(0,-.3f,0),Vector3.up,new Vector2(.7f,.7f));
                var back=Surface("Home back wall",new Vector3(0,0,.35f),Vector3.back,new Vector2(.7f,.6f));
                game.Surfaces=new[]{floor,back};game.Props=new VenomMovableProp[0];
                Decor(PrimitiveType.Cylinder,"Soft resting mat",new Vector3(-.1f,-.294f,-.1f),new Vector3(.20f,.004f,.15f),game.Owner.IndicatorMaterial);
                Decor(PrimitiveType.Sphere,"Round lamp",game.ProductUI!=null?new Vector3(.006f,-.25f,-.075f):new Vector3(.19f,-.25f,.17f),Vector3.one*.065f,game.Owner.IndicatorMaterial);
                Decor(PrimitiveType.Cylinder,"Lamp stand",game.ProductUI!=null?new Vector3(.006f,-.28f,-.075f):new Vector3(.19f,-.28f,.17f),new Vector3(.025f,.022f,.025f),furnishing);
            }
            Vector3 shift=(furnished?new Vector3(0,-.274f,0):new Vector3(-.1f,-.274f,-.1f))-game.Motion.Centre(0);
            foreach(var b in game.Matter.Bodies){b.position+=shift;b.linearVelocity=b.angularVelocity=Vector3.zero;}
            Physics.SyncTransforms();game.Motion.Reset();Greet();
            if(Room!=null)game.Personality.EnterHome(Room);
        }
        // the level's own glass shader, so the room's panes and the ghost preview look like the puzzle boxes
        private Material GlassTemplate()
        {
            foreach(var r in game.Owner.Apparatus.GetComponentsInChildren<Renderer>(true))
                foreach(var m in r.sharedMaterials)if(m!=null&&m.shader!=null&&m.shader.name=="COghe/Lab Glass")return m;
            var shader=Shader.Find("COghe/Lab Glass");return shader!=null?new Material(shader):furnishing;
        }
        private VenomSurfacePatch Surface(string name,Vector3 position,Vector3 normal,Vector2 size,bool visual=true)
        {
            var go=new GameObject(name,typeof(BoxCollider),typeof(VenomSurfacePatch));go.transform.SetParent(room.transform,false);
            go.transform.localPosition=position;go.transform.localRotation=Quaternion.LookRotation(normal,Mathf.Abs(normal.y)>.9f?Vector3.forward:Vector3.up);
            var c=go.GetComponent<BoxCollider>();c.center=Vector3.back*.006f;c.size=new Vector3(size.x,size.y,.012f);c.sharedMaterial=game.Matter.Profile.Contact;
            var p=go.GetComponent<VenomSurfacePatch>();p.Shape=c;p.Size=size;
            if(visual){var cube=GameObject.CreatePrimitive(PrimitiveType.Cube);cube.transform.SetParent(go.transform,false);cube.transform.localPosition=c.center;cube.transform.localScale=c.size;cube.GetComponent<Renderer>().sharedMaterial=furnishing;Object.Destroy(cube.GetComponent<Collider>());}
            return p;
        }
        private GameObject Decor(PrimitiveType type,string name,Vector3 p,Vector3 scale,Material material)
        {
            var o=GameObject.CreatePrimitive(type);o.name=name;o.transform.SetParent(room.transform,false);o.transform.localPosition=p;o.transform.localScale=scale;o.GetComponent<Renderer>().sharedMaterial=material;Object.Destroy(o.GetComponent<Collider>());return o;
        }
        public void SetMenuPresentation(bool menu)
        {
            if(room!=null)foreach(var r in room.GetComponentsInChildren<Renderer>())r.forceRenderingOff=menu||r.transform.parent.GetComponent<VenomSurfacePatch>()!=null;
            if(game.Personality!=null)game.Personality.Showcase=menu;
        }
        public void Greet(){greeting=game.Matter.SimulationTime+3;}
        public void Feed()
        {
            if(FeedBalls!=null){FeedBalls.Feed();return;}
            if(food!=null)Object.Destroy(food);
            food=Decor(PrimitiveType.Sphere,"Snack",new Vector3(-.04f,-.279f,-.12f),Vector3.one*.024f,game.Owner.IndicatorMaterial);
            game.Motion.Move(0,food.transform.position);
        }
        public void Step(float dt)
        {
            FeedBalls?.Step(dt);
            if(food==null||Vector3.Distance(game.Motion.Centre(0),food.transform.position)>.045f)return;
            food.transform.localScale*=Mathf.Exp(-dt*4);Greet();if(food.transform.localScale.x<.002f)Object.Destroy(food);
        }
        public void Leave()
        {
            if(surfaces==null)return;
            game.Surfaces=surfaces;game.Props=props;surfaces=null;
            game.Personality?.LeaveHome();FeedBalls?.Dispose();FeedBalls=null;Room?.Dispose();Room=null;
            game.Owner.Apparatus.gameObject.SetActive(true);Object.Destroy(room);Object.Destroy(furnishing);room=food=null;
        }
        public void Dispose(){FeedBalls?.Dispose();FeedBalls=null;Room?.Dispose();Room=null;if(furnishing!=null)Object.Destroy(furnishing);}
    }
}
