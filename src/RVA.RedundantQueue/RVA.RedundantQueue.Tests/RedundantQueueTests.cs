using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using RVA.RedundantQueue.Abstractions;
using RVA.RedundantQueue.Exceptions;
using Xunit;
using RVA.RedundantQueue.Implementations;
using Xunit.Abstractions;

namespace RVA.RedundantQueue.Tests
{
    [ExcludeFromCodeCoverage]
    public class RedundantQueueTests : TestContainer
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public RedundantQueueTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        private void ErrorHandler(object sender, RedundantQueueSendException<string> exception)
        {
            _testOutputHelper.WriteLine(exception.Message);
            _testOutputHelper.WriteLine(exception.QueueMessage);
            _testOutputHelper.WriteLine(exception.SubQueue.Name);
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
                s => s.SetupGet(m => m.Priority).Returns(1));

            var secondQueue = SetupMock<ISubQueue<string>>(out var secondQueueMock, false,
                s => s.SetupGet(m => m.Name).Returns("secondQueueMock"),
                s => s.SetupGet(m => m.Priority).Returns(2));

            var thirdQueue = SetupMock<ISubQueue<string>>(out var thirdQueueMock, false,
                s => s.SetupGet(m => m.Name).Returns("thirdQueue"),
                s => s.SetupGet(m => m.Priority).Returns(3));

            var redundantQueue = new RedundantQueue<string>("thirdQueueMock") {ErrorCallback = ErrorHandler};
            await redundantQueue.AddQueueAsync(firstQueue);
            await redundantQueue.AddQueueAsync(secondQueue);
            await redundantQueue.AddQueueAsync(thirdQueue);

            // when the message is sent
            await redundantQueue.SendAsync(message);

            // then the first queue received the message
            firstQueueMock.Verify(m => m.SendAsync(It.Is<string>(s => s.Equals("someMessage"))), Times.Once);
            // and the second queue did not receive the message
            secondQueueMock.Verify(m => m.SendAsync(It.IsAny<string>()), Times.Never);
            // and the third queue did not receive the message
            thirdQueueMock.Verify(m => m.SendAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ShouldRejectMultipleSubQueuesWithEqualPriorities()
        {
            // given multiple sub-queues
            var firstQueue = SetupMock<ISubQueue<string>>(out _, false,
                s => s.SetupGet(m => m.Name).Returns("firstQueueMock"),
                s => s.SetupGet(m => m.Priority).Returns(1));

            var secondQueue = SetupMock<ISubQueue<string>>(out _, false,
                s => s.SetupGet(m => m.Name).Returns("secondQueueMock"),
                s => s.SetupGet(m => m.Priority).Returns(1));

            var redundantQueue = new RedundantQueue<string>("thirdQueueMock") {ErrorCallback = ErrorHandler};

            await redundantQueue.AddQueueAsync(firstQueue);

            // when the second queue is added with a duplicate priority
            // then an exception is thrown
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await redundantQueue.AddQueueAsync(secondQueue));
        }

        [Fact]
        public async Task MessageShouldKeepSendingUntilSuccessful()
        {
            // given a message 
            var message = "someMessage";
            // and multiple sub-queues
            var firstQueue = SetupMock<ISubQueue<string>>(out var firstQueueMock, false,
                s => s.Setup(m => m.SendAsync(It.IsAny<string>())).Throws(new Exception()),
                s => s.SetupGet(m => m.Name).Returns("firstQueueMock"),
                s => s.SetupGet(m => m.Priority).Returns(1));

            var secondQueue = SetupMock<ISubQueue<string>>(out var secondQueueMock, false,
                s => s.Setup(m => m.SendAsync(It.IsAny<string>())).Throws(new Exception()),
                s => s.SetupGet(m => m.Name).Returns("secondQueueMock"),
                s => s.SetupGet(m => m.Priority).Returns(2));

            var thirdQueue = SetupMock<ISubQueue<string>>(out var thirdQueueMock, false,
                s => s.SetupGet(m => m.Name).Returns("thirdQueue"),
                s => s.SetupGet(m => m.Priority).Returns(3));

            var redundantQueue = new RedundantQueue<string>("thirdQueueMock") {ErrorCallback = ErrorHandler};
            await redundantQueue.AddQueueAsync(firstQueue);
            await redundantQueue.AddQueueAsync(secondQueue);
            await redundantQueue.AddQueueAsync(thirdQueue);

            // when the message is sent
            await redundantQueue.SendAsync(message);

            // then we attempted to send the message to the first queue
            firstQueueMock.Verify(m => m.SendAsync(It.Is<string>(s => s.Equals("someMessage"))), Times.Once);
            // and we attempted to send the message to the second queue
            secondQueueMock.Verify(m => m.SendAsync(It.Is<string>(s => s.Equals("someMessage"))), Times.Once);
            // and we attempted to send the message to the third queue
            thirdQueueMock.Verify(m => m.SendAsync(It.Is<string>(s => s.Equals("someMessage"))), Times.Once);
        }
    }
}