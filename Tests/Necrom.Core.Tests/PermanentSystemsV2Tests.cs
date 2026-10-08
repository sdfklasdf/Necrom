using System;
using System.Collections.Generic;
using Necrom.Core.Domain;
using NUnit.Framework;

namespace Necrom.Core.Tests
{
    public sealed class PermanentSystemsV2Tests
    {
        [Test] public void PermanentCollectionIsIndependentOfRaiseAndPreventsDuplicateDeckEntries()
        {
            var roster=new PermanentMonsterRoster();
            Assert.That(roster.UnlockedSlots,Is.EqualTo(3));
            Assert.That(roster.Grant("MON_001"),Is.True);
            Assert.That(roster.Grant("MON_001"),Is.False);
            Assert.That(roster.GetFragments("MON_001"),Is.EqualTo(1));
            Assert.That(roster.Assign(0,"MON_001"),Is.True);
            Assert.That(roster.Assign(1,"MON_001"),Is.False);
            Assert.That(roster.Assign(3,"MON_001"),Is.False);
            var raises=new TemporaryRaiseRegistry();
            for(int i=0;i<5;i++)raises.Register("RAISE:"+i,10+i);
            Assert.That(raises.Register("RAISE:5",100),Is.EqualTo("RAISE:0"));
            Assert.That(raises.Count,Is.EqualTo(5));
            raises.ResetWave();
            Assert.That(raises.Count,Is.Zero);
            Assert.That(roster.Owns("MON_001"),Is.True);
        }
        [Test] public void PityGuaranteesLegendaryAt200AndDiamondOnly()
        {
            var catalog=new MonsterCatalogData { monsters=new MonsterEntry[5] };
            var tiers=new[]{"Common","Advanced","Rare","Hero","Legendary"};
            for(int i=0;i<5;i++)catalog.monsters[i]=new MonsterEntry {id="MON_00"+i,rarity=tiers[i],gachaWeight=1};
            var manager=new MonsterGachaManager(catalog,new PermanentMonsterRoster(),new Random(4));
            Assert.Throws<InvalidOperationException>(()=>manager.DrawDetailed((_,__)=>true,DrawCurrency.Gold,1));
            manager.RestorePityCounter(199);
            var result=manager.DrawDetailed((_,__)=>true,DrawCurrency.Diamond,1);
            Assert.That(result.Rarity,Is.EqualTo(MonsterRarity.Legendary));
            Assert.That(manager.DrawsSinceLegendary,Is.Zero);
        }
        [Test] public void SevenSlotsAndRecipeConsumeFive()
        {
            var gear=new NecromancerEquipmentManager();
            Assert.That(Enum.GetValues(typeof(EquipmentSlot)).Length,Is.EqualTo(7));
            var item=new EquipmentItem{id="a",slot=EquipmentSlot.Gloves,rarity=EquipmentRarity.Common,hpBonus=1};
            for(int i=0;i<5;i++)gear.Unlock(item);
            Assert.That(gear.TrySynthesize("a",new EquipmentItem{id="b",slot=EquipmentSlot.Gloves,rarity=EquipmentRarity.Advanced,hpBonus=3}),Is.True);
            Assert.That(gear.GetQuantity("a"),Is.Zero);
            Assert.That(gear.GetQuantity("b"),Is.EqualTo(1));
        }
    }
}
