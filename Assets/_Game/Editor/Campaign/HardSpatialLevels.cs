using System.Collections.Generic;
using GravityBox.Gameplay;
using GravityBox.Presentation;
using GravityBox.Simulation;
using UnityEngine;

namespace GravityBox.Editor
{
    internal static class HardSpatialLevels
    {
        internal static LevelRuntime Build(CampaignLevelSpec s,CampaignBuildContext c)
        {
            if(s.index==23||s.index==25||s.index==86)return Cages(s,c);
            return Network(s,c);
        }

        private static LevelRuntime Cages(CampaignLevelSpec s,CampaignBuildContext c)
        {
            bool pair=s.index==86;
            var l=c.Planar(HardCampaignBuilder.Name(s),HardCampaignBuilder.Rectangle(pair?.72f:.43f,.28f),
                new Vector2(pair?-.37f:-.08f,0),new Vector2(pair?.62f:.34f,s.index==25?.20f:0),.36f,spawnY:.022f);
            var a=HardCampaignBuilder.Author(l,c);
            Vector3 first=new Vector3(pair?-.37f:-.08f,0,0);
            HardCampaignModules.Cage(a,first,Quaternion.identity,false);
            if(pair)
            {
                // Two different support planes. The second cage rotates about Y
                // in the outer box; the first rotates about Z.
                Vector3 second=new Vector3(.37f,0,0);
                HardCampaignModules.Cage(a,second,Quaternion.Euler(90,0,0),true);
                CageSleeve(a,first,Quaternion.identity);
                CageSleeve(a,second,Quaternion.Euler(90,0,0));
                a.Block("Intermediate broad rest deck",new Vector3(0,-.105f,0),new Vector3(.18f,.008f,.24f),c.Frame);
                HardCampaignModules.Pocket(a,new Vector3(0,-.101f,.065f),Quaternion.identity,.11f,.12f,.07f);
                // A transverse chamber wall routes the transfer into the next
                // cage's entry. The surrounding recovery basin returns locally.
                HardCampaignBuilder.Wall(a,"Second cage north approach",.05f,.18f,.13f,.28f,-.18f,.18f);
                HardCampaignBuilder.Wall(a,"Second cage south approach",.05f,.18f,-.28f,-.13f,-.18f,.18f);
            }
            if(s.index==25)
            {
                // The exit court turns north after the cage; it has a 110 mm
                // braking area before the corner instead of a straight drop out.
                HardCampaignBuilder.Wall(a,"Receiving L inside corner",.17f,.265f,-.28f,.09f,-.18f,.18f);
                HardCampaignBuilder.Wall(a,"Receiving L outer back",.265f,.43f,-.045f,-.035f,-.18f,.18f);
                a.Block("Cage east braking apron",new Vector3(.20f,-.125f,.15f),new Vector3(.18f,.008f,.18f),c.Frame);
            }
            return l;
        }

        // Fixed, close-fitting circular housing around the moving cage's sweep.
        // Its end slots are physical mouths; its outer annulus has no exit.
        internal static void CageSleeve(MechanicalAuthoring a,Vector3 origin,Quaternion rotation)
        {
            var f=new HardCampaignModules.Frame(a,origin,rotation);
            const float r=.154f;
            for(int i=0;i<40;i++)
            {
                float theta=(i+.5f)*Mathf.PI*2/40;
                // Broad mouths align with the two moving mouths near the stops.
                if(Mathf.Abs(Mathf.Sin(theta))<.42f)continue;
                var part=f.Block("Fixed cage swept housing "+i,new Vector3(Mathf.Cos(theta)*r,.05f+Mathf.Sin(theta)*r,0),new Vector3(.027f,.008f,.274f),a.Glass);
                part.transform.localRotation=rotation*Quaternion.Euler(0,0,theta*Mathf.Rad2Deg+90);
            }
        }

