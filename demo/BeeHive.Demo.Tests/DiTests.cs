using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeeHive.ConsoleDemo;
using BeeHive.DataStructures;
using BeeHive.Demo.PrismoEcommerce.Entities;
using Castle.Windsor;
using Xunit;

namespace BeeHive.Tests.Demo
{
    public class DiTests
    {

        [Fact]
        public void CanLoadAnyCollectionStore()
        {

            // arrange
            var container = new WindsorContainer();
            Program.ConfigureDI(container);

            // act
            var store = container.Resolve<ICollectionStore<Customer>>();

            // assert
            Assert.NotNull(store);

        }
    }
}
