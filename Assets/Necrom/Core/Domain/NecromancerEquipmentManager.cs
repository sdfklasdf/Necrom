using System;
using System.Collections.Generic;
namespace Necrom.Core.Domain
{
 public enum EquipmentSlot { Weapon, Hat, Robe, Ring, Necklace }
 [Serializable] public sealed class EquipmentItem
 {
  public string id,displayName,visualKey;
  public EquipmentSlot slot;
  public int hpBonus,attackBonus;
 }
 public sealed class NecromancerEquipmentManager
 {
  readonly Dictionary<string,EquipmentItem> owned = new Dictionary<string,EquipmentItem>();
  readonly Dictionary<EquipmentSlot,string> equipped = new Dictionary<EquipmentSlot,string>();
  public void Unlock(EquipmentItem item)
  {
   if(item==null || string.IsNullOrEmpty(item.id) || item.hpBonus<0 || item.attackBonus<0)
    throw new ArgumentException("Invalid equipment");
   if(owned.ContainsKey(item.id)) return;
   owned.Add(item.id,item);
  }
  public bool Equip(string id)
  {
   EquipmentItem item;
   if(string.IsNullOrEmpty(id) || !owned.TryGetValue(id,out item)) return false;
   equipped[item.slot]=id;
   return true;
  }
  public bool Unequip(EquipmentSlot slot) { return equipped.Remove(slot); }
  public EquipmentItem GetEquipped(EquipmentSlot slot)
  {
   string id; EquipmentItem item;
   return equipped.TryGetValue(slot,out id) && owned.TryGetValue(id,out item) ? item : null;
  }
  public int TotalHpBonus
  {
   get { int total=0; foreach(var id in equipped.Values) checked { total+=owned[id].hpBonus; } return total; }
  }
  public int TotalAttackBonus
  {
   get { int total=0; foreach(var id in equipped.Values) checked { total+=owned[id].attackBonus; } return total; }
  }
  // visualKey identifies the art asset; the renderer applies the actual sprite/skin.
  public string GetVisualKey(EquipmentSlot slot)
  {
   var item=GetEquipped(slot);
   return item==null ? null : item.visualKey;
  }
 }
}