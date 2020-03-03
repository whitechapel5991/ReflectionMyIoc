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
		private List<TypeInfo> registredImportConstructor = new List<TypeInfo>();
		private List<TypeInfo> registredImportProperty = new List<TypeInfo>();
		private List<TypeInfo> registredExport = new List<TypeInfo>();

		public void AddAssembly(Assembly assembly)
		{
			if (assembly == null)
			{
				throw new NullReferenceException("Assembly must not be null!");
			}

			var allClasses = assembly.DefinedTypes.Where(x => x.IsClass);

			if (IsImportConstructorAndImportPropertyAttributiesBoth(allClasses))
			{
				throw new AmbiguousMatchException("Dependency injection can be used only for constructor or property but not both");
			}

			allClasses.ToList().ForEach(y => AddType(y));
		}

		public void AddType(Type type, Type contractType)
		{
			if (type == null || contractType == null)
			{
				throw new NullReferenceException("Type and contract type must not be null!");
			}

			var attr = type.GetTypeInfo().GetCustomAttribute<ExportAttribute>();
			if (attr.Contract?.GetTypeInfo() != contractType.GetTypeInfo())
			{
				throw new AmbiguousMatchException($"Don't have attribute {type.GetTypeInfo().Name} with contract type {contractType.GetTypeInfo().Name}");
			}

			registredExport.Add(type.GetTypeInfo());
		}

		public void AddType(Type type)
		{
			if (type == null)
			{
				throw new NullReferenceException("Type  must not be null!");
			}

			if (IsImportConstructorAndImportPropertyAttributiesBoth(type))
			{
				throw new AmbiguousMatchException("Dependency injection can be used only for constructor or property but not both");
			}


			var IsExportAttributeAttributes = type.GetCustomAttributes(false)
				.Any(x => x.GetType() == typeof(ExportAttribute));

			if (IsExportAttributeAttributes)
			{
				registredExport.Add(type.GetTypeInfo());
			}

			var IsImportConstructorAttributes = type.GetCustomAttributes(false)
				.Any(x => x.GetType() == typeof(ImportConstructorAttribute));

			if (IsImportConstructorAttributes)
			{
				registredImportConstructor.Add(type.GetTypeInfo());

				// return because if import constructor attribute exist than import property attribute not exist
				return;
			}

			var IsPropertyImportAttributes = type.GetProperties()
				.Any(x => x.GetCustomAttributes(false).Any(y => y.GetType() == typeof(ImportAttribute)));

			if (IsPropertyImportAttributes)
			{
				registredImportProperty.Add(type.GetTypeInfo());
			}
		}

		private bool IsImportConstructorAndImportPropertyAttributiesBoth(IEnumerable<TypeInfo> allClasses)
		{
			foreach (var classType in allClasses)
			{
				if (IsImportConstructorAndImportPropertyAttributiesBoth(classType))
				{
					return true;
				}
			}

			return false;
		}

		private bool IsImportConstructorAndImportPropertyAttributiesBoth(Type type)
		{
			var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

			bool isPropertyImportAttribute = properties.Any(y => y.GetCustomAttributes(false).Any(z => z.GetType() == typeof(ImportAttribute)));
			bool isClassImportConstructorAttribute = type.GetCustomAttributes(false).Any(x => x.GetType() == typeof(ImportConstructorAttribute));

			return isPropertyImportAttribute && isClassImportConstructorAttribute;
		}

		

		

		// continue refactoring 


		//private ConstructorInfo IsParamRegistred(ConstructorInfo[] constructors)
		//{
		//	foreach (var constructor in constructors)
		//	{
		//		if (!IsInstanceParamRegistred(constructor))
		//		{
		//			continue;
		//		}

		//		if (!IsAbstractParamRegistred(constructor))
		//		{
		//			continue;
		//		}
		//	}
		//	return;
		//}

		private object CreateExportParam(Type type)
		{
			bool isExport = registredExport.Any(x => x.GetTypeInfo() == type.GetTypeInfo());
			bool isExportContractType = registredExport.Any(x => x.GetCustomAttribute<ExportAttribute>(false).Contract?.GetTypeInfo() == type.GetTypeInfo());

			if (isExport)
			{
				var constructor = type.GetTypeInfo().GetConstructors().First();

				var instance = constructor.Invoke(parameters: null);
				return instance;
			}
			if (isExportContractType)
			{
				var constructor = registredExport.First(x => x.GetCustomAttribute<ExportAttribute>(false).Contract?.GetTypeInfo() == type.GetTypeInfo())
					.GetConstructors().First();

				var instance = constructor.Invoke(parameters: null);
				return instance;
			}

			return null;
		}

		//private bool IsInstanceParamRegistred(ConstructorInfo ctorInfo)
		//{
		//	var classParams = ctorInfo.GetParameters().Where(x => x.ParameterType.IsClass && !x.ParameterType.IsAbstract);
		//	return classParams.All(z => registredExport.Any(x => x.GetTypeInfo() == z.ParameterType.GetTypeInfo()));
		//}

		//private bool IsAbstractParamRegistred(ConstructorInfo ctorInfo)
		//{
		//	var abstractParams = ctorInfo.GetParameters().Where(x => x.ParameterType.IsInterface || x.ParameterType.IsAbstract);
		//	return abstractParams.All(z => registredExport.Any(x => x.GetCustomAttribute<ExportAttribute>(false).Contract?.GetTypeInfo() == z.ParameterType.GetTypeInfo()));
		//}

		private bool IsExportParamRegistred(Type exportType)
		{
			var type = exportType.GetTypeInfo();
			return registredExport.Any(x => x.GetTypeInfo() == type 
				|| x.GetCustomAttribute<ExportAttribute>(false).Contract?.GetTypeInfo() == type);
		}

		private bool IsAllConstructorParametersRegistred(ConstructorInfo constructor)
		{
			foreach (var parameter in constructor.GetParameters())
			{
				if (!IsExportParamRegistred(parameter.ParameterType))
				{
					return false;
				}
			}

			return true;
		}

		public object CreateInstance(Type type)
		{
			bool isConstructorImprort = registredImportConstructor.Any(x => x.GetTypeInfo() == type.GetTypeInfo());
			bool isPropertyImport = registredImportProperty.Any(x => x.GetTypeInfo() == type.GetTypeInfo());

			bool isTypeRegistred = isConstructorImprort || isPropertyImport;

			if (!isTypeRegistred)
			{
				throw new AmbiguousMatchException("Container does not have this type");
			}

			if (isConstructorImprort)
			{
				var constructors = type.GetTypeInfo().GetConstructors();

				foreach (var constructor in constructors)
				{
					//if (!IsInstanceParamRegistred(constructor))
					//{
					//	continue;
					//}

					//if (!IsAbstractParamRegistred(constructor))
					//{
					//	continue;
					//}

					// Find constructor with all registred parameters
					if (!IsAllConstructorParametersRegistred(constructor))
					{
						continue;
					}


					object[] parametersInstance = new object[constructor.GetParameters().Count()];

					int i = 0;
					foreach (ParameterInfo param in constructor.GetParameters())
					{
						var paramType = param.ParameterType;
						var paramInstance = CreateExportParam(paramType);
						parametersInstance[i] = paramInstance;
						i++;
					}

					var instance = constructor.Invoke(parametersInstance);
					return instance;
				}

				return null;
			}
			if (isPropertyImport)
			{
				var properties = type.GetTypeInfo().GetProperties();

				var instance = Activator.CreateInstance(type);

				foreach (var property in properties)
				{
					var propertyType = property.PropertyType;
					var propertyInstance = CreateExportParam(propertyType);

					property.SetValue(instance, propertyInstance);
				}

				return instance;
			}

			return null;
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
