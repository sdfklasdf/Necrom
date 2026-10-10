using System;
using System.Collections.Generic;
using System.Linq;
namespace Necrom.Core.Domain {
 // Explicit values for future typed consumers. Serialized legacy string IDs are retained.
 public enum TftOrigin { Skeleton=0, Plague=1, Beast=2, Phantom=3, Dragon=4, Abyss=5, Elemental=6 }
 public enum TftClass { Guardian=0, Bruiser=1, Slayer=2, Ranger=3, Mage=4, Warlock=5, Supporter=6 }
 public enum TftJoker { Reaper=0, Mimic=1, Colossus=2, Vampire=3 }
 [Serializable] public sealed class TftTraitDefinition {
 public string id, label, category, activation;
 public string[] aliases=Array.Empty<string>();
 public string[] effectIds=Array.Empty<string>();
 public int[] thresholds=Array.Empty<int>();
 public string balanceStatus="DRAFT_NUMBERS_UNDECIDED";
 }
 public static class TftTaxonomy {
 static TftTraitDefinition D(string id,string category,params string[] effects) =>
 new TftTraitDefinition {id=id,label=id,category=category,activation=category=="Joker"?"Unique":"CountTiers",effectIds=effects};
 public static TftTraitDefinition[] Create() {
 var plague=D("Plague","Origin","poison","self_destruct"); plague.aliases=new[]{"Ghoul"};
 return new[]{
 D("Skeleton","Origin","attack_speed","resurrection"),plague,
 D("Beast","Origin","early_rage"),D("Phantom","Origin","physical_evasion"),D("Dragon","Origin","area_breath"),
 D("Abyss","Origin"),D("Elemental","Origin"),
 D("Guardian","Class","shield"),D("Bruiser","Class"),D("Slayer","Class","assassinate","execute"),
 D("Ranger","Class","ranged_penetration"),D("Mage","Class"),D("Warlock","Class","debuff"),D("Supporter","Class"),
 D("Reaper","Joker","execute"),D("Mimic","Joker","currency_farming"),D("Colossus","Joker","super_tank"),D("Vampire","Joker","lifesteal")};
 }
 public static bool IsCanonical(string category,string id) {
 var type=category=="Origin"?typeof(TftOrigin):category=="Class"?typeof(TftClass):category=="Joker"?typeof(TftJoker):null;
 return type!=null && id!=null && Enum.GetNames(type).Contains(id);
 }
 public static string[] Validate(TftTraitDefinition[] definitions) {
 var e=new List<string>();var ids=new HashSet<string>(StringComparer.Ordinal);
 if(definitions==null || definitions.Length==0) return new[]{"TFT definitions required."};
 // A legacy subset may remain readable; Create supplies the complete current matrix.
 foreach(var d in definitions) {
 if(d==null){e.Add("Null TFT definition.");continue;}
 if(string.IsNullOrWhiteSpace(d.id)||!ids.Add(d.id))e.Add("Missing/duplicate TFT ID.");
 if(!IsCanonical(d.category,d.id))e.Add("Unknown or misclassified TFT ID: "+d.id);
 if(d.activation!=(d.category=="Joker"?"Unique":"CountTiers"))e.Add("Invalid activation: "+d.id);
 // Empty effectIds explicitly means unspecified. No effect or balance is inferred.
 if(string.IsNullOrWhiteSpace(d.label)||d.effectIds==null||d.effectIds.Any(string.IsNullOrWhiteSpace)||d.effectIds.Distinct().Count()!=d.effectIds.Length)e.Add("Invalid effect plan/label: "+d.id);
 if(d.balanceStatus!="DRAFT_NUMBERS_UNDECIDED")e.Add("Unapproved TFT numeric status: "+d.id);
 if(d.thresholds==null||d.thresholds.Length!=0)e.Add("TFT numeric tiers not approved: "+d.id);
 }
 var names=new HashSet<string>(ids,StringComparer.Ordinal);
 foreach(var d in definitions.Where(x=>x!=null)) {
 if(d.aliases==null){e.Add("Aliases array required.");continue;}
 foreach(var a in d.aliases)if(string.IsNullOrWhiteSpace(a)||!names.Add(a))e.Add("Ambiguous TFT alias: "+a);
 }
 return e.ToArray();
 }
 }
}