        internal static List<Vector3Int> RouteCells(int index)
        {
            var p=new List<Vector3Int>();
            switch(index)
            {
                case 27:
                    Path(p,V(-2,1,-1),V(0,1,-1),V(0,1,1),V(0,0,1),V(2,0,1),V(2,-1,1),V(2,-1,-1));break;
                case 28:
                    Path(p,V(-2,1,-2),V(0,1,-2),V(0,1,0),V(0,0,0),V(2,0,0),V(2,0,2),V(2,-1,2),V(0,-1,2),V(0,-1,3));
                    Path(p,V(0,1,-2),V(0,2,-2),V(1,2,-2));break;
                case 67:
                    Path(p,V(-2,1,1),V(-2,1,-1),V(1,1,-1),V(1,0,-1),V(1,0,1),V(2,0,1));break;
                case 68:
                    Path(p,V(-2,1,-1),V(0,1,-1),V(0,1,1),V(1,1,1),V(1,0,1),V(1,0,2),V(-1,0,2));break;
                case 77:
                    Path(p,V(-2,-1,-1),V(-1,-1,-1),V(-1,-1,0),V(0,-1,0),V(0,0,0),V(0,1,0),V(1,1,0),V(1,1,1),V(2,1,1));break;
                case 78:
                    Path(p,V(-2,-1,-1),V(0,-1,-1),V(0,-1,1),V(0,0,1),V(1,0,1),V(1,1,1),V(1,1,-1),V(2,1,-1));break;
                case 85:
                    Path(p,V(-2,-1,-1),V(-1,-1,-1),V(-1,0,-1),V(-1,0,0),V(0,0,0),V(1,0,0),V(1,1,0),V(1,1,1),V(2,1,1));break;
                case 87:
                    // Two real alternatives rejoin. The direct branch has a
                    // 60 mm collar, while the long branch has 116 mm clear cells.
                    Path(p,V(-2,0,0),V(2,0,0),V(2,-1,0));
                    Path(p,V(-2,0,0),V(-2,0,2),V(0,0,2),V(0,1,2),V(2,1,2),V(2,1,0),V(2,0,0));break;
                case 88:
                    Path(p,V(-2,1,-2),V(0,1,-2),V(0,1,0),V(0,0,0),V(2,0,0),V(2,0,2),V(0,0,2),V(0,-1,2),V(-2,-1,2));
                    Path(p,V(0,1,0),V(-2,1,0),V(-2,0,0),V(-2,0,2),V(0,0,2));break;
                default:throw new System.ArgumentOutOfRangeException(nameof(index));
            }
            return p;
        }
        private static LevelRuntime Network(CampaignLevelSpec s,CampaignBuildContext c)
        {
            List<Vector3Int> cells=RouteCells(s.index);const float pitch=.12f;
            Vector3Int start=cells[0],end;
            switch(s.index)
            {
                case 28:end=V(0,-1,3);break;
                case 87:end=V(2,-1,0);break;
                case 88:end=V(-2,-1,2);break;
                default:end=cells[cells.Count-1];break;
            }
            bool fluid=s.index==67||s.index==68||s.index==77||s.index==78;
            bool mercury=s.index==77||s.index==78;
            Vector3 offset=new Vector3(0,0,s.index==28?-.06f:0);
            Vector3 spawn=(Vector3)start*pitch+offset+Vector3.up*(mercury?.038f:-.038f);
            Vector2 exit=new Vector2(end.x*pitch,end.z*pitch+offset.z);
            // Rectangular retained-liquid bounds exactly match the collider box.
            float hx=.37f,hz=s.index==28?.43f:.37f;
            var l=fluid||s.index==85
                ?c.Planar(HardCampaignBuilder.Name(s),HardCampaignBuilder.Rectangle(hx,hz),new Vector2(spawn.x,spawn.z),exit,.72f,spawnY:spawn.y)
                :BossGeometry.Sphere(HardCampaignBuilder.Name(s),s.index==28?.66f:.60f,spawn,c);
            var a=HardCampaignBuilder.Author(l,c);
            // There is no inlet hole: the ball starts in the first sealed room.
            BossGeometry.Rooms(a,cells,pitch,offset,start,end,Vector3Int.zero,Vector3Int.down);
            if(s.index==87)
            {
                Vector3 at=offset+new Vector3(-.06f,0,0);
                // 60 mm square direct-route aperture, still twice ball diameter.
                foreach(int sign in new[]{-1,1})
                {
                    a.Block("Short route upper lower collar "+sign,at+new Vector3(0,sign*.046f,0),new Vector3(.008f,.032f,.124f),c.Glass);
                    a.Block("Short route side collar "+sign,at+new Vector3(0,0,sign*.046f),new Vector3(.008f,.060f,.032f),c.Glass);
                }
            }
            if(s.index==85||s.index==77)
            {
                // Explicit 76 mm neck in the middle rest cell, turning the
                // support plane into a visible alignment task.
                Vector3 at=offset+(s.index==85?new Vector3(-.06f,0,0):new Vector3(0,-.06f,0));
                foreach(int sign in new[]{-1,1})
                    a.Block("Transfer neck collar "+sign,at+(s.index==85?new Vector3(0,sign*.051f,0):new Vector3(sign*.051f,0,0)),
                        s.index==85?new Vector3(.008f,.026f,.124f):new Vector3(.026f,.008f,.124f),c.Glass);
            }
            if(fluid)AddLiquid(l,c,mercury,new Vector3(hx,.357f,hz));
            return l;
        }

