using System;
using System.Collections.Generic;
using Necrom.Core.Domain;
using DomainEntityId = Necrom.Core.Domain.EntityId;
using UnityEngine;
using UnityEngine.UI;

namespace Necrom.FirstPlayable.Runtime
{
    [DisallowMultipleComponent]
    public sealed class PermanentDeckCombatSpawner : MonoBehaviour
    {
        private readonly Dictionary<string,Combatant> units = new Dictionary<string,Combatant>(StringComparer.Ordinal);
        private readonly Dictionary<string,GameObject> visuals = new Dictionary<string,GameObject>(StringComparer.Ordinal);
        private PermanentDeckStatBridge statBridge;
        private FirstPlayableTargetingController targeting;
        private FirstPlayableDamageDeathPipeline pipeline;
        private FirstPlayableBattleRuntimeController battle;
        private FirstPlayableDefenseWaveRuntimeController defenseWave;
        private RectTransform spawnHost;
        private MonsterCatalogData catalog;
        private SkillTreeCatalog skillCatalog;
        private SynergyCatalog synergyCatalog;
        private SkillTreeManager skills;
        private PermanentMonsterRoster boundRoster;
        private readonly Dictionary<string,double> attackTimers=new Dictionary<string,double>(StringComparer.Ordinal);
        private int sequence;
        public int SpawnedCount => units.Count;
        public int LivingCount { get { int n=0; foreach(var u in units.Values) if(u!=null && u.LifeState==CombatantLifeState.Active)n++; return n; } }
        public int ConfirmedPermanentHits { get; private set; }
        public int ConfirmedEnemyHits { get; private set; }

