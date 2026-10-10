using System;
using System.Collections.Generic;
using System.Linq;
namespace Necrom.Core.Domain
{
    [Serializable] public class MonsterCatalogData { public int schemaVersion; public string balanceStatus; public MonsterEntry[] monsters; }
    [Serializable] public class MonsterEntry { public string id,name,element; public string[] traits; public int hp,attack,gachaWeight; public string rarity; }
    public enum DrawCurrency { Gold, Diamond }
    public enum MonsterRarity { Common, Advanced, Rare, Hero, Legendary }
    public sealed class GachaResult
    {
        public MonsterEntry Monster;
        public MonsterRarity Rarity;
        public bool FirstAcquisition;
        public int PityCounter;
    }
    public sealed class MonsterGachaManager
    {
        public const int LegendaryRateBasisPoints = 200;
        public const int LegendaryPityThreshold = 200;
        public const int MileageTicketThreshold = 200;
        private static readonly int[] RateCutoffs = {4000,7000,9000,9800,10000};
        private readonly Dictionary<MonsterRarity,List<MonsterEntry>> pools = new Dictionary<MonsterRarity,List<MonsterEntry>>();
        private readonly Random random;
        private readonly PermanentMonsterRoster roster;
        private GachaMileageState mileage=new GachaMileageState();
        private bool mutationInProgress;
        public GachaGuaranteeMode GuaranteeMode { get; }
        public PermanentMonsterRoster Roster => roster;
        public int DrawsSinceLegendary { get; private set; }
        public int MileageProgress => (int)(mileage.successfulDraws % MileageTicketThreshold);
        public long AvailableSelectionTickets => mileage.successfulDraws / MileageTicketThreshold - mileage.redeemedTickets;
        public IReadOnlyDictionary<string,int> Owned => legacyOwned;
        private readonly Dictionary<string,int> legacyOwned = new Dictionary<string,int>();
        public MonsterGachaManager(MonsterCatalogData data) : this(data, new PermanentMonsterRoster(), new Random()) {}
        public MonsterGachaManager(MonsterCatalogData data, PermanentMonsterRoster roster, Random random)
            : this(data,roster,random,GachaGuaranteeMode.LegacyRandomPity) {}
        public MonsterGachaManager(MonsterCatalogData data, PermanentMonsterRoster roster, Random random,GachaGuaranteeMode guaranteeMode)
        {
            if(data?.monsters == null || data.monsters.Length == 0) throw new ArgumentException("Missing catalog");
            if(!Enum.IsDefined(typeof(GachaGuaranteeMode),guaranteeMode)) throw new ArgumentException("Invalid guarantee mode");
            GuaranteeMode=guaranteeMode;
            this.roster = roster ?? throw new ArgumentNullException(nameof(roster));
            this.random = random ?? throw new ArgumentNullException(nameof(random));
            foreach(MonsterRarity tier in Enum.GetValues(typeof(MonsterRarity))) pools.Add(tier,new List<MonsterEntry>());
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach(var original in data.monsters)
            {
                var e=guaranteeMode==GachaGuaranteeMode.SelectionMileage && original!=null ? Copy(original) : original;
                if(e == null || string.IsNullOrEmpty(e.id) || !ids.Add(e.id) || e.gachaWeight <= 0)
                    throw new ArgumentException("Invalid catalog entry");
                if(!Enum.TryParse<MonsterRarity>(e.rarity,true,out var tier) || !Enum.IsDefined(typeof(MonsterRarity),tier))
                    throw new ArgumentException("Missing/invalid rarity for " + e.id);
                pools[tier].Add(e);
            }
            foreach(var pair in pools) if(pair.Value.Count == 0) throw new ArgumentException("Missing rarity pool: " + pair.Key);
        }
        private static MonsterEntry Copy(MonsterEntry e) => new MonsterEntry {
            id=e.id,name=e.name,element=e.element,traits=e.traits==null?null:(string[])e.traits.Clone(),
            hp=e.hp,attack=e.attack,gachaWeight=e.gachaWeight,rarity=e.rarity
        };
        private MonsterEntry Weighted(List<MonsterEntry> entries)
        {
            long total = 0; foreach(var e in entries) total = checked(total + e.gachaWeight);
            long roll = (long)(random.NextDouble() * total);
            foreach(var e in entries) { roll -= e.gachaWeight; if(roll < 0) return e; }
            return entries[entries.Count - 1];
        }
        private void Preflight(int count)
        {
            if(GuaranteeMode==GachaGuaranteeMode.SelectionMileage) _=checked(mileage.successfulDraws+count);
            foreach(var entries in pools.Values)
                foreach(var e in entries) {
                    _=checked(roster.GetFragments(e.id)+count);
                    if(legacyOwned.TryGetValue(e.id,out var n)) _=checked(n+count);
                }
        }
        public GachaResult DrawDetailed(Func<DrawCurrency,int,bool> trySpend, DrawCurrency currency, int cost)
        {
            BeginMutation();
            try {
                if(trySpend == null || cost <= 0) throw new ArgumentException("Invalid purchase");
                if(currency != DrawCurrency.Diamond) throw new InvalidOperationException("Gacha only accepts diamonds.");
                Preflight(1);
                if(!trySpend(currency,cost)) return null;
                return GrantDrawReward();
            } finally { mutationInProgress=false; }
        }
        public IReadOnlyList<GachaResult> DrawBatch(Func<DrawCurrency,int,bool> trySpend,DrawCurrency currency,int unitCost,int count)
        {
            BeginMutation();
            try {
                if(trySpend==null || unitCost<=0 || (count!=1 && count!=10))throw new ArgumentException("Invalid batch");
                if(currency!=DrawCurrency.Diamond)throw new InvalidOperationException("Gacha only accepts diamonds.");
                int total=checked(unitCost*count);
                Preflight(count);
                if(!trySpend(currency,total))return null;
                var results=new List<GachaResult>(count);
                for(int i=0;i<count;i++)results.Add(GrantDrawReward());
                return results;
            } finally { mutationInProgress=false; }
        }
        private GachaResult GrantEntry(MonsterEntry monster,MonsterRarity tier)
        {
            int owned=checked((legacyOwned.TryGetValue(monster.id,out var n)?n:0)+1);
            _=checked(roster.GetFragments(monster.id)+1);
            bool first=roster.Grant(monster.id);
            legacyOwned[monster.id]=owned;
            return new GachaResult { Monster=GuaranteeMode==GachaGuaranteeMode.SelectionMileage?Copy(monster):monster,
                Rarity=tier,FirstAcquisition=first };
        }
        private GachaResult GrantDrawReward()
        {
            int roll = random.Next(10000);
            MonsterRarity tier = MonsterRarity.Legendary;
            if(GuaranteeMode==GachaGuaranteeMode.SelectionMileage || DrawsSinceLegendary < LegendaryPityThreshold - 1)
                for(int i=0;i<RateCutoffs.Length;i++) if(roll<RateCutoffs[i]) { tier=(MonsterRarity)i; break; }
            var result=GrantEntry(Weighted(pools[tier]),tier);
            if(GuaranteeMode==GachaGuaranteeMode.SelectionMileage)
                mileage.successfulDraws=checked(mileage.successfulDraws+1);
            else {
                DrawsSinceLegendary=tier==MonsterRarity.Legendary?0:checked(DrawsSinceLegendary+1);
                result.PityCounter=DrawsSinceLegendary;
            }
            return result;
        }
        public MonsterEntry Draw(Func<DrawCurrency,int,bool> trySpend, DrawCurrency currency, int cost)
            => DrawDetailed(trySpend,currency,cost)?.Monster;
        public void RestorePityCounter(int value)
        {
            RequireIdle();
            if(GuaranteeMode!=GachaGuaranteeMode.LegacyRandomPity)throw new InvalidOperationException("Legacy pity cannot restore mileage; explicit migration required.");
            if(value<0 || value>=LegendaryPityThreshold) throw new ArgumentOutOfRangeException(nameof(value));
            DrawsSinceLegendary=value;
        }
        // Single-thread owner; wallet callbacks must not mutate roster directly.
        private void RequireIdle()
        {
            if(mutationInProgress)throw new InvalidOperationException("Gacha mutation already in progress.");
        }
        private void BeginMutation()
        {
            RequireIdle(); mutationInProgress=true;
        }
        private void RequireMileage()
        {
            if(GuaranteeMode!=GachaGuaranteeMode.SelectionMileage)throw new InvalidOperationException("Selection mileage mode required.");
        }
        public GachaMileageState CaptureMileageState()
        {
            RequireMileage();
            return new GachaMileageState {successfulDraws=mileage.successfulDraws,redeemedTickets=mileage.redeemedTickets};
        }
        public void RestoreMileageState(GachaMileageState state)
        {
            RequireIdle();
            RequireMileage();
            if(state==null || state.schemaVersion!=1 || state.successfulDraws<0 || state.redeemedTickets<0 ||
                state.redeemedTickets>state.successfulDraws/MileageTicketThreshold)throw new ArgumentException("Invalid mileage snapshot");
            mileage=new GachaMileageState{successfulDraws=state.successfulDraws,redeemedTickets=state.redeemedTickets};
        }
        public IReadOnlyList<string> GetMileageCandidateIds(MileageSelection selection)
        {
            RequireMileage();
            if(selection==null)throw new ArgumentNullException(nameof(selection));
            return pools[MonsterRarity.Legendary].Where(e=>e.traits!=null && e.traits.Contains(selection.TraitId,StringComparer.Ordinal))
                .Select(e=>e.id).ToArray();
        }
        public GachaResult RedeemMileage(MileageSelection selection,string monsterId)
        {
            BeginMutation();
            try {
                RequireMileage();
                if(selection==null)throw new ArgumentNullException(nameof(selection));
                if(AvailableSelectionTickets<=0 || string.IsNullOrEmpty(monsterId) ||
                    !GetMileageCandidateIds(selection).Contains(monsterId,StringComparer.Ordinal))return null;
                var monster=pools[MonsterRarity.Legendary].Single(e=>e.id==monsterId);
                long redeemed=checked(mileage.redeemedTickets+1);
                var result=GrantEntry(monster,MonsterRarity.Legendary);
                mileage.redeemedTickets=redeemed;
                return result;
            } finally { mutationInProgress=false; }
        }
    }
}
