using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Necrom.Core.Domain;
namespace Necrom.Core.Tests {
public class TftDraftTests {
 static Type Type => typeof(MonsterEntry).Assembly.GetType("Necrom.Core.Domain.TftTraitDefinition");
 static object Definitions() {
 var registry=typeof(MonsterEntry).Assembly.GetType("Necrom.Core.Domain.TftTaxonomy");
 Assert.That(registry,Is.Not.Null,"Confirmed TFT taxonomy is missing");
 return registry.GetMethod("Create").Invoke(null,null);
 }
 [Test] public void ProvidesExactlyFiveOriginsFourClassesFourJokers() {
 var a=(Array)Definitions(); Assert.That(a.Length,Is.EqualTo(13));
 foreach(var pair in new[]{("Origin",5),("Class",4),("Joker",4)})
 Assert.That(a.Cast<object>().Count(x=>(string)Type.GetField("category").GetValue(x)==pair.Item1),Is.EqualTo(pair.Item2));
 }
 [Test] public void GhoulIsAliasOfPlagueAndEffectPlansArePreserved() {
 var a=((Array)Definitions()).Cast<object>().ToArray();
 var p=a.Single(x=>(string)Type.GetField("id").GetValue(x)=="Plague");
 Assert.That((string[])Type.GetField("aliases").GetValue(p),Does.Contain("Ghoul"));
 Assert.That((string[])Type.GetField("effectIds").GetValue(p),Is.EquivalentTo(new[]{"poison","self_destruct"}));
 }
 [Test] public void RejectsUnknownCategoryAndDuplicateCanonicalIds() {
 var a=(Array)Definitions();
 var validate=typeof(MonsterEntry).Assembly.GetType("Necrom.Core.Domain.TftTaxonomy").GetMethod("Validate");
 Type.GetField("category").SetValue(a.GetValue(0),"Bogus");
 Assert.That((string[])validate.Invoke(null,new object[]{a}),Is.Not.Empty);
 a=(Array)Definitions();
 Type.GetField("id").SetValue(a.GetValue(1),"Skeleton");
 Assert.That((string[])validate.Invoke(null,new object[]{a}),Is.Not.Empty);
 }
 [Test] public void JokersUseUniqueActivationWithoutInventedTierNumbers() {
 foreach(var x in ((Array)Definitions()).Cast<object>().Where(x=>(string)Type.GetField("category").GetValue(x)=="Joker")) {
 Assert.That((string)Type.GetField("activation").GetValue(x),Is.EqualTo("Unique"));
 Assert.That((int[])Type.GetField("thresholds").GetValue(x),Is.Empty);
 Assert.That((string)Type.GetField("balanceStatus").GetValue(x),Is.EqualTo("DRAFT_NUMBERS_UNDECIDED"));
 }
 }
}}