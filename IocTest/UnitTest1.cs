using MyIoC;
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
            var customerBLL = (CustomerBLL)container.CreateInstance(typeof(CustomerBLL));
            var customerBLL2 = (CustomerBLL2)container.CreateInstance(typeof(CustomerBLL2));
            var customer3 = container.CreateInstance<CustomerBLL>();

            Assert.Pass();
        }

        [Test]
        public void AddTypeTest()
        {
            Container container = new Container();
            //container.AddAssembly(Assembly.LoadFrom("MyIoc.dll"));
            container.AddType(typeof(CustomerBLL));
            container.AddType(typeof(Logger));
            container.AddType(typeof(CustomerBLL2));
            container.AddType(typeof(CustomerDAL), typeof(ICustomerDAL));
            


            var customerBLL = (CustomerBLL)container.CreateInstance(typeof(CustomerBLL));
            var customerBLL2 = (CustomerBLL2)container.CreateInstance(typeof(CustomerBLL2));
            var customer3 = container.CreateInstance<CustomerBLL>();

            Assert.Pass();
        }
    }
}