using System;
using System.Reflection;
using System.Text.Json;
using NUnit.Framework;
using Necrom.Core.Domain;
namespace Necrom.Core.Tests {
 public class LocalizationManagerTests {
  static Type TypeOf(string name)=>typeof(MonsterEntry).Assembly.GetType("Necrom.Core.Domain."+name);
  static readonly JsonSerializerOptions Json=new JsonSerializerOptions{IncludeFields=true};
  static string Table="{\"schemaVersion\":1,\"defaultLanguage\":\"ko\",\"languages\":[\"ko\",\"en\"],\"entries\":[{\"key\":\"ui.summon\",\"values\":[{\"language\":\"ko\",\"text\":\"소환\"},{\"language\":\"en\",\"text\":\"Summon\"}]},{\"key\":\"fallback\",\"values\":[{\"language\":\"ko\",\"text\":\"기본\"}]},{\"key\":\"format\",\"values\":[{\"language\":\"ko\",\"text\":\"레벨 {0} · 경험치 {1:F1}%\"},{\"language\":\"en\",\"text\":\"Lv. {0} · EXP {1:F1}%\"}]}]}";
  static dynamic Create(string json=null) {
   Assert.That(TypeOf("LocalizationManager"),Is.Not.Null,"Localization manager missing");
   var data=JsonSerializer.Deserialize(json??Table,TypeOf("LocalizationTableData"),Json);
   return Activator.CreateInstance(TypeOf("LocalizationManager"),data);
  }
  [Test]public void ChangesResolvedTextImmediatelyAndSupportsRegionalTags(){
   var m=Create();Assert.That((string)m.Get("ui.summon"),Is.EqualTo("소환"));
   Assert.That((bool)m.TrySetLanguage("EN-us"),Is.True);Assert.That((string)m.Language,Is.EqualTo("en"));
   Assert.That((string)m.Get("ui.summon"),Is.EqualTo("Summon"));
   m.TrySetLanguage("ko-KR");Assert.That((string)m.Get("ui.summon"),Is.EqualTo("소환"));
  }
  [Test]public void MissingTranslationFallsBackAndMissingKeyIsVisible(){
   var m=Create();m.TrySetLanguage("en");
   Assert.That((string)m.Get("fallback"),Is.EqualTo("기본"));Assert.That((string)m.Get("missing"),Is.EqualTo("[missing]"));
  }
  [Test]public void UnsupportedLanguageDoesNotChangeCurrentSelection(){
   var m=Create();m.TrySetLanguage("en");Assert.That((bool)m.TrySetLanguage("fr"),Is.False);
   Assert.That((string)m.Language,Is.EqualTo("en"));
  }
  [Test]public void SameLanguageDoesNotPublishRedundantChange(){
   var m=Create();int count=0;Action change=()=>count++;
   m.LanguageChanged+=change;m.TrySetLanguage("ko");m.TrySetLanguage("en");m.TrySetLanguage("EN");
   Assert.That(count,Is.EqualTo(1));m.LanguageChanged-=change;
  }
  [Test]public void FormattingUsesSelectedLanguageAndKeepsDynamicValues(){
   var m=Create();m.TrySetLanguage("en");
   Assert.That((string)m.Format("format",new object[]{24,12.5}),Is.EqualTo("Lv. 24 · EXP 12.5%"));
   m.TrySetLanguage("ko");Assert.That((string)m.Format("format",new object[]{24,12.5}),Does.StartWith("레벨 24"));
  }
  [TestCase("schemaVersion",99)]
  [TestCase("defaultLanguage","fr")]
  public void InvalidTableFailsBeforeUse(string field,object value){
   var node=System.Text.Json.Nodes.JsonNode.Parse(Table);node[field]=JsonSerializer.SerializeToNode(value);
   var e=Assert.Throws<TargetInvocationException>(()=>Create(node.ToJsonString()));
   Assert.That(e.InnerException,Is.InstanceOf<ArgumentException>());
  }
  [Test]public void DuplicateKeyAndMissingDefaultTranslationAreRejected(){
   var node=System.Text.Json.Nodes.JsonNode.Parse(Table);node["entries"][1]["key"]="ui.summon";
   Assert.Throws<TargetInvocationException>(()=>Create(node.ToJsonString()));
   node=System.Text.Json.Nodes.JsonNode.Parse(Table);node["entries"][1]["values"][0]["language"]="en";
   Assert.Throws<TargetInvocationException>(()=>Create(node.ToJsonString()));
  }
  [Test]public void DuplicateLanguageEntryIsRejected(){
   var node=System.Text.Json.Nodes.JsonNode.Parse(Table);node["entries"][0]["values"][1]["language"]="ko";
   Assert.Throws<TargetInvocationException>(()=>Create(node.ToJsonString()));
  }
 }
}
