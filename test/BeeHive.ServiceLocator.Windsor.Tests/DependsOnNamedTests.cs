using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Xunit;

namespace BeeHive.ServiceLocator.Windsor.Tests
{

    
    public class DependsOnNamedTests
    {

        [Fact]
        public void CanRsolveNamed()
        {
            var container = new WindsorContainer();
            container.Register(

                Component.For<IA>()
                .ImplementedBy<A1>()
                .Named("1"),
                Component.For<IA>()
                .ImplementedBy<A2>()
                .Named("2"),
               Classes.FromAssemblyContaining<DependsOnNamedTests>()
               .Pick().ConfigureDependsOnNamed()

                );

            var b1 = container.Resolve<B1>();
            var b2 = container.Resolve<B2>();

            Assert.Equal("A1", b1._a.GetType().Name);
            Assert.Equal("A2", b2._a.GetType().Name);
        }


    }

    public interface IA
    {
        
    }

    public class A1 : IA
    {
        
    }

    public class A2 : IA
    {

    }

    public class B1
    {
        public IA _a;

        public B1([DependsOnNamed("1")] IA a)
        {
            _a = a;
        }
    }

    public class B2
    {
        public IA _a;

        public B2([DependsOnNamed("2")]IA a)
        {
            _a = a;
        }
    }

}
