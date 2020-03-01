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
        public void Test1()
        {
            Container container = new Container();
            container.AddAssembly(Assembly.LoadFrom("MyIoc.dll"));
            var customerBLL = (CustomerBLL)container.CreateInstance(typeof(CustomerBLL));
            var customerBLL2 = (CustomerBLL2)container.CreateInstance(typeof(CustomerBLL2));
            var customer3 = container.CreateInstance<CustomerBLL>();

            Assert.Pass();
        }
    }
}