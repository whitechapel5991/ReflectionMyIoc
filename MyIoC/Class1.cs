using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MyIoC
{
	[ImportConstructor]
	public class CustomerBLL
	{
		private readonly ICustomerDAL customer;
		private readonly Logger logger;
		public CustomerBLL(ICustomerDAL dal, Logger logger)
		{
			this.customer = dal;
			this.logger = logger;
		}
	}

	public class CustomerBLL2
	{
		[Import]
		public ICustomerDAL CustomerDAL { get; set; }
		[Import]
		public Logger logger { get; set; }
	}
}
