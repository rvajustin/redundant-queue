using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FluentAssertions;
using RVA.RedundantQueue.Abstractions;
using RVA.RedundantQueue.Exceptions;
using Xunit;
using RVA.RedundantQueue.Implementations;

namespace RVA.RedundantQueue.Tests
{
    [ExcludeFromCodeCoverage]
    public class RedundantQueueOrchestratorTests : TestContainer
    {
        [Fact]
        public async Task ShouldCreateRedundantQueue()
        {
            // given a name
            var name = nameof(ShouldCreateRedundantQueue);

            // when create is called
            var sut = Create<RedundantQueueOrchestrator>();
            var redundantQueue = await sut.CreateAsync<string>(name);

            // then redundant queue is not null
            redundantQueue.Should().NotBeNull();
        }

        [Fact]
        public async Task ShouldNotCreateDuplicateRedundantQueue()
        {
            // given a name
            var name = nameof(ShouldNotCreateDuplicateRedundantQueue);

            // when create is called
            var sut = Create<RedundantQueueOrchestrator>();
            await sut.CreateAsync<string>(name);

            // and then create is called a second time with a duplicate name & type
            // then a duplicate exception is thrown
            await Assert.ThrowsAsync<DuplicateKeyException>(async () => await sut.CreateAsync<string>(name));
        }

        [Fact]
        public async Task ShouldCreateRedundantQueueWithDifferentTypeButSameName()
        {
            // given a name
            var name = nameof(ShouldCreateRedundantQueueWithDifferentTypeButSameName);

            // when create is called
            var sut = Create<RedundantQueueOrchestrator>();
            await sut.CreateAsync<string>(name);

            // and then create is called a second time with a duplicate name and different type
            var redundantQueue = await sut.CreateAsync<int>(name);

            // then redundant queue is not null
            redundantQueue.Should().NotBeNull();
        }

        [Fact]
        public async Task ShouldCreateRedundantQueueWithDifferentNameButSameType()
        {
            // given a name
            var name = nameof(ShouldCreateRedundantQueueWithDifferentNameButSameType);
            var name2 = $"{nameof(ShouldCreateRedundantQueueWithDifferentNameButSameType)}2";

            // when create is called
            var sut = Create<RedundantQueueOrchestrator>();
            await sut.CreateAsync<string>(name);

            // and then create is called a second time with a duplicate name and different type
            var redundantQueue = await sut.CreateAsync<string>(name2);

            // then redundant queue is not null
            redundantQueue.Should().NotBeNull();
        }

        [Theory]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData(null)]
        public async Task ShouldNotCreateRedundantQueueWithBadName(string name)
        {
            // given a bad name (from theory data)

            // when create is called
            var sut = Create<RedundantQueueOrchestrator>();
            // then an error is thrown
            await Assert.ThrowsAsync<ArgumentException>(async () => await sut.CreateAsync<string>(name));
        }

        [Theory]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData(null)]
        public async Task ShouldNotRegisterRedundantQueueWithBadName(string name)
        {
            // given a bad name (from theory data)

            // when register is called
            var sut = Create<RedundantQueueOrchestrator>();
            // then an error is thrown
            await Assert.ThrowsAsync<ArgumentException>(async () => await sut.RegisterAsync<string>(name));
        }

        [Fact]
        public async Task ShouldRegisterRedundantQueue()
        {
            // given a name
            var name = nameof(ShouldRegisterRedundantQueue);
            // and multiple sub-queues
            var firstQueue = SetupMock<ISubQueue<string>>(out _, false,
                s => s.SetupGet(m => m.Name).Returns("firstQueueMock"),
                s => s.SetupGet(m => m.Priority).Returns(QueuePriority.First));

            var secondQueue = SetupMock<ISubQueue<string>>(out _, false,
                s => s.SetupGet(m => m.Name).Returns("secondQueueMock"),
                s => s.SetupGet(m => m.Priority).Returns(QueuePriority.Second));

            // when register is called
            var sut = Create<RedundantQueueOrchestrator>();
            var redundantQueue = await sut.RegisterAsync(name, firstQueue, secondQueue);

            // then redundant queue is not null
            redundantQueue.Should().NotBeNull();
            // and both queues should be contained within
            redundantQueue.SubQueues.Should().Contain(q => q.Name.Equals("firstQueueMock") && q.Priority.Equals(QueuePriority.First));
            redundantQueue.SubQueues.Should().Contain(q => q.Name.Equals("secondQueueMock") && q.Priority.Equals(QueuePriority.Second));
        }

        [Theory]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData(null)]
        public async Task ShouldNotResolveRedundantQueueWithBadName(string name)
        {
            // given a bad name (from theory data)

            // when resolve is called
            var sut = Create<RedundantQueueOrchestrator>();
            // then an error is thrown
            await Assert.ThrowsAsync<ArgumentException>(async () => await sut.ResolveAsync<string>(name));
        }

        [Fact]
        public async Task ShouldResolveTheSameQueue()
        {
            // given a name
            var name = nameof(ShouldResolveTheSameQueue);

            // after register or create has been called
            var sut = Create<RedundantQueueOrchestrator>();
            var registeredRedundantQueue = await sut.RegisterAsync<string>(name);

            // when resolve is called
            var resolvedRedundantQueue = await sut.ResolveAsync<string>(name);

            // then the redundant queue is not null
            resolvedRedundantQueue.Should().NotBeNull();
            // and both instances are the same
            registeredRedundantQueue.Should().BeEquivalentTo(resolvedRedundantQueue);
        }

        [Fact]
        public async Task ShouldNotResolveWhenTheTypeIsDifferent()
        {
            // given a name
            var name = nameof(ShouldNotResolveWhenTheTypeIsDifferent);

            // after register or create has been called
            var sut = Create<RedundantQueueOrchestrator>();
            await sut.RegisterAsync<string>(name);

            // when resolve is called with a different type
            // then an exception is thrown
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await sut.ResolveAsync<int>(name));
        }

        [Fact]
        public async Task ShouldNotResolveWhenTheNameIsDifferent()
        {
            // given a name
            var name = nameof(ShouldNotResolveWhenTheNameIsDifferent);
            var name2 = $"{nameof(ShouldNotResolveWhenTheNameIsDifferent)}2";

            // after register or create has been called
            var sut = Create<RedundantQueueOrchestrator>();
            await sut.RegisterAsync<string>(name);

            // when resolve is called with a different type
            // then an exception is thrown
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await sut.ResolveAsync<string>(name2));
        }
    }
}