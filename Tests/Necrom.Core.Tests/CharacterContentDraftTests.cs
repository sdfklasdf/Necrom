using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using Necrom.Core.Domain;
using NUnit.Framework;

namespace Necrom.Core.Tests
{
    public sealed class CharacterContentDraftTests
    {
        static readonly JsonSerializerOptions Options = new JsonSerializerOptions { IncludeFields=true };
        static JsonObject Fixture()
        {
            return JsonSerializer.SerializeToNode(new {
                schemaVersion=1, expectedCharacterCount=120, contentRevision="TEST_ONLY", balanceStatus="DRAFT_FIXTURE",
                affinityIds=new[]{"neutral","fire"}, traitIds=new[]{"Guardian","Spirit"},
                monsters=new {schemaVersion=3,balanceStatus="DRAFT_FIXTURE", monsters=Enumerable.Range(1,120).Select(i=>new {
                    id="MON_"+i.ToString("D3"), name="fixture", element="neutral", hp=100, attack=10, gachaWeight=1,
                    rarity="Common", traits=new[]{"Guardian"}
                }).ToArray()},
                synergies=new {schemaVersion=2, rules=new[]{new {trait="Guardian",effectKey="party.hp.percent",thresholds=new[]{2,4,6},values=new[]{.05f,.1f,.2f},statKeys=new[]{"party.hp.percent","party.hp.percent","party.hp.percent"}}}},
                profiles=Enumerable.Range(1,120).Select(i=>new {characterId="MON_"+i.ToString("D3"),assetKey="",assetStatus="UNASSIGNED",
                    familyId="",affinityId="neutral",defense=0,attacksPerSecond=1f}).ToArray(),
                matchups=new[]{new {attackerAffinityId="fire",defenderAffinityId="neutral",damageMultiplier=1f}}
            },Options).AsObject();
        }
        static string[] Errors(JsonObject fixture)
        {
            var type=typeof(MonsterEntry).Assembly.GetType("Necrom.Core.Domain.CharacterContentDraft");
            Assert.That(type,Is.Not.Null,"120-character content schema/validator is missing");
            var value=JsonSerializer.Deserialize(fixture.ToJsonString(),type,Options);
            return (string[])type.GetMethod("Validate").Invoke(value,null);
        }
        [Test] public void Accepts120CharactersWithReferencedTraitsAndUnassignedAssets() => Assert.That(Errors(Fixture()),Is.Empty);
        [Test] public void RejectsDuplicateCharacterIds() {var f=Fixture();f["monsters"]["monsters"][1]["id"]="MON_001";Assert.That(Errors(f),Is.Not.Empty);}
        [Test] public void RejectsWrongCharacterCount() {var f=Fixture();f["expectedCharacterCount"]=121;Assert.That(Errors(f),Is.Not.Empty);}
        [Test] public void RejectsUnknownTraitReference() {var f=Fixture();f["monsters"]["monsters"][0]["traits"][0]="missing";Assert.That(Errors(f),Is.Not.Empty);}
        [Test] public void RejectsMoreThanThreeTraits() {var f=Fixture();f["monsters"]["monsters"][0]["traits"]=JsonSerializer.SerializeToNode(new[]{"Guardian","Spirit","X","Y"});Assert.That(Errors(f),Is.Not.Empty);}
        [Test] public void RejectsNegativeBaseStats() {var f=Fixture();f["monsters"]["monsters"][0]["hp"]=-1;Assert.That(Errors(f),Is.Not.Empty);}
        [Test] public void RejectsUnknownProfileCharacter() {var f=Fixture();f["profiles"][0]["characterId"]="missing";Assert.That(Errors(f),Is.Not.Empty);}
        [Test] public void RejectsDuplicateProfile() {var f=Fixture();f["profiles"][1]["characterId"]="MON_001";Assert.That(Errors(f),Is.Not.Empty);}
        [Test] public void RejectsReadyAssetWithoutKey() {var f=Fixture();f["profiles"][0]["assetStatus"]="MAPPED";Assert.That(Errors(f),Is.Not.Empty);}
        [Test] public void RejectsNonPositiveAttackSpeed() {var f=Fixture();f["profiles"][0]["attacksPerSecond"]=0;Assert.That(Errors(f),Is.Not.Empty);}
        [Test] public void RejectsUnknownAffinityReference() {var f=Fixture();f["matchups"][0]["attackerAffinityId"]="missing";Assert.That(Errors(f),Is.Not.Empty);}
        [Test] public void RejectsNegativeMatchupMultiplier() {var f=Fixture();f["matchups"][0]["damageMultiplier"]=-1;Assert.That(Errors(f),Is.Not.Empty);}
        [Test] public void RejectsDuplicateDirectedMatchup() {var f=Fixture();f["matchups"].AsArray().Add(f["matchups"][0].DeepClone());Assert.That(Errors(f),Is.Not.Empty);}
        [Test] public void RejectsNonIncreasingSynergyThresholds() {var f=Fixture();f["synergies"]["rules"][0]["thresholds"][1]=2;Assert.That(Errors(f),Is.Not.Empty);}
        [Test] public void RejectsUnknownSchemaVersion() {var f=Fixture();f["schemaVersion"]=99;Assert.That(Errors(f),Is.Not.Empty);}
    }
}
