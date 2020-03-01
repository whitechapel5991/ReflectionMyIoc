using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MyIoC
{
	public class Container
	{
		private IEnumerable<TypeInfo> registredImportConstructor;
		private IEnumerable<TypeInfo> registredImportProperty;
		private IEnumerable<TypeInfo> registredExport;
		public void AddAssembly(Assembly assembly)
		{
			var isComstructorAndPropertyAttributies = assembly.DefinedTypes.Where(x => x.IsClass && x.GetCustomAttributes(false)
			.Any(y => y.GetType() == typeof(ImportConstructorAttribute)) &&
			x.GetProperties(BindingFlags.Public | BindingFlags.Instance)
			.Any(y => y.GetCustomAttributes(false).Any(z => z.GetType() == typeof(ImportAttribute)))).Any();

			if (isComstructorAndPropertyAttributies)
			{
				throw new AmbiguousMatchException("Dependency injection can be used only for constructor or property but not both");
			}

			registredImportConstructor = assembly.DefinedTypes.Where(x => x.IsClass && x.GetCustomAttributes(false)
				.Any(y => y.GetType() == typeof(ImportConstructorAttribute)));

			registredExport = assembly.DefinedTypes.Where(x => x.IsClass && x.GetCustomAttributes(false)
			.Any(y => y.GetType() == typeof(ExportAttribute)));

			registredImportProperty = assembly.DefinedTypes.Where(x => x.IsClass && x.GetProperties(BindingFlags.Public | BindingFlags.Instance)
			.Any(y => y.GetCustomAttributes(false).Any(z => z.GetType() == typeof(ImportAttribute))));

		}

		public void AddType(Type type)
		{
			//type.GetCustomAttributes().Any(x => x.GetType() == typeof(ExportAttribute) || x.GetType() == typeof(ImportAttribute) || x.GetType() == typeof(ImportConstructorAttribute));
		}

		public void AddType(Type type, Type baseType)
		{ }

		

		private bool IsInstanceParamRegistred(ConstructorInfo ctorInfo)
		{
			var classParams = ctorInfo.GetParameters().Where(x => x.ParameterType.IsClass && !x.ParameterType.IsAbstract);
			return classParams.All(z => registredExport.Any(x => x.GetTypeInfo() == z.ParameterType.GetTypeInfo()));
		}

		private bool IsAbstractParamRegistred(ConstructorInfo ctorInfo)
		{
			var abstractParams = ctorInfo.GetParameters().Where(x => x.ParameterType.IsInterface || x.ParameterType.IsAbstract);
			return abstractParams.All(z => registredExport.Any(x => x.GetCustomAttribute<ExportAttribute>(false).Contract?.GetTypeInfo() == z.ParameterType.GetTypeInfo()));
		}

		public object CreateInstance(Type type)
		{
			if (registredImportConstructor.Any(x => x.GetTypeInfo() == type.GetTypeInfo()))
			{
				var constructors = type.GetTypeInfo().GetConstructors();

				foreach (var constructor in constructors)
				{
					if (!IsInstanceParamRegistred(constructor))
					{
						continue;
					}

					if (!IsAbstractParamRegistred(constructor))
					{
						continue;
					}

					object[] parametersInstance = new object[constructor.GetParameters().Count()];

					int i = 0;
					foreach (ParameterInfo param in constructor.GetParameters())
					{
						var paramType = param.ParameterType;
						var paramInstance = CreateInstance(paramType);
						parametersInstance[i] = paramInstance;
						i++;
					}

					var instance = constructor.Invoke(parametersInstance);
					return instance;
				}

				return null;
			}
			else if (registredImportProperty.Any(x => x.GetTypeInfo() == type.GetTypeInfo()))
			{
				var properties = type.GetTypeInfo().GetProperties();

				var instance = Activator.CreateInstance(type);

				foreach (var property in properties)
				{
					var propertyType = property.PropertyType;
					var propertyInstance = CreateInstance(propertyType);

					property.SetValue(instance, propertyInstance);
				}

				return instance;
			}
			else if (registredExport.Any(x => x.GetTypeInfo() == type.GetTypeInfo()))
			{
				var constructor = type.GetTypeInfo().GetConstructors().First();

				var instance = constructor.Invoke(parameters: null);
				return instance;
			}
			else if (registredExport.Any(x => x.GetCustomAttribute<ExportAttribute>(false).Contract?.GetTypeInfo() == type.GetTypeInfo()))
			{
				var constructor = registredExport.First(x => x.GetCustomAttribute<ExportAttribute>(false).Contract?.GetTypeInfo() == type.GetTypeInfo())
					.GetConstructors().First();

				var instance = constructor.Invoke(parameters: null);
				return instance;
			}
			else
			{
				throw new AmbiguousMatchException("Container does not have this type");
			}
	}


		public T CreateInstance<T>()
		{
			var type = typeof(T);
			return (T)CreateInstance(type);
		}


		public void Sample()
		{
			var container = new Container();
			container.AddAssembly(Assembly.GetExecutingAssembly());

			var customerBLL = (CustomerBLL)container.CreateInstance(typeof(CustomerBLL));
			var customerBLL2 = container.CreateInstance<CustomerBLL>();

			container.AddType(typeof(CustomerBLL));
			container.AddType(typeof(Logger));
			container.AddType(typeof(CustomerDAL), typeof(ICustomerDAL));
		}
	}
}
