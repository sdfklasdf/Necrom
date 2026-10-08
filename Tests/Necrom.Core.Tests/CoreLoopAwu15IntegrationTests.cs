using System;
using System.Linq;
using Necrom.Core.Domain;
using NUnit.Framework;

namespace Necrom.Core.Tests
{
    // AWU-15 bounded integration: purchase -> roster/deck -> skills -> effective attack.
    // Unity's actual damage pipeline and endless-wave runtime require PlayMode verification.
    public sealed class CoreLoopAwu15IntegrationTests
    {
        [Test]
        public void AcquiredMonsterGainsDamageAfterSavedSkillRestoration()
        {
            var monster = new MonsterEntry { id="MON_TEST",name="Test",hp=500,attack=100,
                rarity="Common",gachaWeight=100,traits=new[]{"Spirit","Warrior"} };
            var catalog = new MonsterCatalogData { monsters = new[] { monster,
                new MonsterEntry{id="ADV",rarity="Advanced",hp=500,attack=100,gachaWeight=100,traits=new[]{"Spirit"}},
                new MonsterEntry{id="RARE",rarity="Rare",hp=500,attack=100,gachaWeight=100,traits=new[]{"Spirit"}},
                new MonsterEntry{id="HERO",rarity="Hero",hp=500,attack=100,gachaWeight=100,traits=new[]{"Spirit"}},
                new MonsterEntry{id="LEG",rarity="Legendary",hp=500,attack=100,gachaWeight=100,traits=new[]{"Spirit"}} } };
            var roster = new PermanentMonsterRoster();
            var skillsData = new SkillTreeCatalog { skills = new[] {
                new SkillNode { id="SUMMONING_01",branch="Summoning",tier=1,
                    maxLevel=5,spPerLevel=1,effectKey="summon.attack.percent" },
                new SkillNode { id="SUMMONING_02",branch="Summoning",tier=2,
                    maxLevel=5,spPerLevel=1,effectKey="DRAFT_SUMMONING_2",prerequisiteId="SUMMONING_01" }
            }};
            var synergy = new SynergyManager(new SynergyCatalog { rules = Array.Empty<SynergyRule>() });
            var tree = new SkillTreeManager(skillsData,100);
            var calc = new StatCalculator(synergy,tree,skillsData,catalog.monsters);
            // Permanent roster ownership is acquired through the real domain API.
            Assert.That(roster.Owns(monster.id),Is.False);
            var gacha = new MonsterGachaManager(catalog,roster,new Random(2));
            int spent=0;
            var reward = gacha.DrawDetailed((currency,cost)=> {spent+=cost;return true;},DrawCurrency.Diamond,100);
            Assert.That(reward,Is.Not.Null);
            Assert.That(roster.Owns(reward.Monster.id),Is.True);
            Assert.That(roster.Assign(0,reward.Monster.id),Is.True);
            var deck=new[]{reward.Monster.id};
            var baseline=calc.Calculate(reward.Monster.id,deck);
            Assert.That(baseline.Attack,Is.EqualTo(100));
            for(int i=0;i<5;i++)Assert.That(tree.TryLevelUp("SUMMONING_01"),Is.True);
            Assert.That(tree.TryLevelUp("SUMMONING_02"),Is.True);
            Assert.That(tree.AvailableSp,Is.EqualTo(94));
            var powered=calc.Calculate(reward.Monster.id,deck);
            Assert.That(powered.Attack,Is.EqualTo(105));
            // Reconstructed manager simulates persisted level/SP readback.
            var restored=new SkillTreeManager(skillsData);
            restored.RestoreState(tree.AvailableSp,tree.SnapshotLevels());
            var afterReload=new StatCalculator(synergy,restored,skillsData,catalog.monsters)
                .Calculate(reward.Monster.id,deck);
            Assert.That(afterReload.Attack,Is.EqualTo(powered.Attack));
        }

        [Test]
        public void DraftEffectsDoNotInventGameplayBalance()
        {
            var m=new MonsterEntry{id="M",hp=200,attack=100,traits=new[]{"Spirit"}};
            var data=new SkillTreeCatalog{skills=new[]{
                new SkillNode{id="DESTRUCTION_01",branch="Destruction",tier=1,maxLevel=5,spPerLevel=1,effectKey="DRAFT_DESTRUCTION_1"}
            }};
            var manager=new SkillTreeManager(data,10);
            Assert.That(manager.TryLevelUp("DESTRUCTION_01"),Is.True);
            var calc=new StatCalculator(
                new SynergyManager(new SynergyCatalog{rules=Array.Empty<SynergyRule>()}),
                manager,data,new[]{m});
            Assert.That(calc.Calculate("M",new[]{"M"}).Attack,Is.EqualTo(100));
        }
    }
}
