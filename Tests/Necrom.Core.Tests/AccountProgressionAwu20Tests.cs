using System;
using Necrom.Core.Domain;
using NUnit.Framework;

namespace Necrom.Core.Tests
{
    [TestFixture]
    public class AccountProgressionAwu20Tests
    {
        [Test]
        public void KillExperienceScalesWithWave()
        {
            Assert.That(AccountProgression.ExperienceForKill(1), Is.EqualTo(10));
            Assert.That(AccountProgression.ExperienceForKill(11), Is.GreaterThan(AccountProgression.ExperienceForKill(1)));
        }
        [Test]
        public void ExperienceCarriesAndGrantsOnePointPerLevel()
        {
            var account = new AccountProgression();
            Assert.That(account.GrantExperience(90), Is.Zero);
            Assert.That(account.GrantExperience(25), Is.EqualTo(1));
            Assert.That(account.Level, Is.EqualTo(2));
            Assert.That(account.Experience, Is.EqualTo(15));
            Assert.That(AccountProgression.RequiredExperience(2), Is.GreaterThan(AccountProgression.RequiredExperience(1)));
        }
        [Test]
        public void MultipleLevelUpsAreNotLost()
        {
            var account = new AccountProgression();
            Assert.That(account.GrantExperience(500), Is.GreaterThanOrEqualTo(3));
            Assert.That(account.Percent, Is.InRange(0d, 1d));
        }
        [Test]
        public void InvalidSnapshotCannotBeRestored()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new AccountProgression(0, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new AccountProgression(1, 100));
            Assert.Throws<ArgumentOutOfRangeException>(() => new AccountProgression(1, -1));
        }
    }
}
