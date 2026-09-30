using Necrom.Core.Application;
using Necrom.Core.Domain;
using Necrom.Core.Persistence;
using NUnit.Framework;

namespace Necrom.Core.Tests
{
    public sealed class HydratedContinuationTests
    {
        [Test]
        public void HydratedFormationCanContinueIntoNextCombatWithoutLosingRaisedUnit()
        {
            var formation = new Formation();
            var undeadId = new EntityId("undead-1");
            formation.Assign(0, undeadId, 0);

            var battle = new BattleStateMachine();
            var snapshot = GameStateSnapshot.Capture("1", battle, formation);
            var restored = GameStateHydrator.Restore(snapshot);

            var encounter = new FirstPlayableEncounter(
                new RaiseService(), restored.Battle, restored.Formation);
            var app = new FirstPlayableApplicationService(encounter);

            Assert.That(encounter.IsReadyForNextCombat(undeadId), Is.True);

            app.Execute(new StartBattleCommand("start-restored", restored.Battle.Revision));

            Assert.That(restored.Battle.Phase, Is.EqualTo(BattlePhase.Running));
            Assert.That(restored.Battle.Revision, Is.EqualTo(1));
            Assert.That(restored.Formation.GetSlot(0).Value, Is.EqualTo(undeadId));
            Assert.That(restored.Formation.Revision, Is.EqualTo(1));
        }
    }
}
