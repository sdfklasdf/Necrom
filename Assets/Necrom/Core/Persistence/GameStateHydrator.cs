using System;
using System.Collections.Generic;
using Necrom.Core.Domain;

namespace Necrom.Core.Persistence
{
    public sealed class HydratedGameState
    {
        public BattleStateMachine Battle { get; }
        public Formation Formation { get; }

        public HydratedGameState(BattleStateMachine battle, Formation formation)
        {
            Battle = battle ?? throw new ArgumentNullException(nameof(battle));
            Formation = formation ?? throw new ArgumentNullException(nameof(formation));
        }
    }

    public static class GameStateHydrator
    {
        public static HydratedGameState Restore(GameStateSnapshot snapshot)
        {
            GameStateSnapshotValidator.Validate(snapshot);

            var slots = new List<KeyValuePair<int, EntityId>>();
            foreach (var entry in snapshot.Formation)
                slots.Add(new KeyValuePair<int, EntityId>(entry.Slot, new EntityId(entry.UnitId)));

            return new HydratedGameState(
                BattleStateMachine.Restore(snapshot.BattlePhase, snapshot.BattleRevision),
                Formation.Restore(slots.AsReadOnly(), snapshot.FormationRevision));
        }
    }
}
