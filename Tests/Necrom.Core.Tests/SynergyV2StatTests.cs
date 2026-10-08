using Necrom.Core.Domain;
using NUnit.Framework;
namespace Necrom.Core.Tests {
public class SynergyV2StatTests {
 [Test] public void FourStatBuffsApplyToPermanentStats() {
  var rules=new SynergyCatalog{schemaVersion=2,rules=new[]{
   new SynergyRule{trait="Nature",effectKey="origin.nature",thresholds=new[]{2},values=new[]{.2f},statKeys=new[]{"party.hp.percent"}},
   new SynergyRule{trait="Warrior",effectKey="class.warrior",thresholds=new[]{2},values=new[]{.25f},statKeys=new[]{"party.attack.percent"}},
   new SynergyRule{trait="Guardian",effectKey="class.guardian",thresholds=new[]{2},values=new[]{.5f},statKeys=new[]{"party.defense.percent"}},
   new SynergyRule{trait="Beast",effectKey="origin.beast",thresholds=new[]{2},values=new[]{.15f},statKeys=new[]{"party.attack_speed.percent"}}
  }};
  var a=new MonsterEntry{id="a",hp=100,attack=20,traits=new[]{"Nature","Warrior","Guardian","Beast"}};
  var b=new MonsterEntry{id="b",hp=100,attack=20,traits=new[]{"Nature","Warrior","Guardian","Beast"}};
  // Two traits per monster are the normal production rule; this fixture deliberately uses four
  // independent synthetic trait pairs by six distinct units.
  var units=new[]{
   new MonsterEntry{id="a",hp=100,attack=20,traits=new[]{"Nature","Warrior"}},
   new MonsterEntry{id="b",hp=100,attack=20,traits=new[]{"Nature","Warrior"}},
   new MonsterEntry{id="c",hp=100,attack=20,traits=new[]{"Guardian","Beast"}},
   new MonsterEntry{id="d",hp=100,attack=20,traits=new[]{"Guardian","Beast"}}
  };
  var skill=new SkillTreeCatalog{skills=new[]{new SkillNode{id="x",branch="Summoning",tier=1,maxLevel=1,spPerLevel=1,effectKey="DRAFT_X"}}};
  var calc=new StatCalculator(new SynergyManager(rules),new SkillTreeManager(skill),skill,units);
  var s=calc.Calculate("a",new[]{"a","b","c","d"});
  Assert.That(s.MaxHealth,Is.EqualTo(120));
  Assert.That(s.Attack,Is.EqualTo(25));
  Assert.That(s.Defense,Is.EqualTo(5));
  Assert.That(s.AttackSpeedMultiplier,Is.EqualTo(1.15).Within(.001));
 }
}
}