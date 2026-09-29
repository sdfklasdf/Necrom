using System;
using System.Collections.Generic;

namespace Necrom.Core.Domain
{
    public sealed class Formation
    {
        public const int Capacity = 5;
        private readonly EntityId?[] _slots = new EntityId?[Capacity];

        public long Revision { get; private set; }

        public static Formation Restore(IReadOnlyList<KeyValuePair<int, EntityId>> occupiedSlots, long revision)
        {
            if (occupiedSlots == null) throw new ArgumentNullException(nameof(occupiedSlots));
            if (revision < 0) throw new ArgumentOutOfRangeException(nameof(revision));
            if (occupiedSlots.Count > Capacity) throw new InvalidOperationException("Formation exceeds capacity.");

            var restored = new Formation();
            var seenUnits = new HashSet<EntityId>();
            foreach (var entry in occupiedSlots)
            {
                ValidateIndex(entry.Key);
                if (restored._slots[entry.Key].HasValue) throw new InvalidOperationException("Duplicate formation slot.");
                if (!seenUnits.Add(entry.Value)) throw new InvalidOperationException("Unit already occupies another slot.");
                restored._slots[entry.Key] = entry.Value;
            }

            restored.Revision = revision;
            return restored;
        }

        public EntityId? GetSlot(int index)
        {
            ValidateIndex(index);
            return _slots[index];
        }

        public void EnsureCanAssign(int index, EntityId unitId, long expectedRevision)
        {
            ValidateIndex(index);
            RequireRevision(expectedRevision);
            if (_slots[index].HasValue && !_slots[index].Value.Equals(unitId))
                throw new InvalidOperationException("Target formation slot is occupied.");

            for (var i = 0; i < _slots.Length; i++)
            {
                if (_slots[i].HasValue && _slots[i].Value.Equals(unitId) && i != index)
                    throw new InvalidOperationException("Unit already occupies another slot.");
            }
        }

        public void Assign(int index, EntityId unitId, long expectedRevision)
        {
            EnsureCanAssign(index, unitId, expectedRevision);
            _slots[index] = unitId;
            Revision++;
        }

        public IReadOnlyList<EntityId?> Snapshot() => Array.AsReadOnly((EntityId?[])_slots.Clone());

        private void RequireRevision(long expectedRevision)
        {
            if (expectedRevision != Revision) throw new InvalidOperationException("Revision conflict.");
        }

        private static void ValidateIndex(int index)
        {
            if (index < 0 || index >= Capacity) throw new ArgumentOutOfRangeException(nameof(index));
        }
    }
}
