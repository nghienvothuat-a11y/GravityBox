using System;
using System.Collections.Generic;
using UnityEngine;

namespace GravityBox.Venom
{
    [Serializable]
    public sealed class VenomCampaignSave
    {
        public int Version=2;
        public List<string> Completed=new List<string>();
        public bool HomeUnlocked, RevealHome;
        public static bool PersistenceEnabled=true;
        public const string Key="venom.origin.v2";
        public static VenomCampaignSave Read()
        {
            if(!PersistenceEnabled)return new VenomCampaignSave();
            try{return JsonUtility.FromJson<VenomCampaignSave>(PlayerPrefs.GetString(Key,""))??new VenomCampaignSave();}
            catch(ArgumentException){return new VenomCampaignSave();}
        }
        public void Win(VenomCampaignDefinition definition)
        {
            if(!Completed.Contains(definition.Id))Completed.Add(definition.Id);
            if(definition.Boss&&!HomeUnlocked){HomeUnlocked=true;RevealHome=true;}
            Write();
        }
        public void Write(){if(!PersistenceEnabled)return;PlayerPrefs.SetString(Key,JsonUtility.ToJson(this));PlayerPrefs.Save();}
    }
}
