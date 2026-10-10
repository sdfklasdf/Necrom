using System;
using System.Reflection;
using System.Linq;
using Necrom.Core.Domain;
using NUnit.Framework;
namespace Necrom.Core.Tests {
 public class LocalizedContentNamesTests {
  static dynamic Resolver(out LocalizationManager manager){
   manager=new LocalizationManager(new LocalizationTableData{defaultLanguage="ko",languages=new[]{"ko","en"},entries=new[]{
    new LocalizationEntry{key="trait.Origin.Skeleton.name",values=new[]{new LocalizationValue{language="ko",text="스켈레톤"},new LocalizationValue{language="en",text="Skeleton"}}},
    new LocalizationEntry{key="monster.MON_TEST.name",values=new[]{new LocalizationValue{language="ko",text="시험"},new LocalizationValue{language="en",text="Test"}}}
   }});
   var t=typeof(MonsterEntry).Assembly.GetType("Necrom.Core.Domain.LocalizedContentNames");
   Assert.That(t,Is.Not.Null,"Schema text resolver missing");return Activator.CreateInstance(t,manager);
  }
  [Test]public void EveryCanonicalTraitExposesStableExternalLocalizationKey(){
   var field=typeof(TftTraitDefinition).GetField("localizationKey");
   Assert.That(field,Is.Not.Null,"Trait localization field missing");
   foreach(var t in TftTaxonomy.Create())Assert.That((string)field.GetValue(t),Is.EqualTo("trait."+t.category+"."+t.id+".name"));
  }
  [Test]public void TraitNameUpdatesAfterLanguageSwitchWithoutChangingStableId(){
   LocalizationManager m;var r=Resolver(out m);var t=TftTaxonomy.Create().Single(x=>x.id=="Skeleton");
   Assert.That((string)r.ResolveTraitName(t),Is.EqualTo("스켈레톤"));m.TrySetLanguage("en");
   Assert.That((string)r.ResolveTraitName(t),Is.EqualTo("Skeleton"));Assert.That(t.id,Is.EqualTo("Skeleton"));
  }
  [Test]public void MonsterKeyTranslationUsesIdAndUntranslatedContentRetainsOriginalName(){
   LocalizationManager m;var r=Resolver(out m);m.TrySetLanguage("en");
   Assert.That((string)r.ResolveMonsterName(new MonsterEntry{id="MON_TEST",name="original"}),Is.EqualTo("Test"));
   Assert.That((string)r.ResolveMonsterName(new MonsterEntry{id="MON_001",name="원래 이름"}),Is.EqualTo("원래 이름"));
  }
  [Test]public void LegacyTraitWithoutNewFieldStillResolvesCanonicalKey(){
   LocalizationManager m;var r=Resolver(out m);
   Assert.That((string)r.ResolveTraitName(new TftTraitDefinition{id="Skeleton",category="Origin",label="Skeleton"}),Is.EqualTo("스켈레톤"));
  }
 }
}
