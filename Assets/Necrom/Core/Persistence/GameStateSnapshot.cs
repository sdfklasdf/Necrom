using System;
using System.Collections.Generic;
using Necrom.Core.Domain;

namespace Necrom.Core.Persistence
{
    public sealed class FormationSlotSnapshot
    {
        public int Slot { get; }
        public string UnitId { get; }

        public FormationSlotSnapshot(int slot, string unitId)
        {
            if (slot < 0 || slot >= Formation.Capacity) throw new ArgumentOutOfRangeException(nameof(slot));
            if (string.IsNullOrWhiteSpace(unitId)) throw new ArgumentException("Unit id is required.", nameof(unitId));
            Slot = slot;
            UnitId = unitId;
        }
    }

    public sealed class GameStateSnapshot
    {
        public string SchemaVersion { get; }
        public BattlePhase BattlePhase { get; }
        public long BattleRevision { get; }
        public long FormationRevision { get; }
        public IReadOnlyList<FormationSlotSnapshot> Formation { get; }

        public GameStateSnapshot(string schemaVersion, BattlePhase battlePhase, long battleRevision,
            long formationRevision, IReadOnlyList<FormationSlotSnapshot> formation)
        {
            if (string.IsNullOrWhiteSpace(schemaVersion)) throw new ArgumentException("Schema version is required.", nameof(schemaVersion));
            if (formation == null) throw new ArgumentNullException(nameof(formation));

            SchemaVersion = schemaVersion;
            BattlePhase = battlePhase;
            BattleRevision = battleRevision;
            FormationRevision = formationRevision;

            var copy = new FormationSlotSnapshot[formation.Count];
            for (var i = 0; i < formation.Count; i++) copy[i] = formation[i];
            Formation = Array.AsReadOnly(copy);
        }

        public static GameStateSnapshot Capture(string schemaVersion, BattleStateMachine battle, Formation formation)
        {
            if (battle == null) throw new ArgumentNullException(nameof(battle));
            if (formation == null) throw new ArgumentNullException(nameof(formation));

            var slots = new List<FormationSlotSnapshot>();
            for (var i = 0; i < Formation.Capacity; i++)
            {
                var unit = formation.GetSlot(i);
                if (unit.HasValue) slots.Add(new FormationSlotSnapshot(i, unit.Value.Value));
            }

            return new GameStateSnapshot(schemaVersion, battle.Phase, battle.Revision, formation.Revision, slots.AsReadOnly());
        }
    }
}
