using System;
using System.Collections.Generic;
using UnityEngine;

namespace GravityBox.Venom
{
    [Serializable]
    public sealed class VenomGuidanceMemory
    {
        [Serializable] public struct Visit
        {
            public int Face;
            public Vector3 Point;
            public Visit(int face,Vector3 point){Face=face;Point=point;}
        }
        public List<Visit> Visits=new List<Visit>();
        public bool KnowsSwitch;
        // Tests inject an isolated store; normal Editor and player runs persist locally.
        public static bool PersistenceEnabled=true;
        private static string Key(int level)=>$"venom.guidance.v1.{level:00}";
        public static VenomGuidanceMemory Load(int level)
        {
            if(!PersistenceEnabled)return new VenomGuidanceMemory();
            try
            {
                var memory=JsonUtility.FromJson<VenomGuidanceMemory>(PlayerPrefs.GetString(Key(level),""));
                if(memory!=null && memory.Visits!=null && memory.Visits.Count<=24)
                {
                    bool valid=true;
                    foreach(var visit in memory.Visits)
                        valid&=visit.Face>=0 && visit.Face<6 && visit.Point.sqrMagnitude<=.26f*.26f*3 && !float.IsNaN(visit.Point.sqrMagnitude);
                    if(valid)return memory;
                }
            }
            catch(ArgumentException) { }
            return new VenomGuidanceMemory();
        }
        public void Remember(int face,Vector3 point)
        {
            if(Visits.Count>0 && Visits[Visits.Count-1].Face==face && Vector3.Distance(Visits[Visits.Count-1].Point,point)<.04f)return;
            if(Visits.Count==24)Visits.RemoveAt(0);
            Visits.Add(new Visit(face,point));
        }
        public void Save(int level)
        {
            if(!PersistenceEnabled)return;
            PlayerPrefs.SetString(Key(level),JsonUtility.ToJson(this));PlayerPrefs.Save();
        }
    }
}
