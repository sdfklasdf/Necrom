using System;
using System.Collections.Generic;
using Necrom.Core.Domain;
using UnityEngine;

namespace Necrom.FirstPlayable.Runtime
{
    [Serializable] public sealed class RosterSaveRecord { public string id; public int stars; public int fragments; }
    [Serializable] public sealed class RosterSaveData
    {
        public int schemaVersion=1;
        public int unlockedSlots=3;
        public RosterSaveRecord[] owned;
        public string[] deck;
    }
    // Single runtime owner, usable even when legacy demo UI GameObject is disabled.
    public static class PermanentRosterRuntime
    {
        private static PermanentMonsterRoster current;
        public static PermanentMonsterRoster GetOrCreate(MonsterCatalogData catalog)
        {
            if(current!=null)return current;
            if(catalog?.monsters==null)throw new ArgumentException("Monster catalog required");
            PermanentMonsterRoster loaded;
            if(!PermanentRosterSaveManager.TryLoad(out loaded))
            {
                if(PermanentRosterSaveManager.HasSavedData)
                    throw new InvalidOperationException("Invalid saved roster; refusing data overwrite");
                loaded=new PermanentMonsterRoster();
                if(PermanentRosterDebugInjector.InjectWhenEmpty(loaded,catalog))
                    PermanentRosterSaveManager.Save(loaded);
            }
            current=loaded;
            Debug.Log("[NECRO AWU-6] Runtime roster ready: owned="+current.OwnedCount+
                ", slots="+current.UnlockedSlots);
            return current;
        }
    }
    public static class PermanentRosterSaveManager
    {
        private const string Key="NECROM_PERMANENT_ROSTER_V1";
        public static bool TryLoad(out PermanentMonsterRoster roster)
        {
            roster=null;
            if(!PlayerPrefs.HasKey(Key))return false;
            try
            {
                var save=JsonUtility.FromJson<RosterSaveData>(PlayerPrefs.GetString(Key));
                if(save==null || save.schemaVersion!=1 || save.owned==null || save.deck==null ||
                   save.deck.Length!=6 || save.unlockedSlots<3 || save.unlockedSlots>6)
                    throw new InvalidOperationException("Save schema mismatch");
                var restored=new PermanentMonsterRoster();
                restored.UnlockSlotAtApprovedLevel(save.unlockedSlots);
                var ids=new HashSet<string>(StringComparer.Ordinal);
                foreach(var record in save.owned)
                {
                    if(record==null || !ids.Add(record.id??""))throw new InvalidOperationException("Duplicate save id");
                    restored.RestoreOwned(record.id,record.stars,record.fragments);
                }
                for(int i=0;i<save.deck.Length;i++)
                    if(!string.IsNullOrEmpty(save.deck[i]) && !restored.Assign(i,save.deck[i]))
                        throw new InvalidOperationException("Invalid saved formation");
                roster=restored;return true;
            }
            catch(Exception e)
            {
                // Do not overwrite damaged data or replace it with a fabricated starter deck.
                Debug.LogError("Roster save rejected; backup preserved: "+e.Message);
                return false;
            }
        }
        public static bool HasSavedData=>PlayerPrefs.HasKey(Key);
        public static void Save(PermanentMonsterRoster roster)
        {
            if(roster==null)throw new ArgumentNullException(nameof(roster));
            var records=new List<RosterSaveRecord>();
            foreach(var id in roster.OwnedIds)
                records.Add(new RosterSaveRecord{id=id,stars=roster.GetStars(id),fragments=roster.GetFragments(id)});
            var slots=new string[6];
            for(int i=0;i<6;i++)slots[i]=roster.GetDeckSlot(i);
            var data=new RosterSaveData{owned=records.ToArray(),deck=slots,unlockedSlots=roster.UnlockedSlots};
            PlayerPrefs.SetString(Key,JsonUtility.ToJson(data));
            PlayerPrefs.Save();
        }
    }

    public static class PermanentRosterDebugInjector
    {
        public static bool InjectWhenEmpty(PermanentMonsterRoster roster,MonsterCatalogData catalog)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if(roster==null || catalog?.monsters==null || roster.OwnedCount!=0)return false;
            var chosen=new List<string>(3);
            foreach(var m in catalog.monsters)
            {
                if(m!=null && m.rarity=="Common" && !string.IsNullOrEmpty(m.id) && !chosen.Contains(m.id))
                {
                    chosen.Add(m.id);
                    if(chosen.Count==3)break;
                }
            }
            if(chosen.Count!=3){Debug.LogError("Debug injector needs 3 Common monsters");return false;}
            for(int i=0;i<3;i++){roster.Grant(chosen[i]);roster.Assign(i,chosen[i]);}
            Debug.Log("[NECRO AWU-6] Debug starter deck injected: "+string.Join(", ",chosen));
            return true;
#else
            return false;
#endif
        }
    }
}
