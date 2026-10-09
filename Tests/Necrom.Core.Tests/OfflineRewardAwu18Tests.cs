using Necrom.Core.Domain;
using NUnit.Framework;
namespace Necrom.Core.Tests
{
    public sealed class OfflineRewardAwu18Tests
    {
        [TestCase(0,0,0)]
        [TestCase(59,0,0)]
        [TestCase(60,100,10)]
        [TestCase(61,100,10)]
        [TestCase(3600,6000,600)]
        [TestCase(28800,48000,4800)]
        public void RewardsForElapsedSeconds(int seconds,long gold,long diamonds)
        {
            Assert.That(OfflineRewardMath.Gold(seconds),Is.EqualTo(gold));
            Assert.That(OfflineRewardMath.Diamonds(seconds),Is.EqualTo(diamonds));
        }
        [Test]
        public void CapsAtEightHoursAndRejectsClockRollback()
        {
            Assert.That(OfflineRewardMath.Elapsed(1000,1000+36000),Is.EqualTo(28800));
            Assert.That(OfflineRewardMath.Elapsed(1000,999),Is.Zero);
            Assert.That(OfflineRewardMath.Elapsed(0,500),Is.Zero);
        }
    }
}
