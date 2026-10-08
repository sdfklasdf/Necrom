using System;
using Necrom.Core.Domain;
using NUnit.Framework;
namespace Necrom.Core.Tests {
public class StatCalculatorV4Tests {
 [Test] public void PermanentBuffAppliesWithoutChangingRaise() {
  var cat=new[]{new MonsterEntry{id="A",hp=100,attack=20,traits=new[]{"Spirit"}},new MonsterEntry{id="B",hp=100,attack=20,traits=new[]{"Spirit"}}};
  var synergy=new SynergyManager(new SynergyCatalog{rules=new[]{new SynergyRule{trait="Spirit",effectKey="party.hp.percent",thresholds=new[]{2},values=new[]{.2f}}}});
  var nodes=new SkillTreeCatalog{skills=new[]{new SkillNode{id="s",branch="Summoning",tier=1,maxLevel=5,spPerLevel=1,effectKey="summon.attack.percent"}}};
  var skills=new SkillTreeManager(nodes,2);
  Assert.That(skills.TryLevelUp("s"),Is.True);
  var calc=new StatCalculator(synergy,skills,nodes,cat);
  var result=calc.Calculate("A",new[]{"A","B"});
  Assert.That(result.MaxHealth,Is.EqualTo(120));
  Assert.That(result.Attack,Is.EqualTo(20)); // 1% rounds to nearest int; no runtime mutation
  Assert.Throws<ArgumentException>(()=>calc.Calculate("A",new[]{"B"}));
 }
 [Test] public void UltimateIsContractOnly() {
  Assert.That(UltimateContract.ShadowLegion().DurationSeconds,Is.EqualTo(10));
  Assert.Throws<ArgumentOutOfRangeException>(()=>UltimateContract.FusionOfDead(6));
 }
}
}