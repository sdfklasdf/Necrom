using System;
using System.Collections.Generic;
using System.Linq;
namespace Necrom.Core.Domain {
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
 var plague=D("Plague","Origin","poison","self_destruct"); plague.label="Ghoul/Plague";plague.aliases=new[]{"Ghoul"};
 return new[]{
 D("Skeleton","Origin","attack_speed","resurrection"),plague,
 D("Phantom","Origin","physical_evasion"),D("Beast","Origin","early_rage"),D("Dragon","Origin","area_breath"),
 D("Guardian","Class","shield"),D("Slayer","Class","assassinate","execute"),D("Warlock","Class","debuff"),D("Ranger","Class","ranged_penetration"),
 D("Reaper","Joker","execute"),D("Mimic","Joker","currency_farming"),D("Colossus","Joker","super_tank"),D("Vampire","Joker","lifesteal")};
 }
 public static string[] Validate(TftTraitDefinition[] definitions) {
 var e=new List<string>();var ids=new HashSet<string>(StringComparer.Ordinal);
 if(definitions==null || definitions.Length==0) return new[]{"TFT definitions required."};
 foreach(var d in definitions) {
 if(d==null){e.Add("Null TFT definition.");continue;}
 if(string.IsNullOrWhiteSpace(d.id)||!ids.Add(d.id))e.Add("Missing/duplicate TFT ID.");
 if(d.category!="Origin"&&d.category!="Class"&&d.category!="Joker")e.Add("Unknown TFT category: "+d.id);
 if(d.activation!=(d.category=="Joker"?"Unique":"CountTiers"))e.Add("Invalid activation: "+d.id);
 if(string.IsNullOrWhiteSpace(d.label)||d.effectIds==null||d.effectIds.Length==0||d.effectIds.Any(string.IsNullOrWhiteSpace)||d.effectIds.Distinct().Count()!=d.effectIds.Length)e.Add("Effect plan/label required: "+d.id);
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