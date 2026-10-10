using System.Linq;
using Necrom.Core.Domain;
using NUnit.Framework;
namespace Necrom.Core.Tests {
 public class TftMatrixExpansionTests {
  [Test] public void CanonicalMatrixContainsExactlySevenOriginsSevenClassesFourJokers() {
   var d=TftTaxonomy.Create();
   Assert.That(d.Where(x=>x.category=="Origin").Select(x=>x.id),Is.EquivalentTo(new[]{"Skeleton","Plague","Beast","Phantom","Dragon","Abyss","Elemental"}));
   Assert.That(d.Where(x=>x.category=="Class").Select(x=>x.id),Is.EquivalentTo(new[]{"Guardian","Bruiser","Slayer","Ranger","Mage","Warlock","Supporter"}));
   Assert.That(d.Where(x=>x.category=="Joker").Select(x=>x.id),Is.EquivalentTo(new[]{"Reaper","Mimic","Colossus","Vampire"}));
  }
  [Test] public void ExpandedTraitsRemainExplicitlyUnspecifiedWithoutInventedEffects() {
   var d=TftTaxonomy.Create();
   foreach(var id in new[]{"Abyss","Elemental","Bruiser","Mage","Supporter"}) {
    var t=d.SingleOrDefault(x=>x.id==id);
    Assert.That(t,Is.Not.Null,id);
    Assert.That(t.effectIds,Is.Empty);
    Assert.That(t.thresholds,Is.Empty);
   }
   Assert.That(TftTaxonomy.Validate(d),Is.Empty);
  }
  [Test] public void AuthoringRejectsNonCanonicalTraitAndMisclassifiedId() {
   var d=TftTaxonomy.Create(); d[0].id="Unknown";
   Assert.That(TftTaxonomy.Validate(d),Is.Not.Empty);
   d=TftTaxonomy.Create();d[0].category="Class";
   Assert.That(TftTaxonomy.Validate(d),Is.Not.Empty);
  }
 }
}
