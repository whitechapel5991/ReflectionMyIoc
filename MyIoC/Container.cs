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

		#region Registration typies to container
		public void AddAssembly(Assembly assembly)
		{
			if (assembly == null)
			{
				throw new NullReferenceException("Assembly must not be null!");
			}

			var allClassesWithAttributes = assembly.DefinedTypes.Where(x => x.IsClass && x.GetCustomAttributes(false)
				.Any(y => y.GetType() == typeof(ExportAttribute) ||
				y.GetType() == typeof(ImportAttribute) ||
				y.GetType() == typeof(ImportConstructorAttribute)));

			if (IsImportConstructorAndImportPropertyAttributiesBoth(allClassesWithAttributes))
			{
				throw new AmbiguousMatchException("Dependency injection can be used only for constructor or property but not both");
			}

			allClassesWithAttributes.ToList().ForEach(y => AddType(y));
		}

		public void AddType(Type type)
		{
			ValidateType(type);

			var IsExportAttribute = type.GetCustomAttributes(false)
				.Any(x => x.GetType() == typeof(ExportAttribute));

			if (IsExportAttribute)
			{
				ExportAttribute exportAttribute = type.GetTypeInfo().GetCustomAttribute<ExportAttribute>();

				bool isExportAttributeWithContract = exportAttribute.Contract != null;

				if (isExportAttributeWithContract)
				{
					// add using special methods for export with contract
					AddType(type, exportAttribute.Contract);
				}
				else
				{
					registredExport.Add(type.GetTypeInfo());
				}
			}

			RegistredImportConstructorAndPropertyIfExistAttribute(type);
		}

		public void AddType(Type type, Type contractType)
		{
			ValidateType(type);
			ValidateType(contractType);

			ValidateContractType(type, contractType);

			registredExport.Add(type.GetTypeInfo());

			RegistredImportConstructorAndPropertyIfExistAttribute(type);
		}

		private void RegistredImportConstructorAndPropertyIfExistAttribute(Type type)
		{
			var IsImportConstructorAttributes = type.GetCustomAttributes(false)
				.Any(x => x.GetType() == typeof(ImportConstructorAttribute));

			if (IsImportConstructorAttributes)
			{
				registredImportConstructor.Add(type.GetTypeInfo());

				// return because if import constructor attribute exist = import property attribute not exist
				return;
			}

			var IsPropertyImportAttributes = type.GetProperties()
				.Any(x => x.GetCustomAttributes(false).Any(y => y.GetType() == typeof(ImportAttribute)));

			if (IsPropertyImportAttributes)
			{
				registredImportProperty.Add(type.GetTypeInfo());
			}
		}

		#region Validation
		private void ValidateType(Type type)
		{
			if (type == null)
			{
				throw new NullReferenceException("Type  must not be null!");
			}

			if (IsImportConstructorAndImportPropertyAttributiesBoth(type))
			{
				throw new AmbiguousMatchException("Dependency injection can be used only for constructor or property but not both");
			}
		}

		private void ValidateContractType(Type type, Type contractType)
		{
			bool isImplementInterface = type.GetTypeInfo().GetInterfaces().Any(x => x.GetTypeInfo() == contractType.GetTypeInfo());
			bool isImplementAbstractClass = type.GetTypeInfo().BaseType.GetTypeInfo() == contractType.GetTypeInfo();
			if (!isImplementInterface && !isImplementAbstractClass)
			{
				throw new AmbiguousMatchException("Class must implemented contract type");
			}

			ExportAttribute exportAttribute = type.GetTypeInfo().GetCustomAttribute<ExportAttribute>();
			if (exportAttribute.Contract?.GetTypeInfo() != contractType.GetTypeInfo())
			{
				throw new AmbiguousMatchException($"Don't have attribute {type.GetTypeInfo().Name} with contract type {contractType.GetTypeInfo().Name}");
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
		#endregion
		#endregion

		#region Create instance
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
				return CreateInstanceWithConstructorInjection(type);
			}
			if (isPropertyImport)
			{
				return CreateInstanceWithPropertyInjection(type);
			}

			throw new Exception("Unknow exception!");
		}


		public T CreateInstance<T>()
		{
			var type = typeof(T);
			return (T)CreateInstance(type);
		}
		#endregion


		#region Constructor Injection
		private object CreateInstanceWithConstructorInjection(Type type)
		{
			var constructors = type.GetTypeInfo().GetConstructors();

			foreach (var constructor in constructors)
			{
				// Find constructor with all registred parameters
				if (IsAllConstructorParametersRegistred(constructor))
				{
					object[] parametersInstance = new object[constructor.GetParameters().Count()];

					int i = 0;
					foreach (ParameterInfo param in constructor.GetParameters())
					{
						var paramInstance = CreateExportParam(param.ParameterType);
						parametersInstance[i] = paramInstance;
						i++;
					}

					return constructor.Invoke(parametersInstance);
				}
			}

			throw new Exception("Unknow exception");
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
		#endregion

		#region Property Injection
		private object CreateInstanceWithPropertyInjection(Type type)
		{
			// Properties which mark Import attribute
			var properties = type.GetTypeInfo().GetProperties().Where(x => x.GetCustomAttributes(false).Any(y => y.GetType() == typeof(ImportAttribute)));

			var instance = Activator.CreateInstance(type);

			foreach (var property in properties)
			{
				if (IsExportParamRegistred(property.PropertyType))
				{
					var propertyInstance = CreateExportParam(property.PropertyType);

					property.SetValue(instance, propertyInstance);
				}
			}

			return instance;
		}
		#endregion


		private bool IsExportParamRegistred(Type exportType)
		{
			var type = exportType.GetTypeInfo();
			return registredExport.Any(x => x.GetTypeInfo() == type
				|| x.GetCustomAttribute<ExportAttribute>(false).Contract?.GetTypeInfo() == type);
		}

		#region Export types
		private object CreateExportParam(Type type)
		{
			bool isExport = registredExport.Any(x => x.GetTypeInfo() == type.GetTypeInfo());
			bool isExportContractType = registredExport.Any(x => x.GetCustomAttribute<ExportAttribute>(false).Contract?.GetTypeInfo() == type.GetTypeInfo());

			if (isExport)
			{
				return CreateExportInstance(type);
			}
			if (isExportContractType)
			{
				var contractType = registredExport.First(x => x.GetCustomAttribute<ExportAttribute>(false).Contract?.GetTypeInfo() == type.GetTypeInfo());
				return CreateExportInstance(contractType);
			}

			throw new Exception("Unknow exception");
		}

		private object CreateExportInstance(Type type)
		{
			object instance = null;

			bool isConstructorImprort = registredImportConstructor.Any(x => x.GetTypeInfo() == type.GetTypeInfo());
			bool isPropertyImport = registredImportProperty.Any(x => x.GetTypeInfo() == type.GetTypeInfo());

			if (isConstructorImprort || isPropertyImport)
			{
				instance = CreateInstance(type);
			}
			else
			{
				var constructor = type.GetTypeInfo().GetConstructor(Type.EmptyTypes);

				instance = constructor.Invoke(parameters: null);
			}

			return instance;
		}
		#endregion
	}
}
