using System;

namespace Necrom.FirstPlayable.Runtime
{
    public sealed class EnemyArchetypeDefinition
    {
        public string ArchetypeId { get; }
        public string RoleId { get; }
        public int MaxHealth { get; }

        public EnemyArchetypeDefinition(string archetypeId, string roleId, int maxHealth)
        {
            if (string.IsNullOrWhiteSpace(archetypeId))
                throw new ArgumentException("Archetype id is required.", nameof(archetypeId));
            if (string.IsNullOrWhiteSpace(roleId))
                throw new ArgumentException("Role id is required.", nameof(roleId));
            if (maxHealth <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxHealth));

            ArchetypeId = archetypeId;
            RoleId = roleId;
            MaxHealth = maxHealth;
        }
    }
}
