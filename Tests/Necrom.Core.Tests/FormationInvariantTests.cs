using System;
using Necrom.Core.Domain;
using NUnit.Framework;

namespace Necrom.Core.Tests
{
    public sealed class FormationInvariantTests
    {
        [Test]
        public void AssignAdvancesRevisionAndStoresUnit()
        {
            var formation = new Formation();
            var unit = new EntityId("unit-a");

            formation.Assign(0, unit, 0);

            Assert.That(formation.GetSlot(0).Value, Is.EqualTo(unit));
            Assert.That(formation.Revision, Is.EqualTo(1));
        }

        [Test]
        public void StaleRevisionRejectsWithoutMutation()
        {
            var formation = new Formation();
            var unit = new EntityId("unit-a");

            Assert.Throws<InvalidOperationException>(() => formation.Assign(0, unit, 1));
            Assert.That(formation.GetSlot(0).HasValue, Is.False);
            Assert.That(formation.Revision, Is.EqualTo(0));
        }

        [Test]
        public void OccupiedTargetSlotRejectsDifferentUnitWithoutMutation()
        {
            var formation = new Formation();
            formation.Assign(0, new EntityId("unit-a"), 0);

            Assert.Throws<InvalidOperationException>(() =>
                formation.Assign(0, new EntityId("unit-b"), 1));

            Assert.That(formation.GetSlot(0).Value.Value, Is.EqualTo("unit-a"));
            Assert.That(formation.Revision, Is.EqualTo(1));
        }

        [Test]
        public void SameUnitCannotOccupyTwoSlots()
        {
            var formation = new Formation();
            var unit = new EntityId("unit-a");
            formation.Assign(0, unit, 0);

            Assert.Throws<InvalidOperationException>(() => formation.Assign(1, unit, 1));

            Assert.That(formation.GetSlot(0).Value, Is.EqualTo(unit));
            Assert.That(formation.GetSlot(1).HasValue, Is.False);
            Assert.That(formation.Revision, Is.EqualTo(1));
        }

        [Test]
        public void ReassigningSameUnitToSameSlotIsIdempotentInStateButAdvancesRevision()
        {
            var formation = new Formation();
            var unit = new EntityId("unit-a");
            formation.Assign(0, unit, 0);

            formation.Assign(0, unit, 1);

            Assert.That(formation.GetSlot(0).Value, Is.EqualTo(unit));
            Assert.That(formation.Revision, Is.EqualTo(2));
        }
    }
}
