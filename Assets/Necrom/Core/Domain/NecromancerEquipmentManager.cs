using System;
using System.Collections.Generic;
namespace Necrom.Core.Domain
{
    public enum EquipmentSlot { Weapon, Hat, Robe, Ring, Necklace, Gloves, Shoes }
    public enum EquipmentRarity { Common, Advanced, Rare, Hero, Legendary }
    [Serializable] public sealed class EquipmentItem
    {
        public string id, displayName, visualKey;
        public EquipmentSlot slot;
        public EquipmentRarity rarity;
        public int hpBonus, attackBonus;
    }
    public sealed class NecromancerEquipmentManager
    {
        private readonly Dictionary<string,EquipmentItem> owned = new Dictionary<string,EquipmentItem>();
        private readonly Dictionary<string,int> quantities = new Dictionary<string,int>();
        private readonly Dictionary<EquipmentSlot,string> equipped = new Dictionary<EquipmentSlot,string>();
        public void Unlock(EquipmentItem item)
        {
            if(item == null || string.IsNullOrEmpty(item.id) || item.hpBonus < 0 || item.attackBonus < 0 ||
                !Enum.IsDefined(typeof(EquipmentSlot),item.slot) || !Enum.IsDefined(typeof(EquipmentRarity),item.rarity))
                throw new ArgumentException("Invalid equipment");
            if(owned.TryGetValue(item.id,out var existing))
            {
                if(existing.slot != item.slot || existing.rarity != item.rarity)
                    throw new InvalidOperationException("Equipment identity collision.");
                quantities[item.id] = checked(quantities[item.id]+1);
                return;
            }
            owned.Add(item.id,item);
            quantities.Add(item.id,1);
        }
        public int GetQuantity(string id) => id != null && quantities.TryGetValue(id,out var n) ? n : 0;
        public bool Equip(string id)
        {
            if(string.IsNullOrEmpty(id) || !owned.TryGetValue(id,out var item) || GetQuantity(id)<=0) return false;
            equipped[item.slot] = id;
            return true;
        }
        public bool Unequip(EquipmentSlot slot) => equipped.Remove(slot);
        public EquipmentItem GetEquipped(EquipmentSlot slot)
            => equipped.TryGetValue(slot,out var id) && owned.TryGetValue(id,out var item) ? item : null;
        public int TotalHpBonus { get { int n=0; foreach(var id in equipped.Values) n=checked(n+owned[id].hpBonus); return n; } }
        public int TotalAttackBonus { get { int n=0; foreach(var id in equipped.Values) n=checked(n+owned[id].attackBonus); return n; } }
        public string GetVisualKey(EquipmentSlot slot) => GetEquipped(slot)?.visualKey;

        // Consumes five identical items. The upgraded output must be a separate, approved recipe.
        public bool TrySynthesize(string inputId, EquipmentItem approvedOutput)
        {
            if(!owned.TryGetValue(inputId,out var input) || GetQuantity(inputId)<5 || approvedOutput==null) return false;
            if(input.rarity==EquipmentRarity.Legendary ||
               approvedOutput.rarity != input.rarity+1 ||
               approvedOutput.slot != input.slot ||
               string.Equals(input.id,approvedOutput.id,StringComparison.Ordinal))
                return false;
            if(equipped.TryGetValue(input.slot,out var equippedId) && equippedId==inputId)
                return false; // Do not silently consume currently equipped items.
            if(approvedOutput.hpBonus<0 || approvedOutput.attackBonus<0 || string.IsNullOrEmpty(approvedOutput.id))
                return false;
            // Validate output identity before mutation.
            if(owned.TryGetValue(approvedOutput.id,out var existing) &&
               (existing.slot!=approvedOutput.slot || existing.rarity!=approvedOutput.rarity)) return false;
            quantities[inputId] -= 5;
            Unlock(approvedOutput);
            return true;
        }
    }
}
