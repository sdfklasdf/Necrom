using System;

namespace Necrom.Core.Domain
{
    public sealed class BattleStateMachine
    {
        public BattlePhase Phase { get; private set; } = BattlePhase.Ready;
        public long Revision { get; private set; }

        public static BattleStateMachine Restore(BattlePhase phase, long revision)
        {
            if (!Enum.IsDefined(typeof(BattlePhase), phase)) throw new ArgumentOutOfRangeException(nameof(phase));
            if (revision < 0) throw new ArgumentOutOfRangeException(nameof(revision));

            return new BattleStateMachine { Phase = phase, Revision = revision };
        }

        public void Start(long expectedRevision)
        {
            RequireRevision(expectedRevision);
            if (Phase != BattlePhase.Ready) throw new InvalidOperationException("Battle can start only from Ready.");
            Phase = BattlePhase.Running;
            Revision++;
        }

        public void Resolve(bool playerWon, long expectedRevision)
        {
            RequireRevision(expectedRevision);
            if (Phase != BattlePhase.Running) throw new InvalidOperationException("Only a running battle can resolve.");
            Phase = playerWon ? BattlePhase.Victory : BattlePhase.Defeat;
            Revision++;
        }

        public void FinalizeResult(long expectedRevision)
        {
            RequireRevision(expectedRevision);
            if (Phase != BattlePhase.Victory && Phase != BattlePhase.Defeat)
                throw new InvalidOperationException("A terminal result is required before finalization.");
            Phase = BattlePhase.Resolved;
            Revision++;
        }

        private void RequireRevision(long expectedRevision)
        {
            if (expectedRevision != Revision) throw new InvalidOperationException("Revision conflict.");
        }
    }
}
