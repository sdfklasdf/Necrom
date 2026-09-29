using System;

namespace Necrom.Core.Domain
{
    public sealed class Combatant
    {
        public EntityId Id { get; }
        public string ArchetypeId { get; }
        public Faction Faction { get; private set; }
        public CombatantLifeState LifeState { get; private set; }
        public int Health { get; private set; }

        public Combatant(EntityId id, string archetypeId, Faction faction, int health)
        {
            if (string.IsNullOrWhiteSpace(archetypeId)) throw new ArgumentException("Archetype is required.", nameof(archetypeId));
            if (health <= 0) throw new ArgumentOutOfRangeException(nameof(health));
            Id = id;
            ArchetypeId = archetypeId;
            Faction = faction;
            Health = health;
            LifeState = CombatantLifeState.Active;
        }

        public bool ApplyDamage(int amount)
        {
            if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));
            if (LifeState == CombatantLifeState.Defeated) return false;
            Health = Math.Max(0, Health - amount);
            if (Health == 0) LifeState = CombatantLifeState.Defeated;
            return true;
        }

        internal void ConvertToPlayerFaction(int restoredHealth)
        {
            if (LifeState != CombatantLifeState.Defeated) throw new InvalidOperationException("Only defeated combatants can be converted.");
            if (restoredHealth <= 0) throw new ArgumentOutOfRangeException(nameof(restoredHealth));
            Faction = Faction.Player;
            Health = restoredHealth;
            LifeState = CombatantLifeState.Active;
        }
    }
}
