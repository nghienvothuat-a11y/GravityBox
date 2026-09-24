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
        [NonSerialized] private string storageKey=Key;
        public static VenomCampaignSave Read(string progressKey=null)
        {
            string key=string.IsNullOrEmpty(progressKey)?Key:progressKey;
            VenomCampaignSave result;
            try{result=PersistenceEnabled?JsonUtility.FromJson<VenomCampaignSave>(PlayerPrefs.GetString(key,"")):null;}
            catch(ArgumentException){result=null;}
            result??=new VenomCampaignSave();
            result.storageKey=key;
            return result;
        }
        public void Win(VenomCampaignDefinition definition)
        {
            if(!Completed.Contains(definition.Id))Completed.Add(definition.Id);
            if(definition.Boss&&!HomeUnlocked){HomeUnlocked=true;RevealHome=true;}
            Write();
        }
        public void Write(){if(!PersistenceEnabled)return;PlayerPrefs.SetString(storageKey,JsonUtility.ToJson(this));PlayerPrefs.Save();}
    }
}