        public void Initialize(RectTransform host, FirstPlayableTargetingController target,
            FirstPlayableDamageDeathPipeline damage,FirstPlayableBattleRuntimeController battleController)
        {
            if(spawnHost!=null)throw new InvalidOperationException("Spawner already initialized.");
            spawnHost=host??throw new ArgumentNullException(nameof(host));
            targeting=target??throw new ArgumentNullException(nameof(target));
            pipeline=damage??throw new ArgumentNullException(nameof(damage));
            battle=battleController??throw new ArgumentNullException(nameof(battleController));
            statBridge=GetComponent<PermanentDeckStatBridge>() ?? gameObject.AddComponent<PermanentDeckStatBridge>();
            var monsters=Resources.Load<TextAsset>("monster_catalog");
            var skillsAsset=Resources.Load<TextAsset>("skill_tree_v1");
            var synergyAsset=Resources.Load<TextAsset>("synergy_rules_v2");
            if(monsters==null || skillsAsset==null || synergyAsset==null)
            {
                Debug.LogError("Permanent deck spawn disabled: V2 catalog assets unavailable.");
                enabled=false;return;
            }
            try
            {
                catalog=JsonUtility.FromJson<MonsterCatalogData>(monsters.text);
                skillCatalog=JsonUtility.FromJson<SkillTreeCatalog>(skillsAsset.text);
                synergyCatalog=JsonUtility.FromJson<SynergyCatalog>(synergyAsset.text);
                if(catalog?.monsters==null || skillCatalog?.skills==null || synergyCatalog?.rules==null)
                    throw new InvalidOperationException("Invalid V2 catalog structures.");
                skills=new SkillTreeManager(skillCatalog);
            }
            catch(Exception ex){ Debug.LogError("Permanent deck catalog invalid: "+ex);enabled=false; }
        }
        public void ConfigureDefenseWave(FirstPlayableDefenseWaveRuntimeController waves)
        {
            if(defenseWave!=null)throw new InvalidOperationException("Defense wave already configured");
            defenseWave=waves??throw new ArgumentNullException(nameof(waves));
        }
        private void Update()
        {
            if(!enabled || spawnHost==null || battle==null || catalog==null)return;
            PermanentMonsterRoster roster;
            try { roster=PermanentRosterRuntime.GetOrCreate(catalog); }
            catch(Exception ex) { Debug.LogError("Permanent roster unavailable: "+ex);enabled=false;return; }
            if(boundRoster==null)
            {
                boundRoster=roster;
                try {
                    var calc=new StatCalculator(new SynergyManager(synergyCatalog),skills,skillCatalog,catalog.monsters);
                    statBridge.Initialize(calc,boundRoster);
                }
                catch(Exception ex){Debug.LogError("Permanent stat bridge disabled: "+ex);enabled=false;return;}
            }
            if(!ReferenceEquals(roster,boundRoster))
            {
                Debug.LogError("Permanent roster ownership changed mid-session; refusing duplicate spawn.");
                enabled=false;return;
            }
            if(battle.Phase!=BattlePhase.Running){attackTimers.Clear();return;}
            SyncSpawnedDeck(roster);
            if(!targeting.TryAcquireTarget(out var target) || target==null || target.Model==null)return;
            foreach(var pair in units)
            {
                if(pair.Value==null || pair.Value.LifeState!=CombatantLifeState.Active)continue;
                if(target.Model.LifeState!=CombatantLifeState.Active)break;
                if(!targeting.TryAcquireTarget(out target) || target?.Model==null)break;
                try
                {
                    var id=pair.Key;
                    var stats=statBridgeStats[id];
                    if(stats.Attack<=0)continue;
                    double elapsed=attackTimers.TryGetValue(id,out var time)?time:0;
                    elapsed+=Time.deltaTime;
                    double interval=0.8/stats.AttackSpeedMultiplier;
                    attackTimers[id]=elapsed;
                    if(elapsed<interval)continue;
                    attackTimers[id]=Math.Max(0,elapsed-interval);
                    // Canonical source identity must remain stable across player, Raise and permanent attacks.
                    var sourceId=new DomainEntityId("source:"+target.Model.Id.Value);
                    var damageResult=pipeline.Apply(target,stats.Attack,sourceId);
                    if(damageResult.Changed)
                    {
                        ConfirmedPermanentHits++;
                        if(ConfirmedPermanentHits<=6)Debug.Log("[NECRO AWU-6] Permanent attack: "+id+" -> "+target.Model.Id.Value+" damage="+stats.Attack);
                    }
                    if(damageResult.BecameDefeated)
                    {
                        if(defenseWave==null)throw new InvalidOperationException("Defense wave notifier missing");
                        defenseWave.RecordDefeatedThreat(target);
                        break;
                    }
                }
                catch(Exception ex){Debug.LogError("Permanent attack failed: "+ex);enabled=false;break;}
            }
        }
        private readonly Dictionary<string,EffectiveCombatStats> statBridgeStats = new Dictionary<string,EffectiveCombatStats>(StringComparer.Ordinal);
        // Formation editor invokes this only after a successful saved commit.
        // Clear all old combat entities to recalculate party-wide synergy on every replacement.
        public void RefreshPermanentFormation()
        {
            if(!enabled || boundRoster==null || spawnHost==null || battle==null)return;
            foreach(var id in new List<string>(units.Keys))RemoveUnit(id);
            if(battle.Phase==BattlePhase.Running)SyncSpawnedDeck(boundRoster);
            Debug.Log("[NECRO AWU-12] Permanent deck refreshed; active="+units.Count);
        }
        private void SyncSpawnedDeck(PermanentMonsterRoster roster)
        {
            var selected=new HashSet<string>(StringComparer.Ordinal);
            for(int i=0;i<roster.UnlockedSlots;i++)
            {
                var id=roster.GetDeckSlot(i);
                if(!string.IsNullOrEmpty(id))selected.Add(id);
            }
            var obsolete=new List<string>();
            foreach(var id in units.Keys)if(!selected.Contains(id))obsolete.Add(id);
            foreach(var id in obsolete)RemoveUnit(id);
            int index=0;
            foreach(var id in selected)
            {
                if(!units.ContainsKey(id))SpawnUnit(id,index);
                index++;
            }
        }
        private void SpawnUnit(string monsterId,int slot)
        {
            if(units.ContainsKey(monsterId) || statBridge==null || spawnHost==null)return;
            EffectiveCombatStats stats;
            try { stats=statBridge.RegisterPermanentEntity(monsterId); }
            catch(Exception ex){Debug.LogError("Permanent spawn rejected: "+ex.Message);return;}
            var model=new Combatant(new DomainEntityId("permanent:"+monsterId+":"+(++sequence)),
                monsterId,Faction.Player,stats.MaxHealth);
            // Separate visual entity: it never consumes Raise's five Formation slots.
            var go=new GameObject("PermanentDeck:"+monsterId,typeof(RectTransform),typeof(Image));
            go.transform.SetParent(spawnHost,false);
            var rt=go.GetComponent<RectTransform>();
            rt.anchorMin=rt.anchorMax=new Vector2(.15f+(slot%3)*.32f,.30f+(slot/3)*.35f);
            rt.sizeDelta=new Vector2(66,66);
            var icon=go.GetComponent<Image>();
            icon.color=new Color(.05f,.34f,.82f,.97f);
            icon.raycastTarget=false;
            var badge=new GameObject("PermanentRosterLabel",typeof(RectTransform),typeof(Text));
            badge.transform.SetParent(go.transform,false);
            var badgeRect=badge.GetComponent<RectTransform>();
            badgeRect.anchorMin=Vector2.zero;badgeRect.anchorMax=Vector2.one;
            badgeRect.offsetMin=Vector2.zero;badgeRect.offsetMax=Vector2.zero;
            var badgeText=badge.GetComponent<Text>();
            badgeText.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            badgeText.fontSize=15;badgeText.alignment=TextAnchor.MiddleCenter;
            badgeText.color=Color.white;badgeText.text="PERM "+monsterId.Replace("MON_","");
            badgeText.raycastTarget=false;
            units.Add(monsterId,model);
            visuals.Add(monsterId,go);
            statBridgeStats.Add(monsterId,stats);
            attackTimers[monsterId]=0;
            Debug.Log("[NECRO AWU-6] Permanent spawned: "+monsterId+" HP="+stats.MaxHealth+" ATK="+stats.Attack);
        }
        // Enemy attacks are resolved against permanent Combatant health, never Raise slots.
        public bool TryTakeEnemyHit(int damage)
        {
            if(damage<=0)throw new ArgumentOutOfRangeException(nameof(damage));
            foreach(var pair in units)
            {
                if(pair.Value==null || pair.Value.LifeState!=CombatantLifeState.Active)continue;
                var stats=statBridgeStats.TryGetValue(pair.Key,out var effective)?effective:null;
                var mitigated=Math.Max(1,damage-(stats?.Defense??0));
                var changed=pair.Value.ApplyDamage(mitigated);
                if(changed)
                {
                    ConfirmedEnemyHits++;
                    Debug.Log("[NECRO AWU-6] Enemy hit permanent monster "+pair.Key+"; HP="+pair.Value.Health);
                }
                return changed;
            }
            return false;
        }
        public bool ApplyIncomingDamage(string monsterId,int damage)
        {
            if(damage<=0)throw new ArgumentOutOfRangeException(nameof(damage));
            if(!units.TryGetValue(monsterId??"",out var model) || model==null)return false;
            var stats=statBridgeStats.TryGetValue(monsterId,out var effective)?effective:null;
            return model.ApplyDamage(Math.Max(1,damage-(stats?.Defense??0)));
        }
        public Combatant GetPermanentCombatant(string monsterId)
            => monsterId!=null && units.TryGetValue(monsterId,out var model)?model:null;
        private void RemoveUnit(string id)
        {
            if(visuals.TryGetValue(id,out var visual) && visual!=null)Destroy(visual);
            visuals.Remove(id);units.Remove(id);statBridgeStats.Remove(id);attackTimers.Remove(id);
            if(statBridge!=null)statBridge.UnregisterPermanentEntity(id);
        }
        private void OnDestroy()
        {
            foreach(var id in new List<string>(units.Keys))RemoveUnit(id);
        }
    }
}
