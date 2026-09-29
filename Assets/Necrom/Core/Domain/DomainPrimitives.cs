using System;

namespace Necrom.Core.Domain
{
    public readonly struct EntityId : IEquatable<EntityId>
    {
        public string Value { get; }

        public EntityId(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Entity id is required.", nameof(value));
            Value = value;
        }

        public bool Equals(EntityId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);
        public override bool Equals(object obj) => obj is EntityId other && Equals(other);
        public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);
        public override string ToString() => Value;
    }

    public enum Faction { Player, Enemy }
    public enum CombatantLifeState { Active, Defeated }
    public enum BattlePhase { Ready, Running, Victory, Defeat, Resolved }
}
