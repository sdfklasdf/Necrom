using System;

namespace Necrom.Core.Domain
{
    public sealed class BasicAutoBehaviorSpec
    {
        public int DamagePerAction { get; }
        public int AttackIntervalMilliseconds { get; }

        public BasicAutoBehaviorSpec(int damagePerAction, int attackIntervalMilliseconds)
        {
            if (damagePerAction <= 0)
                throw new ArgumentOutOfRangeException(nameof(damagePerAction));
            if (attackIntervalMilliseconds <= 0)
                throw new ArgumentOutOfRangeException(nameof(attackIntervalMilliseconds));

            DamagePerAction = damagePerAction;
            AttackIntervalMilliseconds = attackIntervalMilliseconds;
        }
    }

    public sealed class BasicAttackIntent
    {
        public EntityId ActorId { get; }
        public int Damage { get; }

        public BasicAttackIntent(EntityId actorId, int damage)
        {
            if (damage <= 0) throw new ArgumentOutOfRangeException(nameof(damage));
            ActorId = actorId;
            Damage = damage;
        }
    }

    public sealed class NecromancerBasicAutoBehavior
    {
        public Combatant Actor { get; }
        public BasicAutoBehaviorSpec Spec { get; }

        public NecromancerBasicAutoBehavior(Combatant actor, BasicAutoBehaviorSpec spec)
        {
            Actor = actor ?? throw new ArgumentNullException(nameof(actor));
            Spec = spec ?? throw new ArgumentNullException(nameof(spec));
            if (Actor.Faction != Faction.Player)
                throw new InvalidOperationException("Necromancer auto behavior requires Player faction.");
        }

        public bool TryCreateBasicAttack(out BasicAttackIntent intent)
        {
            if (Actor.LifeState != CombatantLifeState.Active)
            {
                intent = null;
                return false;
            }

            intent = new BasicAttackIntent(Actor.Id, Spec.DamagePerAction);
            return true;
        }
    }
}
