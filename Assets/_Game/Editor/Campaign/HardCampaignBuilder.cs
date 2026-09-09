using System;
using GravityBox.Gameplay;
using UnityEngine;

namespace GravityBox.Editor
{
    internal static class HardCampaignBuilder
    {
        internal static readonly int[] Indices={15,16,17,18,23,25,27,28,33,35,36,37,38,45,46,47,48,56,57,58,67,68,77,78,85,86,87,88,91,92,93,95,96,97,98};
        internal static bool Handles(int index)=>Array.IndexOf(Indices,index)>=0;
        internal static LevelRuntime Build(CampaignLevelSpec spec,CampaignBuildContext c)
        {
            if(!Handles(spec.index))throw new ArgumentOutOfRangeException(nameof(spec.index));
            switch(spec.index)
            {
                case 15: case 16: case 17: case 18:return HardMechanicalLevels.Bridge(spec,c);
                case 33: case 35: case 36: case 37: case 38:return HardMechanicalLevels.Cooperation(spec,c);
                case 45: case 46: case 47: case 48: case 91: case 92: case 93: case 95: case 97: case 98:return HardMechanicalLevels.Memory(spec,c);
                case 56: case 57: case 58: case 96:return HardMechanicalLevels.Flight(spec,c);
                default:return HardSpatialLevels.Build(spec,c);
            }
        }
        internal static string Name(CampaignLevelSpec s)=>$"C{s.index:000} {s.title}";
        internal static Vector2[] Rectangle(float x,float z)=>new[]{new Vector2(-x,-z),new Vector2(x,-z),new Vector2(x,z),new Vector2(-x,z)};
        internal static MechanicalAuthoring Author(LevelRuntime level,CampaignBuildContext c)=>new MechanicalAuthoring(level,c.Glass,c.Frame,c.Rim,c.Contact);
        internal static void Wall(MechanicalAuthoring a,string name,float x0,float x1,float z0,float z1,float bottom,float top)
        {a.Block(name,new Vector3((x0+x1)*.5f,(bottom+top)*.5f,(z0+z1)*.5f),new Vector3(x1-x0,top-bottom,z1-z0),a.Glass);}
    }
}
