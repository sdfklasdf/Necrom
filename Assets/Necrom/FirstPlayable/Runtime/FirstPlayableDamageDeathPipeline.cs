using System;
using Necrom.Core.Domain;

namespace Necrom.FirstPlayable.Runtime
{
    public sealed class FirstPlayableDamageDeathPipeline
    {
        private readonly Func<string> _damageEventIdProvider;
        private readonly Func<string> _defeatEventIdProvider;

        public FirstPlayableDamageDeathPipeline(
            Func<string> damageEventIdProvider,
            Func<string> defeatEventIdProvider)
        {
            _damageEventIdProvider = damageEventIdProvider
                ?? throw new ArgumentNullException(nameof(damageEventIdProvider));
            _defeatEventIdProvider = defeatEventIdProvider
                ?? throw new ArgumentNullException(nameof(defeatEventIdProvider));
        }

        public DamageDeathResult Apply(
            EnemyRuntimeEntity target,
            int amount,
            Necrom.Core.Domain.EntityId raiseSourceId)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (target.Model == null)
                throw new InvalidOperationException(
                    "Damage target must be initialized.");
            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount));

            var model = target.Model;
            if (model.LifeState == CombatantLifeState.Defeated)
            {
                var repeatedChange = target.ApplyDamage(amount, raiseSourceId);
                if (repeatedChange)
                    throw new InvalidOperationException(
                        "A defeated target unexpectedly accepted damage.");

                return new DamageDeathResult(
                    false,
                    false,
                    Array.Empty<DomainEvent>());
            }

            var healthBefore = model.Health;
            var willDefeat = amount >= healthBefore;

            var damageEventId = _damageEventIdProvider();
            if (string.IsNullOrWhiteSpace(damageEventId))
                throw new ArgumentException(
                    "Damage event id is required.",
                    nameof(_damageEventIdProvider));

            string defeatEventId = null;
            if (willDefeat)
            {
                defeatEventId = _defeatEventIdProvider();
                if (string.IsNullOrWhiteSpace(defeatEventId))
                    throw new ArgumentException(
                        "Defeat event id is required.",
                        nameof(_defeatEventIdProvider));
            }

            var changed = target.ApplyDamage(amount, raiseSourceId);
            if (!changed)
            {
                return new DamageDeathResult(
                    false,
                    false,
                    Array.Empty<DomainEvent>());
            }

            var healthAfter = model.Health;
            var damageEvent = new DamageApplied(
                damageEventId,
                model.Id,
                amount,
                healthBefore - healthAfter,
                healthBefore,
                healthAfter);

            var becameDefeated =
                model.LifeState == CombatantLifeState.Defeated;
            if (!becameDefeated)
            {
                return new DamageDeathResult(
                    true,
                    false,
                    new DomainEvent[] { damageEvent });
            }

            var source = target.RaiseSource;
            if (source == null ||
                !source.DefeatedEntityId.Equals(model.Id))
            {
                throw new InvalidOperationException(
                    "A defeated target must expose its matching raise source.");
            }

            var defeatEvent = new CombatantDefeated(
                defeatEventId,
                model.Id,
                source.SourceId);

            return new DamageDeathResult(
                true,
                true,
                new DomainEvent[] { damageEvent, defeatEvent });
        }
    }
}