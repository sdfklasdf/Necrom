using System;
using Necrom.Core.Application;
using NUnit.Framework;

namespace Necrom.Core.Tests
{
    public sealed class LocalCommandQueueRecoveryTests
    {
        [Test]
        public void FailedCommandStopsCurrentDrainButLeavesLaterCommandPendingForExplicitRetry()
        {
            var queue = new LocalSerializedCommandQueue();
            var executed = 0;

            queue.Enqueue(() => throw new InvalidOperationException("command failed"));
            queue.Enqueue(() => executed++);

            Assert.Throws<InvalidOperationException>(() => queue.Drain());

            Assert.That(executed, Is.EqualTo(0));
            Assert.That(queue.PendingCount, Is.EqualTo(1));

            queue.Drain();

            Assert.That(executed, Is.EqualTo(1));
            Assert.That(queue.PendingCount, Is.EqualTo(0));
        }

        [Test]
        public void ReentrantDrainIsRejectedWithoutConsumingPendingLaterCommand()
        {
            var queue = new LocalSerializedCommandQueue();
            var laterExecuted = 0;

            queue.Enqueue(() =>
            {
                Assert.Throws<InvalidOperationException>(() => queue.Drain());
            });
            queue.Enqueue(() => laterExecuted++);

            queue.Drain();

            Assert.That(laterExecuted, Is.EqualTo(1));
            Assert.That(queue.PendingCount, Is.EqualTo(0));
        }
    }
}
