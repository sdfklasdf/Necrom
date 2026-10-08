using System;
using System.Collections.Generic;
using System.Linq;

namespace Necrom.Core.Domain
{
    public sealed class EffectiveCombatStats
    {
        public int MaxHealth { get; }
        public int Attack { get; }
        public int Defense { get; }
        public double AttackSpeedMultiplier { get; }
        public EffectiveCombatStats(int health,int attack,int defense=0,double speed=1) {
            if(health<=0 || attack<0)throw new ArgumentOutOfRangeException();
            if(defense<0 || speed<=0 || double.IsNaN(speed) || double.IsInfinity(speed))throw new ArgumentOutOfRangeException();
            MaxHealth=health;Attack=attack;Defense=defense;AttackSpeedMultiplier=speed;
        }
    }
    // Never mutates Combatant, RaiseSource, Formation or the canonical wave state.
    public sealed class StatCalculator
    {
        private readonly ISynergyManager synergies;
        private readonly ISkillTree skills;
        private readonly IReadOnlyDictionary<string,SkillNode> nodes;
        private readonly IReadOnlyDictionary<string,MonsterEntry> catalog;
        private readonly HashSet<string> supportedEffects=new HashSet<string>(StringComparer.Ordinal) {
            "party.hp.percent", "party.attack.percent", "summon.hp.percent", "summon.attack.percent"
        };
        public StatCalculator(ISynergyManager synergies,ISkillTree skills,SkillTreeCatalog skillCatalog,IEnumerable<MonsterEntry> monsters) {
            this.synergies=synergies??throw new ArgumentNullException(nameof(synergies));
            this.skills=skills??throw new ArgumentNullException(nameof(skills));
            if(skillCatalog?.skills==null || monsters==null)throw new ArgumentException("Missing catalogs");
            nodes=skillCatalog.skills.ToDictionary(n=>n.id,n=>n,StringComparer.Ordinal);
            catalog=monsters.ToDictionary(m=>m.id,m=>m,StringComparer.Ordinal);
        }
        public EffectiveCombatStats Calculate(string monsterId,IReadOnlyList<string> permanentDeck) {
            if(permanentDeck==null || permanentDeck.Count>6 || permanentDeck.Distinct().Count()!=permanentDeck.Count ||
               string.IsNullOrEmpty(monsterId) || !permanentDeck.Contains(monsterId) || !catalog.TryGetValue(monsterId,out var unit))
                throw new ArgumentException("Only an enlisted permanent monster receives permanent buffs");
            var deployed=permanentDeck.Select(id=>catalog.TryGetValue(id,out var entry)?entry:throw new ArgumentException("Unknown deck id")).ToArray();
            double hp=0,atk=0,def=0,speed=0;
            foreach(var s in synergies.Evaluate(deployed)) {
                var key=s.StatKey??s.EffectKey;
                if(key=="party.hp.percent")hp+=s.Value;
                else if(key=="party.attack.percent")atk+=s.Value;
                else if(key=="party.defense.percent")def+=s.Value;
                else if(key=="party.attack_speed.percent")speed+=s.Value;
                // SPEC_ONLY tiers remain explicitly not implemented; no simulated resurrection or AoE.
            }
            foreach(var n in nodes.Values) {
                if(n.branch!="Summoning" || skills.LevelOf(n.id)<=0)continue;
                // Explicit numeric effect mapping only; DRAFT_* effect IDs do not silently grant a buff.
                if(n.effectKey=="summon.hp.percent")hp+=0.01*skills.LevelOf(n.id);
                if(n.effectKey=="summon.attack.percent")atk+=0.01*skills.LevelOf(n.id);
            }
            if(hp< -0.99 || atk< -0.99 || def< -0.99 || speed< -0.99)throw new InvalidOperationException("Invalid negative multiplier");
            return new EffectiveCombatStats(checked((int)Math.Max(1,Math.Round(unit.hp*(1+hp)))),
                checked((int)Math.Max(0,Math.Round(unit.attack*(1+atk)))),
                checked((int)Math.Max(0,Math.Round(unit.hp*0.1*def))),1+speed);
        }
    }
    public enum UltimateKind { ShadowLegion, FusionOfDead, SoulRampage }
    public sealed class UltimateState
    {
        public UltimateKind Kind { get; }
        public int ConsumedRaiseCount { get; }
        public double DurationSeconds { get; }
        public UltimateState(UltimateKind kind,int consumedRaiseCount,double durationSeconds) {
            Kind=kind;ConsumedRaiseCount=consumedRaiseCount;DurationSeconds=durationSeconds;
        }
    }
    // Semantic contracts; actual targeting, AoE hits, entity removal and health pooling belong to runtime adapters.
    public static class UltimateContract
    {
        public static UltimateState ShadowLegion()=>new UltimateState(UltimateKind.ShadowLegion,0,10);
        public static UltimateState FusionOfDead(int consumedRaiseCount) {
            if(consumedRaiseCount<0 || consumedRaiseCount>5)throw new ArgumentOutOfRangeException(nameof(consumedRaiseCount));
            return new UltimateState(UltimateKind.FusionOfDead,consumedRaiseCount,0);
        }
        public static UltimateState SoulRampage()=>new UltimateState(UltimateKind.SoulRampage,0,0);
    }
}
