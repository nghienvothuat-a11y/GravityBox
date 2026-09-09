using GravityBox.Gameplay;
using UnityEngine;

namespace GravityBox.Editor
{
    internal static class BossCampaignBuilder
    {
        internal static LevelRuntime Build(CampaignLevelSpec spec,CampaignBuildContext c)
        {
            LevelRuntime level;
            switch(spec.index)
            {
                case 10:level=BossStaticBuilders.Lion(spec,c);break;
                case 20:level=BossMechanicalBuilders.Garden(spec,c);break;
                case 30:level=BossStaticBuilders.Constellation(spec,c,false);break;
                case 40:level=BossCooperationBuilder.Build(spec,c,false);break;
                case 50:level=BossMechanicalBuilders.Lotus(spec,c);break;
                case 60:level=BossMechanicalBuilders.Orbit(spec,c);break;
                case 70:level=BossStaticBuilders.Liquid(spec,c,false);break;
                case 80:level=BossStaticBuilders.Liquid(spec,c,true);break;
                case 90:level=BossStaticBuilders.Constellation(spec,c,true);break;
                case 100:level=BossCooperationBuilder.Build(spec,c,true);break;
                default:throw new System.ArgumentOutOfRangeException(nameof(spec.index),"Boss authoring exists only at the ten campaign milestones.");
            }
            level.name=$"C{spec.index:000} BOSS {spec.title}";
            return level;
        }
    }
}
