using System;
using UnityEngine;

namespace GravityBox.Venom
{
    [Flags] public enum VenomSkill { None=0, Arrive=1, Climb=2, Hold=4, Divide=8, Cooperate=16, Allocate=32 }

    /// <summary>Knowledge survives a retry; apparatus state and orders never do.</summary>
    [Serializable] public sealed class VenomJourneyProgress
    {
        public int Skills, Completed;
        public static bool PersistenceEnabled=true;
        public const string StorageKey="venom.journey.v1";
        public static string PersistenceKey=StorageKey;
        public bool Knows(VenomSkill skill)=>(Skills&(int)skill)!=0;
        public int Familiarity { get { int n=0;for(int i=0;i<5;i++)if((Completed&(1<<i))!=0)n++;return n; } }
        public static VenomJourneyProgress Load()
        {
            if(!PersistenceEnabled)return new VenomJourneyProgress();
            try
            {
                var result=JsonUtility.FromJson<VenomJourneyProgress>(PlayerPrefs.GetString(PersistenceKey,""))??new VenomJourneyProgress();
                result.Skills&=63;result.Completed&=31;return result;
            }
            catch(ArgumentException){return new VenomJourneyProgress();}
        }
        public bool Learn(VenomSkill skill)
        {
            if(Knows(skill))return false;Skills|=(int)skill;Save();return true;
        }
        public void Finish(int chapter){Completed|=1<<(chapter-1);Save();}
        private void Save(){if(!PersistenceEnabled)return;PlayerPrefs.SetString(PersistenceKey,JsonUtility.ToJson(this));PlayerPrefs.Save();}
    }
}
