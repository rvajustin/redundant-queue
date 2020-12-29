using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FluentAssertions;
using RVA.RedundantQueue.Exceptions;
using RVA.RedundantQueue.Implementations;
using Xunit;

namespace RVA.RedundantQueue.Tests
{
    [ExcludeFromCodeCoverage]
    public class SubQueueTests : TestContainer
    {
        [Fact]
        public void ConfiguresCorrectly()
        {
            // given a sub-queue and redundant queue
            var redundantQueue = new RedundantQueue<string>("someQueue");
            var subQueue = SubQueue.Create<string>("subQueue", QueuePriority.First, m => NoOperation());

            // when configured
            redundantQueue.Configure(subQueue, c =>
            {
                c.RetryCount = 42;
            });

            // then configuration is correct
            redundantQueue.Configure(subQueue, c =>
            {
                c.RetryCount.Should().Be(42);
            });
        }

        [Fact]
        public void ConfigurationIsIsolatedToRedundantQueue()
        {
            // given a sub-queue and two redundant queue
            var redundantQueue1 = new RedundantQueue<string>("someQueue1");
            var redundantQueue2 = new RedundantQueue<string>("someQueue2");
            var subQueue = SubQueue.Create<string>("subQueue", QueuePriority.First, m => NoOperation());

            // when configured
            redundantQueue1.Configure(subQueue, c =>
            {
                c.RetryCount = 42;
            });

            // then configuration is not transported to the other redundant queue
            redundantQueue2.Configure(subQueue, c =>
            {
                c.RetryCount.Should().NotBe(42);
            });
        }

        [Fact]
        public async Task RetriesUntilAttemptsAreExhausted()
        {
            // given a sub-queue and redundant queue and a counter, i
            var i = 0;
            var redundantQueue = new RedundantQueue<string>("someQueue");
            var subQueue = SubQueue.Create<string>("subQueue", QueuePriority.First, m =>
            {
                i++;
                throw new Exception();
            });
            // and the sub-queue is configured to retry 4 times
            redundantQueue
                .AddSubQueue(subQueue)
                .Configure(c =>
                {
                    c.RetryCount = 4;
                });

            // when a message is sent
           await Assert.ThrowsAsync<RedundantQueueSendException<string>>(async () =>
                await redundantQueue.SendAsync("someMessage"));

            // then we attempted four times
            Assert.Equal(4, i);
        }

        private static void NoOperation()
        {
        }
    }
}