        private static void AddLiquid(LevelRuntime l,CampaignBuildContext c,bool mercury,Vector3 halfSize)
        {
            var source=c.Lab.Levels[mercury?13:12].Prefab;
            var original=source.GetComponent<WaterVolume>();
            var water=l.gameObject.AddComponent<WaterVolume>();water.Profile=original.Profile;water.HalfSize=halfSize;
            // No single central cube in these layouts. The retained liquid
            // queries all actual static collider faces for wall rolling drag.
            water.Obstacle=new Bounds(new Vector3(20,20,20),Vector3.zero);
            var oldVisual=source.GetComponent<WaterVisuals>();
            var visual=l.gameObject.AddComponent<WaterVisuals>();visual.MercuryCutaway=mercury;visual.TracerMaterial=oldVisual.TracerMaterial;
            visual.FloorRenderer=l.transform.Find("Floor with circular cut").GetComponent<Renderer>();
            visual.FloorRenderer.sharedMaterial=oldVisual.FloorRenderer.sharedMaterial;
            foreach(string name in new[]{"Clear top cover","Clear side walls"})
                l.transform.Find(name).GetComponent<Renderer>().sharedMaterial=source.transform.Find(name).GetComponent<Renderer>().sharedMaterial;
            var volume=GameObject.CreatePrimitive(PrimitiveType.Cube);volume.name=mercury?"Retained mercury campaign volume":"Retained water campaign volume";
            volume.transform.SetParent(l.transform,false);volume.transform.localScale=halfSize*2;
            Object.DestroyImmediate(volume.GetComponent<Collider>());visual.VolumeRenderer=volume.GetComponent<Renderer>();
            visual.VolumeRenderer.sharedMaterial=oldVisual.VolumeRenderer.sharedMaterial;
        }
        private static Vector3Int V(int x,int y,int z)=>new Vector3Int(x,y,z);
        private static void Path(List<Vector3Int> result,params Vector3Int[] points)
        {
            if(!result.Contains(points[0]))result.Add(points[0]);
            for(int i=1;i<points.Length;i++)
            {
                Vector3Int current=points[i-1],delta=points[i]-current;
                if((delta.x!=0?1:0)+(delta.y!=0?1:0)+(delta.z!=0?1:0)!=1)throw new System.InvalidOperationException("Routes must join along exactly one axis.");
                Vector3Int step=new Vector3Int(System.Math.Sign(delta.x),System.Math.Sign(delta.y),System.Math.Sign(delta.z));
                while(current!=points[i]){current+=step;if(!result.Contains(current))result.Add(current);}
            }
        }
    }
}
