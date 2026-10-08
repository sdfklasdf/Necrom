using System;
using Necrom.Core.Domain;
using NUnit.Framework;
namespace Necrom.Core.Tests
{
 public sealed class GachaBatchV10Tests
 {
  private static MonsterGachaManager Create(int seed=7)
  {
   var types=new[]{"Common","Advanced","Rare","Hero","Legendary"};
   var pool=new MonsterEntry[5];
   for(int i=0;i<5;i++)pool[i]=new MonsterEntry{id="M"+i,rarity=types[i],gachaWeight=1};
   return new MonsterGachaManager(new MonsterCatalogData{monsters=pool},new PermanentMonsterRoster(),new Random(seed));
  }
  [Test] public void TwoHundredthDrawMustBeLegendary()
  {
   var g=Create();g.RestorePityCounter(199);
   int payments=0;var result=g.DrawBatch((currency,cost)=>{payments++;return true;},DrawCurrency.Diamond,100,1);
   Assert.That(result[0].Rarity,Is.EqualTo(MonsterRarity.Legendary));
   Assert.That(g.DrawsSinceLegendary,Is.Zero);
   Assert.That(payments,Is.EqualTo(1));
  }
  [Test] public void TenDrawChargesOnceAndTracksDuplicates()
  {
   var g=Create();int total=0,calls=0;
   var results=g.DrawBatch((_,amount)=>{calls++;total+=amount;return true;},DrawCurrency.Diamond,100,10);
   Assert.That(results.Count,Is.EqualTo(10));
   Assert.That(calls,Is.EqualTo(1));
   Assert.That(total,Is.EqualTo(1000));
   Assert.That(results.Count,Is.GreaterThan(g.Roster.OwnedCount));
  }
  [Test] public void PityCanTriggerMidTenDraw()
  {
   var g=Create();g.RestorePityCounter(198);
   var draws=g.DrawBatch((_,__)=>true,DrawCurrency.Diamond,100,10);
   Assert.That(draws[1].Rarity,Is.EqualTo(MonsterRarity.Legendary));
   Assert.That(draws.Count,Is.EqualTo(10));
  }
  [Test] public void InsufficientBalanceIsAtomic()
  {
   var g=Create();var result=g.DrawBatch((_,amount)=>false,DrawCurrency.Diamond,100,10);
   Assert.That(result,Is.Null);
   Assert.That(g.Roster.OwnedCount,Is.Zero);
   Assert.That(g.DrawsSinceLegendary,Is.Zero);
  }
 }
}