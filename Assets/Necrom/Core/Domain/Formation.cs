using System;
using System.Collections.Generic;

namespace Necrom.Core.Domain
{
    public sealed class Formation
    {
        public const int Capacity = 5;
        private readonly EntityId?[] _slots = new EntityId?[Capacity];

        public long Revision { get; private set; }

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
