using System;
using System.Collections.Generic;
using System.Linq;

namespace Necrom.Core.Domain
{
    [Serializable] public sealed class SkillTreeCatalog { public int schemaVersion; public SkillNode[] skills; }
    [Serializable] public sealed class SkillNode {
        public string id, branch, effectKey, prerequisiteId;
        public int tier, maxLevel, spPerLevel;
    }
    public interface ISkillTree {
        int AvailableSp { get; }
        int LevelOf(string id);
        bool CanLevelUp(string id);
        bool TryLevelUp(string id);
    }
    public sealed class SkillTreeManager : ISkillTree {
        private readonly Dictionary<string,SkillNode> nodes = new Dictionary<string,SkillNode>(StringComparer.Ordinal);
        private readonly Dictionary<string,int> levels = new Dictionary<string,int>(StringComparer.Ordinal);
        public int AvailableSp { get; private set; }
        public SkillTreeManager(SkillTreeCatalog catalog,int initialSp=0) {
            if(catalog?.skills == null || catalog.skills.Length == 0 || initialSp<0) throw new ArgumentException("Invalid skill catalog");
            AvailableSp=initialSp;
            foreach(var n in catalog.skills) {
                if(n==null || string.IsNullOrWhiteSpace(n.id) || (n.branch!="Destruction" && n.branch!="Summoning" && n.branch!="CurseCC") ||
                   n.tier<1 || n.maxLevel<1 || n.spPerLevel<1 || string.IsNullOrWhiteSpace(n.effectKey) || !nodes.TryAdd(n.id,n))
                    throw new ArgumentException("Invalid or duplicate skill");
            }
            foreach(var n in nodes.Values) {
                if(n.tier==1 && !string.IsNullOrEmpty(n.prerequisiteId)) throw new ArgumentException("Tier one cannot have prerequisite");
                if(n.tier>1 && (string.IsNullOrEmpty(n.prerequisiteId) || !nodes.TryGetValue(n.prerequisiteId,out var parent) ||
                    parent.branch!=n.branch || parent.tier>=n.tier)) throw new ArgumentException("Invalid prerequisite");
                var seen=new HashSet<string>();var cursor=n;
                while(!string.IsNullOrEmpty(cursor.prerequisiteId)) {
                    if(!seen.Add(cursor.id))throw new ArgumentException("Skill prerequisite cycle");
                    cursor=nodes[cursor.prerequisiteId];
                }
            }
        }
        public int LevelOf(string id)=>id!=null && levels.TryGetValue(id,out var v)?v:0;
        public void GrantSp(int amount){ if(amount<0)throw new ArgumentOutOfRangeException(nameof(amount));AvailableSp=checked(AvailableSp+amount); }
        public bool CanLevelUp(string id){
            if(!nodes.TryGetValue(id??"",out var n) || LevelOf(id)>=n.maxLevel || AvailableSp<n.spPerLevel)return false;
            return string.IsNullOrEmpty(n.prerequisiteId) || LevelOf(n.prerequisiteId)==nodes[n.prerequisiteId].maxLevel;
        }
        public bool TryLevelUp(string id){
            if(!CanLevelUp(id))return false;
            var n=nodes[id];AvailableSp-=n.spPerLevel;levels[id]=LevelOf(id)+1;return true;
        }
        public IReadOnlyDictionary<string,int> SnapshotLevels()=>new Dictionary<string,int>(levels);
        // Restore only validated states; never partially mutate a live tree on corrupt saves.
        public void RestoreState(int availableSp, IReadOnlyDictionary<string,int> restoredLevels)
        {
            if (availableSp < 0 || restoredLevels == null) throw new ArgumentException("Invalid skill save");
            foreach (var pair in restoredLevels)
            {
                if (pair.Key == null || !nodes.TryGetValue(pair.Key, out var node) ||
                    pair.Value < 0 || pair.Value > node.maxLevel)
                    throw new ArgumentException("Invalid saved skill level");
            }
            foreach (var pair in restoredLevels)
            {
                if (pair.Value == 0) continue;
                var node = nodes[pair.Key];
                if (!string.IsNullOrEmpty(node.prerequisiteId) &&
                    (!restoredLevels.TryGetValue(node.prerequisiteId, out var previous) ||
                     previous != nodes[node.prerequisiteId].maxLevel))
                    throw new ArgumentException("Invalid saved prerequisite progression");
            }
            levels.Clear();
            foreach (var pair in restoredLevels) if (pair.Value > 0) levels.Add(pair.Key, pair.Value);
            AvailableSp = availableSp;
        }
    }
    [Serializable] public sealed class SynergyCatalog { public int schemaVersion; public SynergyRule[] rules; }
    [Serializable] public sealed class SynergyRule { public string trait, effectKey; public int[] thresholds; public float[] values; public string[] statKeys; }
    public sealed class ActiveSynergy { public string Trait,EffectKey,StatKey; public int Count, Threshold; public float Value; }
    public interface ISynergyManager { IReadOnlyList<ActiveSynergy> Evaluate(IEnumerable<MonsterEntry> deployed); }
    public sealed class SynergyManager : ISynergyManager {
        private readonly SynergyCatalog catalog;
        public SynergyManager(SynergyCatalog catalog){
            this.catalog=catalog??throw new ArgumentNullException(nameof(catalog));
            if(catalog.rules==null)throw new ArgumentException("Rules required");
            var seen=new HashSet<string>(StringComparer.Ordinal);
            foreach(var rule in catalog.rules) {
                if(rule==null || string.IsNullOrEmpty(rule.trait) || !seen.Add(rule.trait) || string.IsNullOrEmpty(rule.effectKey) ||
                   rule.thresholds==null || rule.values==null || rule.thresholds.Length==0 || rule.thresholds.Length!=rule.values.Length ||
                   (rule.statKeys!=null && rule.statKeys.Length!=rule.thresholds.Length))
                   throw new ArgumentException("Invalid synergy rule");
                for(int i=0;i<rule.thresholds.Length;i++)
                    if(rule.thresholds[i]<1 || (i>0 && rule.thresholds[i]<=rule.thresholds[i-1]) ||
                       float.IsNaN(rule.values[i]) || float.IsInfinity(rule.values[i]))throw new ArgumentException("Invalid synergy tier");
            }
        }
        public IReadOnlyList<ActiveSynergy> Evaluate(IEnumerable<MonsterEntry> deployed){
            if(deployed==null)throw new ArgumentNullException(nameof(deployed));
            var units=deployed.ToList();
            if(units.Count>6 || units.Any(u=>u==null || string.IsNullOrEmpty(u.id)) ||
               units.Select(u=>u.id).Distinct(StringComparer.Ordinal).Count()!=units.Count)
               throw new ArgumentException("Invalid permanent deck");
            var counts=new Dictionary<string,int>(StringComparer.Ordinal);
            foreach(var unit in units){
                if(unit.traits==null || unit.traits.Length<1 || unit.traits.Length>3 ||
                   unit.traits.Any(string.IsNullOrWhiteSpace) || unit.traits.Distinct(StringComparer.Ordinal).Count()!=unit.traits.Length)
                    throw new ArgumentException("Invalid monster traits");
                foreach(var trait in unit.traits)counts[trait]=counts.TryGetValue(trait,out var n)?n+1:1;
            }
            var output=new List<ActiveSynergy>();
            foreach(var rule in catalog.rules){
                int count=counts.TryGetValue(rule.trait,out var n)?n:0;
                for(int i=rule.thresholds.Length-1;i>=0;i--)if(count>=rule.thresholds[i]){
                    output.Add(new ActiveSynergy{Trait=rule.trait,EffectKey=rule.effectKey,Count=count,
                        StatKey=rule.statKeys!=null?rule.statKeys[i]:rule.effectKey,
                        Threshold=rule.thresholds[i],Value=rule.values[i]});break;
                }
            }
            return output;
        }
    }
    public enum AutoDeckGoal { CombatPower, MaximumSynergy }
    public sealed class AutoDeckRecommendation {
        public IReadOnlyList<string> MonsterIds { get; internal set; }
        public long BasePower { get; internal set; }
        public int SynergyScore { get; internal set; }
    }
    public sealed class AutoDeckRecommender {
        private readonly ISynergyManager synergy;
        public AutoDeckRecommender(ISynergyManager synergy){this.synergy=synergy??throw new ArgumentNullException(nameof(synergy));}
        // Power is a provisional deterministic proxy until combat formulas are approved.
        public AutoDeckRecommendation Recommend(IPermanentRoster roster,IEnumerable<MonsterEntry> catalog,AutoDeckGoal goal){
            if(roster==null || catalog==null)throw new ArgumentNullException();
            var owned=catalog.Where(e=>e!=null && roster.Owns(e.id)).GroupBy(e=>e.id).Select(g=>g.First())
                .OrderBy(e=>e.id,StringComparer.Ordinal).ToArray();
            int target=Math.Min(roster.UnlockedSlots,owned.Length);
            // Bounded heuristic for large owned pools; global optimality is not claimed.
            if(owned.Length>16)
            {
                var strongest=owned.OrderByDescending(e=>(long)e.hp+e.attack).ThenBy(e=>e.id,StringComparer.Ordinal).Take(8);
                var diverse=owned.GroupBy(e=>e.traits!=null&&e.traits.Length>0?e.traits[0]:"",StringComparer.Ordinal)
                    .SelectMany(g=>g.OrderByDescending(e=>(long)e.hp+e.attack).Take(2));
                owned=strongest.Concat(diverse).Distinct().Take(16).OrderBy(e=>e.id,StringComparer.Ordinal).ToArray();
            }
            if(target>6)throw new ArgumentException("Invalid unlocked slots");
            AutoDeckRecommendation best=null;
            // Exact combinatorial optimization is capped; never silently return an arbitrary subset.
            long combinations=1;
            for(int i=1;i<=target;i++){combinations=checked(combinations*(owned.Length-i+1)/i);if(combinations>250000)throw new InvalidOperationException("Recommendation search too large; use bounded solver");}
            var selected=new List<MonsterEntry>();
            void Search(int index){
                if(selected.Count==target){
                    long power=selected.Sum(e=>(long)e.hp+e.attack);
                    int score=synergy.Evaluate(selected).Sum(s=>s.Threshold);
                    var ids=selected.Select(e=>e.id).ToArray();
                    bool better=best==null || (goal==AutoDeckGoal.CombatPower
                        ? power>best.BasePower || (power==best.BasePower && score>best.SynergyScore)
                        : score>best.SynergyScore || (score==best.SynergyScore && power>best.BasePower));
                    if(better)best=new AutoDeckRecommendation{MonsterIds=ids,BasePower=power,SynergyScore=score};
                    return;
                }
                for(int i=index;i<=owned.Length-(target-selected.Count);i++){
                    selected.Add(owned[i]);Search(i+1);selected.RemoveAt(selected.Count-1);
                }
            }
            Search(0);
            return best??new AutoDeckRecommendation{MonsterIds=Array.Empty<string>()};
        }
        public bool Apply(PermanentMonsterRoster roster,AutoDeckRecommendation result){
            if(roster==null || result?.MonsterIds==null || result.MonsterIds.Count>roster.UnlockedSlots ||
               result.MonsterIds.Distinct(StringComparer.Ordinal).Count()!=result.MonsterIds.Count ||
               result.MonsterIds.Any(id=>!roster.Owns(id)))return false;
            for(int i=0;i<6;i++)roster.Clear(i);
            for(int i=0;i<result.MonsterIds.Count;i++)roster.Assign(i,result.MonsterIds[i]);
            return true;
        }
    }
}
