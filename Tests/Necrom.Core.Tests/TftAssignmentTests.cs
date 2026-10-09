using System;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using NUnit.Framework;
using Necrom.Core.Domain;
namespace Necrom.Core.Tests {
 public class TftAssignmentTests {
 static readonly JsonSerializerOptions Options=new JsonSerializerOptions{IncludeFields=true};
 static JsonObject Fixture(){
 return JsonSerializer.SerializeToNode(new CharacterContentDraft {
 contentRevision="DUMMY_TEST",expectedCharacterCount=3,balanceStatus="DRAFT_DUMMY",
 tftDefinitions=TftTaxonomy.Create(),traitIds=TftTaxonomy.Create().Select(x=>x.id).ToArray(),affinityIds=new[]{"Neutral"},
 monsters=new MonsterCatalogData{monsters=Enumerable.Range(1,3).Select(i=>new MonsterEntry{id="MON_"+i.ToString("D3"),name="dummy",element="Neutral",hp=100,attack=10,gachaWeight=1,rarity="Common",traits=new[]{"Skeleton","Guardian","Reaper"}}).ToArray()},
 synergies=new SynergyCatalog{rules=Array.Empty<SynergyRule>()},
 profiles=Enumerable.Range(1,3).Select(i=>new CharacterContentProfile{characterId="MON_"+i.ToString("D3"),affinityId="Neutral",assetStatus="MAPPED",assetKey="dummy/"+i}).ToArray()
 },Options).AsObject();
 }
 static string[] Errors(JsonObject f){
 f["enforceTftAssignments"]=true;
 return JsonSerializer.Deserialize<CharacterContentDraft>(f.ToJsonString(),Options).Validate();
 }
 [Test]public void AcceptsOriginClassAndOptionalJoker(){Assert.That(Errors(Fixture()),Is.Empty);}
 [Test]public void RejectsTwoOrigins(){var f=Fixture();f["monsters"]["monsters"][0]["traits"]=JsonSerializer.SerializeToNode(new[]{"Skeleton","Beast","Guardian"});Assert.That(Errors(f),Is.Not.Empty);}
 [Test]public void RejectsMissingClass(){var f=Fixture();f["monsters"]["monsters"][0]["traits"]=JsonSerializer.SerializeToNode(new[]{"Skeleton","Reaper"});Assert.That(Errors(f),Is.Not.Empty);}
 [Test]public void RejectsAliasInsteadOfCanonicalId(){var f=Fixture();f["monsters"]["monsters"][0]["traits"][0]="Ghoul";f["traitIds"].AsArray().Add("Ghoul");Assert.That(Errors(f),Is.Not.Empty);}
 }
}