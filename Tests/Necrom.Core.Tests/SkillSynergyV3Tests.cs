using System;
using System.Linq;
using Necrom.Core.Domain;
using NUnit.Framework;
namespace Necrom.Core.Tests {
 public sealed class SkillSynergyV3Tests {
  [Test] public void SkillTreeRequiresMasteredPrerequisiteAndSp() {
   var tree=new SkillTreeManager(new SkillTreeCatalog {skills=new[]{
    new SkillNode{id="a",branch="Destruction",tier=1,maxLevel=2,spPerLevel=1,effectKey="x"},
    new SkillNode{id="b",branch="Destruction",tier=2,maxLevel=1,spPerLevel=2,effectKey="y",prerequisiteId="a"}
   }},4);
   Assert.That(tree.TryLevelUp("b"),Is.False);
   Assert.That(tree.TryLevelUp("a"),Is.True);
   Assert.That(tree.TryLevelUp("b"),Is.False);
   Assert.That(tree.TryLevelUp("a"),Is.True);
   Assert.That(tree.TryLevelUp("b"),Is.True);
   Assert.That(tree.AvailableSp,Is.Zero);
  }
  [Test] public void SynergyCountsDistinctMonstersAndTiers() {
   var manager=new SynergyManager(new SynergyCatalog{rules=new[]{new SynergyRule{
    trait="Beast",effectKey="party",thresholds=new[]{2,4,6},values=new[]{.05f,.1f,.2f}}}});
   var units=Enumerable.Range(0,4).Select(i=>new MonsterEntry{id="m"+i,traits=new[]{"Beast"}}).ToArray();
   Assert.That(manager.Evaluate(units).Single().Threshold,Is.EqualTo(4));
   Assert.Throws<ArgumentException>(()=>manager.Evaluate(new[]{units[0],units[0]}));
  }
  [Test] public void RecommendNeverUsesUnownedOrLockedSlot() {
   var roster=new PermanentMonsterRoster();
   roster.Grant("m1");roster.Grant("m2");roster.Grant("m3");
   var pool=Enumerable.Range(1,4).Select(i=>new MonsterEntry{id="m"+i,hp=i*10,attack=i,traits=new[]{"Beast"}}).ToArray();
   var manager=new AutoDeckRecommender(new SynergyManager(new SynergyCatalog{rules=new[]{new SynergyRule{
    trait="Beast",effectKey="party",thresholds=new[]{2},values=new[]{.05f}}}}));
   var plan=manager.Recommend(roster,pool,AutoDeckGoal.CombatPower);
   Assert.That(plan.MonsterIds.Count,Is.EqualTo(3));
   Assert.That(plan.MonsterIds.Contains("m4"),Is.False);
   Assert.That(manager.Apply(roster,plan),Is.True);
  }
 }
}