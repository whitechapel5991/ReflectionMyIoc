using MyIoC;
using MyIoC.Framework;
using MyIoC.Framework.Repository;
using MyIoC.Framework.Service;
using NUnit.Framework;
using System.Reflection;

namespace IocTest
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void AddAssemblyTest()
        {
            Container container = new Container();
            container.AddAssembly(Assembly.LoadFrom("MyIoc.dll"));
            var customerBLL = (Service)container.CreateInstance(typeof(Service));
            var customerBLL2 = (SomeRepository)container.CreateInstance(typeof(SomeRepository));
            var customer3 = container.CreateInstance<Service>();

            Assert.Pass();
        }

        [Test]
        public void AddTypeTest()
        {
            Container container = new Container();
            //container.AddAssembly(Assembly.LoadFrom("MyIoc.dll"));
            container.AddType(typeof(Connection));
            container.AddType(typeof(SomeRepository), typeof(IRepository));
            container.AddType(typeof(Service));
            //container.AddType(typeof(CustomerBLL2));



            var customerBLL2 = (SomeRepository)container.CreateInstance(typeof(SomeRepository));
            var customerBLL = (Service)container.CreateInstance(typeof(Service));
            var customer3 = container.CreateInstance<Service>();

            Assert.Pass();
        }
    }
}