using System;
using Necrom.Core.Domain;

namespace Necrom.Core.Application
{
    public abstract class FirstPlayableCommand
    {
        public string CommandId { get; }

        protected FirstPlayableCommand(string commandId)
        {
            if (string.IsNullOrWhiteSpace(commandId)) throw new ArgumentException("Command id is required.", nameof(commandId));
            CommandId = commandId;
        }
    }

    public sealed class StartBattleCommand : FirstPlayableCommand
    {
        public long ExpectedBattleRevision { get; }
        public StartBattleCommand(string commandId, long expectedBattleRevision) : base(commandId)
            => ExpectedBattleRevision = expectedBattleRevision;
    }

    public sealed class ResolveBattleCommand : FirstPlayableCommand
    {
        public bool PlayerWon { get; }
        public long ExpectedBattleRevision { get; }

        public ResolveBattleCommand(string commandId, bool playerWon, long expectedBattleRevision) : base(commandId)
        {
            PlayerWon = playerWon;
            ExpectedBattleRevision = expectedBattleRevision;
        }
    }

    public sealed class FinalizeBattleCommand : FirstPlayableCommand
    {
        public long ExpectedBattleRevision { get; }

        public FinalizeBattleCommand(string commandId, long expectedBattleRevision) : base(commandId)
            => ExpectedBattleRevision = expectedBattleRevision;
    }

    public sealed class RestartBattleCommand : FirstPlayableCommand
    {
        public long ExpectedBattleRevision { get; }

        public RestartBattleCommand(string commandId, long expectedBattleRevision) : base(commandId)
            => ExpectedBattleRevision = expectedBattleRevision;
    }

    public sealed class RaiseIntoFormationCommand : FirstPlayableCommand
    {
        public RaiseSource Source { get; }
        public long ExpectedSourceRevision { get; }
        public EntityId UndeadId { get; }
        public int RestoredHealth { get; }
        public int Slot { get; }
        public long ExpectedFormationRevision { get; }

        public RaiseIntoFormationCommand(string commandId, RaiseSource source, long expectedSourceRevision,
            EntityId undeadId, int restoredHealth, int slot, long expectedFormationRevision) : base(commandId)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            if (restoredHealth <= 0) throw new ArgumentOutOfRangeException(nameof(restoredHealth));
            ExpectedSourceRevision = expectedSourceRevision;
            UndeadId = undeadId;
            RestoredHealth = restoredHealth;
            Slot = slot;
            ExpectedFormationRevision = expectedFormationRevision;
        }
    }
}
