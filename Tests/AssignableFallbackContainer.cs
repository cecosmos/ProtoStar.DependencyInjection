using Xunit;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using Microsoft.Extensions.DependencyInjection;

namespace ProtoStar.DependencyInjection.Tests
{
    public class AssignableFallbackContainerTest
    {
        [Fact]
        public void RegisterServices()
        {
            var abstractType = typeof(IList<int>);
            var concrete = new List<int>();
            var container = new AssignableFallbackContainer();
            container.AddService<IList<int>>(concrete);
            Assert.Equal(concrete, container.GetService(abstractType));
        }

        [Fact]
        public void FallbackService()
        {
            var concrete = new List<int>();
            var container = new AssignableFallbackContainer();
            container.AddService<IList<int>>(concrete);
            Assert.Equal(concrete,container.GetService(typeof(IEnumerable<int>))); 
        }

        private class MockedConcreteDependency :
            IDependent<IEnumerable<int>>
        {
            public IEnumerable<int> Dependency { get; set; }
        }

        [Fact]
        public void InjectDependencies()
        {
            var container = new AssignableFallbackContainer();
            var service = new List<int>() {1,2,3};
            container.AddService<IList<int>>(service);
            var dependent = new MockedConcreteDependency();
            container.SolveDependencies(dependent);
            Assert.Equal(service,dependent.Dependency);
        }
    }
}