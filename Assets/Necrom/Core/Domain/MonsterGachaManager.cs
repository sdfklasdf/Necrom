using System;
using System.Collections.Generic;
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
        private static readonly int[] RateCutoffs = {4000,7000,9000,9800,10000};
        private readonly Dictionary<MonsterRarity,List<MonsterEntry>> pools = new Dictionary<MonsterRarity,List<MonsterEntry>>();
        private readonly Random random;
        private readonly PermanentMonsterRoster roster;
        public PermanentMonsterRoster Roster => roster;
        public int DrawsSinceLegendary { get; private set; }
        public IReadOnlyDictionary<string,int> Owned => legacyOwned;
        private readonly Dictionary<string,int> legacyOwned = new Dictionary<string,int>();
        public MonsterGachaManager(MonsterCatalogData data) : this(data, new PermanentMonsterRoster(), new Random()) {}
        public MonsterGachaManager(MonsterCatalogData data, PermanentMonsterRoster roster, Random random)
        {
            if(data?.monsters == null || data.monsters.Length == 0) throw new ArgumentException("Missing catalog");
            this.roster = roster ?? throw new ArgumentNullException(nameof(roster));
            this.random = random ?? throw new ArgumentNullException(nameof(random));
            foreach(MonsterRarity tier in Enum.GetValues(typeof(MonsterRarity))) pools.Add(tier,new List<MonsterEntry>());
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach(var e in data.monsters)
            {
                if(e == null || string.IsNullOrEmpty(e.id) || !ids.Add(e.id) || e.gachaWeight <= 0)
                    throw new ArgumentException("Invalid catalog entry");
                // Explicit rarity in catalog is required. Never infer rarity from index.
                if(!Enum.TryParse<MonsterRarity>(e.rarity,true,out var tier) ||
                    !Enum.IsDefined(typeof(MonsterRarity),tier))
                    throw new ArgumentException("Missing/invalid rarity for " + e.id);
                pools[tier].Add(e);
            }
            foreach(var pair in pools) if(pair.Value.Count == 0) throw new ArgumentException("Missing rarity pool: " + pair.Key);
        }
        private MonsterEntry Weighted(List<MonsterEntry> entries)
        {
            long total = 0; foreach(var e in entries) total = checked(total + e.gachaWeight);
            long roll = (long)(random.NextDouble() * total);
            foreach(var e in entries) { roll -= e.gachaWeight; if(roll < 0) return e; }
            return entries[entries.Count - 1];
        }
        public GachaResult DrawDetailed(Func<DrawCurrency,int,bool> trySpend, DrawCurrency currency, int cost)
        {
            if(trySpend == null || cost <= 0) throw new ArgumentException("Invalid purchase");
            if(currency != DrawCurrency.Diamond) throw new InvalidOperationException("Gacha only accepts diamonds.");
            if(!trySpend(currency,cost)) return null;
            return GrantDrawReward();
        }
        public IReadOnlyList<GachaResult> DrawBatch(Func<DrawCurrency,int,bool> trySpend,DrawCurrency currency,int unitCost,int count)
        {
            if(trySpend==null || unitCost<=0 || (count!=1 && count!=10))throw new ArgumentException("Invalid batch");
            if(currency!=DrawCurrency.Diamond)throw new InvalidOperationException("Gacha only accepts diamonds.");
            int total=checked(unitCost*count);
            if(!trySpend(currency,total))return null;
            var results=new List<GachaResult>(count);
            for(int i=0;i<count;i++)results.Add(GrantDrawReward());
            return results;
        }
        private GachaResult GrantDrawReward()
        {
            int roll = random.Next(10000);
            MonsterRarity tier = MonsterRarity.Legendary;
            if(DrawsSinceLegendary < LegendaryPityThreshold - 1)
                for(int i=0;i<RateCutoffs.Length;i++) if(roll<RateCutoffs[i]) { tier=(MonsterRarity)i; break; }
            var monster = Weighted(pools[tier]);
            bool first = roster.Grant(monster.id);
            legacyOwned[monster.id] = legacyOwned.TryGetValue(monster.id,out var n) ? checked(n+1) : 1;
            DrawsSinceLegendary = tier == MonsterRarity.Legendary ? 0 : checked(DrawsSinceLegendary+1);
            return new GachaResult { Monster=monster, Rarity=tier, FirstAcquisition=first, PityCounter=DrawsSinceLegendary };
        }
        // Keeps old call sites compiling while returning the new collection-backed reward.
        public MonsterEntry Draw(Func<DrawCurrency,int,bool> trySpend, DrawCurrency currency, int cost)
            => DrawDetailed(trySpend,currency,cost)?.Monster;
        // Must be restored from a trusted/versioned save before monetized use.
        public void RestorePityCounter(int value)
        {
            if(value<0 || value>=LegendaryPityThreshold) throw new ArgumentOutOfRangeException(nameof(value));
            DrawsSinceLegendary=value;
        }
    }
}
