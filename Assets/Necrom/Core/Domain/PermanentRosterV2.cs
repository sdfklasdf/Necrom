using System;
using System.Collections.Generic;

namespace Necrom.Core.Domain
{
    // Permanent collection is deliberately independent of Combatant, Formation and RaiseSource.
    public interface IPermanentRoster
    {
        int UnlockedSlots { get; }
        int OwnedCount { get; }
        bool Owns(string monsterId);
        string GetDeckSlot(int slot);
    }

    public sealed class PermanentMonsterRoster : IPermanentRoster
    {
        private readonly Dictionary<string, int> stars = new Dictionary<string, int>(StringComparer.Ordinal);
        private readonly Dictionary<string, int> fragments = new Dictionary<string, int>(StringComparer.Ordinal);
        private readonly string[] deck = new string[6];
        private int unlockedSlots = 3;
        public int UnlockedSlots => unlockedSlots;
        public int OwnedCount => stars.Count;
        public IReadOnlyList<string> OwnedIds => new List<string>(stars.Keys);
        public void RestoreOwned(string id,int star,int fragment)
        {
            if(string.IsNullOrWhiteSpace(id) || star<1 || star>4 || fragment<0 || stars.ContainsKey(id))
                throw new ArgumentException("Invalid persisted monster record");
            stars.Add(id,star);fragments.Add(id,fragment);
        }
        public bool Owns(string id) => !string.IsNullOrEmpty(id) && stars.ContainsKey(id);
        public int GetStars(string id) => stars.TryGetValue(id, out var value) ? value : 0;
        public int GetFragments(string id) => fragments.TryGetValue(id, out var value) ? value : 0;
        public string GetDeckSlot(int slot)
        {
            if (slot < 0 || slot >= deck.Length) throw new ArgumentOutOfRangeException(nameof(slot));
            return deck[slot];
        }
        public void UnlockSlotAtApprovedLevel(int newSlotCount)
        {
            // Progression thresholds are a separate, not-yet-approved rule.
            if (newSlotCount < unlockedSlots || newSlotCount > 6) throw new ArgumentOutOfRangeException(nameof(newSlotCount));
            unlockedSlots = newSlotCount;
        }
        public bool Assign(int slot, string id)
        {
            if (slot < 0 || slot >= unlockedSlots || !Owns(id)) return false;
            for (int i = 0; i < deck.Length; i++)
                if (i != slot && string.Equals(deck[i], id, StringComparison.Ordinal)) return false;
            deck[slot] = id;
            return true;
        }
        public void Clear(int slot)
        {
            if (slot < 0 || slot >= deck.Length) throw new ArgumentOutOfRangeException(nameof(slot));
            deck[slot] = null;
        }
        public bool Grant(string id)
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentException("Monster id required.", nameof(id));
            if (!stars.ContainsKey(id))
            {
                stars.Add(id, 1);
                fragments.Add(id, 0);
                return true;
            }
            fragments[id] = checked(fragments[id] + 1);
            return false;
        }
        public bool TryUpgrade(string id, int fragmentCost)
        {
            if (fragmentCost <= 0) throw new ArgumentOutOfRangeException(nameof(fragmentCost));
            if (!Owns(id) || stars[id] >= 4 || fragments[id] < fragmentCost) return false;
            fragments[id] -= fragmentCost;
            stars[id]++;
            return true;
        }
    }

    // Runtime Raise never mutates permanent ownership; IDs must never be reused across domains.
    public sealed class TemporaryRaiseRegistry
    {
        private readonly Dictionary<string, int> active = new Dictionary<string, int>(StringComparer.Ordinal);
        private readonly List<string> insertionOrder = new List<string>();
        public int Count => active.Count;
        public string Register(string runtimeId, int strength)
        {
            if (string.IsNullOrEmpty(runtimeId) || !runtimeId.StartsWith("RAISE:", StringComparison.Ordinal))
                throw new ArgumentException("Raise instance must use RAISE: namespace.", nameof(runtimeId));
            if (strength < 0 || active.ContainsKey(runtimeId)) throw new InvalidOperationException("Invalid or duplicate Raise instance.");
            string evicted = null;
            if (active.Count == 5)
            {
                int weakest = int.MaxValue;
                foreach (var id in insertionOrder)
                    if (active[id] < weakest) { weakest = active[id]; evicted = id; }
                Remove(evicted);
            }
            active.Add(runtimeId, strength);
            insertionOrder.Add(runtimeId);
            return evicted;
        }
        public bool Remove(string id)
        {
            if (!active.Remove(id)) return false;
            insertionOrder.Remove(id);
            return true;
        }
        public void ResetWave() { active.Clear(); insertionOrder.Clear(); }
    }
}
