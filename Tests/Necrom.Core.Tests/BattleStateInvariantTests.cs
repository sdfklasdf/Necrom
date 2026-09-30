using System;
using Necrom.Core.Domain;
using NUnit.Framework;

namespace Necrom.Core.Tests
{
    public sealed class BattleStateInvariantTests
    {
        [Test]
        public void AllowedPathReadyRunningVictoryResolvedAdvancesRevision()
        {
            var battle = new BattleStateMachine();

            battle.Start(0);
            battle.Resolve(true, 1);
            battle.FinalizeResult(2);

            Assert.That(battle.Phase, Is.EqualTo(BattlePhase.Resolved));
            Assert.That(battle.Revision, Is.EqualTo(3));
        }

        [Test]
        public void AllowedPathReadyRunningDefeatResolvedAdvancesRevision()
        {
            var battle = new BattleStateMachine();

            battle.Start(0);
            battle.Resolve(false, 1);
            battle.FinalizeResult(2);

            Assert.That(battle.Phase, Is.EqualTo(BattlePhase.Resolved));
            Assert.That(battle.Revision, Is.EqualTo(3));
        }

        [Test]
        public void StaleExpectedRevisionIsRejectedWithoutMutation()
        {
            var battle = new BattleStateMachine();
            battle.Start(0);

            Assert.Throws<InvalidOperationException>(() => battle.Resolve(true, 0));
            Assert.That(battle.Phase, Is.EqualTo(BattlePhase.Running));
            Assert.That(battle.Revision, Is.EqualTo(1));
        }

        [Test]
        public void StartOutsideReadyIsRejectedWithoutMutation()
        {
            var battle = new BattleStateMachine();
            battle.Start(0);

            Assert.Throws<InvalidOperationException>(() => battle.Start(1));
            Assert.That(battle.Phase, Is.EqualTo(BattlePhase.Running));
            Assert.That(battle.Revision, Is.EqualTo(1));
        }

        [Test]
        public void ResolveOutsideRunningIsRejectedWithoutMutation()
        {
            var battle = new BattleStateMachine();

            Assert.Throws<InvalidOperationException>(() => battle.Resolve(true, 0));
            Assert.That(battle.Phase, Is.EqualTo(BattlePhase.Ready));
            Assert.That(battle.Revision, Is.EqualTo(0));
        }

        [Test]
        public void ResolvedCanRestartToReadyAndAdvanceRevision()
        {
            var battle = BattleStateMachine.Restore(BattlePhase.Resolved, 3);

            battle.Restart(3);

            Assert.That(battle.Phase, Is.EqualTo(BattlePhase.Ready));
            Assert.That(battle.Revision, Is.EqualTo(4));
        }

        [TestCase(BattlePhase.Ready)]
        [TestCase(BattlePhase.Running)]
        [TestCase(BattlePhase.Victory)]
        [TestCase(BattlePhase.Defeat)]
        public void RestartOutsideResolvedIsRejectedWithoutMutation(BattlePhase phase)
        {
            var battle = BattleStateMachine.Restore(phase, 7);

            Assert.Throws<InvalidOperationException>(() => battle.Restart(7));
            Assert.That(battle.Phase, Is.EqualTo(phase));
            Assert.That(battle.Revision, Is.EqualTo(7));
        }

        [Test]
        public void StaleRestartRevisionIsRejectedWithoutMutation()
        {
            var battle = BattleStateMachine.Restore(BattlePhase.Resolved, 7);

            Assert.Throws<InvalidOperationException>(() => battle.Restart(6));
            Assert.That(battle.Phase, Is.EqualTo(BattlePhase.Resolved));
            Assert.That(battle.Revision, Is.EqualTo(7));
        }

        [TestCase(BattlePhase.Ready)]
        [TestCase(BattlePhase.Running)]
        [TestCase(BattlePhase.Resolved)]
        public void FinalizeOutsideTerminalResultIsRejectedWithoutMutation(BattlePhase phase)
        {
            var battle = BattleStateMachine.Restore(phase, 7);

            Assert.Throws<InvalidOperationException>(() => battle.FinalizeResult(7));
            Assert.That(battle.Phase, Is.EqualTo(phase));
            Assert.That(battle.Revision, Is.EqualTo(7));
        }
    }
}

[executed on device: DESKTOP-7174LTC (46b0e2c9-d71e-4a77-8023-4823da2159d2)]
