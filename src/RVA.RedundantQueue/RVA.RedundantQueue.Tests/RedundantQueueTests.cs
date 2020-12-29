using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using RVA.RedundantQueue.Abstractions;
using RVA.RedundantQueue.Exceptions;
using RVA.RedundantQueue.Implementations;
using Xunit;
using Xunit.Abstractions;

namespace RVA.RedundantQueue.Tests
{
    [ExcludeFromCodeCoverage]
    public class RedundantQueueTests : TestContainer
    {
        private readonly ITestOutputHelper testOutputHelper;

        public RedundantQueueTests(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

        private void ErrorHandler(object sender, RedundantQueueSendException<string> exception)
        {
            testOutputHelper.WriteLine(exception.Message);
            testOutputHelper.WriteLine(exception.QueueMessage);
            testOutputHelper.WriteLine(exception.SubQueue.Name);
        }

        [Fact]
        public void NameShouldBeCorrect()
        {
            // given a name
            const string name = "sampleName";

            // when a redundant queue is created with that name
            var sut = new RedundantQueue<string>(name);

            // then the redundant queue is named properly
            sut.Name.Should().Be("sampleName");
        }

        [Fact]
        public async Task MessageShouldStopSendingOnceSuccessful()
        {
            // given a message 
            var message = "someMessage";
            // and multiple sub-queues
            var firstQueue = SetupMock<ISubQueue<string>>(out var firstQueueMock, false,
                s => s.SetupGet(m => m.Name).Returns("firstQueueMock"),
                s => s.SetupGet(m => m.Priority).Returns(QueuePriority.First));

            var secondQueue = SetupMock<ISubQueue<string>>(out var secondQueueMock, false,
                s => s.SetupGet(m => m.Name).Returns("secondQueueMock"),
                s => s.SetupGet(m => m.Priority).Returns(QueuePriority.Second));

            var redundantQueue = new RedundantQueue<string>("redundantQueue") {ErrorCallback = ErrorHandler};
             redundantQueue.AddSubQueue(firstQueue);
             redundantQueue.AddSubQueue(secondQueue);

            // when the message is sent
            await redundantQueue.SendAsync(message);

            // then the first queue received the message
            firstQueueMock.Verify(
                m => m.SendAsync(It.Is<string>(s => s.Equals("someMessage")), It.IsAny<CancellationToken>()),
                Times.Once);
            // and the second queue did not receive the message
            secondQueueMock.Verify(m => m.SendAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public void ShouldRejectMultipleSubQueuesWithEqualPriorities()
        {
            // given multiple sub-queues
            var firstQueue = SetupMock<ISubQueue<string>>(out _, false,
                s => s.SetupGet(m => m.Name).Returns("firstQueueMock"),
                s => s.SetupGet(m => m.Priority).Returns(QueuePriority.First));

            var secondQueue = SetupMock<ISubQueue<string>>(out _, false,
                s => s.SetupGet(m => m.Name).Returns("secondQueueMock"),
                s => s.SetupGet(m => m.Priority).Returns(QueuePriority.First));

            var redundantQueue = new RedundantQueue<string>("thirdQueueMock") {ErrorCallback = ErrorHandler};

            redundantQueue.AddSubQueue(firstQueue);

            // when the second queue is added with a duplicate priority
            // then an exception is thrown
            Assert.Throws<InvalidOperationException>( () => redundantQueue.AddSubQueue(secondQueue));
        }

        [Fact]
        public async Task MessageShouldKeepSendingUntilSuccessful()
        {
            // given a message 
            var message = "someMessage";
            // and multiple sub-queues
            var firstQueue = SetupMock<ISubQueue<string>>(out var firstQueueMock, false,
                s => s.Setup(m => m.SendAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .Throws(new Exception()),
                s => s.SetupGet(m => m.Name).Returns("firstQueueMock"),
                s => s.SetupGet(m => m.Priority).Returns(QueuePriority.First));

            var secondQueue = SetupMock<ISubQueue<string>>(out var secondQueueMock, false,
                s => s.Setup(m => m.SendAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .Throws(new Exception()),
                s => s.SetupGet(m => m.Name).Returns("secondQueueMock"),
                s => s.SetupGet(m => m.Priority).Returns(QueuePriority.Second));

            var thirdQueue = SetupMock<ISubQueue<string>>(out var thirdQueueMock, false,
                s => s.SetupGet(m => m.Name).Returns("thirdQueue"),
                s => s.SetupGet(m => m.Priority).Returns(QueuePriority.Last));

            var redundantQueue = new RedundantQueue<string>("thirdQueueMock") {ErrorCallback = ErrorHandler};
             redundantQueue.AddSubQueue(firstQueue);
             redundantQueue.AddSubQueue(secondQueue);
             redundantQueue.AddSubQueue(thirdQueue);

            // when the message is sent
            await redundantQueue.SendAsync(message);

            // then we attempted to send the message to the first queue
            firstQueueMock.Verify(
                m => m.SendAsync(It.Is<string>(s => s.Equals("someMessage")), It.IsAny<CancellationToken>()),
                Times.Once);
            // and we attempted to send the message to the second queue
            secondQueueMock.Verify(
                m => m.SendAsync(It.Is<string>(s => s.Equals("someMessage")), It.IsAny<CancellationToken>()),
                Times.Once);
            // and we attempted to send the message to the third queue
            thirdQueueMock.Verify(
                m => m.SendAsync(It.Is<string>(s => s.Equals("someMessage")), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}