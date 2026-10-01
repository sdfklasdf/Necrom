using System;
using System.Linq;
using Necrom.Core.Application;
using Necrom.Core.Domain;

namespace Necrom.FirstPlayable.Runtime
{
    public sealed class SoulResourceRaiseResult
    {
        public CommandResult CommandResult { get; }
        public SoulResourceTransactionResult ResourceTransaction { get; }

        public SoulResourceRaiseResult(
            CommandResult commandResult,
            SoulResourceTransactionResult resourceTransaction)
        {
            CommandResult = commandResult ?? throw new ArgumentNullException(nameof(commandResult));
            ResourceTransaction = resourceTransaction ?? throw new ArgumentNullException(nameof(resourceTransaction));
        }
    }

    public sealed class FirstPlayableSoulResourceBridge
    {
        private readonly SoulResourceAccount _account;
        private readonly Func<CombatantDefeated, int> _defeatGrantPolicy;
        private readonly Func<RaiseSource, int> _raiseCostPolicy;
        private readonly Func<RaiseSource, string> _raiseTransactionIdProvider;

        public FirstPlayableSoulResourceBridge(
            SoulResourceAccount account,
            Func<CombatantDefeated, int> defeatGrantPolicy,
            Func<RaiseSource, int> raiseCostPolicy,
            Func<RaiseSource, string> raiseTransactionIdProvider)
        {
            _account = account ?? throw new ArgumentNullException(nameof(account));
            _defeatGrantPolicy = defeatGrantPolicy ?? throw new ArgumentNullException(nameof(defeatGrantPolicy));
            _raiseCostPolicy = raiseCostPolicy ?? throw new ArgumentNullException(nameof(raiseCostPolicy));
            _raiseTransactionIdProvider = raiseTransactionIdProvider
                ?? throw new ArgumentNullException(nameof(raiseTransactionIdProvider));
        }

        public SoulResourceTransactionResult ApplyDefeatGrant(
            DamageDeathResult damageResult,
            long expectedResourceRevision)
        {
            if (damageResult == null) throw new ArgumentNullException(nameof(damageResult));
            if (!damageResult.BecameDefeated)
                return null;

            var defeatEvents = damageResult.Events
                .OfType<CombatantDefeated>()
                .ToArray();

            if (defeatEvents.Length != 1)
                throw new InvalidOperationException(
                    "A defeated damage result must contain exactly one defeat event.");

            var defeat = defeatEvents[0];
            var amount = _defeatGrantPolicy(defeat);

            return _account.Grant(
                defeat.EventId,
                amount,
                expectedResourceRevision);
        }

        public SoulResourceRaiseResult ExecuteRaise(
            EnemySpawnController enemySpawn,
            FirstPlayableRaiseActionController raiseAction,
            long expectedResourceRevision)
        {
            if (enemySpawn == null) throw new ArgumentNullException(nameof(enemySpawn));
            if (raiseAction == null) throw new ArgumentNullException(nameof(raiseAction));

            var target = enemySpawn.CurrentTarget;
            if (target == null || !target.TryGetAvailableRaiseSource(out var source))
                throw new InvalidOperationException("Current target is not eligible for Raise.");

            var cost = _raiseCostPolicy(source);
            var transactionId = _raiseTransactionIdProvider(source);

            CommandResult commandResult = null;
            var resourceTransaction = _account.SpendOnSuccess(
                transactionId,
                cost,
                expectedResourceRevision,
                () =>
                {
                    commandResult = raiseAction.Execute();
                    if (commandResult == null)
                        throw new InvalidOperationException(
                            "Raise action returned no command result.");
                });

            return new SoulResourceRaiseResult(
                commandResult,
                resourceTransaction);
        }
    }
}