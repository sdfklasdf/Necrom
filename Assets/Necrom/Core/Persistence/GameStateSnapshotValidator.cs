using System;
using System.Collections.Generic;
using Necrom.Core.Domain;

namespace Necrom.Core.Persistence
{
    public static class GameStateSnapshotValidator
    {
        public static void Validate(GameStateSnapshot snapshot)
        {
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
            if (string.IsNullOrWhiteSpace(snapshot.SchemaVersion)) throw new InvalidOperationException("Snapshot schema version is required.");
            if (!Enum.IsDefined(typeof(BattlePhase), snapshot.BattlePhase)) throw new InvalidOperationException("Unknown battle phase.");
            if (snapshot.BattleRevision < 0) throw new InvalidOperationException("Battle revision cannot be negative.");
            if (snapshot.FormationRevision < 0) throw new InvalidOperationException("Formation revision cannot be negative.");

            var slots = new HashSet<int>();
            var units = new HashSet<string>(StringComparer.Ordinal);
            foreach (var entry in snapshot.Formation)
            {
                if (entry == null) throw new InvalidOperationException("Formation snapshot entry cannot be null.");
                if (entry.Slot < 0 || entry.Slot >= Formation.Capacity) throw new InvalidOperationException("Formation slot is out of range.");
                if (string.IsNullOrWhiteSpace(entry.UnitId)) throw new InvalidOperationException("Formation unit id is required.");
                if (!slots.Add(entry.Slot)) throw new InvalidOperationException("Duplicate formation slot.");
                if (!units.Add(entry.UnitId)) throw new InvalidOperationException("A unit cannot occupy multiple formation slots.");
            }

            if (snapshot.Formation.Count > Formation.Capacity) throw new InvalidOperationException("Formation exceeds capacity.");
        }
    }
}
