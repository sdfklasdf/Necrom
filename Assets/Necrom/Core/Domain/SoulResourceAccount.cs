using System;
using System.Collections.Generic;

namespace Necrom.Core.Domain
{
    public enum SoulResourceTransactionKind
    {
        Grant,
        Spend
    }

    public sealed class SoulResourceTransactionResult
    {
        public string TransactionId { get; }
        public SoulResourceTransactionKind Kind { get; }
        public int Amount { get; }
        public int BalanceBefore { get; }
        public int BalanceAfter { get; }
        public long RevisionBefore { get; }
        public long RevisionAfter { get; }

        public SoulResourceTransactionResult(
            string transactionId,
            SoulResourceTransactionKind kind,
            int amount,
            int balanceBefore,
            int balanceAfter,
            long revisionBefore,
            long revisionAfter)
        {
            if (string.IsNullOrWhiteSpace(transactionId))
                throw new ArgumentException("Transaction id is required.", nameof(transactionId));
            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount));
            if (balanceBefore < 0 || balanceAfter < 0)
                throw new ArgumentOutOfRangeException(nameof(balanceBefore));
            if (revisionBefore < 0 || revisionAfter != revisionBefore + 1)
                throw new ArgumentOutOfRangeException(nameof(revisionAfter));

            TransactionId = transactionId;
            Kind = kind;
            Amount = amount;
            BalanceBefore = balanceBefore;
            BalanceAfter = balanceAfter;
            RevisionBefore = revisionBefore;
            RevisionAfter = revisionAfter;
        }
    }

    public sealed class SoulResourceAccount
    {
        private readonly HashSet<string> _processedTransactionIds = new HashSet<string>(StringComparer.Ordinal);
        private bool _spendInProgress;

        public int Balance { get; private set; }
        public long Revision { get; private set; }

        public SoulResourceAccount(int initialBalance)
        {
            if (initialBalance < 0)
                throw new ArgumentOutOfRangeException(nameof(initialBalance));

            Balance = initialBalance;
            Revision = 0;
        }

        public SoulResourceTransactionResult Grant(
            string transactionId,
            int amount,
            long expectedRevision)
        {
            EnsureCanStartTransaction(transactionId, amount, expectedRevision);

            var beforeBalance = Balance;
            var beforeRevision = Revision;

            checked
            {
                Balance += amount;
            }

            Revision++;
            _processedTransactionIds.Add(transactionId);

            return new SoulResourceTransactionResult(
                transactionId,
                SoulResourceTransactionKind.Grant,
                amount,
                beforeBalance,
                Balance,
                beforeRevision,
                Revision);
        }

        public bool CanSpend(int amount, long expectedRevision)
        {
            if (_spendInProgress || amount <= 0)
                return false;
            if (expectedRevision != Revision)
                return false;
            return Balance >= amount;
        }

        public SoulResourceTransactionResult SpendOnSuccess(
            string transactionId,
            int amount,
            long expectedRevision,
            Action action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            EnsureCanStartTransaction(transactionId, amount, expectedRevision);

            if (Balance < amount)
                throw new InvalidOperationException("Insufficient soul resource.");

            var beforeBalance = Balance;
            var beforeRevision = Revision;

            _spendInProgress = true;
            try
            {
                action();
                Balance -= amount;
                Revision++;
                _processedTransactionIds.Add(transactionId);
            }
            finally
            {
                _spendInProgress = false;
            }

            return new SoulResourceTransactionResult(
                transactionId,
                SoulResourceTransactionKind.Spend,
                amount,
                beforeBalance,
                Balance,
                beforeRevision,
                Revision);
        }

        private void EnsureCanStartTransaction(
            string transactionId,
            int amount,
            long expectedRevision)
        {
            if (_spendInProgress)
                throw new InvalidOperationException("A soul resource transaction is already in progress.");
            if (string.IsNullOrWhiteSpace(transactionId))
                throw new ArgumentException("Transaction id is required.", nameof(transactionId));
            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount));
            if (_processedTransactionIds.Contains(transactionId))
                throw new InvalidOperationException("Soul resource transaction was already processed.");
            if (expectedRevision != Revision)
                throw new InvalidOperationException("Soul resource revision conflict.");
        }
    }
}