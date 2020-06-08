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
        [Test]
        public void AddAssemblyTest()
        {
            Container container = new Container();
            container.AddAssembly(Assembly.LoadFrom("MyIoc.dll"));
            var service = (Service)container.CreateInstance(typeof(Service));
            var repository = (SomeRepository)container.CreateInstance(typeof(SomeRepository));
            var serviceGenericCreate = container.CreateInstance<Service>();

            Assert.Multiple(() =>
            {
                Assert.IsNotNull(service);
                Assert.IsNotNull(service.repository);
                Assert.IsNotNull(((SomeRepository)service.repository).Connection);
                Assert.IsNotNull(repository);
                Assert.IsNotNull(repository.Connection);
                Assert.IsNotNull(serviceGenericCreate);
                Assert.IsNotNull(serviceGenericCreate.repository);
                Assert.IsNotNull(((SomeRepository)serviceGenericCreate.repository).Connection);

                Assert.IsInstanceOf<Service>(service);
                Assert.IsInstanceOf<SomeRepository>(service.repository);
                Assert.IsInstanceOf<Connection>(((SomeRepository)service.repository).Connection);
                Assert.IsInstanceOf<SomeRepository>(repository);
                Assert.IsInstanceOf<Connection>(repository.Connection);
                Assert.IsInstanceOf<Service>(serviceGenericCreate);
                Assert.IsInstanceOf<SomeRepository>(serviceGenericCreate.repository);
                Assert.IsInstanceOf<Connection>(((SomeRepository)serviceGenericCreate.repository).Connection);
            });
        }

        [Test]
        public void AddTypeTest()
        {
            Container container = new Container();
            container.AddType(typeof(Connection));
            container.AddType(typeof(SomeRepository), typeof(IRepository));
            container.AddType(typeof(Service));

            var repository = (SomeRepository)container.CreateInstance(typeof(SomeRepository));
            var service = (Service)container.CreateInstance(typeof(Service));
            var serviceGenericCreate = container.CreateInstance<Service>();

            Assert.Multiple(() =>
            {
                Assert.IsNotNull(service);
                Assert.IsNotNull(service.repository);
                Assert.IsNotNull(((SomeRepository)service.repository).Connection);
                Assert.IsNotNull(repository);
                Assert.IsNotNull(repository.Connection);
                Assert.IsNotNull(serviceGenericCreate);
                Assert.IsNotNull(serviceGenericCreate.repository);
                Assert.IsNotNull(((SomeRepository)serviceGenericCreate.repository).Connection);

                Assert.IsInstanceOf<Service>(service);
                Assert.IsInstanceOf<SomeRepository>(service.repository);
                Assert.IsInstanceOf<Connection>(((SomeRepository)service.repository).Connection);
                Assert.IsInstanceOf<SomeRepository>(repository);
                Assert.IsInstanceOf<Connection>(repository.Connection);
                Assert.IsInstanceOf<Service>(serviceGenericCreate);
                Assert.IsInstanceOf<SomeRepository>(serviceGenericCreate.repository);
                Assert.IsInstanceOf<Connection>(((SomeRepository)serviceGenericCreate.repository).Connection);
            });
        }
    }
}