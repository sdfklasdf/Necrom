using System;

namespace Necrom.Core.Domain
{
    public sealed class Combatant
    {
        public EntityId Id { get; }
        public string ArchetypeId { get; }
        public Faction Faction { get; }
        public CombatantLifeState LifeState { get; private set; }
        public int Health { get; private set; }
        public int MaxHealth { get; private set; }

        public Combatant(EntityId id, string archetypeId, Faction faction, int health)
        {
            if (string.IsNullOrWhiteSpace(archetypeId)) throw new ArgumentException("Archetype is required.", nameof(archetypeId));
            if (health <= 0) throw new ArgumentOutOfRangeException(nameof(health));
            Id = id;
            ArchetypeId = archetypeId;
            Faction = faction;
            Health = health;
            MaxHealth = health;
            LifeState = CombatantLifeState.Active;
        }

        // Preserve the existing health percentage when a skill changes the maximum.
        // Defeated units stay defeated; living units retain at least 1 HP.
        public void RescaleMaxHealth(int newMaxHealth)
        {
            if (newMaxHealth <= 0) throw new ArgumentOutOfRangeException(nameof(newMaxHealth));
            if (newMaxHealth == MaxHealth) return;
            int next = LifeState == CombatantLifeState.Defeated ? 0 :
                (int)Math.Max(1, Math.Min(newMaxHealth,
                    Math.Round((double)Health * newMaxHealth / MaxHealth, MidpointRounding.AwayFromZero)));
            MaxHealth = newMaxHealth;
            Health = next;
        }

        public bool ApplyDamage(int amount)
        {
            if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));
            if (LifeState == CombatantLifeState.Defeated) return false;
            Health = Math.Max(0, Health - amount);
            if (Health == 0) LifeState = CombatantLifeState.Defeated;
            return true;
        }
    }
}
