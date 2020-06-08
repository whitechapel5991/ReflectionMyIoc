using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyIoC
{
	[AttributeUsage(AttributeTargets.Property)]
	public class ImportAttribute : Attribute
	{
	}

	[AttributeUsage(AttributeTargets.Class)]
	public class ImportConstructorAttribute : Attribute
	{
	}

	[AttributeUsage(AttributeTargets.Class)]

	public class ExportAttribute : Attribute
	{
		public ExportAttribute()
		{ }

		public ExportAttribute(Type contract)
		{
			Contract = contract;
		}

		public Type Contract { get; private set; }
	}
}
