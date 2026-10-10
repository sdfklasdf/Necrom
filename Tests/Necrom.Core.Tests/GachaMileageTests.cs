using System;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Necrom.Core.Domain;
using NUnit.Framework;
namespace Necrom.Core.Tests {
 public class GachaMileageTests {
  static readonly JsonSerializerOptions Json=new JsonSerializerOptions{IncludeFields=true};
  static Type Mode=>typeof(MonsterEntry).Assembly.GetType("Necrom.Core.Domain.GachaGuaranteeMode");
  static MonsterCatalogData Catalog() {
   return new MonsterCatalogData{monsters=new[]{
    new MonsterEntry{id="C",rarity="Common",gachaWeight=1,traits=new[]{"Skeleton","Guardian"}},
    new MonsterEntry{id="A",rarity="Advanced",gachaWeight=1},new MonsterEntry{id="R",rarity="Rare",gachaWeight=1},
    new MonsterEntry{id="H",rarity="Hero",gachaWeight=1},
    new MonsterEntry{id="L1",rarity="Legendary",gachaWeight=1,traits=new[]{"Skeleton","Guardian","Reaper"}},
    new MonsterEntry{id="L2",rarity="Legendary",gachaWeight=1,traits=new[]{"Dragon","Mage"}}}};
  }
  sealed class Roll:Random {readonly int roll;public Roll(int roll){this.roll=roll;} public override int Next(int maxValue)=>roll;public override double NextDouble()=>0;}
  static dynamic G(int roll=0) {
   Assert.That(Mode,Is.Not.Null,"Selection mileage guarantee mode missing");
   return Activator.CreateInstance(typeof(MonsterGachaManager),Catalog(),new PermanentMonsterRoster(),new Roll(roll),Enum.Parse(Mode,"SelectionMileage"));
  }
  static dynamic Selection(string category,string id) {
   var t=typeof(MonsterEntry).Assembly.GetType("Necrom.Core.Domain.MileageSelection");
   Assert.That(t,Is.Not.Null);return Activator.CreateInstance(t,category,id);
  }
  static void Draw(dynamic g,int count=1,bool pay=true) {
   Func<DrawCurrency,int,bool> spend=(_,__)=>pay;
   g.DrawBatch(spend,DrawCurrency.Diamond,100,count);
  }
  static void Restore(dynamic g,long draws,long redeemed=0,int version=1) {
   var t=typeof(MonsterEntry).Assembly.GetType("Necrom.Core.Domain.GachaMileageState");
   Assert.That(t,Is.Not.Null);
   var state=JsonSerializer.Deserialize("{\"schemaVersion\":"+version+",\"successfulDraws\":"+draws+",\"redeemedTickets\":"+redeemed+"}",t,Json);
   g.RestoreMileageState((dynamic)state);
  }
  [Test]public void TwoHundredSuccessfulDrawsYieldTicketWithoutForcedRandomLegendary(){
   var g=G();for(int i=0;i<199;i++)Draw(g);
   Assert.That((long)g.AvailableSelectionTickets,Is.Zero);
   Draw(g);Assert.That((long)g.AvailableSelectionTickets,Is.EqualTo(1));Assert.That((int)g.MileageProgress,Is.Zero);
   Assert.That(g.Roster.Owns("L1"),Is.False);
  }
  [Test]public void RandomLegendaryDoesNotEraseMileage(){
   var g=G(9800);Restore(g,199);Draw(g);
   Assert.That(g.Roster.Owns("L1"),Is.True);Assert.That((long)g.AvailableSelectionTickets,Is.EqualTo(1));
  }
  [Test]public void TenDrawCrossesBoundaryAndChargesOnce(){
   var g=G();Restore(g,198);int calls=0,paid=0;
   Func<DrawCurrency,int,bool> spend=(_,amount)=>{calls++;paid+=amount;return true;};
   g.DrawBatch(spend,DrawCurrency.Diamond,100,10);
   Assert.That(calls,Is.EqualTo(1));Assert.That(paid,Is.EqualTo(1000));
   Assert.That((long)g.AvailableSelectionTickets,Is.EqualTo(1));Assert.That((int)g.MileageProgress,Is.EqualTo(8));
  }
  [Test]public void FailedPaymentDoesNotAdvanceDrawsOrTickets(){
   var g=G();Restore(g,199);Draw(g,10,false);
   Assert.That((long)g.AvailableSelectionTickets,Is.Zero);Assert.That((int)g.MileageProgress,Is.EqualTo(199));Assert.That((int)g.Roster.OwnedCount,Is.Zero);
  }
  [Test]public void OriginChoiceGrantsExactMatchingLegendaryAndConsumesOnce(){
   var g=G();Restore(g,200);
   var r=g.RedeemMileage(Selection("Origin","Skeleton"),"L1");
   Assert.That((string)r.Monster.id,Is.EqualTo("L1"));Assert.That(g.Roster.Owns("L1"),Is.True);
   Assert.That((long)g.AvailableSelectionTickets,Is.Zero);
   Assert.That((object)g.RedeemMileage(Selection("Origin","Skeleton"),"L1"),Is.Null);
  }
  [Test]public void ClassChoiceReturnsOnlyMatchingLegendaryIds(){
   var g=G();var ids=(System.Collections.Generic.IReadOnlyList<string>)g.GetMileageCandidateIds(Selection("Class","Mage"));
   Assert.That(ids,Is.EquivalentTo(new[]{"L2"}));
   Restore(g,200);Assert.That((string)g.RedeemMileage(Selection("Class","Mage"),"L2").Monster.id,Is.EqualTo("L2"));
  }
  [Test]public void EmptyOrMismatchedChoicesLeaveTicketAndRosterUntouched(){
   var g=G();Restore(g,200);
   Assert.That((object)g.RedeemMileage(Selection("Origin","Abyss"),"L1"),Is.Null);
   Assert.That((object)g.RedeemMileage(Selection("Class","Mage"),"L1"),Is.Null);
   Assert.That((object)g.RedeemMileage(Selection("Origin","Skeleton"),"C"),Is.Null);
   Assert.That((long)g.AvailableSelectionTickets,Is.EqualTo(1));Assert.That((int)g.Roster.OwnedCount,Is.Zero);
  }
  [Test]public void JokerAliasesAndUnknownCategoryCannotBeSelectionFilters(){
   G();
   foreach(var pair in new[]{("Joker","Reaper"),("Origin","Ghoul"),("Origin","Unknown"),("class","Mage")}) {
    var e=Assert.Throws<TargetInvocationException>(()=>{Selection(pair.Item1,pair.Item2);});
    Assert.That(e.InnerException,Is.InstanceOf<ArgumentException>());
   }
  }
  [Test]public void SnapshotRoundtripRetainsUnspentTicketAndIsDefensive(){
   var g=G();Restore(g,450,1);
   var state=g.CaptureMileageState();var t=state.GetType();
   var text=JsonSerializer.Serialize((object)state,(Type)t,Json);
   state.successfulDraws=0L;
   Assert.That((long)g.AvailableSelectionTickets,Is.EqualTo(1));Assert.That((int)g.MileageProgress,Is.EqualTo(50));
   var h=G();h.RestoreMileageState((dynamic)JsonSerializer.Deserialize(text,(Type)t,Json));
   Assert.That((long)h.AvailableSelectionTickets,Is.EqualTo(1));Assert.That((int)h.MileageProgress,Is.EqualTo(50));
  }
  [Test]public void InvalidSnapshotIsRejectedWithoutOverwritingValidState(){
   var g=G();Restore(g,200);
   Assert.Throws<ArgumentException>(()=>Restore(g,-1));
   Assert.Throws<ArgumentException>(()=>Restore(g,200,2));
   Assert.Throws<ArgumentException>(()=>Restore(g,200,0,99));
   Assert.That((long)g.AvailableSelectionTickets,Is.EqualTo(1));
  }
  [Test]public void OverflowRejectedBeforeSpending(){
   var g=G();Restore(g,long.MaxValue);int calls=0;
   Func<DrawCurrency,int,bool> spend=(_,__)=>{calls++;return true;};
   Assert.Throws<OverflowException>(()=>g.DrawBatch(spend,DrawCurrency.Diamond,100,10));Assert.That(calls,Is.Zero);
  }
  [Test]public void DuplicateSelectionGrantsRosterFragmentWithoutDrawOrMileageProgress(){
   var g=G();Restore(g,400);g.RedeemMileage(Selection("Origin","Skeleton"),"L1");
   var r=g.RedeemMileage(Selection("Class","Guardian"),"L1");
   Assert.That((bool)r.FirstAcquisition,Is.False);Assert.That((int)g.Roster.GetFragments("L1"),Is.EqualTo(1));
   Assert.That((long)g.AvailableSelectionTickets,Is.Zero);Assert.That((int)g.MileageProgress,Is.Zero);
   Assert.That((long)g.CaptureMileageState().successfulDraws,Is.EqualTo(400));
  }
  [Test]public void CatalogMutationDoesNotChangeSelectionEligibility(){
   Assert.That(Mode,Is.Not.Null);var data=Catalog();
   dynamic g=Activator.CreateInstance(typeof(MonsterGachaManager),data,new PermanentMonsterRoster(),new Roll(0),Enum.Parse(Mode,"SelectionMileage"));
   data.monsters[4].traits[0]="Abyss";data.monsters[4].id="changed";
   Restore(g,200);Assert.That((string)g.RedeemMileage(Selection("Origin","Skeleton"),"L1").Monster.id,Is.EqualTo("L1"));
  }
  [Test]public void SpendCallbackCannotReenterDrawAndOuterDrawRemainsValid(){
   var g=G();Restore(g,long.MaxValue-1);
   Func<DrawCurrency,int,bool> spend=(_,__)=>{Assert.Throws<InvalidOperationException>(()=>Draw(g));return true;};
   g.DrawDetailed(spend,DrawCurrency.Diamond,100);
   Assert.That((long)g.CaptureMileageState().successfulDraws,Is.EqualTo(long.MaxValue));
   Assert.That((int)g.Roster.GetFragments("C"),Is.Zero);
  }
  [Test]public void SpendCallbackCannotRestoreMileageOrRedeemExistingTicket(){
   var g=G();Restore(g,200);
   Func<DrawCurrency,int,bool> spend=(_,__)=>{
    Assert.Throws<InvalidOperationException>(()=>Restore(g,0));
    Assert.Throws<InvalidOperationException>(()=>g.RedeemMileage(Selection("Origin","Skeleton"),"L1"));return true;};
   g.DrawDetailed(spend,DrawCurrency.Diamond,100);
   Assert.That((long)g.AvailableSelectionTickets,Is.EqualTo(1));Assert.That((int)g.MileageProgress,Is.EqualTo(1));
  }
  [Test]public void FailedSpendReleasesGuardForLaterDraw(){var g=G();Draw(g,1,false);Draw(g);Assert.That((int)g.MileageProgress,Is.EqualTo(1));}
  [Test]public void RedemptionOverflowLeavesTicketAndRosterUntouched(){
   var g=G();g.Roster.RestoreOwned("L1",1,int.MaxValue);Restore(g,200);
   Assert.Throws<OverflowException>(()=>g.RedeemMileage(Selection("Origin","Skeleton"),"L1"));
   Assert.That((long)g.AvailableSelectionTickets,Is.EqualTo(1));Assert.That((int)g.Roster.GetFragments("L1"),Is.EqualTo(int.MaxValue));
  }
  [Test]public void ReturnedRewardMutationCannotChangeLaterEligibility(){
   var g=G();Restore(g,400);var result=g.RedeemMileage(Selection("Origin","Skeleton"),"L1");
   result.Monster.traits[0]="Abyss";result.Monster.id="changed";
   Assert.That((string)g.RedeemMileage(Selection("Origin","Skeleton"),"L1").Monster.id,Is.EqualTo("L1"));
  }
 }
}
