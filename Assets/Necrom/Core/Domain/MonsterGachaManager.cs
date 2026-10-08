using System;
using System.Collections.Generic;
namespace Necrom.Core.Domain
{
 [Serializable] public class MonsterCatalogData { public int schemaVersion; public string balanceStatus; public MonsterEntry[] monsters; }
 [Serializable] public class MonsterEntry { public string id,name,element; public int hp,attack,gachaWeight; }
 public enum DrawCurrency { Gold, Diamond }
 public sealed class MonsterGachaManager
 {
  // 2% legendary tier; change only with disclosure and balance approval.
  public const int LegendaryRateBasisPoints = 200;
  readonly List<MonsterEntry> regular = new List<MonsterEntry>();
  readonly List<MonsterEntry> legendary = new List<MonsterEntry>();
  readonly Random random = new Random();
  readonly Dictionary<string,int> owned = new Dictionary<string,int>();
  public IReadOnlyDictionary<string,int> Owned => owned;
  public MonsterGachaManager(MonsterCatalogData data)
  {
   if(data == null || data.monsters == null) throw new ArgumentException("Missing catalog");
   var ids = new HashSet<string>();
   foreach(var e in data.monsters)
   {
    if(e == null || string.IsNullOrEmpty(e.id) || !ids.Add(e.id) || e.gachaWeight <= 0)
     throw new ArgumentException("Invalid monster entry");
    int index;
    if(!e.id.StartsWith("MON_") || !int.TryParse(e.id.Substring(4),out index))
     throw new ArgumentException("Invalid monster id");
    // Draft catalog: MON_096 through MON_100 are designated legendary.
    (index >= 96 && index <= 100 ? legendary : regular).Add(e);
   }
   if(regular.Count == 0 || legendary.Count == 0) throw new ArgumentException("Both pools required");
  }
  static MonsterEntry Weighted(List<MonsterEntry> pool, Random rng)
  {
   long total=0;
   foreach(var e in pool) checked { total+=e.gachaWeight; }
   long roll=(long)(rng.NextDouble()*total);
   foreach(var e in pool) { roll-=e.gachaWeight; if(roll<0) return e; }
   return pool[pool.Count-1];
  }
  // The currency callback must reject an unaffordable purchase.
  public MonsterEntry Draw(Func<DrawCurrency,int,bool> trySpend, DrawCurrency currency, int cost)
  {
   if(trySpend == null || cost <= 0) throw new ArgumentException("Invalid payment");
   if(!trySpend(currency,cost)) return null;
   var pool = random.Next(10000) < LegendaryRateBasisPoints ? legendary : regular;
   MonsterEntry reward = Weighted(pool,random);
   owned[reward.id] = owned.TryGetValue(reward.id,out int n) ? checked(n+1) : 1;
   return reward;
  }
 }
}