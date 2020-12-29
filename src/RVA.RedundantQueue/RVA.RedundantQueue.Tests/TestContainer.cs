using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using AutoFixture;
using AutoFixture.AutoMoq;
using Moq;

namespace RVA.RedundantQueue.Tests
{
    [ExcludeFromCodeCoverage]
    public abstract class TestContainer
    {
        private readonly Dictionary<Type, (Mock, object)> mocks = new Dictionary<Type, (Mock, object)>();

        protected TestContainer()
        {
            Fixture = new Fixture()
                .Customize(new AutoMoqCustomization
                {
                    ConfigureMembers = true
                });
            Mocks = new ReadOnlyDictionary<Type, (Mock, object)>(mocks);
        }

        protected IFixture Fixture { get; }

        protected IReadOnlyDictionary<Type, (Mock, object)> Mocks { get; }

        public TService Create<TService>(params Action<TService>[] actions)
        {
            var service = Fixture.Create<TService>();

            foreach (var action in actions) action(service);

            return service;
        }

        public TService SetupMock<TService>(out Mock<TService> mock, bool shared = true,
            params Action<Mock<TService>>[] actions)
            where TService : class
        {
            TService service = default;
            return SetupMock(ref service, out mock, shared, actions);
        }

        public TService SetupMock<TService>(ref TService service, out Mock<TService> mock, bool shared = true,
            params Action<Mock<TService>>[] actions)
            where TService : class
        {
            var serviceType = typeof(TService);

            mock = shared ? Fixture.Freeze<Mock<TService>>() : Fixture.Create<Mock<TService>>();
            service = mock.Object;

            foreach (var action in actions) action(mock);

            if (shared)
            {
                mocks[serviceType] = (mock, service);
                Fixture.Inject(service);
            }

            return service;
        }

        public TService SetupMock<TService>(params Action<Mock<TService>>[] actions)
            where TService : class
        {
            TService service = null;
            return SetupMock(ref service, out _, true, actions);
        }

        public TService SetupMock<TService>(ref TService service, params Action<Mock<TService>>[] actions)
            where TService : class
        {
            service = SetupMock(actions);
            return service;
        }

        public abstract class ForService<TServiceUnderTest> : TestContainer
        {
            public virtual TServiceUnderTest GetServiceUnderTest(params Action<TServiceUnderTest>[] actions)
            {
                var sut = Create(actions);
                return sut;
            }
        }
    }
